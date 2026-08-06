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
set "TAB_INIT=%REPO_ROOT%\scripts\utility\Initialize-CommandTab.ps1"

if not exist "%PWSH%" (
    echo PowerShell 7 was not found at "%PWSH%".
    exit /b 1
)
if not exist "%TAB_INIT%" (
    echo Terminal initialization script was not found at "%TAB_INIT%".
    exit /b 1
)

set "WD=%REPO_ROOT%\scripts\ContainerRegistry"


wt.exe -w new ^
  new-tab --title "External_Img" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\build_external_test_image.ps1 -envName " ^
  ; new-tab --title "Dependent_Img" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\build_dependent_test_image.ps1 -envName " ^
  ; new-tab --title "Runtime_Img" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\build_runtime_image_fromenv.ps1 -envName "


set "WD=%REPO_ROOT%\scripts\tests"
wt.exe -w new ^
  new-tab --title "SqlServer_External_Tests" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\run_all_sqlserver_external_tests_in_aci.ps1 -envName " ^
  ; new-tab --title "Postgres_External_Tests" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\run_all_postgres_external_tests_in_aci.ps1 -envName " ^
  ; new-tab --title "MySQL_External_Tests" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\run_all_mysql_external_tests_in_aci.ps1 -envName " ^
  ; new-tab --title "Dependent_Tests" -d "%WD%" -- "%PWSH%" -NoExit -File "%TAB_INIT%" -CommandText ".\run_dependent_tests_in_aci.ps1 -envName "

endlocal
