param
(
    [Parameter(Mandatory=$true)]
    [string] $envName,

    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\src\TestConfig",
    [string] $resourceGroupName,
    [switch] $batch = $true,
    [switch] $aks = $true,
    [switch] $aci = $true,
    [switch] $containerApp = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-AzdValueSafe {
    param([Parameter(Mandatory=$true)][string] $name)
    $value = azd env get-value $name 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($value) -or $value -like "ERROR:*") {
        return $null
    }
    return $value
}

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path $PSScriptRoot -Parent
}

$resourceGroupNameOverride = $resourceGroupName
. (Join-Path $repoRoot "scripts\prefix_resource_names.ps1") -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}

$mySqlUser = Get-AzdValueSafe "MYSQL_ADMIN_USER"
if ([string]::IsNullOrWhiteSpace($mySqlUser)) {
    $mySqlUser = $mySqlAdminUser
}
$mySqlPassword = Get-AzdValueSafe "MYSQL_ADMIN_PASSWORD"
if ([string]::IsNullOrWhiteSpace($mySqlPassword)) {
    throw "MYSQL_ADMIN_PASSWORD is not set in the active azd environment."
}

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "Creating MySQL Password-Based Settings Files" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Environment: $envName" -ForegroundColor DarkGreen
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Output Path: $path" -ForegroundColor DarkGreen
Write-Host "MySQL Auth: Password" -ForegroundColor DarkGreen
Write-Host "MySQL User: $mySqlUser" -ForegroundColor DarkGreen
Write-Host ""

if ($batch) {
    Write-Host "Generating Batch MySQL password settings files..." -ForegroundColor Yellow
    $batchScript = Join-Path $repoRoot "scripts\Batch\create_batch_settingsfiles_mi_only.ps1"
    if (Test-Path $batchScript) {
        & $batchScript `
            -envName $envName `
            -sbmExe $sbmExe `
            -path $path `
            -resourceGroupName $resourceGroupName `
            -databaseAuthType Password `
            -databasePlatform MySQL `
            -databaseUserName $mySqlUser `
            -databasePassword $mySqlPassword `
            -settingsFileSuffix "mysql-password"
    } else {
        Write-Host "  Script not found: $batchScript" -ForegroundColor Red
    }
    Write-Host ""
}

if ($aks) {
    Write-Host "Generating AKS MySQL password settings file..." -ForegroundColor Yellow
    $aksScript = Join-Path $repoRoot "scripts\kubernetes\create_aks_settingsfile_mi_only.ps1"
    if (Test-Path $aksScript) {
        & $aksScript `
            -envName $envName `
            -sbmExe $sbmExe `
            -path $path `
            -resourceGroupName $resourceGroupName `
            -databaseAuthType Password `
            -databasePlatform MySQL `
            -databaseUserName $mySqlUser `
            -databasePassword $mySqlPassword `
            -settingsFileSuffix "mysql-password"
    } else {
        Write-Host "  Script not found: $aksScript" -ForegroundColor Red
    }
    Write-Host ""
}

if ($aci) {
    Write-Host "Generating ACI MySQL password settings file..." -ForegroundColor Yellow
    $aciScript = Join-Path $repoRoot "scripts\aci\create_aci_settingsfile_mi_only.ps1"
    if (Test-Path $aciScript) {
        & $aciScript `
            -envName $envName `
            -sbmExe $sbmExe `
            -path $path `
            -resourceGroupName $resourceGroupName `
            -databaseAuthType Password `
            -databasePlatform MySQL `
            -databaseUserName $mySqlUser `
            -databasePassword $mySqlPassword `
            -settingsFileSuffix "mysql-password"
    } else {
        Write-Host "  Script not found: $aciScript" -ForegroundColor Red
    }
    Write-Host ""
}

if ($containerApp) {
    Write-Host "Generating Container App MySQL password settings file..." -ForegroundColor Yellow
    $containerAppScript = Join-Path $repoRoot "scripts\ContainerApp\create_containerapp_settingsfile_mi_only.ps1"
    if (Test-Path $containerAppScript) {
        & $containerAppScript `
            -envName $envName `
            -sbmExe $sbmExe `
            -path $path `
            -resourceGroupName $resourceGroupName `
            -databaseAuthType Password `
            -databasePlatform MySQL `
            -databaseUserName $mySqlUser `
            -databasePassword $mySqlPassword `
            -settingsFileSuffix "mysql-password"
    } else {
        Write-Host "  Script not found: $containerAppScript" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "====================================================" -ForegroundColor Green
Write-Host "MySQL Password-Based Settings Files Generated" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
