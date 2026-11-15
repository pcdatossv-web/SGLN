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

        If caso.IDCliente <= 0 Then Return Respuesta.Fallo("Seleccione el cliente del caso.")
        If String.IsNullOrWhiteSpace(caso.NumeroExpediente) Then Return Respuesta.Fallo("El número de expediente es obligatorio.")
        If String.IsNullOrWhiteSpace(caso.Materia) Then Return Respuesta.Fallo("Seleccione la materia del caso.")

        caso.NumeroExpediente = caso.NumeroExpediente.Trim()

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
        End Try

        Return New Respuesta With {
            .Exito = True,
            .Mensaje = "Caso guardado correctamente.",
            .ID = caso.ID
        }
    End Function

    Public Function Actuaciones(idCaso As Integer) As DataTable
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
