[CmdletBinding()]
param (
    [Parameter()]
    [string] $prefix 
)

# Resolve prefix: parameter > azd env AZURE_NAME_PREFIX
if ([string]::IsNullOrWhiteSpace($prefix)) {
    $prefix = azd env get-value AZURE_NAME_PREFIX 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($prefix)) {
        $prefix = $null
    } else {
        Write-Host "Using prefix '$prefix' from azd environment variable AZURE_NAME_PREFIX" -ForegroundColor DarkGreen
    }
}

if ([string]::IsNullOrWhiteSpace($prefix)) {
    Write-Host "ERROR: The -prefix parameter is required." -ForegroundColor Red
    Write-Host "  Provide it as a parameter:  .\run_all_external_tests_in_aci.ps1 -prefix <your-prefix>" -ForegroundColor Yellow
    Write-Host "  Or set it in your azd environment:  azd env set AZURE_NAME_PREFIX <your-prefix>" -ForegroundColor Yellow
    exit 1
}

$exitCode = 0
$timestamp = (Get-Date -Format 'yyyy-MM-dd-HHmmss')
Clear-Host 

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Integration Test Runners (ACI in VNet)" -ForegroundColor Cyan
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
$hasPostgreSQL   = Test-DeployFlag 'DEPLOY_POSTGRESQL'

Write-Host ""
Write-Host "Platform Availability:" -ForegroundColor Cyan
Write-Host "  SQL Server:     $(if ($hasSqlServer)    { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasSqlServer)    { 'Green' } else { 'DarkGray' })
Write-Host "  PostgreSQL:     $(if ($hasPostgreSQL)   { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasPostgreSQL)   { 'Green' } else { 'DarkGray' })
Write-Host "  ACI:            $(if ($hasAci)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAci)           { 'Green' } else { 'DarkGray' })
Write-Host "  Batch:          $(if ($hasBatch)         { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasBatch)         { 'Green' } else { 'DarkGray' })
Write-Host "  Container Apps: $(if ($hasContainerApp)  { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasContainerApp)  { 'Green' } else { 'DarkGray' })
Write-Host "  AKS:            $(if ($hasAks)           { 'Deployed' } else { 'Not deployed' })" -ForegroundColor $(if ($hasAks)           { 'Green' } else { 'DarkGray' })
Write-Host ""

function Invoke-TestIfAvailable {
    param(
        [string]$customName,
        [string]$testFilter,
        [string]$computeLabel,
        [bool]$computeAvailable,
        [string]$databaseLabel,
        [bool]$databaseAvailable,
        [int]$timeoutMinutes = 300
    )
    
    $skipReasons = @()
    if (-not $computeAvailable) { $skipReasons += "$computeLabel compute is not deployed" }
    if (-not $databaseAvailable) { $skipReasons += "$databaseLabel database is not deployed" }
    
    if ($skipReasons.Count -gt 0) {
        Write-Host "SKIPPING [$customName]: $($skipReasons -join '; ')" -ForegroundColor Yellow
        return 0
    }
    
    .\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName $customName -testFilter $testFilter -timeoutMinutes $timeoutMinutes -timestamp $timestamp
    return $LASTEXITCODE
}

#############################################
# SQL Server tests (each requires SQL Server + specific compute)
#############################################

$exitCode += Invoke-TestIfAvailable -customName "aci" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.AciTests" `
    -computeLabel "ACI" -computeAvailable $hasAci `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer

$exitCode += Invoke-TestIfAvailable -customName "containerapp" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.ContainerAppTests" `
    -computeLabel "Container Apps" -computeAvailable $hasContainerApp `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer

$exitCode += Invoke-TestIfAvailable -customName "batchqueue" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Queue" `
    -computeLabel "Batch" -computeAvailable $hasBatch `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer

$exitCode += Invoke-TestIfAvailable -customName "aks" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.KubernetesTests" `
    -computeLabel "AKS" -computeAvailable $hasAks `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer

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
        .\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName pg -testFilter $pgTestFilter -timeoutMinutes 300 -timestamp $timestamp
        $exitCode += $LASTEXITCODE
    } else {
        Write-Host "SKIPPING [pg]: No compute platforms available for PostgreSQL tests" -ForegroundColor Yellow
    }
}

#############################################
# Additional SQL Server Batch tests
#############################################

$exitCode += Invoke-TestIfAvailable -customName "batchoverride" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Override" `
    -computeLabel "Batch" -computeAvailable $hasBatch `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer

$exitCode += Invoke-TestIfAvailable -customName "batchquery" `
    -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Query" `
    -computeLabel "Batch" -computeAvailable $hasBatch `
    -databaseLabel "SQL Server" -databaseAvailable $hasSqlServer


# Download test results 
if( (Test-Path ./testresults) -eq $false) { mkdir testresults }
az storage blob download-batch --account-name "$($prefix)storage" --source testresults --pattern "$($timestamp)*" --destination ./testresults  --auth-mode login --overwrite

    # Analyze test results with GitHub Copilot
$promptTemplate = Get-Content -Path "$PSScriptRoot\analyze-test-results-prompt.md" -Raw
$prompt = $promptTemplate -replace '\{\{timestamp\}\}', $timestamp
$output = copilot --yolo -p $prompt 2>&1

