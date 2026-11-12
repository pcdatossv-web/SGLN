Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Mantenimiento de usuarios. El rol se elige de la tabla Roles y la regla
''' de "siempre debe quedar un administrador activo" la aplica el servicio
''' (y los triggers de MySQL).
''' </summary>
Public Class FrmUsuarios

    Private ReadOnly _servicio As New ServicioUsuarios()
    Private _idSeleccionado As Integer
    Private _cargando As Boolean

    Private Sub FrmUsuarios_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("config.usuarios", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvUsuarios)
        CargarRoles()
        CargarUsuarios()
        Nuevo()
    End Sub

    Private Sub CargarRoles()
        Try
            UtileriasUI.CargarCombo(cboRol, _servicio.RolesDisponibles(), "Nombre", "ID")
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - roles", ex)
            Mostrar("No se pudieron cargar los roles: " & ex.Message, False)
        End Try
    End Sub

    Private Sub CargarUsuarios()
        Try
            _cargando = True
            dgvUsuarios.DataSource = _servicio.Listar()

            If dgvUsuarios.Columns.Contains("ID") Then dgvUsuarios.Columns("ID").Visible = False
            If dgvUsuarios.Columns.Contains("ID_Rol") Then dgvUsuarios.Columns("ID_Rol").Visible = False
            If dgvUsuarios.Columns.Contains("Activo") Then dgvUsuarios.Columns("Activo").FillWeight = 40
            If dgvUsuarios.Columns.Contains("Rol") Then dgvUsuarios.Columns("Rol").FillWeight = 90
            If dgvUsuarios.Columns.Contains("Fecha_Creacion") Then
                dgvUsuarios.Columns("Fecha_Creacion").DefaultCellStyle.Format = "dd/MM/yyyy"
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - listar", ex)
            Mostrar("No se pudieron cargar los usuarios: " & ex.Message, False)
        Finally
            _cargando = False
        End Try
    End Sub

    Private Sub dgvUsuarios_SelectionChanged(sender As Object, e As EventArgs) Handles dgvUsuarios.SelectionChanged
        If _cargando Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvUsuarios)
        If fila Is Nothing Then Return

        _idSeleccionado = UtileriasUI.Entero(fila, "ID")
        txtUsuario.Text = UtileriasUI.Texto(fila, "Usuario")
        txtUsuario.ReadOnly = True
        txtNombre.Text = UtileriasUI.Texto(fila, "Nombre_Completo")
        UtileriasUI.SeleccionarValor(cboRol, UtileriasUI.Entero(fila, "ID_Rol"))
        chkActivo.Checked = UtileriasUI.Booleano(fila, "Activo")
        btnDesactivar.Enabled = chkActivo.Checked
        btnEliminar.Enabled = EsAdministradorActual() AndAlso _idSeleccionado > 0
        Mostrar("", True)
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Nuevo()
    End Sub

    Private Sub Nuevo()
        _idSeleccionado = 0
        txtUsuario.ReadOnly = False
        txtUsuario.Clear()
        txtNombre.Clear()
        If cboRol.Items.Count > 0 Then cboRol.SelectedIndex = 0
        chkActivo.Checked = True
        txtClaveNueva.Clear()
        txtClaveConfirmar.Clear()
        btnDesactivar.Enabled = False
        btnEliminar.Enabled = False
        Mostrar("", True)
        txtUsuario.Focus()
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If Not Permisos.ExigirAccion("config.usuarios") Then Return

        Dim respuesta As Respuesta

        If _idSeleccionado = 0 Then
            If txtClaveNueva.Text <> txtClaveConfirmar.Text Then
                Mostrar("Las claves no coinciden.", False)
                Return
            End If

            respuesta = _servicio.Crear(
                txtUsuario.Text, txtNombre.Text, txtClaveNueva.Text,
                UtileriasUI.ValorCombo(cboRol), chkActivo.Checked)
        Else
            respuesta = _servicio.Actualizar(
                _idSeleccionado, txtNombre.Text,
                UtileriasUI.ValorCombo(cboRol), chkActivo.Checked)
        End If

        Mostrar(respuesta.Mensaje, respuesta.Exito)

        If respuesta.Exito Then
            CargarUsuarios()
            Nuevo()
        End If
    End Sub

    Private Sub btnRestablecer_Click(sender As Object, e As EventArgs) Handles btnRestablecer.Click
        If Not Permisos.ExigirAccion("config.usuarios") Then Return

        If _idSeleccionado = 0 Then
            Mostrar("Seleccione el usuario al que desea restablecer la clave.", False)
            Return
        End If

        If txtClaveNueva.Text <> txtClaveConfirmar.Text Then
            Mostrar("Las claves no coinciden.", False)
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Restablecer la clave del usuario '" & txtUsuario.Text & "'?") Then Return

        Dim respuesta As Respuesta = _servicio.RestablecerClave(_idSeleccionado, txtClaveNueva.Text)
        Mostrar(respuesta.Mensaje, respuesta.Exito)

        If respuesta.Exito Then
            txtClaveNueva.Clear()
            txtClaveConfirmar.Clear()
        End If
    End Sub

    Private Sub btnDesactivar_Click(sender As Object, e As EventArgs) Handles btnDesactivar.Click
        If Not Permisos.ExigirAccion("config.usuarios") Then Return

        If _idSeleccionado = 0 Then
            Mostrar("Seleccione el usuario que desea desactivar.", False)
            Return
        End If

        If Not UtileriasUI.Confirmar("¿Desactivar el usuario '" & txtUsuario.Text & "'?") Then Return

        Dim respuesta As Respuesta = _servicio.Desactivar(_idSeleccionado)
        Mostrar(respuesta.Mensaje, respuesta.Exito)

        If respuesta.Exito Then
            CargarUsuarios()
            Nuevo()
        End If
    End Sub


    Private Sub btnEliminar_Click(sender As Object, e As EventArgs) Handles btnEliminar.Click
        If Not EsAdministradorActual() Then
            Mostrar("Solo un administrador puede eliminar definitivamente usuarios.", False)
            Return
        End If
        If _idSeleccionado <= 0 Then
            Mostrar("Seleccione el usuario que desea eliminar.", False)
            Return
        End If
        If _idSeleccionado = Sesion.UsuarioActual.ID Then
            Mostrar("No puede eliminar la cuenta con la que inició sesión.", False)
            Return
        End If

        Dim aviso As String =
            "Esta acción es irreversible." & Environment.NewLine & Environment.NewLine &
            "Solo se permitirá si el usuario no tiene registros operativos, referencias históricas ni actividad propia." &
            Environment.NewLine & "La bitácora de creación y eliminación se conservará." &
            Environment.NewLine & Environment.NewLine &
            "¿Desea intentar eliminar definitivamente a '" & txtUsuario.Text & "'?"

        If Not UtileriasUI.Confirmar(aviso) Then Return

        Dim respuesta As Respuesta = _servicio.EliminarDefinitivamente(_idSeleccionado)
        Mostrar(respuesta.Mensaje, respuesta.Exito)
        If respuesta.Exito Then
            CargarUsuarios()
            Nuevo()
        End If
    End Sub

    Private Function EsAdministradorActual() As Boolean
        Return Sesion.UsuarioActual IsNot Nothing AndAlso
               Sesion.UsuarioActual.RolProtegido AndAlso Permisos.AccesoTotal
    End Function

    Private Sub Mostrar(mensaje As String, correcto As Boolean)
        lblMensaje.Text = mensaje
        lblMensaje.ForeColor = If(correcto, Color.FromArgb(0, 100, 0), Color.Firebrick)
    End Sub

End Class
