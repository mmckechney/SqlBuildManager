using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using SqlBuildManager.Interfaces.Console;
using log4net;
using System.Text;
namespace SqlBuildManager.Console
{
    public class ThreadedExecution
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        private static bool logAsText;
        /// <summary>
        /// Determines whether or not to create the commit and error logs as text. If false, an HTML log is created.
        /// </summary>
        internal static bool LogAsText
        {
            get
            {
                return logAsText;
            }

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
            cmdLine = CommandLine.ParseCommandLineArg(args);

            //Set the logging type
            logAsText = cmdLine.LogAsText;

            if(string.IsNullOrEmpty(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = @"C:\tmp-sqlbuildlogging";
            }

            SetLoggingPaths(cmdLine.RootLoggingPath);


            int tmpReturn = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                WriteToLog(errorMessages, LogType.Error);
                return tmpReturn;
            }

            //Start logging
            WriteToLog("**** Starting log for Run ID: " + ThreadedExecution.RunID + " ****", LogType.Message);

            //If we don't have a pre-constructed build file, but rather a script source directory, we'll build one from there...
            if (cmdLine.ScriptSrcDir.Length > 0)
            {
                ConstructBuildFileFromScriptDirectory(cmdLine.ScriptSrcDir);
            }
            else if(cmdLine.BuildFileName.Length > 0 )
            {
                ThreadedExecution.buildZipFileName = cmdLine.BuildFileName;
                WriteToLog("/build setting found. Using '" + ThreadedExecution.buildZipFileName + "' as build source", LogType.Message);
            }
            else if(cmdLine.PlatinumDacpac.Length > 0)
            {
                ThreadedExecution.platinumDacPacFileName = cmdLine.PlatinumDacpac;
                WriteToLog("/PlatinumDacpac setting found. Using '" + ThreadedExecution.platinumDacPacFileName + "' as build source", LogType.Message);

            }
            

            //Load the multi database configuration data from XML or flat file...
            string message = string.Empty;

            int tmpValReturn = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, out  multiData, out errorMessages);
            if (tmpValReturn != 0)
            {
                WriteToLog(errorMessages, LogType.Error);
                return tmpReturn;
            }

            //Check for the platinum dacpac and configure it if necessary
            tmpValReturn = Validation.ValidateAndLoadPlatinumDacpac(ref cmdLine, ref multiData);
            if (tmpValReturn == 0 && string.IsNullOrEmpty(ThreadedExecution.buildZipFileName))
            {
                ThreadedExecution.buildZipFileName = cmdLine.BuildFileName;
            }

            //Set the number of allowed retries...
            multiData.AllowableTimeoutRetries = cmdLine.AllowableTimeoutRetries;
            //Set Trial
            multiData.RunAsTrial = cmdLine.Trial;

            return Execute(ThreadedExecution.buildZipFileName, cmdLine.PlatinumDacpac, multiData, cmdLine.RootLoggingPath, cmdLine.Description, System.Environment.UserName);
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
        public int Execute(string buildZipFileName, string platinumDacPacFileName, MultiDbData multiData, string rootLoggingPath, string description, string buildRequestedBy)
        {
            ThreadedExecution.buildZipFileName = buildZipFileName;
            
            this.buildRequestedBy = buildRequestedBy;

            //Build out reused arguments from the CommandLineArgs object.. this will be null if called from the BuildService
            if (this.cmdLine == null)
            {
                log.InfoFormat("Creating CommandLineArgs object for build {0}", buildZipFileName);
                logAsText = true;
                this.cmdLine = new CommandLineArgs();
                this.cmdLine.RootLoggingPath = rootLoggingPath;
                this.cmdLine.LogAsText = true;
                this.cmdLine.Transactional = multiData.IsTransactional;
                this.cmdLine.Trial = multiData.RunAsTrial;
                this.cmdLine.Description = description;
                this.cmdLine.AllowableTimeoutRetries = multiData.AllowableTimeoutRetries; //set the retries count...
                if(!string.IsNullOrWhiteSpace(multiData.UserName))
                    this.cmdLine.UserName = multiData.UserName;
                if (!string.IsNullOrWhiteSpace(multiData.Password))
                    this.cmdLine.Password = multiData.Password;

                this.cmdLine.PlatinumDacpac = platinumDacPacFileName;
            }

               

            if (!this.loggingPathsInitialized)
                SetLoggingPaths(rootLoggingPath);

            string error;
            //Looks like we're good to go... extract the build Zip file (.sbm) into a working folder...
            ExtractAndLoadBuildFile(ThreadedExecution.buildZipFileName, out ThreadedExecution.buildData);
            if (buildData == null)
            {
                error = "Unable to procees. SqlSyncBuild data object is null";
                log.Error(error);
                WriteToLog(new string[] { error, "Returning error code: " + (int)ExecutionReturn.NullBuildData }, LogType.Error);
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.NullBuildData;
            }

            int threadTotal = 0;
            try
            {
                startTime = DateTime.Now;
                log.InfoFormat("Starting Threaded processing at {0}", startTime.ToString());
                //Increase the number of threads in the threadpool...
                System.Threading.ThreadPool.SetMaxThreads(200, 200);
                //Load up the batched scripts into a shared object so that we can conserve memory
                ThreadedExecution.batchColl = SqlBuildHelper.LoadAndBatchSqlScripts(ThreadedExecution.buildData, this.projectFilePath);
                foreach (ServerData srv in multiData)
                {
                    foreach (List<DatabaseOverride> ovr in srv.OverrideSequence.Values)
                    {
                        threadTotal++;
                        lock (ThreadedExecution.SyncObj)
                        {
                            ThreadedExecution.SyncObj.WorkingRunners++;
                        }

                        ThreadedRunner runner = new ThreadedRunner(srv.ServerName, ovr, cmdLine, buildRequestedBy);
                        string msg = "Queuing up thread for " + runner.Server + "." + runner.TargetDatabases;
                        log.Debug(msg);
                        WriteToLog(msg, LogType.Message);
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
            log.InfoFormat("Ending threaded processing at {0}", DateTime.Now.ToString());
            WriteToLog("Execution Duration: " + interval.ToString(), LogType.Message);
            WriteToLog("Total number of targets: " + threadTotal.ToString(), LogType.Message);
            if (this.hasError)
            {
                WriteToLog("", LogType.SuccessDatabases);
                WriteToLog("", LogType.FailureDatabases);
                WriteToLog("Finishing with Errors", LogType.Error);
                WriteToLog("Finishing with Errors", LogType.Message);
                log.Error("Finishing with Errors");
                return (int)ExecutionReturn.FinishingWithErrors;
            }
            else
            {
                WriteToLog("", LogType.SuccessDatabases);
                log.Info("Successful");
                return (int)ExecutionReturn.Successful;
            }
        }
       
        private void ProcessThreadedBuild(object state)
        {
            try
            {
                
                ThreadedRunner runner = (ThreadedRunner)state;
                //SERVER:defaultDb,override
                string cfgString = String.Format("{0}:{1},{2}", runner.Server, runner.DefaultDatabaseName, runner.TargetDatabases);
            
                string msg = "Starting up thread for " + runner.Server + ": " + runner.TargetDatabases;
                log.Debug(msg);
                WriteToLog(msg, LogType.Message);
                runner.RunDatabaseBuild();

                switch (runner.ReturnValue)
                {
                    case (int)RunnerReturn.BuildCommitted:
                        sbSuccessDatabasesCfg.AppendLine(cfgString);
                        if (logAsText)
                            WriteToLog(runner.Server + "." + runner.TargetDatabases + " : Build Committed", LogType.Commit);
                        else
                            WriteToLog("<a href=\"" + runner.Server + "/" + runner.TargetDatabases + "/\">" + runner.Server + "/" + runner.TargetDatabases + "</a> : Build Committed", LogType.Commit);
                        break;

                    case (int)RunnerReturn.SuccessWithTrialRolledBack:
                        sbSuccessDatabasesCfg.AppendLine(cfgString);
                        if (logAsText)
                            WriteToLog(runner.Server + "." + runner.TargetDatabases + " : Build Successful. Trial Rolled-back", LogType.Commit);
                        else
                            WriteToLog("<a href=\"" + runner.Server + "/" + runner.TargetDatabases + "/\">" + runner.Server + "/" + runner.TargetDatabases + "</a> : Build Successful. Trial Rolled-back", LogType.Commit);
                        break;

                    case (int)RunnerReturn.RolledBack:
                        sbFailureDatabasesCfg.AppendLine(cfgString);
                        if (logAsText)
                            WriteToLog(runner.Server + "." + runner.TargetDatabases + " : Changes Rolled back. Return code: " + runner.ReturnValue.ToString(), LogType.Error);
                        else
                            WriteToLog("<a href=\"" + runner.Server + "/" + runner.TargetDatabases + "/\">" + runner.Server + "/" + runner.TargetDatabases + "</a>: Changes Rolled back. Return code: " + runner.ReturnValue.ToString(), LogType.Error);
                        this.hasError = true;
                        break;
                    case (int)RunnerReturn.BuildErrorNonTransactional:
                        sbFailureDatabasesCfg.AppendLine(cfgString);
                        if (logAsText)
                            WriteToLog(runner.Server + "." + runner.TargetDatabases + " : Build Error. Running non-transactional, unable to rollback. Return code: " + runner.ReturnValue.ToString(), LogType.Error);
                        else
                            WriteToLog("<a href=\"" + runner.Server + "/" + runner.TargetDatabases + "/\">" + runner.Server + "/" + runner.TargetDatabases + "</a>: Build Error. Running non-transactional, unable to rollback. Return code: " + runner.ReturnValue.ToString(), LogType.Error);
                        this.hasError = true;
                        break;

                    default:
                        sbFailureDatabasesCfg.AppendLine(cfgString);
                        WriteToLog(runner.Server + "." + runner.TargetDatabases + " : Return code: " + runner.ReturnValue.ToString(), LogType.Error);
                        this.hasError = true;
                        break;
                }

                WriteToLog("Thread complete for " + runner.Server + ": " + runner.TargetDatabases, LogType.Message);
                runner = null;

            }
            catch (Exception exe)
            {
                WriteToLog(exe.ToString(), LogType.Error);
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

                this.workingDirectory = ThreadedExecution.rootLoggingPath + @"\Working";
                if (!Directory.Exists(this.workingDirectory))
                    Directory.CreateDirectory(this.workingDirectory);

                this.logFileName = rootLoggingPath + @"\Execution.log";
                this.successDatabaseConfigLogName = rootLoggingPath + @"\SuccessDatabases.cfg";
                this.failureDatabaseConfigLogName = rootLoggingPath + @"\FailureDatabases.cfg";

                if (logAsText)
                {
                    this.errorFileName = rootLoggingPath + @"\Errors.log";
                    this.commitLogName = rootLoggingPath + @"\Commits.log";
                }
                else
                {
                    this.errorFileName = rootLoggingPath + @"\Errors.html";
                    this.commitLogName = rootLoggingPath + @"\Commits.html";
                }
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
            if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sqlBuildProjectFileName, ref this.workingDirectory, ref this.projectFilePath, ref ThreadedExecution.projectFileName, false, out result))
            {

                string error = "Unable to Extract Sql Build file at '"+sqlBuildProjectFileName+"'. You you need to specify a full directory path? " + result;
                log.ErrorFormat("Zip extraction error. {0}", error);
                WriteToLog(new string[] { error, "Returning error code: " + (int)ExecutionReturn.BuildFileExtractionError }, LogType.Error);
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.BuildFileExtractionError;
            }

            if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, ThreadedExecution.projectFileName, false))
            {
                string error = "Unable to load project file.";
                log.ErrorFormat("Build project load error. {0}", error);
                WriteToLog(new string[] { error, "Returning error code: " + (int)ExecutionReturn.LoadProjectFileError }, LogType.Error);
                System.Console.Error.WriteLine(error);
                return (int)ExecutionReturn.LoadProjectFileError;
            }

            return 0;
        }
       
        private void ConstructBuildFileFromScriptDirectory(string directoryName)
        {
            string shortFileName = string.Empty;
            ThreadedExecution.buildZipFileName = ThreadedExecution.rootLoggingPath +"\\"+ ThreadedExecution.RunID +".sbm";
            string projFileName = this.workingDirectory + @"\"+SqlSync.SqlBuild.XmlFileNames.MainProjectFile;
            SqlSyncBuildData localBuildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            List<string> fileList = new List<string>(Directory.GetFiles(directoryName, "*.sql", SearchOption.TopDirectoryOnly));
            fileList.Sort();

            for (int i = 0; i < fileList.Count; i++)
            {
                shortFileName = Path.GetFileName(fileList[i]);
                File.Copy(fileList[i], this.workingDirectory + @"\"+ shortFileName,true);

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
                    System.Environment.UserDomainName +@"\"+ System.Environment.UserName,
                    20,
                    "");
            }

        }

        private void WriteToLog(string[] message, LogType type, int iteration)
        {
            string initLog = string.Empty;
            if (!haveWrittenToCommit || !haveWrittenToError)
            {
                if (logAsText)
                    initLog = "[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "]\t\t***Log for Run ID:" + ThreadedExecution.RunID + "\r\n";
                else
                    initLog = "[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "]&nbsp;&nbsp;&nbsp;***Log for Run ID:" + ThreadedExecution.RunID + "<BR>";
            }
            string log = string.Empty;
            for (int i = 0; i < message.Length; i++)
            {
                DateTime now = DateTime.Now;
                if (type == LogType.Message || logAsText)
                    log += "[" + now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "]\t\t" + message[i] + "\r\n";
                else
                    log += "[" + now.ToString("MM/dd/yyyy HH:mm:ss.fff") + "]&nbsp;&nbsp;&nbsp;" + message[i] + "<br>";

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
        private void WriteToLog(string[] message, LogType type)
        {
            WriteToLog(message, type, 0);
        }
        private void WriteToLog(string message, LogType type)
        {
            WriteToLog(new string[] { message }, type, 0);
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
