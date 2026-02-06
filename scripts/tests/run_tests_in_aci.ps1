 param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $resourceGroupName,
    [string] $customName,
    
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
if ([string]::IsNullOrWhiteSpace($customName)) {
    $testContainerName = "$prefix-test-runner"
} else {
    $testContainerName = "$prefix-test-runner-$customName"
}
$testImageName = "sqlbuildmanager-tests"

# Create log file path
$logDir = Join-Path $repoRoot "src\TestConfig\TestResults"
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}
$logFileName = "test-results-$(Get-Date -Format 'yyyy-MM-dd-HHmmss')"
if (-not [string]::IsNullOrWhiteSpace($customName)) {
    $logFileName += "-$customName"
}
$logFileName += ".log"
$logFilePath = Join-Path $logDir $logFileName

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Integration Test Runner (ACI in VNet)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Container Name: $testContainerName" -ForegroundColor DarkGreen
Write-Host "VNet: $vnet" -ForegroundColor DarkGreen
Write-Host "Subnet: $aciSubnet" -ForegroundColor DarkGreen
Write-Host "Log File: $logFilePath" -ForegroundColor DarkGreen
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
    
    $batchScriptPath = Join-Path $repoRoot "scripts\Batch\build_and_upload_batch_fromprefix.ps1"
    $outputPath = Join-Path $repoRoot "src\TestConfig"
    
    if (Test-Path $batchScriptPath) {
        & $batchScriptPath -prefix $prefix -resourceGroupName $resourceGroupName -path $outputPath -action "BuildAndUpload"
    } else {
        Write-Host "Batch build script not found at: $batchScriptPath" -ForegroundColor Yellow
        Write-Host "Run manually: .\scripts\Batch\build_and_upload_batch_fromprefix.ps1 -prefix $prefix" -ForegroundColor Yellow
    }
   
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
Write-Host "Test filter: $testFilter" -ForegroundColor DarkGray

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

# Build command array for YAML - override entrypoint to capture exit code and upload results
$blobContainerName = "testresults"
$timestamp = Get-Date -Format 'yyyy-MM-dd-HHmmss'
$blobPath = "$testContainerName/$timestamp"

# Build the test command with filter - quote arguments containing semicolons
# Use --ResultsDirectory to capture all test output including logs
# Use html logger to capture per-test output, and --Diag for diagnostics
# Use --Blame to capture per-test diagnostic data and crash dumps
# Use tee to capture console output to a log file while still displaying it
if ($testFilter) {
    $testCmd = "dotnet vstest SqlBuildManager.Console.ExternalTest.dll '--logger:trx;LogFileName=TestResults.trx' '--logger:html;LogFileName=TestResults.html' '--logger:console;verbosity=detailed' '--TestCaseFilter:$testFilter' --ResultsDirectory:/tests/TestResults --Diag:/tests/TestResults/diag.log 2>&1 | tee /tests/TestResults/console-output.log"
} else {
    $testCmd = "dotnet vstest SqlBuildManager.Console.ExternalTest.dll '--logger:trx;LogFileName=TestResults.trx' '--logger:html;LogFileName=TestResults.html' '--logger:console;verbosity=detailed' --ResultsDirectory:/tests/TestResults --Diag:/tests/TestResults/diag.log 2>&1 | tee /tests/TestResults/console-output.log"
}

# Upload entire TestResults directory (includes TRX and log attachments)
$uploadCmd = "az storage blob upload-batch --account-name $storageAccountName --destination $blobContainerName --source /tests/TestResults --destination-path $blobPath --auth-mode login --overwrite"

# Build Kubernetes pre-requisite commands if test filter contains "Kubernetes"
$aksPreCmd = ""
if ($testFilter -like "*Kubernetes*") {
    $aksClusterName = "$($prefix)aks"
    $aksPreCmd = "az aks install-cli; az aks get-credentials --resource-group $resourceGroupName --name $aksClusterName --overwrite-existing; "
    Write-Host "Kubernetes tests detected - will install kubectl and get AKS credentials" -ForegroundColor DarkGreen
}

# Create results directory first, then run tests, capture exit code, login and upload
# Use PIPESTATUS to get the exit code of dotnet vstest (not tee)
# Exit with the test exit code so the container terminates with the correct status
$shellCmd = "mkdir -p /tests/TestResults; az login --identity --client-id `$AZURE_CLIENT_ID; $aksPreCmd$testCmd; TEST_EXIT_CODE=`${PIPESTATUS[0]}; echo TEST_EXIT_CODE=`$TEST_EXIT_CODE;  $uploadCmd; exit `$TEST_EXIT_CODE"

$commandYaml = @"
      - /bin/bash
      - -c
      - "$shellCmd"
"@

# Build environment variables for YAML
$envVarsYaml = @"
      - name: AZURE_CLIENT_ID
        value: $($identity.clientId)
"@

if ($testFilter) {
    $envVarsYaml += @"

      - name: TEST_FILTER
        value: $testFilter
"@
}

# Generate YAML deployment file
$location = az group show --name $resourceGroupName --query location -o tsv
$aciYaml = @"
apiVersion: 2021-09-01
location: $location
name: $testContainerName
identity:
  type: UserAssigned
  userAssignedIdentities:
    $($identity.id): {}
properties:
  imageRegistryCredentials:
  - server: $acrLoginServer
    identity: $($identity.id)
  containers:
  - name: $testContainerName
    properties:
      image: $fullImageName
      command:
$commandYaml
      environmentVariables:
$envVarsYaml
      resources:
        requests:
          cpu: 2
          memoryInGb: 4
  osType: Linux
  restartPolicy: Never
  subnetIds:
  - id: $subnetId
"@

# Write YAML to temp file
$yamlFilePath = Join-Path $env:TEMP "aci-test-runner-$(Get-Date -Format 'yyyyMMddHHmmss').yaml"
$aciYaml | Set-Content -Path $yamlFilePath -Encoding UTF8

Write-Host "Generated ACI YAML deployment file: $yamlFilePath" -ForegroundColor DarkGray
Write-Host "YAML Contents:" -ForegroundColor DarkGray
Write-Host $aciYaml -ForegroundColor DarkGray
Write-Host ""
Write-Host "Deploying test container to ACI..." -ForegroundColor DarkGreen
# Deploy using YAML file
az container create --resource-group $resourceGroupName --file $yamlFilePath -o none

# Clean up temp YAML file
Remove-Item $yamlFilePath -Force -ErrorAction SilentlyContinue

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create test container" -ForegroundColor Red
    exit 1
}

Write-Host "Container deployed. Waiting for tests to complete..." -ForegroundColor DarkGreen
Write-Host ""

#############################################
# Wait for tests to complete (container terminates when done)
#############################################
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Monitoring Test Execution" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$startTime = Get-Date
$timeoutTime = $startTime.AddMinutes($timeoutMinutes)
$lastLogTime = $startTime
$testsCompleted = $false
$testExitCode = $null

while ($true) {
    $container = az container show --name $testContainerName --resource-group $resourceGroupName 2>$null | ConvertFrom-Json -Depth 10
    $state = $null
    $state2 = $null
    if ($null -ne $container -and $null -ne $container.instanceView) {
        $state = $container.containers.instanceView.currentState.detailStatus
        $state2 = $container.containers.instanceView.currentState.state
    }
    
    # Stream logs periodically and check for test completion
    $currentTime = Get-Date
    if (($currentTime - $lastLogTime).TotalSeconds -ge 10) {
        Write-Host ""
        Write-Host "--- Container Logs ($(Get-Date -Format 'HH:mm:ss')) ---" -ForegroundColor DarkGray
        $recentLogs = az container logs --name $testContainerName --resource-group $resourceGroupName 2>$null
        if ($null -ne $recentLogs) {
            $recentLogs | Select-Object -Last 20
            
            # Join array to string for regex matching
            $logString = $recentLogs -join "`n"
            # Extract exit code from logs if present
            if ($logString -match "TEST_EXIT_CODE=(\d+)") {
                $testExitCode = [int]$Matches[1]
            }
        }
        $lastLogTime = $currentTime
    }
    
    # Container terminates when tests and upload are complete
    if ($state -eq "Terminated" -or $state -eq "Completed" -or $state2 -eq "Terminated" -or $state2 -eq "Completed") {
        $testsCompleted = $true
        Write-Host ""
        Write-Host "Container terminated. Tests and upload complete." -ForegroundColor Cyan
        break
    }
    
    if ($state -eq "Failed" -or $state2 -eq "Failed") {
        Write-Host ""
        Write-Host "Container failed (state: $state)" -ForegroundColor Red
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

# Get container state with null checks
$container = az container show --name $testContainerName --resource-group $resourceGroupName 2>$null | ConvertFrom-Json -Depth 10
$containerState = "Unknown"
$containerExitCode = $null

if ($null -ne $container -and $null -ne $container.PSObject -and $container.PSObject.Properties.Name -contains 'containers') {
    # $containers = $container.containers
#     if ($null -ne $containers -and $containers.Count -gt 0) {
        $containerInstance = $container.containers
        if ($null -ne $containerInstance -and $null -ne $containerInstance.instanceView -and $null -ne $containerInstance.instanceView.currentState) {
            $containerState = $containerInstance.containers.instanceView.currentState.detailStatus
            $containerExitCode = $containerInstance.containers.instanceView.currentState.detailStatus
        }
#     }
}

# Use the exit code we extracted from logs, or default to container exit code
if ($null -ne $testExitCode) {
    $exitCode = $testExitCode
} elseif ($null -ne $containerExitCode) {
    $exitCode = $containerExitCode
} else {
    $exitCode = 1
}

Write-Host ""
Write-Host "Container State: $containerState" -ForegroundColor DarkGreen
Write-Host "Test Exit Code: $exitCode" -ForegroundColor DarkGreen
Write-Host ""

# Get full logs and save to file
Write-Host "--- Full Test Output ---" -ForegroundColor Cyan
$testLogs = az container logs --name $testContainerName --resource-group $resourceGroupName 2>&1 | Out-String

# Display logs to console
Write-Host $testLogs

# Save logs to file with proper formatting
$logHeader = @"
============================================
Integration Test Results
============================================
Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Container Name: $testContainerName
Resource Group: $resourceGroupName
Test Filter: $(if ($testFilter) { $testFilter } else { "(all tests)" })
Container State: $containerState
Exit Code: $exitCode
============================================

"@

$logHeader | Set-Content -Path $logFilePath -Encoding UTF8
$testLogs | Add-Content -Path $logFilePath -Encoding UTF8

# Test results are now uploaded to blob storage
Write-Host ""
Write-Host "Test results uploaded to blob storage: $blobContainerName/$blobPath" -ForegroundColor Cyan
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
