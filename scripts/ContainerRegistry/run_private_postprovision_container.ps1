param
(
    [Parameter(Mandatory=$true)]
    [string] $envName,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [Parameter(Mandatory=$true)]
    [string] $repoRoot,

    [bool] $deploySqlServer,

    [bool] $deployPostgreSQL,

    [bool] $deployMySQL,

    [bool] $deployRelayProxy = $false,

    [bool] $mySqlUseManagedIdentityAuth = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-AzdValue {
    param([Parameter(Mandatory=$true)][string] $Name)

    $value = azd env get-value $Name 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($value) -or $value -like 'ERROR:*') {
        throw "Required azd environment value '$Name' is unavailable."
    }
    return $value
}

function Invoke-Az {
    param(
        [Parameter(Mandatory=$true)][string[]] $Arguments,
        [Parameter(Mandatory=$true)][string] $FailureMessage
    )

    & az @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $FailureMessage
    }
}

$containerRegistryName = Get-AzdValue 'CONTAINER_REGISTRY_NAME'
$containerRegistryLoginServer = Get-AzdValue 'CONTAINER_REGISTRY_LOGIN_SERVER'
$aciSubnetId = Get-AzdValue 'ACI_SUBNET_ID'
$postProvisionIdentityName = Get-AzdValue 'POSTPROVISION_IDENTITY_NAME'
$postProvisionIdentityId = Get-AzdValue 'POSTPROVISION_IDENTITY_ID'
$postProvisionClientId = Get-AzdValue 'POSTPROVISION_IDENTITY_CLIENT_ID'
$postProvisionPrincipalId = Get-AzdValue 'POSTPROVISION_IDENTITY_PRINCIPAL_ID'
$targetIdentityName = Get-AzdValue 'MANAGED_IDENTITY_NAME'
$subscriptionId = az account show --query id -o tsv
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($subscriptionId)) {
    throw 'Unable to determine the active Azure subscription.'
}

$resourceGroupOverride = $resourceGroupName
. (Join-Path $repoRoot 'scripts/prefix_resource_names.ps1') -envName $envName
$resourceGroupName = $resourceGroupOverride
$containerName = $postProvisionContainerName
$imageName = 'sqlbuildmanager-postprovision:latest'
$image = "${containerRegistryLoginServer}/${imageName}"
$tempBuildContext = Join-Path $repoRoot ".sbm-postprovision-$([guid]::NewGuid())"
$sqlServers = @()
$sqlServersWithBootstrapAdmin = @()
$sqlOriginalAdministrators = @{}
$cleanupErrors = [System.Collections.Generic.List[string]]::new()
$created = $false
$containerCreationAttempted = $false
$needsDeployer = $deployPostgreSQL -or ($deployMySQL -and $mySqlUseManagedIdentityAuth)
$deployerPrincipalId = if ($needsDeployer) { Get-AzdValue 'AZURE_PRINCIPAL_ID' } else { $null }
$deployerPrincipalName = if ($needsDeployer) { Get-AzdValue 'AZURE_PRINCIPAL_NAME' } else { $null }
$mySqlBootstrapToken = $null
$pgBootstrapToken = $null
$databaseBootstrapToken = $null
$secureEnvironmentValues = @()

try {
    New-Item -Path (Join-Path $tempBuildContext 'scripts/Database') -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $repoRoot 'infra/postprovision/Dockerfile') (Join-Path $tempBuildContext 'Dockerfile')
    Copy-Item (Join-Path $repoRoot 'infra/postprovision/run-private-postprovision.ps1') (Join-Path $tempBuildContext 'run-private-postprovision.ps1')
    New-Item -Path (Join-Path $tempBuildContext 'infra') -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $repoRoot 'infra/resourcetypes.json') (Join-Path $tempBuildContext 'infra/resourcetypes.json')
    Copy-Item (Join-Path $repoRoot 'scripts/prefix_resource_names.ps1') (Join-Path $tempBuildContext 'scripts/prefix_resource_names.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/Database/grant_identity_permissions.ps1') (Join-Path $tempBuildContext 'scripts/Database/grant_identity_permissions.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/Database/grant_pg_identity_permissions.ps1') (Join-Path $tempBuildContext 'scripts/Database/grant_pg_identity_permissions.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/Database/grant_mysql_identity_permissions.ps1') (Join-Path $tempBuildContext 'scripts/Database/grant_mysql_identity_permissions.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/Database/mysql_entra_helpers.ps1') (Join-Path $tempBuildContext 'scripts/Database/mysql_entra_helpers.ps1')

    Write-Host "Building private post-provision image remotely in ACR..." -ForegroundColor Cyan
    Push-Location $tempBuildContext
    try {
        Invoke-Az -Arguments @(
            'acr', 'build',
            '--registry', $containerRegistryName,
            '--resource-group', $resourceGroupName,
            '--image', $imageName,
            '--file', 'Dockerfile',
            '--no-logs',
            '--output', 'none',
            '.'
        ) -FailureMessage 'The ACR build for the private post-provision image failed.'
    }
    finally {
        Pop-Location
    }

    if ($deploySqlServer) {
        $sqlServers = @(az sql server list --resource-group $resourceGroupName --query '[].name' -o tsv)
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to enumerate SQL servers before assigning the bootstrap administrator.'
        }

        foreach ($server in ($sqlServers | Where-Object { $_ -in @($sqlServerNameA, $sqlServerNameB) })) {
            $originalAdmins = @(az sql server ad-admin list --resource-group $resourceGroupName --server-name $server --output json | ConvertFrom-Json)
            if ($LASTEXITCODE -ne 0 -or $originalAdmins.Count -ne 1 -or
                [string]::IsNullOrWhiteSpace($originalAdmins[0].login) -or
                [string]::IsNullOrWhiteSpace($originalAdmins[0].sid)) {
                throw "Cannot safely capture the existing SQL Entra administrator on '$server'; no administrator will be replaced without a restorable identity."
            }
            if ($originalAdmins[0].sid -eq $postProvisionPrincipalId) {
                throw "SQL server '$server' already has the bootstrap identity as administrator. Recover its intended administrator explicitly before retrying; this fresh-deployment hook does not guess or migrate administrator state."
            }
            $sqlOriginalAdministrators[$server] = $originalAdmins[0]
            # Track before the write so an ambiguous CLI failure still triggers restoration.
            Write-Host "Temporarily assigning '$postProvisionIdentityName' as SQL Entra administrator on '$server'..." -ForegroundColor DarkGreen
            $sqlServersWithBootstrapAdmin += $server
            Invoke-Az -Arguments @(
                'sql', 'server', 'ad-admin', 'update',
                '--resource-group', $resourceGroupName,
                '--server-name', $server,
                '--display-name', $postProvisionIdentityName,
                '--object-id', $postProvisionPrincipalId,
                '--output', 'none'
            ) -FailureMessage "Unable to assign the bootstrap SQL administrator on '$server'."
        }
        Start-Sleep -Seconds 20
    }

    $null = az container show --resource-group $resourceGroupName --name $containerName --output none 2>$null
    if ($LASTEXITCODE -eq 0) {
        Invoke-Az -Arguments @(
            'container', 'delete',
            '--resource-group', $resourceGroupName,
            '--name', $containerName,
            '--yes'
        ) -FailureMessage "Unable to replace existing container group '$containerName'."
    }

    $createArguments = @(
        'container', 'create',
        '--resource-group', $resourceGroupName,
        '--name', $containerName,
        '--image', $image,
        '--registry-login-server', $containerRegistryLoginServer,
        '--acr-identity', $postProvisionIdentityId,
        '--assign-identity', $postProvisionIdentityId,
        '--subnet', $aciSubnetId,
        '--os-type', 'Linux',
        '--restart-policy', 'Never',
        '--cpu', '1',
        '--memory', '1.5',
        '--environment-variables',
        "ENV_NAME=$envName",
        "RESOURCE_GROUP_NAME=$resourceGroupName",
        "SUBSCRIPTION_ID=$subscriptionId",
        "POSTPROVISION_CLIENT_ID=$postProvisionClientId",
        "POSTPROVISION_IDENTITY_NAME=$postProvisionIdentityName",
        "TARGET_IDENTITY_NAME=$targetIdentityName",
        "DEPLOY_SQLSERVER=$($deploySqlServer.ToString().ToLowerInvariant())",
        "DEPLOY_POSTGRESQL=$($deployPostgreSQL.ToString().ToLowerInvariant())",
        "DEPLOY_MYSQL=$($deployMySQL.ToString().ToLowerInvariant())",
        "DEPLOY_RELAY_PROXY=$($deployRelayProxy.ToString().ToLowerInvariant())",
        "MYSQL_AUTH_MODE=$(if ($mySqlUseManagedIdentityAuth) { 'ManagedIdentity' } else { 'Password' })"
    )

    if ($needsDeployer) {
        $signedInUserId = az ad signed-in-user show --query id -o tsv
        if ($LASTEXITCODE -ne 0 -or $signedInUserId -ne $deployerPrincipalId) {
            throw "Sign in with the deploying Entra user '$deployerPrincipalName' before initializing PostgreSQL or MySQL."
        }
        $databaseBootstrapToken = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($databaseBootstrapToken)) {
            throw 'Unable to acquire the PostgreSQL/MySQL administrator token. Run az login and retry provisioning.'
        }
    }
    if ($deployMySQL -and $mySqlUseManagedIdentityAuth) {
        $mySqlBootstrapToken = $databaseBootstrapToken
        $createArguments += "MYSQL_ENTRA_ADMIN_LOGIN=$deployerPrincipalName"
        $secureEnvironmentValues += "MYSQL_BOOTSTRAP_ACCESS_TOKEN=$mySqlBootstrapToken"
    }
    if ($deployPostgreSQL) {
        # Never make the reusable bootstrap identity a PostgreSQL administrator.
        $pgBootstrapToken = $databaseBootstrapToken
        $createArguments += "PG_ENTRA_ADMIN_LOGIN=$deployerPrincipalName"
        $secureEnvironmentValues += "PG_BOOTSTRAP_ACCESS_TOKEN=$pgBootstrapToken"
    }
    if ($secureEnvironmentValues.Count -gt 0) {
        $createArguments += @('--secure-environment-variables') + $secureEnvironmentValues
    }
    $createArguments += @('--output', 'none')

    $created = $false
    for ($attempt = 1; $attempt -le 5 -and -not $created; $attempt++) {
        $containerCreationAttempted = $true
        & az @createArguments
        if ($LASTEXITCODE -eq 0) {
            $created = $true
            break
        }

        if ($attempt -lt 5) {
            Write-Host "ACI creation attempt $attempt failed while role assignments propagate; retrying..." -ForegroundColor Yellow
            Start-Sleep -Seconds 20
        }
    }
    if (-not $created) {
        throw "Unable to create private post-provision container '$containerName'."
    }

    $deadline = [DateTime]::UtcNow.AddMinutes(10)
    do {
        Start-Sleep -Seconds 5
        $provisioningState = az container show --resource-group $resourceGroupName --name $containerName --query provisioningState -o tsv
        $containerState = az container show --resource-group $resourceGroupName --name $containerName --query 'containers[0].instanceView.currentState.state' -o tsv

        if ($provisioningState -eq 'Failed') {
            throw "Container group '$containerName' failed to provision."
        }
        if ($containerState -in @('Running', 'Terminated')) {
            break
        }
    } while ([DateTime]::UtcNow -lt $deadline)

    if ($containerState -notin @('Running', 'Terminated')) {
        throw "Timed out waiting for private post-provision container '$containerName' to start."
    }

    if ($containerState -eq 'Running') {
        Write-Host "Streaming private initialization container output..." -ForegroundColor Cyan
        az container attach --resource-group $resourceGroupName --name $containerName
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Live container log attachment ended unexpectedly; retrieving the available logs."
            az container logs --resource-group $resourceGroupName --name $containerName
        }
    }
    else {
        az container logs --resource-group $resourceGroupName --name $containerName
    }

    $completionDeadline = [DateTime]::UtcNow.AddMinutes(30)
    while ($containerState -ne 'Terminated' -and [DateTime]::UtcNow -lt $completionDeadline) {
        Start-Sleep -Seconds 5
        $containerState = az container show --resource-group $resourceGroupName --name $containerName --query 'containers[0].instanceView.currentState.state' -o tsv
    }
    if ($containerState -ne 'Terminated') {
        throw "Timed out waiting for private post-provision container '$containerName' to complete."
    }

    $exitCode = az container show --resource-group $resourceGroupName --name $containerName --query 'containers[0].instanceView.currentState.exitCode' -o tsv
    if ($LASTEXITCODE -ne 0 -or $exitCode -ne '0') {
        throw "Private post-provision container failed with exit code '$exitCode'."
    }
}
finally {
    $mySqlBootstrapToken = $null
    $pgBootstrapToken = $null
    $databaseBootstrapToken = $null
    $secureEnvironmentValues = $null
    $createArguments = $null
    if ($sqlServersWithBootstrapAdmin.Count -gt 0) {
        foreach ($server in $sqlServersWithBootstrapAdmin) {
            $originalAdmin = $sqlOriginalAdministrators[$server]
            Write-Host "Restoring captured SQL Entra administrator '$($originalAdmin.login)' on '$server'..." -ForegroundColor DarkGreen
            $restored = $false
            for ($attempt = 1; $attempt -le 5 -and -not $restored; $attempt++) {
                & az sql server ad-admin update `
                    --resource-group $resourceGroupName `
                    --server-name $server `
                    --display-name $originalAdmin.login `
                    --object-id $originalAdmin.sid `
                    --output none
                if ($LASTEXITCODE -eq 0) {
                    $restored = $true
                }
                elseif ($attempt -lt 5) {
                    Write-Host "SQL administrator restore attempt $attempt failed; retrying..." -ForegroundColor Yellow
                    Start-Sleep -Seconds 10
                }
            }
            if (-not $restored) {
                $cleanupErrors.Add("Unable to restore the SQL Entra administrator on '$server'.")
            }
        }
    }

    if ($containerCreationAttempted) {
        & az container delete --resource-group $resourceGroupName --name $containerName --yes --output none
        if ($LASTEXITCODE -ne 0) {
            $cleanupErrors.Add("Unable to delete bootstrap container '$containerName'.")
        }
    }
    if (Test-Path $tempBuildContext) {
        Remove-Item -Path $tempBuildContext -Recurse -Force
    }
    if ($cleanupErrors.Count -gt 0) {
        throw "Bootstrap cleanup failed: $($cleanupErrors -join ' ')"
    }
}
