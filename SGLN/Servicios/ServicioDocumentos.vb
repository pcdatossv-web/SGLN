Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.Security.Cryptography
Imports System.Windows.Forms

''' <summary>
''' Reglas de negocio de documentos: importación al repositorio local,
''' cálculo de hash SHA-256 y verificación de integridad.
''' </summary>
Public Class ServicioDocumentos

    Private ReadOnly _repositorio As New DocumentoRepositorio()
    Private ReadOnly _catalogo As New CatalogoRepositorio()

    Private Shared ReadOnly ExtensionesPermitidas As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".tif", ".tiff"
    }

    Private Const TamanoMaximoMb As Integer = 50

    Public Function Buscar(filtro As String, tipo As String) As DataTable
        Return _repositorio.Buscar(filtro, tipo)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return _repositorio.Obtener(id)
    End Function

    Public Function CasosCombo() As DataTable
        Return _repositorio.CasosCombo()
    End Function

    Public Function InstrumentosCombo() As DataTable
        Return _repositorio.InstrumentosCombo()
    End Function

    ''' <summary>
    ''' Importa un archivo, lo copia a la carpeta de documentos y registra
    ''' su hash SHA-256 para detectar alteraciones posteriores.
    ''' </summary>
    Public Function Importar(rutaOrigen As String, tipoDocumento As String,
                             idCaso As Integer?, idInstrumento As Integer?) As Respuesta

        If String.IsNullOrWhiteSpace(rutaOrigen) OrElse Not File.Exists(rutaOrigen) Then
            Return Respuesta.Fallo("El archivo seleccionado no existe.")
        End If
        If String.IsNullOrWhiteSpace(tipoDocumento) Then
            Return Respuesta.Fallo("Seleccione el tipo de documento.")
        End If

        Dim extension As String = Path.GetExtension(rutaOrigen)
        If Not ExtensionesPermitidas.Contains(extension) Then
            Return Respuesta.Fallo("Extensión no permitida. Use PDF, Word o imágenes.")
        End If

        Dim informacion As New FileInfo(rutaOrigen)
        If informacion.Length > TamanoMaximoMb * 1024L * 1024L Then
            Return Respuesta.Fallo("El archivo supera el límite de " & TamanoMaximoMb & " MB.")
        End If

        Dim rutaDestino As String = ""
        Try
            Dim carpeta As String = RutaBaseDocumentos()
            Directory.CreateDirectory(carpeta)

            Dim nombreDestino As String = DateTime.Now.ToString("yyyyMMdd_HHmmss") & "_" &
                Guid.NewGuid().ToString("N").Substring(0, 8) & extension.ToLowerInvariant()
            rutaDestino = Path.Combine(carpeta, nombreDestino)

            Dim hash As String = CalcularHashSha256(rutaOrigen)
            File.Copy(rutaOrigen, rutaDestino)

            Dim documento As New Documento With {
                .TipoDocumento = tipoDocumento.Trim(),
                .RutaArchivo = RutaRelativa(rutaDestino),
                .Hash = hash,
                .EstadoProcesamientoIA = "No procesado",
                .IDCaso = idCaso,
                .IDInstrumento = idInstrumento
            }

            documento.ID = _repositorio.Insertar(documento)

            Return New Respuesta With {
                .Exito = True,
                .Mensaje = "Documento importado. SHA-256: " & hash.Substring(0, 16) & "...",
                .ID = documento.ID
            }
        Catch ex As Exception
            ' Si falla el registro, no se deja una copia huérfana en disco.
            Try
                If File.Exists(rutaDestino) Then File.Delete(rutaDestino)
            Catch
                ' La limpieza es best-effort.
            End Try

            RegistroErrores.Escribir("Documentos - importar", ex)
            Return Respuesta.Fallo("No se pudo importar el documento: " & ex.Message)
        End Try
    End Function

    Public Function ActualizarMetadatos(id As Integer, tipoDocumento As String,
                                        idCaso As Integer?, idInstrumento As Integer?) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un documento.")
        If String.IsNullOrWhiteSpace(tipoDocumento) Then Return Respuesta.Fallo("Seleccione el tipo de documento.")

        Dim documento As New Documento With {
            .ID = id,
            .TipoDocumento = tipoDocumento.Trim(),
            .IDCaso = idCaso,
            .IDInstrumento = idInstrumento
        }

        _repositorio.ActualizarMetadatos(documento)
        Return Respuesta.Ok("Metadatos del documento actualizados.")
    End Function

    ''' <summary>
    ''' Recalcula el hash del archivo y lo compara con el registrado.
    ''' </summary>
    Public Function VerificarIntegridad(id As Integer) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un documento.")

        Dim tabla As DataTable = _repositorio.Obtener(id)
        If tabla.Rows.Count = 0 Then Return Respuesta.Fallo("El documento no existe.")

        Dim fila As DataRow = tabla.Rows(0)
        Dim ruta As String = RutaCompleta(Convert.ToString(fila("Ruta_Archivo")))
        If Not File.Exists(ruta) Then
            Return Respuesta.Fallo("El archivo no se encuentra en la ruta registrada.")
        End If

        Dim hashRegistrado As String = Convert.ToString(fila("Hash"))
        Dim hashActual As String = CalcularHashSha256(ruta)

        If String.Equals(hashRegistrado, hashActual, StringComparison.OrdinalIgnoreCase) Then
            Return Respuesta.Ok("Integridad verificada: el archivo coincide con el hash registrado.")
        End If

        Return Respuesta.Fallo("ALERTA: el hash no coincide. El archivo pudo ser alterado.")
    End Function

    Public Function RutaDelDocumento(id As Integer) As String
        Dim tabla As DataTable = _repositorio.Obtener(id)
        If tabla.Rows.Count = 0 Then Return ""
        Return RutaCompleta(Convert.ToString(tabla.Rows(0)("Ruta_Archivo")))
    End Function

    Public Function RutaBaseDocumentos() As String
        Dim carpeta As String = _catalogo.ObtenerConfig("Carpeta_Documentos")
        If String.IsNullOrWhiteSpace(carpeta) Then carpeta = "Documentos"
        If Not Path.IsPathRooted(carpeta) Then
            carpeta = Path.Combine(Application.StartupPath, carpeta)
        End If
        Return carpeta
    End Function

    Public Function RutaCompleta(rutaRegistrada As String) As String
        If String.IsNullOrWhiteSpace(rutaRegistrada) Then Return ""
        If Path.IsPathRooted(rutaRegistrada) Then Return rutaRegistrada
        Return Path.Combine(Application.StartupPath, rutaRegistrada)
    End Function

    Private Function RutaRelativa(rutaAbsoluta As String) As String
        Dim base As String = Application.StartupPath.TrimEnd(Path.DirectorySeparatorChar) & Path.DirectorySeparatorChar
        If rutaAbsoluta.StartsWith(base, StringComparison.OrdinalIgnoreCase) Then
            Return rutaAbsoluta.Substring(base.Length)
        End If
        Return rutaAbsoluta
    End Function

    Public Shared Function CalcularHashSha256(ruta As String) As String
        Using algoritmo As SHA256 = SHA256.Create()
            Using flujo As FileStream = File.OpenRead(ruta)
                Dim bytes As Byte() = algoritmo.ComputeHash(flujo)
                Return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Using
    End Function

End Class
