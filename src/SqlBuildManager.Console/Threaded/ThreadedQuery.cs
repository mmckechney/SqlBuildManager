using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Status;
using Azure.Messaging.ServiceBus;

namespace SqlBuildManager.Console.Threaded
{
    internal class ThreadedQuery
    {
        private ILogger log = null!;
        private QueueManager qManager = null!;
        private ThreadedLogging threadLogger = null!;
        private string runId = string.Empty;
        
        internal async Task<int> QueryDatabasesAsync(CommandLineArgs cmdLine, string runId, CancellationToken cancellationToken = default)
        {
            this.runId = runId;
            threadLogger = new ThreadedLogging(cmdLine, runId);
            if (!string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                log = Logging.ApplicationLogging.CreateLogger<ThreadedQuery>(Program.applicationLogFileName, cmdLine.RootLoggingPath);
            }
            else
            {
                log = Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);
            }

            var query = File.ReadAllText(cmdLine.QueryFile.FullName);
            var connData = new ConnectionData() { UserId = cmdLine.AuthenticationArgs.UserName, Password = cmdLine.AuthenticationArgs.Password, AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType, ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId, DatabasePlatform = cmdLine.AuthenticationArgs.DatabasePlatform };
            // For PG MI auth, use identity name as UserId (PG role name)
            if (connData.DatabasePlatform == DatabasePlatform.PostgreSQL
                && string.IsNullOrEmpty(connData.UserId)
                && !string.IsNullOrEmpty(cmdLine.IdentityArgs.IdentityName))
            {
                connData.UserId = cmdLine.IdentityArgs.IdentityName;
            }

            try
            {
                //Run from override settings or from a Service Bus topic?
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    log.LogInformation($"Sourcing targets from Service Bus topic with Concurrency Type: {cmdLine.ConcurrencyType} and Concurency setting: {cmdLine.Concurrency}");
                    return await ExecuteQueryFromQueueAsync(cmdLine, connData, query, System.Environment.UserName, cancellationToken);
                }
                else
                {
                    var multiData = MultiDbHelper.ImportMultiDbTextConfig(cmdLine.MultiDbRunConfigFileName);
                    //Set the number of allowed retries...
                    multiData.AllowableTimeoutRetries = cmdLine.TimeoutRetryCount;
                    //Set Trial
                    multiData.RunAsTrial = cmdLine.Trial;
                    multiData.BuildRevision = cmdLine.BuildRevision;
                    log.LogInformation($"Sourcing targets from target override file with Concurrency Type: {cmdLine.ConcurrencyType} and Concurency setting: {cmdLine.Concurrency}");
                    var success = ExecuteQueryFromOverrides(cmdLine, multiData, connData, query);
                    return success ? 0 : 1;
                }
            }
            finally
            {

                if (qManager != null)
                {
                    qManager.Dispose();
                }
                //Make sure these logs are fully written
                threadLogger.Flush();
            }
        }
        private async Task<int> ExecuteQueryFromQueueAsync(CommandLineArgs cmdLine, ConnectionData connData, string query, string userName, CancellationToken cancellationToken = default)
        {
            const int QueuePollDelayMs = 5000;
            const int MaxNoMessageRetries = 4;
            
            log.LogInformation("Executing database queries from queued targets.");
            var resultCode = 0;
            var listResultsTempFiles = new List<string>();
            var tmpOutput = Path.Combine(Path.GetDirectoryName(cmdLine.OutputFile.FullName)!, Guid.NewGuid().ToString());
            qManager = new Queue.QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName, cmdLine.ConcurrencyType);
            var collector = new QueryCollector(null!, connData);
            if(!collector.EnsureOutputPath(tmpOutput))
            {
                return -3545;
            }
            bool messagesSinceLastLoop = true;
            int noMessagesCounter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                var messages = await qManager.GetDatabaseTargetFromQueue(cmdLine.Concurrency, cmdLine.ConcurrencyType);

                if (messages.Count == 0)
                {
                    if (cmdLine.RunningAsContainer)
                    {
                        log.LogInformation($"No messages found in Service Bus Topic. Waiting {QueuePollDelayMs / 1000} seconds to check again...");
                        var msg = new LogMsg() { RunId = this.runId, Message = $"Waiting for additional Service Bus messages on {Environment.MachineName}", LogType = LogType.Message };
                        threadLogger.WriteToLog(msg);
                        if (messagesSinceLastLoop)
                        {
                            await CloudStorage.StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, cmdLine.RootLoggingPath);
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
                    log.LogInformation($"{messages.Count} messages retrieved...");
                    messagesSinceLastLoop = true;
                    var tasks = new List<Task<(int, string)>>();
                    foreach (var message in messages)
                    {
                        var target = message.As<TargetMessage>();
                        var targetDb = target.DbOverrideSequence[0].OverrideDbTarget;
                    
                        if(target.ServerName.StartsWith("#"))
                        {
                            target.ServerName = target.DbOverrideSequence[0].Server ?? string.Empty;
                        }
                        var runner = new QueryCollectionRunner(target.ServerName, targetDb, query, new List<QueryRowItem>(), ReportType.CSV, tmpOutput, cmdLine.DefaultScriptTimeout, connData);
                        var msg = new LogMsg() { DatabaseName = targetDb, ServerName = target.ServerName, RunId = this.runId, Message = "Queuing up thread", LogType = LogType.Message, ConcurrencyTag = target.ConcurrencyTag };
                        threadLogger.WriteToLog(msg);
                        tasks.Add(ProcessThreadedQueryWithQueueAsync(runner, message, cancellationToken));
                    }
                    await Task.WhenAll(tasks);
                    listResultsTempFiles.AddRange(tasks.Select(t => t.Result.Item2).ToList());
                    resultCode += tasks.Sum(t => t.Result.Item1);
                }
               
            }
            log.LogInformation($"Generating consolidated results report from {listResultsTempFiles.Count} temporary files.");
            collector.GenerateReport(cmdLine.OutputFile.FullName, ReportType.CSV, listResultsTempFiles);

            return resultCode;
        }
        private async Task<(int, string)> ProcessThreadedQueryWithQueueAsync(QueryCollectionRunner runner, ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            var target = message.As<TargetMessage>();
            target.DbOverrideSequence[0].ConcurrencyTag = target.ConcurrencyTag;
            threadLogger.WriteToLog(new LogMsg() { DatabaseName = target.DbOverrideSequence[0].OverrideDbTarget, ServerName = target.ServerName, RunId = this.runId, Message = "Starting up thread", LogType = LogType.Message, ConcurrencyTag = target.DbOverrideSequence[0].ConcurrencyTag });
            var retVal = await runner.CollectQueryData();
            threadLogger.WriteToLog(new LogMsg() { DatabaseName = target.DbOverrideSequence[0].OverrideDbTarget, ServerName = target.ServerName, RunId = this.runId, Message = "Thread complete", LogType = LogType.Message, ConcurrencyTag = target.DbOverrideSequence[0].ConcurrencyTag });

            if (retVal.Item1 == 0)
            {
                await qManager.CompleteMessage(message);
                threadLogger.WriteToLog(new LogMsg() { DatabaseName = target.DbOverrideSequence[0].OverrideDbTarget, ServerName = target.ServerName, RunId = this.runId, Message = "Thread complete", LogType = LogType.Commit, ConcurrencyTag = target.DbOverrideSequence[0].ConcurrencyTag });
            }
            else
            {
                await qManager.DeadletterMessage(message);
                threadLogger.WriteToLog(new LogMsg() { DatabaseName = target.DbOverrideSequence[0].OverrideDbTarget, ServerName = target.ServerName, RunId = this.runId, Message = "Thread complete", LogType = LogType.Commit, ConcurrencyTag = target.DbOverrideSequence[0].ConcurrencyTag });
            }
            return retVal;
        }
        private bool ExecuteQueryFromOverrides(CommandLineArgs cmdLine, MultiDbData multiData, ConnectionData connData,string query)
        {
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(ThreadedQuery_ProgressChanged);
            var collector = new QueryCollector(multiData, connData);

            collector.BackgroundWorker = bg;

            var serverCount = multiData.Select(m => m.ServerName).Distinct().Count();
            var dbCount = multiData.Sum(d => d.Overrides.Count);

            log.LogInformation($"Running query across {serverCount} servers and {dbCount} databases...");
            bool success = collector.GetQueryResults(cmdLine.OutputFile.FullName, ReportType.CSV, query, cmdLine.DefaultScriptTimeout);

            if (!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.LogInformation("Writing log files to storage...");
                var blobTask = StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }

            return success;
        }
        
        internal void ThreadedQuery_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                log.LogInformation(e.UserState.ToString());
            }
            else if (e.UserState is QueryCollectionRunnerUpdateEventArgs)
            {
                // var x = (QueryCollectionRunnerUpdateEventArgs)e.UserState;
                //log.LogInformation($"{x.Server}:{x.Database} -- {x.Message}");
            }
        }
    }
}
