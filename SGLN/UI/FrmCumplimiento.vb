Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmCumplimiento

    Private ReadOnly _servicio As New ServicioCumplimiento()
    Private _cargando As Boolean
    Private _idOperacion As Integer
    Private _idSolicitud As Integer
    Private _umbral As Decimal

    ''' <summary>Solicitud ARCO que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDSolicitudInicial As Integer
    ''' <summary>Operación UIF que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDOperacionInicial As Integer

    Private Sub FrmCumplimiento_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim puedeVer As Boolean = Permisos.TieneAcceso("cumplimiento.ver")
        Dim puedeOperaciones As Boolean = Permisos.TieneAcceso("cumplimiento.operaciones")
        Dim puedeReportar As Boolean = Permisos.TieneAcceso("cumplimiento.reportar_uif")
        Dim puedeArco As Boolean = Permisos.TieneAcceso("cumplimiento.arco")
        Dim puedeVerOperaciones As Boolean = puedeVer OrElse puedeOperaciones OrElse puedeReportar
        Dim puedeVerArco As Boolean = puedeVer OrElse puedeArco

        If Not (puedeVerOperaciones OrElse puedeVerArco) Then
            If Not Permisos.Exigir("cumplimiento.ver", Me) Then Return
        End If

        btnBuscarOp.Enabled = puedeVerOperaciones
        btnRefrescarOp.Enabled = puedeVerOperaciones
        btnNuevoOp.Enabled = puedeOperaciones
        btnGuardarOp.Enabled = puedeOperaciones
        btnDescartarOp.Enabled = puedeOperaciones
        btnReportarOp.Enabled = puedeReportar

        ' Un rol de consulta no debe poder modificar campos aunque el servicio
        ' vuelva a validar la autorización al guardar.
        cboTipoOperacion.Enabled = puedeOperaciones
        nudMonto.Enabled = puedeOperaciones
        cboMoneda.Enabled = puedeOperaciones
        dtpFechaOperacion.Enabled = puedeOperaciones
        cboCasoOp.Enabled = puedeOperaciones
        cboInstrumentoOp.Enabled = puedeOperaciones
        txtDescripcionOp.ReadOnly = Not puedeOperaciones
        txtObservacionesOp.ReadOnly = Not puedeOperaciones

        btnBuscarArco.Enabled = puedeVerArco
        btnRefrescarArco.Enabled = puedeVerArco
        btnNuevoArco.Enabled = puedeArco
        btnGuardarArco.Enabled = puedeArco
        btnResponderArco.Enabled = puedeArco
        grpArco.Enabled = puedeArco

        ' No mostrar pestañas que el usuario no puede consultar.
        ' La autorización real sigue validándose en ServicioCumplimiento.
        If Not puedeVerOperaciones Then tabPrincipal.TabPages.Remove(tabOperaciones)
        If Not puedeVerArco Then tabPrincipal.TabPages.Remove(tabArco)

        UtileriasUI.ConfigurarGrid(dgvOperaciones)
        UtileriasUI.ConfigurarGrid(dgvArco)

        ' Limitar la captura en pantalla a los mismos máximos que valida el servicio.
        txtDescripcionOp.MaxLength = 1000
        txtObservacionesOp.MaxLength = 500
        txtDetalleArco.MaxLength = 1000

        cboTipoOperacion.Items.Clear()
        cboTipoOperacion.Items.AddRange(New Object() {"Regulada", "Inusual", "Sospechosa"})
        cboTipoOperacion.SelectedIndex = 0

        cboEstadoOperacion.Items.Clear()
        cboEstadoOperacion.Items.AddRange(New Object() {"Registrada", "En análisis", "Reportada", "Descartada"})
        cboEstadoOperacion.SelectedIndex = 0
        ' El estado cambia únicamente mediante acciones controladas (reportar/descartar).
        cboEstadoOperacion.Enabled = False

        cboMoneda.Items.Clear()
        cboMoneda.Items.AddRange(New Object() {"USD"})
        cboMoneda.SelectedIndex = 0

        cboTipoDerecho.Items.Clear()
        cboTipoDerecho.Items.AddRange(New Object() {"Acceso", "Rectificación", "Cancelación", "Oposición"})
        cboTipoDerecho.SelectedIndex = 0

        cboEstadoArco.Items.Clear()
        cboEstadoArco.Items.AddRange(New Object() {"Pendiente", "Vencida", "Respondida", "Revisar prórroga"})
        cboEstadoArco.SelectedIndex = 0
        ' El estado ARCO se controla desde las acciones del servicio, no desde el formulario.
        cboEstadoArco.Enabled = False

        _cargando = True
        CargarCombos()
        If puedeVerOperaciones Then CargarOperaciones()
        If puedeVerArco Then CargarSolicitudes()
        _cargando = False

        _umbral = _servicio.UmbralEfectivo()
        lblNotaUmbral.Text = "Umbral de efectivo configurado: $" & _umbral.ToString("N2") &
            ". Los plazos de análisis y reporte son configurables y deben validarse con el oficial de cumplimiento."

        LimpiarOperacion()
        LimpiarSolicitud()
        If IDOperacionInicial > 0 AndAlso puedeVerOperaciones Then
            tabPrincipal.SelectedTab = tabOperaciones
            SeleccionarOperacion(IDOperacionInicial)
            If _idOperacion <> IDOperacionInicial Then
                LimpiarOperacion()
                MostrarMensaje("La operación UIF asociada a la alerta ya no está disponible.", False)
            Else
                MostrarMensaje("", True)
            End If
        ElseIf IDSolicitudInicial > 0 AndAlso puedeVerArco Then
            tabPrincipal.SelectedTab = tabArco
            If SeleccionarSolicitud(IDSolicitudInicial) Then
                Dim filaInicial As DataRow = UtileriasUI.FilaActual(dgvArco)
                If filaInicial IsNot Nothing Then CargarSolicitud(filaInicial)
            Else
                LimpiarSolicitud()
                MostrarMensaje("La solicitud ARCO asociada a la alerta ya no está disponible.", False)
            End If
        Else
            MostrarMensaje("", True)
        End If
    End Sub

    Private Sub CargarCombos()
        Try
            UtileriasUI.CargarCombo(cboCasoOp, ConOpcionNinguno(_servicio.CasosCombo()), "Nombre", "ID")
            UtileriasUI.CargarCombo(cboInstrumentoOp, ConOpcionNinguno(_servicio.InstrumentosCombo()), "Nombre", "ID")
            UtileriasUI.CargarCombo(cboClienteArco, _servicio.ClientesCombo(), "Nombre", "ID")
        Catch ex As Exception
            cboCasoOp.DataSource = Nothing
            cboInstrumentoOp.DataSource = Nothing
            cboClienteArco.DataSource = Nothing
            RegistroErrores.Escribir("Cumplimiento - combos", ex)
            MostrarMensaje("No se pudieron cargar los catálogos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Function ConOpcionNinguno(origen As DataTable) As DataTable
        Dim tabla As DataTable = origen.Copy()
        Dim fila As DataRow = tabla.NewRow()
        fila("ID") = 0
        fila("Nombre") = "(Ninguno)"
        tabla.Rows.InsertAt(fila, 0)
        Return tabla
    End Function

    ' ------------------------------------------------------------------
    ' Operaciones LDA/FT
    ' ------------------------------------------------------------------

    Private Sub CargarOperaciones()
        Try
            _cargando = True
            dgvOperaciones.DataSource = _servicio.Operaciones(txtFiltroOp.Text)
            OcultarColumnas(dgvOperaciones, "ID", "ID_Caso", "ID_Instrumento")
            If dgvOperaciones.Columns.Contains("Monto") Then
                dgvOperaciones.Columns("Monto").DefaultCellStyle.Format = "N2"
            End If
            _cargando = False

            Dim actual As DataRow = UtileriasUI.FilaActual(dgvOperaciones)
            If actual IsNot Nothing Then
                CargarOperacion(actual)
            Else
                LimpiarOperacion()
            End If
        Catch ex As Exception
            _cargando = False
            dgvOperaciones.DataSource = Nothing
            LimpiarOperacion()
            RegistroErrores.Escribir("Cumplimiento - operaciones", ex)
            MostrarMensaje("No se pudieron cargar las operaciones de cumplimiento. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub dgvOperaciones_SelectionChanged(sender As Object, e As EventArgs) Handles dgvOperaciones.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvOperaciones)
        If fila Is Nothing Then Return
        CargarOperacion(fila)
    End Sub

    Private Sub CargarOperacion(fila As DataRow)
        _idOperacion = UtileriasUI.Entero(fila, "ID")

        SeleccionarTexto(cboTipoOperacion, UtileriasUI.Texto(fila, "Tipo_Operacion"))
        nudMonto.Value = Math.Min(UtileriasUI.DecimalDesde(fila, "Monto"), nudMonto.Maximum)
        SeleccionarTexto(cboMoneda, UtileriasUI.Texto(fila, "Moneda"))
        dtpFechaOperacion.Value = UtileriasUI.Fecha(fila, "Fecha_Operacion").GetValueOrDefault(Date.Today)
        SeleccionarTexto(cboEstadoOperacion, UtileriasUI.Texto(fila, "Estado"))
        UtileriasUI.SeleccionarValor(cboCasoOp, If(fila.IsNull("ID_Caso"), 0, fila("ID_Caso")))
        UtileriasUI.SeleccionarValor(cboInstrumentoOp, If(fila.IsNull("ID_Instrumento"), 0, fila("ID_Instrumento")))

        txtDescripcionOp.Text = UtileriasUI.Texto(fila, "Descripcion")
        txtObservacionesOp.Text = UtileriasUI.Texto(fila, "Observaciones")

        Dim analisis As Date? = UtileriasUI.Fecha(fila, "Fecha_Vencimiento_Analisis")
        Dim reporte As Date? = UtileriasUI.Fecha(fila, "Fecha_Vencimiento_Reporte")
        txtAnalisis.Text = If(analisis.HasValue, analisis.Value.ToString("dd/MM/yyyy"), "")
        txtReporte.Text = If(reporte.HasValue, reporte.Value.ToString("dd/MM/yyyy"), "")

        EvaluarUmbral()
        MostrarMensaje("", True)
    End Sub

    Private Sub LimpiarOperacion()
        _idOperacion = 0
        cboTipoOperacion.SelectedIndex = 0
        nudMonto.Value = 0
        cboMoneda.SelectedIndex = 0
        dtpFechaOperacion.Value = Date.Today
        cboEstadoOperacion.SelectedIndex = 0
        If cboCasoOp.Items.Count > 0 Then cboCasoOp.SelectedIndex = 0
        If cboInstrumentoOp.Items.Count > 0 Then cboInstrumentoOp.SelectedIndex = 0
        txtDescripcionOp.Clear()
        txtObservacionesOp.Clear()
        txtAnalisis.Clear()
        txtReporte.Clear()
        lblAlertaUmbral.Text = ""
        _cargando = True
        dgvOperaciones.CurrentCell = Nothing
        _cargando = False
    End Sub

    Private Sub GuardarOperacion()
        If Not Permisos.ExigirAccion("cumplimiento.operaciones") Then Return

        Try
            Dim operacion As New OperacionLdaft With {
                .ID = _idOperacion,
                .IDCaso = IdOpcional(cboCasoOp),
                .IDInstrumento = IdOpcional(cboInstrumentoOp),
                .TipoOperacion = Convert.ToString(cboTipoOperacion.SelectedItem),
                .Monto = nudMonto.Value,
                .Moneda = Convert.ToString(cboMoneda.SelectedItem),
                .FechaOperacion = dtpFechaOperacion.Value.Date,
                .Descripcion = txtDescripcionOp.Text,
                .Estado = Convert.ToString(cboEstadoOperacion.SelectedItem),
                .Observaciones = txtObservacionesOp.Text
            }

            Dim respuesta As Respuesta = _servicio.GuardarOperacion(operacion)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            CargarOperaciones()
            SeleccionarOperacion(operacion.ID)
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Cumplimiento - guardar operación", ex)
            MostrarMensaje("No se pudo guardar la operación de cumplimiento. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub ReportarOperacion()
        If Not Permisos.ExigirAccion("cumplimiento.reportar_uif") Then Return

        If _idOperacion <= 0 Then
            UtileriasUI.MostrarError("Seleccione una operación.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Registrar el reporte a la UIF con fecha de hoy?") Then Return

        Dim respuesta As Respuesta = _servicio.MarcarReportada(_idOperacion, Date.Today)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarOperaciones()
        SeleccionarOperacion(_idOperacion)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub DescartarOperacion()
        If Not Permisos.ExigirAccion("cumplimiento.operaciones") Then Return

        If _idOperacion <= 0 Then
            UtileriasUI.MostrarError("Seleccione una operación.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Descartar la operación seleccionada? El registro se conserva.") Then Return

        Dim respuesta As Respuesta = _servicio.Descartar(_idOperacion)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarOperaciones()
        SeleccionarOperacion(_idOperacion)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub EvaluarUmbral()
        lblAlertaUmbral.Text = ""
        lblAlertaUmbral.ForeColor = Color.Firebrick

        If nudMonto.Value < _umbral Then Return

        If Convert.ToString(cboTipoOperacion.SelectedItem) = "Regulada" Then
            lblAlertaUmbral.Text = "Supera el umbral de $" & _umbral.ToString("N2") & "."
        Else
            lblAlertaUmbral.Text = "Monto alto: revise el tipo de operación."
        End If
    End Sub

    Private Sub SeleccionarOperacion(id As Integer)
        For Each fila As DataGridViewRow In dgvOperaciones.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' La columna ID está oculta; seleccionar una celda visible y cargar el registro explícitamente.
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvOperaciones.CurrentCell = celda
                    CargarOperacion(vista.Row)
                    Return
                End If
            Next
            Return
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' Solicitudes ARCO
    ' ------------------------------------------------------------------

    Private Sub CargarSolicitudes()
        Try
            _cargando = True
            dgvArco.DataSource = _servicio.SolicitudesArco(txtFiltroArco.Text)
            OcultarColumnas(dgvArco, "ID", "ID_Cliente")
            _cargando = False

            Dim actual As DataRow = UtileriasUI.FilaActual(dgvArco)
            If actual IsNot Nothing Then
                CargarSolicitud(actual)
            Else
                LimpiarSolicitud()
            End If
        Catch ex As Exception
            _cargando = False
            dgvArco.DataSource = Nothing
            LimpiarSolicitud()
            RegistroErrores.Escribir("Cumplimiento - ARCO", ex)
            MostrarMensaje("No se pudieron cargar las solicitudes ARCO. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub dgvArco_SelectionChanged(sender As Object, e As EventArgs) Handles dgvArco.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvArco)
        If fila Is Nothing Then Return
        CargarSolicitud(fila)
    End Sub

    Private Sub CargarSolicitud(fila As DataRow)
        _idSolicitud = UtileriasUI.Entero(fila, "ID")

        ' La identidad del titular, el derecho y la fecha de recepción son
        ' datos históricos inmutables una vez registrada la solicitud.
        cboClienteArco.Enabled = False
        cboTipoDerecho.Enabled = False
        dtpFechaSolicitud.Enabled = False

        UtileriasUI.SeleccionarValor(cboClienteArco, fila("ID_Cliente"))
        SeleccionarTexto(cboTipoDerecho, UtileriasUI.Texto(fila, "Tipo_Derecho"))
        dtpFechaSolicitud.Value = UtileriasUI.Fecha(fila, "Fecha_Solicitud").GetValueOrDefault(Date.Today)
        SeleccionarTexto(cboEstadoArco, UtileriasUI.Texto(fila, "Estado"))

        Dim vencimiento As Date? = UtileriasUI.Fecha(fila, "Fecha_Vencimiento")
        txtVencimientoArco.Text = If(vencimiento.HasValue, vencimiento.Value.ToString("dd/MM/yyyy"), "")

        chkProrroga.Checked = UtileriasUI.Booleano(fila, "Prorroga_Aplicada")
        Dim prorroga As Date? = UtileriasUI.Fecha(fila, "Fecha_Vencimiento_Prorroga")
        txtVencimientoProrroga.Text = If(prorroga.HasValue, prorroga.Value.ToString("dd/MM/yyyy"), "")

        txtDetalleArco.Text = UtileriasUI.Texto(fila, "Detalle")

        Dim respuesta As Date? = UtileriasUI.Fecha(fila, "Fecha_Respuesta")
        UtileriasUI.AsignarFecha(dtpFechaRespuesta, respuesta)

        MostrarMensaje("", True)
    End Sub

    Private Sub LimpiarSolicitud()
        _idSolicitud = 0
        cboClienteArco.Enabled = True
        cboTipoDerecho.Enabled = True
        dtpFechaSolicitud.Enabled = True
        If cboClienteArco.Items.Count > 0 Then cboClienteArco.SelectedIndex = 0
        cboTipoDerecho.SelectedIndex = 0
        dtpFechaSolicitud.Value = Date.Today
        cboEstadoArco.SelectedIndex = 0
        chkProrroga.Checked = False
        dtpFechaRespuesta.Checked = False
        txtDetalleArco.Clear()
        ActualizarFechasArco()

        _cargando = True
        dgvArco.CurrentCell = Nothing
        _cargando = False
    End Sub

    Private Sub ActualizarFechasArco()
        If Not Permisos.TieneAcceso("cumplimiento.arco") Then
            txtVencimientoArco.Clear()
            txtVencimientoProrroga.Clear()
            Return
        End If

        Try
            Dim vencimiento As Date = _servicio.CalcularVencimientoArco(dtpFechaSolicitud.Value.Date)
            txtVencimientoArco.Text = vencimiento.ToString("dd/MM/yyyy")

            If chkProrroga.Checked Then
                txtVencimientoProrroga.Text = _servicio.CalcularProrrogaArco(vencimiento).ToString("dd/MM/yyyy")
            Else
                txtVencimientoProrroga.Text = ""
            End If
        Catch ex As Exception
            ' No mostrar fechas calculadas anteriormente si el nuevo cálculo falla.
            txtVencimientoArco.Clear()
            txtVencimientoProrroga.Clear()
            RegistroErrores.Escribir("Cumplimiento - fechas ARCO", ex)
        End Try
    End Sub

    Private Sub GuardarSolicitud()
        If Not Permisos.ExigirAccion("cumplimiento.arco") Then Return

        Try
            Dim solicitud As New SolicitudArco With {
                .ID = _idSolicitud,
                .IDCliente = UtileriasUI.ValorCombo(cboClienteArco),
                .TipoDerecho = Convert.ToString(cboTipoDerecho.SelectedItem),
                .FechaSolicitud = dtpFechaSolicitud.Value.Date,
                .ProrrogaAplicada = chkProrroga.Checked,
                .Estado = Convert.ToString(cboEstadoArco.SelectedItem),
                .FechaRespuesta = UtileriasUI.LeerFecha(dtpFechaRespuesta),
                .Detalle = txtDetalleArco.Text
            }

            Dim respuesta As Respuesta = _servicio.GuardarSolicitud(solicitud)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            CargarSolicitudes()
            SeleccionarSolicitud(solicitud.ID)
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Cumplimiento - guardar ARCO", ex)
            MostrarMensaje("No se pudo guardar la solicitud ARCO. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub ResponderSolicitud()
        If Not Permisos.ExigirAccion("cumplimiento.arco") Then Return

        If _idSolicitud <= 0 Then
            UtileriasUI.MostrarError("Seleccione una solicitud ARCO.")
            Return
        End If

        Dim fecha As Date = If(dtpFechaRespuesta.Checked, dtpFechaRespuesta.Value.Date, Date.Today)
        If Not UtileriasUI.Confirmar("¿Marcar la solicitud como respondida el " & fecha.ToString("dd/MM/yyyy") & "?") Then Return

        Dim respuesta As Respuesta = _servicio.ResponderSolicitud(_idSolicitud, fecha)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarSolicitudes()
        SeleccionarSolicitud(_idSolicitud)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Function SeleccionarSolicitud(id As Integer) As Boolean
        For Each fila As DataGridViewRow In dgvArco.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' La columna ID está oculta; seleccionar únicamente una celda visible.
            _cargando = True
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvArco.CurrentCell = celda
                    _cargando = False
                    Return True
                End If
            Next
            _cargando = False
            Return False
        Next
        Return False
    End Function

    ' ------------------------------------------------------------------
    ' Utilidades
    ' ------------------------------------------------------------------

    Private Function IdOpcional(combo As ComboBox) As Integer?
        Dim id As Integer = UtileriasUI.ValorCombo(combo)
        If id <= 0 Then Return Nothing
        Return id
    End Function

    Private Sub OcultarColumnas(grid As DataGridView, ParamArray nombres As String())
        For Each nombre As String In nombres
            If grid.Columns.Contains(nombre) Then grid.Columns(nombre).Visible = False
        Next
    End Sub

    Private Sub SeleccionarTexto(combo As ComboBox, valor As String)
        If String.IsNullOrWhiteSpace(valor) Then Return
        For Each elemento As Object In combo.Items
            If String.Equals(Convert.ToString(elemento), valor, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedItem = elemento
                Return
            End If
        Next
    End Sub

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    ' ------------------------------------------------------------------
    ' Eventos
    ' ------------------------------------------------------------------

    Private Sub txtFiltroOp_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFiltroOp.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarOperaciones()
        End If
    End Sub

    Private Sub txtFiltroArco_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFiltroArco.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarSolicitudes()
        End If
    End Sub

    Private Sub btnBuscarOp_Click(sender As Object, e As EventArgs) Handles btnBuscarOp.Click
        CargarOperaciones()
    End Sub

    Private Sub btnRefrescarOp_Click(sender As Object, e As EventArgs) Handles btnRefrescarOp.Click
        CargarCombos()
        CargarOperaciones()
    End Sub

    Private Sub btnNuevoOp_Click(sender As Object, e As EventArgs) Handles btnNuevoOp.Click
        LimpiarOperacion()
        nudMonto.Focus()
    End Sub

    Private Sub btnGuardarOp_Click(sender As Object, e As EventArgs) Handles btnGuardarOp.Click
        UtileriasUI.EjecutarUnaVez(btnGuardarOp, AddressOf GuardarOperacion)
    End Sub

    Private Sub btnReportarOp_Click(sender As Object, e As EventArgs) Handles btnReportarOp.Click
        UtileriasUI.EjecutarUnaVez(btnReportarOp, AddressOf ReportarOperacion)
    End Sub

    Private Sub btnDescartarOp_Click(sender As Object, e As EventArgs) Handles btnDescartarOp.Click
        UtileriasUI.EjecutarUnaVez(btnDescartarOp, AddressOf DescartarOperacion)
    End Sub

    Private Sub nudMonto_ValueChanged(sender As Object, e As EventArgs) Handles nudMonto.ValueChanged
        EvaluarUmbral()
    End Sub

    Private Sub cboTipoOperacion_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTipoOperacion.SelectedIndexChanged
        EvaluarUmbral()
    End Sub

    Private Sub btnBuscarArco_Click(sender As Object, e As EventArgs) Handles btnBuscarArco.Click
        CargarSolicitudes()
    End Sub

    Private Sub btnRefrescarArco_Click(sender As Object, e As EventArgs) Handles btnRefrescarArco.Click
        CargarCombos()
        CargarSolicitudes()
    End Sub

    Private Sub btnNuevoArco_Click(sender As Object, e As EventArgs) Handles btnNuevoArco.Click
        LimpiarSolicitud()
    End Sub

    Private Sub btnGuardarArco_Click(sender As Object, e As EventArgs) Handles btnGuardarArco.Click
        UtileriasUI.EjecutarUnaVez(btnGuardarArco, AddressOf GuardarSolicitud)
    End Sub

    Private Sub btnResponderArco_Click(sender As Object, e As EventArgs) Handles btnResponderArco.Click
        UtileriasUI.EjecutarUnaVez(btnResponderArco, AddressOf ResponderSolicitud)
    End Sub

    Private Sub dtpFechaSolicitud_ValueChanged(sender As Object, e As EventArgs) Handles dtpFechaSolicitud.ValueChanged
        ActualizarFechasArco()
    End Sub

    Private Sub chkProrroga_CheckedChanged(sender As Object, e As EventArgs) Handles chkProrroga.CheckedChanged
        ActualizarFechasArco()
    End Sub

End Class
