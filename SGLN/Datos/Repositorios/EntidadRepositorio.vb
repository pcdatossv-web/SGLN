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
                                         idEntidad, entidad.RazonSocial)
                Else
                    Dim razonActual As String = Nothing
                    Dim nitActual As String = Nothing
                    Dim nrcActual As String = Nothing
                    Using comandoExiste As New MySqlCommand(
                            "SELECT Razon_Social, NIT, NRC FROM Entidades_Legales WHERE ID = @id FOR UPDATE",
                            conexion, transaccion)
                        Parametro(comandoExiste, "@id", idEntidad)
                        Using lector As MySqlDataReader = comandoExiste.ExecuteReader()
                            If Not lector.Read() Then
                                Throw New InvalidOperationException("La entidad que intenta modificar ya no existe.")
                            End If
                            razonActual = Convert.ToString(lector("Razon_Social"))
                            nitActual = Convert.ToString(lector("NIT"))
                            nrcActual = Convert.ToString(lector("NRC"))
                        End Using
                    End Using

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

                    Ejecutar(conexion, transaccion,
                        "UPDATE Entidades_Legales SET Razon_Social = @razon, NIT = @nit, NRC = @nrc, " &
                        "       Domicilio = @domicilio, Activo = @activo WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            AgregarParametros(comando, entidad)
                            Parametro(comando, "@id", idEntidad)
                        End Sub)

                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Entidades_Legales",
                                         idEntidad, entidad.RazonSocial)
                End If

                If perfil IsNot Nothing Then
                    perfil.IDCliente = Nothing
                    perfil.IDEntidad = idEntidad

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

                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Perfil_LDAFT",
                                         idEntidad, "Perfil LDA/FT de entidad actualizado")
                End If

                Return idEntidad
            End Function)
    End Function

    Public Sub Desactivar(id As Integer)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Entidades_Legales SET Activo = 0 WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)

                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "DESACTIVAR", "Entidades_Legales",
                                         id, "Entidad desactivada")
                End If
                Return filas
            End Function)
    End Sub

    Public Function Representantes(idEntidad As Integer) As DataTable
        Return Consultar(
            "SELECT r.ID, c.ID AS ID_Cliente, CONCAT(c.Nombres, ' ', c.Apellidos) AS Nombre, " &
            "       r.Cargo, r.Tipo_Representacion " &
            "FROM Representantes_Legales r " &
            "INNER JOIN Clientes c ON c.ID = r.ID_Cliente " &
            "WHERE r.ID_Entidad = @id ORDER BY c.Apellidos, c.Nombres",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idEntidad)
            End Sub)
    End Function

    Public Function ClientesCombo() As DataTable
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos) AS Nombre FROM Clientes " &
            "WHERE Activo = 1 ORDER BY Apellidos, Nombres")
    End Function

    Public Function AgregarRepresentante(idEntidad As Integer, idCliente As Integer, cargo As String, tipo As String) As Respuesta
        Try
            EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
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

    Public Sub QuitarRepresentante(id As Integer)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "DELETE FROM Representantes_Legales WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)

                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "ELIMINAR", "Representantes_Legales",
                                         id, "Representante removido")
                End If
                Return filas
            End Function)
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
