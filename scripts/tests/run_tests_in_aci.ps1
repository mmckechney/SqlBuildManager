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

#############################################
# Function to parse and summarize test results
#############################################
$script:lastRenderLineCount = 0

# Accumulated test results across all log fetches (survives log truncation)
$script:allPassed = [System.Collections.Generic.List[string]]::new()
$script:allFailed = [System.Collections.Generic.List[string]]::new()
$script:allSkipped = [System.Collections.Generic.List[string]]::new()
$script:seenTests = [System.Collections.Generic.HashSet[string]]::new()
$script:monitoringStartTime = $null

function Show-TestSummary {
    param(
        [string[]]$logs,
        [switch]$refresh
    )
    
    # Parse new results from current log batch and merge into accumulated state
    foreach ($line in $logs) {
        if ($line -match '^\s{2}Passed\s+(.+?)(?:\s+\[|$)') {
            $testName = $matches[1]
            if (-not $script:seenTests.Contains($testName)) {
                $script:seenTests.Add($testName) | Out-Null
                $script:allPassed.Add($testName)
            }
        }
        elseif ($line -match '^\s{2}Failed\s+(.+?)(?:\s+\[|$)') {
            $testName = $matches[1]
            if (-not $script:seenTests.Contains($testName)) {
                $script:seenTests.Add($testName) | Out-Null
                $script:allFailed.Add($testName)
            }
        }
        elseif ($line -match '^\s{2}Skipped\s+(.+?)(?:$|\s)') {
            $testName = $matches[1]
            if (-not $script:seenTests.Contains($testName)) {
                $script:seenTests.Add($testName) | Out-Null
                $script:allSkipped.Add($testName)
            }
        }
    }
    
    # Use accumulated results for display
    $passed = $script:allPassed
    $failed = $script:allFailed
    $skipped = $script:allSkipped
    
    # Determine console width for padding (prevents wrapping so line count = visual row count)
    $consoleWidth = [Math]::Max(40, [Console]::WindowWidth)
    
    # If refreshing, move cursor up by the number of lines we rendered last time
    # Using relative movement so it works correctly even when the buffer has scrolled
    if ($refresh -and $script:lastRenderLineCount -gt 0) {
        $targetRow = [Math]::Max(0, [Console]::CursorTop - $script:lastRenderLineCount)
        [Console]::SetCursorPosition(0, $targetRow)
    }
    
    # Build elapsed time string
    $elapsedStr = ""
    if ($null -ne $script:monitoringStartTime) {
        $elapsed = (Get-Date) - $script:monitoringStartTime
        $elapsedStr = " | Elapsed: {0:d2}:{1:d2}:{2:d2}" -f [int]$elapsed.TotalHours, $elapsed.Minutes, $elapsed.Seconds
    }
    
    # Build the output as a string buffer
    $output = @()
    
    if (-not $refresh) {
        $output += ""
    }
    
    $output += "========================================================="
    $output += "Test Summary ($(Get-Date -Format 'HH:mm:ss')$elapsedStr)"
    $output += "========================================================="
    $output += ""
    
    $output += "$($passed.Count) Passed"
    if ($passed.Count -gt 0 -and $passed.Count -le 5) {
        foreach ($test in $passed) {
            $output += " - $test"
        }
    }
    elseif ($passed.Count -gt 5) {
        $output += " (showing last 5 of $($passed.Count))"
        foreach ($test in ($passed | Select-Object -Last 5)) {
            $output += " - $test"
        }
    }
    
    $output += ""
    $output += "$($failed.Count) Failed"
    if ($failed.Count -gt 0) {
        foreach ($test in $failed) {
            $output += " - $test"
        }
    }
    
    $output += ""
    $output += "$($skipped.Count) Skipped"
    if ($skipped.Count -gt 0 -and $skipped.Count -le 10) {
        foreach ($test in $skipped) {
            $output += " - $test"
        }
    }
    elseif ($skipped.Count -gt 10) {
        $output += " (list truncated - showing first 5)"
        foreach ($test in ($skipped | Select-Object -First 5)) {
            $output += " - $test"
        }
    }
    $output += ""
    
    # If previous render had more lines, pad with blank lines to overwrite them
    $prevCount = $script:lastRenderLineCount
    while ($output.Count -lt $prevCount) {
        $output += ""
    }
    
    # Print the output with appropriate colors, padding each line to full width
    # Padding to consoleWidth-1 prevents wrapping, so line count = visual row count
    $currentSection = ""
    foreach ($line in $output) {
        # Track which section we are in for coloring " - " items
        if ($line -match "^\d+ Passed") { $currentSection = "passed" }
        elseif ($line -match "^\d+ Failed") { $currentSection = "failed" }
        elseif ($line -match "^\d+ Skipped") { $currentSection = "skipped" }
        
        $paddedLine = $line.PadRight($consoleWidth - 1).Substring(0, $consoleWidth - 1)
        if ($line -match "^Test Summary") {
            Write-Host $paddedLine -ForegroundColor Cyan
        }
        elseif ($line -match "^=+") {
            Write-Host $paddedLine -ForegroundColor Cyan
        }
        elseif ($line -match "^\d+ Passed") {
            Write-Host $paddedLine -ForegroundColor Green
        }
        elseif ($line -match "^\d+ Failed") {
            Write-Host $paddedLine -ForegroundColor Red
        }
        elseif ($line -match "^\d+ Skipped") {
            Write-Host $paddedLine -ForegroundColor Yellow
        }
        elseif ($line -match "^ - " -and $currentSection -eq "failed") {
            Write-Host $paddedLine -ForegroundColor DarkGray
        }
        elseif ($line -match "^ - ") {
            Write-Host $paddedLine -ForegroundColor DarkGray
        }
        elseif ($line -match "truncated|showing last") {
            Write-Host $paddedLine -ForegroundColor DarkGray
        }
        else {
            Write-Host $paddedLine
        }
    }
    
    # Record how many lines we rendered so next refresh can move back the right amount
    $script:lastRenderLineCount = $output.Count
}

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
    
    $testImageScriptPath = Join-Path $repoRoot "scripts\ContainerRegistry\build_container_registry_testimage.ps1"
    $outputPath = Join-Path $repoRoot "src\TestConfig"
    
    if (Test-Path $testImageScriptPath) {
        & $testImageScriptPath -prefix $prefix -resourceGroupName $resourceGroupName 
    } else {
        Write-Host "Test image build script not found at: $testImageScriptPath" -ForegroundColor Yellow
        Write-Host "Run manually: .\scripts\ContainerRegistry\build_container_registry_testimage.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $outputPath -action BuildAndUpload" -ForegroundColor Yellow
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
$script:monitoringStartTime = $startTime
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
        $recentLogs = az container logs --name $testContainerName --resource-group $resourceGroupName 2>$null
        if ($null -ne $recentLogs) {
            # Join array to string for regex matching
            $logString = $recentLogs -join "`n"
            # Extract exit code from logs if present
            if ($logString -match "TEST_EXIT_CODE=(\d+)") {
                $testExitCode = [int]$Matches[1]
            }
            
            # Show refreshing test summary
            if ($script:lastRenderLineCount -eq 0) {
                # First time - add a header
                Write-Host ""
                Write-Host "--- Live Test Progress ---" -ForegroundColor DarkGray
                Write-Host ""
            }
            Show-TestSummary -logs $recentLogs -refresh
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
        
        # Get final logs and show summary
        Write-Host ""
        Write-Host "Retrieving test logs and generating summary..." -ForegroundColor Yellow
        $testLogs = az container logs --name $testContainerName --resource-group $resourceGroupName 2>$null
        if ($testLogs) {
            Show-TestSummary -logs $testLogs
        }
        else {
            Write-Host "No test logs available yet" -ForegroundColor Yellow
        }
        
        if (-not $keepContainer) {
            az container delete --name $testContainerName --resource-group $resourceGroupName --yes -o none
        }
        exit 1
    }
    
    Start-Sleep -Seconds 10
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
$testLogs = az container logs --name $testContainerName --resource-group $resourceGroupName 2>&1 | Out-String

# Show test summary instead of full logs
if ($testLogs) {
    $testLogsArray = $testLogs -split "`n"
    Show-TestSummary -logs $testLogsArray
}
else {
    Write-Host "No test logs available" -ForegroundColor Yellow
}

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
