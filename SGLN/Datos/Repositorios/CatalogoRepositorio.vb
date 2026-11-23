Imports System
Imports System.Collections.Generic
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Catálogos y configuración general del sistema.
''' </summary>
Public Class CatalogoRepositorio
    Inherits RepositorioBase

    ''' <summary>
    ''' Revalida una vez por consulta y evalúa los permisos desde la matriz actual.
    ''' </summary>
    Public Function Juzgados() As DataTable
        If Not Permisos.TieneAccesoAlguno("casos.ver", "casos.gestionar") Then Return New DataTable()
        Return Consultar(
            "SELECT ID, CONCAT(Nombre, ' (', Materia, ')') AS Nombre FROM Juzgados " &
            "WHERE Activo = 1 ORDER BY Nombre")
    End Function

    Public Function TiposActuacion() As DataTable
        If Not Permisos.TieneAccesoAlguno("casos.ver", "casos.gestionar") Then Return New DataTable()
        Return Consultar(
            "SELECT ID, CONCAT(Nombre, ' - ', Plazo_Dias_Default, ' días') AS Nombre, " &
            "Plazo_Dias_Default, Materia FROM Tipos_Actuacion WHERE Activo = 1 ORDER BY Nombre")
    End Function

    Public Function Feriados() As DataTable
        If Not Permisos.TieneAccesoAlguno("config.feriados", "config.ver") Then Return New DataTable()
        Return Consultar("SELECT ID, Fecha, Descripcion FROM Feriados ORDER BY Fecha")
    End Function

    Public Function FechasFeriadas() As List(Of Date)
        Dim puedeConsultar As Boolean = Permisos.TieneAccesoAlguno(
            "casos.confirmar_plazo",
            "protocolo.ver", "protocolo.entregar", "protocolo.gestionar",
            "cumplimiento.ver", "cumplimiento.operaciones",
            "cumplimiento.arco", "cumplimiento.reportar_uif")
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
                permisoLectura = Permisos.TieneAccesoAlguno(
                    "cumplimiento.ver", "cumplimiento.operaciones",
                    "cumplimiento.arco", "cumplimiento.reportar_uif")
            Case "Protocolo_Dias_Entrega"
                permisoLectura = Permisos.TieneAccesoAlguno(
                    "protocolo.ver", "protocolo.entregar", "protocolo.gestionar")
            Case "Carpeta_Documentos"
                permisoLectura = Permisos.TieneAccesoAlguno(
                    "documentos.ver", "documentos.importar",
                    "documentos.verificar", "documentos.metadatos")
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
                ' GUARDAR no presupone si la clave fue creada o modificada.
                ' Evita atribuir una modificación cuando la clave se inserta por primera vez.
                If filas > 0 Then
                    AuditarEnTransaccion(conexion, transaccion, "GUARDAR", "Configuracion",
                        Nothing, "Clave guardada: " & clave.Trim())
                End If
                Return filas
            End Function)
        Return Respuesta.Ok("Configuración guardada.")
    End Function

    Public Function ListarConfig() As DataTable
        If Not Permisos.TieneAcceso("config.ver") Then Return New DataTable()
        Return Consultar("SELECT Clave, Valor FROM Configuracion ORDER BY Clave")
    End Function

End Class
