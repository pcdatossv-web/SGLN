Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmCasos

    Private ReadOnly _servicio As New ServicioCasos()
    Private ReadOnly _plazos As New ServicioPlazos()
    Private _cargando As Boolean
    Private _idCaso As Integer
    Private _fechaPropuesta As Date?

    Private Sub FrmCasos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("casos.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvCasos)
        UtileriasUI.ConfigurarGrid(dgvActuaciones)

        cboMateria.Items.Clear()
        cboMateria.Items.AddRange(New Object() {
            "Civil", "Mercantil", "Laboral", "Familia", "Penal", "Administrativo", "Notarial"})
        cboMateria.SelectedIndex = 0

        cboEstado.Items.Clear()
        cboEstado.Items.AddRange(New Object() {"Activo", "Suspendido", "Cerrado"})
        cboEstado.SelectedIndex = 0

        _cargando = True
        CargarCombos()
        CargarGrid("")
        _cargando = False

        ActualizarVencidas()
        Limpiar()
    End Sub

    Private Sub CargarCombos()
        Try
            UtileriasUI.CargarCombo(cboCliente, _servicio.ClientesCombo(), "Nombre", "ID")

            Dim juzgados As DataTable = _servicio.JuzgadosCombo()
            Dim sinJuzgado As DataRow = juzgados.NewRow()
            sinJuzgado("ID") = 0
            sinJuzgado("Nombre") = "(Sin juzgado asignado)"
            juzgados.Rows.InsertAt(sinJuzgado, 0)
            UtileriasUI.CargarCombo(cboJuzgado, juzgados, "Nombre", "ID")

            UtileriasUI.CargarCombo(cboTipoActuacion, _servicio.TiposActuacionCombo(), "Nombre", "ID")
            AplicarPlazoDelTipo()
        Catch ex As Exception
            RegistroErrores.Escribir("Casos - combos", ex)
            MostrarMensaje("No se pudieron cargar los catálogos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarGrid(filtro As String)
        Try
            _cargando = True
            dgvCasos.DataSource = _servicio.Buscar(filtro)
            If dgvCasos.Columns.Contains("ID") Then dgvCasos.Columns("ID").Visible = False
            _cargando = False
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Casos - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de casos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub dgvCasos_SelectionChanged(sender As Object, e As EventArgs) Handles dgvCasos.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvCasos)
        If fila Is Nothing Then Return
        CargarCaso(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarCaso(id As Integer)
        Try
            Dim tabla As DataTable = _servicio.Obtener(id)
            If tabla.Rows.Count = 0 Then Return

            Dim fila As DataRow = tabla.Rows(0)
            _idCaso = id

            UtileriasUI.SeleccionarValor(cboCliente, fila("ID_Cliente"))
            txtExpediente.Text = UtileriasUI.Texto(fila, "Numero_Expediente")
            SeleccionarTexto(cboMateria, UtileriasUI.Texto(fila, "Materia"))
            UtileriasUI.SeleccionarValor(cboJuzgado, If(fila.IsNull("ID_Juzgado"), 0, fila("ID_Juzgado")))
            dtpFechaApertura.Value = UtileriasUI.Fecha(fila, "Fecha_Apertura").GetValueOrDefault(Date.Today)
            SeleccionarTexto(cboEstado, UtileriasUI.Texto(fila, "Estado"))
            txtResponsable.Text = UtileriasUI.Texto(fila, "Responsable")
            txtDescripcion.Text = UtileriasUI.Texto(fila, "Descripcion")

            CargarActuaciones()
            LimpiarPlazo()
            MostrarMensaje("", True)
        Catch ex As Exception
            RegistroErrores.Escribir("Casos - detalle", ex)
            MostrarMensaje("No se pudo cargar el detalle del caso. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarActuaciones()
        If _idCaso <= 0 Then
            dgvActuaciones.DataSource = Nothing
            Return
        End If

        Try
            dgvActuaciones.DataSource = _plazos.Actuaciones(_idCaso)
            If dgvActuaciones.Columns.Contains("ID") Then dgvActuaciones.Columns("ID").Visible = False
            If dgvActuaciones.Columns.Contains("ID_Tipo_Actuacion") Then dgvActuaciones.Columns("ID_Tipo_Actuacion").Visible = False
        Catch ex As Exception
            RegistroErrores.Escribir("Casos - actuaciones", ex)
        End Try
    End Sub

    Private Sub Limpiar()
        _idCaso = 0
        txtExpediente.Clear()
        cboMateria.SelectedIndex = 0
        If cboCliente.Items.Count > 0 Then cboCliente.SelectedIndex = 0
        If cboJuzgado.Items.Count > 0 Then cboJuzgado.SelectedIndex = 0
        dtpFechaApertura.Value = Date.Today
        cboEstado.SelectedIndex = 0
        txtResponsable.Clear()
        txtDescripcion.Clear()
        dgvActuaciones.DataSource = Nothing
        LimpiarPlazo()
        MostrarMensaje("", True)

        _cargando = True
        dgvCasos.CurrentCell = Nothing
        _cargando = False
        txtExpediente.Focus()
    End Sub

    Private Sub LimpiarPlazo()
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
        dtpFechaNotificacion.Value = Date.Today
        txtObservaciones.Clear()
        AplicarPlazoDelTipo()
    End Sub

    Private Sub AplicarPlazoDelTipo()
        Dim vista As DataRowView = TryCast(cboTipoActuacion.SelectedItem, DataRowView)
        If vista Is Nothing Then Return
        If Not vista.Row.Table.Columns.Contains("Plazo_Dias_Default") Then Return
        If vista.Row.IsNull("Plazo_Dias_Default") Then Return

        Dim dias As Integer = Convert.ToInt32(vista.Row("Plazo_Dias_Default"))
        If dias > 0 Then nudPlazoDias.Value = Math.Min(dias, nudPlazoDias.Maximum)
    End Sub

    Private Sub GuardarCaso()
        If Not Permisos.ExigirAccion("casos.gestionar") Then Return

        Try
            Dim caso As New Caso With {
                .ID = _idCaso,
                .IDCliente = UtileriasUI.ValorCombo(cboCliente),
                .NumeroExpediente = txtExpediente.Text,
                .Materia = Convert.ToString(cboMateria.SelectedItem),
                .IDJuzgado = IdJuzgadoSeleccionado(),
                .Descripcion = txtDescripcion.Text,
                .Estado = Convert.ToString(cboEstado.SelectedItem),
                .FechaApertura = dtpFechaApertura.Value.Date,
                .Responsable = txtResponsable.Text
            }

            Dim respuesta As Respuesta = _servicio.Guardar(caso)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            _idCaso = caso.ID
            CargarGrid("")
            SeleccionarCaso(caso.ID)
            CargarActuaciones()
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Casos - guardar", ex)
            MostrarMensaje("No se pudo guardar el caso. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Function IdJuzgadoSeleccionado() As Integer?
        Dim id As Integer = UtileriasUI.ValorCombo(cboJuzgado)
        If id <= 0 Then Return Nothing
        Return id
    End Function

    Private Sub CalcularPropuesta()
        Try
            Dim dias As Integer = Convert.ToInt32(nudPlazoDias.Value)
            _fechaPropuesta = _plazos.CalcularVencimiento(dtpFechaNotificacion.Value.Date, dias)

            lblPropuesta.Text = "Propuesta del sistema: " & _fechaPropuesta.Value.ToString("dd/MM/yyyy") &
                " (" & dias & " días hábiles). Revise y confirme."
            lblPropuesta.ForeColor = Color.DarkOrange
        Catch ex As Exception
            _fechaPropuesta = Nothing
            RegistroErrores.Escribir("Casos - calcular plazo", ex)
            lblPropuesta.Text = "No se pudo calcular el plazo propuesto. Verifique la fecha y el tipo de actuación."
            lblPropuesta.ForeColor = Color.Firebrick
        End Try
    End Sub

    Private Sub ConfirmarPlazo()
        If Not Permisos.ExigirAccion("casos.confirmar_plazo") Then Return

        If _idCaso <= 0 Then
            UtileriasUI.MostrarError("Seleccione o guarde primero el caso.")
            Return
        End If

        If Not _fechaPropuesta.HasValue Then
            UtileriasUI.MostrarError("Primero presione ""Calcular"" para generar la propuesta de vencimiento.")
            Return
        End If

        If Not UtileriasUI.Confirmar(
            "¿Confirma el vencimiento del " & _fechaPropuesta.Value.ToString("dd/MM/yyyy") & "?" & Environment.NewLine &
            "Esta confirmación queda registrada a su nombre en la auditoría.") Then
            Return
        End If

        Dim actuacion As New ActuacionProcesal With {
            .IDCaso = _idCaso,
            .IDTipoActuacion = UtileriasUI.ValorCombo(cboTipoActuacion),
            .FechaNotificacion = dtpFechaNotificacion.Value.Date,
            .PlazoLegalDias = Convert.ToInt32(nudPlazoDias.Value),
            .Estado = "Pendiente",
            .Observaciones = txtObservaciones.Text
        }

        Dim respuesta As Respuesta = _plazos.Confirmar(actuacion)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarActuaciones()
        LimpiarPlazo()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub MarcarCumplida()
        If Not Permisos.ExigirAccion("casos.cumplir_plazo") Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvActuaciones)
        If fila Is Nothing Then
            UtileriasUI.MostrarError("Seleccione una actuación de la lista.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Marcar la actuación seleccionada como cumplida?") Then Return

        Dim respuesta As Respuesta = _plazos.MarcarCumplida(UtileriasUI.Entero(fila, "ID"))
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarActuaciones()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub ActualizarVencidas()
        Try
            Dim marcadas As Integer = _plazos.ActualizarVencidas()
            If marcadas > 0 Then
                MostrarMensaje(marcadas & " plazo(s) se marcaron como vencidos.", True)
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Casos - vencidas", ex)
        End Try
    End Sub

    Private Sub SeleccionarCaso(id As Integer)
        For Each fila As DataGridViewRow In dgvCasos.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista IsNot Nothing AndAlso Convert.ToInt32(vista.Row("ID")) = id Then
                dgvCasos.CurrentCell = fila.Cells(0)
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

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Limpiar()
    End Sub

    Private Sub btnGuardarCaso_Click(sender As Object, e As EventArgs) Handles btnGuardarCaso.Click
        GuardarCaso()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarGrid(txtFiltro.Text)
        ActualizarVencidas()
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        CargarGrid(txtFiltro.Text)
    End Sub

    Private Sub btnCalcular_Click(sender As Object, e As EventArgs) Handles btnCalcular.Click
        CalcularPropuesta()
    End Sub

    Private Sub btnConfirmarPlazo_Click(sender As Object, e As EventArgs) Handles btnConfirmarPlazo.Click
        ConfirmarPlazo()
    End Sub

    Private Sub btnMarcarCumplida_Click(sender As Object, e As EventArgs) Handles btnMarcarCumplida.Click
        MarcarCumplida()
    End Sub

    Private Sub cboTipoActuacion_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTipoActuacion.SelectedIndexChanged
        If _cargando Then Return
        AplicarPlazoDelTipo()
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
    End Sub

    Private Sub dtpFechaNotificacion_ValueChanged(sender As Object, e As EventArgs) Handles dtpFechaNotificacion.ValueChanged
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
    End Sub

    Private Sub nudPlazoDias_ValueChanged(sender As Object, e As EventArgs) Handles nudPlazoDias.ValueChanged
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
    End Sub

End Class
