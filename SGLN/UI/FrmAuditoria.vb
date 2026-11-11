Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmAuditoria

    Private ReadOnly _servicio As New ServicioAuditoria()
    Private _cargando As Boolean

    Private Sub FrmAuditoria_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("auditoria.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvAuditoria)

        dtpDesde.Value = Date.Today.AddDays(-30)
        dtpHasta.Value = Date.Today

        _cargando = True
        CargarFiltros()
        _cargando = False

        Buscar()
    End Sub

    Private Sub CargarFiltros()
        Try
            UtileriasUI.CargarCombo(cboTabla, ConTodos(_servicio.TablasAfectadas(), "Tabla_Afectada"), "Nombre", "Valor")
            UtileriasUI.CargarCombo(cboAccion, ConTodos(_servicio.Acciones(), "Accion"), "Nombre", "Valor")
        Catch ex As Exception
            RegistroErrores.Escribir("Auditoría - filtros", ex)
            MostrarMensaje("No se pudieron cargar los filtros: " & ex.Message, False)
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
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Auditoría - buscar", ex)
            MostrarMensaje("No se pudo consultar la auditoría: " & ex.Message, False)
        End Try
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

            Try
                UtileriasUI.ExportarCsv(tabla, dialogo.FileName)
                MostrarMensaje("Auditoría exportada en " & dialogo.FileName, True)
            Catch ex As Exception
                RegistroErrores.Escribir("Auditoría - exportar", ex)
                MostrarMensaje("No se pudo exportar: " & ex.Message, False)
            End Try
        End Using
    End Sub

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        Buscar()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        _cargando = True
        CargarFiltros()
        _cargando = False
        Buscar()
    End Sub

    Private Sub btnExportar_Click(sender As Object, e As EventArgs) Handles btnExportar.Click
        Exportar()
    End Sub

End Class
