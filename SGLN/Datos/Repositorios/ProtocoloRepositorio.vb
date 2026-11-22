Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos del protocolo notarial: libros, instrumentos,
''' comparecientes, testimonios e índice.
''' </summary>
Public Class ProtocoloRepositorio
    Inherits RepositorioBase

    ''' <summary>
    ''' Revalida la sesión una vez y consulta los permisos desde la matriz vigente.
    ''' </summary>
    ' ------------------------------------------------------------------
    ' Libros
    ' ------------------------------------------------------------------

    Public Function Libros() As DataTable
        If Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar", "protocolo.cerrar_libro", "protocolo.entregar", "protocolo.testimonios") Then Return New DataTable()
        Return Consultar(
            "SELECT ID, Numero_Libro, Anio_Vigencia, Fecha_Autorizacion, Fecha_Vencimiento, " &
            "       Estado, Fecha_Cierre, Fecha_Entrega_Notariado, Indice_Generado, Razon_Cierre " &
            "FROM Libros_Protocolo ORDER BY Anio_Vigencia DESC, Numero_Libro DESC")
    End Function

    Public Function ObtenerLibro(id As Integer) As DataTable
        If id <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar",
                                                 "protocolo.cerrar_libro", "protocolo.entregar",
                                                 "protocolo.testimonios") Then Return New DataTable()
        Return Consultar(
            "SELECT * FROM Libros_Protocolo WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function InsertarLibro(libro As LibroProtocolo) As Integer
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If libro Is Nothing Then Throw New ArgumentNullException(NameOf(libro))
        If libro.ID <> 0 Then Throw New ArgumentException("Para insertar un libro nuevo, el identificador debe ser cero.", NameOf(libro))
        ValidarDatosLibro(libro)
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim nuevoId As Integer = InsertarYObtenerId(conexion, transaccion,
                    "INSERT INTO Libros_Protocolo (Numero_Libro, Anio_Vigencia, Fecha_Autorizacion, Fecha_Vencimiento, Estado) " &
                    "VALUES (@numero, @anio, @autorizacion, @vencimiento, 'Vigente')",
                    Sub(comando As MySqlCommand)
                        AgregarParametrosLibro(comando, libro)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "CREAR", "Libros_Protocolo", nuevoId,
                    "Libro " & libro.NumeroLibro & " del año " & libro.AnioVigencia)
                Return nuevoId
            End Function)
    End Function

    Public Sub ActualizarLibro(libro As LibroProtocolo)
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If libro Is Nothing OrElse libro.ID <= 0 Then Throw New ArgumentException("El libro seleccionado no es válido.", NameOf(libro))
        ValidarDatosLibro(libro)
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
                    "Fecha_Autorizacion = @autorizacion, Fecha_Vencimiento = @vencimiento WHERE ID = @id",
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
        If Not Permisos.TieneAcceso("protocolo.cerrar_libro") Then Throw New UnauthorizedAccessException("No tiene permiso para cerrar libros del protocolo.")
        If id <= 0 Then Return 0
        If String.IsNullOrWhiteSpace(razon) OrElse razon.Trim().Length > 1000 Then
            Throw New ArgumentException("La razón del cierre debe contener entre 1 y 1000 caracteres.", NameOf(razon))
        End If
        If fechaCierre = Date.MinValue OrElse fechaCierre.Date.Year < 1000 OrElse fechaCierre.Date > Date.Today Then
            Throw New ArgumentException("La fecha de cierre no puede estar vacía ni ser futura.", NameOf(fechaCierre))
        End If
        razon = razon.Trim()
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim estado As String = Nothing
                Dim fechaAutorizacion As Date = Date.MinValue
                Using consultar As New MySqlCommand(
                    "SELECT Estado, Fecha_Autorizacion FROM Libros_Protocolo WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultar, "@id", id)
                    Using lector As MySqlDataReader = consultar.ExecuteReader()
                        If lector.Read() Then
                            estado = Convert.ToString(lector("Estado"))
                            If Not lector.IsDBNull(lector.GetOrdinal("Fecha_Autorizacion")) Then
                                fechaAutorizacion = Convert.ToDateTime(lector("Fecha_Autorizacion")).Date
                            End If
                        End If
                    End Using
                End Using

                If estado <> "Vigente" Then Return 0
                If fechaAutorizacion = Date.MinValue OrElse fechaCierre.Date < fechaAutorizacion Then
                    Throw New ArgumentException("La fecha de cierre no puede ser anterior a la autorización del libro.", NameOf(fechaCierre))
                End If

                Dim ultimaEscritura As Object
                Using consultarFecha As New MySqlCommand(
                    "SELECT MAX(DATE(Fecha_Hora_Otorgamiento)) FROM Instrumentos WHERE ID_Libro = @id",
                    conexion, transaccion)
                    Parametro(consultarFecha, "@id", id)
                    ultimaEscritura = consultarFecha.ExecuteScalar()
                End Using
                If ultimaEscritura IsNot Nothing AndAlso Not Convert.IsDBNull(ultimaEscritura) AndAlso
                   fechaCierre.Date < Convert.ToDateTime(ultimaEscritura).Date Then
                    Throw New ArgumentException("La fecha de cierre no puede ser anterior a la última escritura vigente.", NameOf(fechaCierre))
                End If

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
        If Not Permisos.TieneAcceso("protocolo.entregar") Then Throw New UnauthorizedAccessException("No tiene permiso para registrar entregas al Notariado.")
        If id <= 0 Then Return 0
        If fechaEntrega = Date.MinValue OrElse fechaEntrega.Date.Year < 1000 OrElse fechaEntrega.Date > Date.Today Then
            Throw New ArgumentException("La fecha de entrega no puede estar vacía ni ser futura.", NameOf(fechaEntrega))
        End If
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
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If idLibro <= 0 Then Return 0
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                ' Validar dentro de la misma transacción: no permitir que una
                ' llamada directa al repositorio marque un índice vacío o abierto.
                Dim estado As String = Nothing
                Dim indiceGenerado As Boolean = False
                Using consultarLibro As New MySqlCommand(
                    "SELECT Estado, Indice_Generado FROM Libros_Protocolo WHERE ID = @id FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultarLibro, "@id", idLibro)
                    Using lector As MySqlDataReader = consultarLibro.ExecuteReader()
                        If Not lector.Read() Then Return 0
                        estado = Convert.ToString(lector("Estado"))
                        indiceGenerado = Convert.ToBoolean(lector("Indice_Generado"))
                    End Using
                End Using

                If (estado <> "Cerrado" AndAlso estado <> "Entregado") OrElse indiceGenerado Then Return 0

                Dim cantidadInstrumentos As Integer
                Using consultarInstrumentos As New MySqlCommand(
                    "SELECT COUNT(*) FROM Instrumentos WHERE ID_Libro = @id",
                    conexion, transaccion)
                    Parametro(consultarInstrumentos, "@id", idLibro)
                    cantidadInstrumentos = Convert.ToInt32(consultarInstrumentos.ExecuteScalar())
                End Using
                If cantidadInstrumentos = 0 Then Return 0

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "UPDATE Libros_Protocolo SET Indice_Generado = 1 " &
                    "WHERE ID = @id AND Estado IN ('Cerrado', 'Entregado') AND Indice_Generado = 0",
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
        If idLibro <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar", "protocolo.cerrar_libro", "protocolo.entregar", "protocolo.testimonios") Then Return New DataTable()
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

    ''' <summary>
    ''' Comprueba una referencia notarial al vincular documentos sin exponer
    ''' los datos del instrumento a usuarios sin permiso de consulta del protocolo.
    ''' </summary>
    Public Function ExisteInstrumentoParaVinculoDocumento(id As Integer) As Boolean
        If id <= 0 OrElse Not Permisos.TieneAccesoAlguno("documentos.importar", "documentos.metadatos") Then Return False
        Return Consultar("SELECT ID FROM Instrumentos WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub).Rows.Count > 0
    End Function

    Public Function ObtenerInstrumento(id As Integer) As DataTable
        If id <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar",
                                                 "protocolo.testimonios") Then Return New DataTable()
        Return Consultar(
            "SELECT * FROM Instrumentos WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function InsertarInstrumento(instrumento As Instrumento) As Integer
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If instrumento Is Nothing Then Throw New ArgumentNullException(NameOf(instrumento))
        If instrumento.ID <> 0 Then Throw New ArgumentException("Para insertar una escritura nueva, el identificador debe ser cero.", NameOf(instrumento))

        Dim idGuardado As Integer = 0
        Dim motivo As String = ""
        If Not GuardarInstrumentoConBloqueo(instrumento, idGuardado, motivo) Then
            Throw New InvalidOperationException(If(String.IsNullOrWhiteSpace(motivo),
                "No fue posible registrar la escritura.", motivo))
        End If
        instrumento.ID = idGuardado
        Return idGuardado
    End Function

    Public Sub ActualizarInstrumento(instrumento As Instrumento)
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If instrumento Is Nothing OrElse instrumento.ID <= 0 Then Throw New ArgumentException("El instrumento seleccionado no es válido.", NameOf(instrumento))

        Dim idGuardado As Integer = 0
        Dim motivo As String = ""
        If Not GuardarInstrumentoConBloqueo(instrumento, idGuardado, motivo) Then
            Throw New InvalidOperationException(If(String.IsNullOrWhiteSpace(motivo),
                "No fue posible actualizar la escritura.", motivo))
        End If
    End Sub

    Public Function AnularInstrumento(id As Integer) As Integer
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If id <= 0 Then Return 0
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
        If idLibro <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar") Then Return New DataTable()
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
    Public Function GuardarInstrumentoConBloqueo(instrumento As Instrumento, _
                                                 ByRef idGuardado As Integer, _
                                                 ByRef motivo As String) As Boolean
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If instrumento Is Nothing Then Throw New ArgumentNullException(NameOf(instrumento))
        If instrumento.ID < 0 Then Throw New ArgumentException("El identificador de la escritura no es válido.", NameOf(instrumento))
        If instrumento.IDLibro <= 0 Then Throw New ArgumentException("Seleccione un libro del protocolo.", NameOf(instrumento))
        If instrumento.NumeroEscritura <= 0 Then Throw New ArgumentException("El número de escritura debe ser mayor que cero.", NameOf(instrumento))
        If instrumento.FolioInicio < 1 OrElse instrumento.FolioFin < instrumento.FolioInicio Then
            Throw New ArgumentException("El rango de folios no es válido.", NameOf(instrumento))
        End If
        If instrumento.FechaHoraOtorgamiento = Date.MinValue OrElse instrumento.FechaHoraOtorgamiento.Date.Year < 1000 OrElse instrumento.FechaHoraOtorgamiento.Date > Date.Today Then
            Throw New ArgumentException("La fecha de otorgamiento no puede estar vacía ni ser futura.", NameOf(instrumento))
        End If
        If String.IsNullOrWhiteSpace(instrumento.ObjetoActo) OrElse instrumento.ObjetoActo.Trim().Length > 500 Then
            Throw New ArgumentException("El objeto del acto debe contener entre 1 y 500 caracteres.", NameOf(instrumento))
        End If
        instrumento.ObjetoActo = instrumento.ObjetoActo.Trim()
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

                    ' El número de escritura es único dentro del libro y no debe
                    ' reutilizarse aunque el instrumento anterior esté anulado.
                    ' El bloqueo previo de la fila del libro serializa esta validación
                    ' con las altas y modificaciones concurrentes del mismo libro.
                    Dim numeroDuplicado As Integer
                    Using comandoNumero As New MySqlCommand(
                        "SELECT COUNT(*) FROM Instrumentos " &
                        "WHERE ID_Libro = @libro AND ID <> @excluir AND Numero_Escritura = @numero",
                        conexion, transaccion)
                        Parametro(comandoNumero, "@libro", instrumento.IDLibro)
                        Parametro(comandoNumero, "@excluir", instrumento.ID)
                        Parametro(comandoNumero, "@numero", instrumento.NumeroEscritura)
                        numeroDuplicado = Convert.ToInt32(comandoNumero.ExecuteScalar())
                    End Using

                    If numeroDuplicado > 0 Then
                        motivo = "El número de escritura ya está registrado en este libro. Las escrituras anuladas también conservan su numeración."
                        transaccion.Rollback()
                        Return False
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
                            "VALUES (@libro, @numero, @fecha, @inicio, @fin, @objeto, 'Vigente')",
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
                            "Folio_Inicio = @inicio, Folio_Fin = @fin, Objeto_Acto = @objeto " &
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
        If idLibro <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.gestionar", "protocolo.cerrar_libro") Then Return 0
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
        If idInstrumento <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar", "protocolo.testimonios") Then Return New DataTable()
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
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If idInstrumento <= 0 OrElse idCliente <= 0 Then Throw New ArgumentException("Seleccione un instrumento y compareciente válidos.")
        If Not (String.Equals(calidad, "Otorgante", StringComparison.Ordinal) OrElse
                String.Equals(calidad, "Favorecido", StringComparison.Ordinal) OrElse
                String.Equals(calidad, "Testigo", StringComparison.Ordinal) OrElse
                String.Equals(calidad, "Intérprete", StringComparison.Ordinal)) Then
            Throw New ArgumentException("La calidad del compareciente no pertenece al catálogo permitido.", NameOf(calidad))
        End If
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

                Dim estadoCliente As Object
                Using consultarCliente As New MySqlCommand(
                    "SELECT Activo FROM Clientes WHERE ID = @cliente FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultarCliente, "@cliente", idCliente)
                    estadoCliente = consultarCliente.ExecuteScalar()
                End Using
                If estadoCliente Is Nothing OrElse Convert.IsDBNull(estadoCliente) OrElse
                   Not Convert.ToBoolean(estadoCliente) Then
                    Throw New InvalidOperationException("El cliente no existe o está inactivo; no puede agregarse como compareciente.")
                End If

                Dim duplicado As Integer
                Using consultarDuplicado As New MySqlCommand(
                    "SELECT COUNT(*) FROM Comparecientes_Instrumento " &
                    "WHERE ID_Instrumento = @instrumento AND ID_Cliente = @cliente AND Calidad = @calidad",
                    conexion, transaccion)
                    Parametro(consultarDuplicado, "@instrumento", idInstrumento)
                    Parametro(consultarDuplicado, "@cliente", idCliente)
                    Parametro(consultarDuplicado, "@calidad", calidad)
                    duplicado = Convert.ToInt32(consultarDuplicado.ExecuteScalar())
                End Using
                If duplicado > 0 Then
                    Throw New InvalidOperationException("El cliente ya está registrado con esa calidad en esta escritura.")
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
        If Not Permisos.TieneAcceso("protocolo.gestionar") Then Throw New UnauthorizedAccessException("No tiene permiso para gestionar el protocolo.")
        If id <= 0 Then Return 0
        Return EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                ' Primero se obtiene la escritura para respetar el orden global de bloqueos:
                ' libro -> instrumento -> compareciente. Así se reduce el riesgo de deadlocks
                ' con el cierre del libro y las operaciones de testimonios.
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

                Dim detalleHistorico As String = Nothing
                Using consultarDetalle As New MySqlCommand(
                    "SELECT i.Numero_Escritura, c.ID_Cliente, c.Calidad " &
                    "FROM Comparecientes_Instrumento c " &
                    "INNER JOIN Instrumentos i ON i.ID = c.ID_Instrumento " &
                    "WHERE c.ID = @id AND c.ID_Instrumento = @instrumento FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultarDetalle, "@id", id)
                    Parametro(consultarDetalle, "@instrumento", idInstrumento)
                    Using lector As MySqlDataReader = consultarDetalle.ExecuteReader()
                        If Not lector.Read() Then Return 0
                        ' No copiar DUI ni otros datos personales al log de auditoría.
                        ' Los identificadores permiten rastrear la relación sin duplicar PII.
                        detalleHistorico = "Escritura: " & Convert.ToString(lector("Numero_Escritura")) &
                            "; ID compareciente: " & id &
                            "; ID cliente: " & Convert.ToString(lector("ID_Cliente")) &
                            "; calidad: " & Convert.ToString(lector("Calidad"))
                    End Using
                End Using

                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "DELETE FROM Comparecientes_Instrumento WHERE ID = @id AND ID_Instrumento = @instrumento",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@id", id)
                        Parametro(comando, "@instrumento", idInstrumento)
                    End Sub)
                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "QUITAR COMPARECIENTE",
                        "Comparecientes_Instrumento", id, detalleHistorico)
                End If
                Return filas
            End Function)
    End Function

    ' ------------------------------------------------------------------
    ' Testimonios
    ' ------------------------------------------------------------------

    Public Function Testimonios(idLibro As Integer) As DataTable
        If idLibro <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar", "protocolo.testimonios") Then Return New DataTable()
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
        If Not Permisos.TieneAcceso("protocolo.testimonios") Then Throw New UnauthorizedAccessException("No tiene permiso para expedir testimonios.")
        If idInstrumento <= 0 Then Throw New ArgumentException("Seleccione un instrumento válido.", NameOf(idInstrumento))
        If Not (String.Equals(tipo, "Primer", StringComparison.Ordinal) OrElse
                String.Equals(tipo, "Ulterior", StringComparison.Ordinal)) Then
            Throw New ArgumentException("El tipo de testimonio debe ser Primer o Ulterior.", NameOf(tipo))
        End If
        If fecha = Date.MinValue OrElse fecha.Date.Year < 1000 OrElse fecha.Date > Date.Today Then
            Throw New ArgumentException("La fecha de expedición no puede estar vacía ni ser futura.", NameOf(fecha))
        End If
        If String.IsNullOrWhiteSpace(destinatario) OrElse destinatario.Trim().Length > 200 Then
            Throw New ArgumentException("Indique un destinatario válido de hasta 200 caracteres.", NameOf(destinatario))
        End If
        destinatario = destinatario.Trim()
        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim idLibro As Integer = 0
                Using consultarLibroId As New MySqlCommand(
                    "SELECT ID_Libro FROM Instrumentos WHERE ID = @id",
                    conexion, transaccion)
                    Parametro(consultarLibroId, "@id", idInstrumento)
                    Dim valor As Object = consultarLibroId.ExecuteScalar()
                    If valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) Then
                        idLibro = Convert.ToInt32(valor)
                    End If
                End Using
                If idLibro <= 0 Then Throw New InvalidOperationException("La escritura no existe.")

                ' El libro se bloquea antes que la escritura para serializar la
                ' expedición con el registro de entrega al Notariado.
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
                    Throw New InvalidOperationException(
                        "No se pueden expedir testimonios después de registrar la entrega del libro al Notariado.")
                End If

                Dim fechaOtorgamiento As Date
                Dim estadoInstrumento As String = Nothing
                Using consultar As New MySqlCommand(
                    "SELECT Estado, Fecha_Hora_Otorgamiento FROM Instrumentos " &
                    "WHERE ID = @id AND ID_Libro = @libro FOR UPDATE",
                    conexion, transaccion)
                    Parametro(consultar, "@id", idInstrumento)
                    Parametro(consultar, "@libro", idLibro)
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
        If idLibro <= 0 OrElse Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar") Then Return New DataTable()
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
        If Not Permisos.TieneAccesoAlguno("protocolo.ver", "protocolo.gestionar") Then Return New DataTable()
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

    Private Sub ValidarDatosLibro(libro As LibroProtocolo)
        If libro.NumeroLibro <= 0 Then Throw New ArgumentException("El número de libro debe ser mayor que cero.", NameOf(libro))
        If libro.AnioVigencia < 2000 OrElse libro.AnioVigencia > 2100 Then
            Throw New ArgumentException("El año de vigencia no es válido.", NameOf(libro))
        End If
        If libro.FechaAutorizacion = Date.MinValue OrElse libro.FechaVencimiento = Date.MinValue OrElse
           libro.FechaAutorizacion.Date.Year < 1000 OrElse libro.FechaVencimiento.Date.Year < 1000 Then
            Throw New ArgumentException("Las fechas de autorización y vencimiento son obligatorias.", NameOf(libro))
        End If
        If libro.FechaVencimiento.Date < libro.FechaAutorizacion.Date Then
            Throw New ArgumentException("El vencimiento no puede ser anterior a la autorización.", NameOf(libro))
        End If
    End Sub

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
    End Sub

End Class
