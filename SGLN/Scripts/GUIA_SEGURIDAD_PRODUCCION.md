# SGLN — Guía de seguridad para producción

## Controles incorporados

- El rol Administrador es protegido por la base de datos: no se puede eliminar, desactivar ni convertir en un rol ordinario mientras existan usuarios activos.
- Los permisos del espacio `config.*` se reservan al rol protegido. Al ejecutar `Triggers_MySQL.sql`, se eliminan asignaciones antiguas de esos permisos a roles ordinarios.
- La cuenta `sgln_app` solo puede consultar el catálogo `Permisos`; no puede insertar ni modificar códigos de permiso.
- La bitácora `Audit_Log` permite lectura e inserción, pero no actualización ni eliminación directa.
- La cuenta de aplicación no recibe `DELETE` sobre `Usuarios`. La eliminación excepcional se canaliza por `sp_EliminarUsuarioSeguro`, que vuelve a comprobar historial, referencias, intentos de acceso y conservación de otro Administrador activo dentro de una transacción serializable.
- Los servicios y repositorios vuelven a comprobar permisos para operaciones de usuarios, roles, configuración, respaldos y auditoría.
- La batería funcional usa una base de pruebas aislada y una cuenta SQL con los mismos permisos por tabla que producción.

## Límite importante del modelo de conexión actual

SGLN es una aplicación de escritorio que utiliza una cuenta MySQL compartida (`sgln_app`) desde los equipos cliente. MySQL ve a todos esos equipos como la misma identidad SQL; no conoce qué usuario interno de SGLN inició sesión.

Por eso, las comprobaciones de permisos por usuario se aplican en los servicios y repositorios de la aplicación. Los triggers protegen invariantes de la base, pero no pueden demostrar por sí solos quién es el operador de SGLN. En particular, `sp_EliminarUsuarioSeguro` recibe el identificador del administrador que la aplicación autenticó; ese identificador no es una prueba criptográfica para MySQL. Cualquier persona que obtenga las credenciales SQL compartidas podría invocar directamente el procedimiento y falsificar ese parámetro, además de ejecutar otras operaciones permitidas a `sgln_app`.

**No debe considerarse que la cuenta compartida ofrece aislamiento frente a un usuario que ya conoce sus credenciales SQL.** Para cerrar ese límite de forma sólida, la arquitectura de producción debe impedir conexiones MySQL directas desde estaciones de trabajo y colocar el acceso a datos detrás de una API/servicio de aplicación autenticado, o utilizar identidades SQL individuales que el cliente no pueda suplantar.

## Despliegue y recuperación

1. Ejecutar como administrador MySQL, en este orden: `Instalacion_Servidor_MySQL.sql`, `Esquema_MySQL.sql`, `Triggers_MySQL.sql` y `Privilegios_Minimos.sql`.
2. En bases ya existentes, aplicar las migraciones correspondientes y volver a ejecutar `Triggers_MySQL.sql` y `Privilegios_Minimos.sql` para instalar el procedimiento y retirar privilegios heredados.
3. Restringir el puerto MySQL a los hosts autorizados; no usar el host `%`. Mantener TLS con validación de CA e identidad del servidor.
4. No incluir contraseñas reales en el repositorio, scripts, archivos de configuración compartidos ni registros de error.
5. Los respaldos lógicos no incluyen las cuentas MySQL ni el procedimiento almacenado. Después de restaurar, volver a ejecutar `Triggers_MySQL.sql` y `Privilegios_Minimos.sql`, tal como indica `GUIA_RESPALDO_RESTAURACION.md`.
6. Ejecutar las pruebas con `Pruebas\\Ejecutar-Pruebas.cmd`. El arnés crea una base de pruebas separada; no debe apuntarse a `sgln_sv` de producción.

## Estado de validación

La compilación automatizada y la compilación del arnés funcional se validan mediante GitHub Actions. La batería funcional con MySQL debe ejecutarse en una instancia de pruebas real; una compilación correcta no equivale a una restauración probada ni a una certificación de producción.
