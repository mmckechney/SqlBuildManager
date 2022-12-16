param
(
    [string] $path,
    [string] $resourceGroupName
)

Write-Host "Create Database override files"  -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$outputDbConfigFile = Join-Path $path "databasetargets.cfg"
$databaseDbWithBadTargetConfigFile = Join-Path $path "databasetargets-badtargets.cfg"
$clientDbConfigFile = Join-Path $path "clientdbtargets.cfg"
$doubleClientDbConfigFile = Join-Path $path "clientdbtargets-doubledb.cfg"

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
            $outputDbConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,"+$db) 
            $databaseDbWithBadTargetConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,"+$db) 
            $clientDbConfig += ,@($server.fullyQualifiedDomainName + ":client,"+$db) 
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

Write-Host "Writing test database config to  path set to $outputDbConfigFile" -ForegroundColor DarkGreen
$outputDbConfig | Set-Content -Path $outputDbConfigFile

Write-Host "Writing test database config to  path set to $clientDbConfigFile" -ForegroundColor DarkGreen
$clientDbConfig | Set-Content -Path $clientDbConfigFile

Write-Host "Writing test database config to  path set to $doubleClientDbConfigFile" -ForegroundColor DarkGreen
$doubleClientDbConfig | Set-Content -Path $doubleClientDbConfigFile

Write-Host "Writing test database config to  path set to $databaseDbWithBadTargetConfigFile" -ForegroundColor DarkGreen
$databaseDbWithBadTargetConfig | Set-Content -Path $databaseDbWithBadTargetConfigFile