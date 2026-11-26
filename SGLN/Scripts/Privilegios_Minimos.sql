/* SGLN - privilegios mínimos para la cuenta de aplicación.
   Ejecutar como root DESPUÉS de Esquema_MySQL.sql y Triggers_MySQL.sql.
   Para cada cliente remoto, use una copia de este archivo y reemplace
   'localhost' por la IP exacta autorizada. No use el host '%'. */

REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'sgln_app'@'localhost';

GRANT SELECT, INSERT, UPDATE ON sgln_sv.Roles TO 'sgln_app'@'localhost';
GRANT SELECT ON sgln_sv.Permisos TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Rol_Permiso TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Usuarios TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Intentos_Acceso TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Configuracion TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Feriados TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Juzgados TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Tipos_Actuacion TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Clientes TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Entidades_Legales TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Representantes_Legales TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Perfil_LDAFT TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Casos TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Actuaciones_Procesales TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Libros_Protocolo TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Instrumentos TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Comparecientes_Instrumento TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Testimonios_Expedidos TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT ON sgln_sv.Documentos TO 'sgln_app'@'localhost';
/* La aplicación solo puede rectificar metadatos documentales.
   Ruta, hash, fecha de carga y usuario quedan protegidos contra UPDATE directo. */
REVOKE UPDATE ON sgln_sv.Documentos FROM 'sgln_app'@'localhost';
GRANT UPDATE (Tipo_Documento, ID_Caso, ID_Instrumento)
    ON sgln_sv.Documentos TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Operaciones_LDAFT TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Solicitudes_ARCO TO 'sgln_app'@'localhost';
GRANT SELECT, INSERT, UPDATE ON sgln_sv.Plantillas_Documentos TO 'sgln_app'@'localhost';

/* Audit_Log es de solo lectura e inserción: no se conceden UPDATE ni DELETE.
   Los intentos de acceso conservan historial; tampoco se concede DELETE ahí. */
GRANT SELECT, INSERT ON sgln_sv.Audit_Log TO 'sgln_app'@'localhost';

/* DELETE solo se permite en tablas cuyos repositorios tienen una operación
   física controlada. Usuarios se excluye: solo se elimina por procedimiento.
   Las demás entidades usan baja lógica o conservación. */
GRANT DELETE ON sgln_sv.Roles TO 'sgln_app'@'localhost';
GRANT DELETE ON sgln_sv.Rol_Permiso TO 'sgln_app'@'localhost';
GRANT DELETE ON sgln_sv.Feriados TO 'sgln_app'@'localhost';
GRANT DELETE ON sgln_sv.Representantes_Legales TO 'sgln_app'@'localhost';
GRANT DELETE ON sgln_sv.Comparecientes_Instrumento TO 'sgln_app'@'localhost';

/* La eliminación de usuarios solo se ejecuta mediante el procedimiento
   DEFINER, que vuelve a validar historial y referencias en el servidor. */
GRANT EXECUTE ON PROCEDURE sgln_sv.sp_EliminarUsuarioSeguro TO 'sgln_app'@'localhost';

FLUSH PRIVILEGES;
