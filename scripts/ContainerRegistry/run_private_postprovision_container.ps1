param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [Parameter(Mandatory=$true)]
    [string] $repoRoot,

    [bool] $deploySqlServer,

    [bool] $deployPostgreSQL
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

$containerName = "${prefix}postprovision"
$imageName = 'sqlbuildmanager-postprovision:latest'
$image = "${containerRegistryLoginServer}/${imageName}"
$tempBuildContext = Join-Path ([IO.Path]::GetTempPath()) "sbm-postprovision-$(Get-Random)"
$sqlServers = @()
$sqlServersWithBootstrapAdmin = @()
$deployerPrincipalId = if ($deploySqlServer) { Get-AzdValue 'AZURE_PRINCIPAL_ID' } else { $null }
$deployerPrincipalName = if ($deploySqlServer) { Get-AzdValue 'AZURE_PRINCIPAL_NAME' } else { $null }

try {
    New-Item -Path (Join-Path $tempBuildContext 'scripts/Database') -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $repoRoot 'infra/postprovision/Dockerfile') (Join-Path $tempBuildContext 'Dockerfile')
    Copy-Item (Join-Path $repoRoot 'infra/postprovision/run-private-postprovision.ps1') (Join-Path $tempBuildContext 'run-private-postprovision.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/prefix_resource_names.ps1') (Join-Path $tempBuildContext 'scripts/prefix_resource_names.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/Database/grant_identity_permissions.ps1') (Join-Path $tempBuildContext 'scripts/Database/grant_identity_permissions.ps1')
    Copy-Item (Join-Path $repoRoot 'scripts/Database/grant_pg_identity_permissions.ps1') (Join-Path $tempBuildContext 'scripts/Database/grant_pg_identity_permissions.ps1')

    Write-Host "Building private post-provision image remotely in ACR..." -ForegroundColor Cyan
    Invoke-Az -Arguments @(
        'acr', 'build',
        '--registry', $containerRegistryName,
        '--resource-group', $resourceGroupName,
        '--image', $imageName,
        '--file', 'Dockerfile',
        $tempBuildContext
    ) -FailureMessage 'The ACR build for the private post-provision image failed.'

    if ($deploySqlServer) {
        $sqlServers = @(az sql server list --resource-group $resourceGroupName --query '[].name' -o tsv)
        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to enumerate SQL servers before assigning the bootstrap administrator.'
        }

        foreach ($server in $sqlServers) {
            Write-Host "Temporarily assigning '$postProvisionIdentityName' as SQL Entra administrator on '$server'..." -ForegroundColor DarkGreen
            Invoke-Az -Arguments @(
                'sql', 'server', 'ad-admin', 'update',
                '--resource-group', $resourceGroupName,
                '--server-name', $server,
                '--display-name', $postProvisionIdentityName,
                '--object-id', $postProvisionPrincipalId,
                '--output', 'none'
            ) -FailureMessage "Unable to assign the bootstrap SQL administrator on '$server'."
            $sqlServersWithBootstrapAdmin += $server
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
        '--restart-policy', 'Never',
        '--cpu', '1',
        '--memory', '1.5',
        '--environment-variables',
        "PREFIX=$prefix",
        "RESOURCE_GROUP_NAME=$resourceGroupName",
        "SUBSCRIPTION_ID=$subscriptionId",
        "POSTPROVISION_CLIENT_ID=$postProvisionClientId",
        "POSTPROVISION_IDENTITY_NAME=$postProvisionIdentityName",
        "TARGET_IDENTITY_NAME=$targetIdentityName",
        "DEPLOY_SQLSERVER=$($deploySqlServer.ToString().ToLowerInvariant())",
        "DEPLOY_POSTGRESQL=$($deployPostgreSQL.ToString().ToLowerInvariant())",
        '--output', 'none'
    )

    $created = $false
    for ($attempt = 1; $attempt -le 5 -and -not $created; $attempt++) {
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

    $deadline = [DateTime]::UtcNow.AddMinutes(30)
    do {
        Start-Sleep -Seconds 10
        $provisioningState = az container show --resource-group $resourceGroupName --name $containerName --query provisioningState -o tsv
        $containerState = az container show --resource-group $resourceGroupName --name $containerName --query 'containers[0].instanceView.currentState.state' -o tsv

        if ($provisioningState -eq 'Failed') {
            throw "Container group '$containerName' failed to provision."
        }
        if ($containerState -eq 'Terminated') {
            break
        }
    } while ([DateTime]::UtcNow -lt $deadline)

    if ($containerState -ne 'Terminated') {
        throw "Timed out waiting for private post-provision container '$containerName'."
    }

    az container logs --resource-group $resourceGroupName --name $containerName
    $exitCode = az container show --resource-group $resourceGroupName --name $containerName --query 'containers[0].instanceView.currentState.exitCode' -o tsv
    if ($LASTEXITCODE -ne 0 -or $exitCode -ne '0') {
        throw "Private post-provision container failed with exit code '$exitCode'."
    }
}
finally {
    if ($sqlServersWithBootstrapAdmin.Count -gt 0) {
        foreach ($server in $sqlServersWithBootstrapAdmin) {
            Write-Host "Restoring '$deployerPrincipalName' as SQL Entra administrator on '$server'..." -ForegroundColor DarkGreen
            $restored = $false
            for ($attempt = 1; $attempt -le 5 -and -not $restored; $attempt++) {
                & az sql server ad-admin update `
                    --resource-group $resourceGroupName `
                    --server-name $server `
                    --display-name $deployerPrincipalName `
                    --object-id $deployerPrincipalId `
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
                Write-Error "Unable to restore the SQL Entra administrator on '$server'."
            }
        }
    }

    if (Test-Path $tempBuildContext) {
        Remove-Item -Path $tempBuildContext -Recurse -Force
    }
}
