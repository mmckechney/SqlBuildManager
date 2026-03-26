@echo off


setlocal

set "PWSH=C:\Program Files\PowerShell\7\pwsh.exe"
set "WD=C:\Users\mimcke\source\repos\SqlBuildManager\scripts\ContainerRegistry"


wt.exe -w new ^
  new-tab -p "PowerShell" --title "External_Img"  -d "%WD%" -- "%PWSH%" -NoExit  -Command "ls\; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\build_external_test_image.ps1 -prefix ') }\; Write-Host ''\; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Dependent_Img" -d "%WD%" -- "%PWSH%" -NoExit -Command "ls\; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\build_dependent_test_image.ps1 -prefix ') }\; Write-Host ''\; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Runtime_Img"   -d "%WD%" -- "%PWSH%" -NoExit -Command "ls\; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\build_runtime_image_fromprefix.ps1 -prefix ') }\; Write-Host ''\; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan"


set "WD=C:\Users\mimcke\source\repos\SqlBuildManager\scripts\tests"
wt.exe -w new ^
  new-tab -p "PowerShell" --title "External_Img"  -d "%WD%" -- "%PWSH%" -NoExit  -Command "ls\; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\run_all_external_tests_in_aci.ps1 -prefix ') }\; Write-Host ''\; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan" ^
  ; new-tab -p "PowerShell" --title "Dependent_Img" -d "%WD%" -- "%PWSH%" -NoExit -Command "ls\; Set-PSReadLineKeyHandler -Key F12 -ScriptBlock { [Microsoft.PowerShell.PSConsoleReadLine]::Insert('.\run_dependent_tests_in_aci.ps1 -prefix ') }\; Write-Host ''\; Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan"

endlocal
