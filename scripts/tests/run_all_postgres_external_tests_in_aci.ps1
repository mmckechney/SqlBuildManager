<#
.SYNOPSIS
    Runs all PostgreSQL integration test suites in ACI, filtered by deployed platforms.
.DESCRIPTION
    Reads AZD environment configuration to determine which compute platforms (ACI,
    Batch, Container Apps, AKS) and PostgreSQL database platform are deployed. For
    all available compute platforms, launches the filtered PostgreSQL external test
    runner in ACI. After all tests complete, downloads results from Azure Storage
    and invokes GitHub Copilot CLI to analyze the test output.
.PARAMETER envName
    Azure Developer CLI environment name. Defaults to the selected azd environment.
#>
[CmdletBinding()]
param (
    [Parameter()]
    [string] $envName
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
    Write-Host "  Provide it as a parameter:  .\run_all_postgres_external_tests_in_aci.ps1 -envName <your-env>" -ForegroundColor Yellow
    Write-Host "  Or select an azd environment with 'azd env select'." -ForegroundColor Yellow
    exit 1
}

. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName

$exitCode = 0
$timestamp = (Get-Date -Format 'yyyy-MM-dd-HHmmss')
if ($Host.Name -eq 'ConsoleHost' -and [string]::IsNullOrWhiteSpace($env:CI)) { Clear-Host }

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Integration Test Runners (ACI in VNet)" -ForegroundColor Cyan
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
$hasPostgreSQL   = Test-DeployFlag 'DEPLOY_POSTGRESQL'

Write-Host ""
Write-Host "Platform Availability:" -ForegroundColor Cyan
Write-Host "  PostgreSQL:     $(if ($hasPostgreSQL)   { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasPostgreSQL)   { 'Green' } else { 'DarkGray' })
Write-Host "  ACI:            $(if ($hasAci)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAci)           { 'Green' } else { 'DarkGray' })
Write-Host "  Batch:          $(if ($hasBatch)         { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasBatch)         { 'Green' } else { 'DarkGray' })
Write-Host "  Container Apps: $(if ($hasContainerApp)  { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasContainerApp)  { 'Green' } else { 'DarkGray' })
Write-Host "  AKS:            $(if ($hasAks)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAks)           { 'Green' } else { 'DarkGray' })
Write-Host ""

#############################################
# PostgreSQL tests (requires PostgreSQL + dynamically filters by available compute)
#############################################

if (-not $hasPostgreSQL) {
    Write-Host "SKIPPING [pg]: PostgreSQL database is not deployed" -ForegroundColor Yellow
} else {
    # Build filter dynamically based on available compute platforms
    $pgFilters = @()
    $pgSkipped = @()

    if ($hasAci) {
        $pgFilters += "FullyQualifiedName~SqlBuildManager.Console.PostgreSQL.ExternalTest.AciTests"
    } else {
        $pgSkipped += "ACI"
    }
    if ($hasBatch) {
        $pgFilters += "FullyQualifiedName~SqlBuildManager.Console.PostgreSQL.ExternalTest.BatchTests"
    } else {
        $pgSkipped += "Batch"
    }
    if ($hasContainerApp) {
        $pgFilters += "FullyQualifiedName~SqlBuildManager.Console.PostgreSQL.ExternalTest.ContainerAppTests"
    } else {
        $pgSkipped += "Container Apps"
    }
    if ($hasAks) {
        $pgFilters += "FullyQualifiedName~SqlBuildManager.Console.PostgreSQL.ExternalTest.KubernetesTests"
    } else {
        $pgSkipped += "AKS"
    }

    if ($pgSkipped.Count -gt 0) {
        Write-Host "SKIPPING PostgreSQL tests for unavailable compute: $($pgSkipped -join ', ')" -ForegroundColor Yellow
    }

    if ($pgFilters.Count -gt 0) {
        $pgTestFilter = $pgFilters -join '|'
        & (Join-Path $PSScriptRoot 'run_filtered_external_tests_in_aci.ps1') -envName $envName -customName pg -testFilter $pgTestFilter -timeoutMinutes 300 -timestamp $timestamp
        $exitCode += $LASTEXITCODE
    } else {
        Write-Host "SKIPPING [pg]: No compute platforms available for PostgreSQL tests" -ForegroundColor Yellow
    }
}

# Download test results
if ((Test-Path ./testresults) -eq $false) { New-Item -ItemType Directory ./testresults | Out-Null }
az storage blob download-batch --account-name $storageAccountName --source testresults --pattern "$($timestamp)*" --destination ./testresults --auth-mode login --overwrite
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Failed to download test results from storage (exit code $LASTEXITCODE)." -ForegroundColor Yellow
}

# Analyze test results with GitHub Copilot CLI (local developer convenience; skip in CI).
if (Get-Command copilot -ErrorAction SilentlyContinue) {
    $promptTemplate = Get-Content -Path (Join-Path $PSScriptRoot 'analyze-test-results-prompt.md') -Raw
    $prompt = $promptTemplate -replace '\{\{timestamp\}\}', $timestamp
    $output = copilot --yolo -p $prompt 2>&1
}
