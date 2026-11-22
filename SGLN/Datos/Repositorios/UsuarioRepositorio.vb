Imports System
Imports System.Data
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.Text
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
    Private Shared ReadOnly PatronUsuario As New Regex("\A[a-zA-Z0-9._-]{3,50}\z", RegexOptions.CultureInvariant)

    Public Function Autenticar(usuario As String, clave As String) As UsuarioSesion
        ' Los límites se aplican también en el repositorio, no solo en los
        ' controles visuales. Evita consultas y derivaciones costosas con
        ' entradas desproporcionadas enviadas desde otros puntos de la app.
        If String.IsNullOrWhiteSpace(usuario) OrElse usuario.Length > 50 OrElse
           Not PatronUsuario.IsMatch(usuario) OrElse
           String.IsNullOrEmpty(clave) OrElse clave.Length > 128 Then
            ' Mantener un coste PBKDF2 similar incluso para identificadores mal
            ' formados, sin consultar la base ni revelar si el usuario existe.
            SeguridadClave.Verificar("EntradaNoValida99!", HashTiempoConstante, SaltTiempoConstante)
            Return Nothing
        End If

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
            Dim filasMigradas As Integer = Ejecutar(
                "UPDATE Usuarios SET Clave_Hash = @nuevoHash, Clave_Salt = @nuevoSalt " &
                "WHERE ID = @id AND Clave_Hash = @hashAnterior",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nuevoHash", credencialNueva.Hash)
                    Parametro(comando, "@nuevoSalt", credencialNueva.Salt)
                    Parametro(comando, "@id", Convert.ToInt32(fila("ID")))
                    Parametro(comando, "@hashAnterior", hash)
                End Sub)

            ' Si otra estación cambió la clave durante la migración, no abrir
            ' una sesión basada en la credencial anterior.
            If filasMigradas <> 1 Then Return Nothing
            hash = credencialNueva.Hash
            salt = credencialNueva.Salt
        End If

        Dim sesion As New UsuarioSesion With {
            .ID = Convert.ToInt32(fila("ID")),
            .Usuario = Convert.ToString(fila("Usuario")),
            .NombreCompleto = Convert.ToString(fila("Nombre_Completo")),
            .IDRol = Convert.ToInt32(fila("ID_Rol")),
            .Rol = Convert.ToString(fila("Rol")),
            .RolProtegido = Convert.ToInt32(fila("Rol_Protegido")) = 1,
            .DebeCambiarClave = SeguridadClave.EsClaveInicial(clave),
            .HuellaCredencial = CrearHuellaCredencial(hash, salt)
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
        If Not Permisos.TieneAcceso("config.usuarios") Then Return New DataTable()
        Return Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, IFNULL(u.ID_Rol, 0) AS ID_Rol, " &
            "       IFNULL(r.Nombre, 'Sin rol') AS Rol, u.Activo, u.Fecha_Creacion " &
            "FROM Usuarios u LEFT JOIN Roles r ON r.ID = u.ID_Rol ORDER BY u.Usuario")
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If Not Permisos.TieneAcceso("config.usuarios") Then Return New DataTable()
        Return Consultar(
            "SELECT ID, Usuario, Nombre_Completo, IFNULL(ID_Rol, 0) AS ID_Rol, Activo " &
            "FROM Usuarios WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    ''' <summary>Obtiene el estado vigente de una cuenta para revalidar una sesión.</summary>
    Public Function ObtenerEstadoSesion(id As Integer) As UsuarioSesion
        If id <= 0 OrElse Sesion.UsuarioActual Is Nothing Then Return Nothing
        If Sesion.UsuarioActual.ID <> id AndAlso Not EsAdministradorAutenticado() Then Return Nothing

        Dim tabla As DataTable = Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, u.Clave_Hash, u.Clave_Salt, " &
            "r.ID AS ID_Rol, r.Nombre AS Rol, r.Protegido AS Rol_Protegido " &
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
             .RolProtegido = Convert.ToInt32(fila("Rol_Protegido")) = 1,
            .HuellaCredencial = CrearHuellaCredencial(
                Convert.ToString(fila("Clave_Hash")), Convert.ToString(fila("Clave_Salt")))
        }
    End Function

    Public Function Crear(usuario As String, nombreCompleto As String, clave As String,
                          idRol As Integer, activo As Boolean) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then
            Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")
        End If
        If String.IsNullOrWhiteSpace(usuario) OrElse Not PatronUsuario.IsMatch(usuario.Trim()) Then
            Return Respuesta.Fallo("El usuario debe tener entre 3 y 50 caracteres y solo letras, números, punto, guion o guion bajo.")
        End If
        If String.IsNullOrWhiteSpace(nombreCompleto) OrElse nombreCompleto.Trim().Length > 150 Then
            Return Respuesta.Fallo("El nombre completo debe contener entre 1 y 150 caracteres.")
        End If
        If idRol <= 0 OrElse Not RolActivo(idRol) Then Return Respuesta.Fallo("El rol seleccionado no existe o está desactivado.")
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(clave)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)
        Dim administradorAutenticado As Boolean = EsAdministradorAutenticado()
        If EsRolProtegido(idRol) AndAlso Not administradorAutenticado Then
            Return Respuesta.Fallo("Solo un administrador puede asignar el rol protegido Administrador.")
        End If

        Dim credencial As SalHash = SeguridadClave.CrearHash(clave)

        Try
            EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim rolProtegidoActual As Boolean = False
                    If Not BloquearYValidarRolActivo(conexion, transaccion, idRol, rolProtegidoActual) Then
                        Throw New InvalidOperationException("El rol seleccionado ya no existe o fue desactivado.")
                    End If
                    If rolProtegidoActual AndAlso Not administradorAutenticado Then
                        Throw New InvalidOperationException("Solo un administrador puede asignar el rol protegido Administrador.")
                    End If
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
        Catch ex As InvalidOperationException
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - crear", ex)
            Return Respuesta.Fallo("No se pudo crear el usuario. Verifique los datos y la conexión.")
        End Try
    End Function

    Public Function Actualizar(id As Integer, nombreCompleto As String, idRol As Integer,
                               activo As Boolean) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then
            Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")
        End If
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario válido.")
        If String.IsNullOrWhiteSpace(nombreCompleto) OrElse nombreCompleto.Trim().Length > 150 Then
            Return Respuesta.Fallo("El nombre completo debe contener entre 1 y 150 caracteres.")
        End If
        If idRol <= 0 OrElse Not RolActivo(idRol) Then Return Respuesta.Fallo("El rol seleccionado no existe o está desactivado.")
        Dim administradorAutenticado As Boolean = EsAdministradorAutenticado()
        If Sesion.UsuarioActual IsNot Nothing AndAlso Sesion.UsuarioActual.ID = id AndAlso
           (idRol <> Sesion.UsuarioActual.IDRol OrElse Not activo) Then
            Return Respuesta.Fallo("No puede cambiar su propio rol ni desactivar su cuenta desde esta sesión.")
        End If
        If EsAdministrador(id) AndAlso (Not EsRolProtegido(idRol) OrElse Not activo) AndAlso
           ContarAdministradoresActivos(id) = 0 Then
            Return Respuesta.Fallo("Debe quedar al menos un usuario administrador activo.")
        End If
        If EsAdministrador(id) AndAlso Not administradorAutenticado Then
            Return Respuesta.Fallo("Solo un administrador puede modificar otra cuenta Administrador.")
        End If
        If EsRolProtegido(idRol) AndAlso Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede asignar el rol protegido Administrador.")
        End If

        Try
            Dim filas As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim estadoActual As Object = Nothing
                    Dim rolActualProtegido As Boolean = False
                    Using comandoEstado As New MySqlCommand(
                        "SELECT u.Activo, r.Protegido FROM Usuarios u " &
                        "INNER JOIN Roles r ON r.ID = u.ID_Rol WHERE u.ID = @id FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comandoEstado, "@id", id)
                        Using lector As MySqlDataReader = comandoEstado.ExecuteReader()
                            If lector.Read() Then
                                estadoActual = lector("Activo")
                                rolActualProtegido = Convert.ToInt32(lector("Protegido")) = 1
                            End If
                        End Using
                    End Using

                    If estadoActual Is Nothing OrElse estadoActual Is DBNull.Value Then Return 0
                    If Convert.ToBoolean(estadoActual) <> activo Then Return -1

                    ' La validez del nuevo rol se comprueba bajo bloqueo en la
                    ' misma transacción que asigna el usuario.
                    Dim rolProtegidoActual As Boolean = False
                    If Not BloquearYValidarRolActivo(conexion, transaccion, idRol, rolProtegidoActual) Then
                        Throw New InvalidOperationException("El rol seleccionado ya no existe o fue desactivado.")
                    End If
                    If rolProtegidoActual AndAlso Not administradorAutenticado Then
                        Throw New InvalidOperationException("Solo un administrador puede asignar el rol protegido Administrador.")
                    End If

                    ' Si se retira el rol Administrador, serializar el conteo
                    ' impide que dos cambios concurrentes dejen cero admins.
                    If rolActualProtegido AndAlso Not rolProtegidoActual Then
                        Using comandoBloqueoAdmin As New MySqlCommand(
                            "SELECT ID FROM Roles WHERE Protegido = 1 FOR UPDATE", conexion, transaccion)
                            comandoBloqueoAdmin.ExecuteScalar()
                        End Using
                        Dim administradoresRestantes As Integer
                        Using comandoAdmin As New MySqlCommand(
                            "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
                            "WHERE r.Protegido = 1 AND u.Activo = 1 AND u.ID <> @id", conexion, transaccion)
                            Parametro(comandoAdmin, "@id", id)
                            administradoresRestantes = Convert.ToInt32(comandoAdmin.ExecuteScalar())
                        End Using
                        If administradoresRestantes = 0 Then
                            Throw New InvalidOperationException("Debe quedar al menos un usuario administrador activo.")
                        End If
                    End If

                    Dim afectadas As Integer = Ejecutar(conexion, transaccion,
                        "UPDATE Usuarios SET Nombre_Completo = @nombre, ID_Rol = @rol WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@nombre", nombreCompleto.Trim())
                            Parametro(comando, "@rol", idRol)
                            Parametro(comando, "@id", id)
                        End Sub)

                    If afectadas > 0 Then
                        AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Usuarios", id,
                            "Rol " & idRol & "; datos generales actualizados")
                    End If
                    Return afectadas
                End Function)

            If filas = -1 Then Return Respuesta.Fallo("El estado Activo no se puede cambiar desde la edición general. Utilice la acción específica de desactivación.")
            If filas = 0 AndAlso Not Existe(id) Then Return Respuesta.Fallo("El usuario ya no existe.")
            Return Respuesta.Ok("Usuario actualizado.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("El rol seleccionado ya no existe.")
        Catch ex As InvalidOperationException
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - actualizar", ex)
            Return Respuesta.Fallo("No se pudo actualizar el usuario. Verifique los datos y la conexión.")
        End Try
    End Function

    ''' <summary>
    ''' Reactiva una cuenta únicamente si su rol continúa activo.
    ''' La operación y su auditoría se confirman en una sola transacción.
    ''' </summary>
    Public Function Activar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario válido.")
        If Sesion.UsuarioActual IsNot Nothing AndAlso Sesion.UsuarioActual.ID = id Then
            Return Respuesta.Fallo("No puede reactivar la cuenta con la que inició sesión.")
        End If

        Try
            Dim resultado As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim idRol As Integer
                    Dim activo As Boolean
                    Using comandoEstado As New MySqlCommand(
                        "SELECT u.ID_Rol, u.Activo FROM Usuarios u WHERE u.ID = @id FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comandoEstado, "@id", id)
                        Using lector As MySqlDataReader = comandoEstado.ExecuteReader()
                            If Not lector.Read() Then Return 0
                            idRol = Convert.ToInt32(lector("ID_Rol"))
                            activo = Convert.ToBoolean(lector("Activo"))
                        End Using
                    End Using
                    If activo Then Return 2

                    Dim rolActivo As Boolean = False
                    Dim esAdministrador As Boolean = False
                    Using comandoRol As New MySqlCommand(
                        "SELECT Activo, Protegido FROM Roles WHERE ID = @id FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comandoRol, "@id", idRol)
                        Using lector As MySqlDataReader = comandoRol.ExecuteReader()
                            If lector.Read() Then
                                rolActivo = Convert.ToBoolean(lector("Activo"))
                                esAdministrador = Convert.ToBoolean(lector("Protegido"))
                            End If
                        End Using
                    End Using
                    If Not rolActivo Then Return -3
                    If esAdministrador AndAlso Not EsAdministradorAutenticado() Then Return -2

                    Dim afectadas As Integer = Ejecutar(conexion, transaccion,
                        "UPDATE Usuarios SET Activo = 1 WHERE ID = @id AND Activo = 0",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@id", id)
                        End Sub)
                    If afectadas > 0 Then
                        AuditarEnTransaccion(conexion, transaccion, "ACTIVAR", "Usuarios", id,
                            "Cuenta reactivada por administrador")
                    End If
                    Return afectadas
                End Function)

            If resultado = 0 Then Return Respuesta.Fallo("El usuario ya no existe.")
            If resultado = -2 Then Return Respuesta.Fallo("Solo un administrador puede reactivar otra cuenta Administrador.")
            If resultado = -3 Then Return Respuesta.Fallo("No se puede reactivar el usuario porque su rol está desactivado. Active primero el rol correspondiente.")
            If resultado = 2 Then Return Respuesta.Ok("El usuario ya estaba activo.")
            Return Respuesta.Ok("Usuario reactivado correctamente.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - reactivar", ex)
            Return Respuesta.Fallo("No se pudo reactivar el usuario. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    ''' <summary>
    ''' Desactiva una cuenta mediante una operación separada de la edición
    ''' general. Mantiene la protección del último administrador activo.
    ''' </summary>
    Public Function Desactivar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario válido.")
        If Sesion.UsuarioActual IsNot Nothing AndAlso Sesion.UsuarioActual.ID = id Then
            Return Respuesta.Fallo("No puede desactivar la cuenta con la que inició sesión.")
        End If

        Try
            Dim resultado As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim estado As Object
                    Using comandoEstado As New MySqlCommand(
                        "SELECT u.Activo FROM Usuarios u WHERE u.ID = @id FOR UPDATE", conexion, transaccion)
                        Parametro(comandoEstado, "@id", id)
                        estado = comandoEstado.ExecuteScalar()
                    End Using
                    If estado Is Nothing OrElse estado Is DBNull.Value Then Return 0
                    If Not Convert.ToBoolean(estado) Then Return 2

                    ' Serializa la desactivación de cuentas Administrador.
                    ' Sin este bloqueo, dos transacciones podrían contar a la
                    ' otra cuenta como último administrador y desactivarse a la vez.
                    Using comandoBloqueoAdmin As New MySqlCommand(
                        "SELECT ID FROM Roles WHERE Protegido = 1 FOR UPDATE", conexion, transaccion)
                        comandoBloqueoAdmin.ExecuteScalar()
                    End Using

                    Dim administradoresRestantes As Integer
                    Using comandoAdmin As New MySqlCommand(
                        "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
                        "WHERE r.Protegido = 1 AND u.Activo = 1 AND u.ID <> @id", conexion, transaccion)
                        Parametro(comandoAdmin, "@id", id)
                        administradoresRestantes = Convert.ToInt32(comandoAdmin.ExecuteScalar())
                    End Using

                    Dim esAdministrador As Boolean
                    Using comandoRol As New MySqlCommand(
                        "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
                        "WHERE u.ID = @id AND r.Protegido = 1", conexion, transaccion)
                        Parametro(comandoRol, "@id", id)
                        esAdministrador = Convert.ToInt32(comandoRol.ExecuteScalar()) > 0
                    End Using
                    If esAdministrador AndAlso Not EsAdministradorAutenticado() Then Return -2
                    If esAdministrador AndAlso administradoresRestantes = 0 Then Return -1

                    Dim afectadas As Integer = Ejecutar(conexion, transaccion,
                        "UPDATE Usuarios SET Activo = 0 WHERE ID = @id AND Activo = 1",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@id", id)
                        End Sub)
                    If afectadas > 0 Then
                        AuditarEnTransaccion(conexion, transaccion, "DESACTIVAR", "Usuarios", id,
                            "Cuenta desactivada por administrador")
                    End If
                    Return afectadas
                End Function)

            If resultado = 0 Then Return Respuesta.Fallo("El usuario ya no existe.")
            If resultado = -1 Then Return Respuesta.Fallo("Debe quedar al menos un usuario administrador activo.")
            If resultado = -2 Then Return Respuesta.Fallo("Solo un administrador puede desactivar otra cuenta Administrador.")
            If resultado = 2 Then Return Respuesta.Ok("El usuario ya estaba desactivado.")
            Return Respuesta.Ok("Usuario desactivado.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - desactivar", ex)
            Return Respuesta.Fallo("No se pudo desactivar el usuario. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    Public Function CambiarClavePropia(idUsuario As Integer, claveActual As String, claveNueva As String) As Respuesta
        If Sesion.UsuarioActual Is Nothing OrElse Sesion.UsuarioActual.ID <> idUsuario OrElse idUsuario <= 0 Then
            Return Respuesta.Fallo("Solo puede cambiar la clave de la cuenta con la que inició sesión.")
        End If
        If String.IsNullOrEmpty(claveActual) OrElse claveActual.Length > 128 Then
            Return Respuesta.Fallo("La clave actual no es válida.")
        End If
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)

        Try
            Dim credencial As SalHash = Nothing
            Dim resultado As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim hashActual As String = Nothing
                    Dim saltActual As String = Nothing
                    Dim usuarioActual As String = Nothing
                    Using consultar As New MySqlCommand(
                        "SELECT Usuario, Clave_Hash, Clave_Salt FROM Usuarios WHERE ID = @id AND Activo = 1 FOR UPDATE",
                        conexion, transaccion)
                        Parametro(consultar, "@id", idUsuario)
                        Using lector As MySqlDataReader = consultar.ExecuteReader()
                            If Not lector.Read() Then Return 0
                            usuarioActual = Convert.ToString(lector("Usuario"))
                            hashActual = Convert.ToString(lector("Clave_Hash"))
                            saltActual = Convert.ToString(lector("Clave_Salt"))
                        End Using
                    End Using

                    ' El cambio de clave comparte el mismo bloqueo de intentos que el login.
                    ' Así no se puede probar indefinidamente la clave actual desde esta pantalla.
                    If EstaBloqueado(idUsuario) Then
                        SeguridadClave.Verificar(claveActual, hashActual, saltActual)
                        Return -2
                    End If

                    If Not SeguridadClave.Verificar(claveActual, hashActual, saltActual) Then
                        RegistrarIntentoFallido(idUsuario, usuarioActual)
                        Return -1
                    End If

                    ' No permitir que el cambio propio se convierta en una operación
                    ' sin efecto: la nueva clave debe ser distinta de la actual.
                    If String.Equals(claveActual, claveNueva, StringComparison.Ordinal) Then
                        Return -3
                    End If

                    ' Derivar la nueva clave solo después de autenticar la clave actual.
                    credencial = SeguridadClave.CrearHash(claveNueva)

                    Ejecutar(conexion, transaccion,
                        "UPDATE Usuarios SET Clave_Hash = @hash, Clave_Salt = @salt WHERE ID = @id AND Activo = 1",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@hash", credencial.Hash)
                            Parametro(comando, "@salt", credencial.Salt)
                            Parametro(comando, "@id", idUsuario)
                        End Sub)
                    Ejecutar(conexion, transaccion,
                        "UPDATE Intentos_Acceso SET Intentos = 0, Inicio_Ventana = NULL, " &
                        "Ultimo_Intento = NOW(), Bloqueado_Hasta = NULL WHERE Usuario_ID = @id",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@id", idUsuario)
                        End Sub)
                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Usuarios",
                        idUsuario, "Cambio de clave propia")
                    Return 1
                End Function)

            If resultado = 0 Then Return Respuesta.Fallo("La cuenta ya no existe o está inactiva.")
            If resultado = -1 Then Return Respuesta.Fallo("La clave actual no es correcta. El intento quedó registrado.")
            If resultado = -2 Then Return Respuesta.Fallo("La cuenta está temporalmente bloqueada por intentos fallidos. Intente nuevamente cuando finalice el bloqueo.")
            If resultado = -3 Then Return Respuesta.Fallo("La nueva clave debe ser diferente de la clave actual.")

            ' Mantener únicamente la sesión que realizó el cambio. Las demás
            ' estaciones detectarán la nueva huella y deberán autenticarse otra vez.
            If Sesion.UsuarioActual IsNot Nothing AndAlso Sesion.UsuarioActual.ID = idUsuario Then
                Sesion.UsuarioActual.HuellaCredencial =
                    CrearHuellaCredencial(credencial.Hash, credencial.Salt)
            End If
            Return Respuesta.Ok("Clave actualizada correctamente.")
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - cambio de clave propia", ex)
            Return Respuesta.Fallo("No se pudo actualizar la clave. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    Public Function CambiarClave(idUsuario As Integer, claveNueva As String) As Respuesta
        If Sesion.UsuarioActual Is Nothing OrElse Not Permisos.TieneAcceso("config.usuarios") OrElse
           Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador con permiso puede restablecer claves.")
        End If
        If idUsuario <= 0 OrElse Sesion.UsuarioActual.ID = idUsuario Then
            Return Respuesta.Fallo("El restablecimiento administrativo solo se permite para otra cuenta.")
        End If

        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)

        Try
            Dim credencial As SalHash = SeguridadClave.CrearHash(claveNueva)
            Dim filas As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim existe As Object
                    Using comandoExiste As New MySqlCommand(
                        "SELECT ID FROM Usuarios WHERE ID = @id FOR UPDATE", conexion, transaccion)
                        Parametro(comandoExiste, "@id", idUsuario)
                        existe = comandoExiste.ExecuteScalar()
                    End Using
                    If existe Is Nothing OrElse existe Is DBNull.Value Then Return 0

                    Ejecutar(conexion, transaccion,
                        "UPDATE Usuarios SET Clave_Hash = @hash, Clave_Salt = @salt WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@hash", credencial.Hash)
                            Parametro(comando, "@salt", credencial.Salt)
                            Parametro(comando, "@id", idUsuario)
                        End Sub)
                    Ejecutar(conexion, transaccion,
                        "UPDATE Intentos_Acceso SET Intentos = 0, Inicio_Ventana = NULL, " &
                        "Ultimo_Intento = NOW(), Bloqueado_Hasta = NULL WHERE Usuario_ID = @id",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@id", idUsuario)
                        End Sub)
                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Usuarios",
                        idUsuario, "Restablecimiento administrativo de clave")
                    Return 1
                End Function)

            If filas = 0 Then Return Respuesta.Fallo("El usuario ya no existe.")
            Return Respuesta.Ok("Clave actualizada correctamente.")
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - restablecer clave", ex)
            Return Respuesta.Fallo("No se pudo actualizar la clave. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    ''' <summary>
    ''' Reautenticación de la sesión actual para operaciones sensibles.
    ''' Comparte el bloqueo de intentos con el inicio de sesión: una clave
    ''' incorrecta en una confirmación administrativa también cuenta como
    ''' intento fallido y no permite eludir la política de bloqueo.
    ''' </summary>
    Public Function VerificarClave(idUsuario As Integer, clave As String) As Boolean
        If Sesion.UsuarioActual Is Nothing OrElse Sesion.UsuarioActual.ID <> idUsuario OrElse
           idUsuario <= 0 OrElse String.IsNullOrEmpty(clave) OrElse clave.Length > 128 Then
            Return False
        End If

        Dim tabla As DataTable = Consultar(
            "SELECT Usuario, Clave_Hash, Clave_Salt FROM Usuarios WHERE ID = @id AND Activo = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idUsuario)
            End Sub)

        If tabla.Rows.Count = 0 Then Return False

        Dim fila As DataRow = tabla.Rows(0)
        Dim usuario As String = Convert.ToString(fila("Usuario"))
        Dim hash As String = Convert.ToString(fila("Clave_Hash"))
        Dim salt As String = Convert.ToString(fila("Clave_Salt"))

        ' Se conserva un coste de verificación similar aunque la cuenta ya
        ' esté bloqueada, pero nunca se autoriza una acción durante el bloqueo.
        If EstaBloqueado(idUsuario) Then
            SeguridadClave.Verificar(clave, hash, salt)
            Return False
        End If

        If Not SeguridadClave.Verificar(clave, hash, salt) Then
            RegistrarIntentoFallido(idUsuario, usuario)
            Return False
        End If

        RestablecerIntentos(idUsuario, usuario)
        Return True
    End Function

    ''' <summary>
    ''' Cuenta los administradores activos distintos de idExcluir. Sirve para
    ''' impedir que el sistema quede sin acceso total desde la interfaz; el
    ''' trigger de la base hace la misma validación.
    ''' </summary>
    Public Function ContarAdministradoresActivos(idExcluir As Integer) As Integer
        If Not Permisos.TieneAcceso("config.usuarios") Then Return 0
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol " &
            "WHERE r.Protegido = 1 AND u.Activo = 1 AND u.ID <> @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idExcluir)
            End Sub)
    End Function

    Public Function EsAdministrador(id As Integer) As Boolean
        If Not Permisos.TieneAcceso("config.usuarios") OrElse id <= 0 Then Return False
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
        If Not Permisos.TieneAcceso("config.usuarios") OrElse Not Permisos.AccesoTotal OrElse
           Sesion.UsuarioActual Is Nothing OrElse Not Sesion.UsuarioActual.RolProtegido Then
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
            ' El procedimiento devuelve errores de negocio controlados por SIGNAL.
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1305
            RegistroErrores.Escribir("Usuarios - procedimiento de eliminación no instalado", ex)
            Return Respuesta.Fallo("La eliminación segura no está instalada en la base de datos. Un administrador debe ejecutar Scripts\Triggers_MySQL.sql y después aplicar Scripts\Privilegios_Minimos.sql.")
        Catch ex As MySqlException When ex.Number = 1370
            RegistroErrores.Escribir("Usuarios - permiso EXECUTE de eliminación", ex)
            Return Respuesta.Fallo("La cuenta de aplicación no tiene permiso para ejecutar la eliminación segura. Un administrador debe volver a aplicar Scripts\Privilegios_Minimos.sql.")
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - eliminación segura", ex)
            Return Respuesta.Fallo("No se pudo eliminar el usuario. La operación fue cancelada para proteger la integridad de los datos.")
        End Try
    End Function

    Private Shared Function CrearHuellaCredencial(hash As String, salt As String) As String
        Dim datos As Byte() = Encoding.UTF8.GetBytes(If(hash, "") & "|" & If(salt, ""))
        Using sha As SHA256 = SHA256.Create()
            Return Convert.ToBase64String(sha.ComputeHash(datos))
        End Using
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

    ''' <summary>
    ''' Bloquea el rol mientras se valida su estado y se crea/asigna el usuario.
    ''' La fila bloqueada serializa esta operación con la desactivación del rol.
    ''' </summary>
    Private Function BloquearYValidarRolActivo(conexion As MySqlConnection,
                                               transaccion As MySqlTransaction,
                                               idRol As Integer,
                                               ByRef protegido As Boolean) As Boolean
        protegido = False
        Using comando As New MySqlCommand(
            "SELECT Protegido FROM Roles WHERE ID = @id AND Activo = 1 FOR UPDATE",
            conexion, transaccion)
            Parametro(comando, "@id", idRol)
            Dim valor As Object = comando.ExecuteScalar()
            If valor Is Nothing OrElse valor Is DBNull.Value Then Return False
            protegido = Convert.ToInt32(valor) = 1
            Return True
        End Using
    End Function

    Private Function RolActivo(idRol As Integer) As Boolean
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Roles WHERE ID = @id AND Activo = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idRol)
            End Sub) > 0
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
