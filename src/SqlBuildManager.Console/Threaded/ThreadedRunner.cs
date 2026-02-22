using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace SqlBuildManager.Console.Threaded
{
    class ThreadedRunner
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);
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
        private RunnerReturn returnValue;

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
        public RunnerReturn ReturnValue
        {
            get { return returnValue; }
        }

        private string server = string.Empty;
        public string Server
        {
            get { return server; }
        }
        private string targetDatabases = string.Empty;
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

        private string concurrencyTag = string.Empty;
        public string ConcurrencyTag
        {
            get
            {
                return concurrencyTag;
            }
            set
            {
                concurrencyTag = value;
            }
        }
        public string DacpacName { get; set; } = string.Empty;
        private string username = string.Empty;
        private string password = string.Empty;
        private AuthenticationType authType = AuthenticationType.Password;
        private string buildRequestedBy = string.Empty;
        private ThreadedLogging threadedLog = null!;
        private string jobName = string.Empty;
        private readonly BuildExecutionContext _context;

        public ThreadedRunner(string serverName, List<DatabaseOverride> overrides, CommandLineArgs cmdArgs, string buildRequestedBy, bool forceCustomDacpac, BuildExecutionContext context = null!)
        {
            _context = context ?? new BuildExecutionContext();
            if (serverName.StartsWith("#"))
            {
                this.server = overrides[0].Server ?? string.Empty;
            }   
            else
            {
                this.server = serverName;
            }
            this.concurrencyTag = overrides[0].ConcurrencyTag;
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
        /// <param name="threadedLog">The logging instance for threaded execution</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>Return code indicating success or failure</returns>
        internal async Task<RunnerReturn> RunDatabaseBuildAsync(ThreadedLogging threadedLog, CancellationToken cancellationToken = default)
        {
            returnValue = RunnerReturn.BuildResultInconclusive;
            this.threadedLog = threadedLog;
            this.jobName = cmdArgs.JobName;
            ConnectionData connData = null!;
            SqlBuildRunDataModel runDataModel = new SqlBuildRunDataModel();
            string targetDatabase = overrides[0].OverrideDbTarget;
            string loggingDirectory = Path.Combine(_context.WorkingDirectory, server, targetDatabase);
            try
            {
                //Start setting properties on the object that contains the run configuration data.
                runDataModel.BuildType = "Other";
                if (!string.IsNullOrEmpty(cmdArgs.Description))
                    runDataModel.BuildDescription = cmdArgs.Description;
                else
                    runDataModel.BuildDescription = "Threaded Multi-Database. Run ID:" + _context.RunId;

                runDataModel.IsTrial = isTrial;
                runDataModel.RunScriptOnly = false;
                runDataModel.TargetDatabaseOverrides = overrides;
                runDataModel.Server = server;
                runDataModel.IsTransactional = cmdArgs.Transactional;
                if (cmdArgs.LogToDatabaseName.Length > 0)
                    runDataModel.LogToDatabaseName = cmdArgs.LogToDatabaseName;

                runDataModel.PlatinumDacPacFileName = cmdArgs.DacPacArgs.PlatinumDacpac;
                runDataModel.BuildRevision = cmdArgs.BuildRevision;
                runDataModel.DefaultScriptTimeout = cmdArgs.DefaultScriptTimeout;
                runDataModel.AllowObjectDelete = cmdArgs.AllowObjectDelete;


                //Initilize the logging directory for this run
                if (!Directory.Exists(loggingDirectory))
                {
                    Directory.CreateDirectory(loggingDirectory);
                }

                if (forceCustomDacpac)
                {
                    runDataModel.ForceCustomDacpac = true;
                    //This will set the BuildData and BuildFileName and ProjectFileName properties on runData
                    (var status, runDataModel) = await DacPacHelper.UpdateBuildRunDataForDacPacSyncAsync(runDataModel, server, targetDatabase, authType, username, password, loggingDirectory, cmdArgs.BuildRevision, cmdArgs.DefaultScriptTimeout, cmdArgs.AllowObjectDelete, cmdArgs.IdentityArgs.ClientId, cancellationToken).ConfigureAwait(false);
                    switch (status)
                    {
                        case DacpacDeltasStatus.Success:
                            //nothing to do
                            break;
                        case DacpacDeltasStatus.InSync:
                        case DacpacDeltasStatus.OnlyPostDeployment:
                            log.LogInformation($"Target database {targetDatabase} is already in sync with {cmdArgs.DacPacArgs.PlatinumDacpac}. Nothing to do!");
                            returnValue = RunnerReturn.DacpacDatabasesInSync;
                            break;
                        default:
                            log.LogError($"Error creating custom dacpac and scripts for {targetDatabase}. No update was performed");
                            returnValue = RunnerReturn.PackageCreationError;
                            return RunnerReturn.PackageCreationError;

                    }

                }
                else
                {
                    runDataModel.ForceCustomDacpac = false;
                    //Get a full copy of the build data to work with (avoid threading sync issues)
                    SqlSyncBuildDataModel cloned = _context.BuildDataModel;
                    //Clear out any existing CommittedScript data.. just log what is relevant to this run.
                    cloned.CommittedScript = new List<SqlSync.SqlBuild.Models.CommittedScript>();

                    runDataModel.BuildDataModel = cloned;
                    runDataModel.ProjectFileName = Path.Combine(loggingDirectory, Path.GetFileName(_context.ProjectFileName));
                    await SqlSyncBuildDataXmlSerializer.SaveAsync(runDataModel.ProjectFileName, cloned);
                    runDataModel.BuildFileName = _context.BuildZipFileName;
                }



                //Create a connection object.. all we need is the server here, the DB will be filled in at execution time
                connData = new ConnectionData(server, "");
                if (cmdArgs.AuthenticationArgs.UserName.Length > 0 && cmdArgs.AuthenticationArgs.Password.Length > 0)
                {
                    connData.UserId = cmdArgs.AuthenticationArgs.UserName;
                    connData.Password = cmdArgs.AuthenticationArgs.Password;
                }
                connData.AuthenticationType = cmdArgs.AuthenticationArgs.AuthenticationType;
                connData.ManagedIdentityClientId = cmdArgs.IdentityArgs.ClientId;
                connData.DatabasePlatform = cmdArgs.AuthenticationArgs.DatabasePlatform;

                // For PG MI auth, use identity name as UserId (PG role name)
                if (connData.DatabasePlatform == SqlSync.Connection.DatabasePlatform.PostgreSQL
                    && string.IsNullOrEmpty(connData.UserId)
                    && !string.IsNullOrEmpty(cmdArgs.IdentityArgs.IdentityName))
                {
                    connData.UserId = cmdArgs.IdentityArgs.IdentityName;
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Error Initializing run for {TargetTag}");
                WriteErrorLog(loggingDirectory, exe.ToString());
                returnValue = ExecutionReturn.RunInitializationError.ToRunnerReturn();
                return ExecutionReturn.RunInitializationError.ToRunnerReturn();
            }

            log.LogDebug("Initializing run for " + TargetTag + ". Starting \"ProcessBuild\"");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                //Initilize the run helper object and kick it off.
                SqlBuildHelper helper = new SqlBuildHelper(connData, true, string.Empty, cmdArgs.Transactional); //don't need an "external" log for this, it's all external!
                helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
                helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
                helper.BuildSuccessTrialRolledBackEvent += new EventHandler(helper_BuildSuccessTrialRolledBackEvent);

                //Determine whether or not to log each script result to EventhHub
                if (cmdArgs.EventHubLogging.Contains(EventHubLogging.ConsolidatedScriptResults) 
                     || cmdArgs.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults)
                     || cmdArgs.EventHubLogging.Contains(EventHubLogging.ScriptErrors))
                {
                    helper.ScriptLogWriteEvent += new ScriptLogWriteEventHandler(helper_ScriptLogWriteEvent);
                }

                if(runDataModel.BuildDataModel == null) runDataModel.BuildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

                var result = await helper.ProcessBuildAsync(runDataModel, cmdArgs.TimeoutRetryCount, buildRequestedBy, _context.BatchCollection);
                returnValue = result.FinalStatus!.Value.ToRunnerReturn();


            }
            catch (OperationCanceledException)
            {
                log.LogWarning($"Build for {TargetTag} was cancelled");
                returnValue = ExecutionReturn.ProcessBuildError.ToRunnerReturn();
                throw;
            }
            catch (Exception exe)
            {
                log.LogError("Error Processing run for " + TargetTag, exe);
                WriteErrorLog(loggingDirectory, exe.ToString());
                returnValue = ExecutionReturn.ProcessBuildError.ToRunnerReturn();
                return ExecutionReturn.ProcessBuildError.ToRunnerReturn();
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
            return RunnerReturn.BuildCommitted;

        }

        private StringBuilder consolidatedScriptLog = new();
        private string targetDb = string.Empty;
        private void helper_ScriptLogWriteEvent(object sender, bool isError, ScriptLogEventArgs e)
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
            else if (cmdArgs.EventHubLogging.Contains(EventHubLogging.ScriptErrors) && isError)
            {
                LogMsg lm = new LogMsg()
                {
                    LogType = LogType.ScriptError,
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
        }

        void helper_BuildSuccessTrialRolledBackEvent(object? sender, EventArgs e)
        {
            log.LogDebug(TargetTag + " BuildSuccessTrialRolledBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.SuccessWithTrialRolledBack));
            returnValue = RunnerReturn.SuccessWithTrialRolledBack;
        }

        void helper_BuildErrorRollBackEvent(object? sender, EventArgs e)
        {
            if (IsTransactional)
            {
                log.LogDebug(TargetTag + " BuildErrorRollBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.RolledBack));
                returnValue = RunnerReturn.RolledBack;
            }
            else
            {
                log.LogDebug(TargetTag + " BuildErrorRollBackEvent status: " + Enum.GetName(typeof(RunnerReturn), RunnerReturn.BuildErrorNonTransactional));
                returnValue = RunnerReturn.BuildErrorNonTransactional;
            }
        }

        void helper_BuildCommittedEvent(object sender, RunnerReturn returnValue)
        {
            log.LogDebug(TargetTag + " BuildCommittedEvent status: " + Enum.GetName(typeof(RunnerReturn), returnValue));
            this.returnValue = returnValue;
        }

        private void WriteErrorLog(string loggingDirectory, string message)
        {
            File.WriteAllText(loggingDirectory + "Error.log", message);
        }

    }
}
