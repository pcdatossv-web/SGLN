/* ============================================================
   SGLN El Salvador - Instalacion del servidor MySQL 8.0.18+

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

   Las cuentas se crean con contraseñas aleatorias que MySQL muestra en
   la salida de este script. Guarde esos valores en la configuración del
   cliente. Ejecute este instalador una sola vez: al repetirlo se rotan
   las contraseñas locales y deberá actualizar los clientes.
   ============================================================ */

CREATE DATABASE IF NOT EXISTS sgln_sv
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   Usuario de la aplicación: solo DML y limitado al propio servidor.
   MySQL 8.0.18+ genera una contraseña aleatoria al ejecutar ALTER USER y
   la muestra en la salida. Guárdela en Configurar servidor de base de datos.
   Para clientes remotos, cree una cuenta adicional usando la IP exacta:
     CREATE USER 'sgln_app'@'192.168.1.25' IDENTIFIED BY RANDOM PASSWORD;
     GRANT SELECT, INSERT, UPDATE, DELETE ON sgln_sv.* TO 'sgln_app'@'192.168.1.25';
   Repita con cada IP autorizada. No utilice el host comodín.
   Este instalador se ejecuta una sola vez; no vuelve a mostrar contraseñas
   de cuentas que ya existen.
   ------------------------------------------------------------------ */

DROP USER IF EXISTS 'sgln_app'@'%';
DROP USER IF EXISTS 'sgln_backup'@'%';

CREATE USER IF NOT EXISTS 'sgln_app'@'localhost';
ALTER USER 'sgln_app'@'localhost' IDENTIFIED BY RANDOM PASSWORD;
GRANT SELECT, INSERT, UPDATE, DELETE ON sgln_sv.* TO 'sgln_app'@'localhost';

/* ------------------------------------------------------------------
   Usuario exclusivo de respaldos. No puede modificar datos.
   Si el respaldo se ejecuta desde otro equipo, cree su cuenta con la IP
   exacta de ese equipo y los mismos privilegios de solo lectura.
   ------------------------------------------------------------------ */

CREATE USER IF NOT EXISTS 'sgln_backup'@'localhost';
ALTER USER 'sgln_backup'@'localhost' IDENTIFIED BY RANDOM PASSWORD;
GRANT SELECT, SHOW VIEW, TRIGGER
    ON sgln_sv.* TO 'sgln_backup'@'localhost';
FLUSH PRIVILEGES;
