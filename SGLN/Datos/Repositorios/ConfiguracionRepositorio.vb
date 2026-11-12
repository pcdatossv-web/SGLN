Imports System
Imports System.Configuration
Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
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
                AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Configuracion", Nothing, clave & " = " & valor)
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
    Public Sub RespaldarBaseDatos(rutaDestino As String)
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

        Dim modoSslDump As String = ""
        Select Case constructor.SslMode.ToString()
            Case "VerifyCA"
                modoSslDump = "VERIFY_CA"
            Case "VerifyFull"
                ' mysqldump usa VERIFY_IDENTITY para validar CA y nombre DNS/IP.
                modoSslDump = "VERIFY_IDENTITY"
            Case Else
                Throw New InvalidOperationException(
                    "El respaldo requiere TLS con validación de CA y del servidor. Configure VerifyCA o VerifyFull.")
        End Select
        Dim certificadoCa As String = Convert.ToString(constructor("SslCa"))
        If String.IsNullOrWhiteSpace(certificadoCa) OrElse Not File.Exists(certificadoCa) Then
            Throw New InvalidOperationException("No se encontró el certificado CA configurado para validar TLS en el respaldo.")
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
            If usuarioBackup.Contains(Convert.ToChar(13).ToString()) OrElse usuarioBackup.Contains(Convert.ToChar(10).ToString()) OrElse
               claveBackup.Contains(Convert.ToChar(13).ToString()) OrElse claveBackup.Contains(Convert.ToChar(10).ToString()) Then
                Throw New InvalidOperationException("Las credenciales de respaldo contienen caracteres de control no permitidos.")
            End If
            File.WriteAllText(credenciales,
                "[client]" & Environment.NewLine &
                "user=""" & EscaparOpcionMySQL(usuarioBackup) & """" & Environment.NewLine &
                "password=""" & EscaparOpcionMySQL(claveBackup) & """" & Environment.NewLine)

            Dim sslArgumentos As String = "--ssl-mode=" & modoSslDump & " --ssl-ca=""" & certificadoCa & """ "
            Dim argumentos As String =
                "--defaults-extra-file=""" & credenciales & """ " &
                sslArgumentos &
                "--host=""" & constructor.Server & """ --port=" & constructor.Port & " " &
                "--single-transaction --skip-lock-tables --no-tablespaces --triggers " &
                "--default-character-set=utf8mb4 " &
                "--databases """ & constructor.Database & """"

            EjecutarMySqlDump(ejecutable, argumentos, temporalSalida)
            If Not File.Exists(temporalSalida) OrElse New FileInfo(temporalSalida).Length = 0 Then
                Throw New InvalidOperationException("mysqldump no generó un archivo de respaldo válido.")
            End If
            If File.Exists(destinoFinal) Then
                ' Reemplazo atómico: si falla, el respaldo anterior permanece intacto.
                File.Replace(temporalSalida, destinoFinal, Nothing)
            Else
                File.Move(temporalSalida, destinoFinal)
            End If
        Finally
            Try
                If File.Exists(credenciales) Then File.Delete(credenciales)
                If File.Exists(temporalSalida) Then File.Delete(temporalSalida)
            Catch
                ' La limpieza temporal no debe ocultar el error original.
            End Try
        End Try

        Auditar("RESPALDO", "BaseDatos", Nothing, "Respaldo en " & destinoFinal)
    End Sub

    Private Shared Function EscaparOpcionMySQL(valor As String) As String
        Dim barra As String = Convert.ToChar(92).ToString()
        Dim comilla As String = Convert.ToChar(34).ToString()
        Return If(valor, "").Replace(barra, barra & barra).
            Replace(comilla, barra & comilla)
    End Function

    Private Sub EjecutarMySqlDump(ejecutable As String, argumentos As String, rutaDestino As String)
        Dim informacion As New ProcessStartInfo(ejecutable, argumentos) With {
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }

        Using proceso As Process = Process.Start(informacion)
            Using salida As FileStream = File.Create(rutaDestino)
                ' La salida estándar se copia en segundo plano para que el
                ' proceso no se bloquee si el respaldo es grande.
                Dim copia As Task = Task.Run(Sub() proceso.StandardOutput.BaseStream.CopyTo(salida))
                Dim errores As String = proceso.StandardError.ReadToEnd()
                proceso.WaitForExit()
                copia.Wait()

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

        Dim raiz As String = "C:\Program Files\MySQL"
        If Directory.Exists(raiz) Then
            Dim encontrada As String = Directory.GetFiles(raiz, NombreEjecutable, SearchOption.AllDirectories).FirstOrDefault()
            If encontrada IsNot Nothing Then Return encontrada
        End If

        Return ""
    End Function

End Class
