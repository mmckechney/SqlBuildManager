# Leveraging Azure Batch for distributed builds

## Set up your Batch Account

To use Azure Batch, you will need to have an Azure Batch account and upload the Sql Build Manager code package zip file (either from a [GitHub release](https://github.com/mmckechney/SqlBuildManager/releases/latest) or a custom build) to the account. This setup is a one-time event and can be done via scripts or manually via the Azure portal

_Note_: this will upload a build of the latest code version from a local build

1. _Prerequisite_: Make sure you have the [Azure PowerShell Modules installed](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps)
2. Clone the Git repo locally, then in a PowerShell window navigate to the `docs/templates` folder
3. Run the `deploy_batch.ps1` file, providing values for:
    - `-subscriptionId` - Guid for the subscription to deploy to
    - `-resourceGroupName` - Resource group name to put the Batch and storage accounts into
    - `-resourceGroupLocation` - Azure region for the accounts. You can get the location values via the PowerShell `Get-AzLocation | Select-Object -Property Location`
    - `-batchprefix` - up to 6 characters that will be used to create the resource names (keep in mind the [rules for naming storage accounts](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview#naming-storage-accounts))
    - `-outputpath` - a directory path where the project build output zip files will be saved. (These Zip files are then uploaded to the Batch account)

Sample script

``` powershell
Connect-AzAccount

.\deploy_batch.ps1 -subscriptionId <your sub GUID> -resourceGroupName myresourcegrp -resourceGroupLocation EastUs -batchprefix sbmzyx -outputpath "C:\temp"
```

Assuming the script succeeds, you can then create a settings file with the  resources URLs and keys that can be used to for your Batch runs (You can also retrieve this data manually from the [Azure portal](#option-3-manually-via-azure-portal)):

```powershell
.\Create_SettingsFile.ps1 -subscriptionId <your sub GUID>  -resourceGroupName myresourcegrp -batchprefix sbmzyx -settingsFileName myfile.json
```

----

## Upload a SQL Build Manager code package

_Note_: a local build code package was uploaded via the `deploy_batch.ps1`.

**You can re-run `deploy_batch.ps1` at any time (using the same parameter values) to easily update the code package**

But, if you want to do it manually... 

1. First, make sure you have a build of SQL Build Manager either from [GitHub Release](https://github.com/mmckechney/SqlBuildManager/releases) or built locally:
    - Clone/pull from [Github](https://github.com/mmckechney/SqlBuildManager)
    - Build locally either in Visual Studio or via command line:

      ```bash
        cd ./src/SqlBuildManager.Console
        dotnet restore sbm.csproj
        dotnet build sbm.csproj
        dotnet publish sbm.csproj -r [win-x64 or linux-x64] --configuration [Debug or Release]
      ```

2. Zip up all of the files in the publish folder - or grab the latest release Zip file from [here](https://github.com/mmckechney/SqlBuildManager/releases/latest)
3. In the Azure Portal, navigate to your Azure Batch account and click the "Applications" blade.
4. Click the "+ Add" link
5. Fill in the Application Id with "sqlbuildmanager" (no quotes) and the version field (can be any alpha-numeric) 
6. Use the folder icon to select your zip file that contains the compiled binaries
7. Click the "OK" button - this will upload your zip package and it will now show up in the Application list


----

## Running a Batch execution

(for a full end-to-end example, see [this document](./AzureBatchExample.md))

Azure Batch builds are started locally via `sbm.exe`. This process communicates with the Azure Storage account and Azure Batch account to execute in  parallel across the pool of Batch compute nodes. The number of nodes that are provisioned is determined by your command line arguments.

### Recommended order of execution:
 
#### 1. Pre-stage the Azure Batch pool VMs

Execute `sbm batch prestage [options]`. This will create the desired number of Azure Batch VM's ahead of time\
(_NOTE:_ it can take 10-20 minutes for the VMs to be provisioned and ready). See the argument details [here](#azure-batch---pre-stage-batch-nodes)

#### 2. Execute batch build

 Execute `sbm batch run [options]`. See the argument details [here](#azure-batch-execution)

This will start the following process:

1. The provided command line arguments are validated for completeness
2. The target database list is split into pieces for distribution to the compute nodes
3. The resource files are uploaded to the Storage account 
4. The workload tasks are send to each Batch node
5. The local executable polls for node status, waiting for each to complete
6. Once complete, the aggregate return code is used as the exit code for `sbm` 
7. The log files for each of the nodes is uploaded to the Storage account associated with the Batch
8. A Sas token URL to get read-only access to the log files is included in the console output. You can also view these files via the Azure portal or the [Azure Batch Explorer](https://azure.github.io/BatchExplorer/)

#### 3. Inspect logs if an issue is reported

If there is a issue with the execution - either with the SQL updates or something with the program, logs will be created. See the [log details](#Log-details) to see what files to expect.

#### 4. Cleanup post build

1. Execute `sbm batch cleanup [options]`. This will delete the Azure Batch VM's so you are no longer charged for the compute. See the argument details [here](#azure-batch-clean-up-delete-nodes)\
_NOTE:_ this will not delete the log files, these are generally needed more long term and they will stay in the storage account

### Alternative run options

If you prefer a one step execution, you can run the command line to create and delete the pool VMs in-line with your execution. To do this, you would use `sbm batch run` along with the [additional arguments](#Additional-arguments) to create and delete the pool

## Examples

The following command contains all of the required arguments to run a Batch job:

``` bash
sbm.exe batch run --override="C:\temp\override.cfg" --packagename="c:\temp\mybuild.sbm" --username=myname --password=P@ssw0rd! --deletebatchpool=false --batchnodecount=5 --batchvmsize=STANDARD_DS1_V2 --batchaccountname=mybatch --batchaccounturl=https://mybatch.eastus.batch.azure.com --batchaccountkey=x1hGLIIrdd3rroqXpfc2QXubzzCYOAtrNf23d3dCtOL9cQ+WV6r/raNrsAdV7xTaAyNGsEagbF0VhsaOTxk6A== --storageaccountname=mystorage --storageaccountkey=lt2e2dr7JYVnaswZJiv1J5g8v2ser20B0pcO0PacPaVl33AAsuT2zlxaobdQuqs0GHr8+CtlE6DUi0AH+oUIeg==
```

The following command line uses a generated DACPAC and assumes that the Batch,  Storage and password settings are in the [`--settingsfile`](#azure-batch-save-settings):

``` bash
sbm.exe batch run --settingsfile="C:\temp\my_settings.json" --override="C:\temp\override.cfg" --platinumdbsource="platinumDb" --platinumserversource="platinumdbserver" --database=targetDb --server="targetdbserver" 
```

----

## Azure Batch - Pre-Stage Batch nodes

`sbm batch prestage [options]`\
_Note:_ You can also leverage the [--settingsfile](#azure-batch-save-settings) option to reuse most of the arguments

- `--batchnodecount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `--batchvmsize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]
- `--batchaccountname="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `--batchaccountkey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `--batchaccounturl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `--pollbatchpoolstatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

----

## Azure Batch Execution

`sbm batch run [options]`\
In addition to the [authentication](commandline.md#General-Authentication-settings) and [runtime](commandline.md#General-Runtime-settings) arguments above, these are specifically needed for Azure Batch executions.
\
_Note:_

1. You can also leverage the [--settingsfile](#azure-batch-save-settings) option to reuse most of the arguments
2. either `--platinumdacpac` _or_ `--packagename` are required. If both are given, then `--packagename` will be used.

- `--platinumdacpac="<filename>"` - Name of the dacpac containing the platinum schema
- `--packagename="<filename>"` - Name of the .sbm or .sbx file to execute save execution logs 
- `--batchaccountname="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `--batchaccountkey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `--batchaccounturl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `--storageaccountname="<storage acct name>"` - Name of storage account associated with the Azure Batch account  [can also be set via StorageAccountName app settings key]
- `--storageaccountkey="<storage acct key>"` - Account Key for the storage account  [can also be set via StorageAccountKey app settings key]
- `--batchjobname="<name>"` - [Optional] User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed

#### Additional arguments 

If you don't run the `sbm batch prestage`  and `sbm batch cleanup [options]` command sequence you will need to use the following:

- `--deletebatchpool=(true|false)` - Whether or not to delete the batch pool servers after an execution (default is `false`)
- `--deletebatchjob=(true|false)` - Whether or not to delete the batch job after an execution (default is `true`)
- `--batchnodecount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `--batchvmsize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]

----

## Azure Batch Clean Up (delete) nodes

`sbm batch cleanup [options]`\
_Note:_ You can also leverage the [--settingsfile](#azure-batch-save-settings) option to reuse most of the arguments

- `--batchaccountname="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `--batchaccountkey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `--batchaccounturl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `--pollbatchpoolstatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

----

## Azure Batch Save Settings

`sbm batch savesettings [options]`\
This utility action will save a reusable Json file to make running the command line easier for Batch processing.

The next time you run a build action, use the `--settingsfile="<file path>"` in place of the arguments below.

- Authentication: `--username`, `--password`
- Azure Batch: `--batchnodecount`, `--batchaccountname`, `--batchaccountkey`, `--batchaccounturl`, `--storageaccountname`, `--storageaccountkey`, `--batchvmsize`, `--deletebatchpool`, `--deletebatchjob`, `--pollbatchpoolstatus`, `--eventhubconnectionstring`
- Run time settings: `--rootloggingpath`, `--logastext`, `--concurrency`, `--concurrencytype`

_Note:_ 

1. the values for `--username`, `--password`, `--batchaccountkey`, `--storageaccountkey` and  `--eventhubconnectionstring` will be encrypted.
2. If there are duplicate values in the `--settingsfile` and the command line, the command line argument will take precedence. 
3. You can hand-craft the Json yourself in the [format below](#settings-file-format) but the password and keys will not be encrypted (which may be OK depending on where you save the files)

----
## Log Details

The logs will be stored in the Azure Storage account associated with the Batch account. You can view the logs in several ways, including the the [Azure portal](http://portal.azure.com), [Azure Batch Explorer](https://azure.github.io/BatchExplorer/) and [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/). However you view the logs, you will find a set of files and folders:

### Files

_Note:_ All of these files are consolidated logs from across all of your Batch nodes.

- `successdatabases.cfg` - this file contains a list of all databases that were successfully updated. The file is in the format used as an `--override` argument. If there were no successful updates, this file will not be created.
- `failuredatabases.cfg` - this file contains a list of all databases that were successfully updated. The file is in the format used as an `--override` argument. If there were no failures, this file will not be created.
- `commits.log` - a log file showing successful updates with time stamps
- `errors.log` -  a log file showing failed updates with time stamps, statuses and return codes

### Folders

- `Task##` - There will be one folder for each worker node VM. The contents of this folder will be the standard output and standard error logs for the executable from that VM.
- `Working` - this is the folder that contains the runtime files such as the DACPAC, SBM and distributed database configuration files
- `<server name>` folders - There is one folder per target SQL Server. Within each of these is a folder for each target database. 
    - `<database name>` folder - within these folders are three files
        - `LogFile-\<date,time\>.log` -  a detailed script by script run result
        - `SqlSyncBuildHistory.xml` - detailed log along with script meta-data (such as start/end times, file hash, status, user id)
        - `SqlSyncBuildProject.xml` - meta-data file for the script package run against the database

## Troubleshooting tips

If you have SQL errors in your execution, you will probably want to figure out what happened. Here is a suggested troubleshooting path:

1. Open up the `failuredatabases.cfg` file to see what databases had problems
2. Taking note of the server and database name, open the server folder then the database folder
3. Open the `logfile` in the database folder. This file should contain an error message that will guide your troubleshooting should you need to correct some scripts
4. Once you have determined the problem, use the `failuredatabases.cfg` file as your `/Override` argument to run your updates again - hopefully successfully this time!

----

## Settings File Format

The format for the saved settings Json file is below. You can include or exclude any values that would like. Also as a reminder, for any duplicate keys found in the settings file and command line arguments, the command line argument's value will be used.

```json
{
  "AuthenticationArgs": {
    "UserName": "<database use name>",
    "Password": "<database password>"
  },
  "BatchArgs": {
    "BatchNodeCount": "<int value>",
    "BatchAccountName": "<batch account name>",
    "BatchAccountKey": "<key for batch account ",
    "BatchAccountUrl": "<https URL for batch account>",
    "StorageAccountName": "<storage account name>",
    "StorageAccountKey": "<storage account key>",
    "BatchVmSize": "<VM size designator>",
    "DeleteBatchPool": "<true|false>",
    "DeleteBatchJob": "<true|false>",
    "PollBatchPoolStatus": "<true|false>",
    "EventHubConnectionString": "<connection string to EventHub (optional)>",
    "BatchPoolName": "[SqlBuildManagerPoolWindows or SqlBuildManagerPoolLinux]",
    "BatchPoolOs": "[Windows or Linux]",
    "ApplicationPackage": "<name of the application package to use>"
  },
  "RootLoggingPath": "<valid folder path>",
  "DefaultScriptTimeout" : "<int>",
  "TimeoutRetryCount" : "<int>",
  "Concurrency": "<int value>",
  "ConcurrencyType": "[Count | Server | MaxPerServer]"
}
```