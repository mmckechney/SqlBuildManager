
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
Copy-Item sample_job.yaml (Join-Path $outputPath basic_job.yaml)
Copy-Item sample_job_keyvault.yaml (Join-Path $outputPath basic_job_keyvault.yaml)

if([string]::IsNullOrWhiteSpace($acrName) -eq $false)
{
    $acrLoginServer = az acr show --resource-group $resourceGroupName --name $acrName -o tsv --query loginServer
    Write-Host "Using ACR login server name:'$acrLoginServer'" -ForegroundColor DarkGreen
    
    ((Get-Content -Path sample_job.yaml) -replace "blueskydevus", $acrLoginServer) | Out-File (Join-Path $outputPath acr_basic_job.yaml)
    ((Get-Content -Path sample_job_keyvault.yaml) -replace "blueskydevus", $acrLoginServer) | Out-File (Join-Path $outputPath acr_basic_job_keyvault.yaml)
}