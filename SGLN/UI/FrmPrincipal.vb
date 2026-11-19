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

        Dim autorizado As Boolean
        Select Case item.Name
            Case "mnuDocumentos"
                autorizado =
                    Permisos.TieneAcceso("documentos.ver") OrElse
                    Permisos.TieneAcceso("documentos.importar") OrElse
                    Permisos.TieneAcceso("documentos.verificar") OrElse
                    Permisos.TieneAcceso("documentos.metadatos")
            Case "mnuCumplimiento"
                autorizado =
                    Permisos.TieneAcceso("cumplimiento.ver") OrElse
                    Permisos.TieneAcceso("cumplimiento.operaciones") OrElse
                    Permisos.TieneAcceso("cumplimiento.reportar_uif") OrElse
                    Permisos.TieneAcceso("cumplimiento.arco")
            Case "mnuConfiguracion"
                autorizado =
                    Permisos.TieneAcceso("config.ver") OrElse
                    Permisos.TieneAcceso("config.feriados") OrElse
                    Permisos.TieneAcceso("config.seguridad") OrElse
                    Permisos.TieneAcceso("config.respaldos")
            Case Else
                autorizado =
                    item.Tag Is Nothing OrElse
                    Permisos.Puede(Convert.ToString(item.Tag))
        End Select

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

            ' No conservar alertas ni contadores de una consulta anterior:
            ' podrían interpretarse como información actual aunque la carga falló.
            dgvAlertas.DataSource = Nothing
            lblResumen.Text = "No se pudieron cargar las alertas ni el resumen. Verifique la conexión con la base de datos."
        End Try
    End Sub

    Private Sub Abrir(Of T As {Form, New})()
        Try
            Using formulario As New T()
                formulario.ShowDialog(Me)
            End Using

            ' La cuenta o el rol pudieron desactivarse mientras el módulo
            ' estaba abierto. Si no se puede revalidar, se fuerza el cierre
            ' de sesión en lugar de restaurar permisos desde el rol en memoria.
            If Not Permisos.RevalidarSesion() Then
                Sesion.CerrarSesionSolicitado = True
                Me.Close()
                Return
            End If

            AplicarPermisos()
            CargarPanel()
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir módulo", ex)
            UtileriasUI.MostrarError("No se pudo abrir el módulo. Verifique los permisos y la conexión con la base de datos.")
        End Try
    End Sub

    Private Sub AbrirCasoDesdeAlerta(idCaso As Integer)
        Try
            Using formulario As New FrmCasos()
                formulario.IDCasoInicial = idCaso
                formulario.ShowDialog(Me)
            End Using

            If Not Permisos.RevalidarSesion() Then
                Sesion.CerrarSesionSolicitado = True
                Me.Close()
                Return
            End If

            AplicarPermisos()
            CargarPanel()
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir caso desde alerta", ex)
            UtileriasUI.MostrarError("No se pudo abrir el expediente asociado a la alerta.")
        End Try
    End Sub

    Private Sub AbrirArcoDesdeAlerta(idSolicitud As Integer)
        Try
            Using formulario As New FrmCumplimiento()
                formulario.IDSolicitudInicial = idSolicitud
                formulario.ShowDialog(Me)
            End Using

            If Not Permisos.RevalidarSesion() Then
                Sesion.CerrarSesionSolicitado = True
                Me.Close()
                Return
            End If

            AplicarPermisos()
            CargarPanel()
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir solicitud ARCO desde alerta", ex)
            UtileriasUI.MostrarError("No se pudo abrir la solicitud ARCO asociada a la alerta.")
        End Try
    End Sub

    Private Sub AbrirClientesDesdeAlerta(idRegistro As Integer)
        Try
            Using formulario As New FrmClientes()
                formulario.IDClienteInicial = idRegistro
                formulario.ShowDialog(Me)
            End Using

            If Not Permisos.RevalidarSesion() Then
                Sesion.CerrarSesionSolicitado = True
                Me.Close()
                Return
            End If

            AplicarPermisos()
            CargarPanel()
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir cliente desde alerta", ex)
            UtileriasUI.MostrarError("No se pudo abrir el registro asociado a la alerta.")
        End Try
    End Sub

    Private Sub AbrirEntidadesDesdeAlerta(idRegistro As Integer)
        Try
            Using formulario As New FrmEntidades()
                formulario.IDEntidadInicial = idRegistro
                formulario.ShowDialog(Me)
            End Using

            If Not Permisos.RevalidarSesion() Then
                Sesion.CerrarSesionSolicitado = True
                Me.Close()
                Return
            End If

            AplicarPermisos()
            CargarPanel()
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir entidad desde alerta", ex)
            UtileriasUI.MostrarError("No se pudo abrir el registro asociado a la alerta.")
        End Try
    End Sub

    Private Sub ModuloPendiente(nombre As String)
        UtileriasUI.MostrarInfo("El módulo '" & nombre & "' se encuentra en construcción.")
    End Sub

    ''' <summary>
    ''' Permite abrir el módulo relacionado con una alerta del panel.
    ''' Las alertas de plazos transportan el ID del expediente para abrir
    ''' directamente el registro relacionado; las demás abren su módulo.
    ''' </summary>
    Private Sub dgvAlertas_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvAlertas.CellDoubleClick
        If e.RowIndex < 0 OrElse e.RowIndex >= dgvAlertas.Rows.Count Then Return

        Dim vista As DataRowView =
            TryCast(dgvAlertas.Rows(e.RowIndex).DataBoundItem, DataRowView)
        If vista Is Nothing Then Return

        Dim tipo As String = Convert.ToString(vista.Row("Tipo"))
        Dim descripcion As String = Convert.ToString(vista.Row("Descripcion"))

        Select Case tipo
            Case "Plazo vencido", "Plazo por vencer"
                Dim idCaso As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idCaso)
                End If
                If idCaso > 0 Then
                    AbrirCasoDesdeAlerta(idCaso)
                Else
                    Abrir(Of FrmCasos)()
                End If
            Case "LDA/FT"
                Dim idPerfil As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idPerfil)
                End If
                If descripcion.IndexOf("(entidad)", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    If idPerfil > 0 Then
                        AbrirEntidadDesdeAlerta(idPerfil)
                    Else
                        Abrir(Of FrmEntidades)()
                    End If
                Else
                    If idPerfil > 0 Then
                        AbrirClienteDesdeAlerta(idPerfil)
                    Else
                        Abrir(Of FrmClientes)()
                    End If
                End If
            Case "Protocolo"
                Abrir(Of FrmProtocolo)()
            Case "UIF vencido", "UIF por vencer"
                Abrir(Of FrmCumplimiento)()
            Case "ARCO vencida", "ARCO revisar prórroga", "ARCO por vencer"
                Dim idSolicitud As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idSolicitud)
                End If
                If idSolicitud > 0 Then
                    AbrirArcoDesdeAlerta(idSolicitud)
                Else
                    Abrir(Of FrmCumplimiento)()
                End If
        End Select
    End Sub

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        CargarPanel()
    End Sub

    Private Sub mnuCambiarClave_Click(sender As Object, e As EventArgs) Handles mnuCambiarClave.Click
        Try
            Using formulario As New FrmCambioClave()
                formulario.ShowDialog(Me)
            End Using
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir cambio de clave", ex)
            UtileriasUI.MostrarError("No se pudo abrir la pantalla de cambio de clave.")
        End Try
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

        UtileriasUI.MostrarInfo("El módulo '" & nombre & "' se encuentra en construcción.")
    End Sub

    ''' <summary>
    ''' Permite abrir el módulo relacionado con una alerta del panel.
    ''' Las alertas de plazos transportan el ID del expediente para abrir
    ''' directamente el registro relacionado; las demás abren su módulo.
    ''' </summary>
    Private Sub dgvAlertas_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvAlertas.CellDoubleClick
        If e.RowIndex < 0 OrElse e.RowIndex >= dgvAlertas.Rows.Count Then Return

        Dim vista As DataRowView =
            TryCast(dgvAlertas.Rows(e.RowIndex).DataBoundItem, DataRowView)
        If vista Is Nothing Then Return

        Dim tipo As String = Convert.ToString(vista.Row("Tipo"))
        Dim descripcion As String = Convert.ToString(vista.Row("Descripcion"))

        Select Case tipo
            Case "Plazo vencido", "Plazo por vencer"
                Dim idCaso As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idCaso)
                End If
                If idCaso > 0 Then
                    AbrirCasoDesdeAlerta(idCaso)
                Else
                    Abrir(Of FrmCasos)()
                End If
            Case "LDA/FT"
                Dim idPerfil As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idPerfil)
                End If
                If descripcion.IndexOf("(entidad)", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    If idPerfil > 0 Then
                        AbrirEntidadDesdeAlerta(idPerfil)
                    Else
                        Abrir(Of FrmEntidades)()
                    End If
                Else
                    If idPerfil > 0 Then
                        AbrirClienteDesdeAlerta(idPerfil)
                    Else
                        Abrir(Of FrmClientes)()
                    End If
                End If
            Case "Protocolo"
                Abrir(Of FrmProtocolo)()
            Case "UIF vencido", "UIF por vencer"
                Abrir(Of FrmCumplimiento)()
            Case "ARCO vencida", "ARCO revisar prórroga", "ARCO por vencer"
                Dim idSolicitud As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idSolicitud)
                End If
                If idSolicitud > 0 Then
                    AbrirArcoDesdeAlerta(idSolicitud)
                Else
                    Abrir(Of FrmCumplimiento)()
                End If
        End Select
    End Sub

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        CargarPanel()
    End Sub

    Private Sub mnuCambiarClave_Click(sender As Object, e As EventArgs) Handles mnuCambiarClave.Click
        Try
            Using formulario As New FrmCambioClave()
                formulario.ShowDialog(Me)
            End Using
        Catch ex As Exception
            RegistroErrores.Escribir("Abrir cambio de clave", ex)
            UtileriasUI.MostrarError("No se pudo abrir la pantalla de cambio de clave.")
        End Try
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
