param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\src\TestConfig",
    [string] $resourceGroupName,
    [switch] $batch = $true,
    [switch] $aks = $true,
    [switch] $aci = $true,
    [switch] $containerApp = $true
)

<#
.SYNOPSIS
    Creates ALL Managed Identity-only settings files for all deployment types.

.DESCRIPTION
    This script generates settings files for Batch, AKS, ACI, and Container App deployments
    that use Managed Identity for ALL Azure service authentication. No keys, connection 
    strings, or passwords are stored in these files.

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER sbmExe
    Path to the sbm.exe executable.

.PARAMETER path
    Output path for the generated settings files.

.PARAMETER resourceGroupName
    The Azure resource group name (defaults to {prefix}-rg).

.PARAMETER batch
    Generate Batch settings files (default: true).

.PARAMETER aks
    Generate AKS settings files (default: true).

.PARAMETER aci
    Generate ACI settings files (default: true).

.PARAMETER containerApp
    Generate Container App settings files (default: true).
#>

if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Creating ALL Managed Identity-Only Settings Files" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Prefix: $prefix" -ForegroundColor DarkGreen
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Output Path: $path" -ForegroundColor DarkGreen
Write-Host ""

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent
}

if ($batch) {
    Write-Host "Generating Batch MI-only settings files..." -ForegroundColor Yellow
    $batchScript = Join-Path $repoRoot "scripts\Batch\create_batch_settingsfiles_mi_only.ps1"
    if (Test-Path $batchScript) {
        & $batchScript -prefix $prefix -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName
    } else {
        Write-Host "  Script not found: $batchScript" -ForegroundColor Red
    }
    Write-Host ""
}

if ($aks) {
    Write-Host "Generating AKS MI-only settings file..." -ForegroundColor Yellow
    $aksScript = Join-Path $repoRoot "scripts\kubernetes\create_aks_settingsfile_mi_only.ps1"
    if (Test-Path $aksScript) {
        & $aksScript -prefix $prefix -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName
    } else {
        Write-Host "  Script not found: $aksScript" -ForegroundColor Red
    }
    Write-Host ""
}

if ($aci) {
    Write-Host "Generating ACI MI-only settings file..." -ForegroundColor Yellow
    $aciScript = Join-Path $repoRoot "scripts\aci\create_aci_settingsfile_mi_only.ps1"
    if (Test-Path $aciScript) {
        & $aciScript -prefix $prefix -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName
    } else {
        Write-Host "  Script not found: $aciScript" -ForegroundColor Red
    }
    Write-Host ""
}

if ($containerApp) {
    Write-Host "Generating Container App MI-only settings file..." -ForegroundColor Yellow
    $containerAppScript = Join-Path $repoRoot "scripts\ContainerApp\create_containerapp_settingsfile_mi_only.ps1"
    if (Test-Path $containerAppScript) {
        & $containerAppScript -prefix $prefix -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName
    } else {
        Write-Host "  Script not found: $containerAppScript" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "============================================" -ForegroundColor Green
Write-Host "All Managed Identity-Only Settings Files Generated" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "These settings files contain NO secrets, keys, or connection strings." -ForegroundColor Cyan
Write-Host "All Azure services will authenticate using Managed Identity." -ForegroundColor Cyan
