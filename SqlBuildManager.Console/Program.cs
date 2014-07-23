﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using log4net;
using System.Reflection;
using SqlSync.SqlBuild;
using SqlBuildManager.Enterprise.Policy;
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
                System.Console.WriteLine(Properties.Resources.ConsoleHelp);
                Environment.Exit(0);
            }

            if (joinedArgs.Contains("/remote=true")) //Remote execution with all flags
            {
                try
                {
                    if (joinedArgs.Contains("/testconnectivity=true"))
                    {
                        log.Info("Entering Remote Server Connectivity Testing: agent and database connectivity");
                        System.Console.WriteLine("Entering Remote Server Connectivity Testing...");
                        RemoteExecution remote = new RemoteExecution(args);

                        int retVal = remote.TestConnectivity();
                        if (retVal != 0)
                            System.Console.WriteLine(
                                string.Format("Test Connectivity Failed for {0} server/databases. - check log.",
                                              retVal.ToString()));
                        else
                            System.Console.WriteLine("Test Connectivity Completed Successfully. Exiting with code: " + retVal.ToString());
                    }
                    else
                    {
                        log.Info("Entering Remote Server Execution - command flag option");
                        System.Console.WriteLine("Running remote execution...");
                        RemoteExecution remote = new RemoteExecution(args);

                        int retVal = remote.Execute();
                        if (retVal != 0)
                            System.Console.WriteLine("Completed with Errors - check log. Exiting with code: " + retVal.ToString());
                        else
                            System.Console.WriteLine("Completed Successfully. Exiting with code: " + retVal.ToString());

                        TimeSpan span = DateTime.Now - start;
                        string msg = "Total Run time: " + span.ToString();
                        System.Console.WriteLine(msg);
                        log.Debug(msg);

                        log.Info("Exiting Remote Execution");
                        System.Environment.Exit(retVal);

                    }
                   

                }
                catch (Exception exe)
                {
                    log.Warn("Exiting Remote Execution with 603: " + exe.ToString());

                    System.Console.WriteLine("Execution error - check logs");
                    System.Environment.Exit(603);
                }

                
            }
            else if (args.Length == 1 && args[0].Trim().ToLower().EndsWith(".resp")) //remote execution with single file
            {
                log.Info("Entering Remote Server Execution - single config file option.");
                try
                {
                    System.Console.WriteLine("Starting Remote Execution...");

                    RemoteExecution remote = new RemoteExecution(args[0]);
                    int retVal = remote.Execute();
                    if (retVal != 0)
                        System.Console.WriteLine("Completed with Errors - check logs");
                    else
                        System.Console.WriteLine("Completed Successfully");


                    TimeSpan span = DateTime.Now - start;
                    string msg = "Total Run time: " + span.ToString();
                    System.Console.WriteLine(msg);
                    log.Debug(msg);


                    log.Debug("Exiting Remote Execution with " + retVal.ToString());

                    System.Environment.Exit(retVal);
                }
                catch (Exception exe)
                {
                    log.Debug("Exiting Remote Execution with 602: " + exe.ToString());

                    System.Console.WriteLine("Execution error - check logs");
                    System.Environment.Exit(602);
                }
            }
            else if (joinedArgs.Contains("/threaded=true") || joinedArgs.Contains("/threaded=\"true\""))
            {
                log.Debug("Entering Threaded Execution");
                System.Console.WriteLine("Running...");
                ThreadedExecution runner = new ThreadedExecution(args);
                int retVal = runner.Execute();
                if (retVal != 0)
                    System.Console.WriteLine("Completed with Errors - check log");
                else
                    System.Console.WriteLine("Completed Successfully");

                TimeSpan span = DateTime.Now - start;
                string msg = "Total Run time: " + span.ToString();
                System.Console.WriteLine(msg);
                log.Debug(msg);

                log.Debug("Exiting Threaded Execution");

                System.Environment.Exit(retVal);
            }
            else if (args.Length == 2 && args[0].ToLower().Contains("/package"))
            {
                string directory = args[1];
                string message;
                List<string> sbmFiles = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directory, out message);
                if (sbmFiles.Count > 0)
                {
                    foreach(string sbm in sbmFiles)
                        System.Console.WriteLine(sbm);
                    
                    System.Environment.Exit(0);
                }
                else if (message.Length > 0)
                {
                    System.Console.WriteLine(message);
                    System.Environment.Exit(604);
                }
                else
                {
                    System.Environment.Exit(0);
                }
            }
            else if (args.Length == 2 && args[0].ToLower() == "/policycheck")
            {
                string packageName = args[1].Trim();
                PolicyHelper helper = new PolicyHelper();
                bool passed;
                List<string> policyMessages = helper.CommandLinePolicyCheck(packageName, out passed);
                if (policyMessages.Count > 0)
                {
                    System.Console.WriteLine("Script Policy Messages:");
                    foreach (var policyMessage in policyMessages)
                    {
                        System.Console.WriteLine(policyMessage);
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
            else if (args.Length == 2 && args[0].ToLower() == "/gethash")
            {
                string packageName = args[1].Trim();
                string hash = SqlBuildFileHelper.CalculateSha1HashFromPackage(packageName);
                if (!String.IsNullOrEmpty(hash))
                {
                    System.Console.WriteLine(hash);
                    System.Environment.Exit(0);
                }
                else
                {
                    System.Environment.Exit(621);
                }
            }
            else if (args.Length > 1 && args[0].ToLower() == "/createbackout")
            {
                string packageName =  BackoutCommandLine.CreateBackoutPackage(args);
                if (!String.IsNullOrEmpty(packageName))
                {
                    System.Console.WriteLine(packageName);
                    System.Environment.Exit(0);
                }
                else
                {
                    System.Environment.Exit(856);
                }
            }
            else if (joinedArgs.Contains("/getdifference"))
            {
                
            }
            else if (joinedArgs.Contains("/synchronize"))
            {
                
            }
            else
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
                    System.Console.Error.WriteLine(prcHelper.Output);

                TimeSpan span = DateTime.Now - start;
                string msg = "Total Run time: " + span.ToString();
                System.Console.WriteLine(msg);
                log.Debug(msg);



                log.Debug("Exiting Standard Execution");
                log4net.LogManager.Shutdown();
                System.Environment.Exit(exitCode);
            }

        }
    }
}
