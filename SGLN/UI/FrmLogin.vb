Imports System
Imports System.Windows.Forms

Public Class FrmLogin

    Private Sub FrmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Resuelve la conexión (registro o App.config) antes de tocar la base.
        ' Si el servidor no responde, se ofrece configurarlo en este momento.
        ConfiguracionConexion.Inicializar()
        VerificarConexionAlInicio()

        txtUsuario.Text = "admin"
        txtClave.Clear()
        txtUsuario.Focus()
    End Sub

    ''' <summary>
    ''' Si el servidor no responde, se ofrece abrir la configuración en lugar de
    ''' dejar al usuario con un error repetido en cada intento de ingreso.
    ''' </summary>
    Private Sub VerificarConexionAlInicio()
        Dim mensaje As String = ""
        If ConfiguracionConexion.Probar(ConfiguracionConexion.Actual, mensaje) Then Return

        RegistroErrores.Anotar("Conexión al iniciar", mensaje)

        Dim respuesta As DialogResult = MessageBox.Show(
            "No se pudo conectar al servidor de base de datos." & Environment.NewLine & Environment.NewLine &
            mensaje & Environment.NewLine & Environment.NewLine &
            "Servidor configurado: " & ConfiguracionConexion.Actual.DescripcionCorta() & Environment.NewLine & Environment.NewLine &
            "¿Desea revisar la configuración de la conexión?",
            "Conexión a la base de datos", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

        If respuesta = DialogResult.Yes Then AbrirConfiguracionConexion()
    End Sub

    Private Sub AbrirConfiguracionConexion()
        Using formulario As New FrmConfiguracionConexion()
            formulario.ShowDialog(Me)
        End Using
    End Sub

    Private Sub lnkConfigurarServidor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) _
            Handles lnkConfigurarServidor.LinkClicked
        AbrirConfiguracionConexion()
    End Sub

    Private Sub btnIngresar_Click(sender As Object, e As EventArgs) Handles btnIngresar.Click
        Ingresar()
    End Sub

    Private Sub Ingresar()
        lblError.Text = ""

        If String.IsNullOrWhiteSpace(txtUsuario.Text) Then
            lblError.Text = "Ingrese el usuario."
            txtUsuario.Focus()
            Return
        End If

        If String.IsNullOrWhiteSpace(txtClave.Text) Then
            lblError.Text = "Ingrese la clave."
            txtClave.Focus()
            Return
        End If

        Try
            Dim repositorio As New UsuarioRepositorio()
            Dim usuario As UsuarioSesion = repositorio.Autenticar(txtUsuario.Text.Trim(), txtClave.Text)

            If usuario Is Nothing Then
                lblError.Text = "Usuario o clave incorrectos."
                txtClave.Clear()
                txtClave.Focus()
                Return
            End If

            Sesion.UsuarioActual = usuario
            Sesion.CargarPermisos()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            RegistroErrores.Escribir("Inicio de sesión", ex)
            lblError.Text = "No se pudo iniciar sesión: " & ex.Message
        End Try
    End Sub

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
