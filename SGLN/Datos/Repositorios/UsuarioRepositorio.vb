Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Autenticación, alta y mantenimiento de usuarios. El rol ya no es un texto
''' libre: se guarda ID_Rol y se valida contra la tabla Roles.
''' </summary>
Public Class UsuarioRepositorio
    Inherits RepositorioBase

    Public Function Autenticar(usuario As String, clave As String) As UsuarioSesion
        Dim tabla As DataTable = Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, u.Clave_Hash, u.Clave_Salt, " &
            "       IFNULL(u.ID_Rol, 0) AS ID_Rol, IFNULL(r.Nombre, '') AS Rol, " &
            "       IFNULL(r.Protegido, 0) AS Rol_Protegido " &
            "FROM Usuarios u LEFT JOIN Roles r ON r.ID = u.ID_Rol " &
            "WHERE u.Usuario = @usuario AND u.Activo = 1 AND IFNULL(r.Activo, 1) = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@usuario", usuario)
            End Sub)

        If tabla.Rows.Count = 0 Then Return Nothing

        Dim fila As DataRow = tabla.Rows(0)
        Dim hash As String = Convert.ToString(fila("Clave_Hash"))
        Dim salt As String = Convert.ToString(fila("Clave_Salt"))

        If Not SeguridadClave.Verificar(clave, hash, salt) Then Return Nothing

        Dim sesion As New UsuarioSesion With {
            .ID = Convert.ToInt32(fila("ID")),
            .Usuario = Convert.ToString(fila("Usuario")),
            .NombreCompleto = Convert.ToString(fila("Nombre_Completo")),
            .IDRol = Convert.ToInt32(fila("ID_Rol")),
            .Rol = Convert.ToString(fila("Rol")),
            .RolProtegido = Convert.ToInt32(fila("Rol_Protegido")) = 1
        }

        Auditar(sesion.Usuario, "INGRESO", "Usuarios", sesion.ID, "Inicio de sesión")
        Return sesion
    End Function

    Public Function Listar() As DataTable
        Return Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, IFNULL(u.ID_Rol, 0) AS ID_Rol, " &
            "       IFNULL(r.Nombre, 'Sin rol') AS Rol, u.Activo, u.Fecha_Creacion " &
            "FROM Usuarios u LEFT JOIN Roles r ON r.ID = u.ID_Rol ORDER BY u.Usuario")
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return Consultar(
            "SELECT ID, Usuario, Nombre_Completo, IFNULL(ID_Rol, 0) AS ID_Rol, Activo " &
            "FROM Usuarios WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function Crear(usuario As String, nombreCompleto As String, clave As String,
                          idRol As Integer, activo As Boolean) As Respuesta
        Dim credencial As SalHash = SeguridadClave.CrearHash(clave)

        Try
            Dim nuevoId As Integer = InsertarYObtenerId(
                "INSERT INTO Usuarios (Usuario, Nombre_Completo, Clave_Hash, Clave_Salt, ID_Rol, Activo) " &
                "VALUES (@usuario, @nombre, @hash, @salt, @rol, @activo)",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@usuario", usuario.Trim())
                    Parametro(comando, "@nombre", nombreCompleto.Trim())
                    Parametro(comando, "@hash", credencial.Hash)
                    Parametro(comando, "@salt", credencial.Salt)
                    Parametro(comando, "@rol", idRol)
                    Parametro(comando, "@activo", activo)
                End Sub)

            Auditar("CREAR", "Usuarios", nuevoId, usuario.Trim() & " con rol " & idRol)
            Return Respuesta.Ok("Usuario creado correctamente.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un usuario con ese nombre de acceso.")
        End Try
    End Function

    Public Function Actualizar(id As Integer, nombreCompleto As String, idRol As Integer,
                               activo As Boolean) As Respuesta
        Try
            Dim filas As Integer = Ejecutar(
                "UPDATE Usuarios SET Nombre_Completo = @nombre, ID_Rol = @rol, Activo = @activo " &
                "WHERE ID = @id",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nombre", nombreCompleto.Trim())
                    Parametro(comando, "@rol", idRol)
                    Parametro(comando, "@activo", activo)
                    Parametro(comando, "@id", id)
                End Sub)

            If filas = 0 Then Return Respuesta.Fallo("El usuario ya no existe.")

            Auditar("MODIFICAR", "Usuarios", id, "Rol " & idRol & ", activo = " & activo)
            Return Respuesta.Ok("Usuario actualizado.")
        Catch ex As MySqlException When ex.Number = 1644
            ' 1644 = SIGNAL del trigger que protege al último administrador.
            Return Respuesta.Fallo(ex.Message)
        End Try
    End Function

    Public Function CambiarClave(idUsuario As Integer, claveNueva As String) As Respuesta
        If String.IsNullOrWhiteSpace(claveNueva) OrElse claveNueva.Length < 8 Then
            Return Respuesta.Fallo("La nueva clave debe tener al menos 8 caracteres.")
        End If

        Dim credencial As SalHash = SeguridadClave.CrearHash(claveNueva)
        Ejecutar(
            "UPDATE Usuarios SET Clave_Hash = @hash, Clave_Salt = @salt WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@hash", credencial.Hash)
                Parametro(comando, "@salt", credencial.Salt)
                Parametro(comando, "@id", idUsuario)
            End Sub)

        Auditar("MODIFICAR", "Usuarios", idUsuario, "Cambio de clave")
        Return Respuesta.Ok("Clave actualizada correctamente.")
    End Function

    Public Function VerificarClave(idUsuario As Integer, clave As String) As Boolean
        Dim tabla As DataTable = Consultar(
            "SELECT Clave_Hash, Clave_Salt FROM Usuarios WHERE ID = @id AND Activo = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idUsuario)
            End Sub)

        If tabla.Rows.Count = 0 Then Return False

        Dim fila As DataRow = tabla.Rows(0)
        Return SeguridadClave.Verificar(clave, Convert.ToString(fila("Clave_Hash")), Convert.ToString(fila("Clave_Salt")))
    End Function

    ''' <summary>
    ''' Cuenta los administradores activos distintos de idExcluir. Sirve para
    ''' impedir que el sistema quede sin acceso total desde la interfaz; el
    ''' trigger de la base hace la misma validación.
    ''' </summary>
    Public Function ContarAdministradoresActivos(idExcluir As Integer) As Integer
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
            "WHERE r.Protegido = 1 AND u.Activo = 1 AND u.ID <> @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idExcluir)
            End Sub)
    End Function

    Public Function EsAdministrador(id As Integer) As Boolean
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
            "WHERE u.ID = @id AND r.Protegido = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub) > 0
    End Function

End Class
