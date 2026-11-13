Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmClientes

    Private ReadOnly _servicio As New ServicioClientes()
    Private _cargando As Boolean
    Private _idCliente As Integer

    Private Sub FrmClientes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("clientes.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvClientes)

        cboNivelRiesgo.Items.Clear()
        cboNivelRiesgo.Items.AddRange(New Object() {"Bajo", "Medio", "Alto"})
        cboNivelRiesgo.SelectedIndex = 0

        _cargando = True
        CargarGrid("")
        _cargando = False
        Limpiar()
    End Sub

    Private Sub CargarGrid(filtro As String)
        Try
            _cargando = True
            dgvClientes.DataSource = _servicio.Buscar(filtro)
            If dgvClientes.Columns.Contains("ID") Then
                dgvClientes.Columns("ID").Visible = False
            End If
            _cargando = False
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Clientes - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de clientes. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub dgvClientes_SelectionChanged(sender As Object, e As EventArgs) Handles dgvClientes.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvClientes)
        If fila Is Nothing Then Return
        CargarCliente(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarCliente(id As Integer)
        Try
            Dim tabla As DataTable = _servicio.Obtener(id)
            If tabla.Rows.Count = 0 Then Return

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
            RegistroErrores.Escribir("Clientes - detalle", ex)
            MostrarMensaje("No se pudo cargar el detalle del cliente. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub Limpiar()
        _idCliente = 0
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
            If vista IsNot Nothing AndAlso Convert.ToInt32(vista.Row("ID")) = id Then
                Dim indice As Integer = 1
                If dgvClientes.Columns.Contains("Nombre") Then indice = dgvClientes.Columns("Nombre").Index
                dgvClientes.CurrentCell = fila.Cells(indice)
                Return
            End If
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
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Limpiar()
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        Guardar()
    End Sub

    Private Sub btnDesactivar_Click(sender As Object, e As EventArgs) Handles btnDesactivar.Click
        Desactivar()
    End Sub

    Private Sub btnRefrescar_Click(sender As Object, e As EventArgs) Handles btnRefrescar.Click
        CargarGrid(txtBuscar.Text.Trim())
    End Sub

    Private Sub btnEntidades_Click(sender As Object, e As EventArgs) Handles btnEntidades.Click
        Using formulario As New FrmEntidades()
            formulario.ShowDialog(Me)
        End Using
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
