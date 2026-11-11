<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmProtocolo
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
        Me.lblSelLibro = New System.Windows.Forms.Label()
        Me.cboLibro = New System.Windows.Forms.ComboBox()
        Me.btnRefrescar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.tabPrincipal = New System.Windows.Forms.TabControl()
        Me.tabLibros = New System.Windows.Forms.TabPage()
        Me.dgvLibros = New System.Windows.Forms.DataGridView()
        Me.grpLibro = New System.Windows.Forms.GroupBox()
        Me.lblNumeroLibro = New System.Windows.Forms.Label()
        Me.nudNumeroLibro = New System.Windows.Forms.NumericUpDown()
        Me.lblAnio = New System.Windows.Forms.Label()
        Me.nudAnio = New System.Windows.Forms.NumericUpDown()
        Me.lblFechaAutorizacion = New System.Windows.Forms.Label()
        Me.dtpFechaAutorizacion = New System.Windows.Forms.DateTimePicker()
        Me.lblFechaVencimiento = New System.Windows.Forms.Label()
        Me.dtpFechaVencimiento = New System.Windows.Forms.DateTimePicker()
        Me.lblEstadoLibro = New System.Windows.Forms.Label()
        Me.cboEstadoLibro = New System.Windows.Forms.ComboBox()
        Me.lblFechaCierre = New System.Windows.Forms.Label()
        Me.txtFechaCierre = New System.Windows.Forms.TextBox()
        Me.lblFechaEntrega = New System.Windows.Forms.Label()
        Me.dtpFechaEntrega = New System.Windows.Forms.DateTimePicker()
        Me.lblRazonCierre = New System.Windows.Forms.Label()
        Me.txtRazonCierre = New System.Windows.Forms.TextBox()
        Me.lblLimiteEntrega = New System.Windows.Forms.Label()
        Me.btnNuevoLibro = New System.Windows.Forms.Button()
        Me.btnGuardarLibro = New System.Windows.Forms.Button()
        Me.btnCerrarLibro = New System.Windows.Forms.Button()
        Me.btnEntregarLibro = New System.Windows.Forms.Button()
        Me.tabInstrumentos = New System.Windows.Forms.TabPage()
        Me.dgvInstrumentos = New System.Windows.Forms.DataGridView()
        Me.grpInstrumento = New System.Windows.Forms.GroupBox()
        Me.lblNumeroEscritura = New System.Windows.Forms.Label()
        Me.nudNumeroEscritura = New System.Windows.Forms.NumericUpDown()
        Me.lblFechaHora = New System.Windows.Forms.Label()
        Me.dtpFechaHoraOtorgamiento = New System.Windows.Forms.DateTimePicker()
        Me.lblFolioInicio = New System.Windows.Forms.Label()
        Me.nudFolioInicio = New System.Windows.Forms.NumericUpDown()
        Me.lblFolioFin = New System.Windows.Forms.Label()
        Me.nudFolioFin = New System.Windows.Forms.NumericUpDown()
        Me.lblObjeto = New System.Windows.Forms.Label()
        Me.txtObjeto = New System.Windows.Forms.TextBox()
        Me.lblEstadoInstrumento = New System.Windows.Forms.Label()
        Me.cboEstadoInstrumento = New System.Windows.Forms.ComboBox()
        Me.btnNuevoInstrumento = New System.Windows.Forms.Button()
        Me.btnGuardarInstrumento = New System.Windows.Forms.Button()
        Me.btnAnularInstrumento = New System.Windows.Forms.Button()
        Me.grpComparecientes = New System.Windows.Forms.GroupBox()
        Me.lblClienteComp = New System.Windows.Forms.Label()
        Me.cboClienteComp = New System.Windows.Forms.ComboBox()
        Me.lblCalidad = New System.Windows.Forms.Label()
        Me.cboCalidad = New System.Windows.Forms.ComboBox()
        Me.btnAgregarComp = New System.Windows.Forms.Button()
        Me.dgvComparecientes = New System.Windows.Forms.DataGridView()
        Me.btnQuitarComp = New System.Windows.Forms.Button()
        Me.tabTestimonios = New System.Windows.Forms.TabPage()
        Me.dgvTestimonios = New System.Windows.Forms.DataGridView()
        Me.grpTestimonio = New System.Windows.Forms.GroupBox()
        Me.lblInstrumentoTest = New System.Windows.Forms.Label()
        Me.cboInstrumentoTest = New System.Windows.Forms.ComboBox()
        Me.lblTipoTestimonio = New System.Windows.Forms.Label()
        Me.cboTipoTestimonio = New System.Windows.Forms.ComboBox()
        Me.lblFechaTestimonio = New System.Windows.Forms.Label()
        Me.dtpFechaTestimonio = New System.Windows.Forms.DateTimePicker()
        Me.lblDestinatario = New System.Windows.Forms.Label()
        Me.txtDestinatario = New System.Windows.Forms.TextBox()
        Me.btnExpedirTestimonio = New System.Windows.Forms.Button()
        Me.tabIndice = New System.Windows.Forms.TabPage()
        Me.dgvIndice = New System.Windows.Forms.DataGridView()
        Me.btnGenerarIndice = New System.Windows.Forms.Button()
        Me.btnExportarIndice = New System.Windows.Forms.Button()
        Me.lblIndiceInfo = New System.Windows.Forms.Label()
        Me.tabPrincipal.SuspendLayout()
        Me.tabLibros.SuspendLayout()
        CType(Me.dgvLibros, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpLibro.SuspendLayout()
        CType(Me.nudNumeroLibro, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAnio, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabInstrumentos.SuspendLayout()
        CType(Me.dgvInstrumentos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpInstrumento.SuspendLayout()
        CType(Me.nudNumeroEscritura, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudFolioInicio, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudFolioFin, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpComparecientes.SuspendLayout()
        CType(Me.dgvComparecientes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabTestimonios.SuspendLayout()
        CType(Me.dgvTestimonios, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpTestimonio.SuspendLayout()
        Me.tabIndice.SuspendLayout()
        CType(Me.dgvIndice, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        ' lblSelLibro
        '
        Me.lblSelLibro.AutoSize = True
        Me.lblSelLibro.Location = New System.Drawing.Point(12, 18)
        Me.lblSelLibro.Name = "lblSelLibro"
        Me.lblSelLibro.Size = New System.Drawing.Size(40, 15)
        Me.lblSelLibro.TabIndex = 0
        Me.lblSelLibro.Text = "Libro:"
        '
        ' cboLibro
        '
        Me.cboLibro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboLibro.FormattingEnabled = True
        Me.cboLibro.Location = New System.Drawing.Point(60, 15)
        Me.cboLibro.Name = "cboLibro"
        Me.cboLibro.Size = New System.Drawing.Size(450, 23)
        Me.cboLibro.TabIndex = 1
        '
        ' btnRefrescar
        '
        Me.btnRefrescar.Location = New System.Drawing.Point(520, 13)
        Me.btnRefrescar.Name = "btnRefrescar"
        Me.btnRefrescar.Size = New System.Drawing.Size(90, 27)
        Me.btnRefrescar.TabIndex = 2
        Me.btnRefrescar.Text = "Refrescar"
        Me.btnRefrescar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(620, 10)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(600, 40)
        Me.lblMensaje.TabIndex = 3
        '
        ' tabPrincipal
        '
        Me.tabPrincipal.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabPrincipal.Controls.Add(Me.tabLibros)
        Me.tabPrincipal.Controls.Add(Me.tabInstrumentos)
        Me.tabPrincipal.Controls.Add(Me.tabTestimonios)
        Me.tabPrincipal.Controls.Add(Me.tabIndice)
        Me.tabPrincipal.Location = New System.Drawing.Point(12, 52)
        Me.tabPrincipal.Name = "tabPrincipal"
        Me.tabPrincipal.SelectedIndex = 0
        Me.tabPrincipal.Size = New System.Drawing.Size(1200, 660)
        Me.tabPrincipal.TabIndex = 4
        '
        ' tabLibros
        '
        Me.tabLibros.Controls.Add(Me.dgvLibros)
        Me.tabLibros.Controls.Add(Me.grpLibro)
        Me.tabLibros.Location = New System.Drawing.Point(4, 24)
        Me.tabLibros.Name = "tabLibros"
        Me.tabLibros.Padding = New System.Windows.Forms.Padding(3)
        Me.tabLibros.Size = New System.Drawing.Size(1192, 632)
        Me.tabLibros.TabIndex = 0
        Me.tabLibros.Text = "Libros"
        Me.tabLibros.UseVisualStyleBackColor = True
        '
        ' dgvLibros
        '
        Me.dgvLibros.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvLibros.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvLibros.Location = New System.Drawing.Point(10, 10)
        Me.dgvLibros.Name = "dgvLibros"
        Me.dgvLibros.Size = New System.Drawing.Size(560, 605)
        Me.dgvLibros.TabIndex = 0
        '
        ' grpLibro
        '
        Me.grpLibro.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpLibro.Controls.Add(Me.lblNumeroLibro)
        Me.grpLibro.Controls.Add(Me.nudNumeroLibro)
        Me.grpLibro.Controls.Add(Me.lblAnio)
        Me.grpLibro.Controls.Add(Me.nudAnio)
        Me.grpLibro.Controls.Add(Me.lblFechaAutorizacion)
        Me.grpLibro.Controls.Add(Me.dtpFechaAutorizacion)
        Me.grpLibro.Controls.Add(Me.lblFechaVencimiento)
        Me.grpLibro.Controls.Add(Me.dtpFechaVencimiento)
        Me.grpLibro.Controls.Add(Me.lblEstadoLibro)
        Me.grpLibro.Controls.Add(Me.cboEstadoLibro)
        Me.grpLibro.Controls.Add(Me.lblFechaCierre)
        Me.grpLibro.Controls.Add(Me.txtFechaCierre)
        Me.grpLibro.Controls.Add(Me.lblFechaEntrega)
        Me.grpLibro.Controls.Add(Me.dtpFechaEntrega)
        Me.grpLibro.Controls.Add(Me.lblRazonCierre)
        Me.grpLibro.Controls.Add(Me.txtRazonCierre)
        Me.grpLibro.Controls.Add(Me.lblLimiteEntrega)
        Me.grpLibro.Controls.Add(Me.btnNuevoLibro)
        Me.grpLibro.Controls.Add(Me.btnGuardarLibro)
        Me.grpLibro.Controls.Add(Me.btnCerrarLibro)
        Me.grpLibro.Controls.Add(Me.btnEntregarLibro)
        Me.grpLibro.Location = New System.Drawing.Point(580, 10)
        Me.grpLibro.Name = "grpLibro"
        Me.grpLibro.Size = New System.Drawing.Size(595, 330)
        Me.grpLibro.TabIndex = 1
        Me.grpLibro.TabStop = False
        Me.grpLibro.Text = "Datos del libro de protocolo"
        '
        ' lblNumeroLibro
        '
        Me.lblNumeroLibro.AutoSize = True
        Me.lblNumeroLibro.Location = New System.Drawing.Point(15, 22)
        Me.lblNumeroLibro.Name = "lblNumeroLibro"
        Me.lblNumeroLibro.Size = New System.Drawing.Size(55, 15)
        Me.lblNumeroLibro.TabIndex = 0
        Me.lblNumeroLibro.Text = "N.º libro:"
        '
        ' nudNumeroLibro
        '
        Me.nudNumeroLibro.Location = New System.Drawing.Point(15, 42)
        Me.nudNumeroLibro.Maximum = New Decimal(New Integer() {9999, 0, 0, 0})
        Me.nudNumeroLibro.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudNumeroLibro.Name = "nudNumeroLibro"
        Me.nudNumeroLibro.Size = New System.Drawing.Size(100, 23)
        Me.nudNumeroLibro.TabIndex = 0
        Me.nudNumeroLibro.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        ' lblAnio
        '
        Me.lblAnio.AutoSize = True
        Me.lblAnio.Location = New System.Drawing.Point(130, 22)
        Me.lblAnio.Name = "lblAnio"
        Me.lblAnio.Size = New System.Drawing.Size(32, 15)
        Me.lblAnio.TabIndex = 2
        Me.lblAnio.Text = "Año:"
        '
        ' nudAnio
        '
        Me.nudAnio.Location = New System.Drawing.Point(130, 42)
        Me.nudAnio.Maximum = New Decimal(New Integer() {2100, 0, 0, 0})
        Me.nudAnio.Minimum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.nudAnio.Name = "nudAnio"
        Me.nudAnio.Size = New System.Drawing.Size(100, 23)
        Me.nudAnio.TabIndex = 1
        Me.nudAnio.Value = New Decimal(New Integer() {2026, 0, 0, 0})
        '
        ' lblFechaAutorizacion
        '
        Me.lblFechaAutorizacion.AutoSize = True
        Me.lblFechaAutorizacion.Location = New System.Drawing.Point(245, 22)
        Me.lblFechaAutorizacion.Name = "lblFechaAutorizacion"
        Me.lblFechaAutorizacion.Size = New System.Drawing.Size(100, 15)
        Me.lblFechaAutorizacion.TabIndex = 4
        Me.lblFechaAutorizacion.Text = "Autorización:"
        '
        ' dtpFechaAutorizacion
        '
        Me.dtpFechaAutorizacion.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaAutorizacion.Location = New System.Drawing.Point(245, 42)
        Me.dtpFechaAutorizacion.Name = "dtpFechaAutorizacion"
        Me.dtpFechaAutorizacion.Size = New System.Drawing.Size(130, 23)
        Me.dtpFechaAutorizacion.TabIndex = 2
        '
        ' lblFechaVencimiento
        '
        Me.lblFechaVencimiento.AutoSize = True
        Me.lblFechaVencimiento.Location = New System.Drawing.Point(390, 22)
        Me.lblFechaVencimiento.Name = "lblFechaVencimiento"
        Me.lblFechaVencimiento.Size = New System.Drawing.Size(90, 15)
        Me.lblFechaVencimiento.TabIndex = 6
        Me.lblFechaVencimiento.Text = "Vencimiento:"
        '
        ' dtpFechaVencimiento
        '
        Me.dtpFechaVencimiento.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaVencimiento.Location = New System.Drawing.Point(390, 42)
        Me.dtpFechaVencimiento.Name = "dtpFechaVencimiento"
        Me.dtpFechaVencimiento.Size = New System.Drawing.Size(130, 23)
        Me.dtpFechaVencimiento.TabIndex = 3
        '
        ' lblEstadoLibro
        '
        Me.lblEstadoLibro.AutoSize = True
        Me.lblEstadoLibro.Location = New System.Drawing.Point(15, 80)
        Me.lblEstadoLibro.Name = "lblEstadoLibro"
        Me.lblEstadoLibro.Size = New System.Drawing.Size(45, 15)
        Me.lblEstadoLibro.TabIndex = 8
        Me.lblEstadoLibro.Text = "Estado:"
        '
        ' cboEstadoLibro
        '
        Me.cboEstadoLibro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboEstadoLibro.FormattingEnabled = True
        Me.cboEstadoLibro.Location = New System.Drawing.Point(15, 100)
        Me.cboEstadoLibro.Name = "cboEstadoLibro"
        Me.cboEstadoLibro.Size = New System.Drawing.Size(150, 23)
        Me.cboEstadoLibro.TabIndex = 4
        '
        ' lblFechaCierre
        '
        Me.lblFechaCierre.AutoSize = True
        Me.lblFechaCierre.Location = New System.Drawing.Point(180, 80)
        Me.lblFechaCierre.Name = "lblFechaCierre"
        Me.lblFechaCierre.Size = New System.Drawing.Size(75, 15)
        Me.lblFechaCierre.TabIndex = 10
        Me.lblFechaCierre.Text = "Fecha cierre:"
        '
        ' txtFechaCierre
        '
        Me.txtFechaCierre.Location = New System.Drawing.Point(180, 100)
        Me.txtFechaCierre.Name = "txtFechaCierre"
        Me.txtFechaCierre.ReadOnly = True
        Me.txtFechaCierre.Size = New System.Drawing.Size(150, 23)
        Me.txtFechaCierre.TabIndex = 5
        '
        ' lblFechaEntrega
        '
        Me.lblFechaEntrega.AutoSize = True
        Me.lblFechaEntrega.Location = New System.Drawing.Point(345, 80)
        Me.lblFechaEntrega.Name = "lblFechaEntrega"
        Me.lblFechaEntrega.Size = New System.Drawing.Size(90, 15)
        Me.lblFechaEntrega.TabIndex = 12
        Me.lblFechaEntrega.Text = "Fecha entrega:"
        '
        ' dtpFechaEntrega
        '
        Me.dtpFechaEntrega.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaEntrega.Location = New System.Drawing.Point(345, 100)
        Me.dtpFechaEntrega.Name = "dtpFechaEntrega"
        Me.dtpFechaEntrega.Size = New System.Drawing.Size(150, 23)
        Me.dtpFechaEntrega.TabIndex = 6
        '
        ' lblRazonCierre
        '
        Me.lblRazonCierre.AutoSize = True
        Me.lblRazonCierre.Location = New System.Drawing.Point(15, 135)
        Me.lblRazonCierre.Name = "lblRazonCierre"
        Me.lblRazonCierre.Size = New System.Drawing.Size(90, 15)
        Me.lblRazonCierre.TabIndex = 14
        Me.lblRazonCierre.Text = "Razón del cierre:"
        '
        ' txtRazonCierre
        '
        Me.txtRazonCierre.Location = New System.Drawing.Point(15, 155)
        Me.txtRazonCierre.Multiline = True
        Me.txtRazonCierre.Name = "txtRazonCierre"
        Me.txtRazonCierre.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtRazonCierre.Size = New System.Drawing.Size(555, 55)
        Me.txtRazonCierre.TabIndex = 7
        '
        ' lblLimiteEntrega
        '
        Me.lblLimiteEntrega.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblLimiteEntrega.ForeColor = System.Drawing.Color.DarkOrange
        Me.lblLimiteEntrega.Location = New System.Drawing.Point(15, 218)
        Me.lblLimiteEntrega.Name = "lblLimiteEntrega"
        Me.lblLimiteEntrega.Size = New System.Drawing.Size(555, 45)
        Me.lblLimiteEntrega.TabIndex = 16
        '
        ' btnNuevoLibro
        '
        Me.btnNuevoLibro.Location = New System.Drawing.Point(15, 272)
        Me.btnNuevoLibro.Name = "btnNuevoLibro"
        Me.btnNuevoLibro.Size = New System.Drawing.Size(100, 30)
        Me.btnNuevoLibro.TabIndex = 8
        Me.btnNuevoLibro.Text = "Nuevo"
        Me.btnNuevoLibro.UseVisualStyleBackColor = True
        '
        ' btnGuardarLibro
        '
        Me.btnGuardarLibro.Location = New System.Drawing.Point(125, 272)
        Me.btnGuardarLibro.Name = "btnGuardarLibro"
        Me.btnGuardarLibro.Size = New System.Drawing.Size(110, 30)
        Me.btnGuardarLibro.TabIndex = 9
        Me.btnGuardarLibro.Text = "Guardar"
        Me.btnGuardarLibro.UseVisualStyleBackColor = True
        '
        ' btnCerrarLibro
        '
        Me.btnCerrarLibro.Location = New System.Drawing.Point(245, 272)
        Me.btnCerrarLibro.Name = "btnCerrarLibro"
        Me.btnCerrarLibro.Size = New System.Drawing.Size(130, 30)
        Me.btnCerrarLibro.TabIndex = 10
        Me.btnCerrarLibro.Text = "Cerrar libro"
        Me.btnCerrarLibro.UseVisualStyleBackColor = True
        '
        ' btnEntregarLibro
        '
        Me.btnEntregarLibro.Location = New System.Drawing.Point(385, 272)
        Me.btnEntregarLibro.Name = "btnEntregarLibro"
        Me.btnEntregarLibro.Size = New System.Drawing.Size(185, 30)
        Me.btnEntregarLibro.TabIndex = 11
        Me.btnEntregarLibro.Text = "Registrar entrega al Notariado"
        Me.btnEntregarLibro.UseVisualStyleBackColor = True
        '
        ' tabInstrumentos
        '
        Me.tabInstrumentos.Controls.Add(Me.dgvInstrumentos)
        Me.tabInstrumentos.Controls.Add(Me.grpInstrumento)
        Me.tabInstrumentos.Controls.Add(Me.grpComparecientes)
        Me.tabInstrumentos.Location = New System.Drawing.Point(4, 24)
        Me.tabInstrumentos.Name = "tabInstrumentos"
        Me.tabInstrumentos.Padding = New System.Windows.Forms.Padding(3)
        Me.tabInstrumentos.Size = New System.Drawing.Size(1192, 632)
        Me.tabInstrumentos.TabIndex = 1
        Me.tabInstrumentos.Text = "Instrumentos y comparecientes"
        Me.tabInstrumentos.UseVisualStyleBackColor = True
        '
        ' dgvInstrumentos
        '
        Me.dgvInstrumentos.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvInstrumentos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvInstrumentos.Location = New System.Drawing.Point(10, 10)
        Me.dgvInstrumentos.Name = "dgvInstrumentos"
        Me.dgvInstrumentos.Size = New System.Drawing.Size(560, 605)
        Me.dgvInstrumentos.TabIndex = 0
        '
        ' grpInstrumento
        '
        Me.grpInstrumento.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpInstrumento.Controls.Add(Me.lblNumeroEscritura)
        Me.grpInstrumento.Controls.Add(Me.nudNumeroEscritura)
        Me.grpInstrumento.Controls.Add(Me.lblFechaHora)
        Me.grpInstrumento.Controls.Add(Me.dtpFechaHoraOtorgamiento)
        Me.grpInstrumento.Controls.Add(Me.lblFolioInicio)
        Me.grpInstrumento.Controls.Add(Me.nudFolioInicio)
        Me.grpInstrumento.Controls.Add(Me.lblFolioFin)
        Me.grpInstrumento.Controls.Add(Me.nudFolioFin)
        Me.grpInstrumento.Controls.Add(Me.lblObjeto)
        Me.grpInstrumento.Controls.Add(Me.txtObjeto)
        Me.grpInstrumento.Controls.Add(Me.lblEstadoInstrumento)
        Me.grpInstrumento.Controls.Add(Me.cboEstadoInstrumento)
        Me.grpInstrumento.Controls.Add(Me.btnNuevoInstrumento)
        Me.grpInstrumento.Controls.Add(Me.btnGuardarInstrumento)
        Me.grpInstrumento.Controls.Add(Me.btnAnularInstrumento)
        Me.grpInstrumento.Location = New System.Drawing.Point(580, 10)
        Me.grpInstrumento.Name = "grpInstrumento"
        Me.grpInstrumento.Size = New System.Drawing.Size(595, 290)
        Me.grpInstrumento.TabIndex = 1
        Me.grpInstrumento.TabStop = False
        Me.grpInstrumento.Text = "Datos de la escritura"
        '
        ' lblNumeroEscritura
        '
        Me.lblNumeroEscritura.AutoSize = True
        Me.lblNumeroEscritura.Location = New System.Drawing.Point(15, 22)
        Me.lblNumeroEscritura.Name = "lblNumeroEscritura"
        Me.lblNumeroEscritura.Size = New System.Drawing.Size(60, 15)
        Me.lblNumeroEscritura.TabIndex = 0
        Me.lblNumeroEscritura.Text = "Escritura:"
        '
        ' nudNumeroEscritura
        '
        Me.nudNumeroEscritura.Location = New System.Drawing.Point(15, 42)
        Me.nudNumeroEscritura.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.nudNumeroEscritura.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudNumeroEscritura.Name = "nudNumeroEscritura"
        Me.nudNumeroEscritura.Size = New System.Drawing.Size(100, 23)
        Me.nudNumeroEscritura.TabIndex = 0
        Me.nudNumeroEscritura.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        ' lblFechaHora
        '
        Me.lblFechaHora.AutoSize = True
        Me.lblFechaHora.Location = New System.Drawing.Point(130, 22)
        Me.lblFechaHora.Name = "lblFechaHora"
        Me.lblFechaHora.Size = New System.Drawing.Size(110, 15)
        Me.lblFechaHora.TabIndex = 2
        Me.lblFechaHora.Text = "Fecha y hora:"
        '
        ' dtpFechaHoraOtorgamiento
        '
        Me.dtpFechaHoraOtorgamiento.CustomFormat = "dd/MM/yyyy HH:mm"
        Me.dtpFechaHoraOtorgamiento.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpFechaHoraOtorgamiento.Location = New System.Drawing.Point(130, 42)
        Me.dtpFechaHoraOtorgamiento.Name = "dtpFechaHoraOtorgamiento"
        Me.dtpFechaHoraOtorgamiento.ShowUpDown = True
        Me.dtpFechaHoraOtorgamiento.Size = New System.Drawing.Size(200, 23)
        Me.dtpFechaHoraOtorgamiento.TabIndex = 1
        '
        ' lblFolioInicio
        '
        Me.lblFolioInicio.AutoSize = True
        Me.lblFolioInicio.Location = New System.Drawing.Point(345, 22)
        Me.lblFolioInicio.Name = "lblFolioInicio"
        Me.lblFolioInicio.Size = New System.Drawing.Size(65, 15)
        Me.lblFolioInicio.TabIndex = 4
        Me.lblFolioInicio.Text = "Folio inicio:"
        '
        ' nudFolioInicio
        '
        Me.nudFolioInicio.Location = New System.Drawing.Point(345, 42)
        Me.nudFolioInicio.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.nudFolioInicio.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudFolioInicio.Name = "nudFolioInicio"
        Me.nudFolioInicio.Size = New System.Drawing.Size(90, 23)
        Me.nudFolioInicio.TabIndex = 2
        Me.nudFolioInicio.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        ' lblFolioFin
        '
        Me.lblFolioFin.AutoSize = True
        Me.lblFolioFin.Location = New System.Drawing.Point(450, 22)
        Me.lblFolioFin.Name = "lblFolioFin"
        Me.lblFolioFin.Size = New System.Drawing.Size(50, 15)
        Me.lblFolioFin.TabIndex = 6
        Me.lblFolioFin.Text = "Folio fin:"
        '
        ' nudFolioFin
        '
        Me.nudFolioFin.Location = New System.Drawing.Point(450, 42)
        Me.nudFolioFin.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.nudFolioFin.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudFolioFin.Name = "nudFolioFin"
        Me.nudFolioFin.Size = New System.Drawing.Size(90, 23)
        Me.nudFolioFin.TabIndex = 3
        Me.nudFolioFin.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        ' lblObjeto
        '
        Me.lblObjeto.AutoSize = True
        Me.lblObjeto.Location = New System.Drawing.Point(15, 78)
        Me.lblObjeto.Name = "lblObjeto"
        Me.lblObjeto.Size = New System.Drawing.Size(100, 15)
        Me.lblObjeto.TabIndex = 8
        Me.lblObjeto.Text = "Objeto del acto:"
        '
        ' txtObjeto
        '
        Me.txtObjeto.Location = New System.Drawing.Point(15, 98)
        Me.txtObjeto.Multiline = True
        Me.txtObjeto.Name = "txtObjeto"
        Me.txtObjeto.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtObjeto.Size = New System.Drawing.Size(555, 55)
        Me.txtObjeto.TabIndex = 4
        '
        ' lblEstadoInstrumento
        '
        Me.lblEstadoInstrumento.AutoSize = True
        Me.lblEstadoInstrumento.Location = New System.Drawing.Point(15, 165)
        Me.lblEstadoInstrumento.Name = "lblEstadoInstrumento"
        Me.lblEstadoInstrumento.Size = New System.Drawing.Size(45, 15)
        Me.lblEstadoInstrumento.TabIndex = 10
        Me.lblEstadoInstrumento.Text = "Estado:"
        '
        ' cboEstadoInstrumento
        '
        Me.cboEstadoInstrumento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboEstadoInstrumento.FormattingEnabled = True
        Me.cboEstadoInstrumento.Location = New System.Drawing.Point(15, 185)
        Me.cboEstadoInstrumento.Name = "cboEstadoInstrumento"
        Me.cboEstadoInstrumento.Size = New System.Drawing.Size(150, 23)
        Me.cboEstadoInstrumento.TabIndex = 5
        '
        ' btnNuevoInstrumento
        '
        Me.btnNuevoInstrumento.Location = New System.Drawing.Point(180, 183)
        Me.btnNuevoInstrumento.Name = "btnNuevoInstrumento"
        Me.btnNuevoInstrumento.Size = New System.Drawing.Size(120, 30)
        Me.btnNuevoInstrumento.TabIndex = 6
        Me.btnNuevoInstrumento.Text = "Nuevo"
        Me.btnNuevoInstrumento.UseVisualStyleBackColor = True
        '
        ' btnGuardarInstrumento
        '
        Me.btnGuardarInstrumento.Location = New System.Drawing.Point(310, 183)
        Me.btnGuardarInstrumento.Name = "btnGuardarInstrumento"
        Me.btnGuardarInstrumento.Size = New System.Drawing.Size(120, 30)
        Me.btnGuardarInstrumento.TabIndex = 7
        Me.btnGuardarInstrumento.Text = "Guardar"
        Me.btnGuardarInstrumento.UseVisualStyleBackColor = True
        '
        ' btnAnularInstrumento
        '
        Me.btnAnularInstrumento.Location = New System.Drawing.Point(440, 183)
        Me.btnAnularInstrumento.Name = "btnAnularInstrumento"
        Me.btnAnularInstrumento.Size = New System.Drawing.Size(130, 30)
        Me.btnAnularInstrumento.TabIndex = 8
        Me.btnAnularInstrumento.Text = "Anular"
        Me.btnAnularInstrumento.UseVisualStyleBackColor = True
        '
        ' grpComparecientes
        '
        Me.grpComparecientes.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpComparecientes.Controls.Add(Me.lblClienteComp)
        Me.grpComparecientes.Controls.Add(Me.cboClienteComp)
        Me.grpComparecientes.Controls.Add(Me.lblCalidad)
        Me.grpComparecientes.Controls.Add(Me.cboCalidad)
        Me.grpComparecientes.Controls.Add(Me.btnAgregarComp)
        Me.grpComparecientes.Controls.Add(Me.dgvComparecientes)
        Me.grpComparecientes.Controls.Add(Me.btnQuitarComp)
        Me.grpComparecientes.Location = New System.Drawing.Point(580, 310)
        Me.grpComparecientes.Name = "grpComparecientes"
        Me.grpComparecientes.Size = New System.Drawing.Size(595, 305)
        Me.grpComparecientes.TabIndex = 2
        Me.grpComparecientes.TabStop = False
        Me.grpComparecientes.Text = "Comparecientes"
        '
        ' lblClienteComp
        '
        Me.lblClienteComp.AutoSize = True
        Me.lblClienteComp.Location = New System.Drawing.Point(15, 22)
        Me.lblClienteComp.Name = "lblClienteComp"
        Me.lblClienteComp.Size = New System.Drawing.Size(47, 15)
        Me.lblClienteComp.TabIndex = 0
        Me.lblClienteComp.Text = "Cliente:"
        '
        ' cboClienteComp
        '
        Me.cboClienteComp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboClienteComp.FormattingEnabled = True
        Me.cboClienteComp.Location = New System.Drawing.Point(15, 42)
        Me.cboClienteComp.Name = "cboClienteComp"
        Me.cboClienteComp.Size = New System.Drawing.Size(260, 23)
        Me.cboClienteComp.TabIndex = 0
        '
        ' lblCalidad
        '
        Me.lblCalidad.AutoSize = True
        Me.lblCalidad.Location = New System.Drawing.Point(285, 22)
        Me.lblCalidad.Name = "lblCalidad"
        Me.lblCalidad.Size = New System.Drawing.Size(50, 15)
        Me.lblCalidad.TabIndex = 2
        Me.lblCalidad.Text = "Calidad:"
        '
        ' cboCalidad
        '
        Me.cboCalidad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCalidad.FormattingEnabled = True
        Me.cboCalidad.Location = New System.Drawing.Point(285, 42)
        Me.cboCalidad.Name = "cboCalidad"
        Me.cboCalidad.Size = New System.Drawing.Size(130, 23)
        Me.cboCalidad.TabIndex = 1
        '
        ' btnAgregarComp
        '
        Me.btnAgregarComp.Location = New System.Drawing.Point(425, 40)
        Me.btnAgregarComp.Name = "btnAgregarComp"
        Me.btnAgregarComp.Size = New System.Drawing.Size(140, 27)
        Me.btnAgregarComp.TabIndex = 2
        Me.btnAgregarComp.Text = "Agregar"
        Me.btnAgregarComp.UseVisualStyleBackColor = True
        '
        ' dgvComparecientes
        '
        Me.dgvComparecientes.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvComparecientes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvComparecientes.Location = New System.Drawing.Point(15, 80)
        Me.dgvComparecientes.Name = "dgvComparecientes"
        Me.dgvComparecientes.Size = New System.Drawing.Size(565, 155)
        Me.dgvComparecientes.TabIndex = 3
        '
        ' btnQuitarComp
        '
        Me.btnQuitarComp.Location = New System.Drawing.Point(15, 245)
        Me.btnQuitarComp.Name = "btnQuitarComp"
        Me.btnQuitarComp.Size = New System.Drawing.Size(150, 27)
        Me.btnQuitarComp.TabIndex = 4
        Me.btnQuitarComp.Text = "Quitar compareciente"
        Me.btnQuitarComp.UseVisualStyleBackColor = True
        '
        ' tabTestimonios
        '
        Me.tabTestimonios.Controls.Add(Me.dgvTestimonios)
        Me.tabTestimonios.Controls.Add(Me.grpTestimonio)
        Me.tabTestimonios.Location = New System.Drawing.Point(4, 24)
        Me.tabTestimonios.Name = "tabTestimonios"
        Me.tabTestimonios.Padding = New System.Windows.Forms.Padding(3)
        Me.tabTestimonios.Size = New System.Drawing.Size(1192, 632)
        Me.tabTestimonios.TabIndex = 2
        Me.tabTestimonios.Text = "Testimonios"
        Me.tabTestimonios.UseVisualStyleBackColor = True
        '
        ' dgvTestimonios
        '
        Me.dgvTestimonios.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvTestimonios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvTestimonios.Location = New System.Drawing.Point(10, 10)
        Me.dgvTestimonios.Name = "dgvTestimonios"
        Me.dgvTestimonios.Size = New System.Drawing.Size(760, 605)
        Me.dgvTestimonios.TabIndex = 0
        '
        ' grpTestimonio
        '
        Me.grpTestimonio.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpTestimonio.Controls.Add(Me.lblInstrumentoTest)
        Me.grpTestimonio.Controls.Add(Me.cboInstrumentoTest)
        Me.grpTestimonio.Controls.Add(Me.lblTipoTestimonio)
        Me.grpTestimonio.Controls.Add(Me.cboTipoTestimonio)
        Me.grpTestimonio.Controls.Add(Me.lblFechaTestimonio)
        Me.grpTestimonio.Controls.Add(Me.dtpFechaTestimonio)
        Me.grpTestimonio.Controls.Add(Me.lblDestinatario)
        Me.grpTestimonio.Controls.Add(Me.txtDestinatario)
        Me.grpTestimonio.Controls.Add(Me.btnExpedirTestimonio)
        Me.grpTestimonio.Location = New System.Drawing.Point(780, 10)
        Me.grpTestimonio.Name = "grpTestimonio"
        Me.grpTestimonio.Size = New System.Drawing.Size(395, 260)
        Me.grpTestimonio.TabIndex = 1
        Me.grpTestimonio.TabStop = False
        Me.grpTestimonio.Text = "Expedir testimonio"
        '
        ' lblInstrumentoTest
        '
        Me.lblInstrumentoTest.AutoSize = True
        Me.lblInstrumentoTest.Location = New System.Drawing.Point(15, 22)
        Me.lblInstrumentoTest.Name = "lblInstrumentoTest"
        Me.lblInstrumentoTest.Size = New System.Drawing.Size(70, 15)
        Me.lblInstrumentoTest.TabIndex = 0
        Me.lblInstrumentoTest.Text = "Instrumento:"
        '
        ' cboInstrumentoTest
        '
        Me.cboInstrumentoTest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboInstrumentoTest.FormattingEnabled = True
        Me.cboInstrumentoTest.Location = New System.Drawing.Point(15, 42)
        Me.cboInstrumentoTest.Name = "cboInstrumentoTest"
        Me.cboInstrumentoTest.Size = New System.Drawing.Size(365, 23)
        Me.cboInstrumentoTest.TabIndex = 0
        '
        ' lblTipoTestimonio
        '
        Me.lblTipoTestimonio.AutoSize = True
        Me.lblTipoTestimonio.Location = New System.Drawing.Point(15, 78)
        Me.lblTipoTestimonio.Name = "lblTipoTestimonio"
        Me.lblTipoTestimonio.Size = New System.Drawing.Size(33, 15)
        Me.lblTipoTestimonio.TabIndex = 2
        Me.lblTipoTestimonio.Text = "Tipo:"
        '
        ' cboTipoTestimonio
        '
        Me.cboTipoTestimonio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTipoTestimonio.FormattingEnabled = True
        Me.cboTipoTestimonio.Location = New System.Drawing.Point(15, 98)
        Me.cboTipoTestimonio.Name = "cboTipoTestimonio"
        Me.cboTipoTestimonio.Size = New System.Drawing.Size(150, 23)
        Me.cboTipoTestimonio.TabIndex = 1
        '
        ' lblFechaTestimonio
        '
        Me.lblFechaTestimonio.AutoSize = True
        Me.lblFechaTestimonio.Location = New System.Drawing.Point(180, 78)
        Me.lblFechaTestimonio.Name = "lblFechaTestimonio"
        Me.lblFechaTestimonio.Size = New System.Drawing.Size(45, 15)
        Me.lblFechaTestimonio.TabIndex = 4
        Me.lblFechaTestimonio.Text = "Fecha:"
        '
        ' dtpFechaTestimonio
        '
        Me.dtpFechaTestimonio.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaTestimonio.Location = New System.Drawing.Point(180, 98)
        Me.dtpFechaTestimonio.Name = "dtpFechaTestimonio"
        Me.dtpFechaTestimonio.Size = New System.Drawing.Size(150, 23)
        Me.dtpFechaTestimonio.TabIndex = 2
        '
        ' lblDestinatario
        '
        Me.lblDestinatario.AutoSize = True
        Me.lblDestinatario.Location = New System.Drawing.Point(15, 135)
        Me.lblDestinatario.Name = "lblDestinatario"
        Me.lblDestinatario.Size = New System.Drawing.Size(75, 15)
        Me.lblDestinatario.TabIndex = 6
        Me.lblDestinatario.Text = "Destinatario:"
        '
        ' txtDestinatario
        '
        Me.txtDestinatario.Location = New System.Drawing.Point(15, 155)
        Me.txtDestinatario.Name = "txtDestinatario"
        Me.txtDestinatario.Size = New System.Drawing.Size(365, 23)
        Me.txtDestinatario.TabIndex = 3
        '
        ' btnExpedirTestimonio
        '
        Me.btnExpedirTestimonio.Location = New System.Drawing.Point(15, 200)
        Me.btnExpedirTestimonio.Name = "btnExpedirTestimonio"
        Me.btnExpedirTestimonio.Size = New System.Drawing.Size(190, 30)
        Me.btnExpedirTestimonio.TabIndex = 4
        Me.btnExpedirTestimonio.Text = "Expedir testimonio"
        Me.btnExpedirTestimonio.UseVisualStyleBackColor = True
        '
        ' tabIndice
        '
        Me.tabIndice.Controls.Add(Me.dgvIndice)
        Me.tabIndice.Controls.Add(Me.btnGenerarIndice)
        Me.tabIndice.Controls.Add(Me.btnExportarIndice)
        Me.tabIndice.Controls.Add(Me.lblIndiceInfo)
        Me.tabIndice.Location = New System.Drawing.Point(4, 24)
        Me.tabIndice.Name = "tabIndice"
        Me.tabIndice.Padding = New System.Windows.Forms.Padding(3)
        Me.tabIndice.Size = New System.Drawing.Size(1192, 632)
        Me.tabIndice.TabIndex = 3
        Me.tabIndice.Text = "Índice del libro"
        Me.tabIndice.UseVisualStyleBackColor = True
        '
        ' dgvIndice
        '
        Me.dgvIndice.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvIndice.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvIndice.Location = New System.Drawing.Point(10, 10)
        Me.dgvIndice.Name = "dgvIndice"
        Me.dgvIndice.Size = New System.Drawing.Size(1165, 555)
        Me.dgvIndice.TabIndex = 0
        '
        ' btnGenerarIndice
        '
        Me.btnGenerarIndice.Location = New System.Drawing.Point(10, 578)
        Me.btnGenerarIndice.Name = "btnGenerarIndice"
        Me.btnGenerarIndice.Size = New System.Drawing.Size(160, 30)
        Me.btnGenerarIndice.TabIndex = 1
        Me.btnGenerarIndice.Text = "Generar índice"
        Me.btnGenerarIndice.UseVisualStyleBackColor = True
        '
        ' btnExportarIndice
        '
        Me.btnExportarIndice.Location = New System.Drawing.Point(180, 578)
        Me.btnExportarIndice.Name = "btnExportarIndice"
        Me.btnExportarIndice.Size = New System.Drawing.Size(160, 30)
        Me.btnExportarIndice.TabIndex = 2
        Me.btnExportarIndice.Text = "Exportar CSV"
        Me.btnExportarIndice.UseVisualStyleBackColor = True
        '
        ' lblIndiceInfo
        '
        Me.lblIndiceInfo.Location = New System.Drawing.Point(350, 583)
        Me.lblIndiceInfo.Name = "lblIndiceInfo"
        Me.lblIndiceInfo.Size = New System.Drawing.Size(825, 25)
        Me.lblIndiceInfo.TabIndex = 3
        '
        ' FrmProtocolo
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1240, 740)
        Me.Controls.Add(Me.tabPrincipal)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnRefrescar)
        Me.Controls.Add(Me.cboLibro)
        Me.Controls.Add(Me.lblSelLibro)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1180, 700)
        Me.Name = "FrmProtocolo"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Protocolo notarial"
        Me.tabPrincipal.ResumeLayout(False)
        Me.tabLibros.ResumeLayout(False)
        CType(Me.dgvLibros, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpLibro.ResumeLayout(False)
        Me.grpLibro.PerformLayout()
        CType(Me.nudNumeroLibro, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAnio, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabInstrumentos.ResumeLayout(False)
        CType(Me.dgvInstrumentos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpInstrumento.ResumeLayout(False)
        Me.grpInstrumento.PerformLayout()
        CType(Me.nudNumeroEscritura, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudFolioInicio, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudFolioFin, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpComparecientes.ResumeLayout(False)
        Me.grpComparecientes.PerformLayout()
        CType(Me.dgvComparecientes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabTestimonios.ResumeLayout(False)
        CType(Me.dgvTestimonios, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpTestimonio.ResumeLayout(False)
        Me.grpTestimonio.PerformLayout()
        Me.tabIndice.ResumeLayout(False)
        CType(Me.dgvIndice, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblSelLibro As System.Windows.Forms.Label
    Friend WithEvents cboLibro As System.Windows.Forms.ComboBox
    Friend WithEvents btnRefrescar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents tabPrincipal As System.Windows.Forms.TabControl
    Friend WithEvents tabLibros As System.Windows.Forms.TabPage
    Friend WithEvents dgvLibros As System.Windows.Forms.DataGridView
    Friend WithEvents grpLibro As System.Windows.Forms.GroupBox
    Friend WithEvents lblNumeroLibro As System.Windows.Forms.Label
    Friend WithEvents nudNumeroLibro As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblAnio As System.Windows.Forms.Label
    Friend WithEvents nudAnio As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblFechaAutorizacion As System.Windows.Forms.Label
    Friend WithEvents dtpFechaAutorizacion As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblFechaVencimiento As System.Windows.Forms.Label
    Friend WithEvents dtpFechaVencimiento As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblEstadoLibro As System.Windows.Forms.Label
    Friend WithEvents cboEstadoLibro As System.Windows.Forms.ComboBox
    Friend WithEvents lblFechaCierre As System.Windows.Forms.Label
    Friend WithEvents txtFechaCierre As System.Windows.Forms.TextBox
    Friend WithEvents lblFechaEntrega As System.Windows.Forms.Label
    Friend WithEvents dtpFechaEntrega As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblRazonCierre As System.Windows.Forms.Label
    Friend WithEvents txtRazonCierre As System.Windows.Forms.TextBox
    Friend WithEvents lblLimiteEntrega As System.Windows.Forms.Label
    Friend WithEvents btnNuevoLibro As System.Windows.Forms.Button
    Friend WithEvents btnGuardarLibro As System.Windows.Forms.Button
    Friend WithEvents btnCerrarLibro As System.Windows.Forms.Button
    Friend WithEvents btnEntregarLibro As System.Windows.Forms.Button
    Friend WithEvents tabInstrumentos As System.Windows.Forms.TabPage
    Friend WithEvents dgvInstrumentos As System.Windows.Forms.DataGridView
    Friend WithEvents grpInstrumento As System.Windows.Forms.GroupBox
    Friend WithEvents lblNumeroEscritura As System.Windows.Forms.Label
    Friend WithEvents nudNumeroEscritura As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblFechaHora As System.Windows.Forms.Label
    Friend WithEvents dtpFechaHoraOtorgamiento As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblFolioInicio As System.Windows.Forms.Label
    Friend WithEvents nudFolioInicio As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblFolioFin As System.Windows.Forms.Label
    Friend WithEvents nudFolioFin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblObjeto As System.Windows.Forms.Label
    Friend WithEvents txtObjeto As System.Windows.Forms.TextBox
    Friend WithEvents lblEstadoInstrumento As System.Windows.Forms.Label
    Friend WithEvents cboEstadoInstrumento As System.Windows.Forms.ComboBox
    Friend WithEvents btnNuevoInstrumento As System.Windows.Forms.Button
    Friend WithEvents btnGuardarInstrumento As System.Windows.Forms.Button
    Friend WithEvents btnAnularInstrumento As System.Windows.Forms.Button
    Friend WithEvents grpComparecientes As System.Windows.Forms.GroupBox
    Friend WithEvents lblClienteComp As System.Windows.Forms.Label
    Friend WithEvents cboClienteComp As System.Windows.Forms.ComboBox
    Friend WithEvents lblCalidad As System.Windows.Forms.Label
    Friend WithEvents cboCalidad As System.Windows.Forms.ComboBox
    Friend WithEvents btnAgregarComp As System.Windows.Forms.Button
    Friend WithEvents dgvComparecientes As System.Windows.Forms.DataGridView
    Friend WithEvents btnQuitarComp As System.Windows.Forms.Button
    Friend WithEvents tabTestimonios As System.Windows.Forms.TabPage
    Friend WithEvents dgvTestimonios As System.Windows.Forms.DataGridView
    Friend WithEvents grpTestimonio As System.Windows.Forms.GroupBox
    Friend WithEvents lblInstrumentoTest As System.Windows.Forms.Label
    Friend WithEvents cboInstrumentoTest As System.Windows.Forms.ComboBox
    Friend WithEvents lblTipoTestimonio As System.Windows.Forms.Label
    Friend WithEvents cboTipoTestimonio As System.Windows.Forms.ComboBox
    Friend WithEvents lblFechaTestimonio As System.Windows.Forms.Label
    Friend WithEvents dtpFechaTestimonio As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblDestinatario As System.Windows.Forms.Label
    Friend WithEvents txtDestinatario As System.Windows.Forms.TextBox
    Friend WithEvents btnExpedirTestimonio As System.Windows.Forms.Button
    Friend WithEvents tabIndice As System.Windows.Forms.TabPage
    Friend WithEvents dgvIndice As System.Windows.Forms.DataGridView
    Friend WithEvents btnGenerarIndice As System.Windows.Forms.Button
    Friend WithEvents btnExportarIndice As System.Windows.Forms.Button
    Friend WithEvents lblIndiceInfo As System.Windows.Forms.Label
End Class
