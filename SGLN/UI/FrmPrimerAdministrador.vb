Imports System
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>Asistente para registrar la primera cuenta administradora.</summary>
Partial Public Class FrmPrimerAdministrador

    Private _creando As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub CrearAdministrador(sender As Object, e As EventArgs)
        If _creando Then Return

        lblEstado.Text = ""
        If Not String.Equals(txtClave.Text, txtConfirmar.Text, StringComparison.Ordinal) Then
            lblEstado.Text = "La confirmación de la clave no coincide."
            txtConfirmar.Focus()
            Return
        End If

        _creando = True
        btnCrear.Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            InicializadorBD.CrearAdministradorInicial(txtUsuario.Text, txtNombre.Text, txtClave.Text)
            MessageBox.Show("La cuenta fue creada y registrada en la bitácora. Ya puede iniciar sesión.",
                "Configuración completada", MessageBoxButtons.OK, MessageBoxIcon.Information)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            RegistroErrores.Escribir("Creación del primer administrador", ex)
            lblEstado.Text = "No se pudo completar la creación del primer administrador. Revise los datos y el registro local de errores."
        Finally
            Cursor = Cursors.Default
            _creando = False
            If Not IsDisposed Then btnCrear.Enabled = True
        End Try
    End Sub
End Class
