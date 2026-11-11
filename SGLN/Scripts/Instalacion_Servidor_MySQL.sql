/* ============================================================
   SGLN El Salvador - Instalacion del servidor MySQL 8.0

   Ejecutar UNA SOLA VEZ como administrador (root) en el servidor:
     mysql -u root -p < Instalacion_Servidor_MySQL.sql

   Este script:
     1. Crea la base sgln_sv (utf8mb4).
     2. Crea el usuario de la aplicacion sgln_app con permisos DML
        (SELECT, INSERT, UPDATE, DELETE).
     3. Crea el usuario de respaldos sgln_backup para mysqldump.

   Despues, en este orden y con el mismo root:
     mysql -u root -p sgln_sv < Esquema_MySQL.sql
     mysql -u root -p sgln_sv < Triggers_MySQL.sql

   Cambie las claves de abajo y repliquelas en App.config antes de
   instalar en el servidor real.
   ============================================================ */

CREATE DATABASE IF NOT EXISTS sgln_sv
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   Usuario de la aplicacion: solo datos, nunca estructura.
   ------------------------------------------------------------------ */

CREATE USER IF NOT EXISTS 'sgln_app'@'localhost' IDENTIFIED BY 'Abog@dos26';
CREATE USER IF NOT EXISTS 'sgln_app'@'%'         IDENTIFIED BY 'Abog@dos26';

GRANT SELECT, INSERT, UPDATE, DELETE ON sgln_sv.* TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON sgln_sv.* TO 'sgln_app'@'%';

/* ------------------------------------------------------------------
   Usuario de respaldos: mysqldump necesita leer estructura y bloqueos.
   No puede modificar datos.
   ------------------------------------------------------------------ */

CREATE USER IF NOT EXISTS 'sgln_backup'@'localhost' IDENTIFIED BY 'Abog@dos26';
CREATE USER IF NOT EXISTS 'sgln_backup'@'%'         IDENTIFIED BY 'Abog@dos26';

GRANT SELECT, LOCK TABLES, SHOW VIEW, EVENT, TRIGGER
    ON sgln_sv.* TO 'sgln_backup'@'localhost';
GRANT SELECT, LOCK TABLES, SHOW VIEW, EVENT, TRIGGER
    ON sgln_sv.* TO 'sgln_backup'@'%';

FLUSH PRIVILEGES;
