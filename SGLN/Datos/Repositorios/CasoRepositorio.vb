Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de casos judiciales y actuaciones procesales.
''' </summary>
Public Class CasoRepositorio
    Inherits RepositorioBase

    Public Function Buscar(filtro As String) As DataTable
        If Not Permisos.TieneAcceso("casos.ver") Then Return New DataTable()
        Dim texto As String = If(filtro, "").Trim()
        Return Consultar(
            "SELECT c.ID, c.Numero_Expediente, CONCAT(cl.Nombres, ' ', cl.Apellidos) AS Cliente, " &
            "       c.Materia, IFNULL(j.Nombre, '') AS Juzgado, c.Estado, c.Fecha_Apertura " &
            "FROM Casos c " &
            "INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "LEFT JOIN Juzgados j ON j.ID = c.ID_Juzgado " &
            "WHERE (@filtro IS NULL OR c.Numero_Expediente LIKE @like " &
            "       OR cl.Nombres LIKE @like OR cl.Apellidos LIKE @like) " &
            "ORDER BY c.Fecha_Apertura DESC, c.Numero_Expediente",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
            End Sub)
    End Function

    ''' <summary>
    ''' Comprueba una referencia de caso sin conceder acceso general al expediente.
    ''' Solo se usa al vincular documentos y no devuelve datos del caso.
    ''' </summary>
    Public Function ExisteParaVinculoDocumento(id As Integer) As Boolean
        If id <= 0 OrElse Not (Permisos.TieneAcceso("documentos.importar") OrElse
                               Permisos.TieneAcceso("documentos.metadatos")) Then Return False
        Return Consultar("SELECT ID FROM Casos WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub).Rows.Count > 0
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If id <= 0 OrElse Not Permisos.TieneAcceso("casos.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT c.*, CONCAT(cl.Nombres, ' ', cl.Apellidos) AS Cliente " &
            "FROM Casos c INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "WHERE c.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function Insertar(caso As Caso) As Integer
        If Not Permisos.TieneAcceso("casos.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar casos.")
        If caso Is Nothing Then Throw New ArgumentNullException(NameOf(caso))
        If caso.ID <> 0 Then Throw New ArgumentException("Para insertar un caso nuevo, el identificador debe ser cero.", NameOf(caso))
        ValidarDatosCaso(caso)
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                ValidarClienteActivo(conexion, transaccion, caso.IDCliente)
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Casos (ID_Cliente, Numero_Expediente, Materia, ID_Juzgado, Descripcion, Estado, Fecha_Apertura, Responsable) " &
                    "VALUES (@cliente, @expediente, @materia, @juzgado, @descripcion, @estado, @apertura, @responsable)",
                    Sub(comando As MySqlCommand)
                        AgregarParametros(comando, caso)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Casos", nuevoId, caso.NumeroExpediente)
                Return nuevoId
            End Function)
    End Function

    Public Function Actualizar(caso As Caso) As Integer
        If Not Permisos.TieneAcceso("casos.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar casos.")
        If caso Is Nothing OrElse caso.ID <= 0 Then Throw New ArgumentException("El caso seleccionado no es válido.", NameOf(caso))
        ValidarDatosCaso(caso)
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim clienteActual As Integer = 0
                Dim expedienteActual As String = Nothing
                Using comandoExiste As New MySqlCommand(
                    "SELECT ID_Cliente, Numero_Expediente FROM Casos WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(comandoExiste, "@id", caso.ID)
                    Using lector As MySqlDataReader = comandoExiste.ExecuteReader()
                        If Not lector.Read() Then Return 0
                        clienteActual = Convert.ToInt32(lector("ID_Cliente"))
                        expedienteActual = Convert.ToString(lector("Numero_Expediente"))
                    End Using
                End Using

                If caso.IDCliente <> clienteActual Then
                    ValidarClienteActivo(conexion, transaccion, caso.IDCliente)
                End If

                Dim referenciasHistoricas As Integer
                Using comandoReferencias As New MySqlCommand(
                    "SELECT " &
                    " (SELECT COUNT(*) FROM Actuaciones_Procesales WHERE ID_Caso = @idActuaciones) + " &
                    " (SELECT COUNT(*) FROM Documentos WHERE ID_Caso = @idDocumentos) + " &
                    " (SELECT COUNT(*) FROM Operaciones_LDAFT WHERE ID_Caso = @idOperaciones)",
                    conexion, transaccion)
                    Parametro(comandoReferencias, "@idActuaciones", caso.ID)
                    Parametro(comandoReferencias, "@idDocumentos", caso.ID)
                    Parametro(comandoReferencias, "@idOperaciones", caso.ID)
                    referenciasHistoricas = Convert.ToInt32(comandoReferencias.ExecuteScalar())
                End Using

                If referenciasHistoricas > 0 AndAlso
                   (caso.IDCliente <> clienteActual OrElse
                    Not String.Equals(If(caso.NumeroExpediente, "").Trim(), expedienteActual,
                                      StringComparison.OrdinalIgnoreCase)) Then
                    Throw New InvalidOperationException(
                        "No se puede cambiar el cliente ni el número de expediente de un caso con actuaciones, documentos u operaciones históricas. Use el procedimiento autorizado de rectificación.")
                End If

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Casos SET ID_Cliente = @cliente, Numero_Expediente = @expediente, Materia = @materia, " &
                    "ID_Juzgado = @juzgado, Descripcion = @descripcion, Estado = @estado, Fecha_Apertura = @apertura, " &
                    "Responsable = @responsable WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        AgregarParametros(comando, caso)
                        Parametro(comando, "@id", caso.ID)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Casos", caso.ID, caso.NumeroExpediente)
                Return filas
            End Function)
    End Function

    Public Function ActuacionesPorCaso(idCaso As Integer) As DataTable
        If idCaso <= 0 OrElse Not Permisos.TieneAcceso("casos.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT a.ID, a.ID_Tipo_Actuacion, t.Nombre AS Tipo, a.Fecha_Notificacion, " &
            "       a.Plazo_Legal_Dias, a.Fecha_Vencimiento_Calculada, a.Estado, a.Observaciones, " &
            "       a.Fecha_Confirmacion, a.Usuario_Confirma " &
            "FROM Actuaciones_Procesales a " &
            "INNER JOIN Tipos_Actuacion t ON t.ID = a.ID_Tipo_Actuacion " &
            "WHERE a.ID_Caso = @id " &
            "ORDER BY a.Fecha_Notificacion DESC, a.ID DESC",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idCaso)
            End Sub)
    End Function

    Public Function GuardarActuacion(actuacion As ActuacionProcesal) As Integer
        If Not Permisos.TieneAcceso("casos.confirmar_plazo") Then Throw New UnauthorizedAccessException("No tiene permiso para confirmar plazos.")
        If actuacion Is Nothing Then Throw New ArgumentNullException(NameOf(actuacion))
        If actuacion.ID < 0 Then Throw New ArgumentException("El identificador de la actuación no es válido.", NameOf(actuacion))
        ValidarDatosActuacion(actuacion)
        actuacion.Observaciones = If(actuacion.Observaciones, "").Trim()
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                If actuacion.ID = 0 Then
                    Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                        "INSERT INTO Actuaciones_Procesales (ID_Caso, ID_Tipo_Actuacion, Fecha_Notificacion, " &
                        "Plazo_Legal_Dias, Fecha_Vencimiento_Calculada, Estado, Fecha_Confirmacion, Usuario_Confirma, Observaciones) " &
                        "VALUES (@caso, @tipo, @notificacion, @plazo, @vencimiento, 'Pendiente', NOW(), @usuario, @observaciones)",
                        Sub(comando As MySqlCommand)
                            AgregarParametrosActuacion(comando, actuacion)
                        End Sub)
                    AuditarEnTransaccion(conexion, transaccion, "CONFIRMAR PLAZO", "Actuaciones_Procesales", nuevoId,
                        "Vencimiento " & If(actuacion.FechaVencimiento.HasValue, actuacion.FechaVencimiento.Value.ToString("dd/MM/yyyy"), "N/D"))
                    Return nuevoId
                End If

                ' Bloquear y comprobar la existencia antes de actualizar. Sin esta
                ' comprobación, un ID inexistente podía devolverse como exitoso.
                Using comandoExiste As New MySqlCommand(
                    "SELECT ID FROM Actuaciones_Procesales WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(comandoExiste, "@id", actuacion.ID)
                    If comandoExiste.ExecuteScalar() Is Nothing Then Return 0
                End Using

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Actuaciones_Procesales SET ID_Tipo_Actuacion = @tipo, Fecha_Notificacion = @notificacion, " &
                    "Plazo_Legal_Dias = @plazo, Fecha_Vencimiento_Calculada = @vencimiento, " &
                    "Fecha_Confirmacion = NOW(), Usuario_Confirma = @usuario, Observaciones = @observaciones " &
                    "WHERE ID = @id AND Estado IN ('Pendiente', 'Vencido')",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosActuacion(comando, actuacion)
                        Parametro(comando, "@id", actuacion.ID)
                    End Sub)
                ' Solo se audita y confirma cuando la actuación sigue pendiente o vencida.
                ' Una actuación cumplida no puede modificarse ni reportarse como confirmada.
                If filas <= 0 Then Return 0

                ' La confirmación es un evento de negocio y debe quedar auditada
                ' incluso si los datos calculados coinciden con los ya almacenados.
                AuditarEnTransaccion(conexion, transaccion, "CONFIRMAR PLAZO", "Actuaciones_Procesales",
                    actuacion.ID, "Vencimiento " & If(actuacion.FechaVencimiento.HasValue, actuacion.FechaVencimiento.Value.ToString("dd/MM/yyyy"), "N/D"))
                Return actuacion.ID
            End Function)
    End Function

    Public Function MarcarVencidas() As Integer
        If Not Permisos.TieneAcceso("casos.cumplir_plazo") Then Throw New UnauthorizedAccessException("No tiene permiso para actualizar plazos.")
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Actuaciones_Procesales SET Estado = 'Vencido' " &
                    "WHERE Estado = 'Pendiente' AND Fecha_Vencimiento_Calculada IS NOT NULL " &
                    "AND Fecha_Vencimiento_Calculada < CURDATE()")
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ACTUALIZAR", "Actuaciones_Procesales",
                    Nothing, filas & " plazos marcados como vencidos")
                Return filas
            End Function)
    End Function

    Public Function MarcarCumplida(id As Integer) As Integer
        If Not Permisos.TieneAcceso("casos.cumplir_plazo") Then Throw New UnauthorizedAccessException("No tiene permiso para marcar plazos cumplidos.")
        If id <= 0 Then Return 0
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Actuaciones_Procesales SET Estado = 'Cumplido' WHERE ID = @id AND Estado IN ('Pendiente', 'Vencido')",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "CUMPLIR PLAZO", "Actuaciones_Procesales",
                    id, "Actuación marcada como cumplida")
                Return filas
            End Function)
    End Function

    Public Function ClientesCombo() As DataTable
        If Not (Permisos.TieneAcceso("casos.ver") OrElse Permisos.TieneAcceso("casos.gestionar")) Then Return New DataTable()
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos) AS Nombre FROM Clientes " &
            "WHERE Activo = 1 ORDER BY Apellidos, Nombres")
    End Function

    Private Sub ValidarClienteActivo(conexion As MySqlConnection,
                                    transaccion As MySqlTransaction,
                                    idCliente As Integer)
        Using comando As New MySqlCommand(
            "SELECT Activo FROM Clientes WHERE ID = @id FOR UPDATE",
            conexion, transaccion)
            Parametro(comando, "@id", idCliente)
            Dim valor As Object = comando.ExecuteScalar()
            If valor Is Nothing OrElse Convert.IsDBNull(valor) Then
                Throw New InvalidOperationException("El cliente asociado al caso no existe.")
            End If
            If Convert.ToInt32(valor) <> 1 Then
                Throw New InvalidOperationException("No se pueden crear casos ni reasignar expedientes a clientes inactivos.")
            End If
        End Using
    End Sub

    Private Sub ValidarDatosCaso(caso As Caso)
        If caso.IDCliente <= 0 Then Throw New ArgumentException("Seleccione un cliente válido para el caso.", NameOf(caso))
        If caso.IDJuzgado.HasValue AndAlso caso.IDJuzgado.Value <= 0 Then
            Throw New ArgumentException("El juzgado seleccionado no es válido.", NameOf(caso))
        End If
        If String.IsNullOrWhiteSpace(caso.NumeroExpediente) OrElse caso.NumeroExpediente.Trim().Length > 80 Then
            Throw New ArgumentException("El número de expediente debe contener entre 1 y 80 caracteres.", NameOf(caso))
        End If
        If String.IsNullOrWhiteSpace(caso.Materia) OrElse caso.Materia.Trim().Length > 40 Then
            Throw New ArgumentException("La materia debe contener entre 1 y 40 caracteres.", NameOf(caso))
        End If
        If If(caso.Descripcion, "").Length > 1000 Then Throw New ArgumentException("La descripción no puede superar 1000 caracteres.", NameOf(caso))
        If If(caso.Responsable, "").Length > 120 Then Throw New ArgumentException("El responsable no puede superar 120 caracteres.", NameOf(caso))
        If Array.IndexOf(New String() {"Activo", "Suspendido", "Cerrado", "Archivado"}, If(caso.Estado, "")) < 0 Then
            Throw New ArgumentException("El estado del caso no es válido.", NameOf(caso))
        End If
        If caso.FechaApertura = Date.MinValue OrElse caso.FechaApertura.Date.Year < 1000 OrElse caso.FechaApertura.Date > Date.Today Then
            Throw New ArgumentException("La fecha de apertura no puede estar vacía ni ser futura.", NameOf(caso))
        End If
        caso.NumeroExpediente = caso.NumeroExpediente.Trim()
        caso.Materia = caso.Materia.Trim()
        caso.Descripcion = If(caso.Descripcion, "").Trim()
        caso.Responsable = If(caso.Responsable, "").Trim()
    End Sub

    Private Sub AgregarParametros(comando As MySqlCommand, caso As Caso)
        Parametro(comando, "@cliente", caso.IDCliente)
        Parametro(comando, "@expediente", caso.NumeroExpediente)
        Parametro(comando, "@materia", caso.Materia)
        Parametro(comando, "@juzgado", If(caso.IDJuzgado.HasValue, CObj(caso.IDJuzgado.Value), DBNull.Value))
        Parametro(comando, "@descripcion", caso.Descripcion)
        Parametro(comando, "@estado", caso.Estado)
        Parametro(comando, "@apertura", caso.FechaApertura)
        Parametro(comando, "@responsable", caso.Responsable)
    End Sub

    Private Sub ValidarDatosActuacion(actuacion As ActuacionProcesal)
        If actuacion.IDCaso <= 0 OrElse actuacion.IDTipoActuacion <= 0 Then
            Throw New ArgumentException("Seleccione un caso y un tipo de actuación válidos.", NameOf(actuacion))
        End If
        If actuacion.PlazoLegalDias <= 0 OrElse actuacion.PlazoLegalDias > CalculadoraDiasHabiles.MaxDiasHabilesPermitidos Then
            Throw New ArgumentException("El plazo legal debe estar entre 1 y " & CalculadoraDiasHabiles.MaxDiasHabilesPermitidos.ToString() & " días hábiles.", NameOf(actuacion))
        End If
        If actuacion.FechaNotificacion = Date.MinValue OrElse actuacion.FechaNotificacion.Date.Year < 1000 OrElse actuacion.FechaNotificacion.Date > Date.Today Then
            Throw New ArgumentException("La fecha de notificación no puede estar vacía ni ser futura.", NameOf(actuacion))
        End If
        If Not actuacion.FechaVencimiento.HasValue OrElse
           actuacion.FechaVencimiento.Value = Date.MinValue OrElse
           actuacion.FechaVencimiento.Value.Date.Year < 1000 OrElse
           actuacion.FechaVencimiento.Value.Date < actuacion.FechaNotificacion.Date Then
            Throw New ArgumentException("La fecha de vencimiento calculada no es válida.", NameOf(actuacion))
        End If
        If If(actuacion.Observaciones, "").Length > 1000 Then
            Throw New ArgumentException("Las observaciones no pueden superar 1000 caracteres.", NameOf(actuacion))
        End If
    End Sub

    Private Sub AgregarParametrosActuacion(comando As MySqlCommand, actuacion As ActuacionProcesal)
        Parametro(comando, "@caso", actuacion.IDCaso)
        Parametro(comando, "@tipo", actuacion.IDTipoActuacion)
        Parametro(comando, "@notificacion", actuacion.FechaNotificacion)
        Parametro(comando, "@plazo", actuacion.PlazoLegalDias)
        Parametro(comando, "@vencimiento", If(actuacion.FechaVencimiento.HasValue, CObj(actuacion.FechaVencimiento.Value), DBNull.Value))
        Parametro(comando, "@usuario", Sesion.NombreUsuario)
        Parametro(comando, "@observaciones", actuacion.Observaciones)
    End Sub

End Class
