Imports System
Imports System.Data

''' <summary>
''' Reglas de consulta de la bitácora. La auditoría es de solo lectura desde
''' la aplicación: no se permite borrar ni editar registros.
''' </summary>
Public Class ServicioAuditoria

    Private ReadOnly _repositorio As New AuditoriaRepositorio()

    Public Function Buscar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date) As DataTable
        Return _repositorio.Buscar(usuario, tabla, accion, desde, hasta)
    End Function

    Public Function TablasAfectadas() As DataTable
        Return _repositorio.TablasAfectadas()
    End Function

    Public Function Acciones() As DataTable
        Return _repositorio.Acciones()
    End Function

    Public Function Usuarios() As DataTable
        Return _repositorio.Usuarios()
    End Function

End Class
