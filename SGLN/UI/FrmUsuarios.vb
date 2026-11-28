Imports System
Imports System.Collections.Generic
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
    Private _tablaUsuarios As DataTable

    Private Sub FrmUsuarios_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("config.usuarios", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvUsuarios)
        txtBuscar.Clear()

        ' Las rutinas de carga registran y muestran sus propios errores, pero
        ' la carga posterior o Nuevo() puede limpiar el mensaje. Conservar
        ' el estado final de ambos orígenes para informar el fallo al usuario.
        CargarRoles()
        Dim errorRoles As Boolean = cboRol.DataSource Is Nothing

        CargarUsuarios()
        Dim errorUsuarios As Boolean = dgvUsuarios.DataSource Is Nothing

        Nuevo()

        If errorRoles AndAlso errorUsuarios Then
            Mostrar("No se pudieron cargar los roles ni los usuarios. Verifique la conexión con la base de datos.", False)
        ElseIf errorRoles Then
            Mostrar("No se pudieron cargar los roles. No será posible asignar un rol hasta recuperar la conexión.", False)
        ElseIf errorUsuarios Then
            Mostrar("No se pudieron cargar los usuarios. Verifique la conexión con la base de datos.", False)
        End If

        _permisosInicializados = True
    End Sub

    Private Sub FrmUsuarios_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("config.usuarios") Then
            Close()
            Return
        End If

        ' La sesión puede conservar el permiso de gestión y perder el acceso
        ' total; sincronizar las acciones sensibles con la matriz vigente.
        ActualizarEstadoAccionesUsuario()
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
            _tablaUsuarios = _servicio.Listar()
            dgvUsuarios.DataSource = New DataView(_tablaUsuarios)
            If txtBuscar.TextLength > 0 Then txtBuscar_TextChanged(txtBuscar, EventArgs.Empty)

            If dgvUsuarios.Columns.Contains("ID") Then dgvUsuarios.Columns("ID").Visible = False
            If dgvUsuarios.Columns.Contains("ID_Rol") Then dgvUsuarios.Columns("ID_Rol").Visible = False

            Dim encabezados As New Dictionary(Of String, String) From {
                {"Usuario", "Usuario"},
                {"Nombre_Completo", "Nombre completo"},
                {"Rol", "Rol asignado"},
                {"Activo", "Estado"},
                {"Fecha_Creacion", "Creado el"}
            }

            For Each par As KeyValuePair(Of String, String) In encabezados
                If dgvUsuarios.Columns.Contains(par.Key) Then
                    dgvUsuarios.Columns(par.Key).HeaderText = par.Value
                    dgvUsuarios.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
                End If
            Next

            If dgvUsuarios.Columns.Contains("Usuario") Then dgvUsuarios.Columns("Usuario").FillWeight = 75
            If dgvUsuarios.Columns.Contains("Nombre_Completo") Then dgvUsuarios.Columns("Nombre_Completo").FillWeight = 125
            If dgvUsuarios.Columns.Contains("Rol") Then dgvUsuarios.Columns("Rol").FillWeight = 75
            If dgvUsuarios.Columns.Contains("Activo") Then
                dgvUsuarios.Columns("Activo").FillWeight = 55
                dgvUsuarios.Columns("Activo").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            End If
            If dgvUsuarios.Columns.Contains("Fecha_Creacion") Then
                With dgvUsuarios.Columns("Fecha_Creacion")
                    .FillWeight = 75
                    .DefaultCellStyle.Format = "dd/MM/yyyy"
                    .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                End With
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - listar", ex)
            Mostrar("No se pudieron cargar los usuarios. Verifique la conexión con la base de datos.", False)
            errorCarga = True
            _tablaUsuarios = Nothing
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

    ''' <summary>
    ''' Filtra localmente el conjunto ya autorizado y cargado; no ejecuta
    ''' consultas por cada tecla ni altera los datos almacenados.
    ''' </summary>
    Private Sub txtBuscar_TextChanged(sender As Object, e As EventArgs) Handles txtBuscar.TextChanged
        If _tablaUsuarios Is Nothing Then Return

        Try
            Dim termino As String = EscaparFiltro(txtBuscar.Text.Trim())
            Dim vista As DataView = TryCast(dgvUsuarios.DataSource, DataView)
            If vista Is Nothing Then Return

            If termino.Length = 0 Then
                vista.RowFilter = String.Empty
            Else
                vista.RowFilter =
                    "Usuario LIKE '%" & termino & "%' OR " &
                    "Nombre_Completo LIKE '%" & termino & "%' OR " &
                    "Rol LIKE '%" & termino & "%'"
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Usuarios - filtro de búsqueda", ex)
            Mostrar("No se pudo aplicar el filtro de búsqueda. Revise el texto ingresado.", False)
        End Try
    End Sub

    Private Function EscaparFiltro(valor As String) As String
        Dim resultado As New System.Text.StringBuilder()
        For Each caracter As Char In If(valor, String.Empty)
            Select Case caracter
                Case "["c
                    resultado.Append("[[]")
                Case "]"c
                    resultado.Append("[]]")
                Case "%"c
                    resultado.Append("[%]")
                Case "*"c
                    resultado.Append("[*]")
                Case "'"c
                    resultado.Append("''")
                Case Else
                    resultado.Append(caracter)
            End Select
        Next
        Return resultado.ToString()
    End Function

    Private Sub dgvUsuarios_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvUsuarios.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse Not dgvUsuarios.Columns.Contains("Activo") Then Return

        Dim fila As DataGridViewRow = dgvUsuarios.Rows(e.RowIndex)
        Dim valorEstado As Object = fila.Cells("Activo").Value
        If valorEstado Is Nothing OrElse valorEstado Is DBNull.Value Then Return

        Dim activo As Boolean
        Try
            activo = Convert.ToBoolean(valorEstado)
        Catch
            Return
        End Try

        If String.Equals(dgvUsuarios.Columns(e.ColumnIndex).Name, "Activo", StringComparison.OrdinalIgnoreCase) Then
            ' Mostrar un estado legible sin modificar el valor booleano de origen,
            ' por lo que el ordenamiento sigue trabajando con el dato real.
            e.Value = If(activo, "Activo", "Inactivo")
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            If fila.Selected Then
                e.CellStyle.SelectionBackColor = dgvUsuarios.DefaultCellStyle.SelectionBackColor
                e.CellStyle.SelectionForeColor = dgvUsuarios.DefaultCellStyle.SelectionForeColor
            ElseIf activo Then
                e.CellStyle.ForeColor = Color.DarkGreen
                e.CellStyle.BackColor = Color.Honeydew
            Else
                e.CellStyle.ForeColor = Color.Firebrick
                e.CellStyle.BackColor = Color.MistyRose
            End If
            e.FormattingApplied = True
            Return
        End If

        If activo OrElse fila.Selected Then Return

        ' Diferenciar visualmente las cuentas inactivas sin ocultarlas
        ' ni alterar los colores de selección del tema del grid.
        e.CellStyle.ForeColor = Color.DimGray
        e.CellStyle.BackColor = Color.FromArgb(245, 245, 245)
    End Sub

    Private Sub dgvUsuarios_SelectionChanged(sender As Object, e As EventArgs) Handles dgvUsuarios.SelectionChanged
        If _cargando Then Return

        ' Las claves temporales nunca deben trasladarse de una cuenta a otra.
        ' Una selección nueva puede cambiar el destino de la acción Restablecer.
        txtClaveNueva.Clear()
        txtClaveConfirmar.Clear()

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
            _idSeleccionado <> Sesion.UsuarioActual.ID AndAlso
            Not chkActivo.Checked
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
        ' El repositorio exige una sesión Administrador para restablecer
        ' credenciales de terceros. Reflejar esa misma regla en la interfaz
        ' evita ofrecer una acción que terminaría rechazada por autorización.
        Dim puedeRestablecer As Boolean = esOtroUsuario AndAlso EsAdministradorActual()
        btnRestablecer.Enabled = puedeRestablecer
        ' Las cajas de clave se usan para crear cuentas nuevas o restablecer
        ' cuentas ajenas; no deben quedar editables al seleccionar la propia.
        txtClaveNueva.Enabled = _idSeleccionado = 0 OrElse puedeRestablecer
        txtClaveConfirmar.Enabled = _idSeleccionado = 0 OrElse puedeRestablecer
        ' La eliminación física exige que la cuenta ya esté desactivada.
        ' El procedimiento MySQL repite esta condición dentro de la transacción.
        btnEliminar.Enabled = EsAdministradorActual() AndAlso esOtroUsuario AndAlso Not chkActivo.Checked
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Nuevo()
    End Sub

    Private Sub Nuevo()
        _idSeleccionado = 0
        _cargando = True
        dgvUsuarios.ClearSelection()
        dgvUsuarios.CurrentCell = Nothing
        _cargando = False
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
        ActualizarEstadoAccionesUsuario()
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
                txtClaveNueva.Clear()
                txtClaveConfirmar.Clear()
                Mostrar("Las claves no coinciden. Ingréselas nuevamente.", False)
                txtClaveNueva.Focus()
                Return
            End If

            If Not UtileriasUI.Confirmar("¿Restablecer la clave del usuario '" & txtUsuario.Text & "'?") Then
                txtClaveNueva.Clear()
                txtClaveConfirmar.Clear()
                Return
            End If

            Dim respuesta As Respuesta
            Try
                respuesta = _servicio.RestablecerClave(_idSeleccionado, txtClaveNueva.Text)
            Finally
                ' Las credenciales temporales no deben permanecer en el formulario,
                ' incluso si el repositorio devuelve un error o lanza una excepción.
                txtClaveNueva.Clear()
                txtClaveConfirmar.Clear()
            End Try

            Mostrar(respuesta.Mensaje, respuesta.Exito)
        Catch ex As Exception
            txtClaveNueva.Clear()
            txtClaveConfirmar.Clear()
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

            Dim respuesta As Respuesta
            Try
                respuesta = _servicio.EliminarDefinitivamente(_idSeleccionado, claveAdministrador)
            Finally
                ' Liberar la referencia a la clave incluso si la operación falla.
                ' String es inmutable: esto no garantiza el borrado físico de memoria.
                claveAdministrador = Nothing
            End Try
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
