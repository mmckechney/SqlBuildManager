param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [int] $testDatabaseCount

)

if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_databases.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path -testDatabaseCount $testDatabaseCount