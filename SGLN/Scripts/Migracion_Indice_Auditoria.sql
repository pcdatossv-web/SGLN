/* SGLN - índice compuesto para la consulta paginada de Audit_Log.
   Ejecutar como administrador MySQL con sgln_sv seleccionado.
   Es aditiva respecto de los datos; puede ejecutarse varias veces. */

SET @sgln_audit_fecha_id := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Audit_Log'
      AND INDEX_NAME = 'IX_Audit_Fecha_ID'
);

SET @sgln_sql_audit_fecha_id := IF(
    @sgln_audit_fecha_id = 0,
    'CREATE INDEX IX_Audit_Fecha_ID ON Audit_Log (Fecha_Hora, ID)',
    'SELECT 1'
);
PREPARE stmt_audit_fecha_id FROM @sgln_sql_audit_fecha_id;
EXECUTE stmt_audit_fecha_id;
DEALLOCATE PREPARE stmt_audit_fecha_id;

SET @sgln_audit_fecha := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Audit_Log'
      AND INDEX_NAME = 'IX_Audit_Fecha'
);

SET @sgln_sql_audit_fecha := IF(
    @sgln_audit_fecha > 0,
    'DROP INDEX IX_Audit_Fecha ON Audit_Log',
    'SELECT 1'
);
PREPARE stmt_audit_fecha FROM @sgln_sql_audit_fecha;
EXECUTE stmt_audit_fecha;
DEALLOCATE PREPARE stmt_audit_fecha;

SET @sgln_audit_fecha_id := NULL;
SET @sgln_sql_audit_fecha_id := NULL;
SET @sgln_audit_fecha := NULL;
SET @sgln_sql_audit_fecha := NULL;
