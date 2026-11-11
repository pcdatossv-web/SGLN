<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmPrincipal
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
        Me.mnuPrincipal = New System.Windows.Forms.MenuStrip()
        Me.mnuArchivo = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCerrarSesion = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSalir = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuModulos = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuClientes = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEntidades = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCasos = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuProtocolo = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDocumentos = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCumplimiento = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAuditoria = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuHerramientas = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuConfiguracion = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuUsuarios = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRoles = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAyuda = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAcercaDe = New System.Windows.Forms.ToolStripMenuItem()
        Me.statusPrincipal = New System.Windows.Forms.StatusStrip()
        Me.lblUsuario = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblRol = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblFecha = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblBienvenida = New System.Windows.Forms.Label()
        Me.lblResumen = New System.Windows.Forms.Label()
        Me.grpAlertas = New System.Windows.Forms.GroupBox()
        Me.dgvAlertas = New System.Windows.Forms.DataGridView()
        Me.btnActualizar = New System.Windows.Forms.Button()
        Me.lblNota = New System.Windows.Forms.Label()
        Me.mnuPrincipal.SuspendLayout()
        Me.statusPrincipal.SuspendLayout()
        Me.grpAlertas.SuspendLayout()
        CType(Me.dgvAlertas, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        ' mnuPrincipal
        '
        Me.mnuPrincipal.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuArchivo, Me.mnuModulos, Me.mnuHerramientas, Me.mnuAyuda})
        Me.mnuPrincipal.Location = New System.Drawing.Point(0, 0)
        Me.mnuPrincipal.Name = "mnuPrincipal"
        Me.mnuPrincipal.Size = New System.Drawing.Size(1150, 24)
        Me.mnuPrincipal.TabIndex = 0
        '
        ' mnuArchivo
        '
        Me.mnuArchivo.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuCerrarSesion, Me.mnuSalir})
        Me.mnuArchivo.Name = "mnuArchivo"
        Me.mnuArchivo.Size = New System.Drawing.Size(60, 20)
        Me.mnuArchivo.Text = "&Archivo"
        '
        ' mnuCerrarSesion
        '
        Me.mnuCerrarSesion.Name = "mnuCerrarSesion"
        Me.mnuCerrarSesion.Size = New System.Drawing.Size(180, 22)
        Me.mnuCerrarSesion.Text = "Cerrar sesión"
        '
        ' mnuSalir
        '
        Me.mnuSalir.Name = "mnuSalir"
        Me.mnuSalir.Size = New System.Drawing.Size(180, 22)
        Me.mnuSalir.Text = "Salir"
        '
        ' mnuModulos
        '
        Me.mnuModulos.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuClientes, Me.mnuEntidades, Me.mnuCasos, Me.mnuProtocolo, Me.mnuDocumentos, Me.mnuCumplimiento, Me.mnuAuditoria})
        Me.mnuModulos.Name = "mnuModulos"
        Me.mnuModulos.Size = New System.Drawing.Size(67, 20)
        Me.mnuModulos.Text = "&Módulos"
        '
        ' mnuClientes
        '
        Me.mnuClientes.Name = "mnuClientes"
        Me.mnuClientes.Size = New System.Drawing.Size(240, 22)
        Me.mnuClientes.Text = "Clientes y debida diligencia"
        Me.mnuClientes.Tag = "clientes.ver"
        '
        ' mnuEntidades
        '
        Me.mnuEntidades.Name = "mnuEntidades"
        Me.mnuEntidades.Size = New System.Drawing.Size(240, 22)
        Me.mnuEntidades.Text = "Entidades legales"
        Me.mnuEntidades.Tag = "entidades.ver"
        '
        ' mnuCasos
        '
        Me.mnuCasos.Name = "mnuCasos"
        Me.mnuCasos.Size = New System.Drawing.Size(240, 22)
        Me.mnuCasos.Text = "Casos y plazos procesales"
        Me.mnuCasos.Tag = "casos.ver"
        '
        ' mnuProtocolo
        '
        Me.mnuProtocolo.Name = "mnuProtocolo"
        Me.mnuProtocolo.Size = New System.Drawing.Size(240, 22)
        Me.mnuProtocolo.Text = "Protocolo notarial"
        Me.mnuProtocolo.Tag = "protocolo.ver"
        '
        ' mnuDocumentos
        '
        Me.mnuDocumentos.Name = "mnuDocumentos"
        Me.mnuDocumentos.Size = New System.Drawing.Size(240, 22)
        Me.mnuDocumentos.Text = "Documentos"
        Me.mnuDocumentos.Tag = "documentos.ver"
        '
        ' mnuCumplimiento
        '
        Me.mnuCumplimiento.Name = "mnuCumplimiento"
        Me.mnuCumplimiento.Size = New System.Drawing.Size(240, 22)
        Me.mnuCumplimiento.Text = "Cumplimiento LDA/FT y ARCO"
        Me.mnuCumplimiento.Tag = "cumplimiento.ver"
        '
        ' mnuAuditoria
        '
        Me.mnuAuditoria.Name = "mnuAuditoria"
        Me.mnuAuditoria.Size = New System.Drawing.Size(240, 22)
        Me.mnuAuditoria.Text = "Auditoría"
        Me.mnuAuditoria.Tag = "auditoria.ver"
        '
        ' mnuHerramientas
        '
        Me.mnuHerramientas.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuConfiguracion, Me.mnuUsuarios, Me.mnuRoles})
        Me.mnuHerramientas.Name = "mnuHerramientas"
        Me.mnuHerramientas.Size = New System.Drawing.Size(90, 20)
        Me.mnuHerramientas.Text = "&Herramientas"
        '
        ' mnuConfiguracion
        '
        Me.mnuConfiguracion.Name = "mnuConfiguracion"
        Me.mnuConfiguracion.Size = New System.Drawing.Size(200, 22)
        Me.mnuConfiguracion.Text = "Configuración y respaldos"
        Me.mnuConfiguracion.Tag = "config.ver"
        '
        ' mnuUsuarios
        '
        Me.mnuUsuarios.Name = "mnuUsuarios"
        Me.mnuUsuarios.Size = New System.Drawing.Size(200, 22)
        Me.mnuUsuarios.Text = "Usuarios"
        Me.mnuUsuarios.Tag = "config.usuarios"
        '
        ' mnuRoles
        '
        Me.mnuRoles.Name = "mnuRoles"
        Me.mnuRoles.Size = New System.Drawing.Size(200, 22)
        Me.mnuRoles.Text = "Roles y permisos"
        Me.mnuRoles.Tag = "config.roles"
        '
        ' mnuAyuda
        '
        Me.mnuAyuda.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuAcercaDe})
        Me.mnuAyuda.Name = "mnuAyuda"
        Me.mnuAyuda.Size = New System.Drawing.Size(53, 20)
        Me.mnuAyuda.Text = "A&yuda"
        '
        ' mnuAcercaDe
        '
        Me.mnuAcercaDe.Name = "mnuAcercaDe"
        Me.mnuAcercaDe.Size = New System.Drawing.Size(180, 22)
        Me.mnuAcercaDe.Text = "Acerca de..."
        '
        ' statusPrincipal
        '
        Me.statusPrincipal.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblUsuario, Me.lblRol, Me.lblFecha})
        Me.statusPrincipal.Location = New System.Drawing.Point(0, 678)
        Me.statusPrincipal.Name = "statusPrincipal"
        Me.statusPrincipal.Size = New System.Drawing.Size(1150, 22)
        Me.statusPrincipal.TabIndex = 1
        '
        ' lblUsuario
        '
        Me.lblUsuario.Name = "lblUsuario"
        Me.lblUsuario.Size = New System.Drawing.Size(60, 17)
        Me.lblUsuario.Text = "Usuario: -"
        '
        ' lblRol
        '
        Me.lblRol.Name = "lblRol"
        Me.lblRol.Size = New System.Drawing.Size(100, 17)
        Me.lblRol.Spring = True
        Me.lblRol.Text = "Rol: -"
        Me.lblRol.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblFecha
        '
        Me.lblFecha.Name = "lblFecha"
        Me.lblFecha.Size = New System.Drawing.Size(100, 17)
        Me.lblFecha.Text = "-"
        '
        ' lblBienvenida
        '
        Me.lblBienvenida.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lblBienvenida.ForeColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(52, Byte), Integer), CType(CType(92, Byte), Integer))
        Me.lblBienvenida.Location = New System.Drawing.Point(20, 34)
        Me.lblBienvenida.Name = "lblBienvenida"
        Me.lblBienvenida.Size = New System.Drawing.Size(700, 28)
        Me.lblBienvenida.TabIndex = 2
        Me.lblBienvenida.Text = "Panel de control"
        '
        ' lblResumen
        '
        Me.lblResumen.ForeColor = System.Drawing.Color.DimGray
        Me.lblResumen.Location = New System.Drawing.Point(22, 65)
        Me.lblResumen.Name = "lblResumen"
        Me.lblResumen.Size = New System.Drawing.Size(1100, 20)
        Me.lblResumen.TabIndex = 3
        Me.lblResumen.Text = "Cargando información..."
        '
        ' grpAlertas
        '
        Me.grpAlertas.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.grpAlertas.Controls.Add(Me.dgvAlertas)
        Me.grpAlertas.Location = New System.Drawing.Point(22, 92)
        Me.grpAlertas.Name = "grpAlertas"
        Me.grpAlertas.Padding = New System.Windows.Forms.Padding(8)
        Me.grpAlertas.Size = New System.Drawing.Size(1106, 500)
        Me.grpAlertas.TabIndex = 4
        Me.grpAlertas.TabStop = False
        Me.grpAlertas.Text = "Alertas y pendientes del bufete"
        '
        ' dgvAlertas
        '
        Me.dgvAlertas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvAlertas.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvAlertas.Location = New System.Drawing.Point(8, 24)
        Me.dgvAlertas.Name = "dgvAlertas"
        Me.dgvAlertas.Size = New System.Drawing.Size(1090, 468)
        Me.dgvAlertas.TabIndex = 0
        '
        ' btnActualizar
        '
        Me.btnActualizar.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnActualizar.Location = New System.Drawing.Point(22, 604)
        Me.btnActualizar.Name = "btnActualizar"
        Me.btnActualizar.Size = New System.Drawing.Size(160, 32)
        Me.btnActualizar.TabIndex = 5
        Me.btnActualizar.Text = "Actualizar panel"
        Me.btnActualizar.UseVisualStyleBackColor = True
        '
        ' lblNota
        '
        Me.lblNota.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNota.ForeColor = System.Drawing.Color.DimGray
        Me.lblNota.Location = New System.Drawing.Point(198, 604)
        Me.lblNota.Name = "lblNota"
        Me.lblNota.Size = New System.Drawing.Size(930, 58)
        Me.lblNota.TabIndex = 6
        Me.lblNota.Text = "Los vencimientos de plazos los calcula el sistema (días hábiles + feriados) y deben ser confirmados por el abogado. La IA/OCR se integrará en la fase final: nunca fijará plazos ni sustituirá la revisión legal."
        '
        ' FrmPrincipal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1150, 700)
        Me.Controls.Add(Me.lblNota)
        Me.Controls.Add(Me.btnActualizar)
        Me.Controls.Add(Me.grpAlertas)
        Me.Controls.Add(Me.lblResumen)
        Me.Controls.Add(Me.lblBienvenida)
        Me.Controls.Add(Me.statusPrincipal)
        Me.Controls.Add(Me.mnuPrincipal)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MainMenuStrip = Me.mnuPrincipal
        Me.MinimumSize = New System.Drawing.Size(1000, 650)
        Me.Name = "FrmPrincipal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SGLN El Salvador - Sistema de Gestión Legal y Notarial"
        Me.mnuPrincipal.ResumeLayout(False)
        Me.mnuPrincipal.PerformLayout()
        Me.statusPrincipal.ResumeLayout(False)
        Me.grpAlertas.ResumeLayout(False)
        CType(Me.dgvAlertas, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents mnuPrincipal As System.Windows.Forms.MenuStrip
    Friend WithEvents mnuArchivo As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCerrarSesion As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSalir As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuModulos As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuClientes As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEntidades As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCasos As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuProtocolo As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDocumentos As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCumplimiento As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAuditoria As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuHerramientas As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuConfiguracion As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuUsuarios As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuRoles As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAyuda As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAcercaDe As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents statusPrincipal As System.Windows.Forms.StatusStrip
    Friend WithEvents lblUsuario As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblRol As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblFecha As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblBienvenida As System.Windows.Forms.Label
    Friend WithEvents lblResumen As System.Windows.Forms.Label
    Friend WithEvents grpAlertas As System.Windows.Forms.GroupBox
    Friend WithEvents dgvAlertas As System.Windows.Forms.DataGridView
    Friend WithEvents btnActualizar As System.Windows.Forms.Button
    Friend WithEvents lblNota As System.Windows.Forms.Label
End Class
