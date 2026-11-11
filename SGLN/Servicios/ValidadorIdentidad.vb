Imports System
Imports System.Linq

''' <summary>
''' Validación de documentos de identidad salvadoreños.
''' El formato es bloqueante; el dígito verificador del DUI se informa como
''' advertencia para que el abogado pueda confirmarlo con el documento físico.
''' </summary>
Public Module ValidadorIdentidad

    Public Function LimpiarDocumento(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then Return ""
        Return New String(valor.Where(Function(caracter As Char) Char.IsDigit(caracter)).ToArray())
    End Function

    Public Function EsFormatoDuiValido(dui As String) As Boolean
        Return LimpiarDocumento(dui).Length = 9
    End Function

    Public Function EsDigitoDuiValido(dui As String) As Boolean
        Dim digitos As String = LimpiarDocumento(dui)
        If digitos.Length <> 9 Then Return False

        Dim pesos As Integer() = {3, 7, 13, 17, 19, 23, 29, 37}
        Dim suma As Integer = 0
        For i As Integer = 0 To 7
            suma += Integer.Parse(digitos.Substring(i, 1)) * pesos(i)
        Next

        Dim verificador As Integer = (10 - (suma Mod 10)) Mod 10
        Return verificador = Integer.Parse(digitos.Substring(8, 1))
    End Function

    Public Function FormatearDui(dui As String) As String
        Dim digitos As String = LimpiarDocumento(dui)
        If digitos.Length = 9 Then
            Return digitos.Substring(0, 8) & "-" & digitos.Substring(8, 1)
        End If
        Return dui
    End Function

    Public Function EsFormatoNitValido(nit As String) As Boolean
        Return LimpiarDocumento(nit).Length = 14
    End Function

    Public Function FormatearNit(nit As String) As String
        Dim digitos As String = LimpiarDocumento(nit)
        If digitos.Length = 14 Then
            Return digitos.Substring(0, 4) & "-" & digitos.Substring(4, 6) & "-" &
                   digitos.Substring(10, 3) & "-" & digitos.Substring(13, 1)
        End If
        Return nit
    End Function

End Module
