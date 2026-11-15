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

    Public Function Buscar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date) As DataTable
        If Not PuedeConsultar() Then Return New DataTable()
        If hasta.Date < desde.Date Then Return New DataTable()
        Return _repositorio.Buscar(usuario, tabla, accion, desde, hasta)
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
