Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms

Public Class FrmPrincipal

    Private Sub FrmPrincipal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblUsuario.Text = "Usuario: " & Sesion.NombreCompleto
        lblRol.Text = "Rol: " & Sesion.RolActual
        lblFecha.Text = Date.Now.ToString("dd/MM/yyyy HH:mm")
        UtileriasUI.ConfigurarGrid(dgvAlertas)
        AplicarPermisos()
        CargarPanel()
        AddHandler txtBuscarAlertas.TextChanged, AddressOf txtBuscarAlertas_TextChanged
        tmrActualizacion.Enabled = True
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
                    Permisos.Puede("documentos.ver") OrElse
                    Permisos.Puede("documentos.importar") OrElse
                    Permisos.Puede("documentos.verificar") OrElse
                    Permisos.Puede("documentos.metadatos")
            Case "mnuCumplimiento"
                autorizado =
                    Permisos.Puede("cumplimiento.ver") OrElse
                    Permisos.Puede("cumplimiento.operaciones") OrElse
                    Permisos.Puede("cumplimiento.reportar_uif") OrElse
                    Permisos.Puede("cumplimiento.arco")
            Case "mnuConfiguracion"
                autorizado =
                    Permisos.Puede("config.ver") OrElse
                    Permisos.Puede("config.feriados") OrElse
                    Permisos.Puede("config.seguridad") OrElse
                    Permisos.Puede("config.respaldos")
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
            Dim alertas As DataTable = servicio.ObtenerAlertas()
            dgvAlertas.DataSource = alertas.DefaultView
            ConfigurarColumnasAlertas()
            AplicarFiltroAlertas()
            lblResumen.Text = servicio.ObtenerResumen()
            lblFecha.Text = "Actualizado: " & Date.Now.ToString("dd/MM/yyyy HH:mm:ss")

            If alertas.ExtendedProperties.ContainsKey("AlertasLimitadas") AndAlso
               Convert.ToBoolean(alertas.ExtendedProperties("AlertasLimitadas")) Then
                lblNota.Text = "Se muestran las primeras 200 alertas prioritarias. Existen más pendientes; revise los módulos correspondientes para consultar el resto." &
                    Environment.NewLine &
                    "Los vencimientos deben ser confirmados por el abogado. La IA/OCR nunca fija plazos ni sustituye la revisión legal."
            Else
                lblNota.Text = "Los vencimientos de plazos los calcula el sistema (días hábiles + feriados) y deben ser confirmados por el abogado. La IA/OCR nunca fijará plazos ni sustituirá la revisión legal."
            End If

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
            lblFecha.Text = "Error de actualización: " & Date.Now.ToString("dd/MM/yyyy HH:mm:ss")
            lblResumen.Text = "No se pudieron cargar las alertas ni el resumen. Verifique la conexión con la base de datos."
            lblNota.Text = "La información del panel no está actualizada. No utilice los datos anteriores para tomar decisiones."
        End Try
    End Sub

    ''' <summary>
    ''' Presentación uniforme del panel. Los identificadores técnicos se
    ''' conservan en el DataTable para navegación, pero no se exponen al usuario.
    ''' </summary>
    Private Sub ConfigurarColumnasAlertas()
        If dgvAlertas.Columns.Count = 0 Then Return

        If dgvAlertas.Columns.Contains("ID_Referencia") Then
            dgvAlertas.Columns("ID_Referencia").Visible = False
        End If

        Dim encabezados As New Dictionary(Of String, String) From {
            {"Tipo", "Categoría"},
            {"Descripcion", "Detalle"},
            {"Fecha_Referencia", "Fecha límite"},
            {"Prioridad", "Prioridad"}
        }

        For Each par As KeyValuePair(Of String, String) In encabezados
            If dgvAlertas.Columns.Contains(par.Key) Then
                dgvAlertas.Columns(par.Key).HeaderText = par.Value
                dgvAlertas.Columns(par.Key).SortMode = DataGridViewColumnSortMode.Automatic
            End If
        Next

        If dgvAlertas.Columns.Contains("Tipo") Then
            With dgvAlertas.Columns("Tipo")
                .MinimumWidth = 105
                .FillWeight = 75
            End With
        End If

        If dgvAlertas.Columns.Contains("Descripcion") Then
            With dgvAlertas.Columns("Descripcion")
                .MinimumWidth = 280
                .FillWeight = 230
                .DefaultCellStyle.WrapMode = DataGridViewTriState.True
            End With
        End If

        If dgvAlertas.Columns.Contains("Fecha_Referencia") Then
            With dgvAlertas.Columns("Fecha_Referencia")
                .MinimumWidth = 105
                .FillWeight = 75
                .DefaultCellStyle.Format = "dd/MM/yyyy"
                .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            End With
        End If

        If dgvAlertas.Columns.Contains("Prioridad") Then
            With dgvAlertas.Columns("Prioridad")
                .MinimumWidth = 85
                .FillWeight = 55
                .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            End With
        End If

        dgvAlertas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        dgvAlertas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 228, 245)
        dgvAlertas.DefaultCellStyle.SelectionForeColor = Color.FromArgb(24, 52, 92)
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

    Private Sub AbrirClienteDesdeAlerta(idRegistro As Integer)
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

    Private Sub AbrirEntidadDesdeAlerta(idRegistro As Integer)
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

    Private Sub AbrirLibroDesdeAlerta(idLibro As Integer)
        Try
            Using formulario As New FrmProtocolo()
                formulario.IDLibroInicial = idLibro
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
            RegistroErrores.Escribir("Abrir libro de protocolo desde alerta", ex)
            UtileriasUI.MostrarError("No se pudo abrir el libro asociado a la alerta.")
        End Try
    End Sub

    Private Sub AbrirOperacionDesdeAlerta(idOperacion As Integer)
        Try
            Using formulario As New FrmCumplimiento()
                formulario.IDOperacionInicial = idOperacion
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
            RegistroErrores.Escribir("Abrir operación UIF desde alerta", ex)
            UtileriasUI.MostrarError("No se pudo abrir la operación asociada a la alerta.")
        End Try
    End Sub

    Private Sub ModuloPendiente(nombre As String)
        UtileriasUI.MostrarInfo("El módulo '" & nombre & "' se encuentra en construcción.")
    End Sub

    ''' <summary>
    ''' Destaca la prioridad y proximidad de los vencimientos sin alterar
    ''' los datos enlazados ni crear recursos GDI durante el repintado.
    ''' </summary>
    Private Sub dgvAlertas_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvAlertas.CellFormatting
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse
           e.Value Is Nothing OrElse e.Value Is DBNull.Value Then Return

        Dim nombreColumna As String = dgvAlertas.Columns(e.ColumnIndex).Name
        If String.Equals(nombreColumna, "Prioridad", StringComparison.OrdinalIgnoreCase) Then
            Select Case Convert.ToString(e.Value).Trim()
                Case "Alta"
                    e.CellStyle.BackColor = Color.MistyRose
                    e.CellStyle.ForeColor = Color.Firebrick
                Case "Media"
                    e.CellStyle.BackColor = Color.LemonChiffon
                    e.CellStyle.ForeColor = Color.DarkGoldenrod
                Case Else
                    Return
            End Select
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor
            e.FormattingApplied = True
            Return
        End If

        If String.Equals(nombreColumna, "Fecha_Referencia", StringComparison.OrdinalIgnoreCase) Then
            Dim fecha As Date
            If Not Date.TryParse(Convert.ToString(e.Value), fecha) Then Return
            If fecha.Date < Date.Today Then
                e.CellStyle.BackColor = Color.MistyRose
                e.CellStyle.ForeColor = Color.Firebrick
            ElseIf fecha.Date <= Date.Today.AddDays(3) Then
                e.CellStyle.BackColor = Color.LemonChiffon
                e.CellStyle.ForeColor = Color.DarkGoldenrod
            Else
                Return
            End If
            e.CellStyle.SelectionBackColor = e.CellStyle.BackColor
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor
            e.FormattingApplied = True
        End If
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
                Dim idLibro As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idLibro)
                End If
                If idLibro > 0 Then
                    AbrirLibroDesdeAlerta(idLibro)
                Else
                    Abrir(Of FrmProtocolo)()
                End If
            Case "UIF vencido", "UIF por vencer"
                Dim idOperacion As Integer = 0
                If Not vista.Row.IsNull("ID_Referencia") Then
                    Integer.TryParse(Convert.ToString(vista.Row("ID_Referencia")), idOperacion)
                End If
                If idOperacion > 0 Then
                    AbrirOperacionDesdeAlerta(idOperacion)
                Else
                    Abrir(Of FrmCumplimiento)()
                End If
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

    Private Sub tmrActualizacion_Tick(sender As Object, e As EventArgs) Handles tmrActualizacion.Tick
        If Not Me.Enabled OrElse Me.IsDisposed Then Return

        If Not Permisos.RevalidarSesion() Then
            tmrActualizacion.Stop()
            Sesion.CerrarSesionSolicitado = True
            Me.Close()
            Return
        End If


        AplicarPermisos()
        CargarPanel()
    End Sub


    Private Sub txtBuscarAlertas_TextChanged(sender As Object, e As EventArgs)
        AplicarFiltroAlertas()
    End Sub

    Private Sub AplicarFiltroAlertas()
        Dim vista As DataView = TryCast(dgvAlertas.DataSource, DataView)
        If vista Is Nothing Then Return

        Dim texto As String = txtBuscarAlertas.Text.Trim()
        If texto.Length = 0 Then
            vista.RowFilter = ""
            Return
        End If

        ' Escapar caracteres especiales de DataView.RowFilter para evitar
        ' errores al buscar nombres o descripciones con apostrofes.
        texto = texto.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
        vista.RowFilter = "Convert(Tipo, 'System.String') LIKE '%" & texto & "%' OR " &
                          "Convert(Descripcion, 'System.String') LIKE '%" & texto & "%' OR " &
                          "Convert(Prioridad, 'System.String') LIKE '%" & texto & "%'"
    End Sub

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        ' La sesión puede revocarse mientras el panel principal permanece abierto.
        ' No actualizar datos sensibles sin confirmar nuevamente cuenta y permisos.
        If Not Permisos.RevalidarSesion() Then
            Sesion.CerrarSesionSolicitado = True
            Me.Close()
            Return
        End If

        AplicarPermisos()
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
