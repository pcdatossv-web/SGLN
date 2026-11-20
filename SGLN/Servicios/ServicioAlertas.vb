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
                "a.Fecha_Vencimiento_Calculada AS Fecha_Referencia, a.ID_Caso AS ID_Referencia, 'Alta' AS Prioridad " &
                "FROM Actuaciones_Procesales a " &
                "INNER JOIN Casos c ON c.ID = a.ID_Caso " &
                "INNER JOIN Tipos_Actuacion t ON t.ID = a.ID_Tipo_Actuacion " &
                "WHERE a.Estado IN ('Pendiente', 'Vencido') AND a.Fecha_Vencimiento_Calculada IS NOT NULL " &
                "AND a.Fecha_Vencimiento_Calculada < CURDATE()")
            consultas.Add(
                "SELECT 'Plazo por vencer' AS Tipo, " &
                "CONCAT('Actuación por vencer: ', t.Nombre, ' - Exp. ', c.Numero_Expediente) AS Descripcion, " &
                "a.Fecha_Vencimiento_Calculada AS Fecha_Referencia, a.ID_Caso AS ID_Referencia, 'Alta' AS Prioridad " &
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
                "NULL AS Fecha_Referencia, c.ID AS ID_Referencia, 'Media' AS Prioridad " &
                "FROM Clientes c INNER JOIN Perfil_LDAFT p ON p.ID_Cliente = c.ID " &
                "WHERE p.Nivel_Riesgo = 'Alto' AND (p.Origen_Fondos_Verificado = 0 " &
                "OR p.Fecha_Verificacion_Listas IS NULL OR p.Fecha_Verificacion_Listas < DATE_SUB(CURDATE(), INTERVAL 1 YEAR))")
        End If

        If Permisos.TieneAcceso("entidades.ver") Then
            consultas.Add(
                "SELECT 'LDA/FT' AS Tipo, " &
                "CONCAT('Perfil de alto riesgo incompleto (entidad): ', e.Razon_Social) AS Descripcion, " &
                "NULL AS Fecha_Referencia, e.ID AS ID_Referencia, 'Media' AS Prioridad " &
                "FROM Entidades_Legales e INNER JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
                "WHERE p.Nivel_Riesgo = 'Alto' AND (p.Origen_Fondos_Verificado = 0 " &
                "OR p.Fecha_Verificacion_Listas IS NULL OR p.Fecha_Verificacion_Listas < DATE_SUB(CURDATE(), INTERVAL 1 YEAR))")
        End If

        If Permisos.TieneAcceso("protocolo.ver") Then
            consultas.Add(
                "SELECT 'Protocolo' AS Tipo, " &
                "CONCAT('Libro vencido sin cerrar: ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)) AS Descripcion, " &
                "l.Fecha_Vencimiento AS Fecha_Referencia, l.ID AS ID_Referencia, 'Alta' AS Prioridad " &
                "FROM Libros_Protocolo l WHERE l.Estado = 'Vigente' AND l.Fecha_Vencimiento < CURDATE()")
            consultas.Add(
                "SELECT 'Protocolo' AS Tipo, " &
                "CONCAT('Libro por vencer: ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)) AS Descripcion, " &
                "l.Fecha_Vencimiento AS Fecha_Referencia, l.ID AS ID_Referencia, 'Media' AS Prioridad " &
                "FROM Libros_Protocolo l WHERE l.Estado = 'Vigente' AND l.Fecha_Vencimiento " &
                "BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 30 DAY)")
            consultas.Add(
                "SELECT 'Protocolo' AS Tipo, " &
                "CONCAT('Entrega al Notariado pendiente: libro ', CAST(l.Numero_Libro AS CHAR), '/', CAST(l.Anio_Vigencia AS CHAR)) AS Descripcion, " &
                "l.Fecha_Cierre AS Fecha_Referencia, l.ID AS ID_Referencia, 'Media' AS Prioridad " &
                "FROM Libros_Protocolo l WHERE l.Estado = 'Cerrado' AND l.Fecha_Entrega_Notariado IS NULL " &
                "AND l.Fecha_Cierre IS NOT NULL")
        End If

        ' Las alertas UIF y ARCO se agregan por separado para no exponer
        ' datos de una categoría a usuarios con permisos exclusivos de la otra.
        If Permisos.TieneAcceso("cumplimiento.ver") OrElse
           Permisos.TieneAcceso("cumplimiento.reportar_uif") Then
            consultas.Add(
                "SELECT 'UIF vencido' AS Tipo, " &
                "CONCAT('Reporte UIF vencido desde ', DATE_FORMAT(o.Fecha_Vencimiento_Reporte, '%d/%m/%Y')) AS Descripcion, " &
                "o.Fecha_Vencimiento_Reporte AS Fecha_Referencia, o.ID AS ID_Referencia, 'Alta' AS Prioridad " &
                "FROM Operaciones_LDAFT o WHERE o.Reportado_UIF = 0 " &
                "AND o.Estado IN ('Registrada', 'En análisis') AND o.Fecha_Vencimiento_Reporte IS NOT NULL " &
                "AND o.Fecha_Vencimiento_Reporte < CURDATE()")
            consultas.Add(
                "SELECT 'UIF por vencer' AS Tipo, " &
                "CONCAT('Reporte UIF pendiente (vence ', DATE_FORMAT(o.Fecha_Vencimiento_Reporte, '%d/%m/%Y'), ')') AS Descripcion, " &
                "o.Fecha_Vencimiento_Reporte AS Fecha_Referencia, o.ID AS ID_Referencia, 'Alta' AS Prioridad " &
                "FROM Operaciones_LDAFT o WHERE o.Reportado_UIF = 0 " &
                "AND o.Estado IN ('Registrada', 'En análisis') " &
                "AND o.Fecha_Vencimiento_Reporte BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 3 DAY)")
        End If

        If Permisos.TieneAcceso("cumplimiento.ver") OrElse
           Permisos.TieneAcceso("cumplimiento.arco") Then
            consultas.Add(
                "SELECT 'ARCO vencida' AS Tipo, " &
                "CONCAT('Solicitud ARCO vencida: ', a.Tipo_Derecho) AS Descripcion, " &
                "IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) AS Fecha_Referencia, a.ID AS ID_Referencia, " &
                "'Alta' AS Prioridad FROM Solicitudes_ARCO a " &
                "WHERE a.Estado = 'Pendiente' AND a.Fecha_Respuesta IS NULL " &
                "AND IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) < CURDATE()")
            consultas.Add(
                "SELECT 'ARCO revisar prórroga' AS Tipo, " &
                "CONCAT('Solicitud ARCO con prórroga sin fecha de vencimiento (ID ', CAST(a.ID AS CHAR), ')') AS Descripcion, " &
                "a.Fecha_Vencimiento AS Fecha_Referencia, a.ID AS ID_Referencia, 'Alta' AS Prioridad " &
                "FROM Solicitudes_ARCO a WHERE a.Estado = 'Pendiente' AND a.Fecha_Respuesta IS NULL " &
                "AND a.Prorroga_Aplicada = 1 AND a.Fecha_Vencimiento_Prorroga IS NULL")
            consultas.Add(
                "SELECT 'ARCO por vencer' AS Tipo, " &
                "CONCAT('Solicitud ARCO por vencer: ', a.Tipo_Derecho) AS Descripcion, " &
                "IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) AS Fecha_Referencia, a.ID AS ID_Referencia, " &
                "'Alta' AS Prioridad FROM Solicitudes_ARCO a " &
                "WHERE a.Estado = 'Pendiente' AND a.Fecha_Respuesta IS NULL " &
                "AND IF(a.Prorroga_Aplicada = 1, a.Fecha_Vencimiento_Prorroga, a.Fecha_Vencimiento) " &
                "BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 3 DAY)")
        End If

        If consultas.Count = 0 Then Return New DataTable()

        ' Limitar el volumen que se transfiere y enlaza al panel. Se consulta
        ' un registro adicional para poder advertir si existen más alertas.
        Dim resultado As DataTable = Consultar(String.Join(" UNION ALL ", consultas) &
            " ORDER BY Prioridad, Fecha_Referencia LIMIT 201")

        ' El plazo de entrega al Notariado se calcula en días hábiles y usa
        ' la configuración vigente y el calendario de feriados. No se puede
        ' aproximar con DATE_ADD(..., 15 DAY), porque generaría alertas falsas.
        If Permisos.TieneAcceso("protocolo.ver") Then
            Dim protocolo As New ServicioProtocolo()
            For indice As Integer = resultado.Rows.Count - 1 To 0 Step -1
                Dim fila As DataRow = resultado.Rows(indice)
                If Convert.ToString(fila("Tipo")) = "Protocolo" AndAlso
                   Convert.ToString(fila("Descripcion")).StartsWith(
                       "Entrega al Notariado pendiente:", StringComparison.Ordinal) Then
                    Try
                        Dim fechaCierre As Date = Convert.ToDateTime(fila("Fecha_Referencia")).Date
                        Dim fechaLimite As Date = protocolo.FechaLimiteEntrega(fechaCierre)
                        fila("Fecha_Referencia") = fechaLimite
                        If fechaLimite > Date.Today Then fila.Delete()
                    Catch ex As Exception
                        RegistroErrores.Escribir("Alertas - calcular plazo de entrega al Notariado", ex)
                        ' Ante una configuración o calendario ilegible se omite
                        ' esta alerta; nunca se muestra una fecha límite inventada.
                        fila.Delete()
                    End Try
                End If
            Next
            resultado.AcceptChanges()
        End If

        ' El panel muestra como máximo 200 alertas para evitar enlazar miles
        ' de filas históricas en cada actualización. La fila 201 solo indica
        ' que el resultado fue truncado; no se presenta al usuario.
        If resultado.Rows.Count > 200 Then
            For indice As Integer = resultado.Rows.Count - 1 To 200 Step -1
                resultado.Rows.RemoveAt(indice)
            Next
        End If

        Return resultado
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
