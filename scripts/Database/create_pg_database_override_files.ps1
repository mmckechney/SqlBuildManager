param
(
    [string] $path,
    [string] $prefix
)

<#
.SYNOPSIS
    Creates PostgreSQL database override config files for integration tests.

.DESCRIPTION
    Enumerates databases on the Azure PostgreSQL Flexible Server and generates
    config files in the same format as create_database_override_files.ps1 but
    targeting PostgreSQL databases (sbm_pg_test1..N).

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER path
    Output path for the generated config files.
#>

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

Write-Host "Create PostgreSQL database override files for server '$pgServerName' in resource group '$resourceGroupName'" -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$outputDbConfigFile = Join-Path $path "pg-databasetargets.cfg"
$clientDbConfigFile = Join-Path $path "pg-clientdbtargets.cfg"
$doubleClientDbConfigFile = Join-Path $path "pg-clientdbtargets-doubledb.cfg"
$pgServerTextFile = Join-Path $path "pg-server.txt"

# Get the PG server FQDN
$pgServer = az postgres flexible-server show --resource-group $resourceGroupName --name $pgServerName | ConvertFrom-Json
if ($null -eq $pgServer) {
    Write-Host "ERROR: Could not find PostgreSQL server '$pgServerName' in resource group '$resourceGroupName'" -ForegroundColor Red
    exit 1
}

$pgFqdn = $pgServer.fullyQualifiedDomainName
Write-Host "PostgreSQL Server FQDN: $pgFqdn" -ForegroundColor DarkGreen

# List databases (excluding system databases)
$dbs = az postgres flexible-server db list --resource-group $resourceGroupName --server-name $pgServerName --query "[].name" -o tsv
Write-Host "Databases found: $dbs" -ForegroundColor Cyan

$outputDbConfig = @()
$clientDbConfig = @()
$doubleClientDbConfig = @()

foreach ($db in $dbs) {
    if ($db -ne "postgres" -and $db -ne "azure_maintenance" -and $db -ne "azure_sys") {
        # Standard config: server:override,target
        $outputDbConfig += "$($pgFqdn):sbm_pg_test,$db"
        # Client config: server:client,target
        $clientDbConfig += "$($pgFqdn):client,$db"
    }
}

# Double-client config: pair even/odd databases
$testDbs = $dbs | Where-Object { $_ -match '^sbm_pg_test\d+$' } | Sort-Object
for ($i = 0; $i -lt $testDbs.Count; $i += 2) {
    if ($i + 1 -lt $testDbs.Count) {
        $doubleClientDbConfig += "$($pgFqdn):client,$($testDbs[$i]);client2,$($testDbs[$i+1])"
    }
}

Write-Host "Writing PostgreSQL database config to $outputDbConfigFile" -ForegroundColor DarkGreen
$outputDbConfig | Set-Content -Path $outputDbConfigFile

Write-Host "Writing PostgreSQL client database config to $clientDbConfigFile" -ForegroundColor DarkGreen
$clientDbConfig | Set-Content -Path $clientDbConfigFile

Write-Host "Writing PostgreSQL double-client database config to $doubleClientDbConfigFile" -ForegroundColor DarkGreen
$doubleClientDbConfig | Set-Content -Path $doubleClientDbConfigFile

Write-Host "Writing PostgreSQL server.txt to $pgServerTextFile" -ForegroundColor DarkGreen
$pgFqdn.Trim() | Set-Content -Path $pgServerTextFile

# Also save PG admin password to a file for test use
$pgPwFile = Join-Path $path "pg-pw.txt"
$pgAdminPassword = azd env get-value PG_ADMIN_PASSWORD 2>$null
if (-not [string]::IsNullOrWhiteSpace($pgAdminPassword) -and $pgAdminPassword -notlike "ERROR:*") {
    $pgAdminPassword | Set-Content -Path $pgPwFile
    Write-Host "Writing PostgreSQL admin password to $pgPwFile" -ForegroundColor DarkGreen
}

# Save PG admin username
$pgUnFile = Join-Path $path "pg-un.txt"
$pgAdminUser = "${prefix}pgadmin"
$pgAdminUser | Set-Content -Path $pgUnFile
Write-Host "Writing PostgreSQL admin username to $pgUnFile" -ForegroundColor DarkGreen

Write-Host ""
Write-Host "PostgreSQL database override files created successfully!" -ForegroundColor Green
