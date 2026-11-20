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
    Private _permisosInicializados As Boolean

    Private Sub FrmUsuarios_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("config.usuarios", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvUsuarios)
        CargarRoles()
        CargarUsuarios()
        Nuevo()
        _permisosInicializados = True
    End Sub

    Private Sub FrmUsuarios_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("config.usuarios") Then Close()
    End Sub

    Private Sub CargarRoles()
        Try
            UtileriasUI.CargarCombo(cboRol, _servicio.RolesDisponibles(), "Nombre", "ID")
        Catch ex As Exception
            cboRol.DataSource = Nothing
            RegistroErrores.Escribir("Usuarios - roles", ex)
            Mostrar("No se pudieron cargar los roles. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarUsuarios()
        Dim errorCarga As Boolean = False
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
            Mostrar("No se pudieron cargar los usuarios. Verifique la conexión con la base de datos.", False)
            errorCarga = True
            dgvUsuarios.DataSource = Nothing
        Finally
            _cargando = False
            Dim actual As DataRow = UtileriasUI.FilaActual(dgvUsuarios)
            If actual Is Nothing Then
                Nuevo()
                If errorCarga Then
                    Mostrar("No se pudieron cargar los usuarios. Verifique la conexión con la base de datos.", False)
                End If
            Else
                dgvUsuarios_SelectionChanged(dgvUsuarios, EventArgs.Empty)
            End If
        End Try
    End Sub

    Private Sub dgvUsuarios_SelectionChanged(sender As Object, e As EventArgs) Handles dgvUsuarios.SelectionChanged
        If _cargando Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvUsuarios)
        If fila Is Nothing Then
            ' No conservar un usuario operativo cuando la selección se limpia.
            ' Evita que las acciones actúen sobre un registro que ya no está visible.
            Nuevo()
            Return
        End If

        _idSeleccionado = UtileriasUI.Entero(fila, "ID")
        txtUsuario.Text = UtileriasUI.Texto(fila, "Usuario")
        txtUsuario.ReadOnly = True
        txtNombre.Text = UtileriasUI.Texto(fila, "Nombre_Completo")
        UtileriasUI.SeleccionarValor(cboRol, UtileriasUI.Entero(fila, "ID_Rol"))
        chkActivo.Checked = UtileriasUI.Booleano(fila, "Activo")
        ' Permitir reactivar/desactivar desde el formulario, excepto la cuenta propia.
        ' El servicio vuelve a validar la regla del último administrador activo.
        ' El estado es informativo; se modifica únicamente con la acción
        ' explícita Desactivar/Reactivar para evitar cambios ambiguos al guardar.
        chkActivo.Enabled = False
        ActualizarEstadoAccionesUsuario()
        ' No ofrecer la eliminación de la cuenta que mantiene abierta la sesión.
        ' El servicio conserva la misma restricción como control definitivo.
        btnEliminar.Enabled =
            EsAdministradorActual() AndAlso
            _idSeleccionado > 0 AndAlso
            Sesion.UsuarioActual IsNot Nothing AndAlso
            _idSeleccionado <> Sesion.UsuarioActual.ID
        Mostrar("", True)
    End Sub

    Private Sub chkActivo_CheckedChanged(sender As Object, e As EventArgs) Handles chkActivo.CheckedChanged
        If _cargando Then Return
        ActualizarEstadoAccionesUsuario()
    End Sub

    Private Sub ActualizarEstadoAccionesUsuario()
        Dim esOtroUsuario As Boolean =
            _idSeleccionado > 0 AndAlso
            Sesion.UsuarioActual IsNot Nothing AndAlso
            _idSeleccionado <> Sesion.UsuarioActual.ID

        btnDesactivar.Text = If(chkActivo.Checked, "Desactivar usuario", "Reactivar usuario")
        btnDesactivar.Enabled = esOtroUsuario
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
        chkActivo.Enabled = True
        btnDesactivar.Text = "Desactivar usuario"
        txtClaveNueva.Clear()
        txtClaveConfirmar.Clear()
        btnDesactivar.Enabled = False
        btnEliminar.Enabled = False
        Mostrar("", True)
        txtUsuario.Focus()
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        UtileriasUI.EjecutarUnaVez(btnGuardar, AddressOf GuardarUsuario)
    End Sub

    Private Sub GuardarUsuario()
        If Not Permisos.ExigirAccion("config.usuarios") Then Return
        Try

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
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - btnguardar", ex)
            Mostrar("No se pudo completar la operación. Verifique la conexión y vuelva a intentarlo.", False)
        End Try
    End Sub

    Private Sub btnRestablecer_Click(sender As Object, e As EventArgs) Handles btnRestablecer.Click
        UtileriasUI.EjecutarUnaVez(btnRestablecer, AddressOf RestablecerClaveUsuario)
    End Sub

    Private Sub RestablecerClaveUsuario()
        If Not Permisos.ExigirAccion("config.usuarios") Then Return
        Try

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
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - btnrestablecer", ex)
            Mostrar("No se pudo completar la operación. Verifique la conexión y vuelva a intentarlo.", False)
        End Try
    End Sub

    Private Sub btnDesactivar_Click(sender As Object, e As EventArgs) Handles btnDesactivar.Click
        UtileriasUI.EjecutarUnaVez(btnDesactivar, AddressOf CambiarEstadoUsuario, Function() _idSeleccionado > 0 AndAlso Sesion.UsuarioActual IsNot Nothing AndAlso _idSeleccionado <> Sesion.UsuarioActual.ID)
    End Sub

    Private Sub CambiarEstadoUsuario()
        If Not Permisos.ExigirAccion("config.usuarios") Then Return
        Try
            If _idSeleccionado <= 0 Then
                Mostrar("Seleccione un usuario.", False)
                Return
            End If

            Dim activar As Boolean = Not chkActivo.Checked
            Dim accion As String = If(activar, "reactivar", "desactivar")
            If Not UtileriasUI.Confirmar("¿Desea " & accion & " el usuario '" & txtUsuario.Text & "'?") Then Return

            Dim respuesta As Respuesta =
                If(activar, _servicio.Activar(_idSeleccionado), _servicio.Desactivar(_idSeleccionado))
            Mostrar(respuesta.Mensaje, respuesta.Exito)

            If respuesta.Exito Then
                CargarUsuarios()
                Nuevo()
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - cambiar estado", ex)
            Mostrar("No se pudo completar la operación. Verifique la conexión y vuelva a intentarlo.", False)
        End Try
    End Sub

    Private Sub btnEliminar_Click(sender As Object, e As EventArgs) Handles btnEliminar.Click
        UtileriasUI.EjecutarUnaVez(btnEliminar, AddressOf EliminarUsuario, Function() _idSeleccionado > 0 AndAlso Sesion.UsuarioActual IsNot Nothing AndAlso _idSeleccionado <> Sesion.UsuarioActual.ID AndAlso EsAdministradorActual())
    End Sub

    Private Sub EliminarUsuario()
        If Not Permisos.ExigirAccion("config.usuarios") Then Return
        Try
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

            ' La confirmación visual no sustituye la reautenticación.
            ' Se solicita la clave actual del administrador antes de una
            ' operación física e irreversible.
            Dim claveAdministrador As String = SolicitarClaveAdministrador()
            If claveAdministrador Is Nothing Then Return

            Dim respuesta As Respuesta =
                _servicio.EliminarDefinitivamente(_idSeleccionado, claveAdministrador)
            claveAdministrador = Nothing
            Mostrar(respuesta.Mensaje, respuesta.Exito)
            If respuesta.Exito Then
                CargarUsuarios()
                Nuevo()
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - btneliminar", ex)
            Mostrar("No se pudo completar la operación. Verifique la conexión y vuelva a intentarlo.", False)
        End Try
    End Sub

    ''' <summary>
    ''' Solicita la clave en un cuadro modal con entrada enmascarada.
    ''' Devuelve Nothing si el usuario cancela.
    ''' </summary>
    Private Function SolicitarClaveAdministrador() As String
        Using dialogo As New FrmCapturaClave(
            False,
            "Confirmar identidad del administrador",
            "Ingrese su clave actual para autorizar la eliminación:")
            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return Nothing
            Return dialogo.ClaveCapturada
        End Using
    End Function

    Private Function EsAdministradorActual() As Boolean
        Return Sesion.UsuarioActual IsNot Nothing AndAlso
               Sesion.UsuarioActual.RolProtegido AndAlso Permisos.AccesoTotal
    End Function

    Private Sub Mostrar(mensaje As String, correcto As Boolean)
        lblMensaje.Text = mensaje
        lblMensaje.ForeColor = If(correcto, Color.FromArgb(0, 100, 0), Color.Firebrick)
    End Sub

End Class
