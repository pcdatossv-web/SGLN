Imports System
Imports System.Configuration
Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Security.AccessControl
Imports System.Security.Principal
Imports System.Text
Imports System.Threading.Tasks
Imports MySql.Data.MySqlClient

''' <summary>
''' Configuración general, feriados y respaldos de la base de datos.
''' </summary>
Public Class ConfiguracionRepositorio
    Inherits RepositorioBase

    Public Function Listar() As DataTable
        Return Consultar("SELECT Clave, Valor FROM Configuracion ORDER BY Clave")
    End Function

    Public Function Obtener(clave As String) As String
        Dim valor As Object = Escalar(Of String)(
            "SELECT Valor FROM Configuracion WHERE Clave = @clave",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@clave", clave)
            End Sub)
        If valor Is Nothing Then Return ""
        Return Convert.ToString(valor)
    End Function

    Public Sub Guardar(clave As String, valor As String)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "INSERT INTO Configuracion (Clave, Valor) VALUES (@clave, @valor) " &
                    "ON DUPLICATE KEY UPDATE Valor = VALUES(Valor)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@clave", clave)
                        Parametro(comando, "@valor", valor)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Configuracion", Nothing, "Clave actualizada: " & clave)
                Return filas
            End Function)
    End Sub

    Public Function Feriados() As DataTable
        Return Consultar("SELECT ID, Fecha, Descripcion FROM Feriados ORDER BY Fecha")
    End Function

    Public Sub InsertarFeriado(fecha As Date, descripcion As String)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim id As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Feriados (Fecha, Descripcion) VALUES (@fecha, @descripcion)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@fecha", fecha.Date)
                        Parametro(comando, "@descripcion", descripcion)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Feriados", id,
                    fecha.ToString("dd/MM/yyyy") & " - " & descripcion)
                Return id
            End Function)
    End Sub

    Public Sub EliminarFeriado(id As Integer)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "DELETE FROM Feriados WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ELIMINAR", "Feriados", id, "Feriado eliminado")
                Return filas
            End Function)
    End Sub

    Public Function Usuarios() As DataTable
        Return Consultar(
            "SELECT u.ID, u.Usuario, u.Nombre_Completo, IFNULL(r.Nombre, 'Sin rol') AS Rol, " &
            "       u.Activo, u.Fecha_Creacion " &
            "FROM Usuarios u LEFT JOIN Roles r ON r.ID = u.ID_Rol ORDER BY u.Usuario")
    End Function

    ''' <summary>
    ''' Ejecuta un respaldo lógico con mysqldump. A diferencia de SQL Server,
    ''' el archivo lo escribe este equipo (el cliente), así que funciona igual
    ''' en local y en red, sin permisos especiales en el servidor.
    ''' </summary>
    Private Shared Sub ValidarMotoresRespaldo()
        Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
            conexion.Open()
            Using comando As New MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables " &
                "WHERE table_schema = DATABASE() AND table_type = 'BASE TABLE' " &
                "AND (ENGINE IS NULL OR ENGINE <> 'InnoDB')", conexion)
                Dim noTransaccionales As Integer = Convert.ToInt32(comando.ExecuteScalar())
                If noTransaccionales > 0 Then
                    Throw New InvalidOperationException(
                        "No se puede garantizar un respaldo consistente: la base contiene tablas que no usan InnoDB. " &
                        "Convierta esas tablas a InnoDB y vuelva a intentar.")
                End If
            End Using
            Using comando As New MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables " &
                "WHERE table_schema = DATABASE() AND table_type = 'BASE TABLE'", conexion)
                If Convert.ToInt32(comando.ExecuteScalar()) = 0 Then
                    Throw New InvalidOperationException("La base de datos no contiene tablas para respaldar.")
                End If
            End Using
        End Using
    End Sub

    Public Sub RespaldarBaseDatos(rutaDestino As String)
        ValidarMotoresRespaldo()
        Dim ejecutable As String = RutaMySqlDump()
        If String.IsNullOrWhiteSpace(ejecutable) Then
            Throw New InvalidOperationException(
                "No se encontró mysqldump.exe. Instale MySQL en este equipo o indique su ruta " &
                "en App.config (clave Respaldos_RutaMySQLDump).")
        End If

        Dim usuarioBackup As String = UsuarioRespaldo()
        Dim claveBackup As String = ClaveRespaldo()
        If String.IsNullOrWhiteSpace(claveBackup) Then
            Throw New InvalidOperationException(
                "No se ha configurado la contraseña de la cuenta de respaldos. " &
                "Abra Configurar servidor de base de datos y registre la clave de la cuenta de respaldos.")
        End If

        Dim constructor As MySqlConnectionStringBuilder = ConexionBD.ConstructorConexion()
        Dim credenciales As String = Path.Combine(
            Path.GetTempPath(), "sgln_dump_" & Guid.NewGuid().ToString("N") & ".cnf")

        Dim modoConexion As String = constructor.SslMode.ToString()
        Dim pruebaAislada As Boolean = String.Equals(
            ConfigurationManager.AppSettings("Conexion_IgnorarRegistro"),
            "True", StringComparison.OrdinalIgnoreCase)
        Dim modoSslDump As String = ""
        Dim requiereCertificadoCa As Boolean = True

        Select Case modoConexion
            Case "VerifyCA"
                modoSslDump = "VERIFY_CA"
            Case "VerifyFull"
                ' mysqldump usa VERIFY_IDENTITY para validar CA y nombre DNS/IP.
                modoSslDump = "VERIFY_IDENTITY"
            Case "Preferred"
                If Not pruebaAislada Then
                    Throw New InvalidOperationException(
                        "El respaldo normal requiere VerifyCA o VerifyFull. Preferred solo se admite en la base aislada de pruebas.")
                End If
                ' La batería funcional usa una base desechable local sin exigir TLS.
                modoSslDump = "PREFERRED"
                requiereCertificadoCa = False
            Case Else
                Throw New InvalidOperationException("El modo TLS configurado no está permitido para respaldos.")
        End Select

        Dim certificadoCa As String = Convert.ToString(constructor("SslCa"))
        If requiereCertificadoCa AndAlso
           (String.IsNullOrWhiteSpace(certificadoCa) OrElse Not File.Exists(certificadoCa)) Then
            Throw New InvalidOperationException("No se encontró el certificado CA configurado para validar TLS en el respaldo.")
        End If

        ' Los valores se insertan como argumentos de proceso, no como SQL.
        ' Validarlos evita que comillas o espacios introduzcan opciones adicionales
        ' en la línea de comandos de mysqldump.
        Dim servidor As String = If(constructor.Server, "").Trim()
        If Uri.CheckHostName(servidor) = UriHostNameType.Unknown Then
            Throw New InvalidOperationException("El servidor MySQL configurado no es un nombre DNS o una dirección IP válida.")
        End If

        Dim baseDatos As String = If(constructor.Database, "").Trim()
        If String.IsNullOrWhiteSpace(baseDatos) OrElse
           baseDatos.Any(Function(caracter As Char)
                             Return Not ((caracter >= "a"c AndAlso caracter <= "z"c) OrElse
                                         (caracter >= "A"c AndAlso caracter <= "Z"c) OrElse
                                         (caracter >= "0"c AndAlso caracter <= "9"c) OrElse
                                         caracter = "_"c OrElse caracter = "$"c)
                         End Function) Then
            Throw New InvalidOperationException("El nombre de la base de datos contiene caracteres no permitidos para el respaldo.")
        End If
        If String.IsNullOrWhiteSpace(rutaDestino) Then Throw New ArgumentException("Indique la ruta del respaldo.")
        Dim destinoFinal As String = Path.GetFullPath(rutaDestino)
        Dim carpetaDestino As String = Path.GetDirectoryName(destinoFinal)
        If String.IsNullOrWhiteSpace(carpetaDestino) Then Throw New ArgumentException("La ruta del respaldo no es válida.")
        Directory.CreateDirectory(carpetaDestino)
        Dim temporalSalida As String = destinoFinal & "." & Guid.NewGuid().ToString("N") & ".tmp"

        Try
            ' La clave no se pasa por argumentos del proceso. Se escribe en un
            ' archivo temporal privado y se elimina al finalizar.
            If usuarioBackup.Any(Function(caracter As Char) Char.IsControl(caracter)) OrElse
               claveBackup.Any(Function(caracter As Char) Char.IsControl(caracter)) Then
                Throw New InvalidOperationException("Las credenciales de respaldo contienen caracteres de control no permitidos.")
            End If
            EscribirCredencialesProtegidas(credenciales, usuarioBackup, claveBackup)

            Dim sslArgumentos As String
            If EsClienteMariaDB(ejecutable) Then
                ' MariaDB usa --ssl-verify-server-cert; no reconoce --ssl-mode de MySQL.
                If modoConexion = "Preferred" AndAlso pruebaAislada Then
                    sslArgumentos = "--disable-ssl "
                Else
                    ' Verifica CA e identidad del servidor; es más estricto que VerifyCA.
                    sslArgumentos = "--ssl --ssl-ca=""" & certificadoCa & """ --ssl-verify-server-cert "
                End If
            Else
                sslArgumentos = "--ssl-mode=" & modoSslDump
                If requiereCertificadoCa Then
                    sslArgumentos &= " --ssl-ca=""" & certificadoCa & """"
                End If
                sslArgumentos &= " "
            End If
            Dim argumentos As String =
                "--defaults-extra-file=""" & credenciales & """ " &
                sslArgumentos &
                "--host=""" & servidor & """ --port=" & constructor.Port & " " &
                "--single-transaction --skip-lock-tables --no-tablespaces --triggers " &
                "--default-character-set=utf8mb4 " &
                "--databases """ & baseDatos & """"

            EjecutarMySqlDump(ejecutable, argumentos, temporalSalida)
            ValidarArchivoRespaldo(temporalSalida, baseDatos)
            If File.Exists(destinoFinal) Then
                ' Reemplazo atómico: si falla, el respaldo anterior permanece intacto.
                File.Replace(temporalSalida, destinoFinal, Nothing)
            Else
                File.Move(temporalSalida, destinoFinal)
            End If
        Finally
            ' Cada archivo se limpia por separado: un fallo al borrar uno no
            ' debe impedir intentar eliminar el otro ni ocultar el error original.
            Try
                If File.Exists(credenciales) Then File.Delete(credenciales)
            Catch exLimpieza As Exception
                RegistroErrores.Escribir("Respaldos - no se pudo eliminar el archivo temporal de credenciales", exLimpieza)
            End Try

            Try
                If File.Exists(temporalSalida) Then File.Delete(temporalSalida)
            Catch exLimpieza As Exception
                RegistroErrores.Escribir("Respaldos - no se pudo eliminar el archivo temporal de salida", exLimpieza)
            End Try
        End Try

        Auditar("RESPALDO", "BaseDatos", Nothing, "Respaldo en " & destinoFinal)
    End Sub

    ''' <summary>
    ''' Comprueba que la salida no sea solo un archivo no vacío: debe tener
    ''' cabecera reconocible y las instrucciones de creación/selección de la
    ''' base solicitada. No sustituye una restauración de prueba.
    ''' </summary>
    Private Shared Sub ValidarArchivoRespaldo(ruta As String, baseDatos As String)
        If Not File.Exists(ruta) OrElse New FileInfo(ruta).Length = 0 Then
            Throw New InvalidOperationException("mysqldump no generó un archivo de respaldo.")
        End If

        Dim buffer(65535) As Char
        Dim leidos As Integer
        Using lector As New StreamReader(ruta, Encoding.UTF8, True)
            leidos = lector.ReadBlock(buffer, 0, buffer.Length)
        End Using

        Dim cabecera As String = New String(buffer, 0, leidos)
        Dim formatoReconocido As Boolean =
            cabecera.IndexOf("-- MySQL dump", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            cabecera.IndexOf("-- MariaDB dump", StringComparison.OrdinalIgnoreCase) >= 0
        Dim creaBase As Boolean =
            cabecera.IndexOf("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) >= 0
        Dim seleccionaBase As Boolean =
            cabecera.IndexOf("USE `" & baseDatos & "`", StringComparison.OrdinalIgnoreCase) >= 0

        If Not formatoReconocido OrElse Not creaBase OrElse Not seleccionaBase Then
            Throw New InvalidOperationException(
                "El archivo generado no contiene la estructura inicial esperada de un respaldo MySQL/MariaDB.")
        End If
    End Sub

    ''' <summary>
    ''' Crea el archivo de opciones de mysqldump con una ACL explícita:
    ''' solo el usuario actual, SYSTEM y Administradores pueden leerlo.
    ''' Si no se puede aplicar la protección, el respaldo se cancela.
    ''' </summary>
    Private Shared Sub EscribirCredencialesProtegidas(ruta As String, usuario As String, clave As String)
        Dim seguridad As FileSecurity = CrearSeguridadArchivoPrivado()

        Using flujo As New FileStream(ruta, FileMode.CreateNew, FileSystemRights.FullControl,
                                      FileShare.None, 4096, FileOptions.None, seguridad)
            Using escritor As New StreamWriter(flujo, New UTF8Encoding(False))
                escritor.WriteLine("[client]")
                escritor.WriteLine("user=""" & EscaparOpcionMySQL(usuario) & """")
                escritor.WriteLine("password=""" & EscaparOpcionMySQL(clave) & """")
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' ACL privada para archivos que contienen datos sensibles: usuario actual,
    ''' SYSTEM y Administradores. No hereda permisos amplios de la carpeta.
    ''' </summary>
    Private Shared Function CrearSeguridadArchivoPrivado() As FileSecurity
        Dim identidad As WindowsIdentity = WindowsIdentity.GetCurrent()
        If identidad Is Nothing OrElse identidad.User Is Nothing Then
            Throw New InvalidOperationException("No se pudo determinar la identidad de Windows para proteger el archivo de respaldo.")
        End If

        Dim seguridad As New FileSecurity()
        seguridad.SetAccessRuleProtection(True, False)
        seguridad.AddAccessRule(New FileSystemAccessRule(
            identidad.User, FileSystemRights.FullControl, AccessControlType.Allow))
        seguridad.AddAccessRule(New FileSystemAccessRule(
            New SecurityIdentifier(WellKnownSidType.LocalSystemSid, Nothing),
            FileSystemRights.FullControl, AccessControlType.Allow))
        seguridad.AddAccessRule(New FileSystemAccessRule(
            New SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, Nothing),
            FileSystemRights.FullControl, AccessControlType.Allow))
        Return seguridad
    End Function

    Private Shared Function EscaparOpcionMySQL(valor As String) As String
        Dim barra As String = Convert.ToChar(92).ToString()
        Dim comilla As String = Convert.ToChar(34).ToString()
        Return If(valor, "").Replace(barra, barra & barra).
            Replace(comilla, barra & comilla)
    End Function

    Private Function EsClienteMariaDB(ejecutable As String) As Boolean
        Dim informacion As New ProcessStartInfo(ejecutable, "--version") With {
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }

        Using proceso As Process = Process.Start(informacion)
            Dim lecturaSalida As Task(Of String) = Task.Run(Function() proceso.StandardOutput.ReadToEnd())
            Dim lecturaErrores As Task(Of String) = Task.Run(Function() proceso.StandardError.ReadToEnd())

            If Not proceso.WaitForExit(10000) Then
                Try
                    proceso.Kill()
                Catch
                End Try
                proceso.WaitForExit()
                Try
                    Task.WaitAll(lecturaSalida, lecturaErrores)
                Catch
                    ' Se conserva el error de identificación que se mostrará al usuario.
                End Try
                Throw New TimeoutException("El cliente de respaldo no respondió al consultar su versión.")
            End If

            Task.WaitAll(lecturaSalida, lecturaErrores)
            Dim version As String = lecturaSalida.Result & " " & lecturaErrores.Result
            Return version.IndexOf("MariaDB", StringComparison.OrdinalIgnoreCase) >= 0
        End Using
    End Function

    Private Sub EjecutarMySqlDump(ejecutable As String, argumentos As String, rutaDestino As String)
        Dim informacion As New ProcessStartInfo(ejecutable, argumentos) With {
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }

        ' El archivo se crea con ACL privada antes de iniciar mysqldump:
        ' si falla la protección, no queda un proceso externo bloqueado.
        Using salida As New FileStream(rutaDestino, FileMode.CreateNew,
                                       FileSystemRights.FullControl, FileShare.None,
                                       81920, FileOptions.None, CrearSeguridadArchivoPrivado())
            Using proceso As Process = Process.Start(informacion)
                ' Ambas tuberías se consumen en paralelo para evitar bloqueos
                ' por buffers llenos. El respaldo admite archivos grandes, pero
                ' no deja el formulario esperando indefinidamente.
                Dim copiaSalida As Task = Task.Run(Sub() proceso.StandardOutput.BaseStream.CopyTo(salida))
                Dim lecturaErrores As Task(Of String) = Task.Run(Function() proceso.StandardError.ReadToEnd())
                Dim termino As Boolean = proceso.WaitForExit(CInt(TimeSpan.FromHours(2).TotalMilliseconds))

                If Not termino Then
                    Try
                        proceso.Kill()
                    Catch
                        ' Puede haber terminado justo al vencer el tiempo.
                    End Try
                    proceso.WaitForExit()
                    Try
                        Task.WaitAll(copiaSalida, lecturaErrores)
                    Catch
                        ' El timeout es el error principal; la limpieza se hace en el Finally externo.
                    End Try
                    Throw New TimeoutException("La generación del respaldo superó el límite de dos horas y se canceló.")
                End If

                Task.WaitAll(copiaSalida, lecturaErrores)
                Dim errores As String = lecturaErrores.Result
                If proceso.ExitCode <> 0 Then
                    Throw New InvalidOperationException(
                        "mysqldump terminó con código " & proceso.ExitCode & ": " & errores.Trim())
                End If
            End Using
        End Using
    End Sub

    Private Function UsuarioRespaldo() As String
        Dim usuario As String = ConfiguracionConexion.Actual.UsuarioRespaldo
        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = ConfigurationManager.AppSettings("Respaldos_Usuario")
        End If
        If String.IsNullOrWhiteSpace(usuario) Then usuario = "sgln_backup"
        Return usuario.Trim()
    End Function

    Private Function ClaveRespaldo() As String
        Dim clave As String = ConfiguracionConexion.Actual.PasswordRespaldo
        If String.IsNullOrEmpty(clave) Then
            clave = If(ConfigurationManager.AppSettings("Respaldos_Clave"), "")
        End If
        Return clave
    End Function

    ''' <summary>
    ''' Busca mysqldump.exe: primero la ruta configurada, luego PATH y por
    ''' último las instalaciones típicas de MySQL y XAMPP.
    ''' </summary>
    Private Function RutaMySqlDump() As String
        Const NombreEjecutable As String = "mysqldump.exe"

        Dim configurada As String = ConfigurationManager.AppSettings("Respaldos_RutaMySQLDump")
        If Not String.IsNullOrWhiteSpace(configurada) AndAlso File.Exists(configurada) Then
            Return configurada
        End If

        Dim rutasPath As String = Environment.GetEnvironmentVariable("PATH")
        If Not String.IsNullOrWhiteSpace(rutasPath) Then
            For Each carpeta As String In rutasPath.Split(";"c)
                Dim candidata As String = Path.Combine(carpeta.Trim(), NombreEjecutable)
                If File.Exists(candidata) Then Return candidata
            Next
        End If

        Dim instalaciones As String() = {
            "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe",
            "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysqldump.exe",
            "C:\Program Files\MariaDB 11.4\bin\mysqldump.exe",
            "C:\xampp\mysql\bin\mysqldump.exe"
        }

        For Each ruta As String In instalaciones
            If File.Exists(ruta) Then Return ruta
        Next

        ' Las instalaciones de MySQL se organizan por versión: revisar solo
        ' el nivel inmediato evita recorrer todo Program Files y fallar por
        ' carpetas protegidas o inaccesibles durante la operación de respaldo.
        Dim raiz As String = "C:\Program Files\MySQL"
        If Directory.Exists(raiz) Then
            Try
                For Each carpetaVersion As String In Directory.EnumerateDirectories(raiz)
                    Dim candidata As String = Path.Combine(carpetaVersion, "bin", NombreEjecutable)
                    If File.Exists(candidata) Then Return candidata
                Next
            Catch ex As UnauthorizedAccessException
                RegistroErrores.Escribir("Respaldos - búsqueda de mysqldump sin acceso a una carpeta", ex)
            Catch ex As IOException
                RegistroErrores.Escribir("Respaldos - error al buscar mysqldump", ex)
            End Try
        End If

        Return ""
    End Function

End Class
