Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient

''' <summary>
''' Prepara la base MySQL al iniciar: verifica la conexión, instala el esquema
''' si la base está vacía (solo posible si el usuario tiene permisos DDL) y
''' siembra catálogos y configuración. La primera cuenta administradora se registra mediante el asistente inicial.
'''
''' En producción el usuario sgln_app solo tiene SELECT, INSERT, UPDATE y DELETE;
''' por eso la instalación normal la hace un administrador con
''' Scripts\Instalacion_Servidor_MySQL.sql. Si el esquema no existe y el usuario
''' no puede crearlo, se detiene con un mensaje que indica qué ejecutar.
''' </summary>
Public Module InicializadorBD

    Private Const RecursoEsquema As String = "SGLN.Esquema_MySQL.sql"
    Public Sub AsegurarBaseDeDatos()
        VerificarConexion()
        If Not TablaExiste("Roles") Then InstalarEsquema()
        VerificarSeguridadInicial()
        SembrarDatosIniciales()
    End Sub

    Private Sub VerificarConexion()
        Try
            Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
                conexion.Open()
            End Using
        Catch ex As Exception
            Throw New InvalidOperationException(
                "No se pudo conectar al servidor MySQL '" & ConexionBD.ConstructorConexion().Server &
                "' con la base '" & ConexionBD.NombreBaseDatos() & "'. " &
                "Verifique que el servicio MySQL esté iniciado, que la base exista y que " &
                "la configuración de conexión (registro de Windows) o App.config tengan " &
                "el servidor y el usuario correctos.", ex)
        End Try
    End Sub

    Private Function TablaExiste(nombre As String) As Boolean
        ' En Windows MySQL guarda los nombres en minúsculas
        ' (lower_case_table_names = 1), por eso se compara sin distinguir mayúsculas.
        Dim total As Integer = EscalarEntero(
            "SELECT COUNT(*) FROM information_schema.tables " &
            "WHERE table_schema = DATABASE() AND LOWER(table_name) = LOWER(@tabla)",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@tabla", nombre)
            End Sub)
        Return total > 0
    End Function

    ''' <summary>
    ''' Ejecuta el esquema embebido. Funciona cuando el usuario tiene permisos
    ''' DDL (equipo de desarrollo o base de pruebas). En producción se espera
    ''' que el esquema ya esté instalado por el administrador.
    ''' </summary>
    Private Sub InstalarEsquema()
        Try
            Dim guion As String = LeerRecurso(RecursoEsquema)
            For Each sentencia As String In DividirSentencias(guion)
                Ejecutar(sentencia, 180)
            Next
        Catch ex As MySqlException
            Throw New InvalidOperationException(
                "La base '" & ConexionBD.NombreBaseDatos() & "' no tiene el esquema instalado y el " &
                "usuario de la aplicación no puede crearlo (permisos DDL)." & Environment.NewLine &
                "Un administrador debe ejecutar, una sola vez y con el usuario root:" & Environment.NewLine &
                "  Scripts\Instalacion_Servidor_MySQL.sql" & Environment.NewLine &
                "  Scripts\Esquema_MySQL.sql" & Environment.NewLine &
                "  Scripts\Triggers_MySQL.sql" & Environment.NewLine & Environment.NewLine &
                "Detalle de MySQL: " & ex.Message, ex)
        End Try
    End Sub

    Private Function LeerRecurso(nombreRecurso As String) As String
        Dim ensamblado As Assembly = Assembly.GetExecutingAssembly()

        If ensamblado.GetManifestResourceStream(nombreRecurso) Is Nothing Then
            nombreRecurso = Array.Find(
                ensamblado.GetManifestResourceNames(),
                Function(nombre As String) nombre.EndsWith("Esquema_MySQL.sql", StringComparison.OrdinalIgnoreCase))
        End If

        If String.IsNullOrWhiteSpace(nombreRecurso) Then
            Throw New InvalidOperationException("No se encontró el recurso embebido del esquema MySQL.")
        End If

        Using flujo As IO.Stream = ensamblado.GetManifestResourceStream(nombreRecurso)
            If flujo Is Nothing Then
                Throw New InvalidOperationException("No se encontró el recurso embebido: " & RecursoEsquema)
            End If
            Using lector As New IO.StreamReader(flujo, Encoding.UTF8)
                Return lector.ReadToEnd()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' El conector ADO no entiende DELIMITER ni lotes grandes: se quitan los
    ''' comentarios y se ejecuta sentencia por sentencia. El esquema no contiene
    ''' procedimientos ni triggers (esos van en Triggers_MySQL.sql).
    ''' </summary>
    Private Function DividirSentencias(guion As String) As List(Of String)
        Dim sinBloques As String = Regex.Replace(guion, "/\*.*?\*/", " ", RegexOptions.Singleline)
        Dim sinLineas As New StringBuilder()

        For Each linea As String In sinBloques.Split(New String() {ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
            If linea.TrimStart().StartsWith("--", StringComparison.Ordinal) Then Continue For
            sinLineas.AppendLine(linea)
        Next

        Dim lista As New List(Of String)
        For Each parte As String In sinLineas.ToString().Split(";"c)
            If Not String.IsNullOrWhiteSpace(parte) Then lista.Add(parte)
        Next
        Return lista
    End Function

    ''' <summary>
    ''' Sin un rol administrador protegido el sistema podría quedar sin acceso
    ''' total. Se detiene con un mensaje claro en lugar de arrancar a medias.
    ''' </summary>
    Private Sub VerificarSeguridadInicial()
        If EscalarEntero("SELECT COUNT(*) FROM Roles WHERE Protegido = 1") = 0 Then
            Throw New InvalidOperationException(
                "No existe un rol administrador protegido. Ejecute Scripts\Esquema_MySQL.sql " &
                "con un usuario administrador para restaurar los roles y permisos base.")
        End If

        If EscalarEntero("SELECT COUNT(*) FROM Permisos") = 0 Then
            Throw New InvalidOperationException(
                "El catálogo de permisos está vacío. Ejecute Scripts\Esquema_MySQL.sql " &
                "con un usuario administrador.")
        End If

        If Not TablaExiste("Intentos_Acceso") Then
            Throw New InvalidOperationException(
                "Falta la tabla de protección contra intentos de acceso. " &
                "Un administrador debe ejecutar Scripts\Migracion_Seguridad_Acceso.sql con permisos DDL.")
        End If
    End Sub

    Private Sub SembrarDatosIniciales()
        SembrarConfiguracion()
        SembrarFeriados()
        SembrarJuzgados()
        SembrarTiposActuacion()
        SembrarPlantillas()
    End Sub

    Public Function HayUsuarios() As Boolean
        Return EscalarEntero("SELECT COUNT(*) FROM Usuarios") > 0
    End Function

    ''' <summary>Alta protegida del primer administrador; evita carreras entre estaciones.</summary>
    Public Sub CrearAdministradorInicial(usuario As String, nombreCompleto As String, clave As String)
        usuario = If(usuario, "").Trim()
        nombreCompleto = If(nombreCompleto, "").Trim()
        If usuario.Length < 3 OrElse usuario.Length > 50 OrElse Not Regex.IsMatch(usuario, "^[A-Za-z0-9._-]+$") Then
            Throw New InvalidOperationException("El usuario debe tener entre 3 y 50 caracteres y usar solo letras, números, punto, guion o guion bajo.")
        End If
        If nombreCompleto.Length = 0 OrElse nombreCompleto.Length > 150 Then
            Throw New InvalidOperationException("Ingrese el nombre completo (máximo 150 caracteres).")
        End If
        Dim errorClave As String = SeguridadClave.ValidarClaveNuevaInicial(clave)
        If errorClave <> "" Then Throw New InvalidOperationException(errorClave)

        Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
            conexion.Open()
            Dim bloqueo As Boolean = False
            Try
                Using cmd As New MySqlCommand("SELECT GET_LOCK('SGLN_BOOTSTRAP_ADMIN', 15)", conexion)
                    bloqueo = Convert.ToInt32(cmd.ExecuteScalar()) = 1
                End Using
                If Not bloqueo Then Throw New InvalidOperationException("No se pudo bloquear la creación inicial. Intente nuevamente.")

                Using transaccion As MySqlTransaction = conexion.BeginTransaction(IsolationLevel.Serializable)
                    Try
                        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM Usuarios", conexion, transaccion)
                            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                                Throw New InvalidOperationException("Ya existe al menos un usuario. La creación inicial quedó deshabilitada.")
                            End If
                        End Using

                        Dim idRol As Integer
                        Using cmd As New MySqlCommand(
                            "SELECT r.ID FROM Roles r WHERE r.Protegido = 1 AND r.Activo = 1 " &
                            "AND EXISTS (SELECT 1 FROM Rol_Permiso rp WHERE rp.ID_Rol = r.ID) LIMIT 1", conexion, transaccion)
                            Dim valor As Object = cmd.ExecuteScalar()
                            If valor Is Nothing OrElse valor Is DBNull.Value Then
                                Throw New InvalidOperationException("No existe un rol administrador protegido con permisos. Revise Esquema_MySQL.sql.")
                            End If
                            idRol = Convert.ToInt32(valor)
                        End Using

                        Dim credencial As SalHash = SeguridadClave.CrearHash(clave)
                        Dim nuevoId As Integer
                        Using cmd As New MySqlCommand(
                            "INSERT INTO Usuarios (Usuario, Nombre_Completo, Clave_Hash, Clave_Salt, ID_Rol, Activo) " &
                            "VALUES (@usuario, @nombre, @hash, @salt, @rol, 1)", conexion, transaccion)
                            cmd.Parameters.AddWithValue("@usuario", usuario)
                            cmd.Parameters.AddWithValue("@nombre", nombreCompleto)
                            cmd.Parameters.AddWithValue("@hash", credencial.Hash)
                            cmd.Parameters.AddWithValue("@salt", credencial.Salt)
                            cmd.Parameters.AddWithValue("@rol", idRol)
                            cmd.ExecuteNonQuery()
                            nuevoId = CInt(cmd.LastInsertedId)
                        End Using

                        Using cmd As New MySqlCommand(
                            "INSERT INTO Audit_Log (Usuario, Accion, Tabla_Afectada, ID_Registro, Detalle) " &
                            "VALUES (@usuario, 'CREAR', 'Usuarios', @id, 'Creación del primer administrador')", conexion, transaccion)
                            cmd.Parameters.AddWithValue("@usuario", usuario)
                            cmd.Parameters.AddWithValue("@id", nuevoId)
                            cmd.ExecuteNonQuery()
                        End Using
                        transaccion.Commit()
                    Catch
                        Try
                            transaccion.Rollback()
                        Catch
                        End Try
                        Throw
                    End Try
                End Using
            Finally
                If bloqueo Then
                    Using cmd As New MySqlCommand("SELECT RELEASE_LOCK('SGLN_BOOTSTRAP_ADMIN')", conexion)
                        cmd.ExecuteScalar()
                    End Using
                End If
            End Try
        End Using
    End Sub

    Private Sub SembrarConfiguracion()
        SembrarConfig("Nombre_Bufete", "Bufete Legal y Notarial")
        SembrarConfig("Umbral_Efectivo", "10000")
        SembrarConfig("Carpeta_Documentos", "Documentos")
        SembrarConfig("Carpeta_Respaldos", "Respaldos")
        SembrarConfig("Protocolo_Dias_Entrega", "15")
        SembrarConfig("LDA_Analisis_Dias", "5")
        SembrarConfig("LDA_Reporte_Dias", "15")
        SembrarConfig("ARCO_Dias_Respuesta", "10")
        SembrarConfig("ARCO_Dias_Prorroga", "5")
        SembrarConfig("Version_Esquema", "2.0")
    End Sub

    Private Sub SembrarConfig(clave As String, valor As String)
        Ejecutar(
            "INSERT IGNORE INTO Configuracion (Clave, Valor) VALUES (@clave, @valor)",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@clave", clave)
                Parametro(comando, "@valor", valor)
            End Sub)
    End Sub

    Private Sub SembrarFeriados()
        For anio As Integer = 2025 To 2028
            SembrarFeriado(New Date(anio, 1, 1), "Año Nuevo")
            SembrarFeriado(New Date(anio, 5, 1), "Día del Trabajo")
            SembrarFeriado(New Date(anio, 8, 6), "Día del Divino Salvador del Mundo")
            SembrarFeriado(New Date(anio, 9, 15), "Día de la Independencia")
            SembrarFeriado(New Date(anio, 11, 2), "Día de los Difuntos")
            SembrarFeriado(New Date(anio, 12, 25), "Navidad")

            Select Case anio
                Case 2025
                    SembrarFeriado(New Date(2025, 4, 17), "Jueves Santo")
                    SembrarFeriado(New Date(2025, 4, 18), "Viernes Santo")
                    SembrarFeriado(New Date(2025, 4, 19), "Sábado Santo")
                Case 2026
                    SembrarFeriado(New Date(2026, 4, 2), "Jueves Santo")
                    SembrarFeriado(New Date(2026, 4, 3), "Viernes Santo")
                    SembrarFeriado(New Date(2026, 4, 4), "Sábado Santo")
                Case 2027
                    SembrarFeriado(New Date(2027, 3, 25), "Jueves Santo")
                    SembrarFeriado(New Date(2027, 3, 26), "Viernes Santo")
                    SembrarFeriado(New Date(2027, 3, 27), "Sábado Santo")
                Case 2028
                    SembrarFeriado(New Date(2028, 4, 13), "Jueves Santo")
                    SembrarFeriado(New Date(2028, 4, 14), "Viernes Santo")
                    SembrarFeriado(New Date(2028, 4, 15), "Sábado Santo")
            End Select
        Next
    End Sub

    Private Sub SembrarFeriado(fecha As Date, descripcion As String)
        Ejecutar(
            "INSERT IGNORE INTO Feriados (Fecha, Descripcion) VALUES (@fecha, @descripcion)",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@fecha", fecha.Date)
                Parametro(comando, "@descripcion", descripcion)
            End Sub)
    End Sub

    Private Sub SembrarJuzgados()
        If EscalarEntero("SELECT COUNT(*) FROM Juzgados") > 0 Then Return

        Dim datos As String()() = {
            New String() {"Juzgado Primero de lo Civil de San Salvador", "Civil", "San Salvador"},
            New String() {"Juzgado Segundo de lo Civil de San Salvador", "Civil", "San Salvador"},
            New String() {"Juzgado Primero de lo Penal de San Salvador", "Penal", "San Salvador"},
            New String() {"Juzgado de lo Laboral de San Salvador", "Laboral", "San Salvador"},
            New String() {"Juzgado de Familia de San Salvador", "Familia", "San Salvador"},
            New String() {"Cámara Primera de lo Civil de la Primera Sección de Occidente", "Civil", "Santa Ana"}
        }

        For Each fila As String() In datos
            Ejecutar(
                "INSERT INTO Juzgados (Nombre, Materia, Distrito, Activo) VALUES (@nombre, @materia, @distrito, 1)",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nombre", fila(0))
                    Parametro(comando, "@materia", fila(1))
                    Parametro(comando, "@distrito", fila(2))
                End Sub)
        Next
    End Sub

    Private Sub SembrarTiposActuacion()
        If EscalarEntero("SELECT COUNT(*) FROM Tipos_Actuacion") > 0 Then Return

        Dim datos As Object()() = {
            New Object() {"Contestación de demanda", 15, "Civil"},
            New Object() {"Apelación de sentencia", 10, "Civil"},
            New Object() {"Recurso de revocatoria", 3, "Civil"},
            New Object() {"Recurso de nulidad", 5, "Civil"},
            New Object() {"Incidente", 5, "Civil"},
            New Object() {"Otro (verificar plazo legal)", 5, "General"}
        }

        For Each fila As Object() In datos
            Ejecutar(
                "INSERT INTO Tipos_Actuacion (Nombre, Plazo_Dias_Default, Materia, Activo) VALUES (@nombre, @plazo, @materia, 1)",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nombre", fila(0))
                    Parametro(comando, "@plazo", fila(1))
                    Parametro(comando, "@materia", fila(2))
                End Sub)
        Next
    End Sub

    Private Sub SembrarPlantillas()
        If EscalarEntero("SELECT COUNT(*) FROM Plantillas_Documentos") > 0 Then Return

        Dim nombres As String() = {"Compraventa de Vehículo", "Poder General", "Constitución de Sociedad"}
        For Each nombre As String In nombres
            Ejecutar(
                "INSERT INTO Plantillas_Documentos (Nombre, Tipo, Ruta_Plantilla, Activa) VALUES (@nombre, 'Escritura', NULL, 1)",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@nombre", nombre)
                End Sub)
        Next
    End Sub

    Private Function EscalarEntero(consulta As String,
                                   Optional parametros As Action(Of MySqlCommand) = Nothing) As Integer
        Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
            conexion.Open()
            Using comando As New MySqlCommand(consulta, conexion)
                If parametros IsNot Nothing Then parametros(comando)
                Return Convert.ToInt32(comando.ExecuteScalar())
            End Using
        End Using
    End Function

    Private Sub Ejecutar(consulta As String, Optional tiempoEspera As Integer = 60,
                         Optional parametros As Action(Of MySqlCommand) = Nothing)
        Using conexion As New MySqlConnection(ConexionBD.CadenaConexion)
            conexion.Open()
            Using comando As New MySqlCommand(consulta, conexion)
                comando.CommandTimeout = tiempoEspera
                If parametros IsNot Nothing Then parametros(comando)
                comando.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Sobrecarga para las llamadas que solo pasan parámetros. Sin ella, VB
    ''' intenta convertir la lambda en el tiempo de espera (Integer).
    ''' </summary>
    Private Sub Ejecutar(consulta As String, parametros As Action(Of MySqlCommand))
        Ejecutar(consulta, 60, parametros)
    End Sub

    ''' <summary>
    ''' Los helpers de parámetros viven en RepositorioBase (clase). El
    ''' inicializador es un módulo, así que repite el alta de parámetros con
    ''' el mismo criterio de tipos.
    ''' </summary>
    Private Sub Parametro(comando As MySqlCommand, nombre As String, valor As Object)
        Dim tipo As MySqlDbType = MySqlDbType.VarString
        Dim contenido As Object = valor

        If valor Is Nothing Then
            contenido = DBNull.Value
        ElseIf TypeOf valor Is Boolean Then
            tipo = MySqlDbType.Byte
            contenido = If(DirectCast(valor, Boolean), 1, 0)
        ElseIf TypeOf valor Is Integer Then
            tipo = MySqlDbType.Int32
        ElseIf TypeOf valor Is Decimal Then
            tipo = MySqlDbType.Decimal
        ElseIf TypeOf valor Is Date Then
            tipo = MySqlDbType.DateTime
        End If

        comando.Parameters.Add(New MySqlParameter(nombre, tipo) With {.Value = contenido})
    End Sub

End Module
