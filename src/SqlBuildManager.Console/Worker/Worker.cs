using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Spectre.Console;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.Batch;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Console.Kubernetes;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Shared;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;
using SqlBuildManager.Logging.Threaded;
using SqlSync.Connection;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
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

        private static (bool, CommandLineArgs) Init(CommandLineArgs cmdLine)
        {
            if (cmdLine.IdentityArgs != null) AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
            if (cmdLine.IdentityArgs != null) AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
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
        
        #region Container Runtime Worker Methods

        #region Database builds
        private static async Task<int> RunGenericContainerQueueWorker(CommandLineArgs cmdLine)
        {
            int result = 1;
            try
            {
                //If the provided build file name is the full path, just get the file name to find it in Blob storage
                cmdLine.BuildFileName = Path.GetFileName(cmdLine.BuildFileName);

                (string jobName, string throwaway) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
                cmdLine.BatchJobName = jobName;

                bool keepGoing = true;
                cmdLine.BuildFileName = await CloudStorage.StorageManager.WriteFileToLocalStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.BuildFileName);
                if (string.IsNullOrEmpty(cmdLine.BuildFileName))
                {
                    log.LogError("Unable to copy build package to local storage. Can not start execution");
                    keepGoing = false;
                }

                if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    cmdLine.PlatinumDacpac = await CloudStorage.StorageManager.WriteFileToLocalStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.DacPacArgs.PlatinumDacpac);
                    if (string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
                    {
                        log.LogError("Unable to copy platinum dacpac package to local storage. Can not start execution");
                        keepGoing = false;
                    }
                }

                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
                {
                    cmdLine.BatchArgs.OutputContainerSasUrl = CloudStorage.StorageManager.GetContainerRawUrl(cmdLine.ConnectionArgs.StorageAccountName, jobName);
                }
                else
                {
                    cmdLine.BatchArgs.OutputContainerSasUrl = CloudStorage.StorageManager.GetOutputContainerSasUrl(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, false);
                }


                if (keepGoing)
                {
                    result = RunThreadedExecution(cmdLine);
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
                log.LogWarning("Error starting and running container processing");
                result = 2;
            }
            return result;
        }
        internal static async Task<int> RunKubernetesQueueWorker(CommandLineArgs cmdLine)
        {
            (bool success, cmdLine) = PrepKubernetesCommandLine(cmdLine);
            if(!success)
            {
                return -1;
            }

            return await RunGenericContainerQueueWorker(cmdLine);
        }
        internal static async Task<int> RunContainerAppWorker(CommandLineArgs cmdLine)
        {
            bool initSuccess;
            (initSuccess, cmdLine) = Init(cmdLine);
            cmdLine.RunningAsContainer = true;
            cmdLine = Shared.EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(cmdLine);
            (bool discard, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            cmdLine = Shared.EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(cmdLine);
            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
                AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            }
            cmdLine.ContainerAppArgs.RunningAsContainerApp = true;

            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            cmdLine.RootLoggingPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(cmdLine.RootLoggingPath))
            {
                Directory.CreateDirectory(cmdLine.RootLoggingPath);
            }
            //Set this so that the threaded service bus loop doesn't terminate


            return await RunGenericContainerQueueWorker(cmdLine);
        }
        internal static async Task<int> RunAciQueueWorker(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            cmdLine.RootLoggingPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(cmdLine.RootLoggingPath))
            {
                Directory.CreateDirectory(cmdLine.RootLoggingPath);
            }
            cmdLine.RunningAsContainer = true;



            int seconds = 5;
            log.LogInformation($"Waiting {seconds} for Managed Identity assignment");
            System.Threading.Thread.Sleep(seconds * 1000);

            cmdLine = Shared.EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(cmdLine);

            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogError("Unable to retrieve required connection secrets. Terminating container");
                return -2;
            }
            //Re-read (mostly for unit tests in case there is a connection string from the KV
            cmdLine = Shared.EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(cmdLine);
            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
                AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            }
            return await RunGenericContainerQueueWorker(cmdLine);
        }

        private static (bool, CommandLineArgs) PrepKubernetesCommandLine(CommandLineArgs cmdLine)
        {
            (var x, cmdLine) = Init(cmdLine);
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            cmdLine.RootLoggingPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(cmdLine.RootLoggingPath))
            {
                Directory.CreateDirectory(cmdLine.RootLoggingPath);
            }
            cmdLine.RunningAsContainer = true;
            bool success = true;

            //Configmap params
            (success, cmdLine) = KubernetesManager.ReadConfigmapParameters(cmdLine);
            if (!success)
            {
                log.LogError("Unable to acquire runtime values. Terminating container.");
                return (false, cmdLine);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                (bool discard, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            }
            else
            {
                (success, cmdLine) = KubernetesManager.ReadOpaqueSecrets(cmdLine);
            }
            return (success, cmdLine);
        }
        #endregion

        #region Database query
        internal static async Task<int> RunKubernetesQueueQueryWorker(CommandLineArgs cmdLine)
        {
            (bool success, cmdLine) = PrepKubernetesCommandLine(cmdLine);
            if (!success)
            {
                return -1;
            }
            return await RunGenericContainerQueueQueryWorker(cmdLine);
        }

        private static async Task<int> RunGenericContainerQueueQueryWorker(CommandLineArgs cmdLine)
        {
            int result = 1;
            try
            {

                (string jobName, string throwaway) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
                cmdLine.BatchJobName = jobName;
                log.LogInformation($"Starting query job: '{jobName}'");
                log.LogInformation(cmdLine.ToString());
                log.LogInformation($"Using Query file '{cmdLine.QueryFile?.Name}'");

                bool keepGoing = true;
                cmdLine.QueryFile = new FileInfo(await CloudStorage.StorageManager.WriteFileToLocalStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.QueryFile.Name));
                if (cmdLine.QueryFile == null)
                {
                    log.LogError("Unable to save query file to local storage. Can not start execution");
                    keepGoing = false;
                }

                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
                {
                    cmdLine.BatchArgs.OutputContainerSasUrl = CloudStorage.StorageManager.GetContainerRawUrl(cmdLine.ConnectionArgs.StorageAccountName, jobName);
                }
                else
                {
                    cmdLine.BatchArgs.OutputContainerSasUrl = CloudStorage.StorageManager.GetOutputContainerSasUrl(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, false);
                }


                if (keepGoing)
                {
                    var runId = Guid.NewGuid().ToString().Replace("-", "");
                    result = new ThreadedQuery().QueryDatabases(cmdLine, runId);

                    //Copy file to storage with unique prefix
                    StorageManager.CopyFileToStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.OutputFile.FullName, runId + "_" + cmdLine.OutputFile.Name);
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
                log.LogWarning("Error starting and running container processing");
                result = 2;
            }
            return result;
        }
        #endregion

        #endregion


        internal async static Task<int> EnqueueOverrideTargets(CommandLineArgs cmdLine)
        {
            Init(cmdLine);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(applicationLogFileName);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(applicationLogFileName, cmdLine.RootLoggingPath);
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            var start = DateTime.Now;
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
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
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
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

       
        internal static async Task<int> EnqueueContainerOverrideTargets(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, string keyvaultname, string jobname, ConcurrencyType concurrencytype, string servicebustopicconnection, FileInfo Override)
        {
            bool valid;
            if (cmdLine == null)
            {
                cmdLine = new CommandLineArgs();
            }
            (var x, cmdLine) = Init(cmdLine);
            if (secretsFile != null && runtimeFile != null)
            {
                cmdLine = KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.Name);
                (valid, cmdLine) = Validation.ValidateContainerQueueArgs(cmdLine, keyvaultname, jobname, concurrencytype, servicebustopicconnection);
                if (!valid)
                {
                    return 1;
                }
            }
            //TODO: validate jobname format

            int tmpValReturn = Validation.ValidateAndLoadMultiDbData(Override.FullName, null, out MultiDbData multiData, out string[] errorMessages);
            if (tmpValReturn != 0)
            {
                log.LogError(String.Join(";", errorMessages));
                return tmpValReturn;
            }
            log.LogInformation("Sending database targets to Service Bus");
            var qManager = new QueueManager(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, cmdLine.JobName, cmdLine.ConcurrencyType);
            int messages = await qManager.SendTargetsToQueue(multiData, cmdLine.ConcurrencyType);
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

        #region ServiceBus and EventHub Monitoring
        private static bool activeServiceBusMonitoring = true;
        internal static async Task<int> MonitorServiceBusRuntimeProgress(CommandLineArgs cmdLine, bool stream, DateTime? utcStartDate, bool unittest = false, bool checkAciState = false)
        {
            Worker.activeServiceBusMonitoring = true;
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
            var ehandler = new Events.EventManager(cmdLine.ConnectionArgs.EventHubConnectionString, cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, jobName);

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
                System.Console.WriteLine($"Monitoring for the status of {targets} databases");
            }

            long messageCount;

            int zeroMessageCounter = 0;
            int lastCommitCount = -1;
            int lastErrorCount = -1;
            int lastEventCount = -1;
            int events = 0;
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
                (commit, error, events) = ehandler.GetCommitErrorAndScannedCounts();
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
                {

                    lines = new List<CursorStatusItem>()
                    {
                        new CursorStatusItem(){Label= "Events Scanned:", Counter = events},
                        new CursorStatusItem(){Label= "Remaining Messages:", Counter = messageCount},
                        new CursorStatusItem(){Label= "Database Commits:", Counter = commit},
                        new CursorStatusItem(){Label= "Database Errors:", Counter = error}
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
                if (messageCount == 0) { zeroMessageCounter++; } else { zeroMessageCounter = 0; unitTestLoops = 0; }
                if (targets == 0 && zeroMessageCounter >= 20 && lastCommitCount == commit && lastErrorCount == error && lastEventCount == events && !unittest) //not seeing progress
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
                else if (targets != 0 && (commit + error == targets)) //we know the target count and we have received updates from them all
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine($"Received status on {targets} databases. Complete!");
                    break;
                }
                else if (lastCommitCount != commit || lastErrorCount != error) //reset the counters if we still see progress.
                {
                    zeroMessageCounter = 0;
                    unitTestLoops = 0;
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
        internal static int GetEventHubEvents(CommandLineArgs cmdLine, DateTime? startDate)
        {
            bool junk;
            bool firstLoop = true;
            (junk, cmdLine) = Init(cmdLine);

            (string jobName, string discard) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
            var ehandler = new Events.EventManager(cmdLine.ConnectionArgs.EventHubConnectionString, cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, jobName);
            if (!startDate.HasValue)
            {
                startDate = DateTime.UtcNow.AddDays(-14);
            }
            var cts = new CancellationTokenSource();
            var ehTask = ehandler.MonitorEventHub(false, startDate, cts.Token);
            int lastCommit = -1, lastError = -1, counter = 0, lastEvents = -1;
            int currentCommit, currentError, currentEvents;

            System.Console.Write("Waiting for EventHub client.");
            while (ehTask.Status == TaskStatus.WaitingForActivation || ehTask.Status == TaskStatus.WaitingToRun)
            {
                Thread.Sleep(2000);
                System.Console.Write(".");
            }
            System.Console.WriteLine();
            System.Console.WriteLine($"Counting Events for job: {jobName}");

            while (true)
            {

                (currentCommit, currentError, currentEvents) = ehandler.GetCommitErrorAndScannedCounts();
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
                }
                if (counter == 10)
                {
                    break;
                }

                var lines = new List<CursorStatusItem>()
                {
                        new CursorStatusItem(){Label= "Events Scanned:", Counter = currentEvents},
                        new CursorStatusItem(){Label= "Database Commits:", Counter = currentCommit},
                        new CursorStatusItem(){Label= "Database Errors:", Counter = currentError}
                };
                SetCursorStatus(lines, firstLoop, false);
                Thread.Sleep(1000);
                firstLoop = false;
            }
            System.Console.WriteLine();
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

        internal static async Task<int> PrepAndUploadContainerBuildPackage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            var success = false;
            (success, cmdLine) = Init(cmdLine);
            if (packageName != null)
            {
                cmdLine.BuildFileName = packageName.FullName;
            }
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                log.LogError("--storageaccountname is required");
                return -1;
            }
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            {
                log.LogError("--storageaccountkey is required if a Managed Identity is not included");
                return -1;
            }

            (bool retVal, string sbmName) = await ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
            if (retVal)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        private static async Task<(bool, string)> ValidateAndUploadContainerBuildFilesToStorage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            if (packageName == null && platinumDacpac == null)
            {
                log.LogError("Either a --packagename or --platinumdacpac argument is required");
                return (false, "");
            }

            bool sbmGenerated = false;
            //Need to build the SBM package 
            if (platinumDacpac != null && packageName == null)
            {
                if (string.IsNullOrEmpty(cmdLine.MultiDbRunConfigFileName))
                {
                    log.LogError("When a --platinumdacpac argument is specified without a --packagename, then an --override value is required so that the SBM package can be generated");
                    return (false, "");
                }

                var multiData = MultiDbHelper.ImportMultiDbTextConfig(cmdLine.MultiDbRunConfigFileName);
                if (multiData == null)
                {
                    log.LogError($"Unable to derive database targets from specified --override setting of '{cmdLine.MultiDbRunConfigFileName}' . Please check that the file exists and is properly formatted.");
                    return (false, "");
                }
                string sbmName;
                cmdLine.PlatinumDacpac = platinumDacpac.FullName;
                var stat = Worker.GetSbmFromDacPac(cmdLine, multiData, out sbmName, true);
                if (stat == sb.DacpacDeltasStatus.Success)
                {
                    if (Path.GetFileNameWithoutExtension(sbmName) != Path.GetFileNameWithoutExtension(platinumDacpac.FullName))
                    {
                        var newSbmName = Path.Combine(Path.GetDirectoryName(platinumDacpac.FullName), Path.GetFileNameWithoutExtension(platinumDacpac.FullName) + ".sbm");
                        File.Copy(sbmName, newSbmName, true);
                        packageName = new FileInfo(newSbmName);
                    }
                    else
                    {
                        packageName = new FileInfo(sbmName);
                    }
                    sbmGenerated = true;
                }
                else
                {
                    log.LogError("Unable to create an SBM package from the specified --platinumdacpac and --override settings. Please check their values.");
                    return (false, "");
                }
            }

            if (StorageManager.StorageContainerHasExistingFiles(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
            {
                if (!force)
                {
                    System.Console.Write($"The container {cmdLine.JobName} already exists in storage account {cmdLine.ConnectionArgs.StorageAccountName}. Do you want to delete any existing files and continue upload? (Y/n)");
                    var key = System.Console.ReadKey().Key;
                    System.Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        force = true;
                    }
                    else
                    {
                        System.Console.WriteLine("Exiting. The package file was not uploaded and no files were deleted from storage");
                        return (true, "");
                    }
                }
                if (force)
                {
                    if (!await StorageManager.DeleteStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
                    {
                        log.LogError("Unable to delete container. The package file was not uploaded");
                        return (false, "");
                    }
                }
            }
            List<string> filePaths = new List<string>();
            if (packageName != null) filePaths.Add(packageName.FullName);
            if (platinumDacpac != null) filePaths.Add(platinumDacpac.FullName);

            if (!await StorageManager.UploadFilesToStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, filePaths.ToArray()))
            {
                log.LogError("Unable to upload files to storage");
                return (false, "");
            }

            if (sbmGenerated)
            {
                log.LogInformation($"An SBM Package file was generated and uploaded with the DACPAC. When running the `deploy` command, please use this argument: --packagename \"{packageName.FullName}\"");
                return (true, packageName.FullName);
            }

            return (true, "");
        }
        private static async Task<(bool, string)> ValidateAndUploadContainerQueryToStorage(CommandLineArgs cmdLine, bool force)
        {
            if (cmdLine.QueryFile == null)
            {
                log.LogError("The  --queryfileargument is required");
                return (false, "");
            }
           

            if (StorageManager.StorageContainerHasExistingFiles(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
            {
                if (!force)
                {
                    System.Console.Write($"The container {cmdLine.JobName} already exists in storage account {cmdLine.ConnectionArgs.StorageAccountName}. Do you want to delete any existing files and continue upload? (Y/n)");
                    var key = System.Console.ReadKey().Key;
                    System.Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        force = true;
                    }
                    else
                    {
                        System.Console.WriteLine("Exiting. The package file was not uploaded and no files were deleted from storage");
                        return (true, "");
                    }
                }
                if (force)
                {
                    if (!await StorageManager.DeleteStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
                    {
                        log.LogError("Unable to delete container. The package file was not uploaded");
                        return (false, "");
                    }
                }
            }
            List<string> filePaths = new List<string>();
            filePaths.Add(cmdLine.QueryFile.FullName);

            if (!await StorageManager.UploadFilesToStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, filePaths.ToArray()))
            {
                log.LogError("Unable to upload files to storage");
                return (false, "");
            }

            return (true, "");
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
