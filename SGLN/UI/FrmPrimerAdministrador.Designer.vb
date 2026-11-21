<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmPrimerAdministrador
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
        Me.lblTitulo = New System.Windows.Forms.Label()
        Me.lblAyuda = New System.Windows.Forms.Label()
        Me.lblUsuario = New System.Windows.Forms.Label()
        Me.txtUsuario = New System.Windows.Forms.TextBox()
        Me.lblNombre = New System.Windows.Forms.Label()
        Me.txtNombre = New System.Windows.Forms.TextBox()
        Me.lblClave = New System.Windows.Forms.Label()
        Me.txtClave = New System.Windows.Forms.TextBox()
        Me.lblConfirmar = New System.Windows.Forms.Label()
        Me.txtConfirmar = New System.Windows.Forms.TextBox()
        Me.chkMostrarClave = New System.Windows.Forms.CheckBox()
        Me.lblEstado = New System.Windows.Forms.Label()
        Me.btnCrear = New System.Windows.Forms.Button()
        Me.btnSalir = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.lblTitulo.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitulo.Location = New System.Drawing.Point(20, 18)
        Me.lblTitulo.Name = "lblTitulo"
        Me.lblTitulo.Size = New System.Drawing.Size(465, 28)
        Me.lblTitulo.Text = "Crear la primera cuenta administradora"
        Me.lblAyuda.Location = New System.Drawing.Point(20, 53)
        Me.lblAyuda.Name = "lblAyuda"
        Me.lblAyuda.Size = New System.Drawing.Size(465, 48)
        Me.lblAyuda.Text = "No existe ningún usuario. Registre el administrador inicial. Use una clave de al menos 12 caracteres con mayúsculas, minúsculas, números y símbolos."
        Me.lblUsuario.Location = New System.Drawing.Point(20, 115)
        Me.lblUsuario.Name = "lblUsuario"
        Me.lblUsuario.Size = New System.Drawing.Size(145, 22)
        Me.lblUsuario.Text = "Usuario:"
        Me.txtUsuario.Location = New System.Drawing.Point(170, 112)
        Me.txtUsuario.MaxLength = 50
        Me.txtUsuario.Name = "txtUsuario"
        Me.txtUsuario.Size = New System.Drawing.Size(315, 23)
        Me.txtUsuario.TabIndex = 0
        Me.lblNombre.Location = New System.Drawing.Point(20, 155)
        Me.lblNombre.Name = "lblNombre"
        Me.lblNombre.Size = New System.Drawing.Size(145, 22)
        Me.lblNombre.Text = "Nombre completo:"
        Me.txtNombre.Location = New System.Drawing.Point(170, 152)
        Me.txtNombre.MaxLength = 150
        Me.txtNombre.Name = "txtNombre"
        Me.txtNombre.Size = New System.Drawing.Size(315, 23)
        Me.txtNombre.TabIndex = 1
        Me.lblClave.Location = New System.Drawing.Point(20, 195)
        Me.lblClave.Name = "lblClave"
        Me.lblClave.Size = New System.Drawing.Size(145, 22)
        Me.lblClave.Text = "Clave:"
        Me.txtClave.Location = New System.Drawing.Point(170, 192)
        Me.txtClave.MaxLength = 128
        Me.txtClave.Name = "txtClave"
        Me.txtClave.Size = New System.Drawing.Size(315, 23)
        Me.txtClave.TabIndex = 2
        Me.txtClave.UseSystemPasswordChar = True
        Me.lblConfirmar.Location = New System.Drawing.Point(20, 235)
        Me.lblConfirmar.Name = "lblConfirmar"
        Me.lblConfirmar.Size = New System.Drawing.Size(145, 22)
        Me.lblConfirmar.Text = "Confirmar clave:"
        Me.txtConfirmar.Location = New System.Drawing.Point(170, 232)
        Me.txtConfirmar.MaxLength = 128
        Me.txtConfirmar.Name = "txtConfirmar"
        Me.txtConfirmar.Size = New System.Drawing.Size(315, 23)
        Me.txtConfirmar.TabIndex = 3
        Me.txtConfirmar.UseSystemPasswordChar = True
        Me.chkMostrarClave.AutoSize = True
        Me.chkMostrarClave.Location = New System.Drawing.Point(170, 263)
        Me.chkMostrarClave.Name = "chkMostrarClave"
        Me.chkMostrarClave.Size = New System.Drawing.Size(104, 19)
        Me.chkMostrarClave.TabIndex = 4
        Me.chkMostrarClave.Text = "Mostrar claves"
        Me.chkMostrarClave.UseVisualStyleBackColor = True
        Me.lblEstado.ForeColor = System.Drawing.Color.Firebrick
        Me.lblEstado.Location = New System.Drawing.Point(20, 288)
        Me.lblEstado.Name = "lblEstado"
        Me.lblEstado.Size = New System.Drawing.Size(465, 35)
        Me.btnCrear.Location = New System.Drawing.Point(215, 330)
        Me.btnCrear.Name = "btnCrear"
        Me.btnCrear.Size = New System.Drawing.Size(160, 32)
        Me.btnCrear.TabIndex = 5
        Me.btnCrear.Text = "Crear administrador"
        Me.btnSalir.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnSalir.Location = New System.Drawing.Point(385, 330)
        Me.btnSalir.Name = "btnSalir"
        Me.btnSalir.Size = New System.Drawing.Size(100, 32)
        Me.btnSalir.TabIndex = 6
        Me.btnSalir.Text = "Salir"
        Me.AcceptButton = Me.btnCrear
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnSalir
        Me.ClientSize = New System.Drawing.Size(510, 380)
        Me.Controls.Add(Me.btnSalir)
        Me.Controls.Add(Me.btnCrear)
        Me.Controls.Add(Me.lblEstado)
        Me.Controls.Add(Me.chkMostrarClave)
        Me.Controls.Add(Me.txtConfirmar)
        Me.Controls.Add(Me.lblConfirmar)
        Me.Controls.Add(Me.txtClave)
        Me.Controls.Add(Me.lblClave)
        Me.Controls.Add(Me.txtNombre)
        Me.Controls.Add(Me.lblNombre)
        Me.Controls.Add(Me.txtUsuario)
        Me.Controls.Add(Me.lblUsuario)
        Me.Controls.Add(Me.lblAyuda)
        Me.Controls.Add(Me.lblTitulo)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmPrimerAdministrador"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Configuración inicial de SGLN"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
    Friend WithEvents lblTitulo As System.Windows.Forms.Label
    Friend WithEvents lblAyuda As System.Windows.Forms.Label
    Friend WithEvents lblUsuario As System.Windows.Forms.Label
    Friend WithEvents txtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents lblNombre As System.Windows.Forms.Label
    Friend WithEvents txtNombre As System.Windows.Forms.TextBox
    Friend WithEvents lblClave As System.Windows.Forms.Label
    Friend WithEvents txtClave As System.Windows.Forms.TextBox
    Friend WithEvents lblConfirmar As System.Windows.Forms.Label
    Friend WithEvents txtConfirmar As System.Windows.Forms.TextBox
    Friend WithEvents chkMostrarClave As System.Windows.Forms.CheckBox
    Friend WithEvents lblEstado As System.Windows.Forms.Label
    Friend WithEvents btnCrear As System.Windows.Forms.Button
    Friend WithEvents btnSalir As System.Windows.Forms.Button
End Class
