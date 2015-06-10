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

            if (!cmdLine.MultiDbRunConfigFileName.EndsWith(".multidb", StringComparison.InvariantCultureIgnoreCase)
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
        /// <summary>
        /// Accepts a Multi-Database configuration file, processes it and outputs a populated MultiDbData object
        /// </summary>
        /// <param name="multiDbOverrideSettingFileName">Valid Multi-database file (.multiDb, .multiDbQ, .cfg)</param>
        /// <param name="multiData">Out parameter of populated MultiDbData object</param>
        /// <param name="errorMessages">Out parameter or error messages (if any)</param>
        /// <returns>Zero (0) if no errors, otherwise an error code</returns>
        public static int ValidateAndLoadMultiDbData(string multiDbOverrideSettingFileName, out MultiDbData multiData, out string[] errorMessages)
        {
            log.Info("Validating target database settings");
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
                string sbmName;
                 var stat = GetSbmFromDacPac(cmdLine, multiDb, out sbmName);
                if (stat == DacpacDeltasStatus.Success)
                {
                     cmdLine.BuildFileName = sbmName;
                    return (int)ExecutionReturn.Successful;
                }
                else if(stat ==DacpacDeltasStatus.InSync)
                {
                    return (int)ExecutionReturn.DacpacDatabasesInSync;
                }
                else
                {
                    log.Error("Error creating SBM package from Platinum dacpac");
                    return -5120;
                }
               
            }

            return 0;
        }
       
        private static DacpacDeltasStatus GetSbmFromDacPac(CommandLineArgs cmd, MultiDbData multiDb, out string sbmName)
        {
            string workingFolder = (!string.IsNullOrEmpty(cmd.RootLoggingPath) ? cmd.RootLoggingPath : Path.GetTempPath());
            if (!workingFolder.EndsWith("\\"))
                workingFolder = workingFolder + "\\";

            workingFolder = workingFolder + "Dacpac\\";
            if(!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            log.Info("Starting process: create SBM build file from dacpac settings");
            DacpacDeltasStatus stat = DacpacDeltasStatus.Processing;
            sbmName = string.Empty;

            if (!String.IsNullOrEmpty(cmd.TargetDacpac))
            {
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(cmd.PlatinumDacpac, cmd.TargetDacpac, out sbmName);
            }
            else if(!string.IsNullOrEmpty(cmd.Database) && !string.IsNullOrEmpty(cmd.Server))
            {
                string targetDacPac = workingFolder + cmd.Database + ".dacpac";
                if (!DacPacHelper.ExtractDacPac(cmd.Database, cmd.Server, cmd.UserName, cmd.Password, targetDacPac))
                {
                    log.Error(string.Format("Error extracting dacpac from {0} : {1}", cmd.Database, cmd.Server));
                    return DacpacDeltasStatus.ExtractionFailure;
                }
                stat = DacPacHelper.CreateSbmFromDacPacDifferences(cmd.PlatinumDacpac, targetDacPac, out sbmName);
            }

            if (stat == DacpacDeltasStatus.Processing)
            {
                string database, server;
                foreach (var serv in multiDb)
                {
                    server = serv.ServerName;
                    for (int i = 0; i < serv.OverrideSequence.Count; i++)
                    {
                        database = serv.OverrideSequence.ElementAt(i).Value[0].OverrideDbTarget;

                        string targetDacPac = workingFolder + database + ".dacpac"; ;
                        if (!DacPacHelper.ExtractDacPac(database, server, cmd.UserName, cmd.Password, targetDacPac))
                        {
                            log.Error(string.Format("Error extracting dacpac from {0} : {1}", server, database));
                            return DacpacDeltasStatus.ExtractionFailure;
                        }
                        stat = DacPacHelper.CreateSbmFromDacPacDifferences(cmd.PlatinumDacpac, targetDacPac, out sbmName);

                        if (stat == DacpacDeltasStatus.InSync)
                        {
                            log.InfoFormat("{0} and {1} are already in  sync. Looping to next database.", Path.GetFileName(cmd.PlatinumDacpac), Path.GetFileName(targetDacPac));
                            stat = DacpacDeltasStatus.Processing;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (stat != DacpacDeltasStatus.Processing)
                        break;
                }
            }

            switch(stat)
            {
                case DacpacDeltasStatus.Success:
                    log.Info("Successfully created SBM from two dacpacs");
                    break;
                case DacpacDeltasStatus.InSync:
                    log.Info("The two dacpac databases are already in sync");
                    break;
                default:
                    log.Error("Error creating build package from supplied Platinum and Target dacpac files");
                    break;
                    
            }
            return stat;


        }

    }
}
