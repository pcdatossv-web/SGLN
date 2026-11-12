Imports System
Imports System.Data
Imports System.Text.RegularExpressions

''' <summary>
''' Reglas de negocio del mantenimiento de usuarios. La validación de "nunca
''' quedarse sin administrador" se hace aquí y también con triggers en MySQL.
''' </summary>
Public Class ServicioUsuarios

    Private ReadOnly _repositorio As New UsuarioRepositorio()
    Private ReadOnly _roles As New RolRepositorio()

    Private Shared ReadOnly PatronUsuario As New Regex("^[a-zA-Z0-9._-]{3,50}$")

    Public Function Listar() As DataTable
        Return _repositorio.Listar()
    End Function

    Public Function RolesDisponibles() As DataTable
        Return _roles.RolesActivos()
    End Function

    Public Function Crear(usuario As String, nombreCompleto As String, clave As String,
                          idRol As Integer, activo As Boolean) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")


        Dim errorValidacion As String = ValidarDatos(usuario, nombreCompleto, clave, idRol)
        If errorValidacion <> "" Then Return Respuesta.Fallo(errorValidacion)

        Return _repositorio.Crear(usuario.Trim(), nombreCompleto.Trim(), clave, idRol, activo)
    End Function

    Public Function Actualizar(id As Integer, nombreCompleto As String, idRol As Integer,
                               activo As Boolean) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")


        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        If String.IsNullOrWhiteSpace(nombreCompleto) Then Return Respuesta.Fallo("Indique el nombre completo.")
        If nombreCompleto.Trim().Length > 150 Then Return Respuesta.Fallo("El nombre no puede superar 150 caracteres.")
        If idRol <= 0 Then Return Respuesta.Fallo("Seleccione un rol.")

        Dim errorRol As String = ValidarRolActivo(idRol)
        If errorRol <> "" Then Return Respuesta.Fallo(errorRol)

        ' Si el usuario es administrador y se le quita el rol o se desactiva,
        ' debe quedar otro administrador activo.
        If _repositorio.EsAdministrador(id) AndAlso Not EsRolProtegido(idRol) Then
            If _repositorio.ContarAdministradoresActivos(id) = 0 Then
                Return Respuesta.Fallo("Debe quedar al menos un usuario administrador activo.")
            End If
        ElseIf _repositorio.EsAdministrador(id) AndAlso Not activo Then
            If _repositorio.ContarAdministradoresActivos(id) = 0 Then
                Return Respuesta.Fallo("Debe quedar al menos un usuario administrador activo.")
            End If
        End If

        Return _repositorio.Actualizar(id, nombreCompleto, idRol, activo)
    End Function

    ''' <summary>
    ''' Restablece la clave sin pedir la anterior. Solo lo usa el administrador
    ''' (permiso config.usuarios).
    ''' </summary>
    Public Function RestablecerClave(id As Integer, claveNueva As String) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then Return Respuesta.Fallo("No tiene permiso para restablecer claves de otros usuarios.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)
        Return _repositorio.CambiarClave(id, claveNueva)
    End Function

    ''' <summary>Cambio de la propia clave: exige la clave actual.</summary>
    Public Function CambiarClavePropia(id As Integer, claveActual As String, claveNueva As String) As Respuesta
        If Sesion.UsuarioActual Is Nothing OrElse Sesion.UsuarioActual.ID <> id Then Return Respuesta.Fallo("Solo puede cambiar la clave de la cuenta con la que inició sesión.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(claveNueva)
        If errorClave <> "" Then Return Respuesta.Fallo(errorClave)
        If Not _repositorio.VerificarClave(id, claveActual) Then
            Return Respuesta.Fallo("La clave actual no es correcta.")
        End If
        Return _repositorio.CambiarClave(id, claveNueva)
    End Function

    Public Function Desactivar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("config.usuarios") Then Return Respuesta.Fallo("No tiene permiso para administrar usuarios.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        If Sesion.UsuarioActual IsNot Nothing AndAlso id = Sesion.UsuarioActual.ID Then
            Return Respuesta.Fallo("No puede desactivar el usuario con el que inició sesión.")
        End If
        If _repositorio.EsAdministrador(id) AndAlso _repositorio.ContarAdministradoresActivos(id) = 0 Then
            Return Respuesta.Fallo("Debe quedar al menos un usuario administrador activo.")
        End If

        Dim fila As DataTable = _repositorio.Obtener(id)
        If fila.Rows.Count = 0 Then Return Respuesta.Fallo("El usuario ya no existe.")

        Return _repositorio.Actualizar(
            id,
            Convert.ToString(fila.Rows(0)("Nombre_Completo")),
            Convert.ToInt32(fila.Rows(0)("ID_Rol")),
            False)
    End Function


    ''' <summary>Eliminación física excepcional reservada al administrador autenticado.</summary>
    Public Function EliminarDefinitivamente(id As Integer) As Respuesta
        If Sesion.UsuarioActual Is Nothing OrElse Not Sesion.UsuarioActual.RolProtegido OrElse
           Not Permisos.AccesoTotal Then
            Return Respuesta.Fallo("Solo un administrador autenticado puede eliminar definitivamente usuarios.")
        End If
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un usuario.")
        If id = Sesion.UsuarioActual.ID Then
            Return Respuesta.Fallo("No puede eliminar la cuenta con la que inició sesión.")
        End If
        Return _repositorio.EliminarSiNoTieneHistorial(id)
    End Function

    Private Function ValidarDatos(usuario As String, nombreCompleto As String, clave As String,
                                  idRol As Integer) As String
        If String.IsNullOrWhiteSpace(usuario) Then Return "Indique el nombre de acceso."
        If Not PatronUsuario.IsMatch(usuario.Trim()) Then
            Return "El usuario debe tener entre 3 y 50 caracteres y solo letras, números, punto, guion o guion bajo."
        End If
        If String.IsNullOrWhiteSpace(nombreCompleto) Then Return "Indique el nombre completo."
        If nombreCompleto.Trim().Length > 150 Then Return "El nombre no puede superar 150 caracteres."
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(clave)
        If errorClave <> "" Then Return errorClave
        If idRol <= 0 Then Return "Seleccione un rol."

        Return ValidarRolActivo(idRol)
    End Function

    Private Function ValidarRolActivo(idRol As Integer) As String
        For Each fila As DataRow In _roles.RolesActivos().Rows
            If Convert.ToInt32(fila("ID")) = idRol Then Return ""
        Next
        Return "El rol seleccionado no existe o está desactivado."
    End Function

    Private Function EsRolProtegido(idRol As Integer) As Boolean
        Dim rol As DataTable = _roles.Obtener(idRol)
        If rol.Rows.Count = 0 Then Return False
        Return Convert.ToInt32(rol.Rows(0)("Protegido")) = 1
    End Function

End Class
