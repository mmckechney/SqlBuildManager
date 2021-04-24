# Set up your Azure Resources

To use Azure Batch, you will need to have an [Azure Subscription](https://azure.microsoft.com/) and several Azure resources including, of course, an Azure Batch account. Once this is done you will need to upload the SQL Build Manager binaries as a zip file. You can use either a pre-built [GitHub release](https://github.com/mmckechney/SqlBuildManager/releases/latest) or a local build. This setup is a one-time event and can be done via scripts or manually via the Azure portal.

To steam line the deployment, you can use the instructions below.

## Overview

This deployment process will get everything ready for you to start leveraging Azure Batch for our SQL Build Manager deployments. The script will:

- Register the Azure Resource Providers for Storage, Batch, EventHub and Service Bus
- Create a Resource Group to contain all of the resources
- Use the [azuredeploy.json](templates/azuredeploy.json) ARM template to create 
  - Storage Account - to save the consolidated log files generated from the builds
  - Batch Account - the resource that will run the builds
  - Event Hub - used as streaming log service to monitor the builds (this is an optional service and can be removed if not needed)
  - Service Bus Namespace and Topic - used as a source for the database target list (this is an optional service and can be removed if you do not want to leverage Service Bus and instead leverage an `--override` database target file
- Compile the `SqlBuildManager.Console.csproj` targeting .NET 5.0 to produce the `sbm` executable (it will build two sets of binaries, one targeting Windows and one for Linux)
- Create a Zip file of each package (Windows and Linux), upload them to the Azure Batch account and register each as a Batch Application
- Call the [Create_SettingsFile.ps1](templates/Create_SettingsFile.ps1) PowerShell to collect the accounts names, keys and connection strings and save them to four JSON files `settingsfile-windows.json`,  `settingsfile-linux.json`, `settingsfile-windows-queue.json` and `settingsfile-linux-queue.json`. The files with the `-queue` suffix have the Service Bus Topic connection value set, the others do not.\
**NOTE**:  these files will have the account secrets in _plain text_. It is highly recommended that you don't skip the `savesettings` command as below

## Deployment

0. _Prerequisite_: Make sure you have the [Azure PowerShell Modules installed](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps)
1. Clone the SQL Build Manager [Git repository](https://github.com/mmckechney/SqlBuildManager.git) locally, then in a PowerShell window navigate to the `docs/templates` folder

``` bash
git clone https://github.com/mmckechney/SqlBuildManager.git
cd SqlBuildManager/docs/templates
```

2. Run the `deploy_azure_resources.ps1` file, providing values for:
    - `-subscriptionId` - GUID for the subscription to deploy to
    - `-resourceGroupName` - Resource group name to put the Batch and storage accounts into
    - `-resourceGroupLocation` - Azure region for the accounts. You can get the location values via the PowerShell `Get-AzLocation | Select-Object -Property Location`
    - `-batchprefix` - **up to 6** characters that will be used to create the resource names (keep in mind the [rules for naming storage accounts](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview#naming-storage-accounts))
    - `-outputpath` - a directory path where the project build output zip files will be saved. (These Zip files are then uploaded to the Batch account)

``` PowerShell
Connect-AzAccount
.\deploy_batch.ps1 -subscriptionId <your sub GUID> -resourceGroupName <resource group name> -resourceGroupLocation <location> -batchprefix <prefix> -outputpath <local path>
```

3. Encrypt the settings files

``` bash
sbm batch savesettings --settingsfile <path>\settingsfile-windows.json --settingsfilekey <16 characters or more>
sbm batch savesettings --settingsfile <path>\settingsfile-linux.json --settingsfilekey <16 characters or more>
sbm batch savesettings --settingsfile <path>\settingsfile-windows-queue.json --settingsfilekey <16 characters or more>
sbm batch savesettings --settingsfile <path>\settingsfile-linux-queue.json --settingsfilekey <16 characters or more>
```

**Important:** Don't forget your settings file key! This will be used at runtime to decrypt the file and access the keys and connection strings. If you do lose the setting file key, you can run the `Create_SettingsFile.ps1` directly to re-created it and `sbm batch savesettings` command to encrypt it.

