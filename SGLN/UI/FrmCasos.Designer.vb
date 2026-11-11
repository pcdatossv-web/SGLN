<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmCasos
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
        Me.btnNuevo = New System.Windows.Forms.Button()
        Me.btnGuardarCaso = New System.Windows.Forms.Button()
        Me.btnRefrescar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.txtFiltro = New System.Windows.Forms.TextBox()
        Me.btnBuscar = New System.Windows.Forms.Button()
        Me.dgvCasos = New System.Windows.Forms.DataGridView()
        Me.grpCaso = New System.Windows.Forms.GroupBox()
        Me.lblCliente = New System.Windows.Forms.Label()
        Me.cboCliente = New System.Windows.Forms.ComboBox()
        Me.lblExpediente = New System.Windows.Forms.Label()
        Me.txtExpediente = New System.Windows.Forms.TextBox()
        Me.lblMateria = New System.Windows.Forms.Label()
        Me.cboMateria = New System.Windows.Forms.ComboBox()
        Me.lblEstado = New System.Windows.Forms.Label()
        Me.cboEstado = New System.Windows.Forms.ComboBox()
        Me.lblJuzgado = New System.Windows.Forms.Label()
        Me.cboJuzgado = New System.Windows.Forms.ComboBox()
        Me.lblFechaApertura = New System.Windows.Forms.Label()
        Me.dtpFechaApertura = New System.Windows.Forms.DateTimePicker()
        Me.lblResponsable = New System.Windows.Forms.Label()
        Me.txtResponsable = New System.Windows.Forms.TextBox()
        Me.lblDescripcion = New System.Windows.Forms.Label()
        Me.txtDescripcion = New System.Windows.Forms.TextBox()
        Me.grpPlazos = New System.Windows.Forms.GroupBox()
        Me.lblTipoActuacion = New System.Windows.Forms.Label()
        Me.cboTipoActuacion = New System.Windows.Forms.ComboBox()
        Me.lblFechaNotificacion = New System.Windows.Forms.Label()
        Me.dtpFechaNotificacion = New System.Windows.Forms.DateTimePicker()
        Me.lblPlazoDias = New System.Windows.Forms.Label()
        Me.nudPlazoDias = New System.Windows.Forms.NumericUpDown()
        Me.btnCalcular = New System.Windows.Forms.Button()
        Me.lblPropuesta = New System.Windows.Forms.Label()
        Me.lblObservaciones = New System.Windows.Forms.Label()
        Me.txtObservaciones = New System.Windows.Forms.TextBox()
        Me.btnConfirmarPlazo = New System.Windows.Forms.Button()
        Me.btnMarcarCumplida = New System.Windows.Forms.Button()
        Me.dgvActuaciones = New System.Windows.Forms.DataGridView()
        CType(Me.dgvCasos, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpCaso.SuspendLayout()
        Me.grpPlazos.SuspendLayout()
        CType(Me.nudPlazoDias, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvActuaciones, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        ' btnNuevo
        '
        Me.btnNuevo.Location = New System.Drawing.Point(12, 12)
        Me.btnNuevo.Name = "btnNuevo"
        Me.btnNuevo.Size = New System.Drawing.Size(80, 28)
        Me.btnNuevo.TabIndex = 0
        Me.btnNuevo.Text = "Nuevo"
        Me.btnNuevo.UseVisualStyleBackColor = True
        '
        ' btnGuardarCaso
        '
        Me.btnGuardarCaso.Location = New System.Drawing.Point(100, 12)
        Me.btnGuardarCaso.Name = "btnGuardarCaso"
        Me.btnGuardarCaso.Size = New System.Drawing.Size(100, 28)
        Me.btnGuardarCaso.TabIndex = 1
        Me.btnGuardarCaso.Text = "Guardar caso"
        Me.btnGuardarCaso.UseVisualStyleBackColor = True
        '
        ' btnRefrescar
        '
        Me.btnRefrescar.Location = New System.Drawing.Point(208, 12)
        Me.btnRefrescar.Name = "btnRefrescar"
        Me.btnRefrescar.Size = New System.Drawing.Size(90, 28)
        Me.btnRefrescar.TabIndex = 2
        Me.btnRefrescar.Text = "Refrescar"
        Me.btnRefrescar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(310, 12)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(870, 40)
        Me.lblMensaje.TabIndex = 3
        '
        ' txtFiltro
        '
        Me.txtFiltro.Location = New System.Drawing.Point(12, 55)
        Me.txtFiltro.Name = "txtFiltro"
        Me.txtFiltro.Size = New System.Drawing.Size(300, 23)
        Me.txtFiltro.TabIndex = 4
        '
        ' btnBuscar
        '
        Me.btnBuscar.Location = New System.Drawing.Point(320, 53)
        Me.btnBuscar.Name = "btnBuscar"
        Me.btnBuscar.Size = New System.Drawing.Size(72, 27)
        Me.btnBuscar.TabIndex = 5
        Me.btnBuscar.Text = "Buscar"
        Me.btnBuscar.UseVisualStyleBackColor = True
        '
        ' dgvCasos
        '
        Me.dgvCasos.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.dgvCasos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvCasos.Location = New System.Drawing.Point(12, 90)
        Me.dgvCasos.Name = "dgvCasos"
        Me.dgvCasos.Size = New System.Drawing.Size(380, 600)
        Me.dgvCasos.TabIndex = 6
        '
        ' grpCaso
        '
        Me.grpCaso.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpCaso.Controls.Add(Me.lblCliente)
        Me.grpCaso.Controls.Add(Me.cboCliente)
        Me.grpCaso.Controls.Add(Me.lblExpediente)
        Me.grpCaso.Controls.Add(Me.txtExpediente)
        Me.grpCaso.Controls.Add(Me.lblMateria)
        Me.grpCaso.Controls.Add(Me.cboMateria)
        Me.grpCaso.Controls.Add(Me.lblEstado)
        Me.grpCaso.Controls.Add(Me.cboEstado)
        Me.grpCaso.Controls.Add(Me.lblJuzgado)
        Me.grpCaso.Controls.Add(Me.cboJuzgado)
        Me.grpCaso.Controls.Add(Me.lblFechaApertura)
        Me.grpCaso.Controls.Add(Me.dtpFechaApertura)
        Me.grpCaso.Controls.Add(Me.lblResponsable)
        Me.grpCaso.Controls.Add(Me.txtResponsable)
        Me.grpCaso.Controls.Add(Me.lblDescripcion)
        Me.grpCaso.Controls.Add(Me.txtDescripcion)
        Me.grpCaso.Location = New System.Drawing.Point(405, 55)
        Me.grpCaso.Name = "grpCaso"
        Me.grpCaso.Size = New System.Drawing.Size(780, 265)
        Me.grpCaso.TabIndex = 7
        Me.grpCaso.TabStop = False
        Me.grpCaso.Text = "Datos del caso"
        '
        ' lblCliente
        '
        Me.lblCliente.AutoSize = True
        Me.lblCliente.Location = New System.Drawing.Point(15, 22)
        Me.lblCliente.Name = "lblCliente"
        Me.lblCliente.Size = New System.Drawing.Size(47, 15)
        Me.lblCliente.TabIndex = 0
        Me.lblCliente.Text = "Cliente:"
        '
        ' cboCliente
        '
        Me.cboCliente.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCliente.FormattingEnabled = True
        Me.cboCliente.Location = New System.Drawing.Point(15, 42)
        Me.cboCliente.Name = "cboCliente"
        Me.cboCliente.Size = New System.Drawing.Size(240, 23)
        Me.cboCliente.TabIndex = 0
        '
        ' lblExpediente
        '
        Me.lblExpediente.AutoSize = True
        Me.lblExpediente.Location = New System.Drawing.Point(270, 22)
        Me.lblExpediente.Name = "lblExpediente"
        Me.lblExpediente.Size = New System.Drawing.Size(110, 15)
        Me.lblExpediente.TabIndex = 2
        Me.lblExpediente.Text = "N.º de expediente:"
        '
        ' txtExpediente
        '
        Me.txtExpediente.Location = New System.Drawing.Point(270, 42)
        Me.txtExpediente.Name = "txtExpediente"
        Me.txtExpediente.Size = New System.Drawing.Size(180, 23)
        Me.txtExpediente.TabIndex = 1
        '
        ' lblMateria
        '
        Me.lblMateria.AutoSize = True
        Me.lblMateria.Location = New System.Drawing.Point(465, 22)
        Me.lblMateria.Name = "lblMateria"
        Me.lblMateria.Size = New System.Drawing.Size(50, 15)
        Me.lblMateria.TabIndex = 4
        Me.lblMateria.Text = "Materia:"
        '
        ' cboMateria
        '
        Me.cboMateria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMateria.FormattingEnabled = True
        Me.cboMateria.Location = New System.Drawing.Point(465, 42)
        Me.cboMateria.Name = "cboMateria"
        Me.cboMateria.Size = New System.Drawing.Size(140, 23)
        Me.cboMateria.TabIndex = 2
        '
        ' lblEstado
        '
        Me.lblEstado.AutoSize = True
        Me.lblEstado.Location = New System.Drawing.Point(620, 22)
        Me.lblEstado.Name = "lblEstado"
        Me.lblEstado.Size = New System.Drawing.Size(45, 15)
        Me.lblEstado.TabIndex = 6
        Me.lblEstado.Text = "Estado:"
        '
        ' cboEstado
        '
        Me.cboEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboEstado.FormattingEnabled = True
        Me.cboEstado.Location = New System.Drawing.Point(620, 42)
        Me.cboEstado.Name = "cboEstado"
        Me.cboEstado.Size = New System.Drawing.Size(130, 23)
        Me.cboEstado.TabIndex = 3
        '
        ' lblJuzgado
        '
        Me.lblJuzgado.AutoSize = True
        Me.lblJuzgado.Location = New System.Drawing.Point(15, 78)
        Me.lblJuzgado.Name = "lblJuzgado"
        Me.lblJuzgado.Size = New System.Drawing.Size(55, 15)
        Me.lblJuzgado.TabIndex = 8
        Me.lblJuzgado.Text = "Juzgado:"
        '
        ' cboJuzgado
        '
        Me.cboJuzgado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboJuzgado.FormattingEnabled = True
        Me.cboJuzgado.Location = New System.Drawing.Point(15, 98)
        Me.cboJuzgado.Name = "cboJuzgado"
        Me.cboJuzgado.Size = New System.Drawing.Size(400, 23)
        Me.cboJuzgado.TabIndex = 4
        '
        ' lblFechaApertura
        '
        Me.lblFechaApertura.AutoSize = True
        Me.lblFechaApertura.Location = New System.Drawing.Point(430, 78)
        Me.lblFechaApertura.Name = "lblFechaApertura"
        Me.lblFechaApertura.Size = New System.Drawing.Size(90, 15)
        Me.lblFechaApertura.TabIndex = 10
        Me.lblFechaApertura.Text = "Fecha apertura:"
        '
        ' dtpFechaApertura
        '
        Me.dtpFechaApertura.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaApertura.Location = New System.Drawing.Point(430, 98)
        Me.dtpFechaApertura.Name = "dtpFechaApertura"
        Me.dtpFechaApertura.Size = New System.Drawing.Size(130, 23)
        Me.dtpFechaApertura.TabIndex = 5
        '
        ' lblResponsable
        '
        Me.lblResponsable.AutoSize = True
        Me.lblResponsable.Location = New System.Drawing.Point(575, 78)
        Me.lblResponsable.Name = "lblResponsable"
        Me.lblResponsable.Size = New System.Drawing.Size(78, 15)
        Me.lblResponsable.TabIndex = 12
        Me.lblResponsable.Text = "Responsable:"
        '
        ' txtResponsable
        '
        Me.txtResponsable.Location = New System.Drawing.Point(575, 98)
        Me.txtResponsable.Name = "txtResponsable"
        Me.txtResponsable.Size = New System.Drawing.Size(175, 23)
        Me.txtResponsable.TabIndex = 6
        '
        ' lblDescripcion
        '
        Me.lblDescripcion.AutoSize = True
        Me.lblDescripcion.Location = New System.Drawing.Point(15, 132)
        Me.lblDescripcion.Name = "lblDescripcion"
        Me.lblDescripcion.Size = New System.Drawing.Size(72, 15)
        Me.lblDescripcion.TabIndex = 14
        Me.lblDescripcion.Text = "Descripción:"
        '
        ' txtDescripcion
        '
        Me.txtDescripcion.Location = New System.Drawing.Point(15, 152)
        Me.txtDescripcion.Multiline = True
        Me.txtDescripcion.Name = "txtDescripcion"
        Me.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDescripcion.Size = New System.Drawing.Size(735, 95)
        Me.txtDescripcion.TabIndex = 7
        '
        ' grpPlazos
        '
        Me.grpPlazos.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpPlazos.Controls.Add(Me.lblTipoActuacion)
        Me.grpPlazos.Controls.Add(Me.cboTipoActuacion)
        Me.grpPlazos.Controls.Add(Me.lblFechaNotificacion)
        Me.grpPlazos.Controls.Add(Me.dtpFechaNotificacion)
        Me.grpPlazos.Controls.Add(Me.lblPlazoDias)
        Me.grpPlazos.Controls.Add(Me.nudPlazoDias)
        Me.grpPlazos.Controls.Add(Me.btnCalcular)
        Me.grpPlazos.Controls.Add(Me.lblPropuesta)
        Me.grpPlazos.Controls.Add(Me.lblObservaciones)
        Me.grpPlazos.Controls.Add(Me.txtObservaciones)
        Me.grpPlazos.Controls.Add(Me.btnConfirmarPlazo)
        Me.grpPlazos.Controls.Add(Me.btnMarcarCumplida)
        Me.grpPlazos.Controls.Add(Me.dgvActuaciones)
        Me.grpPlazos.Location = New System.Drawing.Point(405, 330)
        Me.grpPlazos.Name = "grpPlazos"
        Me.grpPlazos.Size = New System.Drawing.Size(780, 375)
        Me.grpPlazos.TabIndex = 8
        Me.grpPlazos.TabStop = False
        Me.grpPlazos.Text = "Actuaciones y plazos (el cálculo debe confirmarlo el abogado)"
        '
        ' lblTipoActuacion
        '
        Me.lblTipoActuacion.AutoSize = True
        Me.lblTipoActuacion.Location = New System.Drawing.Point(15, 22)
        Me.lblTipoActuacion.Name = "lblTipoActuacion"
        Me.lblTipoActuacion.Size = New System.Drawing.Size(100, 15)
        Me.lblTipoActuacion.TabIndex = 0
        Me.lblTipoActuacion.Text = "Tipo de actuación:"
        '
        ' cboTipoActuacion
        '
        Me.cboTipoActuacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTipoActuacion.FormattingEnabled = True
        Me.cboTipoActuacion.Location = New System.Drawing.Point(15, 42)
        Me.cboTipoActuacion.Name = "cboTipoActuacion"
        Me.cboTipoActuacion.Size = New System.Drawing.Size(300, 23)
        Me.cboTipoActuacion.TabIndex = 0
        '
        ' lblFechaNotificacion
        '
        Me.lblFechaNotificacion.AutoSize = True
        Me.lblFechaNotificacion.Location = New System.Drawing.Point(330, 22)
        Me.lblFechaNotificacion.Name = "lblFechaNotificacion"
        Me.lblFechaNotificacion.Size = New System.Drawing.Size(80, 15)
        Me.lblFechaNotificacion.TabIndex = 2
        Me.lblFechaNotificacion.Text = "Notificación:"
        '
        ' dtpFechaNotificacion
        '
        Me.dtpFechaNotificacion.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFechaNotificacion.Location = New System.Drawing.Point(330, 42)
        Me.dtpFechaNotificacion.Name = "dtpFechaNotificacion"
        Me.dtpFechaNotificacion.Size = New System.Drawing.Size(130, 23)
        Me.dtpFechaNotificacion.TabIndex = 1
        '
        ' lblPlazoDias
        '
        Me.lblPlazoDias.AutoSize = True
        Me.lblPlazoDias.Location = New System.Drawing.Point(475, 22)
        Me.lblPlazoDias.Name = "lblPlazoDias"
        Me.lblPlazoDias.Size = New System.Drawing.Size(70, 15)
        Me.lblPlazoDias.TabIndex = 4
        Me.lblPlazoDias.Text = "Días hábiles:"
        '
        ' nudPlazoDias
        '
        Me.nudPlazoDias.Location = New System.Drawing.Point(475, 42)
        Me.nudPlazoDias.Maximum = New Decimal(New Integer() {365, 0, 0, 0})
        Me.nudPlazoDias.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nudPlazoDias.Name = "nudPlazoDias"
        Me.nudPlazoDias.Size = New System.Drawing.Size(80, 23)
        Me.nudPlazoDias.TabIndex = 2
        Me.nudPlazoDias.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        ' btnCalcular
        '
        Me.btnCalcular.Location = New System.Drawing.Point(570, 40)
        Me.btnCalcular.Name = "btnCalcular"
        Me.btnCalcular.Size = New System.Drawing.Size(100, 27)
        Me.btnCalcular.TabIndex = 3
        Me.btnCalcular.Text = "Calcular"
        Me.btnCalcular.UseVisualStyleBackColor = True
        '
        ' lblPropuesta
        '
        Me.lblPropuesta.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblPropuesta.ForeColor = System.Drawing.Color.DimGray
        Me.lblPropuesta.Location = New System.Drawing.Point(680, 20)
        Me.lblPropuesta.Name = "lblPropuesta"
        Me.lblPropuesta.Size = New System.Drawing.Size(90, 60)
        Me.lblPropuesta.TabIndex = 12
        Me.lblPropuesta.Text = "Sin cálculo."
        '
        ' lblObservaciones
        '
        Me.lblObservaciones.AutoSize = True
        Me.lblObservaciones.Location = New System.Drawing.Point(15, 78)
        Me.lblObservaciones.Name = "lblObservaciones"
        Me.lblObservaciones.Size = New System.Drawing.Size(85, 15)
        Me.lblObservaciones.TabIndex = 13
        Me.lblObservaciones.Text = "Observaciones:"
        '
        ' txtObservaciones
        '
        Me.txtObservaciones.Location = New System.Drawing.Point(15, 98)
        Me.txtObservaciones.Multiline = True
        Me.txtObservaciones.Name = "txtObservaciones"
        Me.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtObservaciones.Size = New System.Drawing.Size(400, 45)
        Me.txtObservaciones.TabIndex = 4
        '
        ' btnConfirmarPlazo
        '
        Me.btnConfirmarPlazo.Location = New System.Drawing.Point(430, 98)
        Me.btnConfirmarPlazo.Name = "btnConfirmarPlazo"
        Me.btnConfirmarPlazo.Size = New System.Drawing.Size(180, 30)
        Me.btnConfirmarPlazo.TabIndex = 5
        Me.btnConfirmarPlazo.Text = "Confirmar plazo"
        Me.btnConfirmarPlazo.UseVisualStyleBackColor = True
        '
        ' btnMarcarCumplida
        '
        Me.btnMarcarCumplida.Location = New System.Drawing.Point(620, 98)
        Me.btnMarcarCumplida.Name = "btnMarcarCumplida"
        Me.btnMarcarCumplida.Size = New System.Drawing.Size(150, 30)
        Me.btnMarcarCumplida.TabIndex = 6
        Me.btnMarcarCumplida.Text = "Marcar cumplida"
        Me.btnMarcarCumplida.UseVisualStyleBackColor = True
        '
        ' dgvActuaciones
        '
        Me.dgvActuaciones.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvActuaciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvActuaciones.Location = New System.Drawing.Point(15, 160)
        Me.dgvActuaciones.Name = "dgvActuaciones"
        Me.dgvActuaciones.Size = New System.Drawing.Size(755, 200)
        Me.dgvActuaciones.TabIndex = 7
        '
        ' FrmCasos
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 720)
        Me.Controls.Add(Me.grpPlazos)
        Me.Controls.Add(Me.grpCaso)
        Me.Controls.Add(Me.dgvCasos)
        Me.Controls.Add(Me.btnBuscar)
        Me.Controls.Add(Me.txtFiltro)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnRefrescar)
        Me.Controls.Add(Me.btnGuardarCaso)
        Me.Controls.Add(Me.btnNuevo)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1150, 700)
        Me.Name = "FrmCasos"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Casos judiciales y control de plazos"
        CType(Me.dgvCasos, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpCaso.ResumeLayout(False)
        Me.grpCaso.PerformLayout()
        Me.grpPlazos.ResumeLayout(False)
        Me.grpPlazos.PerformLayout()
        CType(Me.nudPlazoDias, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvActuaciones, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents btnNuevo As System.Windows.Forms.Button
    Friend WithEvents btnGuardarCaso As System.Windows.Forms.Button
    Friend WithEvents btnRefrescar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents txtFiltro As System.Windows.Forms.TextBox
    Friend WithEvents btnBuscar As System.Windows.Forms.Button
    Friend WithEvents dgvCasos As System.Windows.Forms.DataGridView
    Friend WithEvents grpCaso As System.Windows.Forms.GroupBox
    Friend WithEvents lblCliente As System.Windows.Forms.Label
    Friend WithEvents cboCliente As System.Windows.Forms.ComboBox
    Friend WithEvents lblExpediente As System.Windows.Forms.Label
    Friend WithEvents txtExpediente As System.Windows.Forms.TextBox
    Friend WithEvents lblMateria As System.Windows.Forms.Label
    Friend WithEvents cboMateria As System.Windows.Forms.ComboBox
    Friend WithEvents lblEstado As System.Windows.Forms.Label
    Friend WithEvents cboEstado As System.Windows.Forms.ComboBox
    Friend WithEvents lblJuzgado As System.Windows.Forms.Label
    Friend WithEvents cboJuzgado As System.Windows.Forms.ComboBox
    Friend WithEvents lblFechaApertura As System.Windows.Forms.Label
    Friend WithEvents dtpFechaApertura As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblResponsable As System.Windows.Forms.Label
    Friend WithEvents txtResponsable As System.Windows.Forms.TextBox
    Friend WithEvents lblDescripcion As System.Windows.Forms.Label
    Friend WithEvents txtDescripcion As System.Windows.Forms.TextBox
    Friend WithEvents grpPlazos As System.Windows.Forms.GroupBox
    Friend WithEvents lblTipoActuacion As System.Windows.Forms.Label
    Friend WithEvents cboTipoActuacion As System.Windows.Forms.ComboBox
    Friend WithEvents lblFechaNotificacion As System.Windows.Forms.Label
    Friend WithEvents dtpFechaNotificacion As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblPlazoDias As System.Windows.Forms.Label
    Friend WithEvents nudPlazoDias As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnCalcular As System.Windows.Forms.Button
    Friend WithEvents lblPropuesta As System.Windows.Forms.Label
    Friend WithEvents lblObservaciones As System.Windows.Forms.Label
    Friend WithEvents txtObservaciones As System.Windows.Forms.TextBox
    Friend WithEvents btnConfirmarPlazo As System.Windows.Forms.Button
    Friend WithEvents btnMarcarCumplida As System.Windows.Forms.Button
    Friend WithEvents dgvActuaciones As System.Windows.Forms.DataGridView
End Class
