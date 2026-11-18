Imports System
Imports System.Windows.Forms
Imports System.Threading

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
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf ManejarExcepcionInterfaz
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf ManejarExcepcionProceso

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

    Private Sub ManejarExcepcionInterfaz(sender As Object, e As ThreadExceptionEventArgs)
        RegistrarExcepcionGlobal("Excepción no controlada en la interfaz", e.Exception)
        MessageBox.Show(
            "Se produjo un error inesperado. El detalle quedó registrado localmente. " &
            "Por seguridad, SGLN se cerrará para evitar continuar con la interfaz en un estado inconsistente.",
            "SGLN El Salvador", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Application.Exit()
    End Sub

    Private Sub ManejarExcepcionProceso(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
        If ex Is Nothing Then ex = New Exception(Convert.ToString(e.ExceptionObject))
        RegistrarExcepcionGlobal("Excepción no controlada del proceso", ex)
    End Sub

    Private Sub RegistrarExcepcionGlobal(contexto As String, ex As Exception)
        Try
            RegistroErrores.Escribir(contexto, ex)
        Catch
            ' El registro global nunca debe provocar una segunda excepción.
        End Try
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
                    MostrarErrorBaseDeDatos()
                    Return False
                End If

                If Not PreguntarConfigurarConexion() Then Return False
            End Try
        Loop
    End Function

    Private Function PreguntarConfigurarConexion() As Boolean
        Dim mensaje As String =
            "No se pudo preparar la base de datos." & Environment.NewLine & Environment.NewLine &
            "Compruebe que el servidor MySQL esté disponible y que la configuración de conexión " &
            "sea correcta. El detalle técnico quedó registrado localmente." &
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

    Private Sub MostrarErrorBaseDeDatos()
        MessageBox.Show(
            "No se pudo preparar la base de datos después de varios intentos." &
            Environment.NewLine & Environment.NewLine &
            "Verifique que el servicio MySQL esté iniciado, que el servidor y el puerto " &
            "sean accesibles y que la configuración de conexión sea correcta." &
            Environment.NewLine & Environment.NewLine &
            "El detalle técnico quedó registrado localmente para su revisión.",
            "SGLN El Salvador", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

End Module
