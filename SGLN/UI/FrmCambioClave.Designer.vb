<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmCambioClave
    Inherits System.Windows.Forms.Form
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    Private components As System.ComponentModel.IContainer
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblActual = New System.Windows.Forms.Label()
        Me.txtActual = New System.Windows.Forms.TextBox()
        Me.lblNueva = New System.Windows.Forms.Label()
        Me.txtNueva = New System.Windows.Forms.TextBox()
        Me.lblConfirmar = New System.Windows.Forms.Label()
        Me.txtConfirmar = New System.Windows.Forms.TextBox()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.btnGuardar = New System.Windows.Forms.Button()
        Me.btnCancelar = New System.Windows.Forms.Button()
        Me.chkMostrarClaves = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        Me.lblActual.AutoSize = True
        Me.lblActual.Location = New System.Drawing.Point(24, 28)
        Me.lblActual.Name = "lblActual"
        Me.lblActual.Size = New System.Drawing.Size(72, 15)
        Me.lblActual.Text = "Clave actual:"
        Me.txtActual.Location = New System.Drawing.Point(165, 24)
        Me.txtActual.MaxLength = 128
        Me.txtActual.Name = "txtActual"
        Me.txtActual.Size = New System.Drawing.Size(250, 23)
        Me.txtActual.UseSystemPasswordChar = True
        Me.lblNueva.AutoSize = True
        Me.lblNueva.Location = New System.Drawing.Point(24, 76)
        Me.lblNueva.Name = "lblNueva"
        Me.lblNueva.Size = New System.Drawing.Size(72, 15)
        Me.lblNueva.Text = "Nueva clave:"
        Me.txtNueva.Location = New System.Drawing.Point(165, 72)
        Me.txtNueva.MaxLength = 128
        Me.txtNueva.Name = "txtNueva"
        Me.txtNueva.Size = New System.Drawing.Size(250, 23)
        Me.txtNueva.UseSystemPasswordChar = True
        Me.lblConfirmar.AutoSize = True
        Me.lblConfirmar.Location = New System.Drawing.Point(24, 124)
        Me.lblConfirmar.Name = "lblConfirmar"
        Me.lblConfirmar.Size = New System.Drawing.Size(135, 15)
        Me.lblConfirmar.Text = "Confirmar nueva clave:"
        Me.txtConfirmar.Location = New System.Drawing.Point(165, 120)
        Me.txtConfirmar.MaxLength = 128
        Me.txtConfirmar.Name = "txtConfirmar"
        Me.txtConfirmar.Size = New System.Drawing.Size(250, 23)
        Me.txtConfirmar.UseSystemPasswordChar = True
        Me.lblMensaje.ForeColor = System.Drawing.Color.DimGray
        Me.lblMensaje.Location = New System.Drawing.Point(24, 190)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(410, 34)
        Me.lblMensaje.Text = "Mínimo 12 caracteres, con mayúsculas, minúsculas, números y símbolos."
        Me.btnGuardar.Location = New System.Drawing.Point(165, 238)
        Me.btnGuardar.Name = "btnGuardar"
        Me.btnGuardar.Size = New System.Drawing.Size(120, 32)
        Me.btnGuardar.Text = "Guardar"
        Me.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancelar.Location = New System.Drawing.Point(295, 238)
        Me.btnCancelar.Name = "btnCancelar"
        Me.btnCancelar.Size = New System.Drawing.Size(120, 32)
        Me.btnCancelar.Text = "Cancelar"
        Me.AcceptButton = Me.btnGuardar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancelar
        Me.ClientSize = New System.Drawing.Size(460, 292)
        Me.chkMostrarClaves.AutoSize = True
        Me.chkMostrarClaves.Location = New System.Drawing.Point(165, 160)
        Me.chkMostrarClaves.Name = "chkMostrarClaves"
        Me.chkMostrarClaves.Size = New System.Drawing.Size(139, 19)
        Me.chkMostrarClaves.Text = "Mostrar contraseñas"
        Me.Controls.Add(Me.chkMostrarClaves)
        Me.Controls.Add(Me.btnCancelar)
        Me.Controls.Add(Me.btnGuardar)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.txtConfirmar)
        Me.Controls.Add(Me.lblConfirmar)
        Me.Controls.Add(Me.txtNueva)
        Me.Controls.Add(Me.lblNueva)
        Me.Controls.Add(Me.txtActual)
        Me.Controls.Add(Me.lblActual)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmCambioClave"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Cambiar mi clave"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
    Friend WithEvents lblActual As System.Windows.Forms.Label
    Friend WithEvents txtActual As System.Windows.Forms.TextBox
    Friend WithEvents lblNueva As System.Windows.Forms.Label
    Friend WithEvents txtNueva As System.Windows.Forms.TextBox
    Friend WithEvents lblConfirmar As System.Windows.Forms.Label
    Friend WithEvents txtConfirmar As System.Windows.Forms.TextBox
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents btnGuardar As System.Windows.Forms.Button
    Friend WithEvents btnCancelar As System.Windows.Forms.Button
    Friend WithEvents chkMostrarClaves As System.Windows.Forms.CheckBox
End Class
