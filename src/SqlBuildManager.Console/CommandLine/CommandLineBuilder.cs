using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlBuildManager.Console.ContainerShared;
namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder

    {
        private static Option<FileInfo[]> scriptListOption = new Option<FileInfo[]>(new string[] { "-s", "--scripts" }, "List of script files to create SBM package from - add one flag & file per script (will be added in order provided)") { IsRequired = true }.ExistingOnly();

        private static Option<string> overrideOption = new Option<string>(new string[] { "--override" }, "File containing the target database settings (usually a formatted .cfg file)");
        private static Option<string> serverOption = new Option<string>(new string[] { "-s", "--server" }, "1) Name of a server for single database run or 2) source server for scripting or runtime configuration");
        private static Option<string> databaseOption = new Option<string>(new string[] { "-d", "--database" }, "1) Name of a single database to run against or 2) source database for scripting or runtime configuration");
        private static Option<string> rootloggingpathOption = new Option<string>(new string[] { "--rootloggingpath" }, "Directory to save execution logs (for threaded and remote executions)");
        private static Option<bool> trialOption = new Option<bool>(new string[] { "--trial" }, () => false, "Whether or not to run in trial mode (default is false)");
        private static Option<string> scriptsrcdirOption = new Option<string>(new string[] { "--scriptsrcdir" }, " [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)");

        private static Option<string> logtodatabasenamedOption = new Option<string>(new string[] { "--logtodatabasename" }, "[Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target).");
        private static Option<string> descriptionOption = new Option<string>(new string[] { "--description" }, "Description of build (logged with build)");
        private static Option<string> packagenameOption = new Option<string>(new string[] { "-P", "--packagename", "--buildfilename" }, "Name of the .sbm or .sbx file to execute") { Name = "BuildFileName", IsRequired = true };
        private static Option<DirectoryInfo> unpackDirectoryOption = new Option<DirectoryInfo>(new string[] { "--dir", "--directory" }, "Directory to unpack SBM into (will be created if it doesn't exist") { IsRequired = true };
        private static Option<bool> transactionalOption = new Option<bool>(new string[] { "--transactional" }, () => true, "Whether or not to run with a wrapping transaction (default is true)");
        private static Option<int> timeoutretrycountOption = new Option<int>(new string[] { "--timeoutretrycount" }, "How many retries to attempt if a timeout exception occurs");
        private static Option<bool> continueonfailureOption = new Option<bool>(new string[] { "--continueonfailure" }, "Whether or not to continue on the failure of database syncronization (default is false)");
        private static Option<string> platinumdacpacOption = new Option<string>(new string[] { "-pd", "--pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema");
        private static Option<FileInfo> platinumdacpacFileInfoOption = new Option<FileInfo>(new string[] { "-pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema").ExistingOnly();
        private static Option<string> targetdacpacOption = new Option<string>(new string[] { "-td", "--td", "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated");
        private static Option<bool> forcecustomdacpacOption = new Option<bool>(new string[] { "--forcecustomdacpac" }, "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.");
        private static Option<string> platinumdbsourceOption = new Option<string>(new string[] { "--platinumdbsource" }, "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema");
        private static Option<string> platinumserversourceOption = new Option<string>(new string[] { "--platinumserversource" }, "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema");
        private static Option<string> buildrevisionOption = new Option<string>(new string[] { "--buildrevision" }, "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))");
        private static Option<string> outputsbmOption = new Option<string>(new string[] { "-o", "--outputsbm" }, "Name (and path) of the SBM package or SBX file to create");
        private static Option<int> defaultscripttimeoutOption = new Option<int>(new string[] { "--defaultscripttimeout" }, "Override the default script timeouts set when creating a DACPAC");

        private static Option<bool> silentOption = new Option<bool>(new string[] { "--silent", "--force" }, () => false, "Suppresses overwrite prompt if file already exists");

        private static Option<string> dacpacOutputOption = new Option<string>(new string[] { "--dacpacname" }, "Name of the dacpac that you want to create") { IsRequired = true };
        private static Option<bool> cleartextOption = new Option<bool>(new string[] { "--cleartext" }, () => false, "Flag to save settings file in clear text (vs. encrypted)");
        private static Option<FileInfo> queryFileOption = new Option<FileInfo>(new string[] { "--queryfile" }, "File containing the SELECT query to run across the databases").ExistingOnly();
        private static Option<FileInfo> outputFileOption = new Option<FileInfo>(new string[] { "--outputfile" }, "Results output file to create");
        private static Option<LogLevel> logLevelOption = new Option<LogLevel>(new string[] { "--loglevel" }, "Logging level for console and log file");
        private static Option<LogLevel> logLevelWithInfoDefaultOption = new Option<LogLevel>(new string[] { "--loglevel" }, () => LogLevel.Information, "Logging level for console and log file");
 
        private static Option<FileInfo> overrideAsFileOption = new Option<FileInfo>(new string[] { "--override" }, "File containing the target database settings (usually a formatted .cfg file)") { IsRequired = true }.ExistingOnly();
        private static Option<FileInfo> overrideAsFileNotRequiredOption = new Option<FileInfo>(new string[] { "--override" }, "File containing the target database settings (optional, used as a counter for monitoring") { IsRequired = false }.ExistingOnly();
        private static Option<FileInfo> packagenameAsFileToUploadOption = new Option<FileInfo>(new string[] { "-P", "--packagename" }, "Name of the .sbm file to upload").ExistingOnly();
        private static Option<bool> forceOption = new Option<bool>(new string[] { "--force" }, () => false, "Suppresses warning that the storage container/job already exist. Will delete any existing files without warning");

        private static Option<bool> allowForObjectDeletionOption = new Option<bool>(new string[] { "--allowobjectdelete" }, "Whether or not to script for database object deletion when creating a package from the comparison of two databases or DACPACS");

        private static Option<FileInfo> platinumdacpacSourceOption = new Option<FileInfo>(new string[] { "-pd", "--pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema") { IsRequired = true }.ExistingOnly();
        private static Option<FileInfo> targetdacpacSourceOption = new Option<FileInfo>(new string[] { "-td", "--td", "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated") { IsRequired = true }.ExistingOnly();

        private static Option<DirectoryInfo> pathOption = new Option<DirectoryInfo>("--path", "Path to save yaml files").ExistingOnly();
        private static Option<string> prefixOption = new Option<string>(new string[] { "--prefix" }, "Prefix to add to the the yaml file names");

        private static Option<bool> unitTestOption = new Option<bool>("--unittest", () => false, "Designation that execution is running as a unit test") { IsHidden = true };
        private static Option<bool> streamEventsOption = new Option<bool>("--stream", () => false, "Stream database Event Log events (database Commit and Error messages)");

        private static Option<bool> decryptedOption = new Option<bool>("--decrypted", "Indicating that the settings file is already in clear text");
        private static Option<string> jobnameOption = new Option<string>(new string[] { "--jobname" }, "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed");
        private static Option<int> podCountOption = new Option<int>(new string[] { "--podcount" }, "Number of pods to create for the job");


        //K8s Options
        private static Option<FileInfo> secretsFileOption = new Option<FileInfo>(new string[] { "--secretsfile" }, "Name of the secrets YAML file to use for retrieving connection values").ExistingOnly();
        private static Option<FileInfo> runtimeFileOption = new Option<FileInfo>(new string[] { "--runtimefile" }, "Name of the configmap YAML file to use for retrieving the job name and/or packagename").ExistingOnly();

        /// <summary>
        /// Kubernetes Yaml File Options including --runtimefile, --secretsfile
        /// </summary>
        private static List<Option> kubernetesYamlFileOptions = new List<Option> {
            runtimeFileOption, 
            secretsFileOption };

        //ACI Options
        private static Option<int> aciContainerCountOption = new Option<int>("--containercount", "Number of containers to create for processing") { IsRequired = true };
        private static Option<string> aciInstanceNameOption = new Option<string>("--aciname", "Name of the Azure Container Instance you will create and deploy to") { IsRequired = true };
        private static Option<string> aciInstanceNameNotReqOption = new Option<string>("--aciname", "Name of the Azure Container Instance you will create and deploy to") { IsRequired = false };
        private static Option<string> aciIResourceGroupNameOption = new Option<string>(new string[] { "--acirg", "--aciresourcegroup" }, "Name of the Resource Group for the ACI deployment") { IsRequired = true };
        private static Option<string> aciIResourceGroupNameNotReqOption = new Option<string>(new string[] { "--acirg", "--aciresourcegroup" }, "Name of the Resource Group for the ACI deployment") { IsRequired = false };
        //private static Option<FileInfo> aciOutputFileOption = new Option<FileInfo>("--outputfile", "File name to save ACI ARM template");



        //VNET options
        private static Option<string> vnetNameOption = new Option<string>("--vnetname", "Name of the VNET to use for the deployment");
        private static Option<string> vnetResourceGroupOption = new Option<string>(new string[] { "--vnetresourcegroup", "--vnetrg" }, "Resource group where VNET is deployed");
        private static Option<string> subnetNameOption = new Option<string>("--subnetname", "Name of the subnet to use for the deployment");

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
        //private static Option<FileInfo> containerAppOutputReqFileOption = new Option<FileInfo>("--outputfile", "File name to save Container App ARM template") { IsRequired = true };
        private static Option<FileInfo> containerAppArmTemplateOption = new Option<FileInfo>("--templatefile", "Container App template to deploy Container App (generated from 'sbm containerapp prep')") { IsRequired = true }.ExistingOnly();
        private static Option<string> containerAppEnvironmentOption = new Option<string>(new string[] { "-e", "--environmentname" }, "Name of the Container App Environment");
        private static Option<string> containerAppLocationOption = new Option<string>(new string[] { "-l", "--location" }, "Azure location where Container App environment exists");
        private static Option<string> containerAppResourceGroupOption = new Option<string>(new string[] { "-g", "--resourcegroup" }, "Resource group containing the Container App Environment");
        private static Option<bool> containerAppEnvOnly = new Option<bool>(new string[] { "--env", "--environmentvariablesonly" }, "Deploy using Enviroment Variable only ARM template (vs. using Secrets)") { IsHidden = true };
        private static Option<int> containerAppMaxContainerCount = new Option<int>(new string[] { "--max", "--maxcontainers" }, "Maximum container count when scaling up");
        private static Option<bool> containerAppDeleteAppUponCompletion = new Option<bool>(new string[] { "--delete", "--deletewhendone" }, "Delete Container App deployment once job is complete");

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
        private static Option<string> batchjobnameOption = new Option<string>(new string[] { "--jobname", "--batchjobname" }, "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed");
        private static Option<bool> deletebatchjobOption = new Option<bool>(new string[] { "--deletebatchjob" }, () => false, "Whether or not to delete the batch job after an execution");
        private static Option<bool> pollbatchpoolstatusOption = new Option<bool>(new string[] { "--poll", "--pollbatchpoolstatus" }, "Whether or not you want to get updated status (true) or fire and forget (false)");

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
        private static Option<OsType> batchpoolOsOption = new Option<OsType>(new string[] { "-os", "--os", "--batchpoolos" }, "Operating system for the Azure Batch nodes. Windows is default");
        private static Option<int> batchnodecountOption = new Option<int>(new string[] { "--nodecount", "--batchnodecount" }, "Number of nodes to provision to run the batch job");
        private static Option<string> batchvmsizeOption = new Option<string>(new string[] { "--vmsize", "--batchvmsize" }, "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) ");
        private static Option<string> batchResourceGroupOption = new Option<string>(new string[] { "--batchresourcegroup", "--batchrg" }, "The Resource Group name for the Batch Account");
        private static Option<string> batchpoolnameOption = new Option<string>(new string[] { "--poolname", "--batchpoolname" }, "Override for the default pool name of \"SqlBuildManagerPool\"");
        private static Option<bool> deletebatchpoolOption = new Option<bool>(new string[] { "--deletebatchpool" }, () => false, "Whether or not to delete the batch pool servers after an execution");
        private static Option<string> batchApplicationOption = new Option<string>(new string[] { "--apppackage", "--applicationpackage" }, "The Azure Batch application package name. (Default is 'SqlBuildManagerWindows' for Windows and 'SqlBuildManagerLinux' for Linux");
        private static Option<string> outputcontainersasurlOption = new Option<string>(new string[] { "--outputcontainersasurl" }, "[Internal only] Runtime storage SAS url (auto-generated from `sbm batch run` command") { IsHidden = true };
        private static Option<int> batchJobMonitorTimeoutMin = new Option<int>(new string[] { "--monitortimeout", "--batchjobmonitortimeout" }, "Timeout (in minutes) for the batch job monitor to wait for the job to complete. Default is 30 minutes");


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
                    eventhubconnectionOption

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
                    eventhubconnectionOption

                };
                return list;
            }
        }
        private static Option<string> keyVaultNameOption = new Option<string>(new string[] { "-kv", "--kv", "--keyvaultname" }, "Name of Azure Key Vault to save secrets to/retrieve from. If provided, secrets will be used/saved to settings file or secrets.yaml");
        private static Option<string> eventhubconnectionOption = new Option<string>(new string[] { "-eh", "--eh", "--eventhubconnection" }, "Event Hub connection string for Event Hub logging. If using Managed Identity auth, use '<eventhub namespace>|<eventhub name>'");
        private static Option<string> serviceBusconnectionOption = new Option<string>(new string[] { "-sb", "--sb", "--servicebustopicconnection" }, "Service Bus connection string for Service Bus topic distribution. If using Managed Identity auth, just provide the Service Bus Namespace");
        private static Option<string> storageaccountnameOption = new Option<string>(new string[] { "--storageaccountname" }, "Name of Azure storage account associated build");
        private static Option<string> storageaccountkeyOption = new Option<string>(new string[] { "--storageaccountkey" }, "Account Key for the storage account");
        private static Option<string> batchaccountnameOption = new Option<string>(new string[] { "--acct", "--batchaccountname" }, "String name of the Azure Batch account");
        private static Option<string> batchaccountkeyOption = new Option<string>(new string[] { "-k", "--batchaccountkey" }, "Account Key for the Azure Batch account");
        private static Option<string> batchaccounturlOption = new Option<string>(new string[] { "-U", "--batchaccounturl" }, "URL for the Azure Batch account");
        private static Option<EventHubLogging[]> eventHubLoggingTypeOption = new Option<EventHubLogging[]>(new string[] { "--eventhublogging" }, () => new EventHubLogging[] { EventHubLogging.EssentialOnly }, "Controls EventHub logging, including how to log script results and if to emit verbose message events.\r\nAdd multiple flags to combine settings.[EssentialOnly|IndividualScriptResults|ConsolidatedScriptResults|VerboseMessages]")
        {
            Arity = ArgumentArity.ZeroOrMore
        };
        


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
        private static Option<string> clientIdOption = new Option<string>("--clientid", "Client ID (AppId) for the Azure User Assigned Managed Identity");
        private static Option<string> principalIdOption = new Option<string>("--principalid", "Principal ID for the Azure User Assigned Managed Identity");
        private static Option<string> resourceIdOption = new Option<string>("--resourceid", "Resource ID (full resource path) for the Azure User Assigned Managed Identity");
        private static Option<string> identityResourceGroupOption = new Option<string>(new string[] { "--idrg", "--identityresourcegroup" }, "Resource Group name for the Azure User Assigned Managed Identity");
        private static Option<string> identityNameOption = new Option<string>(new string[] { "-id", "--id", "--identityname" }, "Name of User Assigned Managed identity that will be assigned") { IsRequired = true };
        private static Option<string> subscriptionIdOption = new Option<string>(new string[] { "--subscriptionid" }, "Azure subscription Id for the Azure resources");
        private static Option<string> tenantIdOption = new Option<string>(new string[] { "--tenantid" }, "Azure Active Directory Tenant Id for the Identity");
        private static Option<string> serviceAccountNameOption = new Option<string>(new string[] { "--serviceaccountname", "--serviceaccount" }, "Kubernetes Service Account used for Workload Identity Authentication");


        /// <summary>
        /// Database authentication args including: --username, --password and --authtype
        /// </summary>
        private static List<Option> DatabaseAuthArgs
        {
            get
            {
                var list = new List<Option>()
                {
                    usernameOption,
                    passwordOption,
                    authtypeOption
                };
                return list;
            }
        }
        private static Option<string> usernameOption = new Option<string>(new string[] { "-u", "--username" }, "The username to authenticate against the database if not using integrated or Managed Identity auth");
        private static Option<string> passwordOption = new Option<string>(new string[] { "-p", "--password" }, "The password to authenticate against the database if not using integrated or Managed Identity auth");
        private static Option<SqlSync.Connection.AuthenticationType> authtypeOption = new Option<SqlSync.Connection.AuthenticationType>(new string[] { "--authtype" }, "SQL Authentication type to use.") { Name = "AuthenticationType" };

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
        private static Option<string> imageTagOption = new Option<string>(new string[] { "--tag", "--imagetag" }, "Tag for container image to pull from registry");
        private static Option<string> imageNameOption = new Option<string>(new string[] { "--image", "--imagename" }, "Container image to pull from registry");
        private static Option<string> imageRepositoryOption = new Option<string>(new string[] { "--registry", "--registryserver" }, "Name of container registry server (if not Docker Hub)");
        private static Option<string> imageRepositoryUserNameOption = new Option<string>(new string[] { "--registryusername" }, "Username for private image repository");
        private static Option<string> imageRepositoryPasswordOption = new Option<string>(new string[] { "--registrypassword" }, "Password for private image repository");

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
        private static Option<int> threadedConcurrencyOption = new Option<int>(new string[] { "--concurrency" }, "Maximum concurrency for threaded executions");
        private static Option<ConcurrencyType> threadedConcurrencyTypeOption = new Option<ConcurrencyType>(new string[] { "--concurrencytype" }, "Type of concurrency, used in conjunction with --concurrency ");

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
        private static Option<int> threadedConcurrencyRequiredTypeOption = new Option<int>(new string[] { "--concurrency" }, "Maximum concurrency for threaded executions") { IsRequired = true };
        private static Option<ConcurrencyType> threadedConcurrencyRequiredOption = new Option<ConcurrencyType>(new string[] { "--concurrencytype" }, "Type of concurrency, used in conjunction with --concurrency ") { IsRequired = true };


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
        private static Option<FileInfo> settingsfileExistingOption = new Option<FileInfo>(new string[] { "--settingsfile" }, "Saved settings file to load parameters from") { Name = "FileInfoSettingsFile" }.ExistingOnly();
        private static Option<string> settingsfileKeyOption = new Option<string>(new string[] { "--settingsfilekey" }, "Key for the encryption of sensitive informtation in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable. If not provided a machine value will be used.");
        private static Option<FileInfo> settingsfileNewOption = new Option<FileInfo>(new string[] { "--settingsfile" }, "Saved settings file to load parameters from") { Name = "FileInfoSettingsFile", IsRequired = true };

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
        private static Option<FileInfo> settingsfileExistingRequiredOption = new Option<FileInfo>(new string[] { "--settingsfile" }, "Saved settings file to load parameters from") { Name = "FileInfoSettingsFile", IsRequired = true }.ExistingOnly();
        private static Option<string> settingsfileKeyRequiredOption = new Option<string>(new string[] { "--settingsfilekey" }, "Key for the encryption of sensitive informtation in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable. If not provided a machine value will be used.") { IsRequired = true };

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

            FirstBuildRunCommand = BuildCommand;
            FirstUtilityCommand = UtilityCommand;
            FirstPackageManagementCommand = CreateCommand;
            FirstPackageInformationCommand = ListCommand;
            FirstAdditionalCommand = GetDifferenceCommand;

            return rootCommand;
        }


    }
}
