param
(
    [Parameter(Mandatory=$true)]
    [string] $envName,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName
)

<#
.SYNOPSIS
    Grants Microsoft Graph app permissions required for MySQL Entra user creation.

.DESCRIPTION
    Azure Database for MySQL Flexible Server uses its assigned user-managed identity to
    resolve Entra principals during CREATE AADUSER. This script grants the required
    Microsoft Graph application permissions to that identity:
      - User.Read.All
      - GroupMember.Read.All
      - Application.Read.All

    Requires a signed-in operator with Entra directory permissions to assign app roles
    (Privileged Role Administrator or Global Administrator).
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -envName $envName

$postProvisionIdentity = az identity show --name $postProvisionIdentityName --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $postProvisionIdentity -or [string]::IsNullOrWhiteSpace($postProvisionIdentity.principalId)) {
    Write-Error "Unable to resolve the post-provision managed identity '$postProvisionIdentityName' in resource group '$resourceGroupName'."
    exit 1
}

$identityPrincipalId = $postProvisionIdentity.principalId
Write-Host "Ensuring Graph permissions for MySQL server identity '$postProvisionIdentityName' ($identityPrincipalId)" -ForegroundColor Cyan

$graphAppId = '00000003-0000-0000-c000-000000000000'
$graphServicePrincipal = az ad sp show --id $graphAppId | ConvertFrom-Json
if ($null -eq $graphServicePrincipal -or [string]::IsNullOrWhiteSpace($graphServicePrincipal.id)) {
    Write-Error "Unable to resolve Microsoft Graph service principal."
    exit 1
}

$graphResourceId = $graphServicePrincipal.id
$requiredPermissionValues = @('User.Read.All', 'GroupMember.Read.All', 'Application.Read.All')
$requiredRoles = @()

foreach ($permissionValue in $requiredPermissionValues) {
    $role = $graphServicePrincipal.appRoles |
        Where-Object {
            $_.value -eq $permissionValue -and
            $_.isEnabled -eq $true -and
            ($_.allowedMemberTypes -contains 'Application')
        } |
        Select-Object -First 1

    if ($null -eq $role) {
        Write-Error "Unable to locate Microsoft Graph app role '$permissionValue'."
        exit 1
    }

    $requiredRoles += $role
}

$assignmentResponse = & az rest `
    --method GET `
    --url "https://graph.microsoft.com/v1.0/servicePrincipals/$identityPrincipalId/appRoleAssignments?`$top=999" `
    --only-show-errors 2>&1
if ($LASTEXITCODE -ne 0) {
    $assignmentError = ($assignmentResponse | Out-String).Trim()
    Write-Error "Unable to read current Microsoft Graph app-role assignments: $assignmentError"
    exit 1
}
$existingAssignments = @((($assignmentResponse | ConvertFrom-Json).value))

$changesApplied = $false
foreach ($role in $requiredRoles) {
    $alreadyAssigned = $existingAssignments | Where-Object {
        $_.resourceId -eq $graphResourceId -and $_.appRoleId -eq $role.id
    } | Select-Object -First 1

    if ($null -ne $alreadyAssigned) {
        Write-Host "  ✓ $($role.value) already assigned" -ForegroundColor DarkGreen
        continue
    }

    $payload = @{
        principalId = $identityPrincipalId
        resourceId  = $graphResourceId
        appRoleId   = $role.id
    } | ConvertTo-Json -Compress

    $payloadFile = [System.IO.Path]::GetTempFileName()
    try {
        Set-Content -Path $payloadFile -Value $payload -Encoding ascii -NoNewline
        $postOutput = & az rest `
            --method POST `
            --url "https://graph.microsoft.com/v1.0/servicePrincipals/$identityPrincipalId/appRoleAssignments" `
            --headers "Content-Type=application/json" `
            --body "@$payloadFile" `
            --only-show-errors `
            --output none 2>&1
    }
    finally {
        Remove-Item $payloadFile -Force -ErrorAction SilentlyContinue
    }

    if ($LASTEXITCODE -ne 0) {
        $errorMessage = ($postOutput | Out-String).Trim()
        if ($errorMessage -match 'Insufficient privileges|Authorization_RequestDenied|does not have authorization') {
            Write-Error "Unable to assign Microsoft Graph permission '$($role.value)'. Your signed-in account needs Privileged Role Administrator (or Global Administrator) in Entra ID."
            exit 1
        }

        Write-Error "Unable to assign Microsoft Graph permission '$($role.value)': $errorMessage"
        exit 1
    }

    Write-Host "  ✓ Assigned $($role.value)" -ForegroundColor Green
    $changesApplied = $true
}

if ($changesApplied) {
    Write-Host "Graph permissions were updated. Waiting briefly for propagation..." -ForegroundColor DarkGreen
    Start-Sleep -Seconds 60
}
