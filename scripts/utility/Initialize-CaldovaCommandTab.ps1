param(
    [Parameter(Mandatory = $true)]
    [string] $CommandText,

    [string] $TerminalProfile = 'Caldova'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($TerminalProfile) -or
    $TerminalProfile.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0) {
    throw 'TerminalProfile must be a nonblank profile name without invalid filename characters.'
}

$profileFolder = $TerminalProfile.ToLowerInvariant()
$env:AZURE_CONFIG_DIR = Join-Path $env:USERPROFILE ".azure-$profileFolder-isolated"
$env:AZD_CONFIG_DIR = Join-Path $env:USERPROFILE ".azd-$profileFolder-isolated"
New-Item -ItemType Directory -Force -Path $env:AZURE_CONFIG_DIR, $env:AZD_CONFIG_DIR | Out-Null

Write-Host 'Azure CLI account:' -ForegroundColor Cyan
az account show --query '[name, user.name]' -o tsv
if ($LASTEXITCODE -ne 0) {
    Write-Warning 'Azure CLI account lookup failed. Run az login in this tab before executing Azure commands.'
}
Write-Host ''
Write-Host 'Azure Developer CLI auth:' -ForegroundColor Cyan
azd auth status
if ($LASTEXITCODE -ne 0) {
    Write-Warning 'Azure Developer CLI authentication check failed. Run azd auth login in this tab before executing Azure commands.'
}

& "$PSScriptRoot\Initialize-CommandTab.ps1" -CommandText $CommandText
