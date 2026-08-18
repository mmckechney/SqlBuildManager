<#
.SYNOPSIS
    Deletes non-system storage containers from the test storage account.
.DESCRIPTION
    Lists all blob containers in the environment storage account and deletes those that
    do not start with "app-" or "eventhubcheckpoint". These are test-generated
    containers that can be safely removed after integration tests.
.PARAMETER envName
    Azure Developer CLI environment name used to derive the storage account name.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to rg-{envName}.

.NOTES
    Uses the Microsoft.Storage management plane rather than the Blob data plane,
    so it works when the storage account has public network access disabled.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName
)

$resourceGroupNameOverride = $resourceGroupName
. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}

Write-Host "Deleting storage containers from $storageAccountName" -ForegroundColor Green

$containers = az storage container-rm list `
    --resource-group $resourceGroupName `
    --storage-account $storageAccountName `
    --query "[?deleted != ``true``].name" `
    --output tsv `
    --only-show-errors
if ($LASTEXITCODE -ne 0) {
    throw "Unable to list containers in storage account '$storageAccountName' through the Azure management plane."
}

$containersToDelete = @($containers | Where-Object {
    -not $_.StartsWith("app-") -and -not $_.StartsWith("eventhubcheckpoint")
})
$containersToPreserve = @($containers | Where-Object {
    $_.StartsWith("app-") -or $_.StartsWith("eventhubcheckpoint")
})

foreach ($container in $containersToPreserve) {
    Write-Host "Preserving storage container: $container" -ForegroundColor Yellow
}

$numberWidth = [Math]::Max(3, $containersToDelete.Count.ToString().Length)
for ($index = 0; $index -lt $containersToDelete.Count; $index++) {
    $container = $containersToDelete[$index]
    $currentNumber = ($index + 1).ToString("D$numberWidth")
    $totalNumber = $containersToDelete.Count.ToString("D$numberWidth")
    Write-Host "Deleting storage container $currentNumber of $totalNumber`: $container" -ForegroundColor Green
        az storage container-rm delete `
            --resource-group $resourceGroupName `
            --storage-account $storageAccountName `
            --name $container `
            --yes `
            --output none `
            --only-show-errors
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to delete storage container '$container'."
        }
}
Write-Host "Complete!"
