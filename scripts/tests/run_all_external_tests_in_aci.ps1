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

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName aci -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.AciTests" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName containerapp -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.ContainerAppTests" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName batchqueue -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Queue" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName aks -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.KubernetesTests" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName pg -testFilter "FullyQualifiedName~SqlBuildManager.Console.PostgreSQL.ExternalTest" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName batchoverride -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Override" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE

.\run_filtered_external_tests_in_aci.ps1 -prefix $prefix -customName batchquery -testFilter "FullyQualifiedName~SqlBuildManager.Console.ExternalTest.BatchTests.Batch_Query" -timeoutMinutes 300 -timestamp $timestamp
$exitCode += $LASTEXITCODE


# Download test results 
if( (Test-Path ./testresults) -eq $false) { mkdir testresults }
az storage blob download-batch --account-name "$($prefix)storage" --source testresults --pattern "$($timestamp)*" --destination ./testresults  --auth-mode login --overwrite

    # Analyze test results with GitHub Copilot
$promptTemplate = Get-Content -Path "$PSScriptRoot\analyze-test-results-prompt.md" -Raw
$prompt = $promptTemplate -replace '\{\{timestamp\}\}', $timestamp
$output = copilot --yolo -p $prompt 2>&1

