Imports System
Imports System.Data
Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmConfiguracion

    Private ReadOnly _servicio As New ServicioConfiguracion()
    Private _cargando As Boolean

    Private Sub FrmConfiguracion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not Permisos.Exigir("config.ver", Me) Then Return

        UtileriasUI.ConfigurarGrid(dgvConfig)
        UtileriasUI.ConfigurarGrid(dgvFeriados)

        AplicarPermisos()

        _cargando = True
        CargarConfiguracion()
        CargarFeriados()
        _cargando = False

        dtpFeriado.Value = Date.Today
        lblCarpetaRespaldos.Text = "Carpeta de respaldos: " & _servicio.CarpetaRespaldos()

        ' El cambio de clave es siempre del usuario que tiene la sesión abierta:
        ' nadie puede cambiar la clave de otro desde aquí (eso es FrmUsuarios).
        If Sesion.UsuarioActual IsNot Nothing Then
            lblUsuarioSeguridad.Text = "Usuario: " & Sesion.NombreUsuario & " - " & Sesion.NombreCompleto
        Else
            lblUsuarioSeguridad.Text = "Usuario: -"
        End If

        MostrarMensaje("", True)
    End Sub

    ''' <summary>
    ''' Deshabilita lo que el rol no puede usar. Ocultar el menú no basta:
    ''' este formulario vuelve a validar antes de cada acción crítica.
    ''' </summary>
    Private Sub AplicarPermisos()
        Dim puedeFeriados As Boolean = Permisos.Puede("config.feriados")
        btnAgregarFeriado.Enabled = puedeFeriados
        btnEliminarFeriado.Enabled = puedeFeriados

        Dim puedeSeguridad As Boolean = Permisos.Puede("config.seguridad")
        grpClave.Enabled = puedeSeguridad

        grpEditor.Enabled = Permisos.Puede("config.editar")
        btnRespaldar.Enabled = Permisos.Puede("config.respaldos")
    End Sub

    ' ------------------------------------------------------------------
    ' Configuración general
    ' ------------------------------------------------------------------

    Private Sub CargarConfiguracion()
        Try
            _cargando = True
            dgvConfig.DataSource = _servicio.Listar()
            _cargando = False
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Configuración - listar", ex)
            MostrarMensaje("No se pudo cargar la configuración: " & ex.Message, False)
        End Try
    End Sub

    Private Sub dgvConfig_SelectionChanged(sender As Object, e As EventArgs) Handles dgvConfig.SelectionChanged
        If _cargando Then Return
        Dim fila As DataRow = UtileriasUI.FilaActual(dgvConfig)
        If fila Is Nothing Then Return

        txtClave.Text = UtileriasUI.Texto(fila, "Clave")
        txtValor.Text = UtileriasUI.Texto(fila, "Valor")
    End Sub

    Private Sub GuardarConfiguracion()
        If Not Permisos.ExigirAccion("config.editar") Then Return

        If String.IsNullOrWhiteSpace(txtClave.Text) Then
            UtileriasUI.MostrarError("Indique la clave de configuración.")
            txtClave.Focus()
            Return
        End If

        Dim respuesta As Respuesta = _servicio.Guardar(txtClave.Text, txtValor.Text)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarConfiguracion()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    Private Sub NuevaClaveConfig()
        txtClave.Clear()
        txtValor.Clear()
        txtClave.Focus()
    End Sub

    ' ------------------------------------------------------------------
    ' Feriados
    ' ------------------------------------------------------------------

    Private Sub CargarFeriados()
        Try
            _cargando = True
            dgvFeriados.DataSource = _servicio.Feriados()
            If dgvFeriados.Columns.Contains("ID") Then dgvFeriados.Columns("ID").Visible = False
            If dgvFeriados.Columns.Contains("Fecha") Then
                dgvFeriados.Columns("Fecha").DefaultCellStyle.Format = "dd/MM/yyyy"
            End If
            _cargando = False
        Catch ex As Exception
            _cargando = False
            RegistroErrores.Escribir("Configuración - feriados", ex)
            MostrarMensaje("No se pudieron cargar los feriados: " & ex.Message, False)
        End Try
    End Sub

    Private Sub AgregarFeriado()
        If Not Permisos.ExigirAccion("config.feriados") Then Return

        Dim respuesta As Respuesta = _servicio.AgregarFeriado(dtpFeriado.Value.Date, txtDescripcionFeriado.Text)
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        txtDescripcionFeriado.Clear()
        CargarFeriados()
        MostrarMensaje(respuesta.Mensaje & " Afecta los cálculos futuros de plazos.", True)
    End Sub

    Private Sub EliminarFeriado()
        If Not Permisos.ExigirAccion("config.feriados") Then Return

        Dim fila As DataRow = UtileriasUI.FilaActual(dgvFeriados)
        If fila Is Nothing Then
            UtileriasUI.MostrarError("Seleccione un feriado de la lista.")
            Return
        End If

        If Not UtileriasUI.Confirmar(
            "¿Eliminar el feriado del " & UtileriasUI.Fecha(fila, "Fecha").Value.ToString("dd/MM/yyyy") & "?") Then Return

        Dim respuesta As Respuesta = _servicio.EliminarFeriado(UtileriasUI.Entero(fila, "ID"))
        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        CargarFeriados()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    ' ------------------------------------------------------------------
    ' Seguridad
    ' ------------------------------------------------------------------

    Private Sub CambiarClave()
        If Not Permisos.ExigirAccion("config.seguridad") Then Return

        If Sesion.UsuarioActual Is Nothing Then
            UtileriasUI.MostrarError("No hay una sesión activa.")
            Return
        End If

        If txtClaveNueva.Text <> txtConfirmarClave.Text Then
            UtileriasUI.MostrarError("La nueva clave y su confirmación no coinciden.")
            Return
        End If
        If String.IsNullOrWhiteSpace(txtClaveActual.Text) Then
            UtileriasUI.MostrarError("Escriba la clave actual del usuario.")
            txtClaveActual.Focus()
            Return
        End If

        Dim respuesta As Respuesta = _servicio.CambiarClave(
            Sesion.UsuarioActual.ID, txtClaveActual.Text, txtClaveNueva.Text)

        If Not respuesta.Exito Then
            MostrarMensaje(respuesta.Mensaje, False)
            Return
        End If

        txtClaveActual.Clear()
        txtClaveNueva.Clear()
        txtConfirmarClave.Clear()
        MostrarMensaje(respuesta.Mensaje, True)
    End Sub

    ' ------------------------------------------------------------------
    ' Respaldos
    ' ------------------------------------------------------------------

    Private Sub Respaldar()
        If Not Permisos.ExigirAccion("config.respaldos") Then Return

        If Not UtileriasUI.Confirmar(
            "¿Crear un respaldo completo de la base de datos en la carpeta configurada?") Then Return

        Cursor = Cursors.WaitCursor
        lblResultadoRespaldo.ForeColor = Color.DimGray
        lblResultadoRespaldo.Text = "Creando respaldo..."
        Application.DoEvents()

        Try
            Dim respuesta As Respuesta = _servicio.Respaldar()
            lblResultadoRespaldo.ForeColor = If(respuesta.Exito, Color.ForestGreen, Color.Firebrick)
            lblResultadoRespaldo.Text = respuesta.Mensaje
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub AbrirCarpetaRespaldos()
        Try
            Dim carpeta As String = _servicio.CarpetaRespaldos()
            If Not System.IO.Directory.Exists(carpeta) Then
                System.IO.Directory.CreateDirectory(carpeta)
            End If
            Process.Start(New ProcessStartInfo(carpeta) With {.UseShellExecute = True})
        Catch ex As Exception
            RegistroErrores.Escribir("Configuración - abrir respaldos", ex)
            MostrarMensaje("No se pudo abrir la carpeta: " & ex.Message, False)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Utilidades y eventos
    ' ------------------------------------------------------------------

    Private Sub MostrarMensaje(texto As String, exito As Boolean)
        lblMensaje.Text = texto
        lblMensaje.ForeColor = If(exito, Color.ForestGreen, Color.Firebrick)
    End Sub

    Private Sub btnGuardarConfig_Click(sender As Object, e As EventArgs) Handles btnGuardarConfig.Click
        GuardarConfiguracion()
    End Sub

    Private Sub btnNuevaClaveConfig_Click(sender As Object, e As EventArgs) Handles btnNuevaClaveConfig.Click
        NuevaClaveConfig()
    End Sub

    Private Sub btnAgregarFeriado_Click(sender As Object, e As EventArgs) Handles btnAgregarFeriado.Click
        AgregarFeriado()
    End Sub

    Private Sub btnEliminarFeriado_Click(sender As Object, e As EventArgs) Handles btnEliminarFeriado.Click
        EliminarFeriado()
    End Sub

    Private Sub btnCambiarClave_Click(sender As Object, e As EventArgs) Handles btnCambiarClave.Click
        CambiarClave()
    End Sub

    Private Sub btnRespaldar_Click(sender As Object, e As EventArgs) Handles btnRespaldar.Click
        Respaldar()
    End Sub

    Private Sub btnAbrirCarpeta_Click(sender As Object, e As EventArgs) Handles btnAbrirCarpeta.Click
        AbrirCarpetaRespaldos()
    End Sub

End Class
