param
(
    [string] $path = "..\..\..\src\TestConfig",
    [string] $prefix,
    [string] $resourceGroupName
)   

if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
$path = Resolve-Path $path

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_database_override_files.ps1 -path $path -resourceGroupName $resourceGroupName