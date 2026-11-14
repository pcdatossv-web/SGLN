''' <summary>
''' Formulario para configurar la conexión al servidor MySQL.
''' Guarda los parámetros compartidos sin secretos en HKLM y las credenciales
''' cifradas con DPAPI CurrentUser en HKCU. Si no hay permisos de equipo,
''' guarda toda la configuración en el perfil Windows actual.
''' </summary>
Public Class FrmConfiguracionConexion

    Private Sub FrmConfiguracionConexion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim datos As DatosConexion = ConfiguracionConexion.Cargar()

        txtServidor.Text = datos.Servidor
        txtPuerto.Text = datos.Puerto.ToString()
        txtBaseDatos.Text = datos.BaseDatos
        txtUsuario.Text = datos.Usuario
        txtPassword.Text = datos.Password
        txtUsuarioRespaldo.Text = datos.UsuarioRespaldo
        txtPasswordRespaldo.Text = datos.PasswordRespaldo
        txtCertificadoCa.Text = datos.CertificadoCa

        ' Compatibilidad con instalaciones anteriores: TLS con validación de CA.
        Dim modoActual As String = If(String.IsNullOrWhiteSpace(datos.ModoSsl), "VerifyCA", datos.ModoSsl.Trim())
        If cmbModoSsl.Items.Contains(modoActual) Then
            cmbModoSsl.SelectedItem = modoActual
        Else
            cmbModoSsl.SelectedItem = "VerifyCA"
        End If

        ' Si solo existe configuración compartida, se conserva esa elección.
        chkEquipo.Checked = (datos.Origen = "HKLM")

        lblRuta.Text = "Origen actual: " & DescripcionOrigen(datos.Origen)
        lblEstado.ForeColor = System.Drawing.Color.DimGray
        lblEstado.Text = "Presione ""Probar conexión"" para verificar los datos."
    End Sub

    Private Function DescripcionOrigen(origen As String) As String
        Select Case origen
            Case "HKLM"
                Return "parámetros compartidos del equipo; faltan las credenciales de este usuario Windows"
            Case "HKCU"
                Return "solo el usuario de Windows actual (HKCU\SOFTWARE\SGLN\Conexion)"
            Case "App.config"
                Return "App.config; se copiará al registro al guardar"
            Case Else
                Return "valores por defecto; se crearán en el registro al guardar"
        End Select
    End Function

    ''' <summary>Valida los campos y devuelve los datos listos para probar o guardar.</summary>
    Private Function LeerFormulario() As DatosConexion
        If String.IsNullOrWhiteSpace(txtServidor.Text) Then
            MessageBox.Show("Ingrese el nombre o la IP del servidor.", "Validación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtServidor.Focus()
            Return Nothing
        End If

        Dim puerto As Integer
        If Not Integer.TryParse(txtPuerto.Text.Trim(), puerto) OrElse puerto < 1 OrElse puerto > 65535 Then
            MessageBox.Show("El puerto debe ser un número entre 1 y 65535.", "Validación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPuerto.Focus()
            Return Nothing
        End If

        If String.IsNullOrWhiteSpace(txtBaseDatos.Text) Then
            MessageBox.Show("Ingrese el nombre de la base de datos.", "Validación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtBaseDatos.Focus()
            Return Nothing
        End If

        If String.IsNullOrWhiteSpace(txtUsuario.Text) Then
            MessageBox.Show("Ingrese el usuario de MySQL.", "Validación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtUsuario.Focus()
            Return Nothing
        End If

        If String.IsNullOrWhiteSpace(txtCertificadoCa.Text) OrElse
           Not System.IO.File.Exists(txtCertificadoCa.Text.Trim()) Then
            MessageBox.Show("Seleccione el certificado CA PEM válido que firmó el certificado TLS del servidor MySQL.",
                            "Validación TLS", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtCertificadoCa.Focus()
            Return Nothing
        End If

        Return New DatosConexion With {
            .Servidor = txtServidor.Text.Trim(),
            .Puerto = puerto,
            .BaseDatos = txtBaseDatos.Text.Trim(),
            .Usuario = txtUsuario.Text.Trim(),
            .Password = txtPassword.Text,
            .UsuarioRespaldo = txtUsuarioRespaldo.Text.Trim(),
            .PasswordRespaldo = txtPasswordRespaldo.Text,
            .ModoSsl = If(cmbModoSsl.SelectedItem Is Nothing, "VerifyCA", Convert.ToString(cmbModoSsl.SelectedItem)),
            .CertificadoCa = txtCertificadoCa.Text.Trim()
        }
    End Function

    Private Sub btnBuscarCertificado_Click(sender As Object, e As EventArgs) Handles btnBuscarCertificado.Click
        Using dialogo As New OpenFileDialog()
            dialogo.Title = "Seleccionar certificado CA de MySQL"
            dialogo.Filter = "Certificados PEM (*.pem)|*.pem|Todos los archivos (*.*)|*.*"
            dialogo.CheckFileExists = True
            dialogo.Multiselect = False
            If dialogo.ShowDialog(Me) = DialogResult.OK Then
                txtCertificadoCa.Text = dialogo.FileName
            End If
        End Using
    End Sub

    Private Sub btnProbar_Click(sender As Object, e As EventArgs) Handles btnProbar.Click
        Dim datos As DatosConexion = LeerFormulario()
        If datos Is Nothing Then Return

        Dim mensaje As String = ""
        Dim correcta As Boolean = False

        Try
            Cursor = Cursors.WaitCursor
            btnProbar.Enabled = False
            correcta = ConfiguracionConexion.Probar(datos, mensaje)
        Finally
            Cursor = Cursors.Default
            btnProbar.Enabled = True
        End Try

        lblEstado.ForeColor = If(correcta, System.Drawing.Color.ForestGreen, System.Drawing.Color.Firebrick)
        lblEstado.Text = mensaje
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        Dim datos As DatosConexion = LeerFormulario()
        If datos Is Nothing Then Return

        Try
            Cursor = Cursors.WaitCursor
            Dim ambito As String = ConfiguracionConexion.Guardar(datos, chkEquipo.Checked)

            ' Recarga en memoria para que la aplicación use la configuración nueva de inmediato.
            ConfiguracionConexion.Inicializar(True)

            Cursor = Cursors.Default

            Dim mensaje As String
            If ambito = "HKLM+HKCU" Then
                mensaje = "Parámetros del servidor guardados para todos los usuarios. Las contraseñas quedaron cifradas solo para el usuario Windows actual; cada usuario debe guardar sus propias credenciales."
            Else
                mensaje = "Configuración guardada solo para el usuario de Windows actual."
                If chkEquipo.Checked Then
                    mensaje &= Environment.NewLine & Environment.NewLine &
                               "No se pudo guardar la configuración compartida porque la sesión no tiene permisos de administrador."
                End If
            End If

            MessageBox.Show(mensaje & Environment.NewLine & Environment.NewLine &
                            "Servidor: " & datos.DescripcionCorta(),
                            "Configuración guardada", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            Cursor = Cursors.Default
            RegistroErrores.Escribir("FrmConfiguracionConexion.btnGuardar", ex)
            MessageBox.Show("No se pudo guardar la configuración de conexión. Verifique los datos y los permisos del registro de Windows.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
