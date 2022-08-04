param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [int] $testDatabaseCount

)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_databases.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path -testDatabaseCount $testDatabaseCount