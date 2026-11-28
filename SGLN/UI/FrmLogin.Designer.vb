<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmLogin
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitulo = New System.Windows.Forms.Label()
        Me.lblSubtitulo = New System.Windows.Forms.Label()
        Me.lblUsuario = New System.Windows.Forms.Label()
        Me.txtUsuario = New System.Windows.Forms.TextBox()
        Me.lblClave = New System.Windows.Forms.Label()
        Me.txtClave = New System.Windows.Forms.TextBox()
        Me.chkMostrarClave = New System.Windows.Forms.CheckBox()
        Me.btnIngresar = New System.Windows.Forms.Button()
        Me.btnSalir = New System.Windows.Forms.Button()
        Me.lblError = New System.Windows.Forms.Label()
        Me.lnkConfigurarServidor = New System.Windows.Forms.LinkLabel()
        Me.lblPie = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        ' lblTitulo
        '
        Me.lblTitulo.Font = New System.Drawing.Font("Segoe UI", 15.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(52, Byte), Integer), CType(CType(92, Byte), Integer))
        Me.lblTitulo.Location = New System.Drawing.Point(22, 18)
        Me.lblTitulo.Name = "lblTitulo"
        Me.lblTitulo.Size = New System.Drawing.Size(416, 30)
        Me.lblTitulo.TabIndex = 0
        Me.lblTitulo.Text = "SGLN El Salvador"
        Me.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' lblSubtitulo
        '
        Me.lblSubtitulo.ForeColor = System.Drawing.Color.DimGray
        Me.lblSubtitulo.Location = New System.Drawing.Point(22, 50)
        Me.lblSubtitulo.Name = "lblSubtitulo"
        Me.lblSubtitulo.Size = New System.Drawing.Size(416, 20)
        Me.lblSubtitulo.TabIndex = 1
        Me.lblSubtitulo.Text = "Sistema de Gestión Legal y Notarial"
        Me.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' lblUsuario
        '
        Me.lblUsuario.AutoSize = True
        Me.lblUsuario.Location = New System.Drawing.Point(42, 91)
        Me.lblUsuario.Name = "lblUsuario"
        Me.lblUsuario.Size = New System.Drawing.Size(50, 15)
        Me.lblUsuario.TabIndex = 2
        Me.lblUsuario.Text = "Usuario:"
        '
        ' txtUsuario
        '
        Me.txtUsuario.Location = New System.Drawing.Point(42, 111)
        Me.txtUsuario.MaxLength = 50
        Me.txtUsuario.Name = "txtUsuario"
        Me.txtUsuario.Size = New System.Drawing.Size(376, 26)
        Me.txtUsuario.TabIndex = 0
        '
        ' lblClave
        '
        Me.lblClave.AutoSize = True
        Me.lblClave.Location = New System.Drawing.Point(42, 153)
        Me.lblClave.Name = "lblClave"
        Me.lblClave.Size = New System.Drawing.Size(39, 15)
        Me.lblClave.TabIndex = 4
        Me.lblClave.Text = "Clave:"
        '
        ' txtClave
        '
        Me.txtClave.Location = New System.Drawing.Point(42, 173)
        Me.txtClave.MaxLength = 128
        Me.txtClave.Name = "txtClave"
        Me.txtClave.Size = New System.Drawing.Size(376, 26)
        Me.txtClave.TabIndex = 1
        Me.txtClave.UseSystemPasswordChar = True
        '
        ' chkMostrarClave
        '
        Me.chkMostrarClave.AutoSize = True
        Me.chkMostrarClave.Location = New System.Drawing.Point(42, 205)
        Me.chkMostrarClave.Name = "chkMostrarClave"
        Me.chkMostrarClave.Size = New System.Drawing.Size(104, 19)
        Me.chkMostrarClave.TabIndex = 2
        Me.chkMostrarClave.Text = "Mostrar clave"
        Me.chkMostrarClave.UseVisualStyleBackColor = True
        '
        ' btnIngresar
        '
        Me.btnIngresar.Location = New System.Drawing.Point(42, 239)
        Me.btnIngresar.Name = "btnIngresar"
        Me.btnIngresar.Size = New System.Drawing.Size(180, 34)
        Me.btnIngresar.TabIndex = 3
        Me.btnIngresar.Text = "Ingresar"
        Me.btnIngresar.UseVisualStyleBackColor = True
        '
        ' btnSalir
        '
        Me.btnSalir.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnSalir.Location = New System.Drawing.Point(238, 239)
        Me.btnSalir.Name = "btnSalir"
        Me.btnSalir.Size = New System.Drawing.Size(180, 34)
        Me.btnSalir.TabIndex = 4
        Me.btnSalir.Text = "Salir"
        Me.btnSalir.UseVisualStyleBackColor = True
        '
        ' lblError
        '
        Me.lblError.AutoEllipsis = False
        Me.lblError.AutoSize = False
        Me.lblError.ForeColor = System.Drawing.Color.Firebrick
        Me.lblError.Location = New System.Drawing.Point(42, 282)
        Me.lblError.Name = "lblError"
        Me.lblError.Size = New System.Drawing.Size(376, 52)
        Me.lblError.TabIndex = 8
        Me.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lnkConfigurarServidor
        '
        Me.lnkConfigurarServidor.AutoSize = True
        Me.lnkConfigurarServidor.Location = New System.Drawing.Point(42, 338)
        Me.lnkConfigurarServidor.Name = "lnkConfigurarServidor"
        Me.lnkConfigurarServidor.Size = New System.Drawing.Size(190, 15)
        Me.lnkConfigurarServidor.TabIndex = 9
        Me.lnkConfigurarServidor.TabStop = True
        Me.lnkConfigurarServidor.Text = "Configurar servidor de base de datos"
        '
        ' lblPie
        '
        Me.lblPie.ForeColor = System.Drawing.Color.Gray
        Me.lblPie.Location = New System.Drawing.Point(22, 370)
        Me.lblPie.Name = "lblPie"
        Me.lblPie.Size = New System.Drawing.Size(416, 20)
        Me.lblPie.TabIndex = 10
        Me.lblPie.Text = "v1.0 · Fase 1 (IA/OCR planificada para la fase final)"
        Me.lblPie.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' FrmLogin
        '
        Me.AcceptButton = Me.btnIngresar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnSalir
        Me.ClientSize = New System.Drawing.Size(460, 402)
        Me.Controls.Add(Me.lnkConfigurarServidor)
        Me.Controls.Add(Me.lblPie)
        Me.Controls.Add(Me.lblError)
        Me.Controls.Add(Me.chkMostrarClave)
        Me.Controls.Add(Me.btnSalir)
        Me.Controls.Add(Me.btnIngresar)
        Me.Controls.Add(Me.txtClave)
        Me.Controls.Add(Me.lblClave)
        Me.Controls.Add(Me.txtUsuario)
        Me.Controls.Add(Me.lblUsuario)
        Me.Controls.Add(Me.lblSubtitulo)
        Me.Controls.Add(Me.lblTitulo)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmLogin"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SGLN El Salvador - Iniciar sesión"
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblTitulo As System.Windows.Forms.Label
    Friend WithEvents lblSubtitulo As System.Windows.Forms.Label
    Friend WithEvents lblUsuario As System.Windows.Forms.Label
    Friend WithEvents txtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents lblClave As System.Windows.Forms.Label
    Friend WithEvents txtClave As System.Windows.Forms.TextBox
    Friend WithEvents chkMostrarClave As System.Windows.Forms.CheckBox
    Friend WithEvents btnIngresar As System.Windows.Forms.Button
    Friend WithEvents btnSalir As System.Windows.Forms.Button
    Friend WithEvents lblError As System.Windows.Forms.Label
    Friend WithEvents lnkConfigurarServidor As System.Windows.Forms.LinkLabel
    Friend WithEvents lblPie As System.Windows.Forms.Label
End Class
