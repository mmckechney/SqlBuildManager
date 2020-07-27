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
 $batchprefix,

 [string]
 $windowsZipPackage,

 [string]
 $linuxZipPackage

)

$batchAcctName = $batchprefix + "batchacct"

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
    New-AzBatchApplicationPackage -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName -ApplicationVersion $version -Format zip -FilePath $env.BuildOutputZip
    
    Write-Host "Setting default application for  $($env.ApplicationName) version to $version"
    Set-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName -DefaultVersion $version
}
