
# Command line Usage

**Tip**: If you don't like typing the full name of the exe you can easily create an alias with the files in the *Utility* folder:
- Edit the *alias.bat* file with the path to where you have the SqlBuildManager.Console.exe file. 
- Edit the *command_alias.reg* file to the path where you save the *alias.bat* file.
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
- `Remote` - deprecated - use `Batch` instead] For massively parallel updating of databases simultaneously using remote execution servers deployed as Azure classic cloud services 

##### Utility actions 
- `BatchPreStage` - Pre-stage Azure Batch VM nodes
- `BatchCleanUp` - Deletes Azure Batch VM nodes
- `Package` - Create an SBM package from an SBX configuration file and scripts
- `PolicyCheck` - Performs a script policy check on the specified SBM package
-  `GetHash` - Calculates the SHA-1 hash fingerprint value for the SBM package (scripts + run order)
- `CreateBackout` - Generates a backout package (reversing stored procedure and scripted object changes)
- `GetDifference` - Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between /Database and /GoldDatabase
- `Syncronize` - Performs a database syncronization between between /Database and /GoldDatabase
- `ScriptExtract` - Extract a SBM package from a source `/PlatinumDacPac`

#### General Authentication settings 

Used to provide authentication to databases during a build or for script extraction/ DACPAC creation\
Applies to: `/Action={Build|Threaded|Batch|Remote|GetDifference|Syncronize`
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

In addition to the authentication and runtime arguments above, these are specifically needed for Azure Batch executions

*Note:* either /PlatinumDacpac _or_ /PackageName are required. If both are given, then /PackageName will be used.
- `/PlatinumDacpac="<filename>"` - Name of the dacpac containing the platinum schema
- `/PackageName="<filename>"` - Name of the .sbm or .sbx file to execute save execution logs 
- `/DeleteBatchPool=(true|false)` - Whether or not to delete the batch pool servers after an execution (default is `false`)
- `/DeleteBatchJob=(true|false)` - Whether or not to delete the batch job after an execution (default is `true`)
- `/BatchNodeCount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `/BatchVmSize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]
- `/BatchAccountName="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `/BatchAccountKey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `/BatchAccountUrl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `/StorageAccountName="<storage acct name>"` - Name of storage account associated with the Azure Batch account  [can also be set via StorageAccountName app settings key]
- `/StorageAccountKey="<storage acct key>"` - Account Key for the storage account  [can also be set via StorageAccountKey app settings key]
- `/BatchJobName="<name>"` - [Optional] User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed

#### Azure Batch Pre-Stage Batch nodes (/Action=BatchPreStage)
- `/BatchNodeCount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `/BatchVmSize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]
- `/BatchAccountName="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `/BatchAccountKey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `/BatchAccountUrl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `/PollBatchPoolStatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)
	
#### Azure Batch Clean Up (delete) nodes (/Action=BatchCleanUp)
- `/BatchAccountName="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `/BatchAccountKey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `/BatchAccountUrl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `/PollBatchPoolStatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

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

 
	