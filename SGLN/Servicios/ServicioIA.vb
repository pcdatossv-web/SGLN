Imports System

''' <summary>
''' Reservado para la integración de IA/OCR, que se implementará en la
''' fase final del proyecto. Se deja deshabilitado a propósito: ningún
''' resultado automático sustituye la revisión del abogado.
''' </summary>
Public Class ServicioIA

    Public ReadOnly Property Habilitado As Boolean
        Get
            Return False
        End Get
    End Property

    Public Function ProcesarDocumento(idDocumento As Integer) As Respuesta
        Return Respuesta.Fallo(
            "El procesamiento con IA/OCR está planificado para la fase final. " &
            "Mientras tanto, el documento debe revisarse manualmente.")
    End Function

End Class
