<#
.SYNOPSIS
    Creates SQL Server database override config files for integration tests.
.DESCRIPTION
    Enumerates all SQL Servers and databases in the resource group and generates
    multiple test configuration files: database target lists (with tags, bad targets,
    client targets, double-client targets) and a server.txt file. These files are
    used by ExternalTest projects to target the correct databases during integration tests.
.PARAMETER path
    Output directory for config files. Defaults to src\TestConfig.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
#>
param
(
    [string] $path,
    [string] $prefix
)

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot "src\TestConfig"
}

$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

$keyFileScript = Join-Path $repoRoot "scripts\key_file_names.ps1"
. $keyFileScript -prefix $prefix -path $path

Write-Host "Create Database override files for sql servers in resource group '$resourceGroupName'"  -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$outputDbConfigFile = Join-Path $path "databasetargets.cfg"
$databaseDbWithBadTargetConfigFile = Join-Path $path "databasetargets-badtargets.cfg"
$clientDbConfigFile = Join-Path $path "clientdbtargets.cfg"
$doubleClientDbConfigFile = Join-Path $path "clientdbtargets-doubledb.cfg"
$taggedDbConfigFile = Join-Path $path "databasetargets-tag.cfg"
$taggedClientDbConfigFile = Join-Path $path "clientdbtargets-tag.cfg"
$serverTextFile = Join-Path $path "server.txt"
$tag = @("TagA","TagB","TagC")
$counter = 0

$sqlServers =  (az sql server list --resource-group $resourceGroupName ) | ConvertFrom-Json
Write-Host "Using server targets: $sqlServers"  -ForegroundColor Cyan
foreach($server in $sqlServers)
{
    $dbs = az sql db list --resource-group $resourceGroupName --server "$($server.name)" --query "[].name" -o tsv
    Write-Host "Server $server databases: $dbs"  -ForegroundColor Cyan
    foreach($db in $dbs)
    {
        if($db -ne "master")
        {
            if($counter -gt 2)
            {
                $counter = 0
            }
            $outputDbConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,"+$db) 
            $databaseDbWithBadTargetConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,"+$db) 
            $taggedDbConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,"+$db +"#" + $tag[$counter])
            $taggedClientDbConfig += ,@($server.fullyQualifiedDomainName + ":client,"+$db +"#" + $tag[$counter])
            $clientDbConfig += ,@($server.fullyQualifiedDomainName + ":client,"+$db) 
            $counter = $counter +1; 
        }
        

    }
    $databaseDbWithBadTargetConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,ThisIsABadDbName") 
}

foreach($server in $sqlServers)
{
    foreach($db in $dbs)
    {
        if($db -ne "master")
        {
            $dbNum = ($db -replace "SqlBuildTest", "")/1
            if($dbNum % 2 -eq 0)
            {
                $doubleClientDbConfig += ,@($server.fullyQualifiedDomainName + ":client,"+$db +";client2,SqlBuildTest"+ ($dbNum-1).ToString() )
            }
        }
    }
}
    <# Action that will repeat until the condition is met #>

Write-Host "Writing test database config to   $outputDbConfigFile" -ForegroundColor DarkGreen
$outputDbConfig | Set-Content -Path $outputDbConfigFile

Write-Host "Writing test database config to path $clientDbConfigFile" -ForegroundColor DarkGreen
$clientDbConfig | Set-Content -Path $clientDbConfigFile

Write-Host "Writing test database config with tags to path $taggedClientDbConfigFile" -ForegroundColor DarkGreen
$taggedClientDbConfigFile | Set-Content -Path $taggedClientDbConfigFile

Write-Host "Writing test database config to path $doubleClientDbConfigFile" -ForegroundColor DarkGreen
$doubleClientDbConfig | Set-Content -Path $doubleClientDbConfigFile

Write-Host "Writing test database config to path $databaseDbWithBadTargetConfigFile" -ForegroundColor DarkGreen
$databaseDbWithBadTargetConfig | Set-Content -Path $databaseDbWithBadTargetConfigFile

Write-Host "Writing test database config with tags to path $taggedDbConfigFile" -ForegroundColor DarkGreen
$taggedDbConfig | Set-Content -Path $taggedDbConfigFile

Write-Host "Creating server.txt file for SQL Query override config tests" -ForegroundColor DarkGreen
$sqlServers[0].fullyQualifiedDomainName.trim()   | Set-Content -Path $serverTextFile

