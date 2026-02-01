using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Threaded
{
    public class ThreadedManager
    {
        private static ILogger log = Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ThreadedLogging threadedLog;
        
        /// <summary>
        /// The execution context containing shared state for this run.
        /// </summary>
        private readonly BuildExecutionContext _context;
        
        /// <summary>
        /// Gets the execution context for this manager instance.
        /// </summary>
        public BuildExecutionContext Context => _context;

        MultiDbData multiData = null;
        DateTime startTime;
        bool hasError = false;
        private CommandLineArgs cmdLine = null;
        private QueueManager qManager = null;
        private int queueReturnValue = 0;
        
        string projectFilePath = string.Empty;

        private string buildRequestedBy = string.Empty;

        private readonly IScriptBatcher _scriptBatcher;
        public ThreadedManager(CommandLineArgs cmd, IScriptBatcher scriptBatcher = null, BuildExecutionContext context = null)
        {
            cmdLine = cmd;
            _scriptBatcher = scriptBatcher ?? new DefaultScriptBatcher();
            _context = context ?? new BuildExecutionContext();
        }

        /// <summary>
        /// Execute method that is used from a straight command-line execution
        /// </summary>
        /// <returns></returns>
        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            threadedLog = new ThreadedLogging(cmdLine, _context.RunId);
            log.LogDebug("Entering ExecuteAsync method of ThreadedExecution");
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
            threadedLog.WriteToLog(new LogMsg() { Message = "**** Starting log for Run ID: " + _context.RunId + " ****", LogType = LogType.Message });


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
            var prep = await PrepBuildAndScriptsAsync(_context.BuildZipFileName, buildRequestedBy, cmdLine.DacPacArgs.ForceCustomDacPac).ConfigureAwait(false);
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
                    return await ExecuteFromQueueAsync(cmdLine, System.Environment.UserName, cmdLine.JobName, cancellationToken);
                }
                else
                {
                    //Set the number of allowed retries...
                    multiData.AllowableTimeoutRetries = cmdLine.TimeoutRetryCount;
                    //Set Trial
                    multiData.RunAsTrial = cmdLine.Trial;
                    multiData.BuildRevision = cmdLine.BuildRevision;
                    log.LogInformation($"Sourcing targets from target override file with Concurrency Type: {cmdLine.ConcurrencyType} and Concurency setting: {cmdLine.Concurrency}");
                    return await ExecuteFromOverrideFileAsync(_context.BuildZipFileName, cmdLine.DacPacArgs.PlatinumDacpac, multiData, cmdLine.RootLoggingPath, cmdLine.Description, System.Environment.UserName, cmdLine.DacPacArgs.ForceCustomDacPac, cmdLine.Concurrency, cmdLine.ConcurrencyType, cancellationToken);
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
        /// Execute method that is used inherently from the ExecuteAsync() 
        /// </summary>
        /// <returns></returns>
        private async Task<int> ExecuteFromOverrideFileAsync(string buildZipFileName, string platinumDacPacFileName, MultiDbData multiData, string rootLoggingPath, string description, string buildRequestedBy, bool forceCustomDacpac, int concurrency = 20, ConcurrencyType concurrencyType = ConcurrencyType.Count, CancellationToken cancellationToken = default)
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
                        tasks.Add(ProcessConcurrencyBucketAsync(bucket, forceCustomDacpac, cancellationToken));
                    }
                }
                catch (Exception exe)
                {
                    threadedLog.WriteToLog(new LogMsg() { Message = exe.ToString(), LogType = LogType.Error });
                }

                //Wait for all of the tasks to finish
                await Task.WhenAll(tasks);


                TimeSpan interval = DateTime.Now - startTime;
                var finalMsg = new LogMsg() { RunId = _context.RunId, Message = $"Ending threaded processing at {DateTime.Now.ToUniversalTime()}", LogType = LogType.Message };
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
        private async Task<int> ExecuteFromQueueAsync(CommandLineArgs cmdLine, string buildRequestedBy, string storageContainerName, CancellationToken cancellationToken = default)
        {
            const int QueuePollDelayMs = 5000;
            const int MaxNoMessageRetries = 4;
            
            qManager = new Queue.QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName, cmdLine.ConcurrencyType);
            bool messagesSinceLastLoop = true;
            int noMessagesCounter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                var messages = await qManager.GetDatabaseTargetFromQueue(cmdLine.Concurrency, cmdLine.ConcurrencyType);

                if (messages.Count == 0)
                {
                    if (cmdLine.RunningAsContainer)
                    {
                        log.LogInformation($"No messages found in Service Bus Topic. Waiting {QueuePollDelayMs/1000} seconds to check again...");
                        var msg = new LogMsg() { RunId = _context.RunId, Message = $"Waiting for additional Service Bus messages on {Environment.MachineName}", LogType = LogType.Message };
                        threadedLog.WriteToLog(msg);
                        if (messagesSinceLastLoop)
                        {
                            await CloudStorage.StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, storageContainerName, cmdLine.RootLoggingPath);
                            noMessagesCounter = 0;
                        }
                        else
                        {
                            if (noMessagesCounter == MaxNoMessageRetries)
                            {
                                log.LogInformation($"No messages found in Service Bus Topic after {MaxNoMessageRetries} retries. Terminating Container.");
                                break;
                            }
                            noMessagesCounter++;
                        }
                        messagesSinceLastLoop = false;
                        await Task.Delay(QueuePollDelayMs, cancellationToken);
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

                        ThreadedRunner runner = new ThreadedRunner(target.ServerName, target.DbOverrideSequence, cmdLine, buildRequestedBy, cmdLine.DacPacArgs.ForceCustomDacPac, _context);
                        var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = _context.RunId, Message = "Queuing up thread", LogType = LogType.Message, ConcurrencyTag = runner.ConcurrencyTag };
                        threadedLog.WriteToLog(msg);
                        tasks.Add(ProcessThreadedBuildWithQueueAsync(runner, message, cancellationToken));
                    }
                    await Task.WhenAll(tasks);


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
                _context.BuildZipFileName = cmdLine.BuildFileName;
                string msg = "--packagename setting found. Using '" + _context.BuildZipFileName + "' as build source";
                threadedLog.WriteToLog(new LogMsg() { Message = msg, LogType = LogType.Message });
                log.LogInformation(msg);
            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac)) //using a platinum dacpac as a source
            {
                _context.PlatinumDacPacFileName = cmdLine.DacPacArgs.PlatinumDacpac;
                string msg = "--platinumdacpac setting found. Using '" + _context.PlatinumDacPacFileName + "' as build source";
                threadedLog.WriteToLog(new LogMsg() { Message = msg, LogType = LogType.Message }); ;
                log.LogInformation(msg);

            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource)) //using a platinum database as the source
            {
                log.LogInformation($"Extracting Platinum Dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                string dacpacName = Path.Combine(_context.RootLoggingPath, cmdLine.DacPacArgs.PlatinumDbSource + ".dacpac");

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
                _context.PlatinumDacPacFileName = dacpacName;

            }

            int tmpValReturn;
            //Check for the platinum dacpac and configure it if necessary
            (tmpValReturn, cmdLine) = Validation.ValidateAndLoadPlatinumDacpac(cmdLine, multiData);
            if (tmpValReturn == 0 && string.IsNullOrEmpty(_context.BuildZipFileName))
            {
                _context.BuildZipFileName = cmdLine.BuildFileName;
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

        private async Task<int> PrepBuildAndScriptsAsync(string buildZipFileName, string buildRequestedBy, bool forceCustomDacpac)
        {
            _context.BuildZipFileName = buildZipFileName;
            this.buildRequestedBy = buildRequestedBy;

            //Looks like we're good to go... extract the build Zip file (.sbm) into a working folder...

            if (!forceCustomDacpac)
            {
                var extractResult = await ExtractAndLoadBuildFileAsync(_context.BuildZipFileName).ConfigureAwait(false);
                if (extractResult.returnCode != 0)
                {
                    return extractResult.returnCode;
                }
                _context.BuildDataModel = extractResult.model;
                if (_context.BuildDataModel == null)
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
                    _context.BatchCollection = _scriptBatcher.LoadAndBatchSqlScripts(_context.BuildDataModel, projectFilePath);
                }

            }
            return 0;

        }

        private async Task<int> ProcessConcurrencyBucketAsync(IEnumerable<(string, List<DatabaseOverride>)> bucket, bool forceCustomDacpac, CancellationToken cancellationToken = default)
        {
            foreach ((string server, List<DatabaseOverride> ovr) in bucket)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ThreadedRunner runner = new ThreadedRunner(server, ovr, cmdLine, buildRequestedBy, forceCustomDacpac, _context);
                var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = _context.RunId, Message = "Queuing up thread", LogType = LogType.Message, ConcurrencyTag = runner.ConcurrencyTag };
                threadedLog.WriteToLog(msg);
                await ProcessThreadedBuildAsync(runner, cancellationToken);
            }
            return 0;
        }
        private async Task<int> ProcessThreadedBuildWithQueueAsync(ThreadedRunner runner, ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            const int MessageLockRenewalSeconds = 30;
            
            //Renew the lock on the message every 30 seconds
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var buildTask = ProcessThreadedBuildAsync(runner, cancellationToken);
            while (!buildTask.IsCompleted)
            {
                if (timer.Elapsed.TotalSeconds >= MessageLockRenewalSeconds)
                {
                    await this.qManager.RenewMessageLock(message);
                    timer.Restart();
                }
                await Task.Delay(1000, cancellationToken);
            }
            timer.Stop();
            
            //Get result - task is already completed
            var retVal = await buildTask;
            
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
        private async Task<RunnerReturn> ProcessThreadedBuildAsync(ThreadedRunner runner, CancellationToken cancellationToken = default)
        {
            var msg = new LogMsg();
            var returnVal = RunnerReturn.BuildResultInconclusive;
            try
            {

                //ThreadedRunner runner = (ThreadedRunner)state;
                //SERVER:defaultDb,override
                string cfgString = String.Format("{0}:{1},{2}", runner.Server, runner.DefaultDatabaseName, runner.TargetDatabases);

                msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, SourceDacPac = runner.DacpacName, RunId = _context.RunId, Message = "Starting up thread", LogType = LogType.Message, ConcurrencyTag = runner.ConcurrencyTag};
                threadedLog.WriteToLog(msg);

                //Run the scripts!!
                await runner.RunDatabaseBuildAsync(threadedLog, cancellationToken);

                msg.Message = runner.ReturnValue.GetDescription();
                returnVal =  runner.ReturnValue;
                log.LogDebug($"RunnerReturn value for {runner.Server}:{runner.TargetDatabases} =>  {returnVal}");
                switch (returnVal)
                {
                    case RunnerReturn.BuildCommitted:
                    case RunnerReturn.DacpacDatabasesInSync:
                    case RunnerReturn.CommittedWithCustomDacpac:
                    case RunnerReturn.SuccessWithTrialRolledBack:
                        msg.LogType = LogType.Commit;
                        threadedLog.WriteToLog(msg);
                        msg.LogType = LogType.SuccessDatabases;
                        msg.Message = cfgString;
                        threadedLog.WriteToLog(msg);
                        returnVal = 0;
                        break;

                    case RunnerReturn.RolledBack:
                    case RunnerReturn.BuildErrorNonTransactional:
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

        private async Task<(int returnCode, SqlSyncBuildDataModel model)> ExtractAndLoadBuildFileAsync(string sqlBuildProjectFileName)
        {
            log.LogInformation($"Extracting build file '{sqlBuildProjectFileName}' to working directory '{_context.WorkingDirectory}'");

            await Task.Run(() => Directory.CreateDirectory(_context.WorkingDirectory)).ConfigureAwait(false);

            string workDir = _context.WorkingDirectory;
            var extractResult = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(sqlBuildProjectFileName, workingDirectory: workDir, resetWorkingDirectory: false, overwriteExistingProjectFiles: true).ConfigureAwait(false);
            if (!extractResult.success)
            {
                var msg = new LogMsg()
                {
                    Message = $"Zip extraction error. Unable to Extract Sql Build file at '{sqlBuildProjectFileName}'. Do you need to specify a full directory path? {extractResult.result}",
                    LogType = LogType.Error
                };
                threadedLog.WriteToLog(msg);
                return ((int)ExecutionReturn.BuildFileExtractionError, null);

            }
            _context.WorkingDirectory = extractResult.workingDirectory;
            _context.ProjectFileName = extractResult.projectFileName;
            projectFilePath = extractResult.projectFilePath;

            var (loadSuccess, model) = await SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(_context.ProjectFileName, validateSchema: false).ConfigureAwait(false);
            if (!loadSuccess)
            {
                var msg = new LogMsg()
                {
                    Message = $"Build project load error. Unable to load project file.",
                    LogType = LogType.Error
                };
                threadedLog.WriteToLog(msg);
                return ((int)ExecutionReturn.LoadProjectFileError, null);
            }

            return (0, model);
        }

        private void ConstructBuildFileFromScriptDirectory(string directoryName)
        {
            log.LogInformation("Constructing build file from script directory");
            string shortFileName = string.Empty;
            _context.BuildZipFileName = Path.Combine(_context.RootLoggingPath, _context.RunId + ".sbm");
            string projFileName = Path.Combine(_context.WorkingDirectory, SqlSync.SqlBuild.XmlFileNames.MainProjectFile);
            SqlSyncBuildDataModel localBuildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            List<string> fileList = new List<string>(Directory.GetFiles(directoryName, "*.sql", SearchOption.TopDirectoryOnly));
            fileList.Sort();

            for (int i = 0; i < fileList.Count; i++)
            {
                shortFileName = Path.GetFileName(fileList[i]);
                File.Copy(fileList[i], Path.Combine(_context.WorkingDirectory, shortFileName), true);

                localBuildData = SqlBuildFileHelper.AddScriptFileToBuild(localBuildData,
                    projFileName,
                    shortFileName,
                    i,
                   "'" + fileList[i] + "' added via source directory setting.",
                    true,
                    true,
                    "",
                    false,
                    _context.BuildZipFileName,
                    ((i < fileList.Count - 1) ? false : true),
                    false,
                    System.Environment.UserDomainName + @"/" + System.Environment.UserName,
                    20,
                    Guid.NewGuid(),
                    "");
            }

        }

        private void ResetStaticValues()
        {
            _context.Reset();
            ThreadedLogging.TheadedLoggingInitiated = false;
        }
   
        private void SetRootAndWorkingPaths(string rootLoggingPath)
        {
            try
            {
                log.LogInformation($"Initializing logging paths: {rootLoggingPath}");

                //Set the logging folders, etc...s
                string expanded = System.Environment.ExpandEnvironmentVariables(rootLoggingPath);
                _context.RootLoggingPath = Path.GetFullPath(expanded);

                log.LogInformation($"Logging path expanded to: {_context.RootLoggingPath}");

                if (!Directory.Exists(_context.RootLoggingPath))
                {
                    Directory.CreateDirectory(_context.RootLoggingPath);
                }

                _context.WorkingDirectory = Path.Combine(_context.RootLoggingPath, "Working");
                if (!Directory.Exists(_context.WorkingDirectory))
                {
                    Directory.CreateDirectory(_context.WorkingDirectory);
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
