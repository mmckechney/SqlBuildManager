using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using clb = SqlBuildManager.Console.CommandLine.CommandLineBuilder;
using sb = SqlSync.SqlBuild;

namespace SqlBuildManager.Console
{
    internal partial class Worker : IHostedService
    {
        public const string applicationLogFileName = "SqlBuildManager.Console.log";

        private IHostApplicationLifetime applicationLifetime;
        private static int? exitCode;
        internal static string[] AppendLogFiles = new string[] { "commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg" };
        private static StartArgs startArgs;
        private static CommandLineArgs cmdLine;
        internal static ILogger log;
        static Worker()
        {
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(applicationLogFileName);
        }
        public Worker(Worker.StartArgs startArgs, CommandLineArgs cmdLine, IHostApplicationLifetime appLifetime)
        {
            Worker.startArgs = startArgs;
            Worker.cmdLine = cmdLine;
            applicationLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            var fn = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var currentPath = Path.GetDirectoryName(fn);

            log.LogDebug("Received Command: " + String.Join(" | ", Worker.startArgs.Args));

            applicationLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    var parser = clb.GetCommandParser();

                    try
                    {
                        int result = await parser.InvokeAsync(Worker.startArgs.Args); //  rootCommand.InvokeAsync(Worker.startArgs.Args);
                        exitCode = result;

                    }
                    catch (OperationCanceledException)
                    {
                        log.LogInformation("The job has been killed with CTRL+C");
                        exitCode = -3;
                    }
                    catch (Exception exe)
                    {
                        System.Console.WriteLine($"Error closing: {exe.Message}");
                        System.Environment.FailFast("");
                        exitCode = -100000;
                    }
                    finally
                    {
                        SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                        applicationLifetime.StopApplication();
                    }
                });
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.ExitCode = exitCode.GetValueOrDefault(-1);
            log.LogInformation($"Exiting with code {Environment.ExitCode}");
            SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
            return Task.CompletedTask;
        }

        internal static (bool, CommandLineArgs) Init(CommandLineArgs cmdLine, bool clearText = false)
        {
            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
            }
            else
            {
                AadHelper.ManagedIdentityClientId = string.Empty;
            }

            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            }
            else
            {
                AadHelper.TenantId = string.Empty;
            }
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            if (!clearText)
            {
                bool decryptSuccess;
                (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
                if (!decryptSuccess)
                {
                    log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                }
            }
            bool tmp;
            (tmp, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            return (decryptSuccess, cmdLine);
        }

        internal static int QueryDatabases(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;


            if (!string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);
            }

            var outpt = Validation.ValidateQueryArguments(ref cmdLine);
            if (outpt != 0)
            {
                return outpt;
            }

            var query = File.ReadAllText(cmdLine.QueryFile.FullName);
            var runId = Guid.NewGuid().ToString().Replace("-", "");
            var success = new ThreadedQuery().QueryDatabases(cmdLine,runId);

            if (success == 0 )
            {
                log.LogInformation($"Query complete. The results are in the output file: {cmdLine.OutputFile.FullName}");
            }
            else
            {
                log.LogError("There was an issue collecting and aggregating the query results");
                return 6;
            }

            return 0;

        }
        
        internal async static Task<int> EnqueueOverrideTargets(CommandLineArgs cmdLine)
        {
            (bool success, cmdLine) = Init(cmdLine);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(applicationLogFileName);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(applicationLogFileName, cmdLine.RootLoggingPath);
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            log.LogInformation("Enqueuing Database override targets");

            if(!success)
            {
                log.LogError("Problem reading configuration");
                return 3424;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("A --servicebusconnection value is required. Please include this in either the settings file content or as a specific command option");
                return 9839;
            }
            (int ret, string msg) = Validation.ValidateBatchjobName(cmdLine.BatchArgs.BatchJobName);
            if (ret != 0)
            {
                log.LogError(msg);
                return ret;
            }

            int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out MultiDbData multiData, out string[] errorMessages);
            if (tmpValReturn != 0)
            {
                log.LogError(String.Join(";", errorMessages));
                return tmpValReturn;
            }
            DateTime start = DateTime.Now;
            
            log.LogInformation("Sending database targets to Service Bus");
            var qManager = new QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName, cmdLine.ConcurrencyType);
            int messages = await qManager.SendTargetsToQueue(multiData, cmdLine.ConcurrencyType);


            TimeSpan span = DateTime.Now - start;
            msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            if (messages > 0)
            {
                log.LogInformation($"Successfully sent {messages} targets to Service Bus queue");
                return 0;
            }
            else
            {
                log.LogError("Error sending messages to Service Bus queue");
                return 2355;
            }
        }

        internal static async Task<int> DeQueueOverrideTargets(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            var start = DateTime.Now;
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!decryptSuccess)
            {
                log.LogWarning("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -53443;
            }


            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("A --servicebustopicconnection value is required. Please include this in either the settings file content or as a specific command option");
                return 9839;
            }
            if (string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchJobName))
            {
                log.LogError("A --jobname value is required. Please include this in either the settings file content or as a specific command option");
                return 9839;
            }

            var qManager = new QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName, cmdLine.ConcurrencyType);
            bool success = await qManager.DeleteSubscription();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);
            if (success)
            {
                log.LogInformation($"Successfully deleted Service Bus queue subscription for '{cmdLine.JobName}'");
                return 0;
            }
            else
            {
                log.LogError($"Error deleting Service Bus queue subscription for '{cmdLine.JobName}'");
                return 2355;
            }
        }

        private static (int, CommandLineArgs) PrepForRemoteQueryExecution(CommandLineArgs cmdLine)
        {

            (bool success, cmdLine) = Init(cmdLine);
            if (!success)
            {
                return (-8675, cmdLine);
            }
            log = Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);
            var outpt = Validation.ValidateQueryArguments(ref cmdLine);
            if (outpt != 0)
            {
                return (outpt, cmdLine);
            }

            //Always run the remote Batch as silent or it will get hung up
            if (cmdLine.Silent == false)
            {
                cmdLine.Silent = true;
            }
            return (0, cmdLine);
        }


        #region ServiceBus and EventHub Monitoring
        private static bool activeServiceBusMonitoring = true;
        internal static async Task<int> MonitorServiceBusRuntimeProgress(CommandLineArgs cmdLine, bool stream, DateTime? utcStartDate, bool unittest = false, bool checkAciState = false)
        {
            Worker.activeServiceBusMonitoring = true;
            var workersConfigured = GetWorkerCount(cmdLine);
            CancellationTokenSource ehCancellationSource = new CancellationTokenSource();
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName) && string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                bool kvSuccess;
                (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
                if (!kvSuccess)
                {
                    log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                    return -4;
                }
            }
            int targets = 0;

            var qManager = new Queue.QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.JobName, cmdLine.ConcurrencyType);

            //set up event handler
            (string jobName, string discard) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
            var ehandler = new Events.EventManager(cmdLine.ConnectionArgs.EventHubConnectionString, cmdLine.EventHubArgs.SubscriptionId, cmdLine.EventHubArgs.ResourceGroup, cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName);

            Task eventHubMonitorTask = null;
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                eventHubMonitorTask = ehandler.MonitorEventHub(stream, utcStartDate, ehCancellationSource.Token);
            }
            else
            {
                log.LogInformation("No Event Hub connection provided. Unable to track live progress other than Queue message count.");
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName) && File.Exists(cmdLine.MultiDbRunConfigFileName))
            {
                var lines = File.ReadAllLines(cmdLine.MultiDbRunConfigFileName);
                targets = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Count();
                if (workersConfigured == 0)
                {
                    System.Console.WriteLine($"Monitoring for the status of {targets} databases");
                }
                else
                {
                    System.Console.WriteLine($"Monitoring for the status of {targets} databases and {workersConfigured} workers");
                }
            }

            long messageCount;

            int zeroMessageCounter = 0;
            int lastCommitCount = -1;
            int lastErrorCount = -1;
            int lastEventCount = -1;
            int lastWorkers = -1;
            int events = 0;
            int workersCompleted = 0;
            int error = 0, commit = 0;
            bool firstLoop = true;
            int cursorStepBack = (targets == 0) ? 3 : 4;
            int unitTestLoops = 0;
            if (checkAciState && !string.IsNullOrWhiteSpace(cmdLine.AciArgs.AciName))
            {
                _ = AciGetErrorState(cmdLine);
            }

            while (true && Worker.activeServiceBusMonitoring)
            {

                if (Worker.aciIsInErrorState)
                {
                    log.LogError("The ACI instance is in an error state, please check the container logs for more detail.");
                    log.LogInformation("This is commonly caused by a delay in the assignment of the Managed Identity to the deployment. Running 'sbm aci deloy' again may solve the issue.");
                    return -1;
                }

                var lines = new List<CursorStatusItem>();
                messageCount = await qManager.MonitorServiceBustopic(cmdLine.ConcurrencyType);
                (commit, error, events, workersCompleted) = ehandler.GetCommitErrorScannedAndWorkerCompleteCounts();
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
                {

                    lines = new List<CursorStatusItem>()
                    {
                        new CursorStatusItem(){Label= "Events Scanned:", Counter = events},
                        new CursorStatusItem(){Label= "Remaining Messages:", Counter = messageCount},
                        new CursorStatusItem(){Label= "Database Commits:", Counter = commit},
                        new CursorStatusItem(){Label= "Database Errors:", Counter = error},
                        new CursorStatusItem(){Label= "Workers Complete:", Counter = workersCompleted}
                    };
                    if (targets > 0)
                    {
                        lines.Insert(2, new CursorStatusItem() { Label = "Remaining Databases:", Counter = (targets - commit - error) });
                    }
                }
                else
                {
                    lines.Add(new CursorStatusItem() { Label = "Remaining Queue Messages:", Counter = messageCount });
                }
                if (unittest) firstLoop = true; //Won't have a console to change position for unit tests
                SetCursorStatus(lines, firstLoop, stream);

                System.Threading.Thread.Sleep(500);
                if (messageCount == 0)
                {
                    zeroMessageCounter++;
                }
                else
                {
                    zeroMessageCounter = 0; unitTestLoops = 0;
                }
                
                if (targets == 0 && zeroMessageCounter >= 20 && lastCommitCount == commit && lastErrorCount == error && lastEventCount == events && (workersConfigured != 0 && lastWorkers == workersCompleted) && !unittest) //not seeing progress
                {
                    System.Console.WriteLine();
                    System.Console.Write("Message count has remained 0, do you want to continue monitoring (Y/n)");
                    var key = System.Console.ReadKey().Key;
                    System.Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        zeroMessageCounter = 0;
                        firstLoop = true;
                        for (int i = 0; i < cursorStepBack; i++)
                        { System.Console.WriteLine(); }

                    }
                    else
                    {
                        if (eventHubMonitorTask != null)
                        {
                            ehCancellationSource.Cancel();
                            eventHubMonitorTask.Wait(500);
                        }

                        break;
                    }
                }
                else if (lastCommitCount != commit || lastErrorCount != error || lastWorkers != workersCompleted) //reset the counters if we still see progress.
                {
                    zeroMessageCounter = 0;
                    unitTestLoops = 0;
                }
                else if (targets != 0 && (commit + error >= targets)) //we know the target count and we have received updates from them all)
                {
                    if (workersConfigured == 0)
                    {
                        System.Console.WriteLine();
                        System.Console.WriteLine($"Received status on {targets} databases. Complete!");
                        break;
                    }
                    else if (workersCompleted >= workersConfigured)
                    {
                        System.Console.WriteLine($"Received status on {targets} databases. Complete!");
                        System.Console.WriteLine($"All {workersCompleted} workers have completed.");
                        break;
                    }
                    else if(workersCompleted > 0 && workersCompleted < workersConfigured && zeroMessageCounter >= 25 && !string.IsNullOrWhiteSpace(cmdLine.ContainerAppArgs.EnvironmentName)) //Special case for Container Apps which may have not needed to go to max scale...
                    {
                        System.Console.WriteLine($"Received status on {targets} databases. Complete!");
                        System.Console.WriteLine($"Completing ContainerApp execution.");
                        break;

                    }
                }
                
                else if (unittest && unitTestLoops == 300)
                {
                    System.Console.WriteLine();
                    log.LogError("Unit test taking too long! There is likely something wrong with the containers.");
                    return -1;
                }
                lastErrorCount = error;
                lastCommitCount = commit;
                lastEventCount = events;
                lastWorkers = workersCompleted;
                firstLoop = false;
                
                unitTestLoops++;
            }

            if (eventHubMonitorTask != null)
            {
                try
                { 
                    ehCancellationSource.Cancel();
                    eventHubMonitorTask.Wait(500);
                    eventHubMonitorTask.Dispose();
                }
                catch { }
            }
            ehandler.Dispose();

            await qManager.DeleteSubscription();


           if (error > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        internal static int GetWorkerCount(CommandLineArgs cmdLine)
        {


            if (cmdLine.AciArgs != null && !string.IsNullOrWhiteSpace(cmdLine.AciArgs.AciName))
            {
                return cmdLine.AciArgs.ContainerCount;
            }
            else if (cmdLine.ContainerAppArgs != null && !string.IsNullOrWhiteSpace(cmdLine.ContainerAppArgs.EnvironmentName))
            {
                return cmdLine.ContainerAppArgs.MaxContainerCount;
            }
            else if(cmdLine.BatchArgs != null && !string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchPoolName))
            {
                //Ignore Batch processing, this monitors tasks on it's own
                return 0;
            }
            else if(cmdLine.KubernetesArgs != null && cmdLine.KubernetesArgs.RunningInKubernetes)
            {
                return cmdLine.KubernetesArgs.PodCount;
            }
            else
            {
                return 0;
            }
           
        }
        internal static int GetEventHubEvents(CommandLineArgs cmdLine, bool stream, int timeout, DateTime? startDate)
        {
            bool junk;
            bool firstLoop = true;
            (junk, cmdLine) = Init(cmdLine);
            Stopwatch timeoutStopWatch = new Stopwatch();
            var offSet = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            if (startDate.HasValue && offSet.TotalSeconds != 0)
            {
                startDate = TimeZone.CurrentTimeZone.ToUniversalTime(startDate.Value);
            }

            (string jobName, string discard) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
            var ehandler = new Events.EventManager(cmdLine.ConnectionArgs.EventHubConnectionString, cmdLine.EventHubArgs.SubscriptionId,cmdLine.EventHubArgs.ResourceGroup, cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName);
            if (!startDate.HasValue)
            {
                startDate = DateTime.UtcNow.AddMinutes(-5);
            }
            var cts = new CancellationTokenSource();
            var ehTask = ehandler.MonitorEventHub(stream, startDate, cts.Token);
            int lastCommit = -1, lastError = -1, counter = 0, lastEvents = -1, lastWorkers = -1;
            int currentCommit, currentError, currentEvents, currentWorkers;

            System.Console.Write("Waiting for EventHub client.");
            while (ehTask.Status == TaskStatus.WaitingForActivation || ehTask.Status == TaskStatus.WaitingToRun)
            {
                Thread.Sleep(2000);
                System.Console.Write(".");
            }
            System.Console.WriteLine();
            System.Console.WriteLine($"Counting Events for job: {jobName}");
            timeoutStopWatch.Start();
            while (true &&  (timeoutStopWatch.Elapsed.Seconds <= timeout || timeout == 0))
            {

                (currentCommit, currentError, currentEvents, currentWorkers) = ehandler.GetCommitErrorScannedAndWorkerCompleteCounts();
                if (currentCommit == lastCommit && currentError == lastError && currentEvents == lastEvents)
                {
                    counter++;
                }
                else
                {
                    counter = 0;
                    lastError = currentError;
                    lastCommit = currentCommit;
                    lastEvents = currentEvents;
                    lastWorkers = currentWorkers;
                    timeoutStopWatch.Restart();
                }

                var lines = new List<CursorStatusItem>()
                {
                        new CursorStatusItem(){Label= "Events Scanned:", Counter = currentEvents},
                };
                if(cmdLine.JobName.ToLower() != "all")
                {
                    lines.AddRange(new List<CursorStatusItem>() {
                        new CursorStatusItem() { Label = "Database Commits:", Counter = currentCommit },
                        new CursorStatusItem() { Label = "Database Errors:", Counter = currentError },
                        new CursorStatusItem() { Label = "Workers Completed:", Counter = currentWorkers }
                        }
                    );
                }
                SetCursorStatus(lines, firstLoop, stream);
                Thread.Sleep(2000);
                firstLoop = false;
            }
            System.Console.WriteLine();
            ehandler.RemoveCustomConsumerGroup();
            log.LogInformation($"Scanning complete!");

            return 0;

        }
        internal static void GetQueueMessageCount(CommandLineArgs cmdLine)
        {

        }
        
        private static int cursorCounter = 0;

        public class CursorStatusItem
        {
            public string Label { get; set; } = "";
            public long Counter { get; set; } = 0;
        }
        private static void SetCursorStatus(List<CursorStatusItem> items, bool first, bool stream)
        {
            string spinner = "| ";
            string spacer = "  ";
            cursorCounter++;
            switch (cursorCounter % 4)
            {
                case 0: spinner = "/ "; cursorCounter = 0; break;
                case 1: spinner = "- "; break;
                case 2: spinner = "\\ "; break;
                case 3: spinner = "| "; break;
            }
            if (!first && !stream)
            {
                System.Console.SetCursorPosition(0, System.Console.CursorTop - (items.Count() - 1));
            }
            int maxLabel = items.Select(l => l.Label.Length).Max() + 2;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                if (!stream)
                {
                    sb.AppendLine($"{spinner}{items[i].Label.PadRight(maxLabel, ' ')}{spacer}{items[i].Counter.ToString().PadLeft(5, '0')}");
                    spinner = "  ";
                }
                else
                {
                    sb.Append($"{items[i].Label.PadRight(maxLabel, ' ')}{spacer}{items[i].Counter.ToString().PadLeft(5, '0')} | ");
                }
            }
            if (!stream)
            {
                System.Console.Write(sb.ToString().Trim());
            }
            else
            {
                System.Console.WriteLine(sb.ToString().Substring(0, sb.Length - 2));
            }
        }
        #endregion 
        
        private static void ConsolidateRuntimeLogFiles(CommandLineArgs cmdLine)
        {
            //Batch jobs have their own consolidation....
            if (cmdLine.BatchArgs == null || string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchPoolName))
            {
                log.LogInformation("Consolidating log files");
                StorageManager.ConsolidateLogFiles(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, new List<string>());
                log.LogInformation($"The consolidated log files can be found in the Azure storage account '{cmdLine.ConnectionArgs.StorageAccountName}' in blob container '{cmdLine.JobName}'");
                log.LogInformation("You can download \"Azure Storage Explorer\" from here: https://azure.microsoft.com/en-us/features/storage-explorer/");
            }
        }



       
       
        internal class StartArgs
        {
            public string[] Args { get; set; }
            public StartArgs(string[] args)
            {
                Args = args;
            }
        }
    }
}
