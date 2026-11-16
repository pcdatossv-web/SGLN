Imports System
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Permite a cualquier usuario autenticado cambiar su propia clave,
''' exigiendo la clave actual y confirmación de la nueva.
''' </summary>
Public Class FrmCambioClave
    Inherits Form

    Private ReadOnly _txtActual As New TextBox()
    Private ReadOnly _txtNueva As New TextBox()
    Private ReadOnly _txtConfirmar As New TextBox()
    Private ReadOnly _btnGuardar As New Button()
    Private ReadOnly _btnCancelar As New Button()
    Private ReadOnly _lblMensaje As New Label()

    Public Sub New()
        Me.Text = "Cambiar mi clave"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.ClientSize = New Size(460, 270)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False

        CrearEtiqueta("Clave actual:", 24, 28)
        ConfigurarCampo(_txtActual, 165, 24)

        CrearEtiqueta("Nueva clave:", 24, 76)
        ConfigurarCampo(_txtNueva, 165, 72)

        CrearEtiqueta("Confirmar nueva clave:", 24, 124)
        ConfigurarCampo(_txtConfirmar, 165, 120)

        _lblMensaje.AutoSize = False
        _lblMensaje.Location = New Point(24, 160)
        _lblMensaje.Size = New Size(410, 34)
        _lblMensaje.ForeColor = Color.DimGray
        _lblMensaje.Text = "Mínimo 12 caracteres, con mayúsculas, minúsculas, números y símbolos."
        Me.Controls.Add(_lblMensaje)

        _btnGuardar.Text = "Guardar"
        _btnGuardar.Location = New Point(165, 210)
        _btnGuardar.Size = New Size(120, 32)
        AddHandler _btnGuardar.Click, AddressOf Guardar
        Me.Controls.Add(_btnGuardar)

        _btnCancelar.Text = "Cancelar"
        _btnCancelar.Location = New Point(295, 210)
        _btnCancelar.Size = New Size(120, 32)
        _btnCancelar.DialogResult = DialogResult.Cancel
        Me.Controls.Add(_btnCancelar)

        Me.AcceptButton = _btnGuardar
        Me.CancelButton = _btnCancelar
        AddHandler Me.Shown, Sub(sender As Object, e As EventArgs) _txtActual.Focus()
    End Sub

    Private Sub CrearEtiqueta(texto As String, x As Integer, y As Integer)
        Dim etiqueta As New Label() With {
            .Text = texto,
            .AutoSize = True,
            .Location = New Point(x, y + 4)
        }
        Me.Controls.Add(etiqueta)
    End Sub

    Private Sub ConfigurarCampo(campo As TextBox, x As Integer, y As Integer)
        campo.Location = New Point(x, y)
        campo.Size = New Size(250, 24)
        campo.MaxLength = 128
        campo.UseSystemPasswordChar = True
        Me.Controls.Add(campo)
    End Sub

    Private Sub Guardar(sender As Object, e As EventArgs)
        Try
            If Sesion.UsuarioActual Is Nothing Then
                _lblMensaje.ForeColor = Color.Firebrick
                _lblMensaje.Text = "La sesión ya no está activa. Inicie sesión nuevamente."
                Return
            End If

            If Not String.Equals(_txtNueva.Text, _txtConfirmar.Text, StringComparison.Ordinal) Then
                _lblMensaje.ForeColor = Color.Firebrick
                _lblMensaje.Text = "La confirmación no coincide con la nueva clave."
                _txtConfirmar.Clear()
                _txtConfirmar.Focus()
                Return
            End If

            Dim respuesta As Respuesta =
                New ServicioUsuarios().CambiarClavePropia(
                    Sesion.UsuarioActual.ID, _txtActual.Text, _txtNueva.Text)

            If Not respuesta.Exito Then
                _lblMensaje.ForeColor = Color.Firebrick
                _lblMensaje.Text = respuesta.Mensaje
                _txtActual.Clear()
                _txtActual.Focus()
                Return
            End If

            MessageBox.Show("La clave se cambió correctamente.",
                            "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            RegistroErrores.Escribir("Cambiar mi clave", ex)
            _lblMensaje.Text = "No se pudo cambiar la clave. Verifique la conexión y vuelva a intentarlo."
        End Try
    End Sub
End Class
