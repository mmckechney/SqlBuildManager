﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.Connection;
using System.Text.RegularExpressions;
namespace SqlBuildManager.Console
{
    class Validation
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static int ValidateUserNameAndPassword(ref CommandLineArgs cmdLine, out string[] errorMessages)
        {
            string error = string.Empty;
            errorMessages = new string[0];

            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.AzureADPassword || cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.Password)
            {
                if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    error = "The --username and --password arguments are required when authentication type is set to Password or AzurePassword.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.FinishingWithErrors };
                    log.Error(error);
                    return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
                }
            }

            //Validate that if username or password is specified, then both are
            if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                if(string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    error = "The --username and --password arguments must be used together in command line of --settingsfile Json.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.FinishingWithErrors };
                    log.Error(error);
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
        public static int ValidateCommonCommandLineArgs(ref CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int pwVal = ValidateUserNameAndPassword(ref cmdLine, out errorMessages);
            if(pwVal != 0)
            {
                 return pwVal;
            }
            string error = string.Empty;

            //Validate and set the value for the root logging path
            if (string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                string msg = "Invalid command line set. Missing --rootloggingpath setting.";
                log.Error(msg);
                return -99;
            }

            //Check that they haven't set /Trial=true and /Transaction=false
            if (cmdLine.Transactional == false && cmdLine.Trial == true)
            {
                error = "Invalid command line combination. You cannot have --transactional=\"false\" and --trial=\"true\".";
                errorMessages = new string[] { error, "Returning error code:" + (int)ExecutionReturn.InvalidTransactionAndTrialCombo };
                log.Error(error);
                return (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            }

            //Validate the presence of an /override setting
            if (string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName))
            {
                error = "Invalid command line set. Missing --override option.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingOverrideFlag };
                log.Error(error);
                return (int)ExecutionReturn.MissingOverrideFlag;
            }

            //Validate and set the value for the build file name
            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName) && string.IsNullOrWhiteSpace(cmdLine.ScriptSrcDir)
                && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac)
                && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
            {
                error = "Invalid command line set. Missing --packagename, --platinumdacpac, --scriptsrcdir, or --platinumdbsource and --platinumserversource options.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                log.Error(error);
                return (int)ExecutionReturn.MissingBuildFlag;
            }

            //If using Platinum DB source, make sure we have both DB and Server arguments
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) || !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) || string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
                {
                    error = "The --platinumdbsource and --platinumserversource options must be used together";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                    log.Error(error);
                    return (int)ExecutionReturn.MissingBuildFlag;
                }
            }

            //If using Platinum DB source, make sure we have a username and password as well
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    error = "The --username and --password arguments are required when using --platinumdbsource";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                    log.Error(error);
                    return (int)ExecutionReturn.MissingBuildFlag;
                }
            }

            //Validate that the build file exists if specified
            if (cmdLine.BuildFileName.Length != 0 && !File.Exists(cmdLine.BuildFileName))
            {
                error = "Missing Build file. The build file specified: " + cmdLine.BuildFileName + " could not be found";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
                log.Error(error);
                return (int)ExecutionReturn.InvalidBuildFileNameValue;
            }

            //Validate that the Platinum dacpac file exists if specified
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac) && !File.Exists(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                error = "Missing Platinum dacpac file. The  Platinum dacpac specified: " + cmdLine.DacPacArgs.PlatinumDacpac + " could not be found";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
                log.Error(error);
                return (int)ExecutionReturn.InvalidBuildFileNameValue;
            }

            if (cmdLine.ScriptSrcDir.Length > 0)
            {
                if (!Directory.Exists(cmdLine.ScriptSrcDir))
                {
                    error = "Invalid --scriptsrcdir setting. The directory '" + cmdLine.ScriptSrcDir + "' does not exist.";
                    errorMessages =  new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidScriptSourceDirectory };
                    log.Error(error);
                    return (int)ExecutionReturn.InvalidScriptSourceDirectory;
                }
            }

            if (cmdLine.TimeoutRetryCount < 0)
            {
              
                    error = "The --timeoutretrycount setting is a negative number. This value needs to be a positive integer.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.NegativeTimeoutRetryCount };
                    log.Error(error);
                    return (int)ExecutionReturn.NegativeTimeoutRetryCount;
             }

            if (cmdLine.TimeoutRetryCount > 0 && !cmdLine.Transactional)
            {

                error = "The --timeoutretrycount setting is not allowed when --transactional=false";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.BadRetryCountAndTransactionalCombo };
                log.Error(error);
                return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
            }

            
            if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName))
            {
                 if (!File.Exists(cmdLine.MultiDbRunConfigFileName))
                 {
                     error = string.Format("Specified --override file does not exist at path: {0}", cmdLine.MultiDbRunConfigFileName);
                     errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                     log.Error(error);
                     return (int)ExecutionReturn.InvalidOverrideFlag;
                 }
            }
           
            if(cmdLine.MultiDbRunConfigFileName.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
            {
   
                if(string.IsNullOrWhiteSpace(cmdLine.Database) || string.IsNullOrWhiteSpace(cmdLine.Server) || 
                    string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    error = $"Invalid command line set. When the --override setting specifies a SQL file, the following are also required:{System.Environment.NewLine} --database, --server - will be used as source to run scripts {System.Environment.NewLine} --username, --password - provide authentication to that database";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                    log.Error(error);
                    return (int)ExecutionReturn.InvalidOverrideFlag;
                }

            }
            else if (!cmdLine.MultiDbRunConfigFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase)
                && !cmdLine.MultiDbRunConfigFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase)
                && !cmdLine.MultiDbRunConfigFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
            {

                error = "Invalid command line set. The '--override' setting file value must be .multiDb, .multiDbQ or .cfg file.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                log.Error(error);
                return (int)ExecutionReturn.InvalidOverrideFlag;
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
            log.Info("Validating target database settings");
            string message = string.Empty;
            string error;
            errorMessages = new string[0];
            multiData = null;
            string extension = Path.GetExtension(multiDbOverrideSettingFileName).ToLowerInvariant();

            switch(extension)
            {
                case ".multidb":
                    multiData = MultiDbHelper.DeserializeMultiDbConfiguration(multiDbOverrideSettingFileName);
                    break;
                case ".multidbq":
                    multiData = MultiDbHelper.CreateMultiDbConfigFromQueryFile(multiDbOverrideSettingFileName, out message);
                    break;
                case ".sql":
                    if(cmdLine != null)
                    {
                        ConnectionData connData = new ConnectionData()
                        {
                            DatabaseName = cmdLine.Database,
                            SQLServerName = cmdLine.Server,
                            UserId = cmdLine.AuthenticationArgs.UserName,
                            Password = cmdLine.AuthenticationArgs.Password,
                            AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType
                        };
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
                errorMessages =  new string[] { error, "Returning error code: " + (int)ExecutionReturn.NullMultiDbConfig };
                log.Error(error);
                return (int)ExecutionReturn.NullMultiDbConfig;
            }

            if (!MultiDbHelper.ValidateMultiDatabaseData(multiData))
            {
                error = "One or more scripts is missing a default or target override database setting. Run has been halted. Please correct the error and try again";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingTargetDbOverrideSetting };
                log.Error(error);
                return (int)ExecutionReturn.MissingTargetDbOverrideSetting;
            }
            return 0;
        }

        public static int ValidateAndLoadPlatinumDacpac(ref CommandLineArgs cmdLine, ref MultiDbData multiDb)
        {
            //DacPac settings validation
            if (!String.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                if (!File.Exists(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    string err = String.Format("A Platinum Dacpac file was specified but could not be located at '{0}'", cmdLine.DacPacArgs.PlatinumDacpac);
                    log.Error(err);
                    return -729;
                }

                if (!String.IsNullOrEmpty(cmdLine.DacPacArgs.TargetDacpac) && !File.Exists(cmdLine.DacPacArgs.TargetDacpac))
                {
                    string err = String.Format("A Target Dacpac file was specified but could not be located at '{0}'", cmdLine.DacPacArgs.TargetDacpac);
                    log.Error(err);
                    return -728;
                }
            }


            //If there are Dacpac settings... we will need to create the SBM automatically..
            if (!string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac) && string.IsNullOrEmpty(cmdLine.BuildFileName))
            {
                if (cmdLine.DacPacArgs.ForceCustomDacPac == false)
                {
                    string sbmName;
                    var stat = DacPacHelper.GetSbmFromDacPac(cmdLine, multiDb, out sbmName);
                    if (stat == DacpacDeltasStatus.Success)
                    {
                        cmdLine.BuildFileName = sbmName;
                        return (int)ExecutionReturn.Successful;
                    }
                    else if (stat == DacpacDeltasStatus.InSync)
                    {
                        return (int)ExecutionReturn.DacpacDatabasesInSync;
                    }
                    else
                    {
                        log.Error("Error creating SBM package from Platinum dacpac");
                        return -5120;
                    }
                }
                else
                {
                    log.Info("Found --forcecustomdacpac setting. Skipping the creation of the single platinum SBM package. Individual dacpacs and SBMs will be created");
                }
            }

            return 0;
        }

        public static int ValidateBatchArguments(ref CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int returnVal = 0;
            List<string> messages = new List<string>();
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountName))
            {
                messages.Add("--batchaccountname is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountKey))
            {
                messages.Add("--batchaccountkey is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountUrl))
            {
                messages.Add("--batchaccounturl is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.StorageAccountName))
            {
                messages.Add("--storageaccountname is required in command line or --settingsfile Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.StorageAccountKey))
            {
                messages.Add("--storageaccountkey is required in command line or --settingsfile Json");
                returnVal = -888;
            }

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchVmSize))
            {
                messages.Add("--batchvmsize, is required in command line or --settingsfile Json");
                returnVal = -888;
            }

            errorMessages = messages.ToArray();
            return returnVal;
        }

        public static int ValidateBatchPreStageArguments(ref CommandLineArgs cmdLine, out string[] errorMessages)
        {
            int returnVal = 0;
            List<string> messages = new List<string>();
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountName))
            {
                messages.Add("--batchaccountname is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountKey))
            {
                messages.Add("--batchaccountkey is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountUrl))
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
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountName))
            {
                messages.Add("--batchaccountname is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountKey))
            {
                messages.Add("--batchaccountkey is required in command line or --settingsfile  Json");
                returnVal = -888;
            }
            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountUrl))
            {
                messages.Add("--batchaccounturl is required in command line or --settingsfile  Json");
                returnVal = -888;
            }

            errorMessages = messages.ToArray();
            return returnVal;
        }

        public static int ValidateQueryArguments(ref CommandLineArgs cmdLine)
        {

            if (!cmdLine.QueryFile.Exists)
            {
                log.Error("The --queryfile file was not found. Please check the name or path and try again");
                return 2;
            }

            Regex noNo = new Regex(@"(UPDATE\s)|(INSERT\s)|(DELETE\s)", RegexOptions.IgnoreCase);
            var query = File.ReadAllText(cmdLine.QueryFile.FullName);
            if (noNo.Match(query).Success)
            {
                log.Error($"{Environment.NewLine}An INSERT, UPDATE or DELETE keyword was found. You can not use the query function to modify data.{Environment.NewLine}Instead, please run your data modification script as a SQL Build Package or DACPAC update");
                return 5;
            }
            if (!File.Exists(cmdLine.MultiDbRunConfigFileName))
            {
                log.Error("The --override file was not found. Please check the name or path and try again");
                return 3;
            }
            if (cmdLine.OutputFile.Exists && !cmdLine.Silent)
            {
                System.Console.WriteLine("The output file already exists. Do you want to overwrite it (Y/N)?");
                var keypressed = System.Console.ReadKey();
                if (keypressed.Key != ConsoleKey.Y)
                {
                    log.Info("Exiting");
                    return 1;
                }
            }

            return 0;
        }

    }
}
