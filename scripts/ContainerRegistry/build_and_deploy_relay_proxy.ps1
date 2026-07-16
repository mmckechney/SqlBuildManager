param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [Parameter(Mandatory=$true)]
    [string] $repoRoot
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
$storageAccountName = Get-AzdValue 'STORAGE_ACCOUNT_NAME'
$eventHubNamespaceName = Get-AzdValue 'EVENTHUB_NAMESPACE_NAME'
$eventHubName = Get-AzdValue 'EVENTHUB_NAME'
$aciSubnetId = Get-AzdValue 'ACI_SUBNET_ID'
$runtimeIdentityId = Get-AzdValue 'MANAGED_IDENTITY_ID'
$runtimeIdentityClientId = Get-AzdValue 'MANAGED_IDENTITY_CLIENT_ID'
$senderPrincipalId = Get-AzdValue 'AZURE_PRINCIPAL_ID'
$relayNamespaceName = "${prefix}relay"
$hybridConnectionName = 'relayproxy'
$identityName = "${prefix}relayproxy"
$containerName = "${prefix}relayproxy"
$imageName = 'sqlbuildmanager-relayproxy:latest'
$image = "${containerRegistryLoginServer}/${imageName}"
$sourceContext = Join-Path $repoRoot 'src'
$dockerfile = 'SqlBuildManager.RelayProxy/Dockerfile'
$endpoint = "https://${relayNamespaceName}.servicebus.windows.net/${hybridConnectionName}"
$relayNamespaceId = az relay namespace show `
    --resource-group $resourceGroupName `
    --name $relayNamespaceName `
    --query id `
    --output tsv
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($relayNamespaceId)) {
    throw "Unable to read Relay namespace '$relayNamespaceName'."
}

$existingSenderAssignment = az role assignment list `
    --scope $relayNamespaceId `
    --assignee $senderPrincipalId `
    --role 'Azure Relay Sender' `
    --query '[0].id' `
    --output tsv
if ($LASTEXITCODE -ne 0) {
    throw "Unable to inspect Azure Relay Sender assignments for '$senderPrincipalId'."
}
if ([string]::IsNullOrWhiteSpace($existingSenderAssignment)) {
    Invoke-Az -Arguments @(
        'role', 'assignment', 'create',
        '--assignee-object-id', $senderPrincipalId,
        '--assignee-principal-type', 'User',
        '--role', 'Azure Relay Sender',
        '--scope', $relayNamespaceId,
        '--output', 'none'
    ) -FailureMessage "Unable to grant Azure Relay Sender to '$senderPrincipalId'."
}

$identity = az identity show `
    --resource-group $resourceGroupName `
    --name $identityName `
    --output json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $null -eq $identity) {
    throw "Unable to read Relay proxy identity '$identityName'."
}

$sqlServerFqdns = @(az sql server list `
    --resource-group $resourceGroupName `
    --query '[].fullyQualifiedDomainName' `
    --output tsv)
if ($LASTEXITCODE -ne 0) {
    throw "Unable to enumerate SQL servers in resource group '$resourceGroupName'."
}
$allowedSqlServers = ($sqlServerFqdns |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object { $_.Trim() }) -join ','

Write-Host "Building Relay proxy image remotely in ACR..." -ForegroundColor Cyan
Push-Location $sourceContext
try {
    Invoke-Az -Arguments @(
        'acr', 'build',
        '--registry', $containerRegistryName,
        '--resource-group', $resourceGroupName,
        '--image', $imageName,
        '--file', $dockerfile,
        '--no-logs',
        '--output', 'none',
        '.'
    ) -FailureMessage 'The ACR build for the Relay proxy failed.'
}
finally {
    Pop-Location
}

$null = az container show --resource-group $resourceGroupName --name $containerName --output none 2>$null
if ($LASTEXITCODE -eq 0) {
    Invoke-Az -Arguments @(
        'container', 'delete',
        '--resource-group', $resourceGroupName,
        '--name', $containerName,
        '--yes',
        '--output', 'none'
    ) -FailureMessage "Unable to replace existing Relay proxy container '$containerName'."
}

$createArguments = @(
    'container', 'create',
    '--resource-group', $resourceGroupName,
    '--name', $containerName,
    '--image', $image,
    '--registry-login-server', $containerRegistryLoginServer,
    '--acr-identity', $identity.id,
    '--assign-identity', $identity.id, $runtimeIdentityId,
    '--subnet', $aciSubnetId,
    '--os-type', 'Linux',
    '--restart-policy', 'Always',
    '--cpu', '1',
    '--memory', '1.5',
    '--environment-variables',
    "RELAY_NAMESPACE=$($relayNamespaceName).servicebus.windows.net",
    "RELAY_CONNECTION_NAME=$hybridConnectionName",
    "STORAGE_ACCOUNT_NAME=$storageAccountName",
    "EVENTHUB_NAMESPACE_NAME=$eventHubNamespaceName",
    "EVENTHUB_NAME=$eventHubName",
    "MANAGED_IDENTITY_CLIENT_ID=$($identity.clientId)",
    "SQL_MANAGED_IDENTITY_CLIENT_ID=$runtimeIdentityClientId",
    "SQL_SERVER_FQDNS=$allowedSqlServers",
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
        Write-Host "Relay proxy creation attempt $attempt failed while role assignments propagate; retrying..." -ForegroundColor Yellow
        Start-Sleep -Seconds 20
    }
}
if (-not $created) {
    throw "Unable to create Relay proxy container '$containerName'."
}

$deadline = [DateTime]::UtcNow.AddMinutes(10)
do {
    Start-Sleep -Seconds 10
    $provisioningState = az container show --resource-group $resourceGroupName --name $containerName --query provisioningState -o tsv
    $containerState = az container show --resource-group $resourceGroupName --name $containerName --query 'containers[0].instanceView.currentState.state' -o tsv

    if ($provisioningState -eq 'Failed' -or $containerState -eq 'Terminated') {
        az container logs --resource-group $resourceGroupName --name $containerName
        throw "Relay proxy container '$containerName' failed to start."
    }
    if ($provisioningState -eq 'Succeeded' -and $containerState -eq 'Running') {
        break
    }
} while ([DateTime]::UtcNow -lt $deadline)

if ($containerState -ne 'Running') {
    throw "Timed out waiting for Relay proxy container '$containerName'."
}

Write-Host "Relay proxy is running at $endpoint" -ForegroundColor Green
