param
(
    [string] $prefix,
    [string] $resourceGroupName
)


#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix


Write-Host "Creating Virtual Network from: $prefix"  -ForegroundColor Cyan

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_vnet.ps1 -resourceGroupName $resourceGroupName -vnet $vnet -aksSubnet $aksSubnet -containerAppSubnet $containerAppSubnet -aciSubnet $aciSubnet -batchSubnet $batchSubnet -nsgName $nsgName