Imports System.Collections.Generic
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
    ' False hasta que el historial del expediente se haya consultado correctamente.
    Private _actuacionesDisponibles As Boolean
    Private _tablaTiposActuacion As DataTable
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
        Dim errorCargaCombos As Boolean =
            cboCliente.DataSource Is Nothing OrElse
            cboJuzgado.DataSource Is Nothing OrElse
            cboTipoActuacion.DataSource Is Nothing
        CargarGrid("", False)
        _cargando = False
        Dim errorCargaLista As Boolean = dgvCasos.DataSource Is Nothing

        If errorCargaLista Then
            Limpiar()
        ElseIf IDCasoInicial > 0 Then
            ' SeleccionarCaso ya carga el detalle y el historial del expediente.
            ' Evitar una segunda consulta al abrir desde una alerta.
            If Not SeleccionarCaso(IDCasoInicial) Then
                Limpiar()
                MostrarMensaje("El expediente asociado a la alerta ya no está disponible.", False)
            End If
        Else
            Limpiar()
        End If

        ' Ejecutar al final de la carga inicial: así el mensaje no se pierde
        ' al limpiar/seleccionar el expediente y las actuaciones se refrescan.
        ActualizarVencidas()
        If errorCargaLista Then
            MostrarMensaje("No se pudo cargar la lista de casos. Verifique la conexión con la base de datos.", False)
        ElseIf errorCargaCombos Then
            MostrarMensaje("No se pudieron cargar los catálogos de casos. Verifique la conexión con la base de datos.", False)
        ElseIf dgvCasos.Rows.Count = 0 Then
            MostrarInformacion("No hay expedientes registrados. Puede crear uno con el botón ""Nuevo"".")
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

    ''' <summary>
    ''' Evita consultas desde un expediente abierto si la sesión expiró
    ''' o se revocó el permiso de lectura desde otra sesión.
    ''' </summary>
    Private Function PuedeConsultarCasos() As Boolean
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return False
        End If

        If Not Permisos.Puede("casos.ver") Then
            Close()
            Return False
        End If

        Return True
    End Function

    Private Sub CargarCombos()
        If Not PuedeConsultarCasos() Then Return

        Try
            UtileriasUI.CargarCombo(cboCliente, _servicio.ClientesCombo(), "Nombre", "ID")

            Dim juzgados As DataTable = _servicio.JuzgadosCombo()
            Dim sinJuzgado As DataRow = juzgados.NewRow()
            sinJuzgado("ID") = 0
            sinJuzgado("Nombre") = "(Sin juzgado asignado)"
            juzgados.Rows.InsertAt(sinJuzgado, 0)
            UtileriasUI.CargarCombo(cboJuzgado, juzgados, "Nombre", "ID")

            _tablaTiposActuacion = _servicio.TiposActuacionCombo()
            UtileriasUI.CargarCombo(cboTipoActuacion, _tablaTiposActuacion, "Nombre", "ID")
            FiltrarTiposActuacionPorMateria()
            AplicarPlazoDelTipo()
        Catch ex As Exception
            cboCliente.DataSource = Nothing
            cboJuzgado.DataSource = Nothing
            cboTipoActuacion.DataSource = Nothing
            RegistroErrores.Escribir("Casos - combos", ex)
            MostrarMensaje("No se pudieron cargar los catálogos. Verifique la conexión con la base de datos.", False)
        End Try
        ActualizarAccionesPlazo()
    End Sub

    Private Sub CargarGrid(filtro As String, Optional cargarDetalleSeleccionado As Boolean = True)
        If Not PuedeConsultarCasos() Then Return

        Try
            _cargando = True
            dgvCasos.DataSource = _servicio.Buscar(filtro)
            ConfigurarColumnasCasos()
            _cargando = False
            Dim filaActual As DataRow = UtileriasUI.FilaActual(dgvCasos)
            If filaActual Is Nothing Then
                If cargarDetalleSeleccionado Then Limpiar()
            ElseIf cargarDetalleSeleccionado Then
                CargarCaso(UtileriasUI.Entero(filaActual, "ID"))
            End If

            If cargarDetalleSeleccionado AndAlso dgvCasos.Rows.Count = 0 Then
                Dim termino As String = If(filtro, "").Trim()
                If termino.Length = 0 Then
                    MostrarInformacion("No hay expedientes registrados.")
                Else
                    MostrarInformacion("No se encontraron expedientes que coincidan con """ & termino & """.")
                    ' Facilitar la corrección inmediata del criterio sin obligar
                    ' al usuario a volver a buscar el cuadro de filtro.
                    txtFiltro.Focus()
                    txtFiltro.SelectAll()
                End If
            End If
        Catch ex As Exception
            _cargando = False
            dgvCasos.DataSource = Nothing
            Limpiar()
            RegistroErrores.Escribir("Casos - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de casos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Presenta el listado con encabezados legibles y tamaños previsibles.
    ''' La consulta conserva los nombres técnicos para no alterar servicios.
    ''' </summary>
    Private Sub ConfigurarColumnasCasos()
        If dgvCasos.Columns.Contains("ID") Then dgvCasos.Columns("ID").Visible = False

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Numero_Expediente", "Expediente"},
            {"Cliente", "Cliente"},
            {"Materia", "Materia"},
            {"Juzgado", "Juzgado"},
            {"Estado", "Estado"},
            {"Fecha_Apertura", "Apertura"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvCasos.Columns.Contains(par.Key) Then
                dgvCasos.Columns(par.Key).HeaderText = par.Value
                dgvCasos.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        If dgvCasos.Columns.Contains("Numero_Expediente") Then dgvCasos.Columns("Numero_Expediente").Width = 115
        If dgvCasos.Columns.Contains("Cliente") Then dgvCasos.Columns("Cliente").MinimumWidth = 130
        If dgvCasos.Columns.Contains("Materia") Then dgvCasos.Columns("Materia").Width = 85
        If dgvCasos.Columns.Contains("Juzgado") Then dgvCasos.Columns("Juzgado").MinimumWidth = 110
        If dgvCasos.Columns.Contains("Estado") Then dgvCasos.Columns("Estado").Width = 85
        If dgvCasos.Columns.Contains("Fecha_Apertura") Then
            dgvCasos.Columns("Fecha_Apertura").Width = 90
            dgvCasos.Columns("Fecha_Apertura").DefaultCellStyle.Format = "dd/MM/yyyy"
        End If
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
        If Not PuedeConsultarCasos() Then Return
        If id <= 0 Then
            Limpiar()
            Return
        End If

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
            If _actuacionesDisponibles Then MostrarMensaje("", True)
        Catch ex As Exception
            Limpiar()
            RegistroErrores.Escribir("Casos - detalle", ex)
            MostrarMensaje("No se pudo cargar el detalle del caso. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarActuaciones()
        If Not PuedeConsultarCasos() Then Return
        If _idCaso <= 0 Then
            dgvActuaciones.DataSource = Nothing
            Return
        End If

        _actuacionesDisponibles = False
        ActualizarAccionesPlazo()

        Try
            dgvActuaciones.DataSource = _plazos.Actuaciones(_idCaso)
            ConfigurarColumnasActuaciones()
            _actuacionesDisponibles = True
            ActualizarAccionesPlazo()
        Catch ex As Exception
            dgvActuaciones.DataSource = Nothing
            _actuacionesDisponibles = False
            ActualizarAccionesPlazo()
            RegistroErrores.Escribir("Casos - actuaciones", ex)
            MostrarMensaje("No se pudieron cargar las actuaciones del expediente. No se habilitarán acciones hasta recuperar la información.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Presenta el historial procesal con nombres claros y fechas uniformes.
    ''' Los identificadores técnicos permanecen ocultos y la grilla es de consulta.
    ''' </summary>
    Private Sub ConfigurarColumnasActuaciones()
        If dgvActuaciones.Columns.Contains("ID") Then dgvActuaciones.Columns("ID").Visible = False
        If dgvActuaciones.Columns.Contains("ID_Tipo_Actuacion") Then dgvActuaciones.Columns("ID_Tipo_Actuacion").Visible = False

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Tipo", "Actuación"},
            {"Fecha_Notificacion", "Notificación"},
            {"Plazo_Legal_Dias", "Días hábiles"},
            {"Fecha_Vencimiento_Calculada", "Vencimiento"},
            {"Estado", "Estado"},
            {"Observaciones", "Observaciones"},
            {"Fecha_Confirmacion", "Confirmada el"},
            {"Usuario_Confirma", "Confirmada por"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvActuaciones.Columns.Contains(par.Key) Then
                dgvActuaciones.Columns(par.Key).HeaderText = par.Value
                dgvActuaciones.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        ' La grilla es exclusivamente de consulta: selección por fila y sin
        ' edición accidental. Las dimensiones explícitas evitan que una
        ' observación extensa desplace las fechas y el estado fuera de vista.
        dgvActuaciones.ReadOnly = True
        dgvActuaciones.MultiSelect = False
        dgvActuaciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvActuaciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        dgvActuaciones.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        dgvActuaciones.DefaultCellStyle.WrapMode = DataGridViewTriState.True

        Dim anchos As New Dictionary(Of String, Integer) From {
            {"Tipo", 175},
            {"Fecha_Notificacion", 100},
            {"Plazo_Legal_Dias", 85},
            {"Fecha_Vencimiento_Calculada", 105},
            {"Estado", 95},
            {"Fecha_Confirmacion", 105},
            {"Usuario_Confirma", 120}
        }

        For Each par As KeyValuePair(Of String, Integer) In anchos
            If dgvActuaciones.Columns.Contains(par.Key) Then
                dgvActuaciones.Columns(par.Key).Width = par.Value
                dgvActuaciones.Columns(par.Key).MinimumWidth = Math.Min(par.Value, 75)
            End If
        Next

        For Each nombre As String In {"Fecha_Notificacion", "Fecha_Vencimiento_Calculada", "Fecha_Confirmacion"}
            If dgvActuaciones.Columns.Contains(nombre) Then
                With dgvActuaciones.Columns(nombre)
                    .DefaultCellStyle.Format = "dd/MM/yyyy"
                    .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                End With
            End If
        Next

        If dgvActuaciones.Columns.Contains("Plazo_Legal_Dias") Then
            dgvActuaciones.Columns("Plazo_Legal_Dias").DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter
        End If

        If dgvActuaciones.Columns.Contains("Observaciones") Then
            With dgvActuaciones.Columns("Observaciones")
                .MinimumWidth = 180
                .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                .DefaultCellStyle.WrapMode = DataGridViewTriState.True
            End With
        End If
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
        _actuacionesDisponibles = False
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

    ''' <summary>
    ''' Limita el catálogo visible a tipos genéricos o compatibles con la
    ''' materia del expediente. El repositorio repite la validación al guardar.
    ''' </summary>
    Private Sub FiltrarTiposActuacionPorMateria()
        If _tablaTiposActuacion Is Nothing OrElse Not _tablaTiposActuacion.Columns.Contains("Materia") Then Return

        Dim idAnterior As Integer = UtileriasUI.ValorCombo(cboTipoActuacion)
        Dim materia As String = Convert.ToString(cboMateria.SelectedItem).Trim()
        Dim vista As DataView = _tablaTiposActuacion.DefaultView
        Dim filtro As String = "Materia IS NULL OR Materia = '' OR Materia = 'General'"
        If materia.Length > 0 Then
            filtro &= " OR Materia = '" & materia.Replace("'", "''") & "'"
        End If

        Dim cargandoAnterior As Boolean = _cargando
        _cargando = True
        Try
            vista.RowFilter = filtro
            cboTipoActuacion.DataSource = Nothing
            cboTipoActuacion.DisplayMember = "Nombre"
            cboTipoActuacion.ValueMember = "ID"
            cboTipoActuacion.DataSource = vista

            If vista.Count = 0 Then
                cboTipoActuacion.SelectedIndex = -1
            ElseIf idAnterior > 0 Then
                UtileriasUI.SeleccionarValor(cboTipoActuacion, idAnterior)
                If cboTipoActuacion.SelectedIndex < 0 Then cboTipoActuacion.SelectedIndex = 0
            Else
                cboTipoActuacion.SelectedIndex = 0
            End If
        Finally
            _cargando = cargandoAnterior
        End Try
    End Sub

    Private Sub AplicarPlazoDelTipo()
        Dim vista As DataRowView = TryCast(cboTipoActuacion.SelectedItem, DataRowView)
        If vista Is Nothing OrElse
           Not vista.Row.Table.Columns.Contains("Plazo_Dias_Default") OrElse
           vista.Row.IsNull("Plazo_Dias_Default") Then
            ' No conservar el plazo del tipo anterior cuando el nuevo catálogo
            ' no tenga un valor referencial definido.
            nudPlazoDias.Value = nudPlazoDias.Minimum
            If cboTipoActuacion.Items.Count = 0 Then
                lblPropuesta.Text = "No hay tipos de actuación vigentes para la materia seleccionada."
                lblPropuesta.ForeColor = Color.DarkOrange
            End If
            ActualizarAccionesPlazo()
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

            ' Actualizar la lista sin cargar automáticamente un expediente distinto.
            ' SeleccionarCaso carga el detalle y el historial una sola vez.
            CargarGrid("", False)

            If dgvCasos.DataSource Is Nothing Then
                Limpiar()
                MostrarMensaje(respuesta.Mensaje &
                    " El expediente se guardó, pero no se pudo actualizar la lista. Presione ""Refrescar"" para recuperar la vista.", False)
                Return
            End If

            If Not SeleccionarCaso(caso.ID) Then
                Limpiar()
                MostrarMensaje(respuesta.Mensaje &
                    " El expediente se guardó, pero no se pudo localizar en la lista actualizada. Presione ""Refrescar"" y vuelva a seleccionarlo.", False)
                Return
            End If

            If _actuacionesDisponibles Then
                MostrarMensaje(respuesta.Mensaje, True)
            Else
                MostrarMensaje(respuesta.Mensaje & " El historial no pudo actualizarse; las acciones de plazos permanecen bloqueadas.", False)
            End If
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
        Dim historialDisponible As Boolean = _actuacionesDisponibles AndAlso _idCaso > 0
        Dim tipoActuacionValido As Boolean = UtileriasUI.ValorCombo(cboTipoActuacion) > 0
        btnCalcular.Enabled = _puedeConfirmarPlazo AndAlso tipoActuacionValido
        btnConfirmarPlazo.Enabled = _puedeConfirmarPlazo AndAlso historialDisponible AndAlso
                                    tipoActuacionValido AndAlso _fechaPropuesta.HasValue

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvActuaciones)
        Dim estado As String = If(fila Is Nothing, "", UtileriasUI.Texto(fila, "Estado"))
        Dim puedeMarcar As Boolean = fila IsNot Nothing AndAlso
            (String.Equals(estado, "Pendiente", StringComparison.OrdinalIgnoreCase) OrElse
             String.Equals(estado, "Vencido", StringComparison.OrdinalIgnoreCase))
        btnMarcarCumplida.Enabled = _puedeCumplirPlazo AndAlso historialDisponible AndAlso puedeMarcar
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

        If Not _actuacionesDisponibles Then
            MostrarMensaje("El historial no está disponible. No se pueden registrar plazos hasta recuperar la consulta.", False)
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
        If _actuacionesDisponibles Then
            MostrarMensaje(respuesta.Mensaje, True)
        Else
            MostrarMensaje(respuesta.Mensaje & " El historial no pudo actualizarse; las acciones de plazos permanecen bloqueadas.", False)
        End If
    End Sub

    Private Sub MarcarCumplida()
        If Not Permisos.ExigirAccion("casos.cumplir_plazo") Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvActuaciones)
        If Not _actuacionesDisponibles Then
            MostrarMensaje("El historial no está disponible. No se puede modificar una actuación hasta recuperar la consulta.", False)
            Return
        End If

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
        If _actuacionesDisponibles Then
            MostrarMensaje(respuesta.Mensaje, True)
        Else
            MostrarMensaje(respuesta.Mensaje & " El historial no pudo actualizarse; las acciones de plazos permanecen bloqueadas.", False)
        End If
    End Sub

    Private Sub ActualizarVencidas()
        ' La actualización global de vencimientos requiere el permiso específico.
        ' Omitirla para usuarios de solo lectura evita invocaciones denegadas y
        ' registros de error innecesarios al abrir o refrescar el formulario.
        If Not _puedeCumplirPlazo Then Return

        Try
            Dim marcadas As Integer = _plazos.ActualizarVencidas()
            If marcadas < 0 Then
                MostrarMensaje("No se pudieron actualizar los vencimientos. Los estados podrían no estar al día; revise la conexión y el registro de errores.", False)
                Return
            End If

            If marcadas > 0 Then
                ' La actualización afecta actuaciones de todos los expedientes.
                ' Refrescar el detalle visible para no dejar estados obsoletos.
                If _idCaso > 0 Then CargarActuaciones()
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
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse
           dgvCasos.Columns(e.ColumnIndex).Name <> "Estado" Then Return
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
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim nombreColumna As String = dgvActuaciones.Columns(e.ColumnIndex).Name
        Dim fila As DataGridViewRow = dgvActuaciones.Rows(e.RowIndex)
        Dim estado As String = If(dgvActuaciones.Columns.Contains("Estado"),
                                  Convert.ToString(fila.Cells("Estado").Value), "")

        If nombreColumna = "Estado" Then
            e.CellStyle.ForeColor = Color.Empty
            e.CellStyle.BackColor = Color.Empty
            Select Case estado.Trim().ToUpperInvariant()
                Case "VENCIDO"
                    e.CellStyle.ForeColor = Color.Firebrick
                    e.CellStyle.BackColor = Color.MistyRose
                Case "CUMPLIDO"
                    e.CellStyle.ForeColor = Color.DarkGreen
                    e.CellStyle.BackColor = Color.Honeydew
                Case "PENDIENTE"
                    e.CellStyle.ForeColor = Color.DarkOrange
                    If dgvActuaciones.Columns.Contains("Fecha_Vencimiento_Calculada") Then
                        Dim valorVencimiento As Object = fila.Cells("Fecha_Vencimiento_Calculada").Value
                        If valorVencimiento IsNot Nothing AndAlso Not Convert.IsDBNull(valorVencimiento) Then
                            Dim fechaVencimiento As Date
                            If TypeOf valorVencimiento Is Date Then
                                fechaVencimiento = CDate(valorVencimiento).Date
                            ElseIf Date.TryParse(Convert.ToString(valorVencimiento), fechaVencimiento) Then
                                fechaVencimiento = fechaVencimiento.Date
                            Else
                                Return
                            End If
                            If fechaVencimiento < Date.Today Then
                                e.CellStyle.ForeColor = Color.Firebrick
                                e.CellStyle.BackColor = Color.MistyRose
                            ElseIf fechaVencimiento <= Date.Today.AddDays(3) Then
                                e.CellStyle.BackColor = Color.LemonChiffon
                            End If
                        End If
                    End If
            End Select
            Return
        End If

        ' El color del vencimiento permite detectar plazos críticos incluso
        ' cuando el usuario no tiene permiso para ejecutar la actualización
        ' global que cambia Pendiente a Vencido en la base de datos.
        If nombreColumna = "Fecha_Vencimiento_Calculada" Then
            ' Restablece estilos previos para evitar colores persistentes tras refrescar.
            e.CellStyle.ForeColor = Color.Empty
            e.CellStyle.BackColor = Color.Empty
        End If


        If nombreColumna = "Fecha_Vencimiento_Calculada" AndAlso
           e.Value IsNot Nothing AndAlso Not Convert.IsDBNull(e.Value) Then
            Dim vencimiento As Date
            If TypeOf e.Value Is Date Then
                vencimiento = CDate(e.Value).Date
            ElseIf Not Date.TryParse(Convert.ToString(e.Value), vencimiento) Then
                Return
            Else
                vencimiento = vencimiento.Date
            End If

            If String.Equals(estado, "Cumplido", StringComparison.OrdinalIgnoreCase) Then Return

            If vencimiento < Date.Today OrElse
               String.Equals(estado, "Vencido", StringComparison.OrdinalIgnoreCase) Then
                e.CellStyle.ForeColor = Color.Firebrick
                e.CellStyle.BackColor = Color.MistyRose
            ElseIf vencimiento <= Date.Today.AddDays(3) Then
                e.CellStyle.ForeColor = Color.DarkGoldenrod
                e.CellStyle.BackColor = Color.LemonChiffon
            End If
        End If
    End Sub


    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    ''' <summary>
    ''' Muestra información neutral sin presentarla como éxito o error.
    ''' </summary>
    Private Sub MostrarInformacion(texto As String)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = Color.DimGray
    End Sub

    ''' <summary>
    ''' Atajos de teclado para acelerar la consulta y gestión de expedientes.
    ''' Los botones siguen siendo el punto único de entrada y conservan sus
    ''' validaciones de permisos y protección contra doble ejecución.
    ''' </summary>
    Private Sub FrmCasos_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Alt OrElse e.Shift Then Return

        If e.Control Then
            Select Case e.KeyCode
                Case Keys.F
                    txtFiltro.Focus()
                    txtFiltro.SelectAll()
                Case Keys.N
                    If btnNuevo.Enabled Then btnNuevo.PerformClick()
                Case Keys.S
                    If btnGuardarCaso.Enabled Then btnGuardarCaso.PerformClick()
                Case Else
                    Return
            End Select

            e.Handled = True
            e.SuppressKeyPress = True
            Return
        End If

        If e.KeyCode = Keys.F5 Then
            btnRefrescar.PerformClick()
            e.Handled = True
            e.SuppressKeyPress = True
        End If
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
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            LimpiarFiltroCasos()
        End If
    End Sub

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        CargarGrid(txtFiltro.Text)
    End Sub

    Private Sub btnLimpiarFiltro_Click(sender As Object, e As EventArgs) Handles btnLimpiarFiltro.Click
        LimpiarFiltroCasos()
    End Sub

    Private Sub LimpiarFiltroCasos()
        txtFiltro.Clear()
        CargarGrid("")
        txtFiltro.Focus()
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

    Private Sub cboMateria_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboMateria.SelectedIndexChanged
        If _cargando Then Return
        FiltrarTiposActuacionPorMateria()
        _fechaPropuesta = Nothing
        lblPropuesta.Text = "Sin cálculo. Presione ""Calcular"" y confirme el resultado."
        lblPropuesta.ForeColor = Color.DimGray
        AplicarPlazoDelTipo()
        ActualizarAccionesPlazo()
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
