' Arnés de prueba funcional del SGLN. Ejecuta la capa de Servicios real
' contra una base de pruebas aislada (sgln_sv_pruebas) y reporta OK/FALLA.
' No toca la base de trabajo sgln_sv.
Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Threading
Imports MySql.Data.MySqlClient
Imports SGLN

Module PruebaFuncional

    Private _pruebas As Integer
    Private _fallos As Integer

    Sub Main()
        Console.OutputEncoding = Encoding.UTF8
        Console.WriteLine("=====================================================")
        Console.WriteLine(" Prueba funcional SGLN - base de pruebas sgln_sv_pruebas")
        Console.WriteLine("=====================================================")
        Console.WriteLine()

        Dim repositorioClientesDirecto As New ClienteRepositorio()
        Dim repositorioEntidadesDirecto As New EntidadRepositorio()
        Dim repositorioCasosDirecto As New CasoRepositorio()
        Dim repositorioProtocoloDirecto As New ProtocoloRepositorio()
        Dim repositorioDocumentosDirecto As New DocumentoRepositorio()
        Dim repositorioCumplimientoDirecto As New CumplimientoRepositorio()
        Dim servicioProtocoloConcurrente As New ServicioProtocolo()

        ' La calculadora se usa en varios módulos y debe rechazar entradas que
        ' puedan producir fechas inválidas o ciclos desproporcionados.
        Dim calculadoraInicial As New CalculadoraDiasHabiles(Nothing)
        Dim fechaMinimaRechazada As Boolean = False
        Try
            calculadoraInicial.CalcularVencimiento(Date.MinValue, 1)
        Catch ex As ArgumentOutOfRangeException
            fechaMinimaRechazada = True
        End Try
        Verificar("La calculadora rechaza fechas de notificación no inicializadas", fechaMinimaRechazada)

        Dim plazoExcesivoRechazado As Boolean = False
        Try
            calculadoraInicial.CalcularVencimiento(Date.Today, 36601)
        Catch ex As ArgumentOutOfRangeException
            plazoExcesivoRechazado = True
        End Try
        Verificar("La calculadora limita plazos desproporcionados", plazoExcesivoRechazado)

        Dim rangoExcesivoRechazado As Boolean = False
        Try
            calculadoraInicial.ContarDiasHabiles(New Date(1, 1, 1), Date.MaxValue)
        Catch ex As ArgumentOutOfRangeException
            rangoExcesivoRechazado = True
        End Try
        Verificar("El conteo de días hábiles rechaza períodos excesivos", rangoExcesivoRechazado)

        ' Verificar primero la configuración efectiva del proceso de pruebas.
        ' No se permite continuar si por cualquier motivo no se cargó el App.config
        ' aislado: evita que una prueba toque sgln_sv o use TLS de producción.
        Dim configEfectiva As DatosConexion = ConfiguracionConexion.Cargar()
        Console.WriteLine("Configuración efectiva: Base=" & configEfectiva.BaseDatos &
                          "; TLS=" & configEfectiva.ModoSsl &
                          "; Origen=" & configEfectiva.Origen)
        Dim baseAisladaCorrecta As Boolean =
            String.Equals(configEfectiva.BaseDatos, "sgln_sv_pruebas", StringComparison.OrdinalIgnoreCase)
        Dim modoTlsEfectivo As String = If(configEfectiva.ModoSsl, "").Trim()
        Dim tlsPruebasCorrecto As Boolean =
            String.Equals(modoTlsEfectivo, "Preferred", StringComparison.OrdinalIgnoreCase)
        Verificar("El arnés carga exclusivamente sgln_sv_pruebas", baseAisladaCorrecta)
        Verificar("El arnés carga TLS Preferred únicamente para la base aislada", tlsPruebasCorrecto,
                  "Valor TLS recibido=[" & If(configEfectiva.ModoSsl, "<Nothing>") &
                  "]; longitud=" & If(configEfectiva.ModoSsl, "").Length.ToString())
        If Not baseAisladaCorrecta OrElse Not tlsPruebasCorrecto Then
            Console.WriteLine("Ejecución detenida por seguridad: revise PruebaFuncional.exe.config y su copia en Pruebas\\bin.")
            Environment.ExitCode = 1
            Return
        End If

        ConfiguracionConexion.Inicializar(True)

        Sesion.Limpiar()
        Dim operacionSinSesion As Respuesta = New ServicioUsuarios().Desactivar(1)
        Verificar("La capa de servicios deniega operaciones sin sesión autenticada",
                  Not operacionSinSesion.Exito AndAlso operacionSinSesion.Mensaje.Contains("No tiene permiso"))
        Verificar("La consulta de usuarios no expone datos sin sesión",
                  New ServicioUsuarios().Listar().Rows.Count = 0)
        Verificar("La consulta de roles de usuario no expone datos sin sesión",
                  New ServicioUsuarios().RolesDisponibles().Rows.Count = 0)
        Verificar("La bitácora no se puede consultar sin permiso",
                  New ServicioAuditoria().Buscar("", "", "", Date.Today.AddDays(-1), Date.Today).Rows.Count = 0)
        Verificar("Los clientes no se consultan sin permiso",
                  New ServicioClientes().Buscar("").Rows.Count = 0)
        Verificar("Las entidades no se consultan sin permiso",
                  New ServicioEntidades().Buscar("").Rows.Count = 0)
        Verificar("Los casos no se consultan sin permiso",
                  New ServicioCasos().Buscar("").Rows.Count = 0)
        Verificar("Las actuaciones procesales no se consultan sin permiso",
                  New ServicioCasos().Actuaciones(1).Rows.Count = 0)
        Verificar("Los documentos no se consultan sin permiso",
                  New ServicioDocumentos().Buscar("", "").Rows.Count = 0)
        Verificar("La ruta de documentos no se revela sin permiso",
                  New ServicioDocumentos().RutaBaseDocumentos() = "" AndAlso
                  New ServicioDocumentos().RutaCompleta("archivo.pdf") = "")
        Verificar("El protocolo no se consulta sin permiso",
                  New ServicioProtocolo().Libros().Rows.Count = 0)
        Verificar("Los comparecientes del protocolo no se exponen sin permiso",
                  New ServicioProtocolo().Comparecientes(1).Rows.Count = 0)
        Verificar("Los plazos y parámetros del protocolo se protegen sin permiso",
                  DenegadoPorPermiso(Function() CObj(New ServicioProtocolo().DiasEntregaConfigurados())) AndAlso
                  DenegadoPorPermiso(Function() CObj(New ServicioProtocolo().FechaLimiteEntrega(Date.Today))))
        Verificar("Las consultas del protocolo rechazan identificadores inválidos",
                  New ServicioProtocolo().ObtenerLibro(0).Rows.Count = 0 AndAlso
                  New ServicioProtocolo().Instrumentos(0).Rows.Count = 0 AndAlso
                  New ServicioProtocolo().Testimonios(0).Rows.Count = 0 AndAlso
                  New ServicioProtocolo().IndiceLibro(0).Rows.Count = 0)
        Verificar("Cumplimiento no se consulta sin permiso",
                  New ServicioCumplimiento().Operaciones("").Rows.Count = 0)
        Verificar("El umbral de cumplimiento no revela configuración sin permiso",
                  New ServicioCumplimiento().UmbralEfectivo() = 10000D)
        Verificar("Los cálculos ARCO se deniegan sin permiso",
                  DenegadoPorPermiso(Function() CObj(New ServicioCumplimiento().CalcularVencimientoArco(Date.Today))) AndAlso
                  DenegadoPorPermiso(Function() CObj(New ServicioCumplimiento().CalcularProrrogaArco(Date.Today))))
        Verificar("Los roles no se consultan sin permiso",
                  New ServicioRoles().Listar().Rows.Count = 0)
        Verificar("La configuración no se consulta sin permiso",
                  New ServicioConfiguracion().Listar().Rows.Count = 0)
        Verificar("La ruta de respaldos no se revela sin permiso",
                  New ServicioConfiguracion().CarpetaRespaldos() = "")
        Verificar("El panel de alertas no expone datos sin sesión",
                  New ServicioAlertas().ObtenerAlertas().Rows.Count = 0)
        Verificar("El resumen no expone conteos sin sesión",
                  New ServicioAlertas().ObtenerResumen() = "")
        Verificar("Los representantes no se consultan sin permiso",
                  New ServicioEntidades().Representantes(1).Rows.Count = 0)
        Verificar("Los combos de casos no exponen clientes sin permiso",
                  New ServicioCasos().ClientesCombo().Rows.Count = 0)
        Verificar("Los combos de protocolo no exponen clientes sin permiso",
                  New ServicioProtocolo().ClientesCombo().Rows.Count = 0)
        Verificar("Los combos documentales no exponen expedientes sin permiso",
                  New ServicioDocumentos().CasosCombo().Rows.Count = 0)
        Verificar("Los combos de cumplimiento no exponen clientes sin permiso",
                  New ServicioCumplimiento().ClientesCombo().Rows.Count = 0)
        Verificar("La actualización masiva de vencimientos se deniega sin permiso",
                  New ServicioPlazos().ActualizarVencidas() = 0)
        Verificar("El calendario y cálculo de plazos se protegen sin permiso",
                  DenegadoPorPermiso(Function() CObj(New ServicioPlazos().ObtenerCalculadora())) AndAlso
                  DenegadoPorPermiso(Function() CObj(New ServicioPlazos().CalcularVencimiento(Date.Today, 5))))

        ' ---------- 1. Inicialización de la base de pruebas ----------
        Console.WriteLine("-- 1. Inicialización de la base de datos")
        Dim tlsSinCaRechazado As Boolean = False
        Try
            Dim datosTls As New DatosConexion With {
                 .Servidor = "localhost", .Puerto = 3306, .BaseDatos = "sgln_sv_pruebas",
                .Usuario = "sgln_test_app", .Password = "Prueba",
                .ModoSsl = "VerifyCA", .CertificadoCa = ""
            }
            datosTls.ConstruirCadena()
        Catch ex As InvalidOperationException
            tlsSinCaRechazado = ex.Message.Contains("certificado CA")
        End Try
        Verificar("TLS con validación CA rechaza una conexión sin certificado raíz", tlsSinCaRechazado)

        Dim preferredProduccionRechazado As Boolean = False
        Try
            Dim datosPreferidos As New DatosConexion With {
                .Servidor = "localhost", .Puerto = 3306, .BaseDatos = "sgln_sv",
                .Usuario = "sgln_app", .Password = "Prueba", .ModoSsl = "Preferred"
            }
            datosPreferidos.ConstruirCadena()
        Catch ex As InvalidOperationException
            preferredProduccionRechazado = ex.Message.Contains("sgln_sv_pruebas")
        End Try
        Verificar("TLS Preferred se rechaza en la base de producción", preferredProduccionRechazado)

        Dim baseAjenaRechazada As Boolean = False
        Try
            Dim datosBaseAjena As New DatosConexion With {
                .Servidor = "localhost", .Puerto = 3306, .BaseDatos = "otra_base",
                .Usuario = "sgln_app", .Password = "Prueba",
                .ModoSsl = "Preferred"
            }
            datosBaseAjena.ConstruirCadena()
        Catch ex As InvalidOperationException
            baseAjenaRechazada = ex.Message.Contains("sgln_sv")
        End Try
        Verificar("La aplicación rechaza bases distintas de sgln_sv y sgln_sv_pruebas aislada",
                  baseAjenaRechazada)

        Dim preferredPruebasAceptado As Boolean = False
        Try
            Dim datosPreferidos As New DatosConexion With {
                .Servidor = "localhost", .Puerto = 3306, .BaseDatos = "sgln_sv_pruebas",
                .Usuario = "sgln_test_app", .Password = "Prueba", .ModoSsl = "Preferred"
            }
            preferredPruebasAceptado = datosPreferidos.ConstruirCadena().Length > 0
        Catch
            preferredPruebasAceptado = False
        End Try
        Verificar("TLS Preferred solo se admite para la base aislada de pruebas", preferredPruebasAceptado)

        Dim verifyFullDisponible As Boolean = False
        Dim caTemporal As String = Path.Combine(Path.GetTempPath(), "sgln-ca-" & Guid.NewGuid().ToString("N") & ".pem")
        Try
            File.WriteAllText(caTemporal, "CA de prueba para validar la configuración del constructor.")
            Dim datosVerifyFull As New DatosConexion With {
                .Servidor = "db.sglntest.local", .Puerto = 3306, .BaseDatos = "sgln_sv_pruebas",
                .Usuario = "sgln_test_app", .Password = "Prueba",
                .ModoSsl = "VerifyFull", .CertificadoCa = caTemporal
            }
            verifyFullDisponible = datosVerifyFull.ConstruirCadena().Contains("VerifyFull")
        Catch
            verifyFullDisponible = False
        Finally
            If File.Exists(caTemporal) Then File.Delete(caTemporal)
        End Try
        Verificar("El conector permite configurar VerifyFull con CA y nombre DNS", verifyFullDisponible)
        Dim inicializacionCorrecta As Boolean = False
        Try
            InicializadorBD.AsegurarBaseDeDatos()
            inicializacionCorrecta = True
            Verificar("El inicializador verifica el esquema y siembra catálogos y configuración", True)
        Catch ex As Exception
            Verificar("El inicializador verifica el esquema y siembra catálogos y configuración",
                      False, ex.Message)
        End Try

        If Not inicializacionCorrecta Then
            Console.WriteLine()
            Console.WriteLine("Pruebas detenidas: no se completó la inicialización de la base aislada.")
            Console.WriteLine("Pruebas ejecutadas: " & _pruebas.ToString())
            Console.WriteLine("Fallas: " & _fallos.ToString())
            Environment.ExitCode = 1
            Return
        End If

        ' ---------- 2. Autenticación y seguridad ----------
        Console.WriteLine()
        Console.WriteLine("-- 2. Autenticación y cambio de clave")
        Verificar("La instalación revoca permisos administrativos heredados en roles ordinarios",
                  ContarSqlDirecto(
                      "SELECT COUNT(*) FROM Rol_Permiso rp INNER JOIN Roles r ON r.ID = rp.ID_Rol " &
                      "WHERE r.Protegido = 0 AND rp.Codigo LIKE 'config.%'") = 0)

        Dim usuarios As New UsuarioRepositorio()
        If Not InicializadorBD.HayUsuarios() Then InicializadorBD.CrearAdministradorInicial("admin", "Administrador de pruebas", "PruebaInicial99!")
        Dim admin As UsuarioSesion = usuarios.Autenticar("admin", "PruebaInicial99!")
        If admin IsNot Nothing Then
            Sesion.UsuarioActual = admin
            Sesion.CargarPermisos()
        End If
        Verificar("Login acepta la cuenta creada por el asistente inicial", admin IsNot Nothing)
        Verificar("El inicio de sesión queda registrado obligatoriamente en la bitácora",
                  admin IsNot Nothing AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Usuario = 'admin' " &
                                   "AND Accion = 'INGRESO' AND Tabla_Afectada = 'Usuarios' " &
                                   "AND ID_Registro = " & admin.ID.ToString()) > 0)
        Verificar("La cuenta creada con clave propia no requiere cambio forzado",
                  admin IsNot Nothing AndAlso Not admin.DebeCambiarClave)
        If admin Is Nothing Then
            Console.WriteLine("Pruebas detenidas: no se pudo autenticar el administrador inicial; se omiten pruebas dependientes de sesión.")
            Console.WriteLine("Pruebas ejecutadas: " & _pruebas.ToString())
            Console.WriteLine("Fallas: " & _fallos.ToString())
            Environment.ExitCode = 1
            Return
        End If

        Dim segundoAdminRechazado As Boolean = False
        Try
            InicializadorBD.CrearAdministradorInicial("admin2", "Segundo administrador no permitido",
                                                       "SegundaClaveSegura123!")
        Catch ex As InvalidOperationException
            segundoAdminRechazado = ex.Message.Contains("Ya existe al menos un usuario")
        End Try
        Verificar("El asistente inicial no permite crear un segundo administrador",
                  segundoAdminRechazado)

        IntentarSinExcepcion("Login con clave incorrecta solo devuelve Nothing",
            Sub()
                If usuarios.Autenticar("admin", "ClaveMala1") IsNot Nothing Then
                    Throw New Exception("Aceptó una clave incorrecta")
                End If
            End Sub)
        IntentarSinExcepcion("Login con usuario inexistente solo devuelve Nothing",
            Sub()
                If usuarios.Autenticar("nadie", "x") IsNot Nothing Then
                    Throw New Exception("Aceptó un usuario inexistente")
                End If
            End Sub)

        Dim seguridad As New ServicioConfiguracion()
        Dim claveMal As Respuesta = seguridad.CambiarClave(admin.ID, "Incorrecta99", "NuevaClave99!")
        Verificar("Cambio de clave con clave actual incorrecta se rechaza", Not claveMal.Exito)

        Dim claveBien As Respuesta = seguridad.CambiarClave(admin.ID, "PruebaInicial99!", "TemporalClave99!")
        Verificar("Cambio de clave válido se acepta", claveBien.Exito)
        Verificar("La nueva clave permite iniciar sesión",
                  usuarios.Autenticar("admin", "TemporalClave99!") IsNot Nothing AndAlso
                  Not usuarios.Autenticar("admin", "TemporalClave99!").DebeCambiarClave)
        Verificar("La clave anterior deja de funcionar",
                  usuarios.Autenticar("admin", "PruebaInicial99!") Is Nothing)
        Verificar("La clave inicial ya no vuelve a aceptarse",
                  usuarios.Autenticar("admin", "PruebaInicial99!") Is Nothing)

        Dim servicioUsuariosClave As New ServicioUsuarios()
        Verificar("El cambio de clave propia rechaza una clave actual incorrecta",
                  Not servicioUsuariosClave.CambiarClavePropia(admin.ID, "Incorrecta99!", "CambioPropio99!").Exito)
        Verificar("El repositorio también rechaza una clave actual incorrecta sin pasar por el servicio",
                  Not New UsuarioRepositorio().CambiarClavePropia(
                      admin.ID, "Incorrecta99!", "CambioDirecto99!").Exito)
        Dim cambioClavePropia As Respuesta = servicioUsuariosClave.CambiarClavePropia(
            admin.ID, "TemporalClave99!", "CambioPropioTemporal99!")
        Verificar("El usuario puede cambiar su propia clave con la clave actual",
                  cambioClavePropia.Exito AndAlso usuarios.Autenticar("admin", "CambioPropioTemporal99!") IsNot Nothing)
        Verificar("El repositorio impide restablecer directamente la clave de la cuenta propia sin validar la clave actual",
                  Not New UsuarioRepositorio().CambiarClave(admin.ID, "AccesoDirecto99!").Exito)
        Dim restauracionClavePrueba As Respuesta = servicioUsuariosClave.CambiarClavePropia(
            admin.ID, "CambioPropioTemporal99!", "TemporalClave99!")
        Verificar("La prueba restaura la clave de administrador para el resto del arnés",
                  restauracionClavePrueba.Exito)
        Verificar("Una clave nueva de primer ingreso exige longitud y complejidad",
                  SeguridadClave.ValidarClaveNuevaInicial("ClaveInicial99!") = "")
        Verificar("La clave nueva rechaza más de 128 caracteres",
                  SeguridadClave.ValidarClaveNuevaInicial(New String("A"c, 126) & "a1!") <> "")
        Verificar("El repositorio rechaza nombres de usuario y claves fuera de límite antes de consultar",
                  usuarios.Autenticar(New String("u"c, 51), "ClaveValida99!") Is Nothing AndAlso
                  usuarios.Autenticar("admin", New String("A"c, 129)) Is Nothing)
        Verificar("La clave inicial no puede reutilizarse como nueva",
                  SeguridadClave.ValidarClaveNuevaInicial("Admin123*") <> "")
        Verificar("La política común rechaza claves de 8 a 11 caracteres",
                  SeguridadClave.ValidarClaveNuevaInicial("Prueba123*") <> "")
        Verificar("La política común rechaza claves sin símbolo",
                  SeguridadClave.ValidarClaveNuevaInicial("PruebaSegura123") <> "")

        ' Compatibilidad de hashes PBKDF2-SHA1 creados por versiones anteriores.
        Dim saltLegado(15) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(saltLegado)
        End Using
        Dim hashLegado As String
        Using derivador As New Rfc2898DeriveBytes("ClaveLegada99!", saltLegado, 100000)
            hashLegado = Convert.ToBase64String(derivador.GetBytes(32))
        End Using
        Dim requiereMigracion As Boolean = False
        Verificar("Los hashes PBKDF2-SHA1 antiguos siguen autenticando y se marcan para migración",
                  SeguridadClave.Verificar("ClaveLegada99!", hashLegado,
                      Convert.ToBase64String(saltLegado), requiereMigracion) AndAlso requiereMigracion)

        Sesion.UsuarioActual = admin

        ' ---------- 2b. Roles, permisos y candados ----------
        Console.WriteLine()
        Console.WriteLine("-- 2b. Roles, permisos y candados de seguridad")
        Permisos.Silencioso = True

        Dim roles As New ServicioRoles()
        Dim servicioUsuarios As New ServicioUsuarios()

        Dim catalogoRoles As DataTable = roles.Listar()
        Verificar("Están sembrados los 5 roles iniciales", catalogoRoles.Rows.Count >= 5,
                  "Roles: " & catalogoRoles.Rows.Count.ToString())

        Dim filaRolAdmin As DataRow = Nothing
        Dim idRolAbogado As Integer = 0
        For Each fila As DataRow In catalogoRoles.Rows
            If Convert.ToInt32(fila("Protegido")) = 1 Then filaRolAdmin = fila
            If UtileriasUI.Texto(fila, "Nombre") = "Abogado" Then idRolAbogado = UtileriasUI.Entero(fila, "ID")
        Next

        Verificar("Existe un solo rol protegido y es el Administrador",
                  filaRolAdmin IsNot Nothing AndAlso
                  UtileriasUI.Texto(filaRolAdmin, "Nombre") = "Administrador")
        Dim idRolAdmin As Integer = 0
        If filaRolAdmin IsNot Nothing Then idRolAdmin = UtileriasUI.Entero(filaRolAdmin, "ID")

        Dim sufijoAdminTemporal As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Dim nombreAdminTemporal As String = "adminsec" & sufijoAdminTemporal
        Dim altaAdminTemporal As Respuesta = servicioUsuarios.Crear(
            nombreAdminTemporal, "Administrador temporal de prueba",
            "AdminTemporal99!", idRolAdmin, True)
        Dim idAdminTemporal As Integer = 0
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = nombreAdminTemporal Then
                idAdminTemporal = UtileriasUI.Entero(fila, "ID")
                Exit For
            End If
        Next
        Dim sesionAdminTemporal As UsuarioSesion = usuarios.ObtenerEstadoSesion(idAdminTemporal)
        Verificar("Un administrador puede crear otra cuenta con rol Administrador protegido",
                  altaAdminTemporal.Exito AndAlso idAdminTemporal > 0 AndAlso
                  sesionAdminTemporal IsNot Nothing AndAlso sesionAdminTemporal.RolProtegido)
        If idAdminTemporal > 0 Then
            Verificar("La eliminación segura permite retirar un administrador temporal si queda otro activo",
                      servicioUsuarios.EliminarDefinitivamente(idAdminTemporal).Exito)
        End If

        Dim permisosAdmin As List(Of String) = roles.PermisosDeRol(idRolAdmin)
        Verificar("El Administrador tiene todos los permisos del catálogo",
                  permisosAdmin.Count = roles.CatalogoPermisos().Rows.Count,
                  "Permisos: " & permisosAdmin.Count.ToString())
        Verificar("El Administrador conserva config.usuarios y config.roles",
                  permisosAdmin.Contains("config.usuarios") AndAlso permisosAdmin.Contains("config.roles"))

        Sesion.CargarPermisos()
        Verificar("La sesión se revalida contra la cuenta y rol vigentes", Permisos.TieneAcceso("config.usuarios"))
        Verificar("Un administrador no puede cambiar su propio rol desde mantenimiento de usuarios",
                  Not servicioUsuarios.Actualizar(admin.ID, admin.NombreCompleto, idRolAbogado, True).Exito)
        Verificar("Un administrador no puede desactivar su propia cuenta desde mantenimiento",
                  Not servicioUsuarios.Actualizar(admin.ID, admin.NombreCompleto, idRolAdmin, False).Exito)
        Verificar("El repositorio también impide cambiar directamente el rol de la propia cuenta",
                  Not usuarios.Actualizar(admin.ID, admin.NombreCompleto, idRolAbogado, True).Exito)
        Verificar("El repositorio rechaza directamente nombres de usuario fuera del formato permitido",
                  Not usuarios.Crear("usuario invalido", "Usuario de prueba", "ClaveTemporal99!", idRolAbogado, True).Exito)
        Verificar("El repositorio rechaza directamente nombres completos vacíos o excesivos",
                  Not usuarios.Crear("usuario" & Guid.NewGuid().ToString("N").Substring(0, 8), New String("N"c, 151),
                                     "ClaveTemporal99!", idRolAbogado, True).Exito)
        Verificar("El restablecimiento administrativo no permite omitir la clave actual de la propia cuenta",
                  Not servicioUsuarios.RestablecerClave(admin.ID, "NuevaClaveAdmin99!").Exito)
        Dim eliminacionPropiaDirecta As Respuesta = New UsuarioRepositorio().EliminarSiNoTieneHistorial(admin.ID)
        Verificar("El repositorio impide la autoeliminación incluso si se invoca directamente",
                  Not eliminacionPropiaDirecta.Exito)
        Verificar("La sesión del administrador tiene acceso total", Permisos.AccesoTotal)
        Verificar("El acceso total concede cualquier permiso", Permisos.Puede("modulo.inexistente"))
        Verificar("El Administrador puede modificar configuración", Permisos.Puede("config.editar"))

        If idRolAbogado > 0 Then
            Dim usuarioBloqueo As String = "bloq" & Guid.NewGuid().ToString("N").Substring(0, 8)
            Dim altaUsuarioBloqueo As Respuesta = servicioUsuarios.Crear(
                usuarioBloqueo, "Usuario temporal de bloqueo", "BloqueoClave99!", idRolAbogado, True)
            Verificar("Se crea una cuenta aislada para probar el bloqueo por intentos fallidos", altaUsuarioBloqueo.Exito)

            If altaUsuarioBloqueo.Exito Then
                For intento As Integer = 1 To 5
                    If usuarios.Autenticar(usuarioBloqueo, "Incorrecta99!") IsNot Nothing Then
                        Throw New Exception("Se aceptó una clave incorrecta durante la prueba de bloqueo.")
                    End If
                Next
                Verificar("Cinco intentos fallidos bloquean incluso un ingreso con la clave correcta",
                          usuarios.Autenticar(usuarioBloqueo, "BloqueoClave99!") Is Nothing)

                Dim idUsuarioBloqueo As Integer = 0
                For Each fila As DataRow In servicioUsuarios.Listar().Rows
                    If UtileriasUI.Texto(fila, "Usuario") = usuarioBloqueo Then
                        idUsuarioBloqueo = UtileriasUI.Entero(fila, "ID")
                        Exit For
                    End If
                Next
                Dim restablecimientoBloqueo As Respuesta =
                    servicioUsuarios.RestablecerClave(idUsuarioBloqueo, "DesbloqueoSeguro99!")
                Verificar("Restablecer la clave también retira el bloqueo de acceso",
                          idUsuarioBloqueo > 0 AndAlso restablecimientoBloqueo.Exito AndAlso
                          usuarios.Autenticar(usuarioBloqueo, "DesbloqueoSeguro99!") IsNot Nothing,
                          restablecimientoBloqueo.Mensaje)
                Verificar("No se elimina una cuenta que conserva historial de intentos de acceso",
                          idUsuarioBloqueo > 0 AndAlso
                          Not servicioUsuarios.EliminarDefinitivamente(idUsuarioBloqueo).Exito)
            End If
        End If

        Permisos.Cargar(idRolAbogado, False)
        Verificar("El rol Abogado puede ver casos", Permisos.Puede("casos.ver"))
        Verificar("El rol Abogado no administra usuarios", Not Permisos.Puede("config.usuarios"))
        Verificar("El rol Abogado no administra roles", Not Permisos.Puede("config.roles"))
        Verificar("El rol Abogado no puede modificar configuración", Not Permisos.Puede("config.editar"))
        Verificar("El rol Abogado no puede crear respaldos", Not Permisos.Puede("config.respaldos"))
        Verificar("Un código de permiso vacío se deniega", Not Permisos.Puede(""))
        Verificar("Un código de permiso con espacios se deniega", Not Permisos.Puede("   "))
        Verificar("Abrir un formulario sin código de permiso se deniega",
                  Not Permisos.Exigir("", Nothing))
        Verificar("Ejecutar una acción sin código de permiso se deniega",
                  Not Permisos.ExigirAccion(""))

        Sesion.CargarPermisos()
        Verificar("Al recargar la sesión del administrador vuelve el acceso total", Permisos.AccesoTotal)

        Dim rRolNuevo As Respuesta = roles.Crear("Rol de prueba", "Rol temporal de la batería",
            New List(Of String) From {"clientes.ver", "clientes.gestionar"})
        Verificar("Crear un rol nuevo con permisos", rRolNuevo.Exito, rRolNuevo.Mensaje)

        Dim idRolPrueba As Integer = 0
        For Each fila As DataRow In roles.Listar().Rows
            If UtileriasUI.Texto(fila, "Nombre") = "Rol de prueba" Then idRolPrueba = UtileriasUI.Entero(fila, "ID")
        Next
        Verificar("El rol nuevo aparece en el listado", idRolPrueba > 0)
        Verificar("El rol nuevo guarda exactamente los permisos indicados",
                  roles.PermisosDeRol(idRolPrueba).Count = 2)

        Verificar("No se permite crear un rol con nombre repetido",
                  Not roles.Crear("Rol de prueba", "Otro", New List(Of String)()).Exito)
        Verificar("No se permite crear un rol sin descripción",
                  Not roles.Crear("Rol sin descripción", "", New List(Of String)()).Exito)

        Dim rRolEdit As Respuesta = roles.Actualizar(idRolPrueba, "Rol de prueba editado",
            "Descripción editada", True, New List(Of String) From {"casos.ver"})
        Verificar("Editar el rol de prueba", rRolEdit.Exito, rRolEdit.Mensaje)
        Verificar("Una descripción de rol superior a 200 caracteres se rechaza",
                  Not roles.Actualizar(idRolPrueba, "Rol de prueba editado",
                      New String("D"c, 201), True,
                      New List(Of String) From {"casos.ver"}).Exito)
        Verificar("Un identificador de rol inválido se rechaza",
                  Not roles.Actualizar(0, "Rol inválido", "Descripción", True,
                      New List(Of String) From {"casos.ver"}).Exito)
        Verificar("La matriz de permisos se reemplaza al editar",
                  roles.PermisosDeRol(idRolPrueba).Count = 1 AndAlso
                  roles.PermisosDeRol(idRolPrueba).Contains("casos.ver"))
        Verificar("Un código inexistente se descarta en vez de romper el guardado",
                  roles.Actualizar(idRolPrueba, "Rol de prueba editado", "Descripción editada", True,
                      New List(Of String) From {"casos.ver", "permiso.inventado"}).Exito)
        Verificar("El permiso inventado no queda guardado",
                  roles.PermisosDeRol(idRolPrueba).Count = 1)

        ' Las operaciones directas del repositorio deben revertir también la
        ' fila de Roles si falla la inserción de cualquier permiso relacionado.
        Dim repositorioRolesPrueba As New RolRepositorio()
        Verificar("El repositorio rechaza directamente nombres de rol vacíos o excesivos",
                  Not repositorioRolesPrueba.Crear("", "Descripción válida", New List(Of String)()).Exito AndAlso
                  Not repositorioRolesPrueba.Crear(New String("R"c, 51), "Descripción válida",
                      New List(Of String)()).Exito)
        Verificar("El repositorio rechaza directamente descripciones de rol vacías o excesivas",
                  Not repositorioRolesPrueba.Crear("Rol inválido", "", New List(Of String)()).Exito AndAlso
                  Not repositorioRolesPrueba.Crear("Rol inválido", New String("D"c, 201),
                      New List(Of String)()).Exito)
        Verificar("El repositorio no permite guardar permisos para un rol inexistente",
                  Not repositorioRolesPrueba.GuardarPermisos(0, New List(Of String)()).Exito)
        Dim altaAtomica As Respuesta = repositorioRolesPrueba.Crear(
            "Rol transaccional fallido", "No debe persistir",
            New List(Of String) From {"permiso.inexistente"})
        Verificar("Un permiso inválido hace fallar el alta transaccional", Not altaAtomica.Exito)

        Dim existeRolParcial As Boolean = False
        For Each fila As DataRow In roles.Listar().Rows
            If UtileriasUI.Texto(fila, "Nombre") = "Rol transaccional fallido" Then
                existeRolParcial = True
                Exit For
            End If
        Next
        Verificar("El alta fallida no deja un rol sin su matriz de permisos", Not existeRolParcial)

        Dim edicionAtomica As Respuesta = repositorioRolesPrueba.Actualizar(
            idRolPrueba, "Nombre no persistente", "Descripción no persistente", False,
            New List(Of String) From {"permiso.inexistente"})
        Verificar("Un permiso inválido hace fallar la edición transaccional", Not edicionAtomica.Exito)

        Dim rolTrasFallo As DataTable = roles.Obtener(idRolPrueba)
        Verificar("La edición fallida revierte nombre, descripción y estado",
                  rolTrasFallo.Rows.Count = 1 AndAlso
                  UtileriasUI.Texto(rolTrasFallo.Rows(0), "Nombre") = "Rol de prueba editado" AndAlso
                  UtileriasUI.Texto(rolTrasFallo.Rows(0), "Descripcion") = "Descripción editada" AndAlso
                  UtileriasUI.Booleano(rolTrasFallo.Rows(0), "Activo"))
        Verificar("La edición fallida conserva los permisos anteriores",
                  roles.PermisosDeRol(idRolPrueba).Count = 1 AndAlso
                  roles.PermisosDeRol(idRolPrueba).Contains("casos.ver"))

        Verificar("El servicio impide eliminar el rol Administrador",
                  Not roles.Eliminar(idRolAdmin).Exito)
        Verificar("El servicio impide quitar config.roles al Administrador",
                  Not roles.Actualizar(idRolAdmin, "Administrador", "x", True,
                      New List(Of String) From {"config.usuarios", "config.roles"}).Exito)
        Verificar("El servicio impide quitar config.usuarios al Administrador",
                  Not roles.Actualizar(idRolAdmin, "Administrador", "x", True,
                      New List(Of String) From {"config.roles"}).Exito)

        Dim rUsuarioPrueba As Respuesta = servicioUsuarios.Crear(
            "pruebauser", "Usuario de Prueba", "PruebaSegura123!", idRolPrueba, True)
        Verificar("Crear un usuario con el rol nuevo", rUsuarioPrueba.Exito, rUsuarioPrueba.Mensaje)

        Dim idUsuarioPrueba As Integer = 0
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = "pruebauser" Then idUsuarioPrueba = UtileriasUI.Entero(fila, "ID")
        Next
        Verificar("El usuario nuevo aparece en el listado", idUsuarioPrueba > 0)

        If idUsuarioPrueba > 0 Then
            Dim edicionEstadoDirecta As Respuesta = usuarios.Actualizar(
                idUsuarioPrueba, "Usuario de Prueba", idRolPrueba, False)
            Dim estadoTrasEdicionDirecta As DataTable = usuarios.Obtener(idUsuarioPrueba)
            Verificar("El repositorio impide desactivar usuarios desde la edición general",
                      Not edicionEstadoDirecta.Exito AndAlso estadoTrasEdicionDirecta.Rows.Count = 1 AndAlso
                      UtileriasUI.Booleano(estadoTrasEdicionDirecta.Rows(0), "Activo"))
            Dim desactivacionDedicada As Respuesta = servicioUsuarios.Desactivar(idUsuarioPrueba)
            Dim estadoTrasDesactivar As DataTable = usuarios.Obtener(idUsuarioPrueba)
            Verificar("La acción dedicada desactiva y conserva la cuenta para trazabilidad",
                      desactivacionDedicada.Exito AndAlso estadoTrasDesactivar.Rows.Count = 1 AndAlso
                      Not UtileriasUI.Booleano(estadoTrasDesactivar.Rows(0), "Activo"))
        End If

        ' Un rol ordinario no puede recibir permisos administrativos ni por SQL directo.
        Dim sufijoGestor As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Dim nombreRolGestor As String = "Gestor usuarios " & sufijoGestor
        Dim nombreUsuarioGestor As String = "gestor" & sufijoGestor
        Dim altaRolGestor As Respuesta = roles.Crear(nombreRolGestor,
            "Rol ordinario para probar la separación de privilegios",
            New List(Of String) From {"casos.ver"})
        Dim idRolGestor As Integer = 0
        For Each fila As DataRow In roles.Listar().Rows
            If UtileriasUI.Texto(fila, "Nombre") = nombreRolGestor Then
                idRolGestor = UtileriasUI.Entero(fila, "ID")
                Exit For
            End If
        Next
        Dim altaGestor As Respuesta = servicioUsuarios.Crear(
            nombreUsuarioGestor, "Gestor temporal de pruebas", "GestorPrueba99!", idRolGestor, True)
        Dim idUsuarioGestor As Integer = 0
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = nombreUsuarioGestor Then
                idUsuarioGestor = UtileriasUI.Entero(fila, "ID")
                Exit For
            End If
        Next
        Dim sesionGestor As UsuarioSesion = usuarios.ObtenerEstadoSesion(idUsuarioGestor)
        Sesion.UsuarioActual = sesionGestor
        Sesion.CargarPermisos()
        Verificar("El rol ordinario de prueba se crea sin privilegios administrativos",
                  altaRolGestor.Exito AndAlso altaGestor.Exito AndAlso
                  sesionGestor IsNot Nothing AndAlso Permisos.TieneAcceso("casos.ver") AndAlso
                  Not Permisos.TieneAcceso("config.usuarios") AndAlso
                  Not Permisos.TieneAcceso("config.roles") AndAlso Not Sesion.UsuarioActual.RolProtegido)
        Dim cambioClaveGestor As Respuesta = servicioUsuarios.CambiarClavePropia(
            idUsuarioGestor, "GestorPrueba99!", "GestorCambioTemporal99!")
        Verificar("Un usuario ordinario puede cambiar su propia clave sin permisos administrativos",
                  cambioClaveGestor.Exito AndAlso
                  usuarios.VerificarClave(idUsuarioGestor, "GestorCambioTemporal99!"))
        Dim restauracionClaveGestor As Respuesta = servicioUsuarios.CambiarClavePropia(
            idUsuarioGestor, "GestorCambioTemporal99!", "GestorPrueba99!")
        Verificar("La prueba restaura la clave del usuario ordinario",
                  restauracionClaveGestor.Exito)
        Verificar("Un rol ordinario no puede desactivar ni eliminar usuarios",
                  Not servicioUsuarios.Desactivar(idUsuarioPrueba).Exito AndAlso
                  Not servicioUsuarios.EliminarDefinitivamente(idUsuarioPrueba).Exito)
        Verificar("El repositorio no permite verificar la clave de otra cuenta",
                  Not New UsuarioRepositorio().VerificarClave(admin.ID, "TemporalClave99!"))
        Verificar("El repositorio no expone el listado ni el estado de otros usuarios a un rol ordinario",
                  New UsuarioRepositorio().Listar().Rows.Count = 0 AndAlso
                  New UsuarioRepositorio().Obtener(admin.ID).Rows.Count = 0 AndAlso
                  New UsuarioRepositorio().ObtenerEstadoSesion(admin.ID) Is Nothing)
        Verificar("El repositorio oculta conteos y clasificación de administradores a roles ordinarios",
                  New UsuarioRepositorio().ContarAdministradoresActivos(0) = 0 AndAlso
                  Not New UsuarioRepositorio().EsAdministrador(admin.ID))
        Verificar("El repositorio no expone el catálogo de roles a un rol ordinario",
                  New RolRepositorio().Listar().Rows.Count = 0 AndAlso
                  New RolRepositorio().Obtener(idRolAdmin).Rows.Count = 0 AndAlso
                  New RolRepositorio().RolesActivos().Rows.Count = 0 AndAlso
                  New RolRepositorio().CatalogoPermisos().Rows.Count = 0 AndAlso
                   New RolRepositorio().PermisosDeRol(idRolAdmin).Rows.Count = 0)
        Dim auditoriaDirecta As New AuditoriaRepositorio()
        Verificar("El repositorio bloquea consultas directas a la bitácora sin permiso",
                  auditoriaDirecta.Buscar("", "", "", Date.Today.AddDays(-1), Date.Today).Rows.Count = 0 AndAlso
                  auditoriaDirecta.TablasAfectadas().Rows.Count = 0 AndAlso
                  auditoriaDirecta.Acciones().Rows.Count = 0 AndAlso
                  auditoriaDirecta.Usuarios().Rows.Count = 0)
        Dim repositorioConfiguracionPrueba As New ConfiguracionRepositorio()
        Verificar("El repositorio de configuración no expone datos a un rol sin permisos",
                  repositorioConfiguracionPrueba.Listar().Rows.Count = 0 AndAlso
                  repositorioConfiguracionPrueba.Obtener("Carpeta_Respaldos") = "" AndAlso
                  repositorioConfiguracionPrueba.Feriados().Rows.Count = 0 AndAlso
                  repositorioConfiguracionPrueba.Usuarios().Rows.Count = 0)
        Dim catalogoConfigPrueba As New CatalogoRepositorio()
        Verificar("El catálogo no permite listar ni modificar configuración sin permiso",
                  catalogoConfigPrueba.ListarConfig().Rows.Count = 0 AndAlso
                  Not catalogoConfigPrueba.GuardarConfig("Prueba_SinPermiso", "No debe guardarse").Exito)
        Verificar("Los parámetros internos de cumplimiento y protocolo se ocultan sin permiso de módulo",
                  catalogoConfigPrueba.ObtenerConfig("Umbral_Efectivo") = "" AndAlso
                  catalogoConfigPrueba.ObtenerConfig("Protocolo_Dias_Entrega") = "")

        Dim documentosDirectos As New DocumentoRepositorio()
        Verificar("El repositorio documental bloquea lecturas directas sin permisos del módulo",
                  documentosDirectos.Buscar("", "").Rows.Count = 0 AndAlso
                  documentosDirectos.Obtener(1).Rows.Count = 0 AndAlso
                  documentosDirectos.CasosCombo().Rows.Count = 0 AndAlso
                  documentosDirectos.InstrumentosCombo().Rows.Count = 0)

        Dim clientesDirectos As New ClienteRepositorio()
        Dim entidadesDirectas As New EntidadRepositorio()
        Dim casosDirectos As New CasoRepositorio()
        Verificar("Los repositorios de clientes, entidades y casos bloquean lecturas directas sin permiso",
                  clientesDirectos.Buscar("").Rows.Count = 0 AndAlso
                  clientesDirectos.Obtener(1).Rows.Count = 0 AndAlso
                  entidadesDirectas.Buscar("").Rows.Count = 0 AndAlso
                  entidadesDirectas.Obtener(1).Rows.Count = 0 AndAlso
                  entidadesDirectas.Representantes(1).Rows.Count = 0 AndAlso
                  entidadesDirectas.ClientesCombo().Rows.Count = 0 AndAlso
                  casosDirectos.Buscar("").Rows.Count = 0 AndAlso
                  casosDirectos.Obtener(1).Rows.Count = 0 AndAlso
                  casosDirectos.ActuacionesPorCaso(1).Rows.Count = 0 AndAlso
                  casosDirectos.ClientesCombo().Rows.Count = 0)
        Dim escrituraClienteBloqueada As Boolean = False
        Dim escrituraEntidadBloqueada As Boolean = False
        Dim escrituraCasoBloqueada As Boolean = False
        Try
            clientesDirectos.GuardarCompleto(Nothing, Nothing)
        Catch ex As UnauthorizedAccessException
            escrituraClienteBloqueada = True
        End Try
        Try
            entidadesDirectas.GuardarCompleto(Nothing, Nothing)
        Catch ex As UnauthorizedAccessException
            escrituraEntidadBloqueada = True
        End Try
        Try
            casosDirectos.Insertar(Nothing)
        Catch ex As UnauthorizedAccessException
            escrituraCasoBloqueada = True
        End Try
        Verificar("Los repositorios bloquean escrituras directas de clientes, entidades y casos sin permiso",
                  escrituraClienteBloqueada AndAlso escrituraEntidadBloqueada AndAlso escrituraCasoBloqueada)
        Dim confirmacionPlazoDirectaBloqueada As Boolean = False
        Try
            casosDirectos.GuardarActuacion(Nothing)
        Catch ex As UnauthorizedAccessException
            confirmacionPlazoDirectaBloqueada = True
        End Try
        Verificar("El repositorio de casos bloquea confirmaciones directas de plazos sin permiso",
                  confirmacionPlazoDirectaBloqueada)

        Dim cumplimientoDirecto As New CumplimientoRepositorio()
        Dim protocoloDirecto As New ProtocoloRepositorio()
        Verificar("Los vínculos documentales no permiten consultar referencias sin permiso documental",
                  Not casosDirectos.ExisteParaVinculoDocumento(1) AndAlso
                  Not protocoloDirecto.ExisteInstrumentoParaVinculoDocumento(1))
        Verificar("Los repositorios de cumplimiento y protocolo bloquean lecturas directas sin permiso",
                  cumplimientoDirecto.Operaciones("").Rows.Count = 0 AndAlso
                  cumplimientoDirecto.ObtenerOperacion(1).Rows.Count = 0 AndAlso
                  cumplimientoDirecto.SolicitudesArco("").Rows.Count = 0 AndAlso
                  cumplimientoDirecto.ObtenerSolicitud(1).Rows.Count = 0 AndAlso
                  cumplimientoDirecto.ClientesCombo().Rows.Count = 0 AndAlso
                  cumplimientoDirecto.CasosCombo().Rows.Count = 0 AndAlso
                  cumplimientoDirecto.InstrumentosCombo().Rows.Count = 0 AndAlso
                  protocoloDirecto.Libros().Rows.Count = 0 AndAlso
                  protocoloDirecto.ObtenerLibro(1).Rows.Count = 0 AndAlso
                  protocoloDirecto.Instrumentos(1).Rows.Count = 0 AndAlso
                  protocoloDirecto.ObtenerInstrumento(1).Rows.Count = 0 AndAlso
                  protocoloDirecto.Comparecientes(1).Rows.Count = 0 AndAlso
                  protocoloDirecto.Testimonios(1).Rows.Count = 0 AndAlso
                  protocoloDirecto.IndiceLibro(1).Rows.Count = 0 AndAlso
                  protocoloDirecto.ClientesCombo().Rows.Count = 0)
        Dim insercionOperacionBloqueada As Boolean = False
        Dim insercionArcoBloqueada As Boolean = False
        Dim insercionLibroBloqueada As Boolean = False
        Dim insercionInstrumentoBloqueada As Boolean = False
        Try
            cumplimientoDirecto.InsertarOperacion(Nothing)
        Catch ex As UnauthorizedAccessException
            insercionOperacionBloqueada = True
        End Try
        Try
            cumplimientoDirecto.InsertarSolicitud(Nothing)
        Catch ex As UnauthorizedAccessException
            insercionArcoBloqueada = True
        End Try
        Try
            protocoloDirecto.InsertarLibro(Nothing)
        Catch ex As UnauthorizedAccessException
            insercionLibroBloqueada = True
        End Try
        Try
            protocoloDirecto.InsertarInstrumento(Nothing)
        Catch ex As UnauthorizedAccessException
            insercionInstrumentoBloqueada = True
        End Try
        Verificar("Los repositorios bloquean escrituras directas de cumplimiento y protocolo sin permiso",
                  insercionOperacionBloqueada AndAlso insercionArcoBloqueada AndAlso
                  insercionLibroBloqueada AndAlso insercionInstrumentoBloqueada)
        Dim insercionDocumentoDirectaBloqueada As Boolean = False
        Try
            documentosDirectos.Insertar(New Documento With {
                .TipoDocumento = "Prueba no autorizada",
                .RutaArchivo = "Documentos\\no_autorizado.pdf",
                .Hash = New String("0"c, 64),
                .EstadoProcesamientoIA = "No procesado"
            })
        Catch ex As UnauthorizedAccessException
            insercionDocumentoDirectaBloqueada = True
        End Try
        Verificar("El repositorio documental bloquea altas directas sin permiso de importación",
                  insercionDocumentoDirectaBloqueada)
        Dim metadatosDocumentoDirectosBloqueados As Boolean = False
        Try
            documentosDirectos.ActualizarMetadatos(New Documento With {
                .ID = 1,
                .TipoDocumento = "Cambio no autorizado"
            })
        Catch ex As UnauthorizedAccessException
            metadatosDocumentoDirectosBloqueados = True
        End Try
        Verificar("El repositorio documental bloquea cambios directos sin permiso de metadatos",
                  metadatosDocumentoDirectosBloqueados)
        Dim configuracionDirectaBloqueada As Boolean = False
        Try
            repositorioConfiguracionPrueba.Guardar("Prueba_Acceso_NoAutorizado", "No debe guardarse")
        Catch ex As UnauthorizedAccessException
            configuracionDirectaBloqueada = True
        End Try
        Verificar("El repositorio bloquea cambios directos de configuración sin permiso",
                  configuracionDirectaBloqueada)
        Dim feriadoDirectoBloqueado As Boolean = False
        Try
            repositorioConfiguracionPrueba.InsertarFeriado(New Date(2099, 1, 2), "No autorizado")
        Catch ex As UnauthorizedAccessException
            feriadoDirectoBloqueado = True
        End Try
        Verificar("El repositorio bloquea altas directas de feriados sin permiso",
                  feriadoDirectoBloqueado)
        Dim respaldoDirectoBloqueado As Boolean = False
        Try
            repositorioConfiguracionPrueba.RespaldarBaseDatos(
                Path.Combine(Path.GetTempPath(), "sgln_no_autorizado.sql"))
        Catch ex As UnauthorizedAccessException
            respaldoDirectoBloqueado = True
        End Try
        Verificar("El repositorio bloquea respaldos directos sin permiso",
                  respaldoDirectoBloqueado)
        Dim desactivarRolConUsuariosBloqueado As Boolean = False
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
                Using comando As New MySqlCommand(
                    "UPDATE Roles SET Activo = 0 WHERE ID = @id", conexion)
                    comando.Parameters.AddWithValue("@id", idRolPrueba)
                    comando.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As MySqlException
            desactivarRolConUsuariosBloqueado = ex.Number = 1644
        End Try
        Verificar("MySQL impide desactivar directamente un rol que tiene usuarios asignados",
                  desactivarRolConUsuariosBloqueado AndAlso roles.Obtener(idRolPrueba).Rows.Count = 1 AndAlso
                  UtileriasUI.Booleano(roles.Obtener(idRolPrueba).Rows(0), "Activo"))

        Dim sufijoRolInactivo As String = Guid.NewGuid().ToString("N").Substring(0, 8)
        Dim altaRolInactivo As Respuesta = roles.Crear(
            "Rol inactivo " & sufijoRolInactivo, "Rol temporal para probar triggers", New List(Of String)())
        Dim idRolInactivo As Integer = 0
        For Each fila As DataRow In roles.Listar().Rows
            If UtileriasUI.Texto(fila, "Nombre") = "Rol inactivo " & sufijoRolInactivo Then
                idRolInactivo = UtileriasUI.Entero(fila, "ID")
                Exit For
            End If
        Next
        Dim desactivacionRolTemporal As Boolean = False
        If altaRolInactivo.Exito AndAlso idRolInactivo > 0 Then
            Try
                Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                    conexion.Open()
                    Using comando As New MySqlCommand(
                        "UPDATE Roles SET Activo = 0 WHERE ID = @id", conexion)
                        comando.Parameters.AddWithValue("@id", idRolInactivo)
                        comando.ExecuteNonQuery()
                    End Using
                End Using
                desactivacionRolTemporal = True
            Catch
                desactivacionRolTemporal = False
            End Try
        End If
        Dim asignacionUsuarioInactivaBloqueada As Boolean = False
        If desactivacionRolTemporal Then
            Try
                Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                    conexion.Open()
                    Using comando As New MySqlCommand(
                        "UPDATE Usuarios SET ID_Rol = @rol WHERE ID = @id", conexion)
                        comando.Parameters.AddWithValue("@rol", idRolInactivo)
                        comando.Parameters.AddWithValue("@id", idUsuarioPrueba)
                        comando.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As MySqlException
                asignacionUsuarioInactivaBloqueada = ex.Number = 1644
            End Try
        End If
        Verificar("MySQL impide asignar por SQL directo usuarios a roles desactivados",
                  altaRolInactivo.Exito AndAlso desactivacionRolTemporal AndAlso
                  asignacionUsuarioInactivaBloqueada)
        Dim insercionUsuarioInactivaBloqueada As Boolean = False
        If desactivacionRolTemporal Then
            Try
                Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                    conexion.Open()
                    Using comando As New MySqlCommand(
                        "INSERT INTO Usuarios (Usuario, Nombre_Completo, Clave_Hash, Clave_Salt, ID_Rol, Activo) " &
                        "VALUES ('admin', 'Prueba', 'hash', 'salt', @rol, 1)", conexion)
                        comando.Parameters.AddWithValue("@rol", idRolInactivo)
                        comando.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As MySqlException
                insercionUsuarioInactivaBloqueada = ex.Number = 1644
            End Try
        End If
        Verificar("MySQL impide insertar por SQL directo usuarios con roles desactivados",
                  desactivacionRolTemporal AndAlso insercionUsuarioInactivaBloqueada)
        If idRolInactivo > 0 Then roles.Eliminar(idRolInactivo)

        Dim catalogoPermisosSoloLectura As Boolean = False
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
                Using comando As New MySqlCommand(
                    "UPDATE Permisos SET Descripcion = Descripcion WHERE Codigo = 'config.editar'",
                    conexion)
                    comando.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As MySqlException
            catalogoPermisosSoloLectura = ex.Number = 1142
        End Try
        Verificar("La cuenta de aplicación no puede alterar el catálogo maestro de permisos",
                  catalogoPermisosSoloLectura)
        Dim insercionPermisoAdminBloqueada As Boolean = False
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
                Using comando As New MySqlCommand(
                    "INSERT INTO Rol_Permiso (ID_Rol, Codigo) VALUES (@rol, 'config.usuarios')",
                    conexion)
                    comando.Parameters.AddWithValue("@rol", idRolGestor)
                    comando.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As MySqlException
            insercionPermisoAdminBloqueada = ex.Number = 1644
        End Try
        Verificar("MySQL bloquea la asignación SQL directa de config.usuarios a un rol ordinario",
                  insercionPermisoAdminBloqueada)
        Dim insercionRespaldoBloqueada As Boolean = False
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
                Using comando As New MySqlCommand(
                    "INSERT INTO Rol_Permiso (ID_Rol, Codigo) VALUES (@rol, 'config.respaldos')",
                    conexion)
                    comando.Parameters.AddWithValue("@rol", idRolGestor)
                    comando.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As MySqlException
            insercionRespaldoBloqueada = ex.Number = 1644
        End Try
        Verificar("MySQL bloquea la asignación SQL directa de cualquier permiso config.*",
                  insercionRespaldoBloqueada)
        Dim actualizacionPermisoAdminBloqueada As Boolean = False
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
                Using comando As New MySqlCommand(
                    "UPDATE Rol_Permiso SET Codigo = 'config.roles' WHERE ID_Rol = @rol AND Codigo = 'casos.ver'",
                    conexion)
                    comando.Parameters.AddWithValue("@rol", idRolGestor)
                    comando.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As MySqlException
            actualizacionPermisoAdminBloqueada = ex.Number = 1644
        End Try
        Verificar("MySQL bloquea la modificación SQL directa hacia config.roles",
                  actualizacionPermisoAdminBloqueada)
        Verificar("El rol ordinario conserva sus permisos originales tras los intentos SQL",
                  roles.PermisosDeRol(idRolGestor).Contains("casos.ver") AndAlso
                  Not roles.PermisosDeRol(idRolGestor).Contains("config.usuarios") AndAlso
                  Not roles.PermisosDeRol(idRolGestor).Contains("config.roles"))
        Dim borradoDirectoUsuariosBloqueado As Boolean = False
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
                Using comando As New MySqlCommand(
                    "DELETE FROM Usuarios WHERE ID = @id", conexion)
                    comando.Parameters.AddWithValue("@id", idUsuarioGestor)
                    comando.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As MySqlException
            borradoDirectoUsuariosBloqueado = ex.Number = 1142
        End Try
        Verificar("MySQL deniega DELETE directo sobre Usuarios a la cuenta de aplicación",
                  borradoDirectoUsuariosBloqueado)
        Verificar("Un usuario ordinario no puede crear roles ni permisos",
                  Not roles.Crear("Rol no autorizado " & sufijoGestor, "Prueba", New List(Of String) From {"casos.ver"}).Exito)
        Verificar("Un usuario ordinario no puede modificar roles ni permisos",
                  Not roles.Actualizar(idRolPrueba, "Rol de prueba editado", "Descripción editada",
                      True, New List(Of String) From {"casos.ver"}).Exito)
        Verificar("Un usuario ordinario no puede eliminar roles",
                  Not roles.Eliminar(idRolPrueba).Exito)
        Verificar("El repositorio también bloquea la creación directa de roles por usuarios ordinarios",
                  Not New RolRepositorio().Crear("Rol directo no autorizado " & sufijoGestor,
                      "Prueba", New List(Of String) From {"casos.ver"}).Exito)
        Verificar("El repositorio también bloquea la modificación directa de roles por usuarios ordinarios",
                  Not New RolRepositorio().Actualizar(idRolPrueba, "Cambio directo no autorizado",
                      "Prueba", True, New List(Of String) From {"casos.ver"}).Exito)
        Verificar("El repositorio también bloquea la eliminación directa de roles por usuarios ordinarios",
                  Not New RolRepositorio().Eliminar(idRolPrueba).Exito)
        Verificar("El repositorio bloquea la modificación directa de una cuenta Administrador",
                  Not New UsuarioRepositorio().Actualizar(admin.ID, admin.NombreCompleto,
                      idRolAdmin, True).Exito)
        Verificar("El repositorio bloquea altas directas de usuarios sin permiso administrativo",
                  Not New UsuarioRepositorio().Crear("repo_usuario_no_autorizado_" & sufijoGestor,
                      "Alta directa no autorizada", "IntentoSeguro99!", idRolPrueba, True).Exito)
        Verificar("Un usuario ordinario no puede crear cuentas Administrador",
                  Not servicioUsuarios.Crear("admin_no_autorizado_" & sufijoGestor,
                      "Intento de elevación", "IntentoSeguro99!", idRolAdmin, True).Exito)
        Verificar("Un usuario ordinario no puede promover otra cuenta a Administrador",
                  Not servicioUsuarios.Actualizar(idUsuarioPrueba, "Usuario de Prueba",
                      idRolAdmin, True).Exito)
        Verificar("Un usuario ordinario no puede modificar la cuenta Administrador",
                  Not servicioUsuarios.Actualizar(admin.ID, admin.NombreCompleto, idRolAdmin, True).Exito)
        Verificar("Un usuario ordinario no puede desactivar la cuenta Administrador",
                  Not servicioUsuarios.Desactivar(admin.ID).Exito)
        Verificar("Un usuario ordinario no puede restablecer la clave del Administrador",
                  Not servicioUsuarios.RestablecerClave(admin.ID, "ClaveTemporal99!").Exito)
        Verificar("El repositorio bloquea el restablecimiento directo de la clave Administrador",
                  Not New UsuarioRepositorio().CambiarClave(admin.ID, "ClaveTemporal99!").Exito)
        Verificar("El repositorio bloquea restablecer directamente la clave de otro usuario ordinario",
                  Not New UsuarioRepositorio().CambiarClave(idUsuarioPrueba, "ClaveTemporal99!").Exito)
        Verificar("El repositorio también bloquea el alta directa de cuentas Administrador",
                  Not New UsuarioRepositorio().Crear("repo_admin_" & sufijoGestor,
                      "Intento directo de elevación", "IntentoSeguro99!", idRolAdmin, True).Exito)
        Verificar("El repositorio también bloquea la promoción directa a Administrador",
                  Not New UsuarioRepositorio().Actualizar(idUsuarioPrueba, "Usuario de Prueba",
                      idRolAdmin, True).Exito)
        Sesion.UsuarioActual = admin
        Sesion.CargarPermisos()
        If idUsuarioGestor > 0 Then
            Verificar("La cuenta temporal de gestor se limpia al terminar la prueba",
                      servicioUsuarios.EliminarDefinitivamente(idUsuarioGestor).Exito)
        End If
        If idRolGestor > 0 Then
            Verificar("El rol temporal de gestor se limpia al terminar la prueba",
                      roles.Eliminar(idRolGestor).Exito)
        End If

        Dim actualizarSinCambios As Respuesta = servicioUsuarios.Actualizar(
            idUsuarioPrueba, "Usuario de Prueba", idRolPrueba, True)
        Verificar("Guardar un usuario sin modificar sus datos no se reporta como inexistente",
                  actualizarSinCambios.Exito, actualizarSinCambios.Mensaje)

        Dim claveUsuarioInexistente As Respuesta = usuarios.CambiarClave(
            Integer.MaxValue, "ClavePrueba123")
        Verificar("Cambiar clave de un usuario inexistente se rechaza",
                  Not claveUsuarioInexistente.Exito)

        Dim sesionPrueba As UsuarioSesion = usuarios.Autenticar("pruebauser", "PruebaSegura123!")
        Verificar("El usuario nuevo puede iniciar sesión", sesionPrueba IsNot Nothing)
        Verificar("La sesión del usuario nuevo trae su rol y no es protegido",
                  sesionPrueba IsNot Nothing AndAlso
                  sesionPrueba.IDRol = idRolPrueba AndAlso Not sesionPrueba.RolProtegido)

        Verificar("Un nombre de acceso demasiado corto se rechaza",
                  Not servicioUsuarios.Crear("ab", "X", "PruebaSegura123!", idRolPrueba, True).Exito)
        Verificar("Una clave que no cumple la política robusta se rechaza",
                  Not servicioUsuarios.Crear("otro.usuario", "X", "corta", idRolPrueba, True).Exito)
        Verificar("Un usuario duplicado se rechaza con mensaje",
                  Not servicioUsuarios.Crear("pruebauser", "X", "PruebaSegura123!", idRolPrueba, True).Exito)

        Verificar("El rol con usuarios asignados no se puede desactivar",
                  Not roles.Actualizar(idRolPrueba, "Rol de prueba editado", "x", False,
                      New List(Of String) From {"casos.ver"}).Exito)
        Verificar("El rol con usuarios asignados no se puede eliminar",
                  Not roles.Eliminar(idRolPrueba).Exito)

        ' Sin sesión activa para que el candado de "no desactivar el usuario
        ' con el que se inició sesión" no enmascare la prueba del último admin.
        Sesion.UsuarioActual = Nothing
        Dim rDesactivarAdmin As Respuesta = servicioUsuarios.Desactivar(admin.ID)
        Verificar("El servicio impide desactivar al único administrador",
                  Not rDesactivarAdmin.Exito AndAlso
                  rDesactivarAdmin.Mensaje.ToLower().Contains("administrador"), rDesactivarAdmin.Mensaje)
        Dim rCambiarRolAdmin As Respuesta = servicioUsuarios.Actualizar(
            admin.ID, admin.NombreCompleto, idRolAbogado, True)
        Verificar("El servicio impide cambiar el rol del único administrador",
                  Not rCambiarRolAdmin.Exito, rCambiarRolAdmin.Mensaje)
        Sesion.UsuarioActual = admin
        Sesion.CargarPermisos()

        ' Una cuenta recién creada, sin referencias ni actividad propia, sí
        ' puede eliminarse; el evento de alta y el de eliminación permanecen.
        Dim altaBorrable As Respuesta = servicioUsuarios.Crear(
            "borrable", "Cuenta desechable", "Borrable123*", idRolPrueba, True)
        Dim idBorrable As Integer = 0
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = "borrable" Then
                idBorrable = UtileriasUI.Entero(fila, "ID")
            End If
        Next
        Verificar("Crear cuenta temporal para probar eliminación segura",
                  altaBorrable.Exito AndAlso idBorrable > 0, altaBorrable.Mensaje)
        Dim eliminacionBorrable As Respuesta = servicioUsuarios.EliminarDefinitivamente(idBorrable)
        Verificar("El administrador puede eliminar una cuenta sin historial",
                  eliminacionBorrable.Exito, eliminacionBorrable.Mensaje)
        Verificar("La bitácora conserva los eventos de alta y eliminación de la cuenta",
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Usuarios' " &
                                   "AND ID_Registro = " & idBorrable.ToString() &
                                   " AND Accion IN ('CREAR','ELIMINAR')") = 2)
        Dim cuentaBorrableExiste As Boolean = False
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = "borrable" Then cuentaBorrableExiste = True
        Next
        Verificar("La cuenta temporal ya no aparece después de eliminarla",
                  Not cuentaBorrableExiste)

        Dim rAdmin2 As Respuesta = servicioUsuarios.Crear(
            "admin2", "Administrador de Prueba", "AdminSecundario123!", idRolAdmin, True)
        Verificar("Crear un segundo administrador", rAdmin2.Exito, rAdmin2.Mensaje)

        Dim idAdmin2 As Integer = 0
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = "admin2" Then idAdmin2 = UtileriasUI.Entero(fila, "ID")
        Next
        Dim sesionAdmin2 As UsuarioSesion = usuarios.Autenticar("admin2", "AdminSecundario123!")
        Verificar("El segundo administrador inicia sesión con rol protegido",
                  sesionAdmin2 IsNot Nothing AndAlso sesionAdmin2.RolProtegido)

        Verificar("Con dos administradores activos se puede cambiar el rol de uno",
                  servicioUsuarios.Actualizar(idAdmin2, "Administrador de Prueba", idRolAbogado, True).Exito)
        Verificar("Al devolverle el rol de administrador vuelve a ser protegido",
                  servicioUsuarios.Actualizar(idAdmin2, "Administrador de Prueba", idRolAdmin, True).Exito AndAlso
                  usuarios.Autenticar("admin2", "AdminSecundario123!").RolProtegido)

        ' ---------- 2c. Candados del lado de la base (triggers) ----------
        Console.WriteLine()
        Console.WriteLine("-- 2c. Candados de la base de datos (triggers)")

        ' admin2 queda inactivo por SQL (permitido: admin sigue activo).
        Verificar("Se puede desactivar un administrador cuando queda otro activo",
                  EjecutarSqlDirecto("UPDATE Usuarios SET Activo = 0 WHERE ID = " & idAdmin2.ToString()) = 1)

        Verificar("El trigger impide desactivar al último administrador activo",
                  NumeroErrorSql("UPDATE Usuarios SET Activo = 0 WHERE ID = " & admin.ID.ToString()) = 1644)
        Verificar("El trigger impide quitarle el rol al último administrador activo",
                  NumeroErrorSql("UPDATE Usuarios SET ID_Rol = " & idRolAbogado.ToString() &
                                 " WHERE ID = " & admin.ID.ToString()) = 1644)
        Verificar("El trigger impide eliminar al último administrador activo",
                  NumeroErrorSql("DELETE FROM Usuarios WHERE ID = " & admin.ID.ToString()) = 1644)

        EjecutarSqlDirecto("UPDATE Usuarios SET Activo = 1 WHERE ID = " & idAdmin2.ToString())

        ' Dos estaciones intentan desactivar a los dos administradores al mismo
        ' tiempo. El bloqueo común del rol debe permitir solo una operación.
        Dim inicioDesactivacion As New ManualResetEvent(False)
        Dim desactivacionesExitosas As Integer = 0
        Dim tareaDesactivarAdmin As New Thread(New ThreadStart(
            Sub()
                inicioDesactivacion.WaitOne()
                Try
                    If EjecutarSqlDirecto("UPDATE Usuarios SET Activo = 0 WHERE ID = " & admin.ID.ToString()) = 1 Then
                        Interlocked.Increment(desactivacionesExitosas)
                    End If
                Catch ex As MySqlException
                    ' El trigger debe rechazar la segunda desactivación.
                End Try
            End Sub))
        Dim tareaDesactivarAdmin2 As New Thread(New ThreadStart(
            Sub()
                inicioDesactivacion.WaitOne()
                Try
                    If EjecutarSqlDirecto("UPDATE Usuarios SET Activo = 0 WHERE ID = " & idAdmin2.ToString()) = 1 Then
                        Interlocked.Increment(desactivacionesExitosas)
                    End If
                Catch ex As MySqlException
                    ' El trigger debe rechazar la segunda desactivación.
                End Try
            End Sub))
        tareaDesactivarAdmin.Start()
        tareaDesactivarAdmin2.Start()
        inicioDesactivacion.Set()
        tareaDesactivarAdmin.Join()
        tareaDesactivarAdmin2.Join()
        inicioDesactivacion.Dispose()
        Verificar("Dos desactivaciones simultáneas conservan al menos un administrador",
                  desactivacionesExitosas = 1 AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Usuarios u INNER JOIN Roles r ON r.ID = u.ID_Rol WHERE r.Protegido = 1 AND u.Activo = 1") = 1)
        EjecutarSqlDirecto("UPDATE Usuarios SET Activo = 1 WHERE ID IN (" &
                           admin.ID.ToString() & ", " & idAdmin2.ToString() & ")")

        Verificar("El trigger impide renombrar el rol Administrador",
                  NumeroErrorSql("UPDATE Roles SET Nombre = 'Administrador X' WHERE ID = " &
                                 idRolAdmin.ToString()) = 1644)
        Verificar("El trigger impide desactivar el rol Administrador",
                  NumeroErrorSql("UPDATE Roles SET Activo = 0 WHERE ID = " & idRolAdmin.ToString()) = 1644)
        Verificar("El trigger impide desproteger el rol Administrador",
                  NumeroErrorSql("UPDATE Roles SET Protegido = 0 WHERE ID = " & idRolAdmin.ToString()) = 1644)
        Verificar("El trigger impide elevar un rol ordinario a Administrador protegido",
                  NumeroErrorSql("UPDATE Roles SET Protegido = 1 WHERE ID = " & idRolAbogado.ToString()) = 1644)
        Verificar("El trigger impide insertar un segundo rol protegido por SQL directo",
                  NumeroErrorSql("INSERT INTO Roles (Nombre, Descripcion, Activo, Protegido) " &
                                 "VALUES ('PruebaElevacionProtegida', 'Prueba de seguridad', 1, 1)") = 1644)
        Verificar("El trigger impide eliminar el rol Administrador",
                  NumeroErrorSql("DELETE FROM Roles WHERE ID = " & idRolAdmin.ToString()) = 1644)
        Verificar("El trigger impide quitar config.roles al Administrador",
                  NumeroErrorSql("DELETE FROM Rol_Permiso WHERE ID_Rol = " & idRolAdmin.ToString() &
                                 " AND Codigo = 'config.roles'") = 1644)
        Verificar("El trigger impide quitar config.usuarios al Administrador",
                  NumeroErrorSql("DELETE FROM Rol_Permiso WHERE ID_Rol = " & idRolAdmin.ToString() &
                                 " AND Codigo = 'config.usuarios'") = 1644)
        Verificar("El trigger permite quitar permisos que no son mínimos",
                  EjecutarSqlDirecto("DELETE FROM Rol_Permiso WHERE ID_Rol = " & idRolAdmin.ToString() &
                                     " AND Codigo = 'auditoria.exportar'") = 1)
        EjecutarSqlDirecto("INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo) VALUES (" &
                           idRolAdmin.ToString() & ", 'auditoria.exportar')")

        ' ---------- 2d. Limpieza lógica de los datos de seguridad temporales ----------
        ' La cuenta de aplicación no tiene DELETE sobre Usuarios por diseño.
        ' Las cuentas con eventos de auditoría se conservan inactivas y fuera
        ' del rol temporal; la base aislada se recrea en la siguiente corrida.
        Dim limpiezaUsuarioPrueba As Respuesta = servicioUsuarios.Actualizar(
            idUsuarioPrueba, "Usuario de Prueba", idRolAbogado, False)
        Verificar("La limpieza conserva la cuenta de prueba y su trazabilidad",
                  idUsuarioPrueba > 0 AndAlso limpiezaUsuarioPrueba.Exito,
                  limpiezaUsuarioPrueba.Mensaje)
        Verificar("El rol de prueba se elimina después de reasignar su cuenta",
                  roles.Eliminar(idRolPrueba).Exito)
        Dim limpiezaAdmin2 As Respuesta = servicioUsuarios.Actualizar(
            idAdmin2, "Administrador de Prueba", idRolAbogado, False)
        Verificar("La limpieza conserva inactivo al administrador temporal con su historial",
                  idAdmin2 > 0 AndAlso limpiezaAdmin2.Exito,
                  limpiezaAdmin2.Mensaje)

        Sesion.UsuarioActual = admin
        Sesion.CargarPermisos()
        Verificar("La sesión del administrador queda restablecida", Permisos.AccesoTotal)

        ' ---------- 3. Clientes ----------
        Console.WriteLine()
        Console.WriteLine("-- 3. Clientes y perfil LDA/FT")
        Dim clientes As New ServicioClientes()
        Dim cliente As New Cliente With {
            .Nombres = "Ana María",
            .Apellidos = "Pérez López",
            .DUI = "00000000-0",
            .NIT = "06140108081001",
            .Nacionalidad = "Salvadoreña",
            .Profesion = "Contadora pública",
            .FechaNacimiento = New Date(1985, 3, 14),
            .Direccion = "Col. San Benito, San Salvador",
            .Telefono = "2222-0000",
            .Email = "ana.perez@ejemplo.com"
        }
        Dim perfil As New PerfilLdaft With {
            .NivelRiesgo = "Medio",
            .OrigenFondosVerificado = True,
            .DetalleOrigenFondos = "Salario y honorarios profesionales",
            .FechaVerificacionListas = Date.Today
        }

        Dim clienteDirectoInvalidoRechazado As Boolean = False
        Try
            repositorioClientesDirecto.GuardarCompleto(New Cliente With {
                .Nombres = "", .Apellidos = "Apellido", .Activo = True}, Nothing)
        Catch ex As ArgumentException
            clienteDirectoInvalidoRechazado = True
        End Try
        Verificar("El repositorio de clientes rechaza directamente nombres obligatorios vacíos",
                  clienteDirectoInvalidoRechazado)

        Dim idCliente As Integer = 0
        Dim rCliente As Respuesta = clientes.Guardar(cliente, perfil)
        Verificar("Guardar cliente válido", rCliente.Exito, rCliente.Mensaje)
        If rCliente.ID.HasValue Then idCliente = rCliente.ID.Value

        Dim tablaCliente As DataTable = clientes.Obtener(idCliente)
        Verificar("El cliente se recupera por ID", tablaCliente.Rows.Count = 1)
        Dim cambioEstadoClienteDirectoRechazado As Boolean = False
        Try
            cliente.Activo = False
            repositorioClientesDirecto.GuardarCompleto(cliente, perfil)
        Catch ex As InvalidOperationException
            cambioEstadoClienteDirectoRechazado = True
        Finally
            cliente.Activo = True
        End Try
        Dim clienteTrasCambioEstado As DataTable = clientes.Obtener(idCliente)
        Verificar("La edición directa del cliente no puede desactivarlo ni saltarse la operación autorizada",
                  cambioEstadoClienteDirectoRechazado AndAlso clienteTrasCambioEstado.Rows.Count = 1 AndAlso
                  UtileriasUI.Booleano(clienteTrasCambioEstado.Rows(0), "Activo"))
        If tablaCliente.Rows.Count = 1 Then
            Verificar("El DUI se normaliza al formato 00000000-0",
                      Convert.ToString(tablaCliente.Rows(0)("DUI")) = "00000000-0")
        Dim clienteInactivoPrueba As New Cliente With {
            .Nombres = "Cliente", .Apellidos = "Inactivo", .Activo = True}
        Dim altaClienteInactivo As Respuesta = clientes.Guardar(clienteInactivoPrueba, Nothing)
        Dim clienteInactivoId As Integer = If(altaClienteInactivo.ID.HasValue, altaClienteInactivo.ID.Value, 0)
        Dim bajaClienteInactivo As Respuesta = clientes.Desactivar(clienteInactivoId)
        Dim casoClienteInactivoRechazado As Boolean = False
        Try
            repositorioCasosDirecto.Insertar(New Caso With {
                .IDCliente = clienteInactivoId, .NumeroExpediente = "INACTIVO-CLIENTE-TEST",
                .Materia = "Civil", .Estado = "Activo", .FechaApertura = Date.Today})
        Catch ex As InvalidOperationException
            casoClienteInactivoRechazado = True
        End Try
        Verificar("El repositorio no permite abrir casos para clientes inactivos",
                  altaClienteInactivo.Exito AndAlso bajaClienteInactivo.Exito AndAlso
                  casoClienteInactivoRechazado)
            Verificar("El NIT se normaliza con separadores",
                      Convert.ToString(tablaCliente.Rows(0)("NIT")).Contains("-"))
            Verificar("El perfil LDA/FT quedó guardado con su nivel de riesgo",
                      Convert.ToString(tablaCliente.Rows(0)("Nivel_Riesgo")) = "Medio")
        End If
        Verificar("La búsqueda por apellido encuentra al cliente",
                  clientes.Buscar("Pérez").Rows.Count >= 1)
        Verificar("La búsqueda sin filtro lista los clientes (grid inicial)",
                  clientes.Buscar("").Rows.Count >= 1)
        Verificar("Desactivar un cliente inexistente no se reporta como éxito",
                  Not clientes.Desactivar(Integer.MaxValue).Exito)

        Verificar("DUI con menos de 9 dígitos se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y", .DUI = "123"}, Nothing).Exito)
        Verificar("NIT con menos de 14 dígitos se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y", .NIT = "123"}, Nothing).Exito)
        Verificar("DUI con letras intercaladas se rechaza en el servicio",
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "X", .Apellidos = "Y", .DUI = "ABC000000-0"
                  }, Nothing).Exito)
        Verificar("NIT con letras intercaladas se rechaza en el servicio",
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "X", .Apellidos = "Y", .NIT = "ABCD06140108081001"
                  }, Nothing).Exito)
        Verificar("El dígito verificador DUI rechaza una distribución incorrecta del guion",
                  Not ValidadorIdentidad.EsDigitoDuiValido("0-00000000"))
        Verificar("Email sin arroba se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y", .Email = "correo"}, Nothing).Exito)
        Verificar("Nombres vacíos se rechazan",
                  Not clientes.Guardar(New Cliente With {.Apellidos = "Y"}, Nothing).Exito)
        Verificar("El servicio rechaza un cliente nulo sin excepción",
                  Not clientes.Guardar(Nothing, Nothing).Exito)
        Verificar("Los campos del cliente respetan los tamaños de la base de datos",
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = New String("N"c, 121), .Apellidos = "Prueba"
                  }, Nothing).Exito AndAlso
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Prueba", .Direccion = New String("D"c, 301)
                  }, Nothing).Exito AndAlso
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Prueba", .Email = New String("e"c, 121) & "@x.com"
                  }, Nothing).Exito)
        Verificar("Las fechas futuras de nacimiento y verificación LDA/FT se rechazan",
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Fecha", .FechaNacimiento = Date.Today.AddDays(1)
                  }, Nothing).Exito AndAlso
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Fecha"
                  }, New PerfilLdaft With {
                      .NivelRiesgo = "Bajo", .FechaVerificacionListas = Date.Today.AddDays(1)
                  }).Exito)
        Verificar("Las fechas anteriores al rango DATE de MySQL se rechazan en clientes y perfiles",
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Fecha", .FechaNacimiento = Date.MinValue
                  }, Nothing).Exito AndAlso
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Fecha"
                  }, New PerfilLdaft With {
                      .NivelRiesgo = "Bajo", .FechaVerificacionListas = Date.MinValue
                  }).Exito)
        Verificar("El detalle y observaciones LDA/FT respetan el límite de 500 caracteres",
                  Not clientes.Guardar(New Cliente With {
                      .Nombres = "Prueba", .Apellidos = "Perfil"
                  }, New PerfilLdaft With {
                      .NivelRiesgo = "Bajo", .DetalleOrigenFondos = New String("O"c, 501)
                  }).Exito)
        Verificar("Nivel de riesgo fuera de catálogo se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y"},
                                       New PerfilLdaft With {.NivelRiesgo = "Extremo"}).Exito)

        Dim excepcionDui As String = ""
        Dim rDuplicadoDui As Respuesta = Nothing
        Try
            rDuplicadoDui = clientes.Guardar(
                New Cliente With {.Nombres = "Otro", .Apellidos = "Cliente", .DUI = "00000000-0"}, Nothing)
        Catch ex As Exception
            excepcionDui = ex.Message
        End Try
        Verificar("DUI duplicado se rechaza con mensaje de negocio (no excepción)",
                  excepcionDui = "" AndAlso rDuplicadoDui IsNot Nothing AndAlso Not rDuplicadoDui.Exito,
                  If(excepcionDui <> "", "EXCEPCIÓN SIN CONTROLAR: " & excepcionDui, rDuplicadoDui.Mensaje))

        Dim iniciarDuiConcurrente As New ManualResetEvent(False)
        Dim exitosDuiConcurrentes As Integer = 0
        Dim tareaDuiA As New Thread(New ThreadStart(
            Sub()
                iniciarDuiConcurrente.WaitOne()
                Dim resultado As Respuesta = clientes.Guardar(
                    New Cliente With {.Nombres = "Concurrente A", .Apellidos = "DUI", .DUI = "11111111-1"}, Nothing)
                If resultado.Exito Then Interlocked.Increment(exitosDuiConcurrentes)
            End Sub))
        Dim tareaDuiB As New Thread(New ThreadStart(
            Sub()
                iniciarDuiConcurrente.WaitOne()
                Dim resultado As Respuesta = clientes.Guardar(
                    New Cliente With {.Nombres = "Concurrente B", .Apellidos = "DUI", .DUI = "11111111-1"}, Nothing)
                If resultado.Exito Then Interlocked.Increment(exitosDuiConcurrentes)
            End Sub))
        tareaDuiA.Start()
        tareaDuiB.Start()
        iniciarDuiConcurrente.Set()
        tareaDuiA.Join()
        tareaDuiB.Join()
        Verificar("Dos altas concurrentes con el mismo DUI solo permiten una inserción",
                  exitosDuiConcurrentes = 1 AndAlso clientes.Buscar("11111111-1").Rows.Count = 1)

        Dim errorRollbackCliente As Integer = 0
        Dim detalleRollbackCliente As String = ""
        Try
            Dim repoAltaRollbackCliente As New ClienteRepositorio()
            repoAltaRollbackCliente.GuardarCompleto(
                New Cliente With {.Nombres = "RollbackClienteLDAFT", .Apellidos = "Prueba"},
                New PerfilLdaft With {.NivelRiesgo = "Extremo"})
        Catch ex As MySqlException
            errorRollbackCliente = ex.Number
            detalleRollbackCliente = ex.Message
        Catch ex As Exception
            detalleRollbackCliente = ex.Message
        End Try
        Verificar("Si falla el perfil LDA/FT, también se revierte el cliente y su auditoría",
                  errorRollbackCliente = 3819 AndAlso clientes.Buscar("RollbackClienteLDAFT").Rows.Count = 0 AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Clientes' " &
                                   "AND Accion = 'CREAR' AND Detalle LIKE '%RollbackClienteLDAFT%'") = 0,
                  detalleRollbackCliente)

        Dim errorRollbackActualizacionCliente As Integer = 0
        Try
            Dim repoEdicionRollbackCliente As New ClienteRepositorio()
            repoEdicionRollbackCliente.GuardarCompleto(
                New Cliente With {.ID = idCliente, .Nombres = "Nombre temporal", .Apellidos = "No persistir"},
                New PerfilLdaft With {.NivelRiesgo = "Extremo"})
        Catch ex As MySqlException
            errorRollbackActualizacionCliente = ex.Number
        End Try
        Dim clienteTrasRollback As DataTable = clientes.Obtener(idCliente)
        Verificar("La actualización del cliente también se revierte si falla su perfil",
                  errorRollbackActualizacionCliente = 3819 AndAlso clienteTrasRollback.Rows.Count = 1 AndAlso
                  Convert.ToString(clienteTrasRollback.Rows(0)("Nombres")) = "Ana María" AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Clientes' " &
                                   "AND Accion = 'MODIFICAR' AND Detalle LIKE '%Nombre temporal%'") = 0)

        ' ---------- 4. Entidades legales ----------
        Console.WriteLine()
        Console.WriteLine("-- 4. Entidades legales y representantes")
        Dim entidades As New ServicioEntidades()
        Dim entidad As New EntidadLegal With {
            .RazonSocial = "Inversiones Cuscatlán S.A. de C.V.",
            .NIT = "06140108081002",
            .NRC = "123456-7",
            .Domicilio = "Torre Cuscatlán, San Salvador"
        }
        Dim rEntidad As Respuesta = entidades.Guardar(entidad, New PerfilLdaft With {.NivelRiesgo = "Bajo"})
        Verificar("Guardar entidad válida con perfil", rEntidad.Exito, rEntidad.Mensaje)
        Dim idEntidad As Integer = 0
        If rEntidad.ID.HasValue Then idEntidad = rEntidad.ID.Value

        Dim cambioEstadoEntidadDirectoRechazado As Boolean = False
        Try
            entidad.Activo = False
            repositorioEntidadesDirecto.GuardarCompleto(entidad, Nothing)
        Catch ex As InvalidOperationException
            cambioEstadoEntidadDirectoRechazado = True
        Finally
            entidad.Activo = True
        End Try
        Dim entidadTrasCambioEstado As DataTable = entidades.Obtener(idEntidad)
        Verificar("La edición general no puede desactivar directamente una entidad",
                  cambioEstadoEntidadDirectoRechazado AndAlso entidadTrasCambioEstado.Rows.Count = 1 AndAlso
                  UtileriasUI.Booleano(entidadTrasCambioEstado.Rows(0), "Activo"))

        Verificar("NIT inválido en entidad se rechaza",
                  Not entidades.Guardar(New EntidadLegal With {.RazonSocial = "X", .NIT = "12"}, Nothing).Exito)
        Verificar("La fecha de verificación LDA/FT fuera del rango DATE se rechaza en entidades",
                  Not entidades.Guardar(New EntidadLegal With {.RazonSocial = "Entidad fecha inválida"},
                      New PerfilLdaft With {
                          .NivelRiesgo = "Bajo", .FechaVerificacionListas = Date.MinValue
                      }).Exito)
        Verificar("Razón social vacía se rechaza",
                  Not entidades.Guardar(New EntidadLegal(), Nothing).Exito)
        Dim entidadDirectaInvalidaRechazada As Boolean = False
        Try
            repositorioEntidadesDirecto.GuardarCompleto(New EntidadLegal With {
                .RazonSocial = "", .Activo = True}, Nothing)
        Catch ex As ArgumentException
            entidadDirectaInvalidaRechazada = True
        End Try
        Verificar("El repositorio de entidades rechaza directamente razones sociales vacías",
                  entidadDirectaInvalidaRechazada)

        Dim errorRollbackEntidad As Integer = 0
        Dim detalleRollbackEntidad As String = ""
        Try
            Dim repoAltaRollbackEntidad As New EntidadRepositorio()
            repoAltaRollbackEntidad.GuardarCompleto(
                New EntidadLegal With {.RazonSocial = "RollbackEntidadLDAFT"},
                New PerfilLdaft With {.NivelRiesgo = "Extremo"})
        Catch ex As MySqlException
            errorRollbackEntidad = ex.Number
            detalleRollbackEntidad = ex.Message
        Catch ex As Exception
            detalleRollbackEntidad = ex.Message
        End Try
        Verificar("Si falla el perfil LDA/FT, también se revierte la entidad y su auditoría",
                  errorRollbackEntidad = 3819 AndAlso entidades.Buscar("RollbackEntidadLDAFT").Rows.Count = 0 AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Entidades_Legales' " &
                                   "AND Accion = 'CREAR' AND Detalle LIKE '%RollbackEntidadLDAFT%'") = 0,
                  detalleRollbackEntidad)

        Dim errorRollbackActualizacionEntidad As Integer = 0
        Try
            Dim repoEdicionRollbackEntidad As New EntidadRepositorio()
            repoEdicionRollbackEntidad.GuardarCompleto(
                New EntidadLegal With {.ID = idEntidad, .RazonSocial = "Razón temporal"},
                New PerfilLdaft With {.NivelRiesgo = "Extremo"})
        Catch ex As MySqlException
            errorRollbackActualizacionEntidad = ex.Number
        End Try
        Dim entidadTrasRollback As DataTable = entidades.Obtener(idEntidad)
        Verificar("La actualización de entidad también se revierte si falla su perfil",
                  errorRollbackActualizacionEntidad = 3819 AndAlso entidadTrasRollback.Rows.Count = 1 AndAlso
                  Convert.ToString(entidadTrasRollback.Rows(0)("Razon_Social")) = "Inversiones Cuscatlán S.A. de C.V." AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Entidades_Legales' " &
                                   "AND Accion = 'MODIFICAR' AND Detalle LIKE '%Razón temporal%'") = 0)

        Dim entidadSinPerfil As New EntidadLegal With {
            .RazonSocial = "Entidad sin perfil para validar titular",
            .NIT = "06140108081003"
        }
        Dim rEntidadSinPerfil As Respuesta = entidades.Guardar(entidadSinPerfil, Nothing)
        Verificar("Crear entidad sin perfil para prueba de titular exclusivo", rEntidadSinPerfil.Exito)
        If rEntidadSinPerfil.ID.HasValue Then
            Dim errorTitularDoble As Integer = NumeroErrorSql(
                "UPDATE Perfil_LDAFT SET ID_Entidad = " & rEntidadSinPerfil.ID.Value.ToString() &
                " WHERE ID_Cliente = " & idCliente.ToString())
            Verificar("Un perfil LDA/FT no puede pertenecer a cliente y entidad a la vez",
                      errorTitularDoble = 3819, "Error MySQL: " & errorTitularDoble.ToString())
        Else
            Verificar("Un perfil LDA/FT no puede pertenecer a cliente y entidad a la vez", False,
                      "No se pudo crear la entidad auxiliar.")
        End If

        Dim rRep As Respuesta = entidades.AgregarRepresentante(idEntidad, idCliente, "Presidenta de la junta directiva", "Judicial")
        Verificar("Agregar representante a la entidad", rRep.Exito, rRep.Mensaje)
        Verificar("El servicio rechaza tipos de representación fuera del catálogo",
                  Not entidades.AgregarRepresentante(idEntidad, idCliente, "Representante", "Inventado").Exito)
        Verificar("El repositorio también rechaza tipos de representación fuera del catálogo",
                  Not repositorioEntidadesDirecto.AgregarRepresentante(
                      idEntidad, idCliente, "Representante", "Inventado").Exito)
        Dim rRepDup As Respuesta = entidades.AgregarRepresentante(idEntidad, idCliente, "Auditor", "Judicial")
        Verificar("Representante duplicado se rechaza con mensaje",
                  Not rRepDup.Exito AndAlso rRepDup.Mensaje.Length > 0, rRepDup.Mensaje)
        Verificar("El listado de representantes muestra al agregado",
                  entidades.Representantes(idEntidad).Rows.Count = 1)
        Verificar("No se altera la identidad de una entidad con representantes históricos",
                  Not entidades.Guardar(New EntidadLegal With {
                      .ID = idEntidad, .RazonSocial = "Razón Social Modificada",
                      .NIT = entidad.NIT, .NRC = entidad.NRC,
                      .Domicilio = entidad.Domicilio, .Activo = True}, Nothing).Exito)
        Verificar("Se permite actualizar el domicilio sin cambiar la identidad legal",
                  entidades.Guardar(New EntidadLegal With {
                      .ID = idEntidad, .RazonSocial = entidad.RazonSocial,
                      .NIT = entidad.NIT, .NRC = entidad.NRC,
                      .Domicilio = "Domicilio actualizado de prueba", .Activo = True}, Nothing).Exito)
        Verificar("La búsqueda de entidades sin filtro lista resultados",
                  entidades.Buscar("").Rows.Count >= 1)
        Dim representantesAntesQuitar As DataTable = entidades.Representantes(idEntidad)
        Dim idRepresentantePrueba As Integer = 0
        If representantesAntesQuitar.Rows.Count > 0 Then
            idRepresentantePrueba = UtileriasUI.Entero(representantesAntesQuitar.Rows(0), "ID")
        End If
        Dim quitarRepresentantePrueba As Respuesta = entidades.QuitarRepresentante(idRepresentantePrueba)
        Verificar("Al retirar un representante se conserva su identidad y entidad en la bitácora",
                  idRepresentantePrueba > 0 AndAlso quitarRepresentantePrueba.Exito AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Representantes_Legales' " &
                                   "AND ID_Registro = " & idRepresentantePrueba.ToString() &
                                   " AND Accion = 'ELIMINAR' AND Detalle LIKE '%Ana María%' " &
                                   "AND Detalle LIKE '%Inversiones Cuscatlán%'") = 1)
        Verificar("Desactivar una entidad inexistente no se reporta como éxito",
                  Not entidades.Desactivar(Integer.MaxValue).Exito)
        Verificar("Quitar un representante inexistente no se reporta como éxito",
                  Not entidades.QuitarRepresentante(Integer.MaxValue).Exito)

        ' ---------- 5. Casos ----------
        Console.WriteLine()
        Console.WriteLine("-- 5. Casos judiciales")
        Dim casos As New ServicioCasos()
        Verificar("El servicio de casos rechaza un objeto nulo sin excepción",
                  Not casos.Guardar(Nothing).Exito)
        Verificar("El servicio de casos valida longitudes y fechas antes de guardar",
                  Not casos.Guardar(New Caso With {
                      .IDCliente = idCliente, .NumeroExpediente = New String("E"c, 81),
                      .Materia = "Civil"
                  }).Exito AndAlso
                  Not casos.Guardar(New Caso With {
                      .IDCliente = idCliente, .NumeroExpediente = "FUTURO-1",
                      .Materia = "Civil", .FechaApertura = Date.Today.AddDays(1)
                  }).Exito AndAlso
                  Not casos.Guardar(New Caso With {
                      .IDCliente = idCliente, .NumeroExpediente = "ESTADO-1",
                      .Materia = "Civil", .Estado = "Desconocido"
                  }).Exito)
        Dim casoDirectoInvalidoRechazado As Boolean = False
        Try
            repositorioCasosDirecto.Insertar(New Caso With {
                .IDCliente = idCliente, .NumeroExpediente = New String("X"c, 81),
                .Materia = "Civil", .Estado = "Activo", .FechaApertura = Date.Today})
        Catch ex As ArgumentException
            casoDirectoInvalidoRechazado = True
        End Try
        Verificar("El repositorio de casos rechaza directamente expedientes fuera de longitud",
                  casoDirectoInvalidoRechazado)
        Dim idJuzgado As Integer = UtileriasUI.Entero(casos.JuzgadosCombo().Rows(0), "ID")
        Dim tipoContestacion As DataRow = Nothing
        For Each fila As DataRow In casos.TiposActuacionCombo().Rows
            If UtileriasUI.Texto(fila, "Nombre").StartsWith("Contestación") Then
                tipoContestacion = fila
                Exit For
            End If
        Next
        Verificar("El catálogo de tipos de actuación está sembrado", tipoContestacion IsNot Nothing)

        Dim caso As New Caso With {
            .IDCliente = idCliente,
            .NumeroExpediente = "123-2026",
            .Materia = "Civil",
            .IDJuzgado = idJuzgado,
            .Descripcion = "Juicio ordinario mercantil, reclamación de créditos",
            .Responsable = "Abg. Ana María Pérez"
        }
        Dim rCaso As Respuesta = casos.Guardar(caso)
        Verificar("Guardar caso válido", rCaso.Exito, rCaso.Mensaje)
        Dim idCaso As Integer = 0
        If rCaso.ID.HasValue Then idCaso = rCaso.ID.Value

        ' La referencia histórica en Casos.Responsable puede guardar el nombre
        ' completo y no el login; la eliminación debe detectar ambos formatos.
        Dim altaReferenciaNombre As Respuesta = servicioUsuarios.Crear(
            "refhist", "Abg. Ana María Pérez", "RefHist123*X", idRolPrueba, True)
        Dim idReferenciaNombre As Integer = 0
        For Each fila As DataRow In servicioUsuarios.Listar().Rows
            If UtileriasUI.Texto(fila, "Usuario") = "refhist" Then
                idReferenciaNombre = UtileriasUI.Entero(fila, "ID")
                Exit For
            End If
        Next
        Dim eliminacionReferenciaNombre As Respuesta =
            servicioUsuarios.EliminarDefinitivamente(idReferenciaNombre)
        Verificar("No se elimina una cuenta cuyo nombre completo figura como responsable histórico",
                  altaReferenciaNombre.Exito AndAlso idReferenciaNombre > 0 AndAlso
                  Not eliminacionReferenciaNombre.Exito,
                  eliminacionReferenciaNombre.Mensaje)

        Dim excepcionExp As String = ""
        Dim rDuplicadoExp As Respuesta = Nothing
        Try
            rDuplicadoExp = casos.Guardar(New Caso With {
                .IDCliente = idCliente, .NumeroExpediente = "123-2026",
                .Materia = "Civil", .IDJuzgado = idJuzgado})
        Catch ex As Exception
            excepcionExp = ex.Message
        End Try
        Verificar("Expediente duplicado en el mismo juzgado se rechaza con mensaje de negocio",
                  excepcionExp = "" AndAlso rDuplicadoExp IsNot Nothing AndAlso Not rDuplicadoExp.Exito,
                  If(excepcionExp <> "", "EXCEPCIÓN SIN CONTROLAR: " & excepcionExp, rDuplicadoExp.Mensaje))
        Verificar("Caso sin cliente se rechaza",
                  Not casos.Guardar(New Caso With {.NumeroExpediente = "X", .Materia = "Civil"}).Exito)
        Verificar("La búsqueda de casos encuentra el expediente",
                  casos.Buscar("123-2026").Rows.Count >= 1)
        Verificar("La búsqueda de casos sin filtro lista resultados",
                  casos.Buscar("").Rows.Count >= 1)
        Verificar("Actualizar un caso inexistente se rechaza",
                  Not casos.Guardar(New Caso With {
                      .ID = Integer.MaxValue, .IDCliente = idCliente,
                      .NumeroExpediente = "NO-EXISTE", .Materia = "Civil"
                  }).Exito)

        ' ---------- 6. Plazos (Art. 145 CPCM) ----------
        Console.WriteLine()
        Console.WriteLine("-- 6. Cálculo y confirmación de plazos")
        Dim plazos As New ServicioPlazos()
        Dim calculadora As CalculadoraDiasHabiles = plazos.ObtenerCalculadora()

        Verificar("Sábado no es día hábil", Not calculadora.EsDiaHabil(New Date(2026, 11, 7)))
        Verificar("Domingo no es día hábil", Not calculadora.EsDiaHabil(New Date(2026, 11, 8)))
        Verificar("Feriado (Día de los Difuntos 02/11/2026) no es día hábil",
                  Not calculadora.EsDiaHabil(New Date(2026, 11, 2)))
        Verificar("Lunes ordinario es día hábil", calculadora.EsDiaHabil(New Date(2026, 11, 9)))
        Verificar("Vencimiento a 1 día hábil desde viernes 06/11/2026 cae lunes 09/11/2026",
                  plazos.CalcularVencimiento(New Date(2026, 11, 6), 1) = New Date(2026, 11, 9))
        Verificar("Vencimiento a 15 días hábiles desde viernes 06/11/2026 cae viernes 27/11/2026",
                  plazos.CalcularVencimiento(New Date(2026, 11, 6), 15) = New Date(2026, 11, 27))
        Verificar("ContarDiasHabiles entre 06/11 y 27/11 da 15",
                  calculadora.ContarDiasHabiles(New Date(2026, 11, 6), New Date(2026, 11, 27)) = 15)
        Verificar("SiguienteDiaHabil del sábado 07/11 es lunes 09/11",
                  calculadora.SiguienteDiaHabil(New Date(2026, 11, 7)) = New Date(2026, 11, 9))

        Dim excepcionPlazo As String = ""
        Try
            plazos.CalcularVencimiento(New Date(2026, 11, 6), 0)
        Catch ex As ArgumentOutOfRangeException
            excepcionPlazo = ex.Message
        End Try
        Verificar("Plazo de cero días lanza error de argumento", excepcionPlazo <> "")

        Dim actuacion As New ActuacionProcesal With {
            .IDCaso = idCaso,
            .IDTipoActuacion = UtileriasUI.Entero(tipoContestacion, "ID"),
            .FechaNotificacion = New Date(2026, 11, 6),
            .PlazoLegalDias = 15,
            .Observaciones = "Notificación del traslado de la demanda"
        }
        Dim rPlazo As Respuesta = plazos.Confirmar(actuacion)
        Verificar("Confirmar plazo guarda la actuación", rPlazo.Exito, rPlazo.Mensaje)
        Dim idActuacion As Integer = actuacion.ID
        Dim actuacionDirectaForzadaPendiente As New ActuacionProcesal With {
            .IDCaso = idCaso, .IDTipoActuacion = UtileriasUI.Entero(tipoContestacion, "ID"),
            .FechaNotificacion = Date.Today, .PlazoLegalDias = 10,
            .FechaVencimiento = plazos.CalcularVencimiento(Date.Today, 10),
            .Estado = "Cumplido", .Observaciones = "Prueba de estado inicial protegido"}
        actuacionDirectaForzadaPendiente.ID =
            repositorioCasosDirecto.GuardarActuacion(actuacionDirectaForzadaPendiente)
        Dim actuacionDirectaTabla As DataTable =
            repositorioCasosDirecto.ActuacionesPorCaso(idCaso)
        Dim estadoDirecto As String = ""
        For Each fila As DataRow In actuacionDirectaTabla.Rows
            If UtileriasUI.Entero(fila, "ID") = actuacionDirectaForzadaPendiente.ID Then
                estadoDirecto = UtileriasUI.Texto(fila, "Estado")
                Exit For
            End If
        Next
        Verificar("El repositorio fuerza Pendiente al crear una actuación y no acepta estado enviado por el cliente",
                  actuacionDirectaForzadaPendiente.ID > 0 AndAlso estadoDirecto = "Pendiente")
        Dim auditoriaAntesReconfirmar As Integer = ContarSqlDirecto(
            "SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Actuaciones_Procesales' " &
            "AND ID_Registro = " & idActuacion.ToString() & " AND Accion = 'CONFIRMAR PLAZO'")
        Dim reconfirmacion As Respuesta = plazos.Confirmar(actuacion)
        Dim auditoriaDespuesReconfirmar As Integer = ContarSqlDirecto(
            "SELECT COUNT(*) FROM Audit_Log WHERE Tabla_Afectada = 'Actuaciones_Procesales' " &
            "AND ID_Registro = " & idActuacion.ToString() & " AND Accion = 'CONFIRMAR PLAZO'")
        Verificar("Cada confirmación explícita de plazo queda auditada incluso si no cambia los datos",
                  reconfirmacion.Exito AndAlso auditoriaDespuesReconfirmar > auditoriaAntesReconfirmar,
                  reconfirmacion.Mensaje)

        Dim tablaActuaciones As DataTable = plazos.Actuaciones(idCaso)
        Dim guardada As DataRow = Nothing
        For Each fila As DataRow In tablaActuaciones.Rows
            If UtileriasUI.Entero(fila, "ID") = idActuacion Then guardada = fila
        Next
        Verificar("La actuación confirmada queda en el expediente", guardada IsNot Nothing)
        Verificar("El número de expediente no se puede alterar después de registrar actuaciones",
                  Not casos.Guardar(New Caso With {
                      .ID = idCaso, .IDCliente = idCliente, .NumeroExpediente = "EXPEDIENTE-ALTERADO",
                      .Materia = caso.Materia, .IDJuzgado = idJuzgado, .Descripcion = caso.Descripcion,
                      .Estado = caso.Estado, .FechaApertura = caso.FechaApertura, .Responsable = caso.Responsable
                  }).Exito)
        Verificar("Actualizar una actuación inexistente se rechaza",
                  Not plazos.Confirmar(New ActuacionProcesal With {
                      .ID = Integer.MaxValue, .IDCaso = idCaso,
                      .IDTipoActuacion = actuacion.IDTipoActuacion,
                      .FechaNotificacion = New Date(2026, 11, 6),
                      .PlazoLegalDias = 5
                  }).Exito)
        If guardada IsNot Nothing Then
            Verificar("El vencimiento calculado es 27/11/2026",
                      UtileriasUI.Fecha(guardada, "Fecha_Vencimiento_Calculada") = New Date(2026, 11, 27))
            Verificar("El usuario que confirma queda registrado",
                      UtileriasUI.Texto(guardada, "Usuario_Confirma") = "admin")
        End If

        Verificar("Notificación futura se rechaza",
                  Not plazos.Confirmar(New ActuacionProcesal With {
                      .IDCaso = idCaso, .IDTipoActuacion = actuacion.IDTipoActuacion,
                      .FechaNotificacion = Date.Today.AddDays(1), .PlazoLegalDias = 5}).Exito)
        Verificar("Notificación sin fecha se rechaza con validación de negocio",
                  Not plazos.Confirmar(New ActuacionProcesal With {
                      .IDCaso = idCaso, .IDTipoActuacion = actuacion.IDTipoActuacion,
                      .FechaNotificacion = Date.MinValue, .PlazoLegalDias = 5}).Exito)

        Dim vencida As New ActuacionProcesal With {
            .IDCaso = idCaso,
            .IDTipoActuacion = actuacion.IDTipoActuacion,
            .FechaNotificacion = New Date(2026, 1, 5),
            .PlazoLegalDias = 3
        }
        plazos.Confirmar(vencida)
        Dim marcadas As Integer = plazos.ActualizarVencidas()
        Verificar("Las actuaciones vencidas se marcan al refrescar", marcadas >= 1,
                  "Se marcaron " & marcadas.ToString() & " actuaciones")

        Dim tablaActuaciones2 As DataTable = plazos.Actuaciones(idCaso)
        Dim filaVencida As DataRow = Nothing
        For Each fila As DataRow In tablaActuaciones2.Rows
            If UtileriasUI.Entero(fila, "ID") = vencida.ID Then filaVencida = fila
        Next
        Verificar("La actuación de enero quedó en estado Vencido",
                  filaVencida IsNot Nothing AndAlso UtileriasUI.Texto(filaVencida, "Estado") = "Vencido")
        Verificar("El panel sigue mostrando las actuaciones ya marcadas como vencidas",
                  New ServicioAlertas().ObtenerAlertas().Select("Tipo = 'Plazo vencido'").Length >= 1)

        plazos.MarcarCumplida(idActuacion)
        Dim filaCumplida As DataRow = Nothing
        For Each fila As DataRow In plazos.Actuaciones(idCaso).Rows
            If UtileriasUI.Entero(fila, "ID") = idActuacion Then filaCumplida = fila
        Next
        Verificar("Marcar cumplida cambia el estado a Cumplido",
                  filaCumplida IsNot Nothing AndAlso UtileriasUI.Texto(filaCumplida, "Estado") = "Cumplido")
        Verificar("Una actuación ya cumplida no se vuelve a modificar",
                  Not plazos.MarcarCumplida(idActuacion).Exito)
        Verificar("El repositorio no permite reabrir ni editar una actuación cumplida",
                  repositorioCasosDirecto.GuardarActuacion(actuacion) = 0)
        actuacion.ID = idActuacion
        Verificar("El servicio rechaza confirmar nuevamente una actuación cumplida",
                  Not plazos.Confirmar(actuacion).Exito)
        Verificar("Marcar como cumplida una actuación inexistente se rechaza",
                  Not plazos.MarcarCumplida(Integer.MaxValue).Exito)

        ' ---------- 7. Protocolo notarial ----------
        Console.WriteLine()
        Console.WriteLine("-- 7. Protocolo notarial")
        Dim protocolo As New ServicioProtocolo()
        Dim libro As New LibroProtocolo With {
            .NumeroLibro = 1, .AnioVigencia = 2026,
            .FechaAutorizacion = New Date(2026, 1, 5),
            .FechaVencimiento = New Date(2027, 1, 5),
            .Estado = "Vigente"
        }
        Dim rLibro As Respuesta = protocolo.GuardarLibro(libro)
        Verificar("Guardar libro de protocolo", rLibro.Exito, rLibro.Mensaje)
        Dim idLibro As Integer = libro.ID

        Dim intentoCambioEstadoLibro As Boolean = False
        Try
            repositorioProtocoloDirecto.ActualizarLibro(New LibroProtocolo With {
                .ID = idLibro, .NumeroLibro = libro.NumeroLibro,
                .AnioVigencia = libro.AnioVigencia,
                .FechaAutorizacion = libro.FechaAutorizacion,
                .FechaVencimiento = libro.FechaVencimiento,
                .Estado = "Entregado"})
            Dim libroTrasCambioDirecto As DataTable =
                repositorioProtocoloDirecto.ObtenerLibro(idLibro)
            intentoCambioEstadoLibro =
                libroTrasCambioDirecto.Rows.Count = 1 AndAlso
                Convert.ToString(libroTrasCambioDirecto.Rows(0)("Estado")) = "Vigente"
        Catch
            intentoCambioEstadoLibro = False
        End Try
        Verificar("El repositorio no permite saltarse el cierre y la entrega del libro",
                  intentoCambioEstadoLibro)

        Verificar("Libro duplicado (número y año) se rechaza con mensaje",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 1, .AnioVigencia = 2026,
                      .FechaAutorizacion = New Date(2026, 1, 5),
                      .FechaVencimiento = New Date(2027, 1, 5)}).Exito)
        Verificar("Libro con número cero se rechaza",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 0, .AnioVigencia = 2026,
                      .FechaAutorizacion = Date.Today, .FechaVencimiento = Date.Today.AddYears(1)}).Exito)
        Verificar("Un ID negativo de libro no se interpreta como alta nueva",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .ID = -1, .NumeroLibro = 4, .AnioVigencia = 2026,
                      .FechaAutorizacion = Date.Today, .FechaVencimiento = Date.Today.AddYears(1)}).Exito)
        Verificar("Vencimiento anterior a la autorización se rechaza",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 2, .AnioVigencia = 2026,
                      .FechaAutorizacion = New Date(2026, 6, 1),
                      .FechaVencimiento = New Date(2026, 1, 1)}).Exito)
        Verificar("El libro rechaza fechas de vigencia no inicializadas",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 3, .AnioVigencia = 2026,
                      .FechaAutorizacion = Date.MinValue,
                      .FechaVencimiento = New Date(2027, 1, 5)}).Exito)

        Dim instrumento1 As New Instrumento With {
            .IDLibro = idLibro, .NumeroEscritura = 1,
            .FechaHoraOtorgamiento = New Date(2026, 2, 10, 10, 30, 0),
            .FolioInicio = 1, .FolioFin = 15,
            .ObjetoActo = "Compraventa de vehículo"
        }
        Dim rInst1 As Respuesta = protocolo.GuardarInstrumento(instrumento1)
        Verificar("Guardar escritura 1 (folios 1-15)", rInst1.Exito, rInst1.Mensaje)
        Dim idInstrumento1 As Integer = instrumento1.ID

        Dim instrumento2 As New Instrumento With {
            .IDLibro = idLibro, .NumeroEscritura = 2,
            .FechaHoraOtorgamiento = New Date(2026, 3, 15, 9, 0, 0),
            .FolioInicio = 16, .FolioFin = 30,
            .ObjetoActo = "Poder general de administración"
        }
        Dim rInst2 As Respuesta = protocolo.GuardarInstrumento(instrumento2)
        Verificar("Guardar escritura 2 (folios 16-30)", rInst2.Exito, rInst2.Mensaje)
        Dim idInstrumento2 As Integer = instrumento2.ID

        Verificar("No se puede reducir la vigencia de un libro que ya contiene escrituras",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .ID = idLibro, .NumeroLibro = 1, .AnioVigencia = 2026,
                      .FechaAutorizacion = New Date(2026, 1, 5),
                      .FechaVencimiento = New Date(2026, 2, 28)}).Exito)

        Verificar("Traslape de folios se rechaza",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 3,
                      .FechaHoraOtorgamiento = Date.Now,
                      .FolioInicio = 10, .FolioFin = 12,
                      .ObjetoActo = "Prueba de traslape"}).Exito)

        Dim altaDirectaTraslapeRechazada As Boolean = False
        Try
            repositorioProtocoloDirecto.InsertarInstrumento(New Instrumento With {
                .IDLibro = idLibro, .NumeroEscritura = 90,
                .FechaHoraOtorgamiento = New Date(2026, 4, 10, 9, 0, 0),
                .FolioInicio = 10, .FolioFin = 12,
                .ObjetoActo = "Alta directa con folios ocupados"})
        Catch ex As InvalidOperationException
            altaDirectaTraslapeRechazada = True
        End Try
        Verificar("El repositorio impide altas directas que traslapen folios",
                  altaDirectaTraslapeRechazada)

        Dim edicionDirectaTraslapeRechazada As Boolean = False
        Try
            repositorioProtocoloDirecto.ActualizarInstrumento(New Instrumento With {
                .ID = idInstrumento1, .IDLibro = idLibro, .NumeroEscritura = 1,
                .FechaHoraOtorgamiento = New Date(2026, 2, 10, 10, 30, 0),
                .FolioInicio = 20, .FolioFin = 25,
                .ObjetoActo = "Edición directa con folios ocupados", .Estado = "Vigente"})
        Catch ex As InvalidOperationException
            edicionDirectaTraslapeRechazada = True
        End Try
        Verificar("El repositorio impide ediciones directas que traslapen folios",
                  edicionDirectaTraslapeRechazada)
        Dim cambioEstadoEscrituraBloqueado As Boolean = False
        Try
            repositorioProtocoloDirecto.ActualizarInstrumento(New Instrumento With {
                .ID = idInstrumento1, .IDLibro = idLibro, .NumeroEscritura = 1,
                .FechaHoraOtorgamiento = New Date(2026, 2, 10, 10, 30, 0),
                .FolioInicio = 1, .FolioFin = 15,
                .ObjetoActo = "Intento de anulación directa", .Estado = "Anulado"})
        Catch ex As InvalidOperationException
            cambioEstadoEscrituraBloqueado = True
        End Try
        Dim escrituraTrasCambioEstado As DataTable =
            repositorioProtocoloDirecto.ObtenerInstrumento(idInstrumento1)
        Verificar("El guardado ordinario no permite alterar el estado de una escritura",
                  cambioEstadoEscrituraBloqueado AndAlso escrituraTrasCambioEstado.Rows.Count = 1 AndAlso
                  Convert.ToString(escrituraTrasCambioEstado.Rows(0)("Estado")) = "Vigente")

        Verificar("Escritura duplicada en el libro se rechaza con mensaje",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 1,
                      .FechaHoraOtorgamiento = Date.Now,
                      .FolioInicio = 31, .FolioFin = 40,
                      .ObjetoActo = "Duplicada"}).Exito)
        Verificar("Otorgamiento con fecha futura se rechaza",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 4,
                      .FechaHoraOtorgamiento = Date.Now.AddDays(3),
                      .FolioInicio = 31, .FolioFin = 40,
                      .ObjetoActo = "Futura"}).Exito)
        Verificar("Folio final menor que el inicial se rechaza",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 5,
                      .FechaHoraOtorgamiento = Date.Now,
                      .FolioInicio = 50, .FolioFin = 40,
                      .ObjetoActo = "Folios invertidos"}).Exito)
        Verificar("La escritura rechaza una fecha de otorgamiento no inicializada",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 6,
                      .FechaHoraOtorgamiento = Date.MinValue,
                      .FolioInicio = 50, .FolioFin = 55,
                      .ObjetoActo = "Fecha inválida"}).Exito)
        Verificar("Un ID negativo de escritura no se interpreta como alta nueva",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .ID = -1, .IDLibro = idLibro, .NumeroEscritura = 7,
                      .FechaHoraOtorgamiento = Date.Today,
                      .FolioInicio = 60, .FolioFin = 65,
                      .ObjetoActo = "ID inválido"}).Exito)

        Dim libroDirectoInvalidoRechazado As Boolean = False
        Try
            repositorioProtocoloDirecto.InsertarLibro(New LibroProtocolo With {
                .NumeroLibro = 999, .AnioVigencia = 2200,
                .FechaAutorizacion = Date.Today, .FechaVencimiento = Date.Today.AddYears(1)})
        Catch ex As ArgumentException
            libroDirectoInvalidoRechazado = True
        End Try
        Verificar("El repositorio rechaza directamente años de vigencia de libro fuera de rango",
                  libroDirectoInvalidoRechazado)

        Dim folioDirectoInvalidoRechazado As Boolean = False
        Try
            repositorioProtocoloDirecto.InsertarInstrumento(New Instrumento With {
                .IDLibro = idLibro, .NumeroEscritura = 900,
                .FechaHoraOtorgamiento = Date.Today,
                .FolioInicio = 0, .FolioFin = 5, .ObjetoActo = "Folio directo inválido"})
        Catch ex As ArgumentException
            folioDirectoInvalidoRechazado = True
        End Try
        Verificar("El repositorio rechaza directamente rangos de folios inválidos",
                  folioDirectoInvalidoRechazado)

        ' Dos sesiones intentan reservar intervalos que se cruzan al mismo tiempo.
        ' El bloqueo FOR UPDATE del libro debe permitir exactamente una escritura.
        Dim iniciarConcurrentes As New ManualResetEvent(False)
        Dim exitosConcurrentes As Integer = 0
        Dim erroresConcurrentes As String = ""
        Dim tareaA As New Thread(New ThreadStart(
            Sub()
                Try
                    iniciarConcurrentes.WaitOne()
                    Dim resultado As Respuesta =
                        servicioProtocoloConcurrente.GuardarInstrumento(New Instrumento With {
                            .IDLibro = idLibro, .NumeroEscritura = 20,
                            .FechaHoraOtorgamiento = Date.Now,
                            .FolioInicio = 31, .FolioFin = 40,
                            .ObjetoActo = "Prueba concurrente A"})
                    If resultado.Exito Then Interlocked.Increment(exitosConcurrentes)
                Catch ex As Exception
                    SyncLock erroresConcurrentes
                        erroresConcurrentes &= "A: " & ex.Message & " "
                    End SyncLock
                End Try
            End Sub))
        Dim tareaB As New Thread(New ThreadStart(
            Sub()
                Try
                    iniciarConcurrentes.WaitOne()
                    Dim resultado As Respuesta =
                        servicioProtocoloConcurrente.GuardarInstrumento(New Instrumento With {
                            .IDLibro = idLibro, .NumeroEscritura = 21,
                            .FechaHoraOtorgamiento = Date.Now,
                            .FolioInicio = 35, .FolioFin = 45,
                            .ObjetoActo = "Prueba concurrente B"})
                    If resultado.Exito Then Interlocked.Increment(exitosConcurrentes)
                Catch ex As Exception
                    SyncLock erroresConcurrentes
                        erroresConcurrentes &= "B: " & ex.Message & " "
                    End SyncLock
                End Try
            End Sub))
        tareaA.Start()
        tareaB.Start()
        iniciarConcurrentes.Set()
        tareaA.Join()
        tareaB.Join()
        iniciarConcurrentes.Dispose()
        Verificar("Dos escrituras concurrentes con folios traslapados: solo una se guarda",
                  exitosConcurrentes = 1,
                  "Guardadas: " & exitosConcurrentes.ToString() & ". " & erroresConcurrentes)

        ' Una escritura anulada conserva sus folios: nunca deben asignarse a otra.
        Dim idConcurrente As Integer = 0
        Dim folioInicioAnulado As Integer = 0
        Dim folioFinAnulado As Integer = 0
        For Each filaInstrumento As DataRow In protocolo.Instrumentos(idLibro).Rows
            Dim numero As Integer = UtileriasUI.Entero(filaInstrumento, "Numero_Escritura")
            If numero = 20 OrElse numero = 21 Then
                idConcurrente = UtileriasUI.Entero(filaInstrumento, "ID")
                folioInicioAnulado = UtileriasUI.Entero(filaInstrumento, "Folio_Inicio")
                folioFinAnulado = UtileriasUI.Entero(filaInstrumento, "Folio_Fin")
                Exit For
            End If
        Next
        If idConcurrente > 0 Then
            Dim rAnularConcurrente As Respuesta = protocolo.AnularInstrumento(idConcurrente)
            Verificar("Anular escritura de prueba sin liberar sus folios",
                      rAnularConcurrente.Exito, rAnularConcurrente.Mensaje)
            Dim rReutilizarFolios As Respuesta = protocolo.GuardarInstrumento(New Instrumento With {
                .IDLibro = idLibro, .NumeroEscritura = 22,
                .FechaHoraOtorgamiento = New Date(2026, 4, 1, 10, 0, 0),
                .FolioInicio = folioInicioAnulado, .FolioFin = folioFinAnulado,
                .ObjetoActo = "Intento de reutilizar folios anulados"})
            Verificar("Los folios de una escritura anulada no se pueden reutilizar",
                      Not rReutilizarFolios.Exito, rReutilizarFolios.Mensaje)
        Else
            Verificar("Anular escritura de prueba sin liberar sus folios", False,
                      "No se encontró la escritura concurrente guardada.")
            Verificar("Los folios de una escritura anulada no se pueden reutilizar", False,
                      "No se encontró la escritura concurrente guardada.")
        End If

        Dim rComp1 As Respuesta = protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Otorgante")
        Verificar("Agregar compareciente otorgante a la escritura 1", rComp1.Exito, rComp1.Mensaje)

        Dim clienteIdentidadAlterada As New Cliente With {
            .ID = idCliente, .Nombres = "Ana María Modificada", .Apellidos = cliente.Apellidos,
            .DUI = cliente.DUI, .NIT = cliente.NIT, .Nacionalidad = cliente.Nacionalidad,
            .Profesion = cliente.Profesion, .FechaNacimiento = cliente.FechaNacimiento,
            .Direccion = cliente.Direccion, .Telefono = cliente.Telefono,
            .Email = cliente.Email, .Activo = True}
        Verificar("No se altera la identidad de un cliente que ya figura en el protocolo",
                  Not clientes.Guardar(clienteIdentidadAlterada, Nothing).Exito)
        Dim clienteContactoActualizado As New Cliente With {
            .ID = idCliente, .Nombres = cliente.Nombres, .Apellidos = cliente.Apellidos,
            .DUI = cliente.DUI, .NIT = cliente.NIT, .Nacionalidad = cliente.Nacionalidad,
            .Profesion = cliente.Profesion, .FechaNacimiento = cliente.FechaNacimiento,
            .Direccion = cliente.Direccion, .Telefono = "2222-1111",
            .Email = "ana.perez.actualizado@ejemplo.com", .Activo = True}
        Verificar("Los datos de contacto sí pueden actualizarse sin alterar la identidad histórica",
                  clientes.Guardar(clienteContactoActualizado, Nothing).Exito)

        Dim rCompDup As Respuesta = protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Otorgante")
        Verificar("Compareciente duplicado se rechaza con mensaje", Not rCompDup.Exito, rCompDup.Mensaje)
        Verificar("Calidad fuera de catálogo se rechaza",
                  Not protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Curioso").Exito)
        Dim calidadDirectaRechazada As Boolean = False
        Try
            repositorioProtocoloDirecto.AgregarCompareciente(idInstrumento1, idCliente, "Curioso")
        Catch ex As ArgumentException
            calidadDirectaRechazada = True
        End Try
        Verificar("El repositorio impide omitir la validación de calidad del compareciente",
                  calidadDirectaRechazada)

        Dim fechaCierreAnteriorRechazada As Boolean = False
        Try
            repositorioProtocoloDirecto.CerrarLibro(
                idLibro, "Fecha anterior a autorización", libro.FechaAutorizacion.AddDays(-1))
        Catch ex As ArgumentException
            fechaCierreAnteriorRechazada = True
        End Try
        Verificar("El repositorio impide cerrar un libro antes de su fecha de autorización",
                  fechaCierreAnteriorRechazada)

        Dim cierreAntesUltimaEscrituraRechazado As Boolean = False
        Try
            repositorioProtocoloDirecto.CerrarLibro(
                idLibro, "Cierre anterior a escritura", libro.FechaAutorizacion.AddDays(1))
        Catch ex As ArgumentException
            cierreAntesUltimaEscrituraRechazado = True
        End Try
        Verificar("El repositorio impide cerrar un libro antes de su última escritura vigente",
                  cierreAntesUltimaEscrituraRechazado)

        Dim rCerrar As Respuesta = protocolo.CerrarLibro(idLibro, "Libro completo")
        Verificar("Cerrar libro con escritura sin otorgante se bloquea",
                  Not rCerrar.Exito AndAlso rCerrar.Mensaje.Contains("otorgante"), rCerrar.Mensaje)
        Verificar("El índice no se genera mientras el libro sigue vigente",
                  Not protocolo.GenerarIndice(idLibro).Exito)
        Verificar("El repositorio tampoco permite marcar un índice antes del cierre",
                  New ProtocoloRepositorio().MarcarIndiceGenerado(idLibro) = 0)

        Dim rComp2 As Respuesta = protocolo.AgregarCompareciente(idInstrumento2, idCliente, "Otorgante")
        Verificar("Agregar otorgante a la escritura 2", rComp2.Exito, rComp2.Mensaje)

        Dim rCerrar2 As Respuesta = protocolo.CerrarLibro(idLibro, "Libro completo, 2 instrumentos")
        Verificar("Cerrar libro con todos los otorgantes registrados", rCerrar2.Exito, rCerrar2.Mensaje)
        Verificar("Un libro cerrado no puede modificarse desde el editor",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .ID = idLibro, .NumeroLibro = 2, .AnioVigencia = 2026,
                      .FechaAutorizacion = New Date(2026, 1, 5),
                      .FechaVencimiento = New Date(2027, 1, 5)}).Exito)
        Verificar("No se puede agregar comparecientes después del cierre",
                  Not protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Testigo").Exito)
        Dim idComparecienteCerrado As Integer =
            UtileriasUI.Entero(protocolo.Comparecientes(idInstrumento1).Rows(0), "ID")
        Verificar("No se puede quitar comparecientes después del cierre",
                  Not protocolo.QuitarCompareciente(idComparecienteCerrado).Exito)
        Verificar("Registrar escritura en libro cerrado se rechaza",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 6,
                      .FechaHoraOtorgamiento = Date.Now,
                      .FolioInicio = 31, .FolioFin = 40,
                      .ObjetoActo = "En libro cerrado"}).Exito)

        Dim rTestimonio As Respuesta = protocolo.ExpedirTestimonio(idInstrumento1, "Primer", Date.Today, "Banco Cuscatlán, S.A.")
        Verificar("Expedir primer testimonio de la escritura 1", rTestimonio.Exito, rTestimonio.Mensaje)
        Verificar("No se permite expedir un segundo primer testimonio",
                  Not protocolo.ExpedirTestimonio(idInstrumento1, "Primer", Date.Today, "Destinatario duplicado").Exito)
        Verificar("Se permite expedir un ulterior después del primero",
                  protocolo.ExpedirTestimonio(idInstrumento1, "Ulterior", Date.Today, "Destinatario ulterior").Exito)

        Dim instrumentoConTestimonios As DataRow = protocolo.ObtenerInstrumento(idInstrumento1).Rows(0)
        Dim rEditarConTestimonios As Respuesta = protocolo.GuardarInstrumento(New Instrumento With {
            .ID = idInstrumento1, .IDLibro = idLibro,
            .NumeroEscritura = UtileriasUI.Entero(instrumentoConTestimonios, "Numero_Escritura"),
            .FechaHoraOtorgamiento = Convert.ToDateTime(instrumentoConTestimonios("Fecha_Hora_Otorgamiento")),
            .FolioInicio = UtileriasUI.Entero(instrumentoConTestimonios, "Folio_Inicio"),
            .FolioFin = UtileriasUI.Entero(instrumentoConTestimonios, "Folio_Fin"),
            .ObjetoActo = UtileriasUI.Texto(instrumentoConTestimonios, "Objeto_Acto") & " - cambio"})
        Verificar("No se puede modificar una escritura después de expedir testimonios",
                  Not rEditarConTestimonios.Exito, rEditarConTestimonios.Mensaje)

        Dim comparecientesConTestimonio As DataTable = protocolo.Comparecientes(idInstrumento1)
        Dim idComparecienteProtegido As Integer = UtileriasUI.Entero(comparecientesConTestimonio.Rows(0), "ID")
        Verificar("No se puede quitar un compareciente después de expedir testimonios",
                  Not protocolo.QuitarCompareciente(idComparecienteProtegido).Exito)

        Verificar("No se puede anular una escritura con testimonios expedidos",
                  Not protocolo.AnularInstrumento(idInstrumento1).Exito)
        Verificar("Testimonio con fecha futura se rechaza",
                  Not protocolo.ExpedirTestimonio(idInstrumento1, "Primer", Date.Today.AddDays(1), "X").Exito)
        Verificar("Tipo de testimonio fuera de catálogo se rechaza",
                  Not protocolo.ExpedirTestimonio(idInstrumento1, "Copia", Date.Today, "Destinatario válido").Exito)
        Verificar("Testimonio sin destinatario se rechaza",
                  Not protocolo.ExpedirTestimonio(idInstrumento1, "Ulterior", Date.Today, "   ").Exito)
        Verificar("Destinatario de testimonio mayor de 200 caracteres se rechaza",
                  Not protocolo.ExpedirTestimonio(idInstrumento1, "Ulterior", Date.Today, New String("X"c, 201)).Exito)

        Verificar("No se expiden testimonios de escritura anulada",
                  Not protocolo.ExpedirTestimonio(idConcurrente, "Ulterior", Date.Today, "X").Exito)

        Dim rEntregaAnterior As Respuesta = protocolo.RegistrarEntrega(idLibro, Date.Today.AddDays(-1))
        Verificar("La fecha de entrega no puede ser anterior al cierre", Not rEntregaAnterior.Exito)
        Dim rEntregaFutura As Respuesta = protocolo.RegistrarEntrega(idLibro, Date.Today.AddDays(1))
        Verificar("Entrega al Notariado con fecha futura se rechaza", Not rEntregaFutura.Exito)
        Dim entregaDirectaFuturaRechazada As Boolean = False
        Try
            repositorioProtocoloDirecto.RegistrarEntrega(idLibro, Date.Today.AddDays(1))
        Catch ex As ArgumentException
            entregaDirectaFuturaRechazada = True
        End Try
        Verificar("El repositorio impide registrar directamente una entrega futura",
                  entregaDirectaFuturaRechazada)
        Dim rEntrega As Respuesta = protocolo.RegistrarEntrega(idLibro, Date.Today)
        Verificar("Registrar entrega del libro cerrado", rEntrega.Exito, rEntrega.Mensaje)
        Verificar("No se puede anular una escritura después de entregar el libro",
                  Not protocolo.AnularInstrumento(idInstrumento2).Exito)
        Verificar("No se pueden expedir testimonios después de entregar el libro",
                  Not protocolo.ExpedirTestimonio(idInstrumento2, "Primer", Date.Today, "Destinatario posterior").Exito)

        Dim rIndice As Respuesta = protocolo.GenerarIndice(idLibro)
        Verificar("Generar índice del libro", rIndice.Exito, rIndice.Mensaje)
        Verificar("Volver a solicitar un índice ya generado no produce un falso error",
                  protocolo.GenerarIndice(idLibro).Exito)
        Dim indice As DataTable = protocolo.IndiceLibro(idLibro)
        Verificar("El índice conserva las 3 escrituras, incluida la anulada", indice.Rows.Count = 3)
        Dim escrituraAnuladaEnIndice As Boolean = False
        For Each filaIndice As DataRow In indice.Rows
            If (UtileriasUI.Entero(filaIndice, "Numero_Escritura") = 20 OrElse
                UtileriasUI.Entero(filaIndice, "Numero_Escritura") = 21) AndAlso
               UtileriasUI.Texto(filaIndice, "Estado") = "Anulado" Then
                escrituraAnuladaEnIndice = True
                Exit For
            End If
        Next
        Verificar("El índice conserva la escritura anulada con su estado", escrituraAnuladaEnIndice)
        Dim conOtorgantes As Boolean = False
        For Each fila As DataRow In indice.Rows
            If UtileriasUI.Texto(fila, "Otorgantes").Contains("Pérez") Then conOtorgantes = True
        Next
        Verificar("El índice lista los otorgantes por escritura", conOtorgantes)

        ' ---------- 8. Documentos ----------
        Console.WriteLine()
        Console.WriteLine("-- 8. Documentos e integridad SHA-256")
        Dim documentos As New ServicioDocumentos()
        Dim rutaManipuladaRechazada As Boolean = False
        Try
            documentos.RutaCompleta(Path.Combine("..", "..", "..", "Windows", "win.ini"))
        Catch ex As InvalidOperationException
            rutaManipuladaRechazada = ex.Message.Contains("fuera de la carpeta")
        End Try
        Verificar("Una ruta manipulada no puede salir de la carpeta documental", rutaManipuladaRechazada)

        Dim carpetaTrabajo As String = Path.Combine(Path.GetTempPath(), "sgln_pruebas_archivos")
        Directory.CreateDirectory(carpetaTrabajo)
        Dim rutaPdf As String = Path.Combine(carpetaTrabajo, "contrato_prueba.pdf")
        File.WriteAllText(rutaPdf, "%PDF-1.4" & Environment.NewLine & "Contenido simulado de un PDF de prueba.")
        Dim rutaConControlRechazada As Boolean = False
        Try
            repositorioDocumentosDirecto.Insertar(New Documento With {
                .TipoDocumento = "Contrato", .RutaArchivo = "Documentos/" & ChrW(10) & "prueba.pdf",
                .Hash = New String("A"c, 64)})
        Catch ex As ArgumentException
            rutaConControlRechazada = True
        End Try
        Verificar("El repositorio documental rechaza rutas con caracteres de control",
                  rutaConControlRechazada)

        Dim hashDirectoInvalidoRechazado As Boolean = False
        Try
            repositorioDocumentosDirecto.Insertar(New Documento With {
                .TipoDocumento = "Contrato", .RutaArchivo = "Documentos/prueba.pdf",
                .Hash = "NO_ES_SHA256", .EstadoProcesamientoIA = "Procesado"})
        Catch ex As ArgumentException
            hashDirectoInvalidoRechazado = True
        End Try
        Verificar("El repositorio documental rechaza hashes que no son SHA-256",
                  hashDirectoInvalidoRechazado)
        Dim rutaPdfFalso As String = Path.Combine(carpetaTrabajo, "archivo_falso.pdf")
        File.WriteAllText(rutaPdfFalso, "Este contenido no es un PDF.")
        Verificar("Un archivo con extensión PDF pero firma incorrecta se rechaza",
                  Not documentos.Importar(rutaPdfFalso, "Contrato", Nothing, Nothing).Exito)

        ' Archivo disperso para comprobar el límite de 50 MB sin escribir 50 MB
        ' de datos reales en disco durante la batería automatizada.
        Dim rutaPdfGrande As String = Path.Combine(carpetaTrabajo, "archivo_grande.pdf")
        Using flujoGrande As New FileStream(rutaPdfGrande, FileMode.Create, FileAccess.Write, FileShare.None)
            flujoGrande.SetLength(50L * 1024L * 1024L + 1L)
        End Using
        Verificar("Un documento superior a 50 MB se rechaza antes de importarse",
                  Not documentos.Importar(rutaPdfGrande, "Contrato", Nothing, Nothing).Exito)
        File.Delete(rutaPdfGrande)

        Dim rImportar As Respuesta = documentos.Importar(rutaPdf, "Contrato", idCaso, Nothing)
        Verificar("Importar documento PDF vinculado al caso", rImportar.Exito, rImportar.Mensaje)
        Dim idDocumento As Integer = 0
        If rImportar.ID.HasValue Then idDocumento = rImportar.ID.Value

        Dim tablaDoc As DataTable = documentos.Obtener(idDocumento)
        Verificar("El documento queda registrado con su ruta",
                  tablaDoc.Rows.Count = 1 AndAlso UtileriasUI.Texto(tablaDoc.Rows(0), "Ruta_Archivo") <> "")
        Verificar("El hash SHA-256 queda registrado (64 caracteres)",
                  UtileriasUI.Texto(tablaDoc.Rows(0), "Hash").Length = 64)
        Verificar("La búsqueda de documentos sin filtro lista resultados",
                  documentos.Buscar("", "").Rows.Count >= 1)

        Dim rVerificar As Respuesta = documentos.VerificarIntegridad(idDocumento)
        Verificar("La integridad del archivo original pasa", rVerificar.Exito, rVerificar.Mensaje)

        File.AppendAllText(documentos.RutaDelDocumento(idDocumento), " TEXTO AGREGADO QUE ALTERA EL ARCHIVO.")
        Dim rAlterado As Respuesta = documentos.VerificarIntegridad(idDocumento)
        Verificar("Un archivo alterado dispara la alerta de integridad",
                  Not rAlterado.Exito AndAlso rAlterado.Mensaje.Contains("ALERTA"), rAlterado.Mensaje)
        Dim aperturaAlteradaBloqueada As Boolean = False
        Try
            documentos.RutaDelDocumento(idDocumento)
        Catch ex As System.IO.InvalidDataException
            aperturaAlteradaBloqueada = True
        End Try
        Verificar("Un documento alterado no puede abrirse desde el sistema", aperturaAlteradaBloqueada)

        Dim rMeta As Respuesta = documentos.ActualizarMetadatos(idDocumento, "Escritura pública", Nothing, idInstrumento1)
        Verificar("Actualizar metadatos del documento", rMeta.Exito, rMeta.Mensaje)
        Dim tablaDoc2 As DataTable = documentos.Obtener(idDocumento)
        Verificar("El documento queda vinculado a la escritura 1",
                  UtileriasUI.Entero(tablaDoc2.Rows(0), "ID_Instrumento") = idInstrumento1)
        Verificar("Actualizar metadatos de un documento inexistente se rechaza",
                  Not documentos.ActualizarMetadatos(Integer.MaxValue, "Contrato", Nothing, Nothing).Exito)

        Verificar("Importar un archivo inexistente se rechaza",
                  Not documentos.Importar(Path.Combine(carpetaTrabajo, "no_existe.pdf"), "Contrato", Nothing, Nothing).Exito)
        Dim rutaExe As String = Path.Combine(carpetaTrabajo, "peligro.exe")
        File.WriteAllText(rutaExe, "x")
        Verificar("Importar una extensión no permitida se rechaza",
                  Not documentos.Importar(rutaExe, "Contrato", Nothing, Nothing).Exito)

        ' ---------- 9. Cumplimiento LDA/FT y ARCO ----------
        Console.WriteLine()
        Console.WriteLine("-- 9. Cumplimiento LDA/FT y derechos ARCO")
        Dim cumplimiento As New ServicioCumplimiento()
        Dim calculadoraFeriados As New CalculadoraDiasHabiles(New CatalogoRepositorio().FechasFeriadas())

        Verificar("El código de moneda rechaza caracteres Unicode ajenos a códigos alfabéticos ASCII",
                  Not cumplimiento.GuardarOperacion(New OperacionLdaft With {
                      .TipoOperacion = "Regulada", .Monto = 100D, .Moneda = "USÉ",
                      .FechaOperacion = Date.Today
                  }).Exito)
        Verificar("Los identificadores negativos de caso e instrumento se rechazan",
                  Not cumplimiento.GuardarOperacion(New OperacionLdaft With {
                      .TipoOperacion = "Regulada", .Monto = 100D, .Moneda = "USD",
                      .FechaOperacion = Date.Today, .IDCaso = -1
                  }).Exito AndAlso
                  Not cumplimiento.GuardarOperacion(New OperacionLdaft With {
                      .TipoOperacion = "Regulada", .Monto = 100D, .Moneda = "USD",
                      .FechaOperacion = Date.Today, .IDInstrumento = -1
                  }).Exito)

        Dim operacionDirectaInvalidaRechazada As Boolean = False
        Try
            repositorioCumplimientoDirecto.InsertarOperacion(New OperacionLdaft With {
                .TipoOperacion = "Regulada", .Monto = 0D, .Moneda = "USD",
                .FechaOperacion = Date.Today,
                .FechaVencimientoAnalisis = Date.Today.AddDays(5),
                .FechaVencimientoReporte = Date.Today.AddDays(15),
                .Descripcion = "Monto inválido"})
        Catch ex As ArgumentException
            operacionDirectaInvalidaRechazada = True
        End Try
        Verificar("El repositorio LDA/FT rechaza directamente montos no positivos",
                  operacionDirectaInvalidaRechazada)

        Dim operacion As New OperacionLdaft With {
            .TipoOperacion = "Regulada",
            .Monto = 12500D,
            .Moneda = "USD",
            .FechaOperacion = Date.Today,
            .Descripcion = "Depósito en efectivo del cliente Ana María Pérez",
            .IDInstrumento = idInstrumento1
        }
        Dim rOperacion As Respuesta = cumplimiento.GuardarOperacion(operacion)
        Verificar("Guardar operación regulada", rOperacion.Exito, rOperacion.Mensaje)
        Verificar("El mensaje alerta el umbral de efectivo cuando aplica",
                  rOperacion.Mensaje.Contains("umbral"), rOperacion.Mensaje)

        Dim tablaOperacion As DataTable = cumplimiento.ObtenerOperacion(operacion.ID)
        Verificar("La búsqueda de operaciones sin filtro lista resultados",
                  cumplimiento.Operaciones("").Rows.Count >= 1)
        Verificar("El plazo de análisis se calcula a 5 días hábiles",
                  UtileriasUI.Fecha(tablaOperacion.Rows(0), "Fecha_Vencimiento_Analisis") =
                  calculadoraFeriados.CalcularVencimiento(Date.Today, 5))
        Verificar("El plazo de reporte se calcula a 15 días hábiles",
                  UtileriasUI.Fecha(tablaOperacion.Rows(0), "Fecha_Vencimiento_Reporte") =
                  calculadoraFeriados.CalcularVencimiento(Date.Today, 15))

        Dim sospechosa As New OperacionLdaft With {
            .TipoOperacion = "Sospechosa", .Monto = 800D, .Moneda = "USD",
            .FechaOperacion = Date.Today, .Descripcion = "Estructura de operación extraña"}
        Dim rSospechosa As Respuesta = cumplimiento.GuardarOperacion(sospechosa)
        Verificar("Guardar operación sospechosa sin alerta de umbral",
                  rSospechosa.Exito AndAlso Not rSospechosa.Mensaje.Contains("umbral"), rSospechosa.Mensaje)

        Verificar("Monto cero se rechaza",
                  Not cumplimiento.GuardarOperacion(New OperacionLdaft With {
                      .TipoOperacion = "Regulada", .Monto = 0D, .Moneda = "USD",
                      .FechaOperacion = Date.Today, .Descripcion = "x"}).Exito)
        Verificar("Tipo de operación fuera de catálogo se rechaza",
                  Not cumplimiento.GuardarOperacion(New OperacionLdaft With {
                      .TipoOperacion = "Extraña", .Monto = 100D, .Moneda = "USD",
                      .FechaOperacion = Date.Today, .Descripcion = "x"}).Exito)
        Verificar("Operación con fecha futura se rechaza",
                  Not cumplimiento.GuardarOperacion(New OperacionLdaft With {
                      .TipoOperacion = "Regulada", .Monto = 100D, .Moneda = "USD",
                      .FechaOperacion = Date.Today.AddDays(1), .Descripcion = "x"}).Exito)

        Verificar("Reporte a la UIF con fecha futura se rechaza",
                  Not cumplimiento.MarcarReportada(operacion.ID, Date.Today.AddDays(1)).Exito)
        Dim fechaReporteDirectaFuturaRechazada As Boolean = False
        Try
            repositorioCumplimientoDirecto.MarcarReportada(operacion.ID, Date.Today.AddDays(1))
        Catch ex As ArgumentException
            fechaReporteDirectaFuturaRechazada = True
        End Try
        Verificar("El repositorio también rechaza directamente fechas futuras de reporte UIF",
                  fechaReporteDirectaFuturaRechazada)
        Verificar("El repositorio no permite reportar con fecha anterior a la operación",
                  repositorioCumplimientoDirecto.MarcarReportada(operacion.ID, Date.Today.AddDays(-1)) = 0)
        Verificar("Reporte con fecha anterior a la operación se rechaza",
                  Not cumplimiento.MarcarReportada(operacion.ID, Date.Today.AddDays(-1)).Exito)
        Dim rReporte As Respuesta = cumplimiento.MarcarReportada(operacion.ID, Date.Today)
        Verificar("Registrar reporte a la UIF", rReporte.Exito, rReporte.Mensaje)
        Verificar("No se vuelve a reportar una operación ya reportada",
                  Not cumplimiento.MarcarReportada(operacion.ID, Date.Today).Exito)
        Verificar("No se descarta una operación reportada a la UIF",
                  Not cumplimiento.Descartar(operacion.ID).Exito)

        Dim rDescartar As Respuesta = cumplimiento.Descartar(sospechosa.ID)
        Verificar("Descartar operación conservando el registro", rDescartar.Exito, rDescartar.Mensaje)
        Verificar("No se puede reportar una operación descartada",
                  Not cumplimiento.MarcarReportada(sospechosa.ID, Date.Today).Exito)
        Verificar("No se puede descartar dos veces la misma operación",
                  Not cumplimiento.Descartar(sospechosa.ID).Exito)
        Verificar("El repositorio tampoco permite descartar directamente una operación ya reportada",
                  repositorioCumplimientoDirecto.Descartar(operacion.ID) = 0)
        Verificar("El repositorio impide modificar directamente una operación ya reportada",
                  repositorioCumplimientoDirecto.ActualizarOperacion(New OperacionLdaft With {
                      .ID = operacion.ID, .TipoOperacion = "Regulada", .Monto = 13000D,
                      .Moneda = "USD", .FechaOperacion = Date.Today,
                      .FechaVencimientoAnalisis = UtileriasUI.Fecha(tablaOperacion.Rows(0), "Fecha_Vencimiento_Analisis"),
                      .FechaVencimientoReporte = UtileriasUI.Fecha(tablaOperacion.Rows(0), "Fecha_Vencimiento_Reporte"),
                      .IDInstrumento = idInstrumento1, .Descripcion = "Intento de alterar reporte UIF"
                  }) = 0)
        Verificar("El repositorio impide reportar directamente una operación descartada",
                  repositorioCumplimientoDirecto.MarcarReportada(sospechosa.ID, Date.Today) = 0)
        Verificar("El repositorio impide modificar directamente una operación descartada",
                  repositorioCumplimientoDirecto.ActualizarOperacion(New OperacionLdaft With {
                      .ID = sospechosa.ID, .TipoOperacion = "Sospechosa", .Monto = 800D,
                      .Moneda = "USD", .FechaOperacion = Date.Today,
                      .FechaVencimientoAnalisis = calculadoraFeriados.CalcularVencimiento(Date.Today, 5),
                      .FechaVencimientoReporte = calculadoraFeriados.CalcularVencimiento(Date.Today, 15),
                      .Descripcion = "Intento de alterar operación descartada"
                  }) = 0)

        Dim arco As New SolicitudArco With {
            .IDCliente = idCliente, .TipoDerecho = "Acceso",
            .FechaSolicitud = Date.Today,
            .Detalle = "Solicitud de copia de datos personales"}
        Dim rArco As Respuesta = cumplimiento.GuardarSolicitud(arco)
        Verificar("Guardar solicitud ARCO de acceso", rArco.Exito, rArco.Mensaje)
        Dim arcoDirectoForzadoPendiente As Integer =
            repositorioCumplimientoDirecto.InsertarSolicitud(New SolicitudArco With {
                .IDCliente = idCliente, .TipoDerecho = "Acceso",
                .FechaSolicitud = Date.Today, .FechaVencimiento = Date.Today.AddDays(10),
                .Estado = "Respondida", .FechaRespuesta = Date.Today,
                .Detalle = "Prueba de estado inicial protegido"})
        Dim arcoDirectoForzadoTabla As DataTable =
            repositorioCumplimientoDirecto.ObtenerSolicitud(arcoDirectoForzadoPendiente)
        Verificar("El repositorio fuerza estado Pendiente y fecha de respuesta nula al crear ARCO",
                  arcoDirectoForzadoTabla.Rows.Count = 1 AndAlso
                  Convert.ToString(arcoDirectoForzadoTabla.Rows(0)("Estado")) = "Pendiente" AndAlso
                  arcoDirectoForzadoTabla.Rows(0).IsNull("Fecha_Respuesta"))
        Verificar("La búsqueda de solicitudes ARCO sin filtro lista resultados",
                  cumplimiento.SolicitudesArco("").Rows.Count >= 1)
        Verificar("El vencimiento ARCO se calcula a 10 días hábiles",
                  UtileriasUI.Fecha(cumplimiento.ObtenerSolicitud(arco.ID).Rows(0), "Fecha_Vencimiento") =
                  calculadoraFeriados.CalcularVencimiento(Date.Today, 10))
        Dim intentoAlterarHechoArco As Integer =
            repositorioCumplimientoDirecto.ActualizarSolicitud(New SolicitudArco With {
                .ID = arco.ID, .IDCliente = idCliente + 999, .TipoDerecho = "Cancelación",
                .FechaSolicitud = Date.Today.AddDays(30), .FechaVencimiento = Date.Today.AddDays(60),
                .Estado = "Respondida", .FechaRespuesta = Date.Today,
                .ProrrogaAplicada = False, .Detalle = "Detalle corregido sin alterar el hecho original"})
        Dim arcoTrasAlteracionDirecta As DataTable = cumplimiento.ObtenerSolicitud(arco.ID)
        Verificar("El repositorio conserva titular, derecho, fechas y estado histórico de ARCO al editar",
                  intentoAlterarHechoArco = 1 AndAlso arcoTrasAlteracionDirecta.Rows.Count = 1 AndAlso
                  Convert.ToInt32(arcoTrasAlteracionDirecta.Rows(0)("ID_Cliente")) = idCliente AndAlso
                  Convert.ToString(arcoTrasAlteracionDirecta.Rows(0)("Tipo_Derecho")) = "Acceso" AndAlso
                  UtileriasUI.Fecha(arcoTrasAlteracionDirecta.Rows(0), "Fecha_Solicitud") = Date.Today AndAlso
                  Convert.ToString(arcoTrasAlteracionDirecta.Rows(0)("Estado")) = "Pendiente" AndAlso
                  arcoTrasAlteracionDirecta.Rows(0).IsNull("Fecha_Respuesta"))

        Dim arcoVencida As New SolicitudArco With {
            .IDCliente = idCliente, .TipoDerecho = "Acceso",
            .FechaSolicitud = Date.Today.AddDays(-30),
            .Detalle = "Solicitud para validar vencimiento automático"}
        Dim rArcoVencida As Respuesta = cumplimiento.GuardarSolicitud(arcoVencida)
        Dim tablaArcoVencida As DataTable = cumplimiento.ObtenerSolicitud(arcoVencida.ID)
        Verificar("Una solicitud ARCO cuyo plazo efectivo ya venció se muestra como Vencida",
                  rArcoVencida.Exito AndAlso tablaArcoVencida.Rows.Count = 1 AndAlso
                  Convert.ToString(tablaArcoVencida.Rows(0)("Estado")) = "Vencida")
        Verificar("La lista ARCO también refleja el estado Vencida",
                  cumplimiento.SolicitudesArco("").Select("ID = " & arcoVencida.ID.ToString()).Length = 1 AndAlso
                  Convert.ToString(cumplimiento.SolicitudesArco("").Select("ID = " & arcoVencida.ID.ToString())(0)("Estado")) = "Vencida")
        Dim alertasArco As DataTable = New ServicioAlertas().ObtenerAlertas()
        Verificar("Las solicitudes ARCO vencidas generan una alerta de vencimiento, no de próximo vencimiento",
                  alertasArco.Select("Tipo = 'ARCO vencida'").Length >= 1)

        Dim arcoConProrrogaVigente As New SolicitudArco With {
            .IDCliente = idCliente, .TipoDerecho = "Acceso",
            .FechaSolicitud = Date.Today.AddDays(-15), .ProrrogaAplicada = True,
            .Detalle = "La prórroga mantiene vigente el plazo"}
        Dim rArcoConProrroga As Respuesta = cumplimiento.GuardarSolicitud(arcoConProrrogaVigente)
        Dim tablaArcoConProrroga As DataTable = cumplimiento.ObtenerSolicitud(arcoConProrrogaVigente.ID)
        Verificar("La fecha de prórroga vigente evita marcar prematuramente la solicitud como Vencida",
                  rArcoConProrroga.Exito AndAlso tablaArcoConProrroga.Rows.Count = 1 AndAlso
                  UtileriasUI.Fecha(tablaArcoConProrroga.Rows(0), "Fecha_Vencimiento_Prorroga").GetValueOrDefault(Date.MinValue) >= Date.Today AndAlso
                  Convert.ToString(tablaArcoConProrroga.Rows(0)("Estado")) <> "Vencida")

        Dim arcoProrroga As New SolicitudArco With {
            .IDCliente = idCliente, .TipoDerecho = "Rectificación",
            .FechaSolicitud = Date.Today, .ProrrogaAplicada = True,
            .Detalle = "Solicitud con prórroga"}
        Dim rProrroga As Respuesta = cumplimiento.GuardarSolicitud(arcoProrroga)
        Verificar("Guardar solicitud ARCO con prórroga", rProrroga.Exito, rProrroga.Mensaje)
        Verificar("Una solicitud ARCO no permite sustituir al cliente titular",
                  Not cumplimiento.GuardarSolicitud(New SolicitudArco With {
                      .ID = arcoProrroga.ID, .IDCliente = idCliente + 999,
                      .TipoDerecho = "Rectificación", .FechaSolicitud = Date.Today,
                      .Detalle = "Intento de cambiar titular"}).Exito)
        Verificar("Una solicitud ARCO no permite cambiar el derecho ejercido",
                  Not cumplimiento.GuardarSolicitud(New SolicitudArco With {
                      .ID = arcoProrroga.ID, .IDCliente = idCliente,
                      .TipoDerecho = "Acceso", .FechaSolicitud = Date.Today,
                      .Detalle = "Intento de cambiar derecho"}).Exito)
        Dim tablaProrroga As DataTable = cumplimiento.ObtenerSolicitud(arcoProrroga.ID)
        Verificar("La prórroga agrega 5 días hábiles al vencimiento",
                  UtileriasUI.Fecha(tablaProrroga.Rows(0), "Fecha_Vencimiento_Prorroga") =
                  calculadoraFeriados.CalcularVencimiento(
                      UtileriasUI.Fecha(tablaProrroga.Rows(0), "Fecha_Vencimiento").Value, 5))

        Dim fechaProrrogaOriginal As Date =
            UtileriasUI.Fecha(tablaProrroga.Rows(0), "Fecha_Vencimiento_Prorroga").Value
        Dim editarProrroga As Respuesta = cumplimiento.GuardarSolicitud(New SolicitudArco With {
            .ID = arcoProrroga.ID, .IDCliente = idCliente, .TipoDerecho = "Rectificación",
            .FechaSolicitud = Date.Today, .ProrrogaAplicada = False, .Detalle = "Detalle actualizado"})
        Dim tablaProrrogaActualizada As DataTable = cumplimiento.ObtenerSolicitud(arcoProrroga.ID)
        Verificar("Una prórroga ya aplicada no puede retirarse al editar la solicitud",
                  editarProrroga.Exito AndAlso
                  Convert.ToBoolean(tablaProrrogaActualizada.Rows(0)("Prorroga_Aplicada")) AndAlso
                  UtileriasUI.Fecha(tablaProrrogaActualizada.Rows(0), "Fecha_Vencimiento_Prorroga") = fechaProrrogaOriginal)
        Verificar("El repositorio tampoco permite retirar directamente una prórroga ARCO aplicada",
                  repositorioCumplimientoDirecto.ActualizarSolicitud(New SolicitudArco With {
                      .ID = arcoProrroga.ID, .ProrrogaAplicada = False,
                      .Detalle = "Intento de retirar prórroga directamente"}) = 0)

        Verificar("Solicitud ARCO con fecha futura se rechaza",
                  Not cumplimiento.GuardarSolicitud(New SolicitudArco With {
                      .IDCliente = idCliente, .TipoDerecho = "Acceso",
                      .FechaSolicitud = Date.Today.AddDays(1)}).Exito)
        Verificar("Responder antes de la fecha de solicitud se rechaza",
                  Not cumplimiento.ResponderSolicitud(arco.ID, Date.Today.AddDays(-5)).Exito)
        Verificar("El repositorio tampoco responde ARCO con fecha anterior a la solicitud",
                  repositorioCumplimientoDirecto.Responder(arco.ID, Date.Today.AddDays(-5)) = 0)
        Dim respuestaArcoDirectaFuturaRechazada As Boolean = False
        Try
            repositorioCumplimientoDirecto.Responder(arco.ID, Date.Today.AddDays(1))
        Catch ex As ArgumentException
            respuestaArcoDirectaFuturaRechazada = True
        End Try
        Verificar("El repositorio rechaza directamente fechas futuras de respuesta ARCO",
                  respuestaArcoDirectaFuturaRechazada)
        Dim rResponder As Respuesta = cumplimiento.ResponderSolicitud(arco.ID, Date.Today)
        Verificar("Responder solicitud ARCO", rResponder.Exito, rResponder.Mensaje)
        Verificar("No se responde dos veces una solicitud ARCO",
                  Not cumplimiento.ResponderSolicitud(arco.ID, Date.Today).Exito)
        Verificar("No se modifica una solicitud ARCO después de responderla",
                  Not cumplimiento.GuardarSolicitud(New SolicitudArco With {
                      .ID = arco.ID, .IDCliente = idCliente, .TipoDerecho = "Acceso",
                      .FechaSolicitud = Date.Today, .Detalle = "Intento de modificación posterior"}).Exito)
        Verificar("El repositorio impide modificar directamente una solicitud ARCO respondida",
                  repositorioCumplimientoDirecto.ActualizarSolicitud(New SolicitudArco With {
                      .ID = arco.ID, .ProrrogaAplicada = False,
                      .Detalle = "Intento directo de alterar solicitud respondida"
                  }) = 0)
        Verificar("El repositorio impide responder dos veces directamente una solicitud ARCO",
                  repositorioCumplimientoDirecto.Responder(arco.ID, Date.Today) = 0)

        ' ---------- 10. Auditoría ----------
        Console.WriteLine()
        Console.WriteLine("-- 10. Auditoría")
        Dim auditoria As New ServicioAuditoria()
        Dim bitacoraClientes As DataTable = auditoria.Buscar("", "Clientes", "CREAR", Date.Today.AddDays(-1), Date.Today)
        Verificar("La bitácora registra la creación de clientes", bitacoraClientes.Rows.Count >= 1)
        Dim bitacoraTodo As DataTable = auditoria.Buscar("", "", "", Date.Today.AddDays(-1), Date.Today)
        Verificar("La bitácora acumula todas las operaciones de la prueba",
                  bitacoraTodo.Rows.Count >= 20, "Registros: " & bitacoraTodo.Rows.Count.ToString())

        If bitacoraTodo.Rows.Count > 0 Then
            Dim idAuditInmutable As Integer = Convert.ToInt32(bitacoraTodo.Rows(0)("ID"))
            Dim errorModificarAudit As Integer = NumeroErrorSql(
                "UPDATE Audit_Log SET Detalle = 'alteración no permitida' WHERE ID = " & idAuditInmutable.ToString())
            Dim errorEliminarAudit As Integer = NumeroErrorSql(
                "DELETE FROM Audit_Log WHERE ID = " & idAuditInmutable.ToString())
            Verificar("La bitácora bloquea modificaciones directas", errorModificarAudit = 1644,
                      "Error MySQL: " & errorModificarAudit.ToString())
            Verificar("La bitácora bloquea eliminaciones directas", errorEliminarAudit = 1644,
                      "Error MySQL: " & errorEliminarAudit.ToString())
        Else
            Verificar("La bitácora bloquea modificaciones directas", False, "No hay registros para probar.")
            Verificar("La bitácora bloquea eliminaciones directas", False, "No hay registros para probar.")
        End If

        Dim tablasAfectadas As DataTable = auditoria.TablasAfectadas()
        Dim incluyeClientes As Boolean = False
        For Each fila As DataRow In tablasAfectadas.Rows
            If UtileriasUI.Texto(fila, "Tabla_Afectada") = "Clientes" Then incluyeClientes = True
        Next
        Verificar("El filtro de tablas afectadas expone 'Clientes'", incluyeClientes)

        Dim rutaCsv As String = Path.Combine(Path.GetTempPath(), "sgln_prueba_auditoria.csv")
        UtileriasUI.ExportarCsv(bitacoraTodo, rutaCsv)
        Verificar("La bitácora se exporta a CSV",
                  File.Exists(rutaCsv) AndAlso New FileInfo(rutaCsv).Length > 0)

        ' ---------- 11. Configuración, feriados y respaldo ----------
        Console.WriteLine()
        Console.WriteLine("-- 11. Configuración, feriados y respaldo")
        Verificar("Guardar una clave de configuración nueva",
                  seguridad.Guardar("Prueba_Clav", "Valor 1").Exito)
        Verificar("La clave guardada se lee de vuelta",
                  seguridad.Obtener("Prueba_Clav") = "Valor 1")
        Verificar("Clave vacía se rechaza", Not seguridad.Guardar("", "x").Exito)
        Verificar("Las claves de carpetas se protegen como reservadas",
                  seguridad.EsClaveReservada("Carpeta_Documentos"))
        Dim resultadoRutaDocumentos As Respuesta = seguridad.Guardar("Carpeta_Documentos", Path.Combine(Path.GetTempPath(), "Documentos"))
        Verificar("No se permite modificar la ruta reservada de documentos", resultadoRutaDocumentos.Exito = False)
        Dim resultadoRutaRespaldos As Respuesta = seguridad.Guardar("carpeta_respaldos", Path.Combine(Path.GetTempPath(), "Respaldos"))
        Verificar("La protección de rutas reservadas no distingue mayúsculas", resultadoRutaRespaldos.Exito = False)
        Dim rutaReservadaRepoBloqueada As Boolean = False
        Try
            Dim repositorioConfigAdmin As New ConfiguracionRepositorio()
            repositorioConfigAdmin.Guardar("Carpeta_Documentos", Path.GetTempPath())
        Catch ex As InvalidOperationException
            rutaReservadaRepoBloqueada = True
        End Try
        Verificar("El repositorio impide eludir la protección de rutas reservadas", rutaReservadaRepoBloqueada)
        Dim catalogoConfigAdmin As New CatalogoRepositorio()
        Dim resultadoCatalogoReservado As Respuesta = catalogoConfigAdmin.GuardarConfig("Carpeta_Respaldos", Path.GetTempPath())
        Verificar("El catálogo heredado también bloquea las rutas reservadas", resultadoCatalogoReservado.Exito = False)

        Verificar("Feriado duplicado (01/01/2027) se rechaza",
                  Not seguridad.AgregarFeriado(New Date(2027, 1, 1), "Duplicado").Exito)
        Dim semanaSanta2029 As Boolean = False
        For Each filaFeriado As DataRow In seguridad.Feriados().Rows
            If Convert.ToDateTime(filaFeriado("Fecha")).Date = New Date(2029, 3, 29) AndAlso
               Convert.ToString(filaFeriado("Descripcion")) = "Jueves Santo" Then
                semanaSanta2029 = True
                Exit For
            End If
        Next
        Verificar("Semana Santa 2029 se calcula y se siembra automáticamente", semanaSanta2029)
        Dim rNuevoFeriado As Respuesta = seguridad.AgregarFeriado(New Date(2026, 12, 31), "Feriado de prueba")
        Verificar("Agregar feriado nuevo", rNuevoFeriado.Exito, rNuevoFeriado.Mensaje)

        Dim calculadora2 As CalculadoraDiasHabiles = plazos.ObtenerCalculadora()
        Verificar("El feriado nuevo excluye el 31/12/2026 del cómputo",
                  Not calculadora2.EsDiaHabil(New Date(2026, 12, 31)))
        Verificar("Vencimiento a 1 día desde 30/12/2026 salta al lunes 04/01/2027",
                  calculadora2.CalcularVencimiento(New Date(2026, 12, 30), 1) = New Date(2027, 1, 4))

        Dim idNuevoFeriado As Integer = 0
        For Each fila As DataRow In seguridad.Feriados().Rows
            If UtileriasUI.Fecha(fila, "Fecha") = New Date(2026, 12, 31) Then
                idNuevoFeriado = UtileriasUI.Entero(fila, "ID")
            End If
        Next
        Dim rEliminarFeriado As Respuesta = seguridad.EliminarFeriado(idNuevoFeriado)
        Verificar("Eliminar el feriado de prueba", rEliminarFeriado.Exito, rEliminarFeriado.Mensaje)
        Verificar("Eliminar un feriado inexistente se rechaza",
                  Not seguridad.EliminarFeriado(Integer.MaxValue).Exito)
        Dim calculadora3 As CalculadoraDiasHabiles = plazos.ObtenerCalculadora()
        Verificar("Sin el feriado, el 31/12/2026 vuelve a ser hábil",
                  calculadora3.EsDiaHabil(New Date(2026, 12, 31)))

        Verificar("La configuración aislada carga la contraseña temporal de respaldos",
                  Not String.IsNullOrWhiteSpace(ConfiguracionConexion.Actual.PasswordRespaldo))
        Dim rRespaldo As Respuesta = seguridad.Respaldar()
        Verificar("El respaldo de la base se crea y se reporta su ruta",
                  rRespaldo.Exito, rRespaldo.Mensaje)
        Verificar("El respaldo exitoso queda registrado en la bitácora",
                  rRespaldo.Exito AndAlso
                  ContarSqlDirecto("SELECT COUNT(*) FROM Audit_Log WHERE Accion = 'RESPALDO' " &
                                   "AND Tabla_Afectada = 'BaseDatos'") > 0)

        ' ---------- 12. IA/OCR sigue deshabilitado ----------
        Console.WriteLine()
        Console.WriteLine("-- 12. IA/OCR deshabilitado")
        Dim ia As New ServicioIA()
        Verificar("El servicio de IA reporta estar deshabilitado", Not ia.Habilitado)
        Dim rIa As Respuesta = ia.ProcesarDocumento(idDocumento)
        Verificar("Procesar con IA devuelve mensaje de fase final sin excepción",
                  Not rIa.Exito AndAlso rIa.Mensaje.Length > 0, rIa.Mensaje)

        ' ---------- Resumen ----------
        Console.WriteLine()
        Console.WriteLine("=====================================================")
        Console.WriteLine(" Pruebas ejecutadas: " & _pruebas.ToString())
        Console.WriteLine(" Fallas: " & _fallos.ToString())
        Console.WriteLine("=====================================================")
        Environment.ExitCode = _fallos
    End Sub

    Private Function DenegadoPorPermiso(accion As Func(Of Object)) As Boolean
        Try
            accion()
            Return False
        Catch ex As UnauthorizedAccessException
            Return True
        End Try
    End Function

    Private Sub Verificar(nombre As String, condicion As Boolean, Optional detalle As String = "")
        _pruebas += 1
        If condicion Then
            Console.WriteLine("  [OK]    " & nombre)
        Else
            _fallos += 1
            Console.WriteLine("  [FALLA] " & nombre)
            If detalle <> "" Then
                Console.WriteLine("          Detalle: " & detalle)
            End If
        End If
    End Sub

    Private Sub IntentarSinExcepcion(nombre As String, accion As Action)
        Try
            accion()
            Verificar(nombre, True)
        Catch ex As Exception
            Verificar(nombre, False, ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Ejecuta una sentencia directa contra la base de pruebas. Se usa solo
    ''' para comprobar que los triggers bloquean también fuera de la aplicación.
    ''' </summary>
    Private Function EjecutarSqlDirecto(consulta As String) As Integer
        Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
            conexion.Open()
            Using comando As New MySqlCommand(consulta, conexion)
                Return comando.ExecuteNonQuery()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Ejecuta una consulta escalar de verificación contra la base temporal.
    ''' </summary>
    Private Function ContarSqlDirecto(consulta As String) As Integer
        Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
            conexion.Open()
            Using comando As New MySqlCommand(consulta, conexion)
                Return Convert.ToInt32(comando.ExecuteScalar())
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Devuelve el número de error MySQL de una sentencia, o 0 si se ejecutó.
    ''' 1644 es el SIGNAL que lanzan los triggers de seguridad.
    ''' </summary>
    Private Function NumeroErrorSql(consulta As String) As Integer
        Try
            EjecutarSqlDirecto(consulta)
            Return 0
        Catch ex As MySqlException
            Return ex.Number
        End Try
    End Function

End Module
