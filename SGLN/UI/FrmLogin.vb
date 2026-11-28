Imports System
Imports System.Windows.Forms

Public Class FrmLogin

    Private _limpiandoCredenciales As Boolean

    Private Sub FrmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Resuelve la conexión (registro o App.config) antes de tocar la base.
        ' Si el servidor no responde, se ofrece configurarlo en este momento.
        ConfiguracionConexion.Inicializar()

        LimpiarCamposCredenciales()
        chkMostrarClave.Checked = False
        txtUsuario.Focus()
    End Sub

    Private Sub chkMostrarClave_CheckedChanged(sender As Object, e As EventArgs) Handles chkMostrarClave.CheckedChanged
        txtClave.UseSystemPasswordChar = Not chkMostrarClave.Checked
    End Sub

    ' El mensaje de validación permanece visible hasta que el usuario modifica
    ' alguno de los campos; los borrados programáticos no deben ocultarlo.
    Private Sub Credenciales_TextChanged(sender As Object, e As EventArgs) _
            Handles txtUsuario.TextChanged, txtClave.TextChanged
        If Not _limpiandoCredenciales Then lblError.Text = ""
    End Sub

    Private Sub LimpiarClave()
        _limpiandoCredenciales = True
        Try
            txtClave.Clear()
        Finally
            _limpiandoCredenciales = False
        End Try
    End Sub

    Private Sub LimpiarCamposCredenciales()
        _limpiandoCredenciales = True
        Try
            txtUsuario.Clear()
            txtClave.Clear()
        Finally
            _limpiandoCredenciales = False
        End Try
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
        UtileriasUI.EjecutarUnaVez(btnIngresar, AddressOf Ingresar)
    End Sub

    Private Sub Ingresar()
        ' Cada intento de autenticación comienza sin permisos ni identidad
        ' heredados de un intento anterior dentro del mismo proceso.
        Sesion.Limpiar()
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
                LimpiarClave()
                chkMostrarClave.Checked = False
                txtClave.Focus()
                Return
            End If

            If usuario.DebeCambiarClave Then
                Dim claveNueva As String = SolicitarCambioClaveInicial()
                If String.IsNullOrEmpty(claveNueva) Then
                    lblError.Text = "Debe cambiar la clave inicial para ingresar al sistema."
                    LimpiarClave()
                    chkMostrarClave.Checked = False
                    txtClave.Focus()
                    Return
                End If

                ' La cuenta autenticada se establece temporalmente para que el
                ' repositorio verifique la clave actual y la cambie atómicamente.
                Sesion.UsuarioActual = usuario
                Dim cambio As Respuesta = repositorio.CambiarClavePropia(
                    usuario.ID, txtClave.Text, claveNueva)
                If Not cambio.Exito Then
                    Sesion.Limpiar()
                    lblError.Text = cambio.Mensaje
                    LimpiarClave()
                    chkMostrarClave.Checked = False
                    txtClave.Focus()
                    Return
                End If

                usuario.DebeCambiarClave = False
                MessageBox.Show("La clave inicial fue sustituida correctamente.",
                                "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Sesion.UsuarioActual = usuario
            If Not Sesion.CargarPermisos() Then
                Sesion.Limpiar()
                Throw New InvalidOperationException(
                    "No fue posible verificar el rol activo y cargar sus permisos. El inicio de sesión fue cancelado.")
            End If
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            ' Si falla la carga de permisos después de validar la contraseña,
            ' no debe quedar una sesión parcialmente inicializada en memoria.
            Sesion.Limpiar()
            LimpiarClave()
            chkMostrarClave.Checked = False
            RegistroErrores.Escribir("Inicio de sesión", ex)
            lblError.Text = "No se pudo iniciar sesión. Verifique sus credenciales y la conexión con el servidor."
        End Try
    End Sub

    ''' <summary>
    ''' Solicita y confirma una clave robusta antes de permitir el primer ingreso
    ''' con la cuenta que todavía conserva la contraseña inicial.
    ''' </summary>
    Private Function SolicitarCambioClaveInicial() As String
        Using dialogo As New FrmCapturaClave(
            True,
            "Cambio obligatorio de clave",
            "Por seguridad, debe reemplazar la clave inicial.")
            If dialogo.ShowDialog(Me) = DialogResult.OK Then
                Return dialogo.ClaveCapturada
            End If
            Return Nothing
        End Using
    End Function

    Private Sub FrmLogin_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' Liberar credenciales visibles al cerrar el formulario, incluso si
        ' el usuario cancela el inicio de sesión o la autenticación termina bien.
        LimpiarCamposCredenciales()
        chkMostrarClave.Checked = False
    End Sub

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
