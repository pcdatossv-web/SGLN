Imports System
Imports System.Collections.Generic

''' <summary>
''' Calculadora de días hábiles conforme al Art. 145 del CPCM:
''' se excluyen sábados, domingos y feriados, y el plazo empieza a correr
''' el día siguiente a la notificación.
''' </summary>
Public Class CalculadoraDiasHabiles

    Private ReadOnly _feriados As HashSet(Of Date)
    Private Const MaxDiasHabiles As Integer = 36600
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
        If fechaNotificacion = Date.MinValue Then
            Throw New ArgumentOutOfRangeException(NameOf(fechaNotificacion), "La fecha de notificación no es válida.")
        End If
        If diasHabiles <= 0 OrElse diasHabiles > MaxDiasHabiles Then
            Throw New ArgumentOutOfRangeException(NameOf(diasHabiles),
                "El plazo debe estar entre 1 y " & MaxDiasHabiles.ToString() & " días hábiles.")
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
        If desde = Date.MinValue OrElse hasta = Date.MinValue Then
            Throw New ArgumentOutOfRangeException(NameOf(desde), "Las fechas del período no son válidas.")
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
        If fecha = Date.MinValue Then
            Throw New ArgumentOutOfRangeException(NameOf(fecha), "La fecha no es válida.")
        End If
        Dim actual As Date = fecha.Date
        Do While Not EsDiaHabil(actual)
            actual = actual.AddDays(1)
        Loop
        Return actual
    End Function

End Class
