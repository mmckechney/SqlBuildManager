using System;
using System.CommandLine;
using System.IO;
using Microsoft.Extensions.Logging;
using SqlSync.Connection;

namespace SqlBuildManager.Console.CommandLine
{
    /// <summary>
    /// Exposes all command-line options for use by CommandLineArgsBinder.
    /// This provides internal access to the Option instances defined in CommandLineBuilder.
    /// </summary>
    /// <remarks>
    /// Options use the new System.CommandLine 2.x constructor format:
    /// new Option&lt;T&gt;(name, aliases...) { Description = "..." }
    /// </remarks>
    public static class CommandLineBuilderOptions
    {
        #region Script and Package Options

        public static readonly Option<FileInfo[]> ScriptListOption = new("--scripts", "-s")
        {
            Description = "List of script files to create SBM package from - add one flag & file per script (will be added in order provided)",
            Required = true
        };

        public static readonly Option<string> PackagenameOption = new("--packagename", "-P", "--buildfilename")
        {
            Description = "Name of the .sbm or .sbx file to execute",
            Required = true
        };

        public static readonly Option<string> OutputSbmOption = new("--outputsbm", "-o")
        {
            Description = "Name (and path) of the SBM package or SBX file to create"
        };

        public static readonly Option<DirectoryInfo> UnpackDirectoryOption = new("--directory", "--dir")
        {
            Description = "Directory to unpack SBM into (will be created if it doesn't exist)",
            Required = true
        };

        #endregion

        #region Server and Database Options

        public static readonly Option<string> ServerOption = new("--server", "-s")
        {
            Description = "1) Name of a server for single database run or 2) source server for scripting or runtime configuration"
        };

        public static readonly Option<string> DatabaseOption = new("--database", "-d")
        {
            Description = "1) Name of a single database to run against or 2) source database for scripting or runtime configuration"
        };

        public static readonly Option<string> OverrideOption = new("--override")
        {
            Description = "File containing the target database settings (usually a formatted .cfg file)"
        };

        public static readonly Option<FileInfo> OverrideAsFileOption = new("--override")
        {
            Description = "File containing the target database settings (usually a formatted .cfg file)",
            Required = true
        };

        public static readonly Option<FileInfo> OverrideAsFileNotRequiredOption = new("--override")
        {
            Description = "File containing the target database settings (optional, used as a counter for monitoring)"
        };

        #endregion

        #region Execution Control Options

        public static readonly Option<bool> TrialOption = new("--trial")
        {
            Description = "Whether or not to run in trial mode (default is false)"
        };

        public static readonly Option<bool> TransactionalOption = new("--transactional")
        {
            Description = "Whether or not to run with a wrapping transaction (default is true)"
        };

        public static readonly Option<bool> ContinueOnFailureOption = new("--continueonfailure")
        {
            Description = "Whether or not to continue on the failure of database synchronization (default is false)"
        };

        public static readonly Option<bool> SilentOption = new("--silent", "--force")
        {
            Description = "Suppresses overwrite prompt if file already exists"
        };

        public static readonly Option<bool> ForceOption = new("--force")
        {
            Description = "Suppresses warning that the storage container/job already exist. Will delete any existing files without warning"
        };

        public static readonly Option<int> TimeoutRetryCountOption = new("--timeoutretrycount")
        {
            Description = "How many retries to attempt if a timeout exception occurs"
        };

        public static readonly Option<int> DefaultScriptTimeoutOption = new("--defaultscripttimeout")
        {
            Description = "Override the default script timeouts set when creating a DACPAC"
        };

        public static readonly Option<bool> UnitTestOption = new("--unittest")
        {
            Description = "Designation that execution is running as a unit test",
            Hidden = true
        };

        #endregion

        #region Logging Options

        public static readonly Option<string> RootLoggingPathOption = new("--rootloggingpath")
        {
            Description = "Directory to save execution logs (for threaded and remote executions)"
        };

        public static readonly Option<string> LogToDatabaseNameOption = new("--logtodatabasename")
        {
            Description = "[Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target)."
        };

        public static readonly Option<LogLevel> LogLevelOption = new("--loglevel")
        {
            Description = "Logging level for console and log file"
        };

        public static readonly Option<LogLevel> LogLevelWithInfoDefaultOption = new("--loglevel")
        {
            Description = "Logging level for console and log file"
        };

        public static readonly Option<string> DescriptionOption = new("--description")
        {
            Description = "Description of build (logged with build)"
        };

        public static readonly Option<string> BuildRevisionOption = new("--buildrevision")
        {
            Description = "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))"
        };

        #endregion

        #region Script Source Options

        public static readonly Option<string> ScriptSrcDirOption = new("--scriptsrcdir")
        {
            Description = "[Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)"
        };

        #endregion

        #region DACPAC Options

        public static readonly Option<string> PlatinumDacpacOption = new("--platinumdacpac", "-pd", "--pd")
        {
            Description = "Name of the dacpac containing the platinum schema"
        };

        public static readonly Option<FileInfo> PlatinumDacpacFileInfoOption = new("--platinumdacpac", "-pd")
        {
            Description = "Name of the dacpac containing the platinum schema"
        };

        public static readonly Option<string> TargetDacpacOption = new("--targetdacpac", "-td", "--td")
        {
            Description = "Name of the dacpac containing the schema of the database to be updated"
        };

        public static readonly Option<bool> ForceCustomDacpacOption = new("--forcecustomdacpac")
        {
            Description = "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer."
        };

        public static readonly Option<string> PlatinumDbSourceOption = new("--platinumdbsource")
        {
            Description = "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema"
        };

        public static readonly Option<string> PlatinumServerSourceOption = new("--platinumserversource")
        {
            Description = "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema"
        };

        public static readonly Option<string> DacpacOutputOption = new("--dacpacname")
        {
            Description = "Name of the dacpac that you want to create",
            Required = true
        };

        public static readonly Option<FileInfo> PlatinumDacpacSourceOption = new("--platinumdacpac", "-pd", "--pd")
        {
            Description = "Name of the dacpac containing the platinum schema",
            Required = true
        };

        public static readonly Option<FileInfo> TargetDacpacSourceOption = new("--targetdacpac", "-td", "--td")
        {
            Description = "Name of the dacpac containing the schema of the database to be updated",
            Required = true
        };

        public static readonly Option<bool> AllowForObjectDeletionOption = new("--allowobjectdelete")
        {
            Description = "Whether or not to script for database object deletion when creating a package from the comparison of two databases or DACPACS"
        };

        #endregion

        #region Authentication Options

        public static readonly Option<string> UsernameOption = new("--username", "-u")
        {
            Description = "The username to authenticate against the database if not using integrated or Managed Identity auth"
        };

        public static readonly Option<string> PasswordOption = new("--password", "-p")
        {
            Description = "The password to authenticate against the database if not using integrated or Managed Identity auth"
        };

        public static readonly Option<AuthenticationType> AuthTypeOption = new("--authtype")
        {
            Description = "SQL Authentication type to use."
        };

        #endregion

        #region Connection Options

        public static readonly Option<string> KeyVaultNameOption = new("--keyvaultname", "-kv", "--kv")
        {
            Description = "Name of Azure Key Vault to save secrets to/retrieve from. If provided, secrets will be used/saved to settings file or secrets.yaml"
        };

        public static readonly Option<string> EventHubConnectionOption = new("--eventhubconnection", "-eh", "--eh")
        {
            Description = "Event Hub connection string for Event Hub logging. If using Managed Identity auth, use '<eventhub namespace>|<eventhub name>'"
        };

        public static readonly Option<string> ServiceBusConnectionOption = new("--servicebustopicconnection", "-sb", "--sb")
        {
            Description = "Service Bus connection string for Service Bus topic distribution. If using Managed Identity auth, just provide the Service Bus Namespace"
        };

        public static readonly Option<string> StorageAccountNameOption = new("--storageaccountname")
        {
            Description = "Name of Azure storage account associated build"
        };

        public static readonly Option<string> StorageAccountKeyOption = new("--storageaccountkey")
        {
            Description = "Account Key for the storage account"
        };

        public static readonly Option<string> BatchAccountNameOption = new("--batchaccountname", "--acct")
        {
            Description = "String name of the Azure Batch account"
        };

        public static readonly Option<string> BatchAccountKeyOption = new("--batchaccountkey", "-k")
        {
            Description = "Account Key for the Azure Batch account"
        };

        public static readonly Option<string> BatchAccountUrlOption = new("--batchaccounturl", "-U")
        {
            Description = "URL for the Azure Batch account"
        };

        #endregion

        #region Batch Options

        public static readonly Option<string> BatchJobNameOption = new("--batchjobname", "--jobname")
        {
            Description = "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed"
        };

        public static readonly Option<string> JobNameOption = new("--jobname")
        {
            Description = "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed"
        };

        public static readonly Option<bool> DeleteBatchJobOption = new("--deletebatchjob")
        {
            Description = "Whether or not to delete the batch job after an execution"
        };

        public static readonly Option<bool> DeleteBatchPoolOption = new("--deletebatchpool")
        {
            Description = "Whether or not to delete the batch pool servers after an execution"
        };

        public static readonly Option<bool> PollBatchPoolStatusOption = new("--pollbatchpoolstatus", "--poll")
        {
            Description = "Whether or not you want to get updated status (true) or fire and forget (false)"
        };

        public static readonly Option<OsType> BatchPoolOsOption = new("--batchpoolos", "-os", "--os")
        {
            Description = "Operating system for the Azure Batch nodes. Windows is default"
        };

        public static readonly Option<int> BatchNodeCountOption = new("--batchnodecount", "--nodecount")
        {
            Description = "Number of nodes to provision to run the batch job"
        };

        public static readonly Option<string> BatchVmSizeOption = new("--batchvmsize", "--vmsize")
        {
            Description = "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general)"
        };

        public static readonly Option<string> BatchResourceGroupOption = new("--batchresourcegroup", "--batchrg")
        {
            Description = "The Resource Group name for the Batch Account"
        };

        public static readonly Option<string> BatchPoolNameOption = new("--batchpoolname", "--poolname")
        {
            Description = "Override for the default pool name of \"SqlBuildManagerPool\""
        };

        public static readonly Option<string> BatchApplicationOption = new("--applicationpackage", "--apppackage")
        {
            Description = "The Azure Batch application package name. (Default is 'SqlBuildManagerWindows' for Windows and 'SqlBuildManagerLinux' for Linux"
        };

        public static readonly Option<string> OutputContainerSasUrlOption = new("--outputcontainersasurl")
        {
            Description = "[Internal only] Runtime storage SAS url (auto-generated from `sbm batch run` command",
            Hidden = true
        };

        public static readonly Option<int> BatchJobMonitorTimeoutOption = new("--batchjobmonitortimeout", "--monitortimeout")
        {
            Description = "Timeout (in minutes) for the batch job monitor to wait for the job to complete. Default is 30 minutes"
        };

        #endregion

        #region Identity Options

        public static readonly Option<string> ClientIdOption = new("--clientid")
        {
            Description = "Client ID (AppId) for the Azure User Assigned Managed Identity"
        };

        public static readonly Option<string> PrincipalIdOption = new("--principalid")
        {
            Description = "Principal ID for the Azure User Assigned Managed Identity"
        };

        public static readonly Option<string> ResourceIdOption = new("--resourceid")
        {
            Description = "Resource ID (full resource path) for the Azure User Assigned Managed Identity"
        };

        public static readonly Option<string> IdentityResourceGroupOption = new("--identityresourcegroup", "--idrg")
        {
            Description = "Resource Group name for the Azure User Assigned Managed Identity"
        };

        public static readonly Option<string> IdentityNameOption = new("--identityname", "-id", "--id")
        {
            Description = "Name of User Assigned Managed identity that will be assigned",
            Required = true
        };

        public static readonly Option<string> SubscriptionIdOption = new("--subscriptionid")
        {
            Description = "Azure subscription Id for the Azure resources"
        };

        public static readonly Option<string> TenantIdOption = new("--tenantid")
        {
            Description = "Azure Active Directory Tenant Id for the Identity"
        };

        public static readonly Option<string> ServiceAccountNameOption = new("--serviceaccountname", "--serviceaccount")
        {
            Description = "Kubernetes Service Account used for Workload Identity Authentication"
        };

        #endregion

        #region EventHub Options

        public static readonly Option<EventHubLogging[]> EventHubLoggingTypeOption = new("--eventhublogging")
        {
            Description = "Controls EventHub logging, including how to log script results and if to emit verbose message events.\r\nAdd multiple flags to combine settings.[EssentialOnly|ScriptErrors|IndividualScriptResults|ConsolidatedScriptResults|VerboseMessages]",
            Arity = ArgumentArity.ZeroOrMore
        };

        public static readonly Option<string> EventHubResourceGroupOption = new("--eventhubresourcegroup", "--ehrg")
        {
            Description = "Event Hub resource group. If provided along with --ehsub, the system will attempt to create a customer consumer group for the build"
        };

        public static readonly Option<string> EventHubSubscriptionOption = new("--eventhubsubscriptionid", "--ehsub")
        {
            Description = "Event Hub Subscription Guid. If provided along with --ehrg, the system will attempt to create a customer consumer group for the build"
        };

        public static readonly Option<bool> StreamEventsOption = new("--stream")
        {
            Description = "Stream database Event Log events (database Commit and Error messages)"
        };

        #endregion

        #region Container Registry Options

        public static readonly Option<string> ImageTagOption = new("--imagetag", "--tag")
        {
            Description = "Tag for container image to pull from registry"
        };

        public static readonly Option<string> ImageNameOption = new("--imagename", "--image")
        {
            Description = "Container image to pull from registry"
        };

        public static readonly Option<string> ImageRepositoryOption = new("--registryserver", "--registry")
        {
            Description = "Name of container registry server (if not Docker Hub)"
        };

        public static readonly Option<string> ImageRepositoryUserNameOption = new("--registryusername")
        {
            Description = "Username for private image repository"
        };

        public static readonly Option<string> ImageRepositoryPasswordOption = new("--registrypassword")
        {
            Description = "Password for private image repository"
        };

        #endregion

        #region Container App Options

        public static readonly Option<string> ContainerAppEnvironmentOption = new("--environmentname", "-e")
        {
            Description = "Name of the Container App Environment"
        };

        public static readonly Option<string> ContainerAppLocationOption = new("--location", "-l")
        {
            Description = "Azure location where Container App environment exists"
        };

        public static readonly Option<string> ContainerAppResourceGroupOption = new("--resourcegroup", "-g")
        {
            Description = "Resource group containing the Container App Environment"
        };

        public static readonly Option<int> ContainerAppMaxContainerCountOption = new("--maxcontainers", "--max")
        {
            Description = "Maximum container count when scaling up"
        };

        public static readonly Option<bool> ContainerAppDeleteWhenDoneOption = new("--deletewhendone", "--delete")
        {
            Description = "Delete Container App deployment once job is complete"
        };

        public static readonly Option<bool> ContainerAppEnvOnlyOption = new("--environmentvariablesonly", "--env")
        {
            Description = "Deploy using Environment Variable only ARM template (vs. using Secrets)",
            Hidden = true
        };

        public static readonly Option<FileInfo> ContainerAppArmTemplateOption = new("--templatefile")
        {
            Description = "Container App template to deploy Container App (generated from 'sbm containerapp prep')",
            Required = true
        };

        #endregion

        #region ACI Options

        public static readonly Option<int> AciContainerCountOption = new("--containercount")
        {
            Description = "Number of containers to create for processing",
            Required = true
        };

        public static readonly Option<string> AciInstanceNameOption = new("--aciname")
        {
            Description = "Name of the Azure Container Instance you will create and deploy to",
            Required = true
        };

        public static readonly Option<string> AciInstanceNameNotReqOption = new("--aciname")
        {
            Description = "Name of the Azure Container Instance you will create and deploy to"
        };

        public static readonly Option<string> AciResourceGroupNameOption = new("--aciresourcegroup", "--acirg")
        {
            Description = "Name of the Resource Group for the ACI deployment",
            Required = true
        };

        public static readonly Option<string> AciResourceGroupNameNotReqOption = new("--aciresourcegroup", "--acirg")
        {
            Description = "Name of the Resource Group for the ACI deployment"
        };

        #endregion

        #region Kubernetes Options

        public static readonly Option<int> PodCountOption = new("--podcount")
        {
            Description = "Number of pods to create for the job"
        };

        public static readonly Option<FileInfo> SecretsFileOption = new("--secretsfile")
        {
            Description = "Name of the secrets YAML file to use for retrieving connection values"
        };

        public static readonly Option<FileInfo> RuntimeFileOption = new("--runtimefile")
        {
            Description = "Name of the configmap YAML file to use for retrieving the job name and/or packagename"
        };

        #endregion

        #region Network Options

        public static readonly Option<string> VnetNameOption = new("--vnetname")
        {
            Description = "Name of the VNET to use for the deployment"
        };

        public static readonly Option<string> SubnetNameOption = new("--subnetname")
        {
            Description = "Name of the subnet to use for the deployment"
        };

        public static readonly Option<string> VnetResourceGroupOption = new("--vnetresourcegroup", "--vnetrg")
        {
            Description = "Resource group where VNET is deployed"
        };

        #endregion

        #region Concurrency Options

        public static readonly Option<int> ConcurrencyOption = new("--concurrency")
        {
            Description = "Maximum concurrency for threaded executions"
        };

        public static readonly Option<ConcurrencyType> ConcurrencyTypeOption = new("--concurrencytype")
        {
            Description = "Type of concurrency, used in conjunction with --concurrency"
        };

        public static readonly Option<int> ConcurrencyRequiredOption = new("--concurrency")
        {
            Description = "Maximum concurrency for threaded executions",
            Required = true
        };

        public static readonly Option<ConcurrencyType> ConcurrencyRequiredTypeOption = new("--concurrencytype")
        {
            Description = "Type of concurrency, used in conjunction with --concurrency",
            Required = true
        };

        #endregion

        #region Settings File Options

        public static readonly Option<FileInfo> SettingsFileExistingOption = new("--settingsfile")
        {
            Description = "Saved settings file to load parameters from"
        };

        public static readonly Option<FileInfo> SettingsFileNewOption = new("--settingsfile")
        {
            Description = "Saved settings file to save parameters to",
            Required = true
        };

        public static readonly Option<FileInfo> SettingsFileExistingRequiredOption = new("--settingsfile")
        {
            Description = "Saved settings file to load parameters from",
            Required = true
        };

        public static readonly Option<string> SettingsFileKeyOption = new("--settingsfilekey")
        {
            Description = "Key for the encryption of sensitive information in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable."
        };

        public static readonly Option<string> SettingsFileKeyRequiredOption = new("--settingsfilekey")
        {
            Description = "Key for the encryption of sensitive information in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable.",
            Required = true
        };

        public static readonly Option<bool> ClearTextOption = new("--cleartext")
        {
            Description = "Flag to save settings file in clear text (vs. encrypted)"
        };

        public static readonly Option<bool> DecryptedOption = new("--decrypted")
        {
            Description = "Indicating that the settings file is already in clear text"
        };

        #endregion

        #region Query Options

        public static readonly Option<FileInfo> QueryFileOption = new("--queryfile")
        {
            Description = "File containing the SELECT query to run across the databases"
        };

        public static readonly Option<FileInfo> OutputFileOption = new("--outputfile")
        {
            Description = "Results output file to create"
        };

        #endregion

        #region Miscellaneous Options

        public static readonly Option<DirectoryInfo> PathOption = new("--path")
        {
            Description = "Path to save yaml files"
        };

        public static readonly Option<string> PrefixOption = new("--prefix")
        {
            Description = "Prefix to add to the yaml file names"
        };

        public static readonly Option<bool> SectionPlaceholderOption = new("placeholder")
        {
            Description = ""
        };

        #endregion
    }
}
