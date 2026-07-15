# Leveraging Azure Batch for database builds

- [Why use Azure Batch?](#why-use-azure-batch)
  - [Process Flow](massively_parallel.md#process-flow-details)
- [Getting Started](#getting-started)
- [Build or update the SQL Build Manager container](#build-or-update-the-sql-build-manager-container)
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
2. [Build the SQL Build Manager container](#build-or-update-the-sql-build-manager-container). If
   you ran `azd up`, the image was already built remotely in ACR.
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

## Build or update the SQL Build Manager container

Azure Batch runs SQL Build Manager in a Linux container. Application packages are not used because
they are incompatible with the firewalled/private-only linked Storage account.

`azd up` builds the runtime image remotely with ACR Tasks:

```text
<registry>.azurecr.io/sqlbuildmanager:latest-vNext
```

To rebuild only the runtime image:

```powershell
.\scripts\ContainerRegistry\build_runtime_image_fromprefix.ps1 `
  -prefix <prefix> `
  -resourceGroupName <prefix>-rg `
  -wait $true
```

The Batch pool uses its user-assigned managed identity to pull the image from ACR. No registry
password is required.

----

## Running a Batch Build

(For a full end-to-end example, see [this document](./azure_batch_example.md))

Azure Batch builds are started locally via `sbm.exe`. This process communicates with the Azure
Storage account and Azure Batch account to execute Linux container tasks across the pool of Batch
compute nodes. The number of nodes that are provisioned is determined by your command line
arguments. The initiating machine must have network access to the private Storage endpoint to stage
input files.

### Settings File

While all of the values can be provided as arguments in the command line, it is strongly suggested you leverage `--settingsfile` and `--settingsfilekey` (the `--settingsfilekey` parameter is not required if you are leveraging `--keyvaultname`). A settings JSON file is created for you when running `azd up` (see [Setting up an Azure Environment](setup_azure_environment.md)), but can also be re-created via the `scripts/create_batch_settingsfiles_mi_only.ps1` script.

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

1. Validate the provided command line arguments and Linux container image settings
2. The target database list is split into pieces for distribution to the compute nodes (only if using the `--override` argument and not using Service Bus)
3. The resource files are uploaded to the Storage account
4. A container-enabled AlmaLinux 8 Gen1 pool pulls the runtime image from ACR with managed identity
5. The workload tasks are sent to Azure Batch and run `/app/sbm` inside the container
6. The local executable polls for node status, waiting for each to complete
7. Once complete, the aggregate return code is used as the exit code for `sbm`
8. The log files for each of the nodes is uploaded to the Storage account associated with the Batch
9. A SaS token URL to get read-only access to the log files is included in the console output. You can also view these files via the Azure portal or the [Azure Batch Explorer](https://azure.github.io/BatchExplorer/)

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

