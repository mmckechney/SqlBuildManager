﻿# Leveraging Azure Batch for database builds

- [Why use Azure Batch?](#why-use-azure-batch)
  - [Process Flow](massively_parallel.md#batch-process-flow)
- [Getting Started](#getting-started)
- [Upload or Update SQL Build Manager binaries](#upload-or-update-sql-build-manager-binaries)
- [Running a Batch Build](#running-a-batch-build)
- [Alternative run options](#alternative-run-options)
- [Examples](#examples)
- [Log Details](#log-details)

----

## Why use Azure Batch?

If you have a fleet of databases to update, it could take a very long time to run your build on a single machine, even if you leverage the [threaded](threaded_build.md) model. Similar to leveraging [Kubernetes](kubernetes.md) or [Azure Container Instance](aci.md), to ensure you can complete your updates in a timely fashion, SQL Build Manager can target Azure Batch to distribute you build across multiple compute nodes, each leveraging their own set of concurrent tasks. You can control the level of concurrency to maximize throughput while not overloading your SQL Servers (see [details on concurrency management](concurrency_options.md))

To leverage Azure Batch, you will need an [Azure subscription](https://azure.microsoft.com/) with several Azure resources deployed.

## Get Started

1. Set Up you Azure resources. For this, leverage the automation script as explained [here](massively_parallel.md)
2. [Upload SQL Build Manager binaries](#upload-or-update-sql-build-manager-binaries) to Batch - if you ran the scripts from step 1, you have already uploaded a local build!
3. [Run a Batch build](#running-a-batch-build) 

    - [Settings File](#settings-file) - simplify your command line by saving the most re-used arguments in an encrypted JSON file
    - [Pre-stage your Batch Nodes](#1-pre-stage-the-azure-batch-pool-vms)
    - [Queue the Database Targets in Service Bus](#2-queue-the-database-targets)
    - [Execute batch build](#3-execute-batch-build)
    - [Inspect logs if an issue is reported](#4-inspect-logs-if-an-issue-is-reported)
    - [Cleanup resources post build](#5-cleanup-post-build)

4. Additional information

    - [Alternative run options](#alternative-run-options)
    - [Log Details](#log-details)
    - [Troubleshooting tips](#troubleshooting-tips)

----

## Upload or Update SQL Build Manager binaries

**_Note_**: If you ran the scripts from [here](azure_batch_resources.md) to upload a local build, you can re-run  `create_azure_resources.ps1` at any time (using the same parameter values) to easily update the code package

But, if you want to do it manually:

1. First, make sure you have a build of SQL Build Manager either from [GitHub Release](https://github.com/mmckechney/SqlBuildManager/releases) or built locally:
    - Clone/pull from [Github](https://github.com/mmckechney/SqlBuildManager)
    - Build locally either in Visual Studio or via command line:

      ```bash
        cd ./src/SqlBuildManager.Console
        dotnet publish sbm.csproj -r [win-x64 or linux-x64] --configuration [Debug or Release] -f net5.0
      ```

2. Zip up all of the files in the publish folder - or grab the latest release Zip file from [GitHub](https://github.com/mmckechney/SqlBuildManager/releases/latest)
3. In the Azure Portal, navigate to your Azure Batch account and click the "Applications" blade.
4. Click the "+ Add" link
5. Fill in the Application Id with "SqlBuildManagerWindows" (no quotes) for Windows or "SqlBuildManagerLinux" for Linux and the version field (can be any alpha-numeric) 
6. Use the folder icon to select your zip file that contains the compiled binaries
7. Click the "OK" button - this will upload your zip package and it will now show up in the Application list

----

## Running a Batch Build

(For a full end-to-end example, see [this document](./azure_batch_example.md))

Azure Batch builds are started locally via `sbm.exe`. This process communicates with the Azure Storage account and Azure Batch account to execute across the pool of Batch compute nodes. The number of nodes that are provisioned is determined by your command line arguments.

### Settings File

While all of the values can be provided as arguments in the command line, it is strongly suggested you leverage `--settingsfile` and `--settingsfilekey` (the `--settingsfilekey` parameter is not required if you are leveraging `--keyvaultname`). A settings JSON file was created for you via [`create_azure_resources.ps1`](../scripts/templates/create_azure_resources.ps1) but can also be re-created for any set of Azure resources via [`create_batch_settingsfile.ps1`](../scripts/templates/create_batch_settingsfiles.ps1)

You can also build it manually by executing `sbm batch savesettings` command and providing the appropriate arguments. See the argument details [here](azure_batch_commands.md#azure-batch-save-settings)

### 1. Pre-stage the Azure Batch pool VMs

(Optional)

- Execute `sbm batch prestage [options]`. This will create the desired number of Azure Batch VM's as defined in the `--batchnodecount` argument.\
(_NOTE:_ it can take 10-20 minutes for the VMs to be provisioned and ready which is why `prestage` is recommended). See the argument details [here](azure_batch_commands.md#pre-stage-batch-nodes)

### 2. Queue the database targets

(Optional)

- Execute `sbm batch enqueue [options]`. This will create a Service Bus Topic message for each database target. The batch nodes will pull from this queue to update the database\
It is important to use the same `--concurrencytype` value here that you will use when you run the build as this settings targets the appropriate topic/subscription.\
See full details on leveraging Service Bus [here](override_options.md#service-bus-topic) \
Instead of using a Service Bus Topic, you can target your databases with the `--override` argument directly with `sbm batch run`.  

### 3. Execute batch build

 - Execute `sbm batch run [options]`. See the argument details [here](azure_batch_commands.md#batch-execution)

This will start the following process:

1. Validate the provided command line arguments for completeness
2. The target database list is split into pieces for distribution to the compute nodes (only if using the `--override` argument and not using Service Bus)
3. The resource files are uploaded to the Storage account
4. The workload tasks are send to Azure Batch and distributed to each compute node
5. The local executable polls for node status, waiting for each to complete
6. Once complete, the aggregate return code is used as the exit code for `sbm`
7. The log files for each of the nodes is uploaded to the Storage account associated with the Batch
8. A SaS token URL to get read-only access to the log files is included in the console output. You can also view these files via the Azure portal or the [Azure Batch Explorer](https://azure.github.io/BatchExplorer/)

### 4. Inspect logs if an issue is reported

- If there is a issue with the execution - either with the SQL updates or something with the program, logs will be created. See the [log details](#Log-details) to see what files to expect.
- If applicable use the `failuredatabases.cfg` file as the `--override` target for `sbm batch enqueue` (if leveraging Service Bus) or `sbm batch run` (if leveraging a local target file) to only re-run against the databases that had update issues

### 5. Cleanup post build

1. Execute `sbm batch cleanup [options]`. This will delete the Azure Batch VM's so you are no longer charged for the compute. See the argument details [here](azure_batch_commands.md#batch-clean-up-batch-nodes)\
_NOTE:_ this will not delete the log files, these are generally needed more long term and they will stay in the storage account

## Alternative run options

If you prefer a one step execution, you can run the command line to create and delete the pool VMs in-line with your execution. To do this, you would use `sbm batch run` along with the [additional arguments](azure_batch_commands.md#additional-arguments) to create and delete the pool

## Examples

The following command contains all of the required arguments to run a Batch job:

``` bash
sbm.exe batch run --override="C:\temp\override.cfg" --packagename="c:\temp\mybuild.sbm" --username=myname --password=P@ssw0rd! --deletebatchpool=false --batchnodecount=5 --batchvmsize=STANDARD_DS1_V2 --batchaccountname=mybatch --batchaccounturl=https://mybatch.eastus.batch.azure.com --batchaccountkey=x1hGLIIrdd3rroqXpfc2QXubzzCYOAtrNf23d3dCtOL9cQ+WV6r/raNrsAdV7xTaAyNGsEagbF0VhsaOTxk6A== --storageaccountname=mystorage --storageaccountkey=lt2e2dr7JYVnaswZJiv1J5g8v2ser20B0pcO0PacPaVl33AAsuT2zlxaobdQuqs0GHr8+CtlE6DUi0AH+oUIeg==
```

The following command line uses a generated DACPAC and assumes that the Batch,  Storage and password settings are in the [`--settingsfile`](#azure-batch-save-settings):

``` bash
sbm.exe batch run --settingsfile="C:\temp\my_settings.json" --settingsfilekey="C:\temp\my_keyfile.txt"--override="C:\temp\override.cfg" --platinumdbsource="platinumDb" --platinumserversource="platinumdbserver" --database=targetDb --server="targetdbserver" 
```

----
## Log Details

For details on the log files that are created during a Batch run, see the [Log Details page](threaded_and_batch_logs.md). There is also a section on [troubleshooting tips](threaded_and_batch_logs.md#troubleshooting-tips)



