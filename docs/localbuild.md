# Visual Studio Installer Project
For Visual Studio 2015 and beyond, you will need to install an extension to load the installer project (.vdproj) 

### Visual Studio 2015
https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9

### Visual Studio 2017
https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects


_If you are having trouble with the installer project loading try disable extension "Microsoft Visual Studio Installer Projects", renable, then reload the projects._


# Notes in Unit Testing

**NOTE: There are currently some concurrency issues with the unit tests. You may get some failures in a full run that will then succeed after running aain, selecting only the failed tests** 

There are two types of Unit Tests included in the solution. Those that are dependent on a local SQLEXPRESS database (~.Dependent.UnitTest.csproj) and those that aren't (~UnitTest.csproj). If you eant to be able to run the database dependent tests, you will need to install SQL Express as per the next section

## SQL Express
In order to get some of the unit tests to succeeed, you need to have a local install of SQLExpress. You can find the installer from here [https://www.microsoft.com/en-us/sql-server/sql-server-editions-express] (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express). You should be able to leverage the basic install.

## SQL Package
`sqlpackage.exe` is needed for the use of the DACPAC features of the tool. It should already be available in the `Microsoft_SqlDB_DAC` subfolder where you are running your tests. If not, you can install the package from here [https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download?view=sql-server-2017](https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download?view=sql-server-2017). The unit tests should find the executable but if not, you may need to add the path to `\SqlBuildManager\SqlSync.SqlBuild\DacPacHelper.cs` in the getter for `sqlPackageExe`.

# Setting up a test Azure Environment

## Create Test Target databases
To test against Azure databases, you will need some in Azure! The following PowerShell will create an Azure SQL Server, an Elastic Pool and "X" number of databases for that pool. This can be done locally or via the Azure Cloud Shell (http://shell.azure.com)

1. Create the Resource Group and Server (change your values accordingly)

```
$ResourceGroupName = "SqlResourceGroup"
$Location = "East US"
$ServerName = "TestServer001"

New-AzResourceGroup  -ResourceGroupName $ResourceGroupName -Location $Location 

New-AzSqlServer -ResourceGroupName $ResourceGroupName -Location $Location -ServerName $ServerName -ServerVersion "12.0" -SqlAdministratorCredentials (Get-Credential)

New-AzSqlServerFirewallRule -AllowAllAzureIPs -ResourceGroupName $ResourceGroupName -ServerName $ServerName

```

2. Create the elastic pool

```
$ElasticPoolName="MyBasicPool2"
New-AzSqlElasticPool -ResourceGroupName $ResourceGroupName -ServerName $ServerName -ElasticPoolName $ElasticPoolName -Edition "Basic" -Dtu 50 
```

3. Create databases within the pool for testing

```
$DatabaseName="SqlDemo001"
New-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -ElasticPoolName $ElasticPoolName -DatabaseName $DatabaseName
```

4. Or to create a collection of databases you  can use

```
$DatabaseName = "SqlDemo"

For ($i=1; $i -lt 101; $i++) 
{
    $dbNumber = $DatabaseName + $i.ToString("000")
    New-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -ElasticPoolName $ElasticPoolName -DatabaseName $dbNumber
}
```

## Creating a database target file
To create database target files for a parallel test execution, you can use the following script. Note that this will get _every_ SQL Server and _every_ database. You may want to add some customization to get only those that you want.

```
$outputFile = "C:\temp\databasetargets.cfg"
$servers = Get-AzResourceGroup | Get-AzSqlServer
foreach($server in $servers)
{
    $dbs = Get-AzSqlDatabase -ResourceGroupName $server.ResourceGroupName -ServerName $server.ServerName | Sort-Object -Property DatabaseName
    
    foreach($db in $dbs)
    {
        if($db.DatabaseName -ne "master")
        {
            $server.FullyQualifiedDomainName + ":client,"+$db.DatabaseName | Out-File -Append $outputFile
        }
    }
}
```
