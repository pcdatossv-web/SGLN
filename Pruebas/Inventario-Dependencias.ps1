$ErrorActionPreference = 'Stop'

$dependencyPaths = @(
    'lib/MySql.Data.dll',
    'lib/System.Buffers.dll',
    'lib/System.Memory.dll',
    'lib/System.Numerics.Vectors.dll'
)

$inventory = foreach ($relativePath in $dependencyPaths) {
    $fullPath = Join-Path $PSScriptRoot ("..\" + $relativePath)
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "No se encontró la dependencia requerida: $relativePath"
    }

    $assembly = [System.Reflection.AssemblyName]::GetAssemblyName($fullPath)
    $file = Get-Item -LiteralPath $fullPath
    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fullPath)
    $hash = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash

    [pscustomobject]@{
        File            = $relativePath
        Assembly        = $assembly.Name
        AssemblyVersion = $assembly.Version.ToString()
        FileVersion     = $versionInfo.FileVersion
        ProductVersion  = $versionInfo.ProductVersion
        Company         = $versionInfo.CompanyName
        SizeBytes       = $file.Length
        SHA256          = $hash
    }
}

Write-Host "Inventario de dependencias incluidas en SGLN:"
$inventory | Format-List
