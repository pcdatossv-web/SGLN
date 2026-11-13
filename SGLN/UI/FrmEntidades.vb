Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmEntidades

    Private ReadOnly _servicio As New ServicioEntidades()
    Private _cargando As Boolean
    Private _idEntidad As Integer

    Private Sub FrmEntidades_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("entidades.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvEntidades)
        UtileriasUI.ConfigurarGrid(dgvRepresentantes)

        cboNivelRiesgo.Items.Clear()
        cboNivelRiesgo.Items.AddRange(New Object() {"Bajo", "Medio", "Alto"})
        cboNivelRiesgo.SelectedIndex = 0

        cboTipoRepresentacion.Items.Clear()
        cboTipoRepresentacion.Items.AddRange(New Object() {"Legal", "Voluntario", "Judicial"})
        cboTipoRepresentacion.SelectedIndex = 0

        _cargando = True
        CargarGrid("")
        _cargando = False
        CargarClientesCombo()
        Limpiar()
    End Sub

    Private Sub CargarGrid(filtro As String)
        Try
            _cargando = True
            dgvEntidades.DataSource = _servicio.Buscar(filtro)
            If dgvEntidades.Columns.Contains("ID") Then dgvEntidades.Columns("ID").Visible = False
            _cargando = False
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Entidades - cargar", ex)
            MostrarMensaje("No se pudo cargar la lista de entidades. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarClientesCombo()
        Try
            UtileriasUI.CargarCombo(cboRepCliente, _servicio.ClientesCombo(), "Nombre", "ID")
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - clientes", ex)
        End Try
    End Sub

    Private Sub dgvEntidades_SelectionChanged(sender As Object, e As EventArgs) Handles dgvEntidades.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvEntidades)
        If fila Is Nothing Then Return
        CargarEntidad(UtileriasUI.Entero(fila, "ID"))
    End Sub

    Private Sub CargarEntidad(id As Integer)
        Try
            Dim tabla As DataTable = _servicio.Obtener(id)
            If tabla.Rows.Count = 0 Then Return

            Dim fila As DataRow = tabla.Rows(0)
            _idEntidad = id

            txtRazonSocial.Text = UtileriasUI.Texto(fila, "Razon_Social")
            txtNIT.Text = UtileriasUI.Texto(fila, "NIT")
            txtNRC.Text = UtileriasUI.Texto(fila, "NRC")
            txtDomicilio.Text = UtileriasUI.Texto(fila, "Domicilio")
            chkActivo.Checked = UtileriasUI.Booleano(fila, "Activo")

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
            RegistroErrores.Escribir("Entidades - detalle", ex)
            MostrarMensaje("No se pudo cargar el detalle de la entidad. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarRepresentantes()
        If _idEntidad <= 0 Then
            dgvRepresentantes.DataSource = Nothing
            Return
        End If

        Try
            dgvRepresentantes.DataSource = _servicio.Representantes(_idEntidad)
            If dgvRepresentantes.Columns.Contains("ID") Then dgvRepresentantes.Columns("ID").Visible = False
            If dgvRepresentantes.Columns.Contains("ID_Cliente") Then dgvRepresentantes.Columns("ID_Cliente").Visible = False
        Catch ex As Exception
            RegistroErrores.Escribir("Entidades - representantes", ex)
        End Try
    End Sub

    Private Sub Limpiar()
        _idEntidad = 0
        txtRazonSocial.Clear()
        txtNIT.Clear()
        txtNRC.Clear()
        txtDomicilio.Clear()
        chkActivo.Checked = True
        cboNivelRiesgo.SelectedItem = "Bajo"
        chkOrigenVerificado.Checked = False
        txtDetalleOrigen.Clear()
        dtpVerificacionListas.Checked = False
        lblAlerta.Text = ""
        dgvRepresentantes.DataSource = Nothing
        MostrarMensaje("", True)

        _cargando = True
        dgvEntidades.CurrentCell = Nothing
        _cargando = False
        txtRazonSocial.Focus()
    End Sub

    Private Sub Guardar()
        If Not Permisos.ExigirAccion("entidades.gestionar") Then Return

        Try
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
            CargarGrid("")
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
        CargarGrid("")
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub AgregarRepresentante()
        If Not Permisos.ExigirAccion("entidades.gestionar") Then Return

        If _idEntidad <= 0 Then
            UtileriasUI.MostrarError("Guarde primero la entidad para poder agregar representantes.")
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

        If Not UtileriasUI.Confirmar("¿Quitar el representante seleccionado?") Then Return

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
            If vista IsNot Nothing AndAlso Convert.ToInt32(vista.Row("ID")) = id Then
                Dim indice As Integer = 1
                If dgvEntidades.Columns.Contains("Razon_Social") Then indice = dgvEntidades.Columns("Razon_Social").Index
                dgvEntidades.CurrentCell = fila.Cells(indice)
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
        CargarGrid("")
    End Sub

    Private Sub btnAgregarRep_Click(sender As Object, e As EventArgs) Handles btnAgregarRep.Click
        AgregarRepresentante()
    End Sub

    Private Sub btnQuitarRep_Click(sender As Object, e As EventArgs) Handles btnQuitarRep.Click
        QuitarRepresentante()
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
