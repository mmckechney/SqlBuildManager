 param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $resourceGroupName,
    
    [string] $testFilter = "",
    
    [string] $imageTag = "test-runner",
    
    [switch] $buildImage,
    
    [switch] $keepContainer,
    
    [int] $timeoutMinutes = 30
)

<#
.SYNOPSIS
    Runs integration tests in Azure Container Instances within the VNet.

.DESCRIPTION
    This script builds and deploys a test container to ACI within the VNet subnet,
    allowing tests to run with access to resources via VNet rules and private endpoints.
    
    The container runs the tests, captures the results, and returns pass/fail status.

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group name (defaults to {prefix}-rg).

.PARAMETER testFilter
    Optional test filter (e.g., "FullyQualifiedName~ContainerApp" or "TestCategory=Integration").
    If not specified, runs all tests in SqlBuildManager.Console.ExternalTest.

.PARAMETER imageTag
    The container image tag to use (default: test-runner).

.PARAMETER buildImage
    If specified, builds and pushes the test container image before running.

.PARAMETER keepContainer
    If specified, keeps the ACI container after test completion for debugging.

.PARAMETER timeoutMinutes
    Maximum time to wait for tests to complete (default: 30 minutes).

.EXAMPLE
    # Build image and run all tests
    .\run_tests_in_aci.ps1 -prefix mwm025 -buildImage

.EXAMPLE
    # Run only ContainerApp tests (image already built)
    .\run_tests_in_aci.ps1 -prefix mwm025 -testFilter "FullyQualifiedName~ContainerApp"

.EXAMPLE
    # Run ACI tests and keep container for debugging
    .\run_tests_in_aci.ps1 -prefix mwm025 -testFilter "FullyQualifiedName~AciTests" -keepContainer
#>

$ErrorActionPreference = "Stop"

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

#############################################
# Get resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

# Container name for test runner
$testContainerName = "$prefix-test-runner"
$testImageName = "sqlbuildmanager-tests"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Integration Test Runner (ACI in VNet)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Container Name: $testContainerName" -ForegroundColor DarkGreen
Write-Host "VNet: $vnet" -ForegroundColor DarkGreen
Write-Host "Subnet: $aciSubnet" -ForegroundColor DarkGreen
if ($testFilter) {
    Write-Host "Test Filter: $testFilter" -ForegroundColor DarkGreen
} else {
    Write-Host "Test Filter: (all tests)" -ForegroundColor DarkGreen
}
Write-Host ""

# Get resource information
$subscriptionId = az account show --query id --output tsv
$identity = az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json
$acrLoginServer = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer

Write-Host "Using Managed Identity: $identityName (ClientId: $($identity.clientId))" -ForegroundColor DarkGreen
Write-Host "Using Container Registry: $acrLoginServer" -ForegroundColor DarkGreen
Write-Host ""

#############################################
# Build and push test image if requested
#############################################
if ($buildImage) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Building Test Container Image" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $dockerfilePath = Join-Path $repoRoot "src\Dockerfile.tests"
    $contextPath = Join-Path $repoRoot "src"
    
    Write-Host "Building image using ACR Build..." -ForegroundColor DarkGreen
    Write-Host "  Dockerfile: $dockerfilePath" -ForegroundColor DarkGray
    Write-Host "  Context: $contextPath" -ForegroundColor DarkGray
    
    az acr build `
        --registry $containerRegistryName `
        --resource-group $resourceGroupName `
        --image "${testImageName}:${imageTag}" `
        --file $dockerfilePath `
        $contextPath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to build test container image" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Test image built successfully: $acrLoginServer/${testImageName}:${imageTag}" -ForegroundColor Green
    Write-Host ""
}

#############################################
# Clean up any existing test container
#############################################
Write-Host "Checking for existing test container..." -ForegroundColor DarkGreen
$existingContainer = az container show --name $testContainerName --resource-group $resourceGroupName 2>$null
if ($existingContainer) {
    Write-Host "Deleting existing test container..." -ForegroundColor Yellow
    az container delete --name $testContainerName --resource-group $resourceGroupName --yes -o none
    Start-Sleep -Seconds 5
}

#############################################
# Get subnet ID for VNet deployment
#############################################
$subnetId = az network vnet subnet show `
    --resource-group $resourceGroupName `
    --vnet-name $vnet `
    --name $aciSubnet `
    --query id -o tsv

#############################################
# Build container command with test filter
#############################################
# Use vstest for running pre-built test assemblies
$containerCommand = @("dotnet", "vstest", "SqlBuildManager.Console.ExternalTest.dll", "--logger:trx;LogFileName=TestResults.trx", "--logger:console;verbosity=detailed")

if ($testFilter) {
    $containerCommand += "--TestCaseFilter:$testFilter"
}

# Convert to JSON for ACI (not used, but kept for reference)
$commandJson = $containerCommand | ConvertTo-Json -Compress

#############################################
# Deploy test container to ACI in VNet
#############################################
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deploying Test Container to ACI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$fullImageName = "$acrLoginServer/${testImageName}:${imageTag}"
Write-Host "Image: $fullImageName" -ForegroundColor DarkGreen
Write-Host "Deploying to VNet subnet for network access..." -ForegroundColor DarkGreen
Write-Host ""

# Create container with managed identity in VNet
az container create `
    --name $testContainerName `
    --resource-group $resourceGroupName `
    --image $fullImageName `
    --cpu 2 `
    --memory 4 `
    --restart-policy Never `
    --assign-identity $identity.id `
    --acr-identity $identity.id `
    --subnet $subnetId `
    --command-line $containerCommand `
    --environment-variables AZURE_CLIENT_ID=$($identity.clientId) `
    -o none

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create test container" -ForegroundColor Red
    exit 1
}

Write-Host "Container deployed. Waiting for tests to complete..." -ForegroundColor DarkGreen
Write-Host ""

#############################################
# Wait for container to complete
#############################################
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Monitoring Test Execution" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$startTime = Get-Date
$timeoutTime = $startTime.AddMinutes($timeoutMinutes)
$lastLogTime = $startTime

while ($true) {
    $container = az container show --name $testContainerName --resource-group $resourceGroupName | ConvertFrom-Json
    $state = $container.instanceView.state
    
    # Stream logs periodically
    $currentTime = Get-Date
    if (($currentTime - $lastLogTime).TotalSeconds -ge 10) {
        Write-Host ""
        Write-Host "--- Container Logs ($(Get-Date -Format 'HH:mm:ss')) ---" -ForegroundColor DarkGray
        az container logs --name $testContainerName --resource-group $resourceGroupName 2>$null | Select-Object -Last 20
        $lastLogTime = $currentTime
    }
    
    if ($state -eq "Terminated" -or $state -eq "Succeeded" -or $state -eq "Failed") {
        break
    }
    
    if ($currentTime -gt $timeoutTime) {
        Write-Host ""
        Write-Host "ERROR: Test execution timed out after $timeoutMinutes minutes" -ForegroundColor Red
        
        # Get final logs before cleanup
        Write-Host ""
        Write-Host "Final container logs:" -ForegroundColor Yellow
        az container logs --name $testContainerName --resource-group $resourceGroupName
        
        if (-not $keepContainer) {
            az container delete --name $testContainerName --resource-group $resourceGroupName --yes -o none
        }
        exit 1
    }
    
    Write-Host "." -NoNewline
    Start-Sleep -Seconds 5
}

Write-Host ""
Write-Host ""

#############################################
# Get final results
#############################################
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Get container exit code
$container = az container show --name $testContainerName --resource-group $resourceGroupName | ConvertFrom-Json
$exitCode = $container.containers[0].instanceView.currentState.exitCode
$containerState = $container.containers[0].instanceView.currentState.state

Write-Host ""
Write-Host "Container State: $containerState" -ForegroundColor DarkGreen
Write-Host "Exit Code: $exitCode" -ForegroundColor DarkGreen
Write-Host ""

# Get full logs
Write-Host "--- Full Test Output ---" -ForegroundColor Cyan
az container logs --name $testContainerName --resource-group $resourceGroupName

Write-Host ""
Write-Host ""

#############################################
# Cleanup and report
#############################################
if ($keepContainer) {
    Write-Host "Container kept for debugging: $testContainerName" -ForegroundColor Yellow
    Write-Host "To view logs: az container logs --name $testContainerName --resource-group $resourceGroupName" -ForegroundColor DarkGray
    Write-Host "To delete: az container delete --name $testContainerName --resource-group $resourceGroupName --yes" -ForegroundColor DarkGray
} else {
    Write-Host "Cleaning up test container..." -ForegroundColor DarkGreen
    az container delete --name $testContainerName --resource-group $resourceGroupName --yes -o none
}

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "TESTS PASSED" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    exit 0
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "TESTS FAILED (Exit Code: $exitCode)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit $exitCode
}
