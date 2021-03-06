param(
 [Parameter(Mandatory=$True)]
 [string]
 $ResourceGroupName = "SqlResourceGroupZZ",

 [Parameter(Mandatory=$True)]
 [string]
 $Location = "East US",

 [Parameter(Mandatory=$True)]
 [string]
 $SqlServerName = "TestServer001MMM",

 [Parameter(Mandatory=$True)]
 [string]
 $ElasticPoolName= "MyBasicPool2",

 [Parameter(Mandatory=$True)]
 [string]
 $ResourcePrefix = "sbmmwm",

 [Parameter(Mandatory=$True)]
 [string]
 $SqlServerUserName,

 [Parameter(Mandatory=$True)]
 [string]
 $SqlServerPassword,

 [int]
 $TestDatabaseCount = 10

)

$outputPath = "..\..\src\TestConfig"
$DatabaseNameRoot = "SqlBuildTest"
$outputDbConfigFile = Join-Path $outputPath "databasetargets.cfg"
$clientDbConfigFile = Join-Path $outputPath "clientdbtargets.cfg"
$settingsJsonWindows = Join-Path $outputPath "settingsfile-windows.json"
$settingsJsonLinux = Join-Path $outputPath "settingsfile-linux.json"
$settingsJsonWindowsQueue = Join-Path $outputPath "settingsfile-windows-queue.json"
$settingsJsonLinuxQueue = Join-Path $outputPath "settingsfile-linux-queue.json"

###################################################
# Create the resource Group for your test resources
###################################################
Write-Host "Creating Resourcegroup : $ResourceGroupName"
if($null -eq (Get-AzResourceGroup -ResourceGroupName $ResourceGroupName -Location $Location -ErrorAction SilentlyContinue))
{
    New-AzResourceGroup  -ResourceGroupName $ResourceGroupName -Location $Location 
}

########################################################
# Create the SQL server, Elastic pool and test databases
########################################################
$password = $SqlServerPassword | ConvertTo-SecureString -AsPlainText -Force
$SqlCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $SqlServerUserName,  $password

$nameSuffixes = @("A", "B")
$outputDbConfig = @()
$ClientDbConfig = @()
foreach($suffix in $nameSuffixes)
{

    $tmpServer = $SqlServerName + "-" + $suffix
    $tmpPool = $ElasticPoolName + "-" + $suffix
    if($null -eq (Get-AzSqlServer -ResourceGroupName $ResourceGroupName  -ServerName $tmpServer -ErrorAction SilentlyContinue))
    {
        Write-Host "Creating new SQL Server: $tmpServer"
        New-AzSqlServer -ResourceGroupName $ResourceGroupName -Location $Location -ServerName $tmpServer -ServerVersion "12.0" -SqlAdministratorCredentials $SqlCredential
    }
    else 
    {
        Write-Host "Updating admin password on SQL Server: $tmpServer"
        Set-AzSqlServer -ResourceGroupName $ResourceGroupName -ServerName $tmpServer -SqlAdministratorPassword $password
    }

    if($null -eq (Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $tmpServer.ToLower() -ErrorAction SilentlyContinue))
    {
        Write-Host "Updating firewall rules on SQL Server: $tmpServer"
        New-AzSqlServerFirewallRule -AllowAllAzureIPs -ResourceGroupName $ResourceGroupName -ServerName $tmpServer.ToLower()
    }

    if($null -eq (Get-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $tmpServer -ElasticPoolName $tmpPool -ErrorAction SilentlyContinue))
    {
        Write-Host "Creating Elastic pool $tmpPool for SQL Server: $tmpServer"
        New-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $tmpServer -ElasticPoolName $tmpPool -Edition "Basic" -Dtu 50 
    }
    For ($i=1; $i -lt  $TestDatabaseCount+1; $i++) 
    {
        $dbNumber = $DatabaseNameRoot + $i.ToString("000")
        if($null -eq (Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $tmpServer -DatabaseName $dbNumber -ErrorAction SilentlyContinue))
        {
            Write-Host "Creating database: $dbNumber"
            New-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $tmpServer -ElasticPoolName $tmpPool -DatabaseName $dbNumber
        }
        else 
        {
            Write-Host "Database already exists: $dbNumber"
        }
    }

    ########################################################
    # Output the target database configuration file
    ########################################################
    $server = Get-AzSqlServer -ResourceGroupName $ResourceGroupName  -ServerName $tmpServer
    $dbs = Get-AzSqlDatabase -ResourceGroupName $server.ResourceGroupName -ServerName $server.ServerName | Sort-Object -Property DatabaseName

    foreach($db in $dbs)
    {
        if($db.DatabaseName -ne "master")
        {
            $outputDbConfig += ,@($server.FullyQualifiedDomainName + ":SqlBuildTest,"+$db.DatabaseName) # | Out-File -Append $outputDbConfigFile
            $ClientDbConfig += ,@($server.FullyQualifiedDomainName + ":client,"+$db.DatabaseName) # | Out-File -Append $clientDbConfigFile
        }
    }
}

########################################################
# Output the target database configuration file
########################################################

# check to see if the folder is there, if not, create it
$path = Split-Path $outputDbConfigFile
if( (Test-Path $path) -eq $false)
{
    New-Item -Path $path  -ItemType Directory
}

#Clean out any existing cfg file
if( (Test-Path $outputDbConfigFile) -eq $True)
{
    Remove-Item $outputDbConfigFile
}
if( (Test-Path $clientDbConfigFile) -eq $True)
{
    Remove-Item $clientDbConfigFile
}


Write-Host "Creating database config file for unit testing: $outputDbConfigFile "
foreach($item  in  $outputDbConfig)
{
    $item | Out-File -Append $outputDbConfigFile
}
foreach($item  in  $ClientDbConfig)
{
    $item | Out-File -Append $clientDbConfigFile
}

####################################################################################################
# Call to deploy_azure_resources.ps1 to create the batch, storage, eventhub and queue infrastructure
####################################################################################################
$SubscriptionId = (Get-AzContext).Subscription.Id
./deploy_azure_resources.ps1 -subscriptionId $SubscriptionId -resourceGroupName $ResourceGroupName -resourceGroupLocation $Location -resourcePrefix $ResourcePrefix -outputpath $outputPath

###################################################################
# Call to deploy_kubernetes_resources.ps1 to create the AKS cluster
###################################################################
./deploy_kubernetes_resources.ps1 -subscriptionId $SubscriptionId -resourceGroupName $ResourceGroupName -resourceGroupLocation $Location -resourcePrefix $ResourcePrefix -outputpath $outputPath -sqlServerUserName $SqlServerUserName -sqlServerPassword $SqlServerPassword


#############################################################
# update the settings file with the SQL UserName and Password
#############################################################
$AESKey = New-Object Byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
$settingsFileKey = [System.Convert]::ToBase64String($AESKey);

$tmpPath = Resolve-Path $settingsJsonWindows
Write-Output "Saving settings file to $tmpPath"
..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent --settingsfilekey $settingsFileKey

$tmpPath = Resolve-Path $settingsJsonLinux
Write-Output "Saving settings file to $tmpPath"
..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent --batchpoolos Linux --settingsfilekey $settingsFileKey

$tmpPath = Resolve-Path $settingsJsonWindowsQueue
Write-Output "Saving settings file to $tmpPath"
..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent --settingsfilekey $settingsFileKey

$tmpPath = Resolve-Path $settingsJsonLinuxQueue
Write-Output "Saving settings file to $tmpPath"
..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent --batchpoolos Linux --settingsfilekey $settingsFileKey


$keyFile = Join-Path $outputpath "settingsfilekey.txt"
$settingsFileKey |  Set-Content -Path $keyFile