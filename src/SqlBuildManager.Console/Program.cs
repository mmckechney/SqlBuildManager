using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Queue;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SqlSync.SqlBuild.SqlSyncBuildData;
using sb = SqlSync.SqlBuild;
namespace SqlBuildManager.Console
{

    public class Program
    {
        static Program()
        {
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), applicationLogFileName);
        }
        public const string applicationLogFileName = "SqlBuildManager.Console.log";

        private static Microsoft.Extensions.Logging.ILogger log;
        internal static string[] AppendLogFiles = new string[] { "commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg" };

        static int Main(string[] args)
        {
            var fn = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var currentPath = Path.GetDirectoryName(fn);

            log.LogDebug("Received Command: " + String.Join(" | ", args));

            RootCommand rootCommand = CommandLineBuilder.SetUp();

            try
            {
                Task<int> val = rootCommand.InvokeAsync(args);
                val.Wait();
                rootCommand = null;
                int result = val.Result;
                log.LogInformation($"Exiting with code {result}");


                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                return result;
            }
            catch (Exception exe)
            {
                System.Console.WriteLine($"Error closing: {exe.Message}");
                System.Environment.FailFast("");
                return -100000;
            }
        }

        private static (bool, CommandLineArgs) Init(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
            }
            return (decryptSuccess, cmdLine);
        }

        internal static int QueryDatabases(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;


            if (!string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), Program.applicationLogFileName, cmdLine.RootLoggingPath);
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
                var blobTask = WriteLogsToBlobContainer(cmdLine.BatchArgs.OutputContainerSasUrl, cmdLine.RootLoggingPath);
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

        internal static int RunBatchPreStage(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.PreStageBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal static int RunBatchExecution(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), Program.applicationLogFileName, cmdLine.RootLoggingPath);

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            log.LogDebug("Entering Batch Execution");
            log.LogInformation("Running Batch Execution...");
            int retVal;
            string readOnlySas;
            (retVal, readOnlySas) = batchExe.StartBatch();
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

        internal static int RunBatchQuery(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), Program.applicationLogFileName, cmdLine.RootLoggingPath);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
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

            (retVal, readOnlySas) = batchExe.StartBatch();

            if (!string.IsNullOrWhiteSpace(readOnlySas))
            {
                log.LogInformation("Downloading the consolidated output file...");
                try
                {
                    var cloudBlobContainer = new BlobContainerClient(new Uri(readOnlySas));
                    var blob = cloudBlobContainer.GetBlobClient(cmdLine.OutputFile.Name);
                    blob.DownloadTo(cmdLine.OutputFile.FullName);
                    log.LogInformation($"Output file copied locally to {cmdLine.OutputFile.FullName}");
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

        internal static void SaveAndEncryptSettings(CommandLineArgs cmdLine, bool clearText)
        {

            if (string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                log.LogError("When 'sbm batch savesettings' is specified the --settingsfile argument is also required");
                return;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.SettingsFileKey) && cmdLine.SettingsFileKey.Length < 16)
            {
                log.LogError("The value for the --settingsfilekey must be at least 16 characters long");
                return;
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
                }
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to save settings file.\r\n{exe.ToString()}");
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

            var status = Program.GetSbmFromDacPac(cmdLine, new SqlSync.SqlBuild.MultiDb.MultiDbData(), out name, true);
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
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), Program.applicationLogFileName, cmdLine.RootLoggingPath);


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

                };
                ConnectionData connData = new ConnectionData(cmdLine.Server, cmdLine.Database);
                sb.SqlBuildHelper helper = new sb.SqlBuildHelper(connData,true, "", cmdLine.Transactional);
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
                if(stat.StatusMessage.ToLower().Contains("build failure") || stat.StatusMessage.ToLower().Contains("build failed"))
                {
                    LocalRunInfo.Success = false;
                }
            }
            
            else if (e.UserState is sb.CommitFailureEventArgs)
            {
                log.LogError("Failed to Commit Build " + ((sb.CommitFailureEventArgs)e.UserState).ErrorMessage);
                LocalRunInfo.Success = false;
            }
            else if(e.UserState is sb.ScriptRunStatusEventArgs)
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

        internal static int RunThreadedExecution(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            if(string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = Directory.GetCurrentDirectory();
            }

            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), Program.applicationLogFileName, cmdLine.RootLoggingPath);

            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }

            DateTime start = DateTime.Now;
            log.LogDebug("Entering Threaded Execution");
            log.LogDebug(cmdLine.ToStringExtension(StringType.Basic));
            log.LogDebug(cmdLine.ToStringExtension(StringType.Batch));
            log.LogInformation("Running Threaded Execution...");
            ThreadedExecution runner = new ThreadedExecution(cmdLine);
            int retVal = runner.Execute();
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

            if (!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.LogInformation("Writing log files to storage...");
                var blobTask = WriteLogsToBlobContainer(cmdLine.BatchArgs.OutputContainerSasUrl, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }

            log.LogDebug("Exiting Threaded Execution");
            return retVal;

        }


        private static async Task<bool> WriteLogsToBlobContainer(string outputContainerSasUrl, string rootLoggingPath)
        {
            try
            {
                //var writeTasks = new List<Task>();
                log.LogInformation($"Writing log files to blob storage at {outputContainerSasUrl}");
                var renameLogFiles = new string[] { "sqlbuildmanager", "csv" };
                BlobContainerClient container = new BlobContainerClient(new Uri(outputContainerSasUrl));
                var fileList = Directory.GetFiles(rootLoggingPath, "*.*", SearchOption.AllDirectories);
                var taskId = Environment.GetEnvironmentVariable("AZ_BATCH_TASK_ID");
                string machine = Environment.MachineName;

                foreach (var f in fileList)
                {
                    try
                    {
                        var tmp = Path.GetRelativePath(rootLoggingPath, f);

                        if (Program.AppendLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {

                            tmp = $"{taskId}/{tmp}";
                            log.LogInformation($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                await rename.UploadAsync(fs);

                            }
                        }
                        else if (renameLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {

                            //var localTemp = Path.Combine(Path.GetDirectoryName(f), tmp);
                            //File.Copy(f, localTemp);
                            tmp = $"{taskId}/{tmp}";
                            log.LogInformation($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                await rename.UploadAsync(fs);

                            }
                        }
                        else
                        {
                            log.LogInformation($"Saving File '{f}' as '{tmp}'");
                            var b = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                await b.UploadAsync(fs);

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError($"Unable to upload log file '{f}' to blob storage: {e.Message}");
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to upload log files to blob storage.{Environment.NewLine}{exe.Message}");
                return false;
            }
        }

        #region .: Helper Processes :.
        internal static int CreateDacpac(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);

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
            return 0;

        }

        internal static int CreatePackageFromDacpacs(string outputSbm, FileInfo platinumDacpac, FileInfo targetDacpac)
        {
            var outputSbmFile = Path.GetFullPath(outputSbm);
            var res = sb.DacPacHelper.CreateSbmFromDacPacDifferences(platinumDacpac.FullName, targetDacpac.FullName, true, string.Empty, 500, out string tmpSbm);

            if (res == sb.DacpacDeltasStatus.Success)
            {
                File.Move(tmpSbm, outputSbmFile);
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
            if(File.Exists(sbmFileName))
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

            var res = sb.DacPacHelper.CreateSbmFromDacPacDifferences(goldTmp, targetTmp, true, string.Empty, 500, out string tmpSbm);
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
                switch(res)
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
                    foreach(var s in rows)
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
                if(withHash)
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

        internal async static Task<int> QueueOverrideTargets(CommandLineArgs cmdLine)
        {
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), applicationLogFileName);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(Program), applicationLogFileName, cmdLine.RootLoggingPath);
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            var start = DateTime.Now;
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return 3424;
            }

            if (string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("A --servicebusconnection value is required. Please include this in either the settings file content or as a specific command option");
                return 9839;
            }
            (int ret, string msg) = Validation.ValidateBatchjobName(cmdLine.BatchArgs.BatchJobName);
            if(ret != 0)
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
            var qManager = new QueueManager(cmdLine.BatchArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName);
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
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -53443;
            }


            if (string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ServiceBusTopicConnectionString))
            {
                log.LogError("A --servicebustopicconnection value is required. Please include this in either the settings file content or as a specific command option");
                return 9839;
            }

            var qManager = new QueueManager(cmdLine.BatchArgs.ServiceBusTopicConnectionString, cmdLine.BatchArgs.BatchJobName);
            bool success = await qManager.ClearQueueMessages();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);
            if (success)
            {
                log.LogInformation("Successfully removed messages from Service Bus queue topics");
                return 0;
            }
            else
            {
                log.LogError("Error receiving messages to Service Bus queue");
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
                   multiDb, out sbmName, batchScripts);
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
                    multiDb, out sbmName, batchScripts);
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

                    if(splitLine[i].Length == 0 && string.Join("", splitLine).Trim().Length == 0)  //and empty line used to denote a dash separator
                    {
                        current =  current.Substring(0,current.Length-1) + new string('-', sectionLengths[i]+2) + "| " ;
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
    }
}
   


