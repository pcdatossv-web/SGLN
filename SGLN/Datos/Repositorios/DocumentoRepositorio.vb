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
        Return Permisos.TieneAccesoAlguno("documentos.ver", "documentos.verificar",
                                 "documentos.metadatos", "documentos.importar")
    End Function

    Private Function PuedeCargarCombosDocumentos() As Boolean
        Return Permisos.TieneAccesoAlguno("documentos.ver", "documentos.importar",
                                 "documentos.metadatos")
    End Function

    Public Function Buscar(filtro As String, tipo As String) As DataTable
        If Not Permisos.TieneAccesoAlguno("documentos.ver", "documentos.verificar", "documentos.metadatos") Then Return New DataTable()
        Dim texto As String = If(filtro, "").Trim()
        Dim tipoFiltro As String = If(tipo, "").Trim()

        Return Consultar(
            "SELECT d.ID, d.Tipo_Documento, d.Ruta_Archivo, d.Hash, d.Estado_Procesamiento_IA, " &
            "       d.Fecha_Carga, IFNULL(d.Usuario, '') AS Usuario, " &
            "       IFNULL(c.Numero_Expediente, '') AS Expediente, " &
            "       IFNULL(i.Numero_Escritura, 0) AS Escritura, " &
            "       d.ID_Caso, d.ID_Instrumento " &
            "FROM Documentos d " &
            "LEFT JOIN Casos c ON c.ID = d.ID_Caso " &
            "LEFT JOIN Instrumentos i ON i.ID = d.ID_Instrumento " &
            "WHERE (@filtro IS NULL OR d.Tipo_Documento LIKE @like OR d.Ruta_Archivo LIKE @like " &
            "       OR d.Hash LIKE @like OR IFNULL(c.Numero_Expediente, '') LIKE @like) " &
            "  AND (@tipo IS NULL OR d.Tipo_Documento = @tipo) " &
            "ORDER BY d.Fecha_Carga DESC, d.ID DESC",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
                Parametro(comando, "@tipo", tipoFiltro)
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
                ' Bloquear primero la fila para distinguir un documento inexistente
                ' de una edición válida cuyos metadatos ya tenían esos valores.
                Using comandoExiste As New MySqlCommand(
                    "SELECT ID FROM Documentos WHERE ID = @id FOR UPDATE", conexion, transaccion)
                    Parametro(comandoExiste, "@id", documento.ID)
                    Dim idExistente As Object = comandoExiste.ExecuteScalar()
                    If idExistente Is Nothing OrElse idExistente Is DBNull.Value Then Return 0
                End Using

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
                    AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Documentos", documento.ID,
                        "Metadatos del documento actualizados")
                    Return filas
                End If

                ' La fila está bloqueada y existe; no registrar una modificación
                ' cuando el usuario simplemente vuelve a guardar los mismos datos.
                Return 1
            End Function)
    End Function

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
