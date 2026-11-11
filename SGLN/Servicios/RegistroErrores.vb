Imports System
Imports System.IO

''' <summary>
''' Registro de errores en archivo. Se escribe en la carpeta Logs junto al
''' ejecutable para facilitar el diagnóstico en equipos del bufete.
''' </summary>
Public Module RegistroErrores

    Public Sub Escribir(contexto As String, ex As Exception)
        Agregar(contexto, ex.Message & Environment.NewLine & ex.ToString())
    End Sub

    ''' <summary>
    ''' Anota un mensaje informativo que no es una excepción (por ejemplo, una
    ''' conexión fallida al iniciar) sin inventar un stack trace.
    ''' </summary>
    Public Sub Anotar(contexto As String, mensaje As String)
        Agregar(contexto, mensaje)
    End Sub

    Private Sub Agregar(contexto As String, detalle As String)
        Try
            Dim carpeta As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            Directory.CreateDirectory(carpeta)
            Dim archivo As String = Path.Combine(carpeta, "errores_" & Date.Today.ToString("yyyyMMdd") & ".log")
            Dim texto As String = String.Format(
                "[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}{3}{4}",
                Date.Now, contexto, detalle, Environment.NewLine, New String("-"c, 80))
            File.AppendAllText(archivo, texto & Environment.NewLine)
        Catch
            ' Si falla el registro no se debe interrumpir la aplicación.
        End Try
    End Sub

End Module
