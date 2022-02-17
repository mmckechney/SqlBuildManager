param
(
    [string] $path,
    [string] $resourceGroupName
)

Write-Host "Create Database override files"  -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$outputDbConfigFile = Join-Path $path "databasetargets.cfg"
$clientDbConfigFile = Join-Path $path "clientdbtargets.cfg"

$sqlServers =  (az sql server list --resource-group $resourceGroupName ) | ConvertFrom-Json -AsHashtable
foreach($server in $sqlServers)
{
    $dbs = az sql db list --resource-group $resourceGroupName --server "$($server.name)" --query "[].name" -o tsv

    foreach($db in $dbs)
    {
        if($db -ne "master")
        {
            $outputDbConfig += ,@($server.fullyQualifiedDomainName + ":SqlBuildTest,"+$db) 
            $clientDbConfig += ,@($server.fullyQualifiedDomainName + ":client,"+$db) 
        }
    }
}
Write-Host "Writing test database config to  path set to $outputDbConfigFile" -ForegroundColor DarkGreen
$outputDbConfig | Set-Content -Path $outputDbConfigFile

Write-Host "Writing test database config to  path set to $clientDbConfigFile" -ForegroundColor DarkGreen
$clientDbConfig | Set-Content -Path $clientDbConfigFile