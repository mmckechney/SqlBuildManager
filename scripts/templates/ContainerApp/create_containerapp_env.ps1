param
(
    [string] $containerAppEnvName,
    [string] $logAnalyticsWorkspace,
    [string] $containerRegistryName,
    [string] $location,
    [string] $resourceGroupName,
    [string] $subnetId
)
Write-Host "Create Container App Environment"  -ForegroundColor Cyan

$logAnalyticsClientId = az monitor log-analytics workspace show --query customerId -g $resourceGroupName -n $logAnalyticsWorkspace
$logAnalyticsKey = az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $resourceGroupName -n $logAnalyticsWorkspace

Write-Host "Creating Container App Environment: $containerAppEnvName" -ForegroundColor DarkGreen
if("" -eq $subnetId)
{
 az containerapp env create -n "$containerAppEnvName" -g "$resourceGroupName" --logs-workspace-id "$logAnalyticsClientId" --logs-workspace-key "$logAnalyticsKey" --location "$location" -o table 
}
else
{
    az containerapp env create -n "$containerAppEnvName" -g "$resourceGroupName" --logs-workspace-id "$logAnalyticsClientId" --logs-workspace-key "$logAnalyticsKey" --location "$location" --infrastructure-subnet-resource-id "$subnetId" -o table
}


