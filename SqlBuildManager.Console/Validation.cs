﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.Connection;

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
                return -99;
            }

            //Check that they haven't set /Trial=true and /Transaction=false
            if (cmdLine.Transactional == false && cmdLine.Trial == true)
            {
                error = "Invalid command line combination. You cannot have /Transaction=\"false\" and /Trial=\"true\".";
                errorMessages = new string[] { error, "Returning error code:" + (int)ExecutionReturn.InvalidTransactionAndTrialCombo };
                log.Error(error);
                return (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            }

            //Validate the presence of an /override setting
            if (cmdLine.MultiDbRunConfigFileName.Length == 0)
            {
                error = "Invalid command line set. Missing /override setting.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingOverrideFlag };
                log.Error(error);
                return (int)ExecutionReturn.MissingOverrideFlag;
            }

            //Validate and set the value for the build file name
            if (cmdLine.BuildFileName.Length == 0 && cmdLine.ScriptSrcDir.Length == 0 && cmdLine.PlatinumDacpac.Length == 0)
            {
                error = "Invalid command line set. Missing /build or /ScriptSrcDir setting.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                log.Error(error);
                return (int)ExecutionReturn.MissingBuildFlag;
            }

            //Validate and set the value for the build file name
            if (cmdLine.BuildFileName.Length != 0 && !File.Exists(cmdLine.BuildFileName))
            {
                error = "Missing Build file. The build file specified: "+ cmdLine.BuildFileName +" could not be found";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
                log.Error(error);
                return (int)ExecutionReturn.InvalidBuildFileNameValue;
            }

            if (cmdLine.ScriptSrcDir.Length > 0)
            {
                if (!Directory.Exists(cmdLine.ScriptSrcDir))
                {
                    error = "Invalid /ScriptSrcDir setting. The directory '" + cmdLine.ScriptSrcDir + "' does not exist.";
                    errorMessages =  new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidScriptSourceDirectory };
                    log.Error(error);
                    return (int)ExecutionReturn.InvalidScriptSourceDirectory;
                }
            }

            if (cmdLine.AllowableTimeoutRetries < 0)
            {
              
                    error = "The /TimeoutRetryCount setting is a negative number. This value needs to be a positive integer.";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.NegativeTimeoutRetryCount };
                    log.Error(error);
                    return (int)ExecutionReturn.NegativeTimeoutRetryCount;
             }

            if (cmdLine.AllowableTimeoutRetries > 0 && !cmdLine.Transactional)
            {

                error = "The /TimeoutRetryCount setting is not allowed when /Transactional=false";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.BadRetryCountAndTransactionalCombo };
                log.Error(error);
                return (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName))
            {
                 if (!File.Exists(cmdLine.MultiDbRunConfigFileName))
                 {
                     error = string.Format("Specified /Override file does not exist at path: {0}", cmdLine.MultiDbRunConfigFileName);
                     errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                     log.Error(error);
                     return (int)ExecutionReturn.InvalidOverrideFlag;
                 }
            }
           
            if(cmdLine.MultiDbRunConfigFileName.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
            {
   
                if(string.IsNullOrWhiteSpace(cmdLine.Database) || string.IsNullOrWhiteSpace(cmdLine.Server) || 
                    string.IsNullOrWhiteSpace(cmdLine.UserName) || string.IsNullOrWhiteSpace(cmdLine.Password))
                {
                    error = "Invalid command line set. When the /Override setting specifies a SQL file, the following are also required:\r\n /Database, /Server - will be used as source to run scripts \r\n /Username, /Password - provide authentication to that database";
                    errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                    log.Error(error);
                    return (int)ExecutionReturn.InvalidOverrideFlag;
                }

            }
            else if (!cmdLine.MultiDbRunConfigFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase)
                && !cmdLine.MultiDbRunConfigFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase)
                && !cmdLine.MultiDbRunConfigFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
            {

                error = "Invalid command line set. The '/override' setting file value must be .multiDb, .multiDbQ or .cfg file.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidOverrideFlag };
                log.Error(error);
                return (int)ExecutionReturn.InvalidOverrideFlag;
            }

            return 0;
        }
        //public static int ValidateAndLoadMultiDbData(string multiDbOverrideSettingFileName, out MultiDbData multiData, out string[] errorMessages)
        //{
        //    return ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null, out multiData, out errorMessages);
        //}
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
                case ".cfg":
                    multiData = MultiDbHelper.ImportMultiDbTextConfig(multiDbOverrideSettingFileName);
                    break;
                case ".sql":
                    if(cmdLine != null)
                    {
                        ConnectionData connData = new ConnectionData()
                        {
                            DatabaseName = cmdLine.Database,
                            SQLServerName = cmdLine.Server,
                            UserId = cmdLine.UserName,
                            Password = cmdLine.Password
                        };
                        multiData = MultiDbHelper.CreateMultiDbConfigFromQuery(connData, File.ReadAllText(cmdLine.MultiDbRunConfigFileName), out message);
                    }
                    break;


            }
            //if (multiDbOverrideSettingFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase))
            //    multiData = MultiDbHelper.DeserializeMultiDbConfiguration(multiDbOverrideSettingFileName);
            //else if (multiDbOverrideSettingFileName.EndsWith(".multidbq", StringComparison.InvariantCultureIgnoreCase))
            //    multiData = MultiDbHelper.CreateMultiDbConfigFromQueryFile(multiDbOverrideSettingFileName, out message);
            //else if (multiDbOverrideSettingFileName.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase))
            //    multiData = MultiDbHelper.ImportMultiDbTextConfig(multiDbOverrideSettingFileName);
           

            if (multiData == null)
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
            if (!String.IsNullOrEmpty(cmdLine.PlatinumDacpac))
            {
                if (!File.Exists(cmdLine.PlatinumDacpac))
                {
                    string err = String.Format("A Platinum Dacpac file was specified but could not be located at '{0}'", cmdLine.PlatinumDacpac);
                    log.Error(err);
                    return -729;
                }

                if (!String.IsNullOrEmpty(cmdLine.TargetDacpac) && !File.Exists(cmdLine.TargetDacpac))
                {
                    string err = String.Format("A Target Dacpac file was specified but could not be located at '{0}'", cmdLine.TargetDacpac);
                    log.Error(err);
                    return -728;
                }
            }


            //If there are Dacpac settings... we will need to create the SBM automatically..
            if (!string.IsNullOrEmpty(cmdLine.PlatinumDacpac) && string.IsNullOrEmpty(cmdLine.BuildFileName))
            {
                if (cmdLine.ForceCustomDacPac == false)
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
                    log.Info("Found ForceCustomDacPac setting. Skipping the creation of the single platinum SBM package. Individual dacpacs and SBMs will be created");
                }
            }

            return 0;
        }
       
    }
}
