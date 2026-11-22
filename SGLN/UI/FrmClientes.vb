Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmClientes

    Private ReadOnly _servicio As New ServicioClientes()
    Private _cargando As Boolean
    Private _idCliente As Integer
    Private _permisosInicializados As Boolean

    ''' <summary>Registro que debe seleccionarse al abrir desde una alerta.</summary>
    Public Property IDClienteInicial As Integer

    Private Sub FrmClientes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("clientes.ver", Me) Then Return

        AplicarPermisosInterfaz()

        UtileriasUI.ConfigurarGrid(dgvClientes)
        chkActivo.Enabled = False ' El estado solo cambia mediante la acción de desactivación.

        cboNivelRiesgo.Items.Clear()
        cboNivelRiesgo.Items.AddRange(New Object() {"Bajo", "Medio", "Alto"})
        cboNivelRiesgo.SelectedIndex = 0

        _cargando = True
        CargarGrid("", False)
        _cargando = False
        Dim errorCargaLista As Boolean = dgvClientes.DataSource Is Nothing

        If errorCargaLista Then
            Limpiar()
        ElseIf IDClienteInicial > 0 Then
            SeleccionarCliente(IDClienteInicial)
            If _idCliente = IDClienteInicial Then
                CargarCliente(IDClienteInicial)
            Else
                Limpiar()
                MostrarMensaje("El cliente asociado a la alerta ya no está disponible.", False)
            End If
        Else
            Limpiar()
        End If

        If errorCargaLista Then
            MostrarMensaje("No se pudo cargar la lista de clientes. Verifique la conexión con la base de datos.", False)
        End If

        _permisosInicializados = True
    End Sub

    ''' <summary>
    ''' Sincroniza los controles con la matriz de permisos vigente.
    ''' Debe repetirse al regresar de formularios modales porque el rol puede
    ''' haber cambiado mientras este formulario permanecía abierto.
    ''' </summary>
    Private Sub AplicarPermisosInterfaz()
        Dim puedeGestionar As Boolean = Permisos.Puede("clientes.gestionar")
        btnNuevo.Enabled = puedeGestionar
        btnGuardar.Enabled = puedeGestionar
        btnDesactivar.Enabled = _idCliente > 0 AndAlso Permisos.Puede("clientes.desactivar") AndAlso chkActivo.Checked
        btnEntidades.Enabled = Permisos.Puede("entidades.ver")

        ' Permitir consultar y copiar los datos sin habilitar su modificación.
        txtNombres.ReadOnly = Not puedeGestionar
        txtApellidos.ReadOnly = Not puedeGestionar
        txtDUI.ReadOnly = Not puedeGestionar
        txtNIT.ReadOnly = Not puedeGestionar
        txtNacionalidad.ReadOnly = Not puedeGestionar
        txtProfesion.ReadOnly = Not puedeGestionar
        txtDireccion.ReadOnly = Not puedeGestionar
        txtTelefono.ReadOnly = Not puedeGestionar
        txtEmail.ReadOnly = Not puedeGestionar
        txtDetalleOrigen.ReadOnly = Not puedeGestionar
        txtObservaciones.ReadOnly = Not puedeGestionar
        cboNivelRiesgo.Enabled = puedeGestionar
        chkOrigenVerificado.Enabled = puedeGestionar
        dtpNacimiento.Enabled = puedeGestionar
        dtpVerificacionListas.Enabled = puedeGestionar
    End Sub

    Private Sub FrmClientes_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("clientes.ver") Then
            Close()
            Return
        End If

        AplicarPermisosInterfaz()
    End Sub

    Private Sub CargarGrid(filtro As String, Optional cargarDetalleSeleccionado As Boolean = True)
        ' Revalidar en el punto de acceso a datos: Activated no garantiza que
        ' la sesión siga vigente cuando el formulario permanece abierto.
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If
        If Not Permisos.Puede("clientes.ver") Then
            Close()
            Return
        End If

        Try
            _cargando = True
            dgvClientes.DataSource = _servicio.Buscar(filtro)
            ConfigurarColumnasClientes()
            _cargando = False
            Dim filaActual As DataRow = UtileriasUI.FilaActual(dgvClientes)
            If filaActual Is Nothing Then
                If cargarDetalleSeleccionado Then Limpiar()
            ElseIf cargarDetalleSeleccionado Then
                CargarCliente(UtileriasUI.Entero(filaActual, "ID"))
            End If
        Catch ex As Exception
            _cargando = False
            dgvClientes.DataSource = Nothing
            Limpiar()
            RegistroErrores.Escribir("Clientes - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de clientes. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Presenta la lista de clientes con encabezados claros y espacio
    ''' proporcional para identificar a la persona y revisar su riesgo.
    ''' </summary>
    Private Sub ConfigurarColumnasClientes()
        If dgvClientes.Columns.Contains("ID") Then
            dgvClientes.Columns("ID").Visible = False
        End If

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Nombre", "Cliente"},
            {"DUI", "DUI"},
            {"NIT", "NIT"},
            {"Riesgo", "Nivel de riesgo"},
            {"Activo", "Estado"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvClientes.Columns.Contains(par.Key) Then
                dgvClientes.Columns(par.Key).HeaderText = par.Value
                dgvClientes.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        If dgvClientes.Columns.Contains("Nombre") Then
            With dgvClientes.Columns("Nombre")
                .FillWeight = 190
                .MinimumWidth = 180
            End With
        End If
        If dgvClientes.Columns.Contains("DUI") Then
            dgvClientes.Columns("DUI").FillWeight = 85
            dgvClientes.Columns("DUI").MinimumWidth = 105
        End If
        If dgvClientes.Columns.Contains("NIT") Then
            dgvClientes.Columns("NIT").FillWeight = 105
            dgvClientes.Columns("NIT").MinimumWidth = 125
        End If
        If dgvClientes.Columns.Contains("Riesgo") Then
            dgvClientes.Columns("Riesgo").FillWeight = 75
            dgvClientes.Columns("Riesgo").MinimumWidth = 95
        End If
        If dgvClientes.Columns.Contains("Activo") Then
            dgvClientes.Columns("Activo").FillWeight = 60
            dgvClientes.Columns("Activo").MinimumWidth = 80
            dgvClientes.Columns("Activo").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If
    End Sub

    Private Sub dgvClientes_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvClientes.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim columna As String = dgvClientes.Columns(e.ColumnIndex).Name
        If columna = "Riesgo" Then
            Dim riesgo As String = Convert.ToString(e.Value).Trim()
            Select Case riesgo.ToUpperInvariant()
                Case "ALTO"
                    e.CellStyle.ForeColor = Color.DarkRed
                Case "MEDIO"
                    e.CellStyle.ForeColor = Color.DarkGoldenrod
                Case "BAJO"
                    e.CellStyle.ForeColor = Color.DarkGreen
                Case Else
                    e.CellStyle.ForeColor = dgvClientes.DefaultCellStyle.ForeColor
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

            e.Value = If(activo, "Activo", "Inactivo")
            e.FormattingApplied = True
            e.CellStyle.ForeColor = If(activo, Color.DarkGreen, Color.DimGray)
        End If
    End Sub

    Private Sub dgvClientes_SelectionChanged(sender As Object, e As EventArgs) Handles dgvClientes.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvClientes)
        If fila Is Nothing Then
            Limpiar()
            Return
        End If
        CargarCliente(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarCliente(id As Integer)
        If id <= 0 Then
            Limpiar()
            Return
        End If

        ' El detalle también es una lectura protegida. No basta con validar
        ' al activar la ventana, porque puede permanecer enfocada por tiempo.
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If
        If Not Permisos.Puede("clientes.ver") Then
            Close()
            Return
        End If

        Try
            Dim tabla As DataTable = _servicio.Obtener(id)
            If tabla.Rows.Count = 0 Then
                ' Evita conservar el ID de otro cliente si el registro desapareció.
                Limpiar()
                Return
            End If

            Dim fila As DataRow = tabla.Rows(0)
            _idCliente = id

            txtNombres.Text = UtileriasUI.Texto(fila, "Nombres")
            txtApellidos.Text = UtileriasUI.Texto(fila, "Apellidos")
            txtDUI.Text = UtileriasUI.Texto(fila, "DUI")
            txtNIT.Text = UtileriasUI.Texto(fila, "NIT")
            txtNacionalidad.Text = UtileriasUI.Texto(fila, "Nacionalidad")
            txtProfesion.Text = UtileriasUI.Texto(fila, "Profesion")
            UtileriasUI.AsignarFecha(dtpNacimiento, UtileriasUI.Fecha(fila, "Fecha_Nacimiento"))
            txtDireccion.Text = UtileriasUI.Texto(fila, "Direccion")
            txtTelefono.Text = UtileriasUI.Texto(fila, "Telefono")
            txtEmail.Text = UtileriasUI.Texto(fila, "Email")
            chkActivo.Checked = UtileriasUI.Booleano(fila, "Activo")
            btnDesactivar.Enabled = Permisos.Puede("clientes.desactivar") AndAlso chkActivo.Checked

            Dim riesgo As String = UtileriasUI.Texto(fila, "Nivel_Riesgo")
            If riesgo = "" Then riesgo = "Bajo"
            cboNivelRiesgo.SelectedItem = riesgo
            chkOrigenVerificado.Checked = UtileriasUI.Booleano(fila, "Origen_Fondos_Verificado")
            txtDetalleOrigen.Text = UtileriasUI.Texto(fila, "Detalle_Origen_Fondos")
            UtileriasUI.AsignarFecha(dtpVerificacionListas, UtileriasUI.Fecha(fila, "Fecha_Verificacion_Listas"))
            txtObservaciones.Text = UtileriasUI.Texto(fila, "Observaciones_LDAFT")

            EvaluarAlertaLdaft()
            MostrarMensaje("", True)
        Catch ex As Exception
            Limpiar()
            RegistroErrores.Escribir("Clientes - detalle", ex)
            MostrarMensaje("No se pudo cargar el detalle del cliente. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub Limpiar()
        _idCliente = 0
        btnDesactivar.Enabled = False
        txtNombres.Clear()
        txtApellidos.Clear()
        txtDUI.Clear()
        txtNIT.Clear()
        txtNacionalidad.Clear()
        txtProfesion.Clear()
        txtDireccion.Clear()
        txtTelefono.Clear()
        txtEmail.Clear()
        chkActivo.Checked = True
        chkActivo.Enabled = False
        dtpNacimiento.Checked = False
        cboNivelRiesgo.SelectedItem = "Bajo"
        chkOrigenVerificado.Checked = False
        txtDetalleOrigen.Clear()
        txtObservaciones.Clear()
        dtpVerificacionListas.Checked = False
        lblAlerta.Text = ""
        MostrarMensaje("", True)

        _cargando = True
        dgvClientes.CurrentCell = Nothing
        _cargando = False
        txtNombres.Focus()
    End Sub

    Private Sub Guardar()
        If Not Permisos.ExigirAccion("clientes.gestionar") Then Return

        Try
            If Not String.IsNullOrWhiteSpace(txtDUI.Text) Then
                If Not ValidadorIdentidad.EsFormatoDuiValido(txtDUI.Text) Then
                    UtileriasUI.MostrarError("El DUI debe tener 9 dígitos (formato 00000000-0).")
                    txtDUI.Focus()
                    Return
                End If
                If Not ValidadorIdentidad.EsDigitoDuiValido(txtDUI.Text) Then
                    If Not UtileriasUI.Confirmar(
                        "El dígito verificador del DUI no coincide con el número ingresado." & Environment.NewLine &
                        "Verifique el documento físico antes de continuar. ¿Desea guardar de todos modos?") Then
                        txtDUI.Focus()
                        Return
                    End If
                End If
            End If

            If Not String.IsNullOrWhiteSpace(txtNIT.Text) AndAlso Not ValidadorIdentidad.EsFormatoNitValido(txtNIT.Text) Then
                UtileriasUI.MostrarError("El NIT debe tener 14 dígitos.")
                txtNIT.Focus()
                Return
            End If

            ' Validación inmediata en UI sin limitar MaxDate: así los registros
            ' históricos con fechas futuras inválidas pueden cargarse y corregirse.
            If dtpNacimiento.Checked AndAlso dtpNacimiento.Value.Date > Date.Today Then
                UtileriasUI.MostrarError("La fecha de nacimiento no puede ser futura.")
                dtpNacimiento.Focus()
                Return
            End If
            If dtpVerificacionListas.Checked AndAlso dtpVerificacionListas.Value.Date > Date.Today Then
                UtileriasUI.MostrarError("La fecha de verificación de listas no puede ser futura.")
                dtpVerificacionListas.Focus()
                Return
            End If

            Dim cliente As New Cliente With {
                .ID = _idCliente,
                .Nombres = txtNombres.Text,
                .Apellidos = txtApellidos.Text,
                .DUI = txtDUI.Text,
                .NIT = txtNIT.Text,
                .Nacionalidad = txtNacionalidad.Text,
                .Profesion = txtProfesion.Text,
                .FechaNacimiento = UtileriasUI.LeerFecha(dtpNacimiento),
                .Direccion = txtDireccion.Text,
                .Telefono = txtTelefono.Text,
                .Email = txtEmail.Text,
                .Activo = chkActivo.Checked
            }

            Dim perfil As New PerfilLdaft With {
                .IDCliente = If(_idCliente > 0, CType(_idCliente, Integer?), CType(Nothing, Integer?)),
                .NivelRiesgo = Convert.ToString(cboNivelRiesgo.SelectedItem),
                .OrigenFondosVerificado = chkOrigenVerificado.Checked,
                .DetalleOrigenFondos = txtDetalleOrigen.Text,
                .FechaVerificacionListas = UtileriasUI.LeerFecha(dtpVerificacionListas),
                .Observaciones = txtObservaciones.Text
            }

            Dim respuesta As Respuesta = _servicio.Guardar(cliente, perfil)
            If Not respuesta.Exito Then
                MostrarMensaje(respuesta.Mensaje, False)
                Return
            End If

            _idCliente = cliente.ID
            CargarGrid(txtBuscar.Text.Trim())
            If respuesta.ID.HasValue Then SeleccionarCliente(respuesta.ID.Value)
            EvaluarAlertaLdaft()
            MostrarMensaje(respuesta.Mensaje, True)
        Catch ex As Exception
            RegistroErrores.Escribir("Clientes - guardar", ex)
            MostrarMensaje("No se pudo guardar el cliente. Verifique los datos y la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub Desactivar()
        If Not Permisos.ExigirAccion("clientes.desactivar") Then Return

        If _idCliente <= 0 Then
            UtileriasUI.MostrarError("Seleccione un cliente de la lista.")
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Desactivar el cliente seleccionado? No se elimina su historial.") Then Return

        Dim respuesta As Respuesta = _servicio.Desactivar(_idCliente)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        Limpiar()
        CargarGrid(txtBuscar.Text.Trim())
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub SeleccionarCliente(id As Integer)
        For Each fila As DataGridViewRow In dgvClientes.Rows
            Dim vista As DataRowView = TryCast(fila.DataBoundItem, DataRowView)
            If vista Is Nothing OrElse Convert.ToInt32(vista.Row("ID")) <> id Then Continue For

            ' Evita asignar CurrentCell a una columna oculta o no disponible.
            For Each celda As DataGridViewCell In fila.Cells
                If celda.Visible Then
                    dgvClientes.CurrentCell = celda
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

    Private Sub btnBuscar_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        CargarGrid(txtBuscar.Text.Trim())
    End Sub

    Private Sub txtBuscar_KeyDown(sender As Object, e As KeyEventArgs) Handles txtBuscar.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            CargarGrid(txtBuscar.Text.Trim())
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            LimpiarBusqueda()
        End If
    End Sub

    Private Sub LimpiarBusqueda()
        txtBuscar.Clear()
        CargarGrid("")
        txtBuscar.Focus()
    End Sub

    Private Sub btnLimpiarBusqueda_Click(sender As Object, e As EventArgs) Handles btnLimpiarBusqueda.Click
        LimpiarBusqueda()
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
        CargarGrid(txtBuscar.Text.Trim())
    End Sub

    Private Sub btnEntidades_Click(sender As Object, e As EventArgs) Handles btnEntidades.Click
        If Not Permisos.ExigirAccion("entidades.ver") Then Return

        Using formulario As New FrmEntidades()
            formulario.ShowDialog(Me)
        End Using

        ' Al volver del formulario hijo se revalida la cuenta y se refresca
        ' la interfaz: el rol pudo cambiar mientras el diálogo estaba abierto.
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Me.Close()
            Return
        End If

        AplicarPermisosInterfaz()
        CargarGrid(txtBuscar.Text.Trim())
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
