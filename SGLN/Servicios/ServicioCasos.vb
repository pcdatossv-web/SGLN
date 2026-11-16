Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Reglas de negocio de casos judiciales.
''' </summary>
Public Class ServicioCasos

    Private ReadOnly _repositorio As New CasoRepositorio()
    Private ReadOnly _catalogo As New CatalogoRepositorio()

    Public Function Buscar(filtro As String) As DataTable
        If Not Permisos.TieneAcceso("casos.ver") Then Return New DataTable()
        Return _repositorio.Buscar(filtro)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If Not Permisos.TieneAcceso("casos.ver") Then Return New DataTable()
        Return _repositorio.Obtener(id)
    End Function

    Public Function Guardar(caso As Caso) As Respuesta
        If Not Permisos.TieneAcceso("casos.gestionar") Then Return Respuesta.Fallo("No tiene permiso para gestionar casos.")
        If caso Is Nothing Then Return Respuesta.Fallo("No se recibieron los datos del caso.")
        If caso.IDCliente <= 0 Then Return Respuesta.Fallo("Seleccione el cliente del caso.")
        If String.IsNullOrWhiteSpace(caso.NumeroExpediente) Then Return Respuesta.Fallo("El número de expediente es obligatorio.")
        If caso.NumeroExpediente.Trim().Length > 80 Then Return Respuesta.Fallo("El número de expediente no puede superar 80 caracteres.")
        If String.IsNullOrWhiteSpace(caso.Materia) Then Return Respuesta.Fallo("Seleccione la materia del caso.")
        If caso.Materia.Trim().Length > 40 Then Return Respuesta.Fallo("La materia no puede superar 40 caracteres.")
        If If(caso.Descripcion, "").Length > 1000 Then Return Respuesta.Fallo("La descripción no puede superar 1000 caracteres.")
        If If(caso.Responsable, "").Length > 120 Then Return Respuesta.Fallo("El responsable no puede superar 120 caracteres.")
        If Array.IndexOf(New String() {"Activo", "Suspendido", "Cerrado", "Archivado"},
                             If(caso.Estado, "")) < 0 Then
            Return Respuesta.Fallo("El estado del caso no es válido.")
        End If
        If caso.FechaApertura = Date.MinValue OrElse caso.FechaApertura.Date > Date.Today Then
            Return Respuesta.Fallo("La fecha de apertura no es válida.")
        End If

        caso.NumeroExpediente = caso.NumeroExpediente.Trim()
        caso.Materia = caso.Materia.Trim()
        caso.Descripcion = If(caso.Descripcion, "").Trim()
        caso.Responsable = If(caso.Responsable, "").Trim()

        Try
            If caso.ID = 0 Then
                caso.ID = _repositorio.Insertar(caso)
            Else
                Dim filas As Integer = _repositorio.Actualizar(caso)
                If filas = 0 AndAlso _repositorio.Obtener(caso.ID).Rows.Count = 0 Then
                    Return Respuesta.Fallo("El caso ya no existe.")
                End If
            End If
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un caso con ese número de expediente en el mismo juzgado.")
        Catch ex As InvalidOperationException
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Casos - guardar", ex)
            Return Respuesta.Fallo("No se pudo guardar el caso. Verifique los datos y la conexión.")
        End Try

        Return New Respuesta With {
            .Exito = True,
            .Mensaje = "Caso guardado correctamente.",
            .ID = caso.ID
        }
    End Function

    Public Function Actuaciones(idCaso As Integer) As DataTable
        If Not Permisos.TieneAcceso("casos.ver") Then Return New DataTable()
        If idCaso <= 0 Then Return New DataTable()
        Return _repositorio.ActuacionesPorCaso(idCaso)
    End Function

    Public Function ClientesCombo() As DataTable
        If Not (Permisos.TieneAcceso("casos.ver") OrElse Permisos.TieneAcceso("casos.gestionar")) Then Return New DataTable()
        Return _repositorio.ClientesCombo()
    End Function

    Public Function JuzgadosCombo() As DataTable
        If Not (Permisos.TieneAcceso("casos.ver") OrElse Permisos.TieneAcceso("casos.gestionar")) Then Return New DataTable()
        Return _catalogo.Juzgados()
    End Function

    Public Function TiposActuacionCombo() As DataTable
        If Not (Permisos.TieneAcceso("casos.ver") OrElse Permisos.TieneAcceso("casos.gestionar")) Then Return New DataTable()
        Return _catalogo.TiposActuacion()
    End Function

End Class
