<#
.SYNOPSIS
    Compila y ejecuta la batería funcional del SGLN contra MySQL 8.0.

.DESCRIPTION
    Las pruebas usan la capa de Servicios real contra una base AISLADA
    (sgln_sv_pruebas). Nunca se conectan a la base de trabajo sgln_sv.

    El script:
      1. Localiza mysql.exe / mysqldump.exe.
      2. Recrea sgln_sv_pruebas y aplica Esquema_MySQL.sql y Triggers_MySQL.sql
         como root. La cuenta temporal de la aplicación recibe solo permisos
         DML sobre sgln_sv_pruebas; nunca recibe permisos DDL.
      3. Copia SGLN.exe y las DLL de lib, compila el arnés con vbc y lo ejecuta.

    La clave de root se toma de la variable de entorno MYSQL_ROOT_PASSWORD.
    Si no está definida, se pide de forma segura y solo se usa en un archivo
    temporal de credenciales que se borra al terminar.

.PARAMETER ConservarBase
    Reutiliza sgln_sv_pruebas sin recrearla. Solo útil para depurar un caso:
    la batería inserta datos fijos (DUI, expedientes) y puede chocar con los
    de una corrida anterior.

.EXAMPLE
    $env:MYSQL_ROOT_PASSWORD = 'locoprosv'
    .\Ejecutar-Pruebas.ps1
#>
[CmdletBinding()]
param(
    [switch]$ConservarBase
)

$ErrorActionPreference = 'Stop'

$carpeta = $PSScriptRoot
$raizSolucion = Split-Path -Parent $carpeta
$proyectoSgln = Join-Path $raizSolucion 'SGLN'
$ejecutableSgln = Join-Path $proyectoSgln 'bin\Release\SGLN.exe'
$solucion = Join-Path $raizSolucion 'SGLN.sln'
$configPruebas = Join-Path $carpeta 'PruebaFuncional.exe.config'
$carpetaBin = Join-Path $carpeta 'bin'
$carpetaLib = Join-Path $raizSolucion 'lib'
$carpetaScripts = Join-Path $proyectoSgln 'Scripts'
$basePruebas = 'sgln_sv_pruebas'

# ------------------------------------------------------------------
# 1. Compilación limpia y comprobaciones previas
# ------------------------------------------------------------------

# No reutilizar SGLN.exe de Debug: puede ser una compilación antigua y
# provocar errores engañosos al compilar el arnés (por ejemplo, métodos
# públicos que sí existen en el código fuente pero no en el EXE copiado).
$msbuild = (Get-Command 'MSBuild.exe' -ErrorAction SilentlyContinue).Source
if ([string]::IsNullOrWhiteSpace($msbuild)) {
    $vswhere = Join-Path $env:ProgramFiles(x86) 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $hallado = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($hallado) { $msbuild = $hallado }
    }
}
if ([string]::IsNullOrWhiteSpace($msbuild) -or -not (Test-Path $msbuild)) {
    throw 'No se encontró MSBuild. Instale Visual Studio o Build Tools con la carga de trabajo .NET de escritorio.'
}
if (-not (Test-Path $solucion)) {
    throw "No existe la solución $solucion."
}

Write-Host 'Compilando la solución SGLN en Release...' -ForegroundColor Cyan
& $msbuild $solucion /m /p:Configuration=Release '/p:Platform=Any CPU' /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
    throw "Falló la compilación de SGLN (MSBuild: $LASTEXITCODE). No se ejecutarán las pruebas."
}
if (-not (Test-Path $ejecutableSgln)) {
    throw "La compilación terminó sin generar $ejecutableSgln."
}
if (-not (Test-Path $configPruebas)) {
    throw "No existe $configPruebas."
}
if (-not (Test-Path (Join-Path $carpetaScripts 'Esquema_MySQL.sql'))) {
    throw "No existe Esquema_MySQL.sql en $carpetaScripts."
}
if (-not (Test-Path (Join-Path $carpetaScripts 'Triggers_MySQL.sql'))) {
    throw "No existe Triggers_MySQL.sql en $carpetaScripts."
}

# La base de pruebas debe ser la aislada: se valida antes de borrar nada.
$xmlConfig = [xml](Get-Content $configPruebas)
$cadena = $xmlConfig.configuration.connectionStrings.add.connectionString
if ($cadena -notmatch 'Database=sgln_sv_pruebas') {
    throw "La cadena de conexión de pruebas no apunta a sgln_sv_pruebas. Se cancela para no tocar datos de trabajo."
}

$servidor = 'localhost'
$puerto = '3306'
$usuarioApp = 'sgln_test_app'
if ($cadena -match 'Server=([^;]+)') { $servidor = $Matches[1].Trim() }
if ($cadena -match 'Port=([^;]+)') { $puerto = $Matches[1].Trim() }
if ($cadena -match 'Uid=([^;]+)') { $usuarioApp = $Matches[1].Trim() }

$usuarioRespaldo = 'sgln_test_backup'
foreach ($ajuste in $xmlConfig.configuration.appSettings.add) {
    if ($ajuste.key -eq 'Respaldos_Usuario') { $usuarioRespaldo = $ajuste.value }
}

# Credenciales aleatorias y exclusivas de esta base temporal. Nunca se reutilizan
# las cuentas de la instalación real ni se guardan en los archivos del repositorio.
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $bytesApp = New-Object byte[] 32
    $rng.GetBytes($bytesApp)
    $claveApp = ([BitConverter]::ToString($bytesApp) -replace '-', '') + 'aA1!'
    $bytesBackup = New-Object byte[] 32
    $rng.GetBytes($bytesBackup)
    $claveRespaldo = ([BitConverter]::ToString($bytesBackup) -replace '-', '') + 'bB2!'
}
finally {
    $rng.Dispose()
}

function Buscar-MySqlExe([string]$nombre) {
    $comando = Get-Command $nombre -ErrorAction SilentlyContinue
    if ($comando) { return $comando.Source }

    $candidatos = @(
        "C:\Program Files\MySQL\MySQL Server 8.0\bin\$nombre",
        "C:\Program Files\MySQL\MySQL Server 8.4\bin\$nombre",
        "C:\Program Files\MariaDB 11.4\bin\$nombre",
        "C:\xampp\mysql\bin\$nombre"
    )
    if ($env:MYSQL_HOME) {
        $candidatos = @((Join-Path $env:MYSQL_HOME "bin\$nombre")) + $candidatos
    }

    foreach ($ruta in $candidatos) {
        if (Test-Path $ruta) { return $ruta }
    }
    throw "No se encontró $nombre. Instale MySQL 8.0 o agregue su carpeta bin al PATH."
}

$mysqlExe = Buscar-MySqlExe 'mysql.exe'
$mysqldumpExe = Buscar-MySqlExe 'mysqldump.exe'
Write-Host "MySQL: $mysqlExe" -ForegroundColor DarkGray
Write-Host "mysqldump: $mysqldumpExe" -ForegroundColor DarkGray

$vbc = Join-Path $env:windir 'Microsoft.NET\Framework64\v4.0.30319\vbc.exe'
if (-not (Test-Path $vbc)) {
    throw "No se encontró el compilador de VB.NET en $vbc."
}

# ------------------------------------------------------------------
# 2. Credenciales de root en un archivo temporal
# ------------------------------------------------------------------

$claveRoot = $env:MYSQL_ROOT_PASSWORD
if ([string]::IsNullOrEmpty($claveRoot)) {
    Write-Host 'No se definió MYSQL_ROOT_PASSWORD. Se pedirá la clave de root.' -ForegroundColor Yellow
    $segura = Read-Host 'Clave de root de MySQL' -AsSecureString
    $claveRoot = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura))
}

$archivoCredenciales = Join-Path $env:TEMP ("sgln_root_" + [Guid]::NewGuid().ToString('N') + '.cnf')
[IO.File]::WriteAllText(
    $archivoCredenciales,
    "[client]`r`nuser=root`r`npassword=$claveRoot`r`n",
    (New-Object Text.UTF8Encoding($false)))

function Invoke-MySqlCommand([string]$sql) {
    & $mysqlExe "--defaults-extra-file=$archivoCredenciales" `
        --default-character-set=utf8mb4 -e $sql
    if ($LASTEXITCODE -ne 0) {
        throw "MySQL devolvió el código $LASTEXITCODE al ejecutar: $sql"
    }
}

function Invoke-MySqlFile([string]$rutaArchivo, [string]$baseDatos) {
    $salida = [IO.Path]::GetTempFileName()
    $errores = [IO.Path]::GetTempFileName()
    try {
        $argumentos = @(
            "--defaults-extra-file=$archivoCredenciales",
            '--default-character-set=utf8mb4',
            "--database=$baseDatos"
        )
        $proceso = Start-Process -FilePath $mysqlExe -ArgumentList $argumentos `
            -RedirectStandardInput $rutaArchivo `
            -RedirectStandardOutput $salida `
            -RedirectStandardError $errores `
            -NoNewWindow -Wait -PassThru

        if ($proceso.ExitCode -ne 0) {
            $detalle = Get-Content $errores -Raw
            throw "Falló $rutaArchivo (código $($proceso.ExitCode)): $detalle"
        }
    }
    finally {
        Remove-Item $salida, $errores -Force -ErrorAction SilentlyContinue
    }
}

try {
    # ------------------------------------------------------------------
    # 3. Base de pruebas limpia y permisos
    # ------------------------------------------------------------------

    if (-not $ConservarBase) {
        Write-Host "Recreando la base de pruebas $basePruebas..." -ForegroundColor Cyan
        Invoke-MySqlCommand (
            "DROP DATABASE IF EXISTS ``$basePruebas``; " +
            "CREATE DATABASE ``$basePruebas`` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;")
    }

    Write-Host 'Ajustando usuarios y permisos sobre la base de pruebas...' -ForegroundColor Cyan
    Invoke-MySqlCommand (
        "DROP USER IF EXISTS '$usuarioApp'@'%'; " +
        "DROP USER IF EXISTS '$usuarioRespaldo'@'%'; " +
        "CREATE USER IF NOT EXISTS '$usuarioApp'@'localhost' IDENTIFIED BY '$claveApp'; " +
        "ALTER USER '$usuarioApp'@'localhost' IDENTIFIED BY '$claveApp'; " +
        "CREATE USER IF NOT EXISTS '$usuarioRespaldo'@'localhost' IDENTIFIED BY '$claveRespaldo'; " +
        "ALTER USER '$usuarioRespaldo'@'localhost' IDENTIFIED BY '$claveRespaldo'; " +
        "REVOKE ALL PRIVILEGES, GRANT OPTION FROM '$usuarioApp'@'localhost'; " +
        "REVOKE ALL PRIVILEGES, GRANT OPTION FROM '$usuarioRespaldo'@'localhost'; " +
        "GRANT SELECT, INSERT, UPDATE, DELETE ON ``$basePruebas``.* TO '$usuarioApp'@'localhost'; " +
        "GRANT SELECT, SHOW VIEW, TRIGGER ON ``$basePruebas``.* TO '$usuarioRespaldo'@'localhost'; " +
        "FLUSH PRIVILEGES;")

    Write-Host 'Aplicando esquema y triggers...' -ForegroundColor Cyan
    Invoke-MySqlFile (Join-Path $carpetaScripts 'Esquema_MySQL.sql') $basePruebas
    Invoke-MySqlFile (Join-Path $carpetaScripts 'Triggers_MySQL.sql') $basePruebas

    # ------------------------------------------------------------------
    # 4. Compilación del arnés
    # ------------------------------------------------------------------

    New-Item -ItemType Directory -Path $carpetaBin -Force | Out-Null
    Copy-Item $ejecutableSgln -Destination $carpetaBin -Force
    Get-ChildItem $carpetaLib -Filter '*.dll' | Copy-Item -Destination $carpetaBin -Force
    Copy-Item $configPruebas -Destination (Join-Path $carpetaBin 'PruebaFuncional.exe.config') -Force

    # El arnés usa mysqldump para la prueba de respaldo: se apunta a la ruta
    # detectada, que puede no estar en PATH en el equipo de pruebas.
    $configBin = Join-Path $carpetaBin 'PruebaFuncional.exe.config'
    [xml]$xmlBin = Get-Content $configBin
    foreach ($conexionConfig in $xmlBin.configuration.connectionStrings.add) {
        if ($conexionConfig.name -eq 'SGLN') {
            $conexionConfig.connectionString = $conexionConfig.connectionString -replace '(?i)(Pwd=)[^;]*', "Pwd=$claveApp"
        }
    }
    foreach ($ajuste in $xmlBin.configuration.appSettings.add) {
        if ($ajuste.key -eq 'Respaldos_Usuario') { $ajuste.value = $usuarioRespaldo }
        if ($ajuste.key -eq 'Respaldos_Clave') { $ajuste.value = $claveRespaldo }
        if ($ajuste.key -eq 'Respaldos_RutaMySQLDump') { $ajuste.value = $mysqldumpExe }
    }
    $xmlBin.Save($configBin)

    Write-Host 'Compilando el arnés de pruebas...' -ForegroundColor Cyan
    & $vbc /nologo /quiet /codepage:65001 /target:exe `
        /out:"$carpetaBin\PruebaFuncional.exe" `
        /r:"$carpetaBin\SGLN.exe" `
        /r:"$carpetaBin\MySql.Data.dll" `
        /r:System.dll /r:System.Core.dll /r:System.Data.dll `
        /r:System.Windows.Forms.dll /r:System.Drawing.dll `
        /r:System.Configuration.dll /r:System.Xml.dll `
        "$carpeta\PruebaFuncional.vb"

    if ($LASTEXITCODE -ne 0) {
        throw 'Falló la compilación del arnés de pruebas.'
    }

    # ------------------------------------------------------------------
    # 5. Ejecución
    # ------------------------------------------------------------------

    Write-Host 'Ejecutando la batería funcional...' -ForegroundColor Cyan
    Write-Host ''
    & "$carpetaBin\PruebaFuncional.exe"
    $codigo = $LASTEXITCODE

    Write-Host ''
    if ($codigo -eq 0) {
        Write-Host 'Resultado: todas las pruebas pasaron.' -ForegroundColor Green
    }
    else {
        Write-Host "Resultado: $codigo prueba(s) fallaron." -ForegroundColor Red
    }

    exit $codigo
}
finally {
    if (Test-Path $archivoCredenciales) {
        Remove-Item $archivoCredenciales -Force -ErrorAction SilentlyContinue
    }
}
