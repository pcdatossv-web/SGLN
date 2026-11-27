Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de entidades legales, representantes y perfil LDA/FT.
''' La entidad, su perfil y la auditoría se guardan atómicamente.
''' </summary>
Public Class EntidadRepositorio
    Inherits RepositorioBase

    Public Function Buscar(filtro As String) As DataTable
        If Not Permisos.TieneAcceso("entidades.ver") Then Return New DataTable()
        Dim texto As String = If(filtro, "").Trim()
        Return Consultar(
            "SELECT e.ID, e.Razon_Social, e.NIT, e.NRC, " &
            "       IFNULL(p.Nivel_Riesgo, 'Sin perfil') AS Riesgo, e.Activo " &
            "FROM Entidades_Legales e " &
            "LEFT JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
            "WHERE (@filtro IS NULL OR e.Razon_Social LIKE @like OR e.NIT LIKE @like OR e.NRC LIKE @like) " &
            "ORDER BY e.Razon_Social",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
            End Sub)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If id <= 0 OrElse Not Permisos.TieneAcceso("entidades.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT e.*, p.ID AS ID_Perfil, p.Nivel_Riesgo, p.Origen_Fondos_Verificado, " &
            "       p.Detalle_Origen_Fondos, p.Fecha_Verificacion_Listas, p.Observaciones AS Observaciones_LDAFT " &
            "FROM Entidades_Legales e " &
            "LEFT JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
            "WHERE e.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    ''' <summary>
    ''' Guarda entidad, perfil LDA/FT y auditoría en una única transacción.
    ''' Si falla el perfil o la auditoría, la entidad también se revierte.
    ''' </summary>
    Public Function GuardarCompleto(entidad As EntidadLegal, perfil As PerfilLdaft) As Integer
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar entidades.")
        If entidad Is Nothing Then Throw New ArgumentNullException(NameOf(entidad))
        If entidad.ID < 0 Then Throw New ArgumentException("El identificador de la entidad no es válido.", NameOf(entidad))
        If entidad.ID = 0 AndAlso Not entidad.Activo Then
            Throw New InvalidOperationException("Una entidad nueva debe registrarse activa; la baja se realiza mediante la operación autorizada.")
        End If
        ValidarDatosEntidad(entidad, perfil)
        NormalizarDatosEntidad(entidad, perfil)
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim idEntidad As Integer = entidad.ID

                If idEntidad <= 0 Then
                    idEntidad = InsertarYObtenerId(
                        conexion, transaccion,
                        "INSERT INTO Entidades_Legales (Razon_Social, NIT, NRC, Domicilio, Activo) " &
                        "VALUES (@razon, @nit, @nrc, @domicilio, @activo)",
                        Sub(comando As MySqlCommand)
                            AgregarParametros(comando, entidad)
                        End Sub)

                    AuditarEnTransaccion(conexion, transaccion, "CREAR", "Entidades_Legales",
                                         idEntidad, "Entidad legal registrada")
                Else
                    Dim razonActual As String = Nothing
                    Dim nitActual As String = Nothing
                    Dim nrcActual As String = Nothing
                    Dim domicilioActual As String = Nothing
                    Dim activoActual As Boolean = False
                    Using comandoExiste As New MySqlCommand(
                            "SELECT Razon_Social, NIT, NRC, Domicilio, Activo FROM Entidades_Legales WHERE ID = @id FOR UPDATE",
                            conexion, transaccion)
                        Parametro(comandoExiste, "@id", idEntidad)
                        Using lector As MySqlDataReader = comandoExiste.ExecuteReader()
                            If Not lector.Read() Then
                                Throw New InvalidOperationException("La entidad que intenta modificar ya no existe.")
                            End If
                            razonActual = Convert.ToString(lector("Razon_Social"))
                            nitActual = Convert.ToString(lector("NIT"))
                            nrcActual = Convert.ToString(lector("NRC"))
                            domicilioActual = Convert.ToString(lector("Domicilio"))
                            activoActual = Convert.ToInt32(lector("Activo")) = 1
                        End Using
                    End Using

                    If entidad.Activo <> activoActual Then
                        Throw New InvalidOperationException(
                            "El estado activo de la entidad no se cambia desde la edición general. Utilice la operación autorizada de desactivación.")
                    End If

                    Dim representantesExistentes As Integer
                    Using comandoReferencias As New MySqlCommand(
                        "SELECT COUNT(*) FROM Representantes_Legales WHERE ID_Entidad = @id",
                        conexion, transaccion)
                        Parametro(comandoReferencias, "@id", idEntidad)
                        representantesExistentes = Convert.ToInt32(comandoReferencias.ExecuteScalar())
                    End Using

                    If representantesExistentes > 0 AndAlso
                       (Not TextoIgual(entidad.RazonSocial, razonActual) OrElse
                        Not TextoIgual(entidad.NIT, nitActual) OrElse
                        Not TextoIgual(entidad.NRC, nrcActual)) Then
                        Throw New InvalidOperationException(
                            "No se puede cambiar la razón social, NIT o NRC de una entidad que ya tiene representantes registrados. Use el procedimiento autorizado de rectificación.")
                    End If

                    Dim datosEntidadModificados As Boolean =
                        Not TextoIgual(entidad.RazonSocial, razonActual) OrElse
                        Not TextoIgual(entidad.NIT, nitActual) OrElse
                        Not TextoIgual(entidad.NRC, nrcActual) OrElse
                        Not TextoIgual(entidad.Domicilio, domicilioActual)

                    Ejecutar(conexion, transaccion,
                        "UPDATE Entidades_Legales SET Razon_Social = @razon, NIT = @nit, NRC = @nrc, " &
                        "       Domicilio = @domicilio, Activo = @activo WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            AgregarParametros(comando, entidad)
                            Parametro(comando, "@id", idEntidad)
                        End Sub)

                    If datosEntidadModificados Then
                        AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Entidades_Legales",
                                             idEntidad, "Datos de entidad legal actualizados")
                    End If
                End If

                If perfil IsNot Nothing Then
                    perfil.IDCliente = Nothing
                    perfil.IDEntidad = idEntidad

                    Dim accionPerfil As String = "CREAR"
                    Dim perfilExiste As Boolean = False
                    Dim riesgoActual As String = Nothing
                    Dim origenActual As Boolean = False
                    Dim detalleActual As String = Nothing
                    Dim fechaActual As Date? = Nothing
                    Dim observacionesActual As String = Nothing
                    Using comandoPerfilExiste As New MySqlCommand(
                        "SELECT Nivel_Riesgo, Origen_Fondos_Verificado, Detalle_Origen_Fondos, Fecha_Verificacion_Listas, Observaciones FROM Perfil_LDAFT WHERE ID_Entidad = @idEntidad FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comandoPerfilExiste, "@idEntidad", idEntidad)
                        Using lectorPerfil As MySqlDataReader = comandoPerfilExiste.ExecuteReader()
                            If lectorPerfil.Read() Then
                                perfilExiste = True
                                riesgoActual = Convert.ToString(lectorPerfil("Nivel_Riesgo"))
                                origenActual = Convert.ToBoolean(lectorPerfil("Origen_Fondos_Verificado"))
                                detalleActual = Convert.ToString(lectorPerfil("Detalle_Origen_Fondos"))
                                observacionesActual = If(
                                    lectorPerfil.IsDBNull(lectorPerfil.GetOrdinal("Observaciones")),
                                    Nothing,
                                    Convert.ToString(lectorPerfil("Observaciones")))
                                If Not lectorPerfil.IsDBNull(lectorPerfil.GetOrdinal("Fecha_Verificacion_Listas")) Then
                                    fechaActual = Convert.ToDateTime(lectorPerfil("Fecha_Verificacion_Listas")).Date
                                End If
                            End If
                        End Using
                    End Using

                    ' FrmEntidades no ofrece un campo para editar Observaciones.
                    ' Si el formulario omite ese dato, conservar el valor persistido
                    ' en lugar de sobrescribirlo accidentalmente con NULL.
                    If perfilExiste AndAlso perfil.Observaciones Is Nothing Then
                        perfil.Observaciones = observacionesActual
                    End If

                    If perfilExiste Then accionPerfil = "MODIFICAR"
                    Dim perfilModificado As Boolean =
                        Not perfilExiste OrElse
                        Not String.Equals(perfil.NivelRiesgo, riesgoActual, StringComparison.Ordinal) OrElse
                        perfil.OrigenFondosVerificado <> origenActual OrElse
                        Not TextoIgual(perfil.DetalleOrigenFondos, detalleActual) OrElse
                        Not TextoIgual(perfil.Observaciones, observacionesActual) OrElse
                        (perfil.FechaVerificacionListas.HasValue <> fechaActual.HasValue) OrElse
                        (perfil.FechaVerificacionListas.HasValue AndAlso fechaActual.HasValue AndAlso
                         perfil.FechaVerificacionListas.Value.Date <> fechaActual.Value.Date)

                    If perfilModificado Then
                        Ejecutar(conexion, transaccion,
                            "INSERT INTO Perfil_LDAFT (ID_Cliente, ID_Entidad, Nivel_Riesgo, Origen_Fondos_Verificado, " &
                            "       Detalle_Origen_Fondos, Fecha_Verificacion_Listas, Observaciones) " &
                            "VALUES (NULL, @idEntidad, @riesgo, @origen, @detalle, @fecha, @observaciones) " &
                            "ON DUPLICATE KEY UPDATE Nivel_Riesgo = VALUES(Nivel_Riesgo), " &
                            "       Origen_Fondos_Verificado = VALUES(Origen_Fondos_Verificado), " &
                            "       Detalle_Origen_Fondos = VALUES(Detalle_Origen_Fondos), " &
                            "       Fecha_Verificacion_Listas = VALUES(Fecha_Verificacion_Listas), " &
                            "       Fecha_Actualizacion = NOW(), " &
                            "       Observaciones = VALUES(Observaciones)",
                            Sub(comando As MySqlCommand)
                                Parametro(comando, "@idEntidad", idEntidad)
                                Parametro(comando, "@riesgo", perfil.NivelRiesgo)
                                Parametro(comando, "@origen", perfil.OrigenFondosVerificado)
                                Parametro(comando, "@detalle", perfil.DetalleOrigenFondos)
                                Parametro(comando, "@fecha", If(perfil.FechaVerificacionListas.HasValue,
                                                                 CObj(perfil.FechaVerificacionListas.Value), DBNull.Value))
                                Parametro(comando, "@observaciones", perfil.Observaciones)
                            End Sub)

                        AuditarEnTransaccion(conexion, transaccion, accionPerfil, "Perfil_LDAFT",
                                             idEntidad, If(accionPerfil = "CREAR",
                                                               "Perfil LDA/FT de entidad creado",
                                                               "Perfil LDA/FT de entidad actualizado"))
                    End If
                End If

                Return idEntidad
            End Function)
    End Function

    Public Function Desactivar(id As Integer) As Integer
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar entidades.")
        If id <= 0 Then Return 0
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Entidades_Legales SET Activo = 0 WHERE ID = @id AND Activo = 1",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)

                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "DESACTIVAR", "Entidades_Legales",
                                         id, "Entidad desactivada")
                End If
                Return filas
            End Function)
    End Function

    Public Function Representantes(idEntidad As Integer) As DataTable
        If idEntidad <= 0 OrElse Not Permisos.TieneAcceso("entidades.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT r.ID, c.ID AS ID_Cliente, CONCAT(c.Nombres, ' ', c.Apellidos) AS Nombre, " &
            "       c.DUI, r.Cargo, r.Tipo_Representacion " &
            "FROM Representantes_Legales r " &
            "INNER JOIN Clientes c ON c.ID = r.ID_Cliente " &
            "WHERE r.ID_Entidad = @id ORDER BY c.Apellidos, c.Nombres",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idEntidad)
            End Sub)
    End Function

    Public Function ClientesCombo() As DataTable
        If Not Permisos.TieneAccesoAlguno("entidades.ver", "entidades.gestionar") Then Return New DataTable()
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos, ' | DUI: ', " &
            "       IFNULL(NULLIF(DUI, ''), 'No registrado')) AS Nombre " &
            "FROM Clientes WHERE Activo = 1 ORDER BY Apellidos, Nombres, ID")
    End Function

    Public Function AgregarRepresentante(idEntidad As Integer, idCliente As Integer, cargo As String, tipo As String) As Respuesta
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar representantes.")
        If idEntidad <= 0 OrElse idCliente <= 0 Then Return Respuesta.Fallo("Seleccione una entidad y un cliente válidos.")
        If If(cargo, "").Trim().Length > 120 OrElse If(tipo, "").Trim().Length > 80 Then
            Return Respuesta.Fallo("El cargo o tipo de representación supera la longitud permitida.")
        End If
        If Array.IndexOf(New String() {"Legal", "Voluntario", "Judicial"}, If(tipo, "").Trim()) < 0 Then
            Return Respuesta.Fallo("El tipo de representación no es válido.")
        End If
        cargo = If(cargo, "").Trim()
        tipo = If(tipo, "").Trim()
        Try
            EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim entidadActiva As Integer
                    Using comprobarEntidad As New MySqlCommand(
                        "SELECT Activo FROM Entidades_Legales WHERE ID = @id FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comprobarEntidad, "@id", idEntidad)
                        Dim valorEntidad As Object = comprobarEntidad.ExecuteScalar()
                        If valorEntidad Is Nothing OrElse Convert.IsDBNull(valorEntidad) Then
                            Throw New InvalidOperationException("La entidad seleccionada no existe.")
                        End If
                        entidadActiva = Convert.ToInt32(valorEntidad)
                    End Using
                    If entidadActiva <> 1 Then Throw New InvalidOperationException("No se pueden agregar representantes a una entidad inactiva.")

                    Dim clienteActivo As Integer
                    Using comprobarCliente As New MySqlCommand(
                        "SELECT Activo FROM Clientes WHERE ID = @id FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comprobarCliente, "@id", idCliente)
                        Dim valorCliente As Object = comprobarCliente.ExecuteScalar()
                        If valorCliente Is Nothing OrElse Convert.IsDBNull(valorCliente) Then
                            Throw New InvalidOperationException("El cliente seleccionado no existe.")
                        End If
                        clienteActivo = Convert.ToInt32(valorCliente)
                    End Using
                    If clienteActivo <> 1 Then Throw New InvalidOperationException("No se pueden agregar clientes inactivos como representantes.")

                    Ejecutar(conexion, transaccion,
                        "INSERT INTO Representantes_Legales (ID_Entidad, ID_Cliente, Cargo, Tipo_Representacion) " &
                        "VALUES (@entidad, @cliente, @cargo, @tipo)",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@entidad", idEntidad)
                            Parametro(comando, "@cliente", idCliente)
                            Parametro(comando, "@cargo", cargo)
                            Parametro(comando, "@tipo", tipo)
                        End Sub)

                    AuditarEnTransaccion(conexion, transaccion, "CREAR", "Representantes_Legales",
                                         Nothing, "Representante " & idCliente & " agregado a entidad " & idEntidad)
                    Return 1
                End Function)
            Return Respuesta.Ok("Representante agregado.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ese cliente ya está registrado como representante de la entidad.")
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("La entidad o el cliente ya no existe; actualice la lista e inténtelo de nuevo.")
        End Try
    End Function

    Public Function QuitarRepresentante(id As Integer) As Integer
        If Not Permisos.TieneAcceso("entidades.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar representantes.")
        If id <= 0 Then Return 0
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim detalleHistorico As String = Nothing
                Using consultar As New MySqlCommand(
                    "SELECT e.Razon_Social, c.Nombres, c.Apellidos, c.DUI, r.Cargo, r.Tipo_Representacion " &
                    "FROM Representantes_Legales r " &
                    "INNER JOIN Entidades_Legales e ON e.ID = r.ID_Entidad " &
                    "INNER JOIN Clientes c ON c.ID = r.ID_Cliente " &
                    "WHERE r.ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultar, "@id", id)
                    Using lector As MySqlDataReader = consultar.ExecuteReader()
                        If Not lector.Read() Then Return 0
                        detalleHistorico = "Representante: " & Convert.ToString(lector("Nombres")) & " " &
                            Convert.ToString(lector("Apellidos")) & "; DUI: " &
                            If(lector.IsDBNull(lector.GetOrdinal("DUI")), "No registrado", Convert.ToString(lector("DUI"))) &
                            "; entidad: " & Convert.ToString(lector("Razon_Social")) & "; cargo: " &
                            Convert.ToString(lector("Cargo")) & "; representación: " &
                            Convert.ToString(lector("Tipo_Representacion"))
                    End Using
                End Using

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "DELETE FROM Representantes_Legales WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)

                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "ELIMINAR", "Representantes_Legales",
                                         id, detalleHistorico)
                End If
                Return filas
            End Function)
    End Function

    Private Sub ValidarDatosEntidad(entidad As EntidadLegal, perfil As PerfilLdaft)
        If String.IsNullOrWhiteSpace(entidad.RazonSocial) OrElse entidad.RazonSocial.Trim().Length > 200 Then
            Throw New ArgumentException("La razón social es obligatoria y no puede superar 200 caracteres.", NameOf(entidad))
        End If
        If If(entidad.NRC, "").Length > 30 OrElse If(entidad.Domicilio, "").Length > 300 OrElse
           If(entidad.NIT, "").Length > 20 Then
            Throw New ArgumentException("Uno o más datos de la entidad superan la longitud permitida.", NameOf(entidad))
        End If
        If Not String.IsNullOrWhiteSpace(entidad.NIT) AndAlso Not ValidadorIdentidad.EsFormatoNitValido(entidad.NIT) Then
            Throw New ArgumentException("El NIT de la entidad debe tener 14 dígitos.", NameOf(entidad))
        End If
        If perfil IsNot Nothing Then
            If Array.IndexOf(New String() {"Bajo", "Medio", "Alto"}, If(perfil.NivelRiesgo, "")) < 0 Then
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

    Private Sub NormalizarDatosEntidad(entidad As EntidadLegal, perfil As PerfilLdaft)
        entidad.RazonSocial = entidad.RazonSocial.Trim()
        entidad.NRC = If(entidad.NRC, "").Trim()
        entidad.Domicilio = If(entidad.Domicilio, "").Trim()
        If Not String.IsNullOrWhiteSpace(entidad.NIT) Then
            entidad.NIT = ValidadorIdentidad.FormatearNit(entidad.NIT)
        End If
        If perfil IsNot Nothing Then
            perfil.DetalleOrigenFondos = If(perfil.DetalleOrigenFondos, "").Trim()
            If perfil.Observaciones IsNot Nothing Then
                perfil.Observaciones = perfil.Observaciones.Trim()
            End If
        End If
    End Sub

    Private Shared Function TextoIgual(valorNuevo As String, valorActual As String) As Boolean
        Return String.Equals(If(valorNuevo, "").Trim(), If(valorActual, "").Trim(),
                             StringComparison.Ordinal)
    End Function

    Private Sub AgregarParametros(comando As MySqlCommand, entidad As EntidadLegal)
        Parametro(comando, "@razon", entidad.RazonSocial)
        Parametro(comando, "@nit", entidad.NIT)
        Parametro(comando, "@nrc", entidad.NRC)
        Parametro(comando, "@domicilio", entidad.Domicilio)
        Parametro(comando, "@activo", entidad.Activo)
    End Sub

End Class
