# SQL Build Manager Change Log

### Version 15.6.1
- *FIXED:* [GitHub Issue #469](https://github.com/mmckechney/SqlBuildManager/issues/469) - New tables with foreign key constraints not generating the CREATE TABLE statements
- *FIXED:* GitHub Action for container build

### Version 15.6.0
- *UPDATED:* Now targets .NET 8.0 
- *UPDATED:* Simplified data object classes and regenerated typed DataSet classes
- *UPDATED:* Docker base set to .NET Runtime 8.0 and .NET SDK to 8.0
- *REMOVED:* Removed fall back Settings File Key generation from machine value. Now must be provided via `--settingsfilekey` argument or `sbm-settingsfilekey` Environment variable
- *ADDED:* `--settingsfilekey` is no longer required when a Key Vault Name is provided. This will bypass any settings file decryption and only retrieve the secrets directly from Key Vault

### Version 15.5.0
- *NEW:* For muti-database target builds, you can now specify custom concurrency tag. Previously, the only concurrency differentitor was by SQL Server Name. Please see the docs on [Concurrency](/docs/concurrency_options.md) and [Database targeting options](docs/override_options.md) to understand how to use this new feature.
- *UPDATED:* DACPAC creation timeouts now set to the value of `--defaultscripttimeout`. Previously, it was using the default settings.
- *UPDATED:* If a script package is failing after retries due to a timeout, the build will now immediately fail. The prior behavior was to create a custom DACPAC (if configured) and continue trying the build. This was just delaying the inevitable failure and wasting time.

### Version 15.4.2
- *UPDATED:* Converted Batch Node Pool creation to the new `Azure.ResourceManager.Batch` SDK. **NOTE:** This may require you to add batch specific NSG rules if you deploy into a subnet. See [network.bicep's](./scripts/templates/Modules/network.bicep) `nsgBatchResource` resource to see the rules that are needed.

### Version 15.4.1
- *UPDATED:* Changed the test environment creation from az CLI commands to Bicep templates/modules
- *UPDATED:* Modified/refactored internal handling of Manged Identity Client ID
- *ADDED:* New EventHub logging type of `ScriptErrors` which will event out any script error messages that occur during execution
- *UPDATED:* Optional feature to have Eventhub monitoring to attempt to create a custom consumer group to avoid any event read conflicts. The running identity must have "Event Hub Owner" RBAC priviledges and there are also two new arguments `--ehrg`/`--eventhubresourcegroup` and `--ehsub`/`--eventhubsubscriptionid` which would need to be provided. If these are not all met, it will continue to use the existing $Default. The custom consumer group will be deleted after run is complete. 
- *UPDATED:* `sbm utility eventhub` now accepts a `--jobname=all` to query all events, `--timeout` in seconds for how long to continue to monitor after the last event is received and `--eventhubresourcegroup`/`--ehrg`,  `--eventhubsubscriptionid`/`--ehsub`, `--storageaccountname` and `--storageaccountkey` arguments to support the new optional feature to create a custom consumer group. The storage account is used for the consumer checkpointing 

### Version 15.4.0
- *FIXED:* Corrected bug #386 - `sbm batch *` were not properly handling the `--authtype ManagedIdentity` argument ()
- *FIXED:* Corrected bug #387 - The path for the `--targetdacpac` was not getting trimmed to just the file name when getting sent to the Batch nodes
- *UPDATED* Including Managed Identity client ID as SQL connection user ID property when using `--authtype ManagedIdentity` argument
- *UPDATED:* Added ability for `sbm utility override` to accept `--settingsfile`, `--settingsfilekey` and identity arguments 
- *UPDATED:* Changed `sbm dacpac` command to accept `--settingsfile`, `--settingsfilekey` and identity arguments (previously only accepted SQL authentication via `--username` and `--password`)


### Version 15.3.0
_Consolidated updates in Version 15+_
*NEW:* Removing `beta` tag as the new AKS  [Workload Identity](https://docs.microsoft.com/en-us/azure/aks/use-managed-identity) implementation is now GA.  This replaces of AAD Pod Identity and is a *breaking change* from any previous Kubernetes deployments. To understand how to configure your cluster, review the steps in the [create_aks_cluster.ps1](scripts/templates/kubernetes/create_aks_cluster.ps1) script.

_New & Updated Commands:_
- Eliminated the need for `sbm batch enqueue`. You can now run `sbm batch run` and it will automatically enqueue the database targets for you. You can still run `sbm batch enqueue` first if desired
- `sbm utilty override` command to generate an override cfg file from a SQL script file.
- `sbm k8s query` command to run a query across your database fleet using Kubernetes as a compute platform
- `sbm aci run` command to orchrstrate full ACI process (prep, enqueue, deploy and monitor commands)
- `sbm aci query` command to run a query across your database fleet using ACI as a compute platform
- `sbm batch query` command now fully supports reading messages from Service Bus as well as using Managed Identity


_New Options:_
- `--tenantid` option to provide Azure AD Tenant ID for deployments. This will be necessary if local ID has access to multiple tenants and the target tenant is not the default
- `--batchresourcegroup` (`--batchrg`) argument to specify the resource group for the Batch account.  (If not provided, will be infered from Identity resource group)
- `--podcount` for Kubernetes deployments to specify the number of pods to deploy per job
- `--vnetresourcegroup` (`--vnetrg`) argument to specify the resource group for the VNET. (If not provided, will be infered from compute resource group)
- `--eventhublogging` controls how to log script results and if to emit verbose message events. Add multiple flags to combine settings. Values: `EssentialOnly`,  `IndividualScriptResults`, `ConsolidatedScriptResults`, `VerboseMessages`

_New Configuration:_
- Changed Sample/Test environment to use VNET connections between databases and compute platforms
  - SQL Server private VNET connections only, with local firewall rules and excluding "Azure Services"
  - VNET integration for Azure Container Apps
  - VNET integration for Azure Container Instances
  - VNET integration for Batch Nodes
  - AKS cluster creation now has Azure RBAC enabled

_Bug Fixes & Improvements:_
- EventHub logging now also includes the script results for each script run against the databases as an option. (`--eventhublogging` options of `IndividualScriptResults` or `ConsolidatedScriptResults`)
- Renewing Service Bus message lease every 30 seconds until the build is complete for the target database
- Fixed regression in Batch processing from generated settings files
- Code refactoring for consistency and ease of maintenance
- Corrected bug where Batch execution wasn't properly consolidating certain log files
- *BREAKING CHANGE*: Changed ACI deployment to use SDK vs custom ARM templates. Review new command arguments for `sbm aci prep` and `sbm aci deploy` (and consider using new `sbm aci run` command)

_Platform updates:_
- Application now targets .NET 7
- Docker base images updated to .NET Runtime 7.0.5 and .NET SDK to 7.0.203
- General code cleanup and switch from System.Data.SqlClient for Microsoft.Data.SqlClient


### Version 15.2.2-beta
- *NEW:* Eliminated the need for `sbm batch enqueue`. You can now run `sbm batch run` and it will automatically enqueue the database targets for you. You can still run `sbm batch enqueue` first if desired
- *ADDED:* Renewing Service Bus message lease every 30 seconds until the build is complete for the target database
- *ADDED:* EventHub logging now also includes the script results for each script run against the databases.  
- *ADDED:* New option `--eventhublogging` controls how to log script results and if to emit verbose message events. Add multiple flags to combine settings. Values: `EssentialOnly`,  `IndividualScriptResults`, `ConsolidatedScriptResults`, `VerboseMessages`
- *FIXED:* Corrected bug where Batch execution wasn't properly consolidating certain log files

### Version 15.2.1-beta
- *UPDATED:* Code refactoring for consistency and ease of maintenance

### Version 15.2.0-beta

- *ADDED:* New `sbm utilty override` command to generate an override cfg file from a SQL script file.
- *ADDED:* New `sbm k8s query` command to run a query across your database fleet using Kubernetes as a compute platform
- *ADDED:* New `sbm aci run` command to orchrstrate full ACI process (prep, enqueue, deploy and monitor commands)
- *ADDED:* New `sbm aci query` command to run a query across your database fleet using ACI as a compute platform
- *UPDATED:* The `sbm batch query` command now fully supports reading messages from Service Bus as well as using Managed Identity
- *UPDATED:* *BREAKING CHANGE*: Changed ACI deployment to use SDK vs custom ARM templates. Review new command arguments for `sbm aci prep` and `sbm aci deploy` (and consider using new `sbm aci run` command)
- *ADDED:* New `--vnetresourcegroup` (`--vnetrg`) argument to specify the resource group for the VNET. (If not provided, will be infered from compute resource group)

### Version 15.1.0-beta

- *ADDED:* New `--batchresourcegroup` (`--batchrg`) argument to specify the resource group for the Batch account.  (If not provided, will be infered from Identity resource group)
- *ADDED:* `--podcount` for Kubernetes deployments to specify the number of pods to deploy per job
- *UPDATED:* AKS cluster creation now has Azure RBAC enabled
- *UPDATED:* Changed Sample/Test environment to use VNET connections between databases and compute platforms
  - *ADDED:* SQL Server private VNET connections only, with local firewall rules and excluding "Azure Services"
  - *ADDED:* VNET integration for Azure Container Apps
  - *ADDED:* VNET integration for Azure Container Instances
  - *ADDED:* VNET integration for Batch Nodes
  
### Version 15.0.3-beta

- *NEW:* With v15+ the Kubernetes implementation is switching from using AAD Pod Identity to [Workload Identity (preview)](https://learn.microsoft.com/en-us/azure/aks/workload-identity-overview). Because Workload Identity is in Public Preview, v15 will remain in beta until the feature goes GA. This is a breaking change from any previous Kubernetes deployments. To understand how to configure your cluster, review the steps in the [create_aks_cluster.ps1](scripts/templates/kubernetes/create_aks_cluster.ps1) script.
- *UPDATED:* Application now targets .NET 7
_ *ADDED:* Option to provide Azure AD Tenant ID for deployments. This will be necessary if local ID has access to multiple tenants and the target tenant is not the default
- *FIXED:* Regression in Batch processing from generated settings files. 
- *UPDATED:* General code cleanup and switch from System.Data.SqlClient for Microsoft.Data.SqlClient
- *UPDATED:* Docker base images updated to .NET Runtime 7.0.2 and .NET SDK to 7.0.102
  
### Version 14.6.1

 - *UPDATED:* Added Windows installer setup project for SQL Build Manager windows form app

### Version 14.6.0

 - *ADDED:* Added Kubernetes namespace isolation in `sqlbuildmanager` namespace when using `sbm k8s run` and creating yaml files via `sbm k8s createyaml`
 - *ADDED:* Added `jobname` based kubernetes resources to isolate independent and/or concurrent runs when using `sbm k8s run`
 - *UPDATED:* Switched pre-build image source from docker hub blueskydevus/sqlbuildmanager to GitHub container registry mmckechney/sqlbuildmanager
 
### Version 14.5.0

 - *ADDED:* Simplified Kubernetes with `sbm k8s run` that will orchestrate the individual steps (`prep`, `enqueue`, `monitor`) and encapsulate all `kubectl` commands used to create resources
 - *ADDED:* Simplified Container Apps with `sbm containerapp run` that will orchestrate the individual steps (`prep`, `enqueue`, `deploy` and `montitor`)
 - *UPDATED:* `sbm k8s savesettings` will now create a json file instead of yaml files. All other subcommands will also take `--settingsfile` and `--settingsfilekey` values to be more in sync with the other execution types. You can still generate YAML files dynamically with `sbm k8s creatyaml` if you want to
 - *UPDATED:* Overhaul of [template scripts](scripts/templates/README.md) used to create sample and integration test resources and settings files to unify prefix resource names

### Version 14.4.0

 - *ADDED:* Added new `ManagedIdentity` authentication type to eliminate the need for a UserName and Password to authenticate to Azure SQL databases that have Azure AD authentication enabled and identity assigned
 - *ADDED:* Ability to use Managed Identity for Service Bus, Event Hub and Blob storage connections with most services (see [managed_identity.md](/docs/managed_identity.md) for details and limitations)
 - *ADDED:* New `--monitor` argument for `sbm batch run` to get running count of datbase activity  (commits, error, in queue)
 - *ADDED:* New `--stream` argument for `sbm batch run` (used in conjunction with `--monitor`) to also stream specific database completion messages as the occur
 - *UPDATED:* Monitoring of remaining queue messages only when Service Bus is used, but no Event Hub connection is provided
 - *UPDATED:* Reorganized Unit Test settings file creation scripts to group by execution compute type

### Version 14.3.0

- *ADDED:* Managed Identity and Key Vault support for Container Apps
- *ADDED:* Categorized subcommands in command line help output
- *ADDED:* DACPAC as source now supported with Container Apps, Kubernetes and Azure Container Instance
- *FIXED:* Removed false error return code in Batch execution when a custom DACPAC is created for a target database at runtime.
- *FIXED:* Missing custom container registry settings on ACI deployments
- *ADDED:* Added new package management command `sbm unpack` to extract the contents of an SMB file into scripts and SBX control file

### Version 14.2.1

- *UPDATED:* Updated Azure Container App resource manager API
- *FIXED:* Build issue with Batch deployments in .NET 6
- *FIXED:* Updated AKS creation script to fix VNET assignment
- *UPDATED:* Updated Azure SDK packages to latest versions (requiring some code changes)

### Version 14.2.0

- *ADDED:* You can now use Azure Container Apps as a compute platform for leveraging database builds with the new `sbm containerapps` commands. See the [Azure Container Apps documentation](docs/containerapp.md) for background, information and how-to examples
- *ADDED:* New options for container image tags and container registries for all container options (Azure Container Apps, Kubernetes, Azure Container Instance)
- *UPDATED:* Updates to deployment scripts and unit tests
- *ADDED:* New command `sbm utility eventhub` to scan EventHub for job event counts
- *UPDATED:* Reorganized template scripts by service
- *UPDATED:* Optimized EventHub client to checkpoint by job time so events are not missed when running multiple concurrent jobs

### Version 14.1.0

- *UPDATED:* Updating `sbm` to use IHostedService so SIGTERM from containers is acknowledged

### Version 14.0.1

- *ADDED:* New argument `--stream` for `sbm k8s monitor` and `sbm aci monitor` as a flag whether or not to output the individual database commit/errors messages. Default is `false`

### **Version 14.0.0**

There are three new options to massively parallel processing: [Azure Container Apps](docs/containerapp.md), [Kubernetes](docs/kubernetes.md) and [Azure Container Instance](docs/aci.md)!

 [Batch node pools](docs/massively_parallel.md) are now created with assigned Managed Identities. Because of this, the workstation running `sbm` _needs to have a valid Azure authentication token_. This can be done via Azure CLI `az login`, Azure PowerShell `Connect-AzAccount`, or if running from an automation box, ensure that the machine itself has a Managed Identity that has permissions to create Azure resources. Alternatively, you can pre-create the batch pools manually via the Azure portal, being sure to assign the correct Managed Identity to the pool.

[Kubernetes](docs/massively_parallel.md#kubernetes-process-flow), [Azure Container Apps](docs/containerapp.md), and  [Azure Container Instance](docs/massively_parallel.md#azure-container-instance-process-flow) also require local machine authentication (`az login`) in order to access Azure Key Vault. Authentication is not needed for [local](local_build.md) or [threaded builds](docs/threaded_build.md)

The keys, connection strings and passwords can now be stored in Azure Key Vault rather than saving the encrypted values in a settings file or being passed in via the command line. Regardless if you use Batch, Kubernetes or ACI , this integration is enabled by leveraging [User Assigned Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-manage-user-assigned-managed-identities). To easily accomplish this setup, there are a set of PowerShell scripts in the [`scripts/templates` folder](scripts/templates). A complete environment can be created with [`create_azure_resources.ps1`](scripts/templates/create_azure_resources.ps1). Please note that Azure Key Vault is required for [Azure Container Instance](docs/aci.md) builds.

You will also need to be logged into Azure if you are leveraging Azure Key Vault to store your secrets, regardless if you are using [Azure Batch](docs/massively_parallel.md#batch-process-flow), [Kubernetes](docs/massively_parallel.md#kubernetes-process-flow), [Azure Container Apps](docs/containerapp.md), or [Azure Container Instance](docs/massively_parallel.md#azure-container-instance-process-flow)


- **NEW:** You can now use Kubernetes as a compute platform for leveraging database builds with the new `sbm k8s` commands. See the [Kubernetes documentation](docs/kubernetes.md) for background, information and how-to examples
- **NEW:** You can now use Azure Container Instance as a compute platform for leveraging database builds with the new `sbm aci` commands. See the [ACI documentation](docs/aci.md) for background, information and how-to examples
- *ADDED:* New Azure Key Vault integration -- secrets can now be stored in Key Vault and no longer need to be stored in the settings file or passed via command line. See the [documentation here](docs/massively_parallel.md)
- **IMPORTANT:** Batch node pools are now created with assigned Managed Identities. Because of this, the workstation running `sbm` needs to have a valid Azure authentication token. This can be done via Azure CLI `az login`, Azure PowerShell `Connect-AzAccount`, or if running from an automation box, ensure that the machine itself has a Managed Identity that has permissions to create Azure Batch resources. 
- *UPDATED:* Automation scripts `/scripts/templates` have been updated to simplify the creation of Azure resources, runtime, secrets file and pushing secrets to Key Vault
- *UPDATED:* Batch and Kubernetes now have Managed Identity assigned to allow seamless access to Key Vault

### Version 13.1.0

- *UPDATED:* The `sbm create` command now has four sub-commands `fromscripts`,  `fromdiff`, `fromdacpacs` and `fromdacpacdiff`. See the [Command Line Reference](docs/commandline.md) for details and usage
- **NOTE:** The `sbm scriptextract` command is being deprecated in favor of `sbm create fromdacpacdiff` and will be removed in a future release
- *UPDATED:* Corrected how the `sbm build` local build command handles logging. It is now all encapsulated in the `.sbm` file as it should be
- *UPDATED:* Documentation updates and improvements

### Version 13.0.4

- *ADDED:* New command `sbm add` to add scripts to an existing SBM package or SBX project file from a list of scripts
- *ADDED:* New command `sbm list` to output script information of SBM packages (run order, script name, date added/modified, user info, script ids, script hashes)
- *UPDATED:* The `sbm create` command can now also create an SBX project file, not just an SBM package
- *UPDATED:* Tabular output for `sbm policycheck` command to make it easier to read. Defaulting enterprise config to GitHub file.

### Version 13.0.3

- *FIXED:* Update to Dacpac change scripts to identify new header delimiter
- *FIXED:* Issue creating scripts between incompatible SQL Server versions. Will now output a warning and continue to create the scripts with the flag `AllowIncompatiblePlatform=true` 
- *ADDED:* New command `sbm create` to create a new SBM file from a list of scripts
- *UPDATED:* Can now use Windows authentication for DACPAC creation
- *UPDATED:* updated NuGet packages

### Version 13.0.2

- *FIXED:* Update to ensure all Queue messages are retrieved efficiently
- *ADDED:* New utility method `sbm batch dequeue` to remove all messages from the Service Bus Queue topic (without processing them)
- *UPDATED:* Code clean up and refactoring to accommodate latest version of System.CommandLine
- *FIXED:* Issue with SQL text syntax highlighting formatting in .NET 5.0
- **NOTE:** Removed "Construct Command Line" menu options from Windows UI. Users should leverage the generated help docs for sbm.exe

### Version 13.0.1

- *FIXED:* Updated distribution algorithm for `--concurrencytype` of `Server` and `MaxPerServer` when number of Batch nodes is very close to the number of SQL Server targets. Was yielding less than the number of nodes.  
- *FIXED:* Updated Service Bus message retrieval to better manage when messages not matching the job name are in large quantity

### Version 13.0.0

- *ADDED:* New option to leverage Azure Service Bus Topic as a database target source. See the [Azure Batch](docs/azure_batch.md) docs for more detail
- *ADDED:* New command option `--settingsfilekey`, a key for the encryption of sensitive information in the settings file. If provided when saving the settings file, it of course must also be provided when using the settings file. This version moved away from a static (and not really secure) encryption key used prior. The argument value can be either the key string or a file path to a key file. The key may also be 'silently' provided by setting a `sbm-settingsfilekey` Environment variable. If not provided a machine value will be used.
- *FIXED:* Modified unit tests to Close and Flush loggers to avoid file locking issues

### Version 12.1.0

- *UPDATED:* Removed log4net logging. Unified logging via ILogger created in SqlBuildManager.Logging. Implements Serilog

### Version 12.0.0

- **NOTE:** **Removed old style command line (leveraging the `/Action=verb` flag etc.). Run `sbm --help` for instructions**
- **NOTE:** Now built against .NET 5 and .NET Core 3.1
- *ADDED:* New `threaded` and `batch` command options: `--concurrency` and `--concurrencytype`. See docs on [Concurrency Options](docs/concurrency_options.md)
- *UPDATED:* Now leveraging `Microsoft.SqlServer.DACFx` NuGet package instead of `sqlpackage.exe` command line to manage DACPACs
- *FIXED:* Updated `BlueSkyDev.Logging.AzureEventHubAppender` to v1.3.2 due to app hanging if EventHub connection string was incorrect

### Version 11.3.1

- *UPDATED:* Changes to the deployment templates and PowerShell files to be more friendly with Azure DevOps release pipelines 
- *FIXED:* `sbm batch run` regression introduced with new query option

### Version 11.3.0

- **NOTE:** **The old style command line (leveraging the `/Action=verb` flag etc.) will be deprecated soon!**
- *ADDED:* New querying across databases from command line for threaded and batch [`sbm batch query`] and [`sbm threaded query`]. Brings the exsting UI feature (Action-> Configure Multi server/Database run-> Reports -> Adhoc Query Execution) to command line  
- *UPDATED:* Running a threaded update is now performed via [`sbm threaded run`] (vs just [`sbm threaded`]), now that there is also a query option for threaded runs

### Version 11.2.0

- *ADDED:* The Batch processors now work on Linux worker nodes!
- *UPDATED:* New batch command option to support Linux: `--os [Windows,Linux]`. Note also that the `--batchpoolname` option now has important relevance for Batch accounts that have both Linux and Windows pools
- *UPDATED:* The Azure setup scripts (see /doc/setup_azure_environment) now also include setting up a Linux pool and the creation of settings files and application package zips for both environments

### Version 11.1.0

- **NOTE:** Now built against .NET Standard 2.1 and .NET Core 3.1
- *ADDED:* New feature to allow Azure EventHub logging for Azure Batch and Threaded model execution
- *UPDATED:* Refactored unit tests to separate those that are dependent on a local build environment and SQLExpress install and those that are not
- *ADDED:* 
			Shortened command line executable name `sbm.exe` leveraging the [System.CommmandLine](https://github.com/dotnet/command-line-api) command pattern and validation (`sbm [command] [options]`).
			The old executable (`SqlBuildManager.Console.exe`) with the prior pattern of `/Action=verb` is still supported is still available
		
- **NOTE:** The CLI and core UI components of the app have been thoroughly tested. Some ancillary UI pieces have not. If you find an issue, please log it in GitHub

### Version 11.0.0

- **NOTE:** MAJOR CHANGE -- removed the legacy Azure Cloud Service deployment model. Please update to use the Azure Batch model instead!
- *ADDED:* New feature to allow Azure EventHub logging for Azure Batch and Threaded model execution
- *UPDATED:* Refactored unit tests to separate those that are dependent on a local build environment and SQLExpress install and those that are not
- *ADDED:* YML files for Azure DevOps builds (and changes to ensure they build successfully)

### Version 10.4.4

- *ADDED:* Added command line argument `/DefaultScriptTimeout` (integer) to allow custom settings for the timeout of scripts when created from a DACPAC. Default is 500 seconds
- *UPDATED:* Refactored the `/TimeoutRetryCount` (integer) setting so it will be included in the `/SettingsFile`. This setting will retry build failures `X` times if the build fails because of any timeout error that results in a build rollback

### Version 10.4.3

- *ADDED:* New utility `/Action=SaveSettings` and `/SettingsFile` argument for simple reuse of settings.

### Version 10.4.2

- *ADDED:* Added Batch pool handling `/Action=BatchPreStage` so that Pools can be created in advance of needing the compute. Avoids the waiting "cold start" of a batch job
- *ADDED:* Added Batch pool handling `/Action=BatchCleanup` so that Pools can be deleted separately from a build run (useful if you need to re-run jobs)
- *UPDATED:* Changed default action of `/DeleteBatchPool` flag to `false` so that pool VMs are not immediately deleted after a Batch job. More useful now with the BatchPreStage and BatchCleanUp actions

### Version 10.4.1

- *UPDATED:* Updated Azure Batch to better handle pools, jobs and log file storage as well as job naming
- *UPDATED:* Refactored command line handling to clarify components
- *FIXED:* Cleaned up unit tests

### Version 10.4.0

- *ADDED:* Functionality to leverage Azure Batch as an execution engine
- *UPDATED:* Swapped out SharpZipLib to remove Zip Slip vulnerability 

### Version 10.3.0

- *ADDED:* New authentication types for Azure: Azure AD Password and Azure AD Integrated
- **NOTE:** Install now requires .NET Version 4.6.1 or higher (https://www.microsoft.com/en-us/download/details.aspx?id=49981)
- **NOTE:** Also required is the Microsoft Active Directory Authentication Library for Microsoft SQL Server (https://www.microsoft.com/en-us/download/details.aspx?id=48742)

### Version 10.2.12

- *ADDED:* Added command line to create an SBM package from a Platinum Dacpac and target database. New action /Action=ScriptExtract and /OutputSbm="<filename>"
- *ADDED:* Now masking passwords on log messages
- **NOTE:** Consolidating cloud storage locations
- **NOTE:** Removing obsolete projects from solution

### Version 10.2.11

- *UPDATED:* Changed the default script timeout for a dacpac generated script from 20 seconds to 500 seconds

### Version 10.2.10

- *FIXED:* For a remote execution build console service check now only looks at the endpoints that were tasked with work during the session.

### Version 10.2.9

- *FIXED:* Corrected issue of creating dacpac change scripts from incorrect source

### Version 10.2.8

- *FIXED:* Corrected issue with retrieving client list from SQL statement

### Version 10.2.7

- *FIXED:* Fixed parsing of error logs from the remote servers

### Version 10.2.6

- *FIXED:* Fixed installer to include the SQLPackage.exe in all instances
- *UPDATED:* Removed restriction for the remote execution command option

### Version 10.2.5

- *ADDED:* Can now execute ad hoc queries using SQL Authentication. Querying will use the same credentials (AD or SQL) used when opening SQL Build Manager

### Version 10.2.4

- *ADDED:* Changed dacpac scripting, removed custom scripting settings. These was preventing the drops of column defaults that then resulted in runtme build errors
- *ADDED:* Stripping out login and user scripts

### Version 10.2.2

- *ADDED:* New argument /BuildRevision that will be used as the value to insert into a Versions table 'VersionNumber' column
- *ADDED:* New argument /RemoteDbErrorList=<remote name=>|all. Returns the list of databases that has execution errors in the last run. Use "all" to get list from all remote servers 
- *ADDED:* New argument /RemoteErrorDetail=<server:db>|<instance>|all.  Retrieves the error detail for each instance in error. Is "server:db" is provided, full log detail is returned
- *ADDED:* "Last Status" to values returned with /AzureRemoteStatus=true

### Version 10.2.1

- *ADDED:* Ability to use a SQL file as the source for a multi-database configuration source. The query must return 2 columns: 1)server name 2) database name
- *ADDED:* Ability to specify a /PlatinumDbSource and /PlatinumServerSource as sources to generate the Platinum dacpac at runtime, vs using a pre-composed dacpac file
- *UPDATED:* Reworked command line to use a single "/Action" argument that designates the basic functionality of the runtime instance

### Version 10.1.1

- *ADDED:* Added console argument /AzureRemoteStatus=true to list Azure remote execution machines and their readiness status
- *ADDED:* Added console argument /ForceCustomDacPac=true to force the creation of a custom dacpac and build script for each target database. USE WITH CAUTION this will exponentially increase the deployment time  and processing needs. Use only as a last scenario to ensure full database synchronicity
- *FIXED:* Various remote/ threaded bug fixes

### Version 10.1.0

- *ADDED:* Ability to use a "platinum" Data-tier application file (dacpac) to synchronize database schemas

### Version 10.0.0

- *ADDED:* New ability to compare package deploys between two databases
- *ADDED:* Ability to synchronize package deploys between a "golden copy" and a target
- *ADDED:* New Azure cloud service deployment for remote execution engines!
- *UPDATED:* Installation for the Service is now a self-installing command-line for SqlBuildManager.Services.Host.exe
- *UPDATED:* Installation for the SBM tool is through ClickOnce at www.sqlbuildmanager.com or an InstallShield installer
- **NOTE:** See the command-line reference for appropriate flags for data synchronization

### Version 9.2.2

- *ADDED:* Command line argument /TestConnectivity=true when used in conjunction with /Remote=true will test the connectivity to the remote agents and all of the target databases
- *ADDED:* New command line help (/? or /help) message
- *UPDATED:* Remote execution validation between server names and execution agents is no longer case sensitive

### Version 9.2.1

- *ADDED:* New policy check status that warns you that code should be reviewed before release
- *ADDED:* command line ability to check script policies. Usage: SqlBuildManager.Console.exe /PolicyCheck "<Source Package Path and Name>" 
- *ADDED:* command line ability to create a default backout package. Usage: `SqlBuildManager.Console.exe /CreateBackout "<Source Package Path and Name>" /server="<Backout source server>" /database="<Backout source database>`"
- *FIXED:* Script policy icons were getting set to "Policy checks not run" after a committed build
- *FIXED:* Database login error for accounts that do not have code review database permissions
- *UPDATED:* Added additional quick status update for the remote service to hopefully resolve issue on slower servers
- *UPDATED:* Increased delay of first check between console and remote agents from 500ms to 1 second to accommodate slower agent servers

### Version 9.1.0

- *FIXED:* Issue when creating trigger DROP scripts in backout packages
- *FIXED:* Empty Zip file getting created when archiving script logs
- *FIXED:* Corrected issue with Code Review retrieval that could cause application errors 

### Version 9.0.0

- **NOTE:** **New install requires .NET Framework 4.0** If you do not have this version installed (but you really should), please contact Mike to assist in making your current tool version forward compatible.
- *ADDED:* Ability to track and manage code reviews of the SQL scripts
- *ADDED:* Prohibited the use of the Ad Hoc query execution to perform INSERT, UPDATE or DELETE queries. A build package should be used instead to ensure proper logging and tracking
- *ADDED:* Now logs an individual script start and end time in the SqlBuild_Logging table
- *ADDED:* Refined WITH (NOLOCK) policy to now require the name of the table in the exception tag. This will allow for per table NOLOCK exceptions
- *FIXED:* Updated WITH (NOLOCK) check to capture more edge cases
- *FIXED:* Updated WITH (NOLOCK) check to better handle keywords when they appear on comments
- *FIXED:* Issue with opening scripts for SBM files that are under source control
- *FIXED:* Issue with source control when running an object script update
- *FIXED:* Object script update issue that was not capturing the "current override settings"
- *UPDATED:* Policy icons update in the background as each script is checked via multi-threaded execution
- *ADDED:* New command-line option to calculate the hash of an SBM or SBX file:       /gethash "<packageName>"

### Version 8.8.2

- *ADDED:* Enhanced TFS source control to automatically add new files to source. NOTE: you still need to check-in manually.
- *UPDATED:* Updated TFS source control to checkout only files as needed instead of checking out all of the scripts in an SBX
- *UPDATED:* Policy icon updated to include warnings as well as alerts
- *UPDATED:* Changed background color for "read-only" files to make it less obnoxious
- *FIXED:* Case sensitivity issue when matching remote execution servers and target database servers

### Version 8.8.1

- *UPDATED:* Updated the TFS source control integration to play nicer with the Visual Studio status refresh

### Version 8.8.0.1

- *ADDED:* Integration with Source Control! New notification area indicator if project and files are under source control.
- **NOTE:**  *Will Checkout and Add files only*   You will still need to check-in/commit your changes manually
- *UPDATED:* Compiled against Version 11 of Server Management Objects (SMO) for compatibility with SQL 2012
- *UPDATED:* Improved threading when adding scripted files to ensure no dialog windows are "lost"
- *ADDED:* Will now accept WITH (READPAST) as valid query optimization. This should only be used in distinct use cases!
- *UPDATED:* Updated service installer to accept username and password parameters to allow for unattended service install

### Version 8.7.3

- *ADDED:* Made it easier to combine SBM files via drag and drop into script list
- *ADDED:* Export of script violations to CSV file from Tools menu
- *FIXED:* Bulk Add confirmation window was opening hidden behind the main form

### Version 8.7.2

- **NOTE:** Auditing change! No longer uses an AuditTransactionMaster when creating audit tables and triggers. All system data is now captured directly in the child audit table.
- *UPDATED:* Changed how Primary Keys are scripted for tables. They are now separated from the CREATE TABLE script to allow for easier editing and updating of the settings.
- *FIXED:* Ensured that triggers do not get scripted with the parent table object. They should be scripted out separately.
- *FIXED:* Duplicated table entries when the same table name is found in different schemas
- *FIXED:* Not all indexes were getting scripted
- *ADDED:* Added "EnableVisualStyles" for application to render properly in Windows 7

### Version 8.7.1

- *FIXED:* Removed redundant policy checking when adding files in bulk
- *ADDED:* Automatic creation of a MultiDb .cfg file of failed databases when running from the console. Creates the file in the same directory as the executable.
- *ADDED:* String include context menu shortcut for index options

### Version 8.7.0

- **NOTE:** New Feature! Additional script list icon column to identify policy check status
- *ADDED:* Automatic policy checking on load of a build package - including icon updating
- *ADDED:* New "Run Policy Checks" link on script form so you can check policy adherence without trying to save the file
- *ADDED:* New script policy "syntax pair" type. Allows you to make sure script snippets are always paired (or never paired if that's what you want)
- *FIXED:* Deserialization error when MultiDb file also includes additional query column data

### Version 8.6.13

- *FIXED:* Backout package creation was incorrectly missing pre-existing triggers

### Version 8.6.12

- *FIXED:* Enhanced the Constraint Name policy check to provide more accurate matching
- *FIXED:* Removed added tabbing in scripted object to eliminate "shift" of object with multiple scripting
- *UPDATED:* Altered creation of backout package so that manual scripts with a build order >= 1000 are not marked as "run once"
- *UPDATED:* Backout package creation has new option to have DROP scripts created for "Not Found" stored procedures, functions, triggers and views
- *ADDED:* Ability to copy to clipboard the list of scripts that can not be automatically generated for a backout package

### Version 8.6.11

- *FIXED:* Updated scripting of table objects to ensure that the object schema is included all of the object references
- *UPDATED:* Updated trigger scripts to be the same whether created new via the "Tools --> User Data History and Audit Scripting" or pulled from a database via the "Scripting --> Add Object Create Scripts --> Triggers" menu options
- *ADDED:* Synchronized zooming size between left and right sides in script comparison screens

### Version 8.6.10

- *FIXED:* Error when scripting triggers that included an "IF EXISTS" statement in its script body
- *FIXED:* Intermittent SMO database connection error
- *ADDED:* Ability to set custom highlight colors in script comparison screens

### Version 8.6.9

- *ADDED:* Timeout Retry Count feature added to remote agent execution
- *ADDED:* Can now view the change history of a database object (stored procedures, views, etc) over time as executed by the Sql Build Manager tool.
- **NOTE:** The history feature currently accessible from the main form "Scripting -> Add Object Create Scripts -> (select object type)" in the pop-up window, select the object, right-click and choose "View Object's change history.."
- **NOTE:** Also available from the script list context menu "View object change history as run by Sql Build Manager" when you have selected a scriptable database object (stored proc, trigger, view, etc)
- *FIXED:* Trigger scripting error when scripting via "Scripting -> Add Object Create Scripts -> Triggers". Was always producing the "UPDATE" trigger.

### Version 8.6.8

- *ADDED:* New features for creating rollback packages: options for removal of new object scripts and auto-marking manually created scripts as "run once"
- *ADDED:* New command line argument /TimeoutRetryCount to allow for multiple execution tries if the SQL Server error message received is "Timeout expired" 
- *ADDED:* Ability to control minimum script timeouts by file type and AD group membership
- *ADDED:* Execution timer added to ad hoc query
- *UPDATED:* Re-build feature now allows selection of database vs. scanning entire server on page load
- *UPDATED:* User auditing triggers will produce a file per each trigger type vs. a single files with 3 triggers

### Version 8.6.7

- *ADDED:* Made script timeout for default script configurable
- *ADDED:* Status icons based on server modified date now linked to system scripted views, triggers and tables (i.e. files with .VIW, .TRG, .TAB extensions) as well as stored procedures and functions (.PRC and .UDF)
- *ADDED:* Ability to compare previously run versions of a script to each other (vs. just comparing to the existing current script). Available via the "View Run History Against Current Server" script context menu item. 
- *ADDED:* Shows script count at bottom of main window
- *FIXED:* When retrieving the detailed database log from a remote service agent, the log for the specified run date is returned instead of just the newest file.
- *ADDED:* New command line remote execution validation that will fail the request if any database servers are in the package but don't have a matching remote execution server
- *FIXED:* command line builder now strips of trailing backslash (\) from root logging path to avoid incorrect arguments parsing
- *FIXED:* Corrected SQL Server SMO scripting error - it was leaving off the schema designation on table defaults for the IF NOT EXISTS check.

### Version 8.6.6

- **NOTE:** This update only includes changes related to remote execution (a DBA thing). It is not strictly necessary for anyone else.
- *ADDED:* Ability to retrieve all error logs off a remote server in a single zip package via the remote execution UI
- *FIXED:* Logging error when running command-line remote execution

### Version 8.6.5

- *FIXED:* Script tag was not getting inferred when using the Action -> Add New File menu option
- *FIXED:* Package hash was not changing when scripts were reordered. It will now be different because the execution order can effect the final state of the database and as such the hash should be different

### Version 8.6.4

- **NOTE:** **ATTENTION! Due to a change in the parsing algorithm for scripts, hash values may differ between this version and prior versions of the tool! UPDATE AS SOON AS POSSIBLE.**
- *UPDATED:* Changed the ad hoc query working directory from the user's temp folder to a hidden subfolder of the destination file. This is to avoid issues with lack of storage.
- *ADDED:* Ability to set script timeout for ad hoc queries.
- *UPDATED:* Modified the batching of scripts to ignore "GO" delimiter in a comment block. This may effect the hash value of some scripts but 99% of the ones I have tested are unchanged. For the 1%, they may get a "changed" icon when in fact they were not changed.
- *UPDATED:* Changed highlight color in "diff" view to be more obvious on all monitors
- *ADDED:* Additional database logging: build package hash value, user id of requestor from a remote execution, and the version number of the Sql Build Manager used to execute the run

### Version 8.6.3.1

- *ADDED:* Ability to package all SBX files in a directory structure into their corresponding SBM packages via command line
- *FIXED:* Reinstated the "View Log File" menu option accidentally removed in prior version

### Version 8.6.3

- *FIXED:* Script file was being deleted from directory when removed from SBM file. (This is the expected behavior for an SBX file deletion, not SBM)
- *FIXED:* App used to crash if there was an error running a multi-database query that caused no output file to be created
- *UPDATED:* Consolidated the "About" and "Check for Updates" menu items into a single view
- *FIXED:* No longer forces you to change the script tag when selecting multiple scripts and selecting "Edit/View Script Build Detail"
- *ADDED:* Now script tag value can be inferred from the file name or from within a SQL bulk comment section. If a matching pattern (P #### or CR ####) is found it will be used as the script tag.
- *FIXED:* Error was encountered when updating an object script
- *ADDED:* Can now script triggers directly from a source database via the Scripting --> Add Object Create Scripts --> Triggers menu option
- *ADDED:* Enforces that Triggers scripted via the tool never have transactions stripped out

### Version 8.6.2

- **NOTE:** New Feature! Added the ability to easily create a "backout package" using an unchanged environment as your source via the "Scripting --> Create Backout Package" menu option
- *FIXED:* Corrected an issue with policy validation when encountering non-standard SQL syntax
- *UPDATED:* Changed formatting of log file to enable syntax highlighting
- *ADDED:* Syntax highlighting on log file viewer
- *ADDED:* Cut/Copy/Paste context menu to script and log file viewer

### Version 8.6.1

- *ADDED:* Ability to configure generic "syntax checks" for scripts and add them as policy checks
- *ADDED:* Policy check to disallow "EXECUTE AS" directives
- *ADDED:* Policy check to disallow "WITH RECOMPILE" directives
- *ADDED:* Ability to change the logging level for the current session via the Help -> Set Logging Level menu option
- *ADDED:* Added ability to control feature access by AD group membership.
- *ADDED:* Can now control default script entries on an enterprise level based on AD group membership
- *FIXED:* Corrected crash when default script registry contained no items

### Version 8.6.0

- *UPDATED:* Ability to pull back extremely large amounts of data via the ad hoc query function
- *UPDATED:* Cut/Copy/Paste context menu on "Add Script" and "Configure Via Query" forms
- *ADDED:* Can view the application log via the "Help --> View Application Log File" menu item.

### Version 8.5.10

- *ADDED:* New script policy check on ALTER VIEW. Adds a reminder to check for dropped indexes. Reminder can be suppressed with a [No Indexes] tag
- *UPDATED:* Tool now looks for scripts that require a build description and prompts for one if found
- *FIXED:* Exception was being thrown on startup when the Registered Servers tree list had a folder with no entries
- *UPDATED:* If a script timeout is set below the minimum allowed value, it will automatically be updated to the minimum value on attempted save
- *UPDATED:* Removed release notes from user manual into separate HTML file

### Version 8.5.9

- *UPDATED:* Added exception for "Select Star" policy to accept for views that are prefixed with "vw_" in their name. (Configurable) 
- *ADDED:* Creation of the specified external logging directory if it doesn't pre-exist.
- *UPDATED:* Changed default script timeout to minimum default script timeout (vs. fixed value) to ensure nothing smaller is added
- *UPDATED:* Added database count to the server distribution list form
- *FIXED:* Object scripting error for some objects that had 2 "CREATE" scripts vs. a "CREATE" and an "ALTER"
- *FIXED:* Corrected Null reference error when loading an SBX control file from a newly opened instance of the tool.

### Version 8.5.8

- *ADDED:* Build history tracking on remote build service client
- *ADDED:* Ability to view build history and retrieve log files for historical Build Service requests
- *FIXED:* Finder control to stop skipping to next find as you type
- *FIXED:* ad hoc query would sometimes run "last" query
- *ADDED:* Ability to include additional summary data items in ad hoc query

### Version 8.5.7

- *UPDATED:* Updated "find" feature on script view window to search starting at cursor location vs. always at the top
- *UPDATED:* Increased maximum package size for WCF service call to 5Mb
- *FIXED:* The object scripting window for Views was duplicating objects when there were 2 objects with the same name in different schemas
- *ADDED:* Ability to pull back the SqlBuildManager.Service.log file from the remote agent to the client
- *ADDED:* New TestDatabaseConnectivity WCF method to pre-check the ability of the remote agent to connect to its target databases before actually submitting a build.
- *ADDED:* New "Using" extension method for WCF client calls to properly handle the Close() method when the channel is in a faulted state (http://nimtug.org/blogs/damien-mcgivern/archive/2010/05/13/2320.aspx)
- *ADDED:* New BuildRequestFrom property on BuildSettings object to record on the Remote Execution server the user that submitted the build request

### Version 8.5.6

- *ADDED:* Enterprise level enforcement of script tag requirement
- *ADDED:* "Registered Server" capability on connection form
- *ADDED:* Ability to have and load master registered server lists available at a shared enterprise level

### Version 8.5.5

- *ADDED:* Command-line builder for remote execution
- *ADDED:* Ability to generate the remote execution server list from the list of override target servers
- *UPDATED:* Product manual including new features

### Version 8.5.4

- *FIXED:* Directory parsing on remote service side when the server name is an IP address
- *ADDED:* Made the minimum default script timeout an enterprise level setting

### Version 8.5.3

- *UPDATED:* Rebuild using the SQL Management Objects (SMO) v10 DLL's
- *UPDATED:* "Qualified Names" policy check failures now include line number of offending script
- *ADDED:* Command line options for remote server execution 
- *ADDED:* Ability to use System environment variables in the "remote logging path" for remote server execution.
- *ADDED:* Ability to save a single pre-configured remote execution server package (.resp) file that includes remote server list, SBM contents, target databases, et. al.
- *UPDATED:* Modified connection strings used to be "Pooling=false;" to solve connection and timeout issues.

### Version 8.5.2

- *ADDED:* Copy feature in Database Size Summary and Database Analysis forms to copy data and header information to clipboard for easy analysis in the spreadsheet of your choice.
- *FIXED:* Stored procedure scripting error when using the "Script Object ALTER and CREATE" option. Was leaving out the ALTER script when the CREATE script had the CREATE text in lower case.

### Version 8.5.1

- *ADDED:* Hash Signature calculation Tool functionality and menu item to use as a package validation signature
- *UPDATED:* Manual to include new section "Validating a Build Package"
- *FIXED:* File copy problem with SBX files when the script was already in the same folder as the SBX file itself
- *FIXED:* Issue with adding "tag" values to new scripts then getting lost.

### Version 8.4.0

- *ADDED:* Script policy to check for "SELECT *"
- *ADDED:* Refinements to the Qualified Names script policy including ignoring statements in a comment block
- *ADDED:* Stored Procedure Parameter script policy
- *ADDED:* Beta version of Remote Execution Service and client
- *UPDATED:* Added new features to product manual

### Version 8.3.9

- *UPDATED:* User data auditing user interface improved
- *UPDATED:* User data auditing ON UPDATE trigger changed to use 'deleted' vs 'inserted' data for audit record
- *ADDED:* Ability to directly import auditing scripts into currently open Sql Build Project
- *FIXED:* Refresh error when multi-database UI opening an existing query package

### Version 8.3.8

- *UPDATED:* Code table populate scripts to include WITH (NOLOCK)
- *UPDATED:* Schema policy check to ignore the trigger tables "inserted" and "deleted"
- *UPDATED:* WITH (NOLOCK) policy check to ignore the trigger tables "inserted" and "deleted"
- *UPDATED:* References to SQL 2005 sys.objects view from SQL 2000 sysobjects
- *FIXED:* Qualified names policy to handle an UPDATE statement that does not have a following WHERE

### Version 8.3.7

- *UPDATED:* Changed the Multiple database configration page from using pre-loaded tabs to creating pages on demand (this is to get around a window handle error that was found when loading a very large configuration)
- *UPDATED:* Auditing trigger scripts to rename column from RowsEffected to RowsAffected
- *FIXED:* Script status icon wasn't refreshing after an "update object create script" command

### Version 8.3.6

- *UPDATED:* File copy choices for default scripts when there is a pre-existing script or a read-only one already in place.
- *UPDATED:* Physical file deletion warning in SBX files when that file is shared by another co-resident SBX file.
- *ADDED:* Help Icon in Database Summary form.
- *UPDATED:* Error handling when creating a new project file and default scripts.
- *ADDED:* Updated Regex to ignore all SQL 2005 system views in NOLOCK policy and optimization.
- *ADDED:* Additional SqlBuildManager.Enterprise unit tests to increase code coverage %

### Version 8.3.5

- *FIXED:* When scripting objects as DROP / CREATE, removed the IF NOT EXISTS for the create (we already know it's not there because it was dropped!)
- *ADDED:* Enterprise/Team setting for watching and alerting on table changes
- *UPDATED:* NOLOCK policy to ignore SELECT FROM on functions
- *UPDATED:* NOLOCK and Qualified names policies to ignore keywords that are encapsulated in comments.
- *UPDATED:* Added 'sys.foreign_keys' to the ignore list for NOLOCK policy
- *UPDATED:* Policy violation form UI update.
- *UPDATED:* Manual and help file links

### Version 8.3.4

- *ADDED:* Adding help manual
- *ADDED:* Links within the application to the help manual file
- *UPDATED:* Utility scripts to be fully qualified
- *UPDATED:* Added exclusion for cursors from the Qualified Names policy
- *ADDED:* Checking for valid default and or target override database settings prior to execution of package.
- *UPDATED:* Qualified Names Policy to not require schema qualifiers on temp table or table variables
- *UPDATED:* Modified NOLOCK optimization and NOLOCK policy checker to ignore temp tables (#tablename) and table variables (tablename)
- *UPDATED:* Modified icon for multi-database query configuration files (.multiDbQ)
- *UPDATED:* Moved policy interface into separate project for future growth of interfaces.

### Version 8.3.3

- *UPDATED:* Updated Qualified Names Policy to check for qualifiers on INSERT and UPDATE scripts.
- *UPDATED:* Script Optimization and NO LOCK policy check to ignore UPDATE and DELETE statements
- *FIXED:* Updated Stored Procedure scripting to hit SMO objects only once when scripting.
- *FIXED:* With SBX files, handle adding default scripts when they reside in the same directory

### Version 8.3.1

- *ADDED:* Ability to drag and drop scripts between different Sql Build Manager instances

### Version 8.3.0

- *ADDED:* Policy checking during the saving of a new file, scripting a file from the database and refreshing scripts from the database
- *ADDED:* Policy check to ensure that table names are fully qualified with a schema prefix
- *ADDED:* Policy check for a consistent comment header on procedures and functions.
- *ADDED:* Standard header text insertable from the script context menu when editing a script
- *ADDED:* Policy check to ensure that constraint names include the name of the referenced table
- *ADDED:* Policy check that procedures and functions do not have a GRANT EXECUTE to the 'public' role.
- *UPDATED:* Added a [NOLOCK Exception: <reason>] override tag to the WITH (NOLOCK) policy check. 
- *FIXED:* Default version script includes RAISEERROR if versions table is not present.

### Version 8.2.0

- *ADDED:* Ability to perform Ad Hoc query across multiple servers/databases with results in CSV, HTML or XML format
- *ADDED:* Ability to load/save multi-database configuration via query 
- *ADDED:* Ability to perform multi database run from query via command line

### Version 8.1.13

- *FIXED:* Updated object comparison to use the first sequenced database as the "baseline". Was looking for "0", but didn't enforce its presence.

### Version 8.1.12

- *ADDED:* Ability to generate report showing script run status across multiple databases and servers
- *ADDED:* Ability to generate report to compare database objects across multiple databases and servers
- *ADDED:* Ability to perform additional analysis of database object report after initial run (doesn't need to hit database again)
- *ADDED:* Ability to select alternate Logging database from the UI for build run (formerly only available via command line)
- *ADDED:* Goto line number in edit script window
- *ADDED:* Current Line and Character number labels in edit script window
- *FIXED:* Added plain JOIN statement to the Script Optimization algorithm

### Version 8.1.10

- *ADDED:* Policy enforcement functionality with 3 built in policies: WITH (NOLOCK) directive, scripts wrapped in IF EXISTS for re-runability and GRANT execute for all routines.
- *ADDED:* Ability to change source database and server for object script updates from "Scripting" menu.
- *ADDED:* Ability to select alternate Logging database from the UI for build run (formerly only available via command line)
- *ADDED:* Threaded execution support for non-transactional RunScriptClear
- *UPDATED:* Utility scripts updated for optimization
- *UPDATED:* Various compiler warning clean-ups
- *FIXED:* Changed to catch it when unable to connect to a SQL server and notify the user to try another server.
- *FIXED:* Error loading of an SBX file when the build history file (SqlSyncBuildHistory.xml) is not found in the directory.
- *FIXED:* Error saving/loading a multiDb configuration if there is a duplicate entry error. 

### Version 8.1.8

- *FIXED:* Error loading SBX control file when there were missing files
- *FIXED:* Error loading SBX control file when the SqlSyncBuildHistory.xml file was writeable
- *FIXED:* Error loading command line creator form when the SBM file name or multi-db file is empty

### Version 8.1.7

- **NOTE:** Please manually uninstall your prior version before installing this new one **
- *FIXED:* Serial multi-db runs were always executed as Trial. Now will run as commit as directed.
- *ADDED:* Added the ability to use dynamic script token replacements #BuildDescription# and #BuildFileName#
- *ADDED:* Ability to specify build description for multi-db treaded execution via /description command argument

### Version 8.1.6

- *UPDATED:* Enhanced the Command Line builder form to have Auto-Complete feature
- *ADDED:* Ability to execute a constructed command line directly from the form
- *ADDED:* Ability to set an alternate logging database for threaded builds
- *ADDED:* Ability to set a UserName and Password (as alternative to Windows Authentication) for threaded BuildScriptEvent

### Version 8.1.5

- *FIXED:* Updated the command line constructor for our best friends the DBA's... have fun.
- *ADDED:* "Filtering" for object list in add object list window
- *FIXED:* File name of loaded .multiDb displays in the status bar
- *ADDED:* New column in Logging table to include target database (will be used in later update)
- *ADDED:* New form to construct a command line string
- *FIXED:* When removing a script file from an SBX file, will now physically remove the file from disk as well.
- *FIXED:* Checks for a read-only "SqlSyncBuildHistory.xml" when working with SBX files since these are needed as writeable as well. 

### Version 8.1.4

- *ADDED:* Indicator and control for read-only files when working with the SBX loose file configuration.
- *ADDED:* Ability to generate Stored Procedures, Views and Functions with IF EXISTS CREATE and ALTER pair via setting.
- *ADDED:* Ability to generate Stored Procedures, Views and Functions with permissions script via setting.
- *UPDATED:* Updated auto-scripting settings to accept zip and header settings from UI.
- *FIXED:* Making name of rebuilt file always have a .sbm extension since it only supports packages and not loose configurations.
- *ADDED:* Improved formatting of scripts created as ALTER And CREATE
- *ADDED:* Ability to use the delete key to remove scripts from the List
- *ADDED:* Warning when scripts weren't successfully removed from a script list (still working on root cause of intermittent problem)
- *UPDATED:* Default key for removing scripts is now "No" instead of "Yes" to help eliminate unwanted removals.

### Version 8.1.3

- *FIXED:* Error in script list background color updating under some scenarios
- *FIXED:* Error blocking scripts when they were pre-run on the server, but not the specific database
- *ADDED:* New script helper to automatically add "WITH (NOLOCK)" directives on all "FROM" type statements: FROM, INNER JOIN, OUTER JOIN, RIGHT JOIN, LEFT JOIN. This can be used to improve select performance when a lock is not required on the target data.
- *ADDED:* Alerting when SBM or SBX file is read-only with option to make writable or not load.
- *ADDED:* Count of target databases hit to end of multi-threaded run log file
- *FIXED:* Display of UpdateDate column name in table scripting tool-tip. It was instead being set as the UpdateID column type
- *FIXED:* When an empty build package run was attempted, crashed the app. This is now caught and an error message is displayed.
- *ADDED:* Application icon for the SqlBuildManager.Console.exe tool
- *ADDED:* Additional utility scripts for granting execute and revoke permissions
- *FIXED:* Unhandled exception when using server quick change and no build project was loaded.
- *ADDED:* Ability to add more than one update location for version checking. Can add delimited list for ProgramVersionCheckURL and ProgramUpdateFolderURL appsettings. They match up by array index, so make sure they're in order. 

### Version 8.1.0

- **NOTE:** 
			Major Update!   Now allows for managing scripts in either a single file package (the original .sbm file) or via a "loose" file structure. This new mechanism consists of a build control XML file (.sbx) that contains the script run metadata for the scripts that co-resident in the same file system directory. When you're ready to put them all together, a "package" feature is there to put all the files into a new build .sbm package.
		
- *ADDED:* Ability to create update/insert scripts from Data Extract file
- *FIXED:* Added schema name to the table in database analysis page
- *FIXED:* Sorting of multi-db run. Was sorting alphabetically even for numeric sequence numbers!
- *FIXED:* Database list for scripting option wasn't refreshing when server changed via server quick change drop down
- *UPDATED:*  Added error notification if object scripting was not successful
- *UPDATED:*  updated some menu icons

### Version 8.0.5

- *ADDED:* New threaded execution setting /trial to all you to perform a threaded run that will always get rolled back.
- *ADDED:* New threaded execution setting /ScriptSrcDir to perform execution using a directory containing your scripts vs. using a pre-created SBM file. Keep in mind, you lose quite a bit of functionality, but now you can do it.
- *ADDED:* New threaded execution setting /LogAsText to create log files in simple text. By default, they are created in HTML to allow for easy hyperlinking between the detailed run logs
- *UPDATED:* Updated some menu icons
- *FIXED:* Corrected sorting of "Bulk Add"

### Version 8.0.4

- *ADDED:* NEW FEATURE for the console application (now in version 2.0.0). Added the ability to use Sql Build Manager engine to process builds across multiple database instances in a threaded manner. Instead of the log and messages getting saved in the .SBM file, they are saved in a directory structure. This will allow you to update multiple same-schema databases quickly and efficiently.
- *FIXED:* Coloring of scripts marked as "run once" and also locked against running again. They were incorrectly getting displayed as white.
- *UPDATED:* Updated menu icons

### Version 8.0.3

- *ADDED:* Ability to copy out build history on a per-run basis to an XML file. This is done via a new context menu option on the "Review Past Builds" form (Logging | Show Build History from the main screen). Select the build(s) from the top master list then right-click and select "Save Build Details for Selected Rows".
- *ADDED:* New executable SqlBuildManager.Console.exe to simplify command-line running. This allows you to automate Sql Build Manager and retrieve exit codes as well as the console out and console error values.
- *ADDED:* New command line option to run Stored Procedure testing in an unattended, non-interactive mode.
- *ADDED:* New command line option to run a multi-database/server configuration in an unattended, non-interactive mode.
- *UPDATED:* Cleaned up command line options to a consistent key-value pair argument list.
- *FIXED:* Issue when creating a new Stored Procedure test project file. You weren't able to select a menu item when there were no stored procedures already in the tree view.

### Version 8.0.2

- *ADDED:* Enhancements to the Multi Database/Server run configuration form. Now alerts user when a loaded configuration doesn't match the databases found on the the target servers.
- *ADDED:* Enhancements to the Multi Database/Server run configuration form. Added a tab view inside the server pane to make it easier to navigate the databases in the configuration. Added some coloring to make the area separation clearer.
- *ADDED:* New links to the project web site at http://www.SqlBuildManager.com so the user can easily access the help manual.

### Version 8.0.1

- **NOTE:** Major updates!
- *UPDATED:*  1.Upgrading from using the deprecated SQLDMO for scripting to the "new" SMO scripting
- *UPDATED:*  2. Full compatibility with SQL schema names for all objects. This includes scripted objects as well as all of the script helpers and wrappers.
- *UPDATED:*  3. New multi-database/server configuration. This allows you to run against multiple target databases and across multiple servers in a single run and transaction.

### Version 7.6.9

- *ADDED:* Automatically detects="" and strips our SQL "USE" statements (this should be handled by the tool)
- *ADDED:* Project modifications to allow clean checkout/build/test process.
- *ADDED:* `<Ctrl>` key modifier to open project file temp directory when clicking on project file name in main window.
- *ADDED:*   More robust schema validation to ensure build file load.

### Version 7.6.8

- *ADDED:* Updated content checking

### Version 7.6.7

- *ADDED:* Check for change date on SP's and Functions to look for manual database changes.
- *ADDED:* New status icon to alert user of a manual server side database change.
- *ADDED:* Help menu for status icon meanings and background coloring + tool tips
- *ADDED:* Script tag to the SBM comparison form for easier cross-check

### Version 7.6.5

- *ADDED:* Search feature in script comparison forms
- *ADDED:* "Find next diff" feature in file comparisons

### Version 7.6.4

- *ADDED:* Toggle button to slide build script list to display date added and date modified for scripts.
- *FIXED:* Override database list was case-sensitive when adding names causing doubles.
- *FIXED:* Script search "OK" and "Cancel" buttons respond to Enter and Esc keys respectively
- *FIXED:* "Found" scripts from search weren't always getting highlighted
- *FIXED:*  When there is a build file read error for an unattended build, the tool sends out an error code and exits

### Version 7.6.2

- *FIXED:* Database name showing with "*" when modifying multiple scripts
- *FIXED:* Target database override list not refreshing when a database was removed.

### Version 7.6.1

- *FIXED:* Corrected roll-back error when database connection unavailable causing partially committed builds.
- *FIXED:* Changed docking for resize of build manager group box.
- *FIXED:* Error in refresh of status icons when Override target change is selected.
- *UPDATED:* Updated DefaultScriptRegistry.xml to remove the IFOOrder default script entry.

### Version 7.6.0

- *ADDED:* Override target database settings
- *ADDED:* Ability to maintain manual database entries

### Version 7.5.5

- *ADDED:* Added "find" feature to SQL script windows
- *ADDED:* Updated build review window to display results in full window 
- *FIXED:* Corrected thread contention error in unattended builds.

### Version 7.5.3

- *ADDED:* Time display for each script duration.
- *ADDED:* Setting flag to create build log file or not (default to not) - keeps build file smaller

### Version 7.5.2

- *ADDED:* Completed stored procedure testing framework
- *ADDED:* Compare between current script and script previously run on server
- *ADDED:* Highlighted SQL view of scripts run on a server
- *ADDED:* New index to SQLBuild_logging table to speed up list refresh

### Version 7.5.1

- *ADDED:* Check to ensure that Stored Procedures (.PRC) and Functions (.UDF) never strip transactions.
- *ADDED:* Ability to import Stored Procedure test configurations.

### Version 7.5.0

- *ADDED:* Sorting of build script list by status image.
- *ADDED:* Stored procedure testing framework

### Version 7.4.11

- *ADDED:* Ability to cancel a build.
- *ADDED:* Status timers for build duration and script duration.
- *ADDED:* Added progress bar for Builds
- *ADDED:* No script list refresh for failed builds (no need)
- *ADDED:* Ability to change multiple script tags at once. 
- *ADDED:* New utility scripts to wrap Stats and PK adds to make them re-runnable.
- *ADDED:* Added CreateId and CreateDate for Code Table auditing

### Version 7.4.6

- *FIXED:* Tag value not being saved with bulk add (ex. direct object scripting)
- *ADDED:* Tool tip alert when an autoscripting candidate (<F12>) value is highlighted in script window

### Version 7.4.5

- *FIXED:* Corrected object scripting error
- *FIXED:* Added error message when trying to open newer build files with old version.
- *ADDED:* Automatic Checking for new versions
- *ADDED:* Manual checking for new versions via Help menu
