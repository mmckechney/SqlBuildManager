param
(
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix
)

if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

$acrName = az acr list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Azure Container Registry Name:'$acrName'" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_aks_job_yaml.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -prefix $prefix -acrName $acrName