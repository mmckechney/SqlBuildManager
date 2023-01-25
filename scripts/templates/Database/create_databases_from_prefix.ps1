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
.$scriptDir/create_databases.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path -testDatabaseCount $testDatabaseCount
.$scriptDir/create_login_for_managedidentity_fromprefix.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path
.$scriptDir/create_database_override_files_fromprefix.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path