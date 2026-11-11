<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmDocumentos
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
        Me.txtFiltro = New System.Windows.Forms.TextBox()
        Me.cboFiltroTipo = New System.Windows.Forms.ComboBox()
        Me.btnBuscar = New System.Windows.Forms.Button()
        Me.btnRefrescar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.dgvDocumentos = New System.Windows.Forms.DataGridView()
        Me.grpDetalle = New System.Windows.Forms.GroupBox()
        Me.lblTipoDocumento = New System.Windows.Forms.Label()
        Me.cboTipoDocumento = New System.Windows.Forms.ComboBox()
        Me.lblCaso = New System.Windows.Forms.Label()
        Me.cboCaso = New System.Windows.Forms.ComboBox()
        Me.lblInstrumento = New System.Windows.Forms.Label()
        Me.cboInstrumento = New System.Windows.Forms.ComboBox()
        Me.lblHash = New System.Windows.Forms.Label()
        Me.txtHash = New System.Windows.Forms.TextBox()
        Me.lblEstadoIA = New System.Windows.Forms.Label()
        Me.txtEstadoIA = New System.Windows.Forms.TextBox()
        Me.lblFechaCarga = New System.Windows.Forms.Label()
        Me.txtFechaCarga = New System.Windows.Forms.TextBox()
        Me.btnImportar = New System.Windows.Forms.Button()
        Me.btnAbrir = New System.Windows.Forms.Button()
        Me.btnActualizar = New System.Windows.Forms.Button()
        Me.btnVerificar = New System.Windows.Forms.Button()
        Me.btnProcesarIA = New System.Windows.Forms.Button()
        Me.lblNotaIA = New System.Windows.Forms.Label()
        CType(Me.dgvDocumentos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpDetalle.SuspendLayout()
        Me.SuspendLayout()
        '
        ' txtFiltro
        '
        Me.txtFiltro.Location = New System.Drawing.Point(12, 12)
        Me.txtFiltro.Name = "txtFiltro"
        Me.txtFiltro.Size = New System.Drawing.Size(300, 23)
        Me.txtFiltro.TabIndex = 0
        '
        ' cboFiltroTipo
        '
        Me.cboFiltroTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboFiltroTipo.FormattingEnabled = True
        Me.cboFiltroTipo.Location = New System.Drawing.Point(320, 12)
        Me.cboFiltroTipo.Name = "cboFiltroTipo"
        Me.cboFiltroTipo.Size = New System.Drawing.Size(180, 23)
        Me.cboFiltroTipo.TabIndex = 1
        '
        ' btnBuscar
        '
        Me.btnBuscar.Location = New System.Drawing.Point(510, 10)
        Me.btnBuscar.Name = "btnBuscar"
        Me.btnBuscar.Size = New System.Drawing.Size(80, 27)
        Me.btnBuscar.TabIndex = 2
        Me.btnBuscar.Text = "Buscar"
        Me.btnBuscar.UseVisualStyleBackColor = True
        '
        ' btnRefrescar
        '
        Me.btnRefrescar.Location = New System.Drawing.Point(598, 10)
        Me.btnRefrescar.Name = "btnRefrescar"
        Me.btnRefrescar.Size = New System.Drawing.Size(90, 27)
        Me.btnRefrescar.TabIndex = 3
        Me.btnRefrescar.Text = "Refrescar"
        Me.btnRefrescar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(700, 10)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(480, 40)
        Me.lblMensaje.TabIndex = 4
        '
        ' dgvDocumentos
        '
        Me.dgvDocumentos.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvDocumentos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvDocumentos.Location = New System.Drawing.Point(12, 50)
        Me.dgvDocumentos.Name = "dgvDocumentos"
        Me.dgvDocumentos.Size = New System.Drawing.Size(760, 600)
        Me.dgvDocumentos.TabIndex = 5
        '
        ' grpDetalle
        '
        Me.grpDetalle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpDetalle.Controls.Add(Me.lblTipoDocumento)
        Me.grpDetalle.Controls.Add(Me.cboTipoDocumento)
        Me.grpDetalle.Controls.Add(Me.lblCaso)
        Me.grpDetalle.Controls.Add(Me.cboCaso)
        Me.grpDetalle.Controls.Add(Me.lblInstrumento)
        Me.grpDetalle.Controls.Add(Me.cboInstrumento)
        Me.grpDetalle.Controls.Add(Me.lblHash)
        Me.grpDetalle.Controls.Add(Me.txtHash)
        Me.grpDetalle.Controls.Add(Me.lblEstadoIA)
        Me.grpDetalle.Controls.Add(Me.txtEstadoIA)
        Me.grpDetalle.Controls.Add(Me.lblFechaCarga)
        Me.grpDetalle.Controls.Add(Me.txtFechaCarga)
        Me.grpDetalle.Controls.Add(Me.btnImportar)
        Me.grpDetalle.Controls.Add(Me.btnAbrir)
        Me.grpDetalle.Location = New System.Drawing.Point(785, 50)
        Me.grpDetalle.Name = "grpDetalle"
        Me.grpDetalle.Size = New System.Drawing.Size(395, 420)
        Me.grpDetalle.TabIndex = 6
        Me.grpDetalle.TabStop = False
        Me.grpDetalle.Text = "Documento seleccionado"
        '
        ' lblTipoDocumento
        '
        Me.lblTipoDocumento.AutoSize = True
        Me.lblTipoDocumento.Location = New System.Drawing.Point(15, 22)
        Me.lblTipoDocumento.Name = "lblTipoDocumento"
        Me.lblTipoDocumento.Size = New System.Drawing.Size(100, 15)
        Me.lblTipoDocumento.TabIndex = 0
        Me.lblTipoDocumento.Text = "Tipo de documento:"
        '
        ' cboTipoDocumento
        '
        Me.cboTipoDocumento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTipoDocumento.FormattingEnabled = True
        Me.cboTipoDocumento.Location = New System.Drawing.Point(15, 42)
        Me.cboTipoDocumento.Name = "cboTipoDocumento"
        Me.cboTipoDocumento.Size = New System.Drawing.Size(355, 23)
        Me.cboTipoDocumento.TabIndex = 0
        '
        ' lblCaso
        '
        Me.lblCaso.AutoSize = True
        Me.lblCaso.Location = New System.Drawing.Point(15, 78)
        Me.lblCaso.Name = "lblCaso"
        Me.lblCaso.Size = New System.Drawing.Size(40, 15)
        Me.lblCaso.TabIndex = 2
        Me.lblCaso.Text = "Caso:"
        '
        ' cboCaso
        '
        Me.cboCaso.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCaso.FormattingEnabled = True
        Me.cboCaso.Location = New System.Drawing.Point(15, 98)
        Me.cboCaso.Name = "cboCaso"
        Me.cboCaso.Size = New System.Drawing.Size(355, 23)
        Me.cboCaso.TabIndex = 1
        '
        ' lblInstrumento
        '
        Me.lblInstrumento.AutoSize = True
        Me.lblInstrumento.Location = New System.Drawing.Point(15, 135)
        Me.lblInstrumento.Name = "lblInstrumento"
        Me.lblInstrumento.Size = New System.Drawing.Size(75, 15)
        Me.lblInstrumento.TabIndex = 4
        Me.lblInstrumento.Text = "Instrumento:"
        '
        ' cboInstrumento
        '
        Me.cboInstrumento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboInstrumento.FormattingEnabled = True
        Me.cboInstrumento.Location = New System.Drawing.Point(15, 155)
        Me.cboInstrumento.Name = "cboInstrumento"
        Me.cboInstrumento.Size = New System.Drawing.Size(355, 23)
        Me.cboInstrumento.TabIndex = 2
        '
        ' lblHash
        '
        Me.lblHash.AutoSize = True
        Me.lblHash.Location = New System.Drawing.Point(15, 192)
        Me.lblHash.Name = "lblHash"
        Me.lblHash.Size = New System.Drawing.Size(120, 15)
        Me.lblHash.TabIndex = 6
        Me.lblHash.Text = "Hash SHA-256:"
        '
        ' txtHash
        '
        Me.txtHash.Location = New System.Drawing.Point(15, 212)
        Me.txtHash.Multiline = True
        Me.txtHash.Name = "txtHash"
        Me.txtHash.ReadOnly = True
        Me.txtHash.Size = New System.Drawing.Size(355, 45)
        Me.txtHash.TabIndex = 3
        '
        ' lblEstadoIA
        '
        Me.lblEstadoIA.AutoSize = True
        Me.lblEstadoIA.Location = New System.Drawing.Point(15, 265)
        Me.lblEstadoIA.Name = "lblEstadoIA"
        Me.lblEstadoIA.Size = New System.Drawing.Size(130, 15)
        Me.lblEstadoIA.TabIndex = 8
        Me.lblEstadoIA.Text = "Estado de procesamiento IA:"
        '
        ' txtEstadoIA
        '
        Me.txtEstadoIA.Location = New System.Drawing.Point(15, 285)
        Me.txtEstadoIA.Name = "txtEstadoIA"
        Me.txtEstadoIA.ReadOnly = True
        Me.txtEstadoIA.Size = New System.Drawing.Size(355, 23)
        Me.txtEstadoIA.TabIndex = 4
        '
        ' lblFechaCarga
        '
        Me.lblFechaCarga.AutoSize = True
        Me.lblFechaCarga.Location = New System.Drawing.Point(15, 318)
        Me.lblFechaCarga.Name = "lblFechaCarga"
        Me.lblFechaCarga.Size = New System.Drawing.Size(80, 15)
        Me.lblFechaCarga.TabIndex = 10
        Me.lblFechaCarga.Text = "Fecha de carga:"
        '
        ' txtFechaCarga
        '
        Me.txtFechaCarga.Location = New System.Drawing.Point(15, 338)
        Me.txtFechaCarga.Name = "txtFechaCarga"
        Me.txtFechaCarga.ReadOnly = True
        Me.txtFechaCarga.Size = New System.Drawing.Size(355, 23)
        Me.txtFechaCarga.TabIndex = 5
        '
        ' btnImportar
        '
        Me.btnImportar.Location = New System.Drawing.Point(15, 375)
        Me.btnImportar.Name = "btnImportar"
        Me.btnImportar.Size = New System.Drawing.Size(170, 32)
        Me.btnImportar.TabIndex = 6
        Me.btnImportar.Text = "Importar archivo..."
        Me.btnImportar.UseVisualStyleBackColor = True
        '
        ' btnAbrir
        '
        Me.btnAbrir.Location = New System.Drawing.Point(190, 375)
        Me.btnAbrir.Name = "btnAbrir"
        Me.btnAbrir.Size = New System.Drawing.Size(180, 32)
        Me.btnAbrir.TabIndex = 7
        Me.btnAbrir.Text = "Abrir archivo"
        Me.btnAbrir.UseVisualStyleBackColor = True
        '
        ' btnActualizar
        '
        Me.btnActualizar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnActualizar.Location = New System.Drawing.Point(785, 480)
        Me.btnActualizar.Name = "btnActualizar"
        Me.btnActualizar.Size = New System.Drawing.Size(190, 30)
        Me.btnActualizar.TabIndex = 7
        Me.btnActualizar.Text = "Guardar metadatos"
        Me.btnActualizar.UseVisualStyleBackColor = True
        '
        ' btnVerificar
        '
        Me.btnVerificar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnVerificar.Location = New System.Drawing.Point(985, 480)
        Me.btnVerificar.Name = "btnVerificar"
        Me.btnVerificar.Size = New System.Drawing.Size(195, 30)
        Me.btnVerificar.TabIndex = 8
        Me.btnVerificar.Text = "Verificar integridad"
        Me.btnVerificar.UseVisualStyleBackColor = True
        '
        ' btnProcesarIA
        '
        Me.btnProcesarIA.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnProcesarIA.Enabled = False
        Me.btnProcesarIA.Location = New System.Drawing.Point(785, 520)
        Me.btnProcesarIA.Name = "btnProcesarIA"
        Me.btnProcesarIA.Size = New System.Drawing.Size(395, 30)
        Me.btnProcesarIA.TabIndex = 9
        Me.btnProcesarIA.Text = "Procesar con IA/OCR (fase final)"
        Me.btnProcesarIA.UseVisualStyleBackColor = True
        '
        ' lblNotaIA
        '
        Me.lblNotaIA.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNotaIA.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaIA.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaIA.Location = New System.Drawing.Point(785, 558)
        Me.lblNotaIA.Name = "lblNotaIA"
        Me.lblNotaIA.Size = New System.Drawing.Size(395, 90)
        Me.lblNotaIA.TabIndex = 10
        Me.lblNotaIA.Text = "La IA/OCR se integrará al final del proyecto y solo como apoyo. Ningún resultado automático sustituye la revisión del abogado."
        '
        ' FrmDocumentos
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 680)
        Me.Controls.Add(Me.lblNotaIA)
        Me.Controls.Add(Me.btnProcesarIA)
        Me.Controls.Add(Me.btnVerificar)
        Me.Controls.Add(Me.btnActualizar)
        Me.Controls.Add(Me.grpDetalle)
        Me.Controls.Add(Me.dgvDocumentos)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnRefrescar)
        Me.Controls.Add(Me.btnBuscar)
        Me.Controls.Add(Me.cboFiltroTipo)
        Me.Controls.Add(Me.txtFiltro)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1150, 650)
        Me.Name = "FrmDocumentos"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Documentos e integridad de archivos"
        CType(Me.dgvDocumentos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpDetalle.ResumeLayout(False)
        Me.grpDetalle.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents txtFiltro As System.Windows.Forms.TextBox
    Friend WithEvents cboFiltroTipo As System.Windows.Forms.ComboBox
    Friend WithEvents btnBuscar As System.Windows.Forms.Button
    Friend WithEvents btnRefrescar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents dgvDocumentos As System.Windows.Forms.DataGridView
    Friend WithEvents grpDetalle As System.Windows.Forms.GroupBox
    Friend WithEvents lblTipoDocumento As System.Windows.Forms.Label
    Friend WithEvents cboTipoDocumento As System.Windows.Forms.ComboBox
    Friend WithEvents lblCaso As System.Windows.Forms.Label
    Friend WithEvents cboCaso As System.Windows.Forms.ComboBox
    Friend WithEvents lblInstrumento As System.Windows.Forms.Label
    Friend WithEvents cboInstrumento As System.Windows.Forms.ComboBox
    Friend WithEvents lblHash As System.Windows.Forms.Label
    Friend WithEvents txtHash As System.Windows.Forms.TextBox
    Friend WithEvents lblEstadoIA As System.Windows.Forms.Label
    Friend WithEvents txtEstadoIA As System.Windows.Forms.TextBox
    Friend WithEvents lblFechaCarga As System.Windows.Forms.Label
    Friend WithEvents txtFechaCarga As System.Windows.Forms.TextBox
    Friend WithEvents btnImportar As System.Windows.Forms.Button
    Friend WithEvents btnAbrir As System.Windows.Forms.Button
    Friend WithEvents btnActualizar As System.Windows.Forms.Button
    Friend WithEvents btnVerificar As System.Windows.Forms.Button
    Friend WithEvents btnProcesarIA As System.Windows.Forms.Button
    Friend WithEvents lblNotaIA As System.Windows.Forms.Label
End Class
