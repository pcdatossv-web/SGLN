<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmUsuarios
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
        Me.dgvUsuarios = New System.Windows.Forms.DataGridView()
        Me.grpDatos = New System.Windows.Forms.GroupBox()
        Me.lblUsuario = New System.Windows.Forms.Label()
        Me.txtUsuario = New System.Windows.Forms.TextBox()
        Me.lblNombre = New System.Windows.Forms.Label()
        Me.txtNombre = New System.Windows.Forms.TextBox()
        Me.lblRol = New System.Windows.Forms.Label()
        Me.cboRol = New System.Windows.Forms.ComboBox()
        Me.chkActivo = New System.Windows.Forms.CheckBox()
        Me.btnNuevo = New System.Windows.Forms.Button()
        Me.btnGuardar = New System.Windows.Forms.Button()
        Me.grpClave = New System.Windows.Forms.GroupBox()
        Me.lblClaveNueva = New System.Windows.Forms.Label()
        Me.txtClaveNueva = New System.Windows.Forms.TextBox()
        Me.lblClaveConfirmar = New System.Windows.Forms.Label()
        Me.txtClaveConfirmar = New System.Windows.Forms.TextBox()
        Me.btnRestablecer = New System.Windows.Forms.Button()
        Me.lblNotaClave = New System.Windows.Forms.Label()
        Me.btnDesactivar = New System.Windows.Forms.Button()
        Me.btnEliminar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        CType(Me.dgvUsuarios, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpDatos.SuspendLayout()
        Me.grpClave.SuspendLayout()
        Me.SuspendLayout()
        '
        ' dgvUsuarios
        '
        Me.dgvUsuarios.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.dgvUsuarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvUsuarios.Location = New System.Drawing.Point(12, 12)
        Me.dgvUsuarios.Name = "dgvUsuarios"
        Me.dgvUsuarios.Size = New System.Drawing.Size(690, 660)
        Me.dgvUsuarios.TabIndex = 0
        '
        ' grpDatos
        '
        Me.grpDatos.Controls.Add(Me.lblUsuario)
        Me.grpDatos.Controls.Add(Me.txtUsuario)
        Me.grpDatos.Controls.Add(Me.lblNombre)
        Me.grpDatos.Controls.Add(Me.txtNombre)
        Me.grpDatos.Controls.Add(Me.lblRol)
        Me.grpDatos.Controls.Add(Me.cboRol)
        Me.grpDatos.Controls.Add(Me.chkActivo)
        Me.grpDatos.Controls.Add(Me.btnNuevo)
        Me.grpDatos.Controls.Add(Me.btnGuardar)
        Me.grpDatos.Location = New System.Drawing.Point(715, 12)
        Me.grpDatos.Name = "grpDatos"
        Me.grpDatos.Size = New System.Drawing.Size(458, 245)
        Me.grpDatos.TabIndex = 1
        Me.grpDatos.TabStop = False
        Me.grpDatos.Text = "Datos del usuario"
        '
        ' lblUsuario
        '
        Me.lblUsuario.AutoSize = True
        Me.lblUsuario.Location = New System.Drawing.Point(20, 28)
        Me.lblUsuario.Name = "lblUsuario"
        Me.lblUsuario.Size = New System.Drawing.Size(50, 15)
        Me.lblUsuario.TabIndex = 0
        Me.lblUsuario.Text = "Usuario:"
        '
        ' txtUsuario
        '
        Me.txtUsuario.Location = New System.Drawing.Point(20, 46)
        Me.txtUsuario.MaxLength = 50
        Me.txtUsuario.Name = "txtUsuario"
        Me.txtUsuario.Size = New System.Drawing.Size(200, 23)
        Me.txtUsuario.TabIndex = 1
        '
        ' lblNombre
        '
        Me.lblNombre.AutoSize = True
        Me.lblNombre.Location = New System.Drawing.Point(235, 28)
        Me.lblNombre.Name = "lblNombre"
        Me.lblNombre.Size = New System.Drawing.Size(110, 15)
        Me.lblNombre.TabIndex = 2
        Me.lblNombre.Text = "Nombre completo:"
        '
        ' txtNombre
        '
        Me.txtNombre.Location = New System.Drawing.Point(235, 46)
        Me.txtNombre.MaxLength = 150
        Me.txtNombre.Name = "txtNombre"
        Me.txtNombre.Size = New System.Drawing.Size(203, 23)
        Me.txtNombre.TabIndex = 3
        '
        ' lblRol
        '
        Me.lblRol.AutoSize = True
        Me.lblRol.Location = New System.Drawing.Point(20, 85)
        Me.lblRol.Name = "lblRol"
        Me.lblRol.Size = New System.Drawing.Size(28, 15)
        Me.lblRol.TabIndex = 4
        Me.lblRol.Text = "Rol:"
        '
        ' cboRol
        '
        Me.cboRol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboRol.FormattingEnabled = True
        Me.cboRol.Location = New System.Drawing.Point(20, 103)
        Me.cboRol.Name = "cboRol"
        Me.cboRol.Size = New System.Drawing.Size(200, 23)
        Me.cboRol.TabIndex = 5
        '
        ' chkActivo
        '
        Me.chkActivo.AutoSize = True
        Me.chkActivo.Location = New System.Drawing.Point(235, 105)
        Me.chkActivo.Name = "chkActivo"
        Me.chkActivo.Size = New System.Drawing.Size(60, 19)
        Me.chkActivo.TabIndex = 6
        Me.chkActivo.Text = "Activo"
        Me.chkActivo.UseVisualStyleBackColor = True
        '
        ' btnNuevo
        '
        Me.btnNuevo.Location = New System.Drawing.Point(20, 190)
        Me.btnNuevo.Name = "btnNuevo"
        Me.btnNuevo.Size = New System.Drawing.Size(120, 32)
        Me.btnNuevo.TabIndex = 7
        Me.btnNuevo.Text = "Nuevo"
        Me.btnNuevo.UseVisualStyleBackColor = True
        '
        ' btnGuardar
        '
        Me.btnGuardar.Location = New System.Drawing.Point(150, 190)
        Me.btnGuardar.Name = "btnGuardar"
        Me.btnGuardar.Size = New System.Drawing.Size(120, 32)
        Me.btnGuardar.TabIndex = 8
        Me.btnGuardar.Text = "Guardar"
        Me.btnGuardar.UseVisualStyleBackColor = True
        '
        ' grpClave
        '
        Me.grpClave.Controls.Add(Me.lblClaveNueva)
        Me.grpClave.Controls.Add(Me.txtClaveNueva)
        Me.grpClave.Controls.Add(Me.lblClaveConfirmar)
        Me.grpClave.Controls.Add(Me.txtClaveConfirmar)
        Me.grpClave.Controls.Add(Me.btnRestablecer)
        Me.grpClave.Controls.Add(Me.lblNotaClave)
        Me.grpClave.Location = New System.Drawing.Point(715, 270)
        Me.grpClave.Name = "grpClave"
        Me.grpClave.Size = New System.Drawing.Size(458, 200)
        Me.grpClave.TabIndex = 2
        Me.grpClave.TabStop = False
        Me.grpClave.Text = "Clave del usuario seleccionado"
        '
        ' lblClaveNueva
        '
        Me.lblClaveNueva.AutoSize = True
        Me.lblClaveNueva.Location = New System.Drawing.Point(20, 28)
        Me.lblClaveNueva.Name = "lblClaveNueva"
        Me.lblClaveNueva.Size = New System.Drawing.Size(75, 15)
        Me.lblClaveNueva.TabIndex = 0
        Me.lblClaveNueva.Text = "Clave nueva:"
        '
        ' txtClaveNueva
        '
        Me.txtClaveNueva.Location = New System.Drawing.Point(20, 46)
        Me.txtClaveNueva.MaxLength = 100
        Me.txtClaveNueva.Name = "txtClaveNueva"
        Me.txtClaveNueva.Size = New System.Drawing.Size(200, 23)
        Me.txtClaveNueva.TabIndex = 1
        Me.txtClaveNueva.UseSystemPasswordChar = True
        '
        ' lblClaveConfirmar
        '
        Me.lblClaveConfirmar.AutoSize = True
        Me.lblClaveConfirmar.Location = New System.Drawing.Point(235, 28)
        Me.lblClaveConfirmar.Name = "lblClaveConfirmar"
        Me.lblClaveConfirmar.Size = New System.Drawing.Size(118, 15)
        Me.lblClaveConfirmar.TabIndex = 2
        Me.lblClaveConfirmar.Text = "Confirmar la clave:"
        '
        ' txtClaveConfirmar
        '
        Me.txtClaveConfirmar.Location = New System.Drawing.Point(235, 46)
        Me.txtClaveConfirmar.MaxLength = 100
        Me.txtClaveConfirmar.Name = "txtClaveConfirmar"
        Me.txtClaveConfirmar.Size = New System.Drawing.Size(203, 23)
        Me.txtClaveConfirmar.TabIndex = 3
        Me.txtClaveConfirmar.UseSystemPasswordChar = True
        '
        ' btnRestablecer
        '
        Me.btnRestablecer.Location = New System.Drawing.Point(20, 90)
        Me.btnRestablecer.Name = "btnRestablecer"
        Me.btnRestablecer.Size = New System.Drawing.Size(180, 32)
        Me.btnRestablecer.TabIndex = 4
        Me.btnRestablecer.Text = "Restablecer clave"
        Me.btnRestablecer.UseVisualStyleBackColor = True
        '
        ' lblNotaClave
        '
        Me.lblNotaClave.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaClave.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaClave.Location = New System.Drawing.Point(20, 130)
        Me.lblNotaClave.Name = "lblNotaClave"
        Me.lblNotaClave.Size = New System.Drawing.Size(420, 55)
        Me.lblNotaClave.TabIndex = 5
        Me.lblNotaClave.Text = "La clave debe tener al menos 8 caracteres. Al restablecerla, el usuario deberá cambiarla; comuníquesela por un medio seguro."
        '
        ' btnDesactivar
        '
        Me.btnDesactivar.Location = New System.Drawing.Point(715, 485)
        Me.btnDesactivar.Name = "btnDesactivar"
        Me.btnDesactivar.Size = New System.Drawing.Size(200, 32)
        Me.btnDesactivar.TabIndex = 3
        Me.btnDesactivar.Text = "Desactivar usuario"
        Me.btnDesactivar.UseVisualStyleBackColor = True
        '

        ' btnEliminar
        '
        Me.btnEliminar.Location = New System.Drawing.Point(930, 485)
        Me.btnEliminar.Name = "btnEliminar"
        Me.btnEliminar.Size = New System.Drawing.Size(243, 32)
        Me.btnEliminar.TabIndex = 5
        Me.btnEliminar.Text = "Eliminar definitivamente"
        Me.btnEliminar.UseVisualStyleBackColor = True
        Me.btnEliminar.Enabled = False
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(715, 530)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(458, 60)
        Me.lblMensaje.TabIndex = 4
        '
        ' FrmUsuarios
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1185, 700)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnEliminar)
        Me.Controls.Add(Me.btnDesactivar)
        Me.Controls.Add(Me.grpClave)
        Me.Controls.Add(Me.grpDatos)
        Me.Controls.Add(Me.dgvUsuarios)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1150, 680)
        Me.Name = "FrmUsuarios"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Usuarios del sistema"
        CType(Me.dgvUsuarios, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpDatos.ResumeLayout(False)
        Me.grpDatos.PerformLayout()
        Me.grpClave.ResumeLayout(False)
        Me.grpClave.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents dgvUsuarios As System.Windows.Forms.DataGridView
    Friend WithEvents grpDatos As System.Windows.Forms.GroupBox
    Friend WithEvents lblUsuario As System.Windows.Forms.Label
    Friend WithEvents txtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents lblNombre As System.Windows.Forms.Label
    Friend WithEvents txtNombre As System.Windows.Forms.TextBox
    Friend WithEvents lblRol As System.Windows.Forms.Label
    Friend WithEvents cboRol As System.Windows.Forms.ComboBox
    Friend WithEvents chkActivo As System.Windows.Forms.CheckBox
    Friend WithEvents btnNuevo As System.Windows.Forms.Button
    Friend WithEvents btnGuardar As System.Windows.Forms.Button
    Friend WithEvents grpClave As System.Windows.Forms.GroupBox
    Friend WithEvents lblClaveNueva As System.Windows.Forms.Label
    Friend WithEvents txtClaveNueva As System.Windows.Forms.TextBox
    Friend WithEvents lblClaveConfirmar As System.Windows.Forms.Label
    Friend WithEvents txtClaveConfirmar As System.Windows.Forms.TextBox
    Friend WithEvents btnRestablecer As System.Windows.Forms.Button
    Friend WithEvents lblNotaClave As System.Windows.Forms.Label
    Friend WithEvents btnDesactivar As System.Windows.Forms.Button
    Friend WithEvents btnEliminar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
End Class
