using log4net;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SqlBuildManager.Console
{
    public class ThreadedExecution
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ILog logEvent = LogManager.GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), "AzureEventHubAppenderLogger");

        StringBuilder sbSuccessDatabasesCfg = new System.Text.StringBuilder();
        StringBuilder sbFailureDatabasesCfg = new System.Text.StringBuilder();

        MultiDbData multiData = null;
        DateTime startTime;
        bool hasError = false;
        private string[] args;
        private CommandLineArgs cmdLine = null;
        string workingDirectory = string.Empty;
        private static string projectFileName = string.Empty;
        /// <summary>
        /// Path and file name to the XML metadata configuration project file (SqlSyncBuildProject.xml)
        /// </summary>
        internal static string ProjectFileName
        {
            get { return ThreadedExecution.projectFileName; }
        }
        string projectFilePath = string.Empty;
        private string logFileName;
        private string errorFileName;
        private string commitLogName;
        private string successDatabaseConfigLogName;
        private string failureDatabaseConfigLogName;

        private bool haveWrittenToError;
        private bool haveWrittenToCommit;
        private bool loggingPathsInitialized = false;
        private string buildRequestedBy = string.Empty;

        private static string buildZipFileName = string.Empty;
        private static string platinumDacPacFileName = string.Empty;
        /// <summary>
        /// The name of the zippedbuild file (.sbm)
        /// </summary>
        internal static string BuildZipFileName
        {
            get { return ThreadedExecution.buildZipFileName; }
        }
        internal static string PlatinumDacPacFileName
        {
            get { return ThreadedExecution.platinumDacPacFileName; }
        }

        private static string rootLoggingPath = string.Empty;
        /// <summary>
        /// The root folder where the logging should start
        /// </summary>
        internal static string RootLoggingPath
        {
            get { return ThreadedExecution.rootLoggingPath; }
        }

        private static string runID = string.Empty;
        /// <summary>
        /// "unique" identifier for the run. 
        /// </summary>
        internal static string RunID
        {
            get
            {
                if (ThreadedExecution.runID == string.Empty)
                    ThreadedExecution.runID = Guid.NewGuid().ToString().Replace("-", "");

                return ThreadedExecution.runID;
            }
        }

        /// <summary>
        /// The threaded syncronization object
        /// </summary>
        internal static SyncObject SyncObj = new SyncObject();

        private static ScriptBatchCollection batchColl = null;
        /// <summary>
        /// The pre-batched set of scripts to be run
        /// </summary>
        internal static ScriptBatchCollection BatchColl
        {
            get { return ThreadedExecution.batchColl; }
        }

        private static SqlSyncBuildData buildData = null;
        /// <summary>
        /// The runtime metadata object for the build execution
        /// </summary>
        internal static SqlSyncBuildData BuildData
        {
            get { return buildData; }
        }

        public ThreadedExecution(CommandLineArgs cmd)
        {
            this.cmdLine = cmd;
        }
        public ThreadedExecution(string[] args)
        {
            this.args = args;
        }
        public ThreadedExecution()
        {
        }
        /// <summary>
        /// Execute method that is used from a straight command-line execution
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            log.Debug("Entering Execute method of ThreadedExecution");
            string[] errorMessages;
            //Parse out the command line options
            if (cmdLine == null)
            {
                cmdLine = CommandLine.ParseCommandLineArg(args);
            }

            if(string.IsNullOrEmpty(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = @"C:/tmp-sqlbuildlogging";
            }

            SetLoggingPaths(cmdLine.RootLoggingPath);

            log.Info("Validating command parameters");
            int tmpReturn = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                WriteToLog(msg);
                return tmpReturn;
            }

            //Start logging
            WriteToLog("**** Starting log for Run ID: " + ThreadedExecution.RunID + " ****", LogType.Message);

            //If we don't have a pre-constructed build file, but rather a script source directory, we'll build one from there...
            if (!string.IsNullOrWhiteSpace(cmdLine.ScriptSrcDir))
            {
                ConstructBuildFileFromScriptDirectory(cmdLine.ScriptSrcDir);
            }
            else if(!string.IsNullOrWhiteSpace(cmdLine.BuildFileName)) //using SBM as a source
            {
                ThreadedExecution.buildZipFileName = cmdLine.BuildFileName;
                string msg = "/PackageName setting found. Using '" + ThreadedExecution.buildZipFileName + "' as build source";
                WriteToLog(msg, LogType.Message);
                log.Info(msg);
            }
            else if(!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac)) //using a platinum dacpac as a source
            {
                ThreadedExecution.platinumDacPacFileName = cmdLine.DacPacArgs.PlatinumDacpac;
                string msg = "/PlatinumDacpac setting found. Using '" + ThreadedExecution.platinumDacPacFileName + "' as build source";
                WriteToLog(msg, LogType.Message);
                log.Info(msg);

            }
            else if(!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource)) //using a platinum database as the source
            {
                log.InfoFormat("Extracting Platinum Dacpac from {0} : {1}", cmdLine.DacPacArgs.PlatinumServerSource, cmdLine.DacPacArgs.PlatinumDbSource);
                string dacpacName = Path.Combine(ThreadedExecution.rootLoggingPath, cmdLine.DacPacArgs.PlatinumDbSource + ".dacpac");

                if(!DacPacHelper.ExtractDacPac(cmdLine.DacPacArgs.PlatinumDbSource, cmdLine.DacPacArgs.PlatinumServerSource, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, dacpacName))
                {
                    var m = new LogMsg()
                    {
                        Message = $"Error creating the Platinum dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}",
                        LogType = LogType.Error
                    };
                    WriteToLog(m);
                    return (int)ExecutionReturn.BuildFileExtractionError;

                }
                cmdLine.DacPacArgs.PlatinumDacpac = dacpacName;
                ThreadedExecution.platinumDacPacFileName = dacpacName;

            }

           
            //Load the multi database configuration data from XML or flat file or SQL...
            string message = string.Empty;

            int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out  multiData, out errorMessages);
            if (tmpValReturn != 0)
            {
                var msg = new LogMsg() { Message = String.Join(";", errorMessages), LogType = LogType.Error };
                WriteToLog(msg);
                return tmpValReturn;
            }
            

            //Check for the platinum dacpac and configure it if necessary
            tmpValReturn = Validation.ValidateAndLoadPlatinumDacpac(ref cmdLine, ref multiData);
            if (tmpValReturn == 0 && string.IsNullOrEmpty(ThreadedExecution.buildZipFileName))
            {
                ThreadedExecution.buildZipFileName = cmdLine.BuildFileName;
            }
            else if(tmpValReturn == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                return (int)ExecutionReturn.DacpacDatabasesInSync;
            }else if(tmpReturn != 0)
            {
                return tmpValReturn;
            }

            //Set the number of allowed retries...
            multiData.AllowableTimeoutRetries = cmdLine.TimeoutRetryCount;
            //Set Trial
            multiData.RunAsTrial = cmdLine.Trial;
            multiData.BuildRevision = cmdLine.BuildRevision;

            return Execute(ThreadedExecution.buildZipFileName, cmdLine.DacPacArgs.PlatinumDacpac, multiData, cmdLine.RootLoggingPath, cmdLine.Description, System.Environment.UserName, cmdLine.DacPacArgs.ForceCustomDacPac);
        }

        /// <summary>
        /// Execute method that is used inherently from the Execute() and also from the RemoteExecution service.
        /// </summary>
        /// <param name="buildZipFileName"></param>
        /// <param name="multiData"></param>
        /// <param name="rootLoggingPath"></param>
        /// <param name="description"></param>
        /// <param name="buildRequestedBy"></param>
        /// <returns></returns>
        public int Execute(string buildZipFileName, string platinumDacPacFileName, MultiDbData multiData, string rootLoggingPath, string description, string buildRequestedBy, bool forceCustomDacpac)
        {
            try {
                ThreadedExecution.buildZipFileName = buildZipFileName;

                this.buildRequestedBy = buildRequestedBy;

                //Build out reused arguments from the CommandLineArgs object.. this will be null if called from the BuildService
                if (this.cmdLine == null)
                {
                    log.InfoFormat("Creating CommandLineArgs object for build {0}", buildZipFileName);
                    this.cmdLine = new CommandLineArgs();
                    this.cmdLine.RootLoggingPath = rootLoggingPath;
                    this.cmdLine.Transactional = multiData.IsTransactional;
                    this.cmdLine.Trial = multiData.RunAsTrial;
                    this.cmdLine.Description = description;
                    this.cmdLine.TimeoutRetryCount = multiData.AllowableTimeoutRetries; //set the retries count...
                    if (!string.IsNullOrWhiteSpace(multiData.UserName))
                        this.cmdLine.AuthenticationArgs.UserName = multiData.UserName;
                    if (!string.IsNullOrWhiteSpace(multiData.Password))
                        this.cmdLine.AuthenticationArgs.Password = multiData.Password;

                    try
                    {
                        this.cmdLine.AuthenticationArgs.AuthenticationType = multiData.AuthenticationType;
                    }
                    catch (Exception exe)
                    {
                        log.Warn("Issue setting authentication type. Defaulting to UsernamePassword", exe);
                        this.cmdLine.AuthenticationArgs.AuthenticationType = AuthenticationType.Password;
                    }

                    this.cmdLine.DacPacArgs.PlatinumDacpac = platinumDacPacFileName;
                    this.cmdLine.BuildRevision = multiData.BuildRevision;
                }

               // log.DebugFormat("Commandline configuration created:\r\n{0}", cmdLine.ToString());

                if (!this.loggingPathsInitialized)
                    SetLoggingPaths(rootLoggingPath);

                string error;
                //Looks like we're good to go... extract the build Zip file (.sbm) into a working folder...

                if (forceCustomDacpac == false)
                {
                    ExtractAndLoadBuildFile(ThreadedExecution.buildZipFileName, out ThreadedExecution.buildData);
                    if (buildData == null)
                    {
                        error = "Unable to procees. SqlSyncBuild data object is null";
                        var msg = new LogMsg()
                        {
                            Message = "Unable to procees. SqlSyncBuild data object is null, Returning error code: " + (int)ExecutionReturn.NullBuildData,
                            LogType = LogType.Error
                        };
                        WriteToLog(msg);
                        return (int)ExecutionReturn.NullBuildData;
                    }
                }

                int threadTotal = 0;
                try
                {
                    startTime = DateTime.Now;
                    log.InfoFormat("Starting Threaded processing at {0}", startTime.ToString());
                    //Increase the number of threads in the threadpool...
                    System.Threading.ThreadPool.SetMaxThreads(200, 200);
                    //Load up the batched scripts into a shared object so that we can conserve memory

                    if (!forceCustomDacpac)
                    {
                        ThreadedExecution.batchColl = SqlBuildHelper.LoadAndBatchSqlScripts(ThreadedExecution.buildData, this.projectFilePath);
                    }
                    foreach (ServerData srv in multiData)
                    {
                        foreach (List<DatabaseOverride> ovr in srv.OverrideSequence.Values)
                        {
                            threadTotal++;
                            lock (ThreadedExecution.SyncObj)
                            {
                                ThreadedExecution.SyncObj.WorkingRunners++;
                            }

                            ThreadedRunner runner = new ThreadedRunner(srv.ServerName, ovr, cmdLine, buildRequestedBy, forceCustomDacpac);
                            var msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, RunId = ThreadedExecution.RunID, Message = "Queuing up thread", LogType = LogType.Message };
                            WriteToLog(msg);
                            System.Threading.ThreadPool.QueueUserWorkItem(ProcessThreadedBuild, runner);
                        }
                    }
                }
                catch (Exception exe)
                {
                    WriteToLog(exe.ToString(), LogType.Error);
                }

                while (ThreadedExecution.SyncObj.WorkingRunners > 0)
                {
                    System.Threading.Thread.Sleep(100);
                }

                TimeSpan interval = DateTime.Now - startTime;
                var finalMsg = new LogMsg() { RunId = ThreadedExecution.runID, Message = $"Ending threaded processing at {DateTime.Now.ToUniversalTime()}", LogType =  LogType.Message };
                WriteToLog(finalMsg);
                finalMsg.Message = $"Execution Duration: {interval.ToString()}";
                WriteToLog(finalMsg);
                finalMsg.Message = $"Total number of targets: {threadTotal.ToString()}";
                WriteToLog(finalMsg);
                if (this.hasError)
                {
                    WriteToLog("", LogType.SuccessDatabases);
                    WriteToLog("", LogType.FailureDatabases);
                    finalMsg.Message = "Finishing with Errors";
                    finalMsg.LogType = LogType.Error;
                    WriteToLog(finalMsg);
                    finalMsg.LogType = LogType.Message;
                    WriteToLog(finalMsg);
                    return (int)ExecutionReturn.FinishingWithErrors;
                }
                else
                {
                    WriteToLog("", LogType.SuccessDatabases);
                    log.Info("Successful");
                    return (int)ExecutionReturn.Successful;
                }
            }catch(Exception bigExe)
            {
                log.FatalFormat("Big problem running the threaded build...", bigExe);
                return (int)ExecutionReturn.NullBuildData;

            }
        }
       
        private void ProcessThreadedBuild(object state)
        {
            var msg = new LogMsg();
            try
            {
                
                ThreadedRunner runner = (ThreadedRunner)state;
                //SERVER:defaultDb,override
                string cfgString = String.Format("{0}:{1},{2}", runner.Server, runner.DefaultDatabaseName, runner.TargetDatabases);

                msg = new LogMsg() { DatabaseName = runner.TargetDatabases, ServerName = runner.Server, SourceDacPac = runner.DacpacName,  RunId = ThreadedExecution.RunID, Message = "Starting up thread", LogType= LogType.Message };
                WriteToLog(msg);

                runner.RunDatabaseBuild();

                 msg.Message = ((RunnerReturn)runner.ReturnValue).GetDescription();
                switch (runner.ReturnValue)
                {
                    case (int)RunnerReturn.BuildCommitted:
                    case (int)RunnerReturn.DacpacDatabasesInSync:
                    case (int)RunnerReturn.CommittedWithCustomDacpac:
                    case (int)RunnerReturn.SuccessWithTrialRolledBack:
                        msg.LogType = LogType.Commit;
                        WriteToLog(msg);
                        sbSuccessDatabasesCfg.AppendLine(cfgString);
                        break;

                    case (int)RunnerReturn.RolledBack:
                    case (int)RunnerReturn.BuildErrorNonTransactional:
                    default:
                        msg.LogType = LogType.Error;
                        WriteToLog(msg);
                        sbFailureDatabasesCfg.AppendLine(cfgString);
                        this.hasError = true;
                        break;
                }

                msg.Message = "Thread complete";
                msg.LogType = LogType.Message;
                WriteToLog(msg);
                runner = null;

            }
            catch (Exception exe)
            {
                msg.Message = exe.Message;
                msg.LogType = LogType.Error;
                WriteToLog(msg);
            }
            finally
            {

                lock (ThreadedExecution.SyncObj)
                {
                    ThreadedExecution.SyncObj.WorkingRunners--;
                }
            }
        }

        private void SetLoggingPaths(string rootLoggingPath)
        {
            try
            {
                log.InfoFormat("Initializing logging paths: {0}", rootLoggingPath);

                //Set the logging folders, etc...s
                string expanded = System.Environment.ExpandEnvironmentVariables(rootLoggingPath);
                ThreadedExecution.rootLoggingPath = Path.GetFullPath(expanded);

                log.InfoFormat("Logging path expanded to: {0}", ThreadedExecution.rootLoggingPath);

                if (!Directory.Exists(ThreadedExecution.rootLoggingPath))
                    Directory.CreateDirectory(ThreadedExecution.rootLoggingPath);

                this.workingDirectory = ThreadedExecution.rootLoggingPath + @"/Working";
                if (!Directory.Exists(this.workingDirectory))
                    Directory.CreateDirectory(this.workingDirectory);

                this.logFileName = rootLoggingPath + @"/Execution.log";
                this.successDatabaseConfigLogName = rootLoggingPath + @"/SuccessDatabases.cfg";
                this.failureDatabaseConfigLogName = rootLoggingPath + @"/FailureDatabases.cfg";

                this.errorFileName = rootLoggingPath + @"/Errors.log";
                this.commitLogName = rootLoggingPath + @"/Commits.log";
   
            }
            catch (Exception exe)
            {
                log.Error("Unable to set root logging path for " + rootLoggingPath, exe);
                throw exe;
            }


            this.loggingPathsInitialized = true;

        }

        private int ExtractAndLoadBuildFile(string sqlBuildProjectFileName,  out SqlSyncBuildData buildData)
        {
            log.InfoFormat("Extracting build file '{0}' to working directory '{1}'", sqlBuildProjectFileName, this.workingDirectory);

            buildData = null;

            Directory.CreateDirectory(this.workingDirectory);

            string result;
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sqlBuildProjectFileName, ref this.workingDirectory, ref this.projectFilePath, ref ThreadedExecution.projectFileName, false, true, out result))
            {
                var msg = new LogMsg()
                {
                    Message = $"Zip extraction error. Unable to Extract Sql Build file at '{sqlBuildProjectFileName}'. Do you need to specify a full directory path? {result}",
                    LogType = LogType.Error
                };
                WriteToLog(msg);
                return (int)ExecutionReturn.BuildFileExtractionError;

            }
  
            if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, ThreadedExecution.projectFileName, false))
            {
                var msg = new LogMsg()
                {
                    Message = $"Build project load error. Unable to load project file.",
                    LogType = LogType.Error
                };
                WriteToLog(msg);
                return (int)ExecutionReturn.LoadProjectFileError;
            }

            return 0;
        }
       
        private void ConstructBuildFileFromScriptDirectory(string directoryName)
        {
            log.Info("Constructing build file from script directory");
            string shortFileName = string.Empty;
            ThreadedExecution.buildZipFileName = ThreadedExecution.rootLoggingPath +"/"+ ThreadedExecution.RunID +".sbm";
            string projFileName = this.workingDirectory + @"/"+SqlSync.SqlBuild.XmlFileNames.MainProjectFile;
            SqlSyncBuildData localBuildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            List<string> fileList = new List<string>(Directory.GetFiles(directoryName, "*.sql", SearchOption.TopDirectoryOnly));
            fileList.Sort();

            for (int i = 0; i < fileList.Count; i++)
            {
                shortFileName = Path.GetFileName(fileList[i]);
                File.Copy(fileList[i], this.workingDirectory + @"/"+ shortFileName,true);

                SqlBuildFileHelper.AddScriptFileToBuild(ref localBuildData,
                    projFileName,
                    shortFileName,
                    i,
                   "'"+ fileList[i] + "' added via source directory setting.",
                    true,
                    true,
                    "",
                    false,
                    ThreadedExecution.buildZipFileName,
                    ((i<fileList.Count-1)? false : true),
                    false,
                    System.Environment.UserDomainName +@"/"+ System.Environment.UserName,
                    20,
                    "");
            }

        }

        private void WriteToLog(string[] message, LogType type, int iteration)
        {

            string initLog = string.Empty;
            if (!haveWrittenToCommit || !haveWrittenToError)
            {
                initLog = "[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "]\t\t***Log for Run ID:" + ThreadedExecution.RunID + System.Environment.NewLine;
            }
            string log = string.Empty;
            for (int i = 0; i < message.Length; i++)
            {
                DateTime now = DateTime.Now;
                log += "[" + now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "]\t\t" + message[i] + "\r\n";
            }
            try
            {
                string fileName;
                switch (type)
                {
                    case LogType.FailureDatabases:
                        fileName = this.failureDatabaseConfigLogName;
                        File.WriteAllText(fileName,this.sbFailureDatabasesCfg.ToString());
                        return;
                    case LogType.SuccessDatabases:
                        fileName = this.successDatabaseConfigLogName;
                        File.WriteAllText(fileName,this.sbSuccessDatabasesCfg.ToString());
                        return;
                    case LogType.Commit:
                        fileName = this.commitLogName;
                        if (!haveWrittenToCommit)
                        {
                            log = initLog + log;
                            haveWrittenToCommit = true;
                        }
                        break;
                    case LogType.Error:
                        fileName = this.errorFileName;
                        if (!haveWrittenToError)
                        {
                            log = initLog + log;
                            haveWrittenToError = true;
                        }
                        break;
                    case LogType.Message:
                    default:
                        fileName = this.logFileName;
                        break;

                }
                if (iteration < 5)
                    File.AppendAllText(fileName, log);
            }
            catch
            {
                iteration++;
                WriteToLog(message, type, iteration);

            }
        }
        private void WriteToLog(string message, LogType type)
        {
            WriteToLog(new string[] { message }, type, 0);
        }
        private void WriteToLog(LogMsg msg)
        {
            if (msg.LogType == LogType.Error)
            {
                log.Error(msg.ToString());
            }
            else
            {
                log.Info(msg.ToString());
            }
            logEvent.Info(msg.Message);
            WriteToLog(msg.ToString(), msg.LogType);
        }

    }


    //private void QueueThreadedRunners(ref List<DatabaseOverride> overrides, int serverStartIndex,int overrideStartIndex, int numberToQueue)
    //{
    //    for(int iSrv = serverStartIndex; iSrv< this.multiData.Count;iSrv++)
    //    {
    //        ServerData srv = this.multiData[iSrv];
    //        for(int iOvr = overrideStartIndex; iOvr < srv.OverrideSequence.Values.Count;iOvr++)
    //        foreach (List<DatabaseOverride> ovr in srv.OverrideSequence.Values)
    //        {
    //            Dictionary<string,List<DatabaseOverride>>.KeyCollection.Enumerator enumer = srv.OverrideSequence.Keys.GetEnumerator();
    //            enumer.
    //            keys.
    //            List<DatabaseOverride> ovr = srv.OverrideSequence.Keys
    //            threadTotal++;
    //            lock (ThreadedExecution.SyncObj)
    //            {
    //                ThreadedExecution.SyncObj.WorkingRunners++;
    //            }
    //            ThreadedRunner runner = new ThreadedRunner(srv.ServerName, ovr, cmdLine);
    //            WriteToLog("Queuing up thread for " + runner.Server + "." + runner.TargetDatabases, LogType.Message);
    //            System.Threading.ThreadPool.QueueUserWorkItem(ProcessThreadedBuild, runner);
    //        }
    //    }
    //}


    public class SyncObject
    {
        private int workingRunners = 0;

        public int WorkingRunners
        {
            get { return workingRunners; }
            set { workingRunners = value; }
        }
    }
}
