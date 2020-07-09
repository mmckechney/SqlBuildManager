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
 $batchprefix,

 [string]
 $deploymentName = "batchdeploy",

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


#******************************************************************************
# Script body
# Execution begins here
#******************************************************************************

$applicationIdWindows  = "SqlBuildManager"
$applicationIdLinux  = "SqlBuildManagerLinux"

#Build the solution, publish and create zip file for uploading to Azure Batch
dotnet restore "..\..\src\sqlsync.sln" 
dotnet build "..\..\src\sqlsync.sln" --configuration Debug

#Zip release for Windows 
dotnet publish  "..\..\src\SqlBuildManager.Console\sbm.csproj" -r win-x64 --configuration Debug
$source= Resolve-Path "..\..\src\SqlBuildManager.Console\bin\Debug\netcoreapp3.1\win-x64\publish"
$buildOutputZipWindows = "$(Resolve-Path "..\..\src\TestConfig")\sbm-$($version)..zip"
Add-Type -AssemblyName "system.io.compression.filesystem"
If(Test-path $buildOutputZipWindows) {Remove-item $buildOutputZipWindows}
[io.compression.zipfile]::CreateFromDirectory($source,$buildOutputZipWindows)

#Get version for Batch application
$version = (Get-Item "$($source)\sbm.exe").VersionInfo.ProductVersion

#Zip release for Linux 
dotnet publish  "..\..\src\SqlBuildManager.Console\sbm.csproj" -r linux-x64 --configuration Debug
$source= Resolve-Path "..\..\src\SqlBuildManager.Console\bin\Debug\netcoreapp3.1\linux-x64\publish"
$buildOutputZipLinux = "$(Resolve-Path "..\..\src\TestConfig")\sbm-linux-$($version).zip"
Add-Type -AssemblyName "system.io.compression.filesystem"
If(Test-path $buildOutputZipLinux) {Remove-item $buildOutputZipLinux}
[io.compression.zipfile]::CreateFromDirectory($source,$buildOutputZipLinux)


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

$batchAcctName = $batchprefix + "batchacct"
$storageAcctName = $batchprefix + "storage"
$namespaceName = $batchprefix + "eventhubnamespace"
$eventHubName =  $batchprefix + "eventhub"

$params = @{}
$params.Add("batchAccountName", $batchAcctName);
$params.Add("storageAccountName", $storageAcctName);
$params.Add("namespaceName", $batchprefix + "eventhubnamespace");
$params.Add("eventhubSku", "Standard");
$params.Add("eventHubName", $eventHubName );

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
New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -Name $deploymentName -TemplateFile $templateFile -TemplateParameterObject $params
    #New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -Name $deploymentName -TemplateFile $templateFile;


####
# Create the Windows application
####
Write-Host "Creating new Azure Batch Application named $applicationIdWindows"
New-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationIdWindows

#Add the package 
Write-Host "Uploading application package to Azure Batch account"
New-AzBatchApplicationPackage -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationIdWindows -ApplicationVersion $version -Format zip -FilePath $buildOutputZipWindows

Write-Host "Setting default application version to $version"
Set-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationIdWindows -DefaultVersion $version

####
# Create the Linux application
####
Write-Host "Creating new Azure Batch Application named $applicationIdLinux"
New-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationIdLinux

#Add the package 
Write-Host "Uploading application package to Azure Batch account"
New-AzBatchApplicationPackage -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationIdLinux -ApplicationVersion $version -Format zip -FilePath $buildOutputZipLinux

Write-Host "Setting default application version to $version"
Set-AzBatchApplication -AccountName $batchAcctName -ResourceGroupName $resourceGroupName -ApplicationId $applicationIdLinux -DefaultVersion $version


$batch = Get-AzBatchAccountKey -AccountName $batchAcctName -ResourceGroupName $resourceGroupName
$t = Get-AzBatchAccount -AccountName $batchAcctName -ResourceGroupName $resourceGroupName

$storage = Get-AzStorageAccount -Name $storageAcctName -ResourceGroupName $resourceGroupName
$s = Get-AzStorageAccountKey -Name $storageAcctName -ResourceGroupName $resourceGroupName

$e = Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $namespaceName -EventHubName $eventHubName -AuthorizationRuleName batchbuilder

$settingsFileWindows = [PSCustomObject]@{
    AuthenticationArgs = @{
        UserName = ""
        Password = ""
    }
    BatchArgs = @{
        BatchNodeCount = "2"
        BatchAccountName = $batchAcctName
        BatchAccountKey = "$($batch.PrimaryAccountKey)"
        BatchAccountUrl = "https://$($t.AccountEndpoint)"
        StorageAccountName = $storageAcctName
        StorageAccountKey = "$($s.Value[0])"
        BatchVmSize=  "STANDARD_DS1_V2"
        DeleteBatchPool = $false
        DeleteBatchJob = $false
        PollBatchPoolStatus = $True
        EventHubConnectionString = "$($e.PrimaryConnectionString)"
        BatchPoolOs = "Windows"
    }
    RootLoggingPath = "C:\temp"
    TimeoutRetryCount = 0
    DefaultScriptTimeout = 500
}

$settingsFileLinux= [PSCustomObject]@{
    AuthenticationArgs = @{
        UserName = ""
        Password = ""
    }
    BatchArgs = @{
        BatchNodeCount = "2"
        BatchAccountName = $batchAcctName
        BatchAccountKey = "$($batch.PrimaryAccountKey)"
        BatchAccountUrl = "https://$($t.AccountEndpoint)"
        StorageAccountName = $storageAcctName
        StorageAccountKey = "$($s.Value[0])"
        BatchVmSize=  "STANDARD_DS1_V2"
        DeleteBatchPool = $false
        DeleteBatchJob = $false
        PollBatchPoolStatus = $True
        EventHubConnectionString = "$($e.PrimaryConnectionString)"
        BatchPoolOs = "Windows"
    }
    RootLoggingPath = "C:\temp"
    TimeoutRetryCount = 0
    DefaultScriptTimeout = 500
}

Write-Host "Saving EventHub Connection string to environment variable"
[System.Environment]::SetEnvironmentVariable("AzureEventHubAppenderConnectionString", "$($e.PrimaryConnectionString)",[System.EnvironmentVariableTarget]::User)

$settingsFileWindows | ConvertTo-Json | Set-Content -Path "..\..\src\TestConfig\settingsfile.json"
Write-Host "Saved settings file to " + Resolve-Path "..\..\src\TestConfig\settingsfile.json"

$settingsFileWindows | ConvertTo-Json | Set-Content -Path "..\..\src\TestConfig\settingsfile-linux.json"
Write-Host "Saved settings file to " + Resolve-Path "..\..\src\TestConfig\settingsfile-linux.json"

Write-Host "Pre-populated command line arguments. Record these for use later: "
Write-Host ""
Write-Host "--batchaccountname=$batchAcctName"
Write-Host "--batchaccounturl=https://$($t.AccountEndpoint)"
Write-Host "--batchaccountkey=$($batch.PrimaryAccountKey)"
Write-Host "--storageaccountname=$storageAcctName"
Write-Host "--storageaccountkey=$($s.Value[0])"
Write-Host "--eventhubconnectionstring=$($e.PrimaryConnectionString)"