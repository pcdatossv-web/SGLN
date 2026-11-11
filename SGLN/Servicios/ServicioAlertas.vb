Imports System
Imports System.Data

''' <summary>
''' Alertas transversales del sistema: plazos, LDA/FT, protocolo, UIF y ARCO.
''' </summary>
Public Class ServicioAlertas
    Inherits RepositorioBase

    Public Function ObtenerAlertas() As DataTable
        Dim consulta As String =
            "SELECT 'Plazo vencido' AS Tipo, " &
            "       CONCAT('Actuación vencida: ', t.Nombre, ' - Exp. ', c.Numero_Expediente) AS Descripcion, " &
            "       a.Fecha_Vencimiento_Calculada AS Fecha_Referencia, 'Alta' AS Prioridad " &
            "FROM Actuaciones_Procesales a " &
            "INNER JOIN Casos c ON c.ID = a.ID_Caso " &
            "INNER JOIN Tipos_Actuacion t ON t.ID = a.ID_Tipo_Actuacion " &
            "WHERE a.Estado = 'Pendiente' AND a.Fecha_Vencimiento_Calculada IS NOT NULL " &
            "  AND a.Fecha_Vencimiento_Calculada < CURDATE() " &
            "UNION ALL " &
            "SELECT 'Plazo por vencer', " &
            "       CONCAT('Actuación por vencer: ', t.Nombre, ' - Exp. ', c.Numero_Expediente), " &
            "       a.Fecha_Vencimiento_Calculada, 'Alta' " &
            "FROM Actuaciones_Procesales a " &
            "INNER JOIN Casos c ON c.ID = a.ID_Caso " &
            "INNER JOIN Tipos_Actuacion t ON t.ID = a.ID_Tipo_Actuacion " &
            "WHERE a.Estado = 'Pendiente' AND a.Fecha_Vencimiento_Calculada " &
            "      BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 3 DAY) " &
            "UNION ALL " &
            "SELECT 'LDA/FT', " &
            "       CONCAT('Perfil de alto riesgo incompleto: ', c.Nombres, ' ', c.Apellidos), " &
            "       NULL, 'Media' " &
            "FROM Clientes c " &
            "INNER JOIN Perfil_LDAFT p ON p.ID_Cliente = c.ID " &
            "WHERE p.Nivel_Riesgo = 'Alto' " &
            "  AND (p.Origen_Fondos_Verificado = 0 OR p.Fecha_Verificacion_Listas IS NULL " &
            "       OR p.Fecha_Verificacion_Listas < DATE_SUB(CURDATE(), INTERVAL 1 YEAR)) " &
            "UNION ALL " &
            "SELECT 'LDA/FT', " &
            "       CONCAT('Perfil de alto riesgo incompleto (entidad): ', e.Razon_Social), " &
            "       NULL, 'Media' " &
            "FROM Entidades_Legales e " &
            "INNER JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
            "WHERE p.Nivel_Riesgo = 'Alto' " &
            "  AND (p.Origen_Fondos_Verificado = 0 OR p.Fecha_Verificacion_Listas IS NULL " &
            "       OR p.Fecha_Verificacion_Listas < DATE_SUB(CURDATE(), INTERVAL 1 YEAR)) " &
            "UNION ALL " &
            "SELECT 'Protocolo', " &
            "       CONCAT('Libro vencido sin cerrar: ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)), " &
            "       l.Fecha_Vencimiento, 'Alta' " &
            "FROM Libros_Protocolo l " &
            "WHERE l.Estado = 'Vigente' AND l.Fecha_Vencimiento < CURDATE() " &
            "UNION ALL " &
            "SELECT 'Protocolo', " &
            "       CONCAT('Libro por vencer: ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)), " &
            "       l.Fecha_Vencimiento, 'Media' " &
            "FROM Libros_Protocolo l " &
            "WHERE l.Estado = 'Vigente' AND l.Fecha_Vencimiento " &
            "      BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 30 DAY) " &
            "UNION ALL " &
            "SELECT 'UIF', " &
            "       CONCAT('Reporte UIF pendiente (vence ', DATE_FORMAT(o.Fecha_Vencimiento_Reporte, '%d/%m/%Y'), ')'), " &
            "       o.Fecha_Vencimiento_Reporte, 'Alta' " &
            "FROM Operaciones_LDAFT o " &
            "WHERE o.Reportado_UIF = 0 AND o.Fecha_Vencimiento_Reporte IS NOT NULL " &
            "  AND o.Fecha_Vencimiento_Reporte <= DATE_ADD(CURDATE(), INTERVAL 3 DAY) " &
            "UNION ALL " &
            "SELECT 'ARCO', " &
            "       CONCAT('Solicitud ARCO por vencer: ', a.Tipo_Derecho), " &
            "       COALESCE(a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento), 'Alta' " &
            "FROM Solicitudes_ARCO a " &
            "WHERE a.Estado = 'Pendiente' " &
            "  AND COALESCE(a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) <= DATE_ADD(CURDATE(), INTERVAL 3 DAY) " &
            "UNION ALL " &
            "SELECT 'Protocolo', " &
            "       CONCAT('Entrega al Notariado pendiente: libro ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)), " &
            "       DATE_ADD(l.Fecha_Vencimiento, INTERVAL 15 DAY), 'Media' " &
            "FROM Libros_Protocolo l " &
            "WHERE l.Estado = 'Cerrado' AND l.Fecha_Entrega_Notariado IS NULL " &
            "  AND DATE_ADD(l.Fecha_Vencimiento, INTERVAL 15 DAY) <= CURDATE() " &
            "ORDER BY CASE WHEN Prioridad = 'Alta' THEN 1 ELSE 2 END, Fecha_Referencia"

        Return Consultar(consulta)
    End Function

    Public Function ObtenerResumen() As String
        Dim tabla As DataTable = Consultar(
            "SELECT " &
            " (SELECT COUNT(*) FROM Clientes WHERE Activo = 1) AS Clientes, " &
            " (SELECT COUNT(*) FROM Casos WHERE Estado = 'Activo') AS Casos, " &
            " (SELECT COUNT(*) FROM Libros_Protocolo WHERE Estado = 'Vigente') AS Libros, " &
            " (SELECT COUNT(*) FROM Documentos) AS Documentos, " &
            " (SELECT COUNT(*) FROM Audit_Log) AS Eventos")

        If tabla.Rows.Count = 0 Then Return ""

        Dim fila As DataRow = tabla.Rows(0)
        Return String.Format(
            "Clientes activos: {0}   ·   Casos activos: {1}   ·   Libros vigentes: {2}   ·   Documentos: {3}   ·   Eventos auditados: {4}",
            Convert.ToInt32(fila("Clientes")),
            Convert.ToInt32(fila("Casos")),
            Convert.ToInt32(fila("Libros")),
            Convert.ToInt32(fila("Documentos")),
            Convert.ToInt32(fila("Eventos")))
    End Function

End Class
