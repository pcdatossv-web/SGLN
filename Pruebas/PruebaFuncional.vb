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

        Sesion.Limpiar()
        Dim operacionSinSesion As Respuesta = New ServicioUsuarios().Desactivar(1)
        Verificar("La capa de servicios deniega operaciones sin sesión autenticada",
                  Not operacionSinSesion.Exito AndAlso operacionSinSesion.Mensaje.Contains("No tiene permiso"))

        ' ---------- 1. Inicialización de la base de pruebas ----------
        Console.WriteLine("-- 1. Inicialización de la base de datos")
        Dim tlsSinCaRechazado As Boolean = False
        Try
            Dim datosTls As New DatosConexion With {
                .Servidor = "localhost", .Puerto = 3306, .BaseDatos = "sgln_sv",
                .Usuario = "sgln_app", .Password = "Prueba",
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
            preferredProduccionRechazado = ex.Message.Contains("Preferred solo se permite")
        End Try
        Verificar("TLS Preferred se rechaza en la base de producción", preferredProduccionRechazado)

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
        IntentarSinExcepcion("El inicializador verifica el esquema y siembra catálogos y configuración",
            Sub()
                InicializadorBD.AsegurarBaseDeDatos()
            End Sub)

        ' ---------- 2. Autenticación y seguridad ----------
        Console.WriteLine()
        Console.WriteLine("-- 2. Autenticación y cambio de clave")
        Dim usuarios As New UsuarioRepositorio()
        If Not InicializadorBD.HayUsuarios() Then InicializadorBD.CrearAdministradorInicial("admin", "Administrador de pruebas", "PruebaInicial99!")
        Dim admin As UsuarioSesion = usuarios.Autenticar("admin", "PruebaInicial99!")
        If admin IsNot Nothing Then
            Sesion.UsuarioActual = admin
            Sesion.CargarPermisos()
        End If
        Verificar("Login acepta la cuenta creada por el asistente inicial", admin IsNot Nothing)
        Verificar("La cuenta creada con clave propia no requiere cambio forzado",
                  admin IsNot Nothing AndAlso Not admin.DebeCambiarClave)

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
        Verificar("Una clave nueva de primer ingreso exige longitud y complejidad",
                  SeguridadClave.ValidarClaveNuevaInicial("ClaveInicial99!") = "")
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

        Dim permisosAdmin As List(Of String) = roles.PermisosDeRol(idRolAdmin)
        Verificar("El Administrador tiene todos los permisos del catálogo",
                  permisosAdmin.Count = roles.CatalogoPermisos().Rows.Count,
                  "Permisos: " & permisosAdmin.Count.ToString())
        Verificar("El Administrador conserva config.usuarios y config.roles",
                  permisosAdmin.Contains("config.usuarios") AndAlso permisosAdmin.Contains("config.roles"))

        Sesion.CargarPermisos()
        Verificar("La sesión se revalida contra la cuenta y rol vigentes", Permisos.TieneAcceso("config.usuarios"))
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
                      New List(Of String) From {"config.usuarios"}).Exito)
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

        ' ---------- 2d. Limpieza de los datos de seguridad temporales ----------
        EjecutarSqlDirecto("DELETE FROM Usuarios WHERE ID = " & idUsuarioPrueba.ToString())
        Verificar("El rol de prueba se elimina cuando ya no tiene usuarios",
                  roles.Eliminar(idRolPrueba).Exito)
        EjecutarSqlDirecto("DELETE FROM Usuarios WHERE ID = " & idAdmin2.ToString())

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

        Dim idCliente As Integer = 0
        Dim rCliente As Respuesta = clientes.Guardar(cliente, perfil)
        Verificar("Guardar cliente válido", rCliente.Exito, rCliente.Mensaje)
        If rCliente.ID.HasValue Then idCliente = rCliente.ID.Value

        Dim tablaCliente As DataTable = clientes.Obtener(idCliente)
        Verificar("El cliente se recupera por ID", tablaCliente.Rows.Count = 1)
        If tablaCliente.Rows.Count = 1 Then
            Verificar("El DUI se normaliza al formato 00000000-0",
                      Convert.ToString(tablaCliente.Rows(0)("DUI")) = "00000000-0")
            Verificar("El NIT se normaliza con separadores",
                      Convert.ToString(tablaCliente.Rows(0)("NIT")).Contains("-"))
            Verificar("El perfil LDA/FT quedó guardado con su nivel de riesgo",
                      Convert.ToString(tablaCliente.Rows(0)("Nivel_Riesgo")) = "Medio")
        End If
        Verificar("La búsqueda por apellido encuentra al cliente",
                  clientes.Buscar("Pérez").Rows.Count >= 1)
        Verificar("La búsqueda sin filtro lista los clientes (grid inicial)",
                  clientes.Buscar("").Rows.Count >= 1)

        Verificar("DUI con menos de 9 dígitos se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y", .DUI = "123"}, Nothing).Exito)
        Verificar("NIT con menos de 14 dígitos se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y", .NIT = "123"}, Nothing).Exito)
        Verificar("Email sin arroba se rechaza",
                  Not clientes.Guardar(New Cliente With {.Nombres = "X", .Apellidos = "Y", .Email = "correo"}, Nothing).Exito)
        Verificar("Nombres vacíos se rechazan",
                  Not clientes.Guardar(New Cliente With {.Apellidos = "Y"}, Nothing).Exito)
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

        Verificar("NIT inválido en entidad se rechaza",
                  Not entidades.Guardar(New EntidadLegal With {.RazonSocial = "X", .NIT = "12"}, Nothing).Exito)
        Verificar("Razón social vacía se rechaza",
                  Not entidades.Guardar(New EntidadLegal(), Nothing).Exito)

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

        Dim rRep As Respuesta = entidades.AgregarRepresentante(idEntidad, idCliente, "Presidenta de la junta directiva", "Judicial y extrajudicial")
        Verificar("Agregar representante a la entidad", rRep.Exito, rRep.Mensaje)
        Dim rRepDup As Respuesta = entidades.AgregarRepresentante(idEntidad, idCliente, "Auditor", "Judicial")
        Verificar("Representante duplicado se rechaza con mensaje",
                  Not rRepDup.Exito AndAlso rRepDup.Mensaje.Length > 0, rRepDup.Mensaje)
        Verificar("El listado de representantes muestra al agregado",
                  entidades.Representantes(idEntidad).Rows.Count = 1)
        Verificar("La búsqueda de entidades sin filtro lista resultados",
                  entidades.Buscar("").Rows.Count >= 1)

        ' ---------- 5. Casos ----------
        Console.WriteLine()
        Console.WriteLine("-- 5. Casos judiciales")
        Dim casos As New ServicioCasos()
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

        Dim tablaActuaciones As DataTable = plazos.Actuaciones(idCaso)
        Dim guardada As DataRow = Nothing
        For Each fila As DataRow In tablaActuaciones.Rows
            If UtileriasUI.Entero(fila, "ID") = idActuacion Then guardada = fila
        Next
        Verificar("La actuación confirmada queda en el expediente", guardada IsNot Nothing)
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

        plazos.MarcarCumplida(idActuacion)
        Dim filaCumplida As DataRow = Nothing
        For Each fila As DataRow In plazos.Actuaciones(idCaso).Rows
            If UtileriasUI.Entero(fila, "ID") = idActuacion Then filaCumplida = fila
        Next
        Verificar("Marcar cumplida cambia el estado a Cumplido",
                  filaCumplida IsNot Nothing AndAlso UtileriasUI.Texto(filaCumplida, "Estado") = "Cumplido")

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

        Verificar("Libro duplicado (número y año) se rechaza con mensaje",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 1, .AnioVigencia = 2026,
                      .FechaAutorizacion = New Date(2026, 1, 5),
                      .FechaVencimiento = New Date(2027, 1, 5)}).Exito)
        Verificar("Libro con número cero se rechaza",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 0, .AnioVigencia = 2026,
                      .FechaAutorizacion = Date.Today, .FechaVencimiento = Date.Today.AddYears(1)}).Exito)
        Verificar("Vencimiento anterior a la autorización se rechaza",
                  Not protocolo.GuardarLibro(New LibroProtocolo With {
                      .NumeroLibro = 2, .AnioVigencia = 2026,
                      .FechaAutorizacion = New Date(2026, 6, 1),
                      .FechaVencimiento = New Date(2026, 1, 1)}).Exito)

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

        Verificar("Traslape de folios se rechaza",
                  Not protocolo.GuardarInstrumento(New Instrumento With {
                      .IDLibro = idLibro, .NumeroEscritura = 3,
                      .FechaHoraOtorgamiento = Date.Now,
                      .FolioInicio = 10, .FolioFin = 12,
                      .ObjetoActo = "Prueba de traslape"}).Exito)
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
                        (New ServicioProtocolo()).GuardarInstrumento(New Instrumento With {
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
                        (New ServicioProtocolo()).GuardarInstrumento(New Instrumento With {
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

        Dim rComp1 As Respuesta = protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Otorgante")
        Verificar("Agregar compareciente otorgante a la escritura 1", rComp1.Exito, rComp1.Mensaje)
        Dim rCompDup As Respuesta = protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Otorgante")
        Verificar("Compareciente duplicado se rechaza con mensaje", Not rCompDup.Exito, rCompDup.Mensaje)
        Verificar("Calidad fuera de catálogo se rechaza",
                  Not protocolo.AgregarCompareciente(idInstrumento1, idCliente, "Curioso").Exito)

        Dim rCerrar As Respuesta = protocolo.CerrarLibro(idLibro, "Libro completo")
        Verificar("Cerrar libro con escritura sin otorgante se bloquea",
                  Not rCerrar.Exito AndAlso rCerrar.Mensaje.Contains("otorgante"), rCerrar.Mensaje)
        Verificar("El índice no se genera mientras el libro sigue vigente",
                  Not protocolo.GenerarIndice(idLibro).Exito)

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
        Verificar("Testimonio con fecha futura se rechaza",
                  Not protocolo.ExpedirTestimonio(idInstrumento1, "Primer", Date.Today.AddDays(1), "X").Exito)

        Dim rAnular As Respuesta = protocolo.AnularInstrumento(idInstrumento2)
        Verificar("Anular escritura 2 conserva el registro", rAnular.Exito, rAnular.Mensaje)
        Verificar("No se expiden testimonios de escritura anulada",
                  Not protocolo.ExpedirTestimonio(idInstrumento2, "Ulterior", Date.Today, "X").Exito)

        Dim rEntregaAnterior As Respuesta = protocolo.RegistrarEntrega(idLibro, Date.Today.AddDays(-1))
        Verificar("La fecha de entrega no puede ser anterior al cierre", Not rEntregaAnterior.Exito)
        Dim rEntregaFutura As Respuesta = protocolo.RegistrarEntrega(idLibro, Date.Today.AddDays(1))
        Verificar("Entrega al Notariado con fecha futura se rechaza", Not rEntregaFutura.Exito)
        Dim rEntrega As Respuesta = protocolo.RegistrarEntrega(idLibro, Date.Today)
        Verificar("Registrar entrega del libro cerrado", rEntrega.Exito, rEntrega.Mensaje)

        Dim rIndice As Respuesta = protocolo.GenerarIndice(idLibro)
        Verificar("Generar índice del libro", rIndice.Exito, rIndice.Mensaje)
        Dim indice As DataTable = protocolo.IndiceLibro(idLibro)
        Verificar("El índice contiene las 2 escrituras", indice.Rows.Count = 2)
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
        File.WriteAllText(rutaPdf, "Contenido simulado de un PDF de prueba.")

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

        Dim rMeta As Respuesta = documentos.ActualizarMetadatos(idDocumento, "Escritura pública", Nothing, idInstrumento1)
        Verificar("Actualizar metadatos del documento", rMeta.Exito, rMeta.Mensaje)
        Dim tablaDoc2 As DataTable = documentos.Obtener(idDocumento)
        Verificar("El documento queda vinculado a la escritura 1",
                  UtileriasUI.Entero(tablaDoc2.Rows(0), "ID_Instrumento") = idInstrumento1)

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
        Dim rReporte As Respuesta = cumplimiento.MarcarReportada(operacion.ID, Date.Today)
        Verificar("Registrar reporte a la UIF", rReporte.Exito, rReporte.Mensaje)

        Dim rDescartar As Respuesta = cumplimiento.Descartar(sospechosa.ID)
        Verificar("Descartar operación conservando el registro", rDescartar.Exito, rDescartar.Mensaje)
        Verificar("No se puede reportar una operación descartada",
                  Not cumplimiento.MarcarReportada(sospechosa.ID, Date.Today).Exito)

        Dim arco As New SolicitudArco With {
            .IDCliente = idCliente, .TipoDerecho = "Acceso",
            .FechaSolicitud = Date.Today,
            .Detalle = "Solicitud de copia de datos personales"}
        Dim rArco As Respuesta = cumplimiento.GuardarSolicitud(arco)
        Verificar("Guardar solicitud ARCO de acceso", rArco.Exito, rArco.Mensaje)
        Verificar("La búsqueda de solicitudes ARCO sin filtro lista resultados",
                  cumplimiento.SolicitudesArco("").Rows.Count >= 1)
        Verificar("El vencimiento ARCO se calcula a 10 días hábiles",
                  UtileriasUI.Fecha(cumplimiento.ObtenerSolicitud(arco.ID).Rows(0), "Fecha_Vencimiento") =
                  calculadoraFeriados.CalcularVencimiento(Date.Today, 10))

        Dim arcoProrroga As New SolicitudArco With {
            .IDCliente = idCliente, .TipoDerecho = "Rectificación",
            .FechaSolicitud = Date.Today, .ProrrogaAplicada = True,
            .Detalle = "Solicitud con prórroga"}
        Dim rProrroga As Respuesta = cumplimiento.GuardarSolicitud(arcoProrroga)
        Verificar("Guardar solicitud ARCO con prórroga", rProrroga.Exito, rProrroga.Mensaje)
        Dim tablaProrroga As DataTable = cumplimiento.ObtenerSolicitud(arcoProrroga.ID)
        Verificar("La prórroga agrega 5 días hábiles al vencimiento",
                  UtileriasUI.Fecha(tablaProrroga.Rows(0), "Fecha_Vencimiento_Prorroga") =
                  calculadoraFeriados.CalcularVencimiento(
                      UtileriasUI.Fecha(tablaProrroga.Rows(0), "Fecha_Vencimiento").Value, 5))

        Verificar("Solicitud ARCO con fecha futura se rechaza",
                  Not cumplimiento.GuardarSolicitud(New SolicitudArco With {
                      .IDCliente = idCliente, .TipoDerecho = "Acceso",
                      .FechaSolicitud = Date.Today.AddDays(1)}).Exito)
        Verificar("Responder antes de la fecha de solicitud se rechaza",
                  Not cumplimiento.ResponderSolicitud(arco.ID, Date.Today.AddDays(-5)).Exito)
        Dim rResponder As Respuesta = cumplimiento.ResponderSolicitud(arco.ID, Date.Today)
        Verificar("Responder solicitud ARCO", rResponder.Exito, rResponder.Mensaje)

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

        Verificar("Feriado duplicado (01/01/2027) se rechaza",
                  Not seguridad.AgregarFeriado(New Date(2027, 1, 1), "Duplicado").Exito)
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
        Dim calculadora3 As CalculadoraDiasHabiles = plazos.ObtenerCalculadora()
        Verificar("Sin el feriado, el 31/12/2026 vuelve a ser hábil",
                  calculadora3.EsDiaHabil(New Date(2026, 12, 31)))

        Verificar("La configuración aislada carga la contraseña temporal de respaldos",
                  Not String.IsNullOrWhiteSpace(ConfiguracionConexion.Actual.PasswordRespaldo))
        Dim rRespaldo As Respuesta = seguridad.Respaldar()
        Verificar("El respaldo de la base se crea y se reporta su ruta",
                  rRespaldo.Exito, rRespaldo.Mensaje)

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
