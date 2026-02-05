param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [ValidateSet("db_owner", "db_datareader", "db_datawriter")]
    [string] $databaseRole = "db_owner"
)

<#
.SYNOPSIS
    Grants the current Entra ID user access to all SQL databases.

.DESCRIPTION
    This script connects to each SQL Server and database using the current user's Entra ID credentials
    (who must already be an admin on the SQL server) and grants themselves the specified role on all databases.
    
    This is useful for developers who need to interact with the databases for testing.
    
    Prerequisites:
    - The current user must be an Entra ID admin on the SQL Server
    - Az CLI must be installed and logged in
    - SqlServer PowerShell module must be installed (Install-Module -Name SqlServer)

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group containing the SQL servers.

.PARAMETER databaseRole
    The database role to grant. Default is db_owner.
    Options: db_owner, db_datareader, db_datawriter

.EXAMPLE
    .\grant_current_user_permissions.ps1 -prefix "myprefix" -resourceGroupName "myprefix-rg"
#>

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

# Check if SqlServer module is installed
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Host "SqlServer PowerShell module not found. Installing..." -ForegroundColor Yellow
    Install-Module -Name SqlServer -Force -AllowClobber -Scope CurrentUser
}
Import-Module SqlServer

# Get the current user's details
Write-Host "Retrieving current user details..." -ForegroundColor DarkGreen
$currentUserEmail = az account show --query user.name -o tsv
$currentUserObjectId = az ad signed-in-user show --query id -o tsv

if ($null -eq $currentUserEmail -or $currentUserEmail -eq "") {
    Write-Host "ERROR: Could not determine current user. Make sure you are logged in with 'az login'" -ForegroundColor Red
    exit 1
}

Write-Host "Current User: $currentUserEmail" -ForegroundColor DarkGreen
Write-Host "Object ID: $currentUserObjectId" -ForegroundColor DarkGreen
Write-Host "Database Role: $databaseRole" -ForegroundColor DarkGreen

# Get an access token for Azure SQL using the current user's Entra ID credentials
Write-Host "Obtaining access token for Azure SQL..." -ForegroundColor DarkGreen
$accessToken = az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv
if ($null -eq $accessToken -or $accessToken -eq "") {
    Write-Host "ERROR: Could not obtain access token for Azure SQL." -ForegroundColor Red
    exit 1
}

# Get all SQL servers in the resource group
$sqlServers = az sql server list --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $sqlServers -or $sqlServers.Count -eq 0) {
    Write-Host "No SQL servers found in resource group '$resourceGroupName'" -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($sqlServers.Count) SQL server(s)" -ForegroundColor DarkGreen

# Get current IP address and add firewall rules
$currentIpAddress = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content.Trim()
Write-Host "Current IP Address: $currentIpAddress" -ForegroundColor DarkGreen
$firewallRuleName = "GrantUserPermissions_TempRule"

# Add temporary firewall rules to all SQL servers
foreach ($server in $sqlServers) {
    Write-Host "Adding temporary firewall rule to $($server.name)..." -ForegroundColor DarkGreen
    az sql server firewall-rule create --resource-group $resourceGroupName --server $server.name --name $firewallRuleName --start-ip-address $currentIpAddress --end-ip-address $currentIpAddress --output none 2>$null
}

# Wait a moment for firewall rules to propagate
Write-Host "Waiting for firewall rules to propagate..." -ForegroundColor DarkGreen
Start-Sleep -Seconds 10

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

        # SQL to create user and grant role
        $sql = @"
-- Check if user already exists
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '$currentUserEmail')
BEGIN
    -- Create user from external provider (Entra ID / Azure AD)
    CREATE USER [$currentUserEmail] FROM EXTERNAL PROVIDER;
    PRINT 'Created user [$currentUserEmail]';
END
ELSE
BEGIN
    PRINT 'User [$currentUserEmail] already exists';
END

-- Add user to role if not already a member
IF NOT EXISTS (
    SELECT * FROM sys.database_role_members drm
    INNER JOIN sys.database_principals dp ON drm.member_principal_id = dp.principal_id
    INNER JOIN sys.database_principals dr ON drm.role_principal_id = dr.principal_id
    WHERE dp.name = '$currentUserEmail' AND dr.name = '$databaseRole'
)
BEGIN
    ALTER ROLE [$databaseRole] ADD MEMBER [$currentUserEmail];
    PRINT 'Added [$currentUserEmail] to role [$databaseRole]';
END
ELSE
BEGIN
    PRINT 'User [$currentUserEmail] is already a member of role [$databaseRole]';
END
"@

        try {
            # Execute SQL using the access token
            Invoke-Sqlcmd -ServerInstance $serverFqdn -Database $dbName -AccessToken $accessToken -Query $sql -ErrorAction Stop
            Write-Host "    ✓ Granted $databaseRole to $currentUserEmail" -ForegroundColor Green
        }
        catch {
            Write-Host "    ✗ Failed to grant permissions: $_" -ForegroundColor Red
        }
    }
}

# Clean up temporary firewall rules
Write-Host "`nCleaning up temporary firewall rules..." -ForegroundColor DarkGreen
foreach ($server in $sqlServers) {
    az sql server firewall-rule delete --resource-group $resourceGroupName --server $server.name --name $firewallRuleName --output none 2>$null
    Write-Host "  Removed firewall rule from $($server.name)" -ForegroundColor DarkGreen
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Current User SQL Permissions Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nThe current user '$currentUserEmail' has been granted '$databaseRole' access to all databases." -ForegroundColor Green
