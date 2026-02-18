<#
.SYNOPSIS
    Shared helper functions for ACI test runner scripts.

.DESCRIPTION
    Provides common functionality for running tests in Azure Container Instances,
    including test summary display with in-place refresh, container lifecycle
    management, test monitoring, and cleanup/exit reporting.
    
    Dot-source this file from calling scripts:
        . (Join-Path $PSScriptRoot "aci_test_helpers.ps1")
#>

#############################################
# Test Summary State & Display
#############################################

function Initialize-TestSummaryState {
    <#
    .SYNOPSIS
        Initializes script-scope variables for test result accumulation.
        Call once at the start of the script before any Show-TestSummary calls.
    #>
    $script:lastRenderLineCount = 0
    $script:allPassed = [System.Collections.Generic.List[string]]::new()
    $script:allFailed = [System.Collections.Generic.List[string]]::new()
    $script:allSkipped = [System.Collections.Generic.List[string]]::new()
    $script:seenTests = [System.Collections.Generic.HashSet[string]]::new()
    $script:monitoringStartTime = $null
}

function Show-TestSummary {
    <#
    .SYNOPSIS
        Parses test output logs and displays a formatted, color-coded test summary.
        Accumulates results across multiple calls to survive log truncation.
    .PARAMETER logs
        Array of log lines from container output.
    .PARAMETER refresh
        If specified, overwrites the previous summary in-place (for live monitoring).
    #>
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

#############################################
# Container Management
#############################################

function Get-AciContainerLogs {
    <#
    .SYNOPSIS
        Retrieves logs from an ACI container, optionally targeting a specific container in a group.
    #>
    param(
        [string]$containerName,
        [string]$resourceGroupName,
        [string]$logContainerName
    )
    if ($logContainerName) {
        return az container logs --name $containerName --resource-group $resourceGroupName --container-name $logContainerName 2>$null
    } else {
        return az container logs --name $containerName --resource-group $resourceGroupName 2>$null
    }
}

function Remove-ExistingAciContainer {
    <#
    .SYNOPSIS
        Checks for and removes an existing ACI container/container group.
    #>
    param(
        [string]$containerName,
        [string]$resourceGroupName
    )
    Write-Host "Checking for existing test container..." -ForegroundColor DarkGreen
    $existing = az container show --name $containerName --resource-group $resourceGroupName 2>$null
    if ($existing) {
        Write-Host "Deleting existing test container..." -ForegroundColor Yellow
        az container delete --name $containerName --resource-group $resourceGroupName --yes -o none
        Start-Sleep -Seconds 5
    }
}

function Get-AciSubnetId {
    <#
    .SYNOPSIS
        Gets the resource ID for an ACI subnet.
    #>
    param(
        [string]$resourceGroupName,
        [string]$vnetName,
        [string]$subnetName
    )
    return az network vnet subnet show `
        --resource-group $resourceGroupName `
        --vnet-name $vnetName `
        --name $subnetName `
        --query id -o tsv
}

function Deploy-AciFromYaml {
    <#
    .SYNOPSIS
        Writes ACI YAML to a temp file, deploys it, and verifies success.
    #>
    param(
        [string]$yamlContent,
        [string]$resourceGroupName,
        [string]$yamlFilePrefix = "aci-test"
    )
    $yamlFilePath = Join-Path $env:TEMP "$yamlFilePrefix-$(Get-Date -Format 'yyyyMMddHHmmss').yaml"
    $yamlContent | Set-Content -Path $yamlFilePath -Encoding UTF8
    
    Write-Host "Generated ACI YAML:" -ForegroundColor DarkGray
    Write-Host $yamlContent -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Deploying container to ACI..." -ForegroundColor DarkGreen
    
    az container create --resource-group $resourceGroupName --file $yamlFilePath -o none
    Remove-Item $yamlFilePath -Force -ErrorAction SilentlyContinue
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to create container" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Container deployed. Waiting for tests to complete..." -ForegroundColor DarkGreen
    Write-Host ""
}

#############################################
# Test Monitoring
#############################################

function Wait-ForAciTests {
    <#
    .SYNOPSIS
        Monitors an ACI container running tests until completion, timeout, or failure.
        Displays a live-refreshing test summary during monitoring.
    .PARAMETER containerName
        The ACI container (group) name.
    .PARAMETER resourceGroupName
        Azure resource group name.
    .PARAMETER timeoutMinutes
        Maximum wait time in minutes.
    .PARAMETER logContainerName
        Container name for log retrieval (e.g., "test-runner" for multi-container groups).
        If empty, retrieves logs from the default/only container.
    .PARAMETER keepContainer
        If true, keeps the container on timeout instead of deleting it.
    .PARAMETER sqlContainerName
        If specified, shows this container's logs on timeout (for SQL sidecar debugging).
    .OUTPUTS
        Hashtable with TestExitCode (int or null) and TestsCompleted (bool).
    #>
    param(
        [string]$containerName,
        [string]$resourceGroupName,
        [int]$timeoutMinutes,
        [string]$logContainerName,
        [switch]$keepContainer,
        [string]$sqlContainerName
    )
    
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
        $container = az container show --name $containerName --resource-group $resourceGroupName 2>$null | ConvertFrom-Json -Depth 10
        $state = $null
        $state2 = $null
        if ($null -ne $container -and $null -ne $container.instanceView) {
            $state = $container.containers.instanceView.currentState.detailStatus
            $state2 = $container.containers.instanceView.currentState.state
        }
        
        # Stream logs periodically and update test summary
        $currentTime = Get-Date
        if (($currentTime - $lastLogTime).TotalSeconds -ge 10) {
            $recentLogs = Get-AciContainerLogs -containerName $containerName -resourceGroupName $resourceGroupName -logContainerName $logContainerName
            if ($null -ne $recentLogs) {
                # Join array to string for regex matching
                $logString = $recentLogs -join "`n"
                # Extract exit code from logs if present
                if ($logString -match "TEST_EXIT_CODE=(\d+)") {
                    $testExitCode = [int]$Matches[1]
                }
                
                # Show refreshing test summary
                if ($script:lastRenderLineCount -eq 0) {
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
            Write-Host "Container terminated. Tests complete." -ForegroundColor Cyan
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
            Write-Host ""
            Write-Host "Retrieving test logs and generating summary..." -ForegroundColor Yellow
            $testLogs = Get-AciContainerLogs -containerName $containerName -resourceGroupName $resourceGroupName -logContainerName $logContainerName
            if ($testLogs) {
                Show-TestSummary -logs $testLogs
            } else {
                Write-Host "No test logs available yet" -ForegroundColor Yellow
            }
            
            if ($sqlContainerName) {
                Write-Host ""
                Write-Host "SQL Server logs (last 10 lines):" -ForegroundColor Yellow
                az container logs --name $containerName --resource-group $resourceGroupName --container-name $sqlContainerName 2>$null | Select-Object -Last 10
            }
            
            if (-not $keepContainer) {
                az container delete --name $containerName --resource-group $resourceGroupName --yes -o none
            }
            exit 1
        }
        
        Start-Sleep -Seconds 10
    }
    
    return @{
        TestsCompleted = $testsCompleted
        TestExitCode = $testExitCode
    }
}

#############################################
# Cleanup & Exit
#############################################

function Complete-AciTestRun {
    <#
    .SYNOPSIS
        Performs container cleanup and exits with appropriate status code and messaging.
    .PARAMETER containerName
        The ACI container (group) name to clean up.
    .PARAMETER resourceGroupName
        Azure resource group name.
    .PARAMETER exitCode
        The test exit code (0 = passed, non-zero = failed).
    .PARAMETER keepContainer
        If true, keeps the container for debugging and shows helpful commands.
    .PARAMETER logContainerName
        Container name for showing debug log commands (multi-container groups).
    .PARAMETER sqlContainerName
        SQL sidecar container name for showing debug log commands.
    #>
    param(
        [string]$containerName,
        [string]$resourceGroupName,
        [int]$exitCode,
        [switch]$keepContainer,
        [string]$logContainerName,
        [string]$sqlContainerName
    )
    
    if ($keepContainer) {
        Write-Host ""
        Write-Host "Container kept for debugging: $containerName" -ForegroundColor Yellow
        if ($logContainerName) {
            Write-Host "Test runner logs: az container logs --name $containerName --resource-group $resourceGroupName --container-name $logContainerName" -ForegroundColor DarkGray
            if ($sqlContainerName) {
                Write-Host "SQL Server logs:  az container logs --name $containerName --resource-group $resourceGroupName --container-name $sqlContainerName" -ForegroundColor DarkGray
            }
        } else {
            Write-Host "To view logs: az container logs --name $containerName --resource-group $resourceGroupName" -ForegroundColor DarkGray
        }
        Write-Host "Delete:           az container delete --name $containerName --resource-group $resourceGroupName --yes" -ForegroundColor DarkGray
    } else {
        Write-Host "Cleaning up container..." -ForegroundColor DarkGreen
        az container delete --name $containerName --resource-group $resourceGroupName --yes -o none
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
}
