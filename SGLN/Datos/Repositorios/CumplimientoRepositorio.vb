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

    Public Sub Responder(id As Integer, fechaRespuesta As Date)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Solicitudes_ARCO SET Estado = 'Respondida', Fecha_Respuesta = @fecha WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@fecha", fechaRespuesta)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "RESPONDER ARCO", "Solicitudes_ARCO",
                    id, "Respuesta el " & fechaRespuesta.ToString("dd/MM/yyyy"))
                Return filas
            End Function)
    End Sub

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
