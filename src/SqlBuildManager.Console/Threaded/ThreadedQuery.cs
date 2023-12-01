﻿using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Status;
using Azure.Messaging.ServiceBus;
using Azure.ResourceManager.Resources.Models;

namespace SqlBuildManager.Console.Threaded
{
    internal class ThreadedQuery
    {
        private ILogger log;
        private QueueManager qManager = null;
        private ThreadedLogging threadLogger;
        private string runId;
        internal int QueryDatabases(CommandLineArgs cmdLine, string runId)
        {
            this.runId = runId;
            threadLogger = new ThreadedLogging(cmdLine, runId);
            if (!string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                log = Logging.ApplicationLogging.CreateLogger<ThreadedQuery>(Program.applicationLogFileName, cmdLine.RootLoggingPath);
            }
            else
            {
                log = Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            }

            var query = File.ReadAllText(cmdLine.QueryFile.FullName);
            var connData = new ConnectionData() { UserId = cmdLine.AuthenticationArgs.UserName, Password = cmdLine.AuthenticationArgs.Password, AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType, ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId };

            try
            {
                //Run from override settings or from a Service Bus topic?
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    log.LogInformation($"Sourcing targets from Service Bus topic with Concurrency Type: {cmdLine.ConcurrencyType} and Concurency setting: {cmdLine.Concurrency}");
                    Task<int> queueTask = ExecuteQueryFromQueue(cmdLine, connData, query, System.Environment.UserName);
                    log.LogInformation($"Waiting for ExeuteQueryFromQueue to complete...");
                    queueTask.Wait();
                    log.LogInformation($"ExeuteQueryFromQueue wait completed. Task status is {queueTask.Status}");
                    return queueTask.Result;
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
                    var success = ExecuteQueryFromOverrides(cmdLine,multiData, connData, query);
                    if(success)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1; 
                    }
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
        private async Task<int> ExecuteQueryFromQueue(CommandLineArgs cmdLine, ConnectionData connData, string query, string userName)
        {
            int waitMs = 5000;
            log.LogInformation("Executing database queries from queued targets.");
            var resultCode = 0;
            var listResultsTempFiles = new List<string>();
            var tmpOutput = Path.Combine(Path.GetDirectoryName(cmdLine.OutputFile.FullName), Guid.NewGuid().ToString());
            qManager = new Queue.QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName, cmdLine.ConcurrencyType);
            var collector = new QueryCollector(null, connData);
            if(!collector.EnsureOutputPath(tmpOutput))
            {
                return -3545;
            }
            bool messagesSinceLastLoop = true;
            int noMessagesCounter = 0;
            while (true)
            {
                var messages = await qManager.GetDatabaseTargetFromQueue(cmdLine.Concurrency, cmdLine.ConcurrencyType);

                if (messages.Count == 0)
                {
                    if (cmdLine.RunningAsContainer)
                    {
                        log.LogInformation($"No messages found in Service Bus Topic. Waiting {waitMs / 1000} seconds to check again...");
                        var msg = new LogMsg() { RunId = ThreadedManager.RunID, Message = $"Waiting for additional Service Bus messages on {Environment.MachineName}", LogType = LogType.Message };
                        threadLogger.WriteToLog(msg);
                        if (messagesSinceLastLoop)
                        {
                            await CloudStorage.StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, cmdLine.RootLoggingPath);
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
                    log.LogInformation($"{messages.Count} messages retrieved...");
                    messagesSinceLastLoop = true;
                    var tasks = new List<Task<(int, string)>>();
                    foreach (var message in messages)
                    {
                        var target = message.As<TargetMessage>();
                        var targetDb = target.DbOverrideSequence[0].OverrideDbTarget;
                    
                        if(target.ServerName.StartsWith("#"))
                        {
                            target.ServerName = target.DbOverrideSequence[0].Server;
                        }
                        var runner = new QueryCollectionRunner(target.ServerName, targetDb, query, new List<QueryRowItem>(), ReportType.CSV, tmpOutput,cmdLine.DefaultScriptTimeout, connData);
                        var msg = new LogMsg() { DatabaseName = targetDb, ServerName = target.ServerName, RunId = this.runId, Message = "Queuing up thread", LogType = LogType.Message, ConcurrencyTag = target.ConcurrencyTag };
                        threadLogger.WriteToLog(msg);
                        tasks.Add(ProcessThreadedQueryWithQueue(runner, message));
                    }
                    Task.WaitAll(tasks.ToArray());
                    listResultsTempFiles.AddRange(tasks.Select(t => t.Result.Item2).ToList());
                    resultCode += tasks.Sum(t => t.Result.Item1);
                }
               
            }
            log.LogInformation($"Generating consolidated results report from {listResultsTempFiles.Count} temporary files.");
            collector.GenerateReport(cmdLine.OutputFile.FullName, ReportType.CSV, listResultsTempFiles);

            return resultCode;
        }
        private async Task<(int, string)> ProcessThreadedQueryWithQueue(QueryCollectionRunner runner, ServiceBusReceivedMessage message)
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
        
        internal void ThreadedQuery_ProgressChanged(object sender, ProgressChangedEventArgs e)
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
