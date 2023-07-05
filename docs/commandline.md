
# Command Line Overview

- [Getting started](#getting-started)
- [Logging](#logging)

----

## Getting started

The `sbm` executable uses a command pattern for execution `sbm [command]`

**For detailed information on the available and required options for each command, leverage the self-generated documentation via `sbm [command] --help`**
## `build`

Performs a standard, local SBM execution via command line


## `threaded`

For updating multiple or querying databases simultaneously from the current machine

  - `query` - Run a SELECT query across multiple databases
  - `run` - For updating multiple databases simultaneously from the current machine

## `containerapp`

Commands for setting and executing a build running in pods on Azure Container App

  - `savesettings` - Saves settings file for Azure Container App deployments
  - `run` - Runs a build on Container Apps (orchestrates the prep, enqueue, deploy and montitor commands)
  - `prep` - Creates an Azure storage container and uploads the SBM and/or DACPAC files that will be used for the build.
  - `enqueue` - Sends database override targets to Service Bus Topic
  - `deploy` - Deploy the Container App instance using the template file created from 'sbm containerapp prep' and start containers
  - `monitor` - Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
  - `dequeue` - Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
  - `worker` - [Used by Container Apps] Starts the pod as a worker - polling and retrieving items from target service bus queue topic
  - `worker test` - Create environment variables for Container app and run local execution

## `k8s`

Commands for setting and executing a build running in pods on Kubernetes

  - `savesettings` - Saves settings file for Kubernetes deployments
  - `run` - Run a build in Kubernetes (Orchestrates the prep, enqueue and monitor commands as well as kubectl). [NOTE: 'kubectl' must be installed and in your path]
  - `prep` - Creates a storage container and uploads the SBM and/or DACPAC files that will be used for the build. If the --runtimefile option is provided, it will also update that file with the updated values
  - `enqueue` - Sends database override targets to Service Bus Topic
  - `monitor` - Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
  - `dequeue` - Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
  - `createyaml` - Helper command to create yaml files from a settings json file and runtime parameters
  - `query` - Run a SELECT query across multiple databases using Kubernetes. [NOTE: 'kubectl' must be installed and in your path]
  - `worker` - [Used by Kubernetes] Starts the pod as a worker - polling and retrieving items from target service bus queue topic
  - `worker query` - [Used by Kubernetes] Starts the pod as a worker for database querying - polling and retrieving items from target service bus queue topic

## `aci`

Commands for setting and executing a build running in containers on Azure Container Instances. ACI Containers will always leverage Azure Key Vault.

  - `savesettings` - Saves settings file for Azure Container Instances container deployments
  - `run` - Runs an ACI build (orchestrates the prep, enqueue, deploy and monitor commands
  - `prep` - Creates an Azure storage container and uploads the SBM and/or DACPAC files that will be used for the build.
  - `enqueue` - Sends database override targets to Service Bus Topic
  - `deploy` - Deploy the ACI instance and start containers
  - `monitor` - Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
  - `query` - Run a SELECT query across multiple databases using ACI.
  - `dequeue` - Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
  - `worker` - [Used by ACI] Starts the container(s) as a worker - polling and retrieving items from target service bus queue topic
  - `worker query` - [Used by ACI] Starts the container(s) as a worker for database querying - polling and retrieving items from target service bus queue topic

## `batch`

Commands for setting and executing a batch run or batch query

  - `savesettings` - Save a settings json file for Batch arguments (see Batch documentation)
  - `prestage` - Pre-stage the Azure Batch VM nodes
  - `enqueue` - Sends database override targets to Service Bus Topic
  - `run` - For updating multiple databases simultaneously using Azure batch services
  - `query` - Run a SELECT query across multiple databases using Azure Batch
  - `cleanup` - Azure Batch Clean Up - remove VM nodes
  - `dequeue` - Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them

## `utility`

Utility commands for generating override file from SQL statement and interrogating Service Bus and EventHubs

  - `override` - Generate an override file from a SQL script. Specify either --scriptfile or --scripttext.
  - `queue` - Retrieve the number of messages currently in a Service Bus Topic Subscription
  - `eventhub` - Retrieve the number of messages in the EventHub for a specific job run.

## `create`

Creates an SBM package from script files (fromscripts),  calculated database differences (fromdiff) or diffs between two DACPAC files (fromdacpacs)

  - `fromscripts` - Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension- .sbm or .sbx)
  - `fromdiff` - Creates an SBM package from a calculated diff between two databases
  - `fromdacpacs` - Creates an SBM package from differences between two DACPAC files
  - `fromdacpacdiff` - Extract a SBM package from a source --platinumdacpac and a target database connection

## `add`

Adds one or more scripts to an SBM package or SBX project file from a list of scripts


## `package`

Creates an SBM package from an SBX configuration file and scripts


## `unpack`

Unpacks an SBM file into its script files and SBX project file.


## `dacpac`

Creates a DACPAC file from the target database


## `createbackout`

Generates a backout package (reversing stored procedure and scripted object changes)


## `list`

List the script contents (order, script name, date added/modified, user info, script ids, script hashes) for SBM packages. (For SBX, just open the XML file!)


## `policycheck`

Performs a script policy check on the specified SBM package


## `gethash`

Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)


## `getdifference`

Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between --database and --golddatabase. Only supports Windows Auth


## `synchronize`

Performs a database synchronization between between --database and --golddatabase. Can only be used for Windows Auth database targets


## `showcommands`

Creates export of all command and sub-command descriptions



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

### For for details and script run troubleshooting suggestions, see [Log Files Details for Threaded, Batch,  Kubernetes and ACI execution](threaded_and_batch_logs.md)