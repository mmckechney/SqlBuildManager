Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$pgBootstrapAccessToken = [Environment]::GetEnvironmentVariable('PG_BOOTSTRAP_ACCESS_TOKEN')
$mySqlBootstrapAccessToken = [Environment]::GetEnvironmentVariable('MYSQL_BOOTSTRAP_ACCESS_TOKEN')
[Environment]::SetEnvironmentVariable('PG_BOOTSTRAP_ACCESS_TOKEN', $null)
[Environment]::SetEnvironmentVariable('MYSQL_BOOTSTRAP_ACCESS_TOKEN', $null)

function Get-RequiredEnvironmentVariable {
    param([Parameter(Mandatory=$true)][string] $Name)

    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Required environment variable '$Name' is not set."
    }
    return $value
}

$envName = Get-RequiredEnvironmentVariable 'ENV_NAME'
$resourceGroupName = Get-RequiredEnvironmentVariable 'RESOURCE_GROUP_NAME'
$subscriptionId = Get-RequiredEnvironmentVariable 'SUBSCRIPTION_ID'
$postProvisionClientId = Get-RequiredEnvironmentVariable 'POSTPROVISION_CLIENT_ID'
$mySqlAuthMode = [Environment]::GetEnvironmentVariable('MYSQL_AUTH_MODE')
if ([string]::IsNullOrWhiteSpace($mySqlAuthMode)) {
    $mySqlAuthMode = 'ManagedIdentity'
}
if ($mySqlAuthMode -notin @('ManagedIdentity', 'Password')) {
    throw "Invalid MYSQL_AUTH_MODE='$mySqlAuthMode'."
}

try {
Write-Host "Authenticating private post-provision container with managed identity..." -ForegroundColor Cyan
az login --identity --client-id $postProvisionClientId --allow-no-subscriptions --output none
if ($LASTEXITCODE -ne 0) {
    throw 'Managed identity authentication failed.'
}

az account set --subscription $subscriptionId
if ($LASTEXITCODE -ne 0) {
    throw "Unable to select subscription '$subscriptionId'."
}

$env:AZD_PROJECT_PATH = '/bootstrap'
$failed = $false

if ($env:DEPLOY_MYSQL -eq 'true') {
    if ($mySqlAuthMode -eq 'ManagedIdentity') {
        Write-Host "Running MySQL private initialization before the administrator token expires..." -ForegroundColor Cyan
        try {
            [Environment]::SetEnvironmentVariable('MYSQL_BOOTSTRAP_ACCESS_TOKEN', $mySqlBootstrapAccessToken)
            & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_mysql_identity_permissions.ps1' `
                -envName $envName `
                -resourceGroupName $resourceGroupName
            if ($LASTEXITCODE -ne 0) {
                $failed = $true
            }
        }
        finally {
            [Environment]::SetEnvironmentVariable('MYSQL_BOOTSTRAP_ACCESS_TOKEN', $null)
            $mySqlBootstrapAccessToken = $null
        }
    }
    else {
        Write-Host "Skipping MySQL managed identity initialization because MYSQL_AUTH_MODE='$mySqlAuthMode'." -ForegroundColor DarkGray
    }
}

if ($env:DEPLOY_POSTGRESQL -eq 'true') {
    Write-Host "Running PostgreSQL private initialization with the short-lived deploying-user token..." -ForegroundColor Cyan
    try {
        [Environment]::SetEnvironmentVariable('PG_BOOTSTRAP_ACCESS_TOKEN', $pgBootstrapAccessToken)
        & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_pg_identity_permissions.ps1' `
            -envName $envName `
            -resourceGroupName $resourceGroupName
        if ($LASTEXITCODE -ne 0) {
            $failed = $true
        }
    }
    finally {
        [Environment]::SetEnvironmentVariable('PG_BOOTSTRAP_ACCESS_TOKEN', $null)
        $pgBootstrapAccessToken = $null
    }
}

if ($env:DEPLOY_SQLSERVER -eq 'true') {
    Write-Host "Running SQL Server private initialization..." -ForegroundColor Cyan
    & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_identity_permissions.ps1' `
        -envName $envName `
        -resourceGroupName $resourceGroupName
    if ($LASTEXITCODE -ne 0) {
        $failed = $true
    }
    if ($env:DEPLOY_RELAY_PROXY -eq 'true') {
        & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_identity_permissions.ps1' `
            -envName $envName `
            -resourceGroupName $resourceGroupName `
            -identityPurpose Relay
        if ($LASTEXITCODE -ne 0) {
            $failed = $true
        }
    }
}

if ($failed) {
    throw 'One or more private post-provision initialization steps failed.'
}

Write-Host "Private post-provision initialization completed." -ForegroundColor Green
}
finally {
    [Environment]::SetEnvironmentVariable('MYSQL_BOOTSTRAP_ACCESS_TOKEN', $null)
    [Environment]::SetEnvironmentVariable('PG_BOOTSTRAP_ACCESS_TOKEN', $null)
    $pgBootstrapAccessToken = $null
    $mySqlBootstrapAccessToken = $null
}
