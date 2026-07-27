<#
.SYNOPSIS
    Runs all SQL Server integration test suites in ACI, filtered by deployed platforms.
.DESCRIPTION
    Reads AZD environment configuration to determine which compute platforms (ACI,
    Batch, Container Apps, AKS) and SQL Server database platform are deployed. For
    each available combination, launches the filtered external test runner in ACI.
    After all tests complete, downloads results from Azure Storage and invokes
    GitHub Copilot CLI to analyze the test output.
.PARAMETER envName
    Azure Developer CLI environment name. Defaults to the selected azd environment.
.PARAMETER testGroups
    Optional test groups to run. Valid values are aci, containerapp, batchqueue,
    batchoverride, batchquery, and aks. Omit to run every group whose required
    platform is deployed.
.EXAMPLE
    .\run_all_sqlserver_external_tests_in_aci.ps1 -envName myenv -testGroups aci,containerapp
#>
[CmdletBinding()]
param (
    [Parameter()]
    [string] $envName,

    [Parameter()]
    [ValidateSet('aci', 'containerapp', 'batchqueue', 'batchoverride', 'batchquery', 'aks')]
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
    Write-Host "  Provide it as a parameter:  .\run_all_sqlserver_external_tests_in_aci.ps1 -envName <your-env>" -ForegroundColor Yellow
    Write-Host "  Or select an azd environment with 'azd env select'." -ForegroundColor Yellow
    exit 1
}

. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName

$exitCode = 0
$timestamp = (Get-Date -Format 'yyyy-MM-dd-HHmmss')
# Clear-Host is suppressed in non-interactive (CI) sessions.
if ($Host.Name -eq 'ConsoleHost' -and [string]::IsNullOrWhiteSpace($env:CI)) {
    Clear-Host
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "SQL Server Integration Test Runners (ACI in VNet)" -ForegroundColor Cyan
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
$hasSqlServer    = Test-DeployFlag 'DEPLOY_SQLSERVER'
$requestedTestGroups = if ($testGroups.Count -eq 0) {
    @('aci', 'containerapp', 'batchqueue', 'batchoverride', 'batchquery', 'aks')
} else {
    @($testGroups)
}

Write-Host ""
Write-Host "Platform Availability:" -ForegroundColor Cyan
Write-Host "  SQL Server:     $(if ($hasSqlServer)    { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasSqlServer)    { 'Green' } else { 'DarkGray' })
Write-Host "  ACI:            $(if ($hasAci)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAci)           { 'Green' } else { 'DarkGray' })
Write-Host "  Batch:          $(if ($hasBatch)         { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasBatch)         { 'Green' } else { 'DarkGray' })
Write-Host "  Container Apps: $(if ($hasContainerApp)  { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasContainerApp)  { 'Green' } else { 'DarkGray' })
Write-Host "  AKS:            $(if ($hasAks)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAks)           { 'Green' } else { 'DarkGray' })
Write-Host "  Test groups:    $($requestedTestGroups -join ', ')" -ForegroundColor DarkGreen
Write-Host ""

function Invoke-TestIfAvailable {
    param(
        [string]$customName,
        [string]$testFilter,
        [string]$computeLabel,
        [bool]$computeAvailable,
        [string]$computeDeployFlags,
        [string]$databaseLabel,
        [bool]$databaseAvailable,
        [string]$databaseDeployFlags,
        [int]$timeoutMinutes = 300
    )

    if ($customName -notin $requestedTestGroups) {
        return 0
    }

    $skipReasons = @()
    if (-not $computeAvailable) {
        $skipReasons += "$computeLabel compute is not deployed ($computeDeployFlags is not true)"
    }
    if (-not $databaseAvailable) {
        $skipReasons += "$databaseLabel database is not deployed ($databaseDeployFlags is not true)"
    }

    if ($skipReasons.Count -gt 0) {
        Write-Host "SKIPPING requested test group [$customName]: $($skipReasons -join '; ')" -ForegroundColor Yellow
        return 0
    }

    & (Join-Path $PSScriptRoot 'run_filtered_external_tests_in_aci.ps1') -envName $envName -customName $customName -testFilter $testFilter -timeoutMinutes $timeoutMinutes -timestamp $timestamp
    return $LASTEXITCODE
}

#############################################
# SQL Server tests (each requires SQL Server + specific compute)
#############################################

$exitCode += Invoke-TestIfAvailable -customName "aci" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.SqlServer.AzureTest.AciTests" `
    -computeLabel "ACI" -computeAvailable $hasAci `
    -computeDeployFlags "DEPLOY_ACI" `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer `
    -databaseDeployFlags "DEPLOY_SQLSERVER"

$exitCode += Invoke-TestIfAvailable -customName "containerapp" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.SqlServer.AzureTest.ContainerAppTests" `
    -computeLabel "Container Apps" -computeAvailable $hasContainerApp `
    -computeDeployFlags "DEPLOY_CONTAINERAPP_ENV/DEPLOY_CONTAINERAPP" `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer `
    -databaseDeployFlags "DEPLOY_SQLSERVER"

$exitCode += Invoke-TestIfAvailable -customName "batchqueue" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.SqlServer.AzureTest.BatchTests.Batch_Queue" `
    -computeLabel "Batch" -computeAvailable $hasBatch `
    -computeDeployFlags "DEPLOY_BATCH_ACCOUNT/DEPLOY_BATCH" `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer `
    -databaseDeployFlags "DEPLOY_SQLSERVER"

$exitCode += Invoke-TestIfAvailable -customName "aks" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.SqlServer.AzureTest.KubernetesTests" `
    -computeLabel "AKS" -computeAvailable $hasAks `
    -computeDeployFlags "DEPLOY_AKS" `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer `
    -databaseDeployFlags "DEPLOY_SQLSERVER"

$exitCode += Invoke-TestIfAvailable -customName "batchoverride" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.SqlServer.AzureTest.BatchTests.Batch_Override" `
    -computeLabel "Batch" -computeAvailable $hasBatch `
    -computeDeployFlags "DEPLOY_BATCH_ACCOUNT/DEPLOY_BATCH" `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer `
    -databaseDeployFlags "DEPLOY_SQLSERVER"

$exitCode += Invoke-TestIfAvailable -customName "batchquery" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.SqlServer.AzureTest.BatchTests.Batch_Query" `
    -computeLabel "Batch" -computeAvailable $hasBatch `
    -computeDeployFlags "DEPLOY_BATCH_ACCOUNT/DEPLOY_BATCH" `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer `
    -databaseDeployFlags "DEPLOY_SQLSERVER"

# Analyze test results with GitHub Copilot CLI (local developer convenience; skip in CI).
Write-Host "Running Copilot AI analysis of test logs to look for patterns, failure reasons and areas for improvement" -ForegroundColor Yellow
if (Get-Command copilot -ErrorAction SilentlyContinue) {
    $promptTemplate = Get-Content -Path (Join-Path $PSScriptRoot 'analyze-test-results-prompt.md') -Raw
    $prompt = $promptTemplate -replace '\{\{timestamp\}\}', $timestamp
    $output = copilot --yolo -p $prompt 2>&1
}
