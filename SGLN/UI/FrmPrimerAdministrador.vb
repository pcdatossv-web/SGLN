Imports System
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>Asistente para registrar la primera cuenta administradora.</summary>
Public Class FrmPrimerAdministrador
    Inherits Form

    Private ReadOnly txtUsuario As New TextBox()
    Private ReadOnly txtNombre As New TextBox()
    Private ReadOnly txtClave As New TextBox()
    Private ReadOnly txtConfirmar As New TextBox()
    Private ReadOnly lblEstado As New Label()

    Public Sub New()
        Text = "Configuración inicial de SGLN"
        StartPosition = FormStartPosition.CenterScreen
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(510, 365)
        MaximizeBox = False
        MinimizeBox = False

        Dim titulo As New Label With {.Text = "Crear la primera cuenta administradora",
            .Font = New Font("Segoe UI", 12.0!, FontStyle.Bold), .Location = New Point(20, 18), .Size = New Size(465, 28)}
        Dim ayuda As New Label With {.Text = "No existe ningún usuario. Registre el administrador inicial. Use una clave de al menos 12 caracteres con mayúsculas, minúsculas, números y símbolos.",
            .Location = New Point(20, 53), .Size = New Size(465, 48)}
        AgregarCampo("Usuario:", txtUsuario, 112)
        AgregarCampo("Nombre completo:", txtNombre, 152)
        AgregarCampo("Clave:", txtClave, 192)
        AgregarCampo("Confirmar clave:", txtConfirmar, 232)
        txtUsuario.MaxLength = 50
        txtNombre.MaxLength = 150
        txtClave.MaxLength = 128
        txtConfirmar.MaxLength = 128
        txtClave.UseSystemPasswordChar = True
        txtConfirmar.UseSystemPasswordChar = True

        lblEstado.Location = New Point(20, 273)
        lblEstado.Size = New Size(465, 25)
        lblEstado.ForeColor = Color.Firebrick

        Dim btnCrear As New Button With {.Text = "Crear administrador", .Location = New Point(215, 315), .Size = New Size(160, 32)}
        Dim btnSalir As New Button With {.Text = "Salir", .Location = New Point(385, 315), .Size = New Size(100, 32), .DialogResult = DialogResult.Cancel}
        AddHandler btnCrear.Click, AddressOf CrearAdministrador
        Controls.AddRange(New Control() {titulo, ayuda, lblEstado, btnCrear, btnSalir})
        AcceptButton = btnCrear
        CancelButton = btnSalir
    End Sub

    Private Sub AgregarCampo(etiqueta As String, campo As TextBox, y As Integer)
        Controls.Add(New Label With {.Text = etiqueta, .Location = New Point(20, y + 3), .Size = New Size(145, 22)})
        campo.Location = New Point(170, y)
        campo.Size = New Size(315, 24)
        Controls.Add(campo)
    End Sub

    Private Sub CrearAdministrador(sender As Object, e As EventArgs)
        lblEstado.Text = ""
        If Not String.Equals(txtClave.Text, txtConfirmar.Text, StringComparison.Ordinal) Then
            lblEstado.Text = "La confirmación de la clave no coincide."
            txtConfirmar.Focus()
            Return
        End If
        Try
            Cursor = Cursors.WaitCursor
            InicializadorBD.CrearAdministradorInicial(txtUsuario.Text, txtNombre.Text, txtClave.Text)
            Cursor = Cursors.Default
            MessageBox.Show("La cuenta fue creada y registrada en la bitácora. Ya puede iniciar sesión.",
                "Configuración completada", MessageBoxButtons.OK, MessageBoxIcon.Information)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            Cursor = Cursors.Default
            RegistroErrores.Escribir("Creación del primer administrador", ex)
            lblEstado.Text = ex.Message
        End Try
    End Sub
End Class
