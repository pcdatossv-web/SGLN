Imports System.Collections.Generic
Imports System
Imports System.Data
Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmDocumentos

    Private ReadOnly _servicio As New ServicioDocumentos()
    Private ReadOnly _servicioIA As New ServicioIA()
    Private _cargando As Boolean
    Private _idDocumento As Integer
    Private _puedeVer As Boolean
    Private _puedeImportar As Boolean
    Private _puedeMetadatos As Boolean
    Private _puedeVerificar As Boolean
    Private _permisosInicializados As Boolean
    Private _catalogosDisponibles As Boolean

    Private Sub FrmDocumentos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Revalidar la sesión una vez al abrir y evaluar la matriz recién cargada.
        ' Cada acción crítica vuelve a comprobar su permiso en el servicio.
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        _puedeVer = Permisos.Puede("documentos.ver")
        _puedeImportar = Permisos.Puede("documentos.importar")
        _puedeVerificar = Permisos.Puede("documentos.verificar")
        _puedeMetadatos = Permisos.Puede("documentos.metadatos")

        If Not (_puedeVer OrElse _puedeImportar OrElse _puedeVerificar OrElse _puedeMetadatos) Then
            If Not Permisos.Exigir("documentos.ver", Me) Then Return
        End If

        AplicarPermisosInterfaz()

        UtileriasUI.ConfigurarGrid(dgvDocumentos)

        Dim tipos As String() = TiposDocumento()
        cboTipoDocumento.Items.Clear()
        cboTipoDocumento.Items.AddRange(CType(tipos, Object()))
        cboTipoDocumento.SelectedIndex = 0

        cboFiltroTipo.Items.Clear()
        cboFiltroTipo.Items.Add("Todos")
        cboFiltroTipo.Items.AddRange(CType(tipos, Object()))
        cboFiltroTipo.SelectedIndex = 0

        ' Cargar solo la lista al iniciar; no consultar el detalle de la primera
        ' fila para borrarlo inmediatamente. Limpiar deja los combos en modo alta.
        _cargando = True
        CargarGrid(False)
        _cargando = False
        Dim errorCargaLista As Boolean = dgvDocumentos.DataSource Is Nothing
        CargarCombos()
        Dim errorCargaCombos As Boolean =
            cboCaso.DataSource Is Nothing OrElse cboInstrumento.DataSource Is Nothing
        Limpiar()

        If errorCargaLista Then
            MostrarMensaje("No se pudo cargar la lista de documentos. Verifique la conexión con la base de datos.", False)
        ElseIf errorCargaCombos Then
            MostrarMensaje("No se pudieron cargar los catálogos documentales. Verifique la conexión con la base de datos.", False)
        End If

        btnProcesarIA.Enabled = False ' Integración IA/OCR aún no implementada.
        ActualizarAccionesDocumento()
        _permisosInicializados = True
    End Sub

    ''' <summary>
    ''' Actualiza la interfaz con los permisos vigentes, no con los que tenía
    ''' el usuario al abrir el formulario. Esto evita controles habilitados
    ''' después de que un administrador cambie el rol de la sesión.
    ''' </summary>
    Private Sub AplicarPermisosInterfaz()
        Dim puedeConsultar As Boolean = _puedeVer OrElse _puedeVerificar OrElse _puedeMetadatos

        btnBuscar.Enabled = puedeConsultar
        btnRefrescar.Enabled = puedeConsultar
        btnLimpiarFiltro.Enabled = puedeConsultar
        btnImportar.Enabled = _puedeImportar
        txtFiltro.Enabled = puedeConsultar
        cboFiltroTipo.Enabled = puedeConsultar

        ActualizarEdicionMetadatos(_idDocumento <= 0)
        ActualizarAccionesDocumento()
    End Sub

    Private Sub FrmDocumentos_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        _puedeVer = Permisos.Puede("documentos.ver")
        _puedeImportar = Permisos.Puede("documentos.importar")
        _puedeVerificar = Permisos.Puede("documentos.verificar")
        _puedeMetadatos = Permisos.Puede("documentos.metadatos")

        If Not (_puedeVer OrElse _puedeImportar OrElse _puedeVerificar OrElse _puedeMetadatos) Then
            Close()
            Return
        End If

        AplicarPermisosInterfaz()
    End Sub

    Private Function TiposDocumento() As String()
        Return New String() {
            "Escritura", "Poder", "Demanda", "Contestación", "Sentencia", "Resolución",
            "Contrato", "Identificación", "Comprobante", "Otro"}
    End Function

    Private Sub CargarCombos(Optional idInstrumentoActual As Integer? = Nothing)
        _catalogosDisponibles = False
        Try
            Dim casos As DataTable = ConOpcionNinguno(_servicio.CasosCombo())
            Dim instrumentos As DataTable = ConOpcionNinguno(_servicio.InstrumentosCombo(idInstrumentoActual))
            UtileriasUI.CargarCombo(cboCaso, casos, "Nombre", "ID")
            UtileriasUI.CargarCombo(cboInstrumento, instrumentos, "Nombre", "ID")
            _catalogosDisponibles = True
        Catch ex As Exception
            cboCaso.DataSource = Nothing
            cboInstrumento.DataSource = Nothing
            RegistroErrores.Escribir("Documentos - combos", ex)
            MostrarMensaje("No se pudieron cargar los catálogos. No se permitirá guardar metadatos ni importar con vínculos incompletos.", False)
        Finally
            ActualizarEdicionMetadatos(_idDocumento <= 0)
            ActualizarAccionesDocumento()
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

    Private Sub CargarGrid(Optional cargarDetalleSeleccionado As Boolean = True)
        Try
            Dim tipo As String = ""
            If cboFiltroTipo.SelectedIndex > 0 Then tipo = Convert.ToString(cboFiltroTipo.SelectedItem)

            _cargando = True
            dgvDocumentos.DataSource = _servicio.Buscar(txtFiltro.Text, tipo)
            ConfigurarColumnasDocumentos()
            _cargando = False

            Dim actual As DataRow = UtileriasUI.FilaActual(dgvDocumentos)
            If actual IsNot Nothing Then
                If cargarDetalleSeleccionado Then CargarDetalle(actual)
            ElseIf cargarDetalleSeleccionado Then
                ' Al quedar sin resultados, restablecer el catálogo base para no
                ' conservar instrumentos históricos de la selección anterior.
                _cargando = True
                CargarCombos()
                _cargando = False
                Limpiar()
            End If
        Catch ex As Exception
            _cargando = True
            dgvDocumentos.DataSource = Nothing
            If cargarDetalleSeleccionado Then CargarCombos()
            _cargando = False
            Limpiar()
            RegistroErrores.Escribir("Documentos - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de documentos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Reduce la exposición de rutas y hashes en el listado y presenta
    ''' metadatos útiles para localizar documentos sin saturar la grilla.
    ''' Los valores completos siguen disponibles en el panel de detalle.
    ''' </summary>
    Private Sub ConfigurarColumnasDocumentos()
        OcultarColumnas("ID", "ID_Caso", "ID_Instrumento", "Ruta_Archivo", "Hash")

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Tipo_Documento", "Tipo de documento"},
            {"Estado_Procesamiento_IA", "Estado de análisis"},
            {"Fecha_Carga", "Fecha de carga"},
            {"Usuario", "Cargado por"},
            {"Expediente", "Expediente"},
            {"Escritura", "Escritura"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvDocumentos.Columns.Contains(par.Key) Then
                dgvDocumentos.Columns(par.Key).HeaderText = par.Value
                dgvDocumentos.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        If dgvDocumentos.Columns.Contains("Fecha_Carga") Then
            dgvDocumentos.Columns("Fecha_Carga").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"
            dgvDocumentos.Columns("Fecha_Carga").MinimumWidth = 125
        End If
        If dgvDocumentos.Columns.Contains("Tipo_Documento") Then
            dgvDocumentos.Columns("Tipo_Documento").MinimumWidth = 130
        End If
        If dgvDocumentos.Columns.Contains("Estado_Procesamiento_IA") Then
            dgvDocumentos.Columns("Estado_Procesamiento_IA").MinimumWidth = 115
        End If
        If dgvDocumentos.Columns.Contains("Expediente") Then
            dgvDocumentos.Columns("Expediente").MinimumWidth = 115
        End If
    End Sub

    Private Sub dgvDocumentos_SelectionChanged(sender As Object, e As EventArgs) Handles dgvDocumentos.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvDocumentos)
        If fila Is Nothing Then
            ' Evitar conservar un documento operativo y catálogos históricos
            ' después de perder la selección.
            _cargando = True
            CargarCombos()
            _cargando = False
            Limpiar()
            Return
        End If
        CargarDetalle(fila)
    End Sub

    Private Sub CargarDetalle(fila As DataRow)
        _cargando = True
        _idDocumento = UtileriasUI.Entero(fila, "ID")

        Dim idInstrumentoActual As Integer? = Nothing
        If Not fila.IsNull("ID_Instrumento") Then
            idInstrumentoActual = UtileriasUI.Entero(fila, "ID_Instrumento")
        End If
        ' Recargar siempre el catálogo para evitar que una selección anterior
        ' deje instrumentos históricos disponibles al cambiar de documento.
        CargarCombos(idInstrumentoActual)

        SeleccionarTexto(cboTipoDocumento, UtileriasUI.Texto(fila, "Tipo_Documento"))
        If _catalogosDisponibles Then
            UtileriasUI.SeleccionarValor(cboCaso, If(fila.IsNull("ID_Caso"), 0, fila("ID_Caso")))
            UtileriasUI.SeleccionarValor(cboInstrumento, If(fila.IsNull("ID_Instrumento"), 0, fila("ID_Instrumento")))
        End If

        txtHash.Text = UtileriasUI.Texto(fila, "Hash")
        txtEstadoIA.Text = UtileriasUI.Texto(fila, "Estado_Procesamiento_IA")

        Dim fecha As Date? = UtileriasUI.Fecha(fila, "Fecha_Carga")
        txtFechaCarga.Text = If(fecha.HasValue, fecha.Value.ToString("dd/MM/yyyy HH:mm"), "")

        _cargando = False
        ActualizarEdicionMetadatos(False)
        ActualizarAccionesDocumento()
        If _catalogosDisponibles Then
            MostrarMensaje("", True)
        Else
            MostrarMensaje("No se pudieron cargar los catálogos. La edición de metadatos y la importación están bloqueadas para evitar perder vínculos.", False)
        End If
    End Sub

    Private Sub Limpiar()
        _idDocumento = 0
        cboTipoDocumento.SelectedIndex = 0
        If cboCaso.Items.Count > 0 Then cboCaso.SelectedIndex = 0
        If cboInstrumento.Items.Count > 0 Then cboInstrumento.SelectedIndex = 0
        txtHash.Clear()
        txtEstadoIA.Clear()
        txtFechaCarga.Clear()
        ActualizarEdicionMetadatos(True)
        MostrarMensaje("", True)

        _cargando = True
        dgvDocumentos.CurrentCell = Nothing
        _cargando = False
        ActualizarAccionesDocumento()
    End Sub

    ''' <summary>
    ''' En modo de importación, el usuario puede preparar los metadatos del
    ''' nuevo archivo. Al seleccionar un documento existente, solo quien tiene
    ''' el permiso específico puede editar sus metadatos.
    ''' </summary>
    Private Sub ActualizarEdicionMetadatos(paraNuevoDocumento As Boolean)
        Dim puedeEditar As Boolean = _catalogosDisponibles AndAlso (_puedeMetadatos OrElse (paraNuevoDocumento AndAlso _puedeImportar))
        cboTipoDocumento.Enabled = puedeEditar
        cboCaso.Enabled = puedeEditar
        cboInstrumento.Enabled = puedeEditar
    End Sub

    Private Sub ActualizarAccionesDocumento()
        Dim haySeleccion As Boolean = _idDocumento > 0
        btnAbrir.Enabled = _puedeVer AndAlso haySeleccion
        btnActualizar.Enabled = _puedeMetadatos AndAlso haySeleccion
        btnVerificar.Enabled = _puedeVerificar AndAlso haySeleccion
        btnCopiarHash.Enabled =
            haySeleccion AndAlso
            (_puedeVer OrElse _puedeVerificar OrElse _puedeMetadatos) AndAlso
            Not String.IsNullOrWhiteSpace(txtHash.Text)
        btnProcesarIA.Enabled = False ' ServicioIA declara la integración como no disponible.
    End Sub

    Private Sub Importar()
        If Not Permisos.ExigirAccion("documentos.importar") Then Return
        If Not _catalogosDisponibles Then
            MostrarMensaje("No se puede importar: los catálogos de casos e instrumentos no están disponibles.", False)
            Return
        End If

        Using dialogo As New OpenFileDialog()
            dialogo.Title = "Seleccione el documento a importar"
            dialogo.Filter = "Documentos (*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.tif;*.tiff)|*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.tif;*.tiff"
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return

            ' Solo abandonar el documento actual cuando el usuario confirma
            ' la selección de un archivo. Cancelar el diálogo conserva contexto
            ' y selección, evitando perder el trabajo de metadatos en pantalla.
            _cargando = True
            _idDocumento = 0
            dgvDocumentos.ClearSelection()
            dgvDocumentos.CurrentCell = Nothing
            _cargando = False

            If cboTipoDocumento.Items.Count > 0 Then cboTipoDocumento.SelectedIndex = 0
            If cboCaso.Items.Count > 0 Then cboCaso.SelectedIndex = 0
            If cboInstrumento.Items.Count > 0 Then cboInstrumento.SelectedIndex = 0
            ActualizarEdicionMetadatos(True)
            ActualizarAccionesDocumento()

            Dim respuesta As Respuesta = _servicio.Importar(
                dialogo.FileName,
                Convert.ToString(cboTipoDocumento.SelectedItem),
                IdOpcional(cboCaso),
                IdOpcional(cboInstrumento))

            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            CargarGrid()
            If respuesta.ID.HasValue Then SeleccionarDocumento(respuesta.ID.Value)
            MostrarMensaje(respuesta.Mensaje, True)
        End Using
    End Sub

    Private Sub ActualizarMetadatos()
        If Not Permisos.ExigirAccion("documentos.metadatos") Then Return
        If Not _catalogosDisponibles Then
            MostrarMensaje("No se pueden guardar metadatos porque no se pudieron cargar los vínculos actuales. Actualice los catálogos e intente nuevamente.", False)
            Return
        End If

        If _idDocumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un documento de la lista.")
            Return
        End If

        ' Conservar el identificador antes de recargar: CargarGrid puede cambiar
        ' _idDocumento al seleccionar automáticamente la primera fila.
        Dim idDocumentoActualizado As Integer = _idDocumento
        Dim respuesta As Respuesta = _servicio.ActualizarMetadatos(
            idDocumentoActualizado,
            Convert.ToString(cboTipoDocumento.SelectedItem),
            IdOpcional(cboCaso),
            IdOpcional(cboInstrumento))

        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarGrid()
        SeleccionarDocumento(idDocumentoActualizado)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub AbrirDocumento()
        If Not Permisos.ExigirAccion("documentos.ver") Then Return

        If _idDocumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un documento de la lista.")
            Return
        End If

        Try
            Dim ruta As String = _servicio.RutaDelDocumento(_idDocumento)
            If Not System.IO.File.Exists(ruta) Then
                MostrarMensaje("El archivo no se encuentra en la ruta registrada.", False)
                Return
            End If

            Process.Start(New ProcessStartInfo(ruta) With {.UseShellExecute = True})
        Catch ex As System.IO.InvalidDataException
            RegistroErrores.Escribir("Documentos - integridad antes de abrir", ex)
            MostrarMensaje("ALERTA: el documento fue alterado y no se abrirá.", False)
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - abrir", ex)
            MostrarMensaje("No se pudo abrir el archivo. Verifique que exista una aplicación compatible y que tenga permisos de acceso.", False)
        End Try
    End Sub

    Private Sub CopiarHash()
        If _idDocumento <= 0 OrElse String.IsNullOrWhiteSpace(txtHash.Text) Then
            MostrarMensaje("Seleccione un documento con hash disponible.", False)
            Return
        End If

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.TieneAccesoAlguno("documentos.ver", "documentos.verificar", "documentos.metadatos") Then
            ActualizarAccionesDocumento()
            MostrarMensaje("No tiene permiso para consultar o copiar el hash de documentos.", False)
            Return
        End If

        Try
            Clipboard.SetText(txtHash.Text.Trim())
            MostrarMensaje("Hash SHA-256 copiado al portapapeles.", True)
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - copiar hash", ex)
            MostrarMensaje("No se pudo copiar el hash. Compruebe el acceso al portapapeles.", False)
        End Try
    End Sub

    Private Sub VerificarIntegridad()
        If Not Permisos.ExigirAccion("documentos.verificar") Then Return

        If _idDocumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un documento de la lista.")
            Return
        End If

        Dim respuesta As Respuesta = _servicio.VerificarIntegridad(_idDocumento)
        MostrarMensaje(respuesta.Mensaje, respuesta.Exito)
    End Sub

    Private Sub ProcesarConIA()
        If Not Permisos.ExigirAccion("documentos.metadatos") Then Return

        If _idDocumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un documento de la lista para procesarlo.")
            Return
        End If

        Dim respuesta As Respuesta = _servicioIA.ProcesarDocumento(_idDocumento)
        MostrarMensaje(respuesta.Mensaje, respuesta.Exito)
        If respuesta.Exito Then CargarGrid()
    End Sub

    Private Function IdOpcional(combo As ComboBox) As Integer?
        Dim id As Integer = UtileriasUI.ValorCombo(combo)
        If id <= 0 Then Return Nothing
        Return id
    End Function

    Private Sub SeleccionarDocumento(id As Integer)
        For Each fila As DataGridViewRow In dgvDocumentos.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' La primera columna puede estar oculta por configuración o reordenamiento.
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvDocumentos.CurrentCell = celda
                    Return
                End If
            Next
            Return
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

    Private Sub OcultarColumnas(ParamArray nombres As String())
        For Each nombre As String In nombres
            If dgvDocumentos.Columns.Contains(nombre) Then dgvDocumentos.Columns(nombre).Visible = False
        Next
    End Sub

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub txtFiltro_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFiltro.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarGrid()
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            LimpiarFiltro()
        End If
    End Sub

    Private Sub LimpiarFiltro()
        txtFiltro.Clear()
        If cboFiltroTipo.Items.Count > 0 Then cboFiltroTipo.SelectedIndex = 0
        CargarGrid()
        txtFiltro.Focus()
    End Sub

    Private Sub btnLimpiarFiltro_Click(sender As Object, e As EventArgs) Handles btnLimpiarFiltro.Click
        LimpiarFiltro()
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        CargarGrid()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarCombos()
        CargarGrid()
    End Sub

    Private Sub btnImportar_Click(sender As Object, e As EventArgs) Handles btnImportar.Click
        UtileriasUI.EjecutarUnaVez(btnImportar, AddressOf Importar)
    End Sub

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        UtileriasUI.EjecutarUnaVez(btnActualizar, AddressOf ActualizarMetadatos)
    End Sub

    Private Sub btnAbrir_Click(sender As Object, e As EventArgs) Handles btnAbrir.Click
        AbrirDocumento()
    End Sub

    Private Sub btnCopiarHash_Click(sender As Object, e As EventArgs) Handles btnCopiarHash.Click
        CopiarHash()
    End Sub

    Private Sub btnVerificar_Click(sender As Object, e As EventArgs) Handles btnVerificar.Click
        UtileriasUI.EjecutarUnaVez(btnVerificar, AddressOf VerificarIntegridad)
    End Sub

    Private Sub btnProcesarIA_Click(sender As Object, e As EventArgs) Handles btnProcesarIA.Click
        UtileriasUI.EjecutarUnaVez(btnProcesarIA, AddressOf ProcesarConIA)
    End Sub

    Private Sub cboFiltroTipo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFiltroTipo.SelectedIndexChanged
        If _cargando Then Return
        CargarGrid()
    End Sub

End Class
