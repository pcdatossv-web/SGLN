Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmCasos

    Private ReadOnly _servicio As New ServicioCasos()
    Private ReadOnly _plazos As New ServicioPlazos()
    Private _cargando As Boolean
    Private _idCaso As Integer
    Private _puedeConfirmarPlazo As Boolean
    Private _puedeCumplirPlazo As Boolean
    Private _permisosInicializados As Boolean
    Private _fechaPropuesta As Date?

    ''' <summary>Identificador del expediente que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDCasoInicial As Integer

    Private Sub FrmCasos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("casos.ver", Me) Then Return

        AplicarPermisosInterfaz()


        ' La fecha futura no es válida para una notificación ya practicada.
        ' El servicio mantiene la misma regla como validación definitiva.
        If dtpFechaNotificacion.Value.Date > Date.Today Then
            dtpFechaNotificacion.Value = Date.Today
        End If
        dtpFechaNotificacion.MaxDate = Date.Today

        UtileriasUI.ConfigurarGrid(dgvCasos)
        UtileriasUI.ConfigurarGrid(dgvActuaciones)

        cboMateria.Items.Clear()
        cboMateria.Items.AddRange(New Object() {
            "Civil", "Mercantil", "Laboral", "Familia", "Penal", "Administrativo", "Notarial"})
        cboMateria.SelectedIndex = 0

        cboEstado.Items.Clear()
        cboEstado.Items.AddRange(New Object() {"Activo", "Suspendido", "Cerrado", "Archivado"})
        cboEstado.SelectedIndex = 0

        _cargando = True
        CargarCombos()
        CargarGrid("")
        _cargando = False

        ActualizarVencidas()
        If IDCasoInicial > 0 Then
            If SeleccionarCaso(IDCasoInicial) Then
                CargarCaso(IDCasoInicial)
            Else
                Limpiar()
                MostrarMensaje("El expediente asociado a la alerta ya no está disponible.", False)
            End If
        Else
            Limpiar()
        End If

        _permisosInicializados = True
    End Sub

    ''' <summary>
    ''' Sincroniza los controles y acciones con los permisos actuales de la
    ''' sesión, conservando el cálculo de plazo que el usuario tenga preparado.
    ''' </summary>
    Private Sub AplicarPermisosInterfaz()
        Dim puedeGestionar As Boolean = Permisos.Puede("casos.gestionar")
        _puedeConfirmarPlazo = Permisos.Puede("casos.confirmar_plazo")
        _puedeCumplirPlazo = Permisos.Puede("casos.cumplir_plazo")

        btnNuevo.Enabled = puedeGestionar
        btnGuardarCaso.Enabled = puedeGestionar

        ' Mantener el expediente legible para usuarios con permiso de consulta,
        ' aunque no tengan autorización para modificar sus datos.
        grpCaso.Enabled = True
        txtExpediente.ReadOnly = Not puedeGestionar
        txtResponsable.ReadOnly = Not puedeGestionar
        txtDescripcion.ReadOnly = Not puedeGestionar
        cboCliente.Enabled = puedeGestionar
        cboMateria.Enabled = puedeGestionar
        cboEstado.Enabled = puedeGestionar
        cboJuzgado.Enabled = puedeGestionar
        dtpFechaApertura.Enabled = puedeGestionar

        cboTipoActuacion.Enabled = _puedeConfirmarPlazo
        dtpFechaNotificacion.Enabled = _puedeConfirmarPlazo
        nudPlazoDias.Enabled = _puedeConfirmarPlazo
        txtObservaciones.Enabled = _puedeConfirmarPlazo
        btnCalcular.Enabled = _puedeConfirmarPlazo

        ActualizarAccionesPlazo()
    End Sub

    Private Sub FrmCasos_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("casos.ver") Then
            Close()
            Return
        End If

        AplicarPermisosInterfaz()
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
            cboCliente.DataSource = Nothing
            cboJuzgado.DataSource = Nothing
            cboTipoActuacion.DataSource = Nothing
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
            Dim filaActual As DataRow = UtileriasUI.FilaActual(dgvCasos)
            If filaActual Is Nothing Then
                Limpiar()
            Else
                CargarCaso(UtileriasUI.Entero(filaActual, "ID"))
            End If
        Catch ex As Exception
            _cargando = False
            dgvCasos.DataSource = Nothing
            Limpiar()
            RegistroErrores.Escribir("Casos - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de casos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub dgvCasos_SelectionChanged(sender As Object, e As EventArgs) Handles dgvCasos.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvCasos)
        If fila Is Nothing Then
            ' Evitar conservar el caso anterior al perder la selección.
            Limpiar()
            Return
        End If
        CargarCaso(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarCaso(id As Integer)
        Try
            Dim tabla As DataTable = _servicio.Obtener(id)
            If tabla.Rows.Count = 0 Then
                ' El registro pudo desaparecer entre la carga de la lista y
                ' la consulta del detalle. No conservar el ID anterior:
                ' Guardar podría modificar otro expediente por selección obsoleta.
                Limpiar()
                Return
            End If

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
            Limpiar()
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
            ActualizarAccionesPlazo()
        Catch ex As Exception
            dgvActuaciones.DataSource = Nothing
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
        ActualizarAccionesPlazo()
        txtExpediente.Focus()
    End Sub

    Private Sub LimpiarPlazo()
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
        dtpFechaNotificacion.Value = Date.Today
        txtObservaciones.Clear()
        AplicarPlazoDelTipo()
        ActualizarAccionesPlazo()
    End Sub

    Private Sub AplicarPlazoDelTipo()
        Dim vista As DataRowView = TryCast(cboTipoActuacion.SelectedItem, DataRowView)
        If vista Is Nothing OrElse
           Not vista.Row.Table.Columns.Contains("Plazo_Dias_Default") OrElse
           vista.Row.IsNull("Plazo_Dias_Default") Then
            ' No conservar el plazo del tipo anterior cuando el nuevo catálogo
            ' no tenga un valor referencial definido.
            nudPlazoDias.Value = nudPlazoDias.Minimum
            Return
        End If

        Dim dias As Integer = Convert.ToInt32(vista.Row("Plazo_Dias_Default"))
        If dias > 0 Then
            nudPlazoDias.Value = Math.Min(dias, nudPlazoDias.Maximum)
            lblPropuesta.Text = "Plazo referencial del catálogo: " & dias.ToString() &
                " días. Verifique el plazo legal aplicable antes de calcular."
            lblPropuesta.ForeColor = Color.DarkOrange
        Else
            nudPlazoDias.Value = nudPlazoDias.Minimum
        End If
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

            lblPropuesta.Text = "Cálculo referencial: " & _fechaPropuesta.Value.ToString("dd/MM/yyyy") &
                " (" & dias & " días hábiles). Verifique el plazo legal, la notificación y los feriados aplicables antes de confirmar."
            lblPropuesta.ForeColor = Color.DarkOrange
            ActualizarAccionesPlazo()
        Catch ex As Exception
            _fechaPropuesta = Nothing
            RegistroErrores.Escribir("Casos - calcular plazo", ex)
            lblPropuesta.Text = "No se pudo calcular el plazo propuesto. Verifique la fecha y el tipo de actuación."
            lblPropuesta.ForeColor = Color.Firebrick
            ActualizarAccionesPlazo()
        End Try
    End Sub

    Private Sub ActualizarAccionesPlazo()
        btnConfirmarPlazo.Enabled = _puedeConfirmarPlazo AndAlso _idCaso > 0 AndAlso _fechaPropuesta.HasValue

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvActuaciones)
        Dim estado As String = If(fila Is Nothing, "", UtileriasUI.Texto(fila, "Estado"))
        Dim puedeMarcar As Boolean = fila IsNot Nothing AndAlso
            (String.Equals(estado, "Pendiente", StringComparison.OrdinalIgnoreCase) OrElse
             String.Equals(estado, "Vencido", StringComparison.OrdinalIgnoreCase))
        btnMarcarCumplida.Enabled = _puedeCumplirPlazo AndAlso puedeMarcar
    End Sub

    Private Sub dgvActuaciones_SelectionChanged(sender As Object, e As EventArgs) Handles dgvActuaciones.SelectionChanged
        ActualizarAccionesPlazo()
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
            "¿Confirma el vencimiento propuesto del " & _fechaPropuesta.Value.ToString("dd/MM/yyyy") & "?" & Environment.NewLine &
            "Confirme que verificó el plazo legal aplicable, la fecha de notificación y los feriados del tribunal." & Environment.NewLine &
            "El sistema realiza el cómputo de días hábiles; no determina por sí mismo el plazo legal." & Environment.NewLine &
            "Esta función solo admite plazos expresados en días; no la use para plazos fijados en meses o años." & Environment.NewLine &
            "La confirmación quedará registrada a su nombre en la auditoría.") Then
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

    Private Function SeleccionarCaso(id As Integer) As Boolean
        For Each fila As DataGridViewRow In dgvCasos.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' La columna ID está oculta; CurrentCell debe apuntar a una celda visible.
            _cargando = True
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvCasos.CurrentCell = celda
                    _cargando = False
                    ' El evento SelectionChanged se omite durante la selección
                    ' programática; cargar explícitamente el caso mantiene
                    ' sincronizados el registro visible y el detalle editable.
                    CargarCaso(id)
                    Return True
                End If
            Next
            _cargando = False
            Return False
        Next
        Return False
    End Function

    Private Sub SeleccionarTexto(combo As ComboBox, valor As String)
        If String.IsNullOrWhiteSpace(valor) Then Return
        For Each elemento As Object In combo.Items
            If String.Equals(Convert.ToString(elemento), valor, StringComparison.OrdinalIgnoreCase) Then
                combo.SelectedItem = elemento
                Return
            End If
        Next
    End Sub

    ''' <summary>
    ''' Resalta visualmente estados de expediente y actuaciones sin alterar los datos.
    ''' </summary>
    Private Sub dgvCasos_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvCasos.CellFormatting
        If e.RowIndex < 0 OrElse Not dgvCasos.Columns.Contains("Estado") Then Return
        Dim estado As String = Convert.ToString(dgvCasos.Rows(e.RowIndex).Cells("Estado").Value)
        Select Case estado.Trim().ToUpperInvariant()
            Case "ACTIVO"
                e.CellStyle.ForeColor = Color.DarkGreen
            Case "SUSPENDIDO"
                e.CellStyle.ForeColor = Color.DarkOrange
            Case "CERRADO", "ARCHIVADO"
                e.CellStyle.ForeColor = Color.DimGray
        End Select
    End Sub


    Private Sub dgvActuaciones_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvActuaciones.CellFormatting
        If e.RowIndex < 0 OrElse Not dgvActuaciones.Columns.Contains("Estado") Then Return
        Dim estado As String = Convert.ToString(dgvActuaciones.Rows(e.RowIndex).Cells("Estado").Value)
        Select Case estado.Trim().ToUpperInvariant()
            Case "VENCIDO"
                e.CellStyle.ForeColor = Color.Firebrick
            Case "CUMPLIDO"
                e.CellStyle.ForeColor = Color.DarkGreen
            Case "PENDIENTE"
                e.CellStyle.ForeColor = Color.DarkOrange
        End Select
    End Sub


    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Limpiar()
    End Sub

    Private Sub btnGuardarCaso_Click(sender As Object, e As EventArgs) Handles btnGuardarCaso.Click
        UtileriasUI.EjecutarUnaVez(btnGuardarCaso, AddressOf GuardarCaso)
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarGrid(txtFiltro.Text)
        ActualizarVencidas()
    End Sub

    Private Sub txtFiltro_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFiltro.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarGrid(txtFiltro.Text)
        End If
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        CargarGrid(txtFiltro.Text)
    End Sub

    Private Sub btnCalcular_Click(sender As Object, e As EventArgs) Handles btnCalcular.Click
        CalcularPropuesta()
    End Sub

    Private Sub btnConfirmarPlazo_Click(sender As Object, e As EventArgs) Handles btnConfirmarPlazo.Click
        UtileriasUI.EjecutarUnaVez(btnConfirmarPlazo, AddressOf ConfirmarPlazo)
    End Sub

    Private Sub btnMarcarCumplida_Click(sender As Object, e As EventArgs) Handles btnMarcarCumplida.Click
        UtileriasUI.EjecutarUnaVez(btnMarcarCumplida, AddressOf MarcarCumplida)
    End Sub

    Private Sub cboTipoActuacion_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTipoActuacion.SelectedIndexChanged
        If _cargando Then Return
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
        AplicarPlazoDelTipo()
        ActualizarAccionesPlazo()
    End Sub

    Private Sub dtpFechaNotificacion_ValueChanged(sender As Object, e As EventArgs) Handles dtpFechaNotificacion.ValueChanged
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
        ActualizarAccionesPlazo()
    End Sub

    Private Sub nudPlazoDias_ValueChanged(sender As Object, e As EventArgs) Handles nudPlazoDias.ValueChanged
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
        ActualizarAccionesPlazo()
    End Sub

End Class
