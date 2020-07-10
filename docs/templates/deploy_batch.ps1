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
 $outputpath,

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


##########################################
# Build the solution
##########################################
dotnet restore "..\..\src\sqlsync.sln" 
dotnet build "..\..\src\sqlsync.sln" --configuration Debug


##########################################
# Create the application package zip files
##########################################
foreach ($env in $vars) {


    Write-Output "Publishing for $($env.OSName)"
    dotnet publish  "..\..\src\SqlBuildManager.Console\sbm.csproj" -r $env.BuildTarget --configuration Debug
    $source= Resolve-Path "..\..\src\SqlBuildManager.Console\bin\Debug\netcoreapp3.1\$($env.BuildTarget)\publish"
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


##################################################
# Save settings files for environments
##################################################
$batch = Get-AzBatchAccountKey -AccountName $batchAcctName -ResourceGroupName $resourceGroupName
$t = Get-AzBatchAccount -AccountName $batchAcctName -ResourceGroupName $resourceGroupName
$s = Get-AzStorageAccountKey -Name $storageAcctName -ResourceGroupName $resourceGroupName
$e = Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $namespaceName -EventHubName $eventHubName -AuthorizationRuleName batchbuilder

foreach ($env in $vars)
{

    $settingsFile = [PSCustomObject]@{
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
            BatchPoolOs = $env.OSName
            BatchPoolName = $env.PoolName
            BatchApplicationPackage = $env.ApplicationName
        }
        RootLoggingPath = "C:\temp"
        TimeoutRetryCount = 0
        DefaultScriptTimeout = 500
    }

    $tmpPath = Join-Path $outputPath "settingsfile-$($env.OSName.ToLower()).json"
    $settingsFile | ConvertTo-Json | Set-Content -Path $tmpPath
    Write-Host "Saved settings file to " + $tmpPath
}

##################################################
# Output values for reference
##################################################
Write-Host "Saving EventHub Connection string to environment variable"
[System.Environment]::SetEnvironmentVariable("AzureEventHubAppenderConnectionString", "$($e.PrimaryConnectionString)",[System.EnvironmentVariableTarget]::User)

Write-Host "Pre-populated command line arguments. Record these for use later: "
Write-Host ""
Write-Host "--batchaccountname=$batchAcctName"
Write-Host "--batchaccounturl=https://$($t.AccountEndpoint)"
Write-Host "--batchaccountkey=$($batch.PrimaryAccountKey)"
Write-Host "--storageaccountname=$storageAcctName"
Write-Host "--storageaccountkey=$($s.Value[0])"
Write-Host "--eventhubconnectionstring=$($e.PrimaryConnectionString)"