
# Command line Usage

**NOTE:** For command line operations, you must use `sbm.exe`

----

## Getting started

The `sbm` executable uses a command pattern for execution `sbm [command]`


*For detailed information on the available and required options for each command, leverage the self-generated documentation via `sbm [command] --help`*

### Build execution actions to update databases

- `build` - Performs a standard, local SBM execution via command line
- `threaded` - For updating multiple databases simultaneously from the current machine
- `batch` - Commands for setting and executing a batch run

### Utility actions

- `package` - Creates an SBM package from an SBX configuration file and scripts
- `policycheck` - Performs a script policy check on the specified SBM package
- `gethash` - Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)
- `createbackout` - Generates a back out package (reversing stored procedure and scripted object changes)
- `getdifference` - Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between --database and --golddatabase. Only supports Windows Auth
- `synchronize` - Performs a database synchronization between between --database and --golddatabase. Can only be used for Windows Auth database targets
- `scriptextract` - Extract a SBM package from a source --platinumdacpac
- `dacpac` - Create a DACPAC file from the source --database and --server

### Batch sub-commands (`sbm batch [command]`)
- `savesettings` - Save a settings json file for Batch arguments (see Batch documentation)
- `prestage` - Pre-stage the Azure Batch VM nodes
- `cleanup` - Azure Batch Clean Up - remove VM nodes
- `run` - For updating multiple databases simultaneously using Azure batch services

#### For details information on running batch builds, see the Batch documentation:

- [`sbm batch run` ](AzureBatch.md#azure-batch-execution)
- [`sbm batch prestage`](AzureBatch.md#azure-batch---pre-stage-batch-nodes)
- [`sbm batch cleanup`](AzureBatch.md#azure-batch-clean-up-delete-nodes)
- [`sbm batch savesettings`](AzureBatch.md#azure-batch-savesettings)

----
 ## Logging

For general logging, the
SqlBuildManager.Console.exe has its own local messages. This log file is
named SqlBuildManager.Console.log and can be found in the same folder as
the executable. This file will be the first place to check for general
execution errors or problems.

To accommodate the logging of the actual build, all of the output is
saved to files and folders under the path specified in
the `--rootloggingpath` flag. For a simple threaded execution, this is a
single root folder. For a remote server execution, this folder is
created for each execution server.

**Working folder**

This folder is where the contents of the .SBM file are extracted. This
file is extracted only once and loaded into memory for the duration of
the run to efficiently use memory.

**Commits.log**

Contains a list of all databases that the build was committed on. This
is a quick reference for each location that had a successful execution.

**Errors.log**

Contains a list of all databases that the build failed on and was rolled
back. This is a quick reference for all locations that had failures.

**Server/Database folders**

For each server/database combination that was executed, a folder
structure is created for each server and a subfolder in those for each
database. Inside each database level folder will be three files:

- `LogFile-\<date,time\>.log`: This is the script execution log for the
database. It contains the actual SQL scripts that were executed as well
as the return results of the execution. This file is formatted as a SQL
script itself and can be used manually if need-be.

- `SqlSyncBuildHistory.xml`: the XML file showing run time meta-data
details on each script file as executed including run time, file hash,
run order and results.

- `SqlSyncBuildProject.xml`: the XML file showing the design time
meta-data on each script file that defined the run settings, script
creation user ID's and the committed script record and hash for each.
	
