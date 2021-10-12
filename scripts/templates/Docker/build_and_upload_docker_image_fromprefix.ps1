param
(
    [string] $prefix,
    [string] $resourceGroupName
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Build and Upload Docker Container from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$azureContainerRegistry = az acr list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Azure Container Registry Name: $azureContainerRegistry  " -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/build_and_upload_docker_image.ps1 -azureContainerRegistry $azureContainerRegistry
