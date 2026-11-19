Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de clientes (personas naturales) y su perfil LDA/FT.
''' El cliente, su perfil y su auditoría se guardan en una sola transacción.
''' </summary>
Public Class ClienteRepositorio
    Inherits RepositorioBase

    Public Function Buscar(filtro As String) As DataTable
        If Not Permisos.TieneAcceso("clientes.ver") Then Return New DataTable()
        Dim texto As String = If(filtro, "").Trim()
        Return Consultar(
            "SELECT c.ID, CONCAT(c.Nombres, ' ', c.Apellidos) AS Nombre, c.DUI, c.NIT, " &
            "       IFNULL(p.Nivel_Riesgo, 'Sin perfil') AS Riesgo, c.Activo " &
            "FROM Clientes c " &
            "LEFT JOIN Perfil_LDAFT p ON p.ID_Cliente = c.ID " &
            "WHERE (@filtro IS NULL OR c.Nombres LIKE @like OR c.Apellidos LIKE @like " &
            "       OR c.DUI LIKE @like OR c.NIT LIKE @like) " &
            "ORDER BY c.Apellidos, c.Nombres",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
            End Sub)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If id <= 0 OrElse Not Permisos.TieneAcceso("clientes.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT c.*, p.ID AS ID_Perfil, p.Nivel_Riesgo, p.Origen_Fondos_Verificado, " &
            "       p.Detalle_Origen_Fondos, p.Fecha_Verificacion_Listas, p.Observaciones AS Observaciones_LDAFT " &
            "FROM Clientes c " &
            "LEFT JOIN Perfil_LDAFT p ON p.ID_Cliente = c.ID " &
            "WHERE c.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    ''' <summary>
    ''' Guarda cliente, perfil LDA/FT y auditoría atómicamente.
    ''' Si falla el perfil o la auditoría, el cliente también se revierte.
    ''' </summary>
    Public Function GuardarCompleto(cliente As Cliente, perfil As PerfilLdaft) As Integer
        If Not Permisos.TieneAcceso("clientes.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar clientes.")
        If cliente Is Nothing Then Throw New ArgumentNullException(NameOf(cliente))
        If cliente.ID < 0 Then Throw New ArgumentException("El identificador del cliente no es válido.", NameOf(cliente))
        If cliente.ID = 0 AndAlso Not cliente.Activo Then
            Throw New InvalidOperationException("Un cliente nuevo debe registrarse activo; la baja se realiza mediante la operación autorizada.")
        End If
        ValidarDatosCliente(cliente, perfil)
        NormalizarDatosCliente(cliente, perfil)
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim idCliente As Integer = cliente.ID

                If idCliente <= 0 Then
                    idCliente = InsertarYObtenerId(
                        conexion, transaccion,
                        "INSERT INTO Clientes (Nombres, Apellidos, DUI, NIT, Nacionalidad, Profesion, " &
                        "                      Fecha_Nacimiento, Direccion, Telefono, Email, Activo) " &
                        "VALUES (@nombres, @apellidos, @dui, @nit, @nacionalidad, @profesion, " &
                        "        @nacimiento, @direccion, @telefono, @email, @activo)",
                        Sub(comando As MySqlCommand)
                            AgregarParametros(comando, cliente)
                        End Sub)

                    AuditarEnTransaccion(conexion, transaccion, "CREAR", "Clientes",
                                         idCliente, cliente.Nombres & " " & cliente.Apellidos)
                Else
                    Dim nombresActuales As String = ""
                    Dim apellidosActuales As String = ""
                    Dim duiActual As String = ""
                    Dim nitActual As String = ""
                    Dim nacionalidadActual As String = ""
                    Dim profesionActual As String = ""
                    Dim direccionActual As String = ""
                    Dim fechaNacimientoActual As Date? = Nothing
                    Dim activoActual As Boolean = False

                    Using comandoExiste As New MySqlCommand(
                            "SELECT Nombres, Apellidos, DUI, NIT, Nacionalidad, Profesion, " &
                            "Fecha_Nacimiento, Direccion, Activo FROM Clientes WHERE ID = @id FOR UPDATE",
                            conexion, transaccion)
                        Parametro(comandoExiste, "@id", idCliente)
                        Using lector As MySqlDataReader = comandoExiste.ExecuteReader()
                            If Not lector.Read() Then
                                Throw New InvalidOperationException("El cliente que intenta modificar ya no existe.")
                            End If
                            nombresActuales = Convert.ToString(lector("Nombres"))
                            apellidosActuales = Convert.ToString(lector("Apellidos"))
                            duiActual = Convert.ToString(lector("DUI"))
                            nitActual = Convert.ToString(lector("NIT"))
                            nacionalidadActual = Convert.ToString(lector("Nacionalidad"))
                            profesionActual = Convert.ToString(lector("Profesion"))
                            direccionActual = Convert.ToString(lector("Direccion"))
                            activoActual = Convert.ToInt32(lector("Activo")) = 1
                            If Not lector.IsDBNull(lector.GetOrdinal("Fecha_Nacimiento")) Then
                                fechaNacimientoActual = Convert.ToDateTime(lector("Fecha_Nacimiento")).Date
                            End If
                        End Using
                    End Using

                    If cliente.Activo <> activoActual Then
                        Throw New InvalidOperationException(
                            "El estado activo del cliente no se cambia desde la edición general. Utilice la operación autorizada de desactivación.")
                    End If

                    Dim comparecenciasExistentes As Integer
                    Using comandoReferencias As New MySqlCommand(
                        "SELECT " &
                        " (SELECT COUNT(*) FROM Comparecientes_Instrumento WHERE ID_Cliente = @idCompareciente) + " &
                        " (SELECT COUNT(*) FROM Representantes_Legales WHERE ID_Cliente = @idRepresentante) + " &
                        " (SELECT COUNT(*) FROM Casos WHERE ID_Cliente = @idCaso) + " &
                        " (SELECT COUNT(*) FROM Solicitudes_ARCO WHERE ID_Cliente = @idArco)",
                        conexion, transaccion)
                        Parametro(comandoReferencias, "@idCompareciente", idCliente)
                        Parametro(comandoReferencias, "@idRepresentante", idCliente)
                        Parametro(comandoReferencias, "@idCaso", idCliente)
                        Parametro(comandoReferencias, "@idArco", idCliente)
                        comparecenciasExistentes = Convert.ToInt32(comandoReferencias.ExecuteScalar())
                    End Using

                    Dim identidadModificada As Boolean =
                        Not TextoIgual(cliente.Nombres, nombresActuales) OrElse
                        Not TextoIgual(cliente.Apellidos, apellidosActuales) OrElse
                        Not TextoIgual(cliente.DUI, duiActual) OrElse
                        Not TextoIgual(cliente.NIT, nitActual) OrElse
                        Not TextoIgual(cliente.Nacionalidad, nacionalidadActual) OrElse
                        Not TextoIgual(cliente.Profesion, profesionActual) OrElse
                        Not TextoIgual(cliente.Direccion, direccionActual) OrElse
                        (cliente.FechaNacimiento.HasValue <> fechaNacimientoActual.HasValue) OrElse
                        (cliente.FechaNacimiento.HasValue AndAlso fechaNacimientoActual.HasValue AndAlso
                         cliente.FechaNacimiento.Value.Date <> fechaNacimientoActual.Value.Date)

                    If comparecenciasExistentes > 0 AndAlso identidadModificada Then
                        Throw New InvalidOperationException(
                            "No se puede cambiar la identidad de un cliente con historial en casos, protocolo, representaciones legales o solicitudes ARCO. Actualice solo sus datos de contacto o use el procedimiento autorizado de rectificación.")
                    End If

                    Ejecutar(conexion, transaccion,
                        "UPDATE Clientes SET Nombres = @nombres, Apellidos = @apellidos, DUI = @dui, " &
                        "       NIT = @nit, Nacionalidad = @nacionalidad, Profesion = @profesion, " &
                        "       Fecha_Nacimiento = @nacimiento, Direccion = @direccion, " &
                        "       Telefono = @telefono, Email = @email, Activo = @activo " &
                        "WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            AgregarParametros(comando, cliente)
                            Parametro(comando, "@id", idCliente)
                        End Sub)

                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Clientes",
                                         idCliente, cliente.Nombres & " " & cliente.Apellidos)
                End If

                If perfil IsNot Nothing Then
                    perfil.IDCliente = idCliente
                    perfil.IDEntidad = Nothing

                    Ejecutar(conexion, transaccion,
                        "INSERT INTO Perfil_LDAFT (ID_Cliente, ID_Entidad, Nivel_Riesgo, Origen_Fondos_Verificado, " &
                        "       Detalle_Origen_Fondos, Fecha_Verificacion_Listas, Observaciones) " &
                        "VALUES (@idCliente, NULL, @riesgo, @origen, @detalle, @fecha, @observaciones) " &
                        "ON DUPLICATE KEY UPDATE Nivel_Riesgo = VALUES(Nivel_Riesgo), " &
                        "       Origen_Fondos_Verificado = VALUES(Origen_Fondos_Verificado), " &
                        "       Detalle_Origen_Fondos = VALUES(Detalle_Origen_Fondos), " &
                        "       Fecha_Verificacion_Listas = VALUES(Fecha_Verificacion_Listas), " &
                        "       Fecha_Actualizacion = NOW(), " &
                        "       Observaciones = VALUES(Observaciones)",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@idCliente", idCliente)
                            Parametro(comando, "@riesgo", perfil.NivelRiesgo)
                            Parametro(comando, "@origen", perfil.OrigenFondosVerificado)
                            Parametro(comando, "@detalle", perfil.DetalleOrigenFondos)
                            Parametro(comando, "@fecha", If(perfil.FechaVerificacionListas.HasValue,
                                                             CObj(perfil.FechaVerificacionListas.Value), DBNull.Value))
                            Parametro(comando, "@observaciones", perfil.Observaciones)
                        End Sub)

                    Dim accionPerfil As String = "CREAR"
                    Using comandoPerfilExiste As New MySqlCommand(
                        "SELECT COUNT(*) FROM Perfil_LDAFT WHERE ID_Cliente = @idCliente",
                        conexion, transaccion)
                        Parametro(comandoPerfilExiste, "@idCliente", idCliente)
                        If Convert.ToInt32(comandoPerfilExiste.ExecuteScalar()) > 0 Then
                            accionPerfil = "MODIFICAR"
                        End If
                    End Using

                    AuditarEnTransaccion(conexion, transaccion, accionPerfil, "Perfil_LDAFT",
                                         idCliente, If(accionPerfil = "CREAR",
                                                       "Perfil LDA/FT del cliente creado",
                                                       "Perfil LDA/FT del cliente actualizado"))
                End If

                Return idCliente
            End Function)
    End Function

    Public Function Desactivar(id As Integer) As Integer
        If Not Permisos.TieneAcceso("clientes.desactivar") Then Throw New UnauthorizedAccessException("No tiene permiso para desactivar clientes.")
        If id <= 0 Then Return 0
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Clientes SET Activo = 0 WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)

                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "DESACTIVAR", "Clientes",
                                         id, "Cliente desactivado")
                End If
                Return filas
            End Function)
    End Function

    Private Shared Function TextoIgual(valorNuevo As String, valorActual As String) As Boolean
        Return String.Equals(If(valorNuevo, "").Trim(), If(valorActual, "").Trim(),
                             StringComparison.Ordinal)
    End Function

    Private Sub ValidarDatosCliente(cliente As Cliente, perfil As PerfilLdaft)
        If String.IsNullOrWhiteSpace(cliente.Nombres) OrElse cliente.Nombres.Trim().Length > 120 Then
            Throw New ArgumentException("Los nombres son obligatorios y no pueden superar 120 caracteres.", NameOf(cliente))
        End If
        If String.IsNullOrWhiteSpace(cliente.Apellidos) OrElse cliente.Apellidos.Trim().Length > 120 Then
            Throw New ArgumentException("Los apellidos son obligatorios y no pueden superar 120 caracteres.", NameOf(cliente))
        End If
        If If(cliente.Nacionalidad, "").Length > 60 OrElse If(cliente.Profesion, "").Length > 120 OrElse
           If(cliente.Direccion, "").Length > 300 OrElse If(cliente.Telefono, "").Length > 40 OrElse
           If(cliente.Email, "").Length > 120 Then
            Throw New ArgumentException("Uno o más datos del cliente superan la longitud permitida.", NameOf(cliente))
        End If
        If cliente.FechaNacimiento.HasValue AndAlso
           (cliente.FechaNacimiento.Value.Year < 1000 OrElse cliente.FechaNacimiento.Value.Date > Date.Today) Then
            Throw New ArgumentException("La fecha de nacimiento no es válida.", NameOf(cliente))
        End If
        If Not String.IsNullOrWhiteSpace(cliente.DUI) AndAlso Not ValidadorIdentidad.EsFormatoDuiValido(cliente.DUI) Then
            Throw New ArgumentException("El DUI debe tener 9 dígitos (formato 00000000-0).", NameOf(cliente))
        End If
        If Not String.IsNullOrWhiteSpace(cliente.NIT) AndAlso Not ValidadorIdentidad.EsFormatoNitValido(cliente.NIT) Then
            Throw New ArgumentException("El NIT debe tener 14 dígitos.", NameOf(cliente))
        End If
        If Not String.IsNullOrWhiteSpace(cliente.Email) AndAlso Not cliente.Email.Contains("@") Then
            Throw New ArgumentException("El correo electrónico no es válido.", NameOf(cliente))
        End If
        If perfil IsNot Nothing Then
            If perfil.NivelRiesgo <> "Bajo" AndAlso perfil.NivelRiesgo <> "Medio" AndAlso perfil.NivelRiesgo <> "Alto" Then
                Throw New ArgumentException("El nivel de riesgo LDA/FT no es válido.", NameOf(perfil))
            End If
            If If(perfil.DetalleOrigenFondos, "").Length > 500 OrElse If(perfil.Observaciones, "").Length > 500 Then
                Throw New ArgumentException("Los datos del perfil LDA/FT no pueden superar 500 caracteres.", NameOf(perfil))
            End If
            If perfil.FechaVerificacionListas.HasValue AndAlso
               (perfil.FechaVerificacionListas.Value.Year < 1000 OrElse
                perfil.FechaVerificacionListas.Value.Date > Date.Today) Then
                Throw New ArgumentException("La fecha de verificación de listas no es válida.", NameOf(perfil))
            End If
        End If
    End Sub

    Private Sub NormalizarDatosCliente(cliente As Cliente, perfil As PerfilLdaft)
        cliente.Nombres = If(cliente.Nombres, "").Trim()
        cliente.Apellidos = If(cliente.Apellidos, "").Trim()
        cliente.Nacionalidad = If(cliente.Nacionalidad, "").Trim()
        cliente.Profesion = If(cliente.Profesion, "").Trim()
        cliente.Direccion = If(cliente.Direccion, "").Trim()
        cliente.Telefono = If(cliente.Telefono, "").Trim()
        cliente.Email = If(cliente.Email, "").Trim()
        If Not String.IsNullOrWhiteSpace(cliente.DUI) Then cliente.DUI = ValidadorIdentidad.FormatearDui(cliente.DUI)
        If Not String.IsNullOrWhiteSpace(cliente.NIT) Then cliente.NIT = ValidadorIdentidad.FormatearNit(cliente.NIT)
        If perfil IsNot Nothing Then
            perfil.DetalleOrigenFondos = If(perfil.DetalleOrigenFondos, "").Trim()
            perfil.Observaciones = If(perfil.Observaciones, "").Trim()
        End If
    End Sub

    Private Sub AgregarParametros(comando As MySqlCommand, cliente As Cliente)
        Parametro(comando, "@nombres", cliente.Nombres)
        Parametro(comando, "@apellidos", cliente.Apellidos)
        Parametro(comando, "@dui", cliente.DUI)
        Parametro(comando, "@nit", cliente.NIT)
        Parametro(comando, "@nacionalidad", cliente.Nacionalidad)
        Parametro(comando, "@profesion", cliente.Profesion)
        Parametro(comando, "@nacimiento", If(cliente.FechaNacimiento.HasValue,
                                             CObj(cliente.FechaNacimiento.Value), DBNull.Value))
        Parametro(comando, "@direccion", cliente.Direccion)
        Parametro(comando, "@telefono", cliente.Telefono)
        Parametro(comando, "@email", cliente.Email)
        Parametro(comando, "@activo", cliente.Activo)
    End Sub

End Class
