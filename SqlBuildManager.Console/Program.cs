using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using log4net;
using System.Reflection;
using SqlSync.SqlBuild;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;
using System.Linq;
namespace SqlBuildManager.Console
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();


            log.Info("Received Command: " + String.Join(" | ", args));
            DateTime start = DateTime.Now;

            string joinedArgs = string.Join(",", args).ToLower();

            if (args.Length == 0 || joinedArgs.Contains("/?") || joinedArgs.Contains("/help"))
            {
                log.Info(Properties.Resources.ConsoleHelp);
                Environment.Exit(0);
            }

            if (joinedArgs.Contains("/remote=true") || joinedArgs.Contains("/remote,")) //Remote execution with all flags
            {
                RunRemoteExecution(args, start);
            }
            else if (args.Length == 1 && args[0].Trim().ToLower().EndsWith(".resp")) //remote execution with single file
            {
                RemoteExecutionWithRespFile(args, start);
            }
            else if (joinedArgs.Contains("/threaded=true") || joinedArgs.Contains("/threaded=\"true\""))
            {
                RunThreadedExecution(args, start);
            }
            else if (args.Length == 2 && args[0].ToLower().Contains("/package"))
            {
                PackageSbxFilesIntoSbmFiles(args);
            }
            else if (args.Length == 2 && args[0].ToLower() == "/policycheck")
            {
                ExecutePolicyCheck(args);
            }
            else if (args.Length == 2 && args[0].ToLower() == "/gethash")
            {
                GetPackageHash(args);
            }
            else if (args.Length > 1 && args[0].ToLower() == "/createbackout")
            {
                CreateBackout(args);
            }
            else if (joinedArgs.Contains("/getdifference"))
            {
                GetDifferences(args);
            }
            else if (joinedArgs.Contains("/synchronize"))
            {
                SyncronizeDatabase(args);
            }
            else
            {
                StandardExecution(args, start);
            }

        }
      
        private static void RunRemoteExecution(string[] args, DateTime start)
        {
            try
            {
                string joinedArgs = string.Join(",", args).ToLower();

                if (joinedArgs.Contains("/testconnectivity=true"))
                {
                    RemoteExecutionTestConnectivity(args);
                }
                else
                {
                    log.Info("Entering Remote Server Execution - command flag option");
                    log.Info("Running remote execution...");
                    RemoteExecution remote = new RemoteExecution(args);

                    int retVal = remote.Execute();
                    if (retVal != 0)
                        log.Warn("Completed with Errors - check log. Exiting with code: " + retVal.ToString());
                    else
                        log.Info("Completed Successfully. Exiting with code: " + retVal.ToString());

                    TimeSpan span = DateTime.Now - start;
                    string msg = "Total Run time: " + span.ToString();
                    log.Info(msg);
                  
                    log.Info("Exiting Remote Execution");
                    System.Environment.Exit(retVal);

                }
            }
            catch (Exception exe)
            {
                log.Warn("Exiting Remote Execution with 603: " + exe.ToString());

                log.Error("Execution error - check logs");
                System.Environment.Exit(603);
            }
        }
        private static void RemoteExecutionTestConnectivity(string[] args)
        {
            log.Info("Entering Remote Server Connectivity Testing: agent and database connectivity");
            log.Info("Entering Remote Server Connectivity Testing...");
            RemoteExecution remote = new RemoteExecution(args);

            int retVal = remote.TestConnectivity();
            if (retVal != 0)
                log.Error(
                    string.Format("Test Connectivity Failed for {0} server/databases. - check log.",
                                  retVal.ToString()));
            else
                log.Info("Test Connectivity Completed Successfully. Exiting with code: " + retVal.ToString());
        }

        private static void RunThreadedExecution(string[] args, DateTime start)
        {
            SetWorkingDirectoryLogger(args);
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
          
            log.Debug("Exiting Threaded Execution");

            System.Environment.Exit(retVal);
        }

        private static void SetWorkingDirectoryLogger(string[] args)
        {
            var cmdLine = CommandLine.ParseCommandLineArg(args);
            if(!string.IsNullOrEmpty(cmdLine.RootLoggingPath))
            {
                if(!cmdLine.RootLoggingPath.EndsWith("\\"))
                {
                    cmdLine.RootLoggingPath = cmdLine.RootLoggingPath + "\\";
                }
                if(!Directory.Exists(cmdLine.RootLoggingPath))
                {
                    Directory.CreateDirectory(cmdLine.RootLoggingPath);
                }

                var appender = LogManager.GetRepository().GetAppenders().Where(a => a.Name == "ThreadedExecutionWorkingAppender").FirstOrDefault();
                if(appender != null)
                {
                    var thr = appender as log4net.Appender.FileAppender;
                    thr.File = cmdLine.RootLoggingPath + Path.GetFileName(thr.File);
                    thr.ActivateOptions();
                }
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

        private static void PackageSbxFilesIntoSbmFiles(string[] args)
        {
            string directory = args[1];
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

        private static void RemoteExecutionWithRespFile(string[] args, DateTime start)
        {
             log.Info("Entering Remote Server Execution - single config file option.");
                try
                {
                    log.Info("Starting Remote Execution...");

                    RemoteExecution remote = new RemoteExecution(args[0]);
                    int retVal = remote.Execute();
                    if (retVal != 0)
                        log.Warn("Completed with Errors - check logs");
                    else
                        log.Info("Completed Successfully");


                    TimeSpan span = DateTime.Now - start;
                    string msg = "Total Run time: " + span.ToString();
                    log.Info(msg);

                    log.Debug("Exiting Remote Execution with " + retVal.ToString());

                    System.Environment.Exit(retVal);
                }
                catch (Exception exe)
                {
                    log.Debug("Exiting Remote Execution with 602: " + exe.ToString());

                    log.Error("Execution error - check logs");
                    System.Environment.Exit(602);
                }
        }


        #region .: Helper Processes :.
        private static void SyncronizeDatabase(string[] args)
        {
            bool success = Synchronize.SyncDatabases(args);
            if (success)
                System.Environment.Exit(0);
            else
                System.Environment.Exit(954);
        }

        private static void GetDifferences(string[] args)
        {
            string history = Synchronize.GetDatabaseRunHistoryDifference(args);
            log.Info(history);
            System.Environment.Exit(0);
        }

        private static void CreateBackout(string[] args)
        {
            string packageName = BackoutCommandLine.CreateBackoutPackage(args);
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

        private static void GetPackageHash(string[] args)
        {
            string packageName = args[1].Trim();
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

        private static void ExecutePolicyCheck(string[] args)
        {
            string packageName = args[1].Trim();
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
