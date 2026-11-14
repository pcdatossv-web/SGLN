Imports Microsoft.Win32
Imports MySql.Data.MySqlClient
Imports System
Imports System.Configuration
Imports System.Diagnostics
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' Datos de conexión al servidor MySQL. Se usan tanto para leer el registro
''' como para lo que el usuario escribe en el formulario de configuración.
''' </summary>
Public Class DatosConexion

    Public Property Servidor As String = "localhost"
    Public Property Puerto As Integer = 3306
    Public Property BaseDatos As String = "sgln_sv"
    Public Property Usuario As String = "sgln_app"
    Public Property Password As String = ""
    Public Property UsuarioRespaldo As String = "sgln_backup"
    Public Property PasswordRespaldo As String = ""
    Public Property ModoSsl As String = "VerifyCA"
    Public Property CertificadoCa As String = ""
    ' De dónde se leyó: HKLM, HKCU, App.config o Defecto.
    Public Property Origen As String = ""

    ''' <summary>Copia los datos para poder editarlos sin modificar los originales.</summary>
    Public Function Clonar() As DatosConexion
        Return New DatosConexion With {
            .Servidor = Servidor,
            .Puerto = Puerto,
            .BaseDatos = BaseDatos,
            .Usuario = Usuario,
            .Password = Password,
            .UsuarioRespaldo = UsuarioRespaldo,
            .PasswordRespaldo = PasswordRespaldo,
            .ModoSsl = ModoSsl,
            .CertificadoCa = CertificadoCa,
            .Origen = Origen
        }
    End Function

    ''' <summary>
    ''' Arma la cadena de conexión. VerifyCA valida la cadena contra el CA PEM;
    ''' VerifyFull añade la validación del nombre del servidor. Preferred queda
    ''' reservado al ejecutor de pruebas aislado.
    ''' </summary>
    Public Function ConstruirCadena() As String
        Dim constructor As New MySqlConnectionStringBuilder()
        constructor.Server = If(String.IsNullOrWhiteSpace(Servidor), "localhost", Servidor.Trim())
        constructor.Port = CUInt(If(Puerto <= 0, 3306, Puerto))
        constructor.Database = If(String.IsNullOrWhiteSpace(BaseDatos), "sgln_sv", BaseDatos.Trim())
        constructor.UserID = Usuario
        constructor.Password = Password
        constructor.ConnectionTimeout = 15
        constructor.DefaultCommandTimeout = 30
        Dim modo As String = If(String.IsNullOrWhiteSpace(ModoSsl), "VerifyCA", ModoSsl.Trim())
        If Not String.Equals(modo, "VerifyCA", StringComparison.OrdinalIgnoreCase) AndAlso
           Not String.Equals(modo, "VerifyFull", StringComparison.OrdinalIgnoreCase) AndAlso
           Not String.Equals(modo, "Preferred", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("El modo TLS configurado no está permitido.")
        End If
        If String.Equals(modo, "Preferred", StringComparison.OrdinalIgnoreCase) AndAlso
           Not constructor.Database.EndsWith("_pruebas", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Las conexiones normales requieren TLS con validación de CA. Preferred solo se permite en pruebas aisladas.")
        End If
        If String.Equals(modo, "VerifyCA", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(modo, "VerifyFull", StringComparison.OrdinalIgnoreCase) Then
            If String.IsNullOrWhiteSpace(CertificadoCa) OrElse Not System.IO.File.Exists(CertificadoCa.Trim()) Then
                Throw New InvalidOperationException(
                    "Debe seleccionar el certificado CA PEM de MySQL para validar la conexión TLS.")
            End If
        End If
        constructor.SslMode = CType([Enum].Parse(GetType(MySqlSslMode), modo, True), MySqlSslMode)
        If Not String.IsNullOrWhiteSpace(CertificadoCa) Then
            constructor("SslCa") = CertificadoCa.Trim()
        End If
        constructor.CharacterSet = "utf8mb4"
        constructor.Pooling = True
        Return constructor.ConnectionString
    End Function

    Public Function DescripcionCorta() As String
        Return String.Format("{0}:{1}/{2}", Servidor, Puerto, BaseDatos)
    End Function

End Class

''' <summary>
''' Configuración de conexión guardada en el registro de Windows.
''' Orden de lectura: HKCU (solo el usuario de Windows) -> HKLM (todo el equipo)
''' -> App.config -> valores por defecto.
''' El App.config queda solo como respaldo del primer arranque: la primera vez
''' se copia al registro para que el técnico no tenga que editar XML.
''' </summary>
Public Class ConfiguracionConexion

    Private Const RutaClave As String = "SOFTWARE\SGLN\Conexion"
    Private Const ValorServidor As String = "Servidor"
    Private Const ValorPuerto As String = "Puerto"
    Private Const ValorBaseDatos As String = "BaseDatos"
    Private Const ValorUsuario As String = "Usuario"
    Private Const ValorPassword As String = "Password"
    Private Const ValorUsuarioRespaldo As String = "RespaldosUsuario"
    Private Const ValorPasswordRespaldo As String = "RespaldosPassword"
    Private Const ValorModoSsl As String = "ModoSsl"
    Private Const ValorCertificadoCa As String = "CertificadoCa"
    Private Const ValorActualizado As String = "Actualizado"

    ' Entropía fija: evita que el blob cifrado se pueda descifrar en otro equipo.
    Private Shared ReadOnly Entropia As Byte() = Encoding.UTF8.GetBytes("SGLN.Conexion.v1")

    Private Shared _actual As DatosConexion
    Private Shared _inicializado As Boolean

    ''' <summary>Datos vigentes en memoria. Nunca es Nothing después de Inicializar.</summary>
    Public Shared ReadOnly Property Actual As DatosConexion
        Get
            If _actual Is Nothing Then
                _actual = Cargar()
            End If
            Return _actual
        End Get
    End Property

    ''' <summary>Cadena de conexión lista para MySqlConnection.</summary>
    Public Shared Function CadenaActual() As String
        Return Actual.ConstruirCadena()
    End Function

    ''' <summary>
    ''' Se llama al iniciar la aplicación. Si no hay configuración en el registro,
    ''' la crea a partir del App.config o con valores por defecto. Es idempotente.
    ''' </summary>
    Public Shared Sub Inicializar(Optional forzar As Boolean = False)
        If _inicializado AndAlso Not forzar Then Return

        Dim datos As DatosConexion = Cargar()

        ' El arnés de pruebas y cualquier base terminada en "_pruebas" quedan
        ' fuera del registro: así una corrida de pruebas nunca altera la
        ' configuración real del equipo.
        If IgnorarRegistro() OrElse EsBaseDePruebas(datos) Then
            datos.Origen = "App.config (aislado)"
        ElseIf datos.Origen = "App.config" OrElse datos.Origen = "Defecto" Then
            ' Migración automática: la cadena de respaldo pasa al registro para
            ' poder cambiarla desde el formulario sin editar el App.config.
            Try
                datos.Origen = Guardar(datos, True)
            Catch ex As Exception
                RegistroErrores.Escribir("ConfiguracionConexion.Inicializar (guardado automático)", ex)
                datos.Origen = "Sin guardar"
            End Try
        End If

        _actual = datos
        _inicializado = True
    End Sub

    ''' <summary>
    ''' Lee la configuración sin guardar nada: registro primero, luego App.config
    ''' y por último valores por defecto.
    ''' </summary>
    Public Shared Function Cargar() As DatosConexion
        If IgnorarRegistro() Then
            Return CargarDesdeAppConfig()
        End If

        ' HKCU primero: una configuración por usuario prevalece sobre la del equipo.
        Dim datos As DatosConexion = LeerRegistro(RegistryHive.CurrentUser)
        If datos IsNot Nothing Then
            datos.Origen = "HKCU"
            Return datos
        End If

        datos = LeerRegistro(RegistryHive.LocalMachine)
        If datos IsNot Nothing Then
            datos.Origen = "HKLM"
            Return datos
        End If

        Return CargarDesdeAppConfig()
    End Function

    ''' <summary>
    ''' Guarda en HKLM si hay permisos; si no, en HKCU para que el sistema siga
    ''' funcionando sin obligar a ejecutar como administrador.
    ''' Devuelve el ámbito donde quedó guardado.
    ''' </summary>
    Public Shared Function Guardar(datos As DatosConexion, usarEquipo As Boolean) As String
        If IgnorarRegistro() Then
            Throw New InvalidOperationException(
                "La configuración por registro está desactivada en este ejecutable (modo pruebas).")
        End If

        If usarEquipo Then
            If EscribirRegistro(RegistryHive.LocalMachine, datos) Then
                ' Se limpia HKCU para que no quede una configuración vieja que confunda.
                BorrarRegistro(RegistryHive.CurrentUser)
                Return "HKLM"
            End If
        End If

        If EscribirRegistro(RegistryHive.CurrentUser, datos) Then
            Return "HKCU"
        End If

        Throw New InvalidOperationException("No se pudo escribir la configuración en el registro de Windows.")
    End Function

    ''' <summary>
    ''' Abre una conexión de prueba y devuelve un mensaje entendible.
    ''' No usa la conexión compartida de la aplicación para no alterar la sesión actual.
    ''' </summary>
    Public Shared Function Probar(datos As DatosConexion, ByRef mensaje As String) As Boolean
        Try
            Using conexion As New MySqlConnection(datos.ConstruirCadena())
                conexion.Open()

                Dim version As String = ""
                Using comando As New MySqlCommand("SELECT VERSION()", conexion)
                    version = Convert.ToString(comando.ExecuteScalar())
                End Using

                Dim tablas As Integer = ContarTablas(conexion)
                If tablas = 0 Then
                    mensaje = String.Format("Conectó a MySQL {0}, pero la base '{1}' no tiene tablas.", version, datos.BaseDatos)
                    Return False
                End If

                If Not ExisteEsquemaSgln(conexion) Then
                    mensaje = String.Format(
                        "Conectó a MySQL {0}, pero la base '{1}' no tiene el esquema de SGLN." & Environment.NewLine &
                        "Ejecute Scripts\Instalacion_Servidor_MySQL.sql, Esquema_MySQL.sql y Triggers_MySQL.sql.",
                        version, datos.BaseDatos)
                    Return False
                End If

                mensaje = String.Format("Conexión correcta. MySQL {0} en {1}:{2}, base '{3}' ({4} tablas).",
                                        version, datos.Servidor, datos.Puerto, datos.BaseDatos, tablas)
                Return True
            End Using
        Catch ex As Exception
            RegistroErrores.Escribir("ConfiguracionConexion.Probar", ex)
            mensaje = "No se pudo establecer o validar la conexión. Revise el servidor, la base de datos, las credenciales y la configuración TLS."
            Return False
        End Try
    End Function

    ' ---------------- Registro ----------------

    Private Shared Function LeerRegistro(hive As RegistryHive) As DatosConexion
        Dim claves As List(Of RegistryKey) = AbrirClavesLectura(hive)
        Try
            For Each clave As RegistryKey In claves
                Dim datos As DatosConexion = LeerValores(clave, hive)
                If datos IsNot Nothing Then Return datos
            Next
        Finally
            For Each clave As RegistryKey In claves
                clave.Dispose()
            Next
        End Try
        Return Nothing
    End Function

    Private Shared Function LeerValores(clave As RegistryKey, hive As RegistryHive) As DatosConexion
        Dim servidor As String = Convert.ToString(clave.GetValue(ValorServidor, ""))
        If String.IsNullOrWhiteSpace(servidor) Then Return Nothing

        Dim datos As New DatosConexion()
        datos.Servidor = servidor
        datos.Puerto = Convert.ToInt32(clave.GetValue(ValorPuerto, 3306))
        datos.BaseDatos = Convert.ToString(clave.GetValue(ValorBaseDatos, "sgln_sv"))
        datos.Usuario = Convert.ToString(clave.GetValue(ValorUsuario, "sgln_app"))
        datos.Password = Descifrar(Convert.ToString(clave.GetValue(ValorPassword, "")), hive)
        datos.UsuarioRespaldo = Convert.ToString(clave.GetValue(ValorUsuarioRespaldo, "sgln_backup"))
        datos.PasswordRespaldo = Descifrar(Convert.ToString(clave.GetValue(ValorPasswordRespaldo, "")), hive)
        datos.ModoSsl = Convert.ToString(clave.GetValue(ValorModoSsl, "VerifyCA"))
        datos.CertificadoCa = Convert.ToString(clave.GetValue(ValorCertificadoCa, ""))
        Return datos
    End Function

    Private Shared Function EscribirRegistro(hive As RegistryHive, datos As DatosConexion) As Boolean
        Try
            Using claveBase As RegistryKey = AbrirBase(hive, True)
                Using clave As RegistryKey = claveBase.CreateSubKey(RutaClave)
                    If clave Is Nothing Then Return False

                    clave.SetValue(ValorServidor, datos.Servidor.Trim(), RegistryValueKind.String)
                    clave.SetValue(ValorPuerto, datos.Puerto, RegistryValueKind.DWord)
                    clave.SetValue(ValorBaseDatos, datos.BaseDatos.Trim(), RegistryValueKind.String)
                    clave.SetValue(ValorUsuario, datos.Usuario.Trim(), RegistryValueKind.String)
                    clave.SetValue(ValorPassword, Cifrar(datos.Password, hive), RegistryValueKind.String)
                    clave.SetValue(ValorUsuarioRespaldo, datos.UsuarioRespaldo.Trim(), RegistryValueKind.String)
                    clave.SetValue(ValorPasswordRespaldo, Cifrar(datos.PasswordRespaldo, hive), RegistryValueKind.String)
                    clave.SetValue(ValorModoSsl, datos.ModoSsl, RegistryValueKind.String)
                    clave.SetValue(ValorCertificadoCa, datos.CertificadoCa, RegistryValueKind.String)
                    clave.SetValue(ValorActualizado, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), RegistryValueKind.String)
                    Return True
                End Using
            End Using
        Catch ex As UnauthorizedAccessException
            Return False
        Catch ex As System.Security.SecurityException
            Return False
        Catch ex As Exception
            RegistroErrores.Escribir("ConfiguracionConexion.EscribirRegistro", ex)
            Return False
        End Try
    End Function

    Private Shared Sub BorrarRegistro(hive As RegistryHive)
        Try
            Using claveBase As RegistryKey = AbrirBase(hive, True)
                claveBase.DeleteSubKeyTree(RutaClave, False)
            End Using
        Catch
            ' Que no exista la clave no es un error.
        End Try
    End Sub

    Private Shared Function AbrirClavesLectura(hive As RegistryHive) As List(Of RegistryKey)
        Dim claves As New List(Of RegistryKey)()
        For Each vista As RegistryView In VistasPosibles()
            Try
                Dim claveBase As RegistryKey = RegistryKey.OpenBaseKey(hive, vista)
                Dim clave As RegistryKey = claveBase.OpenSubKey(RutaClave, False)
                claveBase.Dispose()
                If clave IsNot Nothing Then claves.Add(clave)
            Catch
                ' Una vista puede no existir en equipos de 32 bits.
            End Try
        Next
        Return claves
    End Function

    Private Shared Function AbrirBase(hive As RegistryHive, escritura As Boolean) As RegistryKey
        If escritura Then
            ' Para escribir se usa la vista que corresponde al sistema operativo.
            Return RegistryKey.OpenBaseKey(hive, If(Environment.Is64BitOperatingSystem, RegistryView.Registry64, RegistryView.Registry32))
        End If
        Return RegistryKey.OpenBaseKey(hive, RegistryView.Default)
    End Function

    Private Shared Function VistasPosibles() As RegistryView()
        If Environment.Is64BitOperatingSystem Then
            Return New RegistryView() {RegistryView.Registry64, RegistryView.Registry32}
        End If
        Return New RegistryView() {RegistryView.Registry32}
    End Function

    ' ---------------- Aislamiento de pruebas ----------------

    ''' <summary>
    ''' Devuelve True cuando este ejecutable no debe usar ni escribir el registro:
    ''' el arnés compilado como PruebaFuncional.exe y la bandera explícita del
    ''' App.config. Sin esto, una corrida de pruebas podría apuntar a sgln_sv.
    ''' </summary>
    Private Shared Function IgnorarRegistro() As Boolean
        Try
            Dim bandera As String = ConfigurationManager.AppSettings("Conexion_IgnorarRegistro")
            If String.Equals(bandera, "True", StringComparison.OrdinalIgnoreCase) Then Return True
        Catch
            ' Si no hay appSettings se sigue con la detección por nombre.
        End Try

        Try
            Return String.Equals(Process.GetCurrentProcess().ProcessName, "PruebaFuncional",
                                 StringComparison.OrdinalIgnoreCase)
        Catch
            Return False
        End Try
    End Function

    ''' <summary>Una base de pruebas nunca se guarda en el registro del equipo.</summary>
    Private Shared Function EsBaseDePruebas(datos As DatosConexion) As Boolean
        Return datos IsNot Nothing AndAlso
               datos.BaseDatos.Trim().EndsWith("_pruebas", StringComparison.OrdinalIgnoreCase)
    End Function

    ' ---------------- App.config ----------------

    Private Shared Function CargarDesdeAppConfig() As DatosConexion
        Dim datos As DatosConexion = LeerAppConfig()
        If datos IsNot Nothing Then
            datos.Origen = "App.config"
            Return datos
        End If

        Dim porDefecto As New DatosConexion()
        porDefecto.Origen = "Defecto"
        Return porDefecto
    End Function

    Private Shared Function LeerAppConfig() As DatosConexion
        Try
            Dim configurada As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SGLN")
            If configurada Is Nothing OrElse String.IsNullOrWhiteSpace(configurada.ConnectionString) Then Return Nothing

            Dim constructor As New MySqlConnectionStringBuilder(configurada.ConnectionString)
            Dim datos As New DatosConexion()
            datos.Servidor = If(String.IsNullOrWhiteSpace(constructor.Server), "localhost", constructor.Server)
            datos.Puerto = If(constructor.Port = 0UI, 3306, CInt(constructor.Port))
            datos.BaseDatos = If(String.IsNullOrWhiteSpace(constructor.Database), "sgln_sv", constructor.Database)
            datos.Usuario = constructor.UserID
            datos.Password = constructor.Password
            datos.UsuarioRespaldo = If(String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings("Respaldos_Usuario")),
                                       "sgln_backup", ConfigurationManager.AppSettings("Respaldos_Usuario"))
            datos.PasswordRespaldo = If(ConfigurationManager.AppSettings("Respaldos_Clave"), "")
            datos.ModoSsl = constructor.SslMode.ToString()
            datos.CertificadoCa = If(constructor.ContainsKey("SslCa"), Convert.ToString(constructor("SslCa")), "")
            Return datos
        Catch ex As Exception
            RegistroErrores.Escribir("ConfiguracionConexion.LeerAppConfig", ex)
            Return Nothing
        End Try
    End Function

    ' ---------------- Consultas de verificación ----------------

    Private Shared Function ContarTablas(conexion As MySqlConnection) As Integer
        Using comando As New MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE()", conexion)
            Return Convert.ToInt32(comando.ExecuteScalar())
        End Using
    End Function

    Private Shared Function ExisteEsquemaSgln(conexion As MySqlConnection) As Boolean
        ' lower_case_table_names en Windows guarda los nombres en minúsculas.
        Using comando As New MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables " &
                "WHERE table_schema = DATABASE() AND LOWER(table_name) = 'roles'", conexion)
            Return Convert.ToInt32(comando.ExecuteScalar()) > 0
        End Using
    End Function

    ' ---------------- Cifrado de la contraseña ----------------

    ''' <summary>
    ''' Cifra con DPAPI según el ámbito del registro: CurrentUser para HKCU
    ''' y LocalMachine para HKLM. Las claves antiguas se leen con compatibilidad.
    ''' </summary>
    Private Shared Function Cifrar(texto As String, hive As RegistryHive) As String
        If String.IsNullOrEmpty(texto) Then Return ""

        Dim alcance As DataProtectionScope =
            If(hive = RegistryHive.CurrentUser,
               DataProtectionScope.CurrentUser,
               DataProtectionScope.LocalMachine)
        Dim etiqueta As String = If(hive = RegistryHive.CurrentUser, "CU:", "LM:")
        Dim plano As Byte() = Encoding.UTF8.GetBytes(texto)
        Dim cifrado As Byte() = ProtectedData.Protect(plano, Entropia, alcance)
        Return "DPAPI2:" & etiqueta & Convert.ToBase64String(cifrado)
    End Function

    Private Shared Function Descifrar(valor As String, hive As RegistryHive) As String
        If String.IsNullOrEmpty(valor) Then Return ""

        If valor.StartsWith("DPAPI2:CU:", StringComparison.Ordinal) Then
            Return DescifrarProtegido(valor.Substring("DPAPI2:CU:".Length), DataProtectionScope.CurrentUser)
        End If
        If valor.StartsWith("DPAPI2:LM:", StringComparison.Ordinal) Then
            Return DescifrarProtegido(valor.Substring("DPAPI2:LM:".Length), DataProtectionScope.LocalMachine)
        End If

        ' Las versiones anteriores siempre usaban LocalMachine, incluso para HKCU.
        Try
            Return DescifrarProtegido(valor, DataProtectionScope.LocalMachine)
        Catch ex As Exception
            ' Compatibilidad limitada con contraseñas antiguas en texto plano.
            ' Si el valor parece un blob Base64 pero no se puede descifrar, se
            ' considera corrupto/manipulado y no se reutiliza como contraseña.
            If EsBase64(valor) Then
                Throw New CryptographicException(
                    "La credencial cifrada del registro está dañada o pertenece a otro equipo. " &
                    "Vuelva a configurar la conexión y guarde las credenciales nuevamente.", ex)
            End If
            Return valor
        End Try
    End Function

    Private Shared Function DescifrarProtegido(valor As String, alcance As DataProtectionScope) As String
        Dim cifrado As Byte() = Convert.FromBase64String(valor)
        Dim plano As Byte() = ProtectedData.Unprotect(cifrado, Entropia, alcance)
        Return Encoding.UTF8.GetString(plano)
    End Function

    Private Shared Function EsBase64(valor As String) As Boolean
        Try
            Convert.FromBase64String(valor)
            Return True
        Catch
            Return False
        End Try
    End Function

End Class
