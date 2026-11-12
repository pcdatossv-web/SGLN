Imports System
Imports System.Data
Imports System.Collections.Generic
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

        Dim requiereActualizacionHash As Boolean = False
        If Not SeguridadClave.Verificar(clave, hash, salt, requiereActualizacionHash) Then Return Nothing

        If requiereActualizacionHash Then
            ' Migra silenciosamente hashes SHA1 antiguos al formato SHA256 actual.
            ' La condición sobre el hash anterior evita sobrescribir un cambio
            ' de contraseña concurrente realizado desde otra estación.
            Dim credencialNueva As SalHash = SeguridadClave.CrearHash(clave)
            Ejecutar(
                "UPDATE Usuarios SET Clave_Hash = @nuevoHash, Clave_Salt = @nuevoSalt " &
                "WHERE ID = @id AND Clave_Hash = @hashAnterior",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nuevoHash", credencialNueva.Hash)
                    Parametro(comando, "@nuevoSalt", credencialNueva.Salt)
                    Parametro(comando, "@id", Convert.ToInt32(fila("ID")))
                    Parametro(comando, "@hashAnterior", hash)
                End Sub)
        End If

        Dim sesion As New UsuarioSesion With {
            .ID = Convert.ToInt32(fila("ID")),
            .Usuario = Convert.ToString(fila("Usuario")),
            .NombreCompleto = Convert.ToString(fila("Nombre_Completo")),
            .IDRol = Convert.ToInt32(fila("ID_Rol")),
            .Rol = Convert.ToString(fila("Rol")),
            .RolProtegido = Convert.ToInt32(fila("Rol_Protegido")) = 1,
            .DebeCambiarClave = SeguridadClave.EsClaveInicial(hash, salt)
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

            ' MySQL puede devolver cero filas afectadas cuando los valores enviados
            ' son idénticos a los existentes. Solo se informa que no existe si
            ' una consulta independiente confirma que el registro desapareció.
            If filas = 0 AndAlso Not Existe(id) Then
                Return Respuesta.Fallo("El usuario ya no existe.")
            End If

            Auditar("MODIFICAR", "Usuarios", id, "Rol " & idRol & ", activo = " & activo)
            Return Respuesta.Ok("Usuario actualizado.")
        Catch ex As MySqlException When ex.Number = 1644
            ' 1644 = SIGNAL del trigger que protege al último administrador.
            Return Respuesta.Fallo(ex.Message)
        End Try
    End Function

    Public Function CambiarClave(idUsuario As Integer, claveNueva As String) As Respuesta
        ' La regla se aplica en el repositorio para que ningún formulario o
        ' servicio pueda omitirla al cambiar o restablecer una contraseña.
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)

        Dim credencial As SalHash = SeguridadClave.CrearHash(claveNueva)
        Dim filas As Integer = Ejecutar(
            "UPDATE Usuarios SET Clave_Hash = @hash, Clave_Salt = @salt WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@hash", credencial.Hash)
                Parametro(comando, "@salt", credencial.Salt)
                Parametro(comando, "@id", idUsuario)
            End Sub)

        If filas = 0 AndAlso Not Existe(idUsuario) Then
            Return Respuesta.Fallo("El usuario ya no existe.")
        End If

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


    ''' <summary>
    ''' Elimina únicamente cuentas sin referencias operativas ni actividad
    ''' propia. Bloquea la fila padre y comprueba referencias dentro de una
    ''' transacción serializable. Conserva el evento CREAR de la bitácora.
    ''' </summary>
    Public Function EliminarSiNoTieneHistorial(id As Integer) As Respuesta
        If Sesion.UsuarioActual Is Nothing OrElse Not Sesion.UsuarioActual.RolProtegido OrElse
           Not Permisos.AccesoTotal Then
            Return Respuesta.Fallo("Solo un administrador autenticado puede eliminar definitivamente usuarios.")
        End If

        Using conexion As MySqlConnection = CrearConexion()
            Using transaccion As MySqlTransaction = conexion.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim usuario As String = ""
                    Dim esAdmin As Boolean = False
                    Dim existeUsuario As Boolean = False

                    Using cmd As New MySqlCommand(
                        "SELECT u.Usuario, IFNULL(r.Protegido, 0) AS Protegido " &
                        "FROM Usuarios u LEFT JOIN Roles r ON r.ID = u.ID_Rol " &
                        "WHERE u.ID = @id FOR UPDATE", conexion, transaccion)
                        Parametro(cmd, "@id", id)
                        Using lector As MySqlDataReader = cmd.ExecuteReader()
                            If lector.Read() Then
                                existeUsuario = True
                                usuario = Convert.ToString(lector("Usuario"))
                                esAdmin = Convert.ToInt32(lector("Protegido")) = 1
                            End If
                        End Using
                    End Using

                    If Not existeUsuario Then
                        transaccion.Rollback()
                        Return Respuesta.Fallo("El usuario ya no existe.")
                    End If

                    If esAdmin Then
                        Using cmd As New MySqlCommand(
                            "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
                            "WHERE r.Protegido = 1 AND u.Activo = 1 AND u.ID <> @id",
                            conexion, transaccion)
                            Parametro(cmd, "@id", id)
                            If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then
                                transaccion.Rollback()
                                Return Respuesta.Fallo("No se puede eliminar: debe quedar al menos un administrador activo.")
                            End If
                        End Using
                    End If

                    Using cmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM Audit_Log WHERE Usuario = @usuario " &
                        "AND NOT (Tabla_Afectada = 'Usuarios' AND Accion = 'CREAR' AND ID_Registro = @id)",
                        conexion, transaccion)
                        Parametro(cmd, "@usuario", usuario)
                        Parametro(cmd, "@id", id)
                        If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                            transaccion.Rollback()
                            Return Respuesta.Fallo("No se puede eliminar: el usuario tiene actividad registrada en la bitácora.")
                        End If
                    End Using

                    Dim referencias As New List(Of String())()
                    Using cmd As New MySqlCommand(
                        "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " &
                        "WHERE TABLE_SCHEMA = DATABASE() AND REFERENCED_TABLE_NAME = 'Usuarios' " &
                        "AND REFERENCED_COLUMN_NAME = 'ID'",
                        conexion, transaccion)
                        Using lector As MySqlDataReader = cmd.ExecuteReader()
                            While lector.Read()
                                referencias.Add(New String() {Convert.ToString(lector(0)), Convert.ToString(lector(1))})
                            End While
                        End Using
                    End Using

                    For Each referencia As String() In referencias
                        Dim tabla As String = referencia(0).Replace(ChrW(96), ChrW(96) & ChrW(96))
                        Dim columna As String = referencia(1).Replace(ChrW(96), ChrW(96) & ChrW(96))
                        Using cmd As New MySqlCommand(
                            "SELECT COUNT(*) FROM " & ChrW(96) & tabla & ChrW(96) &
                            " WHERE " & ChrW(96) & columna & ChrW(96) & " = @id",
                            conexion, transaccion)
                            Parametro(cmd, "@id", id)
                            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                                transaccion.Rollback()
                                Return Respuesta.Fallo("No se puede eliminar: existen registros relacionados en " & referencia(0) & ".")
                            End If
                        End Using
                    Next

                    Dim columnasTexto As New List(Of String())()
                    Using cmd As New MySqlCommand(
                        "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS " &
                        "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME <> 'Usuarios' " &
                        "AND DATA_TYPE IN ('char','varchar','tinytext','text','mediumtext','longtext') " &
                        "AND COLUMN_NAME REGEXP 'Usuario|Responsable|Creado|Modificado|Autor|Asignado|Propietario|Firmante|Confirma'",
                        conexion, transaccion)
                        Using lector As MySqlDataReader = cmd.ExecuteReader()
                            While lector.Read()
                                columnasTexto.Add(New String() {Convert.ToString(lector(0)), Convert.ToString(lector(1))})
                            End While
                        End Using
                    End Using

                    For Each referencia As String() In columnasTexto
                        Dim tabla As String = referencia(0).Replace(ChrW(96), ChrW(96) & ChrW(96))
                        Dim columna As String = referencia(1).Replace(ChrW(96), ChrW(96) & ChrW(96))
                        Using cmd As New MySqlCommand(
                            "SELECT COUNT(*) FROM " & ChrW(96) & tabla & ChrW(96) &
                            " WHERE CAST(" & ChrW(96) & columna & ChrW(96) & " AS CHAR) = @usuario",
                            conexion, transaccion)
                            Parametro(cmd, "@usuario", usuario)
                            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                                transaccion.Rollback()
                                Return Respuesta.Fallo("No se puede eliminar: existen referencias históricas al usuario en " & referencia(0) & ".")
                            End If
                        End Using
                    Next

                    Using cmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Usuarios' " &
                        "AND ID_Registro = @id AND Accion <> 'CREAR'",
                        conexion, transaccion)
                        Parametro(cmd, "@id", id)
                        If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                            transaccion.Rollback()
                            Return Respuesta.Fallo("No se puede eliminar: el usuario tiene historial de mantenimiento en la bitácora.")
                        End If
                    End Using

                    Using cmd As New MySqlCommand("DELETE FROM Usuarios WHERE ID = @id", conexion, transaccion)
                        Parametro(cmd, "@id", id)
                        If cmd.ExecuteNonQuery() <> 1 Then
                            transaccion.Rollback()
                            Return Respuesta.Fallo("No se pudo confirmar la eliminación del usuario.")
                        End If
                    End Using

                    ' La eliminación y su evento de auditoría deben confirmarse juntos.
                    ' Si el INSERT de auditoría falla, también se revierte el DELETE.
                    AuditarEnTransaccion(
                        conexion, transaccion, "ELIMINAR", "Usuarios", id,
                        "Cuenta sin referencias operativas ni actividad propia: " & usuario)
                    transaccion.Commit()
                    Return Respuesta.Ok("Usuario eliminado. Se conservó la bitácora de creación y eliminación.")
                Catch ex As Exception
                    Try
                        transaccion.Rollback()
                    Catch
                    End Try
                    RegistroErrores.Escribir("Usuarios - eliminación segura", ex)
                    Return Respuesta.Fallo("No se pudo eliminar el usuario. La operación fue cancelada para proteger la integridad de los datos.")
                End Try
            End Using
        End Using
    End Function

    Private Function Existe(id As Integer) As Boolean
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Usuarios WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub) > 0
    End Function

End Class
