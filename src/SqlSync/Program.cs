using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SqlSync.SqlBuild;
using System.Collections.Specialized;
using log4net;
using System.Reflection;
namespace SqlSync
{
    class Program
    {
        private static string logFileName = string.Empty;
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static int returnCode = 0;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            SqlBuildManager.Logging.Configure.SetLoggingPath();

            log.Debug("Sql Build Manager Staring...");
            if(args.Length > 0)
                log.Info("Received Command: " + String.Join(" | ", args));

            Application.EnableVisualStyles();
            try
            {
                if (args.Length == 0) //just open the app
                {
                    Application.Run(new SqlBuildForm());
                }
                else if (args.Length == 1) //open the app with the appropriate form based on the file name extension
                {
                    switch (Path.GetExtension(args[0]).ToLower())
                    {
                        case ".sts":
                            Application.Run(new CodeTableScriptingForm(args[0]));
                            break;
                        case ".sbm":
                        case ".sbe":
                        case ".sbx":
                            Application.Run(new SqlBuildForm(args[0]));
                            break;
                        case ".sqlauto":
                            Application.Run(new AutoScript.AutoScriptMDI(args[0], false));
                            break;
                        case ".audit":
                        case ".adt":
                            Application.Run(new DataAuditForm(args[0]));
                            break;
                        case ".sptest":
                            Application.Run(new Test.SprocTestConfigForm(args[0]));
                            break;

                    }
                }
                else if (args.Length > 1) //this is a "real" command line execution...
                {
                    MessageBox.Show("Looks like you are trying to perform a command line operation. Please use \"SqlbuildManager.Console.exe\" instead");
                    returnCode = -1;
                    #region
                    //CommandLineArgs cmdLine = SqlSync.SqlBuild.CommandLine.ParseCommandLineArg(args);
                    //if (cmdLine.LogFileName.Length > 0)
                    //    Program.logFileName = cmdLine.LogFileName;
                    //if (cmdLine.Action == CommandLineArgs.ActionType.Build)
                    //{
                    //    if (cmdLine.OverrideDesignated && cmdLine.MultiDbRunConfigFileName.Length > 0) //build with multi Db config
                    //    {

                    //        SqlBuild.MultiDb.MultiDbData multData = SqlBuild.MultiDb.MultiDbHelper.DeserializeMultiDbConfiguration(cmdLine.MultiDbRunConfigFileName);
                    //        SqlBuildForm buildForm = new SqlBuildForm(cmdLine.BuildFileName, multData, cmdLine.ScriptLogFileName);
                    //        buildForm.UnattendedProcessingCompleteEvent += new UnattendedProcessingCompleteEventHandler(buildForm_UnattendedProcessingCompleteEvent);
                    //        Application.Run(buildForm);
                    //    }
                    //    else if (cmdLine.OverrideDesignated && cmdLine.ManualOverRideSets.Length > 0 && cmdLine.Server.Length > 0) //build with manual override config
                    //    {
                    //        SqlBuildForm buildForm = new SqlBuildForm(cmdLine.BuildFileName, cmdLine.Server, cmdLine.ScriptLogFileName);
                    //        //parse out the overrides which should be in a format /override:default,override;default2,override
                    //        string[] sets = cmdLine.ManualOverRideSets.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    //        OverrideData.TargetDatabaseOverrides = new List<SqlSync.Connection.DatabaseOverride>();
                    //        for (int i = 0; i < sets.Length; i++)
                    //        {
                    //            string[] pair = sets[i].Split(',');
                    //            if (pair.Length == 2)
                    //            {
                    //                Connection.DatabaseOverride overRide = new SqlSync.Connection.DatabaseOverride(pair[0], pair[1]);
                    //                OverrideData.TargetDatabaseOverrides.Add(overRide);
                    //            }
                    //        }
                    //        buildForm.UnattendedProcessingCompleteEvent += new UnattendedProcessingCompleteEventHandler(buildForm_UnattendedProcessingCompleteEvent);
                    //        Application.Run(buildForm);
                    //    }
                    //    else if (cmdLine.Server.Length > 0) //standard build with server designated
                    //    {
                    //        SqlBuildForm buildForm = new SqlBuildForm(cmdLine.BuildFileName, cmdLine.Server, cmdLine.ScriptLogFileName);
                    //        buildForm.UnattendedProcessingCompleteEvent += new UnattendedProcessingCompleteEventHandler(buildForm_UnattendedProcessingCompleteEvent);
                    //        Application.Run(buildForm);
                    //    }
                    //    else
                    //    {
                    //        //If we get here, there are missing required arguments.
                    //        StringBuilder sb = new StringBuilder();
                    //        sb.AppendLine("The build directive (as designated by the '/Action=Build' argument)");
                    //        sb.AppendLine("is missing supporting arguments.\r\n");
                    //        sb.AppendLine("Additional arguments include:");
                    //        sb.AppendLine("'/server=<server name>' to execute on targer server");
                    //        sb.AppendLine("'/override=<.multiDb file>' for pre-config multiple Db run");
                    //        sb.AppendLine("'/override=<manual set> for single override run (w/ a server arg)");
                    //        sb.AppendLine("");
                    //        sb.AppendLine("For additional help see the manual at: www.SqlBuildManager.com");
                    //        Program.WriteLog(sb.ToString());
                    //        returnCode = 654;
                    //    }
                    //}
                    //else if (cmdLine.StoredProcTestingArgs.SprocTestDesignated)
                    //{
                    //    if (cmdLine.Server.Length == 0 || cmdLine.Database.Length == 0 || cmdLine.LogFileName.Length == 0)
                    //    {
                    //        Program.WriteLog("When '/test' is specified, '/server', '/database' and '/log' settings are required");
                    //        returnCode = 543;
                    //    }
                    //    else
                    //    {
                    //        try
                    //        {
                    //            Program.logFileName = string.Empty;
                    //            Connection.ConnectionData cData = new SqlSync.Connection.ConnectionData(cmdLine.Server, cmdLine.Database);
                    //            SprocTest.Configuration.Database testConfig = SqlSync.SprocTest.TestManager.ReadConfiguration(cmdLine.StoredProcTestingArgs.SpTestFile);
                    //            testConfig.SelectAllTests();
                    //            Application.Run(new Test.SprocTestForm(testConfig, cData, cmdLine.StoredProcTestingArgs.SpTestFile, cmdLine.LogFileName));
                    //        }
                    //        catch (Exception exe)
                    //        {
                    //            Program.WriteLog("Error executing Stored Procedure tests.\r\n" + exe.ToString());
                    //            returnCode = 432;
                    //        }
                    //    }
                    //}
                    //else if (cmdLine.AutoScriptingArgs.AutoScriptDesignated)
                    //{
                    //    Application.Run(new AutoScript.AutoScriptMDI(cmdLine.AutoScriptingArgs.AutoScriptFileName));
                    //}
                    #endregion
                }
            }
            catch (Exception exe)
            {
                log.Fatal("Catastrophic Error!!", exe);
                returnCode = 9999;
            }

            log.Debug("Sql Build Manager Exiting with return code: "+returnCode.ToString());
            log4net.LogManager.Shutdown();
            Environment.Exit(returnCode);
            return returnCode;
        }

        public static void WriteLog(string message)
        {
            if (Program.logFileName.Length > 0)
                WriteLog(message,0);

            Console.WriteLine(message);
        }
        private static void WriteLog(string message, int attempt)
        {
            try
            {
                string msg = "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "]\t\t" + message;
                File.AppendAllText(logFileName, msg + "\r\n");
            }
            catch (Exception)
            {
                if (attempt < 5)
                {
                    System.Threading.Thread.Sleep(50);
                    WriteLog(message, attempt + 1);
                }
            }
        }
        private static void buildForm_UnattendedProcessingCompleteEvent(int rtrnCode)
        {
            returnCode = rtrnCode;
            Environment.Exit(returnCode);
        }

    }
}
