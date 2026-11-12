Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de los documentos del expediente y su vínculo con
''' casos e instrumentos notariales.
''' </summary>
Public Class DocumentoRepositorio
    Inherits RepositorioBase

    Public Function Buscar(filtro As String, tipo As String) As DataTable
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
        Return Consultar(
            "SELECT * FROM Documentos WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function Insertar(documento As Documento) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Documentos (Tipo_Documento, Ruta_Archivo, Hash, Estado_Procesamiento_IA, " &
                    "       ID_Caso, ID_Instrumento, Usuario) " &
                    "VALUES (@tipo, @ruta, @hash, @estado, @caso, @instrumento, @usuario)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@tipo", documento.TipoDocumento)
                        Parametro(comando, "@ruta", documento.RutaArchivo)
                        Parametro(comando, "@hash", documento.Hash)
                        Parametro(comando, "@estado", documento.EstadoProcesamientoIA)
                        Parametro(comando, "@caso", If(documento.IDCaso.HasValue, CObj(documento.IDCaso.Value), DBNull.Value))
                        Parametro(comando, "@instrumento", If(documento.IDInstrumento.HasValue, CObj(documento.IDInstrumento.Value), DBNull.Value))
                        Parametro(comando, "@usuario", Sesion.NombreUsuario)
                    End Sub)

                AuditarEnTransaccion(conexion, transaccion, "IMPORTAR DOCUMENTO", "Documentos", nuevoId,
                    documento.TipoDocumento & " | SHA-256 " & documento.Hash)
                Return nuevoId
            End Function)
    End Function

    Public Sub ActualizarMetadatos(documento As Documento)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
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
                End If
                Return filas
            End Function)
    End Sub

    Public Function CasosCombo() As DataTable
        Return Consultar(
            "SELECT c.ID, CONCAT(c.Numero_Expediente, ' - ', cl.Nombres, ' ', cl.Apellidos) AS Nombre " &
            "FROM Casos c INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "ORDER BY c.Numero_Expediente")
    End Function

    Public Function InstrumentosCombo() As DataTable
        Return Consultar(
            "SELECT i.ID, CONCAT('Libro ', l.Numero_Libro, ' - Escritura ', i.Numero_Escritura) AS Nombre " &
            "FROM Instrumentos i INNER JOIN Libros_Protocolo l ON l.ID = i.ID_Libro " &
            "WHERE i.Estado = 'Vigente' ORDER BY l.Anio_Vigencia DESC, l.Numero_Libro, i.Numero_Escritura")
    End Function

End Class
