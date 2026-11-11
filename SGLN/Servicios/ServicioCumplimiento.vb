Imports System
Imports System.Data

''' <summary>
''' Reglas de negocio de cumplimiento LDA/FT y derechos ARCO.
''' Los plazos son configurables y se calculan en días hábiles; deben
''' validarse con el oficial de cumplimiento antes de usarse formalmente.
''' </summary>
Public Class ServicioCumplimiento

    Private ReadOnly _repositorio As New CumplimientoRepositorio()
    Private ReadOnly _catalogo As New CatalogoRepositorio()

    ' ------------------------------------------------------------------
    ' Operaciones LDA/FT
    ' ------------------------------------------------------------------

    Public Function Operaciones(filtro As String) As DataTable
        Return _repositorio.Operaciones(filtro)
    End Function

    Public Function ObtenerOperacion(id As Integer) As DataTable
        Return _repositorio.ObtenerOperacion(id)
    End Function

    Public Function GuardarOperacion(operacion As OperacionLdaft) As Respuesta
        If Not TipoOperacionValido(operacion.TipoOperacion) Then
            Return Respuesta.Fallo("El tipo de operación no es válido.")
        End If
        If operacion.Monto <= 0 Then Return Respuesta.Fallo("El monto debe ser mayor que cero.")
        If String.IsNullOrWhiteSpace(operacion.Moneda) Then Return Respuesta.Fallo("Indique la moneda.")
        If operacion.FechaOperacion.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de la operación no puede ser futura.")
        End If

        Dim calculadora As CalculadoraDiasHabiles = NuevaCalculadora()

        operacion.FechaVencimientoAnalisis =
            calculadora.CalcularVencimiento(operacion.FechaOperacion, DiasConfigurados("LDA_Analisis_Dias", 5))
        operacion.FechaVencimientoReporte =
            calculadora.CalcularVencimiento(operacion.FechaOperacion, DiasConfigurados("LDA_Reporte_Dias", 15))

        Try
            If operacion.ID = 0 Then
                operacion.ID = _repositorio.InsertarOperacion(operacion)
            Else
                _repositorio.ActualizarOperacion(operacion)
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Cumplimiento - guardar operación", ex)
            Return Respuesta.Fallo("No se pudo guardar la operación: " & ex.Message)
        End Try

        Dim mensaje As String = "Operación guardada. Análisis: " &
            operacion.FechaVencimientoAnalisis.Value.ToString("dd/MM/yyyy") &
            " · Reporte: " & operacion.FechaVencimientoReporte.Value.ToString("dd/MM/yyyy") & "."

        If operacion.Monto >= UmbralEfectivo() AndAlso operacion.TipoOperacion = "Regulada" Then
            mensaje &= " Supera el umbral de efectivo configurado."
        End If

        Return New Respuesta With {.Exito = True, .Mensaje = mensaje, .ID = operacion.ID}
    End Function

    Public Function MarcarReportada(id As Integer, fechaReporte As Date) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione una operación.")

        Dim tabla As DataTable = _repositorio.ObtenerOperacion(id)
        If tabla.Rows.Count = 0 Then Return Respuesta.Fallo("La operación no existe.")

        Dim estado As String = Convert.ToString(tabla.Rows(0)("Estado"))
        If estado = "Descartada" Then
            Return Respuesta.Fallo("No se puede reportar una operación descartada.")
        End If
        If fechaReporte.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de reporte no puede ser futura.")
        End If

        _repositorio.MarcarReportada(id, fechaReporte.Date)
        Return Respuesta.Ok("Reporte a la UIF registrado el " & fechaReporte.ToString("dd/MM/yyyy") & ".")
    End Function

    Public Function Descartar(id As Integer) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione una operación.")
        _repositorio.Descartar(id)
        Return Respuesta.Ok("Operación descartada. El registro se conserva para auditoría.")
    End Function

    Public Function UmbralEfectivo() As Decimal
        Dim valor As String = _catalogo.ObtenerConfig("Umbral_Efectivo")
        Dim monto As Decimal
        If Decimal.TryParse(valor, monto) AndAlso monto > 0 Then Return monto
        Return 10000D
    End Function

    ' ------------------------------------------------------------------
    ' Solicitudes ARCO
    ' ------------------------------------------------------------------

    Public Function SolicitudesArco(filtro As String) As DataTable
        Return _repositorio.SolicitudesArco(filtro)
    End Function

    Public Function ObtenerSolicitud(id As Integer) As DataTable
        Return _repositorio.ObtenerSolicitud(id)
    End Function

    Public Function GuardarSolicitud(solicitud As SolicitudArco) As Respuesta
        If solicitud.IDCliente <= 0 Then Return Respuesta.Fallo("Seleccione el cliente.")
        If Not TipoDerechoValido(solicitud.TipoDerecho) Then
            Return Respuesta.Fallo("El tipo de derecho ARCO no es válido.")
        End If
        If solicitud.FechaSolicitud.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de solicitud no puede ser futura.")
        End If

        Dim calculadora As CalculadoraDiasHabiles = NuevaCalculadora()
        Dim diasRespuesta As Integer = DiasConfigurados("ARCO_Dias_Respuesta", 10)

        solicitud.FechaVencimiento = calculadora.CalcularVencimiento(solicitud.FechaSolicitud, diasRespuesta)

        If solicitud.ProrrogaAplicada Then
            solicitud.FechaVencimientoProrroga = calculadora.CalcularVencimiento(
                solicitud.FechaVencimiento, DiasConfigurados("ARCO_Dias_Prorroga", 5))
        Else
            solicitud.FechaVencimientoProrroga = Nothing
        End If

        Try
            If solicitud.ID = 0 Then
                solicitud.ID = _repositorio.InsertarSolicitud(solicitud)
            Else
                _repositorio.ActualizarSolicitud(solicitud)
            End If
        Catch ex As Exception
            RegistroErrores.Escribir("Cumplimiento - guardar solicitud ARCO", ex)
            Return Respuesta.Fallo("No se pudo guardar la solicitud: " & ex.Message)
        End Try

        Dim mensaje As String = "Solicitud guardada. Vence el " & solicitud.FechaVencimiento.ToString("dd/MM/yyyy")
        If solicitud.FechaVencimientoProrroga.HasValue Then
            mensaje &= " (prórroga hasta " & solicitud.FechaVencimientoProrroga.Value.ToString("dd/MM/yyyy") & ")"
        End If

        Return New Respuesta With {.Exito = True, .Mensaje = mensaje & ".", .ID = solicitud.ID}
    End Function

    Public Function ResponderSolicitud(id As Integer, fechaRespuesta As Date) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione una solicitud.")

        Dim tabla As DataTable = _repositorio.ObtenerSolicitud(id)
        If tabla.Rows.Count = 0 Then Return Respuesta.Fallo("La solicitud no existe.")

        If fechaRespuesta.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de respuesta no puede ser futura.")
        End If

        Dim solicitud As Date = Convert.ToDateTime(tabla.Rows(0)("Fecha_Solicitud")).Date
        If fechaRespuesta.Date < solicitud Then
            Return Respuesta.Fallo("La fecha de respuesta no puede ser anterior a la solicitud.")
        End If

        _repositorio.Responder(id, fechaRespuesta.Date)
        Return Respuesta.Ok("Solicitud ARCO marcada como respondida.")
    End Function

    Public Function ClientesCombo() As DataTable
        Return _repositorio.ClientesCombo()
    End Function

    Public Function CalcularVencimientoArco(fechaSolicitud As Date) As Date
        Return NuevaCalculadora().CalcularVencimiento(
            fechaSolicitud, DiasConfigurados("ARCO_Dias_Respuesta", 10))
    End Function

    Public Function CalcularProrrogaArco(fechaVencimiento As Date) As Date
        Return NuevaCalculadora().CalcularVencimiento(
            fechaVencimiento, DiasConfigurados("ARCO_Dias_Prorroga", 5))
    End Function

    Public Function CasosCombo() As DataTable
        Return _repositorio.CasosCombo()
    End Function

    Public Function InstrumentosCombo() As DataTable
        Return _repositorio.InstrumentosCombo()
    End Function

    ' ------------------------------------------------------------------
    ' Utilidades internas
    ' ------------------------------------------------------------------

    Private Function NuevaCalculadora() As CalculadoraDiasHabiles
        Return New CalculadoraDiasHabiles(_catalogo.FechasFeriadas())
    End Function

    Private Function DiasConfigurados(clave As String, valorPorDefecto As Integer) As Integer
        Dim valor As String = _catalogo.ObtenerConfig(clave)
        Dim dias As Integer
        If Integer.TryParse(valor, dias) AndAlso dias > 0 Then Return dias
        Return valorPorDefecto
    End Function

    Private Function TipoOperacionValido(tipo As String) As Boolean
        Select Case tipo
            Case "Regulada", "Inusual", "Sospechosa"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Function TipoDerechoValido(tipo As String) As Boolean
        Select Case tipo
            Case "Acceso", "Rectificación", "Cancelación", "Oposición"
                Return True
            Case Else
                Return False
        End Select
    End Function

End Class
