/* SGLN - Índice para búsquedas exactas por SHA-256 en Documentos.
   Ejecutar una vez como administrador MySQL en la base sgln_sv existente,
   después de haber instalado Esquema_MySQL.sql.

   Es una migración aditiva: no modifica ni elimina registros documentales.
   Puede ejecutarse más de una vez; comprueba previamente el índice. */

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

SET @sgln_indice_documentos_hash := NULL;
SET @sgln_sql_indice_documentos := NULL;
