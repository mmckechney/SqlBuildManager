//using SqlBuildManager.Interfaces.Console;
//using SqlSync.SqlBuild;
//using System;
//using System.IO;
//namespace SqlBuildManager.Console.Batch
//{
//    class BatchCommandValidation
//    {
//        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//        public static int ValidateBatchCommandLine(CommandLineArgs cmdLine)
//        {
//            string[] errorMessages;
//            int pwVal = Validation.ValidateUserNameAndPassword(ref cmdLine, out errorMessages);
//            if (pwVal != 0)
//            {
//                return pwVal;
//            }
//            string error = string.Empty;

//            //Validate that the build file exists if specified
//            if (cmdLine.BuildFileName.Length != 0 && !File.Exists(cmdLine.BuildFileName))
//            {
//                error = "Missing Build file. The build file specified: " + cmdLine.BuildFileName + " could not be found";
//                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
//                log.Error(error);
//                return (int)ExecutionReturn.InvalidBuildFileNameValue;
//            }

//            //Validate that the Platinum dacpac file exists if specified
//            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac) && !File.Exists(cmdLine.DacPacArgs.PlatinumDacpac))
//            {
//                error = "Missing Platinum dacpac file. The  Platinum dacpac specified: " + cmdLine.DacPacArgs.PlatinumDacpac + " could not be found";
//                errorMessages = new string[] { error, "Returning error code: " + (int)ExecutionReturn.InvalidBuildFileNameValue };
//                log.Error(error);
//                return (int)ExecutionReturn.InvalidBuildFileNameValue;
//            }


//            return 0;
//        }
//}
//}
