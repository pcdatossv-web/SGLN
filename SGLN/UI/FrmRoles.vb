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

    Private Sub FrmRoles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("config.roles", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvRoles)
        CargarCatalogoPermisos()
        CargarRoles()
        Nuevo()
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
            clbPermisos.DisplayMember = "Texto"
            clbPermisos.ValueMember = "Codigo"
            clbPermisos.DataSource = vista
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - catálogo de permisos", ex)
            Mostrar("No se pudo cargar el catálogo de permisos: " & ex.Message, False)
        End Try
    End Sub

    Private Sub CargarRoles()
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
            Mostrar("No se pudieron cargar los roles: " & ex.Message, False)
        Finally
            _cargando = False
        End Try
    End Sub

    Private Sub dgvRoles_SelectionChanged(sender As Object, e As EventArgs) Handles dgvRoles.SelectionChanged
        If _cargando Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvRoles)
        If fila Is Nothing Then Return

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
        If Not Permisos.ExigirAccion("config.roles") Then Return

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
    End Sub

    Private Sub btnEliminar_Click(sender As Object, e As EventArgs) Handles btnEliminar.Click
        If Not Permisos.ExigirAccion("config.roles") Then Return

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
    End Sub

    Private Sub Mostrar(mensaje As String, correcto As Boolean)
        lblMensaje.Text = mensaje
        lblMensaje.ForeColor = If(correcto, Color.FromArgb(0, 100, 0), Color.Firebrick)
    End Sub

End Class
