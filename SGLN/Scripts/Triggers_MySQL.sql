/* ============================================================
   SGLN El Salvador - Candados de seguridad (MySQL 8.0+)

   Se ejecuta DESPUES de Esquema_MySQL.sql y como administrador:
     mysql -u root -p sgln_sv < Triggers_MySQL.sql

   Garantizan, incluso por SQL directo, que:
     - el rol Administrador no se elimina, desactiva ni renombra;
     - siempre queda al menos un usuario administrador activo;
     - el Administrador conserva los permisos de Usuarios y Roles.

   Va en archivo aparte porque el conector ADO no entiende DELIMITER:
   el esquema embebido se ejecuta sentencia por sentencia y esto no.
   ============================================================ */

DROP TRIGGER IF EXISTS trg_roles_protegido_bu;
DROP TRIGGER IF EXISTS trg_roles_protegido_bd;
DROP TRIGGER IF EXISTS trg_usuarios_admin_bu;
DROP TRIGGER IF EXISTS trg_usuarios_admin_bd;
DROP TRIGGER IF EXISTS trg_rol_permiso_admin_bd;

DELIMITER $$

CREATE TRIGGER trg_roles_protegido_bu BEFORE UPDATE ON Roles
FOR EACH ROW
BEGIN
    IF OLD.Protegido = 1 AND (NEW.Protegido = 0 OR NEW.Activo = 0 OR NEW.Nombre <> OLD.Nombre) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El rol Administrador es protegido: no se puede desactivar, renombrar ni desproteger.';
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

    SELECT ID INTO v_rol_protegido FROM Roles WHERE Protegido = 1 LIMIT 1;

    IF OLD.ID_Rol = v_rol_protegido
       AND (NEW.ID_Rol <> OLD.ID_Rol OR NEW.Activo = 0) THEN
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

    SELECT ID INTO v_rol_protegido FROM Roles WHERE Protegido = 1 LIMIT 1;

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

DELIMITER ;
