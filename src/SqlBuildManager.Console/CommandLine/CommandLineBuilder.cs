using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlBuildManager.Console.ContainerShared;
namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder

    {
        internal static Option<FileInfo[]> scriptListOption = new Option<FileInfo[]>("--scripts", "-s") { Description = "List of script files to create SBM package from - add one flag & file per script (will be added in order provided)" , Required = true };

        internal static Option<string> overrideOption = new Option<string>("--override") { Description = "File containing the target database settings (usually a formatted .cfg file)" };
        internal static Option<string> overrideRequiredOption = new Option<string>("--override") { Description = "File containing the target database settings (usually a formatted .cfg file)", Required = true };
        internal static Option<string> serverOption = new Option<string>("--server", "-s") { Description = "1) Name of a server for single database run or 2) source server for scripting or runtime configuration" };
        internal static Option<string> serverRequiredOption = new Option<string>("--server", "-s") { Description = "1) Name of a server for single database run or 2) source server for scripting or runtime configuration", Required = true };
        internal static Option<string> databaseOption = new Option<string>("--database", "-d") { Description = "1) Name of a single database to run against or 2) source database for scripting or runtime configuration" };
        internal static Option<string> databaseRequiredOption = new Option<string>("--database", "-d") { Description = "1) Name of a single database to run against or 2) source database for scripting or runtime configuration", Required = true };
        internal static Option<string> rootloggingpathOption = new Option<string>("--rootloggingpath") { Description = "Directory to save execution logs (for threaded and remote executions)" };
        internal static Option<bool> trialOption = new Option<bool>("--trial") { Description = "Whether or not to run in trial mode (default is false)" };
        internal static Option<string> scriptsrcdirOption = new Option<string>("--scriptsrcdir") { Description = " [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)" };

        internal static Option<string> logtodatabasenamedOption = new Option<string>("--logtodatabasename") { Description = "[Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target)." };
        internal static Option<string> descriptionOption = new Option<string>("--description") { Description = "Description of build (logged with build)" };
        internal static Option<string> packagenameOption = new Option<string>("--buildfilename", "--packagename", "-P") { Description = "Name of the .sbm or .sbx file to execute" , Required = true };
        internal static Option<string> packagenameRequiredOption = new Option<string>("--buildfilename", "--packagename", "-P") { Description = "Name of the .sbm or .sbx file to execute" , Required = true };
        internal static Option<string> packagenameNotReqOption = new Option<string>("--buildfilename", "--packagename", "-P") { Description = "Name of the .sbm or .sbx file to execute" };
        internal static Option<DirectoryInfo> unpackDirectoryOption = new Option<DirectoryInfo>("--directory", "--dir") { Description = "Directory to unpack SBM into (will be created if it doesn't exist" , Required = true };
        internal static Option<bool> transactionalOption = new Option<bool>("--transactional") { Description = "Whether or not to run with a wrapping transaction (default is true)" };
        internal static Option<int> timeoutretrycountOption = new Option<int>("--timeoutretrycount") { Description = "How many retries to attempt if a timeout exception occurs" };
        internal static Option<bool> continueonfailureOption = new Option<bool>("--continueonfailure") { Description = "Whether or not to continue on the failure of database syncronization (default is false)" };
        internal static Option<string> platinumdacpacOption = new Option<string>("--platinumdacpac", "--pd", "-pd") { Description = "Name of the dacpac containing the platinum schema" };
        internal static Option<string> platinumdacpacRequiredOption = new Option<string>("--platinumdacpac", "--pd", "-pd") { Description = "Name of the dacpac containing the platinum schema", Required = true };
        internal static Option<FileInfo> platinumdacpacFileInfoOption = new Option<FileInfo>("--platinumdacpac", "-pd") { Description = "Name of the dacpac containing the platinum schema" };
        internal static Option<string> targetdacpacOption = new Option<string>("--targetdacpac", "--td", "-td") { Description = "Name of the dacpac containing the schema of the database to be updated" };
        internal static Option<bool> forcecustomdacpacOption = new Option<bool>("--forcecustomdacpac") { Description = "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer." };
        internal static Option<string> platinumdbsourceOption = new Option<string>("--platinumdbsource") { Description = "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema" };
        internal static Option<string> platinumserversourceOption = new Option<string>("--platinumserversource") { Description = "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema" };
        internal static Option<string> buildrevisionOption = new Option<string>("--buildrevision") { Description = "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))" };
        internal static Option<string> outputsbmOption = new Option<string>("--outputsbm", "-o") { Description = "Name (and path) of the SBM package or SBX file to create" };
        internal static Option<string> outputsbmRequiredOption = new Option<string>("--outputsbm", "-o") { Description = "Name (and path) of the SBM package or SBX file to create", Required = true };
        internal static Option<int> defaultscripttimeoutOption = new Option<int>("--defaultscripttimeout") { Description = "Override the default script timeouts set when creating a DACPAC" };

        internal static Option<bool> silentOption = new Option<bool>("--silent", "--force") { Description = "Suppresses overwrite prompt if file already exists" };

        internal static Option<string> dacpacOutputOption = new Option<string>("--dacpacname") { Description = "Name of the dacpac that you want to create" , Required = true };
        internal static Option<bool> cleartextOption = new Option<bool>("--cleartext") { Description = "Flag to save settings file in clear text (vs. encrypted)" };
        internal static Option<FileInfo> queryFileOption = new Option<FileInfo>("--queryfile") { Description = "File containing the SELECT query to run across the databases" };
        internal static Option<FileInfo> queryFileRequiredOption = new Option<FileInfo>("--queryfile") { Description = "File containing the SELECT query to run across the databases", Required = true };
        internal static Option<FileInfo> outputFileOption = new Option<FileInfo>("--outputfile") { Description = "Results output file to create" };
        internal static Option<FileInfo> outputFileRequiredOption = new Option<FileInfo>("--outputfile") { Description = "Results output file to create", Required = true };
        internal static Option<LogLevel> logLevelOption = new Option<LogLevel>("--loglevel") { Description = "Logging level for console and log file" };
        internal static Option<LogLevel> logLevelWithInfoDefaultOption = new Option<LogLevel>("--loglevel") { Description = "Logging level for console and log file" };

        internal static Option<FileInfo> overrideAsFileOption = new Option<FileInfo>("--override") { Description = "File containing the target database settings (usually a formatted .cfg file)", Required = true };
        internal static Option<FileInfo> overrideAsFileNotRequiredOption = new Option<FileInfo>("--override") { Description = "File containing the target database settings (optional, used as a counter for monitoring)" };
        internal static Option<FileInfo> packagenameAsFileToUploadOption = new Option<FileInfo>("--packagename", "-P") { Description = "Name of the .sbm file to upload" };
        internal static Option<bool> forceOption = new Option<bool>("--force") { Description = "Suppresses warning that the storage container/job already exist. Will delete any existing files without warning" };

        internal static Option<bool> allowForObjectDeletionOption = new Option<bool>("--allowobjectdelete") { Description = "Whether or not to script for database object deletion when creating a package from the comparison of two databases or DACPACS" };

        internal static Option<FileInfo> platinumdacpacSourceOption = new Option<FileInfo>("--platinumdacpac", "--pd", "-pd") { Description = "Name of the dacpac containing the platinum schema" , Required = true };
        internal static Option<FileInfo> targetdacpacSourceOption = new Option<FileInfo>("--targetdacpac", "--td", "-td") { Description = "Name of the dacpac containing the schema of the database to be updated" , Required = true };

        internal static Option<DirectoryInfo> pathOption = new Option<DirectoryInfo>("--path") { Description = "Path to save yaml files" };
        internal static Option<string> prefixOption = new Option<string>("--prefix") { Description = "Prefix to add to the the yaml file names" };

        internal static Option<bool> unitTestOption = new Option<bool>("--unittest") { Description = "Designation that execution is running as a unit test", Hidden = true };
        

        internal static Option<bool> decryptedOption = new Option<bool>("--decrypted") { Description = "Indicating that the settings file is already in clear text" };
        internal static Option<string> jobnameOption = new Option<string>("--jobname") { Description = "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed" };
        internal static Option<string> jobnameRequiredOption = new Option<string>("--jobname") { Description = "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed", Required = true };
        internal static Option<int> podCountOption = new Option<int>("--podcount") { Description = "Number of pods to create for the job" };


        //K8s Options
        internal static Option<FileInfo> secretsFileOption = new Option<FileInfo>("--secretsfile") { Description = "Name of the secrets YAML file to use for retrieving connection values" };
        internal static Option<FileInfo> runtimeFileOption = new Option<FileInfo>("--runtimefile") { Description = "Name of the configmap YAML file to use for retrieving the job name and/or packagename" };

        /// <summary>
        /// Kubernetes Yaml File Options including --runtimefile, --secretsfile
        /// </summary>
        private static List<Option> kubernetesYamlFileOptions = new List<Option> {
            runtimeFileOption,
            secretsFileOption };

        //ACI Options
        internal static Option<int> aciContainerCountOption = new Option<int>("--containercount") { Description = "Number of containers to create for processing", Required = true };
        internal static Option<int> aciContainerCountRequiredOption = new Option<int>("--containercount") { Description = "Number of containers to create for processing", Required = true };
        internal static Option<string> aciInstanceNameOption = new Option<string>("--aciname") { Description = "Name of the Azure Container Instance you will create and deploy to", Required = true };
        internal static Option<string> aciInstanceNameNotReqOption = new Option<string>("--aciname") { Description = "Name of the Azure Container Instance you will create and deploy to" };
        internal static Option<string> aciIResourceGroupNameOption = new Option<string>("--aciresourcegroup", "--acirg") { Description = "Name of the Resource Group for the ACI deployment", Required = true };
        internal static Option<string> aciIResourceGroupNameNotReqOption = new Option<string>("--aciresourcegroup", "--acirg") { Description = "Name of the Resource Group for the ACI deployment" };
        //private static Option<FileInfo> aciOutputFileOption = new Option<FileInfo>("--outputfile", "File name to save ACI ARM template");



        //VNET options
        internal static Option<string> vnetNameOption = new Option<string>("--vnetname") { Description = "Name of the VNET to use for the deployment" };
        internal static Option<string> vnetResourceGroupOption = new Option<string>("--vnetresourcegroup", "--vnetrg") { Description = "Resource group where VNET is deployed" };
        internal static Option<string> subnetNameOption = new Option<string>("--subnetname") { Description = "Name of the subnet to use for the deployment" };

        /// <summary>
        /// Vnet Options including: --vnetname, --subnetname, --vnetresourcegroup
        /// </summary>
        private static List<Option> VnetOptions
        {
            get
            {
                var list = new List<Option>
                {
                    vnetNameOption,
                    subnetNameOption,
                    vnetResourceGroupOption
                };
                return list;
            }
        }


        /// <summary>
        /// ContainerApp Options including: --environmet, --location, --subscriptionid, --resourcegroup, --maxcontainers
        /// </summary>
        private static List<Option> ContainerAppOptions
        {
            get
            {
                var list = new List<Option>()
                        {
                            containerAppEnvironmentOption,
                            containerAppLocationOption,
                            subscriptionIdOption,
                            containerAppResourceGroupOption,
                            containerAppMaxContainerCount
                       };
                return list;
            }
        }
        //private static Option<FileInfo> containerAppOutputReqFileOption = new Option<FileInfo>("--outputfile") { Description = "File name to save Container App ARM template", Required = true };
        internal static Option<FileInfo> containerAppArmTemplateOption = new Option<FileInfo>("--templatefile") { Description = "Container App template to deploy Container App (generated from 'sbm containerapp prep')", Required = true };
        internal static Option<string> containerAppEnvironmentOption = new Option<string>("--environmentname", "-e") { Description = "Name of the Container App Environment" };
        internal static Option<string> containerAppLocationOption = new Option<string>("--location", "-l") { Description = "Azure location where Container App environment exists" };
        internal static Option<string> containerAppResourceGroupOption = new Option<string>("--resourcegroup", "-g") { Description = "Resource group containing the Container App Environment" };
        internal static Option<bool> containerAppEnvOnly = new Option<bool>("--environmentvariablesonly", "--env") { Description = "Deploy using Enviroment Variable only ARM template (vs. using Secrets)" , Hidden = true };
        internal static Option<int> containerAppMaxContainerCount = new Option<int>("--maxcontainers", "--max") { Description = "Maximum container count when scaling up" };
        internal static Option<bool> containerAppDeleteAppUponCompletion = new Option<bool>("--deletewhendone", "--delete") { Description = "Delete Container App deployment once job is complete" };

        /// <summary>
        /// Batch Options including: --batchjobname, --deletebatchjob
        /// </summary>
        private static List<Option> BatchSettingsOptions
        {
            get
            {
                var list = new List<Option>()
                        {

                            batchjobnameOption,
                            deletebatchjobOption
                        };
                return list;
            }
        }
        internal static Option<string> batchjobnameOption = new Option<string>("--batchjobname", "--jobname") { Description = "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed" };
        internal static Option<string> batchjobnameRequiredOption = new Option<string>("--batchjobname", "--jobname") { Description = "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed", Required = true };
        internal static Option<bool> deletebatchjobOption = new Option<bool>("--deletebatchjob") { Description = "Whether or not to delete the batch job after an execution" };
        internal static Option<bool> pollbatchpoolstatusOption = new Option<bool>("--pollbatchpoolstatus", "--poll") { Description = "Whether or not you want to get updated status (true) or fire and forget (false)" };

        /// <summary>
        /// Batch Compute Options including: --batchresourcegroup, --batchpoolos, --batchpoolname, --batchnodecount, --batchvmsize, --applicationpackage, --deletebatchpool
        /// </summary>
        private static List<Option> BatchComputeOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    batchResourceGroupOption,
                    batchpoolOsOption,
                    batchpoolnameOption,
                    batchnodecountOption,
                    batchvmsizeOption,
                    batchApplicationOption,
                    deletebatchpoolOption
                };
                list.AddRange(VnetOptions);
                return list;
            }
        }
        internal static Option<OsType> batchpoolOsOption = new Option<OsType>("--batchpoolos", "--os", "-os") { Description = "Operating system for the Azure Batch nodes. Windows is default" };
        internal static Option<int> batchnodecountOption = new Option<int>("--batchnodecount", "--nodecount") { Description = "Number of nodes to provision to run the batch job" };
        internal static Option<string> batchvmsizeOption = new Option<string>("--batchvmsize", "--vmsize") { Description = "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) " };
        internal static Option<string> batchResourceGroupOption = new Option<string>("--batchresourcegroup", "--batchrg") { Description = "The Resource Group name for the Batch Account" };
        internal static Option<string> batchpoolnameOption = new Option<string>("--poolname", "--batchpoolname") { Description = "Override for the default pool name of \"SqlBuildManagerPool\"" };
        internal static Option<bool> deletebatchpoolOption = new Option<bool>("--deletebatchpool") { Description = "Whether or not to delete the batch pool servers after an execution" };
        internal static Option<string> batchApplicationOption = new Option<string>("--applicationpackage", "--apppackage") { Description = "The Azure Batch application package name. (Default is 'SqlBuildManagerWindows' for Windows and 'SqlBuildManagerLinux' for Linux" };
        internal static Option<string> outputcontainersasurlOption = new Option<string>("--outputcontainersasurl") { Description = "[Internal only] Runtime storage SAS url (auto-generated from `sbm batch run` command" , Hidden = true };
        internal static Option<int> batchJobMonitorTimeoutMin = new Option<int>("--batchjobmonitortimeout", "--monitortimeout") { Description = "Timeout (in minutes) for the batch job monitor to wait for the job to complete. Default is 30 minutes" };


        /// <summary>
        /// Connection and Secrets Options including: --keyvaultname, --storageaccountname, --storageaccountkey, --servicebustopicconnection, --eventhubconnection
        /// </summary>
        private static List<Option> ConnectionAndSecretsOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    keyVaultNameOption,
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    serviceBusconnectionOption,
                    eventhubconnectionOption,

            };
                return list;
            }
        }
        /// <summary>
        /// Connection and Secrets Options for Batch including: --keyvaultname, -storageaccountname, --storageaccountkey, --servicebustopicconnection, --eventhubconnection, --batchaccountname, --batchaccountkey,--batchaccounturl
        /// </summary>
        private static List<Option> ConnectionAndSecretsOptionsForBatch
        {
            get
            {
                var list = new List<Option>()
                {
                    keyVaultNameOption,
                    batchaccountnameOption,
                    batchaccountkeyOption,
                    batchaccounturlOption,
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    serviceBusconnectionOption,
                    eventhubconnectionOption,

                };
                return list;
            }
        }
        internal static Option<string> keyVaultNameOption = new Option<string>("--keyvaultname", "--kv", "-kv") { Description = "Name of Azure Key Vault to save secrets to/retrieve from. If provided, secrets will be used/saved to settings file or secrets.yaml" };
        internal static Option<string> eventhubconnectionOption = new Option<string>("--eventhubconnection", "--eh", "-eh") { Description = "Event Hub connection string for Event Hub logging. If using Managed Identity auth, use '<eventhub namespace>|<eventhub name>'" };
        internal static Option<string> serviceBusconnectionOption = new Option<string>("--servicebustopicconnection", "--sb", "-sb") { Description = "Service Bus connection string for Service Bus topic distribution. If using Managed Identity auth, just provide the Service Bus Namespace" };
        internal static Option<string> storageaccountnameOption = new Option<string>("--storageaccountname") { Description = "Name of Azure storage account associated build" };
        internal static Option<string> storageaccountkeyOption = new Option<string>("--storageaccountkey") { Description = "Account Key for the storage account" };
        internal static Option<string> batchaccountnameOption = new Option<string>("--batchaccountname", "--acct") { Description = "String name of the Azure Batch account" };
        internal static Option<string> batchaccountkeyOption = new Option<string>("--batchaccountkey", "-k") { Description = "Account Key for the Azure Batch account" };
        internal static Option<string> batchaccounturlOption = new Option<string>("--batchaccounturl", "-U") { Description = "URL for the Azure Batch account" };


        internal static Option<EventHubLogging[]> eventHubLoggingTypeOption = new Option<EventHubLogging[]>("--eventhublogging") { Description = "Controls EventHub logging, including how to log script results and if to emit verbose message events.\r\nAdd multiple flags to combine settings.[EssentialOnly|ScriptErrors|IndividualScriptResults|ConsolidatedScriptResults|VerboseMessages]", Arity = ArgumentArity.ZeroOrMore };
        internal static Option<string> eventhubResourceGroupOption = new Option<string>("--eventhubresourcegroup", "--ehrg") { Description = "Event Hub resource group. If provided along eith --ehsub, the system will attempt to create a customer consumer group for the build" };
        internal static Option<string> eventhubSubscriptionOption = new Option<string>("--eventhubsubscriptionid", "--ehsub") { Description = "Event Hub Subscription Guid. If provided along with --ehrg, the system will attempt to create a customer consumer group for the build" };
        internal static Option<bool> streamEventsOption = new Option<bool>("--stream") { Description = "Stream database Event Log events (database Commit and Error messages)" };
        private static List<Option> EventHubResourceOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    eventHubLoggingTypeOption,
                    eventhubResourceGroupOption,
                    eventhubSubscriptionOption,
                    streamEventsOption
                };
                return list;
            }
        }

        /// <summary>
        /// Managed Identity Options for Batch including: --clientid, --principalid, --resourceid, --identityresourcegroup, --subscriptionid, --identityname, --tenantid
        /// </summary>
        private static List<Option> IdentityArgumentsForBatch
        {
            get
            {
                var list = new List<Option>()
                {
                    clientIdOption,
                    principalIdOption,
                    resourceIdOption,
                    identityResourceGroupOption,
                    subscriptionIdOption,
                    identityNameOption,
                    tenantIdOption
                };
                return list;
            }
        }
        /// <summary>
        /// Managed Identity options for Container App including: --clientid, --identityname, --identityresourcegroup, --tenantid"
        /// </summary>
        private static List<Option> IdentityArgumentsForContainerApp
        {
            get
            {
                var list = new List<Option>()
                {
                    clientIdOption,
                    identityNameOption,
                    identityResourceGroupOption,
                    tenantIdOption
                };
                return list;
            }
        }
        /// <summary>
        /// //Managed Identity Options for Kubernetes including: --serviceaccountname and --tenantid
        /// </summary>
        private static List<Option> IdentityArgumentsForKubernetes
        {
            get
            {
                var list = new List<Option>()
                {
                    serviceAccountNameOption,
                    tenantIdOption
                };
                return list;
            }
        }
        internal static Option<string> clientIdOption = new Option<string>("--clientid") { Description = "Client ID (AppId) for the Azure User Assigned Managed Identity" };
        internal static Option<string> principalIdOption = new Option<string>("--principalid") { Description = "Principal ID for the Azure User Assigned Managed Identity" };
        internal static Option<string> resourceIdOption = new Option<string>("--resourceid") { Description = "Resource ID (full resource path) for the Azure User Assigned Managed Identity" };
        internal static Option<string> identityResourceGroupOption = new Option<string>("--identityresourcegroup", "--idrg") { Description = "Resource Group name for the Azure User Assigned Managed Identity" };
        internal static Option<string> identityResourceGroupNotReqOption = new Option<string>("--identityresourcegroup", "--idrg") { Description = "Resource Group name for the Azure User Assigned Managed Identity" };
        internal static Option<string> identityNameOption = new Option<string>("--identityname", "--id", "-id") { Description = "Name of User Assigned Managed identity that will be assigned" };
        internal static Option<string> identityNameNotReqOption = new Option<string>("--identityname", "--id", "-id") { Description = "Name of User Assigned Managed identity that will be assigned" };
        internal static Option<string> subscriptionIdOption = new Option<string>("--subscriptionid") { Description = "Azure subscription Id for the Azure resources" };
        internal static Option<string> subscriptionIdNotReqOption = new Option<string>("--subscriptionid") { Description = "Azure subscription Id for the Azure resources" };
        internal static Option<string> tenantIdOption = new Option<string>("--tenantid") { Description = "Azure Active Directory Tenant Id for the Identity" };
        internal static Option<string> serviceAccountNameOption = new Option<string>("--serviceaccountname", "--serviceaccount") { Description = "Kubernetes Service Account used for Workload Identity Authentication" };


        /// <summary>
        /// Database authentication args including: --username, --password, --authtype, and --platform
        /// </summary>
        private static List<Option> DatabaseAuthArgs
        {
            get
            {
                var list = new List<Option>()
                {
                    usernameOption,
                    passwordOption,
                    authtypeOption,
                    platformOption
                };
                return list;
            }
        }
        internal static Option<string> usernameOption = new Option<string>("--username", "-u") { Description = "The username to authenticate against the database if not using integrated or Managed Identity auth" };
        internal static Option<string> passwordOption = new Option<string>("--password", "-p") { Description = "The password to authenticate against the database if not using integrated or Managed Identity auth" };
        internal static Option<SqlSync.Connection.AuthenticationType> authtypeOption = new Option<SqlSync.Connection.AuthenticationType>("--authtype") { Description = "SQL Authentication type to use."  };
        internal static Option<SqlSync.Connection.DatabasePlatform> platformOption = new Option<SqlSync.Connection.DatabasePlatform>("--platform", "--databaseplatform") { Description = "Target database platform (default: SqlServer)." };

        /// <summary>
        /// Container Registry and Image Options including "--imagetag, --imagename, --registryserver, --registryusername, --registrypassword
        /// </summary>
        private static List<Option> ContainerRegistryAndImageOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    imageTagOption,
                    imageNameOption,
                    imageRepositoryOption,
                    imageRepositoryUserNameOption,
                    imageRepositoryPasswordOption
                };
                return list;
            }
        }
        internal static Option<string> imageTagOption = new Option<string>("--imagetag", "--tag") { Description = "Tag for container image to pull from registry" };
        internal static Option<string> imageNameOption = new Option<string>("--imagename", "--image") { Description = "Container image to pull from registry" };
        internal static Option<string> imageRepositoryOption = new Option<string>("--registryserver", "--registry") { Description = "Name of container registry server (if not Docker Hub)" };
        internal static Option<string> imageRepositoryUserNameOption = new Option<string>("--registryusername") { Description = "Username for private image repository" };
        internal static Option<string> imageRepositoryPasswordOption = new Option<string>("--registrypassword") { Description = "Password for private image repository" };

        /// <summary>
        /// Concurrency Options including --concurrency and --concurrencytype as OPTIONAL flags
        /// </summary>
        private static List<Option> ConcurrencyOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    threadedConcurrencyTypeOption,
                    threadedConcurrencyOption

                };
                return list;

            }
        }
        internal static Option<int> threadedConcurrencyOption = new Option<int>("--concurrency") { Description = "Maximum concurrency for threaded executions" };
        internal static Option<ConcurrencyType> threadedConcurrencyTypeOption = new Option<ConcurrencyType>("--concurrencytype") { Description = "Type of concurrency, used in conjunction with --concurrency " };
        internal static Option<ConcurrencyType> threadedConcurrencyTypeRequiredOption = new Option<ConcurrencyType>("--concurrencytype") { Description = "Type of concurrency, used in conjunction with --concurrency ", Required = true };

        /// <summary>
        /// Concurrency Options including --concurrency and --concurrencytype as REQUIRED flags
        /// </summary>
        private static List<Option> ConcurrencyRequiredOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    threadedConcurrencyRequiredTypeOption,
                    threadedConcurrencyRequiredOption

                };
                return list;

            }
        }
        internal static Option<int> threadedConcurrencyRequiredTypeOption = new Option<int>("--concurrency") { Description = "Maximum concurrency for threaded executions" , Required = true };
        internal static Option<ConcurrencyType> threadedConcurrencyRequiredOption = new Option<ConcurrencyType>("--concurrencytype") { Description = "Type of concurrency, used in conjunction with --concurrency " , Required = true };


        /// <summary>
        /// Settings File Options including: --settingsfile (as existing FileInfo) and --settingsfilekey
        /// </summary>
        private static List<Option> SettingsFileExistingOptions
        {
            get
            {
                var lst = new List<Option>()
                {
                    settingsfileExistingOption,
                    settingsfileKeyOption
                };
                return lst;
            }
        }
        /// <summary>
        /// Settings File Options including: --settingsfile and --settingsfilekey (for creating NEW settings files
        /// </summary>
        private static List<Option> SettingsFileNewOptions
        {
            get
            {
                var lst = new List<Option>()
                {
                    settingsfileNewOption,
                    settingsfileKeyOption
                };
                return lst;
            }
        }
        internal static Option<FileInfo> settingsfileExistingOption = new Option<FileInfo>("--settingsfile") { Description = "Saved settings file to load parameters from"  };
        internal static Option<string> settingsfileKeyOption = new Option<string>("--settingsfilekey") { Description = "Key for the encryption of sensitive information in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable." };
        internal static Option<FileInfo> settingsfileNewOption = new Option<FileInfo>("--settingsfile") { Description = "Saved settings file to load parameters from" , Required = true };

        /// <summary>
        /// Settings File Options including: --settingsfile (as REQUIRED and existing FileInfo) and --settingsfilekey (REQUIRED)
        /// </summary>
        private static List<Option> SettingsFileExistingRequiredOptions
        {
            get
            {
                var lst = new List<Option>()
                {
                    settingsfileExistingRequiredOption,
                    settingsfileKeyRequiredOption
                };
                return lst;
            }
        }
        internal static Option<FileInfo> settingsfileExistingRequiredOption = new Option<FileInfo>("--settingsfile") { Description = "Saved settings file to load parameters from" , Required = true };
        internal static Option<string> settingsfileKeyRequiredOption = new Option<string>("--settingsfilekey") { Description = "Key for the encryption of sensitive information in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable." , Required = true };

        private static Option<bool> sectionPlaceholderOption = new Option<bool>("placeholder");
    
        
        public static RootCommand SetUp()
        {

            RootCommand rootCommand = new RootCommand(description: $"Tool to manage your SQL server database updates and releases{Environment.NewLine}Full documentation can be found here: https://github.com/mmckechney/SqlBuildManager#sql-build-manager");
            rootCommand.Add(logLevelOption);
            rootCommand.Add(BuildCommand);
            rootCommand.Add(ThreadedCommand);
            rootCommand.Add(ContainerAppCommand);
            rootCommand.Add(KubernetesCommand);
            rootCommand.Add(AciCommand);
            rootCommand.Add(BatchCommand);
            rootCommand.Add(UtilityCommand);
            rootCommand.Add(CreateCommand);
            rootCommand.Add(AddScriptsCommand);
            rootCommand.Add(PackageCommand);
            rootCommand.Add(UnpackCommand);
            rootCommand.Add(DacpacCommand);
            rootCommand.Add(CreateBackoutCommand);
            rootCommand.Add(ListCommand);
            rootCommand.Add(PolicyCheckCommand);
            rootCommand.Add(GetHashCommand);
            rootCommand.Add(GetDifferenceCommand);
            rootCommand.Add(SynchronizeCommand);
            rootCommand.Add(ShowCommandsCommand);

            FirstBuildRunCommand = BuildCommand;
            FirstUtilityCommand = UtilityCommand;
            FirstPackageManagementCommand = CreateCommand;
            FirstPackageInformationCommand = ListCommand;
            FirstAdditionalCommand = GetDifferenceCommand;

            return rootCommand;
        }


    }
}
