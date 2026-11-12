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
        Return Consultar(
            "SELECT ID, CONCAT(Nombre, ' (', Materia, ')') AS Nombre FROM Juzgados " &
            "WHERE Activo = 1 ORDER BY Nombre")
    End Function

    Public Function TiposActuacion() As DataTable
        Return Consultar(
            "SELECT ID, CONCAT(Nombre, ' - ', Plazo_Dias_Default, ' días') AS Nombre, " &
            "Plazo_Dias_Default FROM Tipos_Actuacion WHERE Activo = 1 ORDER BY Nombre")
    End Function

    Public Function Feriados() As DataTable
        Return Consultar("SELECT ID, Fecha, Descripcion FROM Feriados ORDER BY Fecha")
    End Function

    Public Function FechasFeriadas() As List(Of Date)
        Dim fechas As New List(Of Date)()
        Dim tabla As DataTable = Consultar("SELECT Fecha FROM Feriados")
        For Each fila As DataRow In tabla.Rows
            fechas.Add(Convert.ToDateTime(fila("Fecha")).Date)
        Next
        Return fechas
    End Function

    Public Function ObtenerConfig(clave As String) As String
        Dim valor As String = Escalar(Of String)(
            "SELECT Valor FROM Configuracion WHERE Clave = @clave",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@clave", clave)
            End Sub)
        If valor Is Nothing Then Return ""
        Return Convert.ToString(valor)
    End Function

    Public Function GuardarConfig(clave As String, valor As String) As Respuesta
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
                    clave & " = " & valor)
                Return filas
            End Function)
        Return Respuesta.Ok("Configuración guardada.")
    End Function

    Public Function ListarConfig() As DataTable
        Return Consultar("SELECT Clave, Valor FROM Configuracion ORDER BY Clave")
    End Function

End Class
