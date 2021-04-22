using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoreLinq;
using System.Linq;
using System.Threading.Tasks;
using SqlBuildManager.Console.Queue;

namespace SqlBuildManager.Console.Threaded
{
    public class ThreadedExecution 
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ILogger logEventHub;
        private static ILogger logCommitRun;
        private static ILogger logErrorRun;
        private static ILogger logFailures;
        private static ILogger logSuccess;
        private static ILogger logRuntime;

        StringBuilder sbSuccessDatabasesCfg = new System.Text.StringBuilder();
        StringBuilder sbFailureDatabasesCfg = new System.Text.StringBuilder();

        MultiDbData multiData = null;
        DateTime startTime;
        bool hasError = false;
        bool theadedLoggingInitiated = false;
        private string[] args;
        private CommandLineArgs cmdLine = null;
        string workingDirectory = string.Empty;
        private static string projectFileName = string.Empty;
        /// <summary>
        /// Path and file name to the XML metadata configuration project file (SqlSyncBuildProject.xml)
        /// </summary>
        internal static string ProjectFileName
        {
            get { return ThreadedExecution.projectFileName; }
        }
        string projectFilePath = string.Empty;

        private string buildRequestedBy = string.Empty;

        private static string buildZipFileName = string.Empty;
        private static string platinumDacPacFileName = string.Empty;
        /// <summary>
        /// The name of the zippedbuild file (.sbm)
        /// </summary>
        internal static string BuildZipFileName
        {
            get { return ThreadedExecution.buildZipFileName; }
        }
        internal static string PlatinumDacPacFileName
        {
            get { return ThreadedExecution.platinumDacPacFileName; }
        }

        private static string rootLoggingPath = string.Empty;
        /// <summary>
        /// The root folder where the logging should start
        /// </summary>
        internal static string RootLoggingPath
        {
            get { return ThreadedExecution.rootLoggingPath; }
        }

        private static string runID = string.Empty;
        /// <summary>
        /// "unique" identifier for the run. 
        /// </summary>
        internal static string RunID
        {
            get
            {
                if (ThreadedExecution.runID == string.Empty)
                    ThreadedExecution.runID = Guid.NewGuid().ToString().Replace("-", "");

                return ThreadedExecution.runID;
            }
        }

        private static ScriptBatchCollection batchColl = null;
        /// <summary>
        /// The pre-batched set of scripts to be run
        /// </summary>
        internal static ScriptBatchCollection BatchColl
        {
            get { return ThreadedExecution.batchColl; }
        }

        private static SqlSyncBuildData buildData = null;
        /// <summary>
        /// The runtime metadata object for the build execution
        ///// </summary>
        internal static SqlSyncBuildData BuildData
        {
            get { return buildData; }
        }

        public ThreadedExecution(CommandLineArgs cmd)
        {
            this.cmdLine = cmd;
        }
        public ThreadedExecution(string[] args)
        {
            this.args = args;

        }

      

        /// <summary>
        /// Execute method that is used from a straight command-line execution
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {

            log.LogDebug("Entering Execute method of ThreadedExecution");
            string[] errorMessages;

            //Parse out the command line options if needed (should only be coming from unit tests)
            if (cmdLine == null)
            {
                cmdLine = CommandLine.ParseCommandLineArg(args);
            }

            //Create Threaded Run specific loggers -- these are init'd when the first logs are written
            if (string.IsNullOrEmpty(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = @"C:/tmp-sqlbuildlogging";
            }

            //Set paths
            SetRootAndWorkingPaths(cmdLine.RootLoggingPath);

            //Validate all of the arguments
            log.LogInformation("Validating command parameters");
            int tmpReturn = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                WriteToLog(msg);
                return tmpReturn;
            }

            //Determine where to get the scripts from (SBM, DACPAC, generated DACPAC, scripts?)
            int success;
            (success, cmdLine) = ConfigureScriptSource(cmdLine);
            if(success != 0)
            {
                return success;
            }

            //This will set the static variable for the script collection
            var prep = PrepBuildAndScripts(buildZipFileName, buildRequestedBy, cmdLine.DacPacArgs.ForceCustomDacPac);
            if (prep != 0)
            {
                return prep;
            }

            //Start logging
            WriteToLog(new LogMsg() { Message = "**** Starting log for Run ID: " + ThreadedExecution.RunID + " ****", LogType = LogType.Message });

            try
            {
                //Run from override settings or from a Service Bus topic?
                if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ServiceBusTopicConnectionString))
                {
                    Task<int> queueTask = ExecuteFromQueue(cmdLine, System.Environment.UserName);
                    queueTask.Wait();
                    return queueTask.Result;
                }
                else
                {
                    int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out multiData, out errorMessages);
                    if (tmpValReturn != 0)
                    {
                        var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                        WriteToLog(msg);
                        return tmpValReturn;
                    }

                    //Set the number of allowed retries...
                    this.multiData.AllowableTimeoutRetries = cmdLine.TimeoutRetryCount;
                    //Set Trial
                    this.multiData.RunAsTrial = cmdLine.Trial;
                    this.multiData.BuildRevision = cmdLine.BuildRevision;

                    return ExecuteFromOverrideFile(ThreadedExecution.buildZipFileName, cmdLine.DacPacArgs.PlatinumDacpac, multiData, cmdLine.RootLoggingPath, cmdLine.Description, System.Environment.UserName, cmdLine.DacPacArgs.ForceCustomDacPac, cmdLine.Concurrency, cmdLine.ConcurrencyType);
                }
            }
            finally
            {
                //Really only needed when running unit tests, but still a good idea.
                ResetStaticValues();
            }
        }

        /// <summary>
        /// Execute method that is used inherently from the Execute() 
        /// </summary>
        /// <returns></returns>
        private int ExecuteFromOverrideFile(string buildZipFileName, string platinumDacPacFileName, MultiDbData multiData, string rootLoggingPath, string description, string buildRequestedBy, bool forceCustomDacpac, int concurrency = 20, ConcurrencyType concurrencyType = ConcurrencyType.Count)
        {
            try
            {

                var tasks = new List<Task<int>>();
                int targetTotal = 0;
                try
                {
                    startTime = DateTime.Now;
                    log.LogInformation($"Starting Threaded processing at {startTime.ToString()}");
 
                    var concurrencyBuckets = Concurrency.ConcurrencyByType(multiData, concurrency, concurrencyType);
                    targetTotal = concurrencyBuckets.Sum(c => c.Count());
                    foreach (var bucket in concurrencyBuckets)
                    {
                        tasks.Add(ProcessConcurrencyBucket(bucket, forceCustomDacpac));
                    }
                }
                catch (Exception exe)
                {
                    WriteToLog(new LogMsg() { Message = exe.ToString(), LogType = LogType.Error });
                }

                //Wait for all of the tasks to finish
                Task.WaitAll(tasks.ToArray());


                TimeSpan interval = DateTime.Now - startTime;
                var finalMsg = new LogMsg() { RunId = ThreadedExecution.RunID, Message = $"Ending threaded processing at {DateTime.Now.ToUniversalTime()}", LogType = LogType.Message };
                WriteToLog(finalMsg);
                finalMsg.Message = $"Execution Duration: {interval.ToString()}";
                WriteToLog(finalMsg);
                finalMsg.Message = $"Total number of targets: {targetTotal.ToString()}";
                WriteToLog(finalMsg);

                WriteToLog(new LogMsg() { LogType = LogType.SuccessDatabases });
                if (this.hasError)
                {
                    WriteToLog(new LogMsg() { LogType = LogType.FailureDatabases });
                    finalMsg.Message = "Finishing with Errors";
                    finalMsg.LogType = LogType.Error;
                    WriteToLog(finalMsg);
                    finalMsg.LogType = LogType.Message;
                    WriteToLog(finalMsg);
                    return (int)ExecutionReturn.FinishingWithErrors;
                }
                else
                {
                    log.LogInformation("Successful");
                    return (int)ExecutionReturn.Successful;
                }
            }
            catch (Exception bigExe)
            {
                log.LogCritical($"Big problem running the threaded build...{bigExe.ToString()}");
                return (int)ExecutionReturn.NullBuildData;

            }
           
        }
        private async Task<int> ExecuteFromQueue(CommandLineArgs cmdLine, string buildRequestedBy)
        {


            var qManager = new Queue.QueueManager(cmdLine.BatchArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName);

            int finalReturn = 0;
            int retVal = 0;
            while(true)
            {
                var message = await qManager.GetDatabaseTargetFromQueue();
               
                if(message == null)
                {
                    log.LogInformation("No more messages found in Service Bus Topic. Exiting.");
                    break;
                }
                else
                {
                    var target = message.As<TargetMessage>();
                    ThreadedRunner runner = new ThreadedRunner(target.ServerName, target.DbOverrideSequence, cmdLine, buildRequestedBy, cmdLine.DacPacArgs.ForceCustomDacPac);
                    var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = ThreadedExecution.RunID, Message = "Queuing up thread", LogType = LogType.Message };
                    WriteToLog(msg);
                    retVal = await ProcessThreadedBuild(runner);
                    if(retVal == 0)
                    {
                        await qManager.CompleteMessage(message);
                    }else
                    {
                        await qManager.DeadletterMessage(message);
                        finalReturn = -453453;
                    }
                }
            }
            return finalReturn;
        }


        /// <summary>
        /// Using the CommandLineArgs values, determines where the scripts will come from (raw scripts, SBM, DACPAC or generated DACPAC)
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <returns></returns>
        private (int, CommandLineArgs) ConfigureScriptSource(CommandLineArgs cmdLine)
        {
            //If we don't have a pre-constructed build file, but rather a script source directory, we'll build one from there...
            if (!string.IsNullOrWhiteSpace(cmdLine.ScriptSrcDir))
            {
                ConstructBuildFileFromScriptDirectory(cmdLine.ScriptSrcDir);
            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.BuildFileName)) //using SBM as a source
            {
                ThreadedExecution.buildZipFileName = cmdLine.BuildFileName;
                string msg = "--packagename setting found. Using '" + ThreadedExecution.buildZipFileName + "' as build source";
                WriteToLog(new LogMsg() { Message = msg, LogType = LogType.Message });
                log.LogInformation(msg);
            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac)) //using a platinum dacpac as a source
            {
                ThreadedExecution.platinumDacPacFileName = cmdLine.DacPacArgs.PlatinumDacpac;
                string msg = "--platinumdacpac setting found. Using '" + ThreadedExecution.platinumDacPacFileName + "' as build source";
                WriteToLog(new LogMsg() { Message = msg, LogType = LogType.Message }); ;
                log.LogInformation(msg);

            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource)) //using a platinum database as the source
            {
                log.LogInformation($"Extracting Platinum Dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                string dacpacName = Path.Combine(ThreadedExecution.rootLoggingPath, cmdLine.DacPacArgs.PlatinumDbSource + ".dacpac");

                if (!DacPacHelper.ExtractDacPac(cmdLine.DacPacArgs.PlatinumDbSource, cmdLine.DacPacArgs.PlatinumServerSource, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, dacpacName))
                {
                    var m = new LogMsg()
                    {
                        Message = $"Error creating the Platinum dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}",
                        LogType = LogType.Error
                    };
                    WriteToLog(m);
                    return ((int)ExecutionReturn.BuildFileExtractionError, cmdLine);

                }
                cmdLine.DacPacArgs.PlatinumDacpac = dacpacName;
                ThreadedExecution.platinumDacPacFileName = dacpacName;

            }

            int tmpValReturn;
            //Check for the platinum dacpac and configure it if necessary
            (tmpValReturn, cmdLine) = Validation.ValidateAndLoadPlatinumDacpac(cmdLine, multiData);
            if (tmpValReturn == 0 && string.IsNullOrEmpty(ThreadedExecution.buildZipFileName))
            {
                ThreadedExecution.buildZipFileName = cmdLine.BuildFileName;
            }
            else if (tmpValReturn == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                return ((int)ExecutionReturn.DacpacDatabasesInSync, cmdLine);
            }
            else if (tmpValReturn != 0)
            {
                return (tmpValReturn, cmdLine);
            }

            return (0, cmdLine);
        }

        private int PrepBuildAndScripts(string buildZipFileName, string buildRequestedBy, bool forceCustomDacpac)
        {
            ThreadedExecution.buildZipFileName = buildZipFileName;
            this.buildRequestedBy = buildRequestedBy;

            //Looks like we're good to go... extract the build Zip file (.sbm) into a working folder...

            if (!forceCustomDacpac)
            {
                ExtractAndLoadBuildFile(ThreadedExecution.buildZipFileName, out ThreadedExecution.buildData);
                if (buildData == null)
                {
                    var msg = new LogMsg()
                    {
                        Message = "Unable to procees. SqlSyncBuild data object is null, Returning error code: " + (int)ExecutionReturn.NullBuildData,
                        LogType = LogType.Error
                    };
                    WriteToLog(msg);
                    return (int)ExecutionReturn.NullBuildData;
                }
                else
                {
                    //Load up the batched scripts into a shared object so that we can conserve memory
                    ThreadedExecution.batchColl = SqlBuildHelper.LoadAndBatchSqlScripts(ThreadedExecution.buildData, this.projectFilePath);
                }

            }
            return 0;

        }

        private async Task<int> ProcessConcurrencyBucket(IEnumerable<(string, List<DatabaseOverride>)> bucket, bool forceCustomDacpac)
        {
            foreach( (string server, List<DatabaseOverride> ovr) in bucket)
            {
                ThreadedRunner runner = new ThreadedRunner(server, ovr, cmdLine, buildRequestedBy, forceCustomDacpac);
                var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = ThreadedExecution.RunID, Message = "Queuing up thread", LogType = LogType.Message };
                WriteToLog(msg);
                await ProcessThreadedBuild(runner);
            }
            return 0;
        }
        private async Task<int> ProcessThreadedBuild(ThreadedRunner runner)
        {
            var msg = new LogMsg();
            int returnVal = -9000000;
            try
            {

                //ThreadedRunner runner = (ThreadedRunner)state;
                //SERVER:defaultDb,override
                string cfgString = String.Format("{0}:{1},{2}", runner.Server, runner.DefaultDatabaseName, runner.TargetDatabases);

                msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, SourceDacPac = runner.DacpacName, RunId = ThreadedExecution.RunID, Message = "Starting up thread", LogType = LogType.Message };
                WriteToLog(msg);

                //Run the scripts!!
                await runner.RunDatabaseBuild();

                msg.Message = ((RunnerReturn)runner.ReturnValue).GetDescription();
                returnVal = runner.ReturnValue;
                switch (returnVal)
                {
                    case (int)RunnerReturn.BuildCommitted:
                    case (int)RunnerReturn.DacpacDatabasesInSync:
                    case (int)RunnerReturn.CommittedWithCustomDacpac:
                    case (int)RunnerReturn.SuccessWithTrialRolledBack:
                        msg.LogType = LogType.Commit;
                        WriteToLog(msg);
                        sbSuccessDatabasesCfg.AppendLine(cfgString);
                        break;

                    case (int)RunnerReturn.RolledBack:
                    case (int)RunnerReturn.BuildErrorNonTransactional:
                    default:
                        msg.LogType = LogType.Error;
                        WriteToLog(msg);
                        sbFailureDatabasesCfg.AppendLine(cfgString);
                        this.hasError = true;
                        break;
                }

                msg.Message = "Thread complete";
                msg.LogType = LogType.Message;
                WriteToLog(msg);
                runner = null;

            }
            catch (Exception exe)
            {
                msg.Message = exe.Message;
                msg.LogType = LogType.Error;
                WriteToLog(msg);
            }
            return returnVal;
        }
        
        private int ExtractAndLoadBuildFile(string sqlBuildProjectFileName, out SqlSyncBuildData buildData)
        {
            log.LogInformation($"Extracting build file '{sqlBuildProjectFileName}' to working directory '{this.workingDirectory}'");

            buildData = null;

            Directory.CreateDirectory(this.workingDirectory);

            string result;
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sqlBuildProjectFileName, ref this.workingDirectory, ref this.projectFilePath, ref ThreadedExecution.projectFileName, false, true, out result))
            {
                var msg = new LogMsg()
                {
                    Message = $"Zip extraction error. Unable to Extract Sql Build file at '{sqlBuildProjectFileName}'. Do you need to specify a full directory path? {result}",
                    LogType = LogType.Error
                };
                WriteToLog(msg);
                return (int)ExecutionReturn.BuildFileExtractionError;

            }

            if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, ThreadedExecution.projectFileName, false))
            {
                var msg = new LogMsg()
                {
                    Message = $"Build project load error. Unable to load project file.",
                    LogType = LogType.Error
                };
                WriteToLog(msg);
                return (int)ExecutionReturn.LoadProjectFileError;
            }

            return 0;
        }

        private void ConstructBuildFileFromScriptDirectory(string directoryName)
        {
            log.LogInformation("Constructing build file from script directory");
            string shortFileName = string.Empty;
            ThreadedExecution.buildZipFileName = Path.Combine(ThreadedExecution.rootLoggingPath, ThreadedExecution.RunID + ".sbm");
            string projFileName = Path.Combine(this.workingDirectory, SqlSync.SqlBuild.XmlFileNames.MainProjectFile);
            SqlSyncBuildData localBuildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            List<string> fileList = new List<string>(Directory.GetFiles(directoryName, "*.sql", SearchOption.TopDirectoryOnly));
            fileList.Sort();

            for (int i = 0; i < fileList.Count; i++)
            {
                shortFileName = Path.GetFileName(fileList[i]);
                File.Copy(fileList[i], Path.Combine(this.workingDirectory, shortFileName), true);

                SqlBuildFileHelper.AddScriptFileToBuild(ref localBuildData,
                    projFileName,
                    shortFileName,
                    i,
                   "'" + fileList[i] + "' added via source directory setting.",
                    true,
                    true,
                    "",
                    false,
                    ThreadedExecution.buildZipFileName,
                    ((i < fileList.Count - 1) ? false : true),
                    false,
                    System.Environment.UserDomainName + @"/" + System.Environment.UserName,
                    20,
                    "");
            }

        }

        private void ResetStaticValues()
        {
            ThreadedExecution.runID = string.Empty;
            ThreadedExecution.platinumDacPacFileName = string.Empty;
            ThreadedExecution.buildZipFileName = string.Empty;
            ThreadedExecution.projectFileName = string.Empty;
            ThreadedExecution.rootLoggingPath = string.Empty;
            ThreadedExecution.batchColl = null;
            ThreadedExecution.buildData = null;
        }
        private void InitThreadedLogging()
        {
            logEventHub = SqlBuildManager.Logging.Threaded.EventHubLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.BatchArgs.EventHubConnectionString);
            logRuntime = SqlBuildManager.Logging.Threaded.RuntimeLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);

            logFailures = SqlBuildManager.Logging.Threaded.FailureDatabaseLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);
            logSuccess = SqlBuildManager.Logging.Threaded.SuccessDatabaseLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);

            logCommitRun = SqlBuildManager.Logging.Threaded.CommitLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);
            logErrorRun = SqlBuildManager.Logging.Threaded.ErrorLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);

            this.theadedLoggingInitiated = true;
        }
        private void WriteToLog(LogMsg msg)
        {
            if(!theadedLoggingInitiated)
            {
                InitThreadedLogging();
            }
            log.LogInformation($"{ThreadedExecution.RunID}  {msg.ServerName}  {msg.DatabaseName}:{msg.Message}");

            if (logEventHub != null && !string.IsNullOrWhiteSpace(msg.DatabaseName))
            {
                logEventHub.LogInformation("{@LogMsg}", msg);
            }

            switch (msg.LogType)
            {
                case LogType.FailureDatabases:
                    logFailures.LogInformation(this.sbFailureDatabasesCfg.ToString());
                    return;

                case LogType.SuccessDatabases:
                    logSuccess.LogInformation(this.sbSuccessDatabasesCfg.ToString());
                    return;

                case LogType.Commit:
                    logCommitRun.LogInformation($"{ThreadedExecution.RunID}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");
                    return;

                case LogType.Error:
                    logErrorRun.LogInformation($"{ThreadedExecution.RunID}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");
                    return;

                case LogType.Message:
                default:
                    logRuntime.LogInformation($"{ThreadedExecution.RunID}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");

                    return;
            }

        }

        private void SetRootAndWorkingPaths(string rootLoggingPath)
        {
            try
            {
                log.LogInformation($"Initializing logging paths: {rootLoggingPath}");

                //Set the logging folders, etc...s
                string expanded = System.Environment.ExpandEnvironmentVariables(rootLoggingPath);
                ThreadedExecution.rootLoggingPath = Path.GetFullPath(expanded);

                log.LogInformation($"Logging path expanded to: {ThreadedExecution.rootLoggingPath}");

                if (!Directory.Exists(ThreadedExecution.rootLoggingPath))
                {
                    Directory.CreateDirectory(ThreadedExecution.rootLoggingPath);
                }

                this.workingDirectory = Path.Combine(ThreadedExecution.rootLoggingPath, "Working");
                if (!Directory.Exists(this.workingDirectory))
                {
                    Directory.CreateDirectory(this.workingDirectory);
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to set root logging path for " + rootLoggingPath);
                throw;
            }
        }

    }
}
