
# Command line Usage

**NOTE:** For command line operations, you must use `SqlBuildManager.Console.exe`

----

**Tip**: If you don't like typing the full name of the exe you can easily create an alias with the files in the [Utility](../Utility) folder:
- Edit the [alias.bat](../Utility/alias.bat) file with the path to where you have the SqlBuildManager.Console.exe file. 
- Edit the [command_alias.reg](../Utility/command_alias.reg) file to the path where you save the *alias.bat* file.
- Run th *command_alias.reg* file to add the key to your registry. 

Now you can use shorthand *sbm* to run the command-line!

------

## Getting started

### Defining the Action
The basics of running the command line is the /Action argument. This is required for all executions (except for `/?` or`/help` - which I hope are self explanatory)


#### Action value options (`/Action=<value>`)

##### Build execution actions to update databases 
- `Build` - Performs a standard, local execution via command line
- `Threaded` - For updating multiple databases simultaneously. The threading is all run from the current machine
 - `Batch` - For massively parallel updating of databases simultaneously using Azure batch services
- `Remote` - [_deprecated_ - use `Batch` instead] For massively parallel updating of databases simultaneously using remote execution servers deployed as Azure classic cloud services 

##### Utility actions 
- `BatchPreStage` - Pre-stage Azure Batch VM nodes
- `BatchCleanUp` - Deletes Azure Batch VM nodes
- `SaveSettings` - Creates a Json file that will save your userName and password as well as your Batch settings so that you can reference them later.
- `Package` - Create an SBM package from an SBX configuration file and scripts
- `PolicyCheck` - Performs a script policy check on the specified SBM package
-  `GetHash` - Calculates the SHA-1 hash fingerprint value for the SBM package (scripts + run order)
- `CreateBackout` - Generates a backout package (reversing stored procedure and scripted object changes)
- `GetDifference` - Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between /Database and /GoldDatabase
- `Synchronize` - Performs a database synchronization between between /Database and /GoldDatabase
- `ScriptExtract` - Extract a SBM package from a source `/PlatinumDacPac`


#### General Authentication settings 

Used to provide authentication to databases during a build or for script extraction/ DACPAC creation\
Applies to: `/Action={Build|Threaded|Batch|Remote|GetDifference|Synchronize`\
_Note:_ Is using username/password authentication, you can also leverage the [SettingsFile](#save-settings-actionsavesettings) option
- `/AuthType="<type>"` - Values: `AzureADIntegrated`, `AzureADPassword`, `Password` (default if Username/Password set), `Windows` (default if no Username/Password set), 
- `/UserName="<username>"` - The username to authenticate against the database if not using integrate auth (required when `RemoteServers="azure"`)
- `/Password="<password>"` - The password to authenticate against the database if not using integrate auth (required when `/RemoteServers="azure`")
- `/Database="<database name>"` - 1) Name of a single database to run against or 2) source database for scripting or runtime configuration
- `/Server="<server name>"` - 1) Name of a server for single database run or 2) source server for scripting or runtime configuration

#### General Runtime settings 

Arguments needed when performing a database update build\
Applies to: `/Action={Build|Threaded|Batch|Remote}`
- `/PackageName="<filename>"` - Name of the .sbm or .sbx file to execute
- `/RootLoggingPath="<directory>` - Directory to save execution logs (for threaded, batch and remote executions)
- `/Trial=(true|false)` - Whether or not to run in trial mode (i.e. will automatically rollback changes - default is `false`)
- `/Transactional=(true|false)` - Whether or not to run with a wrapping transaction (default is `true`)
- `/Override=("<filename>"|list)` - File containing the target database settings (usually a formatted .cfg file)
- `/Description="<description>"` - A free form description for the execution run. Must be enclosed in quotes if it contains spaces
- `/LogAsText=(true|false) ` - Whether to log as text or HTML. (default is `true` to log as text)
- `/BuildRevision="<rev identifier>"`  - If provided, the build will include an update to a "Versions" table and this will be the value used to add to a "VersionNumber" column (varchar(max))
- `/LogToDatabaseName="<db name>"` - [Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target). 
- `/ScriptSrcDir="<directory>"` - [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)
- `/ScriptLogFile="<filename>"` - [Not recommended] Alternate name for the file containing the script run log
- `/RootLoggingPath="<directory>"` - Directory to

#### Azure Batch Execution (/Action=Batch)


In addition to the authentication and runtime arguments above, these are arguments specifically needed for Azure Batch executions\
See detailed Batch [documentation](AzureBatch.md#azure-batch-execution-actionbatch)


#### Azure Batch Pre-Stage Batch nodes (/Action=BatchPreStage)

See detailed Batch [documentation](AzureBatch.md#azure-batch---pre-stage-batch-nodes-actionbatchprestage)
	
#### Azure Batch Clean Up (delete) nodes (/Action=BatchCleanUp)

See detailed Batch [documentation](AzureBatch.md#azure-batch-clean-up-delete-nodes-actionbatchcleanup)

#### Remote Execution settings (/Action=Remote)
- `/RemoteServers=("<filename>"|derive|azure)` - Pointer to file that contains the list of remote execution servers, "derive" to parse servers from DB list, azure to use Azure PaaS instances
- `/TimeoutRetryCount=(integer)` - Number of times to retry os a script timeout is encountered (default is 0)
- `/DistributionType=(equal|local)` - Sets whether to split execution evenly across all execution servers or have each agent only run against its local databases. Local not supported with RemoteServers="azure" 
- `/TestConnectivity=(true|false)` - True value will test connection to remote agent and databases but will not execute SQL scripts
- `/AzureRemoteStatus=true` - Return a status of the Azure remote execution services. Will not execute SQL 
- `/RemoteDbErrorList=<remote name>|all` - Returns the list of databases that has execution errors in the last run. Use "all" to get list from all remote servers
- `/RemoteErrorDetail=<server:db>|<instance>|all` - Retrieves the error detail for each instance in error. Is "server:db" is provided, full log detail is returned

#### Dacpac Execution

When using a source DACPAC file `/PlatinumDacpac` or creating one one at run time (`/PlatinumDbSource`, `/PlatinumServerSource` along with `/Server` and `/Database`).\
Applies to: `/Action={Threaded|Batch|Remote}`
- `/PlatinumDacpac="<filename>"` - Name of the dacpac containing the platinum schema
- `/TargetDacpac="<filename>"` - Name of the dacpac containing the schema of the database to be updated
- `/ForceCustomDacPac=(true|false)` - *USE WITH CAUTION! (`/Action={Threaded|Batch}` only) This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.
- `/PlatinumDbSource="<database name>"` - Instead of a formally built Platinum Dacpac, target this database as having the desired state schema
- `/PlatinumServerSource="<server name>"` - Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema

#### Save Settings (/Action=SaveSettings)
This utility action will save a reusable Json file to make running the command line easier (especially for Batch processing). To save the settings, create the command line as you normally would, and set the `/Action=SaveSettings` and add a `/SettingsFile="<file path>`" argument. 

The next time you run a build action, use the `/SettingsFile="<file path>` in place of the arguments below.

- Authentication: `/UserName`, `/Password`
- Azure Batch: `BatchNodeCount`, `/BatchAccountName`, `/BatchAccountKey`, `/BatchAccountUrl`, `/StorageAccountName`, `/StorageAccountKey`, `/BatchVmSize`, `/DeleteBatchPool`, `/DeleteBatchJob`, `/PollBatchPoolStatus`
- Run time settings: `/RootLoggingPath`, `/LogAsText`

_Note:_ 
1. the values for `/UserName`, `/Password`, `/BatchAccountKey` and `/StorageAccountKey` will be encrypted. 
2. If there are duplicate values in the `/SettingsFile` and the command line, the command line argument will take precedence. 

#### Script Extraction from Dacpac (/Action=ScriptExtract)
- `/PlatinumDacpac="<filename>"` - Name of the dacpac containing the platinum schema
- `/OutputSbm="<filename>"` - Name (and path) of the SBM package to create

#### Database Synchronization (/Action={Synchronize|GetDifference})
- `/GoldDatabase="<database name>"` - The "gold copy" database that will serve as the model for what the /Database should look like
- `/GoldServer="<server name>"` - The server that the "gold copy" database can be found
- `/ContinueOnFailure=(true|false)` - Whether or not to continue on the failure of a package. Default is false. 

#### SBX to SBM packaging (/Action=Package)
- `/Directory="<directory>"` - Directory containing 1 or more SBX files to package into SBM zip files
   
#### Policy checking (/Action=PolicyCheck)
- `/PackageName="<filename>"` - Name of the SBM package to check policies on

#### Calculate hash/fingerprint (/Action=GetHash)
- `/PackageName="<filename>"` - Name of the SBM package to calculate SHA-1 hash value
   
#### Creating backout packages (/Action=CreateBackout)
- `/PackageName="<filename>"` - Name of the SBM package to calculate a backout package for
- `/Server=<servername>` - Name of the server that contains the desired state schema to "backout to"
- `/Database=<databasename>` - Name of the database that contains the desired state schema to "backout to"

 # Logging

For general logging, the
SqlBuildManager.Console.exe has its own local messages. This log file is
named SqlBuildManager.Console.log and can be found in the same folder as
the executable. This file will be the first place to check for general
execution errors or problems.

To accommodate the logging of the actual build, all of the output is
saved to files and folders under the path specified in
the `/RootLoggingPath` flag. For a simple threaded execution, this is a
single root folder. For a remote server execution, this folder is
created for each execution server.

**Working folder**

This folder is where the contents of the .SBM file are extracted. This
file is extracted only once and loaded into memory for the duration of
the run to efficiently use memory.

**Commits.log**

Contains a list of all databases that the build was committed on. This
is a quick reference for each location that had a successful execution.

**Errors.log**

Contains a list of all databases that the build failed on and was rolled
back. This is a quick reference for all locations that had failures.

**Server/Database folders**

For each server/database combination that was executed, a folder
structure is created for each server and a subfolder in those for each
database. Inside each database level folder will be three files:

- `LogFile-\<date,time\>.log`: This is the script execution log for the
database. It contains the actual SQL scripts that were executed as well
as the return results of the execution. This file is formatted as a SQL
script itself and can be used manually if need-be.

- `SqlSyncBuildHistory.xml`: the XML file showing run time meta-data
details on each script file as executed including run time, file hash,
run order and results.

- `SqlSyncBuildProject.xml`: the XML file showing the design time
meta-data on each script file that defined the run settings, script
creation user ID's and the committed script record and hash for each.
	