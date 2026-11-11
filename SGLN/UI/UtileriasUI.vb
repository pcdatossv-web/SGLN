Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports Microsoft.VisualBasic

''' <summary>
''' Helpers de interfaz para evitar repetir código en los formularios.
''' </summary>
Public Module UtileriasUI

    Public Sub ConfigurarGrid(grid As DataGridView)
        grid.AutoGenerateColumns = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.ReadOnly = True
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.RowHeadersVisible = False
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250)
        grid.ColumnHeadersDefaultCellStyle.Font = New Font(grid.Font, FontStyle.Bold)
    End Sub

    Public Function FilaActual(grid As DataGridView) As DataRow
        If grid.CurrentRow Is Nothing Then Return Nothing
        Dim vista As DataRowView = TryCast(grid.CurrentRow.DataBoundItem, DataRowView)
        If vista Is Nothing Then Return Nothing
        Return vista.Row
    End Function

    Public Function Texto(fila As DataRow, columna As String) As String
        If fila Is Nothing Then Return ""
        If Not fila.Table.Columns.Contains(columna) Then Return ""
        If fila.IsNull(columna) Then Return ""
        Return Convert.ToString(fila(columna))
    End Function

    Public Function Entero(fila As DataRow, columna As String) As Integer
        Dim numero As Integer
        If Integer.TryParse(Texto(fila, columna), numero) Then Return numero
        Return 0
    End Function

    Public Function DecimalDesde(fila As DataRow, columna As String) As Decimal
        Dim numero As Decimal
        If Decimal.TryParse(Texto(fila, columna), numero) Then Return numero
        Return 0D
    End Function

    Public Function Fecha(fila As DataRow, columna As String) As Date?
        If fila Is Nothing Then Return Nothing
        If Not fila.Table.Columns.Contains(columna) Then Return Nothing
        If fila.IsNull(columna) Then Return Nothing
        Return Convert.ToDateTime(fila(columna)).Date
    End Function

    Public Function Booleano(fila As DataRow, columna As String) As Boolean
        If fila Is Nothing Then Return False
        If Not fila.Table.Columns.Contains(columna) Then Return False
        If fila.IsNull(columna) Then Return False
        Return Convert.ToBoolean(fila(columna))
    End Function

    Public Sub CargarCombo(combo As ComboBox, tabla As DataTable, campoTexto As String, campoValor As String)
        combo.DataSource = Nothing
        combo.DisplayMember = campoTexto
        combo.ValueMember = campoValor
        combo.DataSource = tabla
        combo.SelectedIndex = If(tabla.Rows.Count > 0, 0, -1)
    End Sub

    Public Sub SeleccionarValor(combo As ComboBox, valor As Object)
        If combo.DataSource Is Nothing Then Return
        If valor Is Nothing OrElse Convert.IsDBNull(valor) Then
            combo.SelectedIndex = -1
            Return
        End If
        combo.SelectedValue = valor
    End Sub

    Public Function ValorCombo(combo As ComboBox) As Integer
        If combo.SelectedValue Is Nothing Then Return 0
        Dim numero As Integer
        If Integer.TryParse(Convert.ToString(combo.SelectedValue), numero) Then Return numero
        Return 0
    End Function

    Public Sub AsignarFecha(control As DateTimePicker, valor As Date?)
        If valor.HasValue Then
            control.Checked = True
            control.Value = valor.Value
        Else
            control.Checked = False
        End If
    End Sub

    Public Function LeerFecha(control As DateTimePicker) As Date?
        If control.ShowCheckBox AndAlso Not control.Checked Then Return Nothing
        Return control.Value.Date
    End Function

    Public Sub MostrarError(mensaje As String)
        MessageBox.Show(mensaje, "SGLN El Salvador", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Public Sub MostrarInfo(mensaje As String)
        MessageBox.Show(mensaje, "SGLN El Salvador", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Function Confirmar(mensaje As String) As Boolean
        Return MessageBox.Show(mensaje, "SGLN El Salvador", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes
    End Function

    Public Function PedirTexto(mensaje As String, titulo As String, valorInicial As String) As String
        Return InputBox(mensaje, titulo, valorInicial)
    End Function

    Public Sub ExportarCsv(tabla As DataTable, ruta As String)
        Dim contenido As New StringBuilder()

        Dim encabezados As New List(Of String)()
        For Each columna As DataColumn In tabla.Columns
            encabezados.Add(ValorCsv(columna.ColumnName))
        Next
        contenido.AppendLine(String.Join(",", encabezados.ToArray()))

        For Each fila As DataRow In tabla.Rows
            Dim valores As New List(Of String)()
            For Each columna As DataColumn In tabla.Columns
                valores.Add(ValorCsv(Convert.ToString(fila(columna))))
            Next
            contenido.AppendLine(String.Join(",", valores.ToArray()))
        Next

        File.WriteAllText(ruta, contenido.ToString(), Encoding.UTF8)
    End Sub

    Private Function ValorCsv(valor As String) As String
        Return """" & valor.Replace("""", """""") & """"
    End Function

End Module
