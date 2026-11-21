Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Windows.Forms

''' <summary>
''' Control de acceso por roles. Cada opción del sistema tiene un código
''' (ej. "casos.confirmar_plazo") y cada rol tiene un conjunto de códigos en
''' Rol_Permiso. Los permisos se cargan una vez al iniciar sesión.
'''
''' Ocultar un menú NO basta: cada formulario sensible debe llamar a
''' Permisos.Exigir(...) en su Load y Permisos.ExigirAccion(...) antes de las
''' acciones críticas.
''' </summary>
Public NotInheritable Class Permisos

    Private Shared ReadOnly _activos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private Shared ReadOnly _sync As New Object()
    Private Shared _accesoTotal As Boolean

    Private Sub New()
    End Sub

    ''' <summary>
    ''' En pruebas automatizadas se desactivan los cuadros de mensaje para que
    ''' la batería no quede esperando una confirmación.
    ''' </summary>
    Public Shared Property Silencioso As Boolean

    Public Shared ReadOnly Property AccesoTotal As Boolean
        Get
            SyncLock _sync
                Return _accesoTotal
            End SyncLock
        End Get
    End Property

    Public Shared ReadOnly Property CantidadPermisos As Integer
        Get
            SyncLock _sync
                Return _activos.Count
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Carga los permisos del rol del usuario que inicia sesión. El rol
    ''' protegido (Administrador) tiene acceso total aunque su matriz esté
    ''' incompleta: así nunca se queda fuera del sistema.
    ''' </summary>
    Public Shared Function Cargar(idRol As Integer, esRolProtegido As Boolean) As Boolean
        Dim permisosNuevos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim accesoTotalNuevo As Boolean = False
        Dim cargaCorrecta As Boolean = False

        If idRol > 0 Then
            Try
                ' Una sola consulta confirma rol activo, marca protegida y
                ' permisos bajo la misma lectura de MySQL; evita inconsistencias
                ' entre varias conexiones si el rol cambia durante la carga.
                Dim tabla As DataTable = New RolRepositorio().CargarPermisosDeRol(idRol)
                If tabla.Rows.Count > 0 Then
                    cargaCorrecta = True
                    accesoTotalNuevo = Convert.ToInt32(tabla.Rows(0)("Protegido")) = 1
                    For Each fila As DataRow In tabla.Rows
                        If fila.IsNull("Codigo") Then Continue For
                        Dim codigo As String = Convert.ToString(fila("Codigo"))
                        If Not String.IsNullOrWhiteSpace(codigo) Then permisosNuevos.Add(codigo.Trim())
                    Next
                End If
            Catch ex As Exception
                ' Si no se puede verificar el rol, no se concede acceso total.
                accesoTotalNuevo = False
                permisosNuevos.Clear()
                RegistroErrores.Escribir("Permisos - cargar", ex)
            End Try
        End If

        ' La carga puede ejecutarse desde acciones concurrentes; sustituir la
        ' colección bajo un único bloqueo evita enumeraciones/modificaciones cruzadas.
        SyncLock _sync
            _activos.Clear()
            For Each codigo As String In permisosNuevos
                _activos.Add(codigo)
            Next
            _accesoTotal = accesoTotalNuevo
        End SyncLock

        Return cargaCorrecta
    End Function

    Public Shared Sub Limpiar()
        SyncLock _sync
            _activos.Clear()
            _accesoTotal = False
        End SyncLock
    End Sub

    ''' <summary>
    ''' Revalida que la cuenta y su rol sigan activos y actualiza los permisos
    ''' de la sesión. También se usa al volver de formularios modales.
    ''' </summary>
    Public Shared Function RevalidarSesion() As Boolean
        Dim usuarioSesion As UsuarioSesion = Sesion.UsuarioActual
        If usuarioSesion Is Nothing OrElse usuarioSesion.ID <= 0 Then
            ' Una sesión ausente nunca debe dejar permisos de una sesión anterior.
            Limpiar()
            Return False
        End If

        Try
            Dim estado As UsuarioSesion = New UsuarioRepositorio().ObtenerEstadoSesion(usuarioSesion.ID)
            If estado Is Nothing Then
                Sesion.Limpiar()
                Return False
            End If

            usuarioSesion.Usuario = estado.Usuario
            usuarioSesion.NombreCompleto = estado.NombreCompleto
            usuarioSesion.IDRol = estado.IDRol
            usuarioSesion.Rol = estado.Rol
            usuarioSesion.RolProtegido = estado.RolProtegido

            If Not Cargar(estado.IDRol, estado.RolProtegido) Then
                ' La cuenta puede seguir activa y aun así no ser posible
                ' confirmar que su rol está activo y cargar su matriz.
                ' En ese caso la sesión completa se invalida.
                Sesion.Limpiar()
                Return False
            End If
            Return True
        Catch ex As Exception
            ' Si no se puede comprobar el estado actual, se retira la sesión:
            ' nunca se mantienen permisos de una cuenta que no pudo verificarse.
            Limpiar()
            RegistroErrores.Escribir("Permisos - validar sesión", ex)
            Return False
        End Try
    End Function

    ''' <summary>Autoriza operaciones de servicio solo con una sesión autenticada y permiso explícito.</summary>
    Public Shared Function TieneAcceso(codigo As String) As Boolean
        If Not RevalidarSesion() Then Return False
        Return Puede(codigo)
    End Function

    ''' <summary>
    ''' Revalida la sesión una sola vez y comprueba una lista de permisos alternativos.
    ''' Se utiliza cuando una operación admite más de un perfil funcional de acceso.
    ''' </summary>
    Public Shared Function TieneAccesoAlguno(ParamArray codigos As String()) As Boolean
        If Not RevalidarSesion() OrElse codigos Is Nothing Then Return False

        For Each codigo As String In codigos
            If Puede(codigo) Then Return True
        Next

        Return False
    End Function

    Public Shared Function Puede(codigo As String) As Boolean
        ' Un permiso sin código nunca debe conceder acceso (fail-closed).
        If String.IsNullOrWhiteSpace(codigo) Then Return False
        SyncLock _sync
            If _accesoTotal Then Return True
            Return _activos.Contains(codigo.Trim())
        End SyncLock
    End Function

    ''' <summary>
    ''' Verifica un permiso al abrir un formulario. Devuelve False y muestra el
    ''' motivo: el formulario debe salir de su Load sin continuar.
    ''' </summary>
    Public Shared Function Exigir(codigo As String, formulario As Form) As Boolean
        If TieneAcceso(codigo) Then Return True

        Avisar("Su rol (" & Sesion.RolActual & ") no tiene permiso para abrir esta opción." & Environment.NewLine &
               "Solicite el acceso al administrador del sistema.")
        If formulario IsNot Nothing AndAlso Not Silencioso Then formulario.Close()
        Return False
    End Function

    ''' <summary>
    ''' Verifica un permiso justo antes de una acción crítica (confirmar plazo,
    ''' cerrar libro, reportar a la UIF, respaldar, etc.).
    ''' </summary>
    Public Shared Function ExigirAccion(codigo As String) As Boolean
        If TieneAcceso(codigo) Then Return True

        Avisar("Su rol (" & Sesion.RolActual & ") no tiene permiso para realizar esta acción.")
        Return False
    End Function

    Private Shared Sub Avisar(mensaje As String)
        RegistroErrores.Escribir("Permisos - denegado", New UnauthorizedAccessException(mensaje))
        If Not Silencioso Then
            MessageBox.Show(mensaje, "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

End Class
