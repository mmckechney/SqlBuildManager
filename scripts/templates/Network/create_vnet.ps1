[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $resourceGroupName, 

    [Parameter()]
    [string]
    $nsgName,

    [Parameter()]
    [string]
    $vnet,

    [Parameter()]
    [string]
    $aksSubnet,

    [Parameter()]
    [string]
    $containerAppSubnet,

    [Parameter()]
    [string]
    $aciSubnet,

    [Parameter()]
    [string]
    $batchSubnet,
    
    [Parameter()]
    [string]
    $vnetPrefix = "10.180.0.0/20",

    [Parameter()]
    [string]
    $aksSubnetPrefix = "10.180.0.0/22",

    [Parameter()]
    [string]
    $containerAppSubnetPrefix = "10.180.4.0/22",

    [Parameter()]
    [string]
    $aciSubnetPrefix = "10.180.8.0/22",

    [Parameter()]
    [string]
    $batchSubnetPrefix = "10.180.12.0/22"


)

Write-Host "Creating Network Security Group: $nsgName" -ForegroundColor DarkGreen
az network nsg create  --resource-group $resourceGroupName --name $nsgName -o table

Write-Host "Creating VNET: $vnet" -ForegroundColor DarkGreen
az network vnet create --resource-group $resourceGroupName --name $vnet  --address-prefixes  $vnetPrefix  -o table

Write-Host "Creating AKS subnet: $aksSubnet" -ForegroundColor DarkGreen
az network vnet subnet create --resource-group $resourceGroupName --vnet-name $vnet --name $aksSubnet --address-prefixes  $aksSubnetPrefix --network-security-group $nsgName  --service-endpoints Microsoft.Sql  -o table

Write-Host "Creating Container App subnet: $containerAppSubnet" -ForegroundColor DarkGreen
az network vnet subnet create --resource-group $resourceGroupName --vnet-name $vnet --name $containerAppSubnet --address-prefixes $containerAppSubnetPrefix --service-endpoints Microsoft.Sql --network-security-group $nsgName -o table

Write-Host "Creating Container Instance subnet: $aciSubnet" -ForegroundColor DarkGreen
az network vnet subnet create --resource-group $resourceGroupName --vnet-name $vnet --name $aciSubnet --address-prefixes $aciSubnetPrefix --service-endpoints Microsoft.Sql --delegations Microsoft.ContainerInstance/containerGroups --network-security-group $nsgName -o table

Write-Host "Creating Batch Account subnet: $batchSubnet" -ForegroundColor DarkGreen
az network vnet subnet create --resource-group $resourceGroupName --vnet-name $vnet --name $batchSubnet --address-prefixes $batchSubnetPrefix --service-endpoints Microsoft.Sql  --network-security-group $nsgName -o table