<#
 .SYNOPSIS
    Deploys an Azure Batch and Storage account to a resource group, then installs and configures the Azure Batch application

 .DESCRIPTION
    Deploys an Azure Resource Manager template

 .PARAMETER resourceGroupName
    The resource group where the template will be deployed. Can be the name of an existing or a new resource group.

#>

param(

 [Parameter(Mandatory=$True)]
 [string]
 $resourceGroupName,

 [string]
 $envName,

 [string]
 $windowsZipPackage,

 [string]
 $linuxZipPackage,

 [string]
 $releaseVersion,

 [string]
 $resourceTypesPath,

 [string]
 $batchAccountName

)

$batchAcctName = $batchAccountName
if ([string]::IsNullOrWhiteSpace($batchAcctName) -and -not [string]::IsNullOrWhiteSpace($env:ARMOUTPUTS)) {
    $deploymentOutputs = $env:ARMOUTPUTS | ConvertFrom-Json
    $batchAcctName = $deploymentOutputs.BATCH_ACCOUNT_NAME.value
}
if ([string]::IsNullOrWhiteSpace($batchAcctName)) {
    if ([string]::IsNullOrWhiteSpace($resourceTypesPath)) {
        $resourceTypesPath = Join-Path (Split-Path $PSScriptRoot -Parent) "infra\resourcetypes.json"
    }
    if (-not (Test-Path $resourceTypesPath -PathType Leaf)) {
        throw "Neither BATCH_ACCOUNT_NAME deployment output nor Azure resource type prefix map '$resourceTypesPath' was available."
    }

    $resourceTypePrefixes = Get-Content $resourceTypesPath -Raw | ConvertFrom-Json
    $normalizedEnvName = $envName.Replace("-", "").ToLowerInvariant()
    $batchPrefix = $resourceTypePrefixes.batchAccounts -replace '[^a-zA-Z0-9]', ''
    $batchAcctName = "$batchPrefix$normalizedEnvName"
}

##########################################
# Set up variables to be used
##########################################
$winenv =@{
    ApplicationName = "SqlBuildManagerWindows"
    PoolName = "SqlBuildManagerPoolWindows"
    OSName = "Windows"
    BuildOutputZip = $windowsZipPackage

}

$linuxenv = @{
    ApplicationName = "SqlBuildManagerLinux"
    PoolName = "SqlBuildManagerPoolLinux"
    OSName = "Linux"
    BuildOutputZip = $linuxZipPackage
}
$vars = $winenv, $linuxenv 

$ErrorActionPreference = "Stop"


##################################################
# Upload zip application packages to batch account
##################################################
foreach ($env in $vars)
{

    Write-Host "Creating new Azure Batch Application named $($env.ApplicationName)"
    New-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName
    
    Write-Host "Uploading application package $($env.ApplicationName) [$($env.BuildOutputZip)] to Azure Batch account"
    New-AzBatchApplicationPackage -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName -ApplicationVersion $releaseVersion -Format zip -FilePath $env.BuildOutputZip
    
    Write-Host "Setting default application for  $($env.ApplicationName) version to $releaseVersion"
    Set-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName -DefaultVersion $releaseVersion
}
