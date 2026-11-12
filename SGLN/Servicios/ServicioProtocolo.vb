Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de negocio del protocolo notarial. Centraliza las validaciones
''' para que los formularios solo se ocupen de la interfaz.
''' </summary>
Public Class ServicioProtocolo

    Private ReadOnly _repositorio As New ProtocoloRepositorio()
    Private ReadOnly _catalogo As New CatalogoRepositorio()

    ' ------------------------------------------------------------------
    ' Libros
    ' ------------------------------------------------------------------

    Public Function Libros() As DataTable
        Return _repositorio.Libros()
    End Function

    Public Function ObtenerLibro(id As Integer) As DataTable
        Return _repositorio.ObtenerLibro(id)
    End Function

    Public Function GuardarLibro(libro As LibroProtocolo) As Respuesta
        If libro.NumeroLibro <= 0 Then Return Respuesta.Fallo("El número de libro debe ser mayor que cero.")
        If libro.AnioVigencia < 2000 OrElse libro.AnioVigencia > 2100 Then
            Return Respuesta.Fallo("El año de vigencia no es válido.")
        End If
        If libro.FechaVencimiento.Date < libro.FechaAutorizacion.Date Then
            Return Respuesta.Fallo("La fecha de vencimiento no puede ser anterior a la de autorización.")
        End If

        ' Un libro cerrado o entregado no se reabre desde el editor: se conserva
        ' el estado registrado para no romper la trazabilidad del protocolo.
        If libro.ID > 0 Then
            Dim actual As DataTable = _repositorio.ObtenerLibro(libro.ID)
            If actual.Rows.Count > 0 Then
                Dim estadoActual As String = Convert.ToString(actual.Rows(0)("Estado"))
                If estadoActual <> "Vigente" Then libro.Estado = estadoActual
            End If
        End If

        Try
            If libro.ID = 0 Then
                libro.ID = _repositorio.InsertarLibro(libro)
            Else
                _repositorio.ActualizarLibro(libro)
            End If
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un libro con ese número para el mismo año.")
        End Try

        Return New Respuesta With {
            .Exito = True,
            .Mensaje = "Libro guardado correctamente.",
            .ID = libro.ID
        }
    End Function

    Public Function CerrarLibro(id As Integer, razon As String) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un libro.")
        If String.IsNullOrWhiteSpace(razon) Then Return Respuesta.Fallo("Indique la razón del cierre del libro.")

        Dim libro As DataTable = _repositorio.ObtenerLibro(id)
        If libro.Rows.Count = 0 Then Return Respuesta.Fallo("El libro no existe.")
        If Convert.ToString(libro.Rows(0)("Estado")) <> "Vigente" Then
            Return Respuesta.Fallo("Solo se pueden cerrar libros en estado Vigente.")
        End If

        Dim pendientes As Integer = _repositorio.InstrumentosSinOtorgante(id)
        If pendientes > 0 Then
            Return Respuesta.Fallo(
                "No se puede cerrar el libro: " & pendientes &
                " instrumento(s) no tienen otorgante registrado.")
        End If

        _repositorio.CerrarLibro(id, razon.Trim(), Date.Today)
        Return Respuesta.Ok("Libro cerrado. Recuerde la entrega al Notariado.")
    End Function

    ''' <summary>
    ''' Calcula la fecha límite referencial de entrega al Notariado.
    ''' Se toman los días configurados (15 por defecto) en días hábiles.
    ''' </summary>
    Public Function FechaLimiteEntrega(fechaCierre As Date) As Date
        Dim dias As Integer = DiasEntregaConfigurados()
        Return New CalculadoraDiasHabiles(_catalogo.FechasFeriadas()).CalcularVencimiento(fechaCierre, dias)
    End Function

    Public Function DiasEntregaConfigurados() As Integer
        Dim valor As String = _catalogo.ObtenerConfig("Protocolo_Dias_Entrega")
        Dim dias As Integer
        If Integer.TryParse(valor, dias) AndAlso dias > 0 Then Return dias
        Return 15
    End Function

    Public Function RegistrarEntrega(id As Integer, fechaEntrega As Date) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un libro.")

        Dim libro As DataTable = _repositorio.ObtenerLibro(id)
        If libro.Rows.Count = 0 Then Return Respuesta.Fallo("El libro no existe.")

        Dim fila As DataRow = libro.Rows(0)
        If Convert.ToString(fila("Estado")) <> "Cerrado" Then
            Return Respuesta.Fallo("Solo se pueden entregar libros cerrados.")
        End If
        If fechaEntrega.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de entrega no puede ser futura.")
        End If
        If fila.IsNull("Fecha_Cierre") Then
            Return Respuesta.Fallo("El libro no tiene fecha de cierre registrada.")
        End If

        Dim cierre As Date = Convert.ToDateTime(fila("Fecha_Cierre")).Date
        Dim limite As Date = FechaLimiteEntrega(cierre)

        _repositorio.RegistrarEntrega(id, fechaEntrega.Date)

        If fechaEntrega.Date > limite Then
            Return Respuesta.Ok(
                "Entrega registrada con atraso. La fecha límite referencial era el " &
                limite.ToString("dd/MM/yyyy") & ".")
        End If

        Return Respuesta.Ok("Entrega al Notariado registrada correctamente.")
    End Function

    ' ------------------------------------------------------------------
    ' Instrumentos
    ' ------------------------------------------------------------------

    Public Function Instrumentos(idLibro As Integer) As DataTable
        Return _repositorio.Instrumentos(idLibro)
    End Function

    Public Function ObtenerInstrumento(id As Integer) As DataTable
        Return _repositorio.ObtenerInstrumento(id)
    End Function

    Public Function GuardarInstrumento(instrumento As Instrumento) As Respuesta
        If instrumento.IDLibro <= 0 Then Return Respuesta.Fallo("Seleccione el libro del protocolo.")
        If instrumento.NumeroEscritura <= 0 Then Return Respuesta.Fallo("El número de escritura debe ser mayor que cero.")
        If String.IsNullOrWhiteSpace(instrumento.ObjetoActo) Then Return Respuesta.Fallo("Describa el objeto del acto.")
        If instrumento.FolioInicio < 1 Then Return Respuesta.Fallo("El folio inicial debe ser mayor o igual a 1.")
        If instrumento.FolioFin < instrumento.FolioInicio Then
            Return Respuesta.Fallo("El folio final no puede ser menor que el folio inicial.")
        End If
        If instrumento.FechaHoraOtorgamiento.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de otorgamiento no puede ser futura.")
        End If

        Dim libro As DataTable = _repositorio.ObtenerLibro(instrumento.IDLibro)
        If libro.Rows.Count = 0 Then Return Respuesta.Fallo("El libro seleccionado no existe.")
        If Convert.ToString(libro.Rows(0)("Estado")) <> "Vigente" Then
            Return Respuesta.Fallo("Solo se pueden registrar escrituras en libros vigentes.")
        End If

        Try
            Dim idGuardado As Integer = 0
            Dim motivo As String = ""
            If Not _repositorio.GuardarInstrumentoConBloqueo(instrumento, idGuardado, motivo) Then
                Return Respuesta.Fallo(motivo)
            End If

            instrumento.ID = idGuardado
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe una escritura con ese número en el libro.")
        End Try

        Return New Respuesta With {
            .Exito = True,
            .Mensaje = "Instrumento guardado correctamente.",
            .ID = instrumento.ID
        }
    End Function

    Public Function AnularInstrumento(id As Integer) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un instrumento.")
        _repositorio.AnularInstrumento(id)
        Return Respuesta.Ok("Instrumento anulado. El historial se conserva.")
    End Function

    ' ------------------------------------------------------------------
    ' Comparecientes
    ' ------------------------------------------------------------------

    Public Function Comparecientes(idInstrumento As Integer) As DataTable
        Return _repositorio.Comparecientes(idInstrumento)
    End Function

    Public Function AgregarCompareciente(idInstrumento As Integer, idCliente As Integer, calidad As String) As Respuesta
        If idInstrumento <= 0 Then Return Respuesta.Fallo("Seleccione un instrumento.")
        If idCliente <= 0 Then Return Respuesta.Fallo("Seleccione un cliente.")
        If Not CalidadValida(calidad) Then Return Respuesta.Fallo("La calidad del compareciente no es válida.")

        Try
            _repositorio.AgregarCompareciente(idInstrumento, idCliente, calidad)
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ese cliente ya está registrado con la misma calidad en el instrumento.")
        End Try

        Return Respuesta.Ok("Compareciente agregado.")
    End Function

    Private Function CalidadValida(calidad As String) As Boolean
        Select Case calidad
            Case "Otorgante", "Favorecido", "Testigo", "Intérprete"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Function QuitarCompareciente(id As Integer) As Respuesta
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un compareciente.")
        _repositorio.QuitarCompareciente(id)
        Return Respuesta.Ok("Compareciente eliminado.")
    End Function

    Public Function ClientesCombo() As DataTable
        Return _repositorio.ClientesCombo()
    End Function

    ' ------------------------------------------------------------------
    ' Testimonios
    ' ------------------------------------------------------------------

    Public Function Testimonios(idLibro As Integer) As DataTable
        Return _repositorio.Testimonios(idLibro)
    End Function

    Public Function ExpedirTestimonio(idInstrumento As Integer, tipo As String, fecha As Date, destinatario As String) As Respuesta
        If idInstrumento <= 0 Then Return Respuesta.Fallo("Seleccione un instrumento.")
        If String.IsNullOrWhiteSpace(destinatario) Then Return Respuesta.Fallo("Indique el destinatario del testimonio.")
        If tipo <> "Primer" AndAlso tipo <> "Ulterior" Then Return Respuesta.Fallo("El tipo de testimonio no es válido.")
        If fecha.Date > Date.Today Then Return Respuesta.Fallo("La fecha de expedición no puede ser futura.")

        Dim instrumento As DataTable = _repositorio.ObtenerInstrumento(idInstrumento)
        If instrumento.Rows.Count = 0 Then Return Respuesta.Fallo("El instrumento no existe.")
        If Convert.ToString(instrumento.Rows(0)("Estado")) = "Anulado" Then
            Return Respuesta.Fallo("No se pueden expedir testimonios de un instrumento anulado.")
        End If

        _repositorio.InsertarTestimonio(idInstrumento, tipo, fecha.Date, destinatario.Trim())
        Return Respuesta.Ok("Testimonio registrado correctamente.")
    End Function

    ' ------------------------------------------------------------------
    ' Índice
    ' ------------------------------------------------------------------

    Public Function IndiceLibro(idLibro As Integer) As DataTable
        Return _repositorio.IndiceLibro(idLibro)
    End Function

    Public Function GenerarIndice(idLibro As Integer) As Respuesta
        If idLibro <= 0 Then Return Respuesta.Fallo("Seleccione un libro.")
        Dim indice As DataTable = _repositorio.IndiceLibro(idLibro)
        If indice.Rows.Count = 0 Then Return Respuesta.Fallo("El libro no tiene instrumentos para indexar.")

        _repositorio.MarcarIndiceGenerado(idLibro)
        Return Respuesta.Ok("Índice generado con " & indice.Rows.Count & " instrumento(s).")
    End Function

End Class
