
# Command Line Overview

- [Getting started](#getting-started)
- [Global Options](#global-options)
- [Commands](#commands)
- [Common Runtime Options](#common-runtime-options)
- [Event Hub Logging Options](#event-hub-logging-options)
- [Container Environment Variables](#container-environment-variables)
- [Logging](#logging)

----

## Getting started

The `sbm` executable uses a command pattern for execution `sbm [command]`

**For detailed information on the available and required options for each command, leverage the self-generated documentation via `sbm [command] --help`**

----

## Global Options

These options are available across all commands:

| Option | Description |
|--------|-------------|
| `--loglevel` | Logging level for console and log file. Values: `Trace`, `Debug`, `Information` (default), `Warning`, `Error`, `Critical` |
| `--platform` / `--databaseplatform` | Target database platform: `SqlServer` (default) or `PostgreSQL`. See [PostgreSQL docs](postgresql.md) |

----

## Commands

_**Note:** Some commands have aliases shown in parentheses._

## `build`

Performs a standard, local SBM execution via command line


## `threaded`

For updating multiple or querying databases simultaneously from the current machine

  - `query` - Run a SELECT query across multiple databases
  - `run` - For updating multiple databases simultaneously from the current machine

## `containerapp`

_Alias: `ca`_

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

_Alias: `aks`_

Commands for setting and executing a build running in pods on Kubernetes

  - `savesettings` - Saves settings file for Kubernetes deployments
  - `run` - Run a build in Kubernetes (Orchestrates the prep, enqueue and monitor commands as well as kubectl). [NOTE: 'kubectl' must be installed and in your path]
  - `prep` - Creates a storage container and uploads the SBM and/or DACPAC files that will be used for the build. If the --runtimefile option is provided, it will also update that file with the updated values
  - `enqueue` - Sends database override targets to Service Bus Topic
  - `monitor` - Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
  - `dequeue` - Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
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

_Alias: `pack`_

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

## Common Runtime Options

These options apply to `build`, `threaded run`, and the remote execution commands (`batch run`, `k8s run`, `containerapp run`, `aci run`). Use `sbm [command] --help` for the complete list for each command.

### Build Behavior

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--transactional` | bool | `true` | Whether to wrap the build in a transaction. When `true`, a failing script triggers a rollback of the entire build on that database |
| `--trial` | bool | `false` | Runs the build in trial mode — executes all scripts against the database, then rolls back, leaving the database unchanged. Useful for validating a build package without modifying data. _Note:_ `--transactional` must be `true` when using `--trial` |
| `--allowobjectdelete` | bool | `false` | When creating a package from a DACPAC comparison (`create fromdacpacs` / `create fromdacpacdiff`), controls whether scripts for deleting database objects are included |
| `--defaultscripttimeout` | int | (per-script) | Override the per-script timeout (in seconds) set during package creation. Applies to all scripts in the build |
| `--timeoutretrycount` | int | `0` | Number of retries to attempt when a script execution times out. Only valid when `--transactional=true` |
| `--buildrevision` | string | | If provided, the build writes an update to a `Versions` table, using this value for the `VersionNumber` column |
| `--description` | string | | Description of the build, recorded in the build log |

### Authentication

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--authtype` | enum | `Password` | Authentication method: `Password`, `Windows`, `ManagedIdentity`, `AzureADPassword`, `AzureADIntegrated`, `AzureADDefault`, `AzureADInteractive` |
| `--username` / `-u` | string | | Database username (required for `Password` and `AzureADPassword` auth types) |
| `--password` / `-p` | string | | Database password (required for `Password` and `AzureADPassword` auth types) |
| `--identityclientid` / `--clientid` | string | | Client ID of the Azure User Assigned Managed Identity |
| `--tenantid` | string | | Azure AD Tenant ID (optional, for explicit tenant targeting) |

### Logging & Monitoring

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--rootloggingpath` | string | current dir | Directory to save execution log files |
| `--stream` | bool | `false` | Stream real-time database commit and error events from Azure Event Hub during remote builds |
| `--eventhublogging` | flags | `EssentialOnly` | Controls the granularity of Event Hub logging. See [Event Hub Logging Options](#event-hub-logging-options) |

### Settings & Secrets

| Option | Type | Description |
|--------|------|-------------|
| `--settingsfile` | string | Path to a saved settings JSON file to load parameters from |
| `--settingsfilekey` | string | Encryption key for the settings file (min 16 characters). Can be the key string or a path to a key file. Also read from `sbm-settingsfilekey` environment variable |
| `--keyvaultname` / `--kv` | string | Name of Azure Key Vault to store/retrieve secrets |
| `--storageaccountname` | string | Azure storage account for logs and package staging |
| `--storageaccountkey` | string | Storage account access key (not required when using Managed Identity) |
| `--servicebustopicconnection` / `--sb` | string | Service Bus connection string (or namespace when using Managed Identity) |
| `--eventhubconnection` / `--eh` | string | Event Hub connection string (or `<namespace>\|<hubname>` when using Managed Identity) |

### Targeting

| Option | Type | Description |
|--------|------|-------------|
| `--override` | string | Path to the database target override file (`.cfg`, `.multiDb`, `.multiDbQ`, or `.sql`). See [Override Options](override_options.md) |
| `--packagename` / `-P` | string | Path to the `.sbm` or `.sbx` build package |
| `--jobname` | string | User-friendly name for the build job. Also used as the blob storage container name. Must be 3-41 characters, lowercase alphanumeric and dashes only |
| `--concurrency` | int | `8` | Maximum concurrent tasks. See [Concurrency Options](concurrency_options.md) |
| `--concurrencytype` | enum | `Count` | Concurrency strategy: `Count`, `Server`, `MaxPerServer`, `Tag`, `MaxPerTag` |

----

## Event Hub Logging Options

The `--eventhublogging` flag controls what data is sent to Azure Event Hub during remote builds. Multiple values can be combined by specifying the flag multiple times.

| Value | Description |
|-------|-------------|
| `EssentialOnly` | (Default) Minimal logging — only build-level commit and error events |
| `VerboseMessages` | Include verbose diagnostic messages for detailed debugging |
| `IndividualScriptResults` | Log the result of each individual script execution |
| `ConsolidatedScriptResults` | Log aggregated/consolidated script results per database |
| `ScriptErrors` | Explicitly log script execution errors to Event Hub |

**Example:** To log both individual script results and errors:

```bash
sbm batch run --eventhublogging IndividualScriptResults --eventhublogging ScriptErrors ...
```

----

## Container Environment Variables

When running as a container worker (Kubernetes, ACI, or Container Apps), runtime configuration is passed via environment variables prefixed with `Sbm_`. These are set automatically by the orchestrator and generally do not need to be configured manually.

| Environment Variable | Description |
|---------------------|-------------|
| `Sbm_JobName` | Job/build name |
| `Sbm_PackageName` | Name of the `.sbm` package to execute |
| `Sbm_DacpacName` | Name of the DACPAC (if applicable) |
| `Sbm_Concurrency` | Concurrency level |
| `Sbm_ConcurrencyType` | Concurrency strategy (`Count`, `Server`, etc.) |
| `Sbm_KeyVaultName` | Azure Key Vault name for secret retrieval |
| `Sbm_IdentityClientId` | Managed Identity client ID |
| `Sbm_IdentityName` | Managed Identity name |
| `Sbm_StorageAccountName` | Azure storage account name |
| `Sbm_StorageAccountKey` | Storage account key (when not using Key Vault) |
| `Sbm_ServiceBusTopicConnectionString` | Service Bus connection string |
| `Sbm_EventHubConnectionString` | Event Hub connection string |
| `Sbm_AuthType` | Authentication type |
| `Sbm_DatabasePlatform` | Target platform (`SqlServer` or `PostgreSQL`) |
| `Sbm_EventHubLogging` | Pipe-delimited logging flags (e.g. `EssentialOnly\|ScriptErrors`) |
| `Sbm_AllowObjectDelete` | Allow object deletion in DACPAC comparisons |
| `Sbm_UserName` | Database username (when not using Key Vault) |
| `Sbm_Password` | Database password (when not using Key Vault) |
| `Sbm_QueryFile` | Path to query file (for query operations) |
| `Sbm_OutputFile` | Path to output results file |

----

## Logging

For general logging, `sbm` has its own local messages. This log file is
named SqlBuildManager.Console{date stamp}.log and can be found in the
current working directory or the configured `--rootloggingpath`. This file will be the first place to check for general
execution errors or problems.

To accommodate the logging of a threaded or batch build, all of the output is
saved to files and folders under the path specified in
the `--rootloggingpath` flag. For a simple threaded execution, this is a
single root folder. For a remote server execution, this folder is
created for each execution server.

### For for details and script run troubleshooting suggestions, see [Log Files Details for Threaded, Batch,  Kubernetes and ACI execution](threaded_and_batch_logs.md)