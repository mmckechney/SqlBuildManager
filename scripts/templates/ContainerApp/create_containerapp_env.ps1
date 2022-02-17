param
(
    [string] $containerAppEnvName,
    [string] $logAnalyticsWorkspace,
    [string] $containerRegistryName,
    [string] $location,
    [string] $resourceGroupName
)
Write-Host "Create Container App Environment"  -ForegroundColor Cyan

$logAnalyticsClientId = az monitor log-analytics workspace show --query customerId -g $resourceGroupName -n $logAnalyticsWorkspace
$logAnalyticsKey = az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $resourceGroupName -n $logAnalyticsWorkspace

Write-Host "Creating Container App Environment: $containerAppEnvName" -ForegroundColor DarkGreen
az containerapp env create -n "$containerAppEnvName" -g "$resourceGroupName" --logs-workspace-id "$logAnalyticsClientId" --logs-workspace-key "$logAnalyticsKey" --location "$location" -o table 


