param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [string] $path,

    [ValidateSet("db_owner", "db_datareader", "db_datawriter")]
    [string] $databaseRole = "db_owner"
)

<#
.SYNOPSIS
    Grants the managed identity access to all SQL databases using Entra ID (Azure AD) authentication.

.DESCRIPTION
    This script connects to each SQL Server and database using the active Azure identity
    and creates a database user for the managed identity, then grants it the specified role.
    
    Prerequisites:
    - The active Azure identity must be an Entra ID admin on the SQL Server
    - The host running this script must have connectivity to the SQL private endpoints
    - Az CLI must be installed and logged in
    - SqlServer PowerShell module must be installed (Install-Module -Name SqlServer)

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group containing the SQL servers.

.PARAMETER databaseRole
    The database role to grant to the managed identity. Default is db_owner.
    Options: db_owner, db_datareader, db_datawriter

.EXAMPLE
    .\grant_identity_permissions.ps1 -prefix "myprefix" -resourceGroupName "myprefix-rg"
#>

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot 'src' 'TestConfig'
}

#############################################
# Get set resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot 'scripts' 'prefix_resource_names.ps1'
. $prefixScript -prefix $prefix

Write-Host "Granting Managed Identity '$identityName' access to SQL databases" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Database Role: $databaseRole" -ForegroundColor DarkGreen

# Check if SqlServer module is installed
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Host "SqlServer PowerShell module not found. Installing..." -ForegroundColor Yellow
    Install-Module -Name SqlServer -Force -AllowClobber -Scope CurrentUser
}
Import-Module SqlServer

# Get the managed identity details
Write-Host "Retrieving managed identity details..." -ForegroundColor DarkGreen
$identity = az identity show --name $identityName --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $identity) {
    Write-Host "ERROR: Could not find managed identity '$identityName' in resource group '$resourceGroupName'" -ForegroundColor Red
    exit 1
}

$identityClientId = $identity.clientId
Write-Host "Managed Identity Name: $identityName" -ForegroundColor DarkGreen
Write-Host "Managed Identity Client ID: $identityClientId" -ForegroundColor DarkGreen

# Get an access token for Azure SQL using the current user's Entra ID credentials
Write-Host "Obtaining access token for Azure SQL..." -ForegroundColor DarkGreen
$accessToken = az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv
if ($null -eq $accessToken -or $accessToken -eq "") {
    Write-Host "ERROR: Could not obtain access token for Azure SQL. Make sure you are logged in with 'az login'" -ForegroundColor Red
    exit 1
}

# Get all SQL servers in the resource group
$sqlServers = az sql server list --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $sqlServers -or $sqlServers.Count -eq 0) {
    Write-Host "No SQL servers found in resource group '$resourceGroupName'" -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($sqlServers.Count) SQL server(s)" -ForegroundColor DarkGreen

$failureCount = 0
foreach ($server in $sqlServers) {
    $serverFqdn = $server.fullyQualifiedDomainName
    Write-Host "`nProcessing SQL Server: $serverFqdn" -ForegroundColor Cyan

    # Get all databases on this server (excluding master)
    $databases = az sql db list --resource-group $resourceGroupName --server $server.name --query "[?name!='master'].name" -o tsv

    if ($null -eq $databases -or $databases.Count -eq 0) {
        Write-Host "  No user databases found on server $($server.name)" -ForegroundColor Yellow
        continue
    }

    foreach ($dbName in $databases) {
        Write-Host "  Processing database: $dbName" -ForegroundColor DarkGreen

        # Create the external user from its client ID without requiring Microsoft Graph
        # directory lookup permissions on the SQL logical server.
        $sql = @"
DECLARE @name SYSNAME = '$identityName';
DECLARE @clientId UNIQUEIDENTIFIER = '$identityClientId';
DECLARE @sid NVARCHAR(34) = CONVERT(VARCHAR(34), CONVERT(VARBINARY(16), @clientId), 1);

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '$identityName')
BEGIN
    DECLARE @createUser NVARCHAR(MAX) = N'CREATE USER [' + @name + '] WITH SID = ' + @sid + ', TYPE = E;';
    EXEC (@createUser);
    PRINT 'Created user [$identityName]';
END
ELSE
BEGIN
    PRINT 'User [$identityName] already exists';
END

-- Add user to role if not already a member
IF NOT EXISTS (
    SELECT * FROM sys.database_role_members drm
    INNER JOIN sys.database_principals dp ON drm.member_principal_id = dp.principal_id
    INNER JOIN sys.database_principals dr ON drm.role_principal_id = dr.principal_id
    WHERE dp.name = '$identityName' AND dr.name = '$databaseRole'
)
BEGIN
    ALTER ROLE [$databaseRole] ADD MEMBER [$identityName];
    PRINT 'Added [$identityName] to role [$databaseRole]';
END
ELSE
BEGIN
    PRINT 'User [$identityName] is already a member of role [$databaseRole]';
END
"@

        $success = $false
        for ($attempt = 1; $attempt -le 5 -and -not $success; $attempt++) {
            try {
                Invoke-Sqlcmd -ServerInstance $serverFqdn -Database $dbName -AccessToken $accessToken -Query $sql -ErrorAction Stop
                Write-Host "    ✓ Granted $databaseRole to $identityName" -ForegroundColor Green
                $success = $true
            }
            catch {
                if ($attempt -lt 5) {
                    Write-Host "    Connection attempt $attempt failed; retrying while private DNS and administrator changes propagate..." -ForegroundColor Yellow
                    Start-Sleep -Seconds 15
                }
                else {
                    Write-Host "    ✗ Failed to grant permissions: $($_.Exception.Message)" -ForegroundColor Red
                    $failureCount++
                }
            }
        }
    }
}

if ($failureCount -gt 0) {
    Write-Error "Failed to grant SQL permissions on $failureCount database(s)."
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Managed Identity SQL Permissions Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nThe managed identity '$identityName' has been granted '$databaseRole' access to all databases." -ForegroundColor Green
Write-Host "Applications using this identity can now connect using DefaultAzureCredential." -ForegroundColor Green
