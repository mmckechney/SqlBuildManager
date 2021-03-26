<#
.SYNOPSIS
Deploys Azure Batch, Storage Account, and EventHub to a resource group, then compiles, installs and configures the Azure Batch application

.DESCRIPTION
Deploys an Azure Resource Manager template

.PARAMETER subscriptionId
The subscription id where the template will be deployed.

.PARAMETER resourceGroupName
The resource group where the template will be deployed. Can be the name of an existing or a new resource group.

.PARAMETER resourceGroupLocation
Optional, a resource group location. If specified, will try to create a new resource group in this location. If not specified, assumes resource group is existing.

.PARAMETER batchprefix
Up to 6 character prefix that will be used to name the various resources

.PARAMETER outputpath
The path where the application package zip files and settings JSON files will be saved 

.PARAMETER templateFile
Name of the ARM template that will deploy the resources. 

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

 [Parameter(Mandatory=$True)]
 [string]
 $batchprefix,

 [Parameter(Mandatory=$True)]
 [string]
 $outputpath,

 [string]
 $templateFile = "azuredeploy.json"

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
    Register-AzResourceProvider -ProviderNamespace $ResourceProviderNamespace -ErrorAction SilentlyContinue
}

$outputpath = Resolve-Path  $outputpath

##########################################
# Set up variables to be used
##########################################
$winenv =@{
    ApplicationName = "SqlBuildManagerWindows"
    PoolName = "SqlBuildManagerPoolWindows"
    OSName = "Windows"
    BuildTarget = "win-x64"
    BuildOutputZip = ""

}

$linuxenv = @{
    ApplicationName = "SqlBuildManagerLinux"
    PoolName = "SqlBuildManagerPoolLinux"
    OSName = "Linux"
    BuildTarget = "linux-x64"
    BuildOutputZip = ""
}
$vars = $winenv, $linuxenv 


$ErrorActionPreference = "Stop"

#########################################################
# ARM template deployment for batch, storage and eventhub
#########################################################
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


$params = @{}
$params.Add("namePrefix", $batchprefix);
$params.Add("eventhubSku", "Standard");
$params.Add("skuCapacity", 1);
$params.Add("location", $resourceGroupLocation)


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

# Start the deployment
Write-Host "Starting deployment...";
Write-Host "Creating batch, storage and eventhub accounts...";
New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName  -TemplateFile $templateFile -TemplateParameterObject $params -Verbose


##########################################
# Build the solution
##########################################
# dotnet restore "..\..\src\sqlsync.sln" 
# dotnet build "..\..\src\sqlsync.sln" --configuration Debug


##########################################
# Create the application package zip files
##########################################
$frameworkTarget = "net5.0"
foreach ($env in $vars) {


    Write-Output "Publishing for $($env.OSName)"
    dotnet publish  "..\..\src\SqlBuildManager.Console\sbm.csproj" -r $env.BuildTarget --configuration Debug -f $frameworkTarget
    $source= Resolve-Path "..\..\src\SqlBuildManager.Console\bin\Debug\$frameworkTarget\$($env.BuildTarget)\publish"
    if($env.OSName -eq "Windows")
    {
        $version = (Get-Item "$($source)\sbm.exe").VersionInfo.ProductVersion  #Get version for Batch application
    }
    $buildOutput= Join-Path $outputpath "sbm-$($env.OSName.ToLower())-$($version).zip"
    Add-Type -AssemblyName "system.io.compression.filesystem"
    If(Test-path $buildOutput) 
    {
        Remove-item $buildOutput
    }
    Write-Output "Creating Zip file for $($env.OSName) Release package"
    [io.compression.zipfile]::CreateFromDirectory($source,$buildOutput)
    $env.BuildOutputZip = Resolve-Path $buildOutput
}

##################################################
# Upload zip application packages to batch account
##################################################

$batchAcctName = $batchprefix + "batchacct"
foreach ($env in $vars)
{

    Write-Host "Creating new Azure Batch Application named $($env.ApplicationName)"
    New-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName
    
    Write-Host "Uploading application package $($env.ApplicationName) [$($env.BuildOutputZip)] to Azure Batch account"
    New-AzBatchApplicationPackage -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName -ApplicationVersion $version -Format zip -FilePath $env.BuildOutputZip
    
    Write-Host "Setting default application for  $($env.ApplicationName) version to $version"
    Set-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $env.ApplicationName -DefaultVersion $version
}


##################################################
# Save settings files for environments
##################################################

foreach ($env in $vars)
{
    $file = Join-Path $outputpath "settingsfile-$($env.OSName.ToLower()).json"
    .\Create_SettingsFile.ps1 -subscriptionId $subscriptionId -resourceGroupName $resourceGroupName -batchprefix $batchprefix -batchPoolName $env.PoolName -batchApplicationPackage $env.ApplicationName -batchPoolOs $env.OSName -settingsFileName $file
}

