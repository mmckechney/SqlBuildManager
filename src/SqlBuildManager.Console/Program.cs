using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using BlueSkyDev.Logging;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
namespace SqlBuildManager.Console
{

    class Program
    {
       
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static string[] AppendLogFiles = new string[] { "commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg" };

        static int Main(string[] args)
        {
            var fn = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var currentPath = Path.GetDirectoryName(fn);
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(currentPath, "log4net.config")));
            SqlBuildManager.Logging.Configure.SetLoggingPath();

            log.Debug("Received Command: " + String.Join(" | ", args));

            RootCommand rootCommand = CommandLineConfig.SetUp();

            try
            {
                Task<int> val = rootCommand.InvokeAsync(args);
                val.Wait();
                rootCommand = null;
                int result = val.Result;
                log.Info($"Exiting with code {result}");
                
                LogManager.Flush(5000);
                logRepository = null;
                return result;
            }
            catch(Exception exe)
            {
                System.Console.WriteLine($"Error closing: {exe.Message}");
                System.Environment.FailFast("");
                return -100000;
            }

        }


        internal static void RunQueryExecution(CommandLineArgs arg1, string arg2, string arg3)
        {
            throw new NotImplementedException();
        }

        internal static int QueryDatabases(CommandLineArgs cmdLine)
        {

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.EventHubConnectionString))
            {
                SetEventHubAppenderConnection(cmdLine.BatchArgs.EventHubConnectionString);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            }

            var outpt = Validation.ValidateQueryArguments(ref cmdLine);
            if(outpt != 0)
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

            log.Info($"Running query across {serverCount} servers and {dbCount} databases...");
            bool success = collector.GetQueryResults(ref bg, cmdLine.OutputFile.FullName, SqlSync.SqlBuild.Status.ReportType.CSV, query, cmdLine.DefaultScriptTimeout);

            if (!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.Info("Writing log files to storage...");
                var blobTask = WriteLogsToBlobContainer(cmdLine.BatchArgs.OutputContainerSasUrl, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }


            if (success)
            {
                log.Info($"Query complete. The results are in the output file: {cmdLine.OutputFile.FullName}");
            }
            else
            {
                log.Error("There was an issue collecting and aggregating the query results");
                return 6;
            }

            return 0;

        }
        internal static void ThreadedQuery_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                log.Info(e.UserState);
            }
            else if(e.UserState is QueryCollectionRunnerUpdateEventArgs)
            {
               // var x = (QueryCollectionRunnerUpdateEventArgs)e.UserState;
                //log.Info($"{x.Server}:{x.Database} -- {x.Message}");
            }
        }

        internal static int RunBatchCleanUp(CommandLineArgs cmdLine)
        {
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.CleanUpBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            return retVal;
        }

        internal static int RunBatchPreStage(CommandLineArgs cmdLine)
        {
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.PreStageBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            return retVal;
        }

        internal static int RunBatchExecution(CommandLineArgs cmdLine)
        {
            SetEventHubAppenderConnection(cmdLine.BatchArgs.EventHubConnectionString);
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Batch Execution");
            log.Info("Running...");
            int retVal;
            string readOnlySas;
            (retVal,readOnlySas) =  batchExe.StartBatch();
            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.Info("Completed Successfully");
            }
            else if (retVal == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                log.Info("Datbases already in sync");
                retVal = (int)ExecutionReturn.Successful;
            }
            else
            {
                log.Warn("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            log.Debug("Exiting Batch Execution");

            return retVal;
        }

        internal static int RunBatchQuery(CommandLineArgs cmdLine)
        {
            var outpt = Validation.ValidateQueryArguments(ref cmdLine);
            if (outpt != 0)
            {
                return outpt;
            }

            SetEventHubAppenderConnection(cmdLine.BatchArgs.EventHubConnectionString);
            //Always run the remote Batch as silent or it will get hung up
            if (cmdLine.Silent == false)
            {
                cmdLine.Silent = true;
            }
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine, cmdLine.QueryFile.FullName, Path.Combine(cmdLine.RootLoggingPath, cmdLine.OutputFile.Name));
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Batch Query Execution");
            log.Info("Running...");
            int retVal;
            string readOnlySas;
            
            (retVal,readOnlySas) = batchExe.StartBatch();

            if(!string.IsNullOrWhiteSpace(readOnlySas))
            {
                log.Info("Downloading the consolidated output file...");
                try
                {
                    var cloudBlobContainer = new BlobContainerClient(new Uri(readOnlySas));
                    var blob = cloudBlobContainer.GetBlobClient(cmdLine.OutputFile.Name);
                    blob.DownloadTo(cmdLine.OutputFile.FullName);
                    log.Info($"Output file copied locally to {cmdLine.OutputFile.FullName}");
                }
                catch(Exception exe)
                {
                    log.Error($"Unable to download the output file:  {exe.Message}");
                }
            }

            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.Info("Completed Successfully");
            }
            else
            {
                log.Warn("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            log.Debug("Exiting Batch Execution");

            return retVal;
        }

        internal static void SaveAndEncryptSettings(CommandLineArgs cmdLine, bool clearText)
        {

            if(string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                log.Error("When 'sbm batch savesettings' is specified the --settingsfile argument is also required");
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
                if(File.Exists(cmdLine.SettingsFile) && !cmdLine.Silent)
                {
                    System.Console.WriteLine($"The settings file '{cmdLine.SettingsFile}' already exists. Overwrite (Y/N)?");
                    write = System.Console.ReadLine();
                }

                if (write.ToLower() == "y")
                {
                    File.WriteAllText(cmdLine.SettingsFile, mystuff);
                    log.Info($"Settings file saved to '{cmdLine.SettingsFile}'");
                }
                else
                {
                    log.Info("Settings file not saved");
                }
            }
            catch (Exception exe)
            {
                log.Error($"Unable to save settings file.\r\n{exe.ToString()}");
            }
           
        }

        internal static void ScriptExtraction(CommandLineArgs cmdLine)
        {
            #region Validate flags
            if (string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                log.Error("--platinumdacpac flag is required");
                System.Environment.Exit(-1);
            }
            if (string.IsNullOrWhiteSpace(cmdLine.Database))
            {
                log.Error("--database flag is required");
                System.Environment.Exit(-1);
            }
            if (string.IsNullOrWhiteSpace(cmdLine.Server))
            {
                log.Error("--server flag is required");
                System.Environment.Exit(-1);
            }
            string[] errorMessages;
            int pwVal = Validation.ValidateUserNameAndPassword(ref cmdLine,out errorMessages);
            if(pwVal != 0)
            {
                log.Error(errorMessages.Aggregate((a, b) => a + "; " + b));
            }

            if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                log.Error("--username and --password flags are required");
                System.Environment.Exit(-1);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.OutputSbm))
            {
                log.Error("--outputsbm flag is required");
                System.Environment.Exit(-1);
            }

            if(File.Exists(cmdLine.OutputSbm))
            {
                log.ErrorFormat("The --outputsbm file already exists at {0}. Please choose another name or delete the existing file.", cmdLine.OutputSbm);
                System.Environment.Exit(-1);
            }
            #endregion

            string name;
            cmdLine.RootLoggingPath = Path.GetDirectoryName(cmdLine.OutputSbm);

            var status = DacPacHelper.GetSbmFromDacPac(cmdLine, new SqlSync.SqlBuild.MultiDb.MultiDbData(), out name);
            if(status == DacpacDeltasStatus.Success)
            {
                File.Move(name, cmdLine.OutputSbm);
                log.InfoFormat("SBM package successfully created at {0}", cmdLine.OutputSbm);
            }
            else
            {
                log.ErrorFormat("Error creating SBM package: {0}", status.ToString());
            }
        }

        internal static int RunLocalBuildAsync(CommandLineArgs cmdLine)
        {
            SetEventHubAppenderConnection(cmdLine.BatchArgs.EventHubConnectionString);
            DateTime start = DateTime.Now;
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Local Build Execution");
            log.Info("Running  Local Build...");

            //We need an override setting. if not provided, we need to glean it from the SqlSyncBuildProject.xml file 
            if (string.IsNullOrWhiteSpace(cmdLine.ManualOverRideSets) && !string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                cmdLine.ManualOverRideSets = SqlBuildFileHelper.InferOverridesFromPackage(cmdLine.BuildFileName);
            }

            var ovrRide = $"{cmdLine.Server}:{cmdLine.ManualOverRideSets}";
            if(string.IsNullOrEmpty(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
            var tmpName = Path.Combine(cmdLine.RootLoggingPath, $"Override-{Guid.NewGuid().ToString()}.cfg");
            File.WriteAllText(tmpName, ovrRide);
            cmdLine.Override = tmpName;
            ThreadedExecution runner = new ThreadedExecution(cmdLine);
            int retVal = runner.Execute();
            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.Info("Completed Successfully");
            }
            else
            {
                log.Warn("Completed with Errors - check log");
            }
            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);
            log.Debug("Exiting Single Build Execution");

            return retVal;
        }

        internal static int RunThreadedExecution(CommandLineArgs cmdLine)
        {
            SetEventHubAppenderConnection(cmdLine.BatchArgs.EventHubConnectionString);
            DateTime start = DateTime.Now;
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Threaded Execution");
            log.Debug(cmdLine.ToStringExtension(StringType.Basic));
            log.Debug(cmdLine.ToStringExtension(StringType.BatchQuery));
            log.Debug(cmdLine.ToStringExtension(StringType.BatchThreaded));
            log.Info("Running...");
            ThreadedExecution runner = new ThreadedExecution(cmdLine);
            int retVal = runner.Execute();
            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.Info("Completed Successfully");
            }
            else if (retVal == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                log.Info("Datbases already in sync");
                retVal = (int)ExecutionReturn.Successful;
            }
            else
            {
                log.Warn("Completed with Errors - check log");
            }     

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            if(!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.Info("Writing log files to storage...");
                var blobTask = WriteLogsToBlobContainer(cmdLine.BatchArgs.OutputContainerSasUrl, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }
          
            log.Debug("Exiting Threaded Execution");
  
            return retVal;

        }


        private static async Task<bool> WriteLogsToBlobContainer(string outputContainerSasUrl, string rootLoggingPath)
        {
            try
            {
                //var writeTasks = new List<Task>();
                log.Info($"Writing log files to blob storage at {outputContainerSasUrl}");
                var renameLogFiles = new string[] { "sqlbuildmanager", "csv" };
                BlobContainerClient container = new BlobContainerClient(new Uri(outputContainerSasUrl));
                var fileList = Directory.GetFiles(rootLoggingPath, "*.*", SearchOption.AllDirectories);
                string machine = Environment.MachineName;

                foreach(var f in fileList)
                {
                    try
                    {
                        var tmp = Path.GetRelativePath(rootLoggingPath, f);

                        if (Program.AppendLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {

                            tmp = machine + "-" + tmp;
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open))
                            {
                                await rename.UploadAsync(fs);

                            }


                        }
                        else if (renameLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {
                            tmp = machine + "-" + tmp;
                            var localTemp = Path.Combine(Path.GetDirectoryName(f), tmp);
                            File.Copy(f, localTemp);
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(localTemp, FileMode.Open))
                            {
                                await rename.UploadAsync(fs);

                            }

                        }
                        else
                        {
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var b = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open))
                            {
                                await b.UploadAsync(fs);

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat($"Unable to upload log file '{f}' to blob storage: {e.Message}");
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.Error($"Unable to upload log files to blob storage.{Environment.NewLine}{exe.Message}");
                return false;
            }
        }

        private static void SetWorkingDirectoryLogger(string rootLoggingPath)
        {
   
            try
            {

                if (!string.IsNullOrEmpty(rootLoggingPath))
                {
                    if (!Directory.Exists(rootLoggingPath))
                    {
                        Directory.CreateDirectory(rootLoggingPath);
                    }

                    var appender = LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().Where(a => a.Name == "ThreadedExecutionWorkingAppender" || a.Name == "StandardRollingLogFileAppender");
                    if (appender != null)
                    {
                        foreach(var app in appender)
                        {
                            var thr = app as log4net.Appender.FileAppender;
                            thr.File = Path.Combine(rootLoggingPath, Path.GetFileName(thr.File));
                            thr.ActivateOptions();
                        }
                        
                    }
                }
            }catch(Exception exe)
            {
                log.Error(string.Format("Unable to set local root logging path to {0}", rootLoggingPath), exe);
            }

            
        }

        internal static void SetEventHubAppenderConnection(string connectionString)
        {
            Hierarchy hier = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly()) as Hierarchy;
            if (hier != null)
            {
                var ehAppender = (AzureEventHubAppender)LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().Where(a => a.Name.Contains("AzureEventHubAppender")).FirstOrDefault();
                if (ehAppender != null)
                {
                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        ehAppender.ConnectionString = connectionString;
                        ehAppender.ActivateOptions();
                    }
                }
            }
        }

        #region .: Helper Processes :.
        internal static int CreateDacpac(CommandLineArgs cmdLine)
        {
            string fullName = Path.GetFullPath(cmdLine.DacpacName);
            string path = Path.GetDirectoryName(fullName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!DacPacHelper.ExtractDacPac(cmdLine.Database, cmdLine.Server, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, fullName))
            {
                log.Error($"Error creating the dacpac from {cmdLine.Server} : {cmdLine.Database}");
                return (int)ExecutionReturn.BuildFileExtractionError;
            }
            else
            {
                log.Info($"DACPAC created from {cmdLine.Server} : {cmdLine.Database} saved to -- {fullName}");
            }
            return 0;
        }

        internal static void SyncronizeDatabase(CommandLineArgs cmdLine)
        {
            bool success = Synchronize.SyncDatabases(cmdLine);
            if (success)
                System.Environment.Exit(0);
            else
                System.Environment.Exit(954);
        }

        internal static void GetDifferences(CommandLineArgs cmdLine)
        {
            string history = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            log.Info(history);
            System.Environment.Exit(0);
        }

        internal static void CreateBackout(CommandLineArgs cmdLine)
        {
            string packageName = BackoutCommandLine.CreateBackoutPackage(cmdLine);
            if (!String.IsNullOrEmpty(packageName))
            {
                log.Info(packageName);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(856);
            }
        }

        internal static void GetPackageHash(CommandLineArgs cmdLine)
        {
            if(string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.Error("No --packagename was specified. This is required for 'sbm gethash' command");
                System.Environment.Exit(626);

            }
            string packageName = cmdLine.BuildFileName;
            string hash = SqlBuildFileHelper.CalculateSha1HashFromPackage(packageName);
            if (!String.IsNullOrEmpty(hash))
            {
                log.Info(hash);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(621);
            }
        }

        internal static void ExecutePolicyCheck(CommandLineArgs cmdLine)
        {
            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.Error("No --packagename was specified. This is required for 'sbm policycheck' command");
                System.Environment.Exit(34536);

            }
            string packageName = cmdLine.BuildFileName;
            PolicyHelper helper = new PolicyHelper();
            bool passed;
            List<string> policyMessages = helper.CommandLinePolicyCheck(packageName, out passed);
            if (policyMessages.Count > 0)
            {
                log.Info("Script Policy Messages:");
                foreach (var policyMessage in policyMessages)
                {
                    log.Info(policyMessage);
                }
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
            if (string.IsNullOrWhiteSpace(cmdLine.Directory))
            {
                log.Error("The --directory argument is required for 'sbm package' command");
                System.Environment.Exit(9835);
            }
            string directory = cmdLine.Directory;
            string message;
            List<string> sbmFiles = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directory, out message);
            if (sbmFiles.Count > 0)
            {
                foreach (string sbm in sbmFiles)
                    log.Info(sbm);

                System.Environment.Exit(0);
            }
            else if (message.Length > 0)
            {
                log.Warn(message);
                System.Environment.Exit(604);
            }
            else
            {
                System.Environment.Exit(0);
            }
        }


        #endregion
    }


}
