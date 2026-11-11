<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmCumplimiento
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
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.tabPrincipal = New System.Windows.Forms.TabControl()
        Me.tabOperaciones = New System.Windows.Forms.TabPage()
        Me.txtFiltroOp = New System.Windows.Forms.TextBox()
        Me.btnBuscarOp = New System.Windows.Forms.Button()
        Me.btnRefrescarOp = New System.Windows.Forms.Button()
        Me.dgvOperaciones = New System.Windows.Forms.DataGridView()
        Me.grpOperacion = New System.Windows.Forms.GroupBox()
        Me.lblTipoOperacion = New System.Windows.Forms.Label()
        Me.cboTipoOperacion = New System.Windows.Forms.ComboBox()
        Me.lblMonto = New System.Windows.Forms.Label()
        Me.nudMonto = New System.Windows.Forms.NumericUpDown()
        Me.lblMoneda = New System.Windows.Forms.Label()
        Me.cboMoneda = New System.Windows.Forms.ComboBox()
        Me.lblFechaOperacion = New System.Windows.Forms.Label()
        Me.dtpFechaOperacion = New System.Windows.Forms.DateTimePicker()
        Me.lblEstadoOperacion = New System.Windows.Forms.Label()
        Me.cboEstadoOperacion = New System.Windows.Forms.ComboBox()
        Me.lblCasoOp = New System.Windows.Forms.Label()
        Me.cboCasoOp = New System.Windows.Forms.ComboBox()
        Me.lblInstrumentoOp = New System.Windows.Forms.Label()
        Me.cboInstrumentoOp = New System.Windows.Forms.ComboBox()
        Me.lblDescripcionOp = New System.Windows.Forms.Label()
        Me.txtDescripcionOp = New System.Windows.Forms.TextBox()
        Me.lblAnalisis = New System.Windows.Forms.Label()
        Me.txtAnalisis = New System.Windows.Forms.TextBox()
        Me.lblReporte = New System.Windows.Forms.Label()
        Me.txtReporte = New System.Windows.Forms.TextBox()
        Me.lblAlertaUmbral = New System.Windows.Forms.Label()
        Me.lblObservacionesOp = New System.Windows.Forms.Label()
        Me.txtObservacionesOp = New System.Windows.Forms.TextBox()
        Me.btnNuevoOp = New System.Windows.Forms.Button()
        Me.btnGuardarOp = New System.Windows.Forms.Button()
        Me.btnReportarOp = New System.Windows.Forms.Button()
        Me.btnDescartarOp = New System.Windows.Forms.Button()
        Me.lblNotaUmbral = New System.Windows.Forms.Label()
        Me.tabArco = New System.Windows.Forms.TabPage()
        Me.txtFiltroArco = New System.Windows.Forms.TextBox()
        Me.btnBuscarArco = New System.Windows.Forms.Button()
        Me.btnRefrescarArco = New System.Windows.Forms.Button()
        Me.dgvArco = New System.Windows.Forms.DataGridView()
        Me.grpArco = New System.Windows.Forms.GroupBox()
        Me.lblClienteArco = New System.Windows.Forms.Label()
        Me.cboClienteArco = New System.Windows.Forms.ComboBox()
        Me.lblTipoDerecho = New System.Windows.Forms.Label()
        Me.cboTipoDerecho = New System.Windows.Forms.ComboBox()
        Me.lblFechaSolicitud = New System.Windows.Forms.Label()
        Me.dtpFechaSolicitud = New System.Windows.Forms.DateTimePicker()
        Me.lblEstadoArco = New System.Windows.Forms.Label()
        Me.cboEstadoArco = New System.Windows.Forms.ComboBox()
        Me.lblVencimientoArco = New System.Windows.Forms.Label()
        Me.txtVencimientoArco = New System.Windows.Forms.TextBox()
        Me.chkProrroga = New System.Windows.Forms.CheckBox()
        Me.lblVencimientoProrroga = New System.Windows.Forms.Label()
        Me.txtVencimientoProrroga = New System.Windows.Forms.TextBox()
        Me.lblDetalleArco = New System.Windows.Forms.Label()
        Me.txtDetalleArco = New System.Windows.Forms.TextBox()
        Me.lblFechaRespuesta = New System.Windows.Forms.Label()
        Me.dtpFechaRespuesta = New System.Windows.Forms.DateTimePicker()
        Me.btnNuevoArco = New System.Windows.Forms.Button()
        Me.btnGuardarArco = New System.Windows.Forms.Button()
        Me.btnResponderArco = New System.Windows.Forms.Button()
        Me.lblNotaArco = New System.Windows.Forms.Label()
        Me.tabPrincipal.SuspendLayout()
        Me.tabOperaciones.SuspendLayout()
        CType(Me.dgvOperaciones, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpOperacion.SuspendLayout()
        CType(Me.nudMonto, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabArco.SuspendLayout()
        CType(Me.dgvArco, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpArco.SuspendLayout()
        Me.SuspendLayout()
        '
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(12, 8)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(1216, 35)
        Me.lblMensaje.TabIndex = 0
        '
        ' tabPrincipal
        '
        Me.tabPrincipal.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabPrincipal.Controls.Add(Me.tabOperaciones)
        Me.tabPrincipal.Controls.Add(Me.tabArco)
        Me.tabPrincipal.Location = New System.Drawing.Point(12, 50)
        Me.tabPrincipal.Name = "tabPrincipal"
        Me.tabPrincipal.SelectedIndex = 0
        Me.tabPrincipal.Size = New System.Drawing.Size(1216, 678)
        Me.tabPrincipal.TabIndex = 1
        '
        ' tabOperaciones
        '
        Me.tabOperaciones.Controls.Add(Me.txtFiltroOp)
        Me.tabOperaciones.Controls.Add(Me.btnBuscarOp)
        Me.tabOperaciones.Controls.Add(Me.btnRefrescarOp)
        Me.tabOperaciones.Controls.Add(Me.dgvOperaciones)
        Me.tabOperaciones.Controls.Add(Me.grpOperacion)
        Me.tabOperaciones.Location = New System.Drawing.Point(4, 24)
        Me.tabOperaciones.Name = "tabOperaciones"
        Me.tabOperaciones.Padding = New System.Windows.Forms.Padding(3)
        Me.tabOperaciones.Size = New System.Drawing.Size(1208, 650)
        Me.tabOperaciones.TabIndex = 0
        Me.tabOperaciones.Text = "Operaciones LDA/FT"
        Me.tabOperaciones.UseVisualStyleBackColor = True
        '
        ' txtFiltroOp
        '
        Me.txtFiltroOp.Location = New System.Drawing.Point(10, 10)
        Me.txtFiltroOp.Name = "txtFiltroOp"
        Me.txtFiltroOp.Size = New System.Drawing.Size(300, 23)
        Me.txtFiltroOp.TabIndex = 0
        '
        ' btnBuscarOp
        '
        Me.btnBuscarOp.Location = New System.Drawing.Point(320, 8)
        Me.btnBuscarOp.Name = "btnBuscarOp"
        Me.btnBuscarOp.Size = New System.Drawing.Size(80, 27)
        Me.btnBuscarOp.TabIndex = 1
        Me.btnBuscarOp.Text = "Buscar"
        Me.btnBuscarOp.UseVisualStyleBackColor = True
        '
        ' btnRefrescarOp
        '
        Me.btnRefrescarOp.Location = New System.Drawing.Point(408, 8)
        Me.btnRefrescarOp.Name = "btnRefrescarOp"
        Me.btnRefrescarOp.Size = New System.Drawing.Size(90, 27)
        Me.btnRefrescarOp.TabIndex = 2
        Me.btnRefrescarOp.Text = "Refrescar"
        Me.btnRefrescarOp.UseVisualStyleBackColor = True
        '
        ' dgvOperaciones
        '
        Me.dgvOperaciones.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvOperaciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvOperaciones.Location = New System.Drawing.Point(10, 45)
        Me.dgvOperaciones.Name = "dgvOperaciones"
        Me.dgvOperaciones.Size = New System.Drawing.Size(640, 595)
        Me.dgvOperaciones.TabIndex = 3
        '
        ' grpOperacion
        '
        Me.grpOperacion.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpOperacion.Controls.Add(Me.lblTipoOperacion)
        Me.grpOperacion.Controls.Add(Me.cboTipoOperacion)
        Me.grpOperacion.Controls.Add(Me.lblMonto)
        Me.grpOperacion.Controls.Add(Me.nudMonto)
        Me.grpOperacion.Controls.Add(Me.lblMoneda)
        Me.grpOperacion.Controls.Add(Me.cboMoneda)
        Me.grpOperacion.Controls.Add(Me.lblFechaOperacion)
        Me.grpOperacion.Controls.Add(Me.dtpFechaOperacion)
        Me.grpOperacion.Controls.Add(Me.lblEstadoOperacion)
        Me.grpOperacion.Controls.Add(Me.cboEstadoOperacion)
        Me.grpOperacion.Controls.Add(Me.lblCasoOp)
        Me.grpOperacion.Controls.Add(Me.cboCasoOp)
        Me.grpOperacion.Controls.Add(Me.lblInstrumentoOp)
        Me.grpOperacion.Controls.Add(Me.cboInstrumentoOp)
        Me.grpOperacion.Controls.Add(Me.lblDescripcionOp)
        Me.grpOperacion.Controls.Add(Me.txtDescripcionOp)
        Me.grpOperacion.Controls.Add(Me.lblAnalisis)
        Me.grpOperacion.Controls.Add(Me.txtAnalisis)
        Me.grpOperacion.Controls.Add(Me.lblReporte)
        Me.grpOperacion.Controls.Add(Me.txtReporte)
        Me.grpOperacion.Controls.Add(Me.lblAlertaUmbral)
        Me.grpOperacion.Controls.Add(Me.lblObservacionesOp)
        Me.grpOperacion.Controls.Add(Me.txtObservacionesOp)
        Me.grpOperacion.Controls.Add(Me.btnNuevoOp)
        Me.grpOperacion.Controls.Add(Me.btnGuardarOp)
        Me.grpOperacion.Controls.Add(Me.btnReportarOp)
        Me.grpOperacion.Controls.Add(Me.btnDescartarOp)
        Me.grpOperacion.Controls.Add(Me.lblNotaUmbral)
        Me.grpOperacion.Location = New System.Drawing.Point(660, 10)
        Me.grpOperacion.Name = "grpOperacion"
        Me.grpOperacion.Size = New System.Drawing.Size(538, 620)
        Me.grpOperacion.TabIndex = 4
        Me.grpOperacion.TabStop = False
        Me.grpOperacion.Text = "Operación LDA/FT"
        '
        ' lblTipoOperacion
        '
        Me.lblTipoOperacion.AutoSize = True
        Me.lblTipoOperacion.Location = New System.Drawing.Point(15, 22)
        Me.lblTipoOperacion.Name = "lblTipoOperacion"
        Me.lblTipoOperacion.Size = New System.Drawing.Size(110, 15)
        Me.lblTipoOperacion.TabIndex = 0
        Me.lblTipoOperacion.Text = "Tipo de operación:"
        '
        ' cboTipoOperacion
        '
        Me.cboTipoOperacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTipoOperacion.FormattingEnabled = True
        Me.cboTipoOperacion.Location = New System.Drawing.Point(15, 42)
        Me.cboTipoOperacion.Name = "cboTipoOperacion"
        Me.cboTipoOperacion.Size = New System.Drawing.Size(180, 23)
        Me.cboTipoOperacion.TabIndex = 0
        '
        ' lblMonto
        '
        Me.lblMonto.AutoSize = True
        Me.lblMonto.Location = New System.Drawing.Point(210, 22)
        Me.lblMonto.Name = "lblMonto"
        Me.lblMonto.Size = New System.Drawing.Size(45, 15)
        Me.lblMonto.TabIndex = 2
        Me.lblMonto.Text = "Monto:"
        '
        ' nudMonto
        '
        Me.nudMonto.DecimalPlaces = 2
        Me.nudMonto.Location = New System.Drawing.Point(210, 42)
        Me.nudMonto.Maximum = New Decimal(New Integer() {999999999, 0, 0, 0})
        Me.nudMonto.Name = "nudMonto"
        Me.nudMonto.Size = New System.Drawing.Size(150, 23)
        Me.nudMonto.TabIndex = 1
        Me.nudMonto.ThousandsSeparator = True
        '
        ' lblMoneda
        '
        Me.lblMoneda.AutoSize = True
        Me.lblMoneda.Location = New System.Drawing.Point(375, 22)
        Me.lblMoneda.Name = "lblMoneda"
        Me.lblMoneda.Size = New System.Drawing.Size(55, 15)
        Me.lblMoneda.TabIndex = 4
        Me.lblMoneda.Text = "Moneda:"
        '
        ' cboMoneda
        '
        Me.cboMoneda.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMoneda.FormattingEnabled = True
        Me.cboMoneda.Location = New System.Drawing.Point(375, 42)
        Me.cboMoneda.Name = "cboMoneda"
        Me.cboMoneda.Size = New System.Drawing.Size(80, 23)
        Me.cboMoneda.TabIndex = 2
        '
        ' lblFechaOperacion
        '
        Me.lblFechaOperacion.AutoSize = True
        Me.lblFechaOperacion.Location = New System.Drawing.Point(15, 78)
        Me.lblFechaOperacion.Name = "lblFechaOperacion"
        Me.lblFechaOperacion.Size = New System.Drawing.Size(90, 15)
        Me.lblFechaOperacion.TabIndex = 6
        Me.lblFechaOperacion.Text = "Fecha operación:"
        '
        ' dtpFechaOperacion
        '
        Me.dtpFechaOperacion.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaOperacion.Location = New System.Drawing.Point(15, 98)
        Me.dtpFechaOperacion.Name = "dtpFechaOperacion"
        Me.dtpFechaOperacion.Size = New System.Drawing.Size(150, 23)
        Me.dtpFechaOperacion.TabIndex = 3
        '
        ' lblEstadoOperacion
        '
        Me.lblEstadoOperacion.AutoSize = True
        Me.lblEstadoOperacion.Location = New System.Drawing.Point(180, 78)
        Me.lblEstadoOperacion.Name = "lblEstadoOperacion"
        Me.lblEstadoOperacion.Size = New System.Drawing.Size(45, 15)
        Me.lblEstadoOperacion.TabIndex = 8
        Me.lblEstadoOperacion.Text = "Estado:"
        '
        ' cboEstadoOperacion
        '
        Me.cboEstadoOperacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboEstadoOperacion.FormattingEnabled = True
        Me.cboEstadoOperacion.Location = New System.Drawing.Point(180, 98)
        Me.cboEstadoOperacion.Name = "cboEstadoOperacion"
        Me.cboEstadoOperacion.Size = New System.Drawing.Size(150, 23)
        Me.cboEstadoOperacion.TabIndex = 4
        '
        ' lblCasoOp
        '
        Me.lblCasoOp.AutoSize = True
        Me.lblCasoOp.Location = New System.Drawing.Point(345, 78)
        Me.lblCasoOp.Name = "lblCasoOp"
        Me.lblCasoOp.Size = New System.Drawing.Size(40, 15)
        Me.lblCasoOp.TabIndex = 10
        Me.lblCasoOp.Text = "Caso:"
        '
        ' cboCasoOp
        '
        Me.cboCasoOp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCasoOp.FormattingEnabled = True
        Me.cboCasoOp.Location = New System.Drawing.Point(345, 98)
        Me.cboCasoOp.Name = "cboCasoOp"
        Me.cboCasoOp.Size = New System.Drawing.Size(178, 23)
        Me.cboCasoOp.TabIndex = 5
        '
        ' lblInstrumentoOp
        '
        Me.lblInstrumentoOp.AutoSize = True
        Me.lblInstrumentoOp.Location = New System.Drawing.Point(15, 135)
        Me.lblInstrumentoOp.Name = "lblInstrumentoOp"
        Me.lblInstrumentoOp.Size = New System.Drawing.Size(75, 15)
        Me.lblInstrumentoOp.TabIndex = 12
        Me.lblInstrumentoOp.Text = "Instrumento:"
        '
        ' cboInstrumentoOp
        '
        Me.cboInstrumentoOp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboInstrumentoOp.FormattingEnabled = True
        Me.cboInstrumentoOp.Location = New System.Drawing.Point(15, 155)
        Me.cboInstrumentoOp.Name = "cboInstrumentoOp"
        Me.cboInstrumentoOp.Size = New System.Drawing.Size(508, 23)
        Me.cboInstrumentoOp.TabIndex = 6
        '
        ' lblDescripcionOp
        '
        Me.lblDescripcionOp.AutoSize = True
        Me.lblDescripcionOp.Location = New System.Drawing.Point(15, 192)
        Me.lblDescripcionOp.Name = "lblDescripcionOp"
        Me.lblDescripcionOp.Size = New System.Drawing.Size(72, 15)
        Me.lblDescripcionOp.TabIndex = 14
        Me.lblDescripcionOp.Text = "Descripción:"
        '
        ' txtDescripcionOp
        '
        Me.txtDescripcionOp.Location = New System.Drawing.Point(15, 212)
        Me.txtDescripcionOp.Multiline = True
        Me.txtDescripcionOp.Name = "txtDescripcionOp"
        Me.txtDescripcionOp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDescripcionOp.Size = New System.Drawing.Size(508, 55)
        Me.txtDescripcionOp.TabIndex = 7
        '
        ' lblAnalisis
        '
        Me.lblAnalisis.AutoSize = True
        Me.lblAnalisis.Location = New System.Drawing.Point(15, 280)
        Me.lblAnalisis.Name = "lblAnalisis"
        Me.lblAnalisis.Size = New System.Drawing.Size(90, 15)
        Me.lblAnalisis.TabIndex = 16
        Me.lblAnalisis.Text = "Vence análisis:"
        '
        ' txtAnalisis
        '
        Me.txtAnalisis.Location = New System.Drawing.Point(15, 300)
        Me.txtAnalisis.Name = "txtAnalisis"
        Me.txtAnalisis.ReadOnly = True
        Me.txtAnalisis.Size = New System.Drawing.Size(150, 23)
        Me.txtAnalisis.TabIndex = 8
        '
        ' lblReporte
        '
        Me.lblReporte.AutoSize = True
        Me.lblReporte.Location = New System.Drawing.Point(180, 280)
        Me.lblReporte.Name = "lblReporte"
        Me.lblReporte.Size = New System.Drawing.Size(90, 15)
        Me.lblReporte.TabIndex = 18
        Me.lblReporte.Text = "Vence reporte:"
        '
        ' txtReporte
        '
        Me.txtReporte.Location = New System.Drawing.Point(180, 300)
        Me.txtReporte.Name = "txtReporte"
        Me.txtReporte.ReadOnly = True
        Me.txtReporte.Size = New System.Drawing.Size(150, 23)
        Me.txtReporte.TabIndex = 9
        '
        ' lblAlertaUmbral
        '
        Me.lblAlertaUmbral.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblAlertaUmbral.ForeColor = System.Drawing.Color.Firebrick
        Me.lblAlertaUmbral.Location = New System.Drawing.Point(345, 280)
        Me.lblAlertaUmbral.Name = "lblAlertaUmbral"
        Me.lblAlertaUmbral.Size = New System.Drawing.Size(178, 50)
        Me.lblAlertaUmbral.TabIndex = 20
        '
        ' lblObservacionesOp
        '
        Me.lblObservacionesOp.AutoSize = True
        Me.lblObservacionesOp.Location = New System.Drawing.Point(15, 335)
        Me.lblObservacionesOp.Name = "lblObservacionesOp"
        Me.lblObservacionesOp.Size = New System.Drawing.Size(85, 15)
        Me.lblObservacionesOp.TabIndex = 21
        Me.lblObservacionesOp.Text = "Observaciones:"
        '
        ' txtObservacionesOp
        '
        Me.txtObservacionesOp.Location = New System.Drawing.Point(15, 355)
        Me.txtObservacionesOp.Multiline = True
        Me.txtObservacionesOp.Name = "txtObservacionesOp"
        Me.txtObservacionesOp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtObservacionesOp.Size = New System.Drawing.Size(508, 55)
        Me.txtObservacionesOp.TabIndex = 10
        '
        ' btnNuevoOp
        '
        Me.btnNuevoOp.Location = New System.Drawing.Point(15, 425)
        Me.btnNuevoOp.Name = "btnNuevoOp"
        Me.btnNuevoOp.Size = New System.Drawing.Size(100, 30)
        Me.btnNuevoOp.TabIndex = 11
        Me.btnNuevoOp.Text = "Nuevo"
        Me.btnNuevoOp.UseVisualStyleBackColor = True
        '
        ' btnGuardarOp
        '
        Me.btnGuardarOp.Location = New System.Drawing.Point(125, 425)
        Me.btnGuardarOp.Name = "btnGuardarOp"
        Me.btnGuardarOp.Size = New System.Drawing.Size(110, 30)
        Me.btnGuardarOp.TabIndex = 12
        Me.btnGuardarOp.Text = "Guardar"
        Me.btnGuardarOp.UseVisualStyleBackColor = True
        '
        ' btnReportarOp
        '
        Me.btnReportarOp.Location = New System.Drawing.Point(245, 425)
        Me.btnReportarOp.Name = "btnReportarOp"
        Me.btnReportarOp.Size = New System.Drawing.Size(140, 30)
        Me.btnReportarOp.TabIndex = 13
        Me.btnReportarOp.Text = "Reportar a la UIF"
        Me.btnReportarOp.UseVisualStyleBackColor = True
        '
        ' btnDescartarOp
        '
        Me.btnDescartarOp.Location = New System.Drawing.Point(395, 425)
        Me.btnDescartarOp.Name = "btnDescartarOp"
        Me.btnDescartarOp.Size = New System.Drawing.Size(128, 30)
        Me.btnDescartarOp.TabIndex = 14
        Me.btnDescartarOp.Text = "Descartar"
        Me.btnDescartarOp.UseVisualStyleBackColor = True
        '
        ' lblNotaUmbral
        '
        Me.lblNotaUmbral.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaUmbral.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaUmbral.Location = New System.Drawing.Point(15, 470)
        Me.lblNotaUmbral.Name = "lblNotaUmbral"
        Me.lblNotaUmbral.Size = New System.Drawing.Size(508, 80)
        Me.lblNotaUmbral.TabIndex = 25
        '
        ' tabArco
        '
        Me.tabArco.Controls.Add(Me.txtFiltroArco)
        Me.tabArco.Controls.Add(Me.btnBuscarArco)
        Me.tabArco.Controls.Add(Me.btnRefrescarArco)
        Me.tabArco.Controls.Add(Me.dgvArco)
        Me.tabArco.Controls.Add(Me.grpArco)
        Me.tabArco.Location = New System.Drawing.Point(4, 24)
        Me.tabArco.Name = "tabArco"
        Me.tabArco.Padding = New System.Windows.Forms.Padding(3)
        Me.tabArco.Size = New System.Drawing.Size(1208, 650)
        Me.tabArco.TabIndex = 1
        Me.tabArco.Text = "Derechos ARCO"
        Me.tabArco.UseVisualStyleBackColor = True
        '
        ' txtFiltroArco
        '
        Me.txtFiltroArco.Location = New System.Drawing.Point(10, 10)
        Me.txtFiltroArco.Name = "txtFiltroArco"
        Me.txtFiltroArco.Size = New System.Drawing.Size(300, 23)
        Me.txtFiltroArco.TabIndex = 0
        '
        ' btnBuscarArco
        '
        Me.btnBuscarArco.Location = New System.Drawing.Point(320, 8)
        Me.btnBuscarArco.Name = "btnBuscarArco"
        Me.btnBuscarArco.Size = New System.Drawing.Size(80, 27)
        Me.btnBuscarArco.TabIndex = 1
        Me.btnBuscarArco.Text = "Buscar"
        Me.btnBuscarArco.UseVisualStyleBackColor = True
        '
        ' btnRefrescarArco
        '
        Me.btnRefrescarArco.Location = New System.Drawing.Point(408, 8)
        Me.btnRefrescarArco.Name = "btnRefrescarArco"
        Me.btnRefrescarArco.Size = New System.Drawing.Size(90, 27)
        Me.btnRefrescarArco.TabIndex = 2
        Me.btnRefrescarArco.Text = "Refrescar"
        Me.btnRefrescarArco.UseVisualStyleBackColor = True
        '
        ' dgvArco
        '
        Me.dgvArco.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvArco.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvArco.Location = New System.Drawing.Point(10, 45)
        Me.dgvArco.Name = "dgvArco"
        Me.dgvArco.Size = New System.Drawing.Size(640, 595)
        Me.dgvArco.TabIndex = 3
        '
        ' grpArco
        '
        Me.grpArco.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpArco.Controls.Add(Me.lblClienteArco)
        Me.grpArco.Controls.Add(Me.cboClienteArco)
        Me.grpArco.Controls.Add(Me.lblTipoDerecho)
        Me.grpArco.Controls.Add(Me.cboTipoDerecho)
        Me.grpArco.Controls.Add(Me.lblFechaSolicitud)
        Me.grpArco.Controls.Add(Me.dtpFechaSolicitud)
        Me.grpArco.Controls.Add(Me.lblEstadoArco)
        Me.grpArco.Controls.Add(Me.cboEstadoArco)
        Me.grpArco.Controls.Add(Me.lblVencimientoArco)
        Me.grpArco.Controls.Add(Me.txtVencimientoArco)
        Me.grpArco.Controls.Add(Me.chkProrroga)
        Me.grpArco.Controls.Add(Me.lblVencimientoProrroga)
        Me.grpArco.Controls.Add(Me.txtVencimientoProrroga)
        Me.grpArco.Controls.Add(Me.lblDetalleArco)
        Me.grpArco.Controls.Add(Me.txtDetalleArco)
        Me.grpArco.Controls.Add(Me.lblFechaRespuesta)
        Me.grpArco.Controls.Add(Me.dtpFechaRespuesta)
        Me.grpArco.Controls.Add(Me.btnNuevoArco)
        Me.grpArco.Controls.Add(Me.btnGuardarArco)
        Me.grpArco.Controls.Add(Me.btnResponderArco)
        Me.grpArco.Controls.Add(Me.lblNotaArco)
        Me.grpArco.Location = New System.Drawing.Point(660, 10)
        Me.grpArco.Name = "grpArco"
        Me.grpArco.Size = New System.Drawing.Size(538, 620)
        Me.grpArco.TabIndex = 4
        Me.grpArco.TabStop = False
        Me.grpArco.Text = "Solicitud ARCO"
        '
        ' lblClienteArco
        '
        Me.lblClienteArco.AutoSize = True
        Me.lblClienteArco.Location = New System.Drawing.Point(15, 22)
        Me.lblClienteArco.Name = "lblClienteArco"
        Me.lblClienteArco.Size = New System.Drawing.Size(47, 15)
        Me.lblClienteArco.TabIndex = 0
        Me.lblClienteArco.Text = "Cliente:"
        '
        ' cboClienteArco
        '
        Me.cboClienteArco.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboClienteArco.FormattingEnabled = True
        Me.cboClienteArco.Location = New System.Drawing.Point(15, 42)
        Me.cboClienteArco.Name = "cboClienteArco"
        Me.cboClienteArco.Size = New System.Drawing.Size(508, 23)
        Me.cboClienteArco.TabIndex = 0
        '
        ' lblTipoDerecho
        '
        Me.lblTipoDerecho.AutoSize = True
        Me.lblTipoDerecho.Location = New System.Drawing.Point(15, 78)
        Me.lblTipoDerecho.Name = "lblTipoDerecho"
        Me.lblTipoDerecho.Size = New System.Drawing.Size(100, 15)
        Me.lblTipoDerecho.TabIndex = 2
        Me.lblTipoDerecho.Text = "Tipo de derecho:"
        '
        ' cboTipoDerecho
        '
        Me.cboTipoDerecho.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTipoDerecho.FormattingEnabled = True
        Me.cboTipoDerecho.Location = New System.Drawing.Point(15, 98)
        Me.cboTipoDerecho.Name = "cboTipoDerecho"
        Me.cboTipoDerecho.Size = New System.Drawing.Size(180, 23)
        Me.cboTipoDerecho.TabIndex = 1
        '
        ' lblFechaSolicitud
        '
        Me.lblFechaSolicitud.AutoSize = True
        Me.lblFechaSolicitud.Location = New System.Drawing.Point(210, 78)
        Me.lblFechaSolicitud.Name = "lblFechaSolicitud"
        Me.lblFechaSolicitud.Size = New System.Drawing.Size(90, 15)
        Me.lblFechaSolicitud.TabIndex = 4
        Me.lblFechaSolicitud.Text = "Fecha solicitud:"
        '
        ' dtpFechaSolicitud
        '
        Me.dtpFechaSolicitud.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaSolicitud.Location = New System.Drawing.Point(210, 98)
        Me.dtpFechaSolicitud.Name = "dtpFechaSolicitud"
        Me.dtpFechaSolicitud.Size = New System.Drawing.Size(150, 23)
        Me.dtpFechaSolicitud.TabIndex = 2
        '
        ' lblEstadoArco
        '
        Me.lblEstadoArco.AutoSize = True
        Me.lblEstadoArco.Location = New System.Drawing.Point(375, 78)
        Me.lblEstadoArco.Name = "lblEstadoArco"
        Me.lblEstadoArco.Size = New System.Drawing.Size(45, 15)
        Me.lblEstadoArco.TabIndex = 6
        Me.lblEstadoArco.Text = "Estado:"
        '
        ' cboEstadoArco
        '
        Me.cboEstadoArco.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboEstadoArco.FormattingEnabled = True
        Me.cboEstadoArco.Location = New System.Drawing.Point(375, 98)
        Me.cboEstadoArco.Name = "cboEstadoArco"
        Me.cboEstadoArco.Size = New System.Drawing.Size(148, 23)
        Me.cboEstadoArco.TabIndex = 3
        '
        ' lblVencimientoArco
        '
        Me.lblVencimientoArco.AutoSize = True
        Me.lblVencimientoArco.Location = New System.Drawing.Point(15, 135)
        Me.lblVencimientoArco.Name = "lblVencimientoArco"
        Me.lblVencimientoArco.Size = New System.Drawing.Size(110, 15)
        Me.lblVencimientoArco.TabIndex = 8
        Me.lblVencimientoArco.Text = "Vence respuesta:"
        '
        ' txtVencimientoArco
        '
        Me.txtVencimientoArco.Location = New System.Drawing.Point(15, 155)
        Me.txtVencimientoArco.Name = "txtVencimientoArco"
        Me.txtVencimientoArco.ReadOnly = True
        Me.txtVencimientoArco.Size = New System.Drawing.Size(150, 23)
        Me.txtVencimientoArco.TabIndex = 4
        '
        ' chkProrroga
        '
        Me.chkProrroga.AutoSize = True
        Me.chkProrroga.Location = New System.Drawing.Point(180, 157)
        Me.chkProrroga.Name = "chkProrroga"
        Me.chkProrroga.Size = New System.Drawing.Size(115, 19)
        Me.chkProrroga.TabIndex = 5
        Me.chkProrroga.Text = "Aplicar prórroga"
        Me.chkProrroga.UseVisualStyleBackColor = True
        '
        ' lblVencimientoProrroga
        '
        Me.lblVencimientoProrroga.AutoSize = True
        Me.lblVencimientoProrroga.Location = New System.Drawing.Point(305, 135)
        Me.lblVencimientoProrroga.Name = "lblVencimientoProrroga"
        Me.lblVencimientoProrroga.Size = New System.Drawing.Size(120, 15)
        Me.lblVencimientoProrroga.TabIndex = 11
        Me.lblVencimientoProrroga.Text = "Vence prórroga:"
        '
        ' txtVencimientoProrroga
        '
        Me.txtVencimientoProrroga.Location = New System.Drawing.Point(305, 155)
        Me.txtVencimientoProrroga.Name = "txtVencimientoProrroga"
        Me.txtVencimientoProrroga.ReadOnly = True
        Me.txtVencimientoProrroga.Size = New System.Drawing.Size(150, 23)
        Me.txtVencimientoProrroga.TabIndex = 6
        '
        ' lblDetalleArco
        '
        Me.lblDetalleArco.AutoSize = True
        Me.lblDetalleArco.Location = New System.Drawing.Point(15, 192)
        Me.lblDetalleArco.Name = "lblDetalleArco"
        Me.lblDetalleArco.Size = New System.Drawing.Size(100, 15)
        Me.lblDetalleArco.TabIndex = 13
        Me.lblDetalleArco.Text = "Detalle de la solicitud:"
        '
        ' txtDetalleArco
        '
        Me.txtDetalleArco.Location = New System.Drawing.Point(15, 212)
        Me.txtDetalleArco.Multiline = True
        Me.txtDetalleArco.Name = "txtDetalleArco"
        Me.txtDetalleArco.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDetalleArco.Size = New System.Drawing.Size(508, 85)
        Me.txtDetalleArco.TabIndex = 7
        '
        ' lblFechaRespuesta
        '
        Me.lblFechaRespuesta.AutoSize = True
        Me.lblFechaRespuesta.Location = New System.Drawing.Point(15, 310)
        Me.lblFechaRespuesta.Name = "lblFechaRespuesta"
        Me.lblFechaRespuesta.Size = New System.Drawing.Size(100, 15)
        Me.lblFechaRespuesta.TabIndex = 15
        Me.lblFechaRespuesta.Text = "Fecha de respuesta:"
        '
        ' dtpFechaRespuesta
        '
        Me.dtpFechaRespuesta.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaRespuesta.Location = New System.Drawing.Point(15, 330)
        Me.dtpFechaRespuesta.Name = "dtpFechaRespuesta"
        Me.dtpFechaRespuesta.ShowCheckBox = True
        Me.dtpFechaRespuesta.Size = New System.Drawing.Size(150, 23)
        Me.dtpFechaRespuesta.TabIndex = 8
        '
        ' btnNuevoArco
        '
        Me.btnNuevoArco.Location = New System.Drawing.Point(15, 375)
        Me.btnNuevoArco.Name = "btnNuevoArco"
        Me.btnNuevoArco.Size = New System.Drawing.Size(100, 30)
        Me.btnNuevoArco.TabIndex = 9
        Me.btnNuevoArco.Text = "Nuevo"
        Me.btnNuevoArco.UseVisualStyleBackColor = True
        '
        ' btnGuardarArco
        '
        Me.btnGuardarArco.Location = New System.Drawing.Point(125, 375)
        Me.btnGuardarArco.Name = "btnGuardarArco"
        Me.btnGuardarArco.Size = New System.Drawing.Size(110, 30)
        Me.btnGuardarArco.TabIndex = 10
        Me.btnGuardarArco.Text = "Guardar"
        Me.btnGuardarArco.UseVisualStyleBackColor = True
        '
        ' btnResponderArco
        '
        Me.btnResponderArco.Location = New System.Drawing.Point(245, 375)
        Me.btnResponderArco.Name = "btnResponderArco"
        Me.btnResponderArco.Size = New System.Drawing.Size(160, 30)
        Me.btnResponderArco.TabIndex = 11
        Me.btnResponderArco.Text = "Marcar respondida"
        Me.btnResponderArco.UseVisualStyleBackColor = True
        '
        ' lblNotaArco
        '
        Me.lblNotaArco.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaArco.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaArco.Location = New System.Drawing.Point(15, 420)
        Me.lblNotaArco.Name = "lblNotaArco"
        Me.lblNotaArco.Size = New System.Drawing.Size(508, 80)
        Me.lblNotaArco.TabIndex = 22
        Me.lblNotaArco.Text = "Los plazos ARCO son configurables (por defecto 10 días hábiles y prórroga de 5). Confirme los criterios con el oficial de cumplimiento."
        '
        ' FrmCumplimiento
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1240, 740)
        Me.Controls.Add(Me.tabPrincipal)
        Me.Controls.Add(Me.lblMensaje)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1180, 700)
        Me.Name = "FrmCumplimiento"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Cumplimiento LDA/FT y derechos ARCO"
        Me.tabPrincipal.ResumeLayout(False)
        Me.tabOperaciones.ResumeLayout(False)
        CType(Me.dgvOperaciones, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpOperacion.ResumeLayout(False)
        Me.grpOperacion.PerformLayout()
        CType(Me.nudMonto, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabArco.ResumeLayout(False)
        CType(Me.dgvArco, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpArco.ResumeLayout(False)
        Me.grpArco.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents tabPrincipal As System.Windows.Forms.TabControl
    Friend WithEvents tabOperaciones As System.Windows.Forms.TabPage
    Friend WithEvents txtFiltroOp As System.Windows.Forms.TextBox
    Friend WithEvents btnBuscarOp As System.Windows.Forms.Button
    Friend WithEvents btnRefrescarOp As System.Windows.Forms.Button
    Friend WithEvents dgvOperaciones As System.Windows.Forms.DataGridView
    Friend WithEvents grpOperacion As System.Windows.Forms.GroupBox
    Friend WithEvents lblTipoOperacion As System.Windows.Forms.Label
    Friend WithEvents cboTipoOperacion As System.Windows.Forms.ComboBox
    Friend WithEvents lblMonto As System.Windows.Forms.Label
    Friend WithEvents nudMonto As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblMoneda As System.Windows.Forms.Label
    Friend WithEvents cboMoneda As System.Windows.Forms.ComboBox
    Friend WithEvents lblFechaOperacion As System.Windows.Forms.Label
    Friend WithEvents dtpFechaOperacion As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblEstadoOperacion As System.Windows.Forms.Label
    Friend WithEvents cboEstadoOperacion As System.Windows.Forms.ComboBox
    Friend WithEvents lblCasoOp As System.Windows.Forms.Label
    Friend WithEvents cboCasoOp As System.Windows.Forms.ComboBox
    Friend WithEvents lblInstrumentoOp As System.Windows.Forms.Label
    Friend WithEvents cboInstrumentoOp As System.Windows.Forms.ComboBox
    Friend WithEvents lblDescripcionOp As System.Windows.Forms.Label
    Friend WithEvents txtDescripcionOp As System.Windows.Forms.TextBox
    Friend WithEvents lblAnalisis As System.Windows.Forms.Label
    Friend WithEvents txtAnalisis As System.Windows.Forms.TextBox
    Friend WithEvents lblReporte As System.Windows.Forms.Label
    Friend WithEvents txtReporte As System.Windows.Forms.TextBox
    Friend WithEvents lblAlertaUmbral As System.Windows.Forms.Label
    Friend WithEvents lblObservacionesOp As System.Windows.Forms.Label
    Friend WithEvents txtObservacionesOp As System.Windows.Forms.TextBox
    Friend WithEvents btnNuevoOp As System.Windows.Forms.Button
    Friend WithEvents btnGuardarOp As System.Windows.Forms.Button
    Friend WithEvents btnReportarOp As System.Windows.Forms.Button
    Friend WithEvents btnDescartarOp As System.Windows.Forms.Button
    Friend WithEvents lblNotaUmbral As System.Windows.Forms.Label
    Friend WithEvents tabArco As System.Windows.Forms.TabPage
    Friend WithEvents txtFiltroArco As System.Windows.Forms.TextBox
    Friend WithEvents btnBuscarArco As System.Windows.Forms.Button
    Friend WithEvents btnRefrescarArco As System.Windows.Forms.Button
    Friend WithEvents dgvArco As System.Windows.Forms.DataGridView
    Friend WithEvents grpArco As System.Windows.Forms.GroupBox
    Friend WithEvents lblClienteArco As System.Windows.Forms.Label
    Friend WithEvents cboClienteArco As System.Windows.Forms.ComboBox
    Friend WithEvents lblTipoDerecho As System.Windows.Forms.Label
    Friend WithEvents cboTipoDerecho As System.Windows.Forms.ComboBox
    Friend WithEvents lblFechaSolicitud As System.Windows.Forms.Label
    Friend WithEvents dtpFechaSolicitud As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblEstadoArco As System.Windows.Forms.Label
    Friend WithEvents cboEstadoArco As System.Windows.Forms.ComboBox
    Friend WithEvents lblVencimientoArco As System.Windows.Forms.Label
    Friend WithEvents txtVencimientoArco As System.Windows.Forms.TextBox
    Friend WithEvents chkProrroga As System.Windows.Forms.CheckBox
    Friend WithEvents lblVencimientoProrroga As System.Windows.Forms.Label
    Friend WithEvents txtVencimientoProrroga As System.Windows.Forms.TextBox
    Friend WithEvents lblDetalleArco As System.Windows.Forms.Label
    Friend WithEvents txtDetalleArco As System.Windows.Forms.TextBox
    Friend WithEvents lblFechaRespuesta As System.Windows.Forms.Label
    Friend WithEvents dtpFechaRespuesta As System.Windows.Forms.DateTimePicker
    Friend WithEvents btnNuevoArco As System.Windows.Forms.Button
    Friend WithEvents btnGuardarArco As System.Windows.Forms.Button
    Friend WithEvents btnResponderArco As System.Windows.Forms.Button
    Friend WithEvents lblNotaArco As System.Windows.Forms.Label
End Class
