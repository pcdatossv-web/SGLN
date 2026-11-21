Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Consulta de la bitácora de auditoría. Solo lectura: los registros se
''' insertan desde RepositorioBase.Auditar.
''' </summary>
Public Class AuditoriaRepositorio
    Inherits RepositorioBase

    Private Sub AgregarParametrosFiltro(comando As MySqlCommand,
                                        usuario As String,
                                        tabla As String,
                                        accion As String,
                                        desde As Date,
                                        hasta As Date,
                                        limiteMaximo As Boolean,
                                        idMaximo As Long?)
        Parametro(comando, "@usuario", If(usuario, "").Trim())
        Parametro(comando, "@likeUsuario", "%" & If(usuario, "").Trim() & "%")
        Parametro(comando, "@tabla", If(tabla, "").Trim())
        Parametro(comando, "@likeTabla", "%" & If(tabla, "").Trim() & "%")
        Parametro(comando, "@accion", If(String.IsNullOrWhiteSpace(accion), CType(DBNull.Value, Object), accion.Trim()))
        Parametro(comando, "@desde", desde.Date)
        Parametro(comando, "@hasta",
                  If(limiteMaximo, Date.MaxValue, hasta.Date.AddDays(1)))
        Parametro(comando, "@idMaximo",
                  If(idMaximo.HasValue, CObj(idMaximo.Value), DBNull.Value))
    End Sub

    Private Function RangoValido(desde As Date, hasta As Date) As Boolean
        Return desde.Date >= New Date(1000, 1, 1) AndAlso hasta.Date >= desde.Date
    End Function

    Private Function CondicionHasta(limiteMaximo As Boolean) As String
        Return If(limiteMaximo, "Fecha_Hora <= @hasta", "Fecha_Hora < @hasta")
    End Function

    Public Function ObtenerMaximoId() As Long
        If Not Permisos.TieneAcceso("auditoria.ver") Then Return 0L
        Dim resultado As DataTable = Consultar(
            "SELECT COALESCE(MAX(ID), 0) AS Maximo FROM Audit_Log")
        If resultado.Rows.Count = 0 OrElse resultado.Rows(0).IsNull("Maximo") Then Return 0L
        Return Convert.ToInt64(resultado.Rows(0)("Maximo"))
    End Function

    Public Function Buscar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date,
                           Optional idMaximo As Long? = Nothing) As DataTable
        If Not Permisos.TieneAcceso("auditoria.ver") OrElse Not RangoValido(desde, hasta) Then Return New DataTable()

        Dim limiteMaximo As Boolean = hasta.Date = Date.MaxValue.Date
        Return Consultar(
            "SELECT ID, Usuario, Fecha_Hora, Accion, Tabla_Afectada, ID_Registro, Detalle " &
            "FROM Audit_Log " &
            "WHERE (@usuario IS NULL OR Usuario LIKE @likeUsuario) " &
            "  AND (@tabla IS NULL OR Tabla_Afectada LIKE @likeTabla) " &
            "  AND (@accion IS NULL OR Accion = @accion) " &
            "  AND Fecha_Hora >= @desde AND " & CondicionHasta(limiteMaximo) & " " &
            "  AND (@idMaximo IS NULL OR ID <= @idMaximo) " &
            "ORDER BY Fecha_Hora DESC, ID DESC",
            Sub(comando As MySqlCommand)
                AgregarParametrosFiltro(comando, usuario, tabla, accion, desde, hasta, limiteMaximo, idMaximo)
            End Sub)
    End Function

    Public Function Contar(usuario As String, tabla As String, accion As String,
                           desde As Date, hasta As Date, idMaximo As Long) As Long
        If Not Permisos.TieneAcceso("auditoria.ver") OrElse Not RangoValido(desde, hasta) Then Return 0L

        Dim limiteMaximo As Boolean = hasta.Date = Date.MaxValue.Date
        Dim resultado As DataTable = Consultar(
            "SELECT COUNT(*) AS Total FROM Audit_Log " &
            "WHERE (@usuario IS NULL OR Usuario LIKE @likeUsuario) " &
            "  AND (@tabla IS NULL OR Tabla_Afectada LIKE @likeTabla) " &
            "  AND (@accion IS NULL OR Accion = @accion) " &
            "  AND Fecha_Hora >= @desde AND " & CondicionHasta(limiteMaximo) & " " &
            "  AND ID <= @idMaximo",
            Sub(comando As MySqlCommand)
                AgregarParametrosFiltro(comando, usuario, tabla, accion, desde, hasta, limiteMaximo, idMaximo)
            End Sub)
        If resultado.Rows.Count = 0 OrElse resultado.Rows(0).IsNull("Total") Then Return 0L
        Return Convert.ToInt64(resultado.Rows(0)("Total"))
    End Function

    Public Function BuscarPagina(usuario As String, tabla As String, accion As String,
                                 desde As Date, hasta As Date,
                                 idMaximo As Long, pagina As Long, tamanoPagina As Integer) As DataTable
        If Not Permisos.TieneAcceso("auditoria.ver") OrElse Not RangoValido(desde, hasta) Then Return New DataTable()
        If pagina < 0 OrElse tamanoPagina < 1 OrElse tamanoPagina > 500 Then
            Throw New ArgumentOutOfRangeException(NameOf(pagina), "Los parámetros de paginación no son válidos.")
        End If
        If pagina > Long.MaxValue \ tamanoPagina Then
            Throw New ArgumentOutOfRangeException(NameOf(pagina), "El desplazamiento solicitado supera el límite permitido.")
        End If

        Dim desplazamiento As Long = pagina * CLng(tamanoPagina)
        Dim limiteMaximo As Boolean = hasta.Date = Date.MaxValue.Date
        Return Consultar(
            "SELECT ID, Usuario, Fecha_Hora, Accion, Tabla_Afectada, ID_Registro, Detalle " &
            "FROM Audit_Log " &
            "WHERE (@usuario IS NULL OR Usuario LIKE @likeUsuario) " &
            "  AND (@tabla IS NULL OR Tabla_Afectada LIKE @likeTabla) " &
            "  AND (@accion IS NULL OR Accion = @accion) " &
            "  AND Fecha_Hora >= @desde AND " & CondicionHasta(limiteMaximo) & " " &
            "  AND ID <= @idMaximo " &
            "ORDER BY Fecha_Hora DESC, ID DESC LIMIT @limite OFFSET @desplazamiento",
            Sub(comando As MySqlCommand)
                AgregarParametrosFiltro(comando, usuario, tabla, accion, desde, hasta, limiteMaximo, idMaximo)
                Parametro(comando, "@limite", tamanoPagina)
                Parametro(comando, "@desplazamiento", desplazamiento)
            End Sub)
    End Function

    Public Function TablasAfectadas() As DataTable
        If Not Permisos.TieneAcceso("auditoria.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT DISTINCT Tabla_Afectada FROM Audit_Log ORDER BY Tabla_Afectada")
    End Function

    Public Function Acciones() As DataTable
        If Not Permisos.TieneAcceso("auditoria.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT DISTINCT Accion FROM Audit_Log ORDER BY Accion")
    End Function

    Public Function Usuarios() As DataTable
        If Not Permisos.TieneAcceso("auditoria.ver") Then Return New DataTable()
        Return Consultar(
            "SELECT DISTINCT Usuario FROM Audit_Log ORDER BY Usuario")
    End Function

End Class
