param
(
    [string] $path = "..\..\..\src\TestConfig",
    [string] $prefix,
    [string] $resourceGroupName
)   

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

$path = Resolve-Path $path

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_database_override_files.ps1 -path $path -resourceGroupName $resourceGroupName