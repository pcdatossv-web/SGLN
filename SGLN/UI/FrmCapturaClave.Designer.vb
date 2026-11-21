<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmCapturaClave
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
        Me.lblInstruccion = New System.Windows.Forms.Label()
        Me.lblClave = New System.Windows.Forms.Label()
        Me.txtClave = New System.Windows.Forms.TextBox()
        Me.lblConfirmar = New System.Windows.Forms.Label()
        Me.txtConfirmar = New System.Windows.Forms.TextBox()
        Me.btnAceptar = New System.Windows.Forms.Button()
        Me.btnCancelar = New System.Windows.Forms.Button()
        Me.chkMostrarClaves = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        Me.lblInstruccion.Location = New System.Drawing.Point(16, 14)
        Me.lblInstruccion.Name = "lblInstruccion"
        Me.lblInstruccion.Size = New System.Drawing.Size(390, 38)
        Me.lblInstruccion.Text = "Ingrese la clave solicitada."
        Me.lblClave.AutoSize = True
        Me.lblClave.Location = New System.Drawing.Point(18, 66)
        Me.lblClave.Name = "lblClave"
        Me.lblClave.Size = New System.Drawing.Size(39, 15)
        Me.lblClave.Text = "Clave:"
        Me.txtClave.Location = New System.Drawing.Point(18, 86)
        Me.txtClave.MaxLength = 128
        Me.txtClave.Name = "txtClave"
        Me.txtClave.Size = New System.Drawing.Size(388, 23)
        Me.txtClave.UseSystemPasswordChar = True
        Me.lblConfirmar.AutoSize = True
        Me.lblConfirmar.Location = New System.Drawing.Point(18, 122)
        Me.lblConfirmar.Name = "lblConfirmar"
        Me.lblConfirmar.Size = New System.Drawing.Size(91, 15)
        Me.lblConfirmar.Text = "Confirmar clave:"
        Me.txtConfirmar.Location = New System.Drawing.Point(18, 142)
        Me.txtConfirmar.MaxLength = 128
        Me.txtConfirmar.Name = "txtConfirmar"
        Me.txtConfirmar.Size = New System.Drawing.Size(388, 23)
        Me.txtConfirmar.UseSystemPasswordChar = True
        Me.btnAceptar.Location = New System.Drawing.Point(160, 205)
        Me.btnAceptar.Name = "btnAceptar"
        Me.btnAceptar.Size = New System.Drawing.Size(120, 32)
        Me.btnAceptar.Text = "Continuar"
        Me.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancelar.Location = New System.Drawing.Point(286, 205)
        Me.btnCancelar.Name = "btnCancelar"
        Me.btnCancelar.Size = New System.Drawing.Size(120, 32)
        Me.btnCancelar.Text = "Cancelar"
        Me.AcceptButton = Me.btnAceptar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancelar
        Me.ClientSize = New System.Drawing.Size(424, 255)
        Me.chkMostrarClaves.AutoSize = True
        Me.chkMostrarClaves.Location = New System.Drawing.Point(18, 174)
        Me.chkMostrarClaves.Name = "chkMostrarClaves"
        Me.chkMostrarClaves.Size = New System.Drawing.Size(139, 19)
        Me.chkMostrarClaves.Text = "Mostrar contraseñas"
        Me.Controls.Add(Me.chkMostrarClaves)
        Me.Controls.Add(Me.btnCancelar)
        Me.Controls.Add(Me.btnAceptar)
        Me.Controls.Add(Me.txtConfirmar)
        Me.Controls.Add(Me.lblConfirmar)
        Me.Controls.Add(Me.txtClave)
        Me.Controls.Add(Me.lblClave)
        Me.Controls.Add(Me.lblInstruccion)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmCapturaClave"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Confirmar clave"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
    Friend WithEvents lblInstruccion As System.Windows.Forms.Label
    Friend WithEvents lblClave As System.Windows.Forms.Label
    Friend WithEvents txtClave As System.Windows.Forms.TextBox
    Friend WithEvents lblConfirmar As System.Windows.Forms.Label
    Friend WithEvents txtConfirmar As System.Windows.Forms.TextBox
    Friend WithEvents btnAceptar As System.Windows.Forms.Button
    Friend WithEvents btnCancelar As System.Windows.Forms.Button
    Friend WithEvents chkMostrarClaves As System.Windows.Forms.CheckBox
End Class
