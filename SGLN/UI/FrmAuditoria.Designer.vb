<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmAuditoria
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
        Me.lblUsuario = New System.Windows.Forms.Label()
        Me.txtUsuario = New System.Windows.Forms.TextBox()
        Me.lblTabla = New System.Windows.Forms.Label()
        Me.cboTabla = New System.Windows.Forms.ComboBox()
        Me.lblAccion = New System.Windows.Forms.Label()
        Me.cboAccion = New System.Windows.Forms.ComboBox()
        Me.lblDesde = New System.Windows.Forms.Label()
        Me.dtpDesde = New System.Windows.Forms.DateTimePicker()
        Me.lblHasta = New System.Windows.Forms.Label()
        Me.dtpHasta = New System.Windows.Forms.DateTimePicker()
        Me.btnBuscar = New System.Windows.Forms.Button()
        Me.btnRefrescar = New System.Windows.Forms.Button()
        Me.btnExportar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.dgvAuditoria = New System.Windows.Forms.DataGridView()
        Me.lblResumen = New System.Windows.Forms.Label()
        Me.lblNota = New System.Windows.Forms.Label()
        CType(Me.dgvAuditoria, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        ' lblUsuario
        '
        Me.lblUsuario.AutoSize = True
        Me.lblUsuario.Location = New System.Drawing.Point(12, 12)
        Me.lblUsuario.Name = "lblUsuario"
        Me.lblUsuario.Size = New System.Drawing.Size(50, 15)
        Me.lblUsuario.TabIndex = 0
        Me.lblUsuario.Text = "Usuario:"
        '
        ' txtUsuario
        '
        Me.txtUsuario.Location = New System.Drawing.Point(12, 30)
        Me.txtUsuario.Name = "txtUsuario"
        Me.txtUsuario.Size = New System.Drawing.Size(160, 23)
        Me.txtUsuario.TabIndex = 0
        '
        ' lblTabla
        '
        Me.lblTabla.AutoSize = True
        Me.lblTabla.Location = New System.Drawing.Point(185, 12)
        Me.lblTabla.Name = "lblTabla"
        Me.lblTabla.Size = New System.Drawing.Size(90, 15)
        Me.lblTabla.TabIndex = 2
        Me.lblTabla.Text = "Tabla afectada:"
        '
        ' cboTabla
        '
        Me.cboTabla.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTabla.FormattingEnabled = True
        Me.cboTabla.Location = New System.Drawing.Point(185, 30)
        Me.cboTabla.Name = "cboTabla"
        Me.cboTabla.Size = New System.Drawing.Size(190, 23)
        Me.cboTabla.TabIndex = 1
        '
        ' lblAccion
        '
        Me.lblAccion.AutoSize = True
        Me.lblAccion.Location = New System.Drawing.Point(388, 12)
        Me.lblAccion.Name = "lblAccion"
        Me.lblAccion.Size = New System.Drawing.Size(45, 15)
        Me.lblAccion.TabIndex = 4
        Me.lblAccion.Text = "Acción:"
        '
        ' cboAccion
        '
        Me.cboAccion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboAccion.FormattingEnabled = True
        Me.cboAccion.Location = New System.Drawing.Point(388, 30)
        Me.cboAccion.Name = "cboAccion"
        Me.cboAccion.Size = New System.Drawing.Size(170, 23)
        Me.cboAccion.TabIndex = 2
        '
        ' lblDesde
        '
        Me.lblDesde.AutoSize = True
        Me.lblDesde.Location = New System.Drawing.Point(571, 12)
        Me.lblDesde.Name = "lblDesde"
        Me.lblDesde.Size = New System.Drawing.Size(42, 15)
        Me.lblDesde.TabIndex = 6
        Me.lblDesde.Text = "Desde:"
        '
        ' dtpDesde
        '
        Me.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpDesde.Location = New System.Drawing.Point(571, 30)
        Me.dtpDesde.Name = "dtpDesde"
        Me.dtpDesde.Size = New System.Drawing.Size(130, 23)
        Me.dtpDesde.TabIndex = 3
        '
        ' lblHasta
        '
        Me.lblHasta.AutoSize = True
        Me.lblHasta.Location = New System.Drawing.Point(714, 12)
        Me.lblHasta.Name = "lblHasta"
        Me.lblHasta.Size = New System.Drawing.Size(39, 15)
        Me.lblHasta.TabIndex = 8
        Me.lblHasta.Text = "Hasta:"
        '
        ' dtpHasta
        '
        Me.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpHasta.Location = New System.Drawing.Point(714, 30)
        Me.dtpHasta.Name = "dtpHasta"
        Me.dtpHasta.Size = New System.Drawing.Size(130, 23)
        Me.dtpHasta.TabIndex = 4
        '
        ' btnBuscar
        '
        Me.btnBuscar.Location = New System.Drawing.Point(857, 28)
        Me.btnBuscar.Name = "btnBuscar"
        Me.btnBuscar.Size = New System.Drawing.Size(80, 27)
        Me.btnBuscar.TabIndex = 5
        Me.btnBuscar.Text = "Buscar"
        Me.btnBuscar.UseVisualStyleBackColor = True
        '
        ' btnRefrescar
        '
        Me.btnRefrescar.Location = New System.Drawing.Point(945, 28)
        Me.btnRefrescar.Name = "btnRefrescar"
        Me.btnRefrescar.Size = New System.Drawing.Size(90, 27)
        Me.btnRefrescar.TabIndex = 6
        Me.btnRefrescar.Text = "Refrescar"
        Me.btnRefrescar.UseVisualStyleBackColor = True
        '
        ' btnExportar
        '
        Me.btnExportar.Location = New System.Drawing.Point(1043, 28)
        Me.btnExportar.Name = "btnExportar"
        Me.btnExportar.Size = New System.Drawing.Size(130, 27)
        Me.btnExportar.TabIndex = 7
        Me.btnExportar.Text = "Exportar CSV"
        Me.btnExportar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblMensaje.Location = New System.Drawing.Point(12, 62)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(1161, 35)
        Me.lblMensaje.TabIndex = 13
        '
        ' dgvAuditoria
        '
        Me.dgvAuditoria.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvAuditoria.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvAuditoria.Location = New System.Drawing.Point(12, 100)
        Me.dgvAuditoria.Name = "dgvAuditoria"
        Me.dgvAuditoria.Size = New System.Drawing.Size(1161, 570)
        Me.dgvAuditoria.TabIndex = 8
        '
        ' lblResumen
        '
        Me.lblResumen.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblResumen.Location = New System.Drawing.Point(12, 678)
        Me.lblResumen.Name = "lblResumen"
        Me.lblResumen.Size = New System.Drawing.Size(400, 25)
        Me.lblResumen.TabIndex = 15
        '
        ' lblNota
        '
        Me.lblNota.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNota.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNota.ForeColor = System.Drawing.Color.DimGray
        Me.lblNota.Location = New System.Drawing.Point(430, 678)
        Me.lblNota.Name = "lblNota"
        Me.lblNota.Size = New System.Drawing.Size(743, 25)
        Me.lblNota.TabIndex = 16
        Me.lblNota.Text = "La bitácora es de solo lectura. No se permite editar ni eliminar registros desde la aplicación."
        Me.lblNota.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        ' FrmAuditoria
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1185, 710)
        Me.Controls.Add(Me.lblNota)
        Me.Controls.Add(Me.lblResumen)
        Me.Controls.Add(Me.dgvAuditoria)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnExportar)
        Me.Controls.Add(Me.btnRefrescar)
        Me.Controls.Add(Me.btnBuscar)
        Me.Controls.Add(Me.dtpHasta)
        Me.Controls.Add(Me.lblHasta)
        Me.Controls.Add(Me.dtpDesde)
        Me.Controls.Add(Me.lblDesde)
        Me.Controls.Add(Me.cboAccion)
        Me.Controls.Add(Me.lblAccion)
        Me.Controls.Add(Me.cboTabla)
        Me.Controls.Add(Me.lblTabla)
        Me.Controls.Add(Me.txtUsuario)
        Me.Controls.Add(Me.lblUsuario)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1150, 680)
        Me.Name = "FrmAuditoria"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Auditoría del sistema"
        CType(Me.dgvAuditoria, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblUsuario As System.Windows.Forms.Label
    Friend WithEvents txtUsuario As System.Windows.Forms.TextBox
    Friend WithEvents lblTabla As System.Windows.Forms.Label
    Friend WithEvents cboTabla As System.Windows.Forms.ComboBox
    Friend WithEvents lblAccion As System.Windows.Forms.Label
    Friend WithEvents cboAccion As System.Windows.Forms.ComboBox
    Friend WithEvents lblDesde As System.Windows.Forms.Label
    Friend WithEvents dtpDesde As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblHasta As System.Windows.Forms.Label
    Friend WithEvents dtpHasta As System.Windows.Forms.DateTimePicker
    Friend WithEvents btnBuscar As System.Windows.Forms.Button
    Friend WithEvents btnRefrescar As System.Windows.Forms.Button
    Friend WithEvents btnExportar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents dgvAuditoria As System.Windows.Forms.DataGridView
    Friend WithEvents lblResumen As System.Windows.Forms.Label
    Friend WithEvents lblNota As System.Windows.Forms.Label
End Class
