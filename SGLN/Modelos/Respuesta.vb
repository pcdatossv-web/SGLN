''' <summary>
''' Resultado uniforme de una operación de negocio. Evita lanzar excepciones
''' para errores esperados (validaciones) y permite mostrar mensajes claros.
''' </summary>
Public Class Respuesta

    Public Property Exito As Boolean
    Public Property Mensaje As String = ""
    Public Property ID As Integer?

    Public Shared Function Ok(mensaje As String) As Respuesta
        Return New Respuesta With {.Exito = True, .Mensaje = mensaje}
    End Function

    Public Shared Function Fallo(mensaje As String) As Respuesta
        Return New Respuesta With {.Exito = False, .Mensaje = mensaje}
    End Function

End Class
