param(
    [Parameter(Mandatory = $true)]
    [string] $CommandText
)

$handler = {
    [Microsoft.PowerShell.PSConsoleReadLine]::Insert($CommandText)
}.GetNewClosure()

Set-PSReadLineKeyHandler -Key F12 -ScriptBlock $handler
Get-ChildItem
Write-Host
Write-Host 'Press F12 to load command at prompt' -ForegroundColor Cyan
