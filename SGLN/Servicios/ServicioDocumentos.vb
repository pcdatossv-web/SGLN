Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.IO.Compression
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
        If Not Permisos.TieneAcceso("documentos.ver") Then Return New DataTable()
        Return _repositorio.Buscar(filtro, tipo)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If Not Permisos.TieneAcceso("documentos.ver") OrElse id <= 0 Then Return New DataTable()
        Return _repositorio.Obtener(id)
    End Function

    Public Function CasosCombo() As DataTable
        If Not (Permisos.TieneAcceso("documentos.ver") OrElse Permisos.TieneAcceso("documentos.importar") OrElse Permisos.TieneAcceso("documentos.metadatos")) Then Return New DataTable()
        Return _repositorio.CasosCombo()
    End Function

    Public Function InstrumentosCombo() As DataTable
        If Not (Permisos.TieneAcceso("documentos.ver") OrElse Permisos.TieneAcceso("documentos.importar") OrElse Permisos.TieneAcceso("documentos.metadatos")) Then Return New DataTable()
        Return _repositorio.InstrumentosCombo()
    End Function

    ''' <summary>
    ''' Importa un archivo, lo copia a la carpeta de documentos y registra
    ''' su hash SHA-256 para detectar alteraciones posteriores.
    ''' </summary>
    Public Function Importar(rutaOrigen As String, tipoDocumento As String,
                             idCaso As Integer?, idInstrumento As Integer?) As Respuesta
        If Not Permisos.TieneAcceso("documentos.importar") Then Return Respuesta.Fallo("No tiene permiso para importar documentos.")


        If String.IsNullOrWhiteSpace(rutaOrigen) OrElse Not File.Exists(rutaOrigen) Then
            Return Respuesta.Fallo("El archivo seleccionado no existe.")
        End If
        If String.IsNullOrWhiteSpace(tipoDocumento) Then
            Return Respuesta.Fallo("Seleccione el tipo de documento.")
        End If
        If tipoDocumento.Trim().Length > 60 Then
            Return Respuesta.Fallo("El tipo de documento no puede superar 60 caracteres.")
        End If
        Dim errorVinculos As String = ValidarReferencias(idCaso, idInstrumento)
        If errorVinculos <> "" Then Return Respuesta.Fallo(errorVinculos)

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

            ' Validar también antes de escribir: comprobar los enlaces únicamente
            ' al abrir el documento deja una ventana en la que una carpeta
            ' documental sustituida por un junction/symlink recibe archivos.
            ' RutaCompleta verifica la raíz y cada segmento existente.
            Dim rutaDestinoValidada As String = RutaCompleta(
                Path.Combine(carpeta, "validacion_" & Guid.NewGuid().ToString("N") & ".tmp"))

            Dim nombreDestino As String = DateTime.Now.ToString("yyyyMMdd_HHmmss") & "_" &
                Guid.NewGuid().ToString("N").Substring(0, 8) & extension.ToLowerInvariant()
            ' Construir el destino desde la ruta absoluta que acaba de validarse,
            ' no desde la cadena original de configuración.
            rutaDestino = Path.Combine(Path.GetDirectoryName(rutaDestinoValidada), nombreDestino)

            ' Volver a comprobar el nombre exacto que se va a crear. La validación
            ' del archivo temporal anterior solo cubre la carpeta y sus segmentos.
            rutaDestino = RutaCompleta(rutaDestino)

            ' Se calcula el hash sobre la copia final, no sobre el origen:
            ' evita registrar un hash distinto si el archivo cambia durante la copia.
            File.Copy(rutaOrigen, rutaDestino)

            ' Comprobar otra vez la ruta después de crear el archivo para detectar
            ' cambios de directorio o puntos de reanálisis durante la importación.
            Dim rutaCopiaValidada As String = RutaCompleta(rutaDestino)
            If Not String.Equals(Path.GetFullPath(rutaCopiaValidada),
                                 Path.GetFullPath(rutaDestino),
                                 StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("La ruta del archivo cambió durante la importación.")
            End If

            ' Revalidar el tamaño de la copia final: el archivo de origen puede
            ' cambiar entre FileInfo y File.Copy (TOCTOU). El límite debe
            ' aplicarse al contenido que realmente se almacenará.
            Dim informacionCopia As New FileInfo(rutaDestino)
            If informacionCopia.Length <= 0 OrElse
               informacionCopia.Length > TamanoMaximoMb * 1024L * 1024L Then
                Throw New InvalidDataException(
                    "El archivo copiado está vacío o supera el límite de " & TamanoMaximoMb & " MB.")
            End If

            If Not ValidarContenidoArchivo(rutaDestino, extension) Then
                Throw New InvalidDataException(
                    "El contenido del archivo no corresponde a su extensión o el documento está dañado.")
            End If
            Dim hash As String = CalcularHashSha256(rutaDestino)

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
                If File.Exists(rutaDestino) Then
                    ' No borrar a través de una ruta que ahora resuelva mediante
                    ' un enlace/punto de reanálisis: podría apuntar fuera del almacén.
                    Dim rutaLimpieza As String = RutaCompleta(rutaDestino)
                    If String.Equals(Path.GetFullPath(rutaLimpieza),
                                     Path.GetFullPath(rutaDestino),
                                     StringComparison.OrdinalIgnoreCase) Then
                        File.Delete(rutaLimpieza)
                    End If
                End If
            Catch
                ' La limpieza es best-effort; ante una ruta insegura se conserva
                ' el archivo para evitar borrar datos ajenos fuera del almacén.
            End Try

            RegistroErrores.Escribir("Documentos - importar", ex)
            Return Respuesta.Fallo("No se pudo importar el documento. Verifique el archivo, los permisos de almacenamiento y la conexión con la base de datos.")
        End Try
    End Function

    Public Function ActualizarMetadatos(id As Integer, tipoDocumento As String,
                                        idCaso As Integer?, idInstrumento As Integer?) As Respuesta
        If Not Permisos.TieneAcceso("documentos.metadatos") Then Return Respuesta.Fallo("No tiene permiso para modificar metadatos de documentos.")

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un documento.")
        If String.IsNullOrWhiteSpace(tipoDocumento) Then Return Respuesta.Fallo("Seleccione el tipo de documento.")
        If tipoDocumento.Trim().Length > 60 Then Return Respuesta.Fallo("El tipo de documento no puede superar 60 caracteres.")
        Dim errorVinculos As String = ValidarReferencias(idCaso, idInstrumento)
        If errorVinculos <> "" Then Return Respuesta.Fallo(errorVinculos)

        Dim documento As New Documento With {
            .ID = id,
            .TipoDocumento = tipoDocumento.Trim(),
            .IDCaso = idCaso,
            .IDInstrumento = idInstrumento
        }

        Try
            Dim filas As Integer = _repositorio.ActualizarMetadatos(documento)
            If filas = 0 AndAlso _repositorio.Obtener(id).Rows.Count = 0 Then
                Return Respuesta.Fallo("El documento ya no existe o fue eliminado.")
            End If
            Return Respuesta.Ok("Metadatos del documento actualizados.")
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - actualizar metadatos", ex)
            Return Respuesta.Fallo("No se pudieron actualizar los metadatos. Verifique la conexión y los datos.")
        End Try
    End Function

    ''' <summary>
    ''' Recalcula el hash del archivo y lo compara con el registrado.
    ''' </summary>
    Public Function VerificarIntegridad(id As Integer) As Respuesta
        If Not Permisos.TieneAcceso("documentos.verificar") Then Return Respuesta.Fallo("No tiene permiso para verificar documentos.")
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un documento.")

        Try
            Dim tabla As DataTable = _repositorio.Obtener(id)
            If tabla.Rows.Count = 0 Then Return Respuesta.Fallo("El documento no existe.")

            Dim fila As DataRow = tabla.Rows(0)
            Dim ruta As String = RutaCompleta(Convert.ToString(fila("Ruta_Archivo")))
            If Not File.Exists(ruta) Then
                Return Respuesta.Fallo("El archivo no se encuentra en la ruta registrada.")
            End If
            If Not ValidarContenidoArchivo(ruta, Path.GetExtension(ruta)) Then
                Return Respuesta.Fallo("El contenido del archivo no corresponde al formato registrado.")
            End If

            Dim hashRegistrado As String = Convert.ToString(fila("Hash"))
            Dim hashActual As String = CalcularHashSha256(ruta)

            If String.Equals(hashRegistrado, hashActual, StringComparison.OrdinalIgnoreCase) Then
                Return Respuesta.Ok("Integridad verificada: el archivo coincide con el hash registrado.")
            End If

            Return Respuesta.Fallo("ALERTA: el hash no coincide. El archivo pudo ser alterado.")
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - verificar integridad", ex)
            Return Respuesta.Fallo("No se pudo verificar el documento. Revise la ruta, los permisos y la conexión.")
        End Try
    End Function

    Public Function RutaDelDocumento(id As Integer) As String
        If Not Permisos.TieneAcceso("documentos.ver") Then
            Throw New UnauthorizedAccessException("No tiene permiso para consultar documentos.")
        End If

        Dim tabla As DataTable = _repositorio.Obtener(id)
        If tabla.Rows.Count = 0 Then Return ""

        Dim fila As DataRow = tabla.Rows(0)
        Dim ruta As String = RutaCompleta(Convert.ToString(fila("Ruta_Archivo")))
        If Not File.Exists(ruta) Then Return ""
        If Not ValidarContenidoArchivo(ruta, Path.GetExtension(ruta)) Then
            Throw New InvalidDataException(
                "El contenido del archivo no corresponde a un formato documental permitido.")
        End If

        Dim hashRegistrado As String = Convert.ToString(fila("Hash"))
        Dim hashActual As String = CalcularHashSha256(ruta)
        If Not String.Equals(hashRegistrado, hashActual, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException(
                "El hash SHA-256 no coincide. Por seguridad, el documento no se abrirá.")
        End If

        Return ruta
    End Function

    Public Function RutaBaseDocumentos() As String
        If Not (Permisos.TieneAcceso("documentos.ver") OrElse
                Permisos.TieneAcceso("documentos.importar") OrElse
                Permisos.TieneAcceso("documentos.verificar") OrElse
                Permisos.TieneAcceso("documentos.metadatos")) Then Return ""
        Dim carpeta As String = _catalogo.ObtenerConfig("Carpeta_Documentos")
        If String.IsNullOrWhiteSpace(carpeta) Then carpeta = "Documentos"
        If Not Path.IsPathRooted(carpeta) Then
            carpeta = Path.Combine(Application.StartupPath, carpeta)
        End If

        Dim rutaCompleta As String = Path.GetFullPath(carpeta)
        Dim raiz As String = Path.GetPathRoot(rutaCompleta)
        If String.Equals(
            rutaCompleta.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            raiz.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                "La carpeta documental debe ser una carpeta dedicada; no se permite usar la raíz de una unidad o recurso compartido.")
        End If
        Return rutaCompleta
    End Function

    ''' <summary>
    ''' Resuelve una ruta almacenada y rechaza rutas relativas que intenten
    ''' salir de la carpeta documental. Las rutas absolutas heredadas solo se
    ''' aceptan si permanecen dentro de la carpeta documental configurada.
    ''' </summary>
    Public Function RutaCompleta(rutaRegistrada As String) As String
        If Not (Permisos.TieneAcceso("documentos.ver") OrElse
                Permisos.TieneAcceso("documentos.importar") OrElse
                Permisos.TieneAcceso("documentos.verificar") OrElse
                Permisos.TieneAcceso("documentos.metadatos")) Then Return ""
        If String.IsNullOrWhiteSpace(rutaRegistrada) Then Return ""

        Dim base As String = Path.GetFullPath(RutaBaseDocumentos())
        Dim raiz As String = Path.GetPathRoot(base)
        ' No quitar el separador de una raíz (C:\ o una raíz UNC):
        ' hacerlo convertiría, por ejemplo, C:\ en la ruta relativa C:.
        If Not String.Equals(base, raiz, StringComparison.OrdinalIgnoreCase) Then
            base = base.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        End If

        Dim destino As String = If(Path.IsPathRooted(rutaRegistrada),
                                   Path.GetFullPath(rutaRegistrada),
                                   Path.GetFullPath(Path.Combine(Application.StartupPath, rutaRegistrada)))
        Dim prefijo As String = If(base.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal),
                                  base, base & Path.DirectorySeparatorChar)

        If Not destino.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase) AndAlso
           Not String.Equals(destino, base, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("La ruta del documento está fuera de la carpeta documental autorizada.")
        End If

        ValidarRutaSinEnlaces(base, destino)
        Return destino
    End Function

    ''' <summary>
    ''' Rechaza enlaces simbólicos y puntos de reanálisis dentro de la carpeta
    ''' documental para evitar que una ruta aparentemente interna redirija a
    ''' archivos externos mediante junctions o symlinks de Windows.
    ''' </summary>
    Private Sub ValidarRutaSinEnlaces(base As String, destino As String)
        If Directory.Exists(base) AndAlso
           (File.GetAttributes(base) And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
            Throw New InvalidOperationException("La carpeta documental no puede ser un enlace o punto de reanálisis.")
        End If

        Dim relativo As String = destino.Substring(base.TrimEnd(Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar).Length).TrimStart(Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar)
        If relativo = "" Then Return

        Dim segmentos As String() = relativo.Split(
            New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar},
            StringSplitOptions.RemoveEmptyEntries)
        Dim actual As String = base
        For Each segmento As String In segmentos
            actual = Path.Combine(actual, segmento)
            If File.Exists(actual) OrElse Directory.Exists(actual) Then
                If (File.GetAttributes(actual) And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                    Throw New InvalidOperationException("La ruta documental contiene un enlace o punto de reanálisis no permitido.")
                End If
            End If
        Next
    End Sub

    Private Function ValidarReferencias(idCaso As Integer?, idInstrumento As Integer?) As String
        If idCaso.HasValue AndAlso idCaso.Value <= 0 Then Return "El caso asociado no es válido."
        If idInstrumento.HasValue AndAlso idInstrumento.Value <= 0 Then Return "El instrumento asociado no es válido."
        Try
            If idCaso.HasValue AndAlso Not New CasoRepositorio().ExisteParaVinculoDocumento(idCaso.Value) Then
                Return "El caso asociado no existe."
            End If
            If idInstrumento.HasValue AndAlso Not New ProtocoloRepositorio().ExisteInstrumentoParaVinculoDocumento(idInstrumento.Value) Then
                Return "El instrumento asociado no existe."
            End If
            Return ""
        Catch ex As Exception
            RegistroErrores.Escribir("Documentos - validar referencias", ex)
            Return "No se pudieron verificar los registros asociados. Compruebe la conexión y vuelva a intentarlo."
        End Try
    End Function

    ''' <summary>
    ''' Verifica firmas básicas del formato real después de copiar el archivo.
    ''' La extensión por sí sola no demuestra que el contenido sea un documento.
    ''' </summary>
    Private Shared Function ValidarContenidoArchivo(ruta As String, extension As String) As Boolean
        Select Case If(extension, "").ToLowerInvariant()
            Case ".pdf"
                Using flujo As FileStream = File.OpenRead(ruta)
                    Dim buffer(1023) As Byte
                    Dim leidos As Integer = flujo.Read(buffer, 0, buffer.Length)
                    Dim inicio As String = System.Text.Encoding.ASCII.GetString(buffer, 0, leidos)
                    Return inicio.IndexOf("%PDF-", StringComparison.Ordinal) >= 0
                End Using

            Case ".jpg", ".jpeg"
                Using lector As New BinaryReader(File.OpenRead(ruta))
                    Dim firma As Byte() = lector.ReadBytes(3)
                    Return firma.Length = 3 AndAlso firma(0) = &HFF AndAlso
                           firma(1) = &HD8 AndAlso firma(2) = &HFF
                End Using

            Case ".png"
                Using lector As New BinaryReader(File.OpenRead(ruta))
                    Dim firma As Byte() = lector.ReadBytes(8)
                    Dim esperada As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}
                    Return firma.SequenceEqual(esperada)
                End Using

            Case ".tif", ".tiff"
                Using lector As New BinaryReader(File.OpenRead(ruta))
                    Dim firma As Byte() = lector.ReadBytes(4)
                    If firma.Length <> 4 Then Return False
                    Return (firma(0) = &H49 AndAlso firma(1) = &H49 AndAlso
                            (firma(2) = &H2A OrElse firma(2) = &H2B) AndAlso
                            firma(3) = &H0) OrElse
                           (firma(0) = &H4D AndAlso firma(1) = &H4D AndAlso
                            firma(2) = &H0 AndAlso (firma(3) = &H2A OrElse firma(3) = &H2B))
                End Using

            Case ".doc"
                Using lector As New BinaryReader(File.OpenRead(ruta))
                    Dim firma As Byte() = lector.ReadBytes(8)
                    Dim esperada As Byte() = {&HD0, &HCF, &H11, &HE0, &HA1, &HB1, &H1A, &HE1}
                    Return firma.SequenceEqual(esperada)
                End Using

            Case ".docx"
                Try
                    Using archivoZip As ZipArchive = ZipFile.OpenRead(ruta)
                        Return archivoZip.GetEntry("[Content_Types].xml") IsNot Nothing AndAlso
                               archivoZip.GetEntry("word/document.xml") IsNot Nothing AndAlso
                               archivoZip.GetEntry("word/vbaProject.bin") Is Nothing
                    End Using
                Catch ex As InvalidDataException
                    Return False
                End Try
        End Select

        Return False
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
