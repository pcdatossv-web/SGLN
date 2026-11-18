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
        If Not Permisos.TieneAcceso("casos.confirmar_plazo") Then
            Throw New UnauthorizedAccessException("No tiene permiso para consultar el calendario de plazos procesales.")
        End If
        Return New CalculadoraDiasHabiles(_catalogo.FechasFeriadas())
    End Function

    Public Function CalcularVencimiento(fechaNotificacion As Date, diasHabiles As Integer) As Date
        If Not Permisos.TieneAcceso("casos.confirmar_plazo") Then
            Throw New UnauthorizedAccessException("No tiene permiso para calcular plazos procesales.")
        End If
        If fechaNotificacion = Date.MinValue OrElse fechaNotificacion.Date.Year < 1000 Then
            Throw New ArgumentOutOfRangeException(NameOf(fechaNotificacion),
                "La fecha de notificación debe estar dentro del rango admitido por MySQL (1000-01-01 o posterior).")
        End If
        If diasHabiles <= 0 OrElse diasHabiles > CalculadoraDiasHabiles.MaxDiasHabilesPermitidos Then
            Throw New ArgumentOutOfRangeException(NameOf(diasHabiles),
                "El plazo debe estar entre 1 y " & CalculadoraDiasHabiles.MaxDiasHabilesPermitidos.ToString() & " días hábiles.")
        End If
        Return ObtenerCalculadora().CalcularVencimiento(fechaNotificacion, diasHabiles)
    End Function

    Public Function Confirmar(actuacion As ActuacionProcesal) As Respuesta
        If Not Permisos.TieneAcceso("casos.confirmar_plazo") Then Return Respuesta.Fallo("No tiene permiso para confirmar plazos.")

        If actuacion Is Nothing Then Return Respuesta.Fallo("Los datos de la actuación no son válidos.")
        If actuacion.ID < 0 Then Return Respuesta.Fallo("El identificador de la actuación no es válido.")
        If actuacion.IDCaso <= 0 Then Return Respuesta.Fallo("Seleccione un caso.")
        If actuacion.IDTipoActuacion <= 0 Then Return Respuesta.Fallo("Seleccione el tipo de actuación.")
        If actuacion.PlazoLegalDias <= 0 OrElse actuacion.PlazoLegalDias > CalculadoraDiasHabiles.MaxDiasHabilesPermitidos Then
            Return Respuesta.Fallo("El plazo legal debe estar entre 1 y " & CalculadoraDiasHabiles.MaxDiasHabilesPermitidos.ToString() & " días hábiles.")
        End If
        If actuacion.FechaNotificacion = Date.MinValue OrElse actuacion.FechaNotificacion.Date.Year < 1000 Then
            Return Respuesta.Fallo("La fecha de notificación debe ser igual o posterior al 01/01/1000.")
        End If
        If actuacion.FechaNotificacion.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de notificación no puede ser futura.")
        End If

        Try
            ' Se recalcula en el servidor de aplicación para que la fecha no
            ' dependa de lo que muestre o modifique la interfaz.
            actuacion.FechaVencimiento = CalcularVencimiento(actuacion.FechaNotificacion, actuacion.PlazoLegalDias)
            actuacion.ID = _repositorio.GuardarActuacion(actuacion)
            If actuacion.ID <= 0 Then Return Respuesta.Fallo("La actuación que intenta modificar ya no existe.")

            Return Respuesta.Ok(
                "Vencimiento confirmado para el " & actuacion.FechaVencimiento.Value.ToString("dd/MM/yyyy") &
                ". El cálculo quedó registrado en la auditoría.")
        Catch ex As Exception
            RegistroErrores.Escribir("Confirmar plazo", ex)
            Return Respuesta.Fallo("No se pudo confirmar el plazo. Verifique los datos y la conexión con la base de datos.")
        End Try
    End Function

    Public Function ActualizarVencidas() As Integer
        If Not Permisos.TieneAcceso("casos.cumplir_plazo") Then Return 0
        Try
            Return _repositorio.MarcarVencidas()
        Catch ex As Exception
            RegistroErrores.Escribir("Actualizar plazos vencidos", ex)
            Return 0
        End Try
    End Function

    Public Function MarcarCumplida(idActuacion As Integer) As Respuesta
        If Not Permisos.TieneAcceso("casos.cumplir_plazo") Then Return Respuesta.Fallo("No tiene permiso para marcar actuaciones como cumplidas.")

        If idActuacion <= 0 Then Return Respuesta.Fallo("Seleccione una actuación.")
        Try
            Dim filas As Integer = _repositorio.MarcarCumplida(idActuacion)
            If filas = 0 Then Return Respuesta.Fallo("La actuación no existe, ya fue atendida o no está en un estado que permita marcarla como cumplida.")
            Return Respuesta.Ok("Actuación marcada como cumplida.")
        Catch ex As Exception
            RegistroErrores.Escribir("Marcar actuación cumplida", ex)
            Return Respuesta.Fallo("No se pudo actualizar la actuación. Verifique la conexión y vuelva a intentarlo.")
        End Try
    End Function

    Public Function Actuaciones(idCaso As Integer) As DataTable
        If Not Permisos.TieneAcceso("casos.ver") OrElse idCaso <= 0 Then Return New DataTable()
        Return _repositorio.ActuacionesPorCaso(idCaso)
    End Function

End Class
