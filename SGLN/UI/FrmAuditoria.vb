Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmAuditoria

    Private ReadOnly _servicio As New ServicioAuditoria()
    Private _cargando As Boolean
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

        _cargando = True
        CargarFiltros()
        Dim errorFiltros As Boolean =
            cboTabla.DataSource Is Nothing OrElse cboAccion.DataSource Is Nothing
        _cargando = False

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
        If dtpDesde.Value.Date > dtpHasta.Value.Date Then
            dgvAuditoria.DataSource = Nothing
            ActualizarEstadoExportacion()
            lblResumen.Text = "Rango de fechas no válido."
            MostrarMensaje("La fecha inicial no puede ser posterior a la fecha final.", False)
            Return
        End If

        Try
            _cargando = True
            dgvAuditoria.DataSource = _servicio.Buscar(
                txtUsuario.Text,
                ValorFiltro(cboTabla),
                ValorFiltro(cboAccion),
                dtpDesde.Value.Date,
                dtpHasta.Value.Date)

            If dgvAuditoria.Columns.Contains("ID") Then dgvAuditoria.Columns("ID").Visible = False
            If dgvAuditoria.Columns.Contains("Fecha_Hora") Then
                dgvAuditoria.Columns("Fecha_Hora").DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"
                dgvAuditoria.Columns("Fecha_Hora").FillWeight = 110
            End If
            If dgvAuditoria.Columns.Contains("Detalle") Then dgvAuditoria.Columns("Detalle").FillWeight = 250
            _cargando = False

            lblResumen.Text = "Registros encontrados: " & dgvAuditoria.Rows.Count.ToString("N0")
            MostrarMensaje("", True)
            ActualizarEstadoExportacion()
        Catch ex As Exception
            _cargando = False
            dgvAuditoria.DataSource = Nothing
            ActualizarEstadoExportacion()
            lblResumen.Text = "No se pudo cargar la bitácora."
            RegistroErrores.Escribir("Auditoría - buscar", ex)
            MostrarMensaje("No se pudo consultar la bitácora de auditoría. Verifique los filtros y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub ActualizarEstadoExportacion()
        Dim tabla As DataTable = TryCast(dgvAuditoria.DataSource, DataTable)
        btnExportar.Enabled = Permisos.Puede("auditoria.exportar") AndAlso
                             tabla IsNot Nothing AndAlso tabla.Rows.Count > 0
    End Sub


    Private Function ValorFiltro(combo As ComboBox) As String
        If combo.SelectedValue Is Nothing Then Return ""
        Return Convert.ToString(combo.SelectedValue)
    End Function

    Private Sub Exportar()
        If Not Permisos.ExigirAccion("auditoria.exportar") Then Return
        Dim tabla As DataTable = TryCast(dgvAuditoria.DataSource, DataTable)
        If tabla Is Nothing OrElse tabla.Rows.Count = 0 Then
            UtileriasUI.MostrarError("No hay registros para exportar.")
            Return
        End If

        Using dialogo As New SaveFileDialog()
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv"
            dialogo.FileName = "Auditoria_" & Date.Now.ToString("yyyyMMdd_HHmm") & ".csv"
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return

            ' El diálogo de archivo puede permanecer abierto mientras el
            ' administrador revoca el permiso o desactiva la cuenta.
            ' Revalidar inmediatamente antes de escribir el archivo: el
            ' estado del botón y la comprobación inicial no son suficientes.
            If Not Permisos.ExigirAccion("auditoria.exportar") Then
                ActualizarEstadoExportacion()
                Return
            End If

            Try
                UtileriasUI.ExportarCsv(tabla, dialogo.FileName)
                MostrarMensaje("Auditoría exportada en " & dialogo.FileName, True)
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
        _cargando = True
        CargarFiltros()
        Dim errorFiltros As Boolean =
            cboTabla.DataSource Is Nothing OrElse cboAccion.DataSource Is Nothing
        _cargando = False

        Buscar()
        If errorFiltros Then
            MostrarMensaje(
                If(TypeOf dgvAuditoria.DataSource Is DataTable,
                   "No se pudieron actualizar los filtros. La consulta se ejecutó sin esos filtros.",
                   "No se pudieron actualizar los filtros ni consultar la bitácora."),
                False)
        End If
    End Sub

    Private Sub btnExportar_Click(sender As Object, e As EventArgs) Handles btnExportar.Click
        UtileriasUI.EjecutarUnaVez(btnExportar, AddressOf Exportar)
    End Sub

End Class
