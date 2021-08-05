
# Command Line Overview

- [Getting started](#getting-started)
- [Build execution actions to update or query databases](#build-execution-actions-to-update-or-query-databases)
- [Utility Actions](#utility-actions)
- [Batch sub-commands](#batch-sub-commands)
- [Kubernetes sub-commands](#kubernetes-sub-commands)
- [Azure Container Instance sub-commands](#aci-sub-commands)
- [Logging](#logging)

----

## Getting started

The `sbm` executable uses a command pattern for execution `sbm [command]`

**For detailed information on the available and required options for each command, leverage the self-generated documentation via `sbm [command] --help`**

### Build execution actions to update or query databases

- `build` - Performs a standard, local SBM execution via command line ([docs](local_build.md))
- `threaded` - For updating multiple databases simultaneously from the current machine ([docs](threaded_build.md))
- `batch` - Commands for setting and executing a batch run using Azure Batch ([docs](azure_batch.md))
- `k8s` - Commands for setting and executing a distributed run using Kubernetes ([docs](kubernetes.md))

### Utility actions

- `create` - Creates an SBM package or SBX project file from a list of supplied script files
  - `fromscripts` - Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension: .sbm or .sbx)
  - `fromdiff` - Creates an SBM package from a calculated diff between two databases
  - `fromdacpacs` - Creates an SBM package from differences between two DACPAC files
  - `fromdacpacdiff`- Extract a SBM package from a source `--platinumdacpac` and a target database connection
- `add` - Adds scripts to an existing SBM package or SBX project file
- `package` - Creates an SBM package from an SBX configuration file and scripts
- `list` - Output scripts information on SBM packages (run order, script name, date added/modified, user info, script ids, script hashes)
- `dacpac` - Create a DACPAC file from the source `--database` and `--server`
- `policycheck` - Performs a script policy check on the specified SBM package
- `gethash` - Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)
- `createbackout` - Generates a back out package (reversing stored procedure and scripted object changes)
- `getdifference` - Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between `--database` and `--golddatabase`. Only supports Windows Auth
- `synchronize` - Performs a database synchronization between between `--database` and -`-golddatabase`. Can only be used for Windows Auth database targets
- `scriptextract` - Extract a SBM package from a source `--platinumdacpac` (this command is being deprecated in favor of `sbm create fromdacpacdiff` and will be removed in a future release)

### Batch sub-commands

`sbm batch [command]`

- `savesettings` - Save a settings JSON file for Batch arguments (see Batch documentation)
- `prestage` - Pre-stage the Azure Batch VM nodes
- `cleanup` - Azure Batch Clean Up - remove VM nodes
- `run` - For updating multiple databases simultaneously using Azure batch services
- `enqueue` - to send your database targets to an Azure Service Bus Topic (see [queue docs](override_options.md#service-bus-topic))

#### For details information on running batch builds, see the Batch documentation

- [`sbm batch savesettings`](azure_batch.md#settings-file)
- [`sbm batch enqueue`](azure_batch.md#2-queue-the-database-targets)
- [`sbm batch run`](azure_batch.md#3-execute-batch-build)
- [`sbm batch prestage`](azure_batch.md#1-pre-stage-the-azure-batch-pool-vms)
- [`sbm batch cleanup`](azure_batch.md#5-cleanup-post-build)

### Kubernetes sub-commands

`sbm k8s [command]`

For examples of each, see the [Kubernetes documentation](kubernetes.md)

- `savesettings` - Saves settings to `secrets.yaml` and `runtime.yaml` files for Kubernetes pod deployments. You can also leverage [Azure Key Vault](kubernetes.md#environment-setup) to manage secrets, eliminating the local file containing passwords, connection strings, etc.
- `prep` - Creates a storage container and uploads the SBM package file that will be used for the build. If the `--runtimefile` option is provided, it will also update that file with the appropriate values
- `enqueue` - Sends database override targets to Service Bus Topic
- `monitor` - Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
- `dequeue`- Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them. This is just used for clean up is something went wrong and is not necessary in normal processing.
- `worker` - [Used by Kubernetes] Starts the pod as a worker - polling and retrieving items from target service bus topic

### ACI sub-commands

`sbm aci [command]`

For examples of each, see the [Azure Container Instance (ACI) documentation](aci.md)

- `savesettings` - Save a settings JSON file for ACI. This option always leverages [Azure Key Vault](massively_parallel.md#Steps) to manage secrets, eliminating the local file containing passwords, connection strings, etc.
- `prep` - Creates a storage container and uploads the SBM package file that will be used for the build. It will also create a customized Azure Resource Manager (ARM) template as defined by the `--outputfile` argument. This is used in the next step to create the container instances.
- `enqueue` - Sends database override targets to Service Bus Topic
- `deploy` - Uses the ARM template created in the `sbm aci prep` step to deploy and start the containers in ACI. If you leave the `--monitor` flag default, it will automatically start to `monitor` the Service Bus and Event Hub for build progress
- `monitor` - Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors). Not necessary of you leave the `sbm aci deploy` `--monitor` flag as the default `true`
- `dequeue`- Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them. This is just used for clean up is something went wrong and is not necessary in normal processing.
- `worker` - [Used by ACI] Starts the pod as a worker - polling and retrieving items from target service bus topic


----

## Logging

For general logging, the
SqlBuildManager.Console.exe has its own local messages. This log file is
named SqlBuildManager.Console{date stamp}.log and can be found in the same folder as
the executable. This file will be the first place to check for general
execution errors or problems.

To accommodate the logging of a threaded or batch build, all of the output is
saved to files and folders under the path specified in
the `--rootloggingpath` flag. For a simple threaded execution, this is a
single root folder. For a remote server execution, this folder is
created for each execution server.

### For for details and script run troubleshooting suggestions, see [Log Files Details for Threaded, Batch and Kubernetes execution](threaded_and_batch_logs.md)