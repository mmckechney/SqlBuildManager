
param
(
    [string] $azureContainerRegistry
)


Write-Host "Logging into Azure Container Registry" -ForegroundColor DarkGreen
$tmp = az acr login -n $azureContainerRegistry --expose-token

$dateTag = Get-Date -Format "yyyy-MM-dd"
$dateTag =  "sqlbuildmanager:$dateTag"
$vnextTag =  "sqlbuildmanager:latest-vNext"
Write-Host "Uploading and building Container image on registry $azureContainerRegistry  with tag: '$dateTag' and '$vnextTag' (used in integration tests)" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
$dockerFile = Resolve-Path (Join-Path $scriptDir ..\..\..\src\Dockerfile)
$sourcePath = Resolve-Path (Join-Path $scriptDir ..\..\..\src)
Write-Host "Using DockerFile from $sourcePath" -ForegroundColor DarkGreen

az acr build --image $dateTag --image $vnextTag --registry $azureContainerRegistry --file "$dockerFile" "$sourcePath" --no-logs --query outputimages
