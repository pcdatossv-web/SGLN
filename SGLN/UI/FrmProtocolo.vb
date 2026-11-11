Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmProtocolo

    Private ReadOnly _servicio As New ServicioProtocolo()
    Private _cargando As Boolean
    Private _idLibro As Integer
    Private _idInstrumento As Integer

    Private Sub FrmProtocolo_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("protocolo.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvLibros)
        UtileriasUI.ConfigurarGrid(dgvInstrumentos)
        UtileriasUI.ConfigurarGrid(dgvComparecientes)
        UtileriasUI.ConfigurarGrid(dgvTestimonios)
        UtileriasUI.ConfigurarGrid(dgvIndice)

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
        CargarLibros(0)
        _cargando = False

        LimpiarLibro()
        LimpiarInstrumento()
        MostrarMensaje("", True)
    End Sub

    ' ------------------------------------------------------------------
    ' Carga general
    ' ------------------------------------------------------------------

    Private Sub CargarClientes()
        Try
            UtileriasUI.CargarCombo(cboClienteComp, _servicio.ClientesCombo(), "Nombre", "ID")
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - clientes", ex)
        End Try
    End Sub

    Private Sub CargarLibros(seleccionar As Integer)
        Try
            Dim tabla As DataTable = _servicio.Libros()

            _cargando = True
            dgvLibros.DataSource = tabla
            OcultarColumnas(dgvLibros, "ID", "Razon_Cierre")

            UtileriasUI.CargarCombo(cboLibro, TablaLibrosCombo(tabla), "Descripcion", "ID")
            If seleccionar > 0 Then
                UtileriasUI.SeleccionarValor(cboLibro, seleccionar)
            ElseIf cboLibro.Items.Count > 0 Then
                cboLibro.SelectedIndex = 0
            End If
            _cargando = False

            CargarLibroSeleccionado()
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Protocolo - libros", ex)
            MostrarMensaje("No se pudieron cargar los libros: " & ex.Message, False)
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
    End Sub

    Private Sub CargarLibroEditor(id As Integer)
        If id <= 0 Then
            LimpiarLibro()
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.ObtenerLibro(id)
            If tabla.Rows.Count = 0 Then Return

            Dim fila As DataRow = tabla.Rows(0)
            nudNumeroLibro.Value = Math.Max(1, UtileriasUI.Entero(fila, "Numero_Libro"))
            nudAnio.Value = UtileriasUI.Entero(fila, "Anio_Vigencia")
            dtpFechaAutorizacion.Value = UtileriasUI.Fecha(fila, "Fecha_Autorizacion").GetValueOrDefault(Date.Today)
            dtpFechaVencimiento.Value = UtileriasUI.Fecha(fila, "Fecha_Vencimiento").GetValueOrDefault(Date.Today.AddYears(1))
            SeleccionarTexto(cboEstadoLibro, UtileriasUI.Texto(fila, "Estado"))
            txtRazonCierre.Text = UtileriasUI.Texto(fila, "Razon_Cierre")

            Dim cierre As Date? = UtileriasUI.Fecha(fila, "Fecha_Cierre")
            txtFechaCierre.Text = If(cierre.HasValue, cierre.Value.ToString("dd/MM/yyyy"), "Sin cerrar")

            Dim entrega As Date? = UtileriasUI.Fecha(fila, "Fecha_Entrega_Notariado")
            If entrega.HasValue Then dtpFechaEntrega.Value = entrega.Value

            lblLimiteEntrega.Text = ""
            If cierre.HasValue Then
                Dim limite As Date = _servicio.FechaLimiteEntrega(cierre.Value)
                lblLimiteEntrega.Text = "Fecha límite referencial de entrega al Notariado: " &
                    limite.ToString("dd/MM/yyyy") & " (" & _servicio.DiasEntregaConfigurados() & " días hábiles)."
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - editor de libro", ex)
            MostrarMensaje("No se pudo cargar el libro: " & ex.Message, False)
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
            MostrarMensaje("No se pudo guardar el libro: " & ex.Message, False)
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

        If Not UtileriasUI.Confirmar("¿Registrar la entrega del libro al Notariado el " &
                                     dtpFechaEntrega.Value.ToString("dd/MM/yyyy") & "?") Then Return

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
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.Instrumentos(_idLibro)
            _cargando = True
            dgvInstrumentos.DataSource = tabla
            OcultarColumnas(dgvInstrumentos, "ID")
            _cargando = False
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Protocolo - instrumentos", ex)
            MostrarMensaje("No se pudieron cargar los instrumentos: " & ex.Message, False)
        End Try
    End Sub

    Private Sub dgvInstrumentos_SelectionChanged(sender As Object, e As EventArgs) Handles dgvInstrumentos.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvInstrumentos)
        If fila Is Nothing Then Return
        CargarInstrumento(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarInstrumento(id As Integer)
        Try
            Dim tabla As DataTable = _servicio.ObtenerInstrumento(id)
            If tabla.Rows.Count = 0 Then Return

            Dim fila As DataRow = tabla.Rows(0)
            _idInstrumento = id

            nudNumeroEscritura.Value = Math.Max(1, UtileriasUI.Entero(fila, "Numero_Escritura"))
            dtpFechaHoraOtorgamiento.Value = Convert.ToDateTime(fila("Fecha_Hora_Otorgamiento"))
            nudFolioInicio.Value = Math.Max(1, UtileriasUI.Entero(fila, "Folio_Inicio"))
            nudFolioFin.Value = Math.Max(1, UtileriasUI.Entero(fila, "Folio_Fin"))
            txtObjeto.Text = UtileriasUI.Texto(fila, "Objeto_Acto")
            SeleccionarTexto(cboEstadoInstrumento, UtileriasUI.Texto(fila, "Estado"))

            CargarComparecientes()
            MostrarMensaje("", True)
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - detalle de instrumento", ex)
            MostrarMensaje("No se pudo cargar el instrumento: " & ex.Message, False)
        End Try
    End Sub

    Private Sub LimpiarInstrumento()
        _idInstrumento = 0
        nudNumeroEscritura.Value = 1
        dtpFechaHoraOtorgamiento.Value = Date.Now
        nudFolioInicio.Value = 1
        nudFolioFin.Value = 1
        txtObjeto.Clear()
        cboEstadoInstrumento.SelectedIndex = 0
        dgvComparecientes.DataSource = Nothing
        SugerirSiguienteInstrumento()
    End Sub

    ''' <summary>
    ''' Propone el siguiente número de escritura y folio a partir de la
    ''' lista visible, para no repetir datos por descuido.
    ''' </summary>
    Private Sub SugerirSiguienteInstrumento()
        Dim tabla As DataTable = TryCast(dgvInstrumentos.DataSource, DataTable)
        If tabla Is Nothing OrElse tabla.Rows.Count = 0 Then Return

        Dim mayorNumero As Integer = 0
        Dim mayorFolio As Integer = 0
        For Each fila As DataRow In tabla.Rows
            mayorNumero = Math.Max(mayorNumero, UtileriasUI.Entero(fila, "Numero_Escritura"))
            mayorFolio = Math.Max(mayorFolio, UtileriasUI.Entero(fila, "Folio_Fin"))
        Next

        nudNumeroEscritura.Value = Math.Min(mayorNumero + 1, nudNumeroEscritura.Maximum)
        nudFolioInicio.Value = Math.Min(mayorFolio + 1, nudFolioInicio.Maximum)
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
            MostrarMensaje("No se pudo guardar el instrumento: " & ex.Message, False)
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
            If vista IsNot Nothing AndAlso Convert.ToInt32(vista.Row("ID")) = id Then
                dgvInstrumentos.CurrentCell = fila.Cells(0)
                Return
            End If
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' Comparecientes
    ' ------------------------------------------------------------------

    Private Sub CargarComparecientes()
        If _idInstrumento <= 0 Then
            dgvComparecientes.DataSource = Nothing
            Return
        End If

        Try
            dgvComparecientes.DataSource = _servicio.Comparecientes(_idInstrumento)
            OcultarColumnas(dgvComparecientes, "ID", "ID_Cliente")
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - comparecientes", ex)
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
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub QuitarCompareciente()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvComparecientes)
        If fila Is Nothing Then
            UtileriasUI.MostrarError("Seleccione un compareciente.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Quitar el compareciente seleccionado?") Then Return

        Dim respuesta As Respuesta = _servicio.QuitarCompareciente(UtileriasUI.Entero(fila, "ID"))
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarComparecientes()
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

            If _idLibro > 0 Then
                For Each fila As DataRow In _servicio.Instrumentos(_idLibro).Rows
                    tabla.Rows.Add(
                        Convert.ToInt32(fila("ID")),
                        "Escritura " & Convert.ToString(fila("Numero_Escritura")) & " - " &
                        Recortar(Convert.ToString(fila("Objeto_Acto")), 60))
                Next
            End If

            UtileriasUI.CargarCombo(cboInstrumentoTest, tabla, "Descripcion", "ID")
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - combo de instrumentos", ex)
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
            Return
        End If

        Try
            dgvTestimonios.DataSource = _servicio.Testimonios(_idLibro)
            OcultarColumnas(dgvTestimonios, "ID")
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - testimonios", ex)
        End Try
    End Sub

    Private Sub ExpedirTestimonio()
        If Not Permisos.ExigirAccion("protocolo.testimonios") Then Return

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
            If dgvIndice.Columns.Contains("Fecha_Hora_Otorgamiento") Then
                dgvIndice.Columns("Fecha_Hora_Otorgamiento").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Protocolo - índice", ex)
        End Try
    End Sub

    Private Sub GenerarIndice()
        If Not Permisos.ExigirAccion("protocolo.gestionar") Then Return

        Dim respuesta As Respuesta = _servicio.GenerarIndice(_idLibro)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        _cargando = True
        CargarLibros(_idLibro)
        _cargando = False
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub ExportarIndice()
        Dim tabla As DataTable = TryCast(dgvIndice.DataSource, DataTable)
        If tabla Is Nothing OrElse tabla.Rows.Count = 0 Then
            UtileriasUI.MostrarError("No hay índice para exportar.")
            Return
        End If

        Using dialogo As New SaveFileDialog()
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv"
            dialogo.FileName = "Indice_Libro_" & _idLibro & ".csv"
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return

            Try
                UtileriasUI.ExportarCsv(tabla, dialogo.FileName)
                MostrarMensaje("Índice exportado en " & dialogo.FileName, True)
            Catch ex As Exception
                RegistroErrores.Escribir("Protocolo - exportar índice", ex)
                MostrarMensaje("No se pudo exportar el índice: " & ex.Message, False)
            End Try
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' Utilidades
    ' ------------------------------------------------------------------

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
        If fila Is Nothing Then Return

        Dim id As Integer = UtileriasUI.Entero(fila, "ID")
        If id = _idLibro Then Return

        _cargando = True
        UtileriasUI.SeleccionarValor(cboLibro, id)
        _cargando = False
        CargarLibroSeleccionado()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarClientes()
        CargarLibros(_idLibro)
    End Sub

    Private Sub btnNuevoLibro_Click(sender As Object, e As EventArgs) Handles btnNuevoLibro.Click
        LimpiarLibro()
        nudNumeroLibro.Focus()
    End Sub

    Private Sub btnGuardarLibro_Click(sender As Object, e As EventArgs) Handles btnGuardarLibro.Click
        GuardarLibro()
    End Sub

    Private Sub btnCerrarLibro_Click(sender As Object, e As EventArgs) Handles btnCerrarLibro.Click
        CerrarLibro()
    End Sub

    Private Sub btnEntregarLibro_Click(sender As Object, e As EventArgs) Handles btnEntregarLibro.Click
        EntregarLibro()
    End Sub

    Private Sub btnNuevoInstrumento_Click(sender As Object, e As EventArgs) Handles btnNuevoInstrumento.Click
        LimpiarInstrumento()
        nudNumeroEscritura.Focus()
    End Sub

    Private Sub btnGuardarInstrumento_Click(sender As Object, e As EventArgs) Handles btnGuardarInstrumento.Click
        GuardarInstrumento()
    End Sub

    Private Sub btnAnularInstrumento_Click(sender As Object, e As EventArgs) Handles btnAnularInstrumento.Click
        AnularInstrumento()
    End Sub

    Private Sub btnAgregarComp_Click(sender As Object, e As EventArgs) Handles btnAgregarComp.Click
        AgregarCompareciente()
    End Sub

    Private Sub btnQuitarComp_Click(sender As Object, e As EventArgs) Handles btnQuitarComp.Click
        QuitarCompareciente()
    End Sub

    Private Sub btnExpedirTestimonio_Click(sender As Object, e As EventArgs) Handles btnExpedirTestimonio.Click
        ExpedirTestimonio()
    End Sub

    Private Sub btnGenerarIndice_Click(sender As Object, e As EventArgs) Handles btnGenerarIndice.Click
        GenerarIndice()
    End Sub

    Private Sub btnExportarIndice_Click(sender As Object, e As EventArgs) Handles btnExportarIndice.Click
        ExportarIndice()
    End Sub

End Class
