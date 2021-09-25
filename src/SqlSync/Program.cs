using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SqlSync.SqlBuild;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;
using System.Reflection;
namespace SqlSync
{
    class Program
    {
         private static ILogger log;
        public static int returnCode = 0;
        private static readonly string applicationLogFileName = "SqlBuildManager.log"; 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Program>(applicationLogFileName);

            log.LogDebug("Sql Build Manager Staring...");
            if(args.Length > 0)
                log.LogInformation("Received Command: " + String.Join(" | ", args));

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
                    MessageBox.Show("Looks like you are trying to perform a command line operation. Please use \"sbm.exe\" instead");
                    returnCode = -1;
                   
                }
            }
            catch (Exception exe)
            {
                log.LogCritical(exe, "Catastrophic Error!!");
                returnCode = 9999;
            }

            log.LogDebug("Sql Build Manager Exiting with return code: "+returnCode.ToString());
            SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
            Environment.Exit(returnCode);
            return returnCode;
        }


        private static void buildForm_UnattendedProcessingCompleteEvent(int rtrnCode)
        {
            returnCode = rtrnCode;
            Environment.Exit(returnCode);
        }

    }
}
