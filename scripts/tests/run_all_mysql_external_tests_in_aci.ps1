<#
.SYNOPSIS
    Runs all MySQL integration test suites in ACI, filtered by deployed platforms.
.DESCRIPTION
    Reads AZD environment configuration to determine which compute platforms (ACI,
    Batch, Container Apps, AKS) and MySQL database platform are deployed. For
    all available compute platforms, launches the filtered MySQL external test
    runner in ACI. After all tests complete, downloads results from Azure Storage
    and invokes GitHub Copilot CLI to analyze the test output.
.PARAMETER envName
    Azure Developer CLI environment name. Defaults to the selected azd environment.
.PARAMETER testGroups
    Optional test groups to run. Valid values are aci, batch, containerapp, and aks.
    Omit to run every group whose required platform is deployed.
.EXAMPLE
    .\run_all_mysql_external_tests_in_aci.ps1 -envName myenv -testGroups aci,batch
#>
[CmdletBinding()]
param (
    [Parameter()]
    [string] $envName,

    [Parameter()]
    [ValidateSet('aci', 'batch', 'containerapp', 'aks')]
    [string[]] $testGroups = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Resolve environment name: parameter > azd environment
if ([string]::IsNullOrWhiteSpace($envName)) {
    $envName = azd env get-value AZURE_ENV_NAME 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($envName)) {
        $envName = $null
    } else {
        Write-Host "Using environment '$envName' from AZURE_ENV_NAME" -ForegroundColor DarkGreen
    }
}

if ([string]::IsNullOrWhiteSpace($envName)) {
    Write-Host "ERROR: The -envName parameter is required." -ForegroundColor Red
    Write-Host "  Provide it as a parameter:  .\run_all_mysql_external_tests_in_aci.ps1 -envName <your-env>" -ForegroundColor Yellow
    Write-Host "  Or select an azd environment with 'azd env select'." -ForegroundColor Yellow
    exit 1
}

. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName

$exitCode = 0
$timestamp = (Get-Date -Format 'yyyy-MM-dd-HHmmss')
if ($Host.Name -eq 'ConsoleHost' -and [string]::IsNullOrWhiteSpace($env:CI)) { Clear-Host }

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "MySQL Integration Test Runners (ACI in VNet)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

#############################################
# Load AZD deployment configuration
#############################################
Write-Host ""
Write-Host "Loading AZD deployment configuration..." -ForegroundColor Cyan

$azdConfig = @{}
$azdOutput = azd env get-values 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Failed to load AZD environment values. All tests will be attempted." -ForegroundColor Yellow
    Write-Host "  Run 'azd env select' or 'azd init' to configure an environment." -ForegroundColor Yellow
    Write-Host ""
} else {
    $azdOutput | ForEach-Object {
        if ($_ -match '^([^=]+)="?([^"]*)"?$') {
            $azdConfig[$matches[1]] = $matches[2]
        }
    }
}

function Test-DeployFlag {
    param([string[]]$flagNames)
    if ($azdConfig.Count -eq 0) { return $true } # If no AZD config, assume all available
    foreach ($flag in $flagNames) {
        if ($azdConfig[$flag] -eq 'true') { return $true }
    }
    return $false
}

$hasAci          = Test-DeployFlag 'DEPLOY_ACI'
$hasBatch        = Test-DeployFlag 'DEPLOY_BATCH_ACCOUNT', 'DEPLOY_BATCH'
$hasContainerApp = Test-DeployFlag 'DEPLOY_CONTAINERAPP_ENV', 'DEPLOY_CONTAINERAPP'
$hasAks          = Test-DeployFlag 'DEPLOY_AKS'
$hasMySQL        = Test-DeployFlag 'DEPLOY_MYSQL'
$requestedTestGroups = if ($testGroups.Count -eq 0) {
    @('aci', 'batch', 'containerapp', 'aks')
} else {
    @($testGroups)
}

Write-Host ""
Write-Host "Platform Availability:" -ForegroundColor Cyan
Write-Host "  MySQL:          $(if ($hasMySQL)        { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasMySQL)        { 'Green' } else { 'DarkGray' })
Write-Host "  ACI:            $(if ($hasAci)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAci)           { 'Green' } else { 'DarkGray' })
Write-Host "  Batch:          $(if ($hasBatch)         { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasBatch)         { 'Green' } else { 'DarkGray' })
Write-Host "  Container Apps: $(if ($hasContainerApp)  { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasContainerApp)  { 'Green' } else { 'DarkGray' })
Write-Host "  AKS:            $(if ($hasAks)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAks)           { 'Green' } else { 'DarkGray' })
Write-Host "  Test groups:    $($requestedTestGroups -join ', ')" -ForegroundColor DarkGreen
Write-Host ""

#############################################
# MySQL tests (requires MySQL + dynamically filters by available compute)
#############################################

if (-not $hasMySQL) {
    Write-Host "SKIPPING requested MySQL test groups: MySQL is not deployed (DEPLOY_MYSQL is not true)" -ForegroundColor Yellow
} else {
    $mySqlFilters = @()
    $platforms = @(
        @{
            Name = 'aci'
            Label = 'ACI'
            Available = $hasAci
            DeployFlags = 'DEPLOY_ACI'
            Filter = 'FullyQualifiedName~SqlBuildManager.Console.MySQL.ExternalTest.AciTests'
        },
        @{
            Name = 'batch'
            Label = 'Batch'
            Available = $hasBatch
            DeployFlags = 'DEPLOY_BATCH_ACCOUNT/DEPLOY_BATCH'
            Filter = 'FullyQualifiedName~SqlBuildManager.Console.MySQL.ExternalTest.BatchTests'
        },
        @{
            Name = 'containerapp'
            Label = 'Container Apps'
            Available = $hasContainerApp
            DeployFlags = 'DEPLOY_CONTAINERAPP_ENV/DEPLOY_CONTAINERAPP'
            Filter = 'FullyQualifiedName~SqlBuildManager.Console.MySQL.ExternalTest.ContainerAppTests'
        },
        @{
            Name = 'aks'
            Label = 'AKS'
            Available = $hasAks
            DeployFlags = 'DEPLOY_AKS'
            Filter = 'FullyQualifiedName~SqlBuildManager.Console.MySQL.ExternalTest.KubernetesTests'
        }
    )

    foreach ($platform in $platforms) {
        if ($platform.Name -notin $requestedTestGroups) {
            continue
        }
        if (-not $platform.Available) {
            Write-Host "SKIPPING requested test group [$($platform.Name)]: $($platform.Label) compute is not deployed ($($platform.DeployFlags) is not true)" -ForegroundColor Yellow
            continue
        }
        $mySqlFilters += $platform.Filter
    }

    if ($mySqlFilters.Count -gt 0) {
        $mySqlTestFilter = $mySqlFilters -join '|'
        & (Join-Path $PSScriptRoot 'run_filtered_external_tests_in_aci.ps1') -envName $envName -customName mysql -testFilter $mySqlTestFilter -timeoutMinutes 300 -timestamp $timestamp
        $exitCode += $LASTEXITCODE
    } else {
        Write-Host "SKIPPING [mysql]: None of the requested test groups are available" -ForegroundColor Yellow
    }
}

# Analyze test results with GitHub Copilot CLI (local developer convenience; skip in CI).
Write-Host "Running Copilot AI analysis of test logs to look for patterns, failure reasons and areas for improvement" -ForegroundColor Yellow
if (Get-Command copilot -ErrorAction SilentlyContinue) {
    $promptTemplate = Get-Content -Path (Join-Path $PSScriptRoot 'analyze-test-results-prompt.md') -Raw
    $prompt = $promptTemplate -replace '\{\{timestamp\}\}', $timestamp
    $output = copilot --yolo -p $prompt 2>&1
}
