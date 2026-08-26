<#
.SYNOPSIS
    Runs all test artifact cleanup scripts in parallel.
.DESCRIPTION
    Orchestrates parallel cleanup of integration test remnants across multiple Azure
    services: Batch jobs, Service Bus topic subscriptions, storage containers,
    Container Apps, Kubernetes resources (jobs, config maps, secrets), YAML files,
    and ACI JSON files.
.PARAMETER envName
    Azure Developer CLI environment name used to derive resource names.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to rg-{envName}.
.PARAMETER includeActiveBatchJobs
    When true, deletes active Batch jobs too. Default: true.
.PARAMETER removeAllContainerApps
    When true, deletes all Container Apps. Default: true.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName,
    [bool] $includeActiveBatchJobs = $true,
    [bool] $removeAllContainerApps = $true
)
$resourceGroupNameOverride = $resourceGroupName
. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}
$utilityPath = $PSScriptRoot

$aksResourceId = az aks show `
    --resource-group $resourceGroupName `
    --name $aksClusterName `
    --query id `
    --output tsv `
    --only-show-errors 2>$null
$runKubernetesCleanup = $LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($aksResourceId)
if ($runKubernetesCleanup) {
    az aks get-credentials `
        --resource-group $resourceGroupName `
        --name $aksClusterName `
        --overwrite-existing `
        --only-show-errors | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to configure kubectl for AKS cluster '$aksClusterName'."
    }
}
else {
    Write-Host "AKS cluster '$aksClusterName' is not deployed; skipping Kubernetes cleanup." -ForegroundColor Yellow
}

# Run cleanup scripts in parallel
$jobs = @()

Write-Host "Starting cleanup of integration test remnants..." -ForegroundColor Green

Write-Host "Cleaning up Batch jobs..." -ForegroundColor Green
$jobs += Start-Job -ScriptBlock {
    param($scriptPath, $envName, $resourceGroupName, $includeActiveBatchJobs)
    & $scriptPath -envName $envName -resourceGroupName $resourceGroupName -includeActive $includeActiveBatchJobs
} -ArgumentList (Join-Path $utilityPath "Clear_UnitTest_Batch_Jobs.ps1"), $envName, $resourceGroupName, $includeActiveBatchJobs -Name "BatchJobs"

write-Host "Cleaning up Service Bus topics..." -ForegroundColor Green
$jobs += Start-Job -ScriptBlock {
    param($scriptPath, $envName, $resourceGroupName)
    & $scriptPath -envName $envName -resourceGroupName $resourceGroupName
} -ArgumentList (Join-Path $utilityPath "Clear_UnitTest_ServiceBus_Topics.ps1"), $envName, $resourceGroupName -Name "ServiceBus"

Write-Host "Cleaning up Storage containers..." -ForegroundColor Green
$jobs += Start-Job -ScriptBlock {
    param($scriptPath, $envName, $resourceGroupName)
    & $scriptPath -envName $envName -resourceGroupName $resourceGroupName
} -ArgumentList (Join-Path $utilityPath "Clear_UnitTest_StorageAcct_Containers.ps1"), $envName, $resourceGroupName -Name "Storage"

if($removeAllContainerApps)
{
    write-Host "Cleaning up Container Apps..." -ForegroundColor Green
    $jobs += Start-Job -ScriptBlock {
        param($scriptPath, $resourceGroupName)
        & $scriptPath -resourceGroupName $resourceGroupName
    } -ArgumentList (Join-Path $utilityPath "Clean-ContainerApps.ps1"), $resourceGroupName -Name "ContainerApps"
}

Write-Host "Cleaning up YAML files..." -ForegroundColor Green
$jobs += Start-Job -ScriptBlock {
    param($scriptPath)
    & $scriptPath
} -ArgumentList (Join-Path $utilityPath "Clear_UnitTest_YamlFiles.ps1") -Name "YamlFiles"

Write-Host "Cleaning up ACI JSON files..." -ForegroundColor Green
$jobs += Start-Job -ScriptBlock {
    param($scriptPath)
    & $scriptPath
} -ArgumentList (Join-Path $utilityPath "Clear_UnitTest_AciJson.ps1") -Name "AciJson"

if ($runKubernetesCleanup) {

    Write-Host "Cleaning up Kubernetes resources..." -ForegroundColor Green
    $jobs += Start-Job -ScriptBlock {
        Write-Host "Deleting Kubernetes Unit Test resources in sqlbuildmanager namespace" -ForegroundColor Green
        kubectl delete all --all -n sqlbuildmanager
        if ($LASTEXITCODE -ne 0) { throw "Unable to delete Kubernetes resources." }
    } -Name "K8sResources"

    write-Host "Cleaning up Kubernetes Unit Test ConfigMaps..." -ForegroundColor Green
    $jobs += Start-Job -ScriptBlock {
        Write-Host "Deleting Kubernetes Unit Test ConfigMaps" -ForegroundColor Green
        $configMaps = (kubectl get configmap -n sqlbuildmanager -o name)
        if ($LASTEXITCODE -ne 0) { throw "Unable to list Kubernetes ConfigMaps." }
        foreach($map in $configMaps)
        { 
            if($map -ne "configmap/kube-root-ca.crt")
            {
                kubectl delete $map -n sqlbuildmanager
                if ($LASTEXITCODE -ne 0) { throw "Unable to delete Kubernetes ConfigMap '$map'." }
            }
        }
    } -Name "K8sConfigMaps"

    Write-Host "Cleaning up Kubernetes Unit Test secrets..." -ForegroundColor Green
    $jobs += Start-Job -ScriptBlock {
        Write-Host "Deleting Kubernetes Unit Test secrets" -ForegroundColor Green
        kubectl delete secret -n sqlbuildmanager --field-selector=type=Opaque
        if ($LASTEXITCODE -ne 0) { throw "Unable to delete Kubernetes secrets." }
    } -Name "K8sSecrets"
}

# Stream output and state changes until all cleanup jobs finish.
Write-Host "Running cleanup scripts in parallel..." -ForegroundColor Cyan
$jobStates = @{}
foreach ($job in $jobs) {
    $jobStates[$job.Id] = $job.State
    Write-Host "[$($job.Name)] $($job.State)" -ForegroundColor DarkGray
}

while ($jobs | Where-Object State -in @("NotStarted", "Running")) {
    foreach ($job in $jobs) {
        if ($job.HasMoreData) {
            Receive-Job -Job $job
        }
        if ($jobStates[$job.Id] -ne $job.State) {
            $jobStates[$job.Id] = $job.State
            $stateColor = if ($job.State -eq "Completed") { "Green" } elseif ($job.State -eq "Failed") { "Red" } else { "DarkGray" }
            Write-Host "[$($job.Name)] $($job.State)" -ForegroundColor $stateColor
        }
    }
    Start-Sleep -Seconds 1
}

foreach ($job in $jobs) {
    if ($job.HasMoreData) {
        Receive-Job -Job $job
    }
    if ($jobStates[$job.Id] -ne $job.State) {
        $stateColor = if ($job.State -eq "Completed") { "Green" } elseif ($job.State -eq "Failed") { "Red" } else { "DarkGray" }
        Write-Host "[$($job.Name)] $($job.State)" -ForegroundColor $stateColor
    }
}
$failedJobs = @($jobs | Where-Object State -eq "Failed")
$jobs | Remove-Job
if ($failedJobs.Count -gt 0) {
    throw "Cleanup failed for: $($failedJobs.Name -join ', ')"
}