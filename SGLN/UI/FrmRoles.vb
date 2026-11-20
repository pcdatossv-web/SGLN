Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Mantenimiento de roles y de su matriz de permisos. Se pueden crear roles
''' nuevos; el rol Administrador no se puede eliminar ni desactivar y debe
''' conservar los permisos de Usuarios y Roles.
''' </summary>
Public Class FrmRoles

    Private ReadOnly _servicio As New ServicioRoles()
    Private _idSeleccionado As Integer
    Private _esProtegido As Boolean
    Private _cargando As Boolean
    Private _catalogoDisponible As Boolean
    Private _permisosInicializados As Boolean

    Private Sub FrmRoles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("config.roles", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvRoles)
        CargarCatalogoPermisos()
        CargarRoles()
        Nuevo()
        _permisosInicializados = True
    End Sub

    Private Sub FrmRoles_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        If Not _permisosInicializados OrElse IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Close()
            Return
        End If

        If Not Permisos.Puede("config.roles") Then Close()
    End Sub

    ''' <summary>
    ''' El CheckedListBox muestra "Módulo — Descripción (código)" y guarda el
    ''' código real en la columna Codigo.
    ''' </summary>
    Private Sub CargarCatalogoPermisos()
        Try
            Dim catalogo As DataTable = _servicio.CatalogoPermisos()
            Dim vista As New DataTable()
            vista.Columns.Add("Codigo", GetType(String))
            vista.Columns.Add("Texto", GetType(String))

            For Each fila As DataRow In catalogo.Rows
                Dim codigo As String = Convert.ToString(fila("Codigo"))
                Dim texto As String =
                    Convert.ToString(fila("Modulo")) & " — " &
                    Convert.ToString(fila("Descripcion")) & " (" & codigo & ")"
                vista.Rows.Add(codigo, texto)
            Next

            clbPermisos.DataSource = Nothing
            clbPermisos.DisplayMember = String.Empty
            clbPermisos.ValueMember = String.Empty

            ' Asignar primero el origen y después los miembros evita que
            ' CheckedListBox muestre System.Data.DataRowView como texto.
            clbPermisos.DataSource = vista
            clbPermisos.DisplayMember = "Texto"
            clbPermisos.ValueMember = "Codigo"
            clbPermisos.CheckOnClick = True
            _catalogoDisponible = True
        Catch ex As Exception
            clbPermisos.DataSource = Nothing
            _catalogoDisponible = False
            btnNuevo.Enabled = False
            btnGuardar.Enabled = False
            RegistroErrores.Escribir("Roles - catálogo de permisos", ex)
            Mostrar("No se pudo cargar el catálogo de permisos. Verifique la conexión con la base de datos.", False)
        End Try
    End Sub

    Private Sub CargarRoles()
        Dim errorCarga As Boolean = False
        Try
            _cargando = True
            dgvRoles.DataSource = _servicio.Listar()

            If dgvRoles.Columns.Contains("ID") Then dgvRoles.Columns("ID").Visible = False
            If dgvRoles.Columns.Contains("Protegido") Then dgvRoles.Columns("Protegido").FillWeight = 45
            If dgvRoles.Columns.Contains("Activo") Then dgvRoles.Columns("Activo").FillWeight = 40
            If dgvRoles.Columns.Contains("Usuarios") Then dgvRoles.Columns("Usuarios").FillWeight = 45
            If dgvRoles.Columns.Contains("Permisos") Then dgvRoles.Columns("Permisos").FillWeight = 45
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - listar", ex)
            Mostrar("No se pudieron cargar los roles. Verifique la conexión con la base de datos.", False)
            errorCarga = True
            dgvRoles.DataSource = Nothing
        Finally
            _cargando = False
            Dim actual As DataRow = UtileriasUI.FilaActual(dgvRoles)
            If actual Is Nothing Then
                Nuevo()
                If errorCarga Then
                    Mostrar("No se pudieron cargar los roles. Verifique la conexión con la base de datos.", False)
                End If
            Else
                dgvRoles_SelectionChanged(dgvRoles, EventArgs.Empty)
            End If
        End Try
    End Sub

    Private Sub dgvRoles_SelectionChanged(sender As Object, e As EventArgs) Handles dgvRoles.SelectionChanged
        If _cargando Then Return
        Try

            Dim fila As DataRow = UtileriasUI.FilaActual(dgvRoles)
            If fila Is Nothing Then
                Nuevo()
                Return
            End If

            _idSeleccionado = UtileriasUI.Entero(fila, "ID")
            _esProtegido = UtileriasUI.Booleano(fila, "Protegido")

            txtNombre.Text = UtileriasUI.Texto(fila, "Nombre")
            txtDescripcion.Text = UtileriasUI.Texto(fila, "Descripcion")
            chkActivo.Checked = UtileriasUI.Booleano(fila, "Activo")

            ' El rol protegido solo admite cambios de descripción y permisos.
            txtNombre.ReadOnly = _esProtegido
            chkActivo.Enabled = Not _esProtegido
            btnEliminar.Enabled = Not _esProtegido

            MarcarPermisos(_servicio.PermisosDeRol(_idSeleccionado))
            Mostrar("", True)
        Catch ex As Exception
            Nuevo()
            RegistroErrores.Escribir("Roles - seleccionar", ex)
            Mostrar("No se pudieron cargar los permisos del rol. Verifique la conexión.", False)
        End Try
    End Sub

    Private Sub MarcarPermisos(codigos As List(Of String))
        Dim activos As New HashSet(Of String)(codigos, StringComparer.OrdinalIgnoreCase)

        For indice As Integer = 0 To clbPermisos.Items.Count - 1
            Dim vista As DataRowView = TryCast(clbPermisos.Items(indice), DataRowView)
            If vista Is Nothing Then Continue For

            Dim codigo As String = Convert.ToString(vista("Codigo"))
            clbPermisos.SetItemChecked(indice, activos.Contains(codigo))
        Next
    End Sub

    Private Function PermisosMarcados() As List(Of String)
        Dim codigos As New List(Of String)()

        For Each elemento As Object In clbPermisos.CheckedItems
            Dim vista As DataRowView = TryCast(elemento, DataRowView)
            If vista IsNot Nothing Then codigos.Add(Convert.ToString(vista("Codigo")))
        Next

        Return codigos
    End Function

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        Nuevo()
    End Sub

    Private Sub Nuevo()
        _idSeleccionado = 0
        _esProtegido = False
        txtNombre.ReadOnly = False
        txtNombre.Clear()
        txtDescripcion.Clear()
        chkActivo.Checked = True
        chkActivo.Enabled = True
        btnEliminar.Enabled = False

        For indice As Integer = 0 To clbPermisos.Items.Count - 1
            clbPermisos.SetItemChecked(indice, False)
        Next

        Mostrar("", True)
        txtNombre.Focus()
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        UtileriasUI.EjecutarUnaVez(btnGuardar, AddressOf GuardarRol)
    End Sub

    Private Sub GuardarRol()
        If Not _catalogoDisponible Then
            Mostrar("No se puede guardar: el catálogo de permisos no está disponible. Actualice el formulario e inténtelo de nuevo.", False)
            Return
        End If
        If Not Permisos.ExigirAccion("config.roles") Then Return
        Try

            Dim respuesta As Respuesta
            Dim codigosRol As List(Of String) = PermisosMarcados()

            If _idSeleccionado = 0 Then
                respuesta = _servicio.Crear(txtNombre.Text, txtDescripcion.Text, codigosRol)
            Else
                respuesta = _servicio.Actualizar(
                    _idSeleccionado, txtNombre.Text, txtDescripcion.Text, chkActivo.Checked, codigosRol)
            End If

            Mostrar(respuesta.Mensaje, respuesta.Exito)

            If respuesta.Exito Then
                CargarRoles()
                Nuevo()
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - btnguardar", ex)
            Mostrar("No se pudo completar la operación. Verifique la conexión y vuelva a intentarlo.", False)
        End Try
    End Sub

    Private Sub btnEliminar_Click(sender As Object, e As EventArgs) Handles btnEliminar.Click
        UtileriasUI.EjecutarUnaVez(btnEliminar, AddressOf EliminarRol, Function() _idSeleccionado > 0 AndAlso Not _esProtegido)
    End Sub

    Private Sub EliminarRol()
        If Not Permisos.ExigirAccion("config.roles") Then Return
        Try

            If _idSeleccionado = 0 Then
                Mostrar("Seleccione el rol que desea eliminar.", False)
                Return
            End If

            If _esProtegido Then
                Mostrar("El rol Administrador es protegido y no se puede eliminar.", False)
                Return
            End If

            If Not UtileriasUI.Confirmar("¿Eliminar el rol '" & txtNombre.Text & "'? Esta acción no se puede deshacer.") Then Return

            Dim respuesta As Respuesta = _servicio.Eliminar(_idSeleccionado)
            Mostrar(respuesta.Mensaje, respuesta.Exito)

            If respuesta.Exito Then
                CargarRoles()
                Nuevo()
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - btneliminar", ex)
            Mostrar("No se pudo completar la operación. Verifique la conexión y vuelva a intentarlo.", False)
        End Try
    End Sub

    Private Sub Mostrar(mensaje As String, correcto As Boolean)
        lblMensaje.Text = mensaje
        lblMensaje.ForeColor = If(correcto, Color.FromArgb(0, 100, 0), Color.Firebrick)
    End Sub

End Class
