# SQL Build Manager

SQL Build Manager is a multi-faceted tool to allow you to manage the life-cycle of your databases. It provides a comprehensive set of command line options for the management from one to many thousands of databases.

![.NET Core Build](https://github.com/mmckechney/SqlBuildManager/workflows/.NET%20Core%20Build/badge.svg)

### **Important changes in Version 14+:**

There are three new options to massively parallel processing: [Azure Container Apps](docs/containerapps.md), [Kubernetes](docs/kubernetes.md) and [Azure Container Instance](docs/aci.md)!

 [Batch node pools](docs/massively_parallel.md) are now created with assigned Managed Identities. Because of this, the workstation running `sbm` _needs to have a valid Azure authentication token_. This can be done via Azure CLI `az login`, Azure PowerShell `Connect-AzAccount`, or if running from an automation box, ensure that the machine itself has a Managed Identity that has permissions to create Azure resources. Alternatively, you can pre-create the batch pools manually via the Azure portal, being sure to assign the correct Managed Identity to the pool.

[Kubernetes](docs/massively_parallel.md#kubernetes-process-flow) and [Azure Container Instance](docs/massively_parallel.md#azure-container-instance-process-flow) also require local machine authentication in order to access Azure Key Vault. Authentication is not needed for [local](local_build.md) or [threaded builds](docs/threaded_build.md)

The keys, connection strings and passwords can now be stored in Azure Key Vault rather than saving the encrypted values in a settings file or being passed in via the command line. Regardless if you use Batch, Kubernetes or ACI , this integration is enabled by leveraging [User Assigned Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-manage-user-assigned-managed-identities). To easily accomplish this setup, there are a set of PowerShell scripts in the [`scripts/templates` folder](scripts/templates). A complete environment can be created with [`create_azure_resources.ps1`](scripts/templates/create_azure_resources.ps1). Please note that Azure Key Vault is required for [Azure Container Instance](docs/aci.md) builds.

You will also need to be logged into Azure if you are leveraging Azure Key Vault to store your secrets, regardless if you are using [Azure Batch](docs/massively_parallel.md#batch-process-flow), [Kubernetes](docs/massively_parallel.md#kubernetes-process-flow) or [Azure Container Instance](docs/massively_parallel.md#azure-container-instance-process-flow))

---

## Contents

- [Key Features](#key-features)
- [The Basics](#the-basics)
  - [Build package meta-data](#build-package-meta-data)
  - [Creating a build package](#creating-a-build-package)
  - [Targeting multiple databases](#targeting-multiple-databases)
  - [Running builds](#running-builds-command-line)
  - [Querying across databases](#querying-across-databases-command-line)
- [Command Line Reference/ Quickstart](docs/commandline.md)
- [Running Locally](docs/local_build.md)
- [Massively Parallel Database Builds](docs/massively_parallel.md)
  - [Azure Container Apps](docs/containerapp.md)
  - [Azure Batch](docs/azure_batch.md)
  - [Kubernetes](docs/kubernetes.md)
  - [Azure Container Instances (ACI)](docs/aci.md)
- [Change log](CHANGELOG.md)
- For contributors: [Notes on Building and Unit Testing](docs/setup_azure_environment.md)
- For users of the Windows Form app: [SQL Build Manager Manual](docs/SqlBuildManagerManual.md)\
  (Note: this isn't 100% up to date, so the screen shots may vary from the current app)

----

## Key Features

- Packaging of all of your update scripts and runtime meta-data into a single .sbm (zip file) or leverage data-tier application ([DACPAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/deploy-a-data-tier-application)) deployment across your entire database fleet.
- Single transaction handling. If any one script fails, the entire package is rolled back, leaving the database unchanged.
- Handle multiple database updates in one package - seamlessly update all your databases with local threading or massively parallel Azure Batch processing.
- Full script run management. Control the script order, target database, and transaction handling
- Trial mode - runs scripts to test against database, then rolls back to leave in pristine state.
- Automatic logging and version tracking of scripts on a per-server/per-database level
- Full SHA-1 hashing of individual scripts and complete `.sbm` package files to ensure integrity of the scripts
- Execution of a build package (see below) is recorded in the database for full tracking of update history, script validation and potential rebuilding of packages
- Massively parallel execution across thousands of databases utilizing local threading or an [Azure Batch or Kubernetes remote execution](docs/massively_parallel.md)

# The Basics

## Build Package meta-data
At the core of the process is the "SQL Build Manager Package" file (.sbm extension).  Under the hood, this file is a Zip file that contains the scripts that constitute your "release" along with a configuration file  (SqlSyncBuildProject.xml) that contains meta data on the scripts and execution parameters:

- `FileName`: Self explanatory, the name of the script file
- `BuildOrder`: The relative order that the scripts in the package will be run
- `Description`: Optional description about the script
- `RollBackOnError`: Option on whether or not to roll back the transaction if there is an error running this script (default: `true`)
- `CausesBuildFailure`: Option on whether or not to roll back the entire build if there is a failure with this script (default `true`)
- `DateAdded`: Date and time that the script was added to the package
- `ScriptId`: System generated GUID identifier for the script
- `Database`: Target database to run the scripts against. (This can be overridden in the case of multiple DB targets)
- `StripTransactionText`: Script handling to remove any inline transaction statements (default is `true` because you want SQL Build Manager to handle transactions)
- `AllowMultipleRuns`: Whether or not this script can be run on the same database multiple times (default is `true` and you should always write scripts so this is viable)
- `AddedBy`: User ID of the user that added the script to the package
- `ScriptTimeOut`: Timeout setting for the execution of this script
- `DateModified`: If the script has been modified after being added, this will be populated (otherwise `DateTime.Min`)
- `ModifiedBy`: If the script has been modified after being added, this will be populated with the user's ID
- `Tag`: Optional tag for the script

If you are using a DACPAC deployment, this all gets generated for you based on your command line parameters and defaults

Example `SqlSyncBuildProject.xml` file. You can build this by hand to create your own `.sbm` file or leverage the options below (recommended).

``` xml
<?xml version="1.0" standalone="yes"?>
<SqlSyncBuildData xmlns="http://schemas.mckechney.com/SqlSyncBuildProject.xsd">
  <SqlSyncBuildProject ProjectName="" ScriptTagRequired="false">
    <Scripts>
      <Script FileName="select.sql" BuildOrder="1" Description="Testing select script" RollBackOnError="true" CausesBuildFailure="true" DateAdded="2019-04-11T19:45:05.081043-04:00" ScriptId="14f775d2-d026-426b-bece-7faa323e0e14" Database="Client" StripTransactionText="true" AllowMultipleRuns="true" AddedBy="mimcke" ScriptTimeOut="500" DateModified="0001-01-01T00:00:00-05:00" ModifiedBy="" Tag="default" />
    </Scripts>
    <Builds />
  </SqlSyncBuildProject>
</SqlSyncBuildData>
```

----

## Creating a Build Package

### Forms UI

While the focus of the app has changed to command line automation, the forms GUI is fully functional. If you are looking for a visual tool, check out _SqlBuildManager.exe_. There is documentation on the GUI that you can find [here](docs/SqlBuildManagerManual.md) that will walk through the creation of build packages ([PDF version](src/SqlBuildManager%20Manual/SqlBuildManagerManual.pdf)).

### Command line

There are several ways to create a build package from the command line.  Which you choose depends on your starting point:

[Command line reference](docs/commandline.md)

1. From various sources using `sbm create`. There are four sub-commands to help create an SBM package:

   - `fromscripts` - Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension: .sbm or .sbx)
   - `fromdiff` - Creates an SBM package from a calculated diff between two databases (you provide the server and database names, and it connects them them and generates the diff scripts)
   - `fromdacpacs` - Creates an SBM package from differences between two DACPAC files (use DACPACs you have created elsewhere or use `sbm dacpac` to create them)
   - `fromdacpacdiff`- This method leverages a DACPAC that was created against your "Platinum Database" (why platinum? because it's even more precious than gold!). The Platinum database should have the schema that you want all of your other databases to look like.

   Learn more about DACPACs and [data-tier applications](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications?view=sql-server-2017)  method.

2. From an SBX file. What is this? An SBX file is an XML file in the format of the `SqlSyncBuildProject.xml` file (see above) that has an `.sbx` extension. When you use the `sbm package` command, it will read the `.sbx` file and create the `.sbm` file with the referenced scripts.
3. An SBM package file can be created indirectly as well, using the `sbm threaded run` and `sbm batch run` commands along with the `--platinumdbsource="<database name>"` and `--platinumserversource="<server name>"` the app will generate a DACPAC from the source database which will then be used to generate an SBM at run time to build directly on your target(s).
4. You can also add new scripts to an existing SBM package or SBX project file using `sbm add`
5. From a DACPAC file using the `sbm scriptextract` command. This method leverages a DACPAC that was created against your "Platinum Database" (why platinum? because it's even more precious than gold!). The Platinum database should have the schema that you want all of your other databases to look like. (don't have a DACPAC created, don't worry, you can create one with the `sbm dacpac` command) 

**_NOTE:_** The `sbm scriptextract` method is being deprecated in favor of `sbm create fromdacpacdiff` and will be removed in a future release

----

## Targeting multiple databases

You define your database update targets leveraging an `--override` file or using Azure Service Bus Topics (only with Azure Batch). The details of database targeting can be found [here](docs/override_options.md#database-targeting-option)

----

## Running Builds (command line)

There are 5 ways to run your database update builds each with their target use case

### **Local**

Leveraging the `sbm build` command, this runs the build on the [current local machine](docs/local_build.md). If you are targeting more than one database, the execution will be serial, only updating one database at a time and any transaction rollback will occur to all databases in the build.

### **Threaded**

Using the `sbm threaded run` command will allow for updating multiple databases in [parallel](docs/threaded_build.md), but still executed from the local machine. Any transaction rollbacks will occur per-database - meaning if 5 of 6 databases succeed, the build will be committed on the 5 and rolled back only on the 6th

### **Batch**

Using the `sbm batch run` command leverages Azure Batch to permit massively parallel updates across thousands of databases. To leverage Azure Batch, you will first need to set up your Batch account. The instructions for this can be found [here](docs/azure_batch.md).
An excellent tool for viewing and monitoring your Azure batch accounts and jobs can be found here [https://azure.github.io/BatchExplorer/](https://azure.github.io/BatchExplorer/)

### **Kubernetes**

Using the `sbm k8s` commands leverages Kubernetes to permit massively parallel updates across thousands of databases. To leverage Kubernetes, you will first need to set up a Kubernetes Cluster. The instructions for this can be found [here](docs/kubernetes.md#).

### **Azure Container Instance (ACI)**

Using the `sbm aci` commands leverages Azure Container Instance to permit massively parallel updates across thousands of databases. Learn how to use ACI [here](docs/aci.md).

----

## Querying across databases (command line)

In addition to using SQL Build Manager to perform database updates, you can also run SELECT queries across all of your databases to collect data. In the case of both `threaded` and `batch` a consolidated results file is saved to the location of your choice

### Threaded

Using the `sbm threaded query` command will allow for querying multiple databases in parallel, but still executed from the local machine.

### Batch

Using the `sbm batch query` command leverages Azure Batch to permit massively parallel queries across thousands of databases. (For information on how to get started with Azure Batch, go [here](docs/azure_batch.md))
