/* ============================================================
   SGLN - Migración de integridad de perfiles LDA/FT
   Ejecutar UNA SOLA VEZ en bases existentes creadas antes de esta
   corrección, después de realizar un respaldo.

   No ejecutar en una base recién creada con el Esquema_MySQL.sql
   actualizado: esa versión ya incluye CK_Perfil_Dueno_Exclusivo.
   Requiere MySQL 8.0.18+.
   ============================================================ */

-- Primero revisar los registros que incumplen la regla de titular único.
-- Si esta consulta devuelve filas, corregirlas antes de continuar.
SELECT ID, ID_Cliente, ID_Entidad
FROM Perfil_LDAFT
WHERE (ID_Cliente IS NULL AND ID_Entidad IS NULL)
   OR (ID_Cliente IS NOT NULL AND ID_Entidad IS NOT NULL);

-- Cada perfil debe pertenecer a un cliente O a una entidad, nunca a ambos.
ALTER TABLE Perfil_LDAFT
    ADD CONSTRAINT CK_Perfil_Dueno_Exclusivo
    CHECK (
        (ID_Cliente IS NOT NULL AND ID_Entidad IS NULL)
        OR
        (ID_Cliente IS NULL AND ID_Entidad IS NOT NULL)
    );
