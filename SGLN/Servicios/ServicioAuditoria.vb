Imports System
Imports System.Data

''' <summary>
''' Reglas de consulta de la bitácora. La auditoría es de solo lectura desde
''' la aplicación: no se permite borrar ni editar registros.
''' </summary>
Public Class ServicioAuditoria

    Private ReadOnly _repositorio As New AuditoriaRepositorio()

    ' La autorización se aplica también en la capa de servicio: ocultar el
    ' formulario no debe ser el único control para consultar datos sensibles.
    Private Function PuedeConsultar() As Boolean
        Return Permisos.TieneAcceso("auditoria.ver")
    End Function

    Public Function ObtenerMaximoId() As Long
        If Not PuedeConsultar() Then Return 0L
        Return _repositorio.ObtenerMaximoId()
    End Function

    Public Function Buscar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date,
                           Optional idMaximo As Long? = Nothing) As DataTable
        If Not PuedeConsultar() Then Return New DataTable()
        If desde.Date < New Date(1000, 1, 1) OrElse hasta.Date < desde.Date Then Return New DataTable()
        Return _repositorio.Buscar(usuario, tabla, accion, desde, hasta, idMaximo)
    End Function

    Public Function Contar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date, idMaximo As Long) As Long
        If Not PuedeConsultar() OrElse desde.Date < New Date(1000, 1, 1) OrElse hasta.Date < desde.Date Then Return 0L
        Return _repositorio.Contar(usuario, tabla, accion, desde, hasta, idMaximo)
    End Function

    Public Function BuscarPagina(usuario As String, tabla As String, accion As String,
                                 desde As Date, hasta As Date,
                                 idMaximo As Long, pagina As Long, tamanoPagina As Integer) As DataTable
        If Not PuedeConsultar() OrElse desde.Date < New Date(1000, 1, 1) OrElse hasta.Date < desde.Date Then Return New DataTable()
        Return _repositorio.BuscarPagina(usuario, tabla, accion, desde, hasta, idMaximo, pagina, tamanoPagina)
    End Function

    Public Function TablasAfectadas() As DataTable
        If Not PuedeConsultar() Then Return New DataTable()
        Return _repositorio.TablasAfectadas()
    End Function

    Public Function Acciones() As DataTable
        If Not PuedeConsultar() Then Return New DataTable()
        Return _repositorio.Acciones()
    End Function

    Public Function Usuarios() As DataTable
        If Not PuedeConsultar() Then Return New DataTable()
        Return _repositorio.Usuarios()
    End Function

End Class
