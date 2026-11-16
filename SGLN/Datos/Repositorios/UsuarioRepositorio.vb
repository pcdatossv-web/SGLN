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

    ' Credencial ficticia válida en formato Base64 para igualar el coste PBKDF2
    ' cuando el nombre no existe. No corresponde a ninguna cuenta real.
    Private Const HashTiempoConstante As String = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="
    Private Const SaltTiempoConstante As String = "AAAAAAAAAAAAAAAAAAAAAA=="

    Public Function Autenticar(usuario As String, clave As String) As UsuarioSesion
        Dim tabla As DataTable = Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, u.Clave_Hash, u.Clave_Salt, " &
            "       r.ID AS ID_Rol, r.Nombre AS Rol, r.Protegido AS Rol_Protegido " &
            "FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
            "WHERE u.Usuario = @usuario AND u.Activo = 1 AND r.Activo = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@usuario", usuario)
            End Sub)

        If tabla.Rows.Count = 0 Then
            ' Evita que una respuesta inmediata permita inferir si el usuario existe.
            SeguridadClave.Verificar(clave, HashTiempoConstante, SaltTiempoConstante)
            Return Nothing
        End If

        Dim fila As DataRow = tabla.Rows(0)
        Dim idUsuario As Integer = Convert.ToInt32(fila("ID"))
        Dim hash As String = Convert.ToString(fila("Clave_Hash"))
        Dim salt As String = Convert.ToString(fila("Clave_Salt"))

        If EstaBloqueado(idUsuario) Then
            ' El bloqueo no debe convertirse en un canal de enumeración por tiempo.
            SeguridadClave.Verificar(clave, hash, salt)
            Return Nothing
        End If

        Dim requiereActualizacionHash As Boolean = False
        If Not SeguridadClave.Verificar(clave, hash, salt, requiereActualizacionHash) Then
            RegistrarIntentoFallido(idUsuario, Convert.ToString(fila("Usuario")))
            Return Nothing
        End If

        RestablecerIntentos(idUsuario, Convert.ToString(fila("Usuario")))

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

        Try
            AuditarObligatorio(sesion.Usuario, "INGRESO", "Usuarios", sesion.ID, "Inicio de sesión")
        Catch ex As Exception
            ' Si no se puede conservar el evento de acceso, se deniega el login.
            RegistroErrores.Escribir("Usuarios - auditoría obligatoria de ingreso", ex)
            Return Nothing
        End Try

        Return sesion
    End Function

    Private Function EstaBloqueado(idUsuario As Integer) As Boolean
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Intentos_Acceso " &
            "WHERE Usuario_ID = @id AND Bloqueado_Hasta > NOW()",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idUsuario)
            End Sub) > 0
    End Function

    ''' <summary>Registra intentos fallidos en una ventana de 15 minutos y bloquea al quinto.</summary>
    Private Sub RegistrarIntentoFallido(idUsuario As Integer, usuario As String)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Using insertar As New MySqlCommand(
                    "INSERT IGNORE INTO Intentos_Acceso " &
                    "(Usuario_ID, Usuario, Intentos, Inicio_Ventana, Ultimo_Intento, Bloqueado_Hasta) " &
                    "VALUES (@id, @usuario, 0, NULL, NULL, NULL)", conexion, transaccion)
                    Parametro(insertar, "@id", idUsuario)
                    Parametro(insertar, "@usuario", usuario)
                    insertar.ExecuteNonQuery()
                End Using

                Dim intentos As Integer = 0
                Dim inicioVentana As DateTime? = Nothing
                Dim ahora As DateTime = DateTime.Now

                Using consultar As New MySqlCommand(
                    "SELECT Intentos, Inicio_Ventana, NOW() AS Ahora " &
                    "FROM Intentos_Acceso WHERE Usuario_ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultar, "@id", idUsuario)
                    Using lector As MySqlDataReader = consultar.ExecuteReader()
                        If Not lector.Read() Then
                            Throw New InvalidOperationException("No se pudo bloquear el registro de intentos de acceso.")
                        End If
                        intentos = Convert.ToInt32(lector("Intentos"))
                        If Not lector.IsDBNull(1) Then inicioVentana = Convert.ToDateTime(lector("Inicio_Ventana"))
                        ahora = Convert.ToDateTime(lector("Ahora"))
                    End Using
                End Using

                If Not inicioVentana.HasValue OrElse inicioVentana.Value < ahora.AddMinutes(-15) Then
                    intentos = 1
                    inicioVentana = ahora
                Else
                    intentos += 1
                End If

                Dim bloqueo As Object = DBNull.Value
                If intentos >= 5 Then bloqueo = ahora.AddMinutes(15)

                Ejecutar(conexion, transaccion,
                    "UPDATE Intentos_Acceso SET Usuario = @usuario, Intentos = @intentos, " &
                    "Inicio_Ventana = @inicio, Ultimo_Intento = @ahora, Bloqueado_Hasta = @bloqueo " &
                    "WHERE Usuario_ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@usuario", usuario)
                        Parametro(comando, "@intentos", intentos)
                        Parametro(comando, "@inicio", inicioVentana.Value)
                        Parametro(comando, "@ahora", ahora)
                        Parametro(comando, "@bloqueo", bloqueo)
                        Parametro(comando, "@id", idUsuario)
                    End Sub)
                Return intentos
            End Function)
    End Sub

    Private Sub RestablecerIntentos(idUsuario As Integer, usuario As String)
        Ejecutar(
            "UPDATE Intentos_Acceso SET Usuario = @usuario, Intentos = 0, " &
            "Inicio_Ventana = NULL, Ultimo_Intento = NOW(), Bloqueado_Hasta = NULL " &
            "WHERE Usuario_ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@usuario", usuario)
                Parametro(comando, "@id", idUsuario)
            End Sub)
    End Sub

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

    ''' <summary>Obtiene el estado vigente de una cuenta para revalidar una sesión.</summary>
    Public Function ObtenerEstadoSesion(id As Integer) As UsuarioSesion
        If id <= 0 Then Return Nothing

        Dim tabla As DataTable = Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, r.ID AS ID_Rol, r.Nombre AS Rol, r.Protegido AS Rol_Protegido " &
            "FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
            "WHERE u.ID = @id AND u.Activo = 1 AND r.Activo = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)

        If tabla.Rows.Count = 0 Then Return Nothing

        Dim fila As DataRow = tabla.Rows(0)
        Return New UsuarioSesion With {
            .ID = Convert.ToInt32(fila("ID")),
            .Usuario = Convert.ToString(fila("Usuario")),
            .NombreCompleto = Convert.ToString(fila("Nombre_Completo")),
            .IDRol = Convert.ToInt32(fila("ID_Rol")),
            .Rol = Convert.ToString(fila("Rol")),
            .RolProtegido = Convert.ToInt32(fila("Rol_Protegido")) = 1
        }
    End Function

    Public Function Crear(usuario As String, nombreCompleto As String, clave As String,
                          idRol As Integer, activo As Boolean) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then
            Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")
        End If
        If EsRolProtegido(idRol) AndAlso Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede asignar el rol protegido Administrador.")
        End If

        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(clave)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)
        Dim credencial As SalHash = SeguridadClave.CrearHash(clave)

        Try
            EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
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
                    AuditarEnTransaccion(conexion, transaccion, "CREAR", "Usuarios", nuevoId,
                        usuario.Trim() & " con rol " & idRol)
                    Return nuevoId
                End Function)
            Return Respuesta.Ok("Usuario creado correctamente.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un usuario con ese nombre de acceso.")
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("El rol seleccionado ya no existe.")
        End Try
    End Function

    Public Function Actualizar(id As Integer, nombreCompleto As String, idRol As Integer,
                               activo As Boolean) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then
            Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")
        End If
        If EsAdministrador(id) AndAlso Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede modificar otra cuenta Administrador.")
        End If
        If EsRolProtegido(idRol) AndAlso Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede asignar el rol protegido Administrador.")
        End If

        Try
            Dim filas As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim afectadas As Integer = Ejecutar(conexion, transaccion,
                        "UPDATE Usuarios SET Nombre_Completo = @nombre, ID_Rol = @rol, Activo = @activo WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@nombre", nombreCompleto.Trim())
                            Parametro(comando, "@rol", idRol)
                            Parametro(comando, "@activo", activo)
                            Parametro(comando, "@id", id)
                        End Sub)

                    If afectadas > 0 Then
                        AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Usuarios", id,
                            "Rol " & idRol & ", activo = " & activo)
                    End If
                    Return afectadas
                End Function)

            If filas = 0 AndAlso Not Existe(id) Then Return Respuesta.Fallo("El usuario ya no existe.")
            Return Respuesta.Ok("Usuario actualizado.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("El rol seleccionado ya no existe.")
        End Try
    End Function

    Public Function CambiarClave(idUsuario As Integer, claveNueva As String) As Respuesta
        If Sesion.UsuarioActual Is Nothing Then
            Return Respuesta.Fallo("Debe iniciar sesión para cambiar o restablecer una clave.")
        End If
        If Sesion.UsuarioActual.ID <> idUsuario AndAlso Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede restablecer la clave de otra cuenta.")
        End If

        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)

        Dim credencial As SalHash = SeguridadClave.CrearHash(claveNueva)
        Dim filas As Integer = EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim afectadas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Usuarios SET Clave_Hash = @hash, Clave_Salt = @salt WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@hash", credencial.Hash)
                        Parametro(comando, "@salt", credencial.Salt)
                        Parametro(comando, "@id", idUsuario)
                    End Sub)
                If afectadas > 0 Then AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Usuarios",
                    idUsuario, "Cambio de clave")
                Return afectadas
            End Function)

        If filas = 0 AndAlso Not Existe(idUsuario) Then Return Respuesta.Fallo("El usuario ya no existe.")
        Return Respuesta.Ok("Clave actualizada correctamente.")
    End Function

    Public Function VerificarClave(idUsuario As Integer, clave As String) As Boolean
        If Sesion.UsuarioActual Is Nothing OrElse Sesion.UsuarioActual.ID <> idUsuario Then Return False

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
        If Not Permisos.TieneAcceso("config.usuarios") OrElse Sesion.UsuarioActual Is Nothing OrElse
           Not Sesion.UsuarioActual.RolProtegido Then
            Return Respuesta.Fallo("Solo un administrador autenticado puede eliminar definitivamente usuarios.")
        End If
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        If Sesion.UsuarioActual.ID = id Then
            Return Respuesta.Fallo("No puede eliminar la cuenta con la que inició sesión.")
        End If

        Try
            ' La cuenta sgln_app no tiene DELETE sobre Usuarios. El procedimiento
            ' DEFINER repite las comprobaciones en MySQL y registra la eliminación
            ' en la misma transacción del servidor.
            Using conexion As MySqlConnection = CrearConexion()
                Using comando As New MySqlCommand(
                    "CALL sp_EliminarUsuarioSeguro(@id, @actor)", conexion)
                    Parametro(comando, "@id", id)
                    Parametro(comando, "@actor", Sesion.UsuarioActual.ID)
                    comando.ExecuteNonQuery()
                End Using
            End Using

            Return Respuesta.Ok("Usuario eliminado. Se conservó la bitácora de creación y eliminación.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - eliminación segura", ex)
            Return Respuesta.Fallo("No se pudo eliminar el usuario. La operación fue cancelada para proteger la integridad de los datos.")
        End Try
    End Function

    Private Shared Function IdentificadorSeguro(nombre As String) As String
        Dim delimitador As String = Convert.ToChar(96).ToString()
        Return delimitador & nombre.Replace(delimitador, delimitador & delimitador) & delimitador
    End Function

    Private Function EsAdministradorAutenticado() As Boolean
        Return Sesion.UsuarioActual IsNot Nothing AndAlso
               Sesion.UsuarioActual.RolProtegido AndAlso
               Permisos.TieneAcceso("config.usuarios") AndAlso Permisos.AccesoTotal
    End Function

    Private Function EsRolProtegido(idRol As Integer) As Boolean
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Roles WHERE ID = @id AND Protegido = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idRol)
            End Sub) > 0
    End Function

    Private Function Existe(id As Integer) As Boolean
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Usuarios WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub) > 0
    End Function

End Class
