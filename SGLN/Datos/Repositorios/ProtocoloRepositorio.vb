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
                Dim numeroActual As Integer
                Dim anioActual As Integer
                Dim autorizacionActual As Date
                Dim vencimientoActual As Date
                Dim estadoActual As String = Nothing

                ' Bloquear primero el libro: los registros de instrumentos
                ' adquieren este mismo bloqueo antes de validar y reservar folios.
                Using comandoLibro As New MySqlCommand(
                    "SELECT Numero_Libro, Anio_Vigencia, Fecha_Autorizacion, Fecha_Vencimiento, Estado " &
                    "FROM Libros_Protocolo WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(comandoLibro, "@id", libro.ID)
                    Using lector As MySqlDataReader = comandoLibro.ExecuteReader()
                        If lector.Read() Then
                            numeroActual = Convert.ToInt32(lector("Numero_Libro"))
                            anioActual = Convert.ToInt32(lector("Anio_Vigencia"))
                            autorizacionActual = Convert.ToDateTime(lector("Fecha_Autorizacion")).Date
                            vencimientoActual = Convert.ToDateTime(lector("Fecha_Vencimiento")).Date
                            estadoActual = Convert.ToString(lector("Estado"))
                        End If
                    End Using
                End Using

                If estadoActual <> "Vigente" Then
                    Throw New InvalidOperationException("El libro ya no existe o dejó de estar vigente.")
                End If

                Dim instrumentos As Integer
                Using comandoConteo As New MySqlCommand(
                    "SELECT COUNT(*) FROM Instrumentos WHERE ID_Libro = @id",
                    conexion, transaccion)
                    Parametro(comandoConteo, "@id", libro.ID)
                    instrumentos = Convert.ToInt32(comandoConteo.ExecuteScalar())
                End Using

                If instrumentos > 0 AndAlso
                   (libro.NumeroLibro <> numeroActual OrElse
                    libro.AnioVigencia <> anioActual OrElse
                    libro.FechaAutorizacion.Date <> autorizacionActual OrElse
                    libro.FechaVencimiento.Date <> vencimientoActual) Then
                    Throw New InvalidOperationException(
                        "No se puede cambiar el número, año ni vigencia de un libro que ya contiene escrituras, incluso si alguna fue anulada.")
                End If

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

    Public Function CerrarLibro(id As Integer, razon As String, fechaCierre As Date) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim estado As String = Nothing
                Using consultar As New MySqlCommand(
                    "SELECT Estado FROM Libros_Protocolo WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultar, "@id", id)
                    Dim valor As Object = consultar.ExecuteScalar()
                    If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then estado = Convert.ToString(valor)
                End Using

                If estado <> "Vigente" Then Return 0

                ' El bloqueo del libro serializa el cierre con las altas y bajas
                ' de instrumentos/comparecientes que también bloquean esta fila.
                Dim pendientes As Integer
                Using consultarPendientes As New MySqlCommand(
                    "SELECT COUNT(*) FROM Instrumentos i " &
                    "WHERE i.ID_Libro = @id AND i.Estado = 'Vigente' " &
                    "AND NOT EXISTS (SELECT 1 FROM Comparecientes_Instrumento c " &
                    "                WHERE c.ID_Instrumento = i.ID AND c.Calidad = 'Otorgante')",
                    conexion, transaccion)
                    Parametro(consultarPendientes, "@id", id)
                    pendientes = Convert.ToInt32(consultarPendientes.ExecuteScalar())
                End Using
                If pendientes > 0 Then Return -1

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
    End Function

    Public Function RegistrarEntrega(id As Integer, fechaEntrega As Date) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Libros_Protocolo SET Fecha_Entrega_Notariado = @fecha, Estado = 'Entregado' " &
                    "WHERE ID = @id AND Estado = 'Cerrado' AND Fecha_Entrega_Notariado IS NULL " &
                    "AND Fecha_Cierre IS NOT NULL AND Fecha_Cierre <= @fecha",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@fecha", fechaEntrega)
                        Parametro(comando, "@id", id)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ENTREGAR", "Libros_Protocolo", id,
                    "Entrega al Notariado el " & fechaEntrega.ToString("dd/MM/yyyy"))
                Return filas
            End Function)
    End Function

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

    Public Function AnularInstrumento(id As Integer) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim idLibro As Integer = 0
                Using consultarLibroId As New MySqlCommand(
                    "SELECT ID_Libro FROM Instrumentos WHERE ID = @id",
                    conexion, transaccion)
                    Parametro(consultarLibroId, "@id", id)
                    Dim valor As Object = consultarLibroId.ExecuteScalar()
                    If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then
                        idLibro = Convert.ToInt32(valor)
                    End If
                End Using
                If idLibro <= 0 Then Return 0

                ' Mantener el orden de bloqueo libro -> instrumento. Se permite
                ' anular mientras el libro está vigente o cerrado, pero nunca
                ' después de registrar su entrega al Notariado.
                Dim estadoLibro As String = Nothing
                Dim fechaEntregaRegistrada As Boolean = False
                Using consultarLibro As New MySqlCommand(
                    "SELECT Estado, Fecha_Entrega_Notariado FROM Libros_Protocolo WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultarLibro, "@id", idLibro)
                    Using lector As MySqlDataReader = consultarLibro.ExecuteReader()
                        If lector.Read() Then
                            estadoLibro = Convert.ToString(lector("Estado"))
                            fechaEntregaRegistrada = Not lector.IsDBNull(lector.GetOrdinal("Fecha_Entrega_Notariado"))
                        End If
                    End Using
                End Using
                If (estadoLibro <> "Vigente" AndAlso estadoLibro <> "Cerrado") OrElse fechaEntregaRegistrada Then
                    Return 0
                End If

                Dim estadoInstrumento As String = Nothing
                Using consultarInstrumento As New MySqlCommand(
                    "SELECT Estado FROM Instrumentos WHERE ID = @id AND ID_Libro = @libro FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultarInstrumento, "@id", id)
                    Parametro(consultarInstrumento, "@libro", idLibro)
                    Dim valor As Object = consultarInstrumento.ExecuteScalar()
                    If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then
                        estadoInstrumento = Convert.ToString(valor)
                    End If
                End Using
                If estadoInstrumento <> "Vigente" Then Return 0

                If TieneTestimonios(conexion, transaccion, id) Then Return 0

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Instrumentos SET Estado = 'Anulado' WHERE ID = @id AND ID_Libro = @libro AND Estado = 'Vigente'",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                        Parametro(comando, "@libro", idLibro)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "ANULAR", "Instrumentos", id, "Instrumento anulado")
                Return filas
            End Function)
    End Function

    ''' <summary>
    ''' Devuelve todos los folios ocupados, incluidos los de escrituras anuladas:
    ''' una anulación conserva el asiento y nunca libera sus folios para reutilización.
    ''' </summary>
    Public Function FoliosOcupados(idLibro As Integer, idExcluir As Integer) As DataTable
        Return Consultar(
            "SELECT Numero_Escritura, Folio_Inicio, Folio_Fin FROM Instrumentos " &
            "WHERE ID_Libro = @libro AND ID <> @excluir",
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
                    Dim fechaAutorizacion As Date = Date.MinValue
                    Dim fechaVencimiento As Date = Date.MaxValue
                    Using comandoLibro As New MySqlCommand(
                        "SELECT Estado, Fecha_Autorizacion, Fecha_Vencimiento " &
                        "FROM Libros_Protocolo WHERE ID = @libro FOR UPDATE",
                        conexion, transaccion)
                        Parametro(comandoLibro, "@libro", instrumento.IDLibro)
                        Using lector As MySqlDataReader = comandoLibro.ExecuteReader()
                            If lector.Read() Then
                                estadoLibro = Convert.ToString(lector("Estado"))
                                fechaAutorizacion = Convert.ToDateTime(lector("Fecha_Autorizacion")).Date
                                fechaVencimiento = Convert.ToDateTime(lector("Fecha_Vencimiento")).Date
                            End If
                        End Using
                    End Using

                    If estadoLibro <> "Vigente" Then
                        motivo = "El libro no existe o ya no está vigente."
                        transaccion.Rollback()
                        Return False
                    End If
                    If Not String.Equals(instrumento.Estado, "Vigente", StringComparison.OrdinalIgnoreCase) Then
                        motivo = "El guardado solo admite escrituras en estado Vigente; use la acción de anulación."
                        transaccion.Rollback()
                        Return False
                    End If
                    If instrumento.FechaHoraOtorgamiento.Date < fechaAutorizacion OrElse
                       instrumento.FechaHoraOtorgamiento.Date > fechaVencimiento Then
                        motivo = "La fecha de otorgamiento debe estar dentro de la vigencia autorizada del libro."
                        transaccion.Rollback()
                        Return False
                    End If

                    If instrumento.ID > 0 Then
                        Dim estadoExistente As String = Nothing
                        Using comandoExiste As New MySqlCommand(
                            "SELECT Estado FROM Instrumentos WHERE ID = @id AND ID_Libro = @libro FOR UPDATE",
                            conexion, transaccion)
                            Parametro(comandoExiste, "@id", instrumento.ID)
                            Parametro(comandoExiste, "@libro", instrumento.IDLibro)
                            Dim valor As Object = comandoExiste.ExecuteScalar()
                            If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then estadoExistente = Convert.ToString(valor)
                        End Using
                        If estadoExistente <> "Vigente" Then
                            motivo = "El instrumento no existe en el libro seleccionado o ya fue anulado."
                            transaccion.Rollback()
                            Return False
                        End If
                        If TieneTestimonios(conexion, transaccion, instrumento.ID) Then
                            motivo = "No se puede modificar una escritura después de expedir testimonios; el historial debe conservarse."
                            transaccion.Rollback()
                            Return False
                        End If
                    End If

                    If String.Equals(instrumento.Estado, "Vigente", StringComparison.OrdinalIgnoreCase) Then
                        Dim traslapes As Integer
                        Using comandoTraslape As New MySqlCommand(
                            "SELECT COUNT(*) FROM Instrumentos " &
                            "WHERE ID_Libro = @libro AND ID <> @excluir " &
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
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim motivo As String = ""
                If Not BloquearInstrumentoVigente(conexion, transaccion, idInstrumento, motivo) Then
                    Throw New InvalidOperationException(motivo)
                End If
                If TieneTestimonios(conexion, transaccion, idInstrumento) Then
                    Throw New InvalidOperationException(
                        "No se pueden agregar comparecientes después de expedir testimonios.")
                End If

                Ejecutar(conexion, transaccion,
                    "INSERT INTO Comparecientes_Instrumento (ID_Instrumento, ID_Cliente, Calidad) VALUES (@instrumento, @cliente, @calidad)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@instrumento", idInstrumento)
                        Parametro(comando, "@cliente", idCliente)
                        Parametro(comando, "@calidad", calidad)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "AGREGAR COMPARECIENTE", "Comparecientes_Instrumento",
                    idInstrumento, "Cliente " & idCliente & " como " & calidad)
                Return 1
            End Function)
    End Sub

    Public Function QuitarCompareciente(id As Integer) As Integer
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim idInstrumento As Integer = 0
                Using consultar As New MySqlCommand(
                    "SELECT ID_Instrumento FROM Comparecientes_Instrumento WHERE ID = @id",
                    conexion, transaccion)
                    Parametro(consultar, "@id", id)
                    Dim valor As Object = consultar.ExecuteScalar()
                    If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then idInstrumento = Convert.ToInt32(valor)
                End Using
                If idInstrumento <= 0 Then Return 0

                Dim motivo As String = ""
                If Not BloquearInstrumentoVigente(conexion, transaccion, idInstrumento, motivo) Then
                    Throw New InvalidOperationException(motivo)
                End If
                If TieneTestimonios(conexion, transaccion, idInstrumento) Then
                    Throw New InvalidOperationException(
                        "No se pueden quitar comparecientes después de expedir testimonios.")
                End If

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "DELETE FROM Comparecientes_Instrumento WHERE ID = @id AND ID_Instrumento = @instrumento",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                        Parametro(comando, "@instrumento", idInstrumento)
                    End Sub)
                If filas > 0 Then AuditarEnTransaccion(conexion, transaccion, "QUITAR COMPARECIENTE",
                    "Comparecientes_Instrumento", id, "Compareciente eliminado")
                Return filas
            End Function)
    End Function

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
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim fechaOtorgamiento As Date
                Dim estadoInstrumento As String = Nothing
                Using consultar As New MySqlCommand(
                    "SELECT Estado, Fecha_Hora_Otorgamiento FROM Instrumentos WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultar, "@id", idInstrumento)
                    Using lector As MySqlDataReader = consultar.ExecuteReader()
                        If Not lector.Read() Then Throw New InvalidOperationException("La escritura no existe.")
                        estadoInstrumento = Convert.ToString(lector("Estado"))
                        fechaOtorgamiento = Convert.ToDateTime(lector("Fecha_Hora_Otorgamiento")).Date
                    End Using
                End Using
                If estadoInstrumento <> "Vigente" Then
                    Throw New InvalidOperationException("No se pueden expedir testimonios de una escritura anulada.")
                End If
                If fecha.Date < fechaOtorgamiento Then
                    Throw New InvalidOperationException("La fecha del testimonio no puede ser anterior al otorgamiento.")
                End If

                Dim primeros As Integer
                Using consultarPrimer As New MySqlCommand(
                    "SELECT COUNT(*) FROM Testimonios_Expedidos " &
                    "WHERE ID_Instrumento = @id AND Tipo_Testimonio = 'Primer'",
                    conexion, transaccion)
                    Parametro(consultarPrimer, "@id", idInstrumento)
                    primeros = Convert.ToInt32(consultarPrimer.ExecuteScalar())
                End Using
                If tipo = "Primer" AndAlso primeros > 0 Then
                    Throw New InvalidOperationException("Ya existe un primer testimonio expedido para esta escritura.")
                End If
                If tipo = "Ulterior" AndAlso primeros = 0 Then
                    Throw New InvalidOperationException("No se puede expedir un testimonio ulterior sin un primer testimonio registrado.")
                End If

                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Testimonios_Expedidos (ID_Instrumento, Tipo_Testimonio, Fecha_Expedicion, Destinatario, Usuario) " &
                    "VALUES (@instrumento, @tipo, @fecha, @destinatario, @usuario)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@instrumento", idInstrumento)
                        Parametro(comando, "@tipo", tipo)
                        Parametro(comando, "@fecha", fecha)
                        Parametro(comando, "@destinatario", destinatario)
                        Parametro(comando, "@usuario", Sesion.NombreUsuario)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "EXPEDIR TESTIMONIO", "Testimonios_Expedidos",
                    nuevoId, tipo & " testimonio para " & destinatario)
                Return nuevoId
            End Function)
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

    ''' <summary>
    ''' Bloquea primero el libro y después la escritura para impedir cambios
    ''' concurrentes de comparecientes mientras se cierra el protocolo.
    ''' </summary>
    Private Function TieneTestimonios(conexion As MySqlConnection,
                                         transaccion As MySqlTransaction,
                                         idInstrumento As Integer) As Boolean
        Using comando As New MySqlCommand(
            "SELECT COUNT(*) FROM Testimonios_Expedidos WHERE ID_Instrumento = @id",
            conexion, transaccion)
            Parametro(comando, "@id", idInstrumento)
            Return Convert.ToInt32(comando.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function BloquearInstrumentoVigente(conexion As MySqlConnection,
                                                transaccion As MySqlTransaction,
                                                idInstrumento As Integer,
                                                ByRef motivo As String) As Boolean
        Dim idLibro As Integer = 0
        Using consultar As New MySqlCommand(
            "SELECT ID_Libro FROM Instrumentos WHERE ID = @id",
            conexion, transaccion)
            Parametro(consultar, "@id", idInstrumento)
            Dim valor As Object = consultar.ExecuteScalar()
            If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then idLibro = Convert.ToInt32(valor)
        End Using
        If idLibro <= 0 Then
            motivo = "El instrumento no existe."
            Return False
        End If

        Dim estadoLibro As String = Nothing
        Using consultarLibro As New MySqlCommand(
            "SELECT Estado FROM Libros_Protocolo WHERE ID = @id FOR UPDATE",
            conexion, transaccion)
            Parametro(consultarLibro, "@id", idLibro)
            Dim valor As Object = consultarLibro.ExecuteScalar()
            If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then estadoLibro = Convert.ToString(valor)
        End Using
        If estadoLibro <> "Vigente" Then
            motivo = "No se pueden modificar comparecientes de un libro cerrado o entregado."
            Return False
        End If

        Dim estadoInstrumento As String = Nothing
        Using consultarInstrumento As New MySqlCommand(
            "SELECT Estado FROM Instrumentos WHERE ID = @id AND ID_Libro = @libro FOR UPDATE",
            conexion, transaccion)
            Parametro(consultarInstrumento, "@id", idInstrumento)
            Parametro(consultarInstrumento, "@libro", idLibro)
            Dim valor As Object = consultarInstrumento.ExecuteScalar()
            If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then estadoInstrumento = Convert.ToString(valor)
        End Using
        If estadoInstrumento <> "Vigente" Then
            motivo = "No se pueden modificar comparecientes de una escritura inexistente o anulada."
            Return False
        End If

        Return True
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
