# Package (aka Build Package)

Contents:

- [Package and meta-data](#package-and-meta-data)
- [Creating a Package](#creating-a-package)
  - [Forms UI](#forms-ui)
  - [Command Line](#command-line)

---

## Package and meta-data
At the core of the process is the "SQL Build Manager Package" file (.sbm extension).  Under the hood, this file is a Zip file that contains the scripts that constitute your "build" along with a configuration file  (SqlSyncBuildProject.xml) that contains meta data on the scripts and execution parameters:

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

## Creating a Package

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

**_NOTE:_** The `sbm scriptextract` method has been deprecated in favor of `sbm create fromdacpacdiff`.