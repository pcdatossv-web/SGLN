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
    Private _umbralDisponible As Boolean
    Private _puedeVer As Boolean
    Private _puedeGestionarOperaciones As Boolean
    Private _puedeReportar As Boolean
    Private _puedeArco As Boolean
    Private _permisosInicializados As Boolean
    Private _estadoOperacion As String = ""
    Private _operacionReportada As Boolean
    Private _solicitudRespondida As Boolean

    ''' <summary>Solicitud ARCO que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDSolicitudInicial As Integer
    ''' <summary>Operación UIF que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDOperacionInicial As Integer

    Private Sub FrmCumplimiento_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler btnLimpiarOp.Click, AddressOf btnLimpiarOp_Click
        AddHandler btnLimpiarArco.Click, AddressOf btnLimpiarArco_Click
        ' La pantalla puede exponer dos secciones independientes. Validar la
        ' sesión una vez y consultar la matriz ya cargada evita cuatro viajes
        ' redundantes a MySQL durante la apertura del formulario.
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Me.Close()
            Return
        End If

        _puedeVer = Permisos.Puede("cumplimiento.ver")
        Dim puedeVer As Boolean = _puedeVer
        _puedeGestionarOperaciones = Permisos.Puede("cumplimiento.operaciones")
        _puedeReportar = Permisos.Puede("cumplimiento.reportar_uif")
        _puedeArco = Permisos.Puede("cumplimiento.arco")
        Dim puedeOperaciones As Boolean = _puedeGestionarOperaciones
        Dim puedeReportar As Boolean = _puedeReportar
        Dim puedeArco As Boolean = _puedeArco
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

        ' La sección debe permanecer consultable con permiso de lectura.
        ' Los controles de captura se gobiernan individualmente más abajo.
        grpArco.Enabled = True

        ' No mostrar pestañas que el usuario no puede consultar.
        ' La autorización real sigue validándose en ServicioCumplimiento.
        If Not puedeVerOperaciones Then tabPrincipal.TabPages.Remove(tabOperaciones)
        If Not puedeVerArco Then tabPrincipal.TabPages.Remove(tabArco)

        UtileriasUI.ConfigurarGrid(dgvOperaciones)
        UtileriasUI.ConfigurarGrid(dgvArco)

        ' Los estados críticos se distinguen visualmente sin ocultar su etiqueta.
        AddHandler dgvOperaciones.RowPrePaint, AddressOf dgvCumplimiento_RowPrePaint
        AddHandler dgvArco.RowPrePaint, AddressOf dgvCumplimiento_RowPrePaint

        ' Limitar la captura en pantalla a los mismos máximos que valida el servicio.

        ' Las operaciones registradas no pueden tener fecha futura.
        ' El servicio vuelve a validar esta regla antes de guardar.
        If dtpFechaOperacion.Value.Date > Date.Today Then
            dtpFechaOperacion.Value = Date.Today
        End If
        dtpFechaOperacion.MaxDate = Date.Today

        ' La recepción de una solicitud ARCO es un hecho ya ocurrido.
        If dtpFechaSolicitud.Value.Date > Date.Today Then
            dtpFechaSolicitud.Value = Date.Today
        End If
        dtpFechaSolicitud.MaxDate = Date.Today

        ' La respuesta también representa un hecho ya ocurrido; evitar fechas futuras desde la UI.
        If dtpFechaRespuesta.Value.Date > Date.Today Then
            dtpFechaRespuesta.Value = Date.Today
        End If
        dtpFechaRespuesta.MaxDate = Date.Today

        cboTipoOperacion.Items.Clear()
        cboTipoOperacion.Items.AddRange(New Object() {"Regulada", "Inusual", "Sospechosa"})
        cboTipoOperacion.SelectedIndex = 0

        cboEstadoOperacion.Items.Clear()
        cboEstadoOperacion.Items.AddRange(New Object() {"Registrada", "En análisis", "Reportada", "Descartada"})
        cboEstadoOperacion.SelectedIndex = 0

        cboMoneda.Items.Clear()
        cboMoneda.Items.AddRange(New Object() {"USD"})
        cboMoneda.SelectedIndex = 0

        cboTipoDerecho.Items.Clear()
        cboTipoDerecho.Items.AddRange(New Object() {"Acceso", "Rectificación", "Cancelación", "Oposición"})
        cboTipoDerecho.SelectedIndex = 0

        cboEstadoArco.Items.Clear()
        cboEstadoArco.Items.AddRange(New Object() {"Pendiente", "Vencida", "Respondida", "Revisar prórroga"})
        cboEstadoArco.SelectedIndex = 0

        Dim errorCatalogos As Boolean = False
        Dim errorOperaciones As Boolean = False
        Dim errorArco As Boolean = False
        Dim errorUmbral As Boolean = False

        _cargando = True
        CargarCombos()
        errorCatalogos = cboCasoOp.DataSource Is Nothing OrElse
                         cboInstrumentoOp.DataSource Is Nothing OrElse
                         cboClienteArco.DataSource Is Nothing

        If puedeVerOperaciones Then
            CargarOperaciones()
            errorOperaciones = dgvOperaciones.DataSource Is Nothing
        End If

        If puedeVerArco Then
            CargarSolicitudes()
            errorArco = dgvArco.DataSource Is Nothing
        End If
        _cargando = False

        Try
            _umbral = _servicio.UmbralEfectivo()
            _umbralDisponible = True
        Catch ex As Exception
            ' Mantener la pantalla consultable, pero nunca presentar un umbral
            ' alternativo como si fuera el valor efectivo configurado.
            _umbral = 0D
            _umbralDisponible = False
            errorUmbral = True
            RegistroErrores.Escribir("Cumplimiento - cargar umbral inicial", ex)
        End Try
        If errorUmbral Then
            lblNotaUmbral.Text = "No se pudo recuperar el umbral configurado. La evaluación automática de montos queda no disponible hasta restablecer la configuración."
        Else
            lblNotaUmbral.Text = "Umbral efectivo aplicado (configurado o predeterminado): $" & _umbral.ToString("N2") &
                ". Los plazos de análisis y reporte son configurables y deben validarse con el oficial de cumplimiento."
        End If

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

        Dim mensajeErrorInicial As String = ""
        If errorCatalogos Then mensajeErrorInicial &= "No se pudieron cargar los catálogos. "
        If errorOperaciones Then mensajeErrorInicial &= "No se pudieron cargar las operaciones de cumplimiento. "
        If errorArco Then mensajeErrorInicial &= "No se pudieron cargar las solicitudes ARCO. "
        If errorUmbral Then mensajeErrorInicial &= "No se pudo recuperar el umbral configurado; la evaluación automática de montos permanece deshabilitada. "

        If mensajeErrorInicial.Length > 0 Then
            MostrarMensaje(mensajeErrorInicial.Trim(), False)
        End If

        _permisosInicializados = True
    End Sub

    Private Sub FrmCumplimiento_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        ' Las pestañas se filtran al iniciar. Si cambia cualquier permiso del
        ' módulo, cerrar este formulario evita conservar pestañas o controles
        ' visibles con autorizaciones obsoletas; al reabrir se aplica la matriz nueva.
        Dim puedeVer As Boolean = Permisos.Puede("cumplimiento.ver")
        Dim puedeOperaciones As Boolean = Permisos.Puede("cumplimiento.operaciones")
        Dim puedeReportar As Boolean = Permisos.Puede("cumplimiento.reportar_uif")
        Dim puedeArco As Boolean = Permisos.Puede("cumplimiento.arco")

        If Not (puedeVer OrElse puedeOperaciones OrElse puedeReportar OrElse puedeArco) OrElse
           puedeVer <> _puedeVer OrElse
           puedeOperaciones <> _puedeGestionarOperaciones OrElse
           puedeReportar <> _puedeReportar OrElse
           puedeArco <> _puedeArco Then
            Close()
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

    ''' <summary>
    ''' Señala vencimientos y estados de cumplimiento con colores discretos.
    ''' El texto del estado siempre permanece visible para no depender del color.
    ''' </summary>
    Private Sub dgvCumplimiento_RowPrePaint(sender As Object, e As DataGridViewRowPrePaintEventArgs)
        Dim grid As DataGridView = TryCast(sender, DataGridView)
        If grid Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= grid.Rows.Count Then Return

        Dim fila As DataGridViewRow = grid.Rows(e.RowIndex)
        If grid.Columns.Contains("Estado") Then
            Dim estado As String = Convert.ToString(fila.Cells("Estado").Value).Trim()
            Select Case estado.ToUpperInvariant()
                Case "VENCIDA"
                    fila.DefaultCellStyle.BackColor = Color.MistyRose
                    fila.DefaultCellStyle.ForeColor = Color.DarkRed
                Case "REVISAR PRÓRROGA", "REVISAR PRORROGA"
                    fila.DefaultCellStyle.BackColor = Color.LemonChiffon
                    fila.DefaultCellStyle.ForeColor = Color.DarkGoldenrod
                Case "RESPONDIDA", "REPORTADA"
                    fila.DefaultCellStyle.BackColor = Color.Honeydew
                    fila.DefaultCellStyle.ForeColor = Color.DarkGreen
                Case "DESCARTADA"
                    fila.DefaultCellStyle.BackColor = Color.Gainsboro
                    fila.DefaultCellStyle.ForeColor = Color.DimGray
                Case Else
                    fila.DefaultCellStyle.BackColor = grid.DefaultCellStyle.BackColor
                    fila.DefaultCellStyle.ForeColor = grid.DefaultCellStyle.ForeColor
            End Select
        End If

        If grid.Columns.Contains("Reportado_UIF") Then
            Dim reportada As Boolean = False
            If fila.Cells("Reportado_UIF").Value IsNot Nothing AndAlso
               Not Convert.IsDBNull(fila.Cells("Reportado_UIF").Value) Then
                Dim valorReportado As String = Convert.ToString(fila.Cells("Reportado_UIF").Value).Trim()
                If Not Boolean.TryParse(valorReportado, reportada) Then
                    ' MySQL puede entregar TINYINT(1) como 0/1 numérico o como texto.
                    ' Convert.ToBoolean("1") no es válido en .NET; interpretar ambos formatos.
                    Dim numeroReportado As Long
                    reportada = Long.TryParse(valorReportado, numeroReportado) AndAlso numeroReportado <> 0
                End If
            End If
            If reportada Then
                fila.DefaultCellStyle.BackColor = Color.Honeydew
                fila.DefaultCellStyle.ForeColor = Color.DarkGreen
            End If
        End If
    End Sub

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
        If fila Is Nothing Then
            ' No conservar una operación activa fuera de la selección visible.
            LimpiarOperacion()
            Return
        End If
        CargarOperacion(fila)
    End Sub

    Private Sub CargarOperacion(fila As DataRow)
        _idOperacion = UtileriasUI.Entero(fila, "ID")
        _estadoOperacion = UtileriasUI.Texto(fila, "Estado")
        _operacionReportada = UtileriasUI.Booleano(fila, "Reportado_UIF")

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
        ActualizarAccionesOperacion()
        MostrarMensaje("", True)
    End Sub

    Private Sub LimpiarOperacion()
        _idOperacion = 0
        _estadoOperacion = ""
        _operacionReportada = False
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
        ActualizarAccionesOperacion()
    End Sub

    Private Sub ActualizarAccionesOperacion()
        Dim estadoPermiteAccion As Boolean = _idOperacion > 0 AndAlso
            Not _operacionReportada AndAlso
            Not String.Equals(_estadoOperacion, "Reportada", StringComparison.OrdinalIgnoreCase) AndAlso
            Not String.Equals(_estadoOperacion, "Descartada", StringComparison.OrdinalIgnoreCase)
        Dim estadoPermiteEdicion As Boolean = _idOperacion = 0 OrElse
            (estadoPermiteAccion AndAlso
             (String.Equals(_estadoOperacion, "Registrada", StringComparison.OrdinalIgnoreCase) OrElse
              String.Equals(_estadoOperacion, "En análisis", StringComparison.OrdinalIgnoreCase)))
        Dim puedeEditar As Boolean = _puedeGestionarOperaciones AndAlso estadoPermiteEdicion

        btnGuardarOp.Enabled = puedeEditar
        cboTipoOperacion.Enabled = puedeEditar
        nudMonto.Enabled = puedeEditar
        cboMoneda.Enabled = puedeEditar
        dtpFechaOperacion.Enabled = puedeEditar
        cboCasoOp.Enabled = puedeEditar
        cboInstrumentoOp.Enabled = puedeEditar
        txtDescripcionOp.ReadOnly = Not puedeEditar
        txtObservacionesOp.ReadOnly = Not puedeEditar

        btnReportarOp.Enabled = _puedeReportar AndAlso estadoPermiteAccion
        btnDescartarOp.Enabled = _puedeGestionarOperaciones AndAlso estadoPermiteAccion
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

        Dim idOperacion As Integer = _idOperacion
        If Not UtileriasUI.Confirmar("¿Registrar el reporte a la UIF con fecha de hoy?") Then Return

        Dim respuesta As Respuesta = _servicio.MarcarReportada(idOperacion, Date.Today)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarOperaciones()
        SeleccionarOperacion(idOperacion)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub DescartarOperacion()
        If Not Permisos.ExigirAccion("cumplimiento.operaciones") Then Return

        If _idOperacion <= 0 Then
            UtileriasUI.MostrarError("Seleccione una operación.")
            Return
        End If

        Dim idOperacion As Integer = _idOperacion
        If Not UtileriasUI.Confirmar("¿Descartar la operación seleccionada? El registro se conserva.") Then Return

        Dim respuesta As Respuesta = _servicio.Descartar(idOperacion)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarOperaciones()
        SeleccionarOperacion(idOperacion)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub EvaluarUmbral()
        lblAlertaUmbral.Text = ""
        lblAlertaUmbral.ForeColor = Color.Firebrick

        If Not _umbralDisponible Then
            lblAlertaUmbral.Text = "No se puede evaluar el monto: el umbral configurado no está disponible."
            Return
        End If

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
        If fila Is Nothing Then
            ' Evitar responder o editar una solicitud que ya no está seleccionada.
            LimpiarSolicitud()
            Return
        End If
        CargarSolicitud(fila)
    End Sub

    Private Sub CargarSolicitud(fila As DataRow)
        _idSolicitud = UtileriasUI.Entero(fila, "ID")
        _solicitudRespondida = Not fila.IsNull("Fecha_Respuesta") OrElse
            String.Equals(UtileriasUI.Texto(fila, "Estado"), "Respondida", StringComparison.OrdinalIgnoreCase)

        ' La identidad del titular, el derecho y la fecha de recepción son
        ' datos históricos inmutables una vez registrada la solicitud.
        cboClienteArco.Enabled = False
        cboTipoDerecho.Enabled = False
        dtpFechaSolicitud.Enabled = False

        UtileriasUI.SeleccionarValor(cboClienteArco, fila("ID_Cliente"))
        SeleccionarTexto(cboTipoDerecho, UtileriasUI.Texto(fila, "Tipo_Derecho"))
        Dim fechaSolicitud As Date = UtileriasUI.Fecha(fila, "Fecha_Solicitud").GetValueOrDefault(Date.Today).Date
        dtpFechaSolicitud.Value = fechaSolicitud
        ' La respuesta no puede anteceder a la recepción de esta solicitud.
        dtpFechaRespuesta.MinDate = fechaSolicitud
        SeleccionarTexto(cboEstadoArco, UtileriasUI.Texto(fila, "Estado"))

        Dim vencimiento As Date? = UtileriasUI.Fecha(fila, "Fecha_Vencimiento")
        txtVencimientoArco.Text = If(vencimiento.HasValue, vencimiento.Value.ToString("dd/MM/yyyy"), "")

        chkProrroga.Checked = UtileriasUI.Booleano(fila, "Prorroga_Aplicada")
        Dim prorroga As Date? = UtileriasUI.Fecha(fila, "Fecha_Vencimiento_Prorroga")
        txtVencimientoProrroga.Text = If(prorroga.HasValue, prorroga.Value.ToString("dd/MM/yyyy"), "")

        txtDetalleArco.Text = UtileriasUI.Texto(fila, "Detalle")

        Dim respuesta As Date? = UtileriasUI.Fecha(fila, "Fecha_Respuesta")
        UtileriasUI.AsignarFecha(dtpFechaRespuesta, respuesta)

        ActualizarAccionesSolicitud()
        MostrarMensaje("", True)
    End Sub

    Private Sub LimpiarSolicitud()
        _idSolicitud = 0
        _solicitudRespondida = False
        cboClienteArco.Enabled = True
        cboTipoDerecho.Enabled = True
        dtpFechaSolicitud.Enabled = True
        If cboClienteArco.Items.Count > 0 Then cboClienteArco.SelectedIndex = 0
        cboTipoDerecho.SelectedIndex = 0
        dtpFechaSolicitud.Value = Date.Today
        dtpFechaRespuesta.MinDate = DateTimePicker.MinimumDateTime
        cboEstadoArco.SelectedIndex = 0
        chkProrroga.Checked = False
        dtpFechaRespuesta.Checked = False
        txtDetalleArco.Clear()
        ActualizarFechasArco()

        _cargando = True
        dgvArco.CurrentCell = Nothing
        _cargando = False
        ActualizarAccionesSolicitud()
    End Sub

    Private Sub ActualizarAccionesSolicitud()
        Dim puedeEditarSolicitud As Boolean = _puedeArco AndAlso Not _solicitudRespondida

        ' En una solicitud nueva se permite capturar titular, derecho y fecha
        ' solo a quien puede gestionar. En registros existentes son inmutables.
        Dim puedeCrearSolicitud As Boolean = _puedeArco AndAlso _idSolicitud = 0
        cboClienteArco.Enabled = puedeCrearSolicitud
        cboTipoDerecho.Enabled = puedeCrearSolicitud
        dtpFechaSolicitud.Enabled = puedeCrearSolicitud

        ' No deshabilitar el contenedor: impediría consultar solicitudes a
        ' usuarios con permiso de lectura, aunque las acciones estén protegidas.
        grpArco.Enabled = True
        btnGuardarArco.Enabled = puedeEditarSolicitud
        btnResponderArco.Enabled = _puedeArco AndAlso _idSolicitud > 0 AndAlso Not _solicitudRespondida

        ' La fecha solo se captura al preparar la respuesta de una solicitud
        ' existente y pendiente. Un registro nuevo o ya respondido no admite
        ' una fecha de respuesta independiente.
        dtpFechaRespuesta.Enabled = _puedeArco AndAlso
            _idSolicitud > 0 AndAlso Not _solicitudRespondida

        ' Una prórroga ya aplicada forma parte del historial del trámite y no
        ' se puede retirar ni modificar. Evitar ofrecer una edición que el
        ' servicio y el repositorio necesariamente rechazarían.
        chkProrroga.Enabled = puedeEditarSolicitud AndAlso
            (_idSolicitud = 0 OrElse Not chkProrroga.Checked)
        txtDetalleArco.ReadOnly = Not puedeEditarSolicitud
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

        Dim idSolicitud As Integer = _idSolicitud
        Dim fecha As Date = If(dtpFechaRespuesta.Checked, dtpFechaRespuesta.Value.Date, Date.Today)
        If Not UtileriasUI.Confirmar("¿Marcar la solicitud como respondida el " & fecha.ToString("dd/MM/yyyy") & "?") Then Return

        Dim respuesta As Respuesta = _servicio.ResponderSolicitud(idSolicitud, fecha)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarSolicitudes()
        SeleccionarSolicitud(idSolicitud)
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
                    ' SelectionChanged se suprime durante el cambio programático;
                    ' cargar aquí el detalle evita que la tabla y el formulario
                    ' queden apuntando a solicitudes distintas.
                    CargarSolicitud(vista.Row)
                    Return True
                End If
            Next
            _cargando = False
            Return False
        Next
        Return False
    End Function

    Private Sub dgvOperaciones_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvOperaciones.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse
           Not String.Equals(dgvOperaciones.Columns(e.ColumnIndex).Name, "Estado", StringComparison.OrdinalIgnoreCase) OrElse
           e.Value Is Nothing OrElse e.Value Is DBNull.Value Then Return

        Select Case Convert.ToString(e.Value).Trim()
            Case "Registrada"
                e.CellStyle.BackColor = Color.AliceBlue
                e.CellStyle.ForeColor = Color.DarkSlateBlue
            Case "En análisis"
                e.CellStyle.BackColor = Color.LemonChiffon
                e.CellStyle.ForeColor = Color.DarkGoldenrod
            Case "Reportada"
                e.CellStyle.BackColor = Color.Honeydew
                e.CellStyle.ForeColor = Color.DarkGreen
            Case "Descartada"
                e.CellStyle.BackColor = Color.Gainsboro
                e.CellStyle.ForeColor = Color.DimGray
            Case Else
                Return
        End Select

        e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
        e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor
        e.FormattingApplied = True
    End Sub

    Private Sub dgvArco_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvArco.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse
           Not String.Equals(dgvArco.Columns(e.ColumnIndex).Name, "Estado", StringComparison.OrdinalIgnoreCase) OrElse
           e.Value Is Nothing OrElse e.Value Is DBNull.Value Then Return

        Select Case Convert.ToString(e.Value).Trim()
            Case "Pendiente"
                e.CellStyle.BackColor = Color.AliceBlue
                e.CellStyle.ForeColor = Color.DarkSlateBlue
            Case "Vencida"
                e.CellStyle.BackColor = Color.MistyRose
                e.CellStyle.ForeColor = Color.Firebrick
            Case "Revisar prórroga"
                e.CellStyle.BackColor = Color.LemonChiffon
                e.CellStyle.ForeColor = Color.DarkGoldenrod
            Case "Respondida"
                e.CellStyle.BackColor = Color.Honeydew
                e.CellStyle.ForeColor = Color.DarkGreen
            Case Else
                Return
        End Select

        e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
        e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor
        e.FormattingApplied = True
    End Sub

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
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            LimpiarFiltroOperaciones()
        End If
    End Sub

    Private Sub LimpiarFiltroOperaciones()
        txtFiltroOp.Clear()
        CargarOperaciones()
        txtFiltroOp.Focus()
    End Sub

    Private Sub btnLimpiarOp_Click(sender As Object, e As EventArgs)
        LimpiarFiltroOperaciones()
    End Sub

    Private Sub txtFiltroArco_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFiltroArco.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarSolicitudes()
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            LimpiarFiltroArco()
        End If
    End Sub

    Private Sub LimpiarFiltroArco()
        txtFiltroArco.Clear()
        CargarSolicitudes()
        txtFiltroArco.Focus()
    End Sub

    Private Sub btnLimpiarArco_Click(sender As Object, e As EventArgs)
        LimpiarFiltroArco()
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
