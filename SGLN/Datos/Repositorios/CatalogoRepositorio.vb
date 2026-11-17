Imports System
Imports System.Collections.Generic
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Catálogos y configuración general del sistema.
''' </summary>
Public Class CatalogoRepositorio
    Inherits RepositorioBase

    Public Function Juzgados() As DataTable
        If Not (Permisos.TieneAcceso("casos.ver") OrElse Permisos.TieneAcceso("casos.gestionar")) Then Return New DataTable()
        Return Consultar(
            "SELECT ID, CONCAT(Nombre, ' (', Materia, ')') AS Nombre FROM Juzgados " &
            "WHERE Activo = 1 ORDER BY Nombre")
    End Function

    Public Function TiposActuacion() As DataTable
        If Not (Permisos.TieneAcceso("casos.ver") OrElse Permisos.TieneAcceso("casos.gestionar")) Then Return New DataTable()
        Return Consultar(
            "SELECT ID, CONCAT(Nombre, ' - ', Plazo_Dias_Default, ' días') AS Nombre, " &
            "Plazo_Dias_Default FROM Tipos_Actuacion WHERE Activo = 1 ORDER BY Nombre")
    End Function

    Public Function Feriados() As DataTable
        If Not (Permisos.TieneAcceso("config.feriados") OrElse Permisos.TieneAcceso("config.ver")) Then Return New DataTable()
        Return Consultar("SELECT ID, Fecha, Descripcion FROM Feriados ORDER BY Fecha")
    End Function

    Public Function FechasFeriadas() As List(Of Date)
        Dim puedeConsultar As Boolean =
            Permisos.TieneAcceso("casos.confirmar_plazo") OrElse
            Permisos.TieneAcceso("protocolo.ver") OrElse
            Permisos.TieneAcceso("protocolo.entregar") OrElse
            Permisos.TieneAcceso("protocolo.gestionar")
        If Not puedeConsultar Then Return New List(Of Date)()

        Dim fechas As New List(Of Date)()
        Dim tabla As DataTable = Consultar("SELECT Fecha FROM Feriados")
        For Each fila As DataRow In tabla.Rows
            fechas.Add(Convert.ToDateTime(fila("Fecha")).Date)
        Next
        Return fechas
    End Function

    Public Function ObtenerConfig(clave As String) As String
        If String.IsNullOrWhiteSpace(clave) OrElse clave.Trim().Length > 60 Then Return ""
        Dim claveNormalizada As String = clave.Trim()
        Dim permisoLectura As Boolean
        Select Case claveNormalizada
            Case "Umbral_Efectivo", "LDA_Analisis_Dias", "LDA_Reporte_Dias",
                 "ARCO_Dias_Respuesta", "ARCO_Dias_Prorroga"
                permisoLectura =
                    Permisos.TieneAcceso("cumplimiento.ver") OrElse
                    Permisos.TieneAcceso("cumplimiento.operaciones") OrElse
                    Permisos.TieneAcceso("cumplimiento.arco") OrElse
                    Permisos.TieneAcceso("cumplimiento.reportar_uif")
            Case "Protocolo_Dias_Entrega"
                permisoLectura =
                    Permisos.TieneAcceso("protocolo.ver") OrElse
                    Permisos.TieneAcceso("protocolo.entregar") OrElse
                    Permisos.TieneAcceso("protocolo.gestionar")
            Case Else
                permisoLectura = Permisos.TieneAcceso("config.ver")
        End Select
        If Not permisoLectura Then Return ""

        Dim valor As String = Escalar(Of String)(
            "SELECT Valor FROM Configuracion WHERE Clave = @clave",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@clave", claveNormalizada)
            End Sub)
        If valor Is Nothing Then Return ""
        Return Convert.ToString(valor)
    End Function

    Public Function GuardarConfig(clave As String, valor As String) As Respuesta
        If Not Permisos.TieneAcceso("config.editar") Then
            Return Respuesta.Fallo("No tiene permiso para modificar la configuración.")
        End If
        If String.IsNullOrWhiteSpace(clave) OrElse clave.Trim().Length > 60 Then
            Return Respuesta.Fallo("La clave de configuración no es válida.")
        End If
        If String.Equals(clave.Trim(), "Carpeta_Documentos", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(clave.Trim(), "Carpeta_Respaldos", StringComparison.OrdinalIgnoreCase) Then
            Return Respuesta.Fallo("Las rutas de documentos y respaldos no se pueden modificar desde la configuración general.")
        End If
        If If(valor, "").Trim().Length > 500 Then
            Return Respuesta.Fallo("El valor de configuración no puede superar 500 caracteres.")
        End If
        clave = clave.Trim()
        valor = If(valor, "").Trim()

        EnTransaccion(
            Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                Dim filas As Integer = Ejecutar(conexion, transaccion,
                    "INSERT INTO Configuracion (Clave, Valor) VALUES (@clave, @valor) " &
                    "ON DUPLICATE KEY UPDATE Valor = VALUES(Valor)",
                    Sub(comando As MySqlCommand)
                        Parametro(comando, "@clave", clave)
                        Parametro(comando, "@valor", valor)
                    End Sub)
                AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Configuracion", Nothing,
                    "Clave actualizada: " & clave.Trim())
                Return filas
            End Function)
        Return Respuesta.Ok("Configuración guardada.")
    End Function

    Public Function ListarConfig() As DataTable
        If Not Permisos.TieneAcceso("config.ver") Then Return New DataTable()
        Return Consultar("SELECT Clave, Valor FROM Configuracion ORDER BY Clave")
    End Function

End Class
