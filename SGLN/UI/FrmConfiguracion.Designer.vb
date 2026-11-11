<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmConfiguracion
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
        Me.tabConfiguracion = New System.Windows.Forms.TabPage()
        Me.dgvConfig = New System.Windows.Forms.DataGridView()
        Me.grpEditor = New System.Windows.Forms.GroupBox()
        Me.lblClave = New System.Windows.Forms.Label()
        Me.txtClave = New System.Windows.Forms.TextBox()
        Me.lblValor = New System.Windows.Forms.Label()
        Me.txtValor = New System.Windows.Forms.TextBox()
        Me.btnGuardarConfig = New System.Windows.Forms.Button()
        Me.btnNuevaClaveConfig = New System.Windows.Forms.Button()
        Me.lblNotaConfig = New System.Windows.Forms.Label()
        Me.tabFeriados = New System.Windows.Forms.TabPage()
        Me.dgvFeriados = New System.Windows.Forms.DataGridView()
        Me.grpFeriado = New System.Windows.Forms.GroupBox()
        Me.lblFechaFeriado = New System.Windows.Forms.Label()
        Me.dtpFeriado = New System.Windows.Forms.DateTimePicker()
        Me.lblDescripcionFeriado = New System.Windows.Forms.Label()
        Me.txtDescripcionFeriado = New System.Windows.Forms.TextBox()
        Me.btnAgregarFeriado = New System.Windows.Forms.Button()
        Me.btnEliminarFeriado = New System.Windows.Forms.Button()
        Me.lblNotaFeriados = New System.Windows.Forms.Label()
        Me.tabSeguridad = New System.Windows.Forms.TabPage()
        Me.grpClave = New System.Windows.Forms.GroupBox()
        Me.lblUsuarioSeguridad = New System.Windows.Forms.Label()
        Me.lblClaveActual = New System.Windows.Forms.Label()
        Me.txtClaveActual = New System.Windows.Forms.TextBox()
        Me.lblClaveNueva = New System.Windows.Forms.Label()
        Me.txtClaveNueva = New System.Windows.Forms.TextBox()
        Me.lblConfirmarClave = New System.Windows.Forms.Label()
        Me.txtConfirmarClave = New System.Windows.Forms.TextBox()
        Me.btnCambiarClave = New System.Windows.Forms.Button()
        Me.lblNotaSeguridad = New System.Windows.Forms.Label()
        Me.tabRespaldos = New System.Windows.Forms.TabPage()
        Me.lblCarpetaRespaldos = New System.Windows.Forms.Label()
        Me.btnRespaldar = New System.Windows.Forms.Button()
        Me.btnAbrirCarpeta = New System.Windows.Forms.Button()
        Me.lblResultadoRespaldo = New System.Windows.Forms.Label()
        Me.lblNotaRespaldos = New System.Windows.Forms.Label()
        Me.tabPrincipal.SuspendLayout()
        Me.tabConfiguracion.SuspendLayout()
        CType(Me.dgvConfig, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpEditor.SuspendLayout()
        Me.tabFeriados.SuspendLayout()
        CType(Me.dgvFeriados, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpFeriado.SuspendLayout()
        Me.tabSeguridad.SuspendLayout()
        Me.grpClave.SuspendLayout()
        Me.tabRespaldos.SuspendLayout()
        Me.SuspendLayout()
        '
        ' lblMensaje
        '
        Me.lblMensaje.Location = New System.Drawing.Point(12, 8)
        Me.lblMensaje.Name = "lblMensaje"
        Me.lblMensaje.Size = New System.Drawing.Size(1160, 35)
        Me.lblMensaje.TabIndex = 0
        '
        ' tabPrincipal
        '
        Me.tabPrincipal.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabPrincipal.Controls.Add(Me.tabConfiguracion)
        Me.tabPrincipal.Controls.Add(Me.tabFeriados)
        Me.tabPrincipal.Controls.Add(Me.tabSeguridad)
        Me.tabPrincipal.Controls.Add(Me.tabRespaldos)
        Me.tabPrincipal.Location = New System.Drawing.Point(12, 50)
        Me.tabPrincipal.Name = "tabPrincipal"
        Me.tabPrincipal.SelectedIndex = 0
        Me.tabPrincipal.Size = New System.Drawing.Size(1160, 660)
        Me.tabPrincipal.TabIndex = 1
        '
        ' tabConfiguracion
        '
        Me.tabConfiguracion.Controls.Add(Me.dgvConfig)
        Me.tabConfiguracion.Controls.Add(Me.grpEditor)
        Me.tabConfiguracion.Location = New System.Drawing.Point(4, 24)
        Me.tabConfiguracion.Name = "tabConfiguracion"
        Me.tabConfiguracion.Padding = New System.Windows.Forms.Padding(3)
        Me.tabConfiguracion.Size = New System.Drawing.Size(1152, 632)
        Me.tabConfiguracion.TabIndex = 0
        Me.tabConfiguracion.Text = "Configuración"
        Me.tabConfiguracion.UseVisualStyleBackColor = True
        '
        ' dgvConfig
        '
        Me.dgvConfig.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvConfig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvConfig.Location = New System.Drawing.Point(10, 10)
        Me.dgvConfig.Name = "dgvConfig"
        Me.dgvConfig.Size = New System.Drawing.Size(700, 610)
        Me.dgvConfig.TabIndex = 0
        '
        ' grpEditor
        '
        Me.grpEditor.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpEditor.Controls.Add(Me.lblClave)
        Me.grpEditor.Controls.Add(Me.txtClave)
        Me.grpEditor.Controls.Add(Me.lblValor)
        Me.grpEditor.Controls.Add(Me.txtValor)
        Me.grpEditor.Controls.Add(Me.btnGuardarConfig)
        Me.grpEditor.Controls.Add(Me.btnNuevaClaveConfig)
        Me.grpEditor.Controls.Add(Me.lblNotaConfig)
        Me.grpEditor.Location = New System.Drawing.Point(725, 10)
        Me.grpEditor.Name = "grpEditor"
        Me.grpEditor.Size = New System.Drawing.Size(415, 260)
        Me.grpEditor.TabIndex = 1
        Me.grpEditor.TabStop = False
        Me.grpEditor.Text = "Editar valor"
        '
        ' lblClave
        '
        Me.lblClave.AutoSize = True
        Me.lblClave.Location = New System.Drawing.Point(15, 22)
        Me.lblClave.Name = "lblClave"
        Me.lblClave.Size = New System.Drawing.Size(40, 15)
        Me.lblClave.TabIndex = 0
        Me.lblClave.Text = "Clave:"
        '
        ' txtClave
        '
        Me.txtClave.Location = New System.Drawing.Point(15, 42)
        Me.txtClave.Name = "txtClave"
        Me.txtClave.Size = New System.Drawing.Size(385, 23)
        Me.txtClave.TabIndex = 0
        '
        ' lblValor
        '
        Me.lblValor.AutoSize = True
        Me.lblValor.Location = New System.Drawing.Point(15, 78)
        Me.lblValor.Name = "lblValor"
        Me.lblValor.Size = New System.Drawing.Size(36, 15)
        Me.lblValor.TabIndex = 2
        Me.lblValor.Text = "Valor:"
        '
        ' txtValor
        '
        Me.txtValor.Location = New System.Drawing.Point(15, 98)
        Me.txtValor.Multiline = True
        Me.txtValor.Name = "txtValor"
        Me.txtValor.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtValor.Size = New System.Drawing.Size(385, 55)
        Me.txtValor.TabIndex = 1
        '
        ' btnGuardarConfig
        '
        Me.btnGuardarConfig.Location = New System.Drawing.Point(15, 170)
        Me.btnGuardarConfig.Name = "btnGuardarConfig"
        Me.btnGuardarConfig.Size = New System.Drawing.Size(150, 30)
        Me.btnGuardarConfig.TabIndex = 2
        Me.btnGuardarConfig.Text = "Guardar"
        Me.btnGuardarConfig.UseVisualStyleBackColor = True
        '
        ' btnNuevaClaveConfig
        '
        Me.btnNuevaClaveConfig.Location = New System.Drawing.Point(175, 170)
        Me.btnNuevaClaveConfig.Name = "btnNuevaClaveConfig"
        Me.btnNuevaClaveConfig.Size = New System.Drawing.Size(150, 30)
        Me.btnNuevaClaveConfig.TabIndex = 3
        Me.btnNuevaClaveConfig.Text = "Nueva clave"
        Me.btnNuevaClaveConfig.UseVisualStyleBackColor = True
        '
        ' lblNotaConfig
        '
        Me.lblNotaConfig.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaConfig.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaConfig.Location = New System.Drawing.Point(15, 208)
        Me.lblNotaConfig.Name = "lblNotaConfig"
        Me.lblNotaConfig.Size = New System.Drawing.Size(385, 45)
        Me.lblNotaConfig.TabIndex = 7
        Me.lblNotaConfig.Text = "Las claves Carpeta_Documentos y Carpeta_Respaldos controlan dónde se guardan los archivos."
        '
        ' tabFeriados
        '
        Me.tabFeriados.Controls.Add(Me.dgvFeriados)
        Me.tabFeriados.Controls.Add(Me.grpFeriado)
        Me.tabFeriados.Location = New System.Drawing.Point(4, 24)
        Me.tabFeriados.Name = "tabFeriados"
        Me.tabFeriados.Padding = New System.Windows.Forms.Padding(3)
        Me.tabFeriados.Size = New System.Drawing.Size(1152, 632)
        Me.tabFeriados.TabIndex = 1
        Me.tabFeriados.Text = "Feriados"
        Me.tabFeriados.UseVisualStyleBackColor = True
        '
        ' dgvFeriados
        '
        Me.dgvFeriados.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvFeriados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvFeriados.Location = New System.Drawing.Point(10, 10)
        Me.dgvFeriados.Name = "dgvFeriados"
        Me.dgvFeriados.Size = New System.Drawing.Size(700, 610)
        Me.dgvFeriados.TabIndex = 0
        '
        ' grpFeriado
        '
        Me.grpFeriado.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpFeriado.Controls.Add(Me.lblFechaFeriado)
        Me.grpFeriado.Controls.Add(Me.dtpFeriado)
        Me.grpFeriado.Controls.Add(Me.lblDescripcionFeriado)
        Me.grpFeriado.Controls.Add(Me.txtDescripcionFeriado)
        Me.grpFeriado.Controls.Add(Me.btnAgregarFeriado)
        Me.grpFeriado.Controls.Add(Me.btnEliminarFeriado)
        Me.grpFeriado.Controls.Add(Me.lblNotaFeriados)
        Me.grpFeriado.Location = New System.Drawing.Point(725, 10)
        Me.grpFeriado.Name = "grpFeriado"
        Me.grpFeriado.Size = New System.Drawing.Size(415, 240)
        Me.grpFeriado.TabIndex = 1
        Me.grpFeriado.TabStop = False
        Me.grpFeriado.Text = "Agregar feriado"
        '
        ' lblFechaFeriado
        '
        Me.lblFechaFeriado.AutoSize = True
        Me.lblFechaFeriado.Location = New System.Drawing.Point(15, 22)
        Me.lblFechaFeriado.Name = "lblFechaFeriado"
        Me.lblFechaFeriado.Size = New System.Drawing.Size(40, 15)
        Me.lblFechaFeriado.TabIndex = 0
        Me.lblFechaFeriado.Text = "Fecha:"
        '
        ' dtpFeriado
        '
        Me.dtpFeriado.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.dtpFeriado.Location = New System.Drawing.Point(15, 42)
        Me.dtpFeriado.Name = "dtpFeriado"
        Me.dtpFeriado.Size = New System.Drawing.Size(150, 23)
        Me.dtpFeriado.TabIndex = 0
        '
        ' lblDescripcionFeriado
        '
        Me.lblDescripcionFeriado.AutoSize = True
        Me.lblDescripcionFeriado.Location = New System.Drawing.Point(15, 78)
        Me.lblDescripcionFeriado.Name = "lblDescripcionFeriado"
        Me.lblDescripcionFeriado.Size = New System.Drawing.Size(72, 15)
        Me.lblDescripcionFeriado.TabIndex = 2
        Me.lblDescripcionFeriado.Text = "Descripción:"
        '
        ' txtDescripcionFeriado
        '
        Me.txtDescripcionFeriado.Location = New System.Drawing.Point(15, 98)
        Me.txtDescripcionFeriado.Name = "txtDescripcionFeriado"
        Me.txtDescripcionFeriado.Size = New System.Drawing.Size(385, 23)
        Me.txtDescripcionFeriado.TabIndex = 1
        '
        ' btnAgregarFeriado
        '
        Me.btnAgregarFeriado.Location = New System.Drawing.Point(15, 145)
        Me.btnAgregarFeriado.Name = "btnAgregarFeriado"
        Me.btnAgregarFeriado.Size = New System.Drawing.Size(180, 30)
        Me.btnAgregarFeriado.TabIndex = 2
        Me.btnAgregarFeriado.Text = "Agregar feriado"
        Me.btnAgregarFeriado.UseVisualStyleBackColor = True
        '
        ' btnEliminarFeriado
        '
        Me.btnEliminarFeriado.Location = New System.Drawing.Point(205, 145)
        Me.btnEliminarFeriado.Name = "btnEliminarFeriado"
        Me.btnEliminarFeriado.Size = New System.Drawing.Size(195, 30)
        Me.btnEliminarFeriado.TabIndex = 3
        Me.btnEliminarFeriado.Text = "Eliminar seleccionado"
        Me.btnEliminarFeriado.UseVisualStyleBackColor = True
        '
        ' lblNotaFeriados
        '
        Me.lblNotaFeriados.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaFeriados.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaFeriados.Location = New System.Drawing.Point(15, 185)
        Me.lblNotaFeriados.Name = "lblNotaFeriados"
        Me.lblNotaFeriados.Size = New System.Drawing.Size(385, 45)
        Me.lblNotaFeriados.TabIndex = 7
        Me.lblNotaFeriados.Text = "Los feriados afectan el cálculo de días hábiles (Art. 145 CPCM). Revise el calendario oficial cada año."
        '
        ' tabSeguridad
        '
        Me.tabSeguridad.Controls.Add(Me.grpClave)
        Me.tabSeguridad.Location = New System.Drawing.Point(4, 24)
        Me.tabSeguridad.Name = "tabSeguridad"
        Me.tabSeguridad.Padding = New System.Windows.Forms.Padding(3)
        Me.tabSeguridad.Size = New System.Drawing.Size(1152, 632)
        Me.tabSeguridad.TabIndex = 2
        Me.tabSeguridad.Text = "Seguridad"
        Me.tabSeguridad.UseVisualStyleBackColor = True
        '
        ' grpClave
        '
        Me.grpClave.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.grpClave.Controls.Add(Me.lblUsuarioSeguridad)
        Me.grpClave.Controls.Add(Me.lblClaveActual)
        Me.grpClave.Controls.Add(Me.txtClaveActual)
        Me.grpClave.Controls.Add(Me.lblClaveNueva)
        Me.grpClave.Controls.Add(Me.txtClaveNueva)
        Me.grpClave.Controls.Add(Me.lblConfirmarClave)
        Me.grpClave.Controls.Add(Me.txtConfirmarClave)
        Me.grpClave.Controls.Add(Me.btnCambiarClave)
        Me.grpClave.Controls.Add(Me.lblNotaSeguridad)
        Me.grpClave.Location = New System.Drawing.Point(10, 10)
        Me.grpClave.Name = "grpClave"
        Me.grpClave.Size = New System.Drawing.Size(415, 300)
        Me.grpClave.TabIndex = 1
        Me.grpClave.TabStop = False
        Me.grpClave.Text = "Cambio de clave"
        '
        ' lblUsuarioSeguridad
        '
        Me.lblUsuarioSeguridad.AutoSize = True
        Me.lblUsuarioSeguridad.Location = New System.Drawing.Point(15, 22)
        Me.lblUsuarioSeguridad.Name = "lblUsuarioSeguridad"
        Me.lblUsuarioSeguridad.Size = New System.Drawing.Size(50, 15)
        Me.lblUsuarioSeguridad.TabIndex = 0
        Me.lblUsuarioSeguridad.Text = "Usuario: -"
        '
        ' lblClaveActual
        '
        Me.lblClaveActual.AutoSize = True
        Me.lblClaveActual.Location = New System.Drawing.Point(15, 78)
        Me.lblClaveActual.Name = "lblClaveActual"
        Me.lblClaveActual.Size = New System.Drawing.Size(75, 15)
        Me.lblClaveActual.TabIndex = 2
        Me.lblClaveActual.Text = "Clave actual:"
        '
        ' txtClaveActual
        '
        Me.txtClaveActual.Location = New System.Drawing.Point(15, 98)
        Me.txtClaveActual.Name = "txtClaveActual"
        Me.txtClaveActual.Size = New System.Drawing.Size(385, 23)
        Me.txtClaveActual.TabIndex = 1
        Me.txtClaveActual.UseSystemPasswordChar = True
        '
        ' lblClaveNueva
        '
        Me.lblClaveNueva.AutoSize = True
        Me.lblClaveNueva.Location = New System.Drawing.Point(15, 135)
        Me.lblClaveNueva.Name = "lblClaveNueva"
        Me.lblClaveNueva.Size = New System.Drawing.Size(75, 15)
        Me.lblClaveNueva.TabIndex = 4
        Me.lblClaveNueva.Text = "Clave nueva:"
        '
        ' txtClaveNueva
        '
        Me.txtClaveNueva.Location = New System.Drawing.Point(15, 155)
        Me.txtClaveNueva.Name = "txtClaveNueva"
        Me.txtClaveNueva.Size = New System.Drawing.Size(385, 23)
        Me.txtClaveNueva.TabIndex = 2
        Me.txtClaveNueva.UseSystemPasswordChar = True
        '
        ' lblConfirmarClave
        '
        Me.lblConfirmarClave.AutoSize = True
        Me.lblConfirmarClave.Location = New System.Drawing.Point(15, 192)
        Me.lblConfirmarClave.Name = "lblConfirmarClave"
        Me.lblConfirmarClave.Size = New System.Drawing.Size(130, 15)
        Me.lblConfirmarClave.TabIndex = 6
        Me.lblConfirmarClave.Text = "Confirmar clave nueva:"
        '
        ' txtConfirmarClave
        '
        Me.txtConfirmarClave.Location = New System.Drawing.Point(15, 212)
        Me.txtConfirmarClave.Name = "txtConfirmarClave"
        Me.txtConfirmarClave.Size = New System.Drawing.Size(385, 23)
        Me.txtConfirmarClave.TabIndex = 3
        Me.txtConfirmarClave.UseSystemPasswordChar = True
        '
        ' btnCambiarClave
        '
        Me.btnCambiarClave.Location = New System.Drawing.Point(15, 250)
        Me.btnCambiarClave.Name = "btnCambiarClave"
        Me.btnCambiarClave.Size = New System.Drawing.Size(190, 30)
        Me.btnCambiarClave.TabIndex = 4
        Me.btnCambiarClave.Text = "Cambiar clave"
        Me.btnCambiarClave.UseVisualStyleBackColor = True
        '
        ' lblNotaSeguridad
        '
        Me.lblNotaSeguridad.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaSeguridad.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaSeguridad.Location = New System.Drawing.Point(220, 250)
        Me.lblNotaSeguridad.Name = "lblNotaSeguridad"
        Me.lblNotaSeguridad.Size = New System.Drawing.Size(180, 45)
        Me.lblNotaSeguridad.TabIndex = 9
        Me.lblNotaSeguridad.Text = "Mínimo 8 caracteres."
        '
        ' tabRespaldos
        '
        Me.tabRespaldos.Controls.Add(Me.lblCarpetaRespaldos)
        Me.tabRespaldos.Controls.Add(Me.btnRespaldar)
        Me.tabRespaldos.Controls.Add(Me.btnAbrirCarpeta)
        Me.tabRespaldos.Controls.Add(Me.lblResultadoRespaldo)
        Me.tabRespaldos.Controls.Add(Me.lblNotaRespaldos)
        Me.tabRespaldos.Location = New System.Drawing.Point(4, 24)
        Me.tabRespaldos.Name = "tabRespaldos"
        Me.tabRespaldos.Padding = New System.Windows.Forms.Padding(3)
        Me.tabRespaldos.Size = New System.Drawing.Size(1152, 632)
        Me.tabRespaldos.TabIndex = 3
        Me.tabRespaldos.Text = "Respaldos"
        Me.tabRespaldos.UseVisualStyleBackColor = True
        '
        ' lblCarpetaRespaldos
        '
        Me.lblCarpetaRespaldos.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblCarpetaRespaldos.Location = New System.Drawing.Point(20, 20)
        Me.lblCarpetaRespaldos.Name = "lblCarpetaRespaldos"
        Me.lblCarpetaRespaldos.Size = New System.Drawing.Size(1100, 25)
        Me.lblCarpetaRespaldos.TabIndex = 0
        '
        ' btnRespaldar
        '
        Me.btnRespaldar.Location = New System.Drawing.Point(20, 60)
        Me.btnRespaldar.Name = "btnRespaldar"
        Me.btnRespaldar.Size = New System.Drawing.Size(220, 35)
        Me.btnRespaldar.TabIndex = 1
        Me.btnRespaldar.Text = "Crear respaldo ahora"
        Me.btnRespaldar.UseVisualStyleBackColor = True
        '
        ' btnAbrirCarpeta
        '
        Me.btnAbrirCarpeta.Location = New System.Drawing.Point(250, 60)
        Me.btnAbrirCarpeta.Name = "btnAbrirCarpeta"
        Me.btnAbrirCarpeta.Size = New System.Drawing.Size(220, 35)
        Me.btnAbrirCarpeta.TabIndex = 2
        Me.btnAbrirCarpeta.Text = "Abrir carpeta de respaldos"
        Me.btnAbrirCarpeta.UseVisualStyleBackColor = True
        '
        ' lblResultadoRespaldo
        '
        Me.lblResultadoRespaldo.Location = New System.Drawing.Point(20, 115)
        Me.lblResultadoRespaldo.Name = "lblResultadoRespaldo"
        Me.lblResultadoRespaldo.Size = New System.Drawing.Size(1100, 60)
        Me.lblResultadoRespaldo.TabIndex = 3
        '
        ' lblNotaRespaldos
        '
        Me.lblNotaRespaldos.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Italic)
        Me.lblNotaRespaldos.ForeColor = System.Drawing.Color.DimGray
        Me.lblNotaRespaldos.Location = New System.Drawing.Point(20, 190)
        Me.lblNotaRespaldos.Name = "lblNotaRespaldos"
        Me.lblNotaRespaldos.Size = New System.Drawing.Size(1100, 120)
        Me.lblNotaRespaldos.TabIndex = 4
        Me.lblNotaRespaldos.Text = "El respaldo se crea con BACKUP DATABASE de SQL Server. La cuenta del servicio SQL debe tener permiso de escritura sobre la carpeta. Para restaurar, use SQL Server Management Studio o el comando RESTORE DATABASE; la restauración no se hace desde esta aplicación para evitar sobrescribir datos por error."
        '
        ' FrmConfiguracion
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1185, 720)
        Me.Controls.Add(Me.tabPrincipal)
        Me.Controls.Add(Me.lblMensaje)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.MinimumSize = New System.Drawing.Size(1150, 690)
        Me.Name = "FrmConfiguracion"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Configuración, feriados y respaldos"
        Me.tabPrincipal.ResumeLayout(False)
        Me.tabConfiguracion.ResumeLayout(False)
        CType(Me.dgvConfig, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpEditor.ResumeLayout(False)
        Me.grpEditor.PerformLayout()
        Me.tabFeriados.ResumeLayout(False)
        CType(Me.dgvFeriados, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpFeriado.ResumeLayout(False)
        Me.grpFeriado.PerformLayout()
        Me.tabSeguridad.ResumeLayout(False)
        Me.grpClave.ResumeLayout(False)
        Me.grpClave.PerformLayout()
        Me.tabRespaldos.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents lblMensaje As System.Windows.Forms.Label
    Friend WithEvents tabPrincipal As System.Windows.Forms.TabControl
    Friend WithEvents tabConfiguracion As System.Windows.Forms.TabPage
    Friend WithEvents dgvConfig As System.Windows.Forms.DataGridView
    Friend WithEvents grpEditor As System.Windows.Forms.GroupBox
    Friend WithEvents lblClave As System.Windows.Forms.Label
    Friend WithEvents txtClave As System.Windows.Forms.TextBox
    Friend WithEvents lblValor As System.Windows.Forms.Label
    Friend WithEvents txtValor As System.Windows.Forms.TextBox
    Friend WithEvents btnGuardarConfig As System.Windows.Forms.Button
    Friend WithEvents btnNuevaClaveConfig As System.Windows.Forms.Button
    Friend WithEvents lblNotaConfig As System.Windows.Forms.Label
    Friend WithEvents tabFeriados As System.Windows.Forms.TabPage
    Friend WithEvents dgvFeriados As System.Windows.Forms.DataGridView
    Friend WithEvents grpFeriado As System.Windows.Forms.GroupBox
    Friend WithEvents lblFechaFeriado As System.Windows.Forms.Label
    Friend WithEvents dtpFeriado As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblDescripcionFeriado As System.Windows.Forms.Label
    Friend WithEvents txtDescripcionFeriado As System.Windows.Forms.TextBox
    Friend WithEvents btnAgregarFeriado As System.Windows.Forms.Button
    Friend WithEvents btnEliminarFeriado As System.Windows.Forms.Button
    Friend WithEvents lblNotaFeriados As System.Windows.Forms.Label
    Friend WithEvents tabSeguridad As System.Windows.Forms.TabPage
    Friend WithEvents grpClave As System.Windows.Forms.GroupBox
    Friend WithEvents lblUsuarioSeguridad As System.Windows.Forms.Label
    Friend WithEvents lblClaveActual As System.Windows.Forms.Label
    Friend WithEvents txtClaveActual As System.Windows.Forms.TextBox
    Friend WithEvents lblClaveNueva As System.Windows.Forms.Label
    Friend WithEvents txtClaveNueva As System.Windows.Forms.TextBox
    Friend WithEvents lblConfirmarClave As System.Windows.Forms.Label
    Friend WithEvents txtConfirmarClave As System.Windows.Forms.TextBox
    Friend WithEvents btnCambiarClave As System.Windows.Forms.Button
    Friend WithEvents lblNotaSeguridad As System.Windows.Forms.Label
    Friend WithEvents tabRespaldos As System.Windows.Forms.TabPage
    Friend WithEvents lblCarpetaRespaldos As System.Windows.Forms.Label
    Friend WithEvents btnRespaldar As System.Windows.Forms.Button
    Friend WithEvents btnAbrirCarpeta As System.Windows.Forms.Button
    Friend WithEvents lblResultadoRespaldo As System.Windows.Forms.Label
    Friend WithEvents lblNotaRespaldos As System.Windows.Forms.Label
End Class
