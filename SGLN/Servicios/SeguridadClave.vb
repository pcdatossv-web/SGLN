Imports System
Imports System.Security.Cryptography

Public Class SalHash
    Public Property Hash As String = ""
    Public Property Salt As String = ""
End Class

''' <summary>
''' Derivación de claves con PBKDF2 (Rfc2898DeriveBytes). Nunca se guarda la
''' clave en texto plano.
''' </summary>
Public Module SeguridadClave

    ' Solo reconoce cuentas antiguas creadas con la contraseña fija.
    ' Nunca se utiliza para crear nuevas cuentas.
    Private Const ClaveInicialLegada As String = "Admin123*"

    Private Const Iteraciones As Integer = 100000
    Private Const LongitudHash As Integer = 32
    Private Const LongitudSalt As Integer = 16

    Public Function CrearHash(clave As String, Optional saltBase64 As String = Nothing) As SalHash
        Dim salt As Byte()

        If String.IsNullOrEmpty(saltBase64) Then
            salt = New Byte(LongitudSalt - 1) {}
            Using generador As RandomNumberGenerator = RandomNumberGenerator.Create()
                generador.GetBytes(salt)
            End Using
        Else
            salt = Convert.FromBase64String(saltBase64)
        End If

        Using derivador As New Rfc2898DeriveBytes(clave, salt, Iteraciones)
            Dim hash As Byte() = derivador.GetBytes(LongitudHash)
            Return New SalHash With {
                .Hash = Convert.ToBase64String(hash),
                .Salt = Convert.ToBase64String(salt)
            }
        End Using
    End Function

    Public Function EsClaveInicial(hashBase64 As String, saltBase64 As String) As Boolean
        Return Verificar(ClaveInicialLegada, hashBase64, saltBase64)
    End Function

    ''' <summary>
    ''' La clave inicial solo puede sustituirse por una clave robusta.
    ''' Se aplica en el primer ingreso cuando aún se conserva la clave sembrada.
    ''' </summary>
    Public Function ValidarClaveNuevaInicial(clave As String) As String
        If String.IsNullOrWhiteSpace(clave) OrElse clave.Length < 12 Then
            Return "La nueva clave debe tener al menos 12 caracteres."
        End If
        If String.Equals(clave, ClaveInicialLegada, StringComparison.Ordinal) Then
            Return "Debe utilizar una clave distinta de la clave inicial."
        End If
        If Not clave.Any(Function(ch As Char) Char.IsUpper(ch)) OrElse
           Not clave.Any(Function(ch As Char) Char.IsLower(ch)) OrElse
           Not clave.Any(Function(ch As Char) Char.IsDigit(ch)) OrElse
           Not clave.Any(Function(ch As Char) Not Char.IsLetterOrDigit(ch)) Then
            Return "La clave debe incluir mayúsculas, minúsculas, números y símbolos."
        End If
        Return ""
    End Function

    Public Function Verificar(clave As String, hashBase64 As String, saltBase64 As String) As Boolean
        Try
            Dim calculado As SalHash = CrearHash(clave, saltBase64)
            Dim esperado As Byte() = Convert.FromBase64String(hashBase64)
            Dim obtenido As Byte() = Convert.FromBase64String(calculado.Hash)

            If esperado.Length <> obtenido.Length Then Return False

            ' Comparación en tiempo constante para no filtrar información.
            Dim diferencias As Integer = 0
            For i As Integer = 0 To esperado.Length - 1
                diferencias = diferencias Or (esperado(i) Xor obtenido(i))
            Next
            Return diferencias = 0
        Catch
            Return False
        End Try
    End Function

End Module
