param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [Parameter(Mandatory=$true)]
    [string] $prefix,
    [string] $sqlUserName,
    [string] $sqlPassword,
    [string] $imageTag,
    [bool] $withContainerRegistry = $true,
    [bool] $withKeyVault = $true
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

Write-Host "Create Container App Settings file from prefix: $prefix"  -ForegroundColor Cyan
$path = Resolve-Path $path

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
}
 if($withKeyVault)
{
    Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen
}
else 
{
    Write-Host "Not using KeyVault"  -ForegroundColor DarkGreen
}

Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen
$identityClientId = az identity show --resource-group $resourceGroupName --name $identityName -o tsv --query clientId
Write-Host "Using Managed Identity ClientId:'$identityClientId'" -ForegroundColor DarkGreen
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Container App Environment name:'$containerAppEnvName'" -ForegroundColor DarkGreen
if($withContainerRegistry)
{
    Write-Host "Using Container Registry name:'$containerRegistryName'" -ForegroundColor DarkGreen
}

if("" -eq $imageTag)
{
    $imageTag = "latest-vNext"
    Write-Host "Using Image Tag: $imageTag" -ForegroundColor DarkGreen
}

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
if($withKeyVault)
{
    .$scriptDir/create_containerapp_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -containerAppEnvironmentName $containerAppEnvName -containerRegistryName $containerRegistryName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -imageTag $imageTag -withContainerRegistry $withContainerRegistry -keyVaultName $keyVaultName -identityName $identityName -identityClientId $identityClientId
}
else 
{
    .$scriptDir/create_containerapp_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -containerAppEnvironmentName $containerAppEnvName -containerRegistryName $containerRegistryName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -imageTag $imageTag -withContainerRegistry $withContainerRegistry  -identityName $identityName -identityClientId $identityClientId
}
