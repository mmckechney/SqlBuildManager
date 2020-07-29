# SQL Build Manager

SQL Build Manager is a multi-faceted tool to allow you to manage the lifecyle of your databases. It started as a forms based application for the management of a handful of databases and then switched focus to command line automation for the management of thousands of databases.

![.NET Core Build](https://github.com/mmckechney/SqlBuildManager/workflows/.NET%20Core%20Build/badge.svg)

### [Change notes](docs/change_notes.md)

## Key Features

* Packaging of all of your update scripts and runtime meta-data into a single .sbm (zip file) or leverage data-tier application ([DACPAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/deploy-a-data-tier-application)) deployment across your entier database fleet. 
* Single transaction handling. If any one script fails, the entire package is rolled back, leaving the database unchanged.
* Handle multiple database updates in one package - seamlessly update all your databases with local threading or massively parallel Azure Batch processing.
* Full script run management. Control the order, target database, and transaction handling
* Trial mode - runs scripts to test against database, then rolls back to leave in pristine state.
* Automatic logging and version tracking of scripts on a per-server/per-database level
* Full SHA-1 hashing of individual scripts and .sbm package files to ensure integrity of the scripts
* Execution of a build package (see below) is recorded in the database for full tracking of update history, script validation and potential rebuilding of packages
* Massively parallel execution across thousands of databases utilzing local threading, an Azure cloud service deployment or an Azure Batch execution

# The Basics

## Build Package meta-data
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
* `AllowMultipleRuns`: Whether or not this script can be run on the same database multiple times (default is `true` and you should always write scripts so this is viable)
* `AddedBy`: User ID of the user that added the script to the package
* `ScriptTimeOut`: Timeout setting for the execution of this script
* `DateModified`: If the script has been modified after being added, this will be populated (otherwise `DateTime.Min`)
* `ModifiedBy`: If the script has been modified after being added, this will be populated with the user's ID
* `Tag`: Optional tag for the script

If you are using a DACPAC deployment, this all gets generated for you based on your command line parameters and defaults

Example `SqlSyncBuildProject.xml` file. You can build this by hand to create your own `.sbm` file or leverage the options below (recommended).
```
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

While the focus of the app has changed to command line automation, the forms GUI is fully functional. If you are looking for a visual tool, check out _Sql Build Manager.exe_. There is docmentation on the GUI that you can find [here](docs/SqlBuildManagerManual.md) that will walk through the creation of build packages ([PDF version](src/SqlBuildManager%20Manual/SqlBuildManagerManual.pdf)).

### Command line

The command line utility is geared more toward executing a build vs. creating the package itself. You can however extract a build package file from a DACPAC file using the `sbm scriptextract` command. This is useful if you are utilizing the recommended [data-tier application](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications?view=sql-server-2017)  method.

A DACPAC can also be created directly from a target "Platinum Database" (why platinum? because it's even more precious than gold!). Using the `sbm threaded` and `sbm batch run` commands along with the `--platinumdbsource="<database name>"` and `--platinumserversource="<server name>"` the app will generate a DACPAC from the source database then can then be used to run a build directly on your target(s).

----

## Running Builds (command line)

There are 3 ways to run your database update builds each with their target use case

### **Local**

Leveraging the `sbm build` command, this runs the build on the current local machine. If you are targeting more than one database, the execution will be serial, only updating one database at a time and any transaction rollback will occur to all databases in the build.

### **Threaded**

Using the `sbm threaded` command will allow for updating multiple databases in parallel, but still executed from the local machine. Any transaction rollbacks will occur per-database - meaning if 5 of 6 databases succeed, the build will be committed on the 5 and rolled back only on the 6th

### **Batch**

Using the `sbm batch` command leverages Azure Batch to permit massively parallel updates across thousands of databases. To leverage Azure Batch, you will first need to set up your Batch account. The instructions for this can be found [here](docs/AzureBatch.md).\
An excellent tool for viewing and monitoring your Azure batch accounts and jobs can be found here [https://azure.github.io/BatchExplorer/](https://azure.github.io/BatchExplorer/)

----

## Additional Documentation

- ### Full command line reference details: [here](docs/commandline.md)
- ### Detailed information on leveraging Azure Batch for massively parallel deployments: [here](docs/AzureBatch.md)
- ### For help on building and unit testing:  [here](docs/setup_azure_environment.md)
