<#
.SYNOPSIS
    Runs all test artifact cleanup scripts in parallel.
.DESCRIPTION
    Orchestrates parallel cleanup of integration test remnants across multiple Azure
    services: Batch jobs, Service Bus topic subscriptions, storage containers,
    Container Apps, Kubernetes resources (jobs, config maps, secrets), YAML files,
    and ACI JSON files.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to {prefix}-rg.
.PARAMETER includeActiveBatchJobs
    When true, deletes active Batch jobs too. Default: true.
.PARAMETER removeAllContainerApps
    When true, deletes all Container Apps. Default: true.
#>
param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [bool] $includeActiveBatchJobs = $true,
    [bool] $removeAllContainerApps = $true
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

# Run cleanup scripts in parallel
$jobs = @()

$jobs += Start-Job -ScriptBlock {
    param($prefix, $includeActiveBatchJobs)
    Set-Location $using:PWD
    .\Clear_UnitTest_Batch_Jobs.ps1 -prefix $prefix -includeActive $includeActiveBatchJobs
} -ArgumentList $prefix, $includeActiveBatchJobs -Name "BatchJobs"

$jobs += Start-Job -ScriptBlock {
    param($prefix)
    Set-Location $using:PWD
    .\Clear_UnitTest_ServiceBus_Topics.ps1 -prefix $prefix
} -ArgumentList $prefix -Name "ServiceBus"

$jobs += Start-Job -ScriptBlock {
    param($prefix)
    Set-Location $using:PWD
    .\Clear_UnitTest_StorageAcct_Containers.ps1 -prefix $prefix
} -ArgumentList $prefix -Name "Storage"

if($removeAllContainerApps)
{
    $jobs += Start-Job -ScriptBlock {
        param($resourceGroupName)
        Set-Location $using:PWD
        .\Clean-ContainerApps.ps1 -resourceGroupName $resourceGroupName
    } -ArgumentList $resourceGroupName -Name "ContainerApps"
}

$jobs += Start-Job -ScriptBlock {
    Set-Location $using:PWD
    .\Clear_UnitTest_YamlFiles.ps1
} -Name "YamlFiles"

$jobs += Start-Job -ScriptBlock {
    Set-Location $using:PWD
    .\Clear_UnitTest_AciJson.ps1
} -Name "AciJson"

$jobs += Start-Job -ScriptBlock {
    Write-Host "Deleting Kubernetes Unit Test resources in sqlbuildmanager namespace" -ForegroundColor Green
    kubectl delete all --all -n sqlbuildmanager
} -Name "K8sResources"

$jobs += Start-Job -ScriptBlock {
    Write-Host "Deleting Kubernetes Unit Test ConfigMaps" -ForegroundColor Green
    $configMaps = (kubectl get configmap -n sqlbuildmanager -o name)
    foreach($map in $configMaps) 
    { 
        if($map -ne "configmap/kube-root-ca.crt") 
        { 
            kubectl delete $map -n sqlbuildmanager
        }
    }
} -Name "K8sConfigMaps"

$jobs += Start-Job -ScriptBlock {
    Write-Host "Deleting Kubernetes Unit Test secrets" -ForegroundColor Green
    kubectl delete secret -n sqlbuildmanager --field-selector=type=Opaque
} -Name "K8sSecrets"

# Wait for all jobs to complete and display output
Write-Host "Running cleanup scripts in parallel..." -ForegroundColor Cyan
$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job