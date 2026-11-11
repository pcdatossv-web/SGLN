Imports System
Imports System.Collections.Generic

''' <summary>
''' Calculadora de días hábiles conforme al Art. 145 del CPCM:
''' se excluyen sábados, domingos y feriados, y el plazo empieza a correr
''' el día siguiente a la notificación.
''' </summary>
Public Class CalculadoraDiasHabiles

    Private ReadOnly _feriados As HashSet(Of Date)

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
        If diasHabiles <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(diasHabiles), "El plazo debe ser mayor que cero.")
        End If

        Dim fecha As Date = fechaNotificacion.Date
        Dim contador As Integer = 0

        Do While contador < diasHabiles
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
        If hasta.Date < desde.Date Then Return 0

        Dim contador As Integer = 0
        Dim fecha As Date = desde.Date
        Do While fecha < hasta.Date
            fecha = fecha.AddDays(1)
            If EsDiaHabil(fecha) Then contador += 1
        Loop
        Return contador
    End Function

    Public Function SiguienteDiaHabil(fecha As Date) As Date
        Dim actual As Date = fecha.Date
        Do While Not EsDiaHabil(actual)
            actual = actual.AddDays(1)
        Loop
        Return actual
    End Function

End Class
