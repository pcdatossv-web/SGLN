<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmRoles
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
        Me.dgvRoles = New System.Windows.Forms.DataGridView()
        Me.grpEditor = New System.Windows.Forms.GroupBox()
        Me.lblNombre = New System.Windows.Forms.Label()
        Me.txtNombre = New System.Windows.Forms.TextBox()
        Me.chkActivo = New System.Windows.Forms.CheckBox()
        Me.lblDescripcion = New System.Windows.Forms.Label()
        Me.txtDescripcion = New System.Windows.Forms.TextBox()
        Me.lblPermisos = New System.Windows.Forms.Label()
        Me.clbPermisos = New System.Windows.Forms.CheckedListBox()
        Me.btnNuevo = New System.Windows.Forms.Button()
        Me.btnGuardar = New System.Windows.Forms.Button()
        Me.btnEliminar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        CType(Me.dgvRoles, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpEditor.SuspendLayout()
        Me.SuspendLayout()
        '
        ' dgvRoles
        '
        Me.dgvRoles.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.dgvRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRoles.Location = New System.Drawing.Point(12, 12)
        Me.dgvRoles.Name = "dgvRoles"
        Me.dgvRoles.Size = New System.Drawing.Size(500, 660)
        Me.dgvRoles.TabIndex = 0
        '
        ' grpEditor
        '
        Me.grpEditor.Controls.Add(Me.lblNombre)
        Me.grpEditor.Controls.Add(Me.txtNombre)
        Me.grpEditor.Controls.Add(Me.chkActivo)
        Me.grpEditor.Controls.Add(Me.lblDescripcion)
        Me.grpEditor.Controls.Add(Me.txtDescripcion)
        Me.grpEditor.Controls.Add(Me.lblPermisos)
        Me.grpEditor.Controls.Add(Me.clbPermisos)
        Me.grpEditor.Controls.Add(Me.btnNuevo)
        Me.grpEditor.Controls.Add(Me.btnGuardar)
        Me.grpEditor.Controls.Add(Me.btnEliminar)
        Me.grpEditor.Controls.Add(Me.lblMensaje)
        Me.grpEditor.Location = New System.Drawing.Point(528, 12)
        Me.grpEditor.Name = "grpEditor"
        Me.grpEditor.Size = New System.Drawing.Size(645, 660)
        Me.grpEditor.TabIndex = 1
        Me.grpEditor.TabStop = False
        Me.grpEditor.Text = "Rol y permisos"
        '
        ' lblNombre
        '
        Me.lblNombre.AutoSize = True
        Me.lblNombre.Location = New System.Drawing.Point(20, 28)
        Me.lblNombre.Name = "lblNombre"
        Me.lblNombre.Size = New System.Drawing.Size(54, 15)
        Me.lblNombre.TabIndex = 0
        Me.lblNombre.Text = "Nombre:"
        '
        ' txtNombre
        '
        Me.txtNombre.Location = New System.Drawing.Point(20, 46)
        Me.txtNombre.MaxLength = 50
        Me.txtNombre.Name = "txtNombre"
        Me.txtNombre.Size = New System.Drawing.Size(280, 23)
        Me.txtNombre.TabIndex = 1
        '
        ' chkActivo
        '
        Me.chkActivo.AutoSize = True
        Me.chkActivo.Location = New System.Drawing.Point(320, 48)
        Me.chkActivo.Name = "chkActivo"
        Me.chkActivo.Size = New System.Drawing.Size(60, 19)
        Me.chkActivo.TabIndex = 2
        Me.chkActivo.Text = "Activo"
        Me.chkActivo.UseVisualStyleBackColor = True
        '
        ' lblDescripcion
        '
        Me.lblDescripcion.AutoSize = True
        Me.lblDescripcion.Location = New System.Drawing.Point(20, 85)
        Me.lblDescripcion.Name = "lblDescripcion"
        Me.lblDescripcion.Size = New System.Drawing.Size(72, 15)
        Me.lblDescripcion.TabIndex = 3
        Me.lblDescripcion.Text = "Descripción:"
        '
        ' txtDescripcion
        '
        Me.txtDescripcion.Location = New System.Drawing.Point(20, 103)
        Me.txtDescripcion.MaxLength = 200
        Me.txtDescripcion.Multiline = True
        Me.txtDescripcion.Name = "txtDescripcion"
        Me.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDescripcion.Size = New System.Drawing.Size(600, 55)
        Me.txtDescripcion.TabIndex = 4
        '
        ' lblPermisos
        '
        Me.lblPermisos.AutoSize = True
        Me.lblPermisos.Location = New System.Drawing.Point(20, 175)
        Me.lblPermisos.Name = "lblPermisos"
        Me.lblPermisos.Size = New System.Drawing.Size(305, 15)
        Me.lblPermisos.TabIndex = 5
        Me.lblPermisos.Text = "Permisos del rol (marque las opciones permitidas):"
        '
        ' clbPermisos
        '
        Me.clbPermisos.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.clbPermisos.CheckOnClick = True
        Me.clbPermisos.FormattingEnabled = True
        Me.clbPermisos.IntegralHeight = False
        Me.clbPermisos.Location = New System.Drawing.Point(20, 193)
        Me.clbPermisos.Name = "clbPermisos"
        Me.clbPermisos.Size = New System.Drawing.Size(600, 319)
        Me.clbPermisos.TabIndex = 6
        '
        ' btnNuevo
        '
        Me.btnNuevo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnNuevo.Location = New System.Drawing.Point(20, 535)
        Me.btnNuevo.Name = "btnNuevo"
        Me.btnNuevo.Size = New System.Drawing.Size(120, 32)
        Me.btnNuevo.TabIndex = 7
        Me.btnNuevo.Text = "Nuevo rol"
        Me.btnNuevo.UseVisualStyleBackColor = True
        '
        ' btnGuardar
        '
        Me.btnGuardar.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnGuardar.Location = New System.Drawing.Point(150, 535)
        Me.btnGuardar.Name = "btnGuardar"
        Me.btnGuardar.Size = New System.Drawing.Size(120, 32)
        Me.btnGuardar.TabIndex = 8
        Me.btnGuardar.Text = "Guardar"
        Me.btnGuardar.UseVisualStyleBackColor = True
        '
        ' btnEliminar
        '
        Me.btnEliminar.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnEliminar.Location = New System.Drawing.Point(280, 535)
        Me.btnEliminar.Name = "btnEliminar"
        Me.btnEliminar.Size = New System.Drawing.Size(120, 32)
        Me.btnEliminar.TabIndex = 9
        Me.btnEliminar.Text = "Eliminar rol"
        Me.btnEliminar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblMensaje.Location = New System.Drawing.Point(20, 580)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(600, 65)
        Me.lblMensaje.TabIndex = 10
        '
        ' FrmRoles
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1185, 700)
        Me.Controls.Add(Me.grpEditor)
        Me.Controls.Add(Me.dgvRoles)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1150, 680)
        Me.Name = "FrmRoles"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Roles y permisos"
        CType(Me.dgvRoles, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpEditor.ResumeLayout(False)
        Me.grpEditor.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents dgvRoles As System.Windows.Forms.DataGridView
    Friend WithEvents grpEditor As System.Windows.Forms.GroupBox
    Friend WithEvents lblNombre As System.Windows.Forms.Label
    Friend WithEvents txtNombre As System.Windows.Forms.TextBox
    Friend WithEvents chkActivo As System.Windows.Forms.CheckBox
    Friend WithEvents lblDescripcion As System.Windows.Forms.Label
    Friend WithEvents txtDescripcion As System.Windows.Forms.TextBox
    Friend WithEvents lblPermisos As System.Windows.Forms.Label
    Friend WithEvents clbPermisos As System.Windows.Forms.CheckedListBox
    Friend WithEvents btnNuevo As System.Windows.Forms.Button
    Friend WithEvents btnGuardar As System.Windows.Forms.Button
    Friend WithEvents btnEliminar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
End Class
