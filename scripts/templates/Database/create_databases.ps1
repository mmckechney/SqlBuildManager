param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path,
    [int] $testDatabaseCount

)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path


$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
Write-Host "Creating Test databases. $testDatabaseCount per server" -ForegroundColor DarkGreen
az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/azuredeploy_db.bicep" --parameters namePrefix="$prefix" sqladminname="$sqlUserName" sqladminpassword="$sqlPassword" testDbCountPerServer=$testDatabaseCount  -o table

#Create local firewall rule
.$scriptDir/create_database_firewall_rule.ps1 -resourceGroupName $resourceGroupName