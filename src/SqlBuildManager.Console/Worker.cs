using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using clb = SqlBuildManager.Console.CommandLine.CommandLineBuilder;
using sb = SqlSync.SqlBuild;

namespace SqlBuildManager.Console
{
    internal class Worker : IHostedService
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
            this.applicationLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            var fn = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var currentPath = Path.GetDirectoryName(fn);

            log.LogDebug("Received Command: " + String.Join(" | ", Worker.startArgs.Args));

            this.applicationLifetime.ApplicationStarted.Register(() =>
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
            var multiData = MultiDbHelper.ImportMultiDbTextConfig(cmdLine.MultiDbRunConfigFileName);
            var connData = new ConnectionData() { UserId = cmdLine.AuthenticationArgs.UserName, Password = cmdLine.AuthenticationArgs.Password, AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType };


            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(ThreadedQuery_ProgressChanged);
            var collector = new QueryCollector(multiData, connData);

            var serverCount = multiData.Count();
            var dbCount = multiData.Sum(d => d.OverrideSequence.Count);

            log.LogInformation($"Running query across {serverCount} servers and {dbCount} databases...");
            bool success = collector.GetQueryResults(ref bg, cmdLine.OutputFile.FullName, SqlSync.SqlBuild.Status.ReportType.CSV, query, cmdLine.DefaultScriptTimeout);

            if (!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.LogInformation("Writing log files to storage...");
                var blobTask = StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }


            if (success)
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
        internal static void ThreadedQuery_ProgressChanged(object sender, ProgressChangedEventArgs e)
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

        internal static int RunBatchCleanUp(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.CleanUpBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal static int RunBatchJobDelete(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            var retVal = Batch.BatchCleaner.DeleteAllCompletedJobs(cmdLine);

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal async static Task<int> RunBatchPreStage(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = await batchExe.PreStageBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal static int RunBatchExecution(CommandLineArgs cmdLine, bool monitor = false, bool unittest = false, bool stream = false)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);


            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);

            log.LogDebug("Entering Batch Execution");
            log.LogInformation("Running Batch Execution...");
            int retVal;
            string readOnlySas;
            Task monitorTask = null;
         
           
            //Register the monitoring events if designated
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) && monitor)
            {
                batchExe.BatchProcessStartedEvent += new Batch.BatchMonitorEventHandler(Batch_MonitorStart);
                batchExe.BatchExecutionCompletedEvent += new BatchMonitorEventHandler(Batch_MonitorEnd);
            }
            else
            {
                stream = false;
            }

            Task<(int, string)> batchExeTask = batchExe.StartBatch(stream,unittest);
            batchExeTask.Wait();
            (retVal, readOnlySas) = batchExeTask.Result;
            
            if (monitorTask != null)
            {
                monitorTask.Wait(500);
            }

            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.LogInformation("Completed Successfully");
            }
            else if (retVal == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                log.LogInformation("Datbases already in sync");
                retVal = (int)ExecutionReturn.Successful;
            }
            else
            {
                log.LogWarning("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            log.LogDebug("Exiting Batch Execution");

            return retVal;
        }

        private static Task<int> batchMonitorTask = null;
        private static void Batch_MonitorStart(object sender, BatchMonitorEventArgs e)
        {
            batchMonitorTask  = MonitorServiceBusRuntimeProgress(e.CmdLine, e.Stream,DateTime.UtcNow,  e.UnitTest);
        }
        private static void Batch_MonitorEnd(object sender, EventArgs e)
        {
            Worker.activeServiceBusMonitoring = false;
        }

        internal static async Task<int> RunBatchQuery(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                return -4353;
            }
            var outpt = Validation.ValidateQueryArguments(ref cmdLine);
            if (outpt != 0)
            {
                return outpt;
            }

            //Always run the remote Batch as silent or it will get hung up
            if (cmdLine.Silent == false)
            {
                cmdLine.Silent = true;
            }
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine, cmdLine.QueryFile.FullName, Path.Combine(cmdLine.RootLoggingPath, cmdLine.OutputFile.Name));

            log.LogDebug("Entering Batch Query Execution");
            log.LogInformation("Running Batch Query Execution...");
            int retVal;
            string readOnlySas;

            (retVal, readOnlySas) = batchExe.StartBatch().GetAwaiter().GetResult();

            if (!string.IsNullOrWhiteSpace(readOnlySas))
            {
                log.LogInformation("Downloading the consolidated output file...");
                try
                {
                    if(await StorageManager.DownloadBlobToLocal(readOnlySas, cmdLine.OutputFile.FullName))
                    {
                        log.LogInformation($"Output file copied locally to {cmdLine.OutputFile.FullName}");
                    }
                }
                catch (Exception exe)
                {
                    log.LogError($"Unable to download the output file:  {exe.Message}");
                }
            }

            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.LogInformation("Completed Successfully");
            }
            else
            {
                log.LogWarning("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            log.LogDebug("Exiting Batch Execution");

            return retVal;
        }

        internal static int SaveAndEncryptSettings(CommandLineArgs cmdLine, bool clearText)
        {

            if (string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                log.LogError("When 'sbm batch/aci/containerapp/k8s savesettings' is specified the --settingsfile argument is also required");
                return -3;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.SettingsFileKey) && cmdLine.SettingsFileKey.Length < 16)
            {
                log.LogError("The value for the --settingsfilekey must be at least 16 characters long");
                return -4;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                var lst = KeyVaultHelper.SaveSecrets(cmdLine);
                log.LogInformation($"Saved secrets to Azure Key Vault {cmdLine.ConnectionArgs.KeyVaultName}: {string.Join(", ", lst)}");
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                //remove secrets from the command line so they are not saved to the config.
                cmdLine.AuthenticationArgs.UserName = null;
                cmdLine.AuthenticationArgs.Password = null;
                cmdLine.ConnectionArgs.BatchAccountKey = null;
                cmdLine.ConnectionArgs.StorageAccountKey = null;
                cmdLine.ContainerRegistryArgs.RegistryPassword = null;

                if (ConnectionValidator.IsEventHubConnectionString(cmdLine.ConnectionArgs.EventHubConnectionString))
                {
                    cmdLine.ConnectionArgs.EventHubConnectionString = null;
                }

                if (ConnectionValidator.IsServiceBusConnectionString(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    if (cmdLine.ContainerAppArgs == null || string.IsNullOrWhiteSpace(cmdLine.ContainerAppArgs.EnvironmentName))
                    {
                        //will need this for KEDA in ContainerApps, so only remove if NOT for ContainerApps
                        cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = null;

                    }
                }
            }


            //Clear out username and password is AuthenticationType is ManagedIdentity
            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                cmdLine.AuthenticationArgs.UserName = null;
                cmdLine.AuthenticationArgs.Password = null;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                //Clean out ACI, Container App and Container Registry for Batch runs
                cmdLine.AciArgs = null;
                cmdLine.ContainerAppArgs = null;
                cmdLine.ContainerRegistryArgs = null;
            }

            if (!clearText)
            {
                cmdLine = Cryptography.EncryptSensitiveFields(cmdLine);
            }

            var mystuff = JsonConvert.SerializeObject(cmdLine, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            try
            {
                string write = "y";
                if (File.Exists(cmdLine.SettingsFile) && !cmdLine.Silent)
                {
                    System.Console.WriteLine($"The settings file '{cmdLine.SettingsFile}' already exists. Overwrite (Y/N)?");
                    write = System.Console.ReadLine();
                }

                if (write.ToLower() == "y")
                {
                    File.WriteAllText(cmdLine.SettingsFile, mystuff);
                    log.LogInformation($"Settings file saved to '{cmdLine.SettingsFile}'");
                }
                else
                {
                    log.LogInformation("Settings file not saved");
                    return -2;
                }
                return 0;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to save settings file.\r\n{exe.ToString()}");
                return -1;
            }

        }

        internal static int CreateFromDacpacDiff(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            #region Validate flags
            string[] errorMessages;
            int pwVal = Validation.ValidateUserNameAndPassword(cmdLine, out errorMessages);
            if (pwVal != 0)
            {
                log.LogError(errorMessages.Aggregate((a, b) => a + "; " + b));
            }

            if (File.Exists(cmdLine.OutputSbm))
            {
                log.LogError($"The --outputsbm file already exists at {cmdLine.OutputSbm}. Please choose another name or delete the existing file.");
                System.Environment.Exit(-1);
            }
            #endregion

            string name;
            cmdLine.RootLoggingPath = Path.GetDirectoryName(cmdLine.OutputSbm);

            var status = Worker.GetSbmFromDacPac(cmdLine, new SqlSync.SqlBuild.MultiDb.MultiDbData(), out name, true);
            if (status == sb.DacpacDeltasStatus.Success)
            {
                File.Move(name, cmdLine.OutputSbm);
                ListPackageScripts(new FileInfo[] { new FileInfo(cmdLine.OutputSbm) }, true);
                log.LogInformation($"SBM package successfully created at {cmdLine.OutputSbm}");
            }
            else
            {
                log.LogError($"Error creating SBM package: {status.ToString()}");
            }

            return 0;
        }

        internal static int RunLocalBuildAsync(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);


            bool decryptSuccess;
            string workingDir = "";
            DateTime start = DateTime.Now;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }

            try
            {
                cmdLine.BuildFileName = Path.GetFullPath(cmdLine.BuildFileName);
                log.LogDebug("Entering Local Build Execution");
                log.LogInformation($"Running Local Build with: {cmdLine.BuildFileName}");


                //We need an override setting. if not provided, we need to glean it from the SqlSyncBuildProject.xml file 
                if (string.IsNullOrWhiteSpace(cmdLine.ManualOverRideSets) && !string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
                {
                    cmdLine.ManualOverRideSets = sb.SqlBuildFileHelper.InferOverridesFromPackage(cmdLine.BuildFileName, cmdLine.Database);
                }

                var ovrRide = $"{cmdLine.Server}:{cmdLine.ManualOverRideSets}";
                var def = ovrRide.Split(':')[1].Split(',')[0];
                var target = ovrRide.Split(':')[1].Split(',')[1];
                if (string.IsNullOrEmpty(cmdLine.RootLoggingPath))
                {
                    cmdLine.RootLoggingPath = Directory.GetCurrentDirectory();
                }

                string projFilePath = "", projectFileName = "";
                sb.SqlBuildFileHelper.ExtractSqlBuildZipFile(cmdLine.BuildFileName, ref workingDir, ref projFilePath, ref projectFileName, true, true, out string result);
                bool success = sb.SqlBuildFileHelper.LoadSqlBuildProjectFile(out sb.SqlSyncBuildData buildData, projectFileName, true);
                if (!success)
                {
                    log.LogError($"Unable to load and extract build file: {cmdLine.BuildFileName}");
                    return -1;
                }

                sb.SqlBuildRunData sqlBuildRunData = new sb.SqlBuildRunData()
                {
                    ForceCustomDacpac = false,
                    BuildData = buildData,
                    IsTransactional = cmdLine.Transactional,
                    BuildDescription = cmdLine.Description,
                    BuildRevision = cmdLine.BuildRevision,
                    LogToDatabaseName = cmdLine.LogToDatabaseName,
                    TargetDatabaseOverrides = new List<DatabaseOverride>() { new DatabaseOverride() { DefaultDbTarget = def, OverrideDbTarget = target } },
                    ProjectFileName = projectFileName,
                    BuildFileName = cmdLine.BuildFileName,
                    AllowObjectDelete = cmdLine.AllowObjectDelete

                };
                ConnectionData connData = new ConnectionData()
                {
                    SQLServerName = cmdLine.Server,
                    DatabaseName = cmdLine.Database,
                    AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType,
                    UserId = cmdLine.AuthenticationArgs.UserName,
                    Password = cmdLine.AuthenticationArgs.Password
                };
                sb.SqlBuildHelper helper = new sb.SqlBuildHelper(connData, true, "", cmdLine.Transactional);
                BackgroundWorker bg = new BackgroundWorker()
                {
                    WorkerReportsProgress = true,
                };
                bg.ProgressChanged += Bg_ProgressChanged;
                DoWorkEventArgs workArgs = new DoWorkEventArgs(sqlBuildRunData);
                LocalRunInfo.Sq1SyncBuildData = buildData;
                LocalRunInfo.BuildZipFileName = cmdLine.BuildFileName;
                LocalRunInfo.WorkingDirectory = workingDir;
                helper.ProcessBuild(sqlBuildRunData, cmdLine.TimeoutRetryCount, bg, workArgs);
            }
            finally
            {
                sb.SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDir);
            }
            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);
            log.LogDebug("Exiting Single Build Execution");

            if (LocalRunInfo.Success)
                return 0;
            else
                return -2;
        }
        private static class LocalRunInfo
        {
            public static sb.SqlSyncBuildData Sq1SyncBuildData { get; set; }
            public static string WorkingDirectory { get; set; }
            public static string BuildZipFileName { get; set; }
            public static bool Success { get; set; } = true;
        }

        private static void Bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (e.UserState is sb.GeneralStatusEventArgs) //Update the general run status
            {
                var stat = (sb.GeneralStatusEventArgs)e.UserState;
                log.LogInformation(stat.StatusMessage);
                if (stat.StatusMessage.ToLower().Contains("build failure") || stat.StatusMessage.ToLower().Contains("build failed"))
                {
                    LocalRunInfo.Success = false;
                }
            }

            else if (e.UserState is sb.CommitFailureEventArgs)
            {
                log.LogError("Failed to Commit Build " + ((sb.CommitFailureEventArgs)e.UserState).ErrorMessage);
                LocalRunInfo.Success = false;
            }
            else if (e.UserState is sb.ScriptRunStatusEventArgs)
            {
                log.LogInformation(((sb.ScriptRunStatusEventArgs)e.UserState).Status);
            }
            else if (e.UserState is sb.ScriptRunProjectFileSavedEventArgs)
            {
                log.LogInformation("Saving updated build file to disk");
                try
                {
                    sb.SqlBuildFileHelper.PackageProjectFileIntoZip(LocalRunInfo.Sq1SyncBuildData, LocalRunInfo.WorkingDirectory, LocalRunInfo.BuildZipFileName);
                    log.LogInformation("Build file saved to disk");
                }
                catch (Exception exe)
                {
                    log.LogError(exe.ToString());
                }
            }
            else if (e.UserState is Exception)
            {
                log.LogError("ERROR!" + ((Exception)e.UserState).Message);
            }
        }

        internal static int RunThreadedExecution(CommandLineArgs cmdLine, bool unittest = false)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            if (string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = Directory.GetCurrentDirectory();
            }
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                return -4353;
            }

            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
                AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            }
            DateTime start = DateTime.Now;
            log.LogDebug("Entering Threaded Execution");
            log.LogDebug(cmdLine.ToStringExtension(StringType.Basic));
            log.LogDebug(cmdLine.ToStringExtension(StringType.Batch));
            log.LogInformation("Running Threaded Execution...");
            ThreadedExecution runner = new ThreadedExecution(cmdLine);
            int retVal = runner.Execute();
            ExecutionReturn exeResult;
            if (Enum.TryParse<ExecutionReturn>(retVal.ToString(), out exeResult))
            {
                switch(exeResult)
                {
                    case ExecutionReturn.Successful:
                        log.LogInformation("Completed Successfully");
                        break;
                    case ExecutionReturn.DacpacDatabasesInSync:
                        log.LogInformation("Datbases already in sync");
                        retVal = (int)ExecutionReturn.Successful;
                        break;
                    default:
                        log.LogWarning($"Completed with Errors - check log [{exeResult.ToString()}]");
                        break;
                }

            }
            else
            {
                log.LogWarning($"Completed with Errors - check log. Return code [{retVal.ToString()}]");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            if (!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.LogInformation("Writing log files to storage...");
                var blobTask = StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }

            log.LogDebug("Exiting Threaded Execution");
            return retVal;

        }

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

                if(string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
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
                return -2;
            }

            if(!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                (bool discard, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            }
            else
            {
                (success, cmdLine) = KubernetesManager.ReadOpaqueSecrets(cmdLine);
            }
      
            return await RunGenericContainerQueueWorker(cmdLine);
        }

        private static bool aciIsInErrorState = false;
        private static async Task GetAciErrorState(CommandLineArgs cmdLine)
        {
            while (true)
            {
                try
                {
                    var stat = await Aci.AciHelper.AciIsInErrorState(cmdLine.IdentityArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup, cmdLine.AciArgs.AciName);
                    aciIsInErrorState = stat;
                }
                catch { }
                System.Threading.Thread.Sleep(15000);
            }
        }
        private static bool activeServiceBusMonitoring = true;
        internal static async Task<int> MonitorServiceBusRuntimeProgress(CommandLineArgs cmdLine, bool stream, DateTime? utcStartDate, bool unittest = false, bool checkAciState = false)
        {
            Worker.activeServiceBusMonitoring = true;
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
            var ehandler = new Events.EventManager(cmdLine.ConnectionArgs.EventHubConnectionString,cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey,jobName, jobName);

            Task eventHubMonitorTask = null;
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                CancellationTokenSource cancelSource = new CancellationTokenSource();
                eventHubMonitorTask = ehandler.MonitorEventHub(stream, utcStartDate, cancelSource.Token);
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
                _ = GetAciErrorState(cmdLine);
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
                        if(eventHubMonitorTask != null) 
                            eventHubMonitorTask.Wait(500);

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
                eventHubMonitorTask.Dispose();
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
                System.Console.SetCursorPosition(0, System.Console.CursorTop - (items.Count()-1));
            }
            int maxLabel = items.Select(l => l.Label.Length).Max() +2;
            StringBuilder sb = new StringBuilder();
            for(int i=0;i<items.Count;i++)
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
                System.Console.WriteLine(sb.ToString().Substring(0, sb.Length -2));
            }
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

        internal static async Task<int> ContainerAppsRun(CommandLineArgs cmdLine, bool unittest, bool stream, bool monitor, bool deleteWhenDone, bool force)
        {
            FileInfo packageFileInfo = string.IsNullOrWhiteSpace(cmdLine.BuildFileName) ? null : new FileInfo(cmdLine.BuildFileName);
            FileInfo dacpacFileInfo = string.IsNullOrWhiteSpace(cmdLine.DacpacName) ? null : new FileInfo(cmdLine.DacpacName);
            var res = await PrepAndUploadContainerAppBuildPackage(cmdLine, packageFileInfo, dacpacFileInfo, force);
            if(res != 0)
            {
                log.LogError("Failed to upload build package to Blob storage");
                return 1;
            }

            res =  await EnqueueOverrideTargets(cmdLine);
            if (res != 0)
            {
                log.LogError("Failed to enqueue override targets");
                return 1;
            }

            res = await DeployContainerApp(cmdLine, unittest, stream, true, deleteWhenDone);
            if (res != -7)
            {
                log.LogError("Failed to deploy container app");
                log.LogInformation("Cleaning up any remaining queue messages...");
                await DeQueueOverrideTargets(cmdLine);
                return res;
            }
            else
            {
                return res;
            }
        }
        internal static async Task<int> DeployContainerApp(CommandLineArgs cmdLine, bool unittest, bool stream, bool monitor, bool deleteWhenDone)
        {
            bool initSuccess;
            (initSuccess, cmdLine) = Init(cmdLine);
            var validationErrors = Validation.ValidateContainerAppArgs(cmdLine);
           
            if(validationErrors.Count > 0)
            {
                validationErrors.ForEach(m => log.LogError(m));
                return -1;
            }
            int retVal = 0;
            var utcMonitorStart = DateTime.UtcNow;
            var success =  await ContainerApp.ContainerAppHelper.DeployContainerApp(cmdLine);
            if (!success) retVal = -7;


            if (success && monitor)
            {
                retVal = await MonitorContainerAppRuntimeProgress(cmdLine,stream, utcMonitorStart,  unittest);
            }
            if(deleteWhenDone)
            {
               success =  await ContainerAppHelper.DeleteContainerApp(cmdLine);
                if (!success) retVal = -6;
            }

            return retVal;

        }

        internal static async Task<int> PrepAndUploadContainerAppBuildPackage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
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
            if(string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
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

        internal static async Task<int> ContainerAppTestSettings(CommandLineArgs cmdLine)
        {
            bool initSuccess;
            (initSuccess, cmdLine) = Init(cmdLine);
            var validationErrors = Validation.ValidateContainerAppArgs(cmdLine);

            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(m => log.LogError(m));
            }
            ContainerAppHelper.SetEnvVariablesForTest(cmdLine);
            return await RunContainerAppWorker(cmdLine);
        }

        internal static async Task<int> MonitorContainerAppRuntimeProgress(CommandLineArgs cmdLine, bool stream, DateTime? utcMonitorStart, bool unitest)
        {

            if (string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                log.LogError("A --jobname value is required.");
                return 1;
            }

            var retVal =  await MonitorServiceBusRuntimeProgress(cmdLine, stream, utcMonitorStart, unitest, false);
            ConsolidateRuntimeLogFiles(cmdLine);
            
            return retVal;
        }

        internal static async Task<int> MonitorKubernetesRuntimeProgress(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, bool unittest = false, bool stream = false, bool consolidateLogs = true)
        {
            if (cmdLine == null)
            {
                cmdLine = new CommandLineArgs();
            }
            (var x, cmdLine) = Init(cmdLine);
            if (runtimeFile != null && secretsFile != null)
            {
                cmdLine = KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.FullName);
            }

            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogInformation("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                return -4;
            }

            var retVal =  await MonitorServiceBusRuntimeProgress(cmdLine, stream, DateTime.UtcNow.AddMinutes(-15), unittest);
            if (consolidateLogs)
            {
                ConsolidateRuntimeLogFiles(cmdLine);
            }
            return retVal;

        }

        internal static async Task<int> DequeueKubernetesOverrideTargets(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, string keyvaultname, string jobname, ConcurrencyType concurrencytype, string servicebustopicconnection)
        {
            bool valid;
            if (cmdLine == null)
            {
                cmdLine = new CommandLineArgs();
            }
            (var x, cmdLine) = Init(cmdLine);
            if (secretsFile != null && runtimeFile != null)
            {
                KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.Name);
            }

            (valid, cmdLine) = ValidateContainerQueueArgs(cmdLine, keyvaultname, jobname, concurrencytype, servicebustopicconnection);
            if (!valid)
            {
                return 1;
            }
            

            var val = await DeQueueOverrideTargets(cmdLine);
            return val;


        }

        internal static int SaveAndEncryptAciSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;

            return SaveAndEncryptSettings(cmdLine, clearText);
        }
        internal static int SaveAndEncryptContainerAppSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.AciArgs = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;
            return SaveAndEncryptSettings(cmdLine, clearText);
        }
        internal static int SaveAndEncryptKubernetesSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.AciArgs = null;
            cmdLine.ContainerAppArgs = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;
            cmdLine.IdentityArgs.IdentityName = null;
            cmdLine.IdentityArgs.ClientId = null;
            cmdLine.IdentityArgs.ResourceGroup = null;
            cmdLine.IdentityArgs.SubscriptionId = null;
            cmdLine.ContainerRegistryArgs.RegistryUserName = null;
            cmdLine.ContainerRegistryArgs.RegistryPassword = null;
            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                cmdLine.KeyVaultName = null;
            }
           
            return SaveAndEncryptSettings(cmdLine, clearText);
        }


        internal static async Task<(int, CommandLineArgs)> UploadKubernetesBuildPackage(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            (var x, cmdLine) = Init(cmdLine);
            if (runtimeFile != null && secretsFile != null)
            {
                cmdLine = KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.Name);
            }
            
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                return (-4, cmdLine);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.JobName) || string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                log.LogError("Values for --jobname, and --storageaccountname are required as prameters or included in the --secretsfile and --runtimefile");
                return (1, cmdLine);

            }
            //if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            //{
            //    log.LogError("A value for --storageaccountkey are required as prameters or included in the --secretsfile and --runtimefile");
            //    return 1;

            //}
            (bool retVal, string sbmName) = await ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
            if (!retVal)
            {
                return (1, cmdLine);
            }
            else
            {
                if (!string.IsNullOrEmpty(sbmName))
                {
                    cmdLine.BuildFileName = Path.GetFileName(sbmName);
                }
                if (packageName != null)
                {
                    cmdLine.BuildFileName = packageName.Name;
                }
                if (platinumDacpac != null)
                {
                    cmdLine.PlatinumDacpac = platinumDacpac.Name;
                }
                if (runtimeFile != null)
                {
                  
                    string runtimeContents = KubernetesManager.GenerateConfigmapYaml(cmdLine);
                    File.WriteAllText(runtimeFile.FullName, runtimeContents);
                    log.LogInformation($"Updated runtime file '{runtimeFile.FullName}' with job and package name");
                }
                return (0, cmdLine);
            }

        }

        private static async Task<(bool,string)> ValidateAndUploadContainerBuildFilesToStorage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            if (packageName == null && platinumDacpac == null)
            {
                log.LogError("Either a --packagename or --platinumdacpac argument is required");
                return (false,"");
            }

            bool sbmGenerated = false;
            //Need to build the SBM package 
            if(platinumDacpac != null && packageName == null)
            {
                if (string.IsNullOrEmpty(cmdLine.MultiDbRunConfigFileName))
                {
                    log.LogError("When a --platinumdacpac argument is specified without a --packagename, then an --override value is required so that the SBM package can be generated");
                    return (false, "");
                }

                var multiData = MultiDbHelper.ImportMultiDbTextConfig(cmdLine.MultiDbRunConfigFileName);
                if(multiData == null)
                {
                    log.LogError($"Unable to derive database targets from specified --override setting of '{cmdLine.MultiDbRunConfigFileName}' . Please check that the file exists and is properly formatted.");
                    return (false, "");
                }
                string sbmName;
                cmdLine.PlatinumDacpac = platinumDacpac.FullName;
                var stat = Worker.GetSbmFromDacPac(cmdLine, multiData, out sbmName, true);
                if(stat == sb.DacpacDeltasStatus.Success)
                {
                    if (Path.GetFileNameWithoutExtension(sbmName) != Path.GetFileNameWithoutExtension(platinumDacpac.FullName))
                    {
                        var newSbmName = Path.Combine(Path.GetDirectoryName(platinumDacpac.FullName), Path.GetFileNameWithoutExtension(platinumDacpac.FullName) + ".sbm");
                        File.Copy(sbmName, newSbmName,true);
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

            if (!await StorageManager.UploadFilesToContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, filePaths.ToArray()))
            {
                log.LogError("Unable to upload files to storage");
                return (false, "");
            }

            if(sbmGenerated)
            {
                log.LogInformation($"An SBM Package file was generated and uploaded with the DACPAC. When running the `deploy` command, please use this argument: --packagename \"{packageName.FullName}\"");
                return (true, packageName.FullName);
            }

            return (true, "");
        }
        internal static async Task<int> MonitorAciRuntimeProgress(CommandLineArgs cmdLine, FileInfo templateFile, DateTime? utcMonitorStart,  bool unitest, bool stream = false)
        {
            if (templateFile != null)
            {
                cmdLine = Aci.AciHelper.GetRuntimeValuesFromDeploymentTempate(cmdLine, templateFile.FullName);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                log.LogError("A --jobname value is required.");
                return 1;
            }

            var retVal =  await MonitorServiceBusRuntimeProgress(cmdLine, stream, utcMonitorStart, unitest,  true);
            ConsolidateRuntimeLogFiles(cmdLine);
            
            return retVal;
        }

        internal static async Task<int> DeployAciTemplate(CommandLineArgs cmdLine, FileInfo templateFile, bool monitor, bool unittest = false, bool stream = false)
        {
            Init(cmdLine);
            if (monitor)
            {
                bool kvSuccess;
                (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
                if (!kvSuccess)
                {
                    log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                    return -4;
                }
            }

            if (string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.SubscriptionId))
            {
                log.LogError("The value for --subscriptionid is required as a parameter or inclusion in the --settingsfile");
            }

            cmdLine = Aci.AciHelper.GetRuntimeValuesFromDeploymentTempate(cmdLine, templateFile.FullName);
            var utcMonitorStart = DateTime.UtcNow;
            var success = await Aci.AciHelper.DeployAciInstance(templateFile.FullName, cmdLine.IdentityArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup, cmdLine.AciArgs.AciName, cmdLine.JobName);
            if (success && monitor)
            {
                return await MonitorAciRuntimeProgress(cmdLine, templateFile, utcMonitorStart, unittest, stream);
            }
            else if (success) return 0; else return 1;
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

        internal static async Task<int> PrepAndUploadAciBuildPackage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, FileInfo outputFile, bool force)
        {
            Init(cmdLine);
            if(packageName != null) cmdLine.BuildFileName = packageName.FullName;
            var valErrors = Validation.ValidateAciAppArgs(cmdLine);
            if(valErrors.Count > 0)
            {
                valErrors.ForEach(m => log.LogError(m));
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                bool kvSuccess;
                (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
                if (!kvSuccess)
                {
                    log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                    return -4;
                }
            }
            (bool na, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName) && string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                log.LogError("If --keyvaultname is not provided as an argument or in the --settingsfile, then --storageaccountname is required");
                return -1;
            }
            (bool retVal, string sbmName) = await ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
            if (!retVal)
            {
                return -1;
            }
            if(!string.IsNullOrWhiteSpace(sbmName))
            {
                cmdLine.BuildFileName = sbmName;
            }

            string template = Aci.AciHelper.CreateAciArmTemplate(cmdLine);
            string fileName;
            if (outputFile == null)
            {
                fileName = Path.Combine(Directory.GetCurrentDirectory(), $"{cmdLine.JobName}_aci_template.json");
            }
            else
            {
                fileName = outputFile.FullName;
            }
            File.WriteAllText(fileName, template);
            log.LogInformation($"Wrote ACI deployment ARM template to: {fileName}");

            return 0;
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
                if(currentCommit == lastCommit && currentError == lastError && currentEvents == lastEvents)
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
                if(counter == 10)
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

        internal static void TestAuth(string discard)
        {
            System.Console.WriteLine(KeyVault.KeyVaultHelper.GetSecret("sbm3keyvault", "StorageAccountName"));
        }

        

        internal static async Task<int> KubernetesRun(CommandLineArgs cmdLine, FileInfo Override, FileInfo packagename, FileInfo platinumdacpac, bool force, bool allowObjectDelete, bool unittest, bool stream, bool cleanupOnFailure)
        {
            (var x, cmdLine) = Init(cmdLine);
            string k8Jobname = KubernetesManager.KubernetesJobName(cmdLine);
            if (packagename == null && platinumdacpac == null)
            {
                log.LogError("Either an SBM package or DACPAC file is required.");
                return -1;
            }
            //Save secrets
            if (packagename != null)
            {
                cmdLine.BuildFileName = packagename.FullName;
            }
            if (platinumdacpac != null)
            {
                cmdLine.PlatinumDacpac = platinumdacpac.FullName;
            }
            cmdLine.MultiDbRunConfigFileName = Override.FullName;

            //Upload 'prep'
            (var retVal, cmdLine) = await UploadKubernetesBuildPackage(cmdLine, null, null, packagename, platinumdacpac, force);
            if(retVal != 0)
            {
                log.LogError("There was a problem uploading the build package to Azure storage");
                return -1;
            }

            //Create YAML files
            var kubernetesFiles = await KubernetesManager.SaveKubernetesYamlFiles(cmdLine, cmdLine.JobName, new DirectoryInfo(Environment.CurrentDirectory));
            var runtimeFileInfo = new FileInfo(kubernetesFiles.RuntimeConfigMapFile);
            FileInfo secretsFileInfo = null;
            bool secretsExist = false;
            if (!string.IsNullOrWhiteSpace(kubernetesFiles.SecretsFile))
            {
                secretsFileInfo = new FileInfo(kubernetesFiles.SecretsFile);
                secretsExist = true;
            }

            //Enqueue
            retVal = await EnqueueContainerOverrideTargets(cmdLine,secretsFileInfo, runtimeFileInfo, cmdLine.ConnectionArgs.KeyVaultName, cmdLine.JobName, cmdLine.ConcurrencyType, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, Override);
            if (retVal != 0)
            {
                log.LogError("There was a problem enqueuing the database targest to Service Bus");
                return -1;
            }
            //Kubernetes apply
            var success = KubernetesManager.ApplyDeployment(kubernetesFiles);
            if(!success)
            {
                log.LogError("There was a problem deploying to Kubernetes");
                log.LogInformation("Attempting to clean up Kubernetes resources...");
                if (cleanupOnFailure)
                {
                    await DequeueKubernetesOverrideTargets(cmdLine, secretsFileInfo, runtimeFileInfo, cmdLine.ConnectionArgs.KeyVaultName, cmdLine.JobName, cmdLine.ConcurrencyType, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString);
                    KubernetesManager.CleanUpKubernetesResource(secretsExist);
                }
                return -1;
            }
            //Monitor pod creation
            log.LogInformation("Checking for successful job start...");
            
            if (!Kubernetes.KubernetesManager.MonitorForPodStart(k8Jobname))
            {
                log.LogError("Failed to start Kubernetes jobs. Running clean-up");
                await DeQueueOverrideTargets(cmdLine);
                if (cleanupOnFailure)
                {
                    KubernetesManager.CleanUpKubernetesResource(secretsExist);
                }
                return -1;
            }

            //Monitor service bus
            retVal = await MonitorKubernetesRuntimeProgress(cmdLine, secretsFileInfo, runtimeFileInfo, unittest, stream, false);

            //Clean-up
            log.LogInformation("Cleaning up Kubernetes resources...");
            KubernetesManager.CleanUpKubernetesResource(secretsExist, k8Jobname);

            ConsolidateRuntimeLogFiles(cmdLine);
            return retVal;
        }
        internal static async Task<int> SaveKubernetesYamlFiles(CommandLineArgs cmdLine,DirectoryInfo path, string prefix, FileInfo packagename, FileInfo platinumdacpac, bool force)
        {
            (var x, cmdLine) = Init(cmdLine);
            if (packagename != null)
            {
                cmdLine.BuildFileName = packagename.FullName;
            }
            if (platinumdacpac != null)
            {
                cmdLine.PlatinumDacpac = platinumdacpac.FullName;
            }
            await KubernetesManager.SaveKubernetesYamlFiles(cmdLine, prefix, path);
            return 0;
        }
        #region .: Helper Processes :.
        internal static int CreateDacpac(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            log.LogInformation("Creating DACPAC");
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }


            string fullName = Path.GetFullPath(cmdLine.DacpacName);
            string path = Path.GetDirectoryName(fullName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!sb.DacPacHelper.ExtractDacPac(cmdLine.Database, cmdLine.Server, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, fullName))
            {
                log.LogError($"Error creating the dacpac from {cmdLine.Server} : {cmdLine.Database}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.LogInformation($"DACPAC created from {cmdLine.Server} : {cmdLine.Database} saved to -- {fullName}");
            }
            return 0;
        }

        private static (bool, CommandLineArgs) ValidateContainerQueueArgs(CommandLineArgs cmdLine, string keyvaultname, string jobname, ConcurrencyType concurrencytype, string servicebustopicconnection)
        {

            cmdLine.KeyVaultName = keyvaultname;
            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);

            if (!string.IsNullOrWhiteSpace(servicebustopicconnection))
            {
                cmdLine.ServiceBusTopicConnection = servicebustopicconnection;
            }
            if (!string.IsNullOrWhiteSpace(jobname))
            {
                cmdLine.JobName = jobname;
            }
            if (concurrencytype != ConcurrencyType.Count)
            {
                cmdLine.ConcurrencyType = concurrencytype;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("The ServiceBusTopicConnection is required as either a --servicebustopicconnection parameter or as part of the --secretsfile");
                return (false, cmdLine);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                log.LogError("The JobName is required as either a --jobname parameter or as part of the --runtimefile");
                return (false, cmdLine);
            }

            return (true, cmdLine);
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
                (valid, cmdLine) = ValidateContainerQueueArgs(cmdLine, keyvaultname, jobname, concurrencytype, servicebustopicconnection);
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

        internal static void SyncronizeDatabase(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            bool success = Synchronize.SyncDatabases(cmdLine);
            if (success)
                System.Environment.Exit(0);
            else
                System.Environment.Exit(954);
        }


        internal static void GetDifferences(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            string history = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            log.LogInformation(history);
            System.Environment.Exit(0);
        }

        internal static void CreateBackout(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            string packageName = BackoutCommandLine.CreateBackoutPackage(cmdLine);
            if (!String.IsNullOrEmpty(packageName))
            {
                log.LogInformation(packageName);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(856);
            }
        }

        internal static int AddScriptsToPackage(CommandLineArgs cmdLine)
        {
            if (!File.Exists(cmdLine.OutputSbm))
            {
                log.LogWarning($"The specified output file '{cmdLine.OutputSbm}' does not exists. If you want to create a package, please use the \"sbm create\" command");
                return -646;
            }

            if (Path.GetExtension(cmdLine.OutputSbm).ToLower() == ".sbx")
            {
                string sbxFileName = cmdLine.OutputSbm;
                string workingDir = Path.GetDirectoryName(cmdLine.OutputSbm);
                if (string.IsNullOrWhiteSpace(workingDir))
                {
                    workingDir = Directory.GetCurrentDirectory();
                }
                log.LogInformation("Creating Base Build File XML");
                var buildData = sb.SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                buildData.AcceptChanges();
                sb.SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, workingDir, sbxFileName);
                buildData.WriteXml(sbxFileName);
                var counter = 1.0;
                foreach (var file in cmdLine.Scripts)
                {
                    if (file.Directory.ToString().ToLower() != workingDir.ToLower())
                    {
                        File.Copy(file.FullName, Path.Combine(workingDir, file.Name));
                    }
                    sb.SqlBuildFileHelper.AddScriptFileToBuild(ref buildData, sbxFileName, file.Name, counter, "", true, true, "client", true, "", false, true, Environment.UserName, 500, "");
                    counter++;
                }
                buildData.AcceptChanges();
                buildData.WriteXml(sbxFileName);
            }
            else
            {
                string workingDir = "", projFilePath = "", projectFileName = "";
                sb.SqlBuildFileHelper.ExtractSqlBuildZipFile(cmdLine.OutputSbm, ref workingDir, ref projFilePath, ref projectFileName, true, true, out string result);
                bool success = sb.SqlBuildFileHelper.LoadSqlBuildProjectFile(out sb.SqlSyncBuildData buildData, projectFileName, true);
                if (success)
                {
                    List<string> copied = new List<string>();
                    cmdLine.Scripts.ToList().ForEach(f =>
                    {
                        if (f.Directory.ToString().ToLower() != workingDir.ToLower() && !File.Exists(Path.Combine(workingDir, f.Name)))
                        {
                            File.Copy(f.FullName, Path.Combine(workingDir, f.Name));
                            copied.Add(Path.Combine(workingDir, f.Name));
                        }
                    });
                    sb.BuildDataHelper.GetLastBuildNumberAndDb(buildData, out double lastBuildNumber, out string lastDatabase);
                    foreach (var file in cmdLine.Scripts)
                    {
                        lastBuildNumber++;
                        sb.SqlBuildFileHelper.AddScriptFileToBuild(ref buildData, projectFileName, file.Name, lastBuildNumber, "", true, true, "client", true, cmdLine.OutputSbm, true, true, Environment.UserName, 500, "");
                    }
                    sb.SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDir);
                }
                else
                {
                    log.LogError($"Unable to extract and read the build file at {cmdLine.OutputSbm}!");
                    return -952;
                }
            }
            log.LogInformation($"Added {cmdLine.Scripts.Count()} scripts to '{cmdLine.OutputSbm}'");
            var fi = new FileInfo(cmdLine.OutputSbm);
            ListPackageScripts(new FileInfo[] { fi }, true);
            return 0;

        }

        internal static int CreatePackageFromDacpacs(string outputSbm, FileInfo platinumDacpac, FileInfo targetDacpac, bool allowObjectDelete)
        {
            var outputSbmFile = Path.GetFullPath(outputSbm);
            var res = sb.DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacpac.FullName, targetDacpac.FullName, true, string.Empty, 500, allowObjectDelete, out string tmpSbm);

            if (res == sb.DacpacDeltasStatus.Success)
            {
                File.Move(tmpSbm, outputSbmFile, true);
                log.LogInformation($"Created SBM package:  {outputSbmFile}");
                ListPackageScripts(new FileInfo[] { new FileInfo(outputSbmFile) }, true);
                return 0;
            }
            else
            {
                switch (res)
                {
                    case sb.DacpacDeltasStatus.InSync:
                    case sb.DacpacDeltasStatus.OnlyPostDeployment:
                        log.LogWarning("No package created -- the databases are already in sync");
                        break;
                    default:
                        log.LogError("There was an error creating the requested package");
                        break;
                }
                return -232323;
            }
        }

        internal static int CreatePackageFromDiff(CommandLineArgs cmdLine)
        {
            string sbmFileName = Path.GetFullPath(cmdLine.OutputSbm);
            if (File.Exists(sbmFileName))
            {
                log.LogError($"The output file '{sbmFileName}' already exists. Please delete the file or use 'sbm add' if you want to add new scripts to the file");
                return -343;
            }
            string path = Path.GetDirectoryName(sbmFileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string id = Guid.NewGuid().ToString();
            string goldTmp = Path.Combine(path, $"gold-{id}.dacpac");
            string targetTmp = Path.Combine(path, $"target-{id}.dacpac");
            if (!sb.DacPacHelper.ExtractDacPac(cmdLine.SynchronizeArgs.GoldDatabase, cmdLine.SynchronizeArgs.GoldServer, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, goldTmp))
            {
                log.LogError($"Error creating the tempprary dacpac from {cmdLine.SynchronizeArgs.GoldServer} : {cmdLine.SynchronizeArgs.GoldDatabase}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.LogInformation($"Temporary DACPAC created from {cmdLine.SynchronizeArgs.GoldServer} : {cmdLine.SynchronizeArgs.GoldDatabase} saved to -- {goldTmp}");
            }

            if (!sb.DacPacHelper.ExtractDacPac(cmdLine.Database, cmdLine.Server, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, targetTmp))
            {
                log.LogError($"Error creating the tempprary dacpac from {cmdLine.Server} : {cmdLine.Database}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.LogInformation($"Temporary DACPAC created from {cmdLine.Server} : {cmdLine.Database} saved to -- {targetTmp}");
            }

            var res = sb.DacPacHelper.CreateSbmFromDacPacDifferences(goldTmp, targetTmp, true, string.Empty, 500, cmdLine.AllowObjectDelete, out string tmpSbm);
            log.LogInformation("Cleaning up temporary files");
            File.Delete(goldTmp);
            File.Delete(targetTmp);

            if (res == sb.DacpacDeltasStatus.Success)
            {
                File.Move(tmpSbm, sbmFileName);
                log.LogInformation($"Created SBM package:  {sbmFileName}");
                ListPackageScripts(new FileInfo[] { new FileInfo(sbmFileName) }, true);
                return 0;
            }
            else
            {
                switch (res)
                {
                    case sb.DacpacDeltasStatus.InSync:
                    case sb.DacpacDeltasStatus.OnlyPostDeployment:
                        log.LogWarning("No package created -- the databases are already in sync");
                        break;
                    default:
                        log.LogError("There was an error creating the requested package");
                        break;
                }
                return -232323;
            }
        }

        internal static void ListPackageScripts(FileInfo[] packages, bool withHash)
        {
            string workingDir = "", projFilePath = "", projectFileName = "";

            foreach (var file in packages)
            {
                sb.SqlBuildFileHelper.ExtractSqlBuildZipFile(file.FullName, ref workingDir, ref projFilePath, ref projectFileName, true, true, out string result);
                bool success = sb.SqlBuildFileHelper.LoadSqlBuildProjectFile(out sb.SqlSyncBuildData buildData, projectFileName, true);
                List<string[]> contents = new List<string[]>();
                string dateformat = "yyyy-MM-dd hh:mm:ss";
                if (!withHash)
                {
                    contents.Add(new string[] { "", "", "", "", "" });
                    contents.Add(new string[] { "Order", "Script Name", "Last Date", "Last User", "Script Id" });
                    contents.Add(new string[] { "", "", "", "", "" });
                }
                else
                {
                    contents.Add(new string[] { "", "", "", "", "", "" });
                    contents.Add(new string[] { "Order", "Script Name", "Last Date", "Last User", "Script Id", "SHA1 Hash" });
                    contents.Add(new string[] { "", "", "", "", "", "" });
                }
                if (success)
                {
                    var rows = buildData.Script.OrderBy(r => r.BuildOrder).ToList();
                    //for (int i = 0; i < buildData.Script.Rows.Count; i++)
                    foreach (var s in rows)
                    {
                        if (withHash)
                        {
                            sb.SqlBuildFileHelper.GetSHA1Hash(Path.Combine(projFilePath, s.FileName), out string fileHash, out string textHash, s.StripTransactionText);
                            contents.Add(new string[] { s.BuildOrder.ToString(), s.FileName, (s.DateModified == DateTime.MinValue) ? s.DateAdded.ToString(dateformat) : s.DateModified.ToString(dateformat), string.IsNullOrWhiteSpace(s.ModifiedBy) ? s.AddedBy : s.ModifiedBy, s.ScriptId, textHash });

                        }
                        else
                        {
                            contents.Add(new string[] { s.BuildOrder.ToString(), s.FileName, (s.DateModified == DateTime.MinValue) ? s.DateAdded.ToString(dateformat) : s.DateModified.ToString(dateformat), string.IsNullOrWhiteSpace(s.ModifiedBy) ? s.AddedBy : s.ModifiedBy, s.ScriptId });
                        }
                    }
                }
                var sizing = TablePrintSizing(contents);
                var output = ConsoleTableBuilder(contents, sizing);
                string hash = "";
                if (withHash)
                {
                    hash = $" (Package Hash: {sb.SqlBuildFileHelper.CalculateSha1HashFromPackage(file.FullName)})";
                }
                System.Console.WriteLine();
                System.Console.WriteLine(file.FullName + hash);
                System.Console.WriteLine(output);
                System.Console.WriteLine();
            }
        }

        internal static int CreatePackageFromScripts(CommandLineArgs cmdLine)
        {

            if (File.Exists(cmdLine.OutputSbm))
            {
                log.LogWarning($"The specified output file '{cmdLine.OutputSbm}' already exists and can not be created. If you want to add scripts to this file, please use the \"sbm add\" command");
                return -432;
            }

            string workingDir = Path.GetDirectoryName(cmdLine.OutputSbm);
            if (string.IsNullOrWhiteSpace(workingDir))
            {
                workingDir = Directory.GetCurrentDirectory();
            }
            if (Path.GetExtension(cmdLine.OutputSbm).ToLower() == ".sbx")
            {
                string sbxFileName = cmdLine.OutputSbm;
                workingDir = Path.GetDirectoryName(cmdLine.OutputSbm);
                if (string.IsNullOrWhiteSpace(workingDir))
                {
                    workingDir = Directory.GetCurrentDirectory();
                }
                log.LogInformation("Creating Base Build File XML");
                var buildData = sb.SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                buildData.AcceptChanges();
                sb.SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, workingDir, sbxFileName);
                buildData.WriteXml(sbxFileName);
                var counter = 1.0;
                foreach (var file in cmdLine.Scripts)
                {
                    if (file.Directory.ToString().ToLower() != workingDir.ToLower() && !File.Exists(Path.Combine(workingDir, file.Name)))
                    {
                        File.Copy(file.FullName, Path.Combine(workingDir, file.Name));
                    }
                    sb.SqlBuildFileHelper.AddScriptFileToBuild(ref buildData, sbxFileName, file.Name, counter, "", true, true, "client", true, "", false, true, Environment.UserName, 500, "");
                    counter++;
                }
                buildData.AcceptChanges();
                buildData.WriteXml(sbxFileName);

            }
            else
            {
                List<string> copied = new List<string>();
                cmdLine.Scripts.ToList().ForEach(f =>
                {
                    if (f.Directory.ToString().ToLower() != workingDir.ToLower() && !File.Exists(Path.Combine(workingDir, f.Name)))
                    {
                        File.Copy(f.FullName, Path.Combine(workingDir, f.Name));
                        copied.Add(Path.Combine(workingDir, f.Name));
                    }
                });

                bool success = sb.SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(cmdLine.OutputSbm, cmdLine.Scripts.Select(f => f.FullName).ToList(), "client", 500, false);
                copied.ForEach(f => File.Delete(f));
                if (!success)
                {
                    log.LogError("Unable to create the build file!");
                    return -425;
                }
                ListPackageScripts(new FileInfo[] { new FileInfo(cmdLine.OutputSbm) }, true);
            }
            log.LogInformation($"Successfully created build file '{cmdLine.OutputSbm}' with {cmdLine.Scripts.Count()} scripts");
            return 0;
        }

        internal static void GetPackageHash(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.LogError("No --packagename was specified. This is required for 'sbm gethash' command");
                System.Environment.Exit(626);

            }
            string packageName = cmdLine.BuildFileName;
            string hash = sb.SqlBuildFileHelper.CalculateSha1HashFromPackage(packageName);
            if (!String.IsNullOrEmpty(hash))
            {
                //log.LogInformation(hash);
                System.Console.WriteLine(hash);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(621);
            }
        }

        internal static void ExecutePolicyCheck(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }


            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.LogError("No --packagename was specified. This is required for 'sbm policycheck' command");
                System.Environment.Exit(34536);

            }
            string packageName = cmdLine.BuildFileName;
            PolicyHelper helper = new PolicyHelper();
            bool passed;
            List<string[]> policyMessages = helper.CommandLinePolicyCheck(packageName, out passed);
            if (policyMessages.Count > 0)
            {
                List<string[]> tmp = new List<string[]>();
                tmp.Add(new string[] { "", "", "" });
                tmp.Add(new string[] { "Severity", "Script Name", "Message" });
                tmp.Add(new string[] { "", "", "" });
                tmp.AddRange(policyMessages);
                var sizing = TablePrintSizing(tmp);
                var table = ConsoleTableBuilder(tmp, sizing);

                System.Console.WriteLine();
                System.Console.WriteLine($"Policy results for: {cmdLine.BuildFileName}");
                System.Console.WriteLine(table);
                System.Console.WriteLine();

            }

            if (passed)
            {
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(739);
            }
        }

        internal static void PackageSbxFilesIntoSbmFiles(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);


            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.Directory))
            {
                log.LogError("The --directory argument is required for 'sbm package' command");
                System.Environment.Exit(9835);
            }
            string directory = cmdLine.Directory;
            string message;
            List<string> sbmFiles = sb.SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directory, out message);
            if (sbmFiles.Count > 0)
            {
                foreach (string sbm in sbmFiles)
                    log.LogInformation(sbm);

                System.Environment.Exit(0);
            }
            else if (message.Length > 0)
            {
                log.LogWarning(message);
                System.Environment.Exit(604);
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

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

        internal static sb.DacpacDeltasStatus GetSbmFromDacPac(CommandLineArgs cmd, MultiDbData multiDb, out string sbmName, bool batchScripts = false)
        {
            if (cmd.MultiDbRunConfigFileName.Trim().ToLower().EndsWith("sql"))
            {
                //if we are getting the list from a SQL statement, then the database and server settings mean something different! Dont pass them in.
                return sb.DacPacHelper.GetSbmFromDacPac(cmd.RootLoggingPath,
                   cmd.DacPacArgs.PlatinumDacpac,
                   cmd.DacPacArgs.TargetDacpac,
                   string.Empty,
                   string.Empty,
                   cmd.AuthenticationArgs.AuthenticationType,
                   cmd.AuthenticationArgs.UserName,
                   cmd.AuthenticationArgs.Password,
                   cmd.BuildRevision,
                   cmd.DefaultScriptTimeout,
                   multiDb, out sbmName, batchScripts, cmd.AllowObjectDelete);
            }
            else
            {
                return sb.DacPacHelper.GetSbmFromDacPac(cmd.RootLoggingPath,
                    cmd.DacPacArgs.PlatinumDacpac,
                    cmd.DacPacArgs.TargetDacpac,
                    cmd.Database,
                    cmd.Server,
                    cmd.AuthenticationArgs.AuthenticationType,
                    cmd.AuthenticationArgs.UserName,
                    cmd.AuthenticationArgs.Password,
                    cmd.BuildRevision,
                    cmd.DefaultScriptTimeout,
                    multiDb, out sbmName, batchScripts, cmd.AllowObjectDelete);
            }
        }

        #endregion

        internal static List<int> TablePrintSizing(List<string[]> input)
        {
            var delimiterCount = input.Select(s => s.Length).Max();
            List<int> sectionLength = new List<int>();
            int len = 0;
            for (var t = 0; t < delimiterCount; t++)
            {
                if (t == 2 && input.Count > 0)
                {
                    len = input.Select(x => (t < x.Length) ? x[t].Length : 0).Max();
                }
                else
                {
                    len = input.Select(x => (t < x.Length) ? x[t].Length : 0).Max();
                }

                sectionLength.Add(len);
            }

            return sectionLength;
        }
        internal static string ConsoleTableBuilder(List<string[]> splits, List<int> sectionLengths)
        {
            StringBuilder sb = new StringBuilder();
            var total = sectionLengths.Sum(e => e) + sectionLengths.Count() * 3 - 1;

            var tmpLine = string.Empty;
            foreach (var splitLine in splits)
            {
                int currentLoc = 0;
                int endLength = 0;
                string current = "| ";
                for (int i = 0; i < sectionLengths.Count(); i++)
                {
                    endLength += sectionLengths[i] + 3;

                    if (splitLine[i].Length == 0 && string.Join("", splitLine).Trim().Length == 0)  //and empty line used to denote a dash separator
                    {
                        current = current.Substring(0, current.Length - 1) + new string('-', sectionLengths[i] + 2) + "| ";
                    }
                    else if (i < splitLine.Length)
                    {
                        if (splitLine[i].Length + currentLoc > currentLoc + sectionLengths[i]) //content for this section and overflows
                        {
                            current += splitLine[i].Trim();
                            currentLoc += current.Length;
                        }
                        else //there is content for this section
                        {
                            current += splitLine[i].Trim().PadRight(sectionLengths[i]) + " | ";
                            currentLoc += current.Length;
                        }
                    }
                    else if (splitLine.Length > 3)
                    {
                        current += new string(' ', sectionLengths[i]) + " | ";
                        currentLoc += current.Length;
                    }
                    else
                    {
                        if (current.Length < endLength) //no content for this section and isn't an overflow
                        {
                            current = current.PadRight(endLength - 1) + " | ";
                        }
                    }

                }
                sb.Append(current + Environment.NewLine);
            }
            sb.AppendLine("|" + new string('-', total) + "|");
            return sb.ToString().Trim();
        }

        internal static void UnpackSbmFile(DirectoryInfo directory, FileInfo package)
        {
            var projectFileName = "";
            var projectFilePath = "";
            string result;
            string dir = directory.FullName;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            bool success = SqlSync.SqlBuild.SqlBuildFileHelper.ExtractSqlBuildZipFile(package.FullName, ref dir,ref projectFilePath, ref projectFileName,false, true, out result);
            if(File.Exists(Path.Combine(dir,projectFileName)))
            {
                var sbmName = Path.GetFileNameWithoutExtension(package.FullName) + ".sbx";
                File.Move(Path.Combine(dir, projectFileName), Path.Combine(dir, sbmName));
            }
            if(success)
            {
                log.LogInformation($"SBM file extracted to: {Path.GetFullPath(directory.FullName)}");
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
