# Leveraging Azure Batch for distributed builds

## Set up your Batch Account
To use Azure Batch, you will need to have an Azure Batch account and upload the Sql Build Manager code package zip file (either from a [GitHub release](https://github.com/mmckechney/SqlBuildManager/releases) or a custom build) to the account. This setup is a one-time event and can be done via scripts or manually via the Azure portal


 ### **Option 1: PowerShell and ARM Template**
_Note_: this method has the added benefit of also uploading the [latest release zip file](https://github.com/mmckechney/SqlBuildManager/releases/latest) to your batch account so it is ready to go

0. _Prerequisite_: Make sure you have the [Azure PowerShell Modules installed](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps)
1. Download the files `deploy_batch.ps1` and `azuredeploy.parameters.json` files from the [templates](templates) folder
2. Edit the `azuredeploy.parameters.json` file, giving your Azure Batch and Azure Storage account names (keep in mind the [rules for naming storage accounts](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview#naming-storage-accounts))
3. Copy the URL for the [latest release package](https://github.com/mmckechney/SqlBuildManager/releases/tag/v11.0.0) `SqlBuildManager-vX.X.X.zip`
4. Run the `deploy_batch.ps1` file, providing values for:
    - `-subscriptionId` - Guid for the subscription to deploy to
    - `-resourceGroupName` - Resource group name to put the Batch and storage accounts into
    - `-resourceGroupLocation` - Azure region for the accounts. You can get the location values via the PowerShell `Get-AzLocation | Select-Object -Property Location`
    - `-sbmReleaseUrl` - Url to latest release Zip file

Sample script

``` powershell
.\deploy_batch.ps1 -subscriptionId <your sub GUID> -parametersFilePath azuredeploy.parameters.json -resourceGroupName myresourcegrp -resourceGroupLocation EastUs -sbmReleaseUrl https://github.com/mmckechney/SqlBuildManager/releases/download/v11.0.0/SqlBuildManager-v11.0.0.zip

```


Assuming the script succeeds, the last few lines will provide pre-populated arguments that you can save for your command line execution (You can also retrieve this data at a later time from the [Azure portal](#option-3-manually-via-azure-portal)):

```text
Pre-populated command line arguments. Record these for use later:

/BatchAccountName=mybatchaccountname
/BatchAccountUrl=https://mybatchaccountname.eastus.batch.azure.com
/BatchAccountKey=CLHvqpOqRW2X+z6Z2G/25zY9sQn/ePLMRknX1EbA79AJ74UVLV/7X1HqE91xV0UF24fPJYZfqDM/cfU6c1lPTA==
/StorageAccountName=mystorageaccountname
/StorageAccountKey=deDGkC2D3eOzI2BiVVmrxVpP1PPf7AdllA89HRYRAxD703iM/Me4D815aNYJTan8xiRypmfQ7QxCnZhM7QlYog==
```

### **Option 2: Direct ARM Template deployment**
This is by far the easiest way to deploy the require resources!

Use the "Deploy to Azure" button to deploy using the template via the Azure portal. You will need to collect the account information from the resources once created the same as step 7 [here](#option-3-manually-via-azure-portal)

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmmckechney%2FSqlBuildManager%2Fmaster%2Fdocs%2Ftemplates%2Fazuredeploy.json" target="_blank">
    <img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.png"/></a>

<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fmmckechney%2FSqlBuildManager%2Fmaster%2Fdocs%2Ftemplates%2Fazuredeploy.json" target="_blank">
<img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/visualizebutton.png"/></a>



### **Option 3: Manually via Azure Portal**

Login to the Azure Portal at <http://portal.azure.com>

#### Create Batch Account

1. Click "Create a Resource"
2. Search for "Batch Service"
3. On the information page, select "Create"
4. Fill out the new Batch, Storage Account and Event Hub information and click "Create"
5. Wait for the deployment to complete (should only take a few minutes)
6. Collect Azure Batch, Storage Account and Event Hub information
    - Create a new text document to capture the information you will collect
    - In the Azure Portal, navigate to your new Batch account
    - On the Keys blade, record the Batch Account, URL and Primary access key values
    - On the Storage Account blade, record the Storage Account Name and Key1 values

#### Create Event Hub

1. Click "Create a Resource"
2. Search for "Event Hubs"
3. On the information page, select "Create"
4. Fill out the new information for the new Event Hub
5. Navigate to the new Event Hub Namespace you created
6. Click on the "Event Hubs" link
7. Click on "+ Event Hub" and create it
8. On the new Event Hub blade, click on "Shared Access Policy" and create a new policy with "Send" and "Listen" permissions
9. Record the  "Connection string–primary key" value

----

## Upload SQL Build Manager code package

1. First, make sure you have a build of SQL Build Manager either from [GitHub Release](https://github.com/mmckechney/SqlBuildManager/releases) or built locally (clone/pull from [Github](https://github.com/mmckechney/SqlBuildManager) and build, the executables will be in the root bin/debug or bin/release folder)
2. Zip up all of the files in the build folder - or grab the latest release Zip file from [here](https://github.com/mmckechney/SqlBuildManager/releases/latest)
3. In the Azure Portal, navigate to your Azure Batch account and click the "Applications" blade.
4. Click the "+ Add" link
5. Fill in the Application Id with "sqlbuildmanager" (no quotes) and the version field (can be any alpha-numeric) 
6. Use the folder icon to select your zip file that contains the compiled binaries
7. Click the "OK" button - this will upload your zip package and it will now show up in the Application list


----

## Running a Batch execution

(for a full end-to-end example, see [this document](./AzureBatchExample.md))

Azure Batch builds are started locally via `SqlBuildManager.Console.exe`. This process communicates with the Azure Storage account and Azure Batch account to execute in  parallel across the pool of Batch compute nodes. The number of nodes that are provisioned is determined by your command line arguments.

### Recommended order of execution:
 
 #### Pre-stage the Azure Batch pool VMs

Execute SqlBuildManager.Console.exe with the `/Action=BatchPreStage` directive. This will create the desired number of Azure Batch VM's ahead of time\
(_NOTE:_ it can take 10-20 minutes for the VMs to be provisioned and ready). See the argument details [here](#azure-batch---pre-stage-batch-nodes-actionbatchprestage)

#### Execute batch build

 Execute `SqlBuildManager.Console.exe` with the `/Action=batch` directive. See the argument details [here](#azure-batch-execution-actionbatch)

This will start the following process:

1. The provided command line arguments are validated for completeness
2. The target database list is split into pieces for distribution to the compute nodes
3. The resource files are uploaded to the Storage account 
4. The workload tasks are send to each Batch node
5. The local executable polls for node status, waiting for each to complete
6. Once complete, the aggregate return code is used as the exit code for `SqlBuildManager.Console.exe` 
7. The log files for each of the nodes is uploaded to the Storage account associated with the Batch
8. A Sas token URL to get read-only access to the log files is included in the console output. You can also view these files via the Azure portal or the [Azure Batch Explorer](https://azure.github.io/BatchExplorer/)

#### Inspect logs if an issue is reported

If there is a issue with the execution - either with the SQL updates or something with the program, logs will be created. See the [log details](#Log-details) to see what files to expect.

#### Cleanup post build

1. Execute SqlBuildManager.Console.exe with the `/Action=BatchCleanup` directive. This will delete the Azure Batch VM's so you are no longer charged for the compute. See the argument details [here](#azure-batch-clean-up-delete-nodes-actionbatchcleanup)\
_NOTE:_ this will not delete the log files, these are generally needed more long term and they will stay in the storage account

### Alternative run options

If you prefer a one step execution, you can run the command line to create and delete the pool VMs in-line with your execution. To do this, you would use the `/Action=Batch` action argument along with the [additional arguments](#Additional-arguments) to create and delete the pool

## Examples

The following command contains all of the required arguments to run a Batch job:

```
SqlBuildManager.Console.exe /Action=Batch /override="C:\temp\override.cfg" /PackageName=c:\temp\mybuild.sbm /username=myname /password=P@ssw0rd! /DeleteBatchPool=false /BatchNodeCount=5 /BatchVmSize=STANDARD_DS1_V2 /BatchAccountName=mybatch /BatchAccountUrl=https://mybatch.eastus.batch.azure.com /BatchAccountKey=x1hGLIIrdd3rroqXpfc2QXubzzCYOAtrNf23d3dCtOL9cQ+WV6r/raNrsAdV7xTaAyNGsEagbF0VhsaOTxk6A== /StorageAccountName=mystorage /StorageAccountKey=lt2e2dr7JYVnaswZJiv1J5g8v2ser20B0pcO0PacPaVl33AAsuT2zlxaobdQuqs0GHr8+CtlE6DUi0AH+oUIeg==
```

The following command line uses a generated DACPAC and assumes that the Batch,  Storage and password settings are in the `/SettingsFile`:

```
SqlBuildManager.Console.exe /Action=batch /SettingsFile="C:\temp\my_settings.json" /override="C:\temp\override.cfg" /PlatinumDbSource="platinumDb" /PlatinumServerSource="platinumdbserver" /database=targetDb /server="targetdbserber" 
```

----
## Azure Batch - Pre-Stage Batch nodes (/Action=BatchPreStage)

_Note:_ You can also leverage the [SettingsFile](commandline.md#save-settings-actionsavesettings) option to reuse most of the arguments
- `/BatchNodeCount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `/BatchVmSize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]
- `/BatchAccountName="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `/BatchAccountKey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `/BatchAccountUrl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `/PollBatchPoolStatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

----

## Azure Batch Execution (/Action=Batch)

In addition to the [authentication](commandline.md#General-Authentication-settings) and [runtime](commandline.md#General-Runtime-settings) arguments above, these are specifically needed for Azure Batch executions.
\
_Note:_ 
1. You can also leverage the [SettingsFile](commandline.md#save-settings-actionsavesettings) option to reuse most of the arguments
2. either /PlatinumDacpac _or_ /PackageName are required. If both are given, then /PackageName will be used.

- `/PlatinumDacpac="<filename>"` - Name of the dacpac containing the platinum schema
- `/PackageName="<filename>"` - Name of the .sbm or .sbx file to execute save execution logs 
- `/BatchAccountName="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `/BatchAccountKey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `/BatchAccountUrl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `/StorageAccountName="<storage acct name>"` - Name of storage account associated with the Azure Batch account  [can also be set via StorageAccountName app settings key]
- `/StorageAccountKey="<storage acct key>"` - Account Key for the storage account  [can also be set via StorageAccountKey app settings key]
- `/BatchJobName="<name>"` - [Optional] User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed

#### Additional arguments 

If you don't run the `/Action=BatchPreStage`  and `Action=BatchCleanup` command sequence you will need to use the following:
- `/DeleteBatchPool=(true|false)` - Whether or not to delete the batch pool servers after an execution (default is `false`)
- `/DeleteBatchJob=(true|false)` - Whether or not to delete the batch job after an execution (default is `true`)
- `/BatchNodeCount="##"` - Number of nodes to provision to run the batch job  (default is 10)
- `/BatchVmSize="<size>"` - Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]

----

## Azure Batch Clean Up (delete) nodes (/Action=BatchCleanUp)

_Note:_ You can also leverage the [SettingsFile](commandline.md#save-settings-actionsavesettings) option to reuse most of the arguments
- `/BatchAccountName="<batch acct name>"` - String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]
- `/BatchAccountKey="<batch acct key>"` - Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]
- `/BatchAccountUrl="<batch acct url>"` - URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]
- `/PollBatchPoolStatus=(true|false)` - Whether or not you want to get updated status (true, default) or fire and forget (false)

----

## Log Details

The logs will be stored in the Azure Storage account associated with the Batch account. You can view the logs in several ways, including the the [Azure portal](http://portal.azure.com), [Azure Batch Explorer](https://azure.github.io/BatchExplorer/) and [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/). However you view the logs, you will find a set of files and folders:

### Files

_Note:_ All of these files are consolidated logs from across all of your Batch nodes.
- `successdatabases.cfg` - this file contains a list of all datbases that were successfully updated. The file is in the format used as an `/Override` argument. If there were no successful updates, this file will not be created.
- `failuredatabases.cfg` - this file contains a list of all datbases that were successfully updated. The file is in the format used as an `/Override` argument. If there were no failures, this file will not be created.
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