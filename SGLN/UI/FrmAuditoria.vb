Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmAuditoria

    Private ReadOnly _servicio As New ServicioAuditoria()
    Private Const TamanoPagina As Integer = 100
    Private _paginaActual As Long
    Private _totalRegistros As Long
    Private _totalPaginas As Long = 1
    Private _permisosInicializados As Boolean

    Private Sub FrmAuditoria_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler btnLimpiar.Click, AddressOf btnLimpiar_Click
        If Not Permisos.Exigir("auditoria.ver", Me) Then Return

        ' La exportación es independiente del permiso de consulta. Mantener
        ' el botón sincronizado con la matriz de permisos evita ofrecer una
        ' acción que el usuario no puede ejecutar; Exportar vuelve a validar
        ' el permiso antes de abrir el diálogo de archivo.
        ActualizarEstadoExportacion()

        UtileriasUI.ConfigurarGrid(dgvAuditoria)

        dtpDesde.Value = Date.Today.AddDays(-30)
        dtpHasta.Value = Date.Today

        CargarFiltros()
        Dim errorFiltros As Boolean =
            cboTabla.DataSource Is Nothing OrElse cboAccion.DataSource Is Nothing

        Buscar()
        If errorFiltros Then
            MostrarMensaje(
                If(TypeOf dgvAuditoria.DataSource Is DataTable,
                   "No se pudieron cargar los filtros. La consulta se ejecutó sin esos filtros.",
                   "No se pudieron cargar los filtros ni consultar la bitácora."),
                False)
        End If
        _permisosInicializados = True
    End Sub

    Private Sub FrmAuditoria_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("auditoria.ver") Then
            Close()
            Return
        End If

        ' Recalcular también la disponibilidad de datos; el permiso por sí solo
        ' no basta para habilitar una exportación sin registros.
        ActualizarEstadoExportacion()
    End Sub

    Private Sub CargarFiltros()
        Try
            UtileriasUI.CargarCombo(cboTabla, ConTodos(_servicio.TablasAfectadas(), "Tabla_Afectada"), "Nombre", "Valor")
            UtileriasUI.CargarCombo(cboAccion, ConTodos(_servicio.Acciones(), "Accion"), "Nombre", "Valor")
        Catch ex As Exception
            cboTabla.DataSource = Nothing
            cboAccion.DataSource = Nothing
            RegistroErrores.Escribir("Auditoría - filtros", ex)
            MostrarMensaje("No se pudieron cargar los filtros de auditoría. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Convierte una columna de valores distintos en un catálogo con la
    ''' opción "Todos" al inicio, sin tocar la consulta original.
    ''' </summary>
    Private Function ConTodos(origen As DataTable, columna As String) As DataTable
        Dim tabla As New DataTable()
        tabla.Columns.Add("Valor", GetType(String))
        tabla.Columns.Add("Nombre", GetType(String))

        tabla.Rows.Add("", "(Todos)")
        For Each fila As DataRow In origen.Rows
            Dim valor As String = Convert.ToString(fila(columna))
            If Not String.IsNullOrWhiteSpace(valor) Then tabla.Rows.Add(valor, valor)
        Next

        Return tabla
    End Function

    Private Sub Buscar()
        _paginaActual = 0
        CargarPagina()
    End Sub

    Private Sub CargarPagina()
        If dtpDesde.Value.Date > dtpHasta.Value.Date Then
            _totalRegistros = 0
            _totalPaginas = 1
            dgvAuditoria.DataSource = Nothing
            lblResumen.Text = "Rango de fechas no válido."
            MostrarMensaje("La fecha inicial no puede ser posterior a la fecha final.", False)
            ActualizarEstadoPaginacion()
            ActualizarEstadoExportacion()
            Return
        End If

        Try
            Dim usuario As String = txtUsuario.Text
            Dim tabla As String = ValorFiltro(cboTabla)
            Dim accion As String = ValorFiltro(cboAccion)
            Dim desde As Date = dtpDesde.Value.Date
            Dim hasta As Date = dtpHasta.Value.Date

            _totalRegistros = _servicio.Contar(usuario, tabla, accion, desde, hasta)
            _totalPaginas = If(_totalRegistros = 0L, 1L,
                               (_totalRegistros + TamanoPagina - 1L) \ TamanoPagina)
            If _paginaActual >= _totalPaginas Then _paginaActual = _totalPaginas - 1L

            dgvAuditoria.DataSource = _servicio.BuscarPagina(
                usuario, tabla, accion, desde, hasta, _paginaActual, TamanoPagina)

            If dgvAuditoria.Columns.Contains("ID") Then dgvAuditoria.Columns("ID").Visible = False
            If dgvAuditoria.Columns.Contains("Fecha_Hora") Then
                dgvAuditoria.Columns("Fecha_Hora").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"
                dgvAuditoria.Columns("Fecha_Hora").FillWeight = 110
            End If
            If dgvAuditoria.Columns.Contains("Detalle") Then dgvAuditoria.Columns("Detalle").FillWeight = 250

            If _totalRegistros = 0 Then
                lblResumen.Text = "No se encontraron registros."
            Else
                Dim primero As Long = (_paginaActual * TamanoPagina) + 1L
                Dim ultimo As Long = Math.Min(primero + TamanoPagina - 1L, _totalRegistros)
                lblResumen.Text = "Registros: " & _totalRegistros.ToString("N0") &
                                  " | Mostrando " & primero.ToString("N0") & "-" & ultimo.ToString("N0")
            End If

            MostrarMensaje("", True)
            ActualizarEstadoPaginacion()
            ActualizarEstadoExportacion()
        Catch ex As Exception
            _totalRegistros = 0
            _totalPaginas = 1
            dgvAuditoria.DataSource = Nothing
            lblResumen.Text = "No se pudo cargar la bitácora."
            RegistroErrores.Escribir("Auditoría - buscar", ex)
            MostrarMensaje("No se pudo consultar la bitácora de auditoría. Verifique los filtros y la conexión con la base de datos.", False)
            ActualizarEstadoPaginacion()
            ActualizarEstadoExportacion()
        End Try
    End Sub

    Private Sub ActualizarEstadoPaginacion()
        lblPagina.Text = If(_totalRegistros = 0,
                            "Página 0 de 0",
                            "Página " & (_paginaActual + 1L).ToString("N0") &
                            " de " & _totalPaginas.ToString("N0"))
        btnAnterior.Enabled = _totalRegistros > 0 AndAlso _paginaActual > 0
        btnSiguiente.Enabled = _totalRegistros > 0 AndAlso _paginaActual + 1L < _totalPaginas
    End Sub

    Private Sub ActualizarEstadoExportacion()
        btnExportar.Enabled = Permisos.Puede("auditoria.exportar") AndAlso _totalRegistros > 0
    End Sub

    Private Function ValorFiltro(combo As ComboBox) As String
        If combo.SelectedValue Is Nothing Then Return ""
        Return Convert.ToString(combo.SelectedValue)
    End Function

    Private Sub Exportar()
        If Not Permisos.ExigirAccion("auditoria.exportar") Then Return
        If _totalRegistros = 0 Then
            UtileriasUI.MostrarError("No hay registros para exportar.")
            Return
        End If

        Using dialogo As New SaveFileDialog()
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv"
            dialogo.FileName = "Auditoria_" & Date.Now.ToString("yyyyMMdd_HHmm") & ".csv"
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return

            ' Exportar todos los resultados del filtro, no solo la página visible.
            ' Revalidar el permiso después del diálogo modal.
            If Not Permisos.ExigirAccion("auditoria.exportar") Then
                ActualizarEstadoExportacion()
                Return
            End If

            Try
                Dim tablaCompleta As DataTable = _servicio.Buscar(
                    txtUsuario.Text,
                    ValorFiltro(cboTabla),
                    ValorFiltro(cboAccion),
                    dtpDesde.Value.Date,
                    dtpHasta.Value.Date)
                If tablaCompleta.Rows.Count = 0 Then
                    UtileriasUI.MostrarError("No hay registros para exportar con los filtros actuales.")
                    Return
                End If
                UtileriasUI.ExportarCsv(tablaCompleta, dialogo.FileName)
                MostrarMensaje("Se exportaron " & tablaCompleta.Rows.Count.ToString("N0") &
                               " registros de auditoría.", True)
            Catch ex As Exception
                RegistroErrores.Escribir("Auditoría - exportar", ex)
                MostrarMensaje("No se pudo exportar la bitácora. Verifique la ruta y los permisos de escritura.", False)
            End Try
        End Using
    End Sub

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub txtUsuario_KeyDown(sender As Object, e As KeyEventArgs) Handles txtUsuario.KeyDown, cboTabla.KeyDown, cboAccion.KeyDown, dtpDesde.KeyDown, dtpHasta.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            Buscar()
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            LimpiarFiltros()
        End If
    End Sub

    Private Sub LimpiarFiltros()
        txtUsuario.Clear()
        If cboTabla.Items.Count > 0 Then cboTabla.SelectedIndex = 0
        If cboAccion.Items.Count > 0 Then cboAccion.SelectedIndex = 0
        dtpDesde.Value = Date.Today.AddDays(-30)
        dtpHasta.Value = Date.Today
        Buscar()
        txtUsuario.Focus()
    End Sub

    Private Sub btnLimpiar_Click(sender As Object, e As EventArgs)
        LimpiarFiltros()
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        Buscar()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarFiltros()
        Dim errorFiltros As Boolean =
            cboTabla.DataSource Is Nothing OrElse cboAccion.DataSource Is Nothing

        Buscar()
        If errorFiltros Then
            MostrarMensaje(
                If(TypeOf dgvAuditoria.DataSource Is DataTable,
                   "No se pudieron actualizar los filtros. La consulta se ejecutó sin esos filtros.",
                   "No se pudieron actualizar los filtros ni consultar la bitácora."),
                False)
        End If
    End Sub

    Private Sub btnAnterior_Click(sender As Object, e As EventArgs) Handles btnAnterior.Click
        If _paginaActual <= 0 Then Return
        _paginaActual -= 1L
        CargarPagina()
    End Sub

    Private Sub btnSiguiente_Click(sender As Object, e As EventArgs) Handles btnSiguiente.Click
        If _paginaActual + 1L >= _totalPaginas Then Return
        _paginaActual += 1L
        CargarPagina()
    End Sub

    Private Sub btnExportar_Click(sender As Object, e As EventArgs) Handles btnExportar.Click
        UtileriasUI.EjecutarUnaVez(btnExportar, AddressOf Exportar)
    End Sub

End Class
