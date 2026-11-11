Imports System
Imports System.Collections.Generic
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de roles y su relación con el catálogo de permisos.
''' Los candados del Administrador también existen como triggers en la base:
''' aquí solo se adelantan para dar un mensaje claro al usuario.
''' </summary>
Public Class RolRepositorio
    Inherits RepositorioBase

    Public Function Listar() As DataTable
        Return Consultar(
            "SELECT r.ID, r.Nombre, IFNULL(r.Descripcion, '') AS Descripcion, r.Activo, r.Protegido, " &
            "       (SELECT COUNT(*) FROM Usuarios u WHERE u.ID_Rol = r.ID) AS Usuarios, " &
            "       (SELECT COUNT(*) FROM Rol_Permiso rp WHERE rp.ID_Rol = r.ID) AS Permisos " &
            "FROM Roles r ORDER BY r.Protegido DESC, r.Nombre")
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return Consultar(
            "SELECT r.ID, r.Nombre, IFNULL(r.Descripcion, '') AS Descripcion, r.Activo, r.Protegido, " &
            "       (SELECT COUNT(*) FROM Usuarios u WHERE u.ID_Rol = r.ID) AS Usuarios, " &
            "       (SELECT COUNT(*) FROM Rol_Permiso rp WHERE rp.ID_Rol = r.ID) AS Permisos " &
            "FROM Roles r WHERE r.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    ''' <summary>Roles activos para el combo de alta/edición de usuarios.</summary>
    Public Function RolesActivos() As DataTable
        Return Consultar(
            "SELECT ID, Nombre FROM Roles WHERE Activo = 1 ORDER BY Protegido DESC, Nombre")
    End Function

    Public Function CatalogoPermisos() As DataTable
        Return Consultar(
            "SELECT Codigo, Modulo, Descripcion FROM Permisos ORDER BY Modulo, Codigo")
    End Function

    Public Function PermisosDeRol(idRol As Integer) As DataTable
        Return Consultar(
            "SELECT Codigo FROM Rol_Permiso WHERE ID_Rol = @rol",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@rol", idRol)
            End Sub)
    End Function

    Public Function Crear(nombre As String, descripcion As String,
                          codigos As List(Of String)) As Respuesta
        Try
            Dim nuevoId As Integer = InsertarYObtenerId(
                "INSERT INTO Roles (Nombre, Descripcion, Activo, Protegido) VALUES (@nombre, @descripcion, 1, 0)",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nombre", nombre.Trim())
                    Parametro(comando, "@descripcion", descripcion)
                End Sub)

            GuardarPermisos(nuevoId, codigos)
            Auditar("CREAR", "Roles", nuevoId, "Rol " & nombre.Trim())
            Return Respuesta.Ok("Rol creado correctamente.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un rol con ese nombre.")
        End Try
    End Function

    Public Function Actualizar(id As Integer, nombre As String, descripcion As String,
                               activo As Boolean, codigos As List(Of String)) As Respuesta
        Try
            Dim filas As Integer = Ejecutar(
                "UPDATE Roles SET Nombre = @nombre, Descripcion = @descripcion, Activo = @activo " &
                "WHERE ID = @id",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nombre", nombre.Trim())
                    Parametro(comando, "@descripcion", descripcion)
                    Parametro(comando, "@activo", activo)
                    Parametro(comando, "@id", id)
                End Sub)

            If filas = 0 Then Return Respuesta.Fallo("El rol ya no existe.")

            GuardarPermisos(id, codigos)
            Auditar("MODIFICAR", "Roles", id, "Rol " & nombre.Trim())
            Return Respuesta.Ok("Rol actualizado.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un rol con ese nombre.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        End Try
    End Function

    Public Function Eliminar(id As Integer) As Respuesta
        Try
            Dim filas As Integer = Ejecutar(
                "DELETE FROM Roles WHERE ID = @id",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@id", id)
                End Sub)

            If filas = 0 Then Return Respuesta.Fallo("El rol ya no existe.")

            Auditar("ELIMINAR", "Roles", id, "Rol eliminado")
            Return Respuesta.Ok("Rol eliminado.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1451
            ' 1451 = no se puede borrar porque tiene usuarios asignados.
            Return Respuesta.Fallo("No se puede eliminar el rol porque tiene usuarios asignados. " &
                                   "Cámbieles el rol o desactívelos primero.")
        End Try
    End Function

    ''' <summary>
    ''' Sincroniza la matriz de permisos del rol dentro de una transacción:
    ''' o se guarda todo, o no se cambia nada. Solo borra los permisos que
    ''' realmente se quitaron, para no chocar con el candado del Administrador.
    ''' </summary>
    Public Function GuardarPermisos(idRol As Integer, codigos As List(Of String)) As Respuesta
        Dim nuevos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If codigos IsNot Nothing Then
            For Each codigo As String In codigos
                If Not String.IsNullOrWhiteSpace(codigo) Then nuevos.Add(codigo)
            Next
        End If

        Try
            Using conexion As MySqlConnection = CrearConexion()
                Using transaccion As MySqlTransaction = conexion.BeginTransaction()
                    Try
                        Dim actuales As New List(Of String)()
                        Using comando As New MySqlCommand(
                            "SELECT Codigo FROM Rol_Permiso WHERE ID_Rol = @rol", conexion, transaccion)
                            Parametro(comando, "@rol", idRol)
                            Using lector As MySqlDataReader = comando.ExecuteReader()
                                While lector.Read()
                                    actuales.Add(lector.GetString(0))
                                End While
                            End Using
                        End Using

                        ' Se borran únicamente los permisos retirados: el trigger
                        ' del Administrador bloquea quitar los dos mínimos.
                        For Each codigo As String In actuales
                            If nuevos.Contains(codigo) Then Continue For
                            Using comando As New MySqlCommand(
                                "DELETE FROM Rol_Permiso WHERE ID_Rol = @rol AND Codigo = @codigo",
                                conexion, transaccion)
                                Parametro(comando, "@rol", idRol)
                                Parametro(comando, "@codigo", codigo)
                                comando.ExecuteNonQuery()
                            End Using
                        Next

                        For Each codigo As String In nuevos
                            Using comando As New MySqlCommand(
                                "INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo) VALUES (@rol, @codigo)",
                                conexion, transaccion)
                                Parametro(comando, "@rol", idRol)
                                Parametro(comando, "@codigo", codigo)
                                comando.ExecuteNonQuery()
                            End Using
                        Next

                        transaccion.Commit()
                    Catch
                        transaccion.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            Return Respuesta.Ok("Permisos del rol guardados.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("Uno de los permisos ya no existe en el catálogo.")
        End Try
    End Function

End Class
