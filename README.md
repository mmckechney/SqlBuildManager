# SQL Build Manager

SQL Build Manager is a multi-faceted tool to allow you to manage the lifecyle of your databases. It started as a forms based application for the management of a handful of databases and then switched focus to command line automation for the management of thousands of databases.

## Key Features

* Single transaction handling. If any one script fails, the entire package is rolled back, leaving the database unchanged.
* Handle multiple database updates in one package - seamlessly update all your databases.
* Full script run management. Control the order, target database, and transaction handling
* Trial mode - runs scripts to test against database, then rolls back to leave in pristine state.
* Automatic logging and version tracking of scripts on a per-server/per-database level
* Full SHA-1 hashing of individual scripts and .sbm package files to ensure integrity of the scripts
* Execution of a build package (see below) is recorded in the database for full tracking of update history, script validation and potential rebuilding of packages
* Massively parallel execution across thousands of databases utilzing local threading, an Azure cloud service deployment or an Azure Batch execution

## The Basics
At the core of the process is the "SQL Build Manager Package" file (.sbm extension).  Under the hood, this file is a Zip file that contains the scripts that constitute your "release" along with a configuration file  (SqlSyncBuildProject.xml) that contains meta data on the scripts and execution parameters:

* `FileName`: Self explanatary, the name of the script file
* `BuildOrder`: The relative order that the scripts in the package will be run
* `Description`: Optional description about the script
* `RollBackOnError`: Option on whether or not to roll back the transaction if there is an error running this script (default: `true`)
* `CausesBuildFailure`: Option on whether or not to roll back the entire build if there is a failure wiht this script (default `true`)
* `DateAdded`: Date and time that the script was added to the package
* `ScriptId`: System generated GUID identifier for the script
* `Database`: Target database to run the scripts against. (This can be overridden in the case of multiple DB targets)
* `StripTransactionText`: Script handling to remove any inline transction statements (default is `true` because you want SQL Build Manager to handle transactions)
* `AllowMultipleRuns`: Whether or not this script can be run on the same database multiple times (default is `true` and you should always right scripts so this is viable)
* `AddedBy`: User ID of the user that added the script to the package
* `ScriptTimeOut`: Timeout setting for the execution of this script
* `DateModified`: If the script has been modified after being added, this will be populated (otherwise `DateTime.Min`)
* `ModifiedBy`: If the script has been modified after being added, this will be populated with the user's ID
* `Tag`: Optional tag for the script


## Creating a Build Package

### Forms UI

While the focus of the app has changed to command line automation, the forms GUI is fully functional. If you are looking for a visual tool, check out _Sql Build Manager.exe_. There is docmentation on the GUI that you can find [here](https://github.com/mmckechney/SqlBuildManager/blob/master/SqlBuildManager%20Manual/SqlBuildManagerManual.pdf) that will walk through the creation of build packages

### Command line

The command line utility is geared more toward executing a build vs. creating the package itself. You can however extract a build package file from a DACPAC file using the `Action=ScriptExtract` flag. This is useful if you are utilizing the recommended [data-tier application](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications?view=sql-server-2017)  method.


## Full Command Line Reference


```Command line automation is accomplished via  _SqlBuildManager.Console.exe_. The usage 



    Usage: SqlBuildManager.Console /Action=<action> | args below

    /? or /help                      Show this help

Action value options (/Action=<value>)
    Build                    Performs a standard,local SBM execution via command line
    Threaded                 For updating multiple databases simultaneously from the current machine
    Remote                   For updating multiple databases simultaneously using remote execution servers to spread the processing                                                 Uses SqlBuildManager.Services deployed as an Azure Cloud service - NOTE: this is a legacy deployment method)
    Batch                    For updating multiple databases simultaneously using Azure batch services (prefered method for distributed deployment)
    Package                  Creates an SBM package from an SBX configuraiton file and scripts
    PolicyCheck              Performs a script policy check on the specified SBM package
    GetHash                  Calculates the SHA-1 hash fingerprint value for the SBM package (scripts + run order)
    CreateBackout            Generates a backout package (reversing stored procedure and scripted object changes)
    GetDifference            Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between /Database and /GoldDatabase
    Syncronize               Performs a database syncronization between between /Database and /GoldDatabase
    ScriptExtract            Extract a SBM package from a source /PlatinumDacPac


General Authentication settings (/Action={Build|Threaded|Remote|Batch|GetDifference|Syncronize})
    /AuthType="<type>"                Values: "Windows" (default if no Username/Password set), "AzureADIntegrated", "AzureADPassword", "Password" (default if Username/Password set)
    /UserName="<username>"            The username to authenticate against the database if not using integrate auth (required when RemoteServers="azure")
    /Password="<password>"            The password to authenticate against the database if not using integrate auth (required when RemoteServers="azure")
    /Database="<database name>"       1) Name of a single database to run against or 2) source database for scripting or runtime configuration
    /Server="<server name>"           1) Name of a server for single database run or 2) source server for scripting or runtime configuration

General Runtime settings (/Action={Build|Threaded|Remote})
    /PackageName="<filename>"         Name of the .sbm or .sbx file to execute
    /RootLoggingPath="<directory>"    Directory to save execution logs (for threaded and remote executions)
    /Trial=(true|false)               Whether or not to run in trial mode (default is false)
    /Transactional=(true|false)       Whether or not to run with a wrapping transaction (default is true)
    /Override=("<filename>"|list)     File containing the target database settings (usually a formatted .cfg file)
    /Description="<description>"      A free form description for the execution run
    /LogAsText=(true|false)           Whether to log as text or HTML. Default is true
    /BuildRevision="<rev identifier>" If provided, the build will include an update to a "Versions" table and this will be the value used to add to a "VersionNumber" column (varchar(max))
    /LogToDatabaseName="<db name>"    [Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target). 
    /ScriptSrcDir="<directory>"       [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)
    /ScriptLogFile="<filename>"       [Not recommended] Alternate name for the file containing the script run log

Azure Batch Execution (/Action=Batch)
    /PlatinumDacpac="<filename>"              Name of the dacpac containing the platinum schema
    /PackageName="<filename>"                 Name of the .sbm or .sbx file to execute
    /RootLoggingPath="<directory>"            Directory to save execution logs 
    /DeleteBatchPool=(true|false)             Whether or not to delete the batch pool servers after an execution (default is true)
    /BatchNodeCount="##"                      Number of nodes to provision to run the batch job  (default is 10)
    /BatchVmSize="<size>"                     Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]
    /BatchAccountName="<batch acct name>"     String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
    /BatchAccountKey="<batch acct key>"       Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
    /BatchAccountUrl="<batch acct url>"       URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
    /StorageAccountName="<storage acct name>" Name of storage account associated with the Azure Batch account  [can also be set via StorageAccountName app settings key]
    /StorageAccountKey="<storage acct key>"   Account Key for the storage account  [can also be set via StorageAccountKey app settings key]

Remote Execution settings (/Action=Remote)
    /RemoteServers=("<filename>"|derive|azure)     Pointer to file that contains the list of remote execution servers,
	                                               "derive" to parse servers from DB list, azure to use Azure PaaS instances
    /TimeoutRetryCount=(integer)                   Number of times to retry os a script timeout is encountered (default is 0)
    /DistributionType=(equal|local)                Sets whether to split execution evenly across all execution servers 
	                                               or have each agent only run against its local databases. Local not supported with RemoteServers="azure" 
    /TestConnectivity=(true|false)                 True value will test connection to remote agent and databases but will not execute SQL scripts
    /AzureRemoteStatus=true                        Return a status of the Azure remote execution services. Will not execute SQL 
    /RemoteDbErrorList=<remote name>|all           Returns the list of databases that has execution errors in the last run. Use "all" to get list from all remote servers
    /RemoteErrorDetail=<server:db>|<instance>|all  Retrieves the error detail for each instance in error. Is "server:db" is provided, full log detail is returned

Dacpac Execution (/Action={Threaded|Remote|Batch})
    /PlatinumDacpac="<filename>"            Name of the dacpac containing the platinum schema
    /TargetDacpac="<filename>"              Name of the dacpac containing the schema of the database to be updated
    /ForceCustomDacPac=(true|false)         *USE WITH CAUTION! (/Action=Threaded only)This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.
    /PlatinumDbSource="<database name>"     Instead of a formally built Platinum Dacpac, target this database as having the desired state schema
    /PlatinumServerSource="<server name>"   Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema


Script Extraction from Dacpac (/Action=ScriptExtract)
    /PlatinumDacpac="<filename>"            Name of the dacpac containing the platinum schema
    /OutputSbm="<filename>"                 Name (and path) of the SBM package to create

Database Synchronization (/Action={Synchronize|GetDifference})
    /GoldDatabase="<database name>"   The "gold copy" database that will serve as the model for what the /Database should look like
    /GoldServer="<server name>"       The server that the "gold copy" database can be found
    /ContinueOnFailure=(true|false)   Whether or not to continue on the failure of a package. Default is false. 

SBX to SBM packaging (/Action=Package)
    /Directory="<directory>"          Directory containing 1 or more SBX files to package into SBM zip files
   
Policy checking (/Action=PolicyCheck)
    /PackageName="<filename>"        Name of the SBM package to check policies on

Calculate hash/fingerprint (/Action=GetHash)
    /PackageName="<filename>"        Name of the SBM package to calculate SHA-1 hash value
   
Creating backout packages (/Action=CreateBackout)
    /PackageName="<filename>"        Name of the SBM package to calculate a backout package for
    /Server=<servername>             Name of the server that contains the desired state schema to "backout to"
    /Database=<databasename>         Name of the database that contains the desired state schema to "backout to"
```
 