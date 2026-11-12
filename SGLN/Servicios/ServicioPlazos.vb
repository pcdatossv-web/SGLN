Imports System
Imports System.Data

''' <summary>
''' Servicio de plazos procesales. La fecha de vencimiento SIEMPRE la calcula
''' el sistema con días hábiles y feriados; el abogado solo la confirma.
''' </summary>
Public Class ServicioPlazos

    Private ReadOnly _repositorio As New CasoRepositorio()
    Private ReadOnly _catalogo As New CatalogoRepositorio()

    Public Function ObtenerCalculadora() As CalculadoraDiasHabiles
        Return New CalculadoraDiasHabiles(_catalogo.FechasFeriadas())
    End Function

    Public Function CalcularVencimiento(fechaNotificacion As Date, diasHabiles As Integer) As Date
        If diasHabiles <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(diasHabiles), "El plazo debe ser mayor que cero.")
        End If
        Return ObtenerCalculadora().CalcularVencimiento(fechaNotificacion, diasHabiles)
    End Function

    Public Function Confirmar(actuacion As ActuacionProcesal) As Respuesta
        If Not Permisos.TieneAcceso("casos.confirmar_plazo") Then Return Respuesta.Fallo("No tiene permiso para confirmar plazos.")

        If actuacion.IDCaso <= 0 Then Return Respuesta.Fallo("Seleccione un caso.")
        If actuacion.IDTipoActuacion <= 0 Then Return Respuesta.Fallo("Seleccione el tipo de actuación.")
        If actuacion.PlazoLegalDias <= 0 Then Return Respuesta.Fallo("El plazo legal debe ser mayor que cero.")
        If actuacion.FechaNotificacion.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de notificación no puede ser futura.")
        End If

        Try
            ' Se recalcula en el servidor de aplicación para que la fecha no
            ' dependa de lo que muestre o modifique la interfaz.
            actuacion.FechaVencimiento = CalcularVencimiento(actuacion.FechaNotificacion, actuacion.PlazoLegalDias)
            actuacion.ID = _repositorio.GuardarActuacion(actuacion)

            Return Respuesta.Ok(
                "Vencimiento confirmado para el " & actuacion.FechaVencimiento.Value.ToString("dd/MM/yyyy") &
                ". El cálculo quedó registrado en la auditoría.")
        Catch ex As Exception
            RegistroErrores.Escribir("Confirmar plazo", ex)
            Return Respuesta.Fallo("No se pudo confirmar el plazo: " & ex.Message)
        End Try
    End Function

    Public Function ActualizarVencidas() As Integer
        Return _repositorio.MarcarVencidas()
    End Function

    Public Function MarcarCumplida(idActuacion As Integer) As Respuesta
        If Not Permisos.TieneAcceso("casos.cumplir_plazo") Then Return Respuesta.Fallo("No tiene permiso para marcar actuaciones como cumplidas.")

        If idActuacion <= 0 Then Return Respuesta.Fallo("Seleccione una actuación.")
        _repositorio.MarcarCumplida(idActuacion)
        Return Respuesta.Ok("Actuación marcada como cumplida.")
    End Function

    Public Function Actuaciones(idCaso As Integer) As DataTable
        Return _repositorio.ActuacionesPorCaso(idCaso)
    End Function

End Class
