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
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Libros_Protocolo (Numero_Libro, Anio_Vigencia, Fecha_Autorizacion, Fecha_Vencimiento, Estado) " &
                    "VALUES (@numero, @anio, @autorizacion, @vencimiento, @estado)",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosLibro(comando, libro)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Libros_Protocolo", nuevoId,
                    "Libro " & libro.NumeroLibro & " del año " & libro.AnioVigencia)
                Return nuevoId
            End Function)
    End Function

    Public Sub ActualizarLibro(libro As LibroProtocolo)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Libros_Protocolo SET Numero_Libro = @numero, Anio_Vigencia = @anio, " &
                    "Fecha_Autorizacion = @autorizacion, Fecha_Vencimiento = @vencimiento, Estado = @estado WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosLibro(comando, libro)
                        Parametro(comando, "@id", libro.ID)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Libros_Protocolo",
                    libro.ID, "Libro " & libro.NumeroLibro & " del año " & libro.AnioVigencia)
                Return filas
            End Function)
    End Sub

    Public Sub CerrarLibro(id As Integer, razon As String, fechaCierre As Date)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Libros_Protocolo SET Estado = 'Cerrado', Razon_Cierre = @razon, Fecha_Cierre = @fecha " &
                    "WHERE ID = @id AND Estado = 'Vigente'",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@razon", razon)
                        Parametro(comando, "@fecha", fechaCierre)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "CERRAR", "Libros_Protocolo", id, razon)
                Return filas
            End Function)
    End Sub

    Public Sub RegistrarEntrega(id As Integer, fechaEntrega As Date)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Libros_Protocolo SET Fecha_Entrega_Notariado = @fecha, Estado = 'Entregado' " &
                    "WHERE ID = @id AND Estado = 'Cerrado'",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@fecha", fechaEntrega)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ENTREGAR", "Libros_Protocolo", id,
                    "Entrega al Notariado el " & fechaEntrega.ToString("dd/MM/yyyy"))
                Return filas
            End Function)
    End Sub

    Public Function MarcarIndiceGenerado(idLibro As Integer) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Libros_Protocolo SET Indice_Generado = 1 WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", idLibro)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "GENERAR ÍNDICE", "Libros_Protocolo",
                    idLibro, "Índice del libro marcado como generado")
                Return filas
            End Function)
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
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Instrumentos (ID_Libro, Numero_Escritura, Fecha_Hora_Otorgamiento, Folio_Inicio, Folio_Fin, Objeto_Acto, Estado) " &
                    "VALUES (@libro, @numero, @fecha, @inicio, @fin, @objeto, @estado)",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosInstrumento(comando, instrumento)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Instrumentos", nuevoId,
                    "Escritura " & instrumento.NumeroEscritura)
                Return nuevoId
            End Function)
    End Function

    Public Sub ActualizarInstrumento(instrumento As Instrumento)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Instrumentos SET Numero_Escritura = @numero, Fecha_Hora_Otorgamiento = @fecha, " &
                    "Folio_Inicio = @inicio, Folio_Fin = @fin, Objeto_Acto = @objeto, Estado = @estado WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosInstrumento(comando, instrumento)
                        Parametro(comando, "@id", instrumento.ID)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Instrumentos",
                    instrumento.ID, "Escritura " & instrumento.NumeroEscritura)
                Return filas
            End Function)
    End Sub

    Public Sub AnularInstrumento(id As Integer)
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Instrumentos SET Estado = 'Anulado' WHERE ID = @id",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ANULAR", "Instrumentos", id, "Instrumento anulado")
                Return filas
            End Function)
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

    ''' <summary>
    ''' Guarda un instrumento serializando las escrituras por libro.
    ''' El bloqueo de la fila del libro evita que dos sesiones validen
    ''' simultáneamente el mismo intervalo de folios y ambas lo inserten.
    ''' Devuelve False si el libro no está vigente, el instrumento no existe
    ''' en ese libro o los folios se traslapan.
    ''' </summary>
    Public Function GuardarInstrumentoConBloqueo(instrumento As Instrumento,
                                                 ByRef idGuardado As Integer,
                                                 ByRef motivo As String) As Boolean
        idGuardado = 0
        motivo = ""

        Using conexion As MySqlConnection = CrearConexion()
            Using transaccion As MySqlTransaction = conexion.BeginTransaction(IsolationLevel.ReadCommitted)
                Try
                    ' Todos los guardados de instrumentos de un mismo libro
                    ' deben adquirir primero este bloqueo, en el mismo orden.
                    Dim estadoLibro As String = Nothing
                    Using comandoLibro As New MySqlCommand(
                        "SELECT Estado FROM Libros_Protocolo WHERE ID = @libro FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comandoLibro, "@libro", instrumento.IDLibro)
                        Dim valor As Object = comandoLibro.ExecuteScalar()
                        If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then
                            estadoLibro = Convert.ToString(valor)
                        End If
                    End Using

                    If estadoLibro <> "Vigente" Then
                        motivo = "El libro no existe o ya no está vigente."
                        transaccion.Rollback()
                        Return False
                    End If

                    If instrumento.ID > 0 Then
                        Dim existe As Integer
                        Using comandoExiste As New MySqlCommand(
                            "SELECT COUNT(*) FROM Instrumentos WHERE ID = @id AND ID_Libro = @libro",
                            conexion, transaccion)
                            Parametro(comandoExiste, "@id", instrumento.ID)
                            Parametro(comandoExiste, "@libro", instrumento.IDLibro)
                            existe = Convert.ToInt32(comandoExiste.ExecuteScalar())
                        End Using
                        If existe = 0 Then
                            motivo = "El instrumento no existe en el libro seleccionado."
                            transaccion.Rollback()
                            Return False
                        End If
                    End If

                    If String.Equals(instrumento.Estado, "Vigente", StringComparison.OrdinalIgnoreCase) Then
                        Dim traslapes As Integer
                        Using comandoTraslape As New MySqlCommand(
                            "SELECT COUNT(*) FROM Instrumentos " &
                            "WHERE ID_Libro = @libro AND ID <> @excluir AND Estado = 'Vigente' " &
                            "AND Folio_Inicio <= @fin AND Folio_Fin >= @inicio",
                            conexion, transaccion)
                            Parametro(comandoTraslape, "@libro", instrumento.IDLibro)
                            Parametro(comandoTraslape, "@excluir", instrumento.ID)
                            Parametro(comandoTraslape, "@inicio", instrumento.FolioInicio)
                            Parametro(comandoTraslape, "@fin", instrumento.FolioFin)
                            traslapes = Convert.ToInt32(comandoTraslape.ExecuteScalar())
                        End Using

                        If traslapes > 0 Then
                            motivo = "Los folios indicados se traslapan con otra escritura del mismo libro."
                            transaccion.Rollback()
                            Return False
                        End If
                    End If

                    If instrumento.ID = 0 Then
                        Using comandoInsertar As New MySqlCommand(
                            "INSERT INTO Instrumentos (ID_Libro, Numero_Escritura, Fecha_Hora_Otorgamiento, " &
                            "Folio_Inicio, Folio_Fin, Objeto_Acto, Estado) " &
                            "VALUES (@libro, @numero, @fecha, @inicio, @fin, @objeto, @estado)",
                            conexion, transaccion)
                            AgregarParametrosInstrumento(comandoInsertar, instrumento)
                            comandoInsertar.ExecuteNonQuery()
                        End Using
                        Using comandoId As New MySqlCommand("SELECT LAST_INSERT_ID()", conexion, transaccion)
                            idGuardado = Convert.ToInt32(comandoId.ExecuteScalar())
                        End Using
                    Else
                        Using comandoActualizar As New MySqlCommand(
                            "UPDATE Instrumentos SET Numero_Escritura = @numero, Fecha_Hora_Otorgamiento = @fecha, " &
                            "Folio_Inicio = @inicio, Folio_Fin = @fin, Objeto_Acto = @objeto, Estado = @estado " &
                            "WHERE ID = @id AND ID_Libro = @libro",
                            conexion, transaccion)
                            AgregarParametrosInstrumento(comandoActualizar, instrumento)
                            Parametro(comandoActualizar, "@id", instrumento.ID)
                            comandoActualizar.ExecuteNonQuery()
                        End Using
                        idGuardado = instrumento.ID
                    End If

                    AuditarEnTransaccion(conexion, transaccion,
                        If(instrumento.ID = 0, "CREAR", "MODIFICAR"), "Instrumentos", idGuardado,
                        "Escritura " & instrumento.NumeroEscritura)
                    transaccion.Commit()
                Catch
                    Try
                        transaccion.Rollback()
                    Catch
                        ' Se conserva la excepción original si el rollback también falla.
                    End Try
                    Throw
                End Try
            End Using
        End Using

        Return True
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
