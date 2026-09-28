param(
    [Parameter(Mandatory=$true)][string] $envName,
    [Parameter(Mandatory=$true)][string] $resourceGroupName,
    [Parameter(Mandatory=$true)][guid] $userObjectId,
    [Parameter(Mandatory=$true)][string] $userLogin
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$resourceGroupOverride = $resourceGroupName
. (Join-Path (Split-Path $PSScriptRoot -Parent) 'prefix_resource_names.ps1') -envName $envName
$resourceGroupName = $resourceGroupOverride

if ($userObjectId -eq [guid]::Empty -or [string]::IsNullOrWhiteSpace($userLogin)) {
    throw 'A deploying Entra user is required to configure the MySQL administrator.'
}

# Graph roles must already be assigned to the server identity before enabling Entra authentication.
foreach ($serverName in @($mySqlServerNameA, $mySqlServerNameB)) {
    az mysql flexible-server ad-admin create --resource-group $resourceGroupName --server-name $serverName `
        --display-name $userLogin --object-id "$userObjectId" --identity $mysqlDirectoryIdentityName --output none
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to configure the deploying user as Entra administrator on '$serverName'. Check server identity Graph permissions and retry postprovision after propagation."
    }
}
