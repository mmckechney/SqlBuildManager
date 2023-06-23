using Azure.ResourceManager.Network.Models;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace SqlBuildManager.Console
{
    class Validation
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static int ValidateUserNameAndPassword(CommandLineArgs cmdLine, out string[] errorMessages)
        {
            string error = string.Empty;
            errorMessages = new string[0];

            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.AzureADPassword || cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.Password)
            {
                if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    error = "The --username and --password arguments are required when authentication type is set to Password or AzurePassword.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.FinishingWithErrors };
                    log.LogError(error);
                    return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
                }
            }

            //Validate that if username or password is specified, then both are (not required if set to ManagedIdentity)
            if (cmdLine.AuthenticationArgs.AuthenticationType != AuthenticationType.ManagedIdentity && (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password)))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    error = "The --username and --password arguments must be used together in command line of --settingsfile Json.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.FinishingWithErrors };
                    log.LogError(error);
                    return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
                }
            }

            return 0;
        }
        /// <summary>
        /// Validates that the basic command line arguments are correct with no conflicts or missing elements
        /// </summary>
        /// <param name="cmdLine">Incomming CommandLineArgs object</param>
        /// <param name="errorMessages">Any errors that are generated</param>
        /// <returns>Zero (0) if valid, otherwise an error code</returns>
        public static int ValidateCommonCommandLineArgs(CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int pwVal = ValidateUserNameAndPassword(cmdLine, out errorMessages);
            if (pwVal != 0)
            {
                return pwVal;
            }
            string error = string.Empty;

            //Validate and set the value for the root logging path
            //if (string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            //{
            //    string msg = "Invalid command line set. Missing --rootloggingpath setting.";
            //    log.LogError(msg);
            //    return -99;
            //}

            //Check that they haven't set --trial=true and --transaction=false
            if (cmdLine.Transactional == false && cmdLine.Trial == true)
            {
                error = "Invalid command line combination. You cannot have --transactional=\"false\" and --trial=\"true\".";
                errorMessages = new string[] { error, "Returning error code:" + (int)ExecutionReturn.InvalidTransactionAndTrialCombo };
                log.LogError(error);
                return (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            }

            //Validate the presence of an --override setting
            if (string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName) && string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                error = "Invalid command line set. Missing --override option.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingOverrideFlag };
                log.LogError(error);
                return (int)ExecutionReturn.MissingOverrideFlag;
            }

            //Validate and set the value for the build file name
            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName) && string.IsNullOrWhiteSpace(cmdLine.ScriptSrcDir)
                && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac)
                && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
            {
                error = "Invalid command line set. Missing --packagename, --platinumdacpac, --scriptsrcdir, or --platinumdbsource and --platinumserversource options.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                log.LogError(error);
                return (int)ExecutionReturn.MissingBuildFlag;
            }

            //If using Platinum DB source, make sure we have both DB and Server arguments
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) || !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) || string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
                {
                    error = "The --platinumdbsource and --platinumserversource options must be used together";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                    log.LogError(error);
                    return (int)ExecutionReturn.MissingBuildFlag;
                }
            }

            //Validate that the build file exists if specified
            if (cmdLine.BuildFileName.Length != 0 && !File.Exists(cmdLine.BuildFileName))
            {
                error = "Missing Build file. The build file specified: " + cmdLine.BuildFileName + " could not be found";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
                log.LogError(error);
                return (int)ExecutionReturn.InvalidBuildFileNameValue;
            }

            //Validate that the Platinum dacpac file exists if specified
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac) && !File.Exists(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                error = "Missing Platinum dacpac file. The  Platinum dacpac specified: " + cmdLine.DacPacArgs.PlatinumDacpac + " could not be found";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
                log.LogError(error);
                return (int)ExecutionReturn.InvalidBuildFileNameValue;
            }

            if (cmdLine.ScriptSrcDir.Length > 0)
            {
                if (!Directory.Exists(cmdLine.ScriptSrcDir))
                {
                    error = "Invalid --scriptsrcdir setting. The directory '" + cmdLine.ScriptSrcDir + "' does not exist.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidScriptSourceDirectory };
                    log.LogError(error);
                    return (int)ExecutionReturn.InvalidScriptSourceDirectory;
                }
            }

            if (cmdLine.TimeoutRetryCount < 0)
            {

                error = "The --timeoutretrycount setting is a negative number. This value needs to be a positive integer.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.NegativeTimeoutRetryCount };
                log.LogError(error);
                return (int)ExecutionReturn.NegativeTimeoutRetryCount;
            }

            if (cmdLine.TimeoutRetryCount > 0 && !cmdLine.Transactional)
            {

                error = "The --timeoutretrycount setting is not allowed when --transactional=false";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.BadRetryCountAndTransactionalCombo };
                log.LogError(error);
                return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
            }


            if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName))
            {
                if (!File.Exists(cmdLine.MultiDbRunConfigFileName))
                {
                    error = string.Format("Specified --override file does not exist at path: {0}", cmdLine.MultiDbRunConfigFileName);
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                    log.LogError(error);
                    return (int)ExecutionReturn.InvalidOverrideFlag;
                }
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName)) //should have already seen if this was required up above
            {
                if (cmdLine.MultiDbRunConfigFileName.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
                {

                    if (string.IsNullOrWhiteSpace(cmdLine.Database) || string.IsNullOrWhiteSpace(cmdLine.Server) ||
                        string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                    {
                        error = $"Invalid command line set. When the --override setting specifies a SQL file, the following are also required:{System.Environment.NewLine} --database, --server - will be used as source to run scripts {System.Environment.NewLine} --username, --password - provide authentication to that database";
                        errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                        log.LogError(error);
                        return (int)ExecutionReturn.InvalidOverrideFlag;
                    }

                }
                else if (!cmdLine.MultiDbRunConfigFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase)
                    && !cmdLine.MultiDbRunConfigFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase)
                    && !cmdLine.MultiDbRunConfigFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
                {

                    error = "Invalid command line set. The '--override' setting file value must be .multiDb, .multiDbQ or .cfg file.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                    log.LogError(error);
                    return (int)ExecutionReturn.InvalidOverrideFlag;
                }
            }

            return 0;
        }

        /// <summary>
        /// Accepts a Multi-Database configuration file, processes it and outputs a populated MultiDbData object
        /// </summary>
        /// <param name="multiDbOverrideSettingFileName">Valid Multi-database file (.multiDb, .multiDbQ, .cfg)</param>
        /// <param name="multiData">Out parameter of populated MultiDbData object</param>
        /// <param name="errorMessages">Out parameter or error messages (if any)</param>
        /// <returns>Zero (0) if no errors, otherwise an error code</returns>
        public static int ValidateAndLoadMultiDbData(string multiDbOverrideSettingFileName, CommandLineArgs cmdLine, out MultiDbData multiData, out string[] errorMessages)
        {
            log.LogInformation("Validating target database settings");
            string message = string.Empty;
            string error;
            errorMessages = new string[0];
            multiData = null;
            string extension = Path.GetExtension(multiDbOverrideSettingFileName).ToLowerInvariant();

            switch (extension)
            {
                case ".multidb":
                    multiData = MultiDbHelper.DeserializeMultiDbConfiguration(multiDbOverrideSettingFileName);
                    break;
                case ".multidbq":
                    multiData = MultiDbHelper.CreateMultiDbConfigFromQueryFile(multiDbOverrideSettingFileName, out message);
                    break;
                case ".sql":
                    if (cmdLine != null)
                    {
                        ConnectionData connData = GetConnDataFromCommandLine(cmdLine);
                        multiData = MultiDbHelper.CreateMultiDbConfigFromQuery(connData, File.ReadAllText(cmdLine.MultiDbRunConfigFileName), out message);
                    }
                    break;
                case ".cfg":
                default:
                    multiData = MultiDbHelper.ImportMultiDbTextConfig(multiDbOverrideSettingFileName);
                    break;


            }
            //if (multiDbOverrideSettingFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase))
            //    multiData = MultiDbHelper.DeserializeMultiDbConfiguration(multiDbOverrideSettingFileName);
            //else if (multiDbOverrideSettingFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase))
            //    multiData = MultiDbHelper.CreateMultiDbConfigFromQueryFile(multiDbOverrideSettingFileName, out message);
            //else if (multiDbOverrideSettingFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
            //    multiData = MultiDbHelper.ImportMultiDbTextConfig(multiDbOverrideSettingFileName);


            if (multiData == null || multiData.Count() == 0)
            {
                error = "Unable to read in configuration file " + multiDbOverrideSettingFileName + ((message.Length > 0) ? " :: " + message : "");
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.NullMultiDbConfig };
                log.LogError(error);
                return (int)ExecutionReturn.NullMultiDbConfig;
            }

            if (!MultiDbHelper.ValidateMultiDatabaseData(multiData))
            {
                error = "One or more scripts is missing a default or target override database setting. Run has been halted. Please correct the error and try again";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingTargetDbOverrideSetting };
                log.LogError(error);
                return (int)ExecutionReturn.MissingTargetDbOverrideSetting;
            }
            return 0;
        }
        private static ConnectionData GetConnDataFromCommandLine(CommandLineArgs cmdLine)
        {
            ConnectionData connData = new ConnectionData(cmdLine.Server, cmdLine.Database);
            connData.UserId = cmdLine.AuthenticationArgs.UserName;
            connData.Password = cmdLine.AuthenticationArgs.Password;
            connData.AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType;

            return connData;
        }
        public static (int, CommandLineArgs) ValidateAndLoadPlatinumDacpac(CommandLineArgs cmdLine, MultiDbData multiDb)
        {
            //DacPac settings validation
            if (!String.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                if (!File.Exists(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    string err = String.Format("A Platinum Dacpac file was specified but could not be located at '{0}'", cmdLine.DacPacArgs.PlatinumDacpac);
                    log.LogError(err);
                    return (-729, cmdLine);
                }

                if (!String.IsNullOrEmpty(cmdLine.DacPacArgs.TargetDacpac) && !File.Exists(cmdLine.DacPacArgs.TargetDacpac))
                {
                    string err = String.Format("A Target Dacpac file was specified but could not be located at '{0}'", cmdLine.DacPacArgs.TargetDacpac);
                    log.LogError(err);
                    return (-728, cmdLine);
                }
            }


            //If there are Dacpac settings... we will need to create the SBM automatically..
            if (!string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac) && string.IsNullOrEmpty(cmdLine.BuildFileName))
            {
                if (cmdLine.DacPacArgs.ForceCustomDacPac == false)
                {
                    string sbmName;
                    var stat = Worker.GetSbmFromDacPac(cmdLine, multiDb, out sbmName, true);
                    if (stat == DacpacDeltasStatus.Success)
                    {
                        cmdLine.BuildFileName = sbmName;
                        return ((int)ExecutionReturn.Successful, cmdLine);
                    }
                    else if (stat == DacpacDeltasStatus.InSync)
                    {
                        return ((int)ExecutionReturn.DacpacDatabasesInSync, cmdLine);
                    }
                    else
                    {
                        log.LogError("Error creating SBM package from Platinum dacpac");
                        return (-5120, cmdLine);
                    }
                }
                else
                {
                    log.LogInformation("Found --forcecustomdacpac setting. Skipping the creation of the single platinum SBM package. Individual dacpacs and SBMs will be created");
                }
            }

            return (0, cmdLine);
        }

        public static int ValidateBatchArguments(CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int returnVal = 0;
            List<string> messages = new List<string>();
            if (String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountName))
            {
                messages.Add("--batchaccountname is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                messages.Add("--batchaccountkey is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountUrl))
            {
                messages.Add("--batchaccounturl is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                messages.Add("--storageaccountname is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            {
                messages.Add("--storageaccountkey is required in command line or --settingsfile json if a Managed Identity is not included");
                returnVal = -888;
            }

            if (String.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchVmSize))
            {
                messages.Add("--batchvmsize, is required in command line or --settingsfile Json");
                returnVal = -888;
            }

            if (!String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) && string.IsNullOrEmpty(cmdLine.BatchArgs.BatchJobName))
            {
                messages.Add("When --servicebusconnection is provided in command line or --settingsfile Json, then --batchjobname is required");
                returnVal = -888;
            }

            (int ret, string msg) = ValidateBatchjobName(cmdLine.BatchArgs.BatchJobName);
            if (ret != 0)
            {
                messages.Add(msg);
                returnVal = ret;
            }

            errorMessages = messages.ToArray();
            return returnVal;
        }

        public static (int, string) ValidateBatchjobName(string batchJobName)
        {
            if (!String.IsNullOrEmpty(batchJobName))
            {
                if (batchJobName.Length < 3 || batchJobName.Length > 41 || !Regex.IsMatch(batchJobName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
                {
                    return (-888, $"The value for --jobname must be: lower case, between 3 and 41 characters in length, and the only special character allowed are dashes '-'{Environment.NewLine}\tThis requirement is because the job name is also the storage container name and needs to accomodate a timestamp: https://docs.microsoft.com/en-us/rest/api/storageservices/Naming-and-Referencing-Containers--Blobs--and-Metadata");
                }
            }
            return (0, "");
        }

        public static int ValidateBatchPreStageArguments(ref CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int returnVal = 0;
            List<string> messages = new List<string>();
            if (String.IsNullOrEmpty(cmdLine.ConnectionArgs.BatchAccountName))
            {
                messages.Add("--batchaccountname is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                messages.Add("--batchaccountkey is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.ConnectionArgs.BatchAccountUrl))
            {
                messages.Add("--batchaccounturl is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchVmSize))
            {
                messages.Add("--batchvmsize is required in command line or --settingsfile  Json");
                returnVal = -888;
            }

            errorMessages = messages.ToArray();
            return returnVal;
        }

        public static int ValidateBatchCleanUpArguments(ref CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int returnVal = 0;
            List<string> messages = new List<string>();
            if (String.IsNullOrEmpty(cmdLine.ConnectionArgs.BatchAccountName))
            {
                messages.Add("--batchaccountname is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                messages.Add("--batchaccountkey is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.ConnectionArgs.BatchAccountUrl))
            {
                messages.Add("--batchaccounturl is required in command line or --settingsfile  Json");
                returnVal = -888;
            }

            errorMessages = messages.ToArray();
            return returnVal;
        }

        public static int ValidateQueryArguments(ref CommandLineArgs cmdLine)
        {

            if (cmdLine.QueryFile == null || !cmdLine.QueryFile.Exists)
            {
                log.LogError("The --queryfile file was not found. Please check the name or path and try again");
                return 2;
            }

            Regex noNo = new Regex(@"(UPDATE\s)|(INSERT\s)|(DELETE\s)", RegexOptions.IgnoreCase);
            var query = File.ReadAllText(cmdLine.QueryFile.FullName);
            if (noNo.Match(query).Success)
            {
                log.LogError($"An INSERT, UPDATE or DELETE keyword was found. You can not use the query function to modify data. Instead, please run your data modification script as a SQL Build Package or DACPAC update");
                return 5;
            }
            if (!File.Exists(cmdLine.MultiDbRunConfigFileName) && string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("The --override file was not found. Please check the name or path and try again");
                return 3;
            }
            if (cmdLine.OutputFile.Exists && !cmdLine.Silent)
            {
                System.Console.WriteLine("The output file already exists. Do you want to overwrite it (Y/N)?");
                var keypressed = System.Console.ReadKey();
                if (keypressed.Key != ConsoleKey.Y)
                {
                    log.LogInformation("Exiting");
                    return 1;
                }
            }

            return 0;
        }

        public static List<string> ValidateContainerAppArgs(CommandLineArgs cmdLine)
        {

            List<string> messages = new List<string>();
            if (String.IsNullOrEmpty(cmdLine.ContainerAppArgs.EnvironmentName))
            {
                messages.Add("--environmentname is required in command line or --settingsfile");
            }
            if (String.IsNullOrEmpty(cmdLine.ContainerRegistryArgs.ImageTag))
            {
                messages.Add("--imaagetag is required in command line or --settingsfile");
            }
            if (String.IsNullOrEmpty(cmdLine.ContainerRegistryArgs.ImageName))
            {
                messages.Add("--imagename is required in command line or --settingsfile");
            }
            if (String.IsNullOrEmpty(cmdLine.ContainerAppArgs.SubscriptionId))
            {
                messages.Add("--subscriptionid is required in command line or --settingsfile");
            }
            if (String.IsNullOrEmpty(cmdLine.ContainerAppArgs.ResourceGroup))
            {
                messages.Add("--resourcegroup is required in command line or --settingsfile");
            }
            if (String.IsNullOrEmpty(cmdLine.ContainerAppArgs.Location))
            {
                messages.Add("--location is required in command line or --settingsfile");
            }

            messages.AddRange(Validation.ValidateUserNameAndPasswordArgs(cmdLine));
            messages.AddRange(Validation.ValidateStorageAccountArgs(cmdLine));
            messages.AddRange(Validation.ValidateServiceBusAndEventHubArgs(cmdLine));

            return messages;

        }

        public static List<string> ValidateAciAppArgs(CommandLineArgs cmdLine)
        {
            List<string> messages = new List<string>();
            if (string.IsNullOrWhiteSpace(cmdLine.AciArgs.AciName))
            {
                messages.Add("--aciname is required in command line or --settingsfile");
            }
            if (string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ResourceGroup))
            {
                messages.Add("--resourcegroup is required in command line or --settingsfile");
            }
            if (string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.IdentityName))
            {
                messages.Add("--identityname is required in command line or --settingsfile");
            }
            if (cmdLine.AciArgs.ContainerCount == 0)
            {
                messages.Add("--containercount is required in command line or --settingsfile");
            }

            if (string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.ImageTag))
            {
                messages.Add("--imagetag is required in command line or --settingsfile");
            }
            return messages;
        }

        public static List<string> ValidateStorageAccountArgs(CommandLineArgs cmdLine)
        {
            List<string> messages = new List<string>();
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                messages.Add("--storageaccountname is required in command line or --settingsfile");
            }

            if (String.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            {
                messages.Add("--storageaccountkey is required in command line or --settingsfile if a Managed Identity is not included");
            }

            return messages;
        }
        public static List<string> ValidateUserNameAndPasswordArgs(CommandLineArgs cmdLine)
        {
            List<string> messages = new List<string>();

            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.AzureADPassword || cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.Password)
            {
                if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    messages.Add("The --username and --password arguments are required when authentication type is set to Password or AzurePassword.");
                }
            }

            //Validate that if username or password is specified, then both are
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    messages.Add("The --username and --password arguments must be used together in command line or --settingsfile.");
                }
            }

            return messages;
        }

        public static List<string> ValidateServiceBusAndEventHubArgs(CommandLineArgs cmdLine)
        {
            List<string> messages = new List<string>();

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                messages.Add("--servicebustopicconnection is required in command line or --settingsfile");
            }
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                messages.Add("--eventhubconnection is required in command line or --settingsfile");
            }

            return messages;
        }

        public static (bool, CommandLineArgs) ValidateContainerQueueArgs(CommandLineArgs cmdLine, string keyvaultname, string jobname, ConcurrencyType concurrencytype, string servicebustopicconnection)
        {

            cmdLine.KeyVaultName = keyvaultname;
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);

            if (!string.IsNullOrWhiteSpace(servicebustopicconnection))
            {
                cmdLine.ServiceBusTopicConnection = servicebustopicconnection;
            }
            if (!string.IsNullOrWhiteSpace(jobname))
            {
                cmdLine.JobName = jobname;
            }
            if (concurrencytype != ConcurrencyType.Count)
            {
                cmdLine.ConcurrencyType = concurrencytype;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("The ServiceBusTopicConnection is required as either a --servicebustopicconnection parameter or as part of the --secretsfile");
                return (false, cmdLine);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                log.LogError("The JobName is required as either a --jobname parameter or as part of the --runtimefile");
                return (false, cmdLine);
            }

            return (true, cmdLine);
        }

    }
}
