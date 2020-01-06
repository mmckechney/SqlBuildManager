<#
 .SYNOPSIS
    Deploys an Azure Batch and Storage account to a resource group, then installs and configures the Azure Batch application

 .DESCRIPTION
    Deploys an Azure Resource Manager template

 .PARAMETER subscriptionId
    The subscription id where the template will be deployed.

 .PARAMETER resourceGroupName
    The resource group where the template will be deployed. Can be the name of an existing or a new resource group.

 .PARAMETER resourceGroupLocation
    Optional, a resource group location. If specified, will try to create a new resource group in this location. If not specified, assumes resource group is existing.

 .PARAMETER deploymentName
    The deployment name.

 .PARAMETER parametersFilePath
    Optional, path to the parameters file. Defaults to parameters.json. If file is not found, will prompt for parameter values based on template.

 .PARAMETER sbmReleaseUrl
    Optional, GitHub URL to the SQL Build Manager release zip file

 .PARAMETER version
    Optional, Code version number for the Azure Batch application

 .PARAMETER applicationId
    Optional, Name of the Azure Batch application
#>

param(
 [Parameter(Mandatory=$True)]
 [string]
 $subscriptionId,

 [Parameter(Mandatory=$True)]
 [string]
 $resourceGroupName,

 [string]
 $resourceGroupLocation,

 [string]
 $deploymentName = "batchaccount",

 [string]
 $parametersFilePath = "azuredeploy.parameters.json",

  [string]
 $sbmReleaseUrl = "https://github.com/mmckechney/SqlBuildManager/releases/download/v11.0.0/SqlBuildManager-v11.0.0.zip",

  [string]
 $version= "11.0.0",

 [string]
 $applicationId  = "SqlBuldManager"


)

<#
.SYNOPSIS
    Registers RPs
#>
Function RegisterRP {
    Param(
        [string]$ResourceProviderNamespace
    )

    Write-Host "Registering resource provider '$ResourceProviderNamespace'";
    Register-AzResourceProvider -ProviderNamespace $ResourceProviderNamespace;
}

#******************************************************************************
# Script body
# Execution begins here
#******************************************************************************
$ErrorActionPreference = "Stop"

# sign in
Write-Host "Logging in...";
#Login-AzAccount;

# select subscription
Write-Host "Selecting subscription '$subscriptionId'";
Select-AzSubscription -SubscriptionID $subscriptionId;

# Register RPs
$resourceProviders = @("microsoft.storage","microsoft.batch","microsoft.eventhub");
if($resourceProviders.length) {
    Write-Host "Registering resource providers"
    foreach($resourceProvider in $resourceProviders) {
        RegisterRP($resourceProvider);
    }
}

#Create or check for existing resource group
$resourceGroup = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
if(!$resourceGroup)
{
    Write-Host "Resource group '$resourceGroupName' does not exist. To create a new resource group, please enter a location.";
    if(!$resourceGroupLocation) {
        $resourceGroupLocation = Read-Host "resourceGroupLocation";
    }
    Write-Host "Creating resource group '$resourceGroupName' in location '$resourceGroupLocation'";
    New-AzResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
}
else{
    Write-Host "Using existing resource group '$resourceGroupName'";
}

$templateUri = "https://raw.githubusercontent.com/mmckechney/SqlBuildManager/master/docs/templates/azuredeploy.json"

# Start the deployment
Write-Host "Starting deployment...";
if(Test-Path $parametersFilePath) {
    New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -Name $deploymentName -TemplateUri $templateUri -TemplateParameterFile $parametersFilePath; 
} else {
    New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -Name $deploymentName -TemplateUri $templateUri;
}

# Create application
$json = (Get-Content "$parametersFilePath" -Raw) | ConvertFrom-Json

$batchAcctName = $json.parameters.batchAccountName.value
$storageAcctName = $json.parameters.storageAccountName.value

#Create the application
Write-Host "Creating new Azure Batch Application named $applicationId"
New-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationId

#download zip package locally
Write-Host "Downloading release binaries from $sbmReleaseUrl"
$tmp = $sbmReleaseUrl.Split('/');
$localFileName = $tmp[$tmp.Count-1];
$output = "$PSScriptRoot\$localFileName"

$wc = New-Object System.Net.WebClient
$wc.DownloadFile($sbmReleaseUrl, $output)

#Add the package 
Write-Host "Uploading application package to Azure Batch account"
New-AzBatchApplicationPackage -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationId -ApplicationVersion $version -Format zip -FilePath $output

Write-Host "Setting default application version to $version"
Set-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationId -DefaultVersion $version


$batch = Get-AzBatchAccountKey -AccountName $batchAcctName -ResourceGroupName $resourceGroupName
$t = Get-AzBatchAccount -AccountName $batchAcctName -ResourceGroupName $resourceGroupName


$storage = Get-AzStorageAccount -Name $storageAcctName -ResourceGroupName $resourceGroupName
$s = Get-AzStorageAccountKey -Name $storageAcctName -ResourceGroupName $resourceGroupName

$e = Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $namespaceName -EventHubName $eventHubName -AuthorizationRuleName batchbuilder


Write-Host "Pre-populated command line arguments. Record these for use later: "
Write-Host ""
Write-Host "/BatchAccountName=$batchAcctName"
Write-Host "/BatchAccountUrl=https://$($t.AccountEndpoint)"
Write-Host "/BatchAccountKey=$($batch.PrimaryAccountKey)"
Write-Host "/StorageAccountName=$storageAcctName"
Write-Host "/StorageAccountKey=$($s.Value[0])"
Write-Host "/EventHubConnectionString=$($e.PrimaryConnectionString)"