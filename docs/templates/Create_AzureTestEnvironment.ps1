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

$DatabaseNameRoot = "SqlBuildTest"
$outputDbConfigFile = "..\..\src\TestConfig\databasetargets.cfg"

# Create the resource Group for your test resources
if($null -eq (Get-AzResourceGroup -ResourceGroupName $ResourceGroupName -Location $Location -ErrorAction SilentlyContinue))
{
    New-AzResourceGroup  -ResourceGroupName $ResourceGroupName -Location $Location 
}

# Create the SQL server, Elastic pool and test databases
$password = $SqlServerPassword | ConvertTo-SecureString -AsPlainText -Force
$SqlCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $SqlServerUserName,  $password
if($null -eq (Get-AzSqlServer -ResourceGroupName $ResourceGroupName  -ServerName $SqlServerName -ErrorAction SilentlyContinue))
{
    New-AzSqlServer -ResourceGroupName $ResourceGroupName -Location $Location -ServerName $SqlServerName -ServerVersion "12.0" -SqlAdministratorCredentials $SqlCredential
}
else 
{
    Set-AzSqlServer -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -SqlAdministratorPassword $password
}

if($null -eq (Get-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName.ToLower() -ErrorAction SilentlyContinue))
{
    New-AzSqlServerFirewallRule -AllowAllAzureIPs -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName.ToLower()
}

if($null -eq (Get-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ElasticPoolName $ElasticPoolName -ErrorAction SilentlyContinue))
{
    New-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ElasticPoolName $ElasticPoolName -Edition "Basic" -Dtu 50 
}
For ($i=1; $i -lt  $TestDatabaseCount+1; $i++) 
{
    $dbNumber = $DatabaseNameRoot + $i.ToString("000")
    if($null -eq (Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -DatabaseName $dbNumber -ErrorAction SilentlyContinue))
    {
        New-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ElasticPoolName $ElasticPoolName -DatabaseName $dbNumber
    }
}

#Output the target database configuration file
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

foreach($db in $dbs)
{
    if($db.DatabaseName -ne "master")
    {
        $server.FullyQualifiedDomainName + ":SqlBuildTest,"+$db.DatabaseName | Out-File -Append $outputDbConfigFile
    }
}


##Now create the batch 
$SubscriptionId = (Get-AzContext).Subscription.Id
./deploy_batch.ps1 -subscriptionId $SubscriptionId -resourceGroupName $ResourceGroupName -resourceGroupLocation $Location -batchprefix $batchprefix

#update the settings file with the SQL UserName and Password
$tmpPath = Resolve-Path "..\..\src\TestConfig\settingsfile.json"
./..\..\src\SqlBuildManager.Console\bin\Debug\netcoreapp3.1\sbm.exe batch savesettings --settingsfile $tmpPath  --username $SqlServerUserName --password $SqlServerPassword --silent