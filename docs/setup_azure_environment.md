# Setting up a test Azure Environment

## Create Test Target databases

To test against Azure databases, you will need some in Azure! The following PowerShell will:

- Create an Azure SQL Server
- Create a SQL Azure Elastic Pool for cost effective use of the databases
- Provision "X" number of databases for that pool. ("x" is defined by the optional `TestDatabaseCount` parameter)
- Build the solution, create a publish target for the `sbm.csproj` file, create a zip package for uploading to the Azure batch account
- Create an Azure Batch account, with the latest build of the sbm.exe CLI uploaded and installed

It is important to note that you can re-run this script at any time to ensure your environment is set up properly. It will not impact running resources if run multiple times. 

## Steps

1. In a PowerShell window, navigate to the `docs/templates` folder
2. Run the `Login-AzAccount` command to connect to your Azure account
3. Run the `Create_AzureTestEnvironment.ps1` file. You will be prompted for parameters:

    - `ResourceGroupName` - Azure resource group to create and put the resources into
    - `Location` - the Azure region to deploy the resources
    - `SqlServerName` - the name of the Azure SQL PaaS server to create
    - `ElasticPoolName` - the name of the elastic pool to put your databases in
    - `DatabaseNameRoot` - the prefix for you Azure SQL databases. This will be appended with sequence numbers (001, 002, etc.)
    - `Batchprefix` - the prefix for the Azure batch components. Must be 6 or less characters, all lowercase. 
    - `SqlServerUserName` - the SQL Admin name to use when creating the SQL server
    - `SqlServerPassword` - the SQL Admin password to use. Upon re-running the script, it will reset the Admin password to this value
    - `TestDatabaseCount` - an optional parameter. The default is 10

Once the script is done running, it will save two files to the `src\TestConfig` folder:

1. `databasetargets.cfg` - a pre-configured database listing file for use in a batch or threaded execution targeting the SQL Azure databases just created
2. `settingsfile.json` - a batch settings file that contains all of the SQL, Batch and Storage endpoints and connection keys for use in testing

**These files will be used by the tests located in the `SqlBuildManager.Console.ExternalTest` project, and can also be leveraged for any manual testing you need to perform**


# Notes on Unit Testing

**NOTE: There are currently some concurrency issues with the unit tests. You may get some failures in a full run that will then succeed after running aain, selecting only the failed tests** 

There are two types of Unit Tests included in the solution. Those that are dependent on a local SQLEXPRESS database (~.Dependent.UnitTest.csproj) and those that aren't (~UnitTest.csproj). If you want to be able to run the database dependent tests, you will need to install SQL Express as per the next section

## SQL Express

In order to get some of the unit tests to succeed, you need to have a local install of SQLExpress. You can find the installer from here [https://www.microsoft.com/en-us/sql-server/sql-server-editions-express] (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express). You should be able to leverage the basic install.

## sqlpackage.exe

`sqlpackage` is needed for the use of the DACPAC features of the tool. It should already be available in the `Microsoft_SqlDB_DAC` subfolder (Windows) or the `microsoft-sqlpackage-linux` subfolder (Linux) where you are running your tests. If not, you can install or update the package from here [https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download](https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download).

The unit tests should find the executable but if not, you may need to add the path to `\SqlBuildManager\SqlSync.SqlBuild\DacPacHelper.cs` in the getter for `sqlPackageExe`.

-----

# Visual Studio Installer Project
For Visual Studio 2015 and beyond, you will need to install an extension to load the installer project (.vdproj)

### Visual Studio 2015
https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9

### Visual Studio 2017 and 2019
https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects

_If you are having trouble with the installer project loading try disable extension "Microsoft Visual Studio Installer Projects", reenable, then reload the projects.
