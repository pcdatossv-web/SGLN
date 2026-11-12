Imports System
Imports System.Windows.Forms

''' <summary>
''' Punto de entrada del sistema. Resuelve la conexión, prepara la base de datos
''' y mantiene el ciclo inicio de sesión -> aplicación -> cierre de sesión.
''' </summary>
Public Module Program

    ' Tras este número de fallos seguidos se muestra el error final y se cierra.
    Private Const IntentosConexion As Integer = 3

    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Primero la conexión: registro de Windows -> App.config -> valores por defecto.
        ' Así el inicializador y todo el sistema usan la misma cadena.
        ConfiguracionConexion.Inicializar()

        If Not PrepararBaseDeDatos() Then Return

        ' Una instalación nueva exige crear una clave única; no hay credencial predeterminada.
        If Not InicializadorBD.HayUsuarios() Then
            Using primerAdmin As New FrmPrimerAdministrador()
                If primerAdmin.ShowDialog() <> DialogResult.OK Then Return
            End Using
        End If

        Do
            Sesion.UsuarioActual = Nothing
            Sesion.CerrarSesionSolicitado = False

            Using login As New FrmLogin()
                If login.ShowDialog() <> DialogResult.OK Then Return
            End Using

            Application.Run(New FrmPrincipal())

            ' Se descartan los permisos del usuario anterior para que el
            ' siguiente inicio de sesión no herede accesos.
            Sesion.Limpiar()

            ' Si el usuario pidió cerrar sesión, se repite el login; si cerró la
            ' ventana principal, se termina la aplicación.
            If Not Sesion.CerrarSesionSolicitado Then Exit Do
        Loop
    End Sub

    ''' <summary>
    ''' Intenta preparar la base; si falla, ofrece abrir la configuración del
    ''' servidor y reintentar sin tener que cerrar y abrir el programa.
    ''' </summary>
    Private Function PrepararBaseDeDatos() As Boolean
        Dim intento As Integer = 0

        Do
            Try
                InicializadorBD.AsegurarBaseDeDatos()
                Return True
            Catch ex As Exception
                intento += 1
                RegistroErrores.Escribir("Inicio", ex)

                If intento >= IntentosConexion Then
                    MostrarErrorBaseDeDatos(ex)
                    Return False
                End If

                If Not PreguntarConfigurarConexion(ex) Then Return False
            End Try
        Loop
    End Function

    Private Function PreguntarConfigurarConexion(ex As Exception) As Boolean
        Dim mensaje As String =
            "No se pudo preparar la base de datos." & Environment.NewLine & Environment.NewLine &
            ex.Message & Environment.NewLine & Environment.NewLine &
            "Servidor configurado: " & ConfiguracionConexion.Actual.DescripcionCorta() &
            Environment.NewLine & Environment.NewLine &
            "¿Desea revisar la configuración de la conexión?"

        If MessageBox.Show(mensaje, "SGLN El Salvador",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return False
        End If

        Using formulario As New FrmConfiguracionConexion()
            Return formulario.ShowDialog() = DialogResult.OK
        End Using
    End Function

    Private Sub MostrarErrorBaseDeDatos(ex As Exception)
        MessageBox.Show(
            "No se pudo preparar la base de datos." & Environment.NewLine & Environment.NewLine &
            ex.Message & Environment.NewLine & Environment.NewLine &
            "Verifique que el servicio MySQL80 esté iniciado, que el puerto 3306 esté abierto " &
            "y que la configuración de conexión (registro de Windows) o App.config " &
            "apunten al servidor correcto.",
            "SGLN El Salvador", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

End Module
