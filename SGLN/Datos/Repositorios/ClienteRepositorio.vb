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
                    Using comandoExiste As New MySqlCommand(
                            "SELECT ID FROM Clientes WHERE ID = @id FOR UPDATE", conexion, transaccion)
                        Parametro(comandoExiste, "@id", idCliente)
                        If comandoExiste.ExecuteScalar() Is Nothing Then
                            Throw New InvalidOperationException("El cliente que intenta modificar ya no existe.")
                        End If
                    End Using

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

                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Perfil_LDAFT",
                                         idCliente, "Perfil LDA/FT del cliente actualizado")
                End If

                Return idCliente
            End Function)
    End Function

    Public Sub Desactivar(id As Integer)
        EnTransaccion(
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
