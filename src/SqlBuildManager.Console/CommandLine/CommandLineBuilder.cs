using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.CommandLine.Help;
using Spectre.Console;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;

namespace SqlBuildManager.Console.CommandLine
{
    public class CommandLineBuilder

    {
        #region Options
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
        private static Option<string> directoryOption = new Option<string>(new string[] { "--dir", "--directory" }, "Directory containing 1 or more SBX files to package into SBM zip files") { IsRequired = true };
        private static Option<bool> transactionalOption = new Option<bool>(new string[] { "--transactional" }, () => true, "Whether or not to run with a wrapping transaction (default is true)");
        private static Option<int> timeoutretrycountOption = new Option<int>(new string[] { "--timeoutretrycount" }, "How many retries to attempt if a timeout exception occurs");
        private static Option<string> golddatabaseOption = new Option<string>(new string[] { "-gd", "--golddatabase" }, "The \"gold copy\" database that will serve as the model for what the target database should look like") { IsRequired = true };
        private static Option<string> goldserverOption = new Option<string>(new string[] { "-gs", "--goldserver" }, "The server that the \"gold copy\" database can be found") { IsRequired = true };
        private static Option<bool> continueonfailureOption = new Option<bool>(new string[] { "--continueonfailure" }, "Whether or not to continue on the failure of a package (default is false)");
        private static Option<string> platinumdacpacOption = new Option<string>(new string[] { "-pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema");
        private static Option<FileInfo> platinumdacpacFileInfoOption = new Option<FileInfo>(new string[] { "-pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema").ExistingOnly();
        private static Option<string> targetdacpacOption = new Option<string>(new string[] { "-td", "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated");
        private static Option<bool> forcecustomdacpacOption = new Option<bool>(new string[] { "--forcecustomdacpac" }, "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.");
        private static Option<string> platinumdbsourceOption = new Option<string>(new string[] { "--platinumdbsource" }, "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema");
        private static Option<string> platinumserversourceOption = new Option<string>(new string[] { "--platinumserversource" }, "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema");
        private static Option<string> buildrevisionOption = new Option<string>(new string[] { "--buildrevision" }, "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))");
        private static Option<string> outputsbmOption = new Option<string>(new string[] { "-o", "--outputsbm" }, "Name (and path) of the SBM package or SBX file to create");
        private static Option<int> defaultscripttimeoutOption = new Option<int>(new string[] { "--defaultscripttimeout" }, "Override the default script timeouts set when creating a DACPAC");
        private static Option<SqlSync.Connection.AuthenticationType> authtypeOption = new Option<SqlSync.Connection.AuthenticationType>(new string[] { "--authtype" }, "SQL Authentication type to use.") { Name = "AuthenticationType" };
        private static Option<bool> silentOption = new Option<bool>(new string[] { "--silent", "--force" }, () => false, "Suppresses overwrite prompt if file already exists");

        private static Option<string> dacpacOutputOption = new Option<string>(new string[] { "--dacpacname" }, "Name of the dacpac that you want to create") { IsRequired = true };
        private static Option<bool> cleartextOption = new Option<bool>(new string[] { "--cleartext" }, () => false, "Flag to save settings file in clear text (vs. encrypted)");
        private static Option<FileInfo> queryFileOption = new Option<FileInfo>(new string[] { "--queryfile" }, "File containing the SELECT query to run across the databases").ExistingOnly();
        private static Option<FileInfo> outputFileOption = new Option<FileInfo>(new string[] { "--outputfile" }, "Results output file to create");
        private static Option<LogLevel> logLevelOption = new Option<LogLevel>(new string[] { "--loglevel" }, "Logging level for console and log file");
        private static Option<LogLevel> logLevelWithInfoDefaultOption = new Option<LogLevel>(new string[] { "--loglevel" }, () => LogLevel.Information, "Logging level for console and log file");
        private static Option<FileInfo[]> scriptListOption = new Option<FileInfo[]>(new string[] { "-s", "--scripts" }, "List of script files to create SBM package from - add one flag & file per script (will be added in order provided)") { IsRequired = true }.ExistingOnly();
        private static Option<bool> withHashOption = new Option<bool>(new string[] { "-w", "--withhash" }, () => true, "Also include the SHA1 hash of the script files in the package");
        private static Option<FileInfo[]> packagesOption = new Option<FileInfo[]>(new string[] { "-p", "--packages" }, "One or more SBM packages to get contents for") { IsRequired = true }.ExistingOnly();
        private static Option<FileInfo> overrideAsFileOption = new Option<FileInfo>(new string[] { "--override" }, "File containing the target database settings (usually a formatted .cfg file)") { IsRequired = true }.ExistingOnly();
        private static Option<FileInfo> overrideAsFileNotRequiredOption = new Option<FileInfo>(new string[] { "--override" }, "File containing the target database settings (optional, used as a counter for monitoring") { IsRequired = false }.ExistingOnly();
        private static Option<FileInfo> packagenameAsFileToUploadOption = new Option<FileInfo>(new string[] { "-P", "--packagename" }, "Name of the .sbm file to upload").ExistingOnly();
        private static Option<bool> forceOption = new Option<bool>(new string[] { "--force" }, () => false, "Suppresses warning that the storage container/job already exist. Will delete any existing files without warning");
        
        private static Option<bool> allowForObjectDeletionOption = new Option<bool>(new string[] { "--allowobjectdelete" },  "Whether or not to script for database object deletion when creating a package from the comparison of two databases or DACPACS");

        private static Option<FileInfo> platinumdacpacSourceOption = new Option<FileInfo>(new string[] { "-pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema") { IsRequired = true }.ExistingOnly();
        private static Option<FileInfo> targetdacpacSourceOption = new Option<FileInfo>(new string[] { "-td", "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated") { IsRequired = true }.ExistingOnly();

        private static Option<DirectoryInfo> pathOption = new Option<DirectoryInfo>("--path", "Path to save secrets.yaml and runtime.yaml files").ExistingOnly();

        private static Option<bool> unitTestOption = new Option<bool>("--unittest", () => false, "Designation that execution is running as a unit test") { IsHidden = true };
        private static Option<bool> streamEventsOption = new Option<bool>("--stream", () => false, "Stream database Event Log events (database Commit and Error messages)");

        private static Option<bool> decryptedOption = new Option<bool>("--decrypted", "Indicating that the settings file is already in clear text");
        private static Option<string> jobnameOption = new Option<string>(new string[] { "--jobname" }, "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed");


        //K8s Options
        private static Option<FileInfo> secretsFileOption = new Option<FileInfo>(new string[] { "--secretsfile" }, "Name of the secrets YAML file to use for retrieving connection values").ExistingOnly();
        private static Option<FileInfo> runtimeFileOption = new Option<FileInfo>(new string[] { "--runtimefile" }, "Name of the runtime YAML file to use for retrieving the job name and/or packagename").ExistingOnly();


        //ACI Options
        private static Option<int> aciContainerCountOption = new Option<int>("--containercount", "Number of containers to create for processing") { IsRequired = true };
        private static Option<string> aciInstanceNameOption = new Option<string>("--aciname", "Name of the Azure Container Instance you will create and deploy to") { IsRequired = true };
        private static Option<string> aciIResourceGroupNameOption = new Option<string>(new string[] { "--acirg", "--aciresourcegroup" }, "Name of the Resource Group for the ACI deployment") { IsRequired = true };
        private static Option<FileInfo> aciOutputFileOption = new Option<FileInfo>("--outputfile", "File name to save ACI ARM template");
        private static Option<FileInfo> aciArmTemplateOption = new Option<FileInfo>("--templatefile", "ARM template to deploy ACI (generated from 'sbm prep')") { IsRequired = true }.ExistingOnly();
        private static Option<FileInfo> aciArmTemplateNotReqOption = new Option<FileInfo>("--templatefile", "ARM template to deploy ACI (generated from 'sbm aci prep')").ExistingOnly();


        //ContainerApp Options
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
        private static Option<int> containerAppMaxContainerCount = new Option<int>(new string[] { "--max", "--maxcontainers" },  "Maximum container count when scaling up");
        private static Option<bool> containerAppDeleteAppUponCompletion= new Option<bool>(new string[] { "--delete", "--deletewhendone" }, "Delete Container App deployment once job is complete");

        //Batch Options
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

        private static List<Option> BatchComputeOptions
        {
            get
            {
                var list = new List<Option>()
                {
                    batchpoolOsOption,
                    batchpoolnameOption,
                    batchnodecountOption,
                    batchvmsizeOption,
                    batchApplicationOption,
                    deletebatchpoolOption

                };
                return list;
            }
        }
        private static Option<OsType> batchpoolOsOption = new Option<OsType>(new string[] { "-os", "--batchpoolos" }, "Operating system for the Azure Batch nodes. Windows is default");
        private static Option<int> batchnodecountOption = new Option<int>(new string[] { "--nodecount", "--batchnodecount" }, "Number of nodes to provision to run the batch job");
        private static Option<string> batchvmsizeOption = new Option<string>(new string[] { "--vmsize", "--batchvmsize" }, "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) ");
        private static Option<string> batchpoolnameOption = new Option<string>(new string[] { "--poolname", "--batchpoolname" }, "Override for the default pool name of \"SqlBuildManagerPool\"");
        private static Option<bool> deletebatchpoolOption = new Option<bool>(new string[] { "--deletebatchpool" }, () => false, "Whether or not to delete the batch pool servers after an execution");
        private static Option<string> batchApplicationOption = new Option<string>(new string[] { "--apppackage", "--applicationpackage" }, "The Azure Batch application package name. (Default is 'SqlBuildManagerWindows' for Windows and 'SqlBuildManagerLinux' for Linux");
        private static Option<string> outputcontainersasurlOption = new Option<string>(new string[] { "--outputcontainersasurl" }, "[Internal only] Runtime storage SAS url (auto-generated from `sbm batch run` command") { IsHidden = true };


        //Connection and Secrets Options
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
        private static Option<string> keyVaultNameOption = new Option<string>(new string[] { "-kv", "--keyvaultname" }, "Name of Azure Key Vault to save secrets to/retrieve from. If provided, secrets will be used/saved to settings file or secrets.yaml");
        private static Option<string> eventhubconnectionOption = new Option<string>(new string[] { "-eh", "--eventhubconnection" }, "Event Hub connection string for Event Hub logging");
        private static Option<string> serviceBusconnectionOption = new Option<string>(new string[] { "-sb", "--servicebustopicconnection" }, "Service Bus connection string for Service Bus topic distribution");
        private static Option<string> storageaccountnameOption = new Option<string>(new string[] { "--storageaccountname" }, "Name of Azure storage account associated build");
        private static Option<string> storageaccountkeyOption = new Option<string>(new string[] { "--storageaccountkey" }, "Account Key for the storage account");
        private static Option<string> batchaccountnameOption = new Option<string>(new string[] { "--acct", "--batchaccountname" }, "String name of the Azure Batch account");
        private static Option<string> batchaccountkeyOption = new Option<string>(new string[] { "-k", "--batchaccountkey" }, "Account Key for the Azure Batch account");
        private static Option<string> batchaccounturlOption = new Option<string>(new string[] { "-U", "--batchaccounturl" }, "URL for the Azure Batch account");



        //Managed Identity Options
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
                    subscriptionIdOption
                };
                return list;
            }
        }
        private static List<Option> IdentityArgumentsForContainerApp
        {
            get
            {
                var list = new List<Option>()
                {
                    clientIdOption,
                    identityNameOption,
                    identityResourceGroupOption
                };
                return list;
            }
        }
        private static Option<string> clientIdOption = new Option<string>("--clientid", "Client ID (AppId) for the Azure User Assigned Managed Identity");
        private static Option<string> principalIdOption = new Option<string>("--principalid", "Principal ID for the Azure User Assigned Managed Identity");
        private static Option<string> resourceIdOption = new Option<string>("--resourceid", "Resource ID (full resource path) for the Azure User Assigned Managed Identity");
        private static Option<string> identityResourceGroupOption = new Option<string>(new string[] { "--idrg", "--identityresourcegroup" }, "Resource Group name for the Azure User Assigned Managed Identity");
        private static Option<string> identityNameOption = new Option<string>(new string[] { "-id", "--identityname" }, "Name of User Assigned Managed identity that will be assigned") { IsRequired = true };
        private static Option<string> subscriptionIdOption = new Option<string>(new string[] { "--subscriptionid" }, "Azure subscription Id for the Azure resources");


        //Database authentication args
        private static List<Option> DatabaseAuthArgs
        {
            get
            {
                var list = new List<Option>()
                {
                    usernameOption,
                    passwordOption
                };
                return list;
            }
        }
        private static Option<string> usernameOption = new Option<string>(new string[] { "-u", "--username" }, "The username to authenticate against the database if not using integrate auth");
        private static Option<string> passwordOption = new Option<string>(new string[] { "-p", "--password" }, "The password to authenticate against the database if not using integrate auth");

        //Container Registry and Image Options 
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

        //Concurrency Options 
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
        private static Option<int> threadedConcurrencyRequiredTypeOption = new Option<int>(new string[] { "--concurrency" }, "Maximum concurrency for threaded executions") {  IsRequired = true};
        private static Option<ConcurrencyType> threadedConcurrencyRequiredOption = new Option<ConcurrencyType>(new string[] { "--concurrencytype" }, "Type of concurrency, used in conjunction with --concurrency ") { IsRequired = true };


        //Settings File Options
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


        #endregion
        /// <summary>
        /// Create DACPAC from target database
        /// </summary>
        private static Command dacpacCommand
                {
                    get
                    {
                        var cmd = new Command("dacpac", "Creates a DACPAC file from the target database")
                        {
                            authtypeOption,
                            databaseOption.Copy(true),
                            serverOption.Copy(true),
                            dacpacOutputOption
                        };
                        DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                        cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateDacpac);
                        return cmd;
                    }
                }

        #region Local and Threaded Commands

        /// <summary>
        /// Local build command
        /// </summary>
        private static Command buildCommand
        {
            get
            {
                var cmd = new Command("build", "Performs a standard, local SBM execution via command line")
                {
                    packagenameOption.Copy(true),
                    serverOption.Copy(true),
                    databaseOption,
                    rootloggingpathOption,
                    trialOption,
                    transactionalOption,
                    overrideOption,
                    descriptionOption,
                    buildrevisionOption,
                    logtodatabasenamedOption,
                    scriptsrcdirOption,
                    timeoutretrycountOption
                };
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                cmd.Add(authtypeOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunLocalBuildAsync);
                return cmd;
            }
        }

        /// <summary>
        /// Threaded run command
        /// </summary>
        private static Command threadedRunCommand
        {
            get
            {
                var cmd = new Command("run", "For updating multiple databases simultaneously from the current machine")
                {
                    packagenameOption,
                    rootloggingpathOption,
                    trialOption,
                    transactionalOption,
                    overrideOption,
                    descriptionOption,
                    buildrevisionOption,
                    logtodatabasenamedOption,
                    scriptsrcdirOption,
                    platinumdacpacOption,
                    targetdacpacOption,
                    forcecustomdacpacOption,
                    platinumdbsourceOption,
                    platinumserversourceOption,
                    timeoutretrycountOption,
                    defaultscripttimeoutOption, 
                    unitTestOption
                };
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                cmd.Add(authtypeOption);
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.RunThreadedExecution);
                return cmd;
            }
        }

        /// <summary>
        /// Threaded query command
        /// </summary>
        private static Command threadedQueryCommand
        {
            get
            {
                var cmd = new Command("query", "Run a SELECT query across multiple databases")
                {
                    queryFileOption.Copy(true),
                    overrideOption.Copy(true),
                    outputFileOption.Copy(true),
                    defaultscripttimeoutOption,
                    silentOption
                };
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                cmd.Add(authtypeOption);
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.QueryDatabases);
                return cmd;
            }
        }

        /// <summary>
        /// Threaded base commands
        /// </summary>
        private static Command threadedCommand
        {
            get
            {
                var tmp = new Command("threaded", "For updating multiple or querying databases simultaneously from the current machine");
                tmp.Add(threadedQueryCommand);
                tmp.Add(threadedRunCommand);
                return tmp;
            }
        }
        #endregion


        #region Batch Commands

        /// <summary>
        /// Batch running
        /// </summary>
        private static Command batchRunCommand
        {
            get
            {
                var cmd = new Command("run", "For updating multiple databases simultaneously using Azure batch services")
                {
                    overrideOption.Copy(true),
       
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    platinumdacpacOption,
                    packagenameOption.Copy(false),
                    targetdacpacOption,
                    forcecustomdacpacOption,
                    platinumdbsourceOption,
                    platinumserversourceOption,
                    new Option<bool>("--monitor", () => false, "Monitor active progress via Azure Event Hub Events (if configured). To get detailed database statuses, also use the --stream argument"),
                    unitTestOption,
                    streamEventsOption,
                   
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                BatchSettingsOptions.ForEach(o => cmd.Add(o));
                BatchComputeOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ConnectionAndSecretsOptionsForBatch.ForEach(o => cmd.Add(o));
                IdentityArgumentsForBatch.ForEach(o => cmd.Add(o));
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool, bool, bool>(Worker.RunBatchExecution);
                return cmd;
            }
        }

        /// <summary>
        /// Batch threading run -- used to run on Batch node
        /// </summary>
        private static Command batchRunThreadedCommand
        {
            get
            {
                var cmd = new Command("runthreaded", "[Internal use only] - this commmand is used to send threaded commands to Azure Batch Nodes")
                {
                    overrideOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    platinumdacpacOption,
                    packagenameOption.Copy(false),
                    targetdacpacOption,
                    forcecustomdacpacOption,
                    platinumdbsourceOption,
                    platinumserversourceOption,
                    outputcontainersasurlOption,
                    transactionalOption,
                    timeoutretrycountOption,
                    unitTestOption,
                    new Option<bool>("--monitor"){IsHidden = true}, //these two options aren't used and are added just for reusability in unit tests
                    new Option<bool>("--stream"){IsHidden = true},

                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                BatchSettingsOptions.ForEach(o => cmd.Add(o));
                BatchComputeOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ConnectionAndSecretsOptionsForBatch.ForEach(o => cmd.Add(o));
                cmd.Add(authtypeOption);
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.RunThreadedExecution);
                cmd.IsHidden = true;
                return cmd;
            }
        }

        /// <summary>
        /// Batch pre-stage
        /// </summary>
        private static Command batchPreStageCommand
        {
            get
            {
                var cmd = new Command("prestage", "Pre-stage the Azure Batch VM nodes")
                {
                    pollbatchpoolstatusOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                BatchComputeOptions.ForEach(o => {if(o.Name != "deletebatchpool") cmd.Add(o); });
                IdentityArgumentsForBatch.ForEach(o => cmd.Add(o));
                cmd.Add(keyVaultNameOption);
                cmd.Add(batchaccountnameOption);
                cmd.Add(batchaccountkeyOption);
                cmd.Add(batchaccounturlOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunBatchPreStage);
                return cmd;
            }
        }

        /// <summary>
        /// Batch node cleanup
        /// </summary>
        private static Command batchCleanUpCommand
        {
            get
            {
                var cmd = new Command("cleanup", "Azure Batch Clean Up - remove VM nodes")
                {
                    pollbatchpoolstatusOption,
                    keyVaultNameOption,
                    batchaccountnameOption,
                    batchaccountkeyOption,
                    batchaccounturlOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunBatchCleanUp);
                return cmd;
            }
        }

        /// <summary>
        /// Batch delete jobs
        /// </summary>
        private static Command batchDeleteJobsCommand
        {
            get
            {
                var cmd = new Command("deletejobs", "Delete Jobs from the Azure batch account")
                {
                    keyVaultNameOption,
                    batchaccountnameOption,
                    batchaccountkeyOption,
                    batchaccounturlOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.IsHidden = true;
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunBatchJobDelete);
                return cmd;
            }
        }

        /// <summary>
        /// Batch Save settings file
        /// </summary>
        private static Command saveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Save a settings json file for Batch arguments (see Batch documentation)")
                {

                    deletebatchjobOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    //Additional settings
                    timeoutretrycountOption,
                    pollbatchpoolstatusOption,
                    silentOption,
                    cleartextOption
                };
                SettingsFileNewOptions.ForEach(o => cmd.Add(o));
                BatchComputeOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ConnectionAndSecretsOptionsForBatch.ForEach(o => cmd.Add(o));
                IdentityArgumentsForBatch.ForEach(o => cmd.Add(o));
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.SaveAndEncryptSettings);
                return cmd;
            }
        }

        /// <summary>
        /// Batch enqueue database targets
        /// </summary>
        private static Command batchEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    batchjobnameOption.Copy(true),
                    threadedConcurrencyTypeOption.Copy(true),
                    overrideOption.Copy(true),
                    keyVaultNameOption,
                    serviceBusconnectionOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.EnqueueOverrideTargets);
                return cmd;
            }
        }

        /// <summary>
        /// Batch Dequeue datebase targets
        /// </summary>
        private static Command batchDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    batchjobnameOption,
                    threadedConcurrencyTypeOption,
                    keyVaultNameOption,
                    serviceBusconnectionOption,

                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.DeQueueOverrideTargets);
                return cmd;
            }
        }

        /// <summary>
        /// Batch query 
        /// </summary>
        private static Command batchQueryCommand
        {
            get
            {
                var cmd = new Command("query", "Run a SELECT query across multiple databases using Azure Batch")
                {
                    overrideOption.Copy(true),
                    queryFileOption.Copy(true),
                    outputFileOption.Copy(true),
                    silentOption,
                    deletebatchjobOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                BatchComputeOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ConnectionAndSecretsOptionsForBatch.ForEach(o => cmd.Add(o));
                IdentityArgumentsForBatch.ForEach(o => cmd.Add(o));
                cmd.Add(authtypeOption);
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunBatchQuery);
                return cmd;
            }
        }

        /// <summary>
        /// Batch query -- threaded
        /// </summary>
        private static Command batchQueryThreadedCommand
        {
            get
            {
                var cmd = new Command("querythreaded", "[Internal use only] - this commmand is used to send query commands to Azure Batch Nodes")
                {
                    overrideOption.Copy(true),
                    queryFileOption.Copy(true),
                    outputFileOption.Copy(true),
                    deletebatchjobOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    //Batch to Threaded node options
                    outputcontainersasurlOption,
                    transactionalOption,
                    timeoutretrycountOption,
                    silentOption

                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                BatchComputeOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ConnectionAndSecretsOptionsForBatch.ForEach(o => { if (o.Name != "servicebustopicconnection") cmd.Add(o); });
                IdentityArgumentsForBatch.ForEach(o => cmd.Add(o));
                cmd.Add(authtypeOption);
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.QueryDatabases);
                cmd.IsHidden = true;
                return cmd;
            }
        }

        /// <summary>
        /// Azure Batch base command
        /// </summary>
        private static Command batchCommand
        {
            get
            {
                var tmp = new Command("batch", "Commands for setting and executing a batch run or batch query");
                tmp.Add(saveSettingsCommand);
                tmp.Add(batchPreStageCommand);
                tmp.Add(batchEnqueueTargetsCommand);
                tmp.Add(batchRunCommand);
                tmp.Add(batchQueryCommand);
                tmp.Add(batchCleanUpCommand);
                tmp.Add(batchDequeueTargetsCommand);
                tmp.Add(batchRunThreadedCommand);
                tmp.Add(batchQueryThreadedCommand);
                tmp.Add(batchDeleteJobsCommand);
                return tmp;
            }
        }

        #endregion


        #region Container App Commands

        /// <summary>
        /// ContainerApp Save Settings
        /// </summary>
        private static Command containerAppSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Saves settings file for Azure Container App deployments")
                {
                    defaultscripttimeoutOption,
                    cleartextOption,
                    silentOption
                };
                SettingsFileNewOptions.ForEach(o => cmd.Add(o));
                ContainerAppOptions.ForEach(o => cmd.Add(o));
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.IsRequired = false; } cmd.Add(o); });
                ConnectionAndSecretsOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ContainerRegistryAndImageOptions.ForEach(o => cmd.Add(o));
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.SaveAndEncryptContainerAppSettings);
                return cmd;
            }
        }

        /// <summary>
        /// ContainerApp prep
        /// </summary>
        private static Command containerAppPrepCommand
        {
            get
            {
                var cmd = new Command("prep", "Creates an Azure storage container and uploads the SBM and/or DACPAC files that will be used for the build.")
                {
                    jobnameOption.Copy(true),
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    packagenameAsFileToUploadOption,
                    platinumdacpacFileInfoOption,
                    overrideOption,
                    decryptedOption,
                    forceOption, 
                    allowForObjectDeletionOption

                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, bool>(Worker.PrepAndUploadContainerAppBuildPackage);

                return cmd;
            }
        }


        /// <summary>
        /// ContainerApp - enqueue database targets
        /// </summary>
        private static Command containerAppEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption.Copy(true),
                    serviceBusconnectionOption,
                    overrideOption.Copy(true),
                    decryptedOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.EnqueueOverrideTargets);

                return cmd;
            }
        }

        /// <summary>
        /// ContainerApp - Deploy
        /// </summary>
        private static Command containerAppDeployCommand
        {
            get
            {
                var cmd = new Command("deploy", "Deploy the Container App instance using the template file created from 'sbm containerapp prep' and start containers")
                {
                    packagenameOption,
                    platinumdacpacOption,
                    defaultscripttimeoutOption,
                    overrideOption.Copy(false),
                    jobnameOption.Copy(true),
                    decryptedOption,
                    allowForObjectDeletionOption,
                    unitTestOption,
                    streamEventsOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful Container App container deployment"),
                   containerAppEnvOnly,
                   containerAppDeleteAppUponCompletion
                   
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                ContainerAppOptions.ForEach(o => cmd.Add(o));
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.IsRequired = false; } cmd.Add(o); });
                ConnectionAndSecretsOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ContainerRegistryAndImageOptions.ForEach(o => cmd.Add(o));
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool, bool, bool, bool>(Worker.DeployContainerApp);

                return cmd;
            }
        }

        /// <summary>
        /// ContainerApp - monitor
        /// </summary>
        private static Command containerAppMonitorCommand
        {
            get
            {
                var cmd = new Command("monitor", "Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)")
                {
                    jobnameOption,
                    overrideOption,
                    threadedConcurrencyTypeOption,
                    unitTestOption,
                    streamEventsOption,
                    decryptedOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool, DateTime?, bool>(Worker.MonitorContainerAppRuntimeProgress);

                return cmd;
            }
        }

        /// <summary>
        /// ContainerApp - dequeue database targets
        /// </summary>
        private static Command containerAppDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    serviceBusconnectionOption,
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.DeQueueOverrideTargets);

                return cmd;
            }
        }

        private static Command containerAppWorkerTestCommand
        {
            get
            {
                var cmd = new Command("test", "Create environment variables for Container app and run local execution")
                {
                    packagenameOption,
                    overrideOption.Copy(true),
                    jobnameOption.Copy(true),
                    decryptedOption
                };
                SettingsFileExistingOptions.ForEach(o => cmd.Add(o));
                ConcurrencyOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.ContainerAppTestSettings);

                return cmd;
            }
        }

        private static Command containerAppWorkerCommand
        {
            get
            {
                var tmp = new Command("worker", "[Used by Container Apps] Starts the pod as a worker - polling and retrieving items from target service bus queue topic");
                tmp.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunContainerAppWorker);
                tmp.Add(containerAppWorkerTestCommand);

                return tmp;
            }
        }

        /// <summary>
        /// ContainerApp - base command
        /// </summary>
        private static Command containerAppCommand
        {
            get
            {
                var cmd = new Command("containerapp", "Commands for setting and executing a build running in pods on Azure Container App");
                cmd.Add(containerAppSaveSettingsCommand);
                cmd.Add(containerAppPrepCommand);
                cmd.Add(containerAppEnqueueTargetsCommand);
                cmd.Add(containerAppDeployCommand);
                cmd.Add(containerAppMonitorCommand);
                cmd.Add(containerAppDequeueTargetsCommand);
                cmd.Add(containerAppWorkerCommand);

                return cmd;
            }
        }
        #endregion

        #region ACI Commands
        private static Command aciSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Saves settings file for Azure Container Instances container deployments")
                {
                    settingsfileNewOption,
                    aciInstanceNameOption,
                    aciIResourceGroupNameOption,
                    subscriptionIdOption,
                    //Key value option
                    keyVaultNameOption.Copy(true),
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    eventhubconnectionOption,
                    defaultscripttimeoutOption,
                    //threadedConcurrencyOption,
                    //threadedConcurrencyTypeOption,
                    //Service Bus queue options
                    serviceBusconnectionOption,
                    //Additional settings
                    timeoutretrycountOption,
                    silentOption,
                    cleartextOption

                };
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                IdentityArgumentsForContainerApp.ForEach(o => cmd.Add(o));
                ContainerRegistryAndImageOptions.ForEach(o => cmd.Add(o));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.SaveAndEncryptAciSettings);

                return cmd;
            }
        }

        private static Command aciPrepCommand
        {
            get
            {
                var cmd = new Command("prep", "Creates ACI arm template, a storage container, and uploads the SBM and/or DACPAC files that will be used for the build. ")
                {
                    settingsfileExistingOption,
                    aciInstanceNameOption.Copy(false),
                    identityNameOption.Copy(false),
                    identityResourceGroupOption.Copy(false),
                    aciIResourceGroupNameOption.Copy(false),
                    aciContainerCountOption.Copy(true),
                    jobnameOption.Copy(true),
                    packagenameAsFileToUploadOption,
                    overrideOption,
                    platinumdacpacFileInfoOption,
                    aciOutputFileOption,
                    forceOption, 
                    allowForObjectDeletionOption

                };
                ContainerRegistryAndImageOptions.ForEach(o => cmd.Add(o));
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                ConcurrencyRequiredOptions.ForEach(o => cmd.Add(o));

                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, FileInfo, bool>(Worker.PrepAndUploadAciBuildPackage);

                return cmd;
            }
        }

        private static Command aciEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    settingsfileExistingOption,
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption.Copy(true),
                    overrideOption.Copy(true)
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.EnqueueOverrideTargets);

                return cmd;
            }
        }

        private static Command aciDeployCommand
        {
            get
            {
                var cmd = new Command("deploy", "Deploy the ACI instance using the template file created from 'sbm prep' and start containers")
                {
                    aciArmTemplateOption,
                    aciIResourceGroupNameOption.Copy(false),
                    overrideOption.Copy(false),
                    unitTestOption,
                    streamEventsOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful ACI container deployment"), 
                    allowForObjectDeletionOption
                };
                cmd.Add(settingsfileExistingOption);
                ContainerRegistryAndImageOptions.ForEach(o => cmd.Add(o));
                cmd.Add(clientIdOption);
                cmd.Add(subscriptionIdOption.Copy(false));
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, bool, bool, bool>(Worker.DeployAciTemplate);

                return cmd;
            }
        }

        private static Command aciMonitorCommand
        {
            get
            {
                var cmd = new Command("monitor", "Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)")
                {
                    settingsfileExistingOption,
                    aciArmTemplateNotReqOption,
                     jobnameOption,
                    overrideOption,
                    threadedConcurrencyTypeOption,
                    unitTestOption,
                    streamEventsOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, DateTime?, bool, bool>(Worker.MonitorAciRuntimeProgress);

                return cmd;
            }
        }

        private static Command aciDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    settingsfileExistingOption,
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.DeQueueOverrideTargets);

                return cmd;
            }
        }

        private static Command aciWorkerCommand
        {
            get
            {
                var cmd = new Command("worker", "[Used by ACI] Starts the container(s) as a worker - polling and retrieving items from target service bus queue topic");
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunAciQueueWorker);

                return cmd;
            }
        }

        private static Command aciCommand
        {
            get
            {
                var cmd = new Command("aci", "Commands for setting and executing a build running in containers on Azure Container Instances. ACI Containers will always leverage Azure Key Vault.");
                cmd.Add(aciSaveSettingsCommand);
                cmd.Add(aciPrepCommand);
                cmd.Add(aciEnqueueTargetsCommand);
                cmd.Add(aciDeployCommand);
                cmd.Add(aciMonitorCommand);
                cmd.Add(aciDequeueTargetsCommand);
                cmd.Add(aciWorkerCommand);

                return cmd;
            }
        }


        #endregion

        #region Kubernetes Commands
        private static Command kubernetesWorkerCommand
        {
            get
            {
                var cmd = new Command("worker", "[Used by Kubernetes] Starts the pod as a worker - polling and retrieving items from target service bus queue topic");
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunKubernetesQueueWorker);

                return cmd;
            }
        }

        private static Command kubernetesEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    secretsFileOption,
                    runtimeFileOption,
                    jobnameOption,
                    overrideAsFileOption,
                    threadedConcurrencyTypeOption,
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Handler = CommandHandler.Create<FileInfo, FileInfo, string, string, ConcurrencyType, string, FileInfo>(Worker.EnqueueContainerOverrideTargets);


                return cmd;
            }
        }

        private static Command kubernetesSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Saves settings to secrets.yaml and runtime.yaml files for Kubernetes pod deployments")
                {
                    new Option<string>("--prefix", "Settings file's prefix"),
                    pathOption,
                    imageTagOption,
                    imageNameOption,
                    imageRepositoryOption,

                };
                ConnectionAndSecretsOptions.ForEach(o => cmd.Add(o));
                DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                ConcurrencyOptions.ForEach(o => cmd.Add(o));

                cmd.Handler = CommandHandler.Create<CommandLineArgs, string, DirectoryInfo>(Worker.SaveKubernetesSettings);
                return cmd;
            }
        }

        private static Command kubernetesMonitorCommand
        {
            get
            {
                var cmd = new Command("monitor", "Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)")
                {
                    secretsFileOption,
                    runtimeFileOption,
                    jobnameOption,
                    overrideOption,
                    unitTestOption,
                    streamEventsOption,
                    threadedConcurrencyTypeOption,
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.Handler = CommandHandler.Create<FileInfo, FileInfo, CommandLineArgs, bool, bool>(Worker.MonitorKubernetesRuntimeProgress);
                return cmd;
            }
        }

        private static Command kubernetesrPrepCommand
        {
            get
            {
                var cmd = new Command("prep", "Creates a storage container and uploads the SBM and/or DACPAC files that will be used for the build. If the --runtimefile option is provided, it will also update that file with the updated values")
                {
                    secretsFileOption,
                    runtimeFileOption,
                    jobnameOption,
                    packagenameAsFileToUploadOption,
                    platinumdacpacFileInfoOption,
                    overrideOption,
                    forceOption, 
                    allowForObjectDeletionOption

                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.Handler = CommandHandler.Create<FileInfo, FileInfo, FileInfo, FileInfo, string, string, string, string, bool, bool, string>(Worker.UploadKubernetesBuildPackage);
                return cmd;
            }
        }

        private static Command kubernetesDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    secretsFileOption,
                    runtimeFileOption,
                    jobnameOption,
                    keyVaultNameOption,
                    serviceBusconnectionOption,
                    threadedConcurrencyTypeOption

                };
                cmd.Handler = CommandHandler.Create<FileInfo, FileInfo, string, string, ConcurrencyType, string>(Worker.DequeueKubernetesOverrideTargets);
                return cmd;
            }
        }

        private static Command kubernetesCommand
        {
            get
            {
                var cmd = new Command("k8s", "Commands for setting and executing a build running in pods on Kubernetes");
                cmd.Add(kubernetesSaveSettingsCommand);
                cmd.Add(kubernetesrPrepCommand);
                cmd.Add(kubernetesEnqueueTargetsCommand);
                cmd.Add(kubernetesMonitorCommand);
                cmd.Add(kubernetesDequeueTargetsCommand);
                cmd.Add(kubernetesWorkerCommand);
                return cmd;
            }
        }

        #endregion
        public static RootCommand SetUp()
        {
    
            #region Utility Commands
            /****************************************
             * Utility commands 
             ***************************************/

            //Sync two databases
            var synchronizeCommand = new Command("synchronize", "Performs a database synchronization between between --database and --golddatabase. Can only be used for Windows Auth database targets")
            {
                golddatabaseOption,
                goldserverOption,
                databaseOption.Copy(true),
                serverOption.Copy(true),
                continueonfailureOption
            };
            DatabaseAuthArgs.ForEach(o => synchronizeCommand.Add(o));
            synchronizeCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.SyncronizeDatabase);

            //Get SBM run differences between two databases
            var getDifferenceCommand = new Command("getdifference", "Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between --database and --golddatabase. Only supports Windows Auth")
            {
                golddatabaseOption,
                goldserverOption,
                databaseOption.Copy(true),
                serverOption.Copy(true)
            };
            getDifferenceCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.GetDifferences);

            //Create and SBM from and SBX and script files
            var packageCommand = new Command("package", "Creates an SBM package from an SBX configuration file and scripts")
            {
                directoryOption
            };
            packageCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.PackageSbxFilesIntoSbmFiles);

            var packCommand = new Command("pack", "Creates an SBM package from an SBX configuration file and scripts")
            {
                directoryOption
            };
            packCommand.IsHidden = true;
            packCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.PackageSbxFilesIntoSbmFiles);



            //Create from diff
            var createFromDiffCommand = new Command("fromdiff", "Creates an SBM package from a calculated diff between two databases")
            {
                outputsbmOption.Copy(true),
                golddatabaseOption.Copy(true),
                goldserverOption.Copy(true),
                databaseOption.Copy(true),
                serverOption.Copy(true),
                authtypeOption,
                allowForObjectDeletionOption

            };
            createFromDiffCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreatePackageFromDiff);

            //Create from list of scripts
            var createFromScriptsCommand = new Command("fromscripts", "Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension- .sbm or .sbx)")
            {
                outputsbmOption.Copy(true),
                scriptListOption

            };
            createFromScriptsCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreatePackageFromScripts);

            //Create from differences between two DACPACS
            var createFromDacpacsCommand = new Command("fromdacpacs", "Creates an SBM package from differences between two DACPAC files")
            {
                outputsbmOption.Copy(true),
                platinumdacpacSourceOption,
                targetdacpacSourceOption,
                allowForObjectDeletionOption

            };
            createFromDacpacsCommand.Handler = CommandHandler.Create<string, FileInfo, FileInfo, bool>(Worker.CreatePackageFromDacpacs);

            //Create an SBM from a platium DACPAC file
            var createFromDacpacDiffCommand = new Command("fromdacpacdiff", "Extract a SBM package from a source --platinumdacpac and a target database connection")
            {
                platinumdacpacOption.Copy(true),
                outputsbmOption.Copy(true),
                databaseOption.Copy(true),
                serverOption.Copy(true),
                authtypeOption,
                allowForObjectDeletionOption
            };
            DatabaseAuthArgs.ForEach(o => createFromDacpacDiffCommand.Add(o));
            createFromDacpacDiffCommand.Add(authtypeOption);
            createFromDacpacDiffCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateFromDacpacDiff);

            //Create an SBM from a platium DACPAC file (deprecated, but keeping in sync with fromdacpacdiff for now
            var scriptExtractCommand = new Command("scriptextract", "[*Deprecated - please use `sbm create fromdacpacdiff`] Extract a SBM package from a source --platinumdacpac");
            createFromDacpacDiffCommand.Options.ToList().ForEach(o => scriptExtractCommand.AddOption(o));
            scriptExtractCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateFromDacpacDiff);

            //Create
            var createCommand = new Command("create", "Creates an SBM package from script files (fromscripts),  calculated database differences (fromdiff) or diffs between two DACPAC files (fromdacpacs)");
            createCommand.Add(createFromScriptsCommand);
            createCommand.Add(createFromDiffCommand);
            createCommand.Add(createFromDacpacsCommand);
            createCommand.Add(createFromDacpacDiffCommand);


            //Add
            var addCommand = new Command("add", "Adds one or more scripts to an SBM package or SBX project file from a list of scripts")
            {
                outputsbmOption.Copy(true),
                scriptListOption
            };
            addCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.AddScriptsToPackage);

            //List
            var listCommand = new Command("list", "List the script contents (order, script name, date added/modified, user info, script ids, script hashes) for SBM packages. (For SBX, just open the XML file!)")
            {
                packagesOption,
                withHashOption
            };
            listCommand.Handler = CommandHandler.Create<FileInfo[], bool>(Worker.ListPackageScripts);

            //Extract
            var unpackCommand = new Command("unpack", "Unpacks an SBM file into its script files and SBX project file.")
            {
                unpackDirectoryOption,
                new Option<FileInfo>(new string[] { "-p", "--package" }, "Name of the SBM package to unpack") { IsRequired = true }.ExistingOnly()
            };
            unpackCommand.Handler = CommandHandler.Create<DirectoryInfo, FileInfo>(Worker.UnpackSbmFile);

            //Run a policy check
            var policyCheckCommand = new Command("policycheck", "Performs a script policy check on the specified SBM package")
            {
                packagenameOption.Copy(true)
            };
            policyCheckCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.ExecutePolicyCheck);

            //Get the hash of an SBM package
            var getHashCommand = new Command("gethash", "Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)")
            {
                packagenameOption
            };
            getHashCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.GetPackageHash);

            //Create a backout SBM
            var createBackoutCommand = new Command("createbackout", "Generates a backout package (reversing stored procedure and scripted object changes)")
            {
                packagenameOption,
                serverOption.Copy(true),
                databaseOption.Copy(true)
            };
            DatabaseAuthArgs.ForEach(o => createBackoutCommand.Add(o));
            createBackoutCommand.Add(authtypeOption);
            createBackoutCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateBackout);
            #endregion

            //Utility
            var utilJobName = new Option<string>(new string[] { "-j", "--jobname" }, "Name of job run to query") { IsRequired = true };
            var queueUtilityCommand = new Command("queue", "Retrieve the number of messages currently in a Service Bus Topic Subscription")
            {
                utilJobName
            };
            SettingsFileExistingRequiredOptions.ForEach(o => queueUtilityCommand.Add(o));
            queueUtilityCommand.Handler = CommandHandler.Create<CommandLineArgs>(Worker.GetQueueMessageCount);

            var eventHubUtilityCommand = new Command("eventhub", "Retrieve the number of messages in the EventHub for a specific job run.")
            {
                utilJobName,
                new Option<DateTime?>(new string[] { "--date" }, "Date to start counting messages from (will result in faster retrieval if there are a lot of messages)")
            };
            SettingsFileExistingRequiredOptions.ForEach(o => eventHubUtilityCommand.Add(o));
            eventHubUtilityCommand.Handler = CommandHandler.Create<CommandLineArgs, DateTime?>(Worker.GetEventHubEvents);

            var utilityCommand = new Command("utility", "Utility commands for interrogating Service Bus and EventHubs")
            {
                queueUtilityCommand,
                eventHubUtilityCommand
            };


            var authCommand = new Command("authtest", "Test Azure authentication");
            authCommand.Handler = CommandHandler.Create<string>(Worker.TestAuth);
            authCommand.IsHidden = true;

            RootCommand rootCommand = new RootCommand(description: $"Tool to manage your SQL server database updates and releases{Environment.NewLine}Full documentation can be found here: https://github.com/mmckechney/SqlBuildManager#sql-build-manager");
            rootCommand.Add(logLevelOption);
            rootCommand.Add(buildCommand);
            rootCommand.Add(threadedCommand);
            rootCommand.Add(containerAppCommand);
            rootCommand.Add(kubernetesCommand);
            rootCommand.Add(aciCommand);
            rootCommand.Add(batchCommand);
            rootCommand.Add(utilityCommand);
            rootCommand.Add(createCommand);
            rootCommand.Add(addCommand);
            rootCommand.Add(packageCommand);
            rootCommand.Add(packCommand);
            rootCommand.Add(unpackCommand);
            rootCommand.Add(dacpacCommand);
            rootCommand.Add(createBackoutCommand);
            rootCommand.Add(listCommand);
            rootCommand.Add(policyCheckCommand);
            rootCommand.Add(getHashCommand);
            rootCommand.Add(getDifferenceCommand);
            rootCommand.Add(synchronizeCommand);
            //rootCommand.Add(scriptExtractCommand);

            rootCommand.Add(authCommand);

            FirstBuildRunCommand = buildCommand;
            FirstUtilityCommand = utilityCommand;
            FirstPackageManagementCommand = createCommand;
            FirstPackageInformationCommand = listCommand;
            FirstAdditionalCommand = getDifferenceCommand;

            return rootCommand;
        }
        public static Parser GetCommandParser()
        {
            RootCommand rootCommand = SetUp();

            var parser = new System.CommandLine.Builder.CommandLineBuilder(rootCommand)
                       .UseTypoCorrections()
                       .UseDefaults()
                       .UseHelp(ctx =>
                       {
                            ctx.HelpBuilder.CustomizeLayout(_ => HelpBuilder.Default
                                              .GetLayout()
                                              .Prepend(
                                                  _ => AnsiConsole.Write(new FigletText("SQL Build Manager"))
                                              ));
                            ctx.HelpBuilder.CustomizeSymbol(FirstBuildRunCommand,
                                firstColumnText: $"** Build Execution Commands:\u0000{Environment.NewLine}{FirstBuildRunCommand.Name}",
                                secondColumnText: $"\u0000{Environment.NewLine}{FirstBuildRunCommand.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(FirstUtilityCommand,
                                firstColumnText: $"\u0000{Environment.NewLine}** Build Utility Commands:\u0000{Environment.NewLine}{FirstUtilityCommand.Name}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstUtilityCommand.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(FirstPackageManagementCommand,
                                firstColumnText: $"\u0000{Environment.NewLine}** Package Management Commands:\u0000{Environment.NewLine}{FirstPackageManagementCommand.Name}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstPackageManagementCommand.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(FirstPackageInformationCommand,
                                firstColumnText: $"\u0000{Environment.NewLine}** Package Information Commands:\u0000{Environment.NewLine}{FirstPackageInformationCommand.Name}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstPackageInformationCommand.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(FirstAdditionalCommand,
                                firstColumnText: $"\u0000{Environment.NewLine}** Additional Commands:\u0000{Environment.NewLine}{FirstAdditionalCommand.Name}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{FirstAdditionalCommand.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(imageTagOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Container Registry Options:\u0000{Environment.NewLine}{OptionString(imageTagOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{imageTagOption.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(threadedConcurrencyTypeOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Concurrency Options:\u0000{Environment.NewLine}{OptionString(threadedConcurrencyTypeOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{threadedConcurrencyTypeOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(threadedConcurrencyRequiredTypeOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Concurrency Options:\u0000{Environment.NewLine}{OptionString(threadedConcurrencyRequiredTypeOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{threadedConcurrencyRequiredTypeOption.Description}");
                           
                            ctx.HelpBuilder.CustomizeSymbol(settingsfileExistingOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Settings File Options:\u0000{Environment.NewLine}{OptionString(settingsfileExistingOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{settingsfileExistingOption.Description}");

                            ctx.HelpBuilder.CustomizeSymbol(settingsfileNewOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Settings File Options:\u0000{Environment.NewLine}{OptionString(settingsfileNewOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{settingsfileNewOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(settingsfileExistingRequiredOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Settings File Options:\u0000{Environment.NewLine}{OptionString(settingsfileExistingRequiredOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{settingsfileExistingRequiredOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(usernameOption,
                                firstColumnText: $"\u0000{Environment.NewLine}** Database Auth Options:\u0000{Environment.NewLine}{OptionString(usernameOption)}",
                                secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{usernameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(clientIdOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Identity Options:\u0000{Environment.NewLine}{OptionString(clientIdOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{clientIdOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(keyVaultNameOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Connection and Secrets Options:\u0000{Environment.NewLine}{OptionString(keyVaultNameOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{keyVaultNameOption.Description}");
                          
                           ctx.HelpBuilder.CustomizeSymbol(batchpoolOsOption,
                               firstColumnText: $"\u0000{Environment.NewLine}** Batch Pool Compute Options :\u0000{Environment.NewLine}{OptionString(batchpoolOsOption)}",
                               secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{batchpoolOsOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(batchjobnameOption,
                              firstColumnText: $"\u0000{Environment.NewLine}** Batch Job Settings Options :\u0000{Environment.NewLine}{OptionString(batchjobnameOption)}",
                              secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{batchjobnameOption.Description}");

                           ctx.HelpBuilder.CustomizeSymbol(containerAppEnvironmentOption,
                             firstColumnText: $"\u0000{Environment.NewLine}** Container App Deployment Options :\u0000{Environment.NewLine}{OptionString(containerAppEnvironmentOption)}",
                             secondColumnText: $"\u0000{Environment.NewLine}\u0000{Environment.NewLine}{containerAppEnvironmentOption.Description}");


                       })
                       .Build();


            return parser;
        }
        private static string OptionString(Option option)
        {
            var str = string.Join(", ", option.Aliases) + $" <{option.Name}>";

            if (option.IsRequired)
            {
                return str + " (REQUIRED)";
            }
            else
            {
                return str;
            }
        }
        public static (CommandLineArgs, string) ParseArgumentsWithMessage(string[] args)
        {
            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var res = rootCommand.Parse(args);
            if (res.Errors.Count > 0)
            {
                return (null, string.Join<string>(System.Environment.NewLine, res.Errors.Select(e => e.Message).ToArray()));
            }

            var bindingContext = new InvocationContext(rootCommand.Parse(args)).BindingContext;

            var binder = new ModelBinder(typeof(CommandLineArgs));
            var instance = (CommandLineArgs)binder.CreateInstance(bindingContext);

            return (instance, string.Empty);
        }
        public static CommandLineArgs ParseArguments(string[] args)
        {
            (CommandLineArgs cmd, string msg) = ParseArgumentsWithMessage(args);
            if (cmd == null)
            {
                throw new System.Exception($"Unable to parse arguments: {msg}");
            }
            else
            {
                return cmd;
            }
        }

        public static Command FirstBuildRunCommand { get; private set; }
        public static Command FirstUtilityCommand { get; private set; }
        public static Command FirstPackageManagementCommand { get; private set; }

        public static Command FirstPackageInformationCommand { get; private set; }

        public static Command FirstAdditionalCommand { get; private set; }

    }
}
