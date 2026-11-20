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

    ''' <summary>
    ''' Revalida la sesión una vez y consulta los permisos desde la matriz vigente.
    ''' </summary>
    Public Function Listar() As DataTable
        If Not Permisos.TieneAcceso("config.ver") Then Return New DataTable()
        Return Consultar("SELECT Clave, Valor FROM Configuracion ORDER BY Clave")
    End Function

    Public Function Obtener(clave As String) As String
        If String.IsNullOrWhiteSpace(clave) OrElse clave.Trim().Length > 100 Then Return ""

        Dim claveNormalizada As String = clave.Trim()
        Dim puedeLeer As Boolean
        If String.Equals(claveNormalizada, "Carpeta_Respaldos", StringComparison.OrdinalIgnoreCase) Then
            puedeLeer = Permisos.TieneAccesoAlguno("config.ver", "config.respaldos")
        Else
            puedeLeer = Permisos.TieneAcceso("config.ver")
        End If
        If Not puedeLeer Then Return ""

        Dim valor As Object = Escalar(Of String)(
            "SELECT Valor FROM Configuracion WHERE Clave = @clave",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@clave", claveNormalizada)
            End Sub)
        If valor Is Nothing Then Return ""
        Return Convert.ToString(valor)
    End Function

    Public Sub Guardar(clave As String, valor As String)
        If Not Permisos.TieneAcceso("config.editar") Then
            Throw New UnauthorizedAccessException("No tiene permiso para modificar la configuración.")
        End If
        If String.IsNullOrWhiteSpace(clave) OrElse clave.Trim().Length > 60 Then
            Throw New ArgumentException("La clave de configuración no es válida.", NameOf(clave))
        End If
        If String.Equals(clave.Trim(), "Carpeta_Documentos", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(clave.Trim(), "Carpeta_Respaldos", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Las rutas de documentos y respaldos no se pueden modificar desde la configuración general.")
        End If
        If If(valor, "").Trim().Length > 500 Then
            Throw New ArgumentException("El valor de configuración no puede superar 500 caracteres.", NameOf(valor))
        End If

        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "INSERT INTO Configuracion (Clave, Valor) VALUES (@clave, @valor) " &
                    "ON DUPLICATE KEY UPDATE Valor = VALUES(Valor)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@clave", clave)
                        Parametro(comando, "@valor", valor)
                    End Sub)
                ' GUARDAR describe con precisión tanto altas como cambios de configuración.
                ' No registrar auditoría si MySQL informa que el valor ya era idéntico.
                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "GUARDAR", "Configuracion",
                        Nothing, "Clave guardada: " & clave.Trim())
                End If
                Return filas
            End Function)
    End Sub

    Public Function Feriados() As DataTable
        If Not Permisos.TieneAccesoAlguno("config.feriados", "config.ver") Then Return New DataTable()
        Return Consultar("SELECT ID, Fecha, Descripcion FROM Feriados ORDER BY Fecha")
    End Function

    Public Sub InsertarFeriado(fecha As Date, descripcion As String)
        If Not Permisos.TieneAcceso("config.feriados") Then
            Throw New UnauthorizedAccessException("No tiene permiso para administrar feriados.")
        End If
        ' El repositorio también protege sus invariantes: no depender únicamente
        ' de que todas las llamadas futuras pasen por ServicioConfiguracion.
        If fecha = Date.MinValue OrElse fecha.Date.Year < 2000 OrElse fecha.Date.Year > 2100 Then
            Throw New ArgumentOutOfRangeException(NameOf(fecha), "El año del feriado debe estar entre 2000 y 2100.")
        End If
        If String.IsNullOrWhiteSpace(descripcion) OrElse descripcion.Trim().Length > 120 Then
            Throw New ArgumentException("La descripción del feriado debe contener entre 1 y 120 caracteres.", NameOf(descripcion))
        End If
        descripcion = descripcion.Trim()

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

    Public Function EliminarFeriado(id As Integer) As Integer
        If Not Permisos.TieneAcceso("config.feriados") Then
            Throw New UnauthorizedAccessException("No tiene permiso para administrar feriados.")
        End If
        If id <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(id), "Seleccione un feriado válido.")

        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "DELETE FROM Feriados WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ELIMINAR", "Feriados", id, "Feriado eliminado")
                Return filas
            End Function)
    End Function

    Public Function Usuarios() As DataTable
        If Not Permisos.TieneAcceso("config.seguridad") Then Return New DataTable()
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
        If Not Permisos.TieneAcceso("config.respaldos") Then
            Throw New UnauthorizedAccessException("No tiene permiso para crear respaldos.")
        End If

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

        ' El repositorio vuelve a imponer el destino autorizado: no basta con
        ' confiar en que el servicio o la interfaz le entreguen una ruta segura.
        Dim carpetaConfigurada As String = Obtener("Carpeta_Respaldos")
        If String.IsNullOrWhiteSpace(carpetaConfigurada) Then carpetaConfigurada = "Respaldos"
        If Not Path.IsPathRooted(carpetaConfigurada) Then
            carpetaConfigurada = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, carpetaConfigurada)
        End If
        carpetaConfigurada = Path.GetFullPath(carpetaConfigurada)
        Dim destinoNormalizado As String = Path.GetFullPath(carpetaDestino).TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Dim carpetaNormalizada As String = carpetaConfigurada.TrimEnd(
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        If Not String.Equals(destinoNormalizado, carpetaNormalizada,
                             StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("El respaldo solo puede escribirse en la carpeta configurada.")
        End If
        If Not String.Equals(Path.GetExtension(destinoFinal), ".sql", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("El archivo de respaldo debe tener extensión .sql.")
        End If
        ValidarCarpetaSinEnlaces(carpetaConfigurada)
        Directory.CreateDirectory(carpetaConfigurada)
        ValidarCarpetaSinEnlaces(carpetaConfigurada)
        If File.Exists(destinoFinal) Then
            Throw New IOException("El archivo de destino ya existe; no se sobrescribirá un respaldo anterior.")
        End If

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
                    sslArgumentos = "--ssl --ssl-ca=" & CitarArgumentoWindows(certificadoCa) & " --ssl-verify-server-cert "
                End If
            Else
                sslArgumentos = "--ssl-mode=" & modoSslDump
                If requiereCertificadoCa Then
                    sslArgumentos &= " --ssl-ca=" & CitarArgumentoWindows(certificadoCa)
                End If
                sslArgumentos &= " "
            End If
            Dim argumentos As String =
                "--defaults-extra-file=" & CitarArgumentoWindows(credenciales) & " " &
                sslArgumentos &
                "--host=" & CitarArgumentoWindows(servidor) & " --port=" & constructor.Port & " " &
                "--single-transaction --skip-lock-tables --no-tablespaces --triggers " &
                "--default-character-set=utf8mb4 " &
                "--databases " & CitarArgumentoWindows(baseDatos)

            EjecutarMySqlDump(ejecutable, argumentos, temporalSalida)
            ValidarArchivoRespaldo(temporalSalida, baseDatos)
            ' Revalidar inmediatamente antes del movimiento final y crear el
            ' destino sin reemplazar archivos existentes (operación no destructiva).
            ValidarCarpetaSinEnlaces(carpetaConfigurada)
            If File.Exists(destinoFinal) Then
                Throw New IOException("El archivo de destino apareció durante el respaldo; no se sobrescribió.")
            End If
            File.Move(temporalSalida, destinoFinal)
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

        Try
            AuditarObligatorio("RESPALDO", "BaseDatos", Nothing, "Respaldo en " & destinoFinal)
        Catch exAuditoria As Exception
            ' El archivo ya fue publicado, pero el respaldo no debe considerarse
            ' exitoso si no queda constancia obligatoria en Audit_Log. Intentar
            ' retirar únicamente el archivo de esta operación; nunca seguir una
            ' ruta que haya sido sustituida por un punto de reanálisis.
            Dim eliminado As Boolean = False
            Try
                ValidarCarpetaSinEnlaces(carpetaConfigurada)
                If File.Exists(destinoFinal) Then
                    Dim atributos As FileAttributes = File.GetAttributes(destinoFinal)
                    If (atributos And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                        Throw New InvalidOperationException(
                            "El archivo de respaldo fue sustituido por un punto de reanálisis; no se eliminará.")
                    End If
                    File.Delete(destinoFinal)
                    eliminado = True
                End If
            Catch exLimpieza As Exception
                RegistroErrores.Escribir(
                    "Respaldos - falló la limpieza tras un error de auditoría; revisar archivo " & destinoFinal,
                    exLimpieza)
            End Try

            If eliminado Then
                Throw New InvalidOperationException(
                    "No se pudo registrar el respaldo en la bitácora. El archivo generado se eliminó para evitar un respaldo sin trazabilidad.",
                    exAuditoria)
            End If

            Throw New InvalidOperationException(
                "No se pudo registrar el respaldo en la bitácora y no fue posible confirmar su eliminación. Revise manualmente el archivo: " &
                destinoFinal,
                exAuditoria)
        End Try
    End Sub

    ''' <summary>
    ''' Rechaza puntos de reanálisis en todos los segmentos de la carpeta
    ''' autorizada, incluso cuando se invoca directamente el repositorio.
    ''' </summary>
    Private Shared Sub ValidarCarpetaSinEnlaces(rutaCompleta As String)
        ' Recorrer todos los segmentos desde la raíz para detectar enlaces
        ' también en directorios padres de la carpeta configurada.
        Dim rutaAbsoluta As String = Path.GetFullPath(rutaCompleta)
        Dim raiz As String = Path.GetPathRoot(rutaAbsoluta)
        If String.IsNullOrWhiteSpace(raiz) Then
            Throw New InvalidOperationException("La ruta de respaldos no tiene una raíz válida.")
        End If

        Dim relativo As String = rutaAbsoluta.Substring(raiz.Length)
        Dim segmentos As String() = relativo.Split(
            New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar},
            StringSplitOptions.RemoveEmptyEntries)
        Dim actual As String = raiz

        For Each segmento As String In segmentos
            actual = Path.Combine(actual, segmento)
            Try
                Dim atributos As FileAttributes = File.GetAttributes(actual)
                If (atributos And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                    Throw New InvalidOperationException(
                        "La ruta de respaldos contiene un enlace simbólico o punto de reanálisis no permitido.")
                End If
            Catch ex As FileNotFoundException
                ' El archivo o directorio final puede no existir todavía.
            Catch ex As DirectoryNotFoundException
                ' Se permiten segmentos que aún no se han creado.
            End Try
        Next
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
        ' El archivo .cnf es un formato de opciones por líneas. Aunque los
        ' valores se entrecomillan y escapan, nunca se admiten controles que
        ' puedan introducir una segunda opción o truncar una credencial.
        If String.IsNullOrWhiteSpace(usuario) OrElse
           usuario.Any(Function(caracter As Char) Char.IsControl(caracter)) Then
            Throw New InvalidOperationException("El usuario de respaldos contiene caracteres no permitidos.")
        End If
        If clave Is Nothing OrElse clave.Any(Function(caracter As Char) Char.IsControl(caracter)) Then
            Throw New InvalidOperationException("La contraseña de respaldos contiene caracteres de control no permitidos.")
        End If

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

    ''' <summary>
    ''' Encierra un argumento para CreateProcess en Windows y duplica las
    ''' barras invertidas necesarias antes de comillas y al final del valor.
    ''' ProcessStartInfo no escapa automáticamente la cadena Arguments.
    ''' </summary>
    Private Shared Function CitarArgumentoWindows(valor As String) As String
        Dim barra As Char = Convert.ToChar(92)
        Dim comilla As Char = Convert.ToChar(34)
        Dim resultado As New StringBuilder()
        resultado.Append(comilla)

        Dim barrasPendientes As Integer = 0
        For Each caracter As Char In If(valor, "")
            If caracter = barra Then
                barrasPendientes += 1
            ElseIf caracter = comilla Then
                If barrasPendientes > 0 Then
                    resultado.Append(New String(barra, barrasPendientes * 2 + 1))
                Else
                    resultado.Append(barra)
                End If
                resultado.Append(comilla)
                barrasPendientes = 0
            Else
                If barrasPendientes > 0 Then
                    resultado.Append(New String(barra, barrasPendientes))
                    barrasPendientes = 0
                End If
                resultado.Append(caracter)
            End If
        Next

        If barrasPendientes > 0 Then
            resultado.Append(New String(barra, barrasPendientes * 2))
        End If
        resultado.Append(comilla)
        Return resultado.ToString()
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

                ' Si el destino se queda sin espacio, se revoca el acceso al
                ' archivo o falla cualquiera de las tuberías, cerrar la tarea
                ' lectora sin detener mysqldump puede llenar el pipe y dejar
                ' WaitForExit bloqueado hasta agotar el timeout de dos horas.
                ' Ante un fallo de lectura se termina el proceso inmediatamente.
                Dim vigilarSalida As Task = copiaSalida.ContinueWith(
                    Sub(tarea As Task)
                        If tarea.IsFaulted OrElse tarea.IsCanceled Then
                            Try
                                proceso.Kill()
                            Catch
                                ' El proceso puede haber terminado entre tanto.
                            End Try
                        End If
                    End Sub, TaskContinuationOptions.NotOnRanToCompletion)
                Dim vigilarErrores As Task = lecturaErrores.ContinueWith(
                    Sub(tarea As Task)
                        If tarea.IsFaulted OrElse tarea.IsCanceled Then
                            Try
                                proceso.Kill()
                            Catch
                                ' El proceso puede haber terminado entre tanto.
                            End Try
                        End If
                    End Sub, TaskContinuationOptions.NotOnRanToCompletion)

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
        ' Las claves nunca se leen desde App.config: se guardan cifradas
        ' por usuario Windows (DPAPI CurrentUser) en HKCU.
        Return ConfiguracionConexion.Actual.PasswordRespaldo
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
