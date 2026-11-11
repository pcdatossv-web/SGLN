Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos del protocolo notarial: libros, instrumentos,
''' comparecientes, testimonios e índice.
''' </summary>
Public Class ProtocoloRepositorio
    Inherits RepositorioBase

    ' ------------------------------------------------------------------
    ' Libros
    ' ------------------------------------------------------------------

    Public Function Libros() As DataTable
        Return Consultar(
            "SELECT ID, Numero_Libro, Anio_Vigencia, Fecha_Autorizacion, Fecha_Vencimiento, " &
            "       Estado, Fecha_Cierre, Fecha_Entrega_Notariado, Indice_Generado, Razon_Cierre " &
            "FROM Libros_Protocolo ORDER BY Anio_Vigencia DESC, Numero_Libro DESC")
    End Function

    Public Function ObtenerLibro(id As Integer) As DataTable
        Return Consultar(
            "SELECT * FROM Libros_Protocolo WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function InsertarLibro(libro As LibroProtocolo) As Integer
        Dim nuevoId As Integer = InsertarYObtenerId(
            "INSERT INTO Libros_Protocolo (Numero_Libro, Anio_Vigencia, Fecha_Autorizacion, " &
            "       Fecha_Vencimiento, Estado) " &
            "VALUES (@numero, @anio, @autorizacion, @vencimiento, @estado)",
            Sub(comando As MySqlCommand)
                AgregarParametrosLibro(comando, libro)
            End Sub)

        Auditar("CREAR", "Libros_Protocolo", nuevoId,
                "Libro " & libro.NumeroLibro & " del año " & libro.AnioVigencia)
        Return nuevoId
    End Function

    Public Sub ActualizarLibro(libro As LibroProtocolo)
        Dim filas As Integer = Ejecutar(
            "UPDATE Libros_Protocolo SET Numero_Libro = @numero, Anio_Vigencia = @anio, " &
            "       Fecha_Autorizacion = @autorizacion, Fecha_Vencimiento = @vencimiento, " &
            "       Estado = @estado " &
            "WHERE ID = @id",
            Sub(comando As MySqlCommand)
                AgregarParametrosLibro(comando, libro)
                Parametro(comando, "@id", libro.ID)
            End Sub)

        If filas > 0 Then
            Auditar("MODIFICAR", "Libros_Protocolo", libro.ID,
                    "Libro " & libro.NumeroLibro & " del año " & libro.AnioVigencia)
        End If
    End Sub

    Public Sub CerrarLibro(id As Integer, razon As String, fechaCierre As Date)
        Dim filas As Integer = Ejecutar(
            "UPDATE Libros_Protocolo SET Estado = 'Cerrado', Razon_Cierre = @razon, " &
            "       Fecha_Cierre = @fecha " &
            "WHERE ID = @id AND Estado = 'Vigente'",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@razon", razon)
                Parametro(comando, "@fecha", fechaCierre)
                Parametro(comando, "@id", id)
            End Sub)

        If filas > 0 Then Auditar("CERRAR", "Libros_Protocolo", id, razon)
    End Sub

    Public Sub RegistrarEntrega(id As Integer, fechaEntrega As Date)
        Dim filas As Integer = Ejecutar(
            "UPDATE Libros_Protocolo SET Fecha_Entrega_Notariado = @fecha, Estado = 'Entregado' " &
            "WHERE ID = @id AND Estado = 'Cerrado'",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@fecha", fechaEntrega)
                Parametro(comando, "@id", id)
            End Sub)

        If filas > 0 Then Auditar("ENTREGAR", "Libros_Protocolo", id, "Entrega al Notariado el " & fechaEntrega.ToString("dd/MM/yyyy"))
    End Sub

    Public Function MarcarIndiceGenerado(idLibro As Integer) As Integer
        Dim filas As Integer = Ejecutar(
            "UPDATE Libros_Protocolo SET Indice_Generado = 1 WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idLibro)
            End Sub)

        If filas > 0 Then Auditar("GENERAR ÍNDICE", "Libros_Protocolo", idLibro, "Índice del libro marcado como generado")
        Return filas
    End Function

    ' ------------------------------------------------------------------
    ' Instrumentos
    ' ------------------------------------------------------------------

    Public Function Instrumentos(idLibro As Integer) As DataTable
        Return Consultar(
            "SELECT i.ID, i.Numero_Escritura, i.Fecha_Hora_Otorgamiento, i.Folio_Inicio, i.Folio_Fin, " &
            "       i.Objeto_Acto, i.Estado, " &
            "       (SELECT COUNT(*) FROM Comparecientes_Instrumento c WHERE c.ID_Instrumento = i.ID) AS Comparecientes " &
            "FROM Instrumentos i WHERE i.ID_Libro = @libro " &
            "ORDER BY i.Numero_Escritura",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@libro", idLibro)
            End Sub)
    End Function

    Public Function ObtenerInstrumento(id As Integer) As DataTable
        Return Consultar(
            "SELECT * FROM Instrumentos WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function InsertarInstrumento(instrumento As Instrumento) As Integer
        Dim nuevoId As Integer = InsertarYObtenerId(
            "INSERT INTO Instrumentos (ID_Libro, Numero_Escritura, Fecha_Hora_Otorgamiento, " &
            "       Folio_Inicio, Folio_Fin, Objeto_Acto, Estado) " &
            "VALUES (@libro, @numero, @fecha, @inicio, @fin, @objeto, @estado)",
            Sub(comando As MySqlCommand)
                AgregarParametrosInstrumento(comando, instrumento)
            End Sub)

        Auditar("CREAR", "Instrumentos", nuevoId, "Escritura " & instrumento.NumeroEscritura)
        Return nuevoId
    End Function

    Public Sub ActualizarInstrumento(instrumento As Instrumento)
        Dim filas As Integer = Ejecutar(
            "UPDATE Instrumentos SET Numero_Escritura = @numero, Fecha_Hora_Otorgamiento = @fecha, " &
            "       Folio_Inicio = @inicio, Folio_Fin = @fin, Objeto_Acto = @objeto, Estado = @estado " &
            "WHERE ID = @id",
            Sub(comando As MySqlCommand)
                AgregarParametrosInstrumento(comando, instrumento)
                Parametro(comando, "@id", instrumento.ID)
            End Sub)

        If filas > 0 Then Auditar("MODIFICAR", "Instrumentos", instrumento.ID, "Escritura " & instrumento.NumeroEscritura)
    End Sub

    Public Sub AnularInstrumento(id As Integer)
        Dim filas As Integer = Ejecutar(
            "UPDATE Instrumentos SET Estado = 'Anulado' WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)

        If filas > 0 Then Auditar("ANULAR", "Instrumentos", id, "Instrumento anulado")
    End Sub

    ''' <summary>
    ''' Devuelve los folios ya ocupados del libro para validar traslapes.
    ''' </summary>
    Public Function FoliosOcupados(idLibro As Integer, idExcluir As Integer) As DataTable
        Return Consultar(
            "SELECT Numero_Escritura, Folio_Inicio, Folio_Fin FROM Instrumentos " &
            "WHERE ID_Libro = @libro AND ID <> @excluir AND Estado = 'Vigente'",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@libro", idLibro)
                Parametro(comando, "@excluir", idExcluir)
            End Sub)
    End Function

    Public Function InstrumentosSinOtorgante(idLibro As Integer) As Integer
        Return Escalar(Of Integer)(
            "SELECT COUNT(*) FROM Instrumentos i " &
            "WHERE i.ID_Libro = @libro AND i.Estado = 'Vigente' " &
            "  AND NOT EXISTS (SELECT 1 FROM Comparecientes_Instrumento c " &
            "                  WHERE c.ID_Instrumento = i.ID AND c.Calidad = 'Otorgante')",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@libro", idLibro)
            End Sub)
    End Function

    ' ------------------------------------------------------------------
    ' Comparecientes
    ' ------------------------------------------------------------------

    Public Function Comparecientes(idInstrumento As Integer) As DataTable
        Return Consultar(
            "SELECT c.ID, c.ID_Cliente, CONCAT(cl.Nombres, ' ', cl.Apellidos) AS Cliente, " &
            "       IFNULL(cl.DUI, '') AS DUI, c.Calidad " &
            "FROM Comparecientes_Instrumento c " &
            "INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "WHERE c.ID_Instrumento = @instrumento " &
            "ORDER BY c.Calidad, cl.Apellidos, cl.Nombres",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@instrumento", idInstrumento)
            End Sub)
    End Function

    Public Sub AgregarCompareciente(idInstrumento As Integer, idCliente As Integer, calidad As String)
        Ejecutar(
            "INSERT INTO Comparecientes_Instrumento (ID_Instrumento, ID_Cliente, Calidad) " &
            "VALUES (@instrumento, @cliente, @calidad)",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@instrumento", idInstrumento)
                Parametro(comando, "@cliente", idCliente)
                Parametro(comando, "@calidad", calidad)
            End Sub)

        Auditar("AGREGAR COMPARECIENTE", "Comparecientes_Instrumento", idInstrumento,
                "Cliente " & idCliente & " como " & calidad)
    End Sub

    Public Sub QuitarCompareciente(id As Integer)
        Dim filas As Integer = Ejecutar(
            "DELETE FROM Comparecientes_Instrumento WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)

        If filas > 0 Then Auditar("QUITAR COMPARECIENTE", "Comparecientes_Instrumento", id, "Compareciente eliminado")
    End Sub

    ' ------------------------------------------------------------------
    ' Testimonios
    ' ------------------------------------------------------------------

    Public Function Testimonios(idLibro As Integer) As DataTable
        Return Consultar(
            "SELECT t.ID, i.Numero_Escritura, t.Tipo_Testimonio, t.Fecha_Expedicion, " &
            "       t.Destinatario, IFNULL(t.Usuario, '') AS Usuario " &
            "FROM Testimonios_Expedidos t " &
            "INNER JOIN Instrumentos i ON i.ID = t.ID_Instrumento " &
            "WHERE i.ID_Libro = @libro " &
            "ORDER BY t.Fecha_Expedicion DESC, t.ID DESC",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@libro", idLibro)
            End Sub)
    End Function

    Public Sub InsertarTestimonio(idInstrumento As Integer, tipo As String, fecha As Date, destinatario As String)
        Ejecutar(
            "INSERT INTO Testimonios_Expedidos (ID_Instrumento, Tipo_Testimonio, Fecha_Expedicion, Destinatario, Usuario) " &
            "VALUES (@instrumento, @tipo, @fecha, @destinatario, @usuario)",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@instrumento", idInstrumento)
                Parametro(comando, "@tipo", tipo)
                Parametro(comando, "@fecha", fecha)
                Parametro(comando, "@destinatario", destinatario)
                Parametro(comando, "@usuario", Sesion.NombreUsuario)
            End Sub)

        Auditar("EXPEDIR TESTIMONIO", "Testimonios_Expedidos", idInstrumento,
                tipo & " testimonio para " & destinatario)
    End Sub

    ' ------------------------------------------------------------------
    ' Índice
    ' ------------------------------------------------------------------

    Public Function IndiceLibro(idLibro As Integer) As DataTable
        ' GROUP_CONCAT reemplaza el STUFF/FOR XML de SQL Server: agrupa los
        ' otorgantes de cada escritura en una sola celda.
        Return Consultar(
            "SELECT i.Numero_Escritura, i.Fecha_Hora_Otorgamiento, i.Folio_Inicio, i.Folio_Fin, " &
            "       i.Objeto_Acto, i.Estado, " &
            "       (SELECT IFNULL(GROUP_CONCAT(CONCAT(cl.Nombres, ' ', cl.Apellidos) " &
            "                      ORDER BY cl.Apellidos SEPARATOR ', '), '') " &
            "          FROM Comparecientes_Instrumento c " &
            "          INNER JOIN Clientes cl ON cl.ID = c.ID_Cliente " &
            "         WHERE c.ID_Instrumento = i.ID AND c.Calidad = 'Otorgante') AS Otorgantes " &
            "FROM Instrumentos i WHERE i.ID_Libro = @libro ORDER BY i.Numero_Escritura",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@libro", idLibro)
            End Sub)
    End Function

    Public Function ClientesCombo() As DataTable
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos, IFNULL(CONCAT(' (', DUI, ')'), '')) AS Nombre " &
            "FROM Clientes WHERE Activo = 1 ORDER BY Apellidos, Nombres")
    End Function

    Private Sub AgregarParametrosLibro(comando As MySqlCommand, libro As LibroProtocolo)
        Parametro(comando, "@numero", libro.NumeroLibro)
        Parametro(comando, "@anio", libro.AnioVigencia)
        Parametro(comando, "@autorizacion", libro.FechaAutorizacion)
        Parametro(comando, "@vencimiento", libro.FechaVencimiento)
        Parametro(comando, "@estado", libro.Estado)
    End Sub

    Private Sub AgregarParametrosInstrumento(comando As MySqlCommand, instrumento As Instrumento)
        Parametro(comando, "@libro", instrumento.IDLibro)
        Parametro(comando, "@numero", instrumento.NumeroEscritura)
        Parametro(comando, "@fecha", instrumento.FechaHoraOtorgamiento)
        Parametro(comando, "@inicio", instrumento.FolioInicio)
        Parametro(comando, "@fin", instrumento.FolioFin)
        Parametro(comando, "@objeto", instrumento.ObjetoActo)
        Parametro(comando, "@estado", instrumento.Estado)
    End Sub

End Class
