Imports System
Imports System.Collections.Generic
Imports System.Data

''' <summary>
''' Alertas transversales del sistema: plazos, LDA/FT, protocolo, UIF y ARCO.
''' Cada bloque se consulta únicamente si la sesión posee el permiso del módulo.
''' </summary>
Public Class ServicioAlertas
    Inherits RepositorioBase

    Public Function ObtenerAlertas() As DataTable
        If Sesion.UsuarioActual Is Nothing Then Return New DataTable()

        Dim consultas As New List(Of String)()

        If Permisos.TieneAcceso("casos.ver") Then
            consultas.Add(
                "SELECT 'Plazo vencido' AS Tipo, " &
                "CONCAT('Actuación vencida: ', t.Nombre, ' - Exp. ', c.Numero_Expediente) AS Descripcion, " &
                "a.Fecha_Vencimiento_Calculada AS Fecha_Referencia, 'Alta' AS Prioridad " &
                "FROM Actuaciones_Procesales a " &
                "INNER JOIN Casos c ON c.ID = a.ID_Caso " &
                "INNER JOIN Tipos_Actuacion t ON t.ID = a.ID_Tipo_Actuacion " &
                "WHERE a.Estado IN ('Pendiente', 'Vencido') AND a.Fecha_Vencimiento_Calculada IS NOT NULL " &
                "AND a.Fecha_Vencimiento_Calculada < CURDATE()")
            consultas.Add(
                "SELECT 'Plazo por vencer' AS Tipo, " &
                "CONCAT('Actuación por vencer: ', t.Nombre, ' - Exp. ', c.Numero_Expediente) AS Descripcion, " &
                "a.Fecha_Vencimiento_Calculada AS Fecha_Referencia, 'Alta' AS Prioridad " &
                "FROM Actuaciones_Procesales a " &
                "INNER JOIN Casos c ON c.ID = a.ID_Caso " &
                "INNER JOIN Tipos_Actuacion t ON t.ID = a.ID_Tipo_Actuacion " &
                "WHERE a.Estado = 'Pendiente' AND a.Fecha_Vencimiento_Calculada " &
                "BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 3 DAY)")
        End If

        If Permisos.TieneAcceso("clientes.ver") Then
            consultas.Add(
                "SELECT 'LDA/FT' AS Tipo, " &
                "CONCAT('Perfil de alto riesgo incompleto: ', c.Nombres, ' ', c.Apellidos) AS Descripcion, " &
                "NULL AS Fecha_Referencia, 'Media' AS Prioridad " &
                "FROM Clientes c INNER JOIN Perfil_LDAFT p ON p.ID_Cliente = c.ID " &
                "WHERE p.Nivel_Riesgo = 'Alto' AND (p.Origen_Fondos_Verificado = 0 " &
                "OR p.Fecha_Verificacion_Listas IS NULL OR p.Fecha_Verificacion_Listas < DATE_SUB(CURDATE(), INTERVAL 1 YEAR))")
        End If

        If Permisos.TieneAcceso("entidades.ver") Then
            consultas.Add(
                "SELECT 'LDA/FT' AS Tipo, " &
                "CONCAT('Perfil de alto riesgo incompleto (entidad): ', e.Razon_Social) AS Descripcion, " &
                "NULL AS Fecha_Referencia, 'Media' AS Prioridad " &
                "FROM Entidades_Legales e INNER JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
                "WHERE p.Nivel_Riesgo = 'Alto' AND (p.Origen_Fondos_Verificado = 0 " &
                "OR p.Fecha_Verificacion_Listas IS NULL OR p.Fecha_Verificacion_Listas < DATE_SUB(CURDATE(), INTERVAL 1 YEAR))")
        End If

        If Permisos.TieneAcceso("protocolo.ver") Then
            consultas.Add(
                "SELECT 'Protocolo' AS Tipo, " &
                "CONCAT('Libro vencido sin cerrar: ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)) AS Descripcion, " &
                "l.Fecha_Vencimiento AS Fecha_Referencia, 'Alta' AS Prioridad " &
                "FROM Libros_Protocolo l WHERE l.Estado = 'Vigente' AND l.Fecha_Vencimiento < CURDATE()")
            consultas.Add(
                "SELECT 'Protocolo' AS Tipo, " &
                "CONCAT('Libro por vencer: ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)) AS Descripcion, " &
                "l.Fecha_Vencimiento AS Fecha_Referencia, 'Media' AS Prioridad " &
                "FROM Libros_Protocolo l WHERE l.Estado = 'Vigente' AND l.Fecha_Vencimiento " &
                "BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 30 DAY)")
            consultas.Add(
                "SELECT 'Protocolo' AS Tipo, " &
                "CONCAT('Entrega al Notariado pendiente: libro ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)) AS Descripcion, " &
                "DATE_ADD(l.Fecha_Vencimiento, INTERVAL 15 DAY) AS Fecha_Referencia, 'Media' AS Prioridad " &
                "FROM Libros_Protocolo l WHERE l.Estado = 'Cerrado' AND l.Fecha_Entrega_Notariado IS NULL " &
                "AND DATE_ADD(l.Fecha_Vencimiento, INTERVAL 15 DAY) <= CURDATE()")
        End If

        If Permisos.TieneAcceso("cumplimiento.ver") OrElse
           Permisos.TieneAcceso("cumplimiento.reportar_uif") Then
            consultas.Add(
                "SELECT 'UIF vencido' AS Tipo, " &
                "CONCAT('Reporte UIF vencido desde ', DATE_FORMAT(o.Fecha_Vencimiento_Reporte, '%d/%m/%Y')) AS Descripcion, " &
                "o.Fecha_Vencimiento_Reporte AS Fecha_Referencia, 'Alta' AS Prioridad " &
                "FROM Operaciones_LDAFT o WHERE o.Reportado_UIF = 0 " &
                "AND o.Estado IN ('Registrada', 'En análisis') AND o.Fecha_Vencimiento_Reporte IS NOT NULL " &
                "AND o.Fecha_Vencimiento_Reporte < CURDATE()")
            consultas.Add(
                "SELECT 'UIF por vencer' AS Tipo, " &
                "CONCAT('Reporte UIF pendiente (vence ', DATE_FORMAT(o.Fecha_Vencimiento_Reporte, '%d/%m/%Y'), ')') AS Descripcion, " &
                "o.Fecha_Vencimiento_Reporte AS Fecha_Referencia, 'Alta' AS Prioridad " &
                "FROM Operaciones_LDAFT o WHERE o.Reportado_UIF = 0 " &
                "AND o.Estado IN ('Registrada', 'En análisis') " &
                "AND o.Fecha_Vencimiento_Reporte BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 3 DAY)")
        End If

        If Permisos.TieneAcceso("cumplimiento.ver") OrElse
           Permisos.TieneAcceso("cumplimiento.arco") Then
            consultas.Add(
                "SELECT 'ARCO vencida' AS Tipo, " &
                "CONCAT('Solicitud ARCO vencida: ', a.Tipo_Derecho) AS Descripcion, " &
                "IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) AS Fecha_Referencia, " &
                "'Alta' AS Prioridad FROM Solicitudes_ARCO a " &
                "WHERE a.Estado = 'Pendiente' AND a.Fecha_Respuesta IS NULL " &
                "AND IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) < CURDATE()")
            consultas.Add(
                "SELECT 'ARCO por vencer' AS Tipo, " &
                "CONCAT('Solicitud ARCO por vencer: ', a.Tipo_Derecho) AS Descripcion, " &
                "IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) AS Fecha_Referencia, " &
                "'Alta' AS Prioridad FROM Solicitudes_ARCO a " &
                "WHERE a.Estado = 'Pendiente' AND a.Fecha_Respuesta IS NULL " &
                "AND IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) " &
                "BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 3 DAY)")
        End If

        If consultas.Count = 0 Then Return New DataTable()
        Return Consultar(String.Join(" UNION ALL ", consultas) &
                         " ORDER BY CASE WHEN Prioridad = 'Alta' THEN 1 ELSE 2 END, Fecha_Referencia")
    End Function

    Public Function ObtenerResumen() As String
        If Sesion.UsuarioActual Is Nothing Then Return ""

        Dim columnas As New List(Of String)()
        Dim etiquetas As New List(Of String)()

        If Permisos.TieneAcceso("clientes.ver") Then
            columnas.Add("(SELECT COUNT(*) FROM Clientes WHERE Activo = 1) AS Clientes")
            etiquetas.Add("Clientes activos")
        End If
        If Permisos.TieneAcceso("casos.ver") Then
            columnas.Add("(SELECT COUNT(*) FROM Casos WHERE Estado = 'Activo') AS Casos")
            etiquetas.Add("Casos activos")
        End If
        If Permisos.TieneAcceso("protocolo.ver") Then
            columnas.Add("(SELECT COUNT(*) FROM Libros_Protocolo WHERE Estado = 'Vigente') AS Libros")
            etiquetas.Add("Libros vigentes")
        End If
        If Permisos.TieneAcceso("documentos.ver") Then
            columnas.Add("(SELECT COUNT(*) FROM Documentos) AS Documentos")
            etiquetas.Add("Documentos")
        End If
        If Permisos.TieneAcceso("auditoria.ver") Then
            columnas.Add("(SELECT COUNT(*) FROM Audit_Log) AS Eventos")
            etiquetas.Add("Eventos auditados")
        End If

        If columnas.Count = 0 Then Return ""
        Dim tabla As DataTable = Consultar("SELECT " & String.Join(", ", columnas))
        If tabla.Rows.Count = 0 Then Return ""

        Dim valores As New List(Of String)()
        For i As Integer = 0 To etiquetas.Count - 1
            valores.Add(etiquetas(i) & ": " & Convert.ToInt32(tabla.Rows(0)(i)).ToString())
        Next
        Return String.Join("   ·   ", valores)
    End Function

End Class
