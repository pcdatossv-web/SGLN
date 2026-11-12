Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de casos judiciales y actuaciones procesales.
''' </summary>
Public Class CasoRepositorio
    Inherits RepositorioBase

    Public Function Buscar(filtro As String) As DataTable
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

    Public Function Obtener(id As Integer) As DataTable
        Return Consultar(
            "SELECT c.*, CONCAT(cl.Nombres, ' ', cl.Apellidos) AS Cliente " &
            "FROM Casos c INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "WHERE c.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function Insertar(caso As Caso) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
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

    Public Sub Actualizar(caso As Caso)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
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
    End Sub

    Public Function ActuacionesPorCaso(idCaso As Integer) As DataTable
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
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                If actuacion.ID = 0 Then
                    Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                        "INSERT INTO Actuaciones_Procesales (ID_Caso, ID_Tipo_Actuacion, Fecha_Notificacion, " &
                        "Plazo_Legal_Dias, Fecha_Vencimiento_Calculada, Estado, Fecha_Confirmacion, Usuario_Confirma, Observaciones) " &
                        "VALUES (@caso, @tipo, @notificacion, @plazo, @vencimiento, @estado, NOW(), @usuario, @observaciones)",
                        Sub(comando As MySqlCommand)
                            AgregarParametrosActuacion(comando, actuacion)
                        End Sub)
                    AuditarEnTransaccion(conexion, transaccion, "CONFIRMAR PLAZO", "Actuaciones_Procesales", nuevoId,
                        "Vencimiento " & If(actuacion.FechaVencimiento.HasValue, actuacion.FechaVencimiento.Value.ToString("dd/MM/yyyy"), "N/D"))
                    Return nuevoId
                End If

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Actuaciones_Procesales SET ID_Tipo_Actuacion = @tipo, Fecha_Notificacion = @notificacion, " &
                    "Plazo_Legal_Dias = @plazo, Fecha_Vencimiento_Calculada = @vencimiento, Estado = @estado, " &
                    "Fecha_Confirmacion = NOW(), Usuario_Confirma = @usuario, Observaciones = @observaciones WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosActuacion(comando, actuacion)
                        Parametro(comando, "@id", actuacion.ID)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "CONFIRMAR PLAZO", "Actuaciones_Procesales",
                    actuacion.ID, "Vencimiento " & If(actuacion.FechaVencimiento.HasValue, actuacion.FechaVencimiento.Value.ToString("dd/MM/yyyy"), "N/D"))
                Return actuacion.ID
            End Function)
    End Function

    Public Function MarcarVencidas() As Integer
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

    Public Sub MarcarCumplida(id As Integer)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Actuaciones_Procesales SET Estado = 'Cumplido' WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "CUMPLIR PLAZO", "Actuaciones_Procesales",
                    id, "Actuación marcada como cumplida")
                Return filas
            End Function)
    End Sub

    Public Function ClientesCombo() As DataTable
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos) AS Nombre FROM Clientes " &
            "WHERE Activo = 1 ORDER BY Apellidos, Nombres")
    End Function

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

    Private Sub AgregarParametrosActuacion(comando As MySqlCommand, actuacion As ActuacionProcesal)
        Parametro(comando, "@caso", actuacion.IDCaso)
        Parametro(comando, "@tipo", actuacion.IDTipoActuacion)
        Parametro(comando, "@notificacion", actuacion.FechaNotificacion)
        Parametro(comando, "@plazo", actuacion.PlazoLegalDias)
        Parametro(comando, "@vencimiento", If(actuacion.FechaVencimiento.HasValue, CObj(actuacion.FechaVencimiento.Value), DBNull.Value))
        Parametro(comando, "@estado", actuacion.Estado)
        Parametro(comando, "@usuario", Sesion.NombreUsuario)
        Parametro(comando, "@observaciones", actuacion.Observaciones)
    End Sub

End Class
