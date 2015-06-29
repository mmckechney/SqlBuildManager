using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System.IO;
using SqlSync.Connection;
using SqlSync.DbInformation;
using System.ComponentModel;
using SqlBuildManager.Interfaces.Console;
namespace SqlBuildManager.Console
{
    class ThreadedRunner
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    if (this.overrides != null && this.overrides[0].OverrideDbTarget != null)
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

        private string username = string.Empty;
        private string password = string.Empty;

        private string buildRequestedBy = string.Empty;

        public ThreadedRunner(string serverName, List<DatabaseOverride> overrides, CommandLineArgs cmdArgs, string buildRequestedBy, bool forceCustomDacpac)
        {
            this.server = serverName;
            this.overrides = overrides;
            this.isTrial = cmdArgs.Trial;
            this.cmdArgs = cmdArgs;
            this.buildRequestedBy = buildRequestedBy;
            this.forceCustomDacpac = forceCustomDacpac;
            this.username = cmdArgs.UserName;
            this.password = cmdArgs.Password;

            try
            {
                this.DefaultDatabaseName = overrides[0].DefaultDbTarget; //Set the default for reference

                for (int i = 0; i < overrides.Count; i++)
                    targetDatabases += overrides[i].OverrideDbTarget + ";";

                if (this.targetDatabases.Length > 0)
                    this.targetDatabases = this.targetDatabases.Substring(0, this.targetDatabases.Length - 1);
            }
            catch { }

        }
        /// <summary>
        /// Performs the database scripts execution against the specified server/ database settings
        /// </summary>
        /// <param name="serverName">Name of the SQL server to target</param>
        /// <param name="overrides">List of database override settings to use in execution</param>
        /// <returns></returns>
        internal void RunDatabaseBuild()
        {
            this.returnValue = (int)RunnerReturn.BuildResultInconclusive;

            ConnectionData connData = null;
            BackgroundWorker bg = null;
            DoWorkEventArgs e = null;
            SqlBuildRunData runData = new SqlBuildRunData();
            string targetDatabase = overrides[0].OverrideDbTarget;
            string loggingDirectory = ThreadedExecution.RootLoggingPath + @"\" + server + @"\" + targetDatabase + @"\";
            try
            {
                 //Start setting properties on the object that contains the run configuration data.
                runData.BuildType = "Other";
                if (!string.IsNullOrEmpty(cmdArgs.Description))
                    runData.BuildDescription = cmdArgs.Description;
                else
                    runData.BuildDescription = "Threaded Multi-Database. Run ID:" + ThreadedExecution.RunID;
                
                runData.IsTrial = this.isTrial;
                runData.RunScriptOnly = false;
                runData.TargetDatabaseOverrides = overrides;
                runData.Server = this.server;
                runData.IsTransactional = cmdArgs.Transactional;
                if (this.cmdArgs.LogToDatabaseName.Length > 0)
                    runData.LogToDatabaseName = this.cmdArgs.LogToDatabaseName;

                runData.PlatinumDacPacFileName = cmdArgs.PlatinumDacpac;
                runData.BuildRevision = cmdArgs.BuildRevision;


                //Initilize the logging directory for this run
                if (!Directory.Exists(loggingDirectory))
                    Directory.CreateDirectory(loggingDirectory);

                if (forceCustomDacpac)
                {
                    runData.ForceCustomDacpac = true;
                    //This will set the BuildData and BuildFileName and ProjectFileName properties on runData
                    var status = DacPacHelper.UpdateBuildRunDataForDacPacSync(ref runData, server, targetDatabase, this.username, this.password, loggingDirectory, cmdArgs.BuildRevision);
                    switch(status)
                    {
                        case DacpacDeltasStatus.Success:
                            //nothing to do
                            break;
                        case DacpacDeltasStatus.InSync:
                        case DacpacDeltasStatus.OnlyPostDeployment:
                            log.InfoFormat("Target database {0} is already in sync with {1}. Nothing to do!", targetDatabase, cmdArgs.PlatinumDacpac);
                            this.returnValue = (int)RunnerReturn.DacpacDatabasesInSync;
                            break;
                        default:
                            log.ErrorFormat("Error creating custom dacpac and scripts for{0}. No update was performed", targetDatabase);
                            this.returnValue = (int)RunnerReturn.PackageCreationError;
                            return;
                            
                    }

                }
                else
                {
                    runData.ForceCustomDacpac = false;
                    //Get a full copy of the build data to work with (avoid threading sync issues)
                    SqlSyncBuildData buildData = new SqlSyncBuildData();

                    string xml = "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n" + ThreadedExecution.BuildData.GetXml();
                    using (StringReader sr = new StringReader(xml))
                    {
                        buildData.ReadXml(sr);
                    }
                    //Clear out any existing ComittedScript data.. just log what is relevent to this run.
                    buildData.CommittedScript.Clear();
                    
                    
                    runData.BuildData = buildData;
                    runData.ProjectFileName = loggingDirectory + Path.GetFileName(ThreadedExecution.ProjectFileName);
                    runData.BuildFileName = ThreadedExecution.BuildZipFileName;
                }

               

                //Create a connection object.. all we need is the server here, the DB will be filled in at execution time
                connData = new ConnectionData(server, "");
                if (this.cmdArgs.UserName.Length > 0 && this.cmdArgs.Password.Length > 0)
                {
                    connData.UserId = cmdArgs.UserName;
                    connData.Password = cmdArgs.Password;
                    connData.UseWindowAuthentication = false;
                }

                //Set the log file name
                string logFile = loggingDirectory + @"\ExecutionLog.log";

                //Create the objects that will handle the event communication back.
                bg = new BackgroundWorker();
                bg.WorkerReportsProgress = true;
                e = new DoWorkEventArgs(null);
            }
            catch (Exception exe)
            {
                log.Error("Error Initializing run for "+ this.TargetTag, exe);
                WriteErrorLog(loggingDirectory, exe.ToString());
                this.returnValue = (int)ExecutionReturn.RunInitializationError;
                return;
            }

            log.Debug("Initializing run for " + this.TargetTag + ". Starting \"ProcessBuild\"");

            try
            {
                //Initilize the run helper object and kick it off.
                SqlBuildHelper helper = new SqlBuildHelper(connData, true, string.Empty,cmdArgs.Transactional); //don't need an "external" log for this, it's all external!
                helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
                helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
                helper.BuildSuccessTrialRolledBackEvent += new EventHandler(helper_BuildSuccessTrialRolledBackEvent);
                helper.ProcessBuild(runData, bg, e, ThreadedExecution.BatchColl, this.buildRequestedBy,cmdArgs.AllowableTimeoutRetries);
            }
            catch (Exception exe)
            {
                log.Error("Error Processing run for " + this.TargetTag, exe);
                WriteErrorLog(loggingDirectory, exe.ToString());
                this.returnValue = (int)ExecutionReturn.ProcessBuildError;
                return;
            }
            return;

        }

        void helper_BuildSuccessTrialRolledBackEvent(object sender, EventArgs e)
        {
            log.Debug(this.TargetTag + " BuildSuccessTrialRolledBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.SuccessWithTrialRolledBack));
            this.returnValue = (int)RunnerReturn.SuccessWithTrialRolledBack;
        }

        void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            if (this.IsTransactional)
            {
                log.Debug(this.TargetTag + " BuildErrorRollBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.RolledBack));
                this.returnValue = (int)RunnerReturn.RolledBack;
            }
            else
            {
                log.Debug(this.TargetTag + " BuildErrorRollBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.BuildErrorNonTransactional));
                this.returnValue = (int)RunnerReturn.BuildErrorNonTransactional;
            }
        }

        void helper_BuildCommittedEvent(object sender,RunnerReturn returnValue)
        {
            log.Debug(this.TargetTag + " BuildCommittedEvent status: " + Enum.GetName(typeof(RunnerReturn), returnValue));
            this.returnValue = (int)returnValue;
        }

        private void WriteErrorLog(string loggingDirectory, string message)
        {
            File.WriteAllText(loggingDirectory + "Error.log", message);
        }

    }
}
