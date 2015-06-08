using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;

namespace SqlBuildManager.Console
{
    class Validation
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Validates that the basic command line arguments are correct with no conflicts or missing elements
        /// </summary>
        /// <param name="cmdLine">Incomming CommandLineArgs object</param>
        /// <param name="errorMessages">Any errors that are generated</param>
        /// <returns>Zero (0) if valid, otherwise an error code</returns>
        public static int ValidateCommonCommandLineArgs(CommandLineArgs cmdLine, out string[] errorMessages)
        {
            string error = string.Empty;
            errorMessages = new string[0];

            //Validate and set the value for the root logging path
            if (cmdLine.RootLoggingPath.Length == 0)
            {
                string msg = "Invalid command line set. Missing /RootLoggingPath setting.";
                log.Error(msg);
                System.Console.Error.WriteLine(msg);
                return -99;
            }

            //Check that they haven't set /Trial=true and /Transaction=false
            if (cmdLine.Transactional == false && cmdLine.Trial == true)
            {
                error = "Invalid command line combination. You cannot have /Transaction=\"false\" and /Trial=\"true\".";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code:" + (int)ExecutionReturn.InvalidTransactionAndTrialCombo };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            }

            //Validate the presence of an /override setting
            if (cmdLine.MultiDbRunConfigFileName.Length == 0)
            {
                error = "Invalid command line set. Missing /override setting.";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingOverrideFlag };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.MissingOverrideFlag;
            }

            //Validate and set the value for the build file name
            if (cmdLine.BuildFileName.Length == 0 && cmdLine.ScriptSrcDir.Length == 0)
            {
                error = "Invalid command line set. Missing /build or /ScriptSrcDir setting.";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.MissingBuildFlag;
            }

            //Validate and set the value for the build file name
            if (cmdLine.BuildFileName.Length != 0 && !File.Exists(cmdLine.BuildFileName))
            {
                error = "Missing Build file. The build file specified: "+ cmdLine.BuildFileName +" could not be found";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.InvalidBuildFileNameValue;
            }

            if (cmdLine.ScriptSrcDir.Length > 0)
            {
                if (!Directory.Exists(cmdLine.ScriptSrcDir))
                {
                    error = "Invalid /ScriptSrcDir setting. The directory '" + cmdLine.ScriptSrcDir + "' does not exist.";
                    log.Error(error);
                    errorMessages =  new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidScriptSourceDirectory };
                    System.Console.Error.WriteLine(error);
                    return (int)ExecutionReturn.InvalidScriptSourceDirectory;
                }
            }

            if (cmdLine.AllowableTimeoutRetries < 0)
            {
              
                    error = "The /TimeoutRetryCount setting is a negative number. This value needs to be a positive integer.";
                    log.Error(error);
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.NegativeTimeoutRetryCount };
                    System.Console.Error.WriteLine(error);
                    return (int)ExecutionReturn.NegativeTimeoutRetryCount;
             }

            if (cmdLine.AllowableTimeoutRetries > 0 && !cmdLine.Transactional)
            {

                error = "The /TimeoutRetryCount setting is not allowed when /Transactional=false";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.BadRetryCountAndTransactionalCombo };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
            }

            if (!cmdLine.MultiDbRunConfigFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase)
                && !cmdLine.MultiDbRunConfigFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase)
                && !cmdLine.MultiDbRunConfigFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
            {

                error = "Invalid command line set. The '/override' setting file value must be .multiDb, .multiDbQ or .cfg file.";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                System.Console.Error.WriteLine(error);
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
        public static int ValidateAndLoadMultiDbData(string multiDbOverrideSettingFileName, out MultiDbData multiData, out string[] errorMessages)
        {
            string message = string.Empty;
            string error;
            errorMessages = new string[0];
            multiData = null;

            if (multiDbOverrideSettingFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase))
                multiData = MultiDbHelper.DeserializeMultiDbConfiguration(multiDbOverrideSettingFileName);
            else if (multiDbOverrideSettingFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase))
                multiData = MultiDbHelper.CreateMultiDbConfigFromQuery(multiDbOverrideSettingFileName, out message);
            else if (multiDbOverrideSettingFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
                multiData = MultiDbHelper.ImportMultiDbTextConfig(multiDbOverrideSettingFileName);

            if (multiData == null)
            {
                error = "Unable to read in configuration file " + multiDbOverrideSettingFileName + ((message.Length > 0) ? " :: " + message : "");
                log.Error(error);
                errorMessages =  new string[] { error, "Returning error code: " + (int)ExecutionReturn.NullMultiDbConfig };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.NullMultiDbConfig;
            }

            if (!MultiDbHelper.ValidateMultiDatabaseData(multiData))
            {
                error = "One or more scripts is missing a default or target override database setting. Run has been halted. Please correct the error and try again";
                log.Error(error);
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingTargetDbOverrideSetting };
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.MissingTargetDbOverrideSetting;
            }
            return 0;
        }

        public static int ValidateAndLoadPlatinumDacpac(ref CommandLineArgs cmdLine, ref MultiDbData multiDb)
        {
            //DacPac settings validation
            if (!String.IsNullOrEmpty(cmdLine.PlatinumDacpac))
            {
                if (!File.Exists(cmdLine.PlatinumDacpac))
                {
                    string err = String.Format("A Platinum Dacpac file was specified but could not be located at '{0}'", cmdLine.PlatinumDacpac);
                    log.Error(err);
                    System.Console.Error.WriteLine(err);
                    return -729;
                }

                if (!String.IsNullOrEmpty(cmdLine.TargetDacpac) && !File.Exists(cmdLine.TargetDacpac))
                {
                    string err = String.Format("A Target Dacpac file was specified but could not be located at '{0}'", cmdLine.TargetDacpac);
                    log.Error(err);
                    System.Console.Error.WriteLine(err);
                    return -728;
                }
            }


            //If there are Dacpac settings... we will need to create the SBM automatically..
            if (!string.IsNullOrEmpty(cmdLine.PlatinumDacpac) && string.IsNullOrEmpty(cmdLine.BuildFileName))
            {
                string sbmName = GetSbmFromDacPac(cmdLine, multiDb);
                if (string.IsNullOrEmpty(sbmName))
                {
                    log.Error("Error creating SBM package from Platinum dacpac");
                    System.Console.Error.WriteLine("Error creating SBM package from Platinum dacpac");
                    return -5120;
                }
                else
                {
                    cmdLine.BuildFileName = sbmName;
                }
            }

            return 0;
        }

        private static string GetSbmFromDacPac(CommandLineArgs cmd, MultiDbData multiDb)
        {
            string targetDacPac;
            if (String.IsNullOrEmpty(cmd.TargetDacpac))
            {
                //Create SBM from Platinum dacpac and target database
                string database, server;
                if (string.IsNullOrEmpty(cmd.Database) || string.IsNullOrEmpty(cmd.Server))
                {
                    //No specific target identified, so pick the first from the MultiDb config
                    server = multiDb.First().ServerName;
                    database = multiDb.First().Databases.First().DatabaseName;
                }
                else
                {
                    server = cmd.Database;
                    database = cmd.Server;
                }

                targetDacPac = Path.GetTempFileName();
                if (!DacPacHelper.ExtractDacPac(database, server, cmd.UserName, cmd.Password, targetDacPac))
                {
                    System.Console.Error.WriteLine(string.Format("Error extracting dacpac from {0}.{1}", server, database));
                    return string.Empty;
                }
            }
            else
            {
                targetDacPac = cmd.TargetDacpac;
            }


            //Create SBM from the two dacpacs
            string sbmName;
            if (DacPacHelper.CreateSbmFromDacPacDifferences(cmd.PlatinumDacpac, targetDacPac, out sbmName))
            {
                return sbmName;
            }
            else
            {
                System.Console.Error.WriteLine("Error creating build package from supplied Platinum and Target dacpac files");
                return string.Empty;
            }


        }
    }
}
