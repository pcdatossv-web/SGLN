/* ============================================================
   SGLN El Salvador - Instalacion del servidor MySQL 8.0.18+

   Ejecutar UNA SOLA VEZ como administrador (root) en el servidor:
     mysql -u root -p < Instalacion_Servidor_MySQL.sql

   Este script:
     1. Crea la base sgln_sv (utf8mb4).
     2. Crea el usuario sgln_app; los permisos se aplican por tabla después de crear el esquema.
     3. Crea el usuario de respaldos sgln_backup para mysqldump.
     4. Exige TLS para ambas cuentas; MySQL debe tener certificados activos.
      MySQL 8 genera ca.pem en el directorio de datos cuando TLS está disponible.
      Copie ca.pem a cada cliente y selecciónelo en la configuración de SGLN.
      VerifyFull también valida el nombre DNS/IP: el certificado del servidor
      debe incluir en SAN el mismo nombre o IP usado en el campo Servidor.

   Despues, en este orden y con el mismo root:
     mysql -u root -p sgln_sv < Esquema_MySQL.sql
     mysql -u root -p sgln_sv < Triggers_MySQL.sql
     mysql -u root -p sgln_sv < Privilegios_Minimos.sql

   Si sgln_sv YA existía antes de esta versión, ejecutar además UNA VEZ:
     mysql -u root -p sgln_sv < Migracion_Integridad_Clientes_LDAFT.sql
     mysql -u root -p sgln_sv < Migracion_Seguridad_Acceso.sql
   La migración de seguridad crea la tabla de control de intentos fallidos.
   Aplique también Privilegios_Minimos.sql para retirar permisos DELETE globales
   de cuentas sgln_app anteriores.

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
     ALTER USER 'sgln_app'@'192.168.1.25' REQUIRE SSL;
   Después de crear el esquema, copie Privilegios_Minimos.sql, sustituya
   en ese archivo 'localhost' por la IP exacta (por ejemplo '192.168.1.25')
   y ejecútelo como root. Repita para cada IP autorizada. No utilice '%'.
   Este instalador se ejecuta una sola vez; no vuelve a mostrar contraseñas
   de cuentas que ya existen.
   ------------------------------------------------------------------ */

DROP USER IF EXISTS 'sgln_app'@'%';
DROP USER IF EXISTS 'sgln_backup'@'%';

CREATE USER IF NOT EXISTS 'sgln_app'@'localhost';
ALTER USER 'sgln_app'@'localhost' IDENTIFIED BY RANDOM PASSWORD;
ALTER USER 'sgln_app'@'localhost' REQUIRE SSL;
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'sgln_app'@'localhost';
/* Los permisos de tabla se aplican después de crear el esquema mediante
   Scripts/Privilegios_Minimos.sql. No otorgar DELETE global ni DDL. */

/* ------------------------------------------------------------------
   Usuario exclusivo de respaldos. No puede modificar datos.
   Si el respaldo se ejecuta desde otro equipo, cree su cuenta con la IP
   exacta de ese equipo, exija SSL y otorgue los mismos privilegios:
     CREATE USER 'sgln_backup'@'192.168.1.25' IDENTIFIED BY RANDOM PASSWORD;
     ALTER USER 'sgln_backup'@'192.168.1.25' REQUIRE SSL;
     GRANT SELECT, SHOW VIEW, TRIGGER ON sgln_sv.* TO 'sgln_backup'@'192.168.1.25';
   ------------------------------------------------------------------ */

CREATE USER IF NOT EXISTS 'sgln_backup'@'localhost';
ALTER USER 'sgln_backup'@'localhost' IDENTIFIED BY RANDOM PASSWORD;
ALTER USER 'sgln_backup'@'localhost' REQUIRE SSL;
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'sgln_backup'@'localhost';
GRANT SELECT, SHOW VIEW, TRIGGER
    ON sgln_sv.* TO 'sgln_backup'@'localhost';
FLUSH PRIVILEGES;
