''' <summary>
''' Estado de la sesión activa. Se llena después del login y se limpia al
''' cerrar sesión.
''' </summary>
Public Module Sesion

    Public Property UsuarioActual As UsuarioSesion
    Public Property CerrarSesionSolicitado As Boolean

    Public ReadOnly Property NombreUsuario As String
        Get
            If UsuarioActual Is Nothing Then Return "sistema"
            Return UsuarioActual.Usuario
        End Get
    End Property

    Public ReadOnly Property NombreCompleto As String
        Get
            If UsuarioActual Is Nothing Then Return "Sistema"
            Return UsuarioActual.NombreCompleto
        End Get
    End Property

    Public ReadOnly Property RolActual As String
        Get
            If UsuarioActual Is Nothing Then Return ""
            Return UsuarioActual.Rol
        End Get
    End Property

    ''' <summary>
    ''' Carga en memoria los permisos del usuario de la sesión. Se llama al
    ''' iniciar sesión y después de cambiar el rol del usuario actual.
    ''' </summary>
    Public Sub CargarPermisos()
        If UsuarioActual Is Nothing Then
            Permisos.Limpiar()
            Return
        End If
        Permisos.Cargar(UsuarioActual.IDRol, UsuarioActual.RolProtegido)
    End Sub

    Public Sub Limpiar()
        UsuarioActual = Nothing
        Permisos.Limpiar()
    End Sub

End Module
