Imports System
Imports System.Windows.Forms

Public Class FrmLogin

    Private Sub FrmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Resuelve la conexión (registro o App.config) antes de tocar la base.
        ' Si el servidor no responde, se ofrece configurarlo en este momento.
        ConfiguracionConexion.Inicializar()
        VerificarConexionAlInicio()

        txtUsuario.Clear()
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

            If usuario.DebeCambiarClave Then
                Dim claveNueva As String = SolicitarCambioClaveInicial()
                If String.IsNullOrEmpty(claveNueva) Then
                    lblError.Text = "Debe cambiar la clave inicial para ingresar al sistema."
                    txtClave.Clear()
                    txtClave.Focus()
                    Return
                End If

                Dim cambio As Respuesta = repositorio.CambiarClave(usuario.ID, claveNueva)
                If Not cambio.Exito Then
                    lblError.Text = cambio.Mensaje
                    txtClave.Clear()
                    txtClave.Focus()
                    Return
                End If

                usuario.DebeCambiarClave = False
                MessageBox.Show("La clave inicial fue sustituida correctamente.",
                                "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Sesion.UsuarioActual = usuario
            Sesion.CargarPermisos()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            RegistroErrores.Escribir("Inicio de sesión", ex)
            lblError.Text = "No se pudo iniciar sesión. Verifique sus credenciales y la conexión con el servidor."
        End Try
    End Sub

    ''' <summary>
    ''' Solicita y confirma una clave robusta antes de permitir el primer ingreso
    ''' con la cuenta que todavía conserva la contraseña inicial.
    ''' </summary>
    Private Function SolicitarCambioClaveInicial() As String
        Using dialogo As New Form()
            dialogo.Text = "Cambio obligatorio de clave"
            dialogo.StartPosition = FormStartPosition.CenterParent
            dialogo.FormBorderStyle = FormBorderStyle.FixedDialog
            dialogo.ClientSize = New System.Drawing.Size(430, 235)
            dialogo.MaximizeBox = False
            dialogo.MinimizeBox = False
            dialogo.ShowInTaskbar = False

            Dim titulo As New Label() With {
                .Text = "Por seguridad, debe reemplazar la clave inicial.",
                .AutoSize = False,
                .Location = New System.Drawing.Point(18, 15),
                .Size = New System.Drawing.Size(394, 35)
            }
            Dim lblNueva As New Label() With {
                .Text = "Nueva clave:",
                .AutoSize = True,
                .Location = New System.Drawing.Point(20, 65)
            }
            Dim txtNueva As New TextBox() With {
                .Location = New System.Drawing.Point(135, 61),
                .Size = New System.Drawing.Size(265, 23),
                .UseSystemPasswordChar = True,
                .MaxLength = 128
            }
            Dim lblConfirmar As New Label() With {
                .Text = "Confirmar clave:",
                .AutoSize = True,
                .Location = New System.Drawing.Point(20, 105)
            }
            Dim txtConfirmar As New TextBox() With {
                .Location = New System.Drawing.Point(135, 101),
                .Size = New System.Drawing.Size(265, 23),
                .UseSystemPasswordChar = True,
                .MaxLength = 128
            }
            Dim btnAceptar As New Button() With {
                .Text = "Guardar clave",
                .Location = New System.Drawing.Point(135, 155),
                .Size = New System.Drawing.Size(125, 32)
            }
            Dim btnCancelar As New Button() With {
                .Text = "Cancelar",
                .Location = New System.Drawing.Point(275, 155),
                .Size = New System.Drawing.Size(125, 32),
                .DialogResult = DialogResult.Cancel
            }

            Dim claveAceptada As String = Nothing
            AddHandler btnAceptar.Click,
                Sub(sender As Object, e As EventArgs)
                    Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(txtNueva.Text)
                    If errorClave <> "" Then
                        MessageBox.Show(errorClave, "Clave no válida",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        txtNueva.Focus()
                        Return
                    End If
                    If Not String.Equals(txtNueva.Text, txtConfirmar.Text, StringComparison.Ordinal) Then
                        MessageBox.Show("La confirmación no coincide con la nueva clave.",
                                        "Clave no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        txtConfirmar.Clear()
                        txtConfirmar.Focus()
                        Return
                    End If
                    claveAceptada = txtNueva.Text
                    dialogo.DialogResult = DialogResult.OK
                    dialogo.Close()
                End Sub

            dialogo.Controls.AddRange(New Control() {
                titulo, lblNueva, txtNueva, lblConfirmar, txtConfirmar, btnAceptar, btnCancelar
            })
            dialogo.AcceptButton = btnAceptar
            dialogo.CancelButton = btnCancelar

            If dialogo.ShowDialog(Me) = DialogResult.OK Then Return claveAceptada
            Return Nothing
        End Using
    End Function

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
