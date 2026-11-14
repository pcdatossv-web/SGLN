Imports System
Imports System.Linq
Imports System.Text

''' <summary>
''' Validación de documentos de identidad salvadoreños.
''' El formato es bloqueante; el dígito verificador del DUI se informa como
''' advertencia para que el abogado pueda confirmarlo con el documento físico.
''' </summary>
Public Module ValidadorIdentidad

    ''' <summary>
    ''' Quita únicamente separadores permitidos. Letras y dígitos Unicode no
    ''' ASCII se rechazan para evitar aceptar entradas que solo contienen
    ''' nueve/catorce dígitos después de descartar caracteres ajenos.
    ''' </summary>
    Private Function CompactarDocumento(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then Return Nothing

        Dim resultado As New StringBuilder()
        For Each caracter As Char In valor.Trim()
            If caracter >= "0"c AndAlso caracter <= "9"c Then
                resultado.Append(caracter)
            ElseIf caracter = "-"c OrElse Char.IsWhiteSpace(caracter) Then
                If caracter = "-"c Then resultado.Append(caracter)
            Else
                Return Nothing
            End If
        Next
        Return resultado.ToString()
    End Function

    Public Function LimpiarDocumento(valor As String) As String
        Dim compacto As String = CompactarDocumento(valor)
        If compacto Is Nothing Then Return ""
        Return compacto.Replace("-", "")
    End Function

    Public Function EsFormatoDuiValido(dui As String) As Boolean
        Dim compacto As String = CompactarDocumento(dui)
        If compacto Is Nothing Then Return False

        If compacto.Length = 9 Then
            Return compacto.All(Function(c As Char) c >= "0"c AndAlso c <= "9"c)
        End If

        Return compacto.Length = 10 AndAlso compacto(8) = "-"c AndAlso
               compacto.Remove(8, 1).All(Function(c As Char) c >= "0"c AndAlso c <= "9"c)
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
        Dim compacto As String = CompactarDocumento(nit)
        If compacto Is Nothing Then Return False

        If compacto.Length = 14 Then
            Return compacto.All(Function(c As Char) c >= "0"c AndAlso c <= "9"c)
        End If

        Return compacto.Length = 17 AndAlso
               compacto(4) = "-"c AndAlso compacto(11) = "-"c AndAlso compacto(15) = "-"c AndAlso
               compacto.Remove(15, 1).Remove(11, 1).Remove(4, 1).
                   All(Function(c As Char) c >= "0"c AndAlso c <= "9"c)
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
