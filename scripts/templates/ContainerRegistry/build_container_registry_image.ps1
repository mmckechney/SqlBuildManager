
param
(
    [string] $azureContainerRegistry
)


Write-Host "Logging into Azure Container Registry" -ForegroundColor DarkGreen
$tmp = az acr login -n $azureContainerRegistry --expose-token

$dateTag = Get-Date -Format "yyyy-MM-dd"
$dateTag =  "sqlbuildmanager:$dateTag"
$vnextTag =  "sqlbuildmanager:latest-vNext"
Write-Host "Uploading and building Container image on registry $azureContainerRegistry." -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
$dockerFile = Resolve-Path (Join-Path $scriptDir ..\..\..\src\Dockerfile)
$sourcePath = Resolve-Path (Join-Path $scriptDir ..\..\..\src)
Write-Host "Using DockerFile from $sourcePath" -ForegroundColor DarkGreen


$assemblyVersionFile = Resolve-Path (Join-Path $scriptDir ..\..\..\src\AssemblyVersioning.cs)
$success = ((Get-Content $assemblyVersionFile)  -match '\d{1,3}\.\d{1,3}\.\d{1,3}')[0] -match '\d{1,3}\.\d{1,3}\.\d{1,3}' 
if ($success)
{
    $ver = $Matches.0
    Write-Host "Building with image tags: $ver | '$dateTag' | '$vnextTag' (used in integration tests)" -ForegroundColor DarkGreen
    az acr build --image $dateTag --image $vnextTag --image $ver --registry $azureContainerRegistry --file "$dockerFile" "$sourcePath" --no-logs --query outputimages
}
else 
{
    Write-Host "Unable to read AssemblyVersion.cs file. Can not create a version tag" -ForegroundColor Yellow
    Write-Host "Building with image tags: '$dateTag' | '$vnextTag' (used in integrat ion tests)" -ForegroundColor DarkGreen
    az acr build --image $dateTag --image $vnextTag --registry $azureContainerRegistry --file "$dockerFile" "$sourcePath" --no-logs --query outputimages
}

