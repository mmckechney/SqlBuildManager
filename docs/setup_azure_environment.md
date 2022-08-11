# Setting Up an Azure Environment

- [Getting started - Building Azure Resources](#getting-started---building-azure-resources)
- [Notes on Unit Testing](#notes-on-unit-testing)
- [SQL Express for local testing](#sql-express)
- [Visual Studio Installer Project](#Visual-studio-installer-project)

----

## Getting started - Building Azure Resources

To get started leveraging Batch, Kubernetes, Container Apps or Azure Container Instance, you first need to create and configure resources in the Azure cloud. To automate as much of this as possible, there are PowerShell scripts in the `/scripts/templates` folder to leverage. See the associated [Readme](../scripts/templates/README.md) for full descriptions

**NOTE:** Before using these scripts, you will need to install both the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) and Kubernetes CLI (`az aks install-cli`) installed on your machine.

### Steps

1. In a PowerShell window, navigate to the `scripts/templates` folder
2. Run the `az login` command to connect to your Azure account
3. Run the `create_azure_resources.ps1` file. You will be prompted for parameters:
    - `-resourcePrefix` - the prefix for the Azure resources. Must be 6 or less characters, all lowercase.
    - `-Location` - the Azure region to deploy the resources (you can get a list of available locations by running `az account list-locations -o tsv --query "[].name"`)
    - `-outputPath` - optional, will default to the `/scr/TestConfig` folder, the location that the integration tests will look for configuration files 
    - `-testDatabaseCount` - optional, default is 10. If 0, then the SQL Azure resources will not be created. If you plan on running any integration tests, this number should be greater than 1.

    There are additional optional parameters you can set:

    - `-build` - whether or not to build the `sbm` command line code in the `sbm.csproj` file and upload it to the Azure Batch account
    - `deployBatch` -  whether or not to build and Azure Batch environment. If you don't plan on using Batcj, you can set this to `$false`
    - `-deployAks` - whether or not to build the AKS cluster. If you don't plan on using Kubernetes, you can set this to `$false`
    - `-includeAci` - whether or not to include settings for Azure Container Instances (ACI). If you don't plan on using ACI, you can set this to `$false`
    - `-deployContainerAppEnv` - whether or not to build the an Azure Container App environment . If you don't plan on using Container Apps, you can set this to `$false`
    - `-deployContainerRegistry` - whether or not to create a private Azure Container Registry. This is recommended! To use public images,  you can set this to `$false`

### What does the script do?

The `create_azure_resources.ps1` script will create the following resources which will be ready to start processing your database builds once it's complete. Each resources is prefixed with, you guessed it, the value you provided in the `-resourcePrefix` argument

- **Storage Account** (`{prefix}storage`) - this account is used for all of the runtime logs files and a staging location for the Kubernetes build package
- **Service Bus Namespace and Topic** (`{prefix}servicebus` and `sqlbuildmanager` respectively) - this is the Topic where the database target messages are sent and used by both Batch and Kubernetes
- **EventHub Namespace and EventHub** (`{prefix}eventhubnamespace` and `{prefix}eventhub` respectively) - used for progress event tracking in Kubernetes and can also be used for Batch
- **Key Vault** (`{prefix}keyvault`) - used to store the secrets to access the storage account, service bus, event hub and databases at runtime
- **Managed Identity** (`{prefix}identity`) - the identity used by both Kubernetes and Batch to access the secrets in the Key Vault
- **AKS Cluster** (`{prefix}aks`) - a managed Kubernetes cluster with 2 worker nodes for running Kubernetes pods database builds. You can increase the worker node count as needed.
- **Batch Account** (`{prefix}batchacct`) - a Batch account used to process database builds. Pre-configured with two applications `SqlBuildManagerLinux` and `SqlBuildManagerWindows` that have the local build of the console app uploaded to each respective OS target. Also pre-configured to use the Managed Identity
- **Container App Environment** - an environment where individual container apps representing a build will be deployed
- **Azure Container Registry** - a private container registry for the SQL Build Manager images. Images will be automatically build with three tags: The current date, the code version as defined in [AssemblyVersion.cs](../src/AssemblyVersioning.cs) and `latest-vNext` (this last one is used in the integration tests found in  [`SqlBuildManager.Console.ExternalTest`](https://github.com/mmckechney/SqlBuildManager/tree/master/src/SqlBuildManager.Console.ExternalTest)
- 2 **Azure SQL Servers** (`{prefix}sql-a` and `{prefix}sql-b`) each with `-testDatabaseCount` number of databases.These can be used for integration testing from  [`SqlBuildManager.Console.ExternalTest`](https://github.com/mmckechney/SqlBuildManager/tree/master/src/SqlBuildManager.Console.ExternalTest)

In addition to creating the resources above it will create the following files in the `outputPath` location folder. These are used by the [`SqlBuildManager.Console.ExternalTest`](https://github.com/mmckechney/SqlBuildManager/tree/master/src/SqlBuildManager.Console.ExternalTest) project (which is also a great place to look to see execution examples):

1. `settingsfile-batch*.json` - batch settings files that contains all of the SQL, Batch, Storage and Service Bus endpoints and connection keys for use in testing. There will also be two files ending with `-keyvault.json` that will not contain any secrets, but will instead contain the Key Vault name. The secrets will also have been saved to the Key Vault.
2. `settingsfile-aci-*.json` - settings files for ACI builds. This will not contain secrets as ACI will always leverage Key Vault.
3. `settingsfile-containerapp-*.json` - settings files for Container App builds
4. `settingsfile-k8s-*.json` - settings files for Kubernetes builds 
5. `settingsfilekey.txt` - a text file containing the encryption key for the settings files
6. `databasetargets.cfg` - a pre-configured database listing file for use by the integration tests that use an SBM file as a script source 
7. `clientdbtargets.cfg` - a pre-configured database listing file for use by the integration tests that use a DACPAC as a script source

**IMPORTANT:** These files can be used _as is_ for the integration testing but are also great reference examples of how to create your own files for production use

---
## Notes on Unit Testing

**NOTE: There are currently some concurrency issues with the unit tests. You may get some failures in a full run that will then succeed after running aain, selecting only the failed tests** 

There are three types of Tests included in the solution:

1. True unit tests with no external dependency - found in the  `~UnitTest.csproj` projects
2. Those that are dependent on a local SQLEXPRESS database - found in the `~.Dependent.UnitTest.csproj` projects. If you want to be able to run the database dependent tests, you will need to install SQL Express as per the next section.
3. Integration tests that leverage Azure resources for Batch and Kubernetes. These are found in the `SqlBuildManager.Console.ExternalTest.csproj` project. To run these tests, first run  [`create_azure_resources.ps1`](../scripts/templates/create_azure_resources.ps1) with the -`testDatabaseCount` value >1 (the default is 10). This will create the necessary resources and test config files (in `/src/TestConfig` folder) needed to run the tests.

## SQL Express

In order to get some of the unit tests to succeed, you need to have a local install of SQLExpress. You can find the installer from here [https://www.microsoft.com/en-us/sql-server/sql-server-editions-express] (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express). You should be able to leverage the basic install.

---
## Visual Studio Installer Project
For Visual Studio 2015 and beyond, you will need to install an extension to load the installer project (.vdproj)

### Visual Studio 2022
https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2022InstallerProjects

### Visual Studio 2017 and 2019
https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects

### Visual Studio 2015
https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9


If you are having trouble with the installer project loading try disable extension "Microsoft Visual Studio Installer Projects", reenable, then reload the projects.

