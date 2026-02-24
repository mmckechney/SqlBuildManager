[CmdletBinding()]
param (
    [Parameter()]
    [string] $prefix 
)
$exitCode = 0
$timestamp = (Get-Date -Format 'yyyy-MM-dd-HHmmss')
Clear-Host 

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Integration Test Runners (ACI in VNet)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName batchqueue -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Queue" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName batchoverride -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Override" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName batchquery -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Query" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName containerapp -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.ContainerAppTests" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName aci -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.AciTests" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName aks -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.KubernetesTests" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName pg -testFilter "FullyQualifiedName~SqlBuildManager.Console.PostgreSQL.ExternalTest" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

# Download test results 
if( (Test-Path ./testresults) -eq $false) { mkdir testresults }
az storage blob download-batch --account-name "$($prefix)storage" --source testresults --pattern "$($timestamp)*" --destination ./testresults  --auth-mode login --overwrite

    # Analyze test results with GitHub Copilot
$output = copilot --yolo -p "The folder './testresults/$($timestamp)' contains sub-folders named for different Azure integration tests. These sub-folders contain `TestResults.html` test result HTML summaries and `console-output.log` console output log files. Please review these files and for all failures, create an analysis of the failures and how they can be fixed. IMPORTANT: In the `console-output.log` file, the log entries are organized first with the `Passed` or `Failed` message on the same line as the test name, followed by the `Standard Output Messages:` and `TestContext Messages:` lines and content.  Save your analysis to a single `failures.md ` file.  For the tests that didn't fail, please review the logs and identify any messages that either have misleading messages or suggest something may have gone wrong, even if the test passed. Please create a single `observations.md` markdown file with your observations analysis. Save the markdown files to the './testresults/$($timestamp)' directory." 2>&1

