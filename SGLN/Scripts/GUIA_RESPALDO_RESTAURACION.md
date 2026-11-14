# Operación de respaldos y recuperación de SGLN

## Alcance

SGLN genera respaldos SQL mediante `mysqldump`. La aplicación **no incluye una función de restauración**. La recuperación debe ejecutarla un administrador de MySQL siguiendo este procedimiento.

La base de datos de producción es `sgln_sv`. Las pruebas automatizadas deben usar exclusivamente `sgln_sv_pruebas`; nunca se debe recrear ni limpiar `sgln_sv` como parte de una prueba.

## Crear y proteger un respaldo

1. En SGLN, use la función de respaldo con una cuenta autorizada.
2. Confirme que la operación terminó correctamente y que el archivo SQL existe y tiene tamaño mayor que cero.
3. Compruebe el archivo desde PowerShell:

   ```powershell
   Get-Item 'D:\Respaldos\SGLN_YYYYMMDD_HHMMSS.sql' |
       Select-Object FullName, Length, LastWriteTime
   Get-FileHash 'D:\Respaldos\SGLN_YYYYMMDD_HHMMSS.sql' -Algorithm SHA256
   ```

4. Guarde el hash junto al registro del respaldo, pero separado del archivo.
5. Copie el respaldo a un medio o ubicación secundaria cifrada, con acceso limitado. Los SQL contienen datos personales, expedientes y bitácoras; no deben publicarse ni enviarse sin protección.
6. Mantenga varias generaciones de respaldos. No considere suficiente una copia guardada únicamente en el mismo disco del servidor.

El respaldo usa `--single-transaction` y la aplicación ahora comprueba que todas las tablas base sean InnoDB antes de iniciarlo. Evite cambios de esquema durante la copia.

## Ensayo de recuperación

Realice ensayos periódicos en **otro servidor o instancia MySQL aislada**, sin conexión de las estaciones de trabajo de producción. El archivo generado incluye el nombre de base `sgln_sv`; por eso el ensayo debe disponer de una instancia separada donde pueda restaurarse ese nombre sin tocar el servidor productivo.

1. Verifique el SHA-256 contra el hash registrado.
2. Use una cuenta administrativa de MySQL con privilegios para crear/modificar tablas. La cuenta `sgln_backup` es solo para exportar y `sgln_app` no tiene privilegios DDL suficientes para restaurar.
3. Configure un archivo de opciones de cliente MySQL protegido por ACL de Windows (solo el operador, SYSTEM y Administradores). No escriba contraseñas en la línea de comandos ni en scripts versionados.
4. Restaure el SQL en la instancia aislada. Ejemplo de ejecución desde PowerShell, con rutas y archivo de credenciales ajustados al servidor de ensayo:

   ```powershell
   $mysql = 'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe'
   $credenciales = 'C:\RutaProtegida\mysql-admin.cnf'
   $ca = 'C:\RutaProtegida\ca.pem'
   $respaldo = 'D:\Respaldos\SGLN_YYYYMMDD_HHMMSS.sql'

   $info = New-Object System.Diagnostics.ProcessStartInfo
   $info.FileName = $mysql
   $info.Arguments = '--defaults-extra-file="' + $credenciales +
       '" --ssl-mode=VERIFY_CA --ssl-ca="' + $ca +
       '" --default-character-set=utf8mb4'
   $info.UseShellExecute = $false
   $info.CreateNoWindow = $false

   $proceso = [System.Diagnostics.Process]::Start($info)
   try {
       $entrada = [System.IO.File]::OpenRead($respaldo)
       try {
           $entrada.CopyTo($proceso.StandardInput.BaseStream)
           $proceso.StandardInput.Close()
           $proceso.WaitForExit()
           if ($proceso.ExitCode -ne 0) {
               throw "La restauración de ensayo falló. Código: $($proceso.ExitCode)"
           }
       }
       finally {
           $entrada.Dispose()
       }
   }
   finally {
       $proceso.Dispose()
   }
   ```

5. Verifique que existen las tablas esperadas, que los conteos y muestras de registros coinciden con el origen, que las relaciones y triggers están presentes y que SGLN puede iniciar sesión y consultar la información.
6. Registre fecha, operador, archivo, hash, duración, resultado y problemas encontrados.

## Recuperación en producción

La restauración sobrescribe objetos de la base y puede reemplazar datos posteriores a la fecha del respaldo. No la ejecute como una operación rutinaria ni desde la cuenta de la aplicación.

1. Autorice formalmente la recuperación y determine el punto de restauración.
2. Detenga SGLN en todas las estaciones y confirme que no hay conexiones de usuarios modificando datos.
3. Genere un respaldo de contingencia del estado actual de `sgln_sv` antes de restaurar.
4. Verifique el hash del respaldo elegido y, preferiblemente, que ya pasó un ensayo de recuperación.
5. Ejecute la restauración con una cuenta administrativa de MySQL, conexión TLS validada y el archivo de credenciales protegido.
6. Verifique estructura, usuarios, bitácora `Audit_Log`, intentos de acceso, documentos y registros críticos. Compruebe que las rutas externas de documentos y respaldos estén disponibles; el SQL no contiene esos archivos externos.
7. Inicie SGLN y haga pruebas funcionales controladas antes de habilitar a los usuarios.
8. Documente la recuperación y conserve el respaldo de contingencia hasta cerrar la validación.

## Límites y advertencias

- La restauración **no está automatizada en SGLN** y no debe añadirse sin controles de autorización, validación, respaldo previo, modo mantenimiento, verificación y recuperación ante fallo.
- Un respaldo SQL no sustituye las copias de los archivos externos almacenados en las carpetas de documentos.
- La existencia y el tamaño del archivo no demuestran por sí solos que pueda restaurarse; solo un ensayo de recuperación lo confirma.
- `sgln_sv_pruebas` es desechable y nunca debe contener la única copia de datos reales.
