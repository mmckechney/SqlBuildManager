﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Threaded
{
    public class ThreadedManager
    {
        private static ILogger log = Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ThreadedLogging threadedLog; 

        MultiDbData multiData = null;
        DateTime startTime;
        bool hasError = false;
        bool theadedLoggingInitiated = false;
        private CommandLineArgs cmdLine = null;
        private QueueManager qManager = null;
        private int queueReturnValue = 0;
        private static string projectFileName = string.Empty;
        /// <summary>
        /// Path and file name to the XML metadata configuration project file (SqlSyncBuildProject.xml)
        /// </summary>
        internal static string ProjectFileName
        {
            get { return ThreadedManager.projectFileName; }
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
            get { return ThreadedManager.buildZipFileName; }
        }
        internal static string PlatinumDacPacFileName
        {
            get { return ThreadedManager.platinumDacPacFileName; }
        }

        private static string rootLoggingPath = string.Empty;
        /// <summary>
        /// The root folder where the logging should start
        /// </summary>
        internal static string RootLoggingPath
        {
            get { return ThreadedManager.rootLoggingPath; }
        }

        private static string workingDirectory = string.Empty;
        /// <summary>
        /// The root folder where the logging should start
        /// </summary>
        internal static string WorkingDirectory
        {
            get { return ThreadedManager.workingDirectory; }
        }

        private static string runID = string.Empty;
        /// <summary>
        /// "unique" identifier for the run. 
        /// </summary>
        public static string RunID
        {
            get
            {
                if (ThreadedManager.runID == string.Empty)
                    ThreadedManager.runID = Guid.NewGuid().ToString().Replace("-", "");

                return ThreadedManager.runID;
            }
        }

        private static ScriptBatchCollection batchColl = null;
        /// <summary>
        /// The pre-batched set of scripts to be run
        /// </summary>
        internal static ScriptBatchCollection BatchColl
        {
            get { return ThreadedManager.batchColl; }
        }

        private static SqlSyncBuildData buildData = null;
        /// <summary>
        /// The runtime metadata object for the build execution
        ///// </summary>
        internal static SqlSyncBuildData BuildData
        {
            get { return buildData; }
        }

        public ThreadedManager(CommandLineArgs cmd)
        {
            cmdLine = cmd;
        }

        /// <summary>
        /// Execute method that is used from a straight command-line execution
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            threadedLog = new ThreadedLogging(cmdLine, RunID);
            log.LogDebug("Entering Execute method of ThreadedExecution");
            string[] errorMessages;

            //Create Threaded Run specific loggers -- these are init'd when the first logs are written
            var batchWorking = Environment.GetEnvironmentVariable("AZ_BATCH_TASK_WORKING_DIR");
            if (!string.IsNullOrWhiteSpace(batchWorking)) //if running in Batch, set the root directory to the node working dir
            {
                cmdLine.RootLoggingPath = batchWorking;
            }
            else if (string.IsNullOrEmpty(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = Path.Combine(Directory.GetCurrentDirectory(), "tmp-sqlbuildlogging");
            }

            //Set paths
            SetRootAndWorkingPaths(cmdLine.RootLoggingPath);

            //Validate all of the arguments
            log.LogInformation("Validating command parameters");
            int tmpReturn = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                threadedLog.WriteToLog(msg);
                return tmpReturn;
            }

            //Start logging
            threadedLog.WriteToLog(new LogMsg() { Message = "**** Starting log for Run ID: " + ThreadedManager.RunID + " ****", LogType = LogType.Message });


            //Load multi-db data. Won't be used for a queue run unless we have a platinum DACPAC that will need this to create the SBM file
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) ||
                (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac) && !string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName)))
            {
                int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out multiData, out errorMessages);
                if (tmpValReturn != 0)
                {
                    var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                    threadedLog.WriteToLog(msg);
                    return tmpValReturn;
                }
            }
            //Determine where to get the scripts from (SBM, DACPAC, generated DACPAC, scripts?)
            int success;
            (success, cmdLine) = ConfigureScriptSource(cmdLine);
            if (success != 0)
            {
                return success;
            }

            //This will set the static variable for the script collection
            var prep = PrepBuildAndScripts(ThreadedManager.buildZipFileName, buildRequestedBy, cmdLine.DacPacArgs.ForceCustomDacPac);
            if (prep != 0)
            {
                return prep;
            }

            try
            {
                //Run from override settings or from a Service Bus topic?
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    log.LogInformation($"Sourcing targets from Service Bus topic with Concurrency Type: {cmdLine.ConcurrencyType} and Concurency setting: {cmdLine.Concurrency}");
                    Task<int> queueTask = ExecuteFromQueue(cmdLine, System.Environment.UserName, cmdLine.JobName);
                    queueTask.Wait();
                    return queueTask.Result;
                }
                else
                {
                    //Set the number of allowed retries...
                    multiData.AllowableTimeoutRetries = cmdLine.TimeoutRetryCount;
                    //Set Trial
                    multiData.RunAsTrial = cmdLine.Trial;
                    multiData.BuildRevision = cmdLine.BuildRevision;
                    log.LogInformation($"Sourcing targets from target override file with Concurrency Type: {cmdLine.ConcurrencyType} and Concurency setting: {cmdLine.Concurrency}");
                    return ExecuteFromOverrideFile(ThreadedManager.buildZipFileName, cmdLine.DacPacArgs.PlatinumDacpac, multiData, cmdLine.RootLoggingPath, cmdLine.Description, System.Environment.UserName, cmdLine.DacPacArgs.ForceCustomDacPac, cmdLine.Concurrency, cmdLine.ConcurrencyType);
                }
            }
            finally
            {
                //Really only needed when running unit tests, but still a good idea.
                ResetStaticValues();

                if (qManager != null)
                {
                    qManager.Dispose();
                }
                
                //Make sure these logs are fully written
                threadedLog.Flush();
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
                    threadedLog.WriteToLog(new LogMsg() { Message = exe.ToString(), LogType = LogType.Error });
                }

                //Wait for all of the tasks to finish
                Task.WaitAll(tasks.ToArray());


                TimeSpan interval = DateTime.Now - startTime;
                var finalMsg = new LogMsg() { RunId = ThreadedManager.RunID, Message = $"Ending threaded processing at {DateTime.Now.ToUniversalTime()}", LogType = LogType.Message };
                threadedLog.WriteToLog(finalMsg);
                finalMsg.Message = $"Execution Duration: {interval.ToString()}";
                threadedLog.WriteToLog(finalMsg);
                finalMsg.Message = $"Total number of targets: {targetTotal.ToString()}";
                threadedLog.WriteToLog(finalMsg);

                threadedLog.WriteToLog(new LogMsg() { LogType = LogType.SuccessDatabases });
                if (hasError)
                {
                    threadedLog.WriteToLog(new LogMsg() { LogType = LogType.FailureDatabases });
                    finalMsg.Message = "Finishing with Errors";
                    finalMsg.LogType = LogType.Error;
                    threadedLog.WriteToLog(finalMsg);
                    finalMsg.LogType = LogType.Message;
                    threadedLog.WriteToLog(finalMsg);
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
        private async Task<int> ExecuteFromQueue(CommandLineArgs cmdLine, string buildRequestedBy, string storageContainerName)
        {
            int waitMs = 5000;
            qManager = new Queue.QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName, cmdLine.ConcurrencyType);
            bool messagesSinceLastLoop = true;
            int noMessagesCounter = 0;
            while (true)
            {
                var messages = await qManager.GetDatabaseTargetFromQueue(cmdLine.Concurrency, cmdLine.ConcurrencyType);

                if (messages.Count == 0)
                {
                    if (cmdLine.RunningAsContainer)
                    {
                        log.LogInformation($"No messages found in Service Bus Topic. Waiting {waitMs/1000} seconds to check again...");
                        var msg = new LogMsg() { RunId = ThreadedManager.RunID, Message = $"Waiting for additional Service Bus messages on {Environment.MachineName}", LogType = LogType.Message };
                        threadedLog.WriteToLog(msg);
                        if (messagesSinceLastLoop)
                        {
                            await CloudStorage.StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, storageContainerName, cmdLine.RootLoggingPath);
                            noMessagesCounter = 0;
                        }
                        else
                        {
                            if (noMessagesCounter == 4)
                            {
                                log.LogInformation("No messages found in Service Bus Topic after 4 retries. Terminating Container.");
                                break;
                            }
                            noMessagesCounter++;
                        }
                        messagesSinceLastLoop = false;
                        System.Threading.Thread.Sleep(waitMs);
                    }
                    else
                    {
                        log.LogInformation("No more messages found in Service Bus Topic. Exiting.");
                        break;
                    }
                }
                else
                {
                    messagesSinceLastLoop = true;
                    var tasks = new List<Task<int>>();
                    foreach (var message in messages)
                    {
                        var target = message.As<TargetMessage>();
                        target.DbOverrideSequence[0].ConcurrencyTag = target.ConcurrencyTag;

                        ThreadedRunner runner = new ThreadedRunner(target.ServerName, target.DbOverrideSequence, cmdLine, buildRequestedBy, cmdLine.DacPacArgs.ForceCustomDacPac);
                        var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = ThreadedManager.RunID, Message = "Queuing up thread", LogType = LogType.Message, ConcurrencyTag = runner.ConcurrencyTag };
                        threadedLog.WriteToLog(msg);
                        tasks.Add(ProcessThreadedBuildWithQueue(runner, message));
                    }
                    Task.WaitAll(tasks.ToArray());


                }
            }
            return queueReturnValue;
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
                ThreadedManager.buildZipFileName = cmdLine.BuildFileName;
                string msg = "--packagename setting found. Using '" + ThreadedManager.buildZipFileName + "' as build source";
                threadedLog.WriteToLog(new LogMsg() { Message = msg, LogType = LogType.Message });
                log.LogInformation(msg);
            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac)) //using a platinum dacpac as a source
            {
                ThreadedManager.platinumDacPacFileName = cmdLine.DacPacArgs.PlatinumDacpac;
                string msg = "--platinumdacpac setting found. Using '" + ThreadedManager.platinumDacPacFileName + "' as build source";
                threadedLog.WriteToLog(new LogMsg() { Message = msg, LogType = LogType.Message }); ;
                log.LogInformation(msg);

            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource)) //using a platinum database as the source
            {
                log.LogInformation($"Extracting Platinum Dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                string dacpacName = Path.Combine(ThreadedManager.rootLoggingPath, cmdLine.DacPacArgs.PlatinumDbSource + ".dacpac");

                if (!DacPacHelper.ExtractDacPac(cmdLine.DacPacArgs.PlatinumDbSource, cmdLine.DacPacArgs.PlatinumServerSource, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, dacpacName, cmdLine.DefaultScriptTimeout, cmdLine.IdentityArgs.ClientId))
                {
                    var m = new LogMsg()
                    {
                        Message = $"Error creating the Platinum dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}",
                        LogType = LogType.Error
                    };
                    threadedLog.WriteToLog(m);
                    return ((int)ExecutionReturn.BuildFileExtractionError, cmdLine);

                }
                cmdLine.DacPacArgs.PlatinumDacpac = dacpacName;
                ThreadedManager.platinumDacPacFileName = dacpacName;

            }

            int tmpValReturn;
            //Check for the platinum dacpac and configure it if necessary
            (tmpValReturn, cmdLine) = Validation.ValidateAndLoadPlatinumDacpac(cmdLine, multiData);
            if (tmpValReturn == 0 && string.IsNullOrEmpty(ThreadedManager.buildZipFileName))
            {
                ThreadedManager.buildZipFileName = cmdLine.BuildFileName;
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
            ThreadedManager.buildZipFileName = buildZipFileName;
            this.buildRequestedBy = buildRequestedBy;

            //Looks like we're good to go... extract the build Zip file (.sbm) into a working folder...

            if (!forceCustomDacpac)
            {
                ExtractAndLoadBuildFile(ThreadedManager.buildZipFileName, out ThreadedManager.buildData);
                if (buildData == null)
                {
                    var msg = new LogMsg()
                    {
                        Message = "Unable to procees. SqlSyncBuild data object is null, Returning error code: " + (int)ExecutionReturn.NullBuildData,
                        LogType = LogType.Error
                    };
                    threadedLog.WriteToLog(msg);
                    return (int)ExecutionReturn.NullBuildData;
                }
                else
                {
                    //Load up the batched scripts into a shared object so that we can conserve memory
                    ThreadedManager.batchColl = SqlBuildHelper.LoadAndBatchSqlScripts(ThreadedManager.buildData, projectFilePath);
                }

            }
            return 0;

        }

        private async Task<int> ProcessConcurrencyBucket(IEnumerable<(string, List<DatabaseOverride>)> bucket, bool forceCustomDacpac)
        {
            foreach ((string server, List<DatabaseOverride> ovr) in bucket)
            {
                ThreadedRunner runner = new ThreadedRunner(server, ovr, cmdLine, buildRequestedBy, forceCustomDacpac);
                var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = ThreadedManager.RunID, Message = "Queuing up thread", LogType = LogType.Message, ConcurrencyTag = runner.ConcurrencyTag };
                threadedLog.WriteToLog(msg);
                await ProcessThreadedBuild(runner);
            }
            return 0;
        }
        private async Task<int> ProcessThreadedBuildWithQueue(ThreadedRunner runner, ServiceBusReceivedMessage message)
        {
            //Renew the lock on the message every 30 seconds
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var buildTask = ProcessThreadedBuild(runner);
            while (buildTask.Status != TaskStatus.RanToCompletion && buildTask.Status != TaskStatus.Faulted && buildTask.Status != TaskStatus.Canceled)
            {
                if (timer.Elapsed.TotalSeconds >= 30)
                {
                    await this.qManager.RenewMessageLock(message);
                    timer.Restart();
                }
                await Task.Delay(1000);
            }
            timer.Stop();
            
            //Make sure task is done and get result
            buildTask.Wait();
            var retVal = buildTask.Result;
            
            RunnerReturn tmp;
            Enum.TryParse<RunnerReturn>(retVal.ToString(), out tmp);
            switch (tmp)
            {
                case RunnerReturn.SuccessWithTrialRolledBack:
                case RunnerReturn.BuildCommitted:
                case RunnerReturn.CommittedWithCustomDacpac:
                case RunnerReturn.DacpacDatabasesInSync:
                    await qManager.CompleteMessage(message);
                    return 0;
                default:
                    await qManager.DeadletterMessage(message);
                    queueReturnValue += 1;
                    return 1;

            }
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

                msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, SourceDacPac = runner.DacpacName, RunId = ThreadedManager.RunID, Message = "Starting up thread", LogType = LogType.Message, ConcurrencyTag = runner.ConcurrencyTag};
                threadedLog.WriteToLog(msg);

                //Run the scripts!!
                await runner.RunDatabaseBuild(threadedLog);

                msg.Message = ((RunnerReturn)runner.ReturnValue).GetDescription();
                returnVal = runner.ReturnValue;
                switch (returnVal)
                {
                    case (int)RunnerReturn.BuildCommitted:
                    case (int)RunnerReturn.DacpacDatabasesInSync:
                    case (int)RunnerReturn.CommittedWithCustomDacpac:
                    case (int)RunnerReturn.SuccessWithTrialRolledBack:
                        msg.LogType = LogType.Commit;
                        threadedLog.WriteToLog(msg);
                        msg.LogType = LogType.SuccessDatabases;
                        msg.Message = cfgString;
                        threadedLog.WriteToLog(msg);
                        returnVal = 0;
                        break;

                    case (int)RunnerReturn.RolledBack:
                    case (int)RunnerReturn.BuildErrorNonTransactional:
                    default:
                        msg.LogType = LogType.Error;
                        threadedLog.WriteToLog(msg);
                        msg.LogType = LogType.FailureDatabases;
                        msg.Message = cfgString;
                        threadedLog.WriteToLog(msg);
                        hasError = true;
                        break;
                }

                msg.Message = "Thread complete";
                msg.LogType = LogType.Message;
                threadedLog.WriteToLog(msg);
                runner = null;

            }
            catch (Exception exe)
            {
                msg.Message = exe.Message;
                msg.LogType = LogType.Error;
                threadedLog.WriteToLog(msg);
            }
            return returnVal;
        }

        private int ExtractAndLoadBuildFile(string sqlBuildProjectFileName, out SqlSyncBuildData buildData)
        {
            log.LogInformation($"Extracting build file '{sqlBuildProjectFileName}' to working directory '{ThreadedManager.WorkingDirectory}'");

            buildData = null;

            Directory.CreateDirectory(ThreadedManager.WorkingDirectory);

            string result;
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sqlBuildProjectFileName, ref ThreadedManager.workingDirectory, ref projectFilePath, ref ThreadedManager.projectFileName, false, true, out result))
            {
                var msg = new LogMsg()
                {
                    Message = $"Zip extraction error. Unable to Extract Sql Build file at '{sqlBuildProjectFileName}'. Do you need to specify a full directory path? {result}",
                    LogType = LogType.Error
                };
                threadedLog.WriteToLog(msg);
                return (int)ExecutionReturn.BuildFileExtractionError;

            }

            if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, ThreadedManager.projectFileName, false))
            {
                var msg = new LogMsg()
                {
                    Message = $"Build project load error. Unable to load project file.",
                    LogType = LogType.Error
                };
                threadedLog.WriteToLog(msg);
                return (int)ExecutionReturn.LoadProjectFileError;
            }

            return 0;
        }

        private void ConstructBuildFileFromScriptDirectory(string directoryName)
        {
            log.LogInformation("Constructing build file from script directory");
            string shortFileName = string.Empty;
            ThreadedManager.buildZipFileName = Path.Combine(ThreadedManager.rootLoggingPath, ThreadedManager.RunID + ".sbm");
            string projFileName = Path.Combine(ThreadedManager.WorkingDirectory, SqlSync.SqlBuild.XmlFileNames.MainProjectFile);
            SqlSyncBuildData localBuildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            List<string> fileList = new List<string>(Directory.GetFiles(directoryName, "*.sql", SearchOption.TopDirectoryOnly));
            fileList.Sort();

            for (int i = 0; i < fileList.Count; i++)
            {
                shortFileName = Path.GetFileName(fileList[i]);
                File.Copy(fileList[i], Path.Combine(ThreadedManager.WorkingDirectory, shortFileName), true);

                SqlBuildFileHelper.AddScriptFileToBuild(ref localBuildData,
                    projFileName,
                    shortFileName,
                    i,
                   "'" + fileList[i] + "' added via source directory setting.",
                    true,
                    true,
                    "",
                    false,
                    ThreadedManager.buildZipFileName,
                    ((i < fileList.Count - 1) ? false : true),
                    false,
                    System.Environment.UserDomainName + @"/" + System.Environment.UserName,
                    20,
                    "");
            }

        }

        private void ResetStaticValues()
        {
            ThreadedManager.runID = string.Empty;
            ThreadedManager.platinumDacPacFileName = string.Empty;
            ThreadedManager.buildZipFileName = string.Empty;
            ThreadedManager.projectFileName = string.Empty;
            ThreadedManager.rootLoggingPath = string.Empty;
            ThreadedManager.workingDirectory = string.Empty;
            ThreadedManager.batchColl = null;
            ThreadedManager.buildData = null;
            ThreadedLogging.TheadedLoggingInitiated = false;
        }
   
        private void SetRootAndWorkingPaths(string rootLoggingPath)
        {
            try
            {
                log.LogInformation($"Initializing logging paths: {rootLoggingPath}");

                //Set the logging folders, etc...s
                string expanded = System.Environment.ExpandEnvironmentVariables(rootLoggingPath);
                ThreadedManager.rootLoggingPath = Path.GetFullPath(expanded);

                log.LogInformation($"Logging path expanded to: {ThreadedManager.rootLoggingPath}");

                if (!Directory.Exists(ThreadedManager.rootLoggingPath))
                {
                    Directory.CreateDirectory(ThreadedManager.rootLoggingPath);
                }

                ThreadedManager.workingDirectory = Path.Combine(ThreadedManager.rootLoggingPath, "Working");
                if (!Directory.Exists(ThreadedManager.WorkingDirectory))
                {
                    Directory.CreateDirectory(ThreadedManager.WorkingDirectory);
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
