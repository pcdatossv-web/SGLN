Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de cumplimiento LDA/FT (operaciones) y derechos ARCO.
''' </summary>
Public Class CumplimientoRepositorio
    Inherits RepositorioBase

    ' ------------------------------------------------------------------
    ' Operaciones LDA/FT
    ' ------------------------------------------------------------------

    Public Function Operaciones(filtro As String) As DataTable
        Dim texto As String = If(filtro, "").Trim()

        Return Consultar(
            "SELECT o.ID, o.Tipo_Operacion, o.Monto, o.Moneda, o.Fecha_Operacion, o.Estado, " &
            "       o.Reportado_UIF, o.Fecha_Vencimiento_Analisis, o.Fecha_Vencimiento_Reporte, " &
            "       o.Fecha_Reporte, o.Descripcion, o.Observaciones, " &
            "       IFNULL(c.Numero_Expediente, '') AS Expediente, " &
            "       IFNULL(i.Numero_Escritura, 0) AS Escritura, o.ID_Caso, o.ID_Instrumento " &
            "FROM Operaciones_LDAFT o " &
            "LEFT JOIN Casos c ON c.ID = o.ID_Caso " &
            "LEFT JOIN Instrumentos i ON i.ID = o.ID_Instrumento " &
            "WHERE (@filtro IS NULL OR o.Descripcion LIKE @like OR o.Tipo_Operacion LIKE @like " &
            "       OR IFNULL(c.Numero_Expediente, '') LIKE @like) " &
            "ORDER BY o.Fecha_Operacion DESC, o.ID DESC",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
            End Sub)
    End Function

    Public Function ObtenerOperacion(id As Integer) As DataTable
        Return Consultar(
            "SELECT * FROM Operaciones_LDAFT WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function InsertarOperacion(operacion As OperacionLdaft) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Operaciones_LDAFT (ID_Caso, ID_Instrumento, Tipo_Operacion, Monto, Moneda, " &
                    "Fecha_Operacion, Descripcion, Estado, Fecha_Vencimiento_Analisis, Fecha_Vencimiento_Reporte, Observaciones) " &
                    "VALUES (@caso, @instrumento, @tipo, @monto, @moneda, @fecha, @descripcion, @estado, @analisis, @reporte, @observaciones)",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosOperacion(comando, operacion)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Operaciones_LDAFT", nuevoId,
                    operacion.TipoOperacion & " por " & operacion.Monto.ToString("N2") & " " & operacion.Moneda)
                Return nuevoId
            End Function)
    End Function

    Public Function ActualizarOperacion(operacion As OperacionLdaft) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Operaciones_LDAFT SET ID_Caso = @caso, ID_Instrumento = @instrumento, " &
                    "Tipo_Operacion = @tipo, Monto = @monto, Moneda = @moneda, Fecha_Operacion = @fecha, " &
                    "Descripcion = @descripcion, Estado = @estado, Fecha_Vencimiento_Analisis = @analisis, " &
                    "Fecha_Vencimiento_Reporte = @reporte, Observaciones = @observaciones " &
                    "WHERE ID = @id AND Estado IN ('Registrada', 'En análisis') AND Reportado_UIF = 0",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosOperacion(comando, operacion)
                        Parametro(comando, "@id", operacion.ID)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "MODIFICAR",
                    "Operaciones_LDAFT", operacion.ID,
                    operacion.TipoOperacion & " por " & operacion.Monto.ToString("N2") & " " & operacion.Moneda)
                Return filas
            End Function)
    End Function

    Public Function MarcarReportada(id As Integer, fechaReporte As Date) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Operaciones_LDAFT SET Estado = 'Reportada', Reportado_UIF = 1, Fecha_Reporte = @fecha " &
                    "WHERE ID = @id AND Estado IN ('Registrada', 'En análisis') AND Reportado_UIF = 0",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@fecha", fechaReporte)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "REPORTAR UIF",
                    "Operaciones_LDAFT", id, "Reporte registrado el " & fechaReporte.ToString("dd/MM/yyyy"))
                Return filas
            End Function)
    End Function

    Public Function Descartar(id As Integer) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Operaciones_LDAFT SET Estado = 'Descartada' " &
                    "WHERE ID = @id AND Estado IN ('Registrada', 'En análisis') AND Reportado_UIF = 0",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "DESCARTAR",
                    "Operaciones_LDAFT", id, "Operación descartada")
                Return filas
            End Function)
    End Function

    ' ------------------------------------------------------------------
    ' Solicitudes ARCO
    ' ------------------------------------------------------------------

    Public Function SolicitudesArco(filtro As String) As DataTable
        Dim texto As String = If(filtro, "").Trim()

        Return Consultar(
            "SELECT a.ID, a.ID_Cliente, CONCAT(cl.Nombres, ' ', cl.Apellidos) AS Cliente, " &
            "       a.Tipo_Derecho, a.Fecha_Solicitud, a.Fecha_Vencimiento, " &
            "       a.Fecha_Vencimiento_Prorroga, a.Prorroga_Aplicada, a.Estado, " &
            "       a.Fecha_Respuesta, a.Detalle " &
            "FROM Solicitudes_ARCO a " &
            "INNER JOIN Clientes cl ON cl.ID = a.ID_Cliente " &
            "WHERE (@filtro IS NULL OR cl.Nombres LIKE @like OR cl.Apellidos LIKE @like " &
            "       OR a.Tipo_Derecho LIKE @like OR a.Detalle LIKE @like) " &
            "ORDER BY a.Fecha_Solicitud DESC, a.ID DESC",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
            End Sub)
    End Function

    Public Function ObtenerSolicitud(id As Integer) As DataTable
        Return Consultar(
            "SELECT * FROM Solicitudes_ARCO WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function InsertarSolicitud(solicitud As SolicitudArco) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Solicitudes_ARCO (ID_Cliente, Tipo_Derecho, Fecha_Solicitud, Fecha_Vencimiento, " &
                    "Fecha_Vencimiento_Prorroga, Prorroga_Aplicada, Estado, Fecha_Respuesta, Detalle) " &
                    "VALUES (@cliente, @derecho, @solicitud, @vencimiento, @prorroga, @aplicada, @estado, @respuesta, @detalle)",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosSolicitud(comando, solicitud)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Solicitudes_ARCO", nuevoId,
                    solicitud.TipoDerecho & " del cliente " & solicitud.IDCliente)
                Return nuevoId
            End Function)
    End Function

    Public Function ActualizarSolicitud(solicitud As SolicitudArco) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Solicitudes_ARCO SET ID_Cliente = @cliente, Tipo_Derecho = @derecho, " &
                    "Fecha_Solicitud = @solicitud, Fecha_Vencimiento = @vencimiento, " &
                    "Fecha_Vencimiento_Prorroga = @prorroga, Prorroga_Aplicada = @aplicada, " &
                    "Estado = @estado, Fecha_Respuesta = @respuesta, Detalle = @detalle " &
                    "WHERE ID = @id AND Estado IN ('Pendiente', 'Vencida') AND Fecha_Respuesta IS NULL",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosSolicitud(comando, solicitud)
                        Parametro(comando, "@id", solicitud.ID)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "MODIFICAR",
                    "Solicitudes_ARCO", solicitud.ID, solicitud.TipoDerecho)
                Return filas
            End Function)
    End Function

    Public Function Responder(id As Integer, fechaRespuesta As Date) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Solicitudes_ARCO SET Estado = 'Respondida', Fecha_Respuesta = @fecha " &
                    "WHERE ID = @id AND Estado IN ('Pendiente', 'Vencida') AND Fecha_Respuesta IS NULL",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@fecha", fechaRespuesta)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "RESPONDER ARCO",
                    "Solicitudes_ARCO", id, "Respuesta el " & fechaRespuesta.ToString("dd/MM/yyyy"))
                Return filas
            End Function)
    End Function

    ' ------------------------------------------------------------------
    ' Catálogos
    ' ------------------------------------------------------------------

    Public Function ClientesCombo() As DataTable
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos) AS Nombre FROM Clientes " &
            "WHERE Activo = 1 ORDER BY Apellidos, Nombres")
    End Function

    Public Function CasosCombo() As DataTable
        Return Consultar(
            "SELECT c.ID, CONCAT(c.Numero_Expediente, ' - ', cl.Nombres, ' ', cl.Apellidos) AS Nombre " &
            "FROM Casos c INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente ORDER BY c.Numero_Expediente")
    End Function

    Public Function InstrumentosCombo() As DataTable
        Return Consultar(
            "SELECT i.ID, CONCAT('Libro ', l.Numero_Libro, ' - Escritura ', i.Numero_Escritura) AS Nombre " &
            "FROM Instrumentos i INNER JOIN Libros_Protocolo l ON l.ID = i.ID_Libro " &
            "WHERE i.Estado = 'Vigente' ORDER BY l.Anio_Vigencia DESC, l.Numero_Libro, i.Numero_Escritura")
    End Function

    Private Sub AgregarParametrosOperacion(comando As MySqlCommand, operacion As OperacionLdaft)
        Parametro(comando, "@caso", If(operacion.IDCaso.HasValue, CObj(operacion.IDCaso.Value), DBNull.Value))
        Parametro(comando, "@instrumento", If(operacion.IDInstrumento.HasValue, CObj(operacion.IDInstrumento.Value), DBNull.Value))
        Parametro(comando, "@tipo", operacion.TipoOperacion)
        Parametro(comando, "@monto", operacion.Monto)
        Parametro(comando, "@moneda", operacion.Moneda)
        Parametro(comando, "@fecha", operacion.FechaOperacion)
        Parametro(comando, "@descripcion", operacion.Descripcion)
        Parametro(comando, "@estado", operacion.Estado)
        Parametro(comando, "@analisis", If(operacion.FechaVencimientoAnalisis.HasValue, CObj(operacion.FechaVencimientoAnalisis.Value), DBNull.Value))
        Parametro(comando, "@reporte", If(operacion.FechaVencimientoReporte.HasValue, CObj(operacion.FechaVencimientoReporte.Value), DBNull.Value))
        Parametro(comando, "@observaciones", operacion.Observaciones)
    End Sub

    Private Sub AgregarParametrosSolicitud(comando As MySqlCommand, solicitud As SolicitudArco)
        Parametro(comando, "@cliente", solicitud.IDCliente)
        Parametro(comando, "@derecho", solicitud.TipoDerecho)
        Parametro(comando, "@solicitud", solicitud.FechaSolicitud)
        Parametro(comando, "@vencimiento", solicitud.FechaVencimiento)
        Parametro(comando, "@prorroga", If(solicitud.FechaVencimientoProrroga.HasValue, CObj(solicitud.FechaVencimientoProrroga.Value), DBNull.Value))
        Parametro(comando, "@aplicada", solicitud.ProrrogaAplicada)
        Parametro(comando, "@estado", solicitud.Estado)
        Parametro(comando, "@respuesta", If(solicitud.FechaRespuesta.HasValue, CObj(solicitud.FechaRespuesta.Value), DBNull.Value))
        Parametro(comando, "@detalle", solicitud.Detalle)
    End Sub

End Class
