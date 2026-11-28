Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmEntidades

    Private ReadOnly _servicio As New ServicioEntidades()
    Private _cargando As Boolean
    Private _idEntidad As Integer
    Private _permisosInicializados As Boolean

    ''' <summary>Registro que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDEntidadInicial As Integer

    Private Sub FrmEntidades_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("entidades.ver", Me) Then Return

        AplicarPermisosInterfaz()

        UtileriasUI.ConfigurarGrid(dgvEntidades)
        chkActivo.Enabled = False ' El estado solo cambia mediante la acción de desactivación.
        UtileriasUI.ConfigurarGrid(dgvRepresentantes)

        cboNivelRiesgo.Items.Clear()
        cboNivelRiesgo.Items.AddRange(New Object() {"Bajo", "Medio", "Alto"})
        cboNivelRiesgo.SelectedIndex = 0

        cboTipoRepresentacion.Items.Clear()
        cboTipoRepresentacion.Items.AddRange(New Object() {"Legal", "Voluntario", "Judicial"})
        cboTipoRepresentacion.SelectedIndex = 0

        _cargando = True
        CargarGrid("", False)
        _cargando = False
        Dim errorCargaLista As Boolean = dgvEntidades.DataSource Is Nothing
        CargarClientesCombo()
        Dim errorCargaClientes As Boolean = cboRepCliente.DataSource Is Nothing

        If errorCargaLista Then
            Limpiar()
        ElseIf IDEntidadInicial > 0 Then
            SeleccionarEntidad(IDEntidadInicial)
            If _idEntidad = IDEntidadInicial Then
                CargarEntidad(IDEntidadInicial)
            Else
                Limpiar()
                MostrarMensaje("La entidad asociada a la alerta ya no está disponible.", False)
            End If
        Else
            Limpiar()
        End If

        If errorCargaLista AndAlso errorCargaClientes Then
            MostrarMensaje("No se pudieron cargar las entidades ni los clientes para representantes. Verifique la conexión con la base de datos.", False)
        ElseIf errorCargaLista Then
            MostrarMensaje("No se pudo cargar la lista de entidades. Verifique la conexión con la base de datos.", False)
        ElseIf errorCargaClientes Then
            MostrarMensaje("No se pudo cargar la lista de clientes para representantes. Verifique la conexión con la base de datos.", False)
        End If

        _permisosInicializados = True
    End Sub

    Private Sub AplicarPermisosInterfaz()
        Dim puedeGestionar As Boolean = Permisos.Puede("entidades.gestionar")
        btnNuevo.Enabled = puedeGestionar
        btnGuardar.Enabled = puedeGestionar
        btnDesactivar.Enabled = puedeGestionar AndAlso chkActivo.Checked

        txtRazonSocial.ReadOnly = Not puedeGestionar
        txtNIT.ReadOnly = Not puedeGestionar
        txtNRC.ReadOnly = Not puedeGestionar
        txtDomicilio.ReadOnly = Not puedeGestionar
        cboNivelRiesgo.Enabled = puedeGestionar
        chkOrigenVerificado.Enabled = puedeGestionar
        txtDetalleOrigen.ReadOnly = Not puedeGestionar
        dtpVerificacionListas.Enabled = puedeGestionar
        txtCargo.ReadOnly = Not puedeGestionar
        cboTipoRepresentacion.Enabled = puedeGestionar
        ActualizarEstadoRepresentantes()
    End Sub

    Private Sub FrmEntidades_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("entidades.ver") Then
            Close()
            Return
        End If

        AplicarPermisosInterfaz()
    End Sub

    ''' <summary>
    ''' Impide consultas desde un formulario abierto cuando la sesión expiró
    ''' o el permiso de consulta fue revocado en otra sesión.
    ''' </summary>
    Private Function PuedeConsultarEntidades() As Boolean
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return False
        End If

        If Not Permisos.Puede("entidades.ver") Then
            Close()
            Return False
        End If

        Return True
    End Function

    Private Sub CargarGrid(filtro As String, Optional cargarDetalleSeleccionado As Boolean = True)
        If Not PuedeConsultarEntidades() Then Return

        Try
            _cargando = True
            dgvEntidades.DataSource = _servicio.Buscar(filtro)
            ConfigurarColumnasEntidades()
            _cargando = False
            Dim filaActual As DataRow = UtileriasUI.FilaActual(dgvEntidades)
            If filaActual Is Nothing Then
                If cargarDetalleSeleccionado Then Limpiar()
            ElseIf cargarDetalleSeleccionado Then
                CargarEntidad(UtileriasUI.Entero(filaActual, "ID"))
            End If
        Catch ex As Exception
            _cargando = False
            dgvEntidades.DataSource = Nothing
            Limpiar()
            RegistroErrores.Escribir("Entidades - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de entidades. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarClientesCombo()
        If Not PuedeConsultarEntidades() Then Return

        Try
            UtileriasUI.CargarCombo(cboRepCliente, _servicio.ClientesCombo(), "Nombre", "ID")
            ActualizarEstadoRepresentantes()
        Catch ex As Exception
            cboRepCliente.DataSource = Nothing
            ActualizarEstadoRepresentantes()
            RegistroErrores.Escribir("Entidades - clientes", ex)
            MostrarMensaje("No se pudo cargar la lista de clientes para representantes. No se puede asignar un representante hasta actualizar la lista.", False)
        End Try
    End Sub

    Private Sub ActualizarEstadoRepresentantes()
        Dim puedeGestionar As Boolean = Permisos.Puede("entidades.gestionar")
        Dim entidadActiva As Boolean = _idEntidad > 0 AndAlso chkActivo.Checked
        Dim hayClientes As Boolean = cboRepCliente.Items.Count > 0 AndAlso
                                     UtileriasUI.ValorCombo(cboRepCliente) > 0

        ' No habilitar la asignación hasta que exista una entidad activa guardada.
        cboRepCliente.Enabled = puedeGestionar AndAlso entidadActiva AndAlso hayClientes
        txtCargo.Enabled = puedeGestionar AndAlso entidadActiva
        cboTipoRepresentacion.Enabled = puedeGestionar AndAlso entidadActiva
        btnAgregarRep.Enabled = puedeGestionar AndAlso entidadActiva AndAlso hayClientes
        btnQuitarRep.Enabled = puedeGestionar AndAlso _idEntidad > 0 AndAlso
                              UtileriasUI.FilaActual(dgvRepresentantes) IsNot Nothing
    End Sub


    ''' <summary>Organiza los datos de identificación y riesgo de entidades legales.</summary>
    Private Sub ConfigurarColumnasEntidades()
        If dgvEntidades.Columns.Contains("ID") Then dgvEntidades.Columns("ID").Visible = False

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Razon_Social", "Razón social"},
            {"NIT", "NIT"},
            {"NRC", "NRC"},
            {"Riesgo", "Nivel de riesgo"},
            {"Activo", "Estado"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvEntidades.Columns.Contains(par.Key) Then
                dgvEntidades.Columns(par.Key).HeaderText = par.Value
                dgvEntidades.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        If dgvEntidades.Columns.Contains("Razon_Social") Then
            With dgvEntidades.Columns("Razon_Social")
                .FillWeight = 210
                .MinimumWidth = 210
            End With
        End If
        If dgvEntidades.Columns.Contains("NIT") Then
            dgvEntidades.Columns("NIT").FillWeight = 95
            dgvEntidades.Columns("NIT").MinimumWidth = 125
        End If
        If dgvEntidades.Columns.Contains("NRC") Then
            dgvEntidades.Columns("NRC").FillWeight = 75
            dgvEntidades.Columns("NRC").MinimumWidth = 95
        End If
        If dgvEntidades.Columns.Contains("Riesgo") Then
            dgvEntidades.Columns("Riesgo").FillWeight = 75
            dgvEntidades.Columns("Riesgo").MinimumWidth = 95
        End If
        If dgvEntidades.Columns.Contains("Activo") Then
            dgvEntidades.Columns("Activo").FillWeight = 60
            dgvEntidades.Columns("Activo").MinimumWidth = 80
            dgvEntidades.Columns("Activo").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If
    End Sub

    ''' <summary>Oculta claves internas y presenta claramente la representación legal.</summary>
    Private Sub ConfigurarColumnasRepresentantes()
        If dgvRepresentantes.Columns.Contains("ID") Then dgvRepresentantes.Columns("ID").Visible = False
        If dgvRepresentantes.Columns.Contains("ID_Cliente") Then dgvRepresentantes.Columns("ID_Cliente").Visible = False

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Nombre", "Representante"},
            {"DUI", "DUI"},
            {"Cargo", "Cargo"},
            {"Tipo_Representacion", "Tipo de representación"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvRepresentantes.Columns.Contains(par.Key) Then
                dgvRepresentantes.Columns(par.Key).HeaderText = par.Value
                dgvRepresentantes.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        If dgvRepresentantes.Columns.Contains("Nombre") Then
            dgvRepresentantes.Columns("Nombre").FillWeight = 170
            dgvRepresentantes.Columns("Nombre").MinimumWidth = 170
        End If
        If dgvRepresentantes.Columns.Contains("DUI") Then
            dgvRepresentantes.Columns("DUI").FillWeight = 85
            dgvRepresentantes.Columns("DUI").MinimumWidth = 105
        End If
        If dgvRepresentantes.Columns.Contains("Cargo") Then
            dgvRepresentantes.Columns("Cargo").FillWeight = 110
            dgvRepresentantes.Columns("Cargo").MinimumWidth = 110
        End If
        If dgvRepresentantes.Columns.Contains("Tipo_Representacion") Then
            dgvRepresentantes.Columns("Tipo_Representacion").FillWeight = 105
            dgvRepresentantes.Columns("Tipo_Representacion").MinimumWidth = 120
        End If
    End Sub

    Private Sub dgvEntidades_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvEntidades.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim columna As String = dgvEntidades.Columns(e.ColumnIndex).Name
        If columna = "Riesgo" Then
            Select Case Convert.ToString(e.Value).Trim().ToUpperInvariant()
                Case "ALTO"
                    e.CellStyle.ForeColor = Color.DarkRed
                Case "MEDIO"
                    e.CellStyle.ForeColor = Color.DarkGoldenrod
                Case "BAJO"
                    e.CellStyle.ForeColor = Color.DarkGreen
                Case Else
                    e.CellStyle.ForeColor = dgvEntidades.DefaultCellStyle.ForeColor
            End Select
        ElseIf columna = "Activo" Then
            If e.Value Is Nothing OrElse Convert.IsDBNull(e.Value) Then Return

            Dim activo As Boolean
            Dim numero As Integer
            If TypeOf e.Value Is Boolean Then
                activo = CBool(e.Value)
            ElseIf Boolean.TryParse(Convert.ToString(e.Value), activo) Then
                ' El proveedor puede entregar el valor como Boolean.
            ElseIf Integer.TryParse(Convert.ToString(e.Value), numero) Then
                activo = (numero <> 0)
            Else
                Return
            End If

            e.Value = If(activo, "Activa", "Inactiva")
            e.FormattingApplied = True
            e.CellStyle.ForeColor = If(activo, Color.DarkGreen, Color.DimGray)
        End If
    End Sub

    Private Sub dgvRepresentantes_SelectionChanged(sender As Object, e As EventArgs) Handles dgvRepresentantes.SelectionChanged
        ActualizarEstadoRepresentantes()
    End Sub

    Private Sub cboRepCliente_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboRepCliente.SelectedIndexChanged
        ActualizarEstadoRepresentantes()
    End Sub


    Private Sub dgvEntidades_SelectionChanged(sender As Object, e As EventArgs) Handles dgvEntidades.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvEntidades)
        If fila Is Nothing Then
            Limpiar()
            Return
        End If
        CargarEntidad(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarEntidad(id As Integer)
        If Not PuedeConsultarEntidades() Then Return
        If id <= 0 Then
            Limpiar()
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.Obtener(id)
            If tabla.Rows.Count = 0 Then
                ' Evita que Guardar reutilice una entidad seleccionada anteriormente.
                Limpiar()
                Return
            End If

            Dim fila As DataRow = tabla.Rows(0)
            _idEntidad = id

            txtRazonSocial.Text = UtileriasUI.Texto(fila, "Razon_Social")
            txtNIT.Text = UtileriasUI.Texto(fila, "NIT")
            txtNRC.Text = UtileriasUI.Texto(fila, "NRC")
            txtDomicilio.Text = UtileriasUI.Texto(fila, "Domicilio")
            chkActivo.Checked = UtileriasUI.Booleano(fila, "Activo")
            btnDesactivar.Enabled = Permisos.Puede("entidades.gestionar") AndAlso chkActivo.Checked
            ActualizarEstadoRepresentantes()

            Dim riesgo As String = UtileriasUI.Texto(fila, "Nivel_Riesgo")
            If riesgo = "" Then riesgo = "Bajo"
            cboNivelRiesgo.SelectedItem = riesgo
            chkOrigenVerificado.Checked = UtileriasUI.Booleano(fila, "Origen_Fondos_Verificado")
            txtDetalleOrigen.Text = UtileriasUI.Texto(fila, "Detalle_Origen_Fondos")
            UtileriasUI.AsignarFecha(dtpVerificacionListas, UtileriasUI.Fecha(fila, "Fecha_Verificacion_Listas"))

            CargarRepresentantes()
            EvaluarAlertaLdaft()
            MostrarMensaje("", True)
        Catch ex As Exception
            Limpiar()
            RegistroErrores.Escribir("Entidades - detalle", ex)
            MostrarMensaje("No se pudo cargar el detalle de la entidad. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarRepresentantes()
        If Not PuedeConsultarEntidades() Then Return
        If _idEntidad <= 0 Then
            dgvRepresentantes.DataSource = Nothing
            ActualizarEstadoRepresentantes()
            Return
        End If

        Try
            dgvRepresentantes.DataSource = _servicio.Representantes(_idEntidad)
            ConfigurarColumnasRepresentantes()
            ActualizarEstadoRepresentantes()
        Catch ex As Exception
            dgvRepresentantes.DataSource = Nothing
            ActualizarEstadoRepresentantes()
            RegistroErrores.Escribir("Entidades - representantes", ex)
        End Try
    End Sub

    Private Sub Limpiar()
        _idEntidad = 0
        btnDesactivar.Enabled = False
        txtRazonSocial.Clear()
        txtNIT.Clear()
        txtNRC.Clear()
        txtDomicilio.Clear()
        chkActivo.Checked = True
        chkActivo.Enabled = False
        cboNivelRiesgo.SelectedItem = "Bajo"
        chkOrigenVerificado.Checked = False
        txtDetalleOrigen.Clear()
        dtpVerificacionListas.Checked = False
        lblAlerta.Text = ""
        dgvRepresentantes.DataSource = Nothing
        ActualizarEstadoRepresentantes()
        MostrarMensaje("", True)

        _cargando = True
        dgvEntidades.CurrentCell = Nothing
        _cargando = False
        txtRazonSocial.Focus()
    End Sub

    Private Sub Guardar()
        If Not Permisos.ExigirAccion("entidades.gestionar") Then Return

        Try
            If dtpVerificacionListas.Checked AndAlso dtpVerificacionListas.Value.Date > Date.Today Then
                UtileriasUI.MostrarError("La fecha de verificación de listas no puede ser futura.")
                dtpVerificacionListas.Focus()
                Return
            End If

            Dim entidad As New EntidadLegal With {
                .ID = _idEntidad,
                .RazonSocial = txtRazonSocial.Text,
                .NIT = txtNIT.Text,
                .NRC = txtNRC.Text,
                .Domicilio = txtDomicilio.Text,
                .Activo = chkActivo.Checked
            }

            Dim perfil As New PerfilLdaft With {
                .IDEntidad = If(_idEntidad > 0, CType(_idEntidad, Integer?), CType(Nothing, Integer?)),
                .NivelRiesgo = Convert.ToString(cboNivelRiesgo.SelectedItem),
                .OrigenFondosVerificado = chkOrigenVerificado.Checked,
                .DetalleOrigenFondos = txtDetalleOrigen.Text,
                .FechaVerificacionListas = UtileriasUI.LeerFecha(dtpVerificacionListas)
            }

            Dim respuesta As Respuesta = _servicio.Guardar(entidad, perfil)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            _idEntidad = entidad.ID
            CargarGrid(txtBuscarEntidad.Text.Trim())
            If respuesta.ID.HasValue Then SeleccionarEntidad(respuesta.ID.Value)
            CargarRepresentantes()
            EvaluarAlertaLdaft()
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - guardar", ex)
            MostrarMensaje("No se pudo guardar la entidad. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub Desactivar()
        If Not Permisos.ExigirAccion("entidades.gestionar") Then Return

        If _idEntidad <= 0 Then
            UtileriasUI.MostrarError("Seleccione una entidad de la lista.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Desactivar la entidad seleccionada? No se elimina su historial.") Then Return

        Dim respuesta As Respuesta = _servicio.Desactivar(_idEntidad)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        Limpiar()
        CargarGrid(txtBuscarEntidad.Text.Trim())
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub AgregarRepresentante()
        If Not Permisos.ExigirAccion("entidades.gestionar") Then Return

        If _idEntidad <= 0 Then
            UtileriasUI.MostrarError("Guarde primero la entidad para poder agregar representantes.")
            Return
        End If
        If Not chkActivo.Checked Then
            UtileriasUI.MostrarError("No se pueden agregar representantes a una entidad inactiva.")
            Return
        End If

        Dim idCliente As Integer = UtileriasUI.ValorCombo(cboRepCliente)
        If idCliente <= 0 Then
            UtileriasUI.MostrarError("No hay clientes disponibles para asignar como representantes.")
            Return
        End If

        Dim respuesta As Respuesta = _servicio.AgregarRepresentante(
            _idEntidad, idCliente, txtCargo.Text, Convert.ToString(cboTipoRepresentacion.SelectedItem))

        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        txtCargo.Clear()
        CargarRepresentantes()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub QuitarRepresentante()
        If Not Permisos.ExigirAccion("entidades.gestionar") Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvRepresentantes)
        If fila Is Nothing Then
            UtileriasUI.MostrarError("Seleccione un representante.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Retirar esta representación legal? Se eliminará únicamente la asociación de representante con la entidad. El cliente y su historial notarial, de casos y auditoría no se eliminarán.") Then Return

        Dim respuesta As Respuesta = _servicio.QuitarRepresentante(UtileriasUI.Entero(fila, "ID"))
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If
        CargarRepresentantes()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub SeleccionarEntidad(id As Integer)
        For Each fila As DataGridViewRow In dgvEntidades.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' Evita asignar CurrentCell a una columna oculta o no disponible.
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvEntidades.CurrentCell = celda
                    Return
                End If
            Next
            Return
        Next
    End Sub

    Private Sub EvaluarAlertaLdaft()
        lblAlerta.Text = ""
        If Convert.ToString(cboNivelRiesgo.SelectedItem) <> "Alto" Then Return

        Dim problemas As New List(Of String)()
        If Not chkOrigenVerificado.Checked Then problemas.Add("origen de fondos no verificado")

        Dim fechaVerificacion As Date? = UtileriasUI.LeerFecha(dtpVerificacionListas)
        If Not fechaVerificacion.HasValue Then
            problemas.Add("sin fecha de verificación de listas")
        ElseIf fechaVerificacion.Value < Date.Today.AddYears(-1) Then
            problemas.Add("verificación de listas con más de un año")
        End If

        If problemas.Count > 0 Then
            lblAlerta.Text = "ALERTA LDA/FT: " & String.Join(", ", problemas.ToArray())
        End If
    End Sub

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Limpiar()
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        UtileriasUI.EjecutarUnaVez(btnGuardar, AddressOf Guardar)
    End Sub

    Private Sub btnDesactivar_Click(sender As Object, e As EventArgs) Handles btnDesactivar.Click
        UtileriasUI.EjecutarUnaVez(btnDesactivar, AddressOf Desactivar)
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarGrid(txtBuscarEntidad.Text)
    End Sub

    Private Sub btnBuscarEntidad_Click(sender As Object, e As EventArgs) Handles btnBuscarEntidad.Click
        CargarGrid(txtBuscarEntidad.Text)
    End Sub

    Private Sub btnLimpiarBusqueda_Click(sender As Object, e As EventArgs) Handles btnLimpiarBusqueda.Click
        txtBuscarEntidad.Clear()
        CargarGrid("")
        txtBuscarEntidad.Focus()
    End Sub

    Private Sub txtBuscarEntidad_KeyDown(sender As Object, e As KeyEventArgs) Handles txtBuscarEntidad.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarGrid(txtBuscarEntidad.Text)
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            txtBuscarEntidad.Clear()
            CargarGrid("")
        End If
    End Sub

    Private Sub btnAgregarRep_Click(sender As Object, e As EventArgs) Handles btnAgregarRep.Click
        UtileriasUI.EjecutarUnaVez(btnAgregarRep, AddressOf AgregarRepresentante)
    End Sub

    Private Sub btnQuitarRep_Click(sender As Object, e As EventArgs) Handles btnQuitarRep.Click
        UtileriasUI.EjecutarUnaVez(btnQuitarRep, AddressOf QuitarRepresentante)
    End Sub

    Private Sub cboNivelRiesgo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboNivelRiesgo.SelectedIndexChanged
        EvaluarAlertaLdaft()
    End Sub

    Private Sub chkOrigenVerificado_CheckedChanged(sender As Object, e As EventArgs) Handles chkOrigenVerificado.CheckedChanged
        EvaluarAlertaLdaft()
    End Sub

    Private Sub dtpVerificacionListas_ValueChanged(sender As Object, e As EventArgs) Handles dtpVerificacionListas.ValueChanged
        EvaluarAlertaLdaft()
    End Sub

End Class
