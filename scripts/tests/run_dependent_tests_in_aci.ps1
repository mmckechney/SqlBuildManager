 param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $resourceGroupName,
    [string] $customName = "dependent",
    
    [string] $testFilter = "",
    
    [string] $imageTag = "dependent-test-runner",
    
    [string] $sqlPassword = "SqlBM_Test#2026!",

    [switch] $buildImage,
    
    [switch] $keepContainer,
    
    [int] $timeoutMinutes = 300
)

<#
.SYNOPSIS
    Runs Dependent.UnitTest integration tests in ACI with a SQL Server sidecar.

.DESCRIPTION
    Deploys a container group to ACI with two containers:
    1. SQL Server 2022 on Linux (sidecar) - provides the database instance
    2. Test runner - runs the Dependent.UnitTest projects against the SQL Server sidecar
    
    The test runner waits for SQL Server to be ready, then runs all 5 Dependent.UnitTest
    projects. SqlSync.SqlBuild.Dependent.UnitTest runs first to create the test databases.
    
    Environment variables SBM_TEST_SQL_SERVER, SBM_TEST_SQL_USER, and SBM_TEST_SQL_PASSWORD
    are set automatically to connect to the sidecar SQL Server instance.

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER sqlPassword
    SA password for the SQL Server sidecar (must meet SQL Server complexity requirements).

.PARAMETER buildImage
    If specified, builds and pushes the test container image before running.

.PARAMETER keepContainer
    If specified, keeps the ACI container group after test completion for debugging.

.EXAMPLE
    # Build image and run all dependent tests
    .\run_dependent_tests_in_aci.ps1 -prefix mwm025 -buildImage

.EXAMPLE
    # Run with custom SQL password
    .\run_dependent_tests_in_aci.ps1 -prefix mwm025 -sqlPassword "MyStr0ng!Pass"
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
    
    $output += "==============================================================="
    $output += "Test Summary ($(Get-Date -Format 'HH:mm:ss')$elapsedStr)"
    $output += "==============================================================="
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
            Write-Host $paddedLine -ForegroundColor Yellow
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

$testContainerName = "$prefix-test-runner-$customName"
$testImageName = "sqlbuildmanager-dependent-tests"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Dependent Test Runner (ACI + SQL Sidecar)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Container Name: $testContainerName" -ForegroundColor DarkGreen
if ($testFilter) {
    Write-Host "Test Filter: $testFilter" -ForegroundColor DarkGreen
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
    Write-Host "Building Dependent Test Container Image" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $srcPath = Join-Path $repoRoot "src"
    
    # Build using ACR (same pattern as build_container_registry_testimage.ps1)
    $tmpDir = Join-Path $env:TEMP "sbm-dependent-tests-$(Get-Date -Format 'yyyyMMddHHmmss')"
    New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null
    
    # Copy source, excluding build artifacts
    Write-Host "Copying source files..." -ForegroundColor DarkGreen
    $excludeDirs = @('.vs', 'bin', 'obj', 'TestResults')
    Get-ChildItem -Path $srcPath -Recurse -File | Where-Object {
        $relativePath = $_.FullName.Substring($srcPath.Length)
        $excluded = $false
        foreach ($dir in $excludeDirs) {
            if ($relativePath -like "*\$dir\*" -or $relativePath -like "*/$dir/*") {
                $excluded = $true
                break
            }
        }
        -not $excluded
    } | ForEach-Object {
        $destPath = Join-Path $tmpDir $_.FullName.Substring($srcPath.Length)
        $destDir = Split-Path $destPath -Parent
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        Copy-Item $_.FullName -Destination $destPath
    }
    
    # Remove .dockerignore if present
    $dockerIgnore = Join-Path $tmpDir ".dockerignore"
    if (Test-Path $dockerIgnore) { Remove-Item $dockerIgnore -Force }
    
    Write-Host "Building image via ACR..." -ForegroundColor DarkGreen
    az acr build --registry $containerRegistryName `
        --resource-group $resourceGroupName `
        --image "${testImageName}:${imageTag}" `
        --file (Join-Path $tmpDir "Dockerfile.dependent-tests") `
        $tmpDir
    
    # Cleanup
    Remove-Item -Path $tmpDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Image built and pushed: ${testImageName}:${imageTag}" -ForegroundColor Green
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
# Build container commands
#############################################
$blobContainerName = "testresults"
$timestamp = Get-Date -Format 'yyyy-MM-dd-HHmmss'
$blobPath = "$testContainerName/$timestamp"

$uploadCmd = "az storage blob upload-batch --account-name $storageAccountName --destination $blobContainerName --source /tests/TestResults --destination-path $blobPath --auth-mode login --overwrite"

# Test runner: login to Azure, run tests, upload results
$testShellCmd = "az login --identity --client-id `$AZURE_CLIENT_ID; /tests/run-tests.sh; TEST_EXIT_CODE=`$?; echo TEST_EXIT_CODE=`$TEST_EXIT_CODE; $uploadCmd; exit `$TEST_EXIT_CODE"

$fullImageName = "$acrLoginServer/${testImageName}:${imageTag}"

#############################################
# Deploy container group with SQL sidecar
#############################################
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deploying Container Group to ACI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Image: $fullImageName" -ForegroundColor DarkGreen
Write-Host "SQL Server: mcr.microsoft.com/mssql/server:2022-latest" -ForegroundColor DarkGreen
Write-Host ""

$location = az group show --name $resourceGroupName --query location -o tsv

# Build environment variables for test filter
$testFilterEnvVar = ""
if ($testFilter) {
    $testFilterEnvVar = @"

      - name: TEST_FILTER
        value: $testFilter
"@
}

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
  - name: sql-server
    properties:
      image: mcr.microsoft.com/mssql/server:2022-latest
      environmentVariables:
      - name: ACCEPT_EULA
        value: "Y"
      - name: MSSQL_SA_PASSWORD
        value: "$sqlPassword"
      - name: MSSQL_DATA_DIR
        value: "/var/opt/mssql/data"
      resources:
        requests:
          cpu: 2
          memoryInGb: 4
      ports:
      - port: 1433
  - name: test-runner
    properties:
      image: $fullImageName
      command:
      - /bin/bash
      - -c
      - "$testShellCmd"
      environmentVariables:
      - name: AZURE_CLIENT_ID
        value: $($identity.clientId)
      - name: SBM_TEST_SQL_SERVER
        value: localhost
      - name: SBM_TEST_SQL_USER
        value: sa
      - name: SBM_TEST_SQL_PASSWORD
        value: "$sqlPassword"
      - name: SBM_TEST_DB_PATH
        value: "/var/opt/mssql/data"$testFilterEnvVar
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
$yamlFilePath = Join-Path $env:TEMP "aci-dependent-tests-$(Get-Date -Format 'yyyyMMddHHmmss').yaml"
$aciYaml | Set-Content -Path $yamlFilePath -Encoding UTF8

Write-Host "Generated ACI YAML:" -ForegroundColor DarkGray
Write-Host $aciYaml -ForegroundColor DarkGray
Write-Host ""
Write-Host "Deploying container group..." -ForegroundColor DarkGreen

az container create --resource-group $resourceGroupName --file $yamlFilePath -o none
Remove-Item $yamlFilePath -Force -ErrorAction SilentlyContinue

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create container group" -ForegroundColor Red
    exit 1
}

Write-Host "Container group deployed. Waiting for tests to complete..." -ForegroundColor DarkGreen

#############################################
# Wait for tests to complete
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
    
    $currentTime = Get-Date
    if (($currentTime - $lastLogTime).TotalSeconds -ge 10) {
        $recentLogs = az container logs --name $testContainerName --resource-group $resourceGroupName --container-name test-runner 2>$null
        if ($null -ne $recentLogs) {
            # Check for test exit code
            $logString = $recentLogs -join "`n"
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
    
    if ($state -eq "Terminated" -or $state -eq "Completed" -or $state2 -eq "Terminated" -or $state2 -eq "Completed") {
        $testsCompleted = $true
        Write-Host ""
        Write-Host "Container group terminated. Tests complete." -ForegroundColor Cyan
        break
    }
    
    if ($state -eq "Failed" -or $state2 -eq "Failed") {
        Write-Host ""
        Write-Host "Container group failed (state: $state)" -ForegroundColor Red
        break
    }
    
    if ($currentTime -gt $timeoutTime) {
        Write-Host ""
        Write-Host "ERROR: Test execution timed out after $timeoutMinutes minutes" -ForegroundColor Red
        Write-Host ""
        Write-Host "Retrieving test logs and generating summary..." -ForegroundColor Yellow
        $testLogs = az container logs --name $testContainerName --resource-group $resourceGroupName --container-name test-runner 2>$null
        if ($testLogs) {
            Show-TestSummary -logs $testLogs
        }
        else {
            Write-Host "No test logs available yet" -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "SQL Server logs (last 10 lines):" -ForegroundColor Yellow
        az container logs --name $testContainerName --resource-group $resourceGroupName --container-name sql-server 2>$null | Select-Object -Last 10
        
        if (-not $keepContainer) {
            az container delete --name $testContainerName --resource-group $resourceGroupName --yes -o none
        }
        exit 1
    }

    Start-Sleep -Seconds 10
}

#############################################
# Results
#############################################
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($null -eq $testExitCode) {
    $testExitCode = 1
}

Write-Host ""
Write-Host "Test Exit Code: $testExitCode" -ForegroundColor DarkGreen
Write-Host "Results uploaded to: $blobContainerName/$blobPath" -ForegroundColor Cyan
Write-Host ""

# Get and parse full test runner logs
$fullTestLogs = az container logs --name $testContainerName --resource-group $resourceGroupName --container-name test-runner 2>$null
if ($fullTestLogs) {
    Show-TestSummary -logs $fullTestLogs
}
else {
    Write-Host "No test logs available" -ForegroundColor Yellow
}

#############################################
# Cleanup
#############################################
if ($keepContainer) {
    Write-Host ""
    Write-Host "Container group kept for debugging: $testContainerName" -ForegroundColor Yellow
    Write-Host "Test runner logs: az container logs --name $testContainerName --resource-group $resourceGroupName --container-name test-runner" -ForegroundColor DarkGray
    Write-Host "SQL Server logs:  az container logs --name $testContainerName --resource-group $resourceGroupName --container-name sql-server" -ForegroundColor DarkGray
    Write-Host "Delete:           az container delete --name $testContainerName --resource-group $resourceGroupName --yes" -ForegroundColor DarkGray
} else {
    Write-Host "Cleaning up container group..." -ForegroundColor DarkGreen
    az container delete --name $testContainerName --resource-group $resourceGroupName --yes -o none
}

Write-Host ""
if ($testExitCode -eq 0) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "TESTS PASSED" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    exit 0
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "TESTS FAILED (Exit Code: $testExitCode)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit $testExitCode
}


# Download test results 
if( (Test-Path ./testresults) -eq $false) { mkdir testresults }
az storage blob download-batch --account-name "$($prefix)storage" --source testresults --destination ./testresults  --auth-mode login

# Analyze test results with GitHub Copilot
copilot --yolo -p "The folder './testresults' contains sub-folders named for different Azure integration tests. These sub-folders contain `TestResults.html` test result HTML summaries and `console-output.log` console output log files. Please review these files and for all failures, create an analysis of the failures and how they can be fixed. IMPORTANT: In the `console-output.log` file, the log entries are organized first with the `Passed` or `Failed` message on the same line as the test name, followed by the `Standard Output Messages:` and `TestContext Messages:` lines and content.  Save your analysis to a single `failures.md ` file.  For the tests that didn't fail, please review the logs and identify any messages that either have misleading messages or suggest something may have gone wrong, even if the test passed. Please create a single `observations.md` markdown file with your observations analysis. Save the markdown files to the ./testresults directory."