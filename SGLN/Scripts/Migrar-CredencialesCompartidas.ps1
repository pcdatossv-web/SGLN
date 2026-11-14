#requires -RunAsAdministrator
<#
.SYNOPSIS
    Elimina credenciales antiguas almacenadas en HKLM por versiones previas de SGLN.

.DESCRIPTION
    Las versiones actuales ya no leen contraseñas desde HKLM. Este script
    elimina únicamente los valores Password y RespaldosPassword de ambas
    vistas del registro, sin tocar servidor, puerto, base, usuario ni TLS.
    Ejecútelo una vez como administrador en cada PC cliente actualizado.
    Después, cada usuario de Windows debe abrir SGLN y guardar sus propias
    credenciales desde "Configurar servidor de base de datos".
#>

$ErrorActionPreference = 'Stop'
$subclave = 'SOFTWARE\SGLN\Conexion'
$nombres = @('Password', 'RespaldosPassword')

if ([Environment]::Is64BitOperatingSystem) {
    $vistas = @(
        [Microsoft.Win32.RegistryView]::Registry64,
        [Microsoft.Win32.RegistryView]::Registry32
    )
}
else {
    $vistas = @([Microsoft.Win32.RegistryView]::Registry32)
}

$eliminados = 0
foreach ($vista in $vistas) {
    $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey(
        [Microsoft.Win32.RegistryHive]::LocalMachine, $vista)
    try {
        $clave = $base.OpenSubKey($subclave, $true)
        if ($null -eq $clave) { continue }
        try {
            $existentes = $clave.GetValueNames()
            foreach ($nombre in $nombres) {
                if ($existentes -contains $nombre) {
                    $clave.DeleteValue($nombre, $false)
                    $eliminados++
                }
            }
        }
        finally {
            $clave.Dispose()
        }
    }
    finally {
        $base.Dispose()
    }
}

Write-Host "Migración completada. Valores de contraseña eliminados: $eliminados."
Write-Host 'Cada usuario de Windows debe guardar sus propias credenciales en SGLN.'
