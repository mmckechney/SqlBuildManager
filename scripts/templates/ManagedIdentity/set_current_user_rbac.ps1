param
(
    [string] $resourceGroupName
)
Write-Host "Set current user identity RBAC"  -ForegroundColor Cyan
$userIdGuid = az ad signed-in-user show -o tsv --query id
$subscriptionId = az account show --query id --output tsv

Write-Host "Using current user identity '$userId' and subscription '$subscriptionId'"  -ForegroundColor Cyan
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

az deployment group create --resource-group $resourceGroupName --template-file $scriptDir/../Modules/useridentity.bicep --parameters userIdGuid="$userIdGuid" -o table 