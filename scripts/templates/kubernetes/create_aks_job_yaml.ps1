
param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $acrName
)

$outputPath = Resolve-Path $path

Write-Host "Saving AKS Job YAML files to :'$outputPath'" -ForegroundColor DarkGreen

#############################################
# Copy sample K8s YAML files for test configs
#############################################
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
Copy-Item (Join-Path $scriptDir sample_job.yaml) (Join-Path $outputPath k8s-basic_job.yaml)
Copy-Item (Join-Path $scriptDir sample_job_keyvault.yaml) (Join-Path $outputPath k8s-basic_job_keyvault.yaml)

if([string]::IsNullOrWhiteSpace($acrName) -eq $false)
{
    $acrLoginServer = az acr show --resource-group $resourceGroupName --name $acrName -o tsv --query loginServer
    Write-Host "Using ACR login server name:'$acrLoginServer'" -ForegroundColor DarkGreen
    
    ((Get-Content -Path (Join-Path $scriptDir sample_job.yaml)) -replace "blueskydevus", $acrLoginServer) | Out-File (Join-Path $outputPath k8s-acr_basic_job.yaml)
    ((Get-Content -Path (Join-Path $scriptDir sample_job_keyvault.yaml)) -replace "blueskydevus", $acrLoginServer) | Out-File (Join-Path $outputPath k8s-acr_basic_job_keyvault.yaml)
}