Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmProtocolo

    Private ReadOnly _servicio As New ServicioProtocolo()
    Private _cargando As Boolean
    Private _idLibro As Integer
    Private _vistaInstrumentos As DataView
    Private _vistaTestimonios As DataView

    ''' <summary>Libro que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDLibroInicial As Integer
    Private _idInstrumento As Integer
    Private _comparecientesDisponibles As Boolean
    Private _permisosInicializados As Boolean

    Private Sub FrmProtocolo_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar", "protocolo.cerrar_libro", "protocolo.entregar", "protocolo.testimonios") Then
            MessageBox.Show("Su rol no tiene permisos para consultar ni operar el protocolo.", "Permiso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Close()
            Return
        End If

        AplicarPermisosInterfaz()

        ' Reflejar en la captura las mismas restricciones temporales del servicio.
        dtpFechaHoraOtorgamiento.MaxDate = Date.Today
        dtpFechaTestimonio.MaxDate = Date.Today
        ' Exportar el índice es una operación de consulta y queda disponible
        ' para quien ya tiene permiso de lectura del protocolo.

        UtileriasUI.ConfigurarGrid(dgvLibros)
        UtileriasUI.ConfigurarGrid(dgvInstrumentos)
        UtileriasUI.ConfigurarGrid(dgvComparecientes)
        UtileriasUI.ConfigurarGrid(dgvTestimonios)
        UtileriasUI.ConfigurarGrid(dgvIndice)

        ' Diferenciar visualmente estados sin depender únicamente del texto.
        AddHandler dgvLibros.RowPrePaint, AddressOf dgvProtocolo_RowPrePaint
        AddHandler dgvInstrumentos.RowPrePaint, AddressOf dgvProtocolo_RowPrePaint
        AddHandler dgvComparecientes.RowPrePaint, AddressOf dgvProtocolo_RowPrePaint
        AddHandler dgvTestimonios.RowPrePaint, AddressOf dgvProtocolo_RowPrePaint
        AddHandler dgvIndice.RowPrePaint, AddressOf dgvProtocolo_RowPrePaint

        cboEstadoLibro.Items.Clear()
        cboEstadoLibro.Items.AddRange(New Object() {"Vigente", "Cerrado", "Entregado"})
        cboEstadoLibro.SelectedIndex = 0

        cboEstadoInstrumento.Items.Clear()
        cboEstadoInstrumento.Items.AddRange(New Object() {"Vigente", "Anulado"})
        cboEstadoInstrumento.SelectedIndex = 0

        cboCalidad.Items.Clear()
        cboCalidad.Items.AddRange(New Object() {"Otorgante", "Favorecido", "Testigo", "Intérprete"})
        cboCalidad.SelectedIndex = 0

        cboTipoTestimonio.Items.Clear()
        cboTipoTestimonio.Items.AddRange(New Object() {"Primer", "Ulterior"})
        cboTipoTestimonio.SelectedIndex = 0

        nudAnio.Minimum = 2000
        nudAnio.Maximum = 2100
        nudAnio.Value = Date.Today.Year

        _cargando = True
        CargarClientes()
        CargarLibros(If(IDLibroInicial > 0, IDLibroInicial, 0), IDLibroInicial > 0)
        _cargando = False

        If IDLibroInicial > 0 Then
            If _idLibro = IDLibroInicial Then
                MostrarMensaje("", True)
            Else
                _cargando = True
                cboLibro.SelectedIndex = -1
                dgvLibros.ClearSelection()
                dgvLibros.CurrentCell = Nothing
                _cargando = False
                _idLibro = 0
                LimpiarLibro()
                LimpiarInstrumento()
                cboInstrumentoTest.DataSource = Nothing
                dgvInstrumentos.DataSource = Nothing
                dgvComparecientes.DataSource = Nothing
                dgvTestimonios.DataSource = Nothing
                dgvIndice.DataSource = Nothing
                If dgvLibros.DataSource Is Nothing Then
                    MostrarMensaje("No se pudieron cargar los libros. Verifique la conexión con la base de datos.", False)
                Else
                    MostrarMensaje("El libro asociado a la alerta ya no está disponible.", False)
                End If
            End If
        Else
            ' No cargar automáticamente el primer libro y luego descartar sus datos.
            ' Sin contexto inicial, dejar el selector y las secciones dependientes vacíos.
            _cargando = True
            cboLibro.SelectedIndex = -1
            dgvLibros.ClearSelection()
            dgvLibros.CurrentCell = Nothing
            cboInstrumentoTest.DataSource = Nothing
            dgvInstrumentos.DataSource = Nothing
            dgvComparecientes.DataSource = Nothing
            dgvTestimonios.DataSource = Nothing
            dgvIndice.DataSource = Nothing
            _cargando = False
            LimpiarLibro()
            LimpiarInstrumento()
            If dgvLibros.DataSource Is Nothing Then
                MostrarMensaje("No se pudieron cargar los libros. Verifique la conexión con la base de datos.", False)
            Else
                MostrarMensaje("", True)
            End If
        End If

        _permisosInicializados = True
    End Sub

    ''' <summary>
    ''' Aplica la matriz vigente a los controles del protocolo. Los servicios
    ''' mantienen su propia comprobación antes de ejecutar cada operación.
    ''' </summary>
    Private Sub AplicarPermisosInterfaz()
        Dim puedeGestionar As Boolean = Permisos.Puede("protocolo.gestionar")
        Dim puedeCerrar As Boolean = Permisos.Puede("protocolo.cerrar_libro")
        Dim puedeEntregar As Boolean = Permisos.Puede("protocolo.entregar")
        Dim puedeExpedirTestimonios As Boolean = Permisos.Puede("protocolo.testimonios")

        nudNumeroLibro.Enabled = puedeGestionar
        nudAnio.Enabled = puedeGestionar
        dtpFechaAutorizacion.Enabled = puedeGestionar
        dtpFechaVencimiento.Enabled = puedeGestionar
        cboEstadoLibro.Enabled = False
        btnNuevoLibro.Enabled = puedeGestionar
        btnGuardarLibro.Enabled = puedeGestionar
        txtRazonCierre.Enabled = puedeCerrar
        btnCerrarLibro.Enabled = puedeCerrar
        dtpFechaEntrega.Enabled = puedeEntregar
        btnEntregarLibro.Enabled = puedeEntregar

        nudNumeroEscritura.Enabled = puedeGestionar
        dtpFechaHoraOtorgamiento.Enabled = puedeGestionar
        nudFolioInicio.Enabled = puedeGestionar
        nudFolioFin.Enabled = puedeGestionar
        txtObjeto.Enabled = puedeGestionar
        cboEstadoInstrumento.Enabled = False
        btnNuevoInstrumento.Enabled = puedeGestionar
        btnGuardarInstrumento.Enabled = puedeGestionar
        btnAnularInstrumento.Enabled = puedeGestionar

        cboClienteComp.Enabled = puedeGestionar
        cboCalidad.Enabled = puedeGestionar
        btnAgregarComp.Enabled = puedeGestionar
        btnQuitarComp.Enabled = puedeGestionar

        grpTestimonio.Enabled = puedeExpedirTestimonios
        btnGenerarIndice.Enabled = puedeGestionar
        ActualizarEstadoAccionesContextuales()
    End Sub

    Private Sub FrmProtocolo_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("protocolo.ver") AndAlso
           Not Permisos.Puede("protocolo.gestionar") AndAlso
           Not Permisos.Puede("protocolo.cerrar_libro") AndAlso
           Not Permisos.Puede("protocolo.entregar") AndAlso
           Not Permisos.Puede("protocolo.testimonios") Then
            Close()
            Return
        End If

        AplicarPermisosInterfaz()
    End Sub

    ' ------------------------------------------------------------------
    ' Carga general
    ' ------------------------------------------------------------------

    Private Sub CargarClientes()
        Try
            UtileriasUI.CargarCombo(cboClienteComp, _servicio.ClientesCombo(), "Nombre", "ID")
        Catch ex As Exception
            cboClienteComp.DataSource = Nothing
            RegistroErrores.Escribir("Protocolo - clientes", ex)
        End Try
    End Sub

    Private Sub CargarLibros(seleccionar As Integer, Optional cargarDetalleSeleccionado As Boolean = True)
        Try
            Dim tabla As DataTable = _servicio.Libros()

            _cargando = True
            dgvLibros.DataSource = tabla
            ConfigurarPresentacionGrid(dgvLibros)
            OcultarColumnas(dgvLibros, "ID", "Razon_Cierre")

            UtileriasUI.CargarCombo(cboLibro, TablaLibrosCombo(tabla), "Descripcion", "ID")
            If seleccionar > 0 Then
                UtileriasUI.SeleccionarValor(cboLibro, seleccionar)
            ElseIf cboLibro.Items.Count > 0 Then
                cboLibro.SelectedIndex = 0
            End If
            _cargando = False

            If cargarDetalleSeleccionado Then CargarLibroSeleccionado()
        Catch ex As Exception
            _cargando = False
            dgvLibros.DataSource = Nothing
            cboLibro.DataSource = Nothing
            cboInstrumentoTest.DataSource = Nothing
            dgvInstrumentos.DataSource = Nothing
            dgvComparecientes.DataSource = Nothing
            dgvTestimonios.DataSource = Nothing
            dgvIndice.DataSource = Nothing
            LimpiarLibro()
            LimpiarInstrumento()
            RegistroErrores.Escribir("Protocolo - libros", ex)
            MostrarMensaje("No se pudieron cargar los libros. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Function TablaLibrosCombo(origen As DataTable) As DataTable
        Dim tabla As New DataTable()
        tabla.Columns.Add("ID", GetType(Integer))
        tabla.Columns.Add("Descripcion", GetType(String))

        For Each fila As DataRow In origen.Rows
            tabla.Rows.Add(
                Convert.ToInt32(fila("ID")),
                "Libro " & Convert.ToString(fila("Numero_Libro")) & " - " &
                Convert.ToString(fila("Anio_Vigencia")) & " (" & Convert.ToString(fila("Estado")) & ")")
        Next

        Return tabla
    End Function

    Private Sub CargarLibroSeleccionado()
        _idLibro = UtileriasUI.ValorCombo(cboLibro)
        CargarLibroEditor(_idLibro)

        ' Al cambiar de libro se limpia el instrumento en edición para no
        ' mostrar comparecientes de otro libro.
        _idInstrumento = 0
        dgvComparecientes.DataSource = Nothing
        CargarInstrumentos()
        SugerirSiguienteInstrumento()

        CargarInstrumentosCombo()
        CargarTestimonios()
        CargarIndice()
        ActualizarEstadoAccionesContextuales()
    End Sub

    Private Sub CargarLibroEditor(id As Integer)
        If id <= 0 Then
            LimpiarLibro()
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.ObtenerLibro(id)
            If tabla.Rows.Count = 0 Then
                ' El libro pudo eliminarse o quedar inaccesible tras cargar el listado.
                ' No conservar datos anteriores ni permitir operar sobre otro libro.
                LimpiarLibro()
                Return
            End If

            Dim fila As DataRow = tabla.Rows(0)
            nudNumeroLibro.Value = Math.Max(1, UtileriasUI.Entero(fila, "Numero_Libro"))
            nudAnio.Value = UtileriasUI.Entero(fila, "Anio_Vigencia")
            dtpFechaAutorizacion.Value = UtileriasUI.Fecha(fila, "Fecha_Autorizacion").GetValueOrDefault(Date.Today)
            dtpFechaVencimiento.Value = UtileriasUI.Fecha(fila, "Fecha_Vencimiento").GetValueOrDefault(Date.Today.AddYears(1))
            SeleccionarTexto(cboEstadoLibro, UtileriasUI.Texto(fila, "Estado"))
            txtRazonCierre.Text = UtileriasUI.Texto(fila, "Razon_Cierre")

            Dim cierre As Date? = UtileriasUI.Fecha(fila, "Fecha_Cierre")
            txtFechaCierre.Text = If(cierre.HasValue, cierre.Value.ToString("dd/MM/yyyy"), "Sin cerrar")

            ' La fecha de entrega solo puede registrarse desde el cierre del libro
            ' y hasta hoy. Reflejar ambas reglas en el selector evita errores de captura.
            dtpFechaEntrega.MaxDate = Date.Today
            dtpFechaEntrega.MinDate = If(cierre.HasValue AndAlso cierre.Value.Date > DateTimePicker.MinimumDateTime.Date,
                                        cierre.Value.Date, DateTimePicker.MinimumDateTime)

            Dim entrega As Date? = UtileriasUI.Fecha(fila, "Fecha_Entrega_Notariado")
            Dim fechaEntrega As Date = entrega.GetValueOrDefault(Date.Today)
            If fechaEntrega < dtpFechaEntrega.MinDate Then fechaEntrega = dtpFechaEntrega.MinDate
            If fechaEntrega > dtpFechaEntrega.MaxDate Then fechaEntrega = dtpFechaEntrega.MaxDate
            dtpFechaEntrega.Value = fechaEntrega

            lblLimiteEntrega.Text = ""
            If cierre.HasValue Then
                Dim limite As Date = _servicio.FechaLimiteEntrega(cierre.Value)
                lblLimiteEntrega.Text = "Estimación interna (días hábiles configurados): " &
                    limite.ToString("dd/MM/yyyy") & ". Verifique el cómputo legal aplicable. " &
                    "SGLN no genera ni firma el paquete electrónico; registre la presentación ya realizada."
            End If
        Catch ex As Exception
            LimpiarLibro()
            RegistroErrores.Escribir("Protocolo - editor de libro", ex)
            MostrarMensaje("No se pudo cargar el libro seleccionado. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Libros: acciones
    ' ------------------------------------------------------------------

    Private Sub LimpiarLibro()
        _idLibro = 0
        nudNumeroLibro.Value = 1
        nudAnio.Value = Date.Today.Year
        dtpFechaAutorizacion.Value = Date.Today
        dtpFechaVencimiento.Value = Date.Today.AddYears(1)
        cboEstadoLibro.SelectedIndex = 0
        txtRazonCierre.Clear()
        txtFechaCierre.Text = "Sin cerrar"
        dtpFechaEntrega.MaxDate = Date.Today
        dtpFechaEntrega.MinDate = DateTimePicker.MinimumDateTime
        dtpFechaEntrega.Value = Date.Today
        lblLimiteEntrega.Text = ""
    End Sub

    Private Sub GuardarLibro()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        Try
            Dim libro As New LibroProtocolo With {
                .ID = _idLibro,
                .NumeroLibro = Convert.ToInt32(nudNumeroLibro.Value),
                .AnioVigencia = Convert.ToInt32(nudAnio.Value),
                .FechaAutorizacion = dtpFechaAutorizacion.Value.Date,
                .FechaVencimiento = dtpFechaVencimiento.Value.Date,
                .Estado = Convert.ToString(cboEstadoLibro.SelectedItem)
            }

            Dim respuesta As Respuesta = _servicio.GuardarLibro(libro)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            _cargando = True
            CargarLibros(libro.ID)
            _cargando = False
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - guardar libro", ex)
            MostrarMensaje("No se pudo guardar el libro. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CerrarLibro()
        If Not Permisos.ExigirAccion("protocolo.cerrar_libro") Then Return

        If _idLibro <= 0 Then
            UtileriasUI.MostrarError("Seleccione un libro.")
            Return
        End If

        If String.IsNullOrWhiteSpace(txtRazonCierre.Text) Then
            UtileriasUI.MostrarError("Indique la razón del cierre en el campo correspondiente.")
            txtRazonCierre.Focus()
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Cerrar el libro seleccionado? Esta acción no se puede revertir desde la interfaz.") Then Return

        Dim respuesta As Respuesta = _servicio.CerrarLibro(_idLibro, txtRazonCierre.Text)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        _cargando = True
        CargarLibros(_idLibro)
        _cargando = False
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub EntregarLibro()
        If Not Permisos.ExigirAccion("protocolo.entregar") Then Return

        If _idLibro <= 0 Then
            UtileriasUI.MostrarError("Seleccione un libro cerrado.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Registrar que el libro electrónico firmado y sus anexos ya fueron presentados al Notariado el " &
                                     dtpFechaEntrega.Value.ToString("dd/MM/yyyy") & "? SGLN solo registra la fecha; no genera ni firma el paquete.") Then Return

        Dim respuesta As Respuesta = _servicio.RegistrarEntrega(_idLibro, dtpFechaEntrega.Value.Date)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        _cargando = True
        CargarLibros(_idLibro)
        _cargando = False
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    ' ------------------------------------------------------------------
    ' Instrumentos
    ' ------------------------------------------------------------------

    Private Sub CargarInstrumentos()
        If _idLibro <= 0 Then
            dgvInstrumentos.DataSource = Nothing
            _vistaInstrumentos = Nothing
            txtBuscarInstrumento.Clear()
            LimpiarInstrumento()
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.Instrumentos(_idLibro)
            _cargando = True
            txtBuscarInstrumento.Clear()
            _vistaInstrumentos = New DataView(tabla)
            dgvInstrumentos.DataSource = _vistaInstrumentos
            ConfigurarPresentacionGrid(dgvInstrumentos)
            AplicarFiltroInstrumentosInterno()
            OcultarColumnas(dgvInstrumentos, "ID")
            _cargando = False
            Dim actual As DataRow = UtileriasUI.FilaActual(dgvInstrumentos)
            If actual Is Nothing Then
                LimpiarInstrumento()
            Else
                CargarInstrumento(UtileriasUI.Entero(actual, "ID"))
            End If
        Catch ex As Exception
            _cargando = False
            dgvInstrumentos.DataSource = Nothing
            _vistaInstrumentos = Nothing
            LimpiarInstrumento()
            RegistroErrores.Escribir("Protocolo - instrumentos", ex)
            MostrarMensaje("No se pudieron cargar los instrumentos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Filtra localmente las escrituras del libro seleccionado sin volver a
    ''' consultar la base de datos en cada pulsación.
    ''' </summary>
    Private Sub AplicarFiltroInstrumentosInterno()
        If _vistaInstrumentos Is Nothing Then Return

        Dim filtro As String = If(txtBuscarInstrumento.Text, "").Trim()
        If filtro.Length = 0 Then
            _vistaInstrumentos.RowFilter = ""
            Return
        End If

        Dim valor As String = filtro.Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]").Replace("'", "''")
        Dim patron As String = "'%" & valor & "%'"
        _vistaInstrumentos.RowFilter =
            "CONVERT([Numero_Escritura], 'System.String') LIKE " & patron &
            " OR CONVERT([Folio_Inicio], 'System.String') LIKE " & patron &
            " OR CONVERT([Folio_Fin], 'System.String') LIKE " & patron &
            " OR [Objeto_Acto] LIKE " & patron &
            " OR [Estado] LIKE " & patron
    End Sub

    Private Sub txtBuscarInstrumento_KeyDown(sender As Object, e As KeyEventArgs) Handles txtBuscarInstrumento.KeyDown
        If e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            txtBuscarInstrumento.Clear()
            txtBuscarInstrumento.Focus()
        End If
    End Sub

    Private Sub txtBuscarInstrumento_TextChanged(sender As Object, e As EventArgs) Handles txtBuscarInstrumento.TextChanged
        If _cargando OrElse _vistaInstrumentos Is Nothing Then Return

        Try
            _cargando = True
            AplicarFiltroInstrumentosInterno()
            _cargando = False

            Dim actual As DataRow = UtileriasUI.FilaActual(dgvInstrumentos)
            If actual Is Nothing Then
                LimpiarInstrumento()
            Else
                CargarInstrumento(UtileriasUI.Entero(actual, "ID"))
            End If
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Protocolo - filtro de instrumentos", ex)
            MostrarMensaje("No se pudo aplicar el filtro de escrituras.", False)
        End Try
    End Sub

    Private Sub dgvInstrumentos_SelectionChanged(sender As Object, e As EventArgs) Handles dgvInstrumentos.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvInstrumentos)
        If fila Is Nothing Then
            ' No mantener el instrumento anterior si la selección queda vacía.
            LimpiarInstrumento()
            Return
        End If
        CargarInstrumento(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarInstrumento(id As Integer)
        Try
            Dim tabla As DataTable = _servicio.ObtenerInstrumento(id)
            If tabla.Rows.Count = 0 Then
                LimpiarInstrumento()
                Return
            End If

            Dim fila As DataRow = tabla.Rows(0)
            _idInstrumento = id

            nudNumeroEscritura.Value = Math.Max(1, UtileriasUI.Entero(fila, "Numero_Escritura"))
            dtpFechaHoraOtorgamiento.Value = Convert.ToDateTime(fila("Fecha_Hora_Otorgamiento"))
            nudFolioInicio.Value = Math.Max(1, UtileriasUI.Entero(fila, "Folio_Inicio"))
            nudFolioFin.Value = Math.Max(1, UtileriasUI.Entero(fila, "Folio_Fin"))
            txtObjeto.Text = UtileriasUI.Texto(fila, "Objeto_Acto")
            SeleccionarTexto(cboEstadoInstrumento, UtileriasUI.Texto(fila, "Estado"))

            CargarComparecientes()
            ActualizarEstadoAccionesContextuales()
            If _comparecientesDisponibles Then MostrarMensaje("", True)
        Catch ex As Exception
            LimpiarInstrumento()
            RegistroErrores.Escribir("Protocolo - detalle de instrumento", ex)
            MostrarMensaje("No se pudo cargar el instrumento seleccionado. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub LimpiarInstrumento()
        _idInstrumento = 0
        _comparecientesDisponibles = False
        nudNumeroEscritura.Value = 1
        dtpFechaHoraOtorgamiento.Value = Date.Now
        nudFolioInicio.Value = 1
        nudFolioFin.Value = 1
        txtObjeto.Clear()
        cboEstadoInstrumento.SelectedIndex = 0
        dgvComparecientes.DataSource = Nothing
        SugerirSiguienteInstrumento()
        ActualizarEstadoAccionesContextuales()
    End Sub

    ''' <summary>
    ''' Propone el siguiente número de escritura y folio a partir de la
    ''' lista visible, para no repetir datos por descuido.
    ''' </summary>
    Private Sub SugerirSiguienteInstrumento()
        ' El grid está enlazado a un DataView, no directamente a un DataTable.
        ' Consultar la tabla subyacente evita que la sugerencia quede desactivada
        ' por un cast que siempre devuelve Nothing en el enlace actual.
        If _vistaInstrumentos Is Nothing OrElse _vistaInstrumentos.Table Is Nothing OrElse
           _vistaInstrumentos.Table.Rows.Count = 0 Then Return

        Dim mayorNumero As Integer = 0
        Dim mayorFolio As Integer = 0
        For Each fila As DataRow In _vistaInstrumentos.Table.Rows
            mayorNumero = Math.Max(mayorNumero, UtileriasUI.Entero(fila, "Numero_Escritura"))
            mayorFolio = Math.Max(mayorFolio, UtileriasUI.Entero(fila, "Folio_Fin"))
        Next

        nudNumeroEscritura.Value = Math.Min(CDec(mayorNumero) + 1D, nudNumeroEscritura.Maximum)
        nudFolioInicio.Value = Math.Min(CDec(mayorFolio) + 1D, nudFolioInicio.Maximum)
        nudFolioFin.Value = nudFolioInicio.Value
    End Sub

    Private Sub GuardarInstrumento()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        If _idLibro <= 0 Then
            UtileriasUI.MostrarError("Seleccione primero un libro del protocolo.")
            Return
        End If

        Try
            Dim instrumento As New Instrumento With {
                .ID = _idInstrumento,
                .IDLibro = _idLibro,
                .NumeroEscritura = Convert.ToInt32(nudNumeroEscritura.Value),
                .FechaHoraOtorgamiento = dtpFechaHoraOtorgamiento.Value,
                .FolioInicio = Convert.ToInt32(nudFolioInicio.Value),
                .FolioFin = Convert.ToInt32(nudFolioFin.Value),
                .ObjetoActo = txtObjeto.Text,
                .Estado = Convert.ToString(cboEstadoInstrumento.SelectedItem)
            }

            Dim respuesta As Respuesta = _servicio.GuardarInstrumento(instrumento)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            _idInstrumento = instrumento.ID
            CargarInstrumentos()
            CargarInstrumentosCombo()
            SeleccionarInstrumento(instrumento.ID)
            CargarComparecientes()
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - guardar instrumento", ex)
            MostrarMensaje("No se pudo guardar el instrumento. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub AnularInstrumento()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        If _idInstrumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un instrumento.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Anular el instrumento seleccionado? El historial se conserva.") Then Return

        Dim respuesta As Respuesta = _servicio.AnularInstrumento(_idInstrumento)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarInstrumentos()
        CargarInstrumentosCombo()
        CargarComparecientes()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub SeleccionarInstrumento(id As Integer)
        For Each fila As DataGridViewRow In dgvInstrumentos.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' El ID está oculto; seleccionar una celda visible evita errores al restaurar el foco.
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvInstrumentos.CurrentCell = celda
                    Return
                End If
            Next
            Return
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' Comparecientes
    ' ------------------------------------------------------------------

    Private Sub CargarComparecientes()
        If _idInstrumento <= 0 Then
            _comparecientesDisponibles = False
            dgvComparecientes.DataSource = Nothing
            ActualizarEstadoAccionesContextuales()
            Return
        End If

        _comparecientesDisponibles = False
        Try
            dgvComparecientes.DataSource = _servicio.Comparecientes(_idInstrumento)
            ConfigurarPresentacionGrid(dgvComparecientes)
            OcultarColumnas(dgvComparecientes, "ID", "ID_Cliente")
            _comparecientesDisponibles = True
        Catch ex As Exception
            dgvComparecientes.DataSource = Nothing
            RegistroErrores.Escribir("Protocolo - comparecientes", ex)
            MostrarMensaje("No se pudieron cargar los comparecientes. Las operaciones sobre esta lista permanecerán bloqueadas hasta recuperar la información.", False)
        Finally
            ActualizarEstadoAccionesContextuales()
        End Try
    End Sub

    Private Sub AgregarCompareciente()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        If _idInstrumento <= 0 Then
            UtileriasUI.MostrarError("Guarde primero el instrumento para agregar comparecientes.")
            Return
        End If

        Dim respuesta As Respuesta = _servicio.AgregarCompareciente(
            _idInstrumento,
            UtileriasUI.ValorCombo(cboClienteComp),
            Convert.ToString(cboCalidad.SelectedItem))

        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarComparecientes()
        ActualizarEstadoAccionesContextuales()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub QuitarCompareciente()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvComparecientes)
        If fila Is Nothing Then
            UtileriasUI.MostrarError("Seleccione un compareciente.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Retirar este compareciente de la escritura? Se conservarán el expediente del cliente y los demás registros. La operación no está permitida si ya se expidieron testimonios o el libro dejó de estar vigente.") Then Return

        Dim respuesta As Respuesta = _servicio.QuitarCompareciente(UtileriasUI.Entero(fila, "ID"))
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarComparecientes()
        ActualizarEstadoAccionesContextuales()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    ' ------------------------------------------------------------------
    ' Testimonios
    ' ------------------------------------------------------------------

    Private Sub CargarInstrumentosCombo()
        Try
            Dim tabla As New DataTable()
            tabla.Columns.Add("ID", GetType(Integer))
            tabla.Columns.Add("Descripcion", GetType(String))

            If _idLibro > 0 AndAlso Not String.Equals(Convert.ToString(cboEstadoLibro.SelectedItem), "Entregado", StringComparison.OrdinalIgnoreCase) Then
                For Each fila As DataRow In _servicio.Instrumentos(_idLibro).Rows
                    ' Solo las escrituras vigentes pueden recibir testimonios.
                    If Not String.Equals(UtileriasUI.Texto(fila, "Estado"), "Vigente", StringComparison.OrdinalIgnoreCase) Then Continue For
                    tabla.Rows.Add(
                        Convert.ToInt32(fila("ID")),
                        "Escritura " & Convert.ToString(fila("Numero_Escritura")) & " - " &
                        Recortar(Convert.ToString(fila("Objeto_Acto")), 60))
                Next
            End If

            UtileriasUI.CargarCombo(cboInstrumentoTest, tabla, "Descripcion", "ID")
            ActualizarLimitesFechaTestimonio()
            ActualizarEstadoAccionesContextuales()
        Catch ex As Exception
            cboInstrumentoTest.DataSource = Nothing
            ActualizarLimitesFechaTestimonio()
            ActualizarEstadoAccionesContextuales()
            RegistroErrores.Escribir("Protocolo - combo de instrumentos", ex)
            MostrarMensaje("No se pudieron cargar las escrituras disponibles para expedir testimonios. La expedición permanece deshabilitada.", False)
        End Try
    End Sub

    Private Function Recortar(texto As String, largo As Integer) As String
        If String.IsNullOrEmpty(texto) Then Return ""
        If texto.Length <= largo Then Return texto
        Return texto.Substring(0, largo) & "..."
    End Function

    Private Sub CargarTestimonios()
        If _idLibro <= 0 Then
            dgvTestimonios.DataSource = Nothing
            _vistaTestimonios = Nothing
            txtBuscarTestimonio.Clear()
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.Testimonios(_idLibro)
            _cargando = True
            txtBuscarTestimonio.Clear()
            _vistaTestimonios = New DataView(tabla)
            dgvTestimonios.DataSource = _vistaTestimonios
            ConfigurarPresentacionGrid(dgvTestimonios)
            OcultarColumnas(dgvTestimonios, "ID")
            _cargando = False
        Catch ex As Exception
            _cargando = False
            dgvTestimonios.DataSource = Nothing
            _vistaTestimonios = Nothing
            RegistroErrores.Escribir("Protocolo - testimonios", ex)
            MostrarMensaje("No se pudo cargar el historial de testimonios. Verifique la conexión e intente actualizar.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Busca dentro del historial ya cargado, sin alterar ni consultar de nuevo
    ''' los testimonios registrados.
    ''' </summary>
    Private Sub AplicarFiltroTestimoniosInterno()
        If _vistaTestimonios Is Nothing Then Return

        Dim filtro As String = If(txtBuscarTestimonio.Text, "").Trim()
        If filtro.Length = 0 Then
            _vistaTestimonios.RowFilter = ""
            Return
        End If

        Dim valor As String = filtro.Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]").Replace("'", "''")
        Dim patron As String = "'%" & valor & "%'"
        _vistaTestimonios.RowFilter =
            "CONVERT([Numero_Escritura], 'System.String') LIKE " & patron &
            " OR [Tipo_Testimonio] LIKE " & patron &
            " OR [Destinatario] LIKE " & patron &
            " OR [Usuario] LIKE " & patron
    End Sub

    Private Sub txtBuscarTestimonio_KeyDown(sender As Object, e As KeyEventArgs) Handles txtBuscarTestimonio.KeyDown
        If e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            txtBuscarTestimonio.Clear()
            txtBuscarTestimonio.Focus()
        End If
    End Sub

    Private Sub txtBuscarTestimonio_TextChanged(sender As Object, e As EventArgs) Handles txtBuscarTestimonio.TextChanged
        If _cargando OrElse _vistaTestimonios Is Nothing Then Return

        Try
            AplicarFiltroTestimoniosInterno()
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - filtro de testimonios", ex)
            MostrarMensaje("No se pudo aplicar el filtro del historial de testimonios.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Ajusta el rango de fecha del testimonio a la fecha de otorgamiento
    ''' de la escritura seleccionada y al día actual.
    ''' </summary>
    Private Sub ActualizarLimitesFechaTestimonio()
        Try
            dtpFechaTestimonio.MaxDate = Date.Today
            Dim fechaMinima As Date = DateTimePicker.MinimumDateTime
            Dim idInstrumento As Integer = UtileriasUI.ValorCombo(cboInstrumentoTest)
            If idInstrumento > 0 Then
                Dim instrumento As DataTable = _servicio.ObtenerInstrumento(idInstrumento)
                If instrumento.Rows.Count > 0 AndAlso Not instrumento.Rows(0).IsNull("Fecha_Hora_Otorgamiento") Then
                    fechaMinima = Convert.ToDateTime(instrumento.Rows(0)("Fecha_Hora_Otorgamiento")).Date
                End If
            End If
            If fechaMinima > Date.Today Then fechaMinima = Date.Today
            dtpFechaTestimonio.MinDate = fechaMinima
            If dtpFechaTestimonio.Value.Date < fechaMinima Then
                dtpFechaTestimonio.Value = fechaMinima
            ElseIf dtpFechaTestimonio.Value.Date > Date.Today Then
                dtpFechaTestimonio.Value = Date.Today
            End If
        Catch ex As Exception
            dtpFechaTestimonio.MinDate = DateTimePicker.MinimumDateTime
            If dtpFechaTestimonio.Value.Date > Date.Today Then dtpFechaTestimonio.Value = Date.Today
            RegistroErrores.Escribir("Protocolo - límites de fecha del testimonio", ex)
        End Try
    End Sub
    Private Sub ExpedirTestimonio()
        If Not Permisos.ExigirAccion("protocolo.testimonios") Then Return

        ActualizarLimitesFechaTestimonio()
        Dim respuesta As Respuesta = _servicio.ExpedirTestimonio(
            UtileriasUI.ValorCombo(cboInstrumentoTest),
            Convert.ToString(cboTipoTestimonio.SelectedItem),
            dtpFechaTestimonio.Value.Date,
            txtDestinatario.Text)

        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        txtDestinatario.Clear()
        CargarTestimonios()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    ' ------------------------------------------------------------------
    ' Índice
    ' ------------------------------------------------------------------

    Private Sub CargarIndice()
        If _idLibro <= 0 Then
            dgvIndice.DataSource = Nothing
            Return
        End If

        Try
            dgvIndice.DataSource = _servicio.IndiceLibro(_idLibro)
            ConfigurarPresentacionGrid(dgvIndice)
            If dgvIndice.Columns.Contains("Fecha_Hora_Otorgamiento") Then
                dgvIndice.Columns("Fecha_Hora_Otorgamiento").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"
            End If
        Catch ex As Exception
            dgvIndice.DataSource = Nothing
            RegistroErrores.Escribir("Protocolo - índice", ex)
            MostrarMensaje("No se pudo cargar el índice del libro. La exportación permanecerá deshabilitada hasta recuperar la información.", False)
            ActualizarEstadoAccionesContextuales()
        End Try
    End Sub

    Private Sub GenerarIndice()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        Dim respuesta As Respuesta = _servicio.GenerarIndice(_idLibro)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        ' La marca "Indice_Generado" solo se confirma después de que el CSV
        ' se haya escrito correctamente en la ubicación elegida.
        ExportarIndice(True)
    End Sub

    Private Sub ExportarIndice(Optional confirmarGeneracion As Boolean = False)
        ' La disponibilidad visual del botón no es una autorización: revalidar
        ' el permiso en la acción y nuevamente después del diálogo modal.
        If confirmarGeneracion Then
            If Not Permisos.TieneAcceso("protocolo.gestionar") Then
                MostrarMensaje("Su sesión ya no tiene permiso para generar el índice.", False)
                Return
            End If
        ElseIf Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar") Then
            MostrarMensaje("Su sesión ya no tiene permiso para exportar el índice.", False)
            Return
        End If

        Dim tabla As DataTable = TryCast(dgvIndice.DataSource, DataTable)
        If tabla Is Nothing OrElse tabla.Rows.Count = 0 Then
            UtileriasUI.MostrarError("No hay índice para exportar.")
            Return
        End If

        Using dialogo As New SaveFileDialog()
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv"
            dialogo.FileName = "Indice_Libro_" & _idLibro & ".csv"
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return

            ' El usuario puede permanecer en el diálogo mientras un administrador
            ' revoca el permiso desde otra sesión o estación.
            If confirmarGeneracion Then
                If Not Permisos.TieneAcceso("protocolo.gestionar") Then
                    MostrarMensaje("El permiso para generar el índice fue revocado. No se exportó el archivo.", False)
                    Return
                End If
            ElseIf Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar") Then
                MostrarMensaje("El permiso para exportar el índice fue revocado. No se exportó el archivo.", False)
                Return
            End If

            Try
                UtileriasUI.ExportarCsv(tabla, dialogo.FileName)

                If confirmarGeneracion Then
                    Dim confirmacion As Respuesta = _servicio.ConfirmarIndiceGenerado(_idLibro)
                    If Not confirmacion.Exito Then
                        MostrarMensaje("El CSV se exportó en " & dialogo.FileName &
                                       ", pero no se pudo registrar la generación: " &
                                       confirmacion.Mensaje, False)
                        Return
                    End If

                    _cargando = True
                    CargarLibros(_idLibro)
                    _cargando = False
                    MostrarMensaje("Índice exportado y generación registrada en " &
                                   dialogo.FileName, True)
                Else
                    MostrarMensaje("Índice exportado en " & dialogo.FileName, True)
                End If
            Catch ex As Exception
                RegistroErrores.Escribir("Protocolo - exportar índice", ex)
                MostrarMensaje("No se pudo exportar el índice. Verifique la ruta y los permisos de escritura.", False)
            End Try
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' Utilidades
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' Normaliza la presentación de las tablas notariales sin modificar los
    ''' nombres de las columnas devueltas por el repositorio.
    ''' </summary>
    ''' <summary>
    ''' Refuerza la lectura del estado de libros y escrituras. El texto del
    ''' estado permanece visible; el color es una ayuda adicional, no el único indicador.
    ''' </summary>
    Private Sub dgvProtocolo_RowPrePaint(sender As Object, e As DataGridViewRowPrePaintEventArgs)
        Dim grid As DataGridView = TryCast(sender, DataGridView)
        If grid Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= grid.Rows.Count Then Return
        If Not grid.Columns.Contains("Estado") Then Return

        Dim fila As DataGridViewRow = grid.Rows(e.RowIndex)
        Dim estado As String = Convert.ToString(fila.Cells("Estado").Value)
        Select Case estado.Trim().ToUpperInvariant()
            Case "VIGENTE"
                fila.DefaultCellStyle.BackColor = Color.AliceBlue
                fila.DefaultCellStyle.ForeColor = Color.DarkBlue
            Case "ANULADO"
                fila.DefaultCellStyle.BackColor = Color.Gainsboro
                fila.DefaultCellStyle.ForeColor = Color.DimGray
            Case "CERRADO"
                fila.DefaultCellStyle.BackColor = Color.LemonChiffon
                fila.DefaultCellStyle.ForeColor = Color.Black
            Case "ENTREGADO"
                fila.DefaultCellStyle.BackColor = Color.Honeydew
                fila.DefaultCellStyle.ForeColor = Color.DarkGreen
            Case Else
                fila.DefaultCellStyle.BackColor = grid.DefaultCellStyle.BackColor
                fila.DefaultCellStyle.ForeColor = grid.DefaultCellStyle.ForeColor
        End Select
    End Sub

    Private Sub ConfigurarPresentacionGrid(grid As DataGridView)
        If grid Is Nothing OrElse grid.Columns.Count = 0 Then Return

        For Each columna As DataGridViewColumn In grid.Columns
            columna.SortMode = DataGridViewColumnSortMode.Automatic
            columna.HeaderText = columna.HeaderText.Replace("_", " ")
            columna.MinimumWidth = 55
            columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            columna.DefaultCellStyle.Padding = New Padding(3, 2, 3, 2)
        Next

        ' Evitar que las columnas de texto legal compriman fechas, folios
        ' y estados. El detalle conserva varias líneas cuando es extenso.
        If grid.Columns.Contains("Objeto_Acto") Then
            With grid.Columns("Objeto_Acto")
                .FillWeight = 210
                .MinimumWidth = 220
                .DefaultCellStyle.WrapMode = DataGridViewTriState.True
            End With
        End If
        If grid.Columns.Contains("Destinatario") Then
            With grid.Columns("Destinatario")
                .FillWeight = 170
                .MinimumWidth = 150
                .DefaultCellStyle.WrapMode = DataGridViewTriState.True
            End With
        End If
        If grid.Columns.Contains("Otorgantes") Then
            With grid.Columns("Otorgantes")
                .FillWeight = 180
                .MinimumWidth = 170
                .DefaultCellStyle.WrapMode = DataGridViewTriState.True
            End With
        End If
        If grid.Columns.Contains("Fecha_Hora_Otorgamiento") Then
            grid.Columns("Fecha_Hora_Otorgamiento").MinimumWidth = 145
            grid.Columns("Fecha_Hora_Otorgamiento").FillWeight = 105
        End If
        If grid.Columns.Contains("Estado") Then
            grid.Columns("Estado").MinimumWidth = 85
            grid.Columns("Estado").FillWeight = 65
            grid.Columns("Estado").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If
        If grid.Columns.Contains("Folio_Inicio") Then grid.Columns("Folio_Inicio").FillWeight = 65
        If grid.Columns.Contains("Folio_Fin") Then grid.Columns("Folio_Fin").FillWeight = 65

        If grid.Columns.Contains("Objeto_Acto") OrElse
           grid.Columns.Contains("Destinatario") OrElse
           grid.Columns.Contains("Otorgantes") Then
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        Else
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            grid.RowTemplate.Height = 26
        End If

        If grid.Columns.Contains("Numero_Libro") Then grid.Columns("Numero_Libro").HeaderText = "Libro"
        If grid.Columns.Contains("Anio_Vigencia") Then grid.Columns("Anio_Vigencia").HeaderText = "Año"
        If grid.Columns.Contains("Fecha_Autorizacion") Then
            grid.Columns("Fecha_Autorizacion").HeaderText = "Autorización"
            grid.Columns("Fecha_Autorizacion").DefaultCellStyle.Format = "dd/MM/yyyy"
        End If
        If grid.Columns.Contains("Fecha_Vencimiento") Then
            grid.Columns("Fecha_Vencimiento").HeaderText = "Vencimiento"
            grid.Columns("Fecha_Vencimiento").DefaultCellStyle.Format = "dd/MM/yyyy"
        End If
        If grid.Columns.Contains("Numero_Escritura") Then grid.Columns("Numero_Escritura").HeaderText = "Escritura"
        If grid.Columns.Contains("Fecha_Hora_Otorgamiento") Then
            grid.Columns("Fecha_Hora_Otorgamiento").HeaderText = "Otorgamiento"
            grid.Columns("Fecha_Hora_Otorgamiento").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"
        End If
        If grid.Columns.Contains("Folio_Inicio") Then grid.Columns("Folio_Inicio").HeaderText = "Folio inicial"
        If grid.Columns.Contains("Folio_Fin") Then grid.Columns("Folio_Fin").HeaderText = "Folio final"
        If grid.Columns.Contains("Objeto_Acto") Then grid.Columns("Objeto_Acto").HeaderText = "Acto / objeto"
        If grid.Columns.Contains("Comparecientes") Then grid.Columns("Comparecientes").HeaderText = "Comparecientes"
        If grid.Columns.Contains("Cliente") Then grid.Columns("Cliente").HeaderText = "Compareciente"
        If grid.Columns.Contains("DUI") Then grid.Columns("DUI").HeaderText = "DUI"
        If grid.Columns.Contains("Calidad") Then grid.Columns("Calidad").HeaderText = "Calidad"
        If grid.Columns.Contains("Tipo_Testimonio") Then grid.Columns("Tipo_Testimonio").HeaderText = "Tipo"
        If grid.Columns.Contains("Fecha_Expedicion") Then
            grid.Columns("Fecha_Expedicion").HeaderText = "Expedición"
            grid.Columns("Fecha_Expedicion").DefaultCellStyle.Format = "dd/MM/yyyy"
        End If
        If grid.Columns.Contains("Destinatario") Then grid.Columns("Destinatario").HeaderText = "Destinatario"
        If grid.Columns.Contains("Usuario") Then grid.Columns("Usuario").HeaderText = "Registrado por"
        If grid.Columns.Contains("Otorgantes") Then grid.Columns("Otorgantes").HeaderText = "Otorgantes"
        If grid.Columns.Contains("Estado") Then grid.Columns("Estado").HeaderText = "Estado"
    End Sub

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

    Private Sub cboLibro_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboLibro.SelectedIndexChanged
        If _cargando Then Return
        CargarLibroSeleccionado()
    End Sub

    Private Sub dgvLibros_SelectionChanged(sender As Object, e As EventArgs) Handles dgvLibros.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvLibros)

        If fila Is Nothing Then
            ' Si se pierde la selección, retirar también el libro del combo y
            ' limpiar sus instrumentos, testimonios e índice. Así ningún botón
            ' puede operar sobre el último libro que quedó cargado.
            If _idLibro > 0 Then
                _cargando = True
                cboLibro.SelectedIndex = -1
                _cargando = False
                CargarLibroSeleccionado()
            End If
            Return
        End If

        Dim id As Integer = UtileriasUI.Entero(fila, "ID")
        If id = _idLibro Then Return

        _cargando = True
        UtileriasUI.SeleccionarValor(cboLibro, id)
        _cargando = False
        CargarLibroSeleccionado()
    End Sub

    ''' <summary>
    ''' Sincroniza la disponibilidad de acciones con el contexto real de la
    ''' selección. La validación definitiva permanece en ServicioProtocolo.
    ''' </summary>
    Private Sub ActualizarEstadoAccionesContextuales()
        If Not _permisosInicializados AndAlso
           Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar",
                                         "protocolo.cerrar_libro", "protocolo.entregar",
                                         "protocolo.testimonios") Then Return

        Dim puedeGestionar As Boolean = Permisos.Puede("protocolo.gestionar")
        Dim puedeCerrar As Boolean = Permisos.Puede("protocolo.cerrar_libro")
        Dim puedeEntregar As Boolean = Permisos.Puede("protocolo.entregar")
        Dim puedeTestimonios As Boolean = Permisos.Puede("protocolo.testimonios")

        Dim estadoLibro As String = Convert.ToString(cboEstadoLibro.SelectedItem)
        Dim libroSeleccionado As Boolean = _idLibro > 0
        Dim libroVigente As Boolean = libroSeleccionado AndAlso String.Equals(estadoLibro, "Vigente", StringComparison.OrdinalIgnoreCase)
        Dim libroCerrado As Boolean = libroSeleccionado AndAlso String.Equals(estadoLibro, "Cerrado", StringComparison.OrdinalIgnoreCase)
        Dim libroEntregado As Boolean = libroSeleccionado AndAlso String.Equals(estadoLibro, "Entregado", StringComparison.OrdinalIgnoreCase)

        Dim instrumentoSeleccionado As Boolean = _idInstrumento > 0
        Dim instrumentoVigente As Boolean = instrumentoSeleccionado AndAlso
            String.Equals(Convert.ToString(cboEstadoInstrumento.SelectedItem), "Vigente", StringComparison.OrdinalIgnoreCase)
        Dim comparecienteSeleccionado As Boolean = UtileriasUI.FilaActual(dgvComparecientes) IsNot Nothing
        Dim testimonioSeleccionado As Boolean = UtileriasUI.ValorCombo(cboInstrumentoTest) > 0
        Dim tablaIndice As DataTable = TryCast(dgvIndice.DataSource, DataTable)
        Dim indiceDisponible As Boolean = tablaIndice IsNot Nothing AndAlso tablaIndice.Rows.Count > 0

        txtRazonCierre.Enabled = puedeCerrar AndAlso libroVigente
        btnCerrarLibro.Enabled = puedeCerrar AndAlso libroVigente
        dtpFechaEntrega.Enabled = puedeEntregar AndAlso libroCerrado
        btnEntregarLibro.Enabled = puedeEntregar AndAlso libroCerrado

        nudNumeroEscritura.Enabled = puedeGestionar AndAlso libroVigente
        dtpFechaHoraOtorgamiento.Enabled = puedeGestionar AndAlso libroVigente
        nudFolioInicio.Enabled = puedeGestionar AndAlso libroVigente
        nudFolioFin.Enabled = puedeGestionar AndAlso libroVigente
        txtObjeto.Enabled = puedeGestionar AndAlso libroVigente
        btnNuevoInstrumento.Enabled = puedeGestionar AndAlso libroVigente
        btnGuardarInstrumento.Enabled = puedeGestionar AndAlso libroVigente
        ' La anulación se permite hasta antes de registrar la entrega del libro.
        btnAnularInstrumento.Enabled = puedeGestionar AndAlso
            (libroVigente OrElse libroCerrado) AndAlso instrumentoVigente

        cboClienteComp.Enabled = puedeGestionar AndAlso instrumentoVigente
        cboCalidad.Enabled = puedeGestionar AndAlso instrumentoVigente
        btnAgregarComp.Enabled = puedeGestionar AndAlso instrumentoVigente AndAlso _comparecientesDisponibles
        btnQuitarComp.Enabled = puedeGestionar AndAlso instrumentoVigente AndAlso _comparecientesDisponibles AndAlso comparecienteSeleccionado

        grpTestimonio.Enabled = puedeTestimonios AndAlso
            (libroVigente OrElse libroCerrado) AndAlso cboInstrumentoTest.Items.Count > 0
        btnExpedirTestimonio.Enabled = puedeTestimonios AndAlso
            (libroVigente OrElse libroCerrado) AndAlso testimonioSeleccionado

        btnGenerarIndice.Enabled = puedeGestionar AndAlso (libroCerrado OrElse libroEntregado)
        btnExportarIndice.Enabled = indiceDisponible AndAlso
            (Permisos.Puede("protocolo.ver") OrElse puedeGestionar)
    End Sub

    Private Sub dgvComparecientes_SelectionChanged(sender As Object, e As EventArgs) Handles dgvComparecientes.SelectionChanged
        If _cargando Then Return
        ActualizarEstadoAccionesContextuales()
    End Sub

    Private Sub cboInstrumentoTest_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboInstrumentoTest.SelectedIndexChanged
        If _cargando Then Return
        ActualizarLimitesFechaTestimonio()
        ActualizarEstadoAccionesContextuales()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarClientes()
        CargarLibros(_idLibro)
    End Sub

    Private Sub btnNuevoLibro_Click(sender As Object, e As EventArgs) Handles btnNuevoLibro.Click
        ' Iniciar un libro nuevo también debe retirar todo el contexto del libro
        ' anterior para evitar que queden escrituras, comparecientes o testimonios
        ' visibles y parezcan pertenecer al registro que se está creando.
        _cargando = True
        cboLibro.SelectedIndex = -1
        dgvLibros.CurrentCell = Nothing
        _cargando = False

        LimpiarLibro()
        LimpiarInstrumento()
        cboInstrumentoTest.DataSource = Nothing
        ActualizarLimitesFechaTestimonio()
        dgvInstrumentos.DataSource = Nothing
        dgvComparecientes.DataSource = Nothing
        dgvTestimonios.DataSource = Nothing
        dgvIndice.DataSource = Nothing

        ActualizarEstadoAccionesContextuales()
        nudNumeroLibro.Focus()
    End Sub

    Private Sub btnGuardarLibro_Click(sender As Object, e As EventArgs) Handles btnGuardarLibro.Click
        UtileriasUI.EjecutarUnaVez(btnGuardarLibro, AddressOf GuardarLibro)
    End Sub

    Private Sub btnCerrarLibro_Click(sender As Object, e As EventArgs) Handles btnCerrarLibro.Click
        UtileriasUI.EjecutarUnaVez(btnCerrarLibro, AddressOf CerrarLibro)
    End Sub

    Private Sub btnEntregarLibro_Click(sender As Object, e As EventArgs) Handles btnEntregarLibro.Click
        UtileriasUI.EjecutarUnaVez(btnEntregarLibro, AddressOf EntregarLibro)
    End Sub

    Private Sub btnNuevoInstrumento_Click(sender As Object, e As EventArgs) Handles btnNuevoInstrumento.Click
        LimpiarInstrumento()
        nudNumeroEscritura.Focus()
    End Sub

    Private Sub btnGuardarInstrumento_Click(sender As Object, e As EventArgs) Handles btnGuardarInstrumento.Click
        UtileriasUI.EjecutarUnaVez(btnGuardarInstrumento, AddressOf GuardarInstrumento)
    End Sub

    Private Sub btnAnularInstrumento_Click(sender As Object, e As EventArgs) Handles btnAnularInstrumento.Click
        UtileriasUI.EjecutarUnaVez(btnAnularInstrumento, AddressOf AnularInstrumento)
    End Sub

    Private Sub btnAgregarComp_Click(sender As Object, e As EventArgs) Handles btnAgregarComp.Click
        UtileriasUI.EjecutarUnaVez(btnAgregarComp, AddressOf AgregarCompareciente)
    End Sub

    Private Sub btnQuitarComp_Click(sender As Object, e As EventArgs) Handles btnQuitarComp.Click
        UtileriasUI.EjecutarUnaVez(btnQuitarComp, AddressOf QuitarCompareciente)
    End Sub

    Private Sub btnExpedirTestimonio_Click(sender As Object, e As EventArgs) Handles btnExpedirTestimonio.Click
        UtileriasUI.EjecutarUnaVez(btnExpedirTestimonio, AddressOf ExpedirTestimonio)
    End Sub

    Private Sub btnGenerarIndice_Click(sender As Object, e As EventArgs) Handles btnGenerarIndice.Click
        UtileriasUI.EjecutarUnaVez(btnGenerarIndice, AddressOf GenerarIndice)
    End Sub

    Private Sub btnExportarIndice_Click(sender As Object, e As EventArgs) Handles btnExportarIndice.Click
        UtileriasUI.EjecutarUnaVez(btnExportarIndice, Sub() ExportarIndice())
    End Sub

End Class
