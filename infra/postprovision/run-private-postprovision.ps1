Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

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
    $mySqlAuthMode = 'Password'
}

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

if ($env:DEPLOY_SQLSERVER -eq 'true') {
    Write-Host "Running SQL Server private initialization..." -ForegroundColor Cyan
    & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_identity_permissions.ps1' `
        -envName $envName `
        -resourceGroupName $resourceGroupName
    if ($LASTEXITCODE -ne 0) {
        $failed = $true
    }
}

if ($env:DEPLOY_POSTGRESQL -eq 'true') {
    Write-Host "Running PostgreSQL private initialization..." -ForegroundColor Cyan
    & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_pg_identity_permissions.ps1' `
        -envName $envName `
        -resourceGroupName $resourceGroupName
    if ($LASTEXITCODE -ne 0) {
        $failed = $true
    }
}

if ($env:DEPLOY_MYSQL -eq 'true') {
    if ($mySqlAuthMode -eq 'ManagedIdentity') {
        Write-Host "Running MySQL private initialization..." -ForegroundColor Cyan
        & pwsh -NoLogo -NoProfile -File '/bootstrap/scripts/Database/grant_mysql_identity_permissions.ps1' `
            -envName $envName `
            -resourceGroupName $resourceGroupName
        if ($LASTEXITCODE -ne 0) {
            $failed = $true
        }
    }
    else {
        Write-Host "Skipping MySQL managed identity initialization because MYSQL_AUTH_MODE='$mySqlAuthMode'." -ForegroundColor DarkGray
    }
}

if ($failed) {
    throw 'One or more private post-provision initialization steps failed.'
}

Write-Host "Private post-provision initialization completed." -ForegroundColor Green
