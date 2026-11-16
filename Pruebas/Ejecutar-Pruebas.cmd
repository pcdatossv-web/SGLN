@echo off
setlocal EnableExtensions

REM Ejecuta la bateria funcional desde CMD sin que el usuario tenga que
REM escribir comandos de PowerShell manualmente.
set "SCRIPT=%~dp0Ejecutar-Pruebas.ps1"

if not exist "%SCRIPT%" (
    echo ERROR: No se encontro "%SCRIPT%".
    exit /b 2
)

where powershell.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: Windows PowerShell no esta disponible en este equipo.
    echo Instale o habilite Windows PowerShell 5.1 para ejecutar las pruebas.
    exit /b 3
)

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*
set "RESULTADO=%ERRORLEVEL%"

if not "%RESULTADO%"=="0" (
    echo.
    echo ERROR: La bateria funcional termino con codigo %RESULTADO%.
) else (
    echo.
    echo OK: La bateria funcional termino correctamente.
)

exit /b %RESULTADO%
