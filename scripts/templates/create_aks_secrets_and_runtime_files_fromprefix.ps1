param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $prefix,
    [string] $sqlUserName,
    [string] $sqlPassword
)

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$path = Resolve-Path $path
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    Write-Host("Looking for SQL credentials") -ForegroundColor DarkGreen
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
}else {
    Write-Host("Using provided SQL credentials") -ForegroundColor DarkGreen

}

$storageAccountName =  az storage account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen

$eventHubNamespaceName = az eventhubs namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen

$serviceBusNamespaceName = az servicebus namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen

./create_aks_secrets_and_runtime_files.ps1  -path $path -resourceGroupName  $resourceGroupName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -sqlUserName $sqlUserName -sqlPassword $sqlPassword

