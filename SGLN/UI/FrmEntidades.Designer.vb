<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmEntidades
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
        Me.btnGuardar = New System.Windows.Forms.Button()
        Me.btnDesactivar = New System.Windows.Forms.Button()
        Me.btnRefrescar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.dgvEntidades = New System.Windows.Forms.DataGridView()
        Me.grpDatos = New System.Windows.Forms.GroupBox()
        Me.lblRazonSocial = New System.Windows.Forms.Label()
        Me.txtRazonSocial = New System.Windows.Forms.TextBox()
        Me.lblNIT = New System.Windows.Forms.Label()
        Me.txtNIT = New System.Windows.Forms.TextBox()
        Me.lblNRC = New System.Windows.Forms.Label()
        Me.txtNRC = New System.Windows.Forms.TextBox()
        Me.lblDomicilio = New System.Windows.Forms.Label()
        Me.txtDomicilio = New System.Windows.Forms.TextBox()
        Me.chkActivo = New System.Windows.Forms.CheckBox()
        Me.lblNivelRiesgo = New System.Windows.Forms.Label()
        Me.cboNivelRiesgo = New System.Windows.Forms.ComboBox()
        Me.chkOrigenVerificado = New System.Windows.Forms.CheckBox()
        Me.lblDetalleOrigen = New System.Windows.Forms.Label()
        Me.txtDetalleOrigen = New System.Windows.Forms.TextBox()
        Me.lblVerificacionListas = New System.Windows.Forms.Label()
        Me.dtpVerificacionListas = New System.Windows.Forms.DateTimePicker()
        Me.lblAlerta = New System.Windows.Forms.Label()
        Me.grpRepresentantes = New System.Windows.Forms.GroupBox()
        Me.lblRepCliente = New System.Windows.Forms.Label()
        Me.cboRepCliente = New System.Windows.Forms.ComboBox()
        Me.lblCargo = New System.Windows.Forms.Label()
        Me.txtCargo = New System.Windows.Forms.TextBox()
        Me.lblTipoRep = New System.Windows.Forms.Label()
        Me.cboTipoRepresentacion = New System.Windows.Forms.ComboBox()
        Me.btnAgregarRep = New System.Windows.Forms.Button()
        Me.btnQuitarRep = New System.Windows.Forms.Button()
        Me.dgvRepresentantes = New System.Windows.Forms.DataGridView()
        CType(Me.dgvEntidades, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpDatos.SuspendLayout()
        Me.grpRepresentantes.SuspendLayout()
        CType(Me.dgvRepresentantes, System.ComponentModel.ISupportInitialize).BeginInit()
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
        ' btnGuardar
        '
        Me.btnGuardar.Location = New System.Drawing.Point(100, 12)
        Me.btnGuardar.Name = "btnGuardar"
        Me.btnGuardar.Size = New System.Drawing.Size(90, 28)
        Me.btnGuardar.TabIndex = 1
        Me.btnGuardar.Text = "Guardar"
        Me.btnGuardar.UseVisualStyleBackColor = True
        '
        ' btnDesactivar
        '
        Me.btnDesactivar.Location = New System.Drawing.Point(198, 12)
        Me.btnDesactivar.Name = "btnDesactivar"
        Me.btnDesactivar.Size = New System.Drawing.Size(95, 28)
        Me.btnDesactivar.TabIndex = 2
        Me.btnDesactivar.Text = "Desactivar"
        Me.btnDesactivar.UseVisualStyleBackColor = True
        '
        ' btnRefrescar
        '
        Me.btnRefrescar.Location = New System.Drawing.Point(301, 12)
        Me.btnRefrescar.Name = "btnRefrescar"
        Me.btnRefrescar.Size = New System.Drawing.Size(90, 28)
        Me.btnRefrescar.TabIndex = 3
        Me.btnRefrescar.Text = "Refrescar"
        Me.btnRefrescar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(405, 12)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(500, 40)
        Me.lblMensaje.TabIndex = 4
        '
        ' dgvEntidades
        '
        Me.dgvEntidades.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.dgvEntidades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvEntidades.Location = New System.Drawing.Point(12, 55)
        Me.dgvEntidades.Name = "dgvEntidades"
        Me.dgvEntidades.Size = New System.Drawing.Size(380, 580)
        Me.dgvEntidades.TabIndex = 5
        '
        ' grpDatos
        '
        Me.grpDatos.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpDatos.Controls.Add(Me.lblRazonSocial)
        Me.grpDatos.Controls.Add(Me.txtRazonSocial)
        Me.grpDatos.Controls.Add(Me.lblNIT)
        Me.grpDatos.Controls.Add(Me.txtNIT)
        Me.grpDatos.Controls.Add(Me.lblNRC)
        Me.grpDatos.Controls.Add(Me.txtNRC)
        Me.grpDatos.Controls.Add(Me.lblDomicilio)
        Me.grpDatos.Controls.Add(Me.txtDomicilio)
        Me.grpDatos.Controls.Add(Me.chkActivo)
        Me.grpDatos.Controls.Add(Me.lblNivelRiesgo)
        Me.grpDatos.Controls.Add(Me.cboNivelRiesgo)
        Me.grpDatos.Controls.Add(Me.chkOrigenVerificado)
        Me.grpDatos.Controls.Add(Me.lblDetalleOrigen)
        Me.grpDatos.Controls.Add(Me.txtDetalleOrigen)
        Me.grpDatos.Controls.Add(Me.lblVerificacionListas)
        Me.grpDatos.Controls.Add(Me.dtpVerificacionListas)
        Me.grpDatos.Controls.Add(Me.lblAlerta)
        Me.grpDatos.Location = New System.Drawing.Point(405, 55)
        Me.grpDatos.Name = "grpDatos"
        Me.grpDatos.Size = New System.Drawing.Size(680, 350)
        Me.grpDatos.TabIndex = 6
        Me.grpDatos.TabStop = False
        Me.grpDatos.Text = "Datos de la entidad y perfil LDA/FT"
        '
        ' lblRazonSocial
        '
        Me.lblRazonSocial.AutoSize = True
        Me.lblRazonSocial.Location = New System.Drawing.Point(15, 20)
        Me.lblRazonSocial.Name = "lblRazonSocial"
        Me.lblRazonSocial.Size = New System.Drawing.Size(75, 15)
        Me.lblRazonSocial.TabIndex = 0
        Me.lblRazonSocial.Text = "Razón social:"
        '
        ' txtRazonSocial
        '
        Me.txtRazonSocial.Location = New System.Drawing.Point(15, 40)
        Me.txtRazonSocial.Name = "txtRazonSocial"
        Me.txtRazonSocial.Size = New System.Drawing.Size(420, 23)
        Me.txtRazonSocial.TabIndex = 0
        '
        ' lblNIT
        '
        Me.lblNIT.AutoSize = True
        Me.lblNIT.Location = New System.Drawing.Point(450, 20)
        Me.lblNIT.Name = "lblNIT"
        Me.lblNIT.Size = New System.Drawing.Size(28, 15)
        Me.lblNIT.TabIndex = 2
        Me.lblNIT.Text = "NIT:"
        '
        ' txtNIT
        '
        Me.txtNIT.Location = New System.Drawing.Point(450, 40)
        Me.txtNIT.MaxLength = 17
        Me.txtNIT.Name = "txtNIT"
        Me.txtNIT.Size = New System.Drawing.Size(210, 23)
        Me.txtNIT.TabIndex = 1
        '
        ' lblNRC
        '
        Me.lblNRC.AutoSize = True
        Me.lblNRC.Location = New System.Drawing.Point(15, 75)
        Me.lblNRC.Name = "lblNRC"
        Me.lblNRC.Size = New System.Drawing.Size(33, 15)
        Me.lblNRC.TabIndex = 4
        Me.lblNRC.Text = "NRC:"
        '
        ' txtNRC
        '
        Me.txtNRC.Location = New System.Drawing.Point(15, 95)
        Me.txtNRC.Name = "txtNRC"
        Me.txtNRC.Size = New System.Drawing.Size(180, 23)
        Me.txtNRC.TabIndex = 2
        '
        ' lblDomicilio
        '
        Me.lblDomicilio.AutoSize = True
        Me.lblDomicilio.Location = New System.Drawing.Point(210, 75)
        Me.lblDomicilio.Name = "lblDomicilio"
        Me.lblDomicilio.Size = New System.Drawing.Size(61, 15)
        Me.lblDomicilio.TabIndex = 6
        Me.lblDomicilio.Text = "Domicilio:"
        '
        ' txtDomicilio
        '
        Me.txtDomicilio.Location = New System.Drawing.Point(210, 95)
        Me.txtDomicilio.Name = "txtDomicilio"
        Me.txtDomicilio.Size = New System.Drawing.Size(450, 23)
        Me.txtDomicilio.TabIndex = 3
        '
        ' chkActivo
        '
        Me.chkActivo.AutoSize = True
        Me.chkActivo.Location = New System.Drawing.Point(15, 135)
        Me.chkActivo.Name = "chkActivo"
        Me.chkActivo.Size = New System.Drawing.Size(105, 19)
        Me.chkActivo.TabIndex = 4
        Me.chkActivo.Text = "Entidad activa"
        Me.chkActivo.UseVisualStyleBackColor = True
        '
        ' lblNivelRiesgo
        '
        Me.lblNivelRiesgo.AutoSize = True
        Me.lblNivelRiesgo.Location = New System.Drawing.Point(15, 165)
        Me.lblNivelRiesgo.Name = "lblNivelRiesgo"
        Me.lblNivelRiesgo.Size = New System.Drawing.Size(87, 15)
        Me.lblNivelRiesgo.TabIndex = 8
        Me.lblNivelRiesgo.Text = "Nivel de riesgo:"
        '
        ' cboNivelRiesgo
        '
        Me.cboNivelRiesgo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboNivelRiesgo.FormattingEnabled = True
        Me.cboNivelRiesgo.Location = New System.Drawing.Point(15, 185)
        Me.cboNivelRiesgo.Name = "cboNivelRiesgo"
        Me.cboNivelRiesgo.Size = New System.Drawing.Size(150, 23)
        Me.cboNivelRiesgo.TabIndex = 5
        '
        ' chkOrigenVerificado
        '
        Me.chkOrigenVerificado.AutoSize = True
        Me.chkOrigenVerificado.Location = New System.Drawing.Point(180, 187)
        Me.chkOrigenVerificado.Name = "chkOrigenVerificado"
        Me.chkOrigenVerificado.Size = New System.Drawing.Size(200, 19)
        Me.chkOrigenVerificado.TabIndex = 6
        Me.chkOrigenVerificado.Text = "Origen de fondos verificado"
        Me.chkOrigenVerificado.UseVisualStyleBackColor = True
        '
        ' lblDetalleOrigen
        '
        Me.lblDetalleOrigen.AutoSize = True
        Me.lblDetalleOrigen.Location = New System.Drawing.Point(15, 220)
        Me.lblDetalleOrigen.Name = "lblDetalleOrigen"
        Me.lblDetalleOrigen.Size = New System.Drawing.Size(150, 15)
        Me.lblDetalleOrigen.TabIndex = 11
        Me.lblDetalleOrigen.Text = "Detalle del origen de fondos:"
        '
        ' txtDetalleOrigen
        '
        Me.txtDetalleOrigen.Location = New System.Drawing.Point(15, 240)
        Me.txtDetalleOrigen.Multiline = True
        Me.txtDetalleOrigen.Name = "txtDetalleOrigen"
        Me.txtDetalleOrigen.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDetalleOrigen.Size = New System.Drawing.Size(645, 40)
        Me.txtDetalleOrigen.TabIndex = 7
        '
        ' lblVerificacionListas
        '
        Me.lblVerificacionListas.AutoSize = True
        Me.lblVerificacionListas.Location = New System.Drawing.Point(15, 290)
        Me.lblVerificacionListas.Name = "lblVerificacionListas"
        Me.lblVerificacionListas.Size = New System.Drawing.Size(150, 15)
        Me.lblVerificacionListas.TabIndex = 13
        Me.lblVerificacionListas.Text = "Verificación de listas:"
        '
        ' dtpVerificacionListas
        '
        Me.dtpVerificacionListas.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpVerificacionListas.Location = New System.Drawing.Point(15, 310)
        Me.dtpVerificacionListas.Name = "dtpVerificacionListas"
        Me.dtpVerificacionListas.ShowCheckBox = True
        Me.dtpVerificacionListas.Size = New System.Drawing.Size(180, 23)
        Me.dtpVerificacionListas.TabIndex = 8
        '
        ' lblAlerta
        '
        Me.lblAlerta.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblAlerta.ForeColor = System.Drawing.Color.Firebrick
        Me.lblAlerta.Location = New System.Drawing.Point(210, 288)
        Me.lblAlerta.Name = "lblAlerta"
        Me.lblAlerta.Size = New System.Drawing.Size(450, 50)
        Me.lblAlerta.TabIndex = 15
        '
        ' grpRepresentantes
        '
        Me.grpRepresentantes.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpRepresentantes.Controls.Add(Me.lblRepCliente)
        Me.grpRepresentantes.Controls.Add(Me.cboRepCliente)
        Me.grpRepresentantes.Controls.Add(Me.lblCargo)
        Me.grpRepresentantes.Controls.Add(Me.txtCargo)
        Me.grpRepresentantes.Controls.Add(Me.lblTipoRep)
        Me.grpRepresentantes.Controls.Add(Me.cboTipoRepresentacion)
        Me.grpRepresentantes.Controls.Add(Me.btnAgregarRep)
        Me.grpRepresentantes.Controls.Add(Me.btnQuitarRep)
        Me.grpRepresentantes.Controls.Add(Me.dgvRepresentantes)
        Me.grpRepresentantes.Location = New System.Drawing.Point(405, 415)
        Me.grpRepresentantes.Name = "grpRepresentantes"
        Me.grpRepresentantes.Size = New System.Drawing.Size(680, 220)
        Me.grpRepresentantes.TabIndex = 7
        Me.grpRepresentantes.TabStop = False
        Me.grpRepresentantes.Text = "Representantes legales"
        '
        ' lblRepCliente
        '
        Me.lblRepCliente.AutoSize = True
        Me.lblRepCliente.Location = New System.Drawing.Point(15, 22)
        Me.lblRepCliente.Name = "lblRepCliente"
        Me.lblRepCliente.Size = New System.Drawing.Size(50, 15)
        Me.lblRepCliente.TabIndex = 0
        Me.lblRepCliente.Text = "Cliente:"
        '
        ' cboRepCliente
        '
        Me.cboRepCliente.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboRepCliente.FormattingEnabled = True
        Me.cboRepCliente.Location = New System.Drawing.Point(15, 42)
        Me.cboRepCliente.Name = "cboRepCliente"
        Me.cboRepCliente.Size = New System.Drawing.Size(240, 23)
        Me.cboRepCliente.TabIndex = 0
        '
        ' lblCargo
        '
        Me.lblCargo.AutoSize = True
        Me.lblCargo.Location = New System.Drawing.Point(265, 22)
        Me.lblCargo.Name = "lblCargo"
        Me.lblCargo.Size = New System.Drawing.Size(42, 15)
        Me.lblCargo.TabIndex = 2
        Me.lblCargo.Text = "Cargo:"
        '
        ' txtCargo
        '
        Me.txtCargo.Location = New System.Drawing.Point(265, 42)
        Me.txtCargo.Name = "txtCargo"
        Me.txtCargo.Size = New System.Drawing.Size(150, 23)
        Me.txtCargo.TabIndex = 1
        '
        ' lblTipoRep
        '
        Me.lblTipoRep.AutoSize = True
        Me.lblTipoRep.Location = New System.Drawing.Point(425, 22)
        Me.lblTipoRep.Name = "lblTipoRep"
        Me.lblTipoRep.Size = New System.Drawing.Size(33, 15)
        Me.lblTipoRep.TabIndex = 4
        Me.lblTipoRep.Text = "Tipo:"
        '
        ' cboTipoRepresentacion
        '
        Me.cboTipoRepresentacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboTipoRepresentacion.FormattingEnabled = True
        Me.cboTipoRepresentacion.Location = New System.Drawing.Point(425, 42)
        Me.cboTipoRepresentacion.Name = "cboTipoRepresentacion"
        Me.cboTipoRepresentacion.Size = New System.Drawing.Size(110, 23)
        Me.cboTipoRepresentacion.TabIndex = 2
        '
        ' btnAgregarRep
        '
        Me.btnAgregarRep.Location = New System.Drawing.Point(545, 40)
        Me.btnAgregarRep.Name = "btnAgregarRep"
        Me.btnAgregarRep.Size = New System.Drawing.Size(115, 27)
        Me.btnAgregarRep.TabIndex = 3
        Me.btnAgregarRep.Text = "Agregar"
        Me.btnAgregarRep.UseVisualStyleBackColor = True
        '
        ' btnQuitarRep
        '
        Me.btnQuitarRep.Location = New System.Drawing.Point(545, 74)
        Me.btnQuitarRep.Name = "btnQuitarRep"
        Me.btnQuitarRep.Size = New System.Drawing.Size(115, 27)
        Me.btnQuitarRep.TabIndex = 4
        Me.btnQuitarRep.Text = "Quitar"
        Me.btnQuitarRep.UseVisualStyleBackColor = True
        '
        ' dgvRepresentantes
        '
        Me.dgvRepresentantes.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvRepresentantes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRepresentantes.Location = New System.Drawing.Point(15, 112)
        Me.dgvRepresentantes.Name = "dgvRepresentantes"
        Me.dgvRepresentantes.Size = New System.Drawing.Size(645, 95)
        Me.dgvRepresentantes.TabIndex = 9
        '
        ' FrmEntidades
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1100, 650)
        Me.Controls.Add(Me.grpRepresentantes)
        Me.Controls.Add(Me.grpDatos)
        Me.Controls.Add(Me.dgvEntidades)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnRefrescar)
        Me.Controls.Add(Me.btnDesactivar)
        Me.Controls.Add(Me.btnGuardar)
        Me.Controls.Add(Me.btnNuevo)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1050, 620)
        Me.Name = "FrmEntidades"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Entidades legales y representantes"
        CType(Me.dgvEntidades, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpDatos.ResumeLayout(False)
        Me.grpDatos.PerformLayout()
        Me.grpRepresentantes.ResumeLayout(False)
        Me.grpRepresentantes.PerformLayout()
        CType(Me.dgvRepresentantes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents btnNuevo As System.Windows.Forms.Button
    Friend WithEvents btnGuardar As System.Windows.Forms.Button
    Friend WithEvents btnDesactivar As System.Windows.Forms.Button
    Friend WithEvents btnRefrescar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents dgvEntidades As System.Windows.Forms.DataGridView
    Friend WithEvents grpDatos As System.Windows.Forms.GroupBox
    Friend WithEvents lblRazonSocial As System.Windows.Forms.Label
    Friend WithEvents txtRazonSocial As System.Windows.Forms.TextBox
    Friend WithEvents lblNIT As System.Windows.Forms.Label
    Friend WithEvents txtNIT As System.Windows.Forms.TextBox
    Friend WithEvents lblNRC As System.Windows.Forms.Label
    Friend WithEvents txtNRC As System.Windows.Forms.TextBox
    Friend WithEvents lblDomicilio As System.Windows.Forms.Label
    Friend WithEvents txtDomicilio As System.Windows.Forms.TextBox
    Friend WithEvents chkActivo As System.Windows.Forms.CheckBox
    Friend WithEvents lblNivelRiesgo As System.Windows.Forms.Label
    Friend WithEvents cboNivelRiesgo As System.Windows.Forms.ComboBox
    Friend WithEvents chkOrigenVerificado As System.Windows.Forms.CheckBox
    Friend WithEvents lblDetalleOrigen As System.Windows.Forms.Label
    Friend WithEvents txtDetalleOrigen As System.Windows.Forms.TextBox
    Friend WithEvents lblVerificacionListas As System.Windows.Forms.Label
    Friend WithEvents dtpVerificacionListas As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblAlerta As System.Windows.Forms.Label
    Friend WithEvents grpRepresentantes As System.Windows.Forms.GroupBox
    Friend WithEvents lblRepCliente As System.Windows.Forms.Label
    Friend WithEvents cboRepCliente As System.Windows.Forms.ComboBox
    Friend WithEvents lblCargo As System.Windows.Forms.Label
    Friend WithEvents txtCargo As System.Windows.Forms.TextBox
    Friend WithEvents lblTipoRep As System.Windows.Forms.Label
    Friend WithEvents cboTipoRepresentacion As System.Windows.Forms.ComboBox
    Friend WithEvents btnAgregarRep As System.Windows.Forms.Button
    Friend WithEvents btnQuitarRep As System.Windows.Forms.Button
    Friend WithEvents dgvRepresentantes As System.Windows.Forms.DataGridView
End Class
