# Leveraging Azure Batch for distributed builds

## Set up your Batch Account (one time event)

### Create Azure Batch Account
1. Login to the Azure Portal at <http://portal.azure.com>
2. Click "Create a Resource"
3. Search for "Batch Service"
4. On the information page, select "Create"
5. Fill out the new batch and storage account information and click "Create"
6. Wait for the Azure Batch acount to be provisioned (should only take a few minutes)

### Upload SQL Build Manager code package
1. First, make sure you have a build of SQL Build Manager (if you pull from Git and build, the executables will be in the root bin/debug or bin/release folder)
2. Zip up all of the files in the build folder
3. In the Azure Portal, navigate to your Azure Batch account and click the "Applications" blade.
4. Click the "+ Add" link
5. Fill in the Application Id with "sqlbuildmanager" (no quotes) and the version field (can be any alpha-numeric) 
6. Use the folder icon to select your zip file that contains the compiled binaries
7. Click the "OK" button - this will upload your zip package and it will now show up in the Application list

### Collect Azure Batch and storage account information
1. Create a new text document to capture the information you will collect
2. In the Azure Portal, navigate to your new Batch account 
3. On the Keys blade, record the Batch Account, URL and Primary access key values
4. On the Storage Account blade, record the Storage Account Name and Key1 values

## How does Batch execution work?
Azure Batch builds are started locally via `SqlBuildManager.Console.exe`. This process communicates with the Azure Storage account and Azure Batch account to execute in  parallel across the pool of Batch compute nodes. The number of nodes that are provisioned is determined by your command line arguments.

### The order of processing is:
1. Execute `SqlBuildManager.Console.exe` with the `/Action=batch` directive
2. The provided command line arguments are validated for completeness
3. The target database list is split into pieces for distribution to the compute nodes
4. The number of requested compute nodes are provisioned by Batch (note: this can take 5-20 minutes)
5. The resource files are uploaded to the Storage account 
6. The workload tasks are send to each Batch node
7. The local executable polls for node status, waiting for each to complete
8. Once complete, the aggregate return code is used as the exit code for `SqlBuildManager.Console.exe` 
9. The log files for each of the nodes is uploaded to the Storage account associated with the Batch
10. A Sas token URL to get read-only access to the log files is included in the console output. You can also view these files via the Azure portal. 

### Command line requirements:
* `/Action=Batch` - directive to execute over Azure Batch
* `/Override` - reference to file containing target database list
* `/Username` - SQL Account username to be used to run queries
* `/Password` - SQL Account password to be used to run queries
* `/PackageName` or `/PlatinumDacpac` - the source of pre-written scripts (PackageName) or DACPAC to be used to create scripts (PlatinumDacpac)
* `/BatchNodeCount` - The number of compute nodes you want to distribute the load to
* `/DeleteBatchPool` - whether or not to delete the pool compute nodes after execution. Set to `false` if you plan on running more than one package, but don't forget to delete them afterwards via the portal!

**The following can be included as command line arguments _or_ as `<appSettings>` in the `SqlBuildManager.Console.exe.config` file** 

(If they are also included as command line arguments, the command line values will be used)

* `/BatchAccountName` - Name of the Batch account captured when creating the account 
* `/BatchAccountKey` - Access key of the Batch account captured when creating the account 
* `/BatchAccountUrl` - URL of the Batch account captured when creating the account 
* `/BatchVmSize` - The VM size to provision for the compute nodes (see <https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general> for the values)
* `/StorageAccountName` - Name of the Storage account captured when creating the account 
* `/StorageAccountKey` - Access key of the Storage account captured when creating the account 

*Note:* you can always get a command line reference by executing `SqlBuildManager.Console.exe /?`


## Examples

The following command contains all of the required arguments to run a Batch job:

`SqlBuildManager.Console.exe /Action=Batch /override="C:\temp\override.cfg" /PackageName=c:\temp\mybuild.sbm /username=myname /password=P@ssw0rd! /DeleteBatchPool=false /BatchNodeCount=5 /BatchVmSize=STANDARD_DS1_V2 /BatchAccountName=mybatch /BatchAccountUrl=https://mybatch.eastus.batch.azure.com /BatchAccountKey=x1hGLIIrdd3rroqXpfc2QXubzzCYOAtrNf23d3dCtOL9cQ+WV6r/raNrsAdV7xTaAyNGsEagbF0VhsaOTxk6A== /StorageAccountName=mystorage /StorageAccountKey=lt2e2dr7JYVnaswZJiv1J5g8v2ser20B0pcO0PacPaVl33AAsuT2zlxaobdQuqs0GHr8+CtlE6DUi0AH+oUIeg==`

The following command line assumes that the Batch and Storage settings (except for keys) are in the <appSettings>:

`SqlBuildManager.Console.exe /Action=Batch /override="C:\temp\override.cfg" /PackageName=c:\temp\mybuild.sbm /username=mmcmynamekechney /password=P@ssw0rd! /DeleteBatchPool=false /BatchNodeCount=5 /BatchAccountKey=x1hGLIIrdd3rroqXpfc2QXubzzCYOAtrNf23d3dCtOL9cQ+WV6r/raNrsAdV7xTaAyNGsEagbF0VhsaOTxk6A== /StorageAccountKey=lt2e2dr7JYVnaswZJiv1J5g8v2ser20B0pcO0PacPaVl33AAsuT2zlxaobdQuqs0GHr8+CtlE6DUi0AH+oUIeg==`
