Imports System
Imports System.IO
Imports System.Text

''' <summary>
''' Registro de errores por usuario Windows. Se escribe en LocalAppData para
''' que funcione sin permisos de administrador y no exponga datos a otros perfiles.
''' </summary>
Public Module RegistroErrores

    Private ReadOnly BloqueoEscritura As New Object()
    Private Const MaximoArchivoBytes As Long = 5L * 1024L * 1024L
    Private Const MaximoDetalleCaracteres As Integer = 16000
    Private Const MaximoContextoCaracteres As Integer = 120

    Public Sub Escribir(contexto As String, ex As Exception)
        If ex Is Nothing Then
            Agregar(contexto, "Se recibió una excepción nula.")
            Return
        End If
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
            Dim datosLocales As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            If String.IsNullOrWhiteSpace(datosLocales) Then
                Throw New InvalidOperationException("Windows no proporcionó la carpeta LocalAppData del usuario.")
            End If

            Dim carpeta As String = Path.Combine(datosLocales, "SGLN", "Logs")
            Directory.CreateDirectory(carpeta)
            Dim archivo As String = Path.Combine(carpeta, "errores_" & Date.Today.ToString("yyyyMMdd") & ".log")
            Dim archivoAnterior As String = archivo & ".previous"

            Dim contextoSeguro As String = If(contexto, "Sin contexto").
                Replace(ControlChars.Cr, " ").Replace(ControlChars.Lf, " ").Replace(ControlChars.Tab, " ").Trim()
            If contextoSeguro.Length > MaximoContextoCaracteres Then
                contextoSeguro = contextoSeguro.Substring(0, MaximoContextoCaracteres)
            End If

            Dim detalleSeguro As String = If(detalle, "")
            If detalleSeguro.Length > MaximoDetalleCaracteres Then
                detalleSeguro = detalleSeguro.Substring(0, MaximoDetalleCaracteres) &
                    Environment.NewLine & "[Detalle truncado por límite de registro]"
            End If

            Dim texto As String = String.Format(
                "[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}{3}{4}",
                Date.Now, contextoSeguro, detalleSeguro, Environment.NewLine, New String("-"c, 80))
            Dim entrada As String = texto & Environment.NewLine

            SyncLock BloqueoEscritura
                ' Rotación simple: conserva el archivo anterior y limita el
                ' crecimiento del registro diario sin borrar evidencia reciente.
                If File.Exists(archivo) AndAlso New FileInfo(archivo).Length >= MaximoArchivoBytes Then
                    If File.Exists(archivoAnterior) Then File.Delete(archivoAnterior)
                    File.Move(archivo, archivoAnterior)
                End If
                File.AppendAllText(archivo, entrada, Encoding.UTF8)
            End SyncLock
        Catch
            ' Si falla el registro no se debe interrumpir la aplicación.
        End Try
    End Sub

End Module
