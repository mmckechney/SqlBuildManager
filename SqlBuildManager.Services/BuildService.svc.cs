using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using log4net;
using log4net.Appender;
using SqlBuildManager.Interfaces.Console;
using SqlBuildManager.Services.History;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System.Xml.Serialization;
using System.Xml;
namespace SqlBuildManager.Services
{
    // NOTE: If you change the class name "BuildService" here, you must also update the reference to "BuildService" in Web.config.
    public class BuildService : IBuildService
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string buildHistoryFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
        
        public BuildService()
        {

            try
            {
                BuildService.currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
            catch
            {
                BuildService.currentVersion = new Version(0, 0, 0, 0);
            }

            if (!log.Logger.Repository.Configured)
                log4net.Config.BasicConfigurator.Configure();

        }
        BackgroundWorker bgBuild = new BackgroundWorker();
        private static ServiceReadiness myReadiness = ServiceReadiness.ReadyToAccept;
        private static ExecutionReturn myExeStatus = ExecutionReturn.Waiting;
        private bool initialized;
        private WorkArgs arguments = null;
        private static Version currentVersion;
        private void Initialize()
        {
            if (this.initialized)
                return;

            if (!log.Logger.Repository.Configured)
                log4net.Config.BasicConfigurator.Configure();

            bgBuild.WorkerReportsProgress = true;
            bgBuild.WorkerSupportsCancellation = true;
            bgBuild.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgBuild_RunWorkerCompleted);
            bgBuild.ProgressChanged += new ProgressChangedEventHandler(bgBuild_ProgressChanged);
            bgBuild.DoWork += new DoWorkEventHandler(bgBuild_DoWork);

            this.initialized = true;

            try
            {
                BuildService.currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                log.InfoFormat("Initialized service version {0}", currentVersion.ToString());
            }
            catch(Exception exe)
            {
                BuildService.currentVersion = new Version(0, 0, 0, 0);
                log.Error("Unable to retrieve current service version", exe);
            }
        }

        #region IBuildService Members

        public bool SubmitBuildPackage(BuildSettings settings)
        {
            
            this.Initialize();

            if (!log.Logger.Repository.Configured)
                log4net.Config.BasicConfigurator.Configure();


            if (myReadiness != ServiceReadiness.ReadyToAccept)
            {
                log.WarnFormat("Unable to accept package. Service status is {0}", Enum.GetName(typeof(ServiceReadiness), myReadiness));
                return false;
            }
            else
            {
                log.InfoFormat("Accepted package: {0}", settings.ToString());
            }
            myReadiness = ServiceReadiness.Processing;

            try
            {
                string expandedLoggingPath = System.Environment.ExpandEnvironmentVariables(settings.LocalRootLoggingPath);

                if (!Directory.Exists(expandedLoggingPath))
                {
                    log.InfoFormat("Creating logging directory at: {0}", expandedLoggingPath);
                    Directory.CreateDirectory(expandedLoggingPath);
                }
                else
                {
                    log.InfoFormat("Logging directory already exists at: {0}", expandedLoggingPath);
                }

                //BuildService.lastExecutionRootLoggingPath = expandedLoggingPath;

                log.InfoFormat("Processing project contents. Saving {0} bytes to {1}", settings.SqlBuildManagerProjectContents.Length.ToString(), expandedLoggingPath);
                string buildZipFileName = expandedLoggingPath + @"\" + settings.SqlBuildManagerProjectFileName;
                File.WriteAllBytes(buildZipFileName, settings.SqlBuildManagerProjectContents);

                MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(settings.MultiDbTextConfig);
                multiDb.IsTransactional = settings.IsTransactional;
                multiDb.RunAsTrial = settings.IsTrialBuild;
                multiDb.MultiRunId = Guid.NewGuid().ToString();
                multiDb.AllowableTimeoutRetries = settings.TimeoutRetryCount;


                BuildRecord record = new BuildRecord()
                {
                    RequestedBy = settings.BuildRequestFrom,
                    BuildPackageName = Path.GetFileName(buildZipFileName),
                    SubmissionDate = DateTime.Now,
                    RootLogPath = expandedLoggingPath
                };

                this.arguments = new WorkArgs(buildZipFileName, multiDb, expandedLoggingPath, settings.Description, record);
                log.DebugFormat("Starting Async execution of {0}", buildZipFileName);
                bgBuild.RunWorkerAsync();
                return true;

            }
            catch (Exception exe)
            {
                log.Error("Unable to handle BuildSettings package", exe);
                myReadiness = ServiceReadiness.PackageValidationError;
                return false;
            }

        }

        /// <summary>
        /// Gets the status of the service 
        /// </summary>
        /// <returns></returns>
        public ServiceStatus GetServiceStatus()
        {
            try
            {
                log.Debug("Received Service Status Check. Returning:" + System.Enum.GetName(typeof(ServiceReadiness), BuildService.myReadiness) + " | " + Enum.GetName(typeof(ExecutionReturn), BuildService.myExeStatus));
                return new ServiceStatus(BuildService.myReadiness, BuildService.myExeStatus, BuildService.currentVersion.ToString());
            }
            catch (Exception exe)
            {
                log.Error("Unable to get service status", exe);
                return new ServiceStatus(ServiceReadiness.Unknown, ExecutionReturn.Waiting, BuildService.currentVersion.ToString());
            }
        }
        /// <summary>
        /// Retrieves the commits.log file for the last execution on this server
        /// </summary>
        /// <returns>Log file contents</returns>
        public string GetLastExecutionCommitsLog()
        {
            string logPath = GetBuildHistoryRootLogPath(DateTime.MaxValue);
            return GetSummaryLogFileContents(LogType.Commits, logPath);
        }
        /// <summary>
        /// Retrieves the commits.log file for the execution date specified
        /// </summary>
        /// <param name="submittedDate">Date of the execution</param>
        /// <returns>Log file contents</returns>
        public string GetSpecificCommitsLog(DateTime submittedDate)
        {
            string logPath = GetBuildHistoryRootLogPath(submittedDate);
            return GetSummaryLogFileContents(LogType.Commits, logPath);

        }
        /// <summary>
        /// Retrieves the errors.log file for the last execution on this server
        /// </summary>
        /// <returns>Log file contents</returns>
        public string GetLastExecutionErrorsLog()
        {
            string logPath = GetBuildHistoryRootLogPath(DateTime.MaxValue);
            return GetSummaryLogFileContents(LogType.Errors, logPath);
        }
        public string GetLastFailuresDatabaseConfig()
        {
            string logPath = GetBuildHistoryRootLogPath(DateTime.MaxValue);
            return GetSummaryLogFileContents(LogType.FailureDatabases, logPath);
        }
        /// <summary>
        /// Retrieves the errors.log file for the execution date specified
        /// </summary>
        /// <param name="submittedDate">Date of the execution</param>
        /// <returns>Log file contents</returns>
        public string GetSpecificErrorsLog(DateTime submittedDate)
        {
            string logPath = GetBuildHistoryRootLogPath(submittedDate);
            return GetSummaryLogFileContents(LogType.Errors, logPath);
        }
        /// <summary>
        /// Gets the last execution log file for the specified server and database combination
        /// </summary>
        /// <param name="serverAndDatabase">Server and Database in the format of Server\Instance.Database</param>
        /// <returns>Log file contents</returns>
        public string GetDetailedDatabaseExecutionLog(string serverAndDatabase)
        {
            string logPath = GetBuildHistoryRootLogPath(DateTime.MaxValue);
            return GetDetailedDatabaseLogFileContents(logPath, serverAndDatabase, DateTime.MaxValue);
        }
        /// <summary>
        /// Gets the  execution log file for the specified server and database combination for the specified execution date
        /// </summary>
        /// <param name="submittedDate">Date of the execution</param>
        /// <param name="serverAndDatabase">Server and Database in the format of Server\Instance.Database</param>
        /// <returns>Log file contents</returns>
        public string GetSpecificDatabaseExecutionLog(DateTime logEntryDateStamp, string serverAndDatabase)
        {
            string logPath = GetBuildHistoryRootLogPath(logEntryDateStamp);
            return GetDetailedDatabaseLogFileContents(logPath, serverAndDatabase, logEntryDateStamp);
        }
        /// <summary>
        /// Retrieves the version of this service
        /// </summary>
        /// <returns></returns>
        public string GetServiceVersion()
        {
            return currentVersion.ToString();
        }
        /// <summary>
        /// Tests the connectivity to all of the specified database targets
        /// </summary>
        /// <param name="settings">ConnectionTestSettings object with the databases to test</param>
        /// <returns>List of ConnectionTestResult reporting connectivity status</returns>
        public IList<ConnectionTestResult> TestDatabaseConnectivity(ConnectionTestSettings settings)
        {
            this.Initialize();
            return new ThreadedConnectionTester().TestDatabaseConnections(settings.TargetServers);
        }
        /// <summary>
        /// Retrieves the application log file for the service
        /// </summary>
        /// <returns>Log file contents</returns>
        public string GetServiceLogFile()
        {
            string filePath = string.Empty;
            try
            {
                var fileAppender = (from appender in log4net.LogManager.GetRepository().GetAppenders()
                                    where appender is FileAppender
                                    select appender).First();

                filePath = ((FileAppender)fileAppender).File;
                string contents = string.Empty;
                if (File.Exists(filePath))
                {
                    DateTime modDate = File.GetLastWriteTime(filePath);
                    //Need to use FileStream since log4net will have this file open
                    try
                    {
                        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            using (System.IO.StreamReader sr = new StreamReader(fs))
                            {
                                contents = sr.ReadToEnd();
                            }
                        }
                    }
                    catch (IOException) //if reading doesn't work, try a copy
                    {
                        string temp = Path.GetDirectoryName(filePath) + Guid.NewGuid().ToString() +".tmp";
                        File.Copy(filePath, temp);
                        contents = File.ReadAllText(temp);
                        File.Delete(temp);
                    }
                    return GetLogFileInfoHeader(filePath, modDate) + contents;
                }
                else
                    return "Mesage from Remote Execution Service: Unable to retrieve file:" + filePath +"\r\nThe file does not exist";
            }
            catch (Exception exe)
            {
                log.Error("Unable to retrieve Service log from '"+ filePath+"'", exe);
                return "Mesage from Remote Execution Service: Unable to retrieve Service log from '"+ filePath +"'.\r\n"+exe.Message;
            }
        }
        /// <summary>
        /// Retrieves the build request history for this server
        /// </summary>
        /// <returns>List of BuildRecord objects</returns>
        public IList<BuildRecord> GetServiceBuildHistory()
        {
            IList<BuildRecord> history = ReadBuildHistoryFile();
            return history;
        }
        /// <summary>
        /// Retrieves the zip of all of the log files for databases that had errors in their execution for the specified request
        /// </summary>
        /// <param name="submittedDate">Date of the execution</param>
        /// <returns>Zip file byte[] of all the log files</returns>
        public byte[] GetAllErrorLogsForExecution(DateTime submittedDate)
        {
            string tempLogHoldingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\temp_" + Guid.NewGuid().ToString() + @"\";
            try
            {
                string logPath = GetBuildHistoryRootLogPath(submittedDate);
                string mainErrorLogContents = GetSpecificErrorsLog(submittedDate);

                //Create temp holding directory
                
                if (!Directory.Exists(tempLogHoldingPath))
                    Directory.CreateDirectory(tempLogHoldingPath);

                List<string> parsedDirectories = ParseErrorLogForDirectories(mainErrorLogContents, logPath);
                List<string> logFileList = GetAllLogFileNames(parsedDirectories);
                logFileList.Add(logPath + @"\Errors.log");
                string zipFileName = AddLogsFilesToConsolidatedZip(logFileList, tempLogHoldingPath);
                if (zipFileName.Length > 0)
                {
                    return File.ReadAllBytes(zipFileName);
                }
                else
                    return new byte[0];
            }
            catch (Exception exe)
            {
                log.Error(String.Format(" Unable to get consolidated error logs for date {0}", submittedDate.ToString()), exe);
                return new byte[0];
            }
            finally
            {
                if (Directory.Exists(tempLogHoldingPath))
                    Directory.Delete(tempLogHoldingPath, true);
            }
        }



        #endregion

        #region Build BackgroundWorker EventHandlers
        void bgBuild_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            bg.ReportProgress(0, "");
            WorkArgs args = this.arguments;
            BuildRecord record = args.Record;
            
            log.DebugFormat("Creating Threaded execution console object. Processing for {0}", record.BuildPackageName);
            SqlBuildManager.Console.ThreadedExecution threaded = new SqlBuildManager.Console.ThreadedExecution();
            
            int result = threaded.Execute(args.BuildZipFileName, args.MultiDbData, args.RootLoggingPath, args.Description, record.RequestedBy);
            log.Info("Threaded execution console complete with result '" + result.ToString() + "'");
            
            record.ReturnValue = (ExecutionReturn)Enum.Parse(typeof(ExecutionReturn), result.ToString());
            e.Result = record;
        }

        void bgBuild_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            myReadiness = ServiceReadiness.Processing;
            myExeStatus = ExecutionReturn.Running;

        }

        void bgBuild_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BuildRecord record = (BuildRecord)e.Result;
            myReadiness = ServiceReadiness.ReadyToAccept;
            myExeStatus = record.ReturnValue;

            WriteBuildHistoryRecord(record);
        }
        #endregion

        #region Helper methods
        private List<string> ParseErrorLogForDirectories(string mainErrorLogContents, string logPath)
        {
            string serverName, instanceName, databaseName;
            List<string> lst = new List<string>();
            string[] lines = mainErrorLogContents.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = lines.Length - 1; i > 0; i--)
            {
                if (ParseErrorsLogLine(lines[i], out serverName, out instanceName, out databaseName))
                {
                    string dir;
                    if (instanceName.Length > 0)
                    {
                        dir = string.Format(logPath + "\\{0}\\{1}\\{2}", serverName, instanceName, databaseName);
                    }
                    else
                    {
                        dir = string.Format(logPath + "\\{0}\\{1}", serverName, databaseName);
                    }

                    if(!lst.Contains(dir))
                        lst.Add(dir);
                }
            }
            return lst;
        }
        private bool ParseErrorsLogLine(string line, out string serverName, out string instanceName, out string databaseName)
        {
            serverName = string.Empty;
            instanceName = string.Empty;
            databaseName = string.Empty;
            if (line.IndexOf("]") > -1)
            {
                if (line.IndexOf(":", line.IndexOf("]")) > -1 && line.IndexOf("***") == -1)
                {
                    int endOfDateBracket = line.IndexOf("]", 0);
                    int endOfServer = line.IndexOf(":", endOfDateBracket);

                    string sub = line.Substring(endOfDateBracket+1, endOfServer - endOfDateBracket-1);
                    string[] subSplit = sub.Split(new char[] { '\\' });
                    if (subSplit.Length == 2)
                    {
                        serverName = subSplit[0].Trim();
                        string[] serverSplit = subSplit[1].Split(new char[] { '.' });
                        if (serverSplit.Length == 2)
                        {
                            instanceName = serverSplit[0].Trim();
                            databaseName = serverSplit[1].Trim();
                        }
                        else
                        {
                            databaseName = serverSplit[0].Trim();
                        }
                        return true;
                    }
                }

            }
            return false;

        }
        private List<string> GetAllLogFileNames(List<string> parsedDirectories)
        {
            List<string> fileList = new List<string>();
            foreach (string dir in parsedDirectories)
            {
                if (Directory.Exists(dir))
                {
                    String[] files = Directory.GetFiles(dir);
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (!fileList.Contains(files[i]))
                            fileList.Add(files[i]);
                    }

                }
            }
            return fileList;
        }

        private string AddLogsFilesToConsolidatedZip(List<string> logFileList, string tempDirectoryName)
        {
            string zipFileName = tempDirectoryName + "ErrorLogs.zip";
            if (SqlSync.SqlBuild.ZipHelper.CreateZipPackage(logFileList, zipFileName, true, 1))
            {
                return zipFileName;
            }
            else
            {
                return string.Empty;
            }
        }
        private string GetLogFileInfoHeader(string logFilePath, DateTime fileModifiedDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine("Execution Server:\t" + System.Environment.MachineName);
            sb.AppendLine("Local File Path:\t" + Path.GetFullPath(logFilePath));
            sb.AppendLine("File Date:\t\t" + fileModifiedDate.ToString("MM/dd/yyyy hh:mm:ss"));
            sb.AppendLine("*/");
            sb.AppendLine();
            return sb.ToString();

        }

        private void WriteBuildHistoryRecord(BuildRecord record)
        {
            
            List<BuildRecord> history  = ReadBuildHistoryFile().ToList();
            history.Add(record);

            try
            {
                using (XmlTextWriter tw = new XmlTextWriter(buildHistoryFile, Encoding.UTF8))
                {
                    tw.Formatting = Formatting.Indented;
                    XmlSerializer xmlS = new XmlSerializer(typeof(List<BuildRecord>));
                    xmlS.Serialize(tw, history);
                }
                log.DebugFormat("Successfully updated Build History File");
            }
            catch(Exception exe)
            {
                log.Error("Error updating log history file", exe);
            }
        }
        private IList<BuildRecord> ReadBuildHistoryFile()
        {
            IList<BuildRecord> history = null;
            if (File.Exists(buildHistoryFile))
            {
                try
                {
                    log.DebugFormat("Reading build history file at {0}", buildHistoryFile);
                    using (StreamReader sr = new StreamReader(buildHistoryFile))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<BuildRecord>));
                        object obj = serializer.Deserialize(sr);
                        history = (List<BuildRecord>)obj;
                        sr.Close();
                    }
                }
                catch (Exception exe)
                {
                    string moveToFile = buildHistoryFile + DateTime.Now.Ticks.ToString()+".xml";
                    log.Error("Error reading Build History file @ " + buildHistoryFile, exe);
                    try
                    {
                        log.DebugFormat("Moving bad history file to '{0}'", moveToFile);
                        File.Move(buildHistoryFile, moveToFile);
                    }
                    catch(Exception fileExe)
                    {
                        log.ErrorFormat("Unable to move bad history file from '{0}' to '{1}'\r\n{2}", buildHistoryFile, moveToFile, fileExe.ToString());
                    }
                }
            }

            if(history == null)
                history = new List<BuildRecord>();
            
            return history;
        }
        private string GetBuildHistoryRootLogPath(DateTime submittedDate)
        {
            try
            {
                IList<BuildRecord> buildHist = ReadBuildHistoryFile();
                string recordLogPath = string.Empty;

                if (submittedDate != DateTime.MaxValue)
                {
                    //The history submitted date should be the earliest record of the build. All others should be bigger or equal to that.
                    recordLogPath = (from h in buildHist
                                     orderby h.SubmissionDate descending
                                     where h.SubmissionDate <= submittedDate
                                     select h.RootLogPath).First<string>();
                }
                else
                {
                    BuildRecord record = (from h in buildHist
                                          orderby h.SubmissionDate descending
                                          select h).First<BuildRecord>();
                    recordLogPath = record.RootLogPath;
                    submittedDate = record.SubmissionDate;
                }

                if (recordLogPath == null || recordLogPath.Length == 0)
                    log.WarnFormat("Unable to find history record for submitted date of {0}",submittedDate.ToString());
                else
                    log.DebugFormat("Found log path '{0}' for submitted date of {1}", recordLogPath, submittedDate.ToString());

                return recordLogPath;
            }
            catch (Exception exe)
            {
                log.Error(String.Format("Error retriving history record for {0}", submittedDate.ToString()), exe);
                return string.Empty;
            }

        }

        private string GetSummaryLogFileContents(LogType type, string rootLogPath)
        {
            string filePath = rootLogPath;
            try
            {
                switch (type)
                {
                    case LogType.Commits:
                        filePath += @"\Commits.log";
                        break;
                    case LogType.Errors:
                        filePath += @"\Errors.log";
                        break;
                    case LogType.FailureDatabases:
                        filePath += @"\FailureDatabases.cfg";
                        break;

                }


                if (Directory.Exists(rootLogPath) && File.Exists(filePath))
                {
                    if (type != LogType.FailureDatabases)
                    {
                        DateTime modDate = File.GetLastWriteTime(filePath);
                        return GetLogFileInfoHeader(filePath, modDate) + File.ReadAllText(filePath);
                    }
                    else
                    {
                        return File.ReadAllText(filePath);
                    }
                }
                else
                    if (type != LogType.FailureDatabases)
                        return "Unable to retrieve " + filePath;
                    else
                        return "";
            }
            catch (Exception exe)
            {
                string logType = Enum.GetName(typeof(LogType), type);
                log.Error(String.Format("Unable to retrieve \"" + logType + "\" log from {0}", filePath), exe);
                return "Unable to retrieve \"" + logType + "\" log";
            }
        }
        /// <summary>
        /// Gets the detailed LogFile****.log file for a particular server/database/run time
        /// </summary>
        /// <param name="rootLogPath">Local root logging path where the log files can be located</param>
        /// <param name="serverAndDatabase">Server and Database in the format of Server\Instance.Database</param>
        /// <param name="logEntryDateStamp">Date of log entry</param>
        /// <returns>File Contents</returns>
        private string GetDetailedDatabaseLogFileContents(string rootLogPath, string serverAndDatabase, DateTime logEntryDateStamp)
        {
            serverAndDatabase = serverAndDatabase.Trim();
            log.DebugFormat("Recieved execution log request for: {0}. Root log path = '{1}' and submitted date {2}", serverAndDatabase, rootLogPath,logEntryDateStamp.ToString());
            string databasePath = rootLogPath;
            if (Directory.Exists(rootLogPath))
            {
                //break out folders...
                int lastDot = serverAndDatabase.LastIndexOf('.');
                string subFolder = serverAndDatabase.Substring(0, lastDot) + "\\" + serverAndDatabase.Substring(lastDot + 1);

                databasePath = rootLogPath + @"\" + subFolder;
                string newestLogFile = databasePath;
                if (Directory.Exists(databasePath))
                {
                    try
                    {
                       
                        //Try to find the corresponding submitted date file, if not, just pull the latest...
                        FileInfo correspondingLog = GetCorrespondingLogFile(databasePath, logEntryDateStamp);
                        if (correspondingLog != null)
                        {
                            DateTime modDate = correspondingLog.LastWriteTime;
                            newestLogFile = correspondingLog.FullName;
                            return GetLogFileInfoHeader(correspondingLog.FullName, modDate) + File.ReadAllText(correspondingLog.FullName);
                        }
                        else
                        {
                            return String.Format("Unable to find log file in the directory \"{0}\" for requested date {1}", databasePath, logEntryDateStamp.ToString());
                        }
                    }
                    catch (Exception exe)
                    {
                        log.Error(String.Format("Unable to retrieve detailed log file from {0}", newestLogFile), exe);
                    }
                }
                else
                {
                    log.WarnFormat("Unable to find execution log directory: {0} for {1}", databasePath, serverAndDatabase);
                }
            }

            return String.Format("Unable to find directory \"{0}\" for requested database {1}", databasePath, serverAndDatabase);
        }

        /// <summary>
        /// Tries to pull the log file with the closet matching file date to the submitted date.
        /// </summary>
        /// <param name="databasePathLogPath">Path to search to get log files</param>
        /// <param name="logEntryDateStamp">Date that the log entry was made in the Errors.log or Commits.log file. This usually be slightly greater than the last write time of the actual file</param>
        /// <returns>FileInfo of the first file that was created on or first after the log entry DateStamp date. Otherwise, returns latest created file. Return null if nothing is there.</returns>
        private FileInfo GetCorrespondingLogFile(string databasePathLogPath, DateTime logEntryDateStamp)
        {
            if (Directory.Exists(databasePathLogPath))
            {
                //The date stamp should be after the last write date, but lets give it a 10 second window. 
                //If this is coming from a log-file link, there could be some differential.
                logEntryDateStamp = (logEntryDateStamp != DateTime.MaxValue) ? logEntryDateStamp.AddSeconds(10) : logEntryDateStamp;

                FileInfo[] fileInf = new DirectoryInfo(databasePathLogPath).GetFiles("LogFile*.log");
                try
                {
                    if (logEntryDateStamp != DateTime.MaxValue)
                    {
                        FileInfo fi = (from f in fileInf
                                       orderby f.LastWriteTime descending
                                       where f.LastWriteTime <= logEntryDateStamp
                                       select f).First();
                        return fi;
                    }
                    else
                    {
                        FileInfo fi = (from f in fileInf orderby f.LastWriteTime descending select f).First();
                        return fi;
                    }
                }
                catch (Exception exe)
                {

                    log.Warn(String.Format("Unable to retrieve log file for submitted date {0}. Returning null", logEntryDateStamp.ToString()), exe);
                    return null;
                }
            }
            return null;

        }
        #endregion

        private enum LogType
        {
            Commits,
            Errors, 
            FailureDatabases
        }
    }
}
