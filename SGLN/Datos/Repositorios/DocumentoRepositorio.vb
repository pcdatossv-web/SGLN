Imports System
Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text.RegularExpressions

''' <summary>
''' Acceso a datos de los documentos del expediente y su vínculo con
''' casos e instrumentos notariales.
''' </summary>
Public Class DocumentoRepositorio
    Inherits RepositorioBase

    Private Shared Function TipoDocumentoValido(tipo As String) As Boolean
        If String.IsNullOrWhiteSpace(tipo) Then Return False
        Select Case tipo.Trim()
            Case "Escritura", "Poder", "Demanda", "Contestación", "Sentencia",
                 "Resolución", "Contrato", "Identificación", "Comprobante", "Otro"
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Revalida la sesión una vez y consulta la matriz de permisos vigente.
    ''' El repositorio mantiene esta barrera aunque se invoque sin pasar por el servicio.
    ''' </summary>
    Private Function PuedeConsultarDocumentos() As Boolean
        ' Importar no concede acceso de lectura a los registros existentes.
        Return Permisos.TieneAccesoAlguno("documentos.ver", "documentos.verificar",
                                 "documentos.metadatos")
    End Function

    Private Function PuedeCargarCombosDocumentos() As Boolean
        Return Permisos.TieneAccesoAlguno("documentos.ver", "documentos.importar",
                                 "documentos.metadatos")
    End Function

    Private Const TamanoPaginaDocumentos As Integer = 100

    Private Shared Function EsHashSha256Completo(valor As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(valor) AndAlso
               Regex.IsMatch(valor.Trim(), "\A[0-9a-fA-F]{64}\z")
    End Function

    Private Sub AplicarFiltroDocumentos(comando As MySqlCommand, filtro As String,
                                        tipo As String, hashExacto As Boolean)
        Dim texto As String = If(filtro, "").Trim()
        Dim tipoFiltro As String = If(tipo, "").Trim()
        If hashExacto Then
            Parametro(comando, "@hashExacto", texto)
        Else
            Parametro(comando, "@filtro", If(texto = "", DBNull.Value, CObj(texto)))
            Parametro(comando, "@like", "%" & texto & "%")
        End If
        Parametro(comando, "@tipo", If(tipoFiltro = "", DBNull.Value, CObj(tipoFiltro)))
    End Sub

    Public Function Buscar(filtro As String, tipo As String, pagina As Integer) As DataTable
        If Not PuedeConsultarDocumentos() Then Return New DataTable()
        If pagina < 1 Then Throw New ArgumentOutOfRangeException(NameOf(pagina))
        Dim offset As Long = CLng(pagina - 1) * TamanoPaginaDocumentos
        If offset > Integer.MaxValue Then Throw New ArgumentOutOfRangeException(NameOf(pagina))

        Dim hashExacto As Boolean = EsHashSha256Completo(filtro)
        Dim condicionBusqueda As String =
            If(hashExacto,
               "d.Hash = @hashExacto",
               "(@filtro IS NULL OR d.Tipo_Documento LIKE @like OR d.Ruta_Archivo LIKE @like " &
               " OR d.Hash LIKE @like OR IFNULL(c.Numero_Expediente, '') LIKE @like)")

        Return Consultar(
            "SELECT d.ID, d.Tipo_Documento, d.Ruta_Archivo, d.Hash, d.Estado_Procesamiento_IA, " &
            "       d.Fecha_Carga, IFNULL(d.Usuario, '') AS Usuario, " &
            "       IFNULL(c.Numero_Expediente, '') AS Expediente, " &
            "       IFNULL(i.Numero_Escritura, 0) AS Escritura, " &
            "       d.ID_Caso, d.ID_Instrumento " &
            "FROM Documentos d " &
            "LEFT JOIN Casos c ON c.ID = d.ID_Caso " &
            "LEFT JOIN Instrumentos i ON i.ID = d.ID_Instrumento " &
            "WHERE " & condicionBusqueda & " " &
            "  AND (@tipo IS NULL OR d.Tipo_Documento = @tipo) " &
            "ORDER BY d.Fecha_Carga DESC, d.ID DESC LIMIT @limite OFFSET @offset",
            Sub(comando As MySqlCommand)
                AplicarFiltroDocumentos(comando, filtro, tipo, hashExacto)
                Parametro(comando, "@limite", TamanoPaginaDocumentos)
                Parametro(comando, "@offset", CInt(offset))
            End Sub)
    End Function

    Public Function Contar(filtro As String, tipo As String) As Integer
        If Not PuedeConsultarDocumentos() Then Return 0
        Dim hashExacto As Boolean = EsHashSha256Completo(filtro)
        Dim condicionBusqueda As String =
            If(hashExacto,
               "d.Hash = @hashExacto",
               "(@filtro IS NULL OR d.Tipo_Documento LIKE @like OR d.Ruta_Archivo LIKE @like " &
               " OR d.Hash LIKE @like OR IFNULL(c.Numero_Expediente, '') LIKE @like)")

        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Documentos d " &
            "LEFT JOIN Casos c ON c.ID = d.ID_Caso " &
            "WHERE " & condicionBusqueda & " " &
            "  AND (@tipo IS NULL OR d.Tipo_Documento = @tipo)",
            Sub(comando As MySqlCommand)
                AplicarFiltroDocumentos(comando, filtro, tipo, hashExacto)
            End Sub)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If id <= 0 OrElse Not PuedeConsultarDocumentos() Then Return New DataTable()
        Return Consultar(
            "SELECT * FROM Documentos WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    ''' <summary>
    ''' Valida y bloquea las referencias dentro de la misma transacción que
    ''' registra el documento. La validación del servicio es informativa; esta
    ''' comprobación es la garantía final frente a llamadas directas y carreras.
    ''' Se conserva la posibilidad de vincular documentos a instrumentos anulados
    ''' porque forman parte del historial notarial.
    ''' </summary>
    Private Sub ValidarReferenciasEnTransaccion(conexion As MySqlConnection,
                                                transaccion As MySqlTransaction,
                                                idCaso As Integer?,
                                                idInstrumento As Integer?)
        If idCaso.HasValue Then
            Using comando As New MySqlCommand(
                "SELECT ID FROM Casos WHERE ID = @id FOR UPDATE",
                conexion, transaccion)
                Parametro(comando, "@id", idCaso.Value)
                Dim valor As Object = comando.ExecuteScalar()
                If valor Is Nothing OrElse Convert.IsDBNull(valor) Then
                    Throw New InvalidOperationException("El caso asociado ya no existe.")
                End If
            End Using
        End If

        If idInstrumento.HasValue Then
            Using comando As New MySqlCommand(
                "SELECT ID FROM Instrumentos WHERE ID = @id FOR UPDATE",
                conexion, transaccion)
                Parametro(comando, "@id", idInstrumento.Value)
                Dim valor As Object = comando.ExecuteScalar()
                If valor Is Nothing OrElse Convert.IsDBNull(valor) Then
                    Throw New InvalidOperationException("El instrumento asociado ya no existe.")
                End If
            End Using
        End If
    End Sub

    Public Function Insertar(documento As Documento) As Integer
        If Not Permisos.TieneAcceso("documentos.importar") Then
            Throw New UnauthorizedAccessException("No tiene permiso para importar documentos.")
        End If
        If documento Is Nothing Then Throw New ArgumentNullException(NameOf(documento))
        If Not TipoDocumentoValido(documento.TipoDocumento) Then
            Throw New ArgumentException("El tipo de documento no pertenece al catálogo permitido.", NameOf(documento))
        End If
        If String.IsNullOrWhiteSpace(documento.RutaArchivo) OrElse documento.RutaArchivo.Trim().Length > 500 Then
            Throw New ArgumentException("La ruta del documento debe contener entre 1 y 500 caracteres.", NameOf(documento))
        End If
        If Regex.IsMatch(documento.RutaArchivo, "[\x00-\x1F]") Then
            Throw New ArgumentException("La ruta del documento contiene caracteres de control no permitidos.", NameOf(documento))
        End If
        documento.TipoDocumento = documento.TipoDocumento.Trim()
        documento.RutaArchivo = documento.RutaArchivo.Trim()
        If String.IsNullOrWhiteSpace(documento.Hash) OrElse
           Not Regex.IsMatch(documento.Hash, "\A[0-9a-fA-F]{64}\z") Then
            Throw New ArgumentException("El hash del documento debe ser SHA-256 hexadecimal de 64 caracteres.", NameOf(documento))
        End If
        If documento.IDCaso.HasValue AndAlso documento.IDCaso.Value <= 0 Then
            Throw New ArgumentException("El caso asociado no es válido.", NameOf(documento))
        End If
        If documento.IDInstrumento.HasValue AndAlso documento.IDInstrumento.Value <= 0 Then
            Throw New ArgumentException("El instrumento asociado no es válido.", NameOf(documento))
        End If
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                ValidarReferenciasEnTransaccion(conexion, transaccion,
                    documento.IDCaso, documento.IDInstrumento)
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Documentos (Tipo_Documento, Ruta_Archivo, Hash, Estado_Procesamiento_IA, " &
                    "       ID_Caso, ID_Instrumento, Usuario) " &
                    "VALUES (@tipo, @ruta, @hash, 'No procesado', @caso, @instrumento, @usuario)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@tipo", documento.TipoDocumento)
                        Parametro(comando, "@ruta", documento.RutaArchivo)
                        Parametro(comando, "@hash", documento.Hash)
                        Parametro(comando, "@caso", If(documento.IDCaso.HasValue, CObj(documento.IDCaso.Value), DBNull.Value))
                        Parametro(comando, "@instrumento", If(documento.IDInstrumento.HasValue, CObj(documento.IDInstrumento.Value), DBNull.Value))
                        Parametro(comando, "@usuario", Sesion.NombreUsuario)
                    End Sub)

                AuditarEnTransaccion(conexion, transaccion, "IMPORTAR DOCUMENTO", "Documentos", nuevoId,
                    documento.TipoDocumento & " | SHA-256 " & documento.Hash)
                Return nuevoId
            End Function)
    End Function

    ''' <summary>
    ''' Comprueba si una importación quedó confirmada en MySQL aunque el cliente
    ''' haya perdido la respuesta del COMMIT. Se usa para no borrar un archivo
    ''' que ya está referenciado por un registro persistido.
    ''' </summary>
    Public Function ExisteRegistroArchivo(rutaArchivo As String, hash As String) As Boolean
        If Not Permisos.TieneAcceso("documentos.importar") Then
            Throw New UnauthorizedAccessException("No tiene permiso para reconciliar una importación documental.")
        End If
        If String.IsNullOrWhiteSpace(rutaArchivo) OrElse rutaArchivo.Trim().Length > 500 Then
            Throw New ArgumentException("La ruta del documento no es válida.", NameOf(rutaArchivo))
        End If
        If String.IsNullOrWhiteSpace(hash) OrElse
           Not Regex.IsMatch(hash, "\A[0-9a-fA-F]{64}\z") Then
            Throw New ArgumentException("El hash del documento no es válido.", NameOf(hash))
        End If

        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Documentos WHERE Ruta_Archivo = @ruta AND Hash = @hash",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@ruta", rutaArchivo.Trim())
                Parametro(comando, "@hash", hash.Trim())
            End Sub) > 0
    End Function

    Public Function ActualizarMetadatos(documento As Documento) As Integer
        If Not Permisos.TieneAcceso("documentos.metadatos") Then
            Throw New UnauthorizedAccessException("No tiene permiso para modificar metadatos de documentos.")
        End If
        If documento Is Nothing OrElse documento.ID <= 0 Then
            Throw New ArgumentException("El documento seleccionado no es válido.", NameOf(documento))
        End If
        If Not TipoDocumentoValido(documento.TipoDocumento) Then
            Throw New ArgumentException("El tipo de documento no pertenece al catálogo permitido.", NameOf(documento))
        End If
        documento.TipoDocumento = documento.TipoDocumento.Trim()
        If documento.IDCaso.HasValue AndAlso documento.IDCaso.Value <= 0 Then
            Throw New ArgumentException("El caso asociado no es válido.", NameOf(documento))
        End If
        If documento.IDInstrumento.HasValue AndAlso documento.IDInstrumento.Value <= 0 Then
            Throw New ArgumentException("El instrumento asociado no es válido.", NameOf(documento))
        End If
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                ' Bloquear primero la fila y conservar los metadatos anteriores
                ' para registrar una rectificación trazable sin incluir rutas ni datos personales.
                Dim tipoAnterior As String = Nothing
                Dim casoAnterior As Integer? = Nothing
                Dim instrumentoAnterior As Integer? = Nothing
                Using comandoExiste As New MySqlCommand(
                    "SELECT Tipo_Documento, ID_Caso, ID_Instrumento FROM Documentos WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(comandoExiste, "@id", documento.ID)
                    Using lector As MySqlDataReader = comandoExiste.ExecuteReader()
                        If Not lector.Read() Then Return 0
                        tipoAnterior = Convert.ToString(lector("Tipo_Documento"))
                        If Not lector.IsDBNull(1) Then casoAnterior = Convert.ToInt32(lector("ID_Caso"))
                        If Not lector.IsDBNull(2) Then instrumentoAnterior = Convert.ToInt32(lector("ID_Instrumento"))
                    End Using
                End Using

                ValidarReferenciasEnTransaccion(conexion, transaccion,
                    documento.IDCaso, documento.IDInstrumento)

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Documentos SET Tipo_Documento = @tipo, ID_Caso = @caso, ID_Instrumento = @instrumento " &
                    "WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@tipo", documento.TipoDocumento)
                        Parametro(comando, "@caso", If(documento.IDCaso.HasValue, CObj(documento.IDCaso.Value), DBNull.Value))
                        Parametro(comando, "@instrumento", If(documento.IDInstrumento.HasValue, CObj(documento.IDInstrumento.Value), DBNull.Value))
                        Parametro(comando, "@id", documento.ID)
                    End Sub)

                If filas > 0 Then
                    Dim detalle As String =
                        "Metadatos rectificados; tipo: " & tipoAnterior & " -> " & documento.TipoDocumento &
                        "; caso: " & If(casoAnterior.HasValue, casoAnterior.Value.ToString(), "ninguno") &
                        " -> " & If(documento.IDCaso.HasValue, documento.IDCaso.Value.ToString(), "ninguno") &
                        "; instrumento: " & If(instrumentoAnterior.HasValue, instrumentoAnterior.Value.ToString(), "ninguno") &
                        " -> " & If(documento.IDInstrumento.HasValue, documento.IDInstrumento.Value.ToString(), "ninguno")
                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Documentos", documento.ID, detalle)
                    Return filas
                End If

                ' La fila está bloqueada y existe; no registrar una modificación
                ' cuando el usuario simplemente vuelve a guardar los mismos datos.
                Return 1
            End Function)
    End Function

    ''' <summary>
    ''' Registra una alerta cuando un archivo documental no supera una comprobación
    ''' de existencia, formato o hash. No almacena rutas ni contenido del archivo.
    ''' </summary>
    Public Sub RegistrarAlertaIntegridad(idDocumento As Integer, detalle As String)
        If Not Permisos.TieneAccesoAlguno("documentos.ver", "documentos.verificar") Then
            Throw New UnauthorizedAccessException("No tiene permiso para registrar alertas de integridad documental.")
        End If
        If idDocumento <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(idDocumento))
        Dim detalleSeguro As String = If(detalle, "Fallo de integridad documental").Trim()
        If detalleSeguro.Length > 300 Then detalleSeguro = detalleSeguro.Substring(0, 300)

        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Using comando As New MySqlCommand(
                    "SELECT ID FROM Documentos WHERE ID = @id FOR UPDATE", conexion, transaccion)
                    Parametro(comando, "@id", idDocumento)
                    Dim existente As Object = comando.ExecuteScalar()
                    If existente Is Nothing OrElse existente Is DBNull.Value Then Return 0
                End Using

                AuditarEnTransaccion(conexion, transaccion, "ALERTA INTEGRIDAD", "Documentos",
                    idDocumento, detalleSeguro)
                Return 1
            End Function)
    End Sub

    Public Function CasosCombo() As DataTable
        If Not PuedeCargarCombosDocumentos() Then Return New DataTable()
        Return Consultar(
            "SELECT c.ID, CONCAT(c.Numero_Expediente, ' - ', cl.Nombres, ' ', cl.Apellidos) AS Nombre " &
            "FROM Casos c INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "ORDER BY c.Numero_Expediente")
    End Function

    Public Function InstrumentosCombo(Optional idInstrumentoActual As Integer? = Nothing) As DataTable
        If Not PuedeCargarCombosDocumentos() Then Return New DataTable()
        Return Consultar(
            "SELECT i.ID, CONCAT('Libro ', l.Numero_Libro, ' - Escritura ', i.Numero_Escritura, " &
            "' (', i.Estado, ')') AS Nombre " &
            "FROM Instrumentos i INNER JOIN Libros_Protocolo l ON l.ID = i.ID_Libro " &
            "WHERE i.Estado = 'Vigente' OR i.ID = @idActual " &
            "ORDER BY l.Anio_Vigencia DESC, l.Numero_Libro, i.Numero_Escritura",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@idActual", If(idInstrumentoActual.HasValue, CObj(idInstrumentoActual.Value), DBNull.Value))
            End Sub)
    End Function

End Class
