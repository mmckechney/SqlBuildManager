param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix,
    [string] $sqlUserName,
    [string] $sqlPassword
)
#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

Write-Host "Create AKS settings file from prefix: $prefix"  -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$path = Resolve-Path $path
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    Write-Host("Looking for SQL credentials") -ForegroundColor DarkGreen
    if(Test-Path (Join-Path $path "un.txt"))
    {
        $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    }

    if(Test-Path (Join-Path $path "pw.txt"))
    {
        $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
    }
}
    else 
{
    Write-Host("Using provided SQL credentials") -ForegroundColor DarkGreen

}

Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Azure Container Registry name:'$containerRegistryName'" -ForegroundColor DarkGreen
Write-Host "Using Kubernetes Service Account name: '$serviceAccountName'" -ForegroundColor DarkGreen
Write-Host "Using keyvault name: '$keyVaultName'" -ForegroundColor DarkGreen



$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_aks_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName  $resourceGroupName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -acrName $containerRegistryName  -keyVaultName $keyVaultName -serviceAccount $serviceAccountName

