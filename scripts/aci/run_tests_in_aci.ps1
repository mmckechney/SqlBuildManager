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
    
    $contextPath = Join-Path $repoRoot "src"
    $dockerfileName = "Dockerfile.tests"
    
    Write-Host "Building image using ACR Build..." -ForegroundColor DarkGreen
    Write-Host "  Context: $contextPath" -ForegroundColor DarkGray
    
    # Copy source to temp folder excluding .vs and other problematic folders
    $tempContext = Join-Path $env:TEMP "sbm-test-build-$(Get-Date -Format 'yyyyMMddHHmmss')"
    New-Item -ItemType Directory -Path $tempContext -Force | Out-Null
    
    Write-Host "Copying source to temp location (excluding .vs, bin, obj)..." -ForegroundColor DarkGray
    
    # Use Get-ChildItem with exclusions and Copy-Item
    Get-ChildItem -Path $contextPath -Exclude '.vs','bin','obj','TestResults' | ForEach-Object {
        if ($_.PSIsContainer) {
            # For directories, use robocopy to handle subdirectory exclusions
            if ($_.Name -notin @('.vs', 'bin', 'obj', 'TestResults')) {
                $destDir = Join-Path $tempContext $_.Name
                robocopy $_.FullName $destDir /E /XD .vs bin obj TestResults /XF *.user /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
            }
        } else {
            # For files, just copy them
            Copy-Item $_.FullName -Destination $tempContext
        }
    }
    
    # Remove .dockerignore if it exists (it might be excluding things we need)
    $dockerignorePath = Join-Path $tempContext ".dockerignore"
    if (Test-Path $dockerignorePath) {
        Write-Host "Removing .dockerignore to prevent exclusions..." -ForegroundColor DarkGray
        Remove-Item $dockerignorePath -Force
    }
    
    # Verify Dockerfile exists
    $dockerfilePath = Join-Path $tempContext $dockerfileName
    Write-Host "Checking for Dockerfile at: $dockerfilePath" -ForegroundColor DarkGray
    
    if (-not (Test-Path $dockerfilePath)) {
        Write-Host "ERROR: Dockerfile.tests not found at $dockerfilePath" -ForegroundColor Red
        Write-Host "Top-level contents of temp folder:" -ForegroundColor Yellow
        Get-ChildItem $tempContext -Name
        exit 1
    }
    
    Write-Host "Dockerfile found. Starting ACR build..." -ForegroundColor DarkGreen
    Write-Host "Temp context contents:" -ForegroundColor DarkGray
    Get-ChildItem $tempContext -Name | Where-Object { $_ -like "Dockerfile*" -or $_ -like "*.csproj" }
    
    try {
        # Use full path to dockerfile
        az acr build `
            --registry $containerRegistryName `
            --resource-group $resourceGroupName `
            --image "${testImageName}:${imageTag}" `
            --file "$tempContext\$dockerfileName" `
            $tempContext
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Failed to build test container image" -ForegroundColor Red
            exit 1
        }
    }
    finally {
        # Clean up temp folder
        if (Test-Path $tempContext) {
            Remove-Item $tempContext -Recurse -Force -ErrorAction SilentlyContinue
        }
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
# Build test arguments - filter will be passed via environment variable
$testArgs = "vstest SqlBuildManager.Console.ExternalTest.dll --logger:trx;LogFileName=TestResults.trx --logger:console;verbosity=detailed"

# Build environment variables list
$envVars = "AZURE_CLIENT_ID=$($identity.clientId)"
if ($testFilter) {
    $envVars += " TEST_FILTER=$testFilter"
    $testArgs += ' --TestCaseFilter:$TEST_FILTER'
}

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

# Create container with managed identity in VNet
# Command runs tests then keeps container alive for debugging/TRX retrieval
# The shell script uses environment variable for test filter to avoid quoting issues
if ($testFilter) {
    $shellCmd = 'dotnet vstest SqlBuildManager.Console.ExternalTest.dll --logger:trx;LogFileName=TestResults.trx --logger:console;verbosity=detailed --TestCaseFilter:$TEST_FILTER; echo TEST_EXIT_CODE=$?; tail -f /dev/null'
} else {
    $shellCmd = 'dotnet vstest SqlBuildManager.Console.ExternalTest.dll --logger:trx;LogFileName=TestResults.trx --logger:console;verbosity=detailed; echo TEST_EXIT_CODE=$?; tail -f /dev/null'
}

# Build az container create command with proper environment variable handling
$azArgs = @(
    "container", "create",
    "--name", $testContainerName,
    "--resource-group", $resourceGroupName,
    "--image", $fullImageName,
    "--os-type", "Linux",
    "--cpu", "2",
    "--memory", "4",
    "--restart-policy", "Never",
    "--assign-identity", $identity.id,
    "--acr-identity", $identity.id,
    "--subnet", $subnetId,
    "--command-line", "/bin/sh -c '$shellCmd'",
    "--environment-variables", "AZURE_CLIENT_ID=$($identity.clientId)"
)

if ($testFilter) {
    $azArgs += "TEST_FILTER=$testFilter"
}

$azArgs += @("-o", "none")

& az @azArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create test container" -ForegroundColor Red
    exit 1
}

Write-Host "Container deployed. Waiting for tests to complete..." -ForegroundColor DarkGreen
Write-Host ""

#############################################
# Wait for tests to complete (container stays running)
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
    $container = az container show --name $testContainerName --resource-group $resourceGroupName | ConvertFrom-Json
    $state = $container.instanceView.state
    
    # Stream logs periodically and check for test completion
    $currentTime = Get-Date
    if (($currentTime - $lastLogTime).TotalSeconds -ge 10) {
        Write-Host ""
        Write-Host "--- Container Logs ($(Get-Date -Format 'HH:mm:ss')) ---" -ForegroundColor DarkGray
        $recentLogs = az container logs --name $testContainerName --resource-group $resourceGroupName 2>$null
        $recentLogs | Select-Object -Last 20
        $lastLogTime = $currentTime
        
        # Check if tests have completed (look for "Test Run" in output)
        if ($recentLogs -match "Test Run (Passed|Failed)") {
            $testsCompleted = $true
            # Extract exit code from logs
            if ($recentLogs -match "TEST_EXIT_CODE=(\d+)") {
                $testExitCode = [int]$Matches[1]
            }
            Write-Host ""
            Write-Host "Tests completed! Retrieving results..." -ForegroundColor Cyan
            break
        }
    }
    
    # If container terminated unexpectedly, break out
    if ($state -eq "Terminated" -or $state -eq "Failed") {
        Write-Host ""
        Write-Host "Container terminated unexpectedly (state: $state)" -ForegroundColor Yellow
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

# Get container state
$container = az container show --name $testContainerName --resource-group $resourceGroupName | ConvertFrom-Json
$containerState = $container.containers[0].instanceView.currentState.state

# Use the exit code we extracted from logs, or default to container exit code
if ($null -eq $testExitCode) {
    $exitCode = $container.containers[0].instanceView.currentState.exitCode
    if ($null -eq $exitCode) { $exitCode = 1 }
} else {
    $exitCode = $testExitCode
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

# Try to retrieve the TestResults.trx file from the container
Write-Host ""
Write-Host "Retrieving TestResults.trx from container..." -ForegroundColor DarkGreen

$trxFilePath = $logFilePath -replace '\.log$', '.trx'
try {
    # Use az container exec to cat the trx file contents
    $trxContent = az container exec `
        --name $testContainerName `
        --resource-group $resourceGroupName `
        --exec-command "cat /tests/TestResults/TestResults.trx" `
        2>&1 | Out-String
    
    if ($LASTEXITCODE -eq 0 -and $trxContent -match '<\?xml') {
        # Extract just the XML content (remove any shell prompts or extra output)
        $xmlStart = $trxContent.IndexOf('<?xml')
        if ($xmlStart -ge 0) {
            $trxContent = $trxContent.Substring($xmlStart)
            # Remove any trailing shell output after the XML
            $xmlEnd = $trxContent.LastIndexOf('</TestRun>')
            if ($xmlEnd -gt 0) {
                $trxContent = $trxContent.Substring(0, $xmlEnd + '</TestRun>'.Length)
            }
        }
        
        $trxContent | Set-Content -Path $trxFilePath -Encoding UTF8
        Write-Host "TestResults.trx saved to: $trxFilePath" -ForegroundColor Cyan
        
        # Also append a summary to the log file
        Add-Content -Path $logFilePath -Value "`n`n============================================"
        Add-Content -Path $logFilePath -Value "TestResults.trx Content"
        Add-Content -Path $logFilePath -Value "============================================`n"
        Add-Content -Path $logFilePath -Value $trxContent
    } else {
        Write-Host "Could not retrieve TestResults.trx file (container may have terminated)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Warning: Could not retrieve TestResults.trx: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Test results saved to: $logFilePath" -ForegroundColor Cyan
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
