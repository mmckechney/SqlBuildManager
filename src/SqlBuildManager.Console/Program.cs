using log4net;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;

using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{

    class Program
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static string[] AppendLogFiles = new string[] { "commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg" };

        static async Task Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            SqlBuildManager.Logging.Configure.SetLoggingPath();

            log.Debug("Received Command: " + String.Join(" | ", args));
            DateTime start = DateTime.Now;
            int retVal = 0;
            try
            {
                string joinedArgs = string.Join(",", args).ToLower();
                string[] helpRequest = new string[] {"/?", "-?", "--?", "-h", "-help", "/help", "--help", "--h", "/h" };
               
                var cmdLine = CommandLine.ParseCommandLineArg(args);

                if (args.Length == 0 || helpRequest.Any(h => joinedArgs.Contains(h))) 
                {
                    log.Info(Properties.Resources.ConsoleHelp2);
                    Environment.Exit(0);
                }
                switch (cmdLine.Action)
                {
                    //case CommandLineArgs.ActionType.Remote:
                    //    RunRemoteExecution(args, cmdLine, start);
                    //    break;
                    case CommandLineArgs.ActionType.Threaded:
                        retVal = await RunThreadedExecutionAsync(args, cmdLine, start);
                        break;
                    case CommandLineArgs.ActionType.Package:
                        PackageSbxFilesIntoSbmFiles(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.PolicyCheck:
                        ExecutePolicyCheck(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.GetHash:
                        GetPackageHash(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.CreateBackout:
                        CreateBackout(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.GetDifference:
                        GetDifferences(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.Synchronize:
                        SyncronizeDatabase(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.Build:
                        StandardExecution(args, start);
                        break;
                    case CommandLineArgs.ActionType.ScriptExtract:
                        ScriptExtraction(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.SaveSettings:
                        SaveAndEncryptSettings(cmdLine);
                        break;
                    case CommandLineArgs.ActionType.Batch:
                        retVal = await RunBatchExecution(cmdLine, start);
                        break;
                    case CommandLineArgs.ActionType.BatchPreStage:
                        retVal = await RunBatchPreStage(cmdLine, start);
                        break;
                    case CommandLineArgs.ActionType.BatchCleanUp:
                        retVal = await RunBatchCleanUp(cmdLine, start);
                        break;
                    default:
                        log.Error("A valid /Action arument was not found. Please check the help documentation for valid settings (/help or /?)");
                        System.Environment.Exit(8675309);
                        break;

                }

                LogManager.Flush(10000);
                System.Environment.Exit(retVal);
            }
            catch(Exception exe)
            {
                log.ErrorFormat($"Something went wrong!\r\n{exe.ToString()}");
            }

        }

        private static async Task<int> RunBatchCleanUp(CommandLineArgs cmdLine, DateTime start)
        {
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.CleanUpBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            return retVal;
        }

        private static async Task<int> RunBatchPreStage(CommandLineArgs cmdLine, DateTime start)
        {
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.PreStageBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            return retVal;
        }

        private static async Task<int> RunBatchExecution(CommandLineArgs cmdLine, DateTime start)
        {
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Batch Execution");
            log.Info("Running...");
            int retVal =  batchExe.StartBatch();
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

        private static void SaveAndEncryptSettings(CommandLineArgs cmdLine)
        {

            if(string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                log.Error("When /Action=SaveSettings is specified the /SettingsFile argument is also required");
            }

            cmdLine = Cryptography.EncryptSensitiveFields(cmdLine);
            var mystuff = JsonConvert.SerializeObject(cmdLine, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            try
            {
                string write = "y";
                if(File.Exists(cmdLine.SettingsFile))
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

        private static void ScriptExtraction(CommandLineArgs cmdLine)
        {
            #region Validate flags
            if (string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                log.Error("/PlatinumDacpac flag is required");
                System.Environment.Exit(-1);
            }
            if (string.IsNullOrWhiteSpace(cmdLine.Database))
            {
                log.Error("/Database flag is required");
                System.Environment.Exit(-1);
            }
            if (string.IsNullOrWhiteSpace(cmdLine.Server))
            {
                log.Error("/Server flag is required");
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
                log.Error("/Username and /Password flags are required");
                System.Environment.Exit(-1);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.OutputSbm))
            {
                log.Error("/OutputSbm flag is required");
                System.Environment.Exit(-1);
            }

            if(File.Exists(cmdLine.OutputSbm))
            {
                log.ErrorFormat("The /OutputSbm file already exists at {0}. Please choose another name or delete the existing file.",cmdLine.OutputSbm);
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


        #region Deprecated -- Remote Exectution via Azure Cloud Service deployment
        //private static void RunRemoteExecution(string[] args, CommandLineArgs cmdLine, DateTime start)
        //{
        //    try
        //    {
        //        //string joinedArgs = string.Join(",", args).ToLower();

        //        if(cmdLine.RemoteArgs.TestConnectivity == true)
        //        {
        //            RemoteExecutionTestConnectivity(args);
        //        }
        //        else if(cmdLine.RemoteArgs.AzureRemoteStatus == true)
        //        {
        //            GetAzureRemoteStatus(args);
        //        }
        //        else if (!string.IsNullOrWhiteSpace(cmdLine.RemoteArgs.RemoteDbErrorList))
        //        {
        //            var dbsInError = RemoteAzureHealth.GetDatabaseErrorList(cmdLine.RemoteArgs.RemoteDbErrorList);
        //           if (dbsInError != null)
        //           {
        //               log.Info("\r\n" + string.Join("\r\n", dbsInError.ToArray()));
        //           }
        //        }
        //        else if (!string.IsNullOrWhiteSpace(cmdLine.RemoteArgs.RemoteErrorDetail))
        //        {
        //            var errorMessages = RemoteAzureHealth.GetErrorDetail(cmdLine.RemoteArgs.RemoteErrorDetail);
        //            log.Info("Returned error messages:");
        //            log.Info("\r\n" + errorMessages);
        //        }
        //        else if(cmdLine.DacPacArgs.ForceCustomDacPac == true)
        //        {
        //            log.Error("The /ForceCustomDacPac flag is not compatible with the /Action=Remote action");
        //            System.Environment.Exit(681);
        //        }
        //        else
        //        {


        //            log.Info("Entering Remote Server Execution - command flag option");
        //            log.Info("Running remote execution...");
        //            RemoteExecution remote = new RemoteExecution(args);

        //            int retVal = remote.Execute();
        //            if (retVal != 0)
        //                log.Warn("Completed with Errors - check log. Exiting with code: " + retVal.ToString());
        //            else
        //                log.Info("Completed Successfully. Exiting with code: " + retVal.ToString());

        //            TimeSpan span = DateTime.Now - start;
        //            string msg = "Total Run time: " + span.ToString();
        //            log.Info(msg);

        //            log.Info("Exiting Remote Execution");
        //            System.Environment.Exit(retVal);

        //        }
        //    }
        //    catch (Exception exe)
        //    {
        //        log.Warn("Exiting Remote Execution with 603: " + exe.ToString());

        //        log.Error("Execution error - check logs");
        //        System.Environment.Exit(603);
        //    }
        //}

        //#region .: Remote Health Check :.
        //private static void RemoteExecutionTestConnectivity(string[] args)
        //{
        //    log.Info("Entering Remote Server Connectivity Testing: agent and database connectivity");
        //    log.Info("Entering Remote Server Connectivity Testing...");
        //    RemoteExecution remote = new RemoteExecution(args);

        //    int retVal = remote.TestConnectivity();
        //    if (retVal != 0)
        //        log.Error(
        //            string.Format("Test Connectivity Failed for {0} server/databases. - check log.",
        //                          retVal.ToString()));
        //    else
        //        log.Info("Test Connectivity Completed Successfully. Exiting with code: " + retVal.ToString());
        //}
        //private static void GetAzureRemoteStatus(string[] args)
        //{
        //    try
        //    {
        //        string format = "{0}{1}{2}{3}";
        //        log.Info("Getting list of Azure instances...");
        //        BuildServiceManager manager = new BuildServiceManager();
        //        List<ServerConfigData> serverData = manager.GetListOfAzureInstancePublicUrls();
        //        var remote = serverData.Select(s => s.ServerName).ToList();
        //        if (remote.Count() > 0)
        //        {
        //            log.InfoFormat("{0} instances available at {1}", remote.Count(), Regex.Replace(serverData[0].ActiveServiceEndpoint, @":\d{5}", ""));
        //        }
        //        List<ServerConfigData> remoteServer = null;
        //        string[] errorMessages;
        //        log.Info("Retrieving status of each instance...");



        //        int statReturn = RemoteExecution.ValidateRemoteServerAvailability(remote, Protocol.AzureHttp, out remoteServer, out errorMessages);

        //        int serverPad = remoteServer.Max(s => s.ServerName.Length) + 2;
        //        int statusPad = remoteServer.Max(s => s.ServiceReadiness.ToString().Length) + 2;
        //        int exePad = remoteServer.Max(s => s.ExecutionReturn.ToString().Length) + 2;
        //        if (exePad < "Last Status".Length + 2)
        //            exePad = "Last Status".Length + 2;
        //        int versionPad = remoteServer.Max(s => s.ServiceVersion.ToString().Length) + 2;

        //        log.InfoFormat(format, "Service".PadRight(serverPad, ' '), "Status".PadRight(statusPad, ' '), "Last Status".PadRight(exePad, ' '), "Version".PadRight(versionPad, ' '));
        //        log.InfoFormat(format, "-".PadRight(serverPad-2, '-'), "-".PadRight(statusPad-2, '-'), "-".PadRight(exePad-2, '-'), "--".PadRight(versionPad-2, '-'));
        //        remoteServer.ForEach(s =>
        //            log.InfoFormat(format, s.ServerName.PadRight(serverPad, ' '), s.ServiceReadiness.ToString().PadRight(statusPad, ' '), s.ExecutionReturn.ToString().PadRight(exePad, ' '), s.ServiceVersion.PadRight(versionPad, ' ')));

        //        if(errorMessages.Length > 0)
        //        {
        //            errorMessages.ToList().ForEach(e => log.Error(e));
        //        }

        //    }
        //    catch (Exception exe)
        //    {
        //        log.Error("Unable to get list of Azure instances", exe);
        //    }
        //}

        //#endregion

        //private static void RemoteExecutionWithRespFile(string[] args, DateTime start)
        //{
        //    log.Info("Entering Remote Server Execution - single config file option.");
        //    try
        //    {
        //        log.Info("Starting Remote Execution...");

        //        RemoteExecution remote = new RemoteExecution(args[0]);
        //        int retVal = remote.Execute();
        //        if (retVal != 0)
        //            log.Warn("Completed with Errors - check logs");
        //        else
        //            log.Info("Completed Successfully");


        //        TimeSpan span = DateTime.Now - start;
        //        string msg = "Total Run time: " + span.ToString();
        //        log.Info(msg);

        //        log.Debug("Exiting Remote Execution with " + retVal.ToString());

        //        System.Environment.Exit(retVal);
        //    }
        //    catch (Exception exe)
        //    {
        //        log.Debug("Exiting Remote Execution with 602: " + exe.ToString());

        //        log.Error("Execution error - check logs");
        //        System.Environment.Exit(602);
        //    }
        //}
        #endregion

        private static async Task<int> RunThreadedExecutionAsync(string[] args, CommandLineArgs cmdLine, DateTime start)
        {
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Threaded Execution");
            log.Info("Running...");
            ThreadedExecution runner = new ThreadedExecution(args);
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
                bool success = await WriteLogsToBlobContainer(cmdLine.BatchArgs.OutputContainerSasUrl, cmdLine.RootLoggingPath);
            }
          
            log.Debug("Exiting Threaded Execution");
  
            return retVal;

        }

        private static async Task<bool> WriteLogsToBlobContainer(string outputContainerSasUrl, string rootLoggingPath)
        {
            try
            {
                var writeTasks = new List<Task>();
                
                var renameLogFiles = new string[] { "execution.log" };
                CloudBlobContainer container = new CloudBlobContainer(new Uri(outputContainerSasUrl));
                var fileList = Directory.GetFiles(rootLoggingPath, "*.*", SearchOption.AllDirectories);
                string machine = Environment.MachineName;
                fileList.ToList().ForEach(f =>
                {
                    try
                    {
                        var tmp = f.Replace(rootLoggingPath + @"\", "");

                        if (Program.AppendLogFiles.Any(a => f.ToLower().IndexOf(a) > -1))
                        {

                            tmp = machine + "-" + tmp;
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobReference(tmp);

                            writeTasks.Add(
                                rename.UploadFromFileAsync(f,
                                AccessCondition.GenerateIfNotExistsCondition(),
                                new BlobRequestOptions() { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 20) }, null));

                        }
                        else if (renameLogFiles.Any(a => f.ToLower().IndexOf(a) > -1))
                        {
                            tmp = machine + "-" + tmp;
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobReference(tmp);

                            writeTasks.Add(
                                rename.UploadFromFileAsync(f,
                                AccessCondition.GenerateIfNotExistsCondition(),
                                new BlobRequestOptions() { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 20) }, null));
                        }
                        else
                        {
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var b = container.GetBlockBlobReference(tmp);
                            writeTasks.Add(
                                b.UploadFromFileAsync(f,
                                AccessCondition.GenerateIfNotExistsCondition(),
                                new BlobRequestOptions() { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 20) }, null));
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat($"Unable to upload log file '{f}' to blob storage: {e.ToString()}");
                    }
                });

                await Task.WhenAll(writeTasks);
                return true;
            }
            catch (Exception exe)
            {
                log.Error("Unable to upload log files to blob storage", exe);
                return false;
            }
        }

        private static void SetWorkingDirectoryLogger(string rootLoggingPath)
        {
   
            try
            {

                if (!string.IsNullOrEmpty(rootLoggingPath))
                {
                    //if (!cmdLine.RootLoggingPath.EndsWith("\\"))
                    //{
                    //    cmdLine.RootLoggingPath = cmdLine.RootLoggingPath + "\\";
                    //}
                    if (!Directory.Exists(rootLoggingPath))
                    {
                        Directory.CreateDirectory(rootLoggingPath);
                    }

                    var appender = LogManager.GetRepository().GetAppenders().Where(a => a.Name == "ThreadedExecutionWorkingAppender").FirstOrDefault();
                    if (appender != null)
                    {
                        var thr = appender as log4net.Appender.FileAppender;
                        thr.File = Path.Combine(rootLoggingPath, Path.GetFileName(thr.File));
                        thr.ActivateOptions();
                    }
                }
            }catch(Exception exe)
            {
                log.Error(string.Format("Unable to set local root logging path to {0}", rootLoggingPath), exe);
            }

            
        }

        private static void StandardExecution(string[] args, DateTime start)
        {
            log.Debug("Entering Standard Execution");

            //Get the path of the Sql Build Manager executable - need to be co-resident
            string sbmExe = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                            @"\Sql Build Manager.exe";

            //Put any arguments that have spaces into quotes
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].IndexOf(" ") > -1)
                    args[i] = "\"" + args[i] + "\"";
            }
            //Rejoin the args to pass over to Sql Build Manager
            string arg = String.Join(" ", args);
            ProcessHelper prcHelper = new ProcessHelper();
            int exitCode = prcHelper.ExecuteProcess(sbmExe, arg);

            //Send the console output to the console window
            if (prcHelper.Output.Length > 0)
                System.Console.Out.WriteLine(prcHelper.Output);

            //Send the error output to the error stream
            if (prcHelper.Error.Length > 0)
                log.Error(prcHelper.Output);

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            log.Debug("Exiting Standard Execution");
            log4net.LogManager.Shutdown();
            System.Environment.Exit(exitCode);
        }

        private static void PackageSbxFilesIntoSbmFiles(CommandLineArgs cmdLine)
        {
            if(string.IsNullOrWhiteSpace(cmdLine.Directory))
            {
                log.Error("The /Directory argument is required for /Action=Package");
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
                log.Warn (message);
                System.Environment.Exit(604);
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

   


        #region .: Helper Processes :.
        private static void SyncronizeDatabase(CommandLineArgs cmdLine)
        {
            bool success = Synchronize.SyncDatabases(cmdLine);
            if (success)
                System.Environment.Exit(0);
            else
                System.Environment.Exit(954);
        }

        private static void GetDifferences(CommandLineArgs cmdLine)
        {
            string history = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            log.Info(history);
            System.Environment.Exit(0);
        }

        private static void CreateBackout(CommandLineArgs cmdLine)
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

        private static void GetPackageHash(CommandLineArgs cmdLine)
        {
            if(string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.Error("No /PackageName was specified. This is required for /Action=GetHash");
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

        private static void ExecutePolicyCheck(CommandLineArgs cmdLine)
        {
            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.Error("No /PackageName was specified. This is required for /Action=PolicyCheck");
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
        #endregion
    }


}
