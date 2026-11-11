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

    Private Sub FrmDocumentos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("documentos.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvDocumentos)

        Dim tipos As String() = TiposDocumento()
        cboTipoDocumento.Items.Clear()
        cboTipoDocumento.Items.AddRange(CType(tipos, Object()))
        cboTipoDocumento.SelectedIndex = 0

        cboFiltroTipo.Items.Clear()
        cboFiltroTipo.Items.Add("Todos")
        cboFiltroTipo.Items.AddRange(CType(tipos, Object()))
        cboFiltroTipo.SelectedIndex = 0

        _cargando = True
        CargarCombos()
        CargarGrid()
        _cargando = False
        Limpiar()

        btnProcesarIA.Enabled = False
    End Sub

    Private Function TiposDocumento() As String()
        Return New String() {
            "Escritura", "Poder", "Demanda", "Contestación", "Sentencia", "Resolución",
            "Contrato", "Identificación", "Comprobante", "Otro"}
    End Function

    Private Sub CargarCombos()
        Try
            UtileriasUI.CargarCombo(cboCaso, ConOpcionNinguno(_servicio.CasosCombo()), "Nombre", "ID")
            UtileriasUI.CargarCombo(cboInstrumento, ConOpcionNinguno(_servicio.InstrumentosCombo()), "Nombre", "ID")
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - combos", ex)
            MostrarMensaje("No se pudieron cargar los catálogos: " & ex.Message, False)
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

    Private Sub CargarGrid()
        Try
            Dim tipo As String = ""
            If cboFiltroTipo.SelectedIndex > 0 Then tipo = Convert.ToString(cboFiltroTipo.SelectedItem)

            _cargando = True
            dgvDocumentos.DataSource = _servicio.Buscar(txtFiltro.Text, tipo)
            OcultarColumnas("ID", "ID_Caso", "ID_Instrumento")
            If dgvDocumentos.Columns.Contains("Fecha_Carga") Then
                dgvDocumentos.Columns("Fecha_Carga").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"
            End If
            _cargando = False

            Dim actual As DataRow = UtileriasUI.FilaActual(dgvDocumentos)
            If actual IsNot Nothing Then CargarDetalle(actual)
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Documentos - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista: " & ex.Message, False)
        End Try
    End Sub

    Private Sub dgvDocumentos_SelectionChanged(sender As Object, e As EventArgs) Handles dgvDocumentos.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvDocumentos)
        If fila Is Nothing Then Return
        CargarDetalle(fila)
    End Sub

    Private Sub CargarDetalle(fila As DataRow)
        _idDocumento = UtileriasUI.Entero(fila, "ID")

        SeleccionarTexto(cboTipoDocumento, UtileriasUI.Texto(fila, "Tipo_Documento"))
        UtileriasUI.SeleccionarValor(cboCaso, If(fila.IsNull("ID_Caso"), 0, fila("ID_Caso")))
        UtileriasUI.SeleccionarValor(cboInstrumento, If(fila.IsNull("ID_Instrumento"), 0, fila("ID_Instrumento")))

        txtHash.Text = UtileriasUI.Texto(fila, "Hash")
        txtEstadoIA.Text = UtileriasUI.Texto(fila, "Estado_Procesamiento_IA")

        Dim fecha As Date? = UtileriasUI.Fecha(fila, "Fecha_Carga")
        txtFechaCarga.Text = If(fecha.HasValue, fecha.Value.ToString("dd/MM/yyyy HH:mm"), "")

        MostrarMensaje("", True)
    End Sub

    Private Sub Limpiar()
        _idDocumento = 0
        cboTipoDocumento.SelectedIndex = 0
        If cboCaso.Items.Count > 0 Then cboCaso.SelectedIndex = 0
        If cboInstrumento.Items.Count > 0 Then cboInstrumento.SelectedIndex = 0
        txtHash.Clear()
        txtEstadoIA.Clear()
        txtFechaCarga.Clear()
        MostrarMensaje("", True)

        _cargando = True
        dgvDocumentos.CurrentCell = Nothing
        _cargando = False
    End Sub

    Private Sub Importar()
        If Not Permisos.ExigirAccion("documentos.importar") Then Return

        Using dialogo As New OpenFileDialog()
            dialogo.Title = "Seleccione el documento a importar"
            dialogo.Filter = "Documentos (*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.tif;*.tiff)|*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.tif;*.tiff"
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return

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

        If _idDocumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un documento de la lista.")
            Return
        End If

        Dim respuesta As Respuesta = _servicio.ActualizarMetadatos(
            _idDocumento,
            Convert.ToString(cboTipoDocumento.SelectedItem),
            IdOpcional(cboCaso),
            IdOpcional(cboInstrumento))

        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarGrid()
        SeleccionarDocumento(_idDocumento)
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub AbrirDocumento()
        If _idDocumento <= 0 Then
            UtileriasUI.MostrarError("Seleccione un documento de la lista.")
            Return
        End If

        Dim ruta As String = _servicio.RutaDelDocumento(_idDocumento)
        If Not System.IO.File.Exists(ruta) Then
            MostrarMensaje("El archivo no se encuentra en la ruta registrada.", False)
            Return
        End If

        Try
            Process.Start(New ProcessStartInfo(ruta) With {.UseShellExecute = True})
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - abrir", ex)
            MostrarMensaje("No se pudo abrir el archivo: " & ex.Message, False)
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

        Dim respuesta As Respuesta = _servicioIA.ProcesarDocumento(_idDocumento)
        MostrarMensaje(respuesta.Mensaje, False)
    End Sub

    Private Function IdOpcional(combo As ComboBox) As Integer?
        Dim id As Integer = UtileriasUI.ValorCombo(combo)
        If id <= 0 Then Return Nothing
        Return id
    End Function

    Private Sub SeleccionarDocumento(id As Integer)
        For Each fila As DataGridViewRow In dgvDocumentos.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista IsNot Nothing AndAlso Convert.ToInt32(vista.Row("ID")) = id Then
                dgvDocumentos.CurrentCell = fila.Cells(0)
                Return
            End If
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

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        CargarGrid()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarCombos()
        CargarGrid()
    End Sub

    Private Sub btnImportar_Click(sender As Object, e As EventArgs) Handles btnImportar.Click
        Importar()
    End Sub

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        ActualizarMetadatos()
    End Sub

    Private Sub btnAbrir_Click(sender As Object, e As EventArgs) Handles btnAbrir.Click
        AbrirDocumento()
    End Sub

    Private Sub btnVerificar_Click(sender As Object, e As EventArgs) Handles btnVerificar.Click
        VerificarIntegridad()
    End Sub

    Private Sub btnProcesarIA_Click(sender As Object, e As EventArgs) Handles btnProcesarIA.Click
        ProcesarConIA()
    End Sub

    Private Sub cboFiltroTipo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFiltroTipo.SelectedIndexChanged
        If _cargando Then Return
        CargarGrid()
    End Sub

End Class
