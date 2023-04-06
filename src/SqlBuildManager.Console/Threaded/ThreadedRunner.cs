using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SqlBuildManager.Console.Threaded
{
    class ThreadedRunner
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string defaultDatabaseName = string.Empty;

        /// <summary>
        /// Used for reference, this is the default database name from the target override
        /// </summary>
        public string DefaultDatabaseName
        {
            get { return defaultDatabaseName; }
            set { defaultDatabaseName = value; }
        }
        private CommandLineArgs cmdArgs;
        private int returnValue;

        private string targetTag = string.Empty;
        private string TargetTag
        {
            get
            {
                if (targetTag == string.Empty)
                {
                    if (overrides != null && overrides[0].OverrideDbTarget != null)
                        targetTag = server + "." + overrides[0].OverrideDbTarget;
                    else
                        targetTag = server;
                }
                return targetTag;
            }



        }
        public int ReturnValue
        {
            get { return returnValue; }
        }

        private string server;
        public string Server
        {
            get { return server; }
        }
        private string targetDatabases;
        public string TargetDatabases
        {
            get { return targetDatabases; }
        }
        private List<DatabaseOverride> overrides;

        private bool isTrial;

        public bool IsTrial
        {
            get { return isTrial; }
            set { isTrial = value; }
        }

        private bool isTransactional = true;

        public bool IsTransactional
        {
            get { return isTransactional; }
            set { isTransactional = value; }
        }

        bool forceCustomDacpac = false;
        public bool ForceCustomDacpac
        {
            get { return forceCustomDacpac; }
            set { forceCustomDacpac = value; }
        }
        public string DacpacName { get; set; } = string.Empty;
        private string username = string.Empty;
        private string password = string.Empty;
        private AuthenticationType authType = AuthenticationType.Password;
        private string buildRequestedBy = string.Empty;
        private ThreadedLogging threadedLog = null;
        private string jobName = string.Empty;

        public ThreadedRunner(string serverName, List<DatabaseOverride> overrides, CommandLineArgs cmdArgs, string buildRequestedBy, bool forceCustomDacpac)
        {
            server = serverName;
            this.overrides = overrides;
            isTrial = cmdArgs.Trial;
            this.cmdArgs = cmdArgs;
            this.buildRequestedBy = buildRequestedBy;
            this.forceCustomDacpac = forceCustomDacpac;
            username = cmdArgs.AuthenticationArgs.UserName;
            password = cmdArgs.AuthenticationArgs.Password;
            authType = cmdArgs.AuthenticationArgs.AuthenticationType;
            DacpacName = cmdArgs.DacPacArgs.PlatinumDacpac;


            try
            {
                DefaultDatabaseName = overrides[0].DefaultDbTarget; //Set the default for reference

                for (int i = 0; i < overrides.Count; i++)
                    targetDatabases += overrides[i].OverrideDbTarget + ";";

                if (targetDatabases.Length > 0)
                    targetDatabases = targetDatabases.Substring(0, targetDatabases.Length - 1);
            }
            catch { }

        }
        /// <summary>
        /// Performs the database scripts execution against the specified server/ database settings
        /// </summary>
        /// <param name="serverName">Name of the SQL server to target</param>
        /// <param name="overrides">List of database override settings to use in execution</param>
        /// <returns></returns>
        internal async Task<int> RunDatabaseBuild(ThreadedLogging threadedLog)
        {
            returnValue = (int)RunnerReturn.BuildResultInconclusive;
            this.threadedLog = threadedLog;
            this.jobName = cmdArgs.JobName;
            ConnectionData connData = null;
            BackgroundWorker bg = null;
            DoWorkEventArgs e = null;
            SqlBuildRunData runData = new SqlBuildRunData();
            string targetDatabase = overrides[0].OverrideDbTarget;
            string loggingDirectory = Path.Combine(ThreadedManager.WorkingDirectory, server, targetDatabase);
            try
            {
                //Start setting properties on the object that contains the run configuration data.
                runData.BuildType = "Other";
                if (!string.IsNullOrEmpty(cmdArgs.Description))
                    runData.BuildDescription = cmdArgs.Description;
                else
                    runData.BuildDescription = "Threaded Multi-Database. Run ID:" + ThreadedManager.RunID;

                runData.IsTrial = isTrial;
                runData.RunScriptOnly = false;
                runData.TargetDatabaseOverrides = overrides;
                runData.Server = server;
                runData.IsTransactional = cmdArgs.Transactional;
                if (cmdArgs.LogToDatabaseName.Length > 0)
                    runData.LogToDatabaseName = cmdArgs.LogToDatabaseName;

                runData.PlatinumDacPacFileName = cmdArgs.DacPacArgs.PlatinumDacpac;
                runData.BuildRevision = cmdArgs.BuildRevision;
                runData.DefaultScriptTimeout = cmdArgs.DefaultScriptTimeout;
                runData.AllowObjectDelete = cmdArgs.AllowObjectDelete;


                //Initilize the logging directory for this run
                if (!Directory.Exists(loggingDirectory))
                {
                    Directory.CreateDirectory(loggingDirectory);
                }

                if (forceCustomDacpac)
                {
                    runData.ForceCustomDacpac = true;
                    //This will set the BuildData and BuildFileName and ProjectFileName properties on runData
                    var status = DacPacHelper.UpdateBuildRunDataForDacPacSync(ref runData, server, targetDatabase, authType, username, password, loggingDirectory, cmdArgs.BuildRevision, cmdArgs.DefaultScriptTimeout, cmdArgs.AllowObjectDelete);
                    switch (status)
                    {
                        case DacpacDeltasStatus.Success:
                            //nothing to do
                            break;
                        case DacpacDeltasStatus.InSync:
                        case DacpacDeltasStatus.OnlyPostDeployment:
                            log.LogInformation($"Target database {targetDatabase} is already in sync with {cmdArgs.DacPacArgs.PlatinumDacpac}. Nothing to do!");
                            returnValue = (int)RunnerReturn.DacpacDatabasesInSync;
                            break;
                        default:
                            log.LogError($"Error creating custom dacpac and scripts for {targetDatabase}. No update was performed");
                            returnValue = (int)RunnerReturn.PackageCreationError;
                            return (int)RunnerReturn.PackageCreationError; ;

                    }

                }
                else
                {
                    runData.ForceCustomDacpac = false;
                    //Get a full copy of the build data to work with (avoid threading sync issues)
                    SqlSyncBuildData buildData = new SqlSyncBuildData();

                    string xml = "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n" + ThreadedManager.BuildData.GetXml();
                    using (StringReader sr = new StringReader(xml))
                    {
                        buildData.ReadXml(sr);
                    }
                    //Clear out any existing ComittedScript data.. just log what is relevent to this run.
                    buildData.CommittedScript.Clear();


                    runData.BuildData = buildData;
                    runData.ProjectFileName = Path.Combine(loggingDirectory, Path.GetFileName(ThreadedManager.ProjectFileName));
                    runData.BuildFileName = ThreadedManager.BuildZipFileName;
                }



                //Create a connection object.. all we need is the server here, the DB will be filled in at execution time
                connData = new ConnectionData(server, "");
                if (cmdArgs.AuthenticationArgs.UserName.Length > 0 && cmdArgs.AuthenticationArgs.Password.Length > 0)
                {
                    connData.UserId = cmdArgs.AuthenticationArgs.UserName;
                    connData.Password = cmdArgs.AuthenticationArgs.Password;
                }
                connData.AuthenticationType = cmdArgs.AuthenticationArgs.AuthenticationType;

                //Set the log file name
                string logFile = Path.Combine(loggingDirectory, "ExecutionLog.log");

                //Create the objects that will handle the event communication back.
                bg = new BackgroundWorker();
                //bg.ProgressChanged += Bg_ProgressChanged;
                bg.WorkerReportsProgress = true;
                e = new DoWorkEventArgs(null);
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Error Initializing run for {TargetTag}");
                WriteErrorLog(loggingDirectory, exe.ToString());
                returnValue = (int)ExecutionReturn.RunInitializationError;
                return (int)ExecutionReturn.RunInitializationError; ;
            }

            log.LogDebug("Initializing run for " + TargetTag + ". Starting \"ProcessBuild\"");

            try
            {
                //Initilize the run helper object and kick it off.
                SqlBuildHelper helper = new SqlBuildHelper(connData, true, string.Empty, cmdArgs.Transactional); //don't need an "external" log for this, it's all external!
                helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
                helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
                helper.BuildSuccessTrialRolledBackEvent += new EventHandler(helper_BuildSuccessTrialRolledBackEvent);

                //Determine whether or not to log each script result to EventhHub
                if (cmdArgs.EventHubLogging.Contains(EventHubLogging.ConsolidatedScriptResults) || cmdArgs.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults))
                {
                    helper.ScriptLogWriteEvent += new ScriptLogWriteEventHandler(helper_ScriptLogWriteEvent);
                }

                await Task.Run(() =>
                {
                    helper.ProcessBuild(runData, bg, e, ThreadedManager.BatchColl, buildRequestedBy, cmdArgs.TimeoutRetryCount);
                });
            }
            catch (Exception exe)
            {
                log.LogError("Error Processing run for " + TargetTag, exe);
                WriteErrorLog(loggingDirectory, exe.ToString());
                returnValue = (int)ExecutionReturn.ProcessBuildError;
                return (int)ExecutionReturn.ProcessBuildError; ;
            }
            finally
            {
                if (cmdArgs.EventHubLogging.Contains(EventHubLogging.ConsolidatedScriptResults))
                {
                    LogMsg lm = new LogMsg()
                    {
                        LogType = LogType.ScriptLog,
                        DatabaseName = targetDb,
                        JobName = this.jobName,
                        ServerName = this.server,
                        Message = consolidatedScriptLog.ToString()
                       

                    };
                    threadedLog.WriteToLog(lm);
                }
            }
            return 0;

        }

        private StringBuilder consolidatedScriptLog = new();
        private string targetDb = string.Empty;
        private void helper_ScriptLogWriteEvent(object sender, ScriptLogEventArgs e)
        {
            if (cmdArgs.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults))
            {
                LogMsg lm = new LogMsg()
                {
                    LogType = LogType.ScriptLog,
                    DatabaseName = e.Database,
                    JobName = this.jobName,
                    ServerName = this.server,
                    Message = "ScriptLog",
                    ScriptLog = new ScriptLogData()
                    {
                        ScriptFileName = e.SourceFile,
                        ScriptText = e.SqlScript,
                        ScriptIndex = e.ScriptIndex,
                        Result = e.Results
                    }

                };
                threadedLog.WriteToLog(lm);
            }
            else if (cmdArgs.EventHubLogging.Contains(EventHubLogging.ConsolidatedScriptResults))
            {
                targetDb = e.Database;
                if (consolidatedScriptLog.Length == 0)
                {
                    consolidatedScriptLog.AppendLine($"-- Start Time: {DateTime.Now.ToString()} --");
                }

                consolidatedScriptLog.AppendLine("/************************************");
                consolidatedScriptLog.AppendLine("Script #" + e.ScriptIndex.ToString() + "; Source File: " + e.SourceFile);
                consolidatedScriptLog.AppendLine($"Server: {this.server}; Run On Database: {e.Database} */");
                if (e.Database.Length > 0)
                {
                    consolidatedScriptLog.AppendLine($"use {e.Database}");
                    consolidatedScriptLog.AppendLine($"GO");
                }
                consolidatedScriptLog.AppendLine(e.SqlScript);
                consolidatedScriptLog.AppendLine($"GO"); 
                consolidatedScriptLog.AppendLine($"/*Script #{e.ScriptIndex.ToString()} Result: { e.Results.Trim()}  */");


                if (e.ScriptIndex == -10000)
                {
                    consolidatedScriptLog.AppendLine("-- END Time: " + DateTime.Now.ToString() + " --");
                }
            }
        }

        void helper_BuildSuccessTrialRolledBackEvent(object sender, EventArgs e)
        {
            log.LogDebug(TargetTag + " BuildSuccessTrialRolledBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.SuccessWithTrialRolledBack));
            returnValue = (int)RunnerReturn.SuccessWithTrialRolledBack;
        }

        void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            if (IsTransactional)
            {
                log.LogDebug(TargetTag + " BuildErrorRollBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.RolledBack));
                returnValue = (int)RunnerReturn.RolledBack;
            }
            else
            {
                log.LogDebug(TargetTag + " BuildErrorRollBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.BuildErrorNonTransactional));
                returnValue = (int)RunnerReturn.BuildErrorNonTransactional;
            }
        }

        void helper_BuildCommittedEvent(object sender, RunnerReturn returnValue)
        {
            log.LogDebug(TargetTag + " BuildCommittedEvent status: " + Enum.GetName(typeof(RunnerReturn), returnValue));
            this.returnValue = (int)returnValue;
        }

        private void WriteErrorLog(string loggingDirectory, string message)
        {
            File.WriteAllText(loggingDirectory + "Error.log", message);
        }

    }
}
