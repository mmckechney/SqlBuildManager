# SQL Build Manager

SQL Build Manager is a multi-faceted tool to allow you to manage the life-cycle of your databases. It provides a comprehensive set of command line options for the management from one to many thousands of databases.

![.NET Core Build](https://github.com/mmckechney/SqlBuildManager/workflows/.NET%20Core%20Build/badge.svg)

#### _Be sure to review the [change log](CHANGELOG.md) for the latest updates, enhancements and bug fixes_

---
### **Key feature enhancement with Version 14.4+: Expanded use of Azure User Assigned Managed Identity**

With this update, it significantly reduces the the need to save and manage secrets and connection strings. For full details on leveraging Managed Identity to connect to the other Azure resources such as SQL Database, Blob storage, Service Bus, Event Hub, Key Vault and Azure Container registry, see the [Managed Identity documentation here](/docs/managed_identity.md).

---

## Contents

- [Important Concepts!](#important-concepts)
- [Key Features - Why use SQL Build Manager?](#key-features)
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

---

## Important Concepts

Below are some high level concepts used by SQL Build Manager. You will see these used through out the documents and how-to's so it is important to understand what they mean:

### **"Build"**

The action of updating to your database or fleet of databases with SQL Build Manager. Your build is wrapped in an all-or-nothing transaction meaning if a script fails, your database will be rolledback to the state it was prior to your build. The app also maintains a build history in the database by adding a logging table to the database.

### **"Package"**

The bundling of your scripts for updating your databases. In addition to managing the order of script execution, it also manages meta-data about the scripts such as a desription, the author and unique id. The package also maintains hash values of each script and the package as a whole to ensure the integrity and allow tracking. Details on package contents and creation can be found [here](docs/package.md)

### **"Override" file**

The list of servers/databases you want to update with a build. When a package is created, the scripts are assigned a default database named "client". The override file is a list of the SQL Server Name and database targets that will replace "client" at build time. This file is in a format of: `{sql server name}:client,{target database}` with one target per line. This is either used directly or used as a source to create Azure Service Bus messages that are leveraged in a build. See [here](docs/override_options.md) for additional information.

### **Remote Build Execution**

The ability to distribute the load across multiple compute nodes. There are four options with SQL Build Manager: Azure Batch, Azure Kubernetes Service, Azure Container Apps and Azure Container Instance. Each of these has some specific configuration required, but have simliar process steps. The concept of parallel remote build is outlined [here](docs/massively_parallel.md) with specifics for each options are available.

### **"Settings" file**

A configuration file that can be saved and re-used across multiple builds. It saves configurations for your remote envionment, identities, container names,  connection strings, etc. Any sensitive information is encrypted with AES265 encryption with the value you provide with the `--settingsfilekey`. Sensitive information can instead be stored in Azure Key Vault with a `--keyvault` parameter if desired.

### **"jobname"**

The name of a build. This is used as the name or name prefix for all of the Azure services used in a remote build including the blob storage container, running docker containers, service bus topic, etc.

---

## Key Features

- Packaging of all of your update scripts and runtime meta-data into a single .sbm (zip file) or leverage data-tier application ([DACPAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/deploy-a-data-tier-application)) deployment across your entire database fleet.
- Massively parallel execution across thousands of databases utilizing local threading or an [Azure Batch, Kubernetes, Container Apps or Container Instance remote execution](docs/massively_parallel.md)
- Single transaction handling. If any one script fails, the entire package is rolled back, leaving the database unchanged.
- Handle multiple database updates in one package - seamlessly update all your databases with local threading or massively parallel remote processing.
- Full script run management. Control the script order, target database, and transaction handling
- Trial mode - runs scripts to test against database, then rolls back to leave in pristine state.
- Automatic logging and version tracking of scripts on a per-server/per-database level
- Full SHA-1 hashing of individual scripts and complete `.sbm` package files to ensure integrity of the scripts
- Execution of a build package (see below) is recorded in the database for full tracking of update history, script validation and potential rebuilding of packages

---

## Running Builds (command line)

There are 5 ways to run your database update builds each with their target use case

### **Local**

Leveraging the `sbm build` command, this runs the build on the [current local machine](docs/local_build.md). If you are targeting more than one database, the execution will be serial, only updating one database at a time and any transaction rollback will occur to all databases in the build.

### **Threaded**

Using the `sbm threaded run` command will allow for updating multiple databases in [parallel](docs/threaded_build.md), but still executed from the local machine. Any transaction rollbacks will occur per-database - meaning if 5 of 6 databases succeed, the build will be committed on the 5 and rolled back only on the 6th

### **Batch**

Using the `sbm batch run` command leverages Azure Batch to permit massively parallel updates across thousands of databases. To leverage Azure Batch, you will first need to set up your Batch account. The instructions for this can be found [here](docs/azure_batch.md).
An excellent tool for viewing and monitoring your Azure batch accounts and jobs can be found here [https://azure.github.io/BatchExplorer/](https://azure.github.io/BatchExplorer/)

### **Azure Container Apps**

Using the `sbm containerapp run` commands leverages Azure Container Apps to permit massively parallel updates across thousands of databases. Learn how to use Container Apps [here](docs/containerapp.md).

### **Kubernetes**

Using the `sbm k8s run` commands leverages Kubernetes to permit massively parallel updates across thousands of databases. To leverage Kubernetes, you will first need to set up a Kubernetes Cluster. The instructions for this can be found [here](docs/kubernetes.md#).

### **Azure Container Instance (ACI)**

Using the `sbm aci` commands leverages Azure Container Instance to permit massively parallel updates across thousands of databases. Learn how to use ACI [here](docs/aci.md).

---

## Querying across databases (command line)

In addition to using SQL Build Manager to perform database updates, you can also run SELECT queries across all of your databases to collect data. In the case of both `threaded` and `batch` a consolidated results file is saved to the location of your choice

### Threaded

Using the `sbm threaded query` command will allow for querying multiple databases in parallel, but still executed from the local machine.

### Batch

Using the `sbm batch query` command leverages Azure Batch to permit massively parallel queries across thousands of databases. (For information on how to get started with Azure Batch, go [here](docs/azure_batch.md))
