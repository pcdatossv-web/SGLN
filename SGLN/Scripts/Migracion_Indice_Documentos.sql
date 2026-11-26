/* SGLN - Índices para consultas del módulo Documentos.
   Ejecutar como administrador MySQL en sgln_sv después de instalar
   Esquema_MySQL.sql. Es aditiva, no modifica registros ni archivos y puede
   ejecutarse varias veces sin duplicar los índices. */

SET @sgln_indice_documentos_hash := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Documentos'
      AND INDEX_NAME = 'IX_Documentos_Hash'
);

SET @sgln_sql_indice_documentos := IF(
    @sgln_indice_documentos_hash = 0,
    'CREATE INDEX IX_Documentos_Hash ON Documentos (Hash)',
    'SELECT 1'
);
PREPARE stmt_indice_documentos FROM @sgln_sql_indice_documentos;
EXECUTE stmt_indice_documentos;
DEALLOCATE PREPARE stmt_indice_documentos;

SET @sgln_indice_documentos_fecha := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Documentos'
      AND INDEX_NAME = 'IX_Documentos_Fecha_ID'
);

SET @sgln_sql_indice_documentos_fecha := IF(
    @sgln_indice_documentos_fecha = 0,
    'CREATE INDEX IX_Documentos_Fecha_ID ON Documentos (Fecha_Carga, ID)',
    'SELECT 1'
);
PREPARE stmt_indice_documentos_fecha FROM @sgln_sql_indice_documentos_fecha;
EXECUTE stmt_indice_documentos_fecha;
DEALLOCATE PREPARE stmt_indice_documentos_fecha;

SET @sgln_indice_documentos_hash := NULL;
SET @sgln_sql_indice_documentos := NULL;
SET @sgln_indice_documentos_fecha := NULL;
SET @sgln_sql_indice_documentos_fecha := NULL;
