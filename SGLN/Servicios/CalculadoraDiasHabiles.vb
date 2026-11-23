Imports System
Imports System.Collections.Generic

''' <summary>
''' Calculadora aritmética genérica de días hábiles: excluye sábados,
''' domingos y feriados configurados, y comienza el conteo el día siguiente
''' a la fecha inicial. No determina por sí sola la regla legal aplicable a
''' cada materia, jurisdicción o tipo de actuación; el resultado es referencial
''' y debe validarse conforme a la normativa y calendario correspondientes.
''' </summary>
Public Class CalculadoraDiasHabiles

    Private ReadOnly _feriados As HashSet(Of Date)
    Public Const MaxDiasHabilesPermitidos As Integer = 36600
    Private Const MaxRangoConteoDias As Integer = 365000

    Public Sub New(feriados As IEnumerable(Of Date))
        _feriados = New HashSet(Of Date)()
        If feriados IsNot Nothing Then
            For Each fecha As Date In feriados
                _feriados.Add(fecha.Date)
            Next
        End If
    End Sub

    Public Function EsDiaHabil(fecha As Date) As Boolean
        Dim dia As Date = fecha.Date
        If dia.DayOfWeek = DayOfWeek.Saturday OrElse dia.DayOfWeek = DayOfWeek.Sunday Then
            Return False
        End If
        Return Not _feriados.Contains(dia)
    End Function

    ''' <summary>
    ''' Devuelve la fecha de vencimiento contando días hábiles desde el día
    ''' siguiente a la notificación.
    ''' </summary>
    Public Function CalcularVencimiento(fechaNotificacion As Date, diasHabiles As Integer) As Date
        If fechaNotificacion = Date.MinValue OrElse fechaNotificacion.Date.Year < 1000 Then
            Throw New ArgumentOutOfRangeException(NameOf(fechaNotificacion),
                "La fecha de notificación debe estar dentro del rango admitido por MySQL (1000-01-01 o posterior).")
        End If
        If diasHabiles <= 0 OrElse diasHabiles > MaxDiasHabilesPermitidos Then
            Throw New ArgumentOutOfRangeException(NameOf(diasHabiles),
                "El plazo debe estar entre 1 y " & MaxDiasHabilesPermitidos.ToString() & " días hábiles.")
        End If

        Dim fecha As Date = fechaNotificacion.Date
        Dim contador As Integer = 0

        Do While contador < diasHabiles
            If fecha = Date.MaxValue.Date Then
                Throw New ArgumentOutOfRangeException(NameOf(fechaNotificacion),
                    "El vencimiento calculado supera el rango máximo de fechas admitido.")
            End If
            fecha = fecha.AddDays(1)
            If EsDiaHabil(fecha) Then contador += 1
        Loop

        Return fecha
    End Function

    ''' <summary>
    ''' Cuenta los días hábiles transcurridos entre dos fechas (sin incluir el
    ''' día inicial, igual que el cómputo de plazos).
    ''' </summary>
    Public Function ContarDiasHabiles(desde As Date, hasta As Date) As Integer
        If desde = Date.MinValue OrElse hasta = Date.MinValue OrElse
           desde.Date.Year < 1000 OrElse hasta.Date.Year < 1000 Then
            Throw New ArgumentOutOfRangeException(NameOf(desde),
                "Las fechas del período deben estar dentro del rango admitido por MySQL (1000-01-01 o posterior).")
        End If
        If hasta.Date < desde.Date Then Return 0
        If (hasta.Date - desde.Date).TotalDays > MaxRangoConteoDias Then
            Throw New ArgumentOutOfRangeException(NameOf(hasta),
                "El período no puede superar " & MaxRangoConteoDias.ToString() & " días.")
        End If

        Dim contador As Integer = 0
        Dim fecha As Date = desde.Date
        Do While fecha < hasta.Date
            fecha = fecha.AddDays(1)
            If EsDiaHabil(fecha) Then contador += 1
        Loop
        Return contador
    End Function

    Public Function SiguienteDiaHabil(fecha As Date) As Date
        If fecha = Date.MinValue OrElse fecha.Date.Year < 1000 Then
            Throw New ArgumentOutOfRangeException(NameOf(fecha),
                "La fecha debe estar dentro del rango admitido por MySQL (1000-01-01 o posterior).")
        End If
        Dim actual As Date = fecha.Date
        Do While Not EsDiaHabil(actual)
            If actual = Date.MaxValue.Date Then
                Throw New ArgumentOutOfRangeException(NameOf(fecha),
                    "No existe un día hábil posterior dentro del rango de fechas admitido.")
            End If
            actual = actual.AddDays(1)
        Loop
        Return actual
    End Function

End Class
