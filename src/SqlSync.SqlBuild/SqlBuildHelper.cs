using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.SqlLogging;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using BuildModels = SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for SqlBuildHelper.
    /// </summary>
    public class SqlBuildHelper : ISqlBuildRunnerContext, IBuildFinalizerContext
    {
        internal static Func<ISqlBuildRunnerContext, ISqlCommandExecutor, SqlBuildRunner> SqlBuildRunnerFactory = (ctx, exec) => new SqlBuildRunner(ctx, exec);
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal bool isTransactional = true;
        internal List<DatabaseOverride> targetDatabaseOverrides = null;
        public List<DatabaseOverride> TargetDatabaseOverrides
        {
            get { return targetDatabaseOverrides; }
            set { targetDatabaseOverrides = value; }
        }
        internal BackgroundWorker bgWorker;
        /// <summary>
        /// Indicator for external callers that an error occurred. Can also subscribe to ScriptingErrorEvent
        /// </summary>
        public bool ErrorOccured;
        /// <summary>
        /// POCO model holding build configuration data
        /// </summary>
        internal BuildModels.SqlSyncBuildDataModel buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
        /// <summary>
        /// Optional reference to original DataSet for backward compatibility tests
        /// </summary>
        internal SqlSyncBuildData buildDataCompat;

        internal sealed record BuildPreparationResult(
            IReadOnlyList<BuildModels.Script> FilteredScripts,
            BuildModels.Build Build,
            string BuildPackageHash
        );
        /// <summary>
        /// Data collection used to pass connection data
        /// </summary>
        private ConnectionData connData;
        private string buildType;
        /// <summary>
        /// User description of the build
        /// </summary>
        internal string buildDescription;
        public string BuildDescription => buildDescription;
        /// <summary>
        /// Index where the build should start
        /// </summary>
        private double startIndex;
        /// <summary>
        /// Dictionary containing the BuildConnectData objects with the server/database as key
        /// </summary>
        private Dictionary<string, BuildConnectData> connectDictionary = new Dictionary<string, BuildConnectData>();
        /// <summary>
        /// Name of the Sql Build project (sbm) file
        /// </summary>
        internal string projectFileName;
        /// <summary>
        /// Holding string for the last message from Sql server
        /// </summary>
        private string lastSqlMessage = string.Empty;
        /// <summary>
        /// Path to the Sql Build project (sbm) file
        /// </summary>
        private string projectFilePath = string.Empty;
        /// <summary>
        /// Current run for 
        /// </summary>
        private BuildModels.ScriptRun currentRun;
        /// <summary>
        /// Flag for determining if this is a trial (rollback)
        /// </summary>
        internal bool isTrialBuild = false;
        /// <summary>
        /// Name of the file being used to store scripts
        /// </summary>
        internal string scriptLogFileName;
        /// <summary>
        /// String to hold the Sql event messages
        /// </summary>
        private string sqlInfoMessage = string.Empty;
        /// <summary>
        /// Index of the current script of the batch
        /// </summary>
        private int currentBatchScriptIndex;
        /// <summary>
        /// Index of the single script file to run (if single file run)
        /// </summary>
        internal double[] runItemIndexes = new double[0];
        /// <summary>
        /// Id of the current build
        /// </summary>
        private System.Guid currentBuildId = System.Guid.Empty;
        /// <summary>
        /// Flag use to set "script only" setting
        /// </summary>
        internal bool runScriptOnly = false;
        /// <summary>
        /// Name of the build file (.sbm)
        /// </summary>
        internal string buildFileName = string.Empty;
        public string BuildFileName => buildFileName;
        /// <summary>
        /// Names of scripts selected in the GUI
        /// </summary>
        private string[] selectedScriptIds = null;
        /// <summary>
        /// Public accessor to get the GUID of the current build
        /// </summary>
        public System.Guid CurrentBuildId
        {
            get
            {
                return currentBuildId;
            }
        }
        /// <summary>
        /// The hash signature of the build package scripts
        /// </summary>
        internal string buildPackageHash = string.Empty;
        public string BuildPackageHash => buildPackageHash;
        /// <summary>
        /// Database to write commits to if an alternate logging database has been specified
        /// </summary>
        private string logToDatabaseName = string.Empty;
        /// <summary>
        /// Database to write commits to if an alternate logging database has been specified
        /// </summary>
        public string LogToDatabaseName
        {
            get { return logToDatabaseName; }
            set { logToDatabaseName = value; }
        }
        /// <summary>
        /// List of the scripts that have been run and will be committed when the build is committed.
        /// </summary>
        private List<LoggingCommittedScript> committedScripts = new List<LoggingCommittedScript>();
        /// <summary>
        /// Used by the remote service to record the user that requested the build (vs. the user id used in execution) since they may be different
        /// </summary>
        private string buildRequestedBy = string.Empty;

        internal string buildHistoryXmlFile = string.Empty;
        internal SqlSyncBuildData buildHistoryData = null;
        private BuildModels.SqlSyncBuildDataModel buildHistoryModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

        internal BuildModels.SqlSyncBuildDataModel BuildHistoryModel => buildHistoryModel;
        internal BuildModels.SqlSyncBuildDataModel BuildDataModel
        {
            get => buildDataModel;
            set => buildDataModel = value;
        }

        // New dependencies (Phase 2 seams)
        internal IClock Clock { get; }
        internal IGuidProvider GuidProvider { get; }
        internal IFileSystem FileSystem { get; }
        internal IProgressReporter ProgressReporter { get; private set; }
        internal ISqlBuildFileHelper FileHelper { get; }
        internal IBuildRetryPolicy RetryPolicy { get; }
        internal IBuildFinalizer BuildFinalizer { get; }
        internal ILegacyBuildDataAdapter LegacyAdapter { get; }
        internal Services.IBuildPreparationService BuildPreparationService { get; }
        internal Services.IScriptBatcher ScriptBatcher { get; }
        internal Services.ITokenReplacementService TokenReplacementService { get; }
        internal Services.ISqlLoggingService SqlLoggingService { get; }

        internal static BuildModels.SqlSyncBuildDataModel ClearAllowScriptBlocks(BuildModels.SqlSyncBuildDataModel model, string serverName, IReadOnlyList<string> selectedScriptIds)
        {
            var updatedCommitted = model.CommittedScript.ToList();
            var idSet = new HashSet<string>(selectedScriptIds, StringComparer.OrdinalIgnoreCase);
            for (var j = 0; j < updatedCommitted.Count; j++)
            {
                var cs = updatedCommitted[j];
                if (cs.ScriptId != null && idSet.Contains(cs.ScriptId) && string.Equals(cs.ServerName, serverName, StringComparison.OrdinalIgnoreCase))
                {
                    updatedCommitted[j] = cs with { AllowScriptBlock = false };
                }
            }
            return model with { CommittedScript = updatedCommitted };
        }

        private static SqlBuildRunData MapToLegacyRunData(BuildModels.SqlBuildRunDataModel model)
        {
            return new SqlBuildRunData
            {
                BuildDataModel = model.BuildDataModel,
                BuildType = model.BuildType ?? string.Empty,
                Server = model.Server ?? string.Empty,
                BuildDescription = model.BuildDescription ?? string.Empty,
                StartIndex = model.StartIndex ?? 0,
                ProjectFileName = model.ProjectFileName ?? string.Empty,
                IsTrial = model.IsTrial ?? false,
                RunItemIndexes = model.RunItemIndexes?.ToArray() ?? Array.Empty<double>(),
                RunScriptOnly = model.RunScriptOnly ?? false,
                BuildFileName = model.BuildFileName ?? string.Empty,
                LogToDatabaseName = model.LogToDatabaseName ?? string.Empty,
                IsTransactional = model.IsTransactional ?? true,
                PlatinumDacPacFileName = model.PlatinumDacPacFileName ?? string.Empty,
                TargetDatabaseOverrides = model.TargetDatabaseOverrides?.ToList(),
                ForceCustomDacpac = model.ForceCustomDacpac ?? false,
                BuildRevision = model.BuildRevision ?? string.Empty,
                DefaultScriptTimeout = model.DefaultScriptTimeout ?? 500,
                AllowObjectDelete = model.AllowObjectDelete ?? false
            };
        }

        private static BuildModels.SqlBuildRunDataModel MapToRunDataModel(SqlBuildRunData runData)
        {
            return new BuildModels.SqlBuildRunDataModel(
                BuildDataModel: runData.BuildDataModel,
                BuildType: runData.BuildType,
                Server: runData.Server,
                BuildDescription: runData.BuildDescription,
                StartIndex: runData.StartIndex,
                ProjectFileName: runData.ProjectFileName,
                IsTrial: runData.IsTrial,
                RunItemIndexes: runData.RunItemIndexes,
                RunScriptOnly: runData.RunScriptOnly,
                BuildFileName: runData.BuildFileName,
                LogToDatabaseName: runData.LogToDatabaseName,
                IsTransactional: runData.IsTransactional,
                PlatinumDacPacFileName: runData.PlatinumDacPacFileName,
                TargetDatabaseOverrides: runData.TargetDatabaseOverrides,
                ForceCustomDacpac: runData.ForceCustomDacpac,
                BuildRevision: runData.BuildRevision,
                DefaultScriptTimeout: runData.DefaultScriptTimeout,
                AllowObjectDelete: runData.AllowObjectDelete);
        }


        private static BuildModels.Script MapLegacyScriptRowToModel(SqlSyncBuildData.ScriptRow row) => row.ToModel();

        private static BuildModels.Build MapLegacyBuildRowToModel(SqlSyncBuildData.BuildRow row) => row.ToModel();
        private void SyncBuildDataModel(SqlSyncBuildData ds) => buildDataModel = ds.ToModel();

        internal void SetBuildData(SqlSyncBuildData ds)
        {
            buildDataCompat = ds;
            BuildDataModel = ds.ToModel();
        }



        internal string externalScriptLogFileName = string.Empty;

        public SqlBuildHelper(
            ConnectionData data,
            bool createScriptRunLogFile = true,
            string externalScriptLogFileName = "",
            bool isTransactional = true,
            IClock clock = null,
            IGuidProvider guidProvider = null,
            IFileSystem fileSystem = null,
            IProgressReporter progressReporter = null,
            ISqlBuildFileHelper fileHelper = null,
            IBuildRetryPolicy retryPolicy = null,
            IBuildFinalizer buildFinalizer = null,
            ILegacyBuildDataAdapter legacyAdapter = null)
        {
            connData = data;
            this.isTransactional = isTransactional;
            Clock = clock ?? new SystemClock();
            GuidProvider = guidProvider ?? new GuidProvider();
            FileSystem = fileSystem ?? new DotNetFileSystem();
            ProgressReporter = progressReporter; // fallback to BackgroundWorker when available
            FileHelper = fileHelper ?? new DefaultSqlBuildFileHelper();
            RetryPolicy = retryPolicy ?? new DefaultBuildRetryPolicy();
            BuildFinalizer = buildFinalizer ?? new DefaultBuildFinalizer();
            LegacyAdapter = legacyAdapter ?? new DefaultLegacyBuildDataAdapter();
            BuildPreparationService = new Services.DefaultBuildPreparationService(this);
            ScriptBatcher = new Services.DefaultScriptBatcher();
            TokenReplacementService = new Services.DefaultTokenReplacementService();
            SqlLoggingService = new Services.DefaultSqlLoggingService(this);

            if (createScriptRunLogFile)
                ScriptLogWriteEvent += new ScriptLogWriteEventHandler(SqlBuildHelper_ScriptLogWriteEvent);

            if (externalScriptLogFileName != null && externalScriptLogFileName.Length > 0)
                this.externalScriptLogFileName = externalScriptLogFileName;

        }

        public void ProcessMultiDbBuild(MultiDbData multiDbRunData, string projectFileName, BackgroundWorker bgWorker, DoWorkEventArgs e)
        {
            this.projectFileName = projectFileName;
            ProcessMultiDbBuild(multiDbRunData, bgWorker, e); // Call to process the multi-db build
        }
        /// <summary>
        /// Used when the user wants to run the build across multiple databases and/or servers
        /// </summary>
        /// <param name="multiDbRunData"></param>
        /// <param name="bgWorker"></param>
        /// <param name="e"></param>
        public void ProcessMultiDbBuild(MultiDbData multiDbRunData, BackgroundWorker bgWorker, DoWorkEventArgs e)
        {
            this.bgWorker = bgWorker;
            committedScripts.Clear();
            connectDictionary.Clear();
            var buildResults = new List<BuildModels.Build>();

            foreach (var srvData in multiDbRunData)
            {
                var runData = new BuildModels.SqlBuildRunDataModel(
                    BuildDataModel: multiDbRunData.BuildData,
                    BuildType: "Multi Db Build",
                    Server: srvData.ServerName,
                    BuildDescription: string.IsNullOrEmpty(multiDbRunData.BuildDescription)
                        ? $"Multi-Database Run ID: {multiDbRunData.MultiRunId}.  Server:{srvData.ServerName}"
                        : multiDbRunData.BuildDescription,
                    StartIndex: 0,
                    ProjectFileName: multiDbRunData.ProjectFileName,
                    IsTrial: multiDbRunData.RunAsTrial,
                    RunItemIndexes: Array.Empty<double>(),
                    RunScriptOnly: false,
                    BuildFileName: multiDbRunData.BuildFileName,
                    LogToDatabaseName: string.Empty,
                    IsTransactional: true,
                    PlatinumDacPacFileName: string.Empty,
                    TargetDatabaseOverrides: srvData.Overrides,
                    ForceCustomDacpac: false,
                    BuildRevision: string.Empty,
                    DefaultScriptTimeout: 500,
                    AllowObjectDelete: false);

                targetDatabaseOverrides = srvData.Overrides;
                var buildResult = ProcessBuild(runData, bgWorker, e, srvData.ServerName, true, null, multiDbRunData.AllowableTimeoutRetries);
                buildResults.Add(buildResult);

                if (buildResult.FinalStatus == BuildItemStatus.PendingRollBack || buildResult.FinalStatus == BuildItemStatus.FailedNoTransaction)
                {
                    PerformRunScriptFinalization(true, buildResults, multiDbRunData, bgWorker, ref e);
                    return;
                }
            }

            PerformRunScriptFinalization(false, buildResults, multiDbRunData, bgWorker, ref e);
        }


        //public BuildModels.Build ProcessBuild(BuildModels.SqlBuildRunDataModel runData, int allowableTimeoutRetries, BackgroundWorker bgWorker, DoWorkEventArgs e)
        //{
        //    connectDictionary.Clear();
        //    committedScripts.Clear();

        //    return ProcessBuild(runData, bgWorker, e, connData.SQLServerName, false, null, allowableTimeoutRetries);
        //}

        public BuildModels.Build ProcessBuild(BuildModels.SqlBuildRunDataModel runData, BackgroundWorker bgWorker, DoWorkEventArgs e, int allowableTimeoutRetries = 3,  string buildRequestedBy = "", ScriptBatchCollection scriptBatchColl = null)
        {
            connectDictionary.Clear();
            committedScripts.Clear();
            this.buildRequestedBy = buildRequestedBy;

            return ProcessBuild(runData: runData, bgWorker: bgWorker, e: e, serverName: connData.SQLServerName, isMultiDbRun: false, scriptBatchColl: scriptBatchColl, allowableTimeoutRetries: allowableTimeoutRetries);
        }

        internal BuildModels.Build ProcessBuild(BuildModels.SqlBuildRunDataModel runData, BackgroundWorker bgWorker, DoWorkEventArgs e, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, int allowableTimeoutRetries)
        {
            this.bgWorker = bgWorker;
            ErrorOccured = false;
            buildDataModel = runData.BuildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            buildType = runData.BuildType ?? string.Empty;
            buildDescription = runData.BuildDescription ?? string.Empty;
            startIndex = runData.StartIndex ?? 0;
            projectFileName = runData.ProjectFileName ?? string.Empty;
            projectFilePath = Path.GetDirectoryName(projectFileName);
            isTrialBuild = runData.IsTrial ?? false;
            runItemIndexes = runData.RunItemIndexes?.ToArray() ?? Array.Empty<double>();
            runScriptOnly = runData.RunScriptOnly ?? false;
            buildFileName = Path.GetFileName(runData.BuildFileName ?? string.Empty);
            targetDatabaseOverrides = runData.TargetDatabaseOverrides?.ToList();
            logToDatabaseName = runData.LogToDatabaseName ?? string.Empty;

            if (bgWorker != null)
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs($"Starting Build Process targeting: {serverName} "));

            var prep = PrepareBuildForRun(buildDataModel, serverName, isMultiDbRun, scriptBatchColl, ref e);
            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return prep.Build;
            }

            var orchestrator = new Services.SqlBuildOrchestrator(this);
            var buildResultsModel = orchestrator.Execute(runData, prep, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);

            bool candidateForCustomDacPac = false;
            switch (buildResultsModel.FinalStatus)
            {
                case BuildItemStatus.Committed:
                case BuildItemStatus.CommittedWithTimeoutRetries:
                case BuildItemStatus.AlreadyInSync:
                case BuildItemStatus.TrialRolledBack:
                case BuildItemStatus.CommittedWithCustomDacpac:
                case BuildItemStatus.Pending:
                    candidateForCustomDacPac = false;
                    break;
                case BuildItemStatus.FailedDueToScriptTimeout:
                case BuildItemStatus.FailedWithCustomDacpac:
                    candidateForCustomDacPac = false;
                    log.LogWarning($"Build was not successful. Status is {buildResultsModel.FinalStatus} and Platinum DACPAC name is '{runData.PlatinumDacPacFileName}', and this file exists '{File.Exists(runData.PlatinumDacPacFileName ?? string.Empty)}' ");
                    break;
                case BuildItemStatus.RolledBack:
                case BuildItemStatus.PendingRollBack:
                case BuildItemStatus.FailedNoTransaction:
                case BuildItemStatus.RolledBackAfterRetries:
                    candidateForCustomDacPac = true;
                    break;
                default:
                    log.LogWarning($"Unrecognized Build Item status of {buildResultsModel.FinalStatus}");
                    candidateForCustomDacPac = true;
                    break;
            }
            //Do we need to try to update the target using the Platinum Dacpac?
            if (candidateForCustomDacPac && !string.IsNullOrEmpty(runData.PlatinumDacPacFileName) && File.Exists(runData.PlatinumDacPacFileName) && !(runData.ForceCustomDacpac ?? false))
            {
                var database = prep.FilteredScripts[0].Database;
                string targetDatabase = GetTargetDatabase(database);
                log.LogWarning($"Custom dacpac required for {serverName} : {targetDatabase}. Generating file.");
                var legacyRunData = MapToLegacyRunData(runData);
                var stat = DacPacHelper.UpdateBuildRunDataForDacPacSync(ref legacyRunData, serverName, targetDatabase, connData.AuthenticationType, connData.UserId, connData.Password, projectFilePath, runData.BuildRevision ?? string.Empty, runData.DefaultScriptTimeout ?? 500, runData.AllowObjectDelete ?? false, connData.ManagedIdentityClientId);

                if (stat == DacpacDeltasStatus.Success)
                {
                    var updatedRunData = MapToRunDataModel(legacyRunData) with { PlatinumDacPacFileName = string.Empty };
                    log.LogInformation($"Executing custom dacpac on {targetDatabase}");
                    var dacBuild = ProcessBuild(updatedRunData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
                    var dacFinalStatus = dacBuild.FinalStatus;
                    if (dacFinalStatus == BuildItemStatus.Committed.ToString() || dacFinalStatus == BuildItemStatus.CommittedWithTimeoutRetries.ToString())
                    {
                        buildResultsModel = buildResultsModel with { FinalStatus = BuildItemStatus.CommittedWithCustomDacpac.ToString() };
                        if (BuildCommittedEvent != null)
                            BuildCommittedEvent(this, RunnerReturn.CommittedWithCustomDacpac);
                    }
                    else
                    {
                        buildResultsModel = buildResultsModel with { FinalStatus = BuildItemStatus.FailedWithCustomDacpac.ToString() };
                    }
                }
                else if (stat == DacpacDeltasStatus.InSync || stat == DacpacDeltasStatus.OnlyPostDeployment)
                {
                    buildResultsModel = buildResultsModel with { FinalStatus = BuildItemStatus.AlreadyInSync.ToString() };
                    if (BuildCommittedEvent != null)
                        BuildCommittedEvent(this, RunnerReturn.DacpacDatabasesInSync);
                }

            }

            //If a timeout gets here.. need to decide how to label the rollback
            if (buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout.ToString())
            {
                buildResultsModel = buildResultsModel with { FinalStatus = allowableTimeoutRetries > 0 ? BuildItemStatus.RolledBackAfterRetries.ToString() : BuildItemStatus.RolledBack.ToString() };
            }

            switch (buildResultsModel.FinalStatus)
            {
                case BuildItemStatus.Committed:
                case BuildItemStatus.Pending:
                case BuildItemStatus.CommittedWithTimeoutRetries:
                case BuildItemStatus.TrialRolledBack:
                case BuildItemStatus.AlreadyInSync:
                case BuildItemStatus.CommittedWithCustomDacpac:
                    break;
                default:
                    log.LogWarning($"Build was not successful. Status is {buildResultsModel.FinalStatus} and Platinum DACPAC name is '{runData.PlatinumDacPacFileName}', and this file exists '{File.Exists(runData.PlatinumDacPacFileName ?? string.Empty)}' ");
                    break;

            }

            SyncBuildDataModel(buildDataModel.ToDataSet());
            return buildResultsModel;
        }

        internal BuildPreparationResult PrepareBuildForRun(BuildModels.SqlSyncBuildDataModel buildDataModelParam, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, ref DoWorkEventArgs workEventArgs)
        {
            try
            {
                EnsureBgWorker();
                // Make sure the project file is not read-only
                if (File.Exists(projectFileName))
                {
                    File.SetAttributes(projectFileName, FileAttributes.Normal);
                }
                else
                {
                    log.LogError($"[PrepareBuildForRunModel] projectFileName does not exist: '{projectFileName}'");
                }

                if (string.IsNullOrWhiteSpace(projectFilePath))
                {
                    if (!string.IsNullOrWhiteSpace(projectFileName))
                    {
                        projectFilePath = Path.GetDirectoryName(projectFileName);
                    }
                    if (string.IsNullOrWhiteSpace(projectFilePath))
                    {
                        projectFilePath = Path.GetTempPath();
                    }
                }
                buildHistoryXmlFile = Path.Combine(projectFilePath, SqlBuild.XmlFileNames.HistoryFile);
                log.LogDebug($"[PrepareBuildForRunModel] projectFilePath='{projectFilePath}'");

                bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Creating Script Log File"));
                scriptLogFileName = Path.Combine(projectFilePath, $"LogFile-{DateTime.Now:yyyy-MM-dd at HH_mm_ss}.log");

                bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Generating Build Record"));
                var nextBuildId = (buildDataModelParam.Build.Count > 0 ? buildDataModelParam.Build.Max(b => b.Build_Id) + 1 : 1);
                var myBuild = new BuildModels.Build(
                    Name: buildDescription,
                    BuildType: buildType,
                    BuildStart: DateTime.Now,
                    BuildEnd: null,
                    ServerName: serverName,
                    FinalStatus: null,
                    BuildId: Guid.NewGuid().ToString(),
                    UserId: Environment.UserName,
                    Build_Id: nextBuildId,
                    Builds_Id: null);

                // add the build to model
                var builds = buildDataModelParam.Build?.ToList() ?? new List<BuildModels.Build>();
                builds.Add(myBuild);
                buildDataModelParam = buildDataModelParam with { Build = builds };

                bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Reading Scripting configuration"));
                var scripts = buildDataModelParam.Script ?? Array.Empty<BuildModels.Script>();

                var filtered = scripts
                    .Where(s => s != null)
                    .OrderBy(s => s.BuildOrder ?? double.MaxValue)
                    .ToList();

                if (runItemIndexes.Length > 0)
                {
                    var indexSet = new HashSet<double>(runItemIndexes);
                    filtered = filtered.Where(s => (s.BuildOrder ?? double.MaxValue) != double.MaxValue && indexSet.Contains(s.BuildOrder!.Value)).ToList();
                }
                else
                {
                    filtered = filtered.Where(s => (s.BuildOrder ?? double.MaxValue) >= startIndex).ToList();
                }

                if (filtered.Count == 0)
                {
                    if (isMultiDbRun)
                        myBuild = myBuild with { FinalStatus = BuildItemStatus.PendingRollBack.ToString() };
                    else
                        myBuild = myBuild with { FinalStatus = BuildItemStatus.RolledBack.ToString() };

                    return new BuildPreparationResult(Array.Empty<BuildModels.Script>(), myBuild, string.Empty);
                }

                if (scriptBatchColl == null)
                    buildPackageHash = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFilePath, buildDataModelParam.ToDataSet());
                else
                    buildPackageHash = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(scriptBatchColl);

                log.LogInformation($"Prepared build for run. Build Package hash = {buildPackageHash}");
                return new BuildPreparationResult(filtered, myBuild, buildPackageHash);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "[PrepareBuildForRunModel] Exception");
                throw;
            }
        }


        private void EnsureBgWorker()
        {
            if (bgWorker == null)
            {
                var bg = new BackgroundWorker()
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                bgWorker = bg;
            }
        }

        internal BuildModels.Build RunBuildScripts(
            IReadOnlyList<BuildModels.Script> scripts,
            BuildModels.Build myBuild,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            BuildModels.SqlSyncBuildDataModel buildDataModel,
            ref DoWorkEventArgs workEventArgs)
        {
            var runner = SqlBuildRunnerFactory(this, null);
            return runner.Run(scripts, myBuild, serverName, isMultiDbRun, scriptBatchColl, buildDataModel, ref workEventArgs);
        }

    

        internal void ResetConnectionsForRetry()
        {
            try
            {
                foreach (var kvp in connectDictionary.ToList())
                {
                    try { kvp.Value.Transaction?.Dispose(); } catch { }
                    try { kvp.Value.Connection?.Close(); } catch { }
                    try { kvp.Value.Connection?.Dispose(); } catch { }
                }
            }
            catch { }
            connectDictionary.Clear();
            committedScripts.Clear();
        }

        internal BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs)
        {
            return BuildFinalizer.PerformRunScriptFinalization(this, buildFailure, myBuild, buildDataModel, ref workEventArgs);
        }

        internal void PerformRunScriptFinalization(bool buildFailure, List<BuildModels.Build> myBuilds, MultiDbData multiDbRunData, BackgroundWorker bgWorker, ref DoWorkEventArgs workEventArgs)
        {
            DateTime end = DateTime.Now;
            for (int i = 0; i < myBuilds.Count; i++)
                myBuilds[i] = myBuilds[i] with { BuildEnd = DateTime.Now };

            if (buildFailure)
            {
                ErrorOccured = true;
                for (int i = 0; i < myBuilds.Count; i++)
                    myBuilds[i] = myBuilds[i] with { FinalStatus = (isTransactional ? BuildItemStatus.RolledBack.ToString() : BuildItemStatus.FailedNoTransaction.ToString()) };

                if (!isTransactional)
                {
                    RecordCommittedScripts(committedScripts, buildDataModel, out var updatedModel);
                    buildDataModel = updatedModel;
                    LogCommittedScriptsToDatabase(committedScripts, multiDbRunData);
                }
            }
            else
            {
                if (!isTrialBuild)
                {
                    if (isTransactional)
                    {
                        bgWorker?.ReportProgress(0, new GeneralStatusEventArgs("Attempting to Commit Build"));
                        bool commitSuccess = CommitBuild();
                        if (commitSuccess)
                            bgWorker?.ReportProgress(0, new GeneralStatusEventArgs("Commit Successful"));
                    }

                    for (int i = 0; i < myBuilds.Count; i++)
                        myBuilds[i] = myBuilds[i] with { FinalStatus = BuildItemStatus.Committed.ToString() };

                    RecordCommittedScripts(committedScripts, buildDataModel, out var updatedModel);
                    buildDataModel = updatedModel;
                    LogCommittedScriptsToDatabase(committedScripts, multiDbRunData);

                    BuildCommittedEvent?.Invoke(this, RunnerReturn.BuildCommitted);
                }
                else
                {
                    if (isTransactional)
                    {
                        RollbackBuild();
                        for (int i = 0; i < myBuilds.Count; i++)
                            myBuilds[i] = myBuilds[i] with { FinalStatus = BuildItemStatus.TrialRolledBack.ToString() };
                        BuildSuccessTrialRolledBackEvent?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        for (int i = 0; i < myBuilds.Count; i++)
                            myBuilds[i] = myBuilds[i] with { FinalStatus = BuildItemStatus.Committed.ToString() };
                    }
                }
            }

            if (buildFailure)
                BuildErrorRollBackEvent?.Invoke(this, EventArgs.Empty);

            SaveBuildDataSet(true);

            if (buildFailure)
            {
                if (workEventArgs.Cancel)
                {
                    if (isTransactional)
                        bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                    else
                        bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                    workEventArgs.Result = isTransactional ? BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK : BuildResultStatus.BUILD_CANCELLED_NO_TRANSACTION;
                }
                else
                {
                    if (isTransactional)
                        bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                    else
                        bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                    workEventArgs.Result = isTransactional ? BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK : BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
                }
            }
            else
            {
                if (runScriptOnly)
                {
                    bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Script Generation Complete"));
                    workEventArgs.Result = BuildResultStatus.SCRIPT_GENERATION_COMPLETE;
                }
                else
                {
                    if (!isTrialBuild)
                    {
                        bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Committed"));
                        workEventArgs.Result = BuildResultStatus.BUILD_COMMITTED;
                    }
                    else
                    {
                        if (isTransactional)
                        {
                            bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Rolled back for Trial Build"));
                            workEventArgs.Result = BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL;
                        }
                        else
                        {
                            bgWorker?.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Committed with no transaction"));
                            workEventArgs.Result = BuildResultStatus.BUILD_COMMITTED;
                        }
                    }
                }
            }

            connectDictionary.Clear();
        }
 
        internal bool RecordCommittedScripts(List<CommittedScript> committedScripts, BuildModels.SqlSyncBuildDataModel buildDataModel, out BuildModels.SqlSyncBuildDataModel updatedModel)
        {
            var model = buildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Fast POCO path
            if (committedScripts != null)
            {
                var list = new List<BuildModels.CommittedScript>(model.CommittedScript);
                var projectId = model.SqlSyncBuildProject.Count > 0 ? model.SqlSyncBuildProject[0].SqlSyncBuildProject_Id : 0;
                foreach (var cs in committedScripts)
                {
                    list.Add(new BuildModels.CommittedScript(
                        cs.ScriptId.ToString(),
                        cs.ServerName,
                        DateTime.Now,
                        true,
                        cs.FileHash,
                        projectId));
                }
                updatedModel = model with { CommittedScript = list };
                return true;
            }

            updatedModel = model;
            return true;
        }

        public void ClearScriptBlocks(ClearScriptData scrData, BackgroundWorker bgWorker, DoWorkEventArgs e)
        {
            projectFileName = scrData.ProjectFileName;
            var model = scrData.BuildDataModel;
            if (model == null)
                throw new ArgumentException("ClearScriptData must provide BuildDataModel", nameof(scrData));
            buildFileName = scrData.BuildZipFileName;
            selectedScriptIds = scrData.SelectedScriptIds;
            this.bgWorker = bgWorker;

            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Clearing Script Blocks"));

            EnsureLogTablePresence();

            SqlCommand cmd = new SqlCommand("UPDATE SqlBuild_Logging SET AllowScriptBlock = 0, AllowBlockUpdateId = @UserId WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1");
            cmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            cmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            var scriptsById = model.Script
                .Where(s => s.ScriptId != null)
                .ToDictionary(s => s.ScriptId!, s => s, StringComparer.OrdinalIgnoreCase);
            var updatedCommitted = model.CommittedScript.ToList();
            for (int i = 0; i < selectedScriptIds.Length; i++)
            {
                var id = selectedScriptIds[i];
                if (!scriptsById.TryGetValue(id, out var script))
                    continue;

                bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Clearing " + (script.FileName ?? id)));

                model = ClearAllowScriptBlocks(model, connData.SQLServerName, selectedScriptIds);

                //Update Sql server log
                string targetDatabase = GetTargetDatabase(script.Database ?? string.Empty);
                BuildConnectData cData = GetConnectionDataClass(connData.SQLServerName, targetDatabase);
                EnsureLogTablePresence();
                cmd.Connection = cData.Connection;
                cmd.Transaction = cData.Transaction;
                cmd.Parameters["@ScriptId"].Value = new System.Guid(id);
                cmd.ExecuteNonQuery();
            }

            buildDataModel = model;

            CommitBuild();
            SaveBuildDataSet(true);

            bgWorker.ReportProgress(100, new GeneralStatusEventArgs("Selected Script Blocks Cleared"));
        }

        /// <summary>
        /// Gets the database to actually execute against based on the set default and any matching override
        /// </summary>
        /// <param name="defaultDatabase">The default database set up in the SBM configuration</param>
        /// <returns>Actual database to execute against</returns>coo
        internal string GetTargetDatabase(string defaultDatabase)
        {
            if (targetDatabaseOverrides != null)
            {
                return ConnectionHelper.GetTargetDatabase(defaultDatabase, targetDatabaseOverrides);
            }
            else if (OverrideData.TargetDatabaseOverrides != null)
            {
                return ConnectionHelper.GetTargetDatabase(defaultDatabase, OverrideData.TargetDatabaseOverrides);
            }
            else
            {
                return defaultDatabase;
            }

        }
        //public static string GetTargetDatabase(string serverName, string defaultDatabase, MultiDbData multiDbRunData)
        //{
        //    foreach (ServerData srvData in multiDbRunData)
        //    {
        //        if (serverName.ToUpper() != srvData.ServerName.ToUpper())
        //            continue;

        //        foreach (KeyValuePair<string, List<DatabaseOverride>> sequence in srvData.Overrides)
        //        {
        //            List<DatabaseOverride> tmp = sequence.Value;
        //            for (int i = 0; i < tmp.Count; i++)
        //                if (tmp[i].DefaultDbTarget.ToUpper() == defaultDatabase.ToUpper())
        //                    return tmp[i].OverrideDbTarget;
        //        }
        //    }
        //    return defaultDatabase;
        //}

        #region ## SQL table logging ##
        /// <summary>
        /// Ensures that the SqlBuild_Logging table exists and that it is setup properly. Self-heals if it is not. 
        /// </summary>
        internal void EnsureLogTablePresence()
        {
            sqlInfoMessage = string.Empty;
            //Self healing: add the table if needed
            SqlCommand createTableCmd = new SqlCommand(Properties.Resources.LoggingTable);
            Dictionary<string, BuildConnectData>.KeyCollection keys = connectDictionary.Keys;
            foreach (string key in keys)
            {
                sqlInfoMessage = string.Empty;
                try
                {

                    createTableCmd.Connection = ((BuildConnectData)connectDictionary[key]).Connection;

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (logToDatabaseName.Length > 0 && !createTableCmd.Connection.Database.Equals(logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    if (createTableCmd.Connection.State == ConnectionState.Closed)
                        createTableCmd.Connection.Open();

                    if (((BuildConnectData)connectDictionary[key]).Transaction != null)
                        createTableCmd.Transaction = ((BuildConnectData)connectDictionary[key]).Transaction;

                    createTableCmd.ExecuteNonQuery();
                    log.LogDebug($"EnsureLogTablePresence Table Sql Messages for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}:\r\n{sqlInfoMessage}");
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error ensuring log table presence for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}");
                }
            }
            //SqlCommand createCommitIndex = new SqlCommand(GetFromResources("SqlSync.SqlBuild.SqlLogging.LoggingTableCommitCheckIndex.sql"));
            SqlCommand createCommitIndex = new SqlCommand(Properties.Resources.LoggingTableCommitCheckIndex);
            foreach (string key in keys)
            {
                sqlInfoMessage = string.Empty;
                try
                {
                    createCommitIndex.Connection = ((BuildConnectData)connectDictionary[key]).Connection;

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (logToDatabaseName.Length > 0 && !createCommitIndex.Connection.Database.Equals(logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;


                    if (createCommitIndex.Connection.State == ConnectionState.Closed)
                        createCommitIndex.Connection.Open();

                    if (((BuildConnectData)connectDictionary[key]).Transaction != null)
                        createCommitIndex.Transaction = ((BuildConnectData)connectDictionary[key]).Transaction;

                    createCommitIndex.ExecuteNonQuery();
                    log.LogDebug($"EnsureLogTablePresence Index Sql Messages for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}:\r\n{sqlInfoMessage}");

                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error ensuring log table commit check index for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}");
                }
            }
            log.LogDebug($"sqlInfoMessage value: {sqlInfoMessage}");
            log.LogDebug("Exiting EnsureLogPresence method");
            sqlInfoMessage = string.Empty;
        }
        /// <summary>
        /// Checks to see that the logging table exists or not.
        /// </summary>
        /// <param name="conn">Connection object to the target database</param>
        /// <returns></returns>
        internal bool LogTableExists(SqlConnection conn)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT 1 FROM sys.objects WITH (NOLOCK) WHERE name = 'SqlBuild_Logging' AND type = 'U'", conn);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                object result = cmd.ExecuteScalar();
                if (result == null || result == System.DBNull.Value)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Adds the list of scripts that were committed with the run to the SqlBuild_Logging table
        /// </summary>
        /// <param name="committedScripts">List of CommittedScript objects</param>
        /// <param name="multiDbRunData">The MultiDbRun data for the run</param>
        /// <returns>True if the commit was successful</returns>
        internal bool LogCommittedScriptsToDatabase(List<CommittedScript> committedScripts, MultiDbData multiDbRunData)
        {
            var model = buildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            bool returnValue = true;
            //If using an alternate database to log the commits to, we need to initiate the connection objects 
            //so that the EnsureLogTablePresence method catches them and creates the tables as needed.
            if (logToDatabaseName.Length > 0 && this.committedScripts.Count > 0)
            {
                List<string> servers = new List<string>();
                for (int i = 0; i < this.committedScripts.Count; i++)
                    if (!servers.Contains(this.committedScripts[i].ServerName))
                        servers.Add(this.committedScripts[i].ServerName);

                for (int i = 0; i < servers.Count; i++)
                {
                    BuildConnectData tmp = GetConnectionDataClass(servers[i], logToDatabaseName);
                }
            }
            EnsureLogTablePresence();

            //Get date from the server
            DateTime commitDate;
            SqlCommand cmd = null;
            string oldDb = connData.DatabaseName;
            connData.DatabaseName = "master";
            try
            {
                cmd = new SqlCommand("SELECT getdate()", SqlSync.Connection.ConnectionHelper.GetConnection(connData));
                cmd.Connection.Open();
                commitDate = (DateTime)cmd.ExecuteScalar();
            }
            catch
            {
                commitDate = DateTime.Now;
                log.LogInformation($"Unable to getdate() from server/database {connData.SQLServerName}-{connData.DatabaseName}");
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                    cmd.Connection.Close();

                connData.DatabaseName = oldDb;
            }

            //Add the commited scripts to the log
            SqlCommand logCmd = new SqlCommand(Properties.Resources.LogScript);
            if (!String.IsNullOrEmpty(buildFileName))
                logCmd.Parameters.AddWithValue("@BuildFileName", buildFileName);
            else
                logCmd.Parameters.AddWithValue("@BuildFileName", Path.GetFileName(projectFileName));

            logCmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            logCmd.Parameters.AddWithValue("@CommitDate", commitDate);
            logCmd.Parameters.AddWithValue("@RunWithVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            logCmd.Parameters.AddWithValue("@BuildProjectHash", buildPackageHash);
            if (buildRequestedBy == string.Empty)
                logCmd.Parameters.AddWithValue("@BuildRequestedBy", System.Environment.UserDomainName + "\\" + System.Environment.UserName);
            else
                logCmd.Parameters.AddWithValue("@BuildRequestedBy", buildRequestedBy);

            if (buildDescription == null)
                logCmd.Parameters.AddWithValue("@Description", string.Empty);
            else
                logCmd.Parameters.AddWithValue("@Description", buildDescription);

            logCmd.Parameters.Add("@ScriptFileName", SqlDbType.VarChar);
            logCmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            logCmd.Parameters.Add("@ScriptFileHash", SqlDbType.VarChar);
            logCmd.Parameters.Add("@Sequence", SqlDbType.Int);
            logCmd.Parameters.Add("@ScriptText", SqlDbType.Text);
            logCmd.Parameters.Add("@Tag", SqlDbType.VarChar);
            logCmd.Parameters.Add("@TargetDatabase", SqlDbType.VarChar);
            logCmd.Parameters.Add("@ScriptRunStart", SqlDbType.DateTime);
            logCmd.Parameters.Add("@ScriptRunEnd", SqlDbType.DateTime);


            for (int i = 0; i < committedScripts.Count; i++)
            {
                try
                {

                    LoggingCommittedScript script = committedScripts[i];
                    var row = model.Script.FirstOrDefault(s => string.Equals(s.ScriptId, script.ScriptId.ToString(), StringComparison.OrdinalIgnoreCase));
                    if (row != null)
                    {

                        bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Recording Commited Script: " + row.FileName));

                        BuildConnectData tmpConnDat;
                        if (logToDatabaseName.Length > 0)
                            tmpConnDat = GetConnectionDataClass(script.ServerName, logToDatabaseName);
                        else
                            tmpConnDat = GetConnectionDataClass(script.ServerName, script.DatabaseTarget);

                        logCmd.Connection = tmpConnDat.Connection;
                        logCmd.Parameters["@ScriptFileName"].Value = row.FileName;
                        logCmd.Parameters["@ScriptId"].Value = script.ScriptId;
                        logCmd.Parameters["@ScriptFileHash"].Value = script.FileHash;
                        logCmd.Parameters["@Sequence"].Value = script.Sequence;
                        logCmd.Parameters["@ScriptText"].Value = script.ScriptText;
                        logCmd.Parameters["@Tag"].Value = script.Tag;
                        logCmd.Parameters["@TargetDatabase"].Value = script.DatabaseTarget;
                        logCmd.Parameters["@ScriptRunStart"].Value = script.RunStart;
                        logCmd.Parameters["@ScriptRunEnd"].Value = script.RunEnd;
                        if (logCmd.Connection.State == ConnectionState.Closed)
                            logCmd.Connection.Open();

                        logCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception sqlexe)
                {
                    log.LogError(sqlexe, $"Unable to log full text value for script {logCmd.Parameters["@ScriptFileName"].Value.ToString()}. Inserting \"Error\" instead");
                    try
                    {
                        logCmd.Parameters["@ScriptText"].Value = "Error";
                        if (logCmd.Connection.State == ConnectionState.Closed)
                            logCmd.Connection.Open();

                        logCmd.ExecuteNonQuery();
                    }
                    catch (Exception exe)
                    {
                        log.LogError(exe, $"Unable to log commit for script {logCmd.Parameters["@ScriptFileName"].Value.ToString()}.");
                        returnValue = false;
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Checks to see if the specified script has a block against running more than once. If so, returns some data about it
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="cData">The ConnectionData object for the target database</param>
        /// <param name="databaseName">The name of the database that needs to be checked</param>
        /// <param name="scriptHash">out string for the hash of the script</param>
        /// <param name="scriptTextHash">out string for the hash of the parsed script</param>
        /// <param name="commitDate">out DateTime for the commit date that is blocking the re-run</param>
        /// <returns>True if there is a script block in place</returns>
        public bool HasBlockingSqlLog(System.Guid scriptId, ConnectionData cData, string databaseName, out string scriptHash, out string scriptTextHash, out DateTime commitDate)
        {


            bool hasBlock = false;
            scriptHash = string.Empty;
            scriptTextHash = string.Empty;
            commitDate = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return false;
            }

            SqlCommand cmd = new SqlCommand("SELECT AllowScriptBlock,ScriptFileHash,CommitDate,ScriptText FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC");
            cmd.Parameters.AddWithValue("@ScriptId", scriptId);
            cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(databaseName, cData.SQLServerName, cData.UserId, cData.Password, cData.AuthenticationType, 2, cData.ManagedIdentityClientId);
            try
            {
                cmd.Connection.Open();
                int i = 0;
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        if (i == 0)
                        {
                            scriptHash = (reader[1] == DBNull.Value) ? string.Empty : reader[1].ToString();
                            commitDate = (reader[2] == DBNull.Value) ? DateTime.MinValue : DateTime.Parse(reader[2].ToString());
                            scriptTextHash = (reader[3] == DBNull.Value) ? string.Empty : FileHelper.GetSHA1Hash(reader[3].ToString());
                            i++;
                        }

                        if (reader.GetSqlBoolean(0) == true)
                        {
                            hasBlock = true;
                            break;
                        }
                    }
                    reader.Close();
                }
                return hasBlock;
            }
            catch (SqlException)
            {
                //swallow the exception
                return false;
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, $"Unable to check for blocking SQL for script {scriptId.ToString()} on database {cmd.Connection.DataSource}.{cmd.Connection.Database}");
                return false;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        /// <summary>
        /// Quick check to see if the specicified script has a block against it.
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The BuildConnectData for the target database</param>
        /// <returns>True if there is a block</returns>
        internal bool GetBlockingSqlLog(System.Guid scriptId, ref BuildConnectData connData)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1", connData.Connection, connData.Transaction);
                cmd.Parameters.AddWithValue("@ScriptId", scriptId);
                object has = cmd.ExecuteScalar();
                if (has == null || has == DBNull.Value)
                    return false;
                else
                    return true;
            }
            catch // most likely get here because the table doesn't exist?
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the build run log for the specified script
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The ConnectionData object for the target database</param>
        /// <returns>ScriptRunLog table containing the history</returns>
        public static IReadOnlyList<SqlSync.SqlBuild.Models.ScriptRunLogEntry> GetScriptRunLog(System.Guid scriptId, ConnectionData connData)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC");
                cmd.Parameters.AddWithValue("@ScriptId", scriptId);
                cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                cmd.Connection.Open();
                var list = new List<SqlSync.SqlBuild.Models.ScriptRunLogEntry>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadScriptRunLogEntry(reader));
                }
                return list;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to retrieve script run log for {scriptId.ToString()} on database {connData.SQLServerName}.{connData.DatabaseName}");
                throw new ApplicationException("Error retrieving Script Run Log", e);
            }
        }

        /// <summary>
        /// Returns the build run log for the specified script
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The ConnectionData object for the target database</param>
        /// <returns>ScriptRunLog table containing the history</returns>
        public static IReadOnlyList<SqlSync.SqlBuild.Models.ScriptRunLogEntry> GetObjectRunHistoryLog(string objectFileName, ConnectionData connData)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE [ScriptFileName] = @ScriptFileName ORDER BY CommitDate DESC");
                cmd.Parameters.AddWithValue("@ScriptFileName", objectFileName);
                cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                cmd.Connection.Open();
                var list = new List<SqlSync.SqlBuild.Models.ScriptRunLogEntry>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadScriptRunLogEntry(reader));
                }
                return list;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to retrieve object history for {objectFileName} on database {connData.SQLServerName}.{connData.DatabaseName}");
                throw new ApplicationException("Error retrieving Script Run Log", e);
            }
        }

        private static SqlSync.SqlBuild.Models.ScriptRunLogEntry ReadScriptRunLogEntry(IDataRecord reader)
        {
            Guid? TryGuid(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Guid.Parse(val.ToString() ?? string.Empty); } catch { return null; }
            }

            bool? TryBool(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Convert.ToBoolean(val, CultureInfo.InvariantCulture); } catch { return null; }
            }

            int? TryInt(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Convert.ToInt32(val, CultureInfo.InvariantCulture); } catch { return null; }
            }

            DateTime? TryDate(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Convert.ToDateTime(val, CultureInfo.InvariantCulture); } catch { return null; }
            }

            string? TryString(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return val.ToString(); } catch { return null; }
            }

            return new SqlSync.SqlBuild.Models.ScriptRunLogEntry(
                BuildFileName: TryString("BuildFileName"),
                ScriptFileName: TryString("ScriptFileName"),
                ScriptId: TryGuid("ScriptId"),
                ScriptFileHash: TryString("ScriptFileHash"),
                CommitDate: TryDate("CommitDate"),
                Sequence: TryInt("Sequence"),
                UserId: TryString("UserId"),
                AllowScriptBlock: TryBool("AllowScriptBlock"),
                AllowBlockUpdateId: TryString("AllowBlockUpdateId"),
                ScriptText: TryString("ScriptText"),
                Tag: TryString("Tag"));
        }


        #endregion


        
        public static string RemoveUseStatement(string script)
        {
            // Keep this public static helper for backward compatibility - used by external callers
            Regex regUse = new Regex(Properties.Resources.RegexUseStatement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            int startAt = 0;
            while (regUse.Match(script, startAt).Success)
            {
                Match m = regUse.Match(script, startAt);
                if (!IsInComment(script, m.Index))
                {
                    script = regUse.Replace(script, "", 1, m.Index);
                }
                else
                {
                    startAt = m.Index + m.Length;
                }
            }
            return script;
        }
        
        public static string RemoveTransactionReferences(string script)
        {
            // Keep this public static helper for backward compatibility
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTransaction, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTran, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexCommit, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTransactionLevel, script, options);
            return script;
        }
        
        private static string RegexRemoveIfNotInComments(string regexExpression, string script, RegexOptions options)
        {
            Regex regRemoveTag = new Regex(regexExpression, options);
            int startAt = 0;
            while (regRemoveTag.Match(script, startAt).Success)
            {
                Match m = regRemoveTag.Match(script, startAt);
                if (!IsInComment(script, m.Index))
                {
                    script = regRemoveTag.Replace(script, "", 1, m.Index);
                }
                else
                {
                    startAt = m.Index + m.Length;
                }
            }
            return script;
        }

        #region ## SQL Connection Helper Methods ##
        internal BuildConnectData GetConnectionDataClass(string serverName, string databaseName)
        {
            try
            {
                //always Upper case the database name
                string databaseKey = serverName.ToUpper() + ":" + databaseName.ToUpper();
                if (connectDictionary.ContainsKey(databaseKey) == false)
                {
                    BuildConnectData cData = new BuildConnectData();
                    cData.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(databaseName, serverName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);

                    //Add a robust retry policy on database connection opening
                    var pollyConnection = Policy.Handle<SqlException>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                    pollyConnection.Execute(() => cData.Connection.Open());

                    cData.HasLoggingTable = LogTableExists(cData.Connection);
                    cData.Connection.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);
                    if (!databaseName.Equals(logToDatabaseName) && isTransactional) //we don't want a transaction for the logging database
                        cData.Transaction = cData.Connection.BeginTransaction(BuildTransaction.TransactionName);
                    cData.DatabaseName = databaseName;
                    cData.ServerName = serverName;

                    connectDictionary.Add(databaseKey, cData);
                    return cData;
                }
                else
                {
                    return (BuildConnectData)connectDictionary[databaseKey];
                }
            }
            catch (Exception exe)
            {
                log.LogError("Error getting connection data for " + serverName + "." + databaseName + " : " + exe.Message);
                throw;
            }
        }
        internal bool CommitBuild()
        {
            //If we're not in a transaction, they've already committed.
            if (!isTransactional)
                return true;

            Dictionary<string, BuildConnectData>.KeyCollection keys = connectDictionary.Keys;
            bool success = true;
            foreach (string key in keys)
            {
                try
                {
                    log.LogInformation($"Committing transaction for {key}");
                    ((BuildConnectData)connectDictionary[key]).Transaction.Commit();
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error in CommitBuild Transaction.Commit() for database '{key}'");
                    bgWorker.ReportProgress(100, new CommitFailureEventArgs(e.Message));
                    success = false;
                }
                try
                {
                    log.LogDebug($"Closing connection for {key}");
                    ((BuildConnectData)connectDictionary[key]).Connection.Close();
                }
                catch (Exception e)
                {
                    log.LogWarning(e, $"Error in CommitBuild Connection.Close() for database '{e}'");
                    bgWorker.ReportProgress(100, new CommitFailureEventArgs(e.Message));
                    success = false;
                }
            }

            return success;
        }
        internal bool RollbackBuild()
        {
            //If we're not in a transaction, we can't roll back...
            if (!isTransactional)
            {
                log.LogWarning("Build is non-transactional. Unable to rollback");
                return false;
            }

            Dictionary<string, BuildConnectData>.KeyCollection keys = connectDictionary.Keys;
            foreach (string key in keys)
            {
                try
                {
                    log.LogInformation($"Rolling back transaction for {key}");
                    ((BuildConnectData)connectDictionary[key]).Transaction.Rollback();
                }
                catch (Exception e)
                {
                    log.LogError($"Error in RollbackBuild Transaction.Rollback() for database '{key}'. {e.Message}");
                }
                try
                {
                    log.LogDebug($"Closing connection for {key}");
                    ((BuildConnectData)connectDictionary[key]).Connection.Close();
                }
                catch (Exception e)
                {
                    log.LogError($"Error in RollbackBuild Connection.Close() for database '{key}'. {e.Message}");
                }
            }

            return true;
        }
        private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            StringBuilder messages = new StringBuilder();
            foreach (SqlError err in e.Errors)
            {
                messages.Append(err.Message + "\r\n");
            }
            if (currentRun != null)
                currentRun = currentRun with { Results = (currentRun.Results ?? string.Empty) + messages.ToString() };

            sqlInfoMessage += messages.ToString();
        }
        #endregion

        

        #region ## Events ##
        public event ScriptLogWriteEventHandler ScriptLogWriteEvent;
        public event BuildCommittedEventHandler BuildCommittedEvent;
        public event EventHandler BuildSuccessTrialRolledBackEvent;
        public event EventHandler BuildErrorRollBackEvent;
        #endregion

        public static bool IsInComment(string rawScript, int index)
        {
            Regex regDoubleDash = new Regex(@"(--.*\n)", RegexOptions.IgnoreCase);
            Regex regMultiLineComment = new Regex(@"(/\*.+?\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection comment = regDoubleDash.Matches(rawScript);
            for (int i = 0; i < comment.Count; i++)
            {
                if (index > comment[i].Index && index < comment[i].Index + comment[i].Length)
                    return true;
            }

            comment = regMultiLineComment.Matches(rawScript);
            for (int i = 0; i < comment.Count; i++)
            {
                if (index > comment[i].Index && index <= comment[i].Index + comment[i].Length)
                    return true;
            }
            return false;
        }


        internal void SqlBuildHelper_ScriptLogWriteEvent(object sender, bool isError, ScriptLogEventArgs e)
        {
            if (scriptLogFileName == null)
                throw new NullReferenceException("Attempting to write to Script Log File, but \"scriptLogFileName\" field value is null");

            if (File.Exists(scriptLogFileName) == false || e.InsertStartTransaction)
            {
                using (StreamWriter sw = File.AppendText(scriptLogFileName))
                {
                    sw.WriteLine("-- Start Time: " + DateTime.Now.ToString() + " --");
                    if (isTransactional)
                    {
                        sw.WriteLine("-- Start Transaction --");
                        sw.WriteLine("BEGIN TRANSACTION");
                    }
                    else
                    {
                        sw.WriteLine("-- Executed without a transaction --");
                    }
                }
            }


            using (StreamWriter sw = File.AppendText(scriptLogFileName))
            {
                sw.WriteLine("/************************************");
                sw.WriteLine("Script #" + e.ScriptIndex.ToString() + "; Source File: " + e.SourceFile);
                sw.WriteLine("Server: " + connData.SQLServerName + "; Run On Database:" + e.Database + "  */");
                if (e.Database.Length > 0)
                    sw.WriteLine("use " + e.Database + "\r\nGO\r\n");
                sw.WriteLine(e.SqlScript + "\r\nGO\r\n");
                sw.WriteLine("/*Script #" + e.ScriptIndex.ToString() + " Result: " + e.Results.Trim() + "  */");
                sw.Flush();
                sw.Close();
            }

            if (e.ScriptIndex == -10000 && externalScriptLogFileName.Length > 0)
            {
                using (StreamWriter sw = File.AppendText(scriptLogFileName))
                {
                    sw.WriteLine("-- END Time: " + DateTime.Now.ToString() + " --");
                    sw.Flush();
                    sw.Close();
                }

                try
                {
                    string tmpPath = Path.GetDirectoryName(externalScriptLogFileName);
                    if (!Directory.Exists(tmpPath))
                    {
                        bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Creating External Log file directory \"" + tmpPath + "\""));
                        Directory.CreateDirectory(tmpPath);
                    }

                    File.Copy(scriptLogFileName, externalScriptLogFileName, true);
                    bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Copied log file to \"" + externalScriptLogFileName + "\""));
                }
                catch (Exception exe)
                {
                    bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Error copying results file to " + externalScriptLogFileName + "\r\n" + exe.ToString()));
                }
            }

        }

        public string GetFromResources(string resourceName)
        {
            System.Reflection.Assembly assem = GetType().Assembly;
            using (System.IO.Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                try
                {
                    using (System.IO.StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Error retrieving from Resources. Tried '"
                        + resourceName + "'\r\n" + e.ToString());
                }
            }
        }

        internal string PerformScriptTokenReplacement(string script)
        {
            return TokenReplacementService.ReplaceTokens(script, this);
        }

        internal void SaveBuildDataSet(bool fireSavedEvent)
        {
            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Saving Build File Updates"));

            if (projectFileName == null || projectFileName.Length == 0)
            {
                string message = "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.";
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs(message));
                throw new ArgumentException(message);
            }

            SqlBuildFileHelper.SaveSqlBuildProjectFile(buildDataModel, projectFileName, buildFileName, includeHistoryAndLogs: true);


            if (buildHistoryXmlFile == null || buildHistoryXmlFile.Length == 0)
            {
                string message = "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.";
                bgWorker.ReportProgress(0, new GeneralStatusEventArgs(message));
                throw new ArgumentException(message);
            }

            if (buildHistoryData != null)
                buildHistoryData.WriteXml(buildHistoryXmlFile);

            if (fireSavedEvent)
                bgWorker.ReportProgress(0, new ScriptRunProjectFileSavedEventArgs(true));
        }

        private void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild)
        {
            if (run == null) return;
            buildHistoryModel = buildHistoryModel with { ScriptRun = buildHistoryModel.ScriptRun.Concat(new[] { run }).ToList() };
            // optionally keep dataset for compatibility
            if (buildHistoryData != null)
            {
                var row = buildHistoryData.ScriptRun.NewScriptRunRow();
                row.Database = run.Database;
                row.RunOrder = run.RunOrder ?? 0;
                row.RunStart = run.RunStart ?? DateTime.MinValue;
                row.FileName = run.FileName;
                row.ScriptRunId = run.ScriptRunId;
                if (run.FileHash != null) row.FileHash = run.FileHash;
                if (run.RunEnd.HasValue) row.RunEnd = run.RunEnd.Value;
                if (run.Success.HasValue) row.Success = run.Success.Value;
                if (run.Results != null) row.Results = run.Results;
                buildHistoryData.ScriptRun.AddScriptRunRow(row);
                buildHistoryData.AcceptChanges();
            }
        }

        public static SqlSyncBuildData.ScriptDataTable GetScriptSourceTable(BuildModels.SqlSyncBuildDataModel buildDataModel)
        {
            if (buildDataModel == null)
            {
                log.LogWarning("The SqlSyncBuildDataModel object passed into \"GetScriptSourceTable\" was null. Unable to process build");
                return null;
            }

            var ds = buildDataModel.ToDataSet();
            return GetScriptSourceTable(ds.ToModel());
        }

        #region ISqlBuildRunnerContext
        ILogger ISqlBuildRunnerContext.Log => log;
        BackgroundWorker ISqlBuildRunnerContext.BgWorker => bgWorker;
        IProgressReporter ISqlBuildRunnerContext.ProgressReporter => ProgressReporter ?? new BackgroundWorkerProgressReporter(bgWorker);
        bool ISqlBuildRunnerContext.IsTransactional => isTransactional;
        bool ISqlBuildRunnerContext.IsTrialBuild => isTrialBuild;
        bool ISqlBuildRunnerContext.RunScriptOnly => runScriptOnly;
        string ISqlBuildRunnerContext.BuildPackageHash => buildPackageHash;
        string ISqlBuildRunnerContext.ProjectFilePath => projectFilePath;
        List<LoggingCommittedScript> ISqlBuildRunnerContext.CommittedScripts => committedScripts;
        bool ISqlBuildRunnerContext.ErrorOccured { get => ErrorOccured; set => ErrorOccured = value; }
        string ISqlBuildRunnerContext.SqlInfoMessage { get => sqlInfoMessage; set => sqlInfoMessage = value; }
        int ISqlBuildRunnerContext.DefaultScriptTimeout => connData?.ScriptTimeout ?? 30;

        BuildConnectData ISqlBuildRunnerContext.GetConnectionDataClass(string serverName, string databaseName) => GetConnectionDataClass(serverName, databaseName);
        string ISqlBuildRunnerContext.GetTargetDatabase(string defaultDatabase) => GetTargetDatabase(defaultDatabase);
        Task<string[]> ISqlBuildRunnerContext.ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken) => ScriptBatcher.ReadBatchFromScriptFileAsync(path, stripTransaction, useRegex, cancellationToken);
        string ISqlBuildRunnerContext.PerformScriptTokenReplacement(string script) => PerformScriptTokenReplacement(script);
        Task<string> ISqlBuildRunnerContext.PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken) => TokenReplacementService.ReplaceTokensAsync(script, this, cancellationToken);
        void ISqlBuildRunnerContext.AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) => AddScriptRunToHistory(run, myBuild);
        void ISqlBuildRunnerContext.RollbackBuild() => RollbackBuild();
        void ISqlBuildRunnerContext.SaveBuildDataSet(bool fireSavedEvent) => SaveBuildDataSet(fireSavedEvent);
        BuildModels.Build ISqlBuildRunnerContext.PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs) => PerformRunScriptFinalization(buildFailure, myBuild, buildDataModel, ref workEventArgs);
        void ISqlBuildRunnerContext.PublishScriptLog(bool isError, ScriptLogEventArgs args) => ScriptLogWriteEvent?.Invoke(null, isError, args);
        #endregion

        #region IBuildFinalizerContext implementation
        bool IBuildFinalizerContext.IsTransactional => isTransactional;
        bool IBuildFinalizerContext.IsTrialBuild => isTrialBuild;
        bool IBuildFinalizerContext.RunScriptOnly => runScriptOnly;
        BackgroundWorker IBuildFinalizerContext.BgWorker => bgWorker;
        List<LoggingCommittedScript> IBuildFinalizerContext.CommittedScripts => committedScripts;

        bool IBuildFinalizerContext.CommitBuild() => CommitBuild();
        void IBuildFinalizerContext.RollbackBuild() => RollbackBuild();
        void IBuildFinalizerContext.SaveBuildDataSet(bool finalSave) => SaveBuildDataSet(finalSave);
        bool IBuildFinalizerContext.RecordCommittedScripts(List<LoggingCommittedScript> committedScripts, BuildModels.SqlSyncBuildDataModel buildDataModel, out BuildModels.SqlSyncBuildDataModel updatedModel) => RecordCommittedScripts(committedScripts, buildDataModel, out updatedModel);
        bool IBuildFinalizerContext.LogCommittedScriptsToDatabase(List<LoggingCommittedScript> committedScripts, MultiDbData multiDbRunData) => LogCommittedScriptsToDatabase(committedScripts, multiDbRunData);
        void IBuildFinalizerContext.SetErrorOccurred(bool value) => ErrorOccured = value;
        void IBuildFinalizerContext.RaiseBuildCommittedEvent(object sender, RunnerReturn rr) => BuildCommittedEvent?.Invoke(sender, rr);
        void IBuildFinalizerContext.RaiseBuildSuccessTrialRolledBackEvent(object sender) => BuildSuccessTrialRolledBackEvent?.Invoke(sender, EventArgs.Empty);
        void IBuildFinalizerContext.RaiseBuildErrorRollBackEvent(object sender) => BuildErrorRollBackEvent?.Invoke(sender, EventArgs.Empty);
        #endregion

 
    }
}
