param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [int] $testDatabaseCount = 10

)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

$ipAddress = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
Write-Host "Using IP Address: $ipAddress" -ForegroundColor Green

Write-Host "Validating Network Setup" -ForegroundColor DarkGreen
az deployment group create --resource-group $resourceGroupName --template-file "$scriptDir/../Modules/network.bicep" --parameters namePrefix="$prefix"  -o table 

$subnetNames += (az network vnet subnet list --vnet-name $vnet --resource-group $resourceGroupName --query [].name -o tsv)
$subnetNames = $subnetNames -join "," 
Write-Host "Using Subnet Ids: $subnetNames" -ForegroundColor Green
Write-Host "Deploying SQL Servers, Elastic Pool, Firewall Settings and Databases" -ForegroundColor DarkGreen
az deployment group create --resource-group $resourceGroupName --template-file "$scriptDir/../Modules/database.bicep" --parameters namePrefix="$prefix" testDbCountPerServer="$testDatabaseCount"  currentIpAddress=$ipAddress subnetNames=$subnetNames -o table

