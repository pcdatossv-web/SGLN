Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Consulta de la bitácora de auditoría. Solo lectura: los registros se
''' insertan desde RepositorioBase.Auditar.
''' </summary>
Public Class AuditoriaRepositorio
    Inherits RepositorioBase

    Public Function Buscar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date) As DataTable

        Return Consultar(
            "SELECT ID, Usuario, Fecha_Hora, Accion, Tabla_Afectada, ID_Registro, Detalle " &
            "FROM Audit_Log " &
            "WHERE (@usuario IS NULL OR Usuario LIKE @likeUsuario) " &
            "  AND (@tabla IS NULL OR Tabla_Afectada LIKE @likeTabla) " &
            "  AND (@accion IS NULL OR Accion = @accion) " &
            "  AND Fecha_Hora >= @desde AND Fecha_Hora < @hasta " &
            "ORDER BY Fecha_Hora DESC, ID DESC",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@usuario", If(usuario, "").Trim())
                Parametro(comando, "@likeUsuario", "%" & If(usuario, "").Trim() & "%")
                Parametro(comando, "@tabla", If(tabla, "").Trim())
                Parametro(comando, "@likeTabla", "%" & If(tabla, "").Trim() & "%")
                Parametro(comando, "@accion", If(accion, "").Trim())
                Parametro(comando, "@desde", desde.Date)
                Parametro(comando, "@hasta", hasta.Date.AddDays(1))
            End Sub)
    End Function

    Public Function TablasAfectadas() As DataTable
        Return Consultar(
            "SELECT DISTINCT Tabla_Afectada FROM Audit_Log ORDER BY Tabla_Afectada")
    End Function

    Public Function Acciones() As DataTable
        Return Consultar(
            "SELECT DISTINCT Accion FROM Audit_Log ORDER BY Accion")
    End Function

    Public Function Usuarios() As DataTable
        Return Consultar(
            "SELECT DISTINCT Usuario FROM Audit_Log ORDER BY Usuario")
    End Function

End Class
