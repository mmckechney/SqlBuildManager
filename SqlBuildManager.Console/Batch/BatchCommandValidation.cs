using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using System;
using System.IO;
namespace SqlBuildManager.Console.Batch
{
    class BatchCommandValidation
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static int ValidateBatchCommandLine(CommandLineArgs cmdLine)
        {
            string[] errorMessages;
            int pwVal = Validation.ValidateUserNameAndPassword(ref cmdLine, out errorMessages);
            if (pwVal != 0)
            {
                return pwVal;
            }
            string error = string.Empty;

            //Validate and set the value for the build file name
            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName) && string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                error = "Invalid command line set. Missing /PackageName or /PlatinumDacpac";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.MissingBuildFlag };
                log.Error(error);
                return (int)ExecutionReturn.MissingBuildFlag;
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


            if (cmdLine.AllowableTimeoutRetries < 0)
            {

                error = "The /TimeoutRetryCount setting is a negative number. This value needs to be a positive integer.";
                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.NegativeTimeoutRetryCount };
                log.Error(error);
                return (int)ExecutionReturn.NegativeTimeoutRetryCount;
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
}
}
