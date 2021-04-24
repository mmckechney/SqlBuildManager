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
 $Batchprefix = "sbmmwm",

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
if($null -eq (Get-AzSqlServer -ResourceGroupName $ResourceGroupName  -ServerName $SqlServerName -ErrorAction SilentlyContinue))
{
    Write-Host "Creating new SQL Server: $SqlServerName"
    New-AzSqlServer -ResourceGroupName $ResourceGroupName -Location $Location -ServerName $SqlServerName -ServerVersion "12.0" -SqlAdministratorCredentials $SqlCredential
}
else 
{
    Write-Host "Updating admin password on SQL Server: $SqlServerName"
    Set-AzSqlServer -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -SqlAdministratorPassword $password
}

if($null -eq (Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName.ToLower() -ErrorAction SilentlyContinue))
{
    Write-Host "Updating firewall rules on SQL Server: $SqlServerName"
    New-AzSqlServerFirewallRule -AllowAllAzureIPs -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName.ToLower()
}

if($null -eq (Get-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ElasticPoolName $ElasticPoolName -ErrorAction SilentlyContinue))
{
    Write-Host "Creating Elastic pool $ElasticPoolName for SQL Server: $SqlServerName"
    New-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ElasticPoolName $ElasticPoolName -Edition "Basic" -Dtu 50 
}
For ($i=1; $i -lt  $TestDatabaseCount+1; $i++) 
{
    $dbNumber = $DatabaseNameRoot + $i.ToString("000")
    if($null -eq (Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -DatabaseName $dbNumber -ErrorAction SilentlyContinue))
    {
        Write-Host "Creating database: $dbNumber"
        New-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ElasticPoolName $ElasticPoolName -DatabaseName $dbNumber
    }
    else 
    {
        Write-Host "Database already exists: $dbNumber"
    }
}

########################################################
# Output the target database configuration file
########################################################
$server = Get-AzSqlServer -ResourceGroupName $ResourceGroupName  -ServerName $SqlServerName
$dbs = Get-AzSqlDatabase -ResourceGroupName $server.ResourceGroupName -ServerName $server.ServerName | Sort-Object -Property DatabaseName


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
foreach($db in $dbs)
{
    if($db.DatabaseName -ne "master")
    {
        $server.FullyQualifiedDomainName + ":SqlBuildTest,"+$db.DatabaseName | Out-File -Append $outputDbConfigFile
        $server.FullyQualifiedDomainName + ":client,"+$db.DatabaseName | Out-File -Append $clientDbConfigFile
    }
}


#########################################################################################
# Call to deploy_batch.ps to create the batch, storage, eventhub and queue infrastructure
#########################################################################################
$SubscriptionId = (Get-AzContext).Subscription.Id
./deploy_azure_resources.ps1 -subscriptionId $SubscriptionId -resourceGroupName $ResourceGroupName -resourceGroupLocation $Location -batchprefix $batchprefix -outputpath $outputPath


#############################################################
# update the settings file with the SQL UserName and Password
#############################################################
$AESKey = New-Object Byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
$settingsFileKey = [System.Convert]::ToBase64String($AESKey);

$tmpPath = Resolve-Path $settingsJsonWindows
Write-Output "Saving settings file to $tmpPath"
./..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent --settingsfilekey $settingsFileKey

$tmpPath = Resolve-Path $settingsJsonLinux
Write-Output "Saving settings file to $tmpPath"
./..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent --batchpoolos Linux --settingsfilekey $settingsFileKey

$keyFile = Join-Path $outputpath "settingsfilekey.txt"
$settingsFileKey |  Set-Content -Path $keyFile