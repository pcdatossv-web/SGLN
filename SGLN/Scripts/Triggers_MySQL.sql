/* ============================================================
   SGLN El Salvador - Candados de seguridad (MySQL 8.0+)

   Se ejecuta DESPUES de Esquema_MySQL.sql y como administrador:
     mysql -u root -p sgln_sv < Triggers_MySQL.sql

   Garantizan, incluso por SQL directo, que:
     - el rol Administrador no se elimina, desactiva ni renombra;
     - siempre queda al menos un usuario administrador activo;
     - los permisos de administración no se asignan a roles ordinarios;
     - Audit_Log es inmutable: la aplicación no puede editar ni borrar la bitácora;
     - la eliminación física de usuarios se ejecuta por procedimiento controlado.

   Va en archivo aparte porque el conector ADO no entiende DELIMITER:
   el esquema embebido se ejecuta sentencia por sentencia y esto no.
   ============================================================ */

DROP TRIGGER IF EXISTS trg_roles_protegido_bi;
DROP TRIGGER IF EXISTS trg_roles_protegido_bu;
DROP TRIGGER IF EXISTS trg_roles_protegido_bd;
DROP TRIGGER IF EXISTS trg_usuarios_admin_bu;
DROP TRIGGER IF EXISTS trg_usuarios_admin_bd;
DROP TRIGGER IF EXISTS trg_rol_permiso_admin_bi;
DROP TRIGGER IF EXISTS trg_rol_permiso_admin_bu;
DROP TRIGGER IF EXISTS trg_rol_permiso_admin_bd;
DROP TRIGGER IF EXISTS trg_audit_log_no_update;
DROP TRIGGER IF EXISTS trg_audit_log_no_delete;

/* Revoca permisos administrativos que pudieran haberse delegado antes
   de esta versión. El cambio se ejecuta como administrador SQL antes de
   activar los nuevos candados. */
DELETE rp
FROM Rol_Permiso rp
INNER JOIN Roles r ON r.ID = rp.ID_Rol
WHERE r.Protegido = 0
  AND rp.Codigo LIKE 'config.%';

DELIMITER $$

CREATE TRIGGER trg_roles_protegido_bi BEFORE INSERT ON Roles
FOR EACH ROW
BEGIN
    /* Los roles protegidos solo se crean durante la instalación inicial,
       antes de aplicar estos triggers. La cuenta de aplicación no puede
       insertar un segundo rol con privilegios de administrador. */
    IF NEW.Protegido = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No se permite crear roles protegidos desde la cuenta de aplicación.';
    END IF;
END$$

CREATE TRIGGER trg_roles_protegido_bu BEFORE UPDATE ON Roles
FOR EACH ROW
BEGIN
    /* El indicador Protegido define acceso total y nunca debe poder elevarse
       mediante un UPDATE ejecutado con las credenciales DML de la aplicación. */
    IF NOT (NEW.Protegido <=> OLD.Protegido) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El indicador de rol protegido no se puede modificar después de crear el rol.';
    END IF;

    IF OLD.Protegido = 1 AND (NEW.Activo = 0 OR NEW.Nombre <> OLD.Nombre) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El rol Administrador es protegido: no se puede desactivar ni renombrar.';
    END IF;
END$$

CREATE TRIGGER trg_roles_protegido_bd BEFORE DELETE ON Roles
FOR EACH ROW
BEGIN
    IF OLD.Protegido = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El rol Administrador es protegido y no se puede eliminar.';
    END IF;
END$$

CREATE TRIGGER trg_usuarios_admin_bu BEFORE UPDATE ON Usuarios
FOR EACH ROW
BEGIN
    DECLARE v_rol_protegido INT;
    DECLARE v_otros INT;

    /* Bloqueo común para serializar cambios sobre cuentas administradoras. */
    SELECT ID INTO v_rol_protegido FROM Roles WHERE Protegido = 1 LIMIT 1 FOR UPDATE;

    IF OLD.ID_Rol = v_rol_protegido
       AND (NOT (NEW.ID_Rol <=> OLD.ID_Rol) OR NEW.Activo = 0) THEN
        SELECT COUNT(*) INTO v_otros
        FROM Usuarios
        WHERE ID_Rol = v_rol_protegido AND Activo = 1 AND ID <> OLD.ID;

        IF v_otros = 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Debe existir al menos un usuario administrador activo.';
        END IF;
    END IF;
END$$

CREATE TRIGGER trg_usuarios_admin_bd BEFORE DELETE ON Usuarios
FOR EACH ROW
BEGIN
    DECLARE v_rol_protegido INT;
    DECLARE v_otros INT;

    /* Mismo bloqueo en eliminación: serializa la comprobación del último administrador. */
    SELECT ID INTO v_rol_protegido FROM Roles WHERE Protegido = 1 LIMIT 1 FOR UPDATE;

    IF OLD.ID_Rol = v_rol_protegido THEN
        SELECT COUNT(*) INTO v_otros
        FROM Usuarios
        WHERE ID_Rol = v_rol_protegido AND Activo = 1 AND ID <> OLD.ID;

        IF v_otros = 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'No se puede eliminar el último usuario administrador activo.';
        END IF;
    END IF;
END$$

CREATE TRIGGER trg_rol_permiso_admin_bi BEFORE INSERT ON Rol_Permiso
FOR EACH ROW
BEGIN
    DECLARE v_protegido TINYINT DEFAULT 0;

    SELECT Protegido INTO v_protegido
    FROM Roles WHERE ID = NEW.ID_Rol LIMIT 1;

    /* Todo el catálogo config.* queda reservado al rol protegido.
       Ningún rol ordinario puede obtener privilegios de configuración por SQL directo. */
    IF NEW.Codigo LIKE 'config.%'
       AND COALESCE(v_protegido, 0) <> 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Los permisos de configuración solo pueden pertenecer al rol Administrador protegido.';
    END IF;
END$$

CREATE TRIGGER trg_rol_permiso_admin_bu BEFORE UPDATE ON Rol_Permiso
FOR EACH ROW
BEGIN
    DECLARE v_protegido TINYINT DEFAULT 0;
    DECLARE v_nuevo_protegido TINYINT DEFAULT 0;

    SELECT Protegido INTO v_protegido FROM Roles WHERE ID = OLD.ID_Rol LIMIT 1;
    SELECT Protegido INTO v_nuevo_protegido FROM Roles WHERE ID = NEW.ID_Rol LIMIT 1;

    IF v_protegido = 1
       AND OLD.Codigo IN ('config.usuarios', 'config.roles')
       AND (NEW.ID_Rol <> OLD.ID_Rol OR NEW.Codigo <> OLD.Codigo) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No se pueden reasignar ni cambiar los permisos esenciales del Administrador.';
    END IF;

    IF NEW.Codigo LIKE 'config.%'
       AND COALESCE(v_nuevo_protegido, 0) <> 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Los permisos de administración solo pueden pertenecer al rol Administrador protegido.';
    END IF;
END$$

CREATE TRIGGER trg_rol_permiso_admin_bd BEFORE DELETE ON Rol_Permiso
FOR EACH ROW
BEGIN
    DECLARE v_protegido TINYINT;

    SELECT Protegido INTO v_protegido FROM Roles WHERE ID = OLD.ID_Rol LIMIT 1;

    IF v_protegido = 1 AND OLD.Codigo IN ('config.usuarios', 'config.roles') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El Administrador no puede perder los permisos de Usuarios ni de Roles.';
    END IF;
END$$

/* La bitácora es append-only: la aplicación solo puede consultar e insertar.
   UPDATE y DELETE se bloquean incluso si la cuenta conserva permisos DML generales. */
CREATE TRIGGER trg_audit_log_no_update BEFORE UPDATE ON Audit_Log
FOR EACH ROW
BEGIN
    SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Audit_Log es inmutable: no se permite modificar registros.';
END$$

CREATE TRIGGER trg_audit_log_no_delete BEFORE DELETE ON Audit_Log
FOR EACH ROW
BEGIN
    SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Audit_Log es inmutable: no se permite eliminar registros.';
END$$


/* La eliminación física de usuarios se centraliza en un procedimiento
   DEFINER: la cuenta sgln_app no recibe DELETE sobre Usuarios. El procedimiento
   vuelve a comprobar historial, referencias y último administrador en MySQL,
   sin depender exclusivamente de las validaciones de la interfaz. */
DROP PROCEDURE IF EXISTS sp_EliminarUsuarioSeguro$$

CREATE PROCEDURE sp_EliminarUsuarioSeguro(IN p_id INT, IN p_actor_id INT)
SQL SECURITY DEFINER
MODIFIES SQL DATA
BEGIN
    DECLARE v_usuario VARCHAR(50);
    DECLARE v_nombre VARCHAR(150);
    DECLARE v_actor VARCHAR(50);
    DECLARE v_protegido TINYINT DEFAULT 0;
    DECLARE v_total INT DEFAULT 0;
    DECLARE v_fin INT DEFAULT 0;
    DECLARE v_tabla VARCHAR(64);
    DECLARE v_columna VARCHAR(64);

    DECLARE cur_fk CURSOR FOR
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA = DATABASE()
          AND REFERENCED_TABLE_NAME = 'Usuarios'
          AND REFERENCED_COLUMN_NAME = 'ID';

    DECLARE cur_referencias CURSOR FOR
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME NOT IN ('Usuarios', 'Audit_Log', 'Intentos_Acceso')
          AND DATA_TYPE IN ('char','varchar','tinytext','text','mediumtext','longtext')
          AND COLUMN_NAME REGEXP 'Usuario|Responsable|Creado|Modificado|Autor|Asignado|Propietario|Firmante|Confirma';

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_fin = 1;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    /* Igual que la antigua implementación de repositorio: bloquea rangos
       mientras se comprueba que no aparezcan referencias históricas concurrentes. */
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    START TRANSACTION;

    IF p_id IS NULL OR p_id <= 0 OR p_actor_id IS NULL OR p_actor_id <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Identificadores de usuario no válidos.';
    END IF;

    SELECT COUNT(*) INTO v_total FROM Usuarios WHERE ID = p_id;
    IF v_total = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario ya no existe.';
    END IF;

    SELECT u.Usuario, u.Nombre_Completo, COALESCE(r.Protegido, 0)
      INTO v_usuario, v_nombre, v_protegido
    FROM Usuarios u
    LEFT JOIN Roles r ON r.ID = u.ID_Rol
    WHERE u.ID = p_id
    FOR UPDATE;

    SELECT COUNT(*) INTO v_total
    FROM Usuarios u
    INNER JOIN Roles r ON r.ID = u.ID_Rol
    WHERE u.ID = p_actor_id AND u.Activo = 1 AND r.Protegido = 1;

    IF v_total = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La operación requiere un administrador activo.';
    END IF;

    IF p_actor_id = p_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede eliminar la cuenta administradora en uso.';
    END IF;

    SELECT u.Usuario INTO v_actor
    FROM Usuarios u
    INNER JOIN Roles r ON r.ID = u.ID_Rol
    WHERE u.ID = p_actor_id AND u.Activo = 1 AND r.Protegido = 1
    FOR UPDATE;

    SELECT COUNT(*) INTO v_total
    FROM Intentos_Acceso WHERE Usuario_ID = p_id;
    IF v_total > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta tiene historial de intentos de acceso.';
    END IF;

    IF v_protegido = 1 THEN
        SELECT COUNT(*) INTO v_total
        FROM Usuarios u
        INNER JOIN Roles r ON r.ID = u.ID_Rol
        WHERE r.Protegido = 1 AND u.Activo = 1 AND u.ID <> p_id;
        IF v_total = 0 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe quedar al menos un administrador activo.';
        END IF;
    END IF;

    SELECT COUNT(*) INTO v_total
    FROM Audit_Log
    WHERE (Usuario = v_usuario OR Usuario = v_nombre)
      AND NOT (Tabla_Afectada = 'Usuarios' AND Accion = 'CREAR' AND ID_Registro = p_id);
    IF v_total > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta tiene actividad registrada en la bitácora.';
    END IF;

    SELECT COUNT(*) INTO v_total
    FROM Audit_Log
    WHERE Tabla_Afectada = 'Usuarios' AND ID_Registro = p_id AND Accion <> 'CREAR';
    IF v_total > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta tiene historial de mantenimiento en la bitácora.';
    END IF;

    OPEN cur_fk;
    fk_loop: LOOP
        FETCH cur_fk INTO v_tabla, v_columna;
        IF v_fin = 1 THEN
            LEAVE fk_loop;
        END IF;

        SET @sgln_delete_sql = CONCAT(
            'SELECT COUNT(*) INTO @sgln_delete_count FROM `',
            REPLACE(v_tabla, '`', '``'), '` WHERE `',
            REPLACE(v_columna, '`', '``'), '` = ?');
        SET @sgln_delete_id = p_id;
        PREPARE sgln_delete_stmt FROM @sgln_delete_sql;
        EXECUTE sgln_delete_stmt USING @sgln_delete_id;
        DEALLOCATE PREPARE sgln_delete_stmt;

        IF COALESCE(@sgln_delete_count, 0) > 0 THEN
            CLOSE cur_fk;
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'La cuenta tiene registros relacionados en otras tablas.';
        END IF;
    END LOOP;
    CLOSE cur_fk;

    SET v_fin = 0;
    OPEN cur_referencias;
    referencias_loop: LOOP
        FETCH cur_referencias INTO v_tabla, v_columna;
        IF v_fin = 1 THEN
            LEAVE referencias_loop;
        END IF;

        SET @sgln_delete_sql = CONCAT(
            'SELECT COUNT(*) INTO @sgln_delete_count FROM `',
            REPLACE(v_tabla, '`', '``'), '` WHERE CAST(`',
            REPLACE(v_columna, '`', '``'), '` AS CHAR) = ? OR CAST(`',
            REPLACE(v_columna, '`', '``'), '` AS CHAR) = ?');
        SET @sgln_delete_usuario = v_usuario;
        SET @sgln_delete_nombre = v_nombre;
        PREPARE sgln_delete_stmt FROM @sgln_delete_sql;
        EXECUTE sgln_delete_stmt USING @sgln_delete_usuario, @sgln_delete_nombre;
        DEALLOCATE PREPARE sgln_delete_stmt;

        IF COALESCE(@sgln_delete_count, 0) > 0 THEN
            CLOSE cur_referencias;
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'La cuenta tiene referencias textuales históricas en otras tablas.';
        END IF;
    END LOOP;
    CLOSE cur_referencias;

    DELETE FROM Usuarios WHERE ID = p_id;
    INSERT INTO Audit_Log (Usuario, Accion, Tabla_Afectada, ID_Registro, Detalle)
    VALUES (v_actor, 'ELIMINAR', 'Usuarios', p_id,
            CONCAT('Cuenta eliminada tras validar historial y referencias: ', v_usuario));

    COMMIT;
END$$

DELIMITER ;
