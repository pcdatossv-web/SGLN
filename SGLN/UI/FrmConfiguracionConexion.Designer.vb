<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmConfiguracionConexion
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
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

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.grpServidor = New System.Windows.Forms.GroupBox()
        Me.lblAyuda = New System.Windows.Forms.Label()
        Me.lblUsuarioRespaldo = New System.Windows.Forms.Label()
        Me.txtUsuarioRespaldo = New System.Windows.Forms.TextBox()
        Me.lblPasswordRespaldo = New System.Windows.Forms.Label()
        Me.txtPasswordRespaldo = New System.Windows.Forms.TextBox()
        Me.lblCertificadoCa = New System.Windows.Forms.Label()
        Me.txtCertificadoCa = New System.Windows.Forms.TextBox()
        Me.btnBuscarCertificado = New System.Windows.Forms.Button()
        Me.lblModoSsl = New System.Windows.Forms.Label()
        Me.cmbModoSsl = New System.Windows.Forms.ComboBox()
        Me.chkEquipo = New System.Windows.Forms.CheckBox()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.txtUsuario = New System.Windows.Forms.TextBox()
        Me.lblUsuario = New System.Windows.Forms.Label()
        Me.txtBaseDatos = New System.Windows.Forms.TextBox()
        Me.lblBaseDatos = New System.Windows.Forms.Label()
        Me.txtPuerto = New System.Windows.Forms.TextBox()
        Me.lblPuerto = New System.Windows.Forms.Label()
        Me.txtServidor = New System.Windows.Forms.TextBox()
        Me.lblServidor = New System.Windows.Forms.Label()
        Me.lblEstado = New System.Windows.Forms.Label()
        Me.lblRuta = New System.Windows.Forms.Label()
        Me.btnProbar = New System.Windows.Forms.Button()
        Me.btnGuardar = New System.Windows.Forms.Button()
        Me.btnCancelar = New System.Windows.Forms.Button()
        Me.grpServidor.SuspendLayout()
        Me.SuspendLayout()
        '
        ' grpServidor
        '
        Me.grpServidor.Controls.Add(Me.txtPasswordRespaldo)
        Me.grpServidor.Controls.Add(Me.lblPasswordRespaldo)
        Me.grpServidor.Controls.Add(Me.txtUsuarioRespaldo)
        Me.grpServidor.Controls.Add(Me.cmbModoSsl)
        Me.grpServidor.Controls.Add(Me.lblModoSsl)
        Me.grpServidor.Controls.Add(Me.btnBuscarCertificado)
        Me.grpServidor.Controls.Add(Me.txtCertificadoCa)
        Me.grpServidor.Controls.Add(Me.lblCertificadoCa)
        Me.grpServidor.Controls.Add(Me.lblUsuarioRespaldo)
        Me.grpServidor.Controls.Add(Me.lblAyuda)
        Me.grpServidor.Controls.Add(Me.chkEquipo)
        Me.grpServidor.Controls.Add(Me.txtPassword)
        Me.grpServidor.Controls.Add(Me.lblPassword)
        Me.grpServidor.Controls.Add(Me.txtUsuario)
        Me.grpServidor.Controls.Add(Me.lblUsuario)
        Me.grpServidor.Controls.Add(Me.txtBaseDatos)
        Me.grpServidor.Controls.Add(Me.lblBaseDatos)
        Me.grpServidor.Controls.Add(Me.txtPuerto)
        Me.grpServidor.Controls.Add(Me.lblPuerto)
        Me.grpServidor.Controls.Add(Me.txtServidor)
        Me.grpServidor.Controls.Add(Me.lblServidor)
        Me.grpServidor.Location = New System.Drawing.Point(15, 12)
        Me.grpServidor.Name = "grpServidor"
        Me.grpServidor.Size = New System.Drawing.Size(545, 410)
        Me.grpServidor.TabIndex = 0
        Me.grpServidor.TabStop = False
        Me.grpServidor.Text = "Servidor MySQL"
        '
        ' lblAyuda
        '
        Me.lblAyuda.ForeColor = System.Drawing.Color.DimGray
        Me.lblAyuda.Location = New System.Drawing.Point(20, 228)
        Me.lblAyuda.Name = "lblAyuda"
        Me.lblAyuda.Size = New System.Drawing.Size(500, 32)
        Me.lblAyuda.TabIndex = 11
        Me.lblAyuda.Text = "VerifyCA valida la CA. VerifyFull también valida el nombre del servidor contra el SAN del certificado."
        '
        ' Credenciales separadas para respaldos
        '
        Me.lblUsuarioRespaldo.AutoSize = True
        Me.lblUsuarioRespaldo.Location = New System.Drawing.Point(20, 265)
        Me.lblUsuarioRespaldo.Name = "lblUsuarioRespaldo"
        Me.lblUsuarioRespaldo.Size = New System.Drawing.Size(110, 15)
        Me.lblUsuarioRespaldo.TabIndex = 12
        Me.lblUsuarioRespaldo.Text = "Usuario respaldos:"
        '
        ' txtUsuarioRespaldo
        '
        Me.txtUsuarioRespaldo.Location = New System.Drawing.Point(185, 260)
        Me.txtUsuarioRespaldo.Name = "txtUsuarioRespaldo"
        Me.txtUsuarioRespaldo.Size = New System.Drawing.Size(335, 23)
        Me.txtUsuarioRespaldo.TabIndex = 13
        '
        ' lblPasswordRespaldo
        '
        Me.lblPasswordRespaldo.AutoSize = True
        Me.lblPasswordRespaldo.Location = New System.Drawing.Point(20, 299)
        Me.lblPasswordRespaldo.Name = "lblPasswordRespaldo"
        Me.lblPasswordRespaldo.Size = New System.Drawing.Size(125, 15)
        Me.lblPasswordRespaldo.TabIndex = 14
        Me.lblPasswordRespaldo.Text = "Clave de respaldos:"
        '
        ' txtPasswordRespaldo
        '
        Me.txtPasswordRespaldo.Location = New System.Drawing.Point(185, 294)
        Me.txtPasswordRespaldo.Name = "txtPasswordRespaldo"
        Me.txtPasswordRespaldo.Size = New System.Drawing.Size(335, 23)
        Me.txtPasswordRespaldo.TabIndex = 15
        Me.txtPasswordRespaldo.UseSystemPasswordChar = True
        '
        ' lblCertificadoCa
        '
        Me.lblCertificadoCa.AutoSize = True
        Me.lblCertificadoCa.Location = New System.Drawing.Point(20, 333)
        Me.lblCertificadoCa.Name = "lblCertificadoCa"
        Me.lblCertificadoCa.Size = New System.Drawing.Size(92, 15)
        Me.lblCertificadoCa.TabIndex = 16
        Me.lblCertificadoCa.Text = "Certificado CA:"
        '
        ' txtCertificadoCa
        '
        Me.txtCertificadoCa.Location = New System.Drawing.Point(185, 328)
        Me.txtCertificadoCa.Name = "txtCertificadoCa"
        Me.txtCertificadoCa.Size = New System.Drawing.Size(245, 23)
        Me.txtCertificadoCa.TabIndex = 17
        '
        ' btnBuscarCertificado
        '
        Me.btnBuscarCertificado.Location = New System.Drawing.Point(440, 326)
        Me.btnBuscarCertificado.Name = "btnBuscarCertificado"
        Me.btnBuscarCertificado.Size = New System.Drawing.Size(80, 27)
        Me.btnBuscarCertificado.TabIndex = 18
        Me.btnBuscarCertificado.Text = "Examinar..."
        Me.btnBuscarCertificado.UseVisualStyleBackColor = True
        '
        ' lblModoSsl
        '
        Me.lblModoSsl.AutoSize = True
        Me.lblModoSsl.Location = New System.Drawing.Point(20, 367)
        Me.lblModoSsl.Name = "lblModoSsl"
        Me.lblModoSsl.Size = New System.Drawing.Size(88, 15)
        Me.lblModoSsl.TabIndex = 19
        Me.lblModoSsl.Text = "Validación TLS:"
        '
        ' cmbModoSsl
        '
        Me.cmbModoSsl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbModoSsl.FormattingEnabled = True
        Me.cmbModoSsl.Items.AddRange(New Object() {"VerifyCA", "VerifyFull"})
        Me.cmbModoSsl.Location = New System.Drawing.Point(185, 362)
        Me.cmbModoSsl.Name = "cmbModoSsl"
        Me.cmbModoSsl.Size = New System.Drawing.Size(335, 23)
        Me.cmbModoSsl.TabIndex = 20
        '
        ' chkEquipo
        '
        Me.chkEquipo.AutoSize = True
        Me.chkEquipo.Location = New System.Drawing.Point(20, 203)
        Me.chkEquipo.Name = "chkEquipo"
        Me.chkEquipo.Size = New System.Drawing.Size(451, 19)
        Me.chkEquipo.TabIndex = 10
        Me.chkEquipo.Text = "Compartir servidor; cada usuario guarda sus contraseñas (requiere administrador)"
        Me.chkEquipo.UseVisualStyleBackColor = True
        '
        ' txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(185, 166)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New System.Drawing.Size(335, 23)
        Me.txtPassword.TabIndex = 9
        Me.txtPassword.UseSystemPasswordChar = True
        '
        ' lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New System.Drawing.Point(20, 169)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(70, 15)
        Me.lblPassword.TabIndex = 8
        Me.lblPassword.Text = "Contraseña:"
        '
        ' txtUsuario
        '
        Me.txtUsuario.Location = New System.Drawing.Point(185, 132)
        Me.txtUsuario.Name = "txtUsuario"
        Me.txtUsuario.Size = New System.Drawing.Size(335, 23)
        Me.txtUsuario.TabIndex = 7
        '
        ' lblUsuario
        '
        Me.lblUsuario.AutoSize = True
        Me.lblUsuario.Location = New System.Drawing.Point(20, 135)
        Me.lblUsuario.Name = "lblUsuario"
        Me.lblUsuario.Size = New System.Drawing.Size(91, 15)
        Me.lblUsuario.TabIndex = 6
        Me.lblUsuario.Text = "Usuario MySQL:"
        '
        ' txtBaseDatos
        '
        Me.txtBaseDatos.Location = New System.Drawing.Point(185, 98)
        Me.txtBaseDatos.Name = "txtBaseDatos"
        Me.txtBaseDatos.Size = New System.Drawing.Size(335, 23)
        Me.txtBaseDatos.TabIndex = 5
        '
        ' lblBaseDatos
        '
        Me.lblBaseDatos.AutoSize = True
        Me.lblBaseDatos.Location = New System.Drawing.Point(20, 101)
        Me.lblBaseDatos.Name = "lblBaseDatos"
        Me.lblBaseDatos.Size = New System.Drawing.Size(80, 15)
        Me.lblBaseDatos.TabIndex = 4
        Me.lblBaseDatos.Text = "Base de datos:"
        '
        ' txtPuerto
        '
        Me.txtPuerto.Location = New System.Drawing.Point(185, 64)
        Me.txtPuerto.Name = "txtPuerto"
        Me.txtPuerto.Size = New System.Drawing.Size(90, 23)
        Me.txtPuerto.TabIndex = 3
        '
        ' lblPuerto
        '
        Me.lblPuerto.AutoSize = True
        Me.lblPuerto.Location = New System.Drawing.Point(20, 67)
        Me.lblPuerto.Name = "lblPuerto"
        Me.lblPuerto.Size = New System.Drawing.Size(45, 15)
        Me.lblPuerto.TabIndex = 2
        Me.lblPuerto.Text = "Puerto:"
        '
        ' txtServidor
        '
        Me.txtServidor.Location = New System.Drawing.Point(185, 30)
        Me.txtServidor.Name = "txtServidor"
        Me.txtServidor.Size = New System.Drawing.Size(335, 23)
        Me.txtServidor.TabIndex = 1
        '
        ' lblServidor
        '
        Me.lblServidor.AutoSize = True
        Me.lblServidor.Location = New System.Drawing.Point(20, 33)
        Me.lblServidor.Name = "lblServidor"
        Me.lblServidor.Size = New System.Drawing.Size(74, 15)
        Me.lblServidor.TabIndex = 0
        Me.lblServidor.Text = "Servidor o IP:"
        '
        ' lblEstado
        '
        Me.lblEstado.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblEstado.ForeColor = System.Drawing.Color.DimGray
        Me.lblEstado.Location = New System.Drawing.Point(15, 430)
        Me.lblEstado.Name = "lblEstado"
        Me.lblEstado.Size = New System.Drawing.Size(545, 36)
        Me.lblEstado.TabIndex = 1
        Me.lblEstado.Text = "Presione ""Probar conexión"" para verificar los datos."
        '
        ' lblRuta
        '
        Me.lblRuta.AutoSize = True
        Me.lblRuta.ForeColor = System.Drawing.Color.DimGray
        Me.lblRuta.Location = New System.Drawing.Point(15, 470)
        Me.lblRuta.Name = "lblRuta"
        Me.lblRuta.Size = New System.Drawing.Size(0, 15)
        Me.lblRuta.TabIndex = 2
        '
        ' btnProbar
        '
        Me.btnProbar.Location = New System.Drawing.Point(215, 495)
        Me.btnProbar.Name = "btnProbar"
        Me.btnProbar.Size = New System.Drawing.Size(115, 30)
        Me.btnProbar.TabIndex = 3
        Me.btnProbar.Text = "Probar conexión"
        Me.btnProbar.UseVisualStyleBackColor = True
        '
        ' btnGuardar
        '
        Me.btnGuardar.Location = New System.Drawing.Point(340, 495)
        Me.btnGuardar.Name = "btnGuardar"
        Me.btnGuardar.Size = New System.Drawing.Size(100, 30)
        Me.btnGuardar.TabIndex = 4
        Me.btnGuardar.Text = "Guardar"
        Me.btnGuardar.UseVisualStyleBackColor = True
        '
        ' btnCancelar
        '
        Me.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancelar.Location = New System.Drawing.Point(450, 495)
        Me.btnCancelar.Name = "btnCancelar"
        Me.btnCancelar.Size = New System.Drawing.Size(100, 30)
        Me.btnCancelar.TabIndex = 5
        Me.btnCancelar.Text = "Cancelar"
        Me.btnCancelar.UseVisualStyleBackColor = True
        '
        ' FrmConfiguracionConexion
        '
        Me.AcceptButton = Me.btnGuardar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancelar
        Me.ClientSize = New System.Drawing.Size(575, 540)
        Me.Controls.Add(Me.btnCancelar)
        Me.Controls.Add(Me.btnGuardar)
        Me.Controls.Add(Me.btnProbar)
        Me.Controls.Add(Me.lblRuta)
        Me.Controls.Add(Me.lblEstado)
        Me.Controls.Add(Me.grpServidor)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmConfiguracionConexion"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Configuración de conexión - SGLN El Salvador"
        Me.grpServidor.ResumeLayout(False)
        Me.grpServidor.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents grpServidor As System.Windows.Forms.GroupBox
    Friend WithEvents lblServidor As System.Windows.Forms.Label
    Friend WithEvents txtServidor As System.Windows.Forms.TextBox
    Friend WithEvents lblPuerto As System.Windows.Forms.Label
    Friend WithEvents txtPuerto As System.Windows.Forms.TextBox
    Friend WithEvents lblBaseDatos As System.Windows.Forms.Label
    Friend WithEvents txtBaseDatos As System.Windows.Forms.TextBox
    Friend WithEvents lblUsuario As System.Windows.Forms.Label
    Friend WithEvents txtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents chkEquipo As System.Windows.Forms.CheckBox
    Friend WithEvents lblAyuda As System.Windows.Forms.Label
    Friend WithEvents lblUsuarioRespaldo As System.Windows.Forms.Label
    Friend WithEvents txtUsuarioRespaldo As System.Windows.Forms.TextBox
    Friend WithEvents lblPasswordRespaldo As System.Windows.Forms.Label
    Friend WithEvents txtPasswordRespaldo As System.Windows.Forms.TextBox
    Friend WithEvents lblCertificadoCa As System.Windows.Forms.Label
    Friend WithEvents txtCertificadoCa As System.Windows.Forms.TextBox
    Friend WithEvents btnBuscarCertificado As System.Windows.Forms.Button
    Friend WithEvents lblModoSsl As System.Windows.Forms.Label
    Friend WithEvents cmbModoSsl As System.Windows.Forms.ComboBox
    Friend WithEvents lblEstado As System.Windows.Forms.Label
    Friend WithEvents lblRuta As System.Windows.Forms.Label
    Friend WithEvents btnProbar As System.Windows.Forms.Button
    Friend WithEvents btnGuardar As System.Windows.Forms.Button
    Friend WithEvents btnCancelar As System.Windows.Forms.Button
End Class
