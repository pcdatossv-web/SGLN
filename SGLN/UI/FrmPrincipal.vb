Imports System
Imports System.Windows.Forms

Public Class FrmPrincipal

    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblUsuario.Text = "Usuario: " & Sesion.NombreCompleto
        lblRol.Text = "Rol: " & Sesion.RolActual
        lblFecha.Text = Date.Now.ToString("dd/MM/yyyy HH:mm")
        UtileriasUI.ConfigurarGrid(dgvAlertas)
        AplicarPermisos()
        CargarPanel()
    End Sub

    ''' <summary>
    ''' Oculta las opciones de menú que el rol no puede usar. El Tag de cada
    ''' opción contiene el código de permiso (ver FrmPrincipal.Designer.vb).
    ''' Ocultar no es la única defensa: cada formulario vuelve a validar.
    ''' </summary>
    Private Sub AplicarPermisos()
        For Each item As ToolStripItem In mnuPrincipal.Items
            AplicarPermisosItem(item)
        Next

        ' Los menús generales siempre deben permanecer disponibles.
        mnuArchivo.Visible = True
        mnuAyuda.Visible = True
    End Sub

    Private Function AplicarPermisosItem(item As ToolStripItem) As Boolean
        If TypeOf item Is ToolStripSeparator Then Return False

        Dim autorizado As Boolean =
            item.Tag Is Nothing OrElse
            Permisos.Puede(Convert.ToString(item.Tag))

        Dim desplegable As ToolStripDropDownItem =
            TryCast(item, ToolStripDropDownItem)

        If desplegable IsNot Nothing AndAlso
           desplegable.DropDownItems.Count > 0 Then

            Dim tieneOpcionesDisponibles As Boolean = False

            For Each hijo As ToolStripItem In desplegable.DropDownItems
                If AplicarPermisosItem(hijo) Then
                    tieneOpcionesDisponibles = True
                End If
            Next

            Dim mostrar As Boolean = autorizado AndAlso tieneOpcionesDisponibles
            item.Visible = mostrar
            Return mostrar
        End If

        item.Visible = autorizado
        Return autorizado
    End Function

    Private Sub CargarPanel()
        Try
            Dim servicio As New ServicioAlertas()
            dgvAlertas.DataSource = servicio.ObtenerAlertas()
            lblResumen.Text = servicio.ObtenerResumen()

            If dgvAlertas.Columns.Contains("Fecha_Referencia") Then
                dgvAlertas.Columns("Fecha_Referencia").DefaultCellStyle.Format = "dd/MM/yyyy"
            End If
            If dgvAlertas.Columns.Contains("Tipo") Then
                dgvAlertas.Columns("Tipo").FillWeight = 70
            End If
            If dgvAlertas.Columns.Contains("Prioridad") Then
                dgvAlertas.Columns("Prioridad").FillWeight = 50
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Panel principal", ex)
            lblResumen.Text = "No se pudieron cargar las alertas: " & ex.Message
        End Try
    End Sub

    Private Sub Abrir(Of T As {Form, New})()
        Try
            Using formulario As New T()
                formulario.ShowDialog(Me)
            End Using

            ' El administrador pudo cambiar el rol o los permisos del usuario
            ' actual mientras el formulario estaba abierto.
            Sesion.CargarPermisos()
            AplicarPermisos()
            CargarPanel()
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir módulo", ex)
            UtileriasUI.MostrarError("No se pudo abrir el módulo: " & ex.Message)
        End Try
    End Sub

    Private Sub ModuloPendiente(nombre As String)
        UtileriasUI.MostrarInfo("El módulo '" & nombre & "' se encuentra en construcción.")
    End Sub

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        CargarPanel()
    End Sub

    Private Sub mnuCerrarSesion_Click(sender As Object, e As EventArgs) Handles mnuCerrarSesion.Click
        If UtileriasUI.Confirmar("¿Cerrar la sesión actual?") Then
            Sesion.CerrarSesionSolicitado = True
            Me.Close()
        End If
    End Sub

    Private Sub mnuSalir_Click(sender As Object, e As EventArgs) Handles mnuSalir.Click
        Sesion.CerrarSesionSolicitado = False
        Me.Close()
    End Sub

    Private Sub mnuClientes_Click(sender As Object, e As EventArgs) Handles mnuClientes.Click
        Abrir(Of FrmClientes)()
    End Sub

    Private Sub mnuEntidades_Click(sender As Object, e As EventArgs) Handles mnuEntidades.Click
        Abrir(Of FrmEntidades)()
    End Sub

    Private Sub mnuCasos_Click(sender As Object, e As EventArgs) Handles mnuCasos.Click
        Abrir(Of FrmCasos)()
    End Sub

    Private Sub mnuProtocolo_Click(sender As Object, e As EventArgs) Handles mnuProtocolo.Click
        Abrir(Of FrmProtocolo)()
    End Sub

    Private Sub mnuDocumentos_Click(sender As Object, e As EventArgs) Handles mnuDocumentos.Click
        Abrir(Of FrmDocumentos)()
    End Sub

    Private Sub mnuCumplimiento_Click(sender As Object, e As EventArgs) Handles mnuCumplimiento.Click
        Abrir(Of FrmCumplimiento)()
    End Sub

    Private Sub mnuAuditoria_Click(sender As Object, e As EventArgs) Handles mnuAuditoria.Click
        Abrir(Of FrmAuditoria)()
    End Sub

    Private Sub mnuConfiguracion_Click(sender As Object, e As EventArgs) Handles mnuConfiguracion.Click
        Abrir(Of FrmConfiguracion)()
    End Sub

    Private Sub mnuUsuarios_Click(sender As Object, e As EventArgs) Handles mnuUsuarios.Click
        Abrir(Of FrmUsuarios)()
    End Sub

    Private Sub mnuRoles_Click(sender As Object, e As EventArgs) Handles mnuRoles.Click
        Abrir(Of FrmRoles)()
    End Sub

    Private Sub mnuAcercaDe_Click(sender As Object, e As EventArgs) Handles mnuAcercaDe.Click
        Using formulario As New FrmAcercaDe()
            formulario.ShowDialog(Me)
        End Using
    End Sub

End Class
