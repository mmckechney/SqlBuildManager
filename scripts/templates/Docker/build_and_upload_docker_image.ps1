
param
(
    [string] $azureContainerRegistry
)


Write-Host "Logging into Azure Container Registry" -ForegroundColor DarkGreen
az acr login --name $azureContainerRegistry -o table

$dateTag = Get-Date -Format "yyyy-MM-dd"
$tag =  "sqlbuildmanager:$dateTag"
Write-Host "Building Container on registry $containerRegistry  with tag: '$tag'" -ForegroundColor DarkGreen
az acr build --image $tag --registry $containerRegistry ..\..\..\src --no-logs