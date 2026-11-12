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
        If Not Permisos.TieneAcceso("cumplimiento.operaciones") Then Return Respuesta.Fallo("No tiene permiso para gestionar operaciones LDA/FT.")
        If operacion Is Nothing Then Return Respuesta.Fallo("No se recibieron los datos de la operación.")

        If Not TipoOperacionValido(operacion.TipoOperacion) Then
            Return Respuesta.Fallo("El tipo de operación no es válido.")
        End If
        If operacion.Monto <= 0D OrElse operacion.Monto > 9999999999999999.99D Then
            Return Respuesta.Fallo("El monto debe estar dentro del rango permitido.")
        End If
        If String.IsNullOrWhiteSpace(operacion.Moneda) Then Return Respuesta.Fallo("Indique la moneda.")
        operacion.Moneda = operacion.Moneda.Trim().ToUpperInvariant()
        If operacion.Moneda.Length < 3 OrElse operacion.Moneda.Length > 5 Then
            Return Respuesta.Fallo("La moneda debe usar un código de 3 a 5 letras.")
        End If
        For Each caracter As Char In operacion.Moneda
            If Not Char.IsLetter(caracter) Then Return Respuesta.Fallo("El código de moneda solo puede contener letras.")
        Next
        If If(operacion.Descripcion, "").Length > 1000 Then Return Respuesta.Fallo("La descripción no puede superar 1000 caracteres.")
        If If(operacion.Observaciones, "").Length > 500 Then Return Respuesta.Fallo("Las observaciones no pueden superar 500 caracteres.")
        If operacion.FechaOperacion.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de la operación no puede ser futura.")
        End If

        Dim recalcularPlazos As Boolean = True
        If operacion.ID > 0 Then
            Dim actual As DataTable = _repositorio.ObtenerOperacion(operacion.ID)
            If actual.Rows.Count = 0 Then Return Respuesta.Fallo("La operación ya no existe.")
            Dim estadoActual As String = Convert.ToString(actual.Rows(0)("Estado"))
            If estadoActual = "Reportada" OrElse estadoActual = "Descartada" OrElse
               Convert.ToBoolean(actual.Rows(0)("Reportado_UIF")) Then
                Return Respuesta.Fallo("Una operación reportada o descartada no se puede modificar.")
            End If
            If estadoActual <> "Registrada" AndAlso estadoActual <> "En análisis" Then
                Return Respuesta.Fallo("El estado actual de la operación no permite modificarla.")
            End If
            operacion.Estado = estadoActual

            Dim fechaOriginal As Date = Convert.ToDateTime(actual.Rows(0)("Fecha_Operacion")).Date
            If operacion.FechaOperacion.Date = fechaOriginal AndAlso
               Not actual.Rows(0).IsNull("Fecha_Vencimiento_Analisis") AndAlso
               Not actual.Rows(0).IsNull("Fecha_Vencimiento_Reporte") Then
                operacion.FechaVencimientoAnalisis =
                    Convert.ToDateTime(actual.Rows(0)("Fecha_Vencimiento_Analisis")).Date
                operacion.FechaVencimientoReporte =
                    Convert.ToDateTime(actual.Rows(0)("Fecha_Vencimiento_Reporte")).Date
                recalcularPlazos = False
            End If
        Else
            operacion.Estado = "Registrada"
            operacion.FechaReporte = Nothing
            operacion.ReportadoUIF = False
        End If

        If recalcularPlazos Then
            Dim calculadora As CalculadoraDiasHabiles = NuevaCalculadora()
            operacion.FechaVencimientoAnalisis =
                calculadora.CalcularVencimiento(operacion.FechaOperacion, DiasConfigurados("LDA_Analisis_Dias", 5))
            operacion.FechaVencimientoReporte =
                calculadora.CalcularVencimiento(operacion.FechaOperacion, DiasConfigurados("LDA_Reporte_Dias", 15))
        End If

        Try
            If operacion.ID = 0 Then
                operacion.ID = _repositorio.InsertarOperacion(operacion)
            Else
                If _repositorio.ActualizarOperacion(operacion) = 0 Then
                    Return Respuesta.Fallo("La operación cambió de estado y ya no puede modificarse.")
                End If
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
        If Not Permisos.TieneAcceso("cumplimiento.reportar_uif") Then Return Respuesta.Fallo("No tiene permiso para reportar operaciones a la UIF.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione una operación.")

        Dim tabla As DataTable = _repositorio.ObtenerOperacion(id)
        If tabla.Rows.Count = 0 Then Return Respuesta.Fallo("La operación no existe.")

        Dim estado As String = Convert.ToString(tabla.Rows(0)("Estado"))
        If estado = "Descartada" Then
            Return Respuesta.Fallo("No se puede reportar una operación descartada.")
        End If
        If estado = "Reportada" OrElse Convert.ToBoolean(tabla.Rows(0)("Reportado_UIF")) Then
            Return Respuesta.Fallo("La operación ya fue reportada a la UIF.")
        End If
        If fechaReporte.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de reporte no puede ser futura.")
        End If
        Dim fechaOperacion As Date = Convert.ToDateTime(tabla.Rows(0)("Fecha_Operacion")).Date
        If fechaReporte.Date < fechaOperacion Then
            Return Respuesta.Fallo("La fecha de reporte no puede ser anterior a la operación.")
        End If

        If _repositorio.MarcarReportada(id, fechaReporte.Date) = 0 Then
            Return Respuesta.Fallo("La operación cambió de estado. Actualice la lista.")
        End If
        Return Respuesta.Ok("Reporte a la UIF registrado el " & fechaReporte.ToString("dd/MM/yyyy") & ".")
    End Function

    Public Function Descartar(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("cumplimiento.operaciones") Then Return Respuesta.Fallo("No tiene permiso para gestionar operaciones LDA/FT.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione una operación.")
        Dim actual As DataTable = _repositorio.ObtenerOperacion(id)
        If actual.Rows.Count = 0 Then Return Respuesta.Fallo("La operación no existe.")
        Dim estado As String = Convert.ToString(actual.Rows(0)("Estado"))
        If estado = "Reportada" OrElse Convert.ToBoolean(actual.Rows(0)("Reportado_UIF")) Then
            Return Respuesta.Fallo("Una operación reportada a la UIF no se puede descartar.")
        End If
        If estado = "Descartada" Then Return Respuesta.Fallo("La operación ya fue descartada.")
        If _repositorio.Descartar(id) = 0 Then Return Respuesta.Fallo("La operación cambió de estado. Actualice la lista.")
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
        If Not Permisos.TieneAcceso("cumplimiento.arco") Then Return Respuesta.Fallo("No tiene permiso para gestionar solicitudes ARCO.")
        If solicitud Is Nothing Then Return Respuesta.Fallo("No se recibieron los datos de la solicitud.")
        If If(solicitud.Detalle, "").Length > 1000 Then Return Respuesta.Fallo("El detalle no puede superar 1000 caracteres.")

        If solicitud.IDCliente <= 0 Then Return Respuesta.Fallo("Seleccione el cliente.")
        If Not TipoDerechoValido(solicitud.TipoDerecho) Then
            Return Respuesta.Fallo("El tipo de derecho ARCO no es válido.")
        End If
        Dim solicitudNueva As Boolean = solicitud.ID = 0
        Dim prorrogaExistente As Boolean = False
        If Not solicitudNueva Then
            Dim actual As DataTable = _repositorio.ObtenerSolicitud(solicitud.ID)
            If actual.Rows.Count = 0 Then Return Respuesta.Fallo("La solicitud ya no existe.")
            If Not actual.Rows(0).IsNull("Fecha_Respuesta") OrElse
               Convert.ToString(actual.Rows(0)("Estado")) = "Respondida" Then
                Return Respuesta.Fallo("Una solicitud ARCO respondida no se puede modificar.")
            End If

            solicitud.FechaSolicitud = Convert.ToDateTime(actual.Rows(0)("Fecha_Solicitud")).Date
            solicitud.FechaVencimiento = Convert.ToDateTime(actual.Rows(0)("Fecha_Vencimiento")).Date
            solicitud.Estado = Convert.ToString(actual.Rows(0)("Estado"))
            solicitud.FechaRespuesta = Nothing
            prorrogaExistente = Convert.ToBoolean(actual.Rows(0)("Prorroga_Aplicada"))
            If prorrogaExistente Then
                solicitud.ProrrogaAplicada = True
                If actual.Rows(0).IsNull("Fecha_Vencimiento_Prorroga") Then
                    Return Respuesta.Fallo("La solicitud tiene una prórroga marcada sin fecha de vencimiento. Revise el registro antes de modificarlo.")
                End If
                solicitud.FechaVencimientoProrroga =
                    Convert.ToDateTime(actual.Rows(0)("Fecha_Vencimiento_Prorroga")).Date
            End If
        Else
            solicitud.Estado = "Pendiente"
            solicitud.FechaRespuesta = Nothing
            solicitud.FechaVencimiento = NuevaCalculadora().CalcularVencimiento(
                solicitud.FechaSolicitud, DiasConfigurados("ARCO_Dias_Respuesta", 10))
        End If

        If solicitud.FechaSolicitud.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de solicitud no puede ser futura.")
        End If

        If solicitud.ProrrogaAplicada Then
            If Not prorrogaExistente Then
                solicitud.FechaVencimientoProrroga = NuevaCalculadora().CalcularVencimiento(
                    solicitud.FechaVencimiento, DiasConfigurados("ARCO_Dias_Prorroga", 5))
            End If
        Else
            solicitud.FechaVencimientoProrroga = Nothing
        End If

        Try
            If solicitud.ID = 0 Then
                solicitud.ID = _repositorio.InsertarSolicitud(solicitud)
            Else
                If _repositorio.ActualizarSolicitud(solicitud) = 0 Then
                    Return Respuesta.Fallo("La solicitud cambió de estado y ya no puede modificarse.")
                End If
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
        If Not Permisos.TieneAcceso("cumplimiento.arco") Then Return Respuesta.Fallo("No tiene permiso para responder solicitudes ARCO.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione una solicitud.")

        Dim tabla As DataTable = _repositorio.ObtenerSolicitud(id)
        If tabla.Rows.Count = 0 Then Return Respuesta.Fallo("La solicitud no existe.")

        If Not tabla.Rows(0).IsNull("Fecha_Respuesta") OrElse
           Convert.ToString(tabla.Rows(0)("Estado")) = "Respondida" Then
            Return Respuesta.Fallo("La solicitud ARCO ya fue respondida.")
        End If
        If fechaRespuesta.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de respuesta no puede ser futura.")
        End If

        Dim fechaSolicitud As Date = Convert.ToDateTime(tabla.Rows(0)("Fecha_Solicitud")).Date
        If fechaRespuesta.Date < fechaSolicitud Then
            Return Respuesta.Fallo("La fecha de respuesta no puede ser anterior a la solicitud.")
        End If

        If _repositorio.Responder(id, fechaRespuesta.Date) = 0 Then
            Return Respuesta.Fallo("La solicitud cambió de estado. Actualice la lista.")
        End If
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
