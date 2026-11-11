<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmClientes
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
        Me.lblBuscar = New System.Windows.Forms.Label()
        Me.txtBuscar = New System.Windows.Forms.TextBox()
        Me.btnBuscar = New System.Windows.Forms.Button()
        Me.btnNuevo = New System.Windows.Forms.Button()
        Me.btnGuardar = New System.Windows.Forms.Button()
        Me.btnDesactivar = New System.Windows.Forms.Button()
        Me.btnEntidades = New System.Windows.Forms.Button()
        Me.btnRefrescar = New System.Windows.Forms.Button()
        Me.lblMensaje = New System.Windows.Forms.Label()
        Me.dgvClientes = New System.Windows.Forms.DataGridView()
        Me.tabPrincipal = New System.Windows.Forms.TabControl()
        Me.tabPersonales = New System.Windows.Forms.TabPage()
        Me.lblNombres = New System.Windows.Forms.Label()
        Me.txtNombres = New System.Windows.Forms.TextBox()
        Me.lblApellidos = New System.Windows.Forms.Label()
        Me.txtApellidos = New System.Windows.Forms.TextBox()
        Me.lblDUI = New System.Windows.Forms.Label()
        Me.txtDUI = New System.Windows.Forms.TextBox()
        Me.lblNIT = New System.Windows.Forms.Label()
        Me.txtNIT = New System.Windows.Forms.TextBox()
        Me.lblNacionalidad = New System.Windows.Forms.Label()
        Me.txtNacionalidad = New System.Windows.Forms.TextBox()
        Me.lblProfesion = New System.Windows.Forms.Label()
        Me.txtProfesion = New System.Windows.Forms.TextBox()
        Me.lblNacimiento = New System.Windows.Forms.Label()
        Me.dtpNacimiento = New System.Windows.Forms.DateTimePicker()
        Me.lblDireccion = New System.Windows.Forms.Label()
        Me.txtDireccion = New System.Windows.Forms.TextBox()
        Me.lblTelefono = New System.Windows.Forms.Label()
        Me.txtTelefono = New System.Windows.Forms.TextBox()
        Me.lblEmail = New System.Windows.Forms.Label()
        Me.txtEmail = New System.Windows.Forms.TextBox()
        Me.chkActivo = New System.Windows.Forms.CheckBox()
        Me.lblAyuda = New System.Windows.Forms.Label()
        Me.tabLdaft = New System.Windows.Forms.TabPage()
        Me.lblNivelRiesgo = New System.Windows.Forms.Label()
        Me.cboNivelRiesgo = New System.Windows.Forms.ComboBox()
        Me.chkOrigenVerificado = New System.Windows.Forms.CheckBox()
        Me.lblDetalleOrigen = New System.Windows.Forms.Label()
        Me.txtDetalleOrigen = New System.Windows.Forms.TextBox()
        Me.lblVerificacionListas = New System.Windows.Forms.Label()
        Me.dtpVerificacionListas = New System.Windows.Forms.DateTimePicker()
        Me.lblObservacionesLdaft = New System.Windows.Forms.Label()
        Me.txtObservaciones = New System.Windows.Forms.TextBox()
        Me.lblAlerta = New System.Windows.Forms.Label()
        Me.lblNota = New System.Windows.Forms.Label()
        CType(Me.dgvClientes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabPrincipal.SuspendLayout()
        Me.tabPersonales.SuspendLayout()
        Me.tabLdaft.SuspendLayout()
        Me.SuspendLayout()
        '
        ' lblBuscar
        '
        Me.lblBuscar.AutoSize = True
        Me.lblBuscar.Location = New System.Drawing.Point(12, 17)
        Me.lblBuscar.Name = "lblBuscar"
        Me.lblBuscar.Size = New System.Drawing.Size(46, 15)
        Me.lblBuscar.TabIndex = 0
        Me.lblBuscar.Text = "Buscar:"
        '
        ' txtBuscar
        '
        Me.txtBuscar.Location = New System.Drawing.Point(64, 13)
        Me.txtBuscar.Name = "txtBuscar"
        Me.txtBuscar.Size = New System.Drawing.Size(250, 23)
        Me.txtBuscar.TabIndex = 1
        '
        ' btnBuscar
        '
        Me.btnBuscar.Location = New System.Drawing.Point(322, 12)
        Me.btnBuscar.Name = "btnBuscar"
        Me.btnBuscar.Size = New System.Drawing.Size(75, 27)
        Me.btnBuscar.TabIndex = 2
        Me.btnBuscar.Text = "Buscar"
        Me.btnBuscar.UseVisualStyleBackColor = True
        '
        ' btnNuevo
        '
        Me.btnNuevo.Location = New System.Drawing.Point(405, 12)
        Me.btnNuevo.Name = "btnNuevo"
        Me.btnNuevo.Size = New System.Drawing.Size(75, 27)
        Me.btnNuevo.TabIndex = 3
        Me.btnNuevo.Text = "Nuevo"
        Me.btnNuevo.UseVisualStyleBackColor = True
        '
        ' btnGuardar
        '
        Me.btnGuardar.Location = New System.Drawing.Point(488, 12)
        Me.btnGuardar.Name = "btnGuardar"
        Me.btnGuardar.Size = New System.Drawing.Size(85, 27)
        Me.btnGuardar.TabIndex = 4
        Me.btnGuardar.Text = "Guardar"
        Me.btnGuardar.UseVisualStyleBackColor = True
        '
        ' btnDesactivar
        '
        Me.btnDesactivar.Location = New System.Drawing.Point(581, 12)
        Me.btnDesactivar.Name = "btnDesactivar"
        Me.btnDesactivar.Size = New System.Drawing.Size(95, 27)
        Me.btnDesactivar.TabIndex = 5
        Me.btnDesactivar.Text = "Desactivar"
        Me.btnDesactivar.UseVisualStyleBackColor = True
        '
        ' btnEntidades
        '
        Me.btnEntidades.Location = New System.Drawing.Point(684, 12)
        Me.btnEntidades.Name = "btnEntidades"
        Me.btnEntidades.Size = New System.Drawing.Size(120, 27)
        Me.btnEntidades.TabIndex = 6
        Me.btnEntidades.Text = "Entidades legales"
        Me.btnEntidades.UseVisualStyleBackColor = True
        '
        ' btnRefrescar
        '
        Me.btnRefrescar.Location = New System.Drawing.Point(812, 12)
        Me.btnRefrescar.Name = "btnRefrescar"
        Me.btnRefrescar.Size = New System.Drawing.Size(90, 27)
        Me.btnRefrescar.TabIndex = 7
        Me.btnRefrescar.Text = "Refrescar"
        Me.btnRefrescar.UseVisualStyleBackColor = True
        '
        ' lblMensaje
        '
        Me.lblMensaje.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblMensaje.Location = New System.Drawing.Point(918, 12)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(270, 40)
        Me.lblMensaje.TabIndex = 8
        '
        ' dgvClientes
        '
        Me.dgvClientes.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.dgvClientes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvClientes.Location = New System.Drawing.Point(12, 55)
        Me.dgvClientes.Name = "dgvClientes"
        Me.dgvClientes.Size = New System.Drawing.Size(540, 640)
        Me.dgvClientes.TabIndex = 9
        '
        ' tabPrincipal
        '
        Me.tabPrincipal.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabPrincipal.Controls.Add(Me.tabPersonales)
        Me.tabPrincipal.Controls.Add(Me.tabLdaft)
        Me.tabPrincipal.Location = New System.Drawing.Point(565, 55)
        Me.tabPrincipal.Name = "tabPrincipal"
        Me.tabPrincipal.SelectedIndex = 0
        Me.tabPrincipal.Size = New System.Drawing.Size(620, 640)
        Me.tabPrincipal.TabIndex = 10
        '
        ' tabPersonales
        '
        Me.tabPersonales.Controls.Add(Me.lblNombres)
        Me.tabPersonales.Controls.Add(Me.txtNombres)
        Me.tabPersonales.Controls.Add(Me.lblApellidos)
        Me.tabPersonales.Controls.Add(Me.txtApellidos)
        Me.tabPersonales.Controls.Add(Me.lblDUI)
        Me.tabPersonales.Controls.Add(Me.txtDUI)
        Me.tabPersonales.Controls.Add(Me.lblNIT)
        Me.tabPersonales.Controls.Add(Me.txtNIT)
        Me.tabPersonales.Controls.Add(Me.lblNacionalidad)
        Me.tabPersonales.Controls.Add(Me.txtNacionalidad)
        Me.tabPersonales.Controls.Add(Me.lblProfesion)
        Me.tabPersonales.Controls.Add(Me.txtProfesion)
        Me.tabPersonales.Controls.Add(Me.lblNacimiento)
        Me.tabPersonales.Controls.Add(Me.dtpNacimiento)
        Me.tabPersonales.Controls.Add(Me.lblDireccion)
        Me.tabPersonales.Controls.Add(Me.txtDireccion)
        Me.tabPersonales.Controls.Add(Me.lblTelefono)
        Me.tabPersonales.Controls.Add(Me.txtTelefono)
        Me.tabPersonales.Controls.Add(Me.lblEmail)
        Me.tabPersonales.Controls.Add(Me.txtEmail)
        Me.tabPersonales.Controls.Add(Me.chkActivo)
        Me.tabPersonales.Controls.Add(Me.lblAyuda)
        Me.tabPersonales.Location = New System.Drawing.Point(4, 24)
        Me.tabPersonales.Name = "tabPersonales"
        Me.tabPersonales.Padding = New System.Windows.Forms.Padding(3)
        Me.tabPersonales.Size = New System.Drawing.Size(612, 612)
        Me.tabPersonales.TabIndex = 0
        Me.tabPersonales.Text = "Datos personales"
        Me.tabPersonales.UseVisualStyleBackColor = True
        '
        ' lblNombres
        '
        Me.lblNombres.AutoSize = True
        Me.lblNombres.Location = New System.Drawing.Point(20, 18)
        Me.lblNombres.Name = "lblNombres"
        Me.lblNombres.Size = New System.Drawing.Size(62, 15)
        Me.lblNombres.TabIndex = 0
        Me.lblNombres.Text = "Nombres:"
        '
        ' txtNombres
        '
        Me.txtNombres.Location = New System.Drawing.Point(20, 38)
        Me.txtNombres.Name = "txtNombres"
        Me.txtNombres.Size = New System.Drawing.Size(260, 23)
        Me.txtNombres.TabIndex = 0
        '
        ' lblApellidos
        '
        Me.lblApellidos.AutoSize = True
        Me.lblApellidos.Location = New System.Drawing.Point(300, 18)
        Me.lblApellidos.Name = "lblApellidos"
        Me.lblApellidos.Size = New System.Drawing.Size(63, 15)
        Me.lblApellidos.TabIndex = 2
        Me.lblApellidos.Text = "Apellidos:"
        '
        ' txtApellidos
        '
        Me.txtApellidos.Location = New System.Drawing.Point(300, 38)
        Me.txtApellidos.Name = "txtApellidos"
        Me.txtApellidos.Size = New System.Drawing.Size(260, 23)
        Me.txtApellidos.TabIndex = 1
        '
        ' lblDUI
        '
        Me.lblDUI.AutoSize = True
        Me.lblDUI.Location = New System.Drawing.Point(20, 78)
        Me.lblDUI.Name = "lblDUI"
        Me.lblDUI.Size = New System.Drawing.Size(30, 15)
        Me.lblDUI.TabIndex = 4
        Me.lblDUI.Text = "DUI:"
        '
        ' txtDUI
        '
        Me.txtDUI.Location = New System.Drawing.Point(20, 98)
        Me.txtDUI.MaxLength = 10
        Me.txtDUI.Name = "txtDUI"
        Me.txtDUI.Size = New System.Drawing.Size(150, 23)
        Me.txtDUI.TabIndex = 2
        '
        ' lblNIT
        '
        Me.lblNIT.AutoSize = True
        Me.lblNIT.Location = New System.Drawing.Point(190, 78)
        Me.lblNIT.Name = "lblNIT"
        Me.lblNIT.Size = New System.Drawing.Size(28, 15)
        Me.lblNIT.TabIndex = 6
        Me.lblNIT.Text = "NIT:"
        '
        ' txtNIT
        '
        Me.txtNIT.Location = New System.Drawing.Point(190, 98)
        Me.txtNIT.MaxLength = 17
        Me.txtNIT.Name = "txtNIT"
        Me.txtNIT.Size = New System.Drawing.Size(160, 23)
        Me.txtNIT.TabIndex = 3
        '
        ' lblNacionalidad
        '
        Me.lblNacionalidad.AutoSize = True
        Me.lblNacionalidad.Location = New System.Drawing.Point(370, 78)
        Me.lblNacionalidad.Name = "lblNacionalidad"
        Me.lblNacionalidad.Size = New System.Drawing.Size(83, 15)
        Me.lblNacionalidad.TabIndex = 8
        Me.lblNacionalidad.Text = "Nacionalidad:"
        '
        ' txtNacionalidad
        '
        Me.txtNacionalidad.Location = New System.Drawing.Point(370, 98)
        Me.txtNacionalidad.Name = "txtNacionalidad"
        Me.txtNacionalidad.Size = New System.Drawing.Size(190, 23)
        Me.txtNacionalidad.TabIndex = 4
        '
        ' lblProfesion
        '
        Me.lblProfesion.AutoSize = True
        Me.lblProfesion.Location = New System.Drawing.Point(20, 138)
        Me.lblProfesion.Name = "lblProfesion"
        Me.lblProfesion.Size = New System.Drawing.Size(62, 15)
        Me.lblProfesion.TabIndex = 10
        Me.lblProfesion.Text = "Profesión:"
        '
        ' txtProfesion
        '
        Me.txtProfesion.Location = New System.Drawing.Point(20, 158)
        Me.txtProfesion.Name = "txtProfesion"
        Me.txtProfesion.Size = New System.Drawing.Size(260, 23)
        Me.txtProfesion.TabIndex = 5
        '
        ' lblNacimiento
        '
        Me.lblNacimiento.AutoSize = True
        Me.lblNacimiento.Location = New System.Drawing.Point(300, 138)
        Me.lblNacimiento.Name = "lblNacimiento"
        Me.lblNacimiento.Size = New System.Drawing.Size(119, 15)
        Me.lblNacimiento.TabIndex = 12
        Me.lblNacimiento.Text = "Fecha de nacimiento:"
        '
        ' dtpNacimiento
        '
        Me.dtpNacimiento.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpNacimiento.Location = New System.Drawing.Point(300, 158)
        Me.dtpNacimiento.Name = "dtpNacimiento"
        Me.dtpNacimiento.ShowCheckBox = True
        Me.dtpNacimiento.Size = New System.Drawing.Size(200, 23)
        Me.dtpNacimiento.TabIndex = 6
        '
        ' lblDireccion
        '
        Me.lblDireccion.AutoSize = True
        Me.lblDireccion.Location = New System.Drawing.Point(20, 198)
        Me.lblDireccion.Name = "lblDireccion"
        Me.lblDireccion.Size = New System.Drawing.Size(60, 15)
        Me.lblDireccion.TabIndex = 14
        Me.lblDireccion.Text = "Dirección:"
        '
        ' txtDireccion
        '
        Me.txtDireccion.Location = New System.Drawing.Point(20, 218)
        Me.txtDireccion.Multiline = True
        Me.txtDireccion.Name = "txtDireccion"
        Me.txtDireccion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDireccion.Size = New System.Drawing.Size(540, 50)
        Me.txtDireccion.TabIndex = 7
        '
        ' lblTelefono
        '
        Me.lblTelefono.AutoSize = True
        Me.lblTelefono.Location = New System.Drawing.Point(20, 278)
        Me.lblTelefono.Name = "lblTelefono"
        Me.lblTelefono.Size = New System.Drawing.Size(55, 15)
        Me.lblTelefono.TabIndex = 16
        Me.lblTelefono.Text = "Teléfono:"
        '
        ' txtTelefono
        '
        Me.txtTelefono.Location = New System.Drawing.Point(20, 298)
        Me.txtTelefono.Name = "txtTelefono"
        Me.txtTelefono.Size = New System.Drawing.Size(150, 23)
        Me.txtTelefono.TabIndex = 8
        '
        ' lblEmail
        '
        Me.lblEmail.AutoSize = True
        Me.lblEmail.Location = New System.Drawing.Point(190, 278)
        Me.lblEmail.Name = "lblEmail"
        Me.lblEmail.Size = New System.Drawing.Size(39, 15)
        Me.lblEmail.TabIndex = 18
        Me.lblEmail.Text = "Email:"
        '
        ' txtEmail
        '
        Me.txtEmail.Location = New System.Drawing.Point(190, 298)
        Me.txtEmail.Name = "txtEmail"
        Me.txtEmail.Size = New System.Drawing.Size(260, 23)
        Me.txtEmail.TabIndex = 9
        '
        ' chkActivo
        '
        Me.chkActivo.AutoSize = True
        Me.chkActivo.Location = New System.Drawing.Point(20, 340)
        Me.chkActivo.Name = "chkActivo"
        Me.chkActivo.Size = New System.Drawing.Size(100, 19)
        Me.chkActivo.TabIndex = 10
        Me.chkActivo.Text = "Cliente activo"
        Me.chkActivo.UseVisualStyleBackColor = True
        '
        ' lblAyuda
        '
        Me.lblAyuda.ForeColor = System.Drawing.Color.DimGray
        Me.lblAyuda.Location = New System.Drawing.Point(20, 380)
        Me.lblAyuda.Name = "lblAyuda"
        Me.lblAyuda.Size = New System.Drawing.Size(560, 120)
        Me.lblAyuda.TabIndex = 21
        Me.lblAyuda.Text = "El DUI y el NIT se validan al guardar. El dígito verificador del DUI se muestra como advertencia para que el abogado lo confirme con el documento físico." & Microsoft.VisualBasic.ControlChars.CrLf & Microsoft.VisualBasic.ControlChars.CrLf &
            "La profesión es un dato relevante para las escrituras y para el perfil de debida diligencia."
        '
        ' tabLdaft
        '
        Me.tabLdaft.Controls.Add(Me.lblNivelRiesgo)
        Me.tabLdaft.Controls.Add(Me.cboNivelRiesgo)
        Me.tabLdaft.Controls.Add(Me.chkOrigenVerificado)
        Me.tabLdaft.Controls.Add(Me.lblDetalleOrigen)
        Me.tabLdaft.Controls.Add(Me.txtDetalleOrigen)
        Me.tabLdaft.Controls.Add(Me.lblVerificacionListas)
        Me.tabLdaft.Controls.Add(Me.dtpVerificacionListas)
        Me.tabLdaft.Controls.Add(Me.lblObservacionesLdaft)
        Me.tabLdaft.Controls.Add(Me.txtObservaciones)
        Me.tabLdaft.Controls.Add(Me.lblAlerta)
        Me.tabLdaft.Controls.Add(Me.lblNota)
        Me.tabLdaft.Location = New System.Drawing.Point(4, 24)
        Me.tabLdaft.Name = "tabLdaft"
        Me.tabLdaft.Padding = New System.Windows.Forms.Padding(3)
        Me.tabLdaft.Size = New System.Drawing.Size(612, 612)
        Me.tabLdaft.TabIndex = 1
        Me.tabLdaft.Text = "Perfil LDA/FT"
        Me.tabLdaft.UseVisualStyleBackColor = True
        '
        ' lblNivelRiesgo
        '
        Me.lblNivelRiesgo.AutoSize = True
        Me.lblNivelRiesgo.Location = New System.Drawing.Point(20, 18)
        Me.lblNivelRiesgo.Name = "lblNivelRiesgo"
        Me.lblNivelRiesgo.Size = New System.Drawing.Size(87, 15)
        Me.lblNivelRiesgo.TabIndex = 0
        Me.lblNivelRiesgo.Text = "Nivel de riesgo:"
        '
        ' cboNivelRiesgo
        '
        Me.cboNivelRiesgo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboNivelRiesgo.FormattingEnabled = True
        Me.cboNivelRiesgo.Location = New System.Drawing.Point(20, 38)
        Me.cboNivelRiesgo.Name = "cboNivelRiesgo"
        Me.cboNivelRiesgo.Size = New System.Drawing.Size(180, 23)
        Me.cboNivelRiesgo.TabIndex = 0
        '
        ' chkOrigenVerificado
        '
        Me.chkOrigenVerificado.AutoSize = True
        Me.chkOrigenVerificado.Location = New System.Drawing.Point(20, 80)
        Me.chkOrigenVerificado.Name = "chkOrigenVerificado"
        Me.chkOrigenVerificado.Size = New System.Drawing.Size(200, 19)
        Me.chkOrigenVerificado.TabIndex = 1
        Me.chkOrigenVerificado.Text = "Origen de fondos verificado"
        Me.chkOrigenVerificado.UseVisualStyleBackColor = True
        '
        ' lblDetalleOrigen
        '
        Me.lblDetalleOrigen.AutoSize = True
        Me.lblDetalleOrigen.Location = New System.Drawing.Point(20, 115)
        Me.lblDetalleOrigen.Name = "lblDetalleOrigen"
        Me.lblDetalleOrigen.Size = New System.Drawing.Size(150, 15)
        Me.lblDetalleOrigen.TabIndex = 3
        Me.lblDetalleOrigen.Text = "Detalle del origen de fondos:"
        '
        ' txtDetalleOrigen
        '
        Me.txtDetalleOrigen.Location = New System.Drawing.Point(20, 135)
        Me.txtDetalleOrigen.Multiline = True
        Me.txtDetalleOrigen.Name = "txtDetalleOrigen"
        Me.txtDetalleOrigen.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDetalleOrigen.Size = New System.Drawing.Size(560, 60)
        Me.txtDetalleOrigen.TabIndex = 2
        '
        ' lblVerificacionListas
        '
        Me.lblVerificacionListas.AutoSize = True
        Me.lblVerificacionListas.Location = New System.Drawing.Point(20, 210)
        Me.lblVerificacionListas.Name = "lblVerificacionListas"
        Me.lblVerificacionListas.Size = New System.Drawing.Size(220, 15)
        Me.lblVerificacionListas.TabIndex = 5
        Me.lblVerificacionListas.Text = "Última verificación de listas (OFAC/ONU):"
        '
        ' dtpVerificacionListas
        '
        Me.dtpVerificacionListas.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpVerificacionListas.Location = New System.Drawing.Point(20, 230)
        Me.dtpVerificacionListas.Name = "dtpVerificacionListas"
        Me.dtpVerificacionListas.ShowCheckBox = True
        Me.dtpVerificacionListas.Size = New System.Drawing.Size(200, 23)
        Me.dtpVerificacionListas.TabIndex = 3
        '
        ' lblObservacionesLdaft
        '
        Me.lblObservacionesLdaft.AutoSize = True
        Me.lblObservacionesLdaft.Location = New System.Drawing.Point(20, 270)
        Me.lblObservacionesLdaft.Name = "lblObservacionesLdaft"
        Me.lblObservacionesLdaft.Size = New System.Drawing.Size(88, 15)
        Me.lblObservacionesLdaft.TabIndex = 7
        Me.lblObservacionesLdaft.Text = "Observaciones:"
        '
        ' txtObservaciones
        '
        Me.txtObservaciones.Location = New System.Drawing.Point(20, 290)
        Me.txtObservaciones.Multiline = True
        Me.txtObservaciones.Name = "txtObservaciones"
        Me.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtObservaciones.Size = New System.Drawing.Size(560, 60)
        Me.txtObservaciones.TabIndex = 4
        '
        ' lblAlerta
        '
        Me.lblAlerta.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblAlerta.ForeColor = System.Drawing.Color.Firebrick
        Me.lblAlerta.Location = New System.Drawing.Point(20, 365)
        Me.lblAlerta.Name = "lblAlerta"
        Me.lblAlerta.Size = New System.Drawing.Size(560, 70)
        Me.lblAlerta.TabIndex = 9
        '
        ' lblNota
        '
        Me.lblNota.ForeColor = System.Drawing.Color.DimGray
        Me.lblNota.Location = New System.Drawing.Point(20, 450)
        Me.lblNota.Name = "lblNota"
        Me.lblNota.Size = New System.Drawing.Size(560, 120)
        Me.lblNota.TabIndex = 10
        Me.lblNota.Text = "La debida diligencia es obligatoria. Si el cliente es de riesgo Alto, el sistema alerta cuando falta la verificación de origen de fondos o cuando la verificación de listas tiene más de un año."
        '
        ' FrmClientes
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 720)
        Me.Controls.Add(Me.tabPrincipal)
        Me.Controls.Add(Me.dgvClientes)
        Me.Controls.Add(Me.lblMensaje)
        Me.Controls.Add(Me.btnRefrescar)
        Me.Controls.Add(Me.btnEntidades)
        Me.Controls.Add(Me.btnDesactivar)
        Me.Controls.Add(Me.btnGuardar)
        Me.Controls.Add(Me.btnNuevo)
        Me.Controls.Add(Me.btnBuscar)
        Me.Controls.Add(Me.txtBuscar)
        Me.Controls.Add(Me.lblBuscar)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1100, 680)
        Me.Name = "FrmClientes"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Clientes y debida diligencia (LDA/FT)"
        CType(Me.dgvClientes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabPrincipal.ResumeLayout(False)
        Me.tabPersonales.ResumeLayout(False)
        Me.tabPersonales.PerformLayout()
        Me.tabLdaft.ResumeLayout(False)
        Me.tabLdaft.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblBuscar As System.Windows.Forms.Label
    Friend WithEvents txtBuscar As System.Windows.Forms.TextBox
    Friend WithEvents btnBuscar As System.Windows.Forms.Button
    Friend WithEvents btnNuevo As System.Windows.Forms.Button
    Friend WithEvents btnGuardar As System.Windows.Forms.Button
    Friend WithEvents btnDesactivar As System.Windows.Forms.Button
    Friend WithEvents btnEntidades As System.Windows.Forms.Button
    Friend WithEvents btnRefrescar As System.Windows.Forms.Button
    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents dgvClientes As System.Windows.Forms.DataGridView
    Friend WithEvents tabPrincipal As System.Windows.Forms.TabControl
    Friend WithEvents tabPersonales As System.Windows.Forms.TabPage
    Friend WithEvents lblNombres As System.Windows.Forms.Label
    Friend WithEvents txtNombres As System.Windows.Forms.TextBox
    Friend WithEvents lblApellidos As System.Windows.Forms.Label
    Friend WithEvents txtApellidos As System.Windows.Forms.TextBox
    Friend WithEvents lblDUI As System.Windows.Forms.Label
    Friend WithEvents txtDUI As System.Windows.Forms.TextBox
    Friend WithEvents lblNIT As System.Windows.Forms.Label
    Friend WithEvents txtNIT As System.Windows.Forms.TextBox
    Friend WithEvents lblNacionalidad As System.Windows.Forms.Label
    Friend WithEvents txtNacionalidad As System.Windows.Forms.TextBox
    Friend WithEvents lblProfesion As System.Windows.Forms.Label
    Friend WithEvents txtProfesion As System.Windows.Forms.TextBox
    Friend WithEvents lblNacimiento As System.Windows.Forms.Label
    Friend WithEvents dtpNacimiento As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblDireccion As System.Windows.Forms.Label
    Friend WithEvents txtDireccion As System.Windows.Forms.TextBox
    Friend WithEvents lblTelefono As System.Windows.Forms.Label
    Friend WithEvents txtTelefono As System.Windows.Forms.TextBox
    Friend WithEvents lblEmail As System.Windows.Forms.Label
    Friend WithEvents txtEmail As System.Windows.Forms.TextBox
    Friend WithEvents chkActivo As System.Windows.Forms.CheckBox
    Friend WithEvents lblAyuda As System.Windows.Forms.Label
    Friend WithEvents tabLdaft As System.Windows.Forms.TabPage
    Friend WithEvents lblNivelRiesgo As System.Windows.Forms.Label
    Friend WithEvents cboNivelRiesgo As System.Windows.Forms.ComboBox
    Friend WithEvents chkOrigenVerificado As System.Windows.Forms.CheckBox
    Friend WithEvents lblDetalleOrigen As System.Windows.Forms.Label
    Friend WithEvents txtDetalleOrigen As System.Windows.Forms.TextBox
    Friend WithEvents lblVerificacionListas As System.Windows.Forms.Label
    Friend WithEvents dtpVerificacionListas As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblObservacionesLdaft As System.Windows.Forms.Label
    Friend WithEvents txtObservaciones As System.Windows.Forms.TextBox
    Friend WithEvents lblAlerta As System.Windows.Forms.Label
    Friend WithEvents lblNota As System.Windows.Forms.Label
End Class
