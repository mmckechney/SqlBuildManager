@echo off


setlocal

rem Resolve pwsh.exe portably: prefer the user's PATH, fall back to the default install location.
where pwsh.exe >nul 2>&1
if %ERRORLEVEL% equ 0 (
    for /f "tokens=*" %%i in ('where pwsh.exe') do (set "PWSH=%%i" & goto :pwsh_found)
)
set "PWSH=%ProgramFiles%\PowerShell\7\pwsh.exe"
:pwsh_found

rem Derive the repo root from this script's location (%~dp0 is the directory of the .bat file).
set "REPO_ROOT=%~dp0"
if "%REPO_ROOT:~-1%"=="\" set "REPO_ROOT=%REPO_ROOT:~0,-1%"

set "WD=%REPO_ROOT%\scripts\ContainerRegistry"


wt.exe -w new ^
  new-tab -p "PowerShell" --title "External_Img"  -d "%WD%" -- "%PWSH%" -NoExit  -Command "ls; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\build_external_test_image.ps1 -envName ') }; Write-Host ''; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Dependent_Img" -d "%WD%" -- "%PWSH%" -NoExit -Command "ls; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\build_dependent_test_image.ps1 -envName ') }; Write-Host ''; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Runtime_Img"   -d "%WD%" -- "%PWSH%" -NoExit -Command "ls; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\build_runtime_image_fromenv.ps1 -envName ') }; Write-Host ''; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan"


set "WD=%REPO_ROOT%\scripts\tests"
wt.exe -w new ^
  new-tab -p "PowerShell" --title "SqlServer_External"  -d "%WD%" -- "%PWSH%" -NoExit  -Command "ls; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\run_all_sqlserver_external_tests_in_aci.ps1 -envName ') }; Write-Host ''; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Postgres_External"  -d "%WD%" -- "%PWSH%" -NoExit -Command "ls; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\run_all_postgres_external_tests_in_aci.ps1 -envName ') }; Write-Host ''; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Dependent_Img" -d "%WD%" -- "%PWSH%" -NoExit -Command "ls; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\run_dependent_tests_in_aci.ps1 -envName ') }; Write-Host ''; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan"

endlocal