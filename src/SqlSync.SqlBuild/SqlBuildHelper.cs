using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.SqlLogging;
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
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

#nullable enable

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for SqlBuildHelper.
    /// </summary>
    public class SqlBuildHelper : ISqlBuildRunnerContext, ISqlBuildRunnerProperties, IBuildFinalizerContext
    {
        internal static Func<IConnectionsService, ISqlBuildRunnerContext, ISqlCommandExecutor, SqlBuildRunner> SqlBuildRunnerFactory = (connService, ctx, exec) => new SqlBuildRunner(connService,  ctx, exec);
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
        //private Dictionary<string, BuildConnectData> connectDictionary = new Dictionary<string, BuildConnectData>();
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
        private MultiDbData multiDbRunData;
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
        public List<LoggingCommittedScript> CommittedScripts => committedScripts;
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
        internal ILegacyBuildDataAdapter LegacyAdapter { get; }
        internal Services.IBuildPreparationService BuildPreparationService { get; }
        internal Services.IScriptBatcher ScriptBatcher { get; }
        internal Services.ITokenReplacementService TokenReplacementService { get; }
        internal Services.ISqlLoggingService SqlLoggingService { get; }
        internal Services.IDatabaseUtility DatabaseUtility { get; }
        internal Services.IConnectionsService ConnectionsService { get; }
        internal Services.IBuildFinalizer BuildFinalizer { get; }

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
            ILegacyBuildDataAdapter legacyAdapter = null,
            IDatabaseUtility databaseUtility = null,
            IConnectionsService connectionsService = null,
            IBuildFinalizer buildFinalizer = null)
        {
            connData = data;
            this.isTransactional = isTransactional;
            Clock = clock ?? new SystemClock();
            GuidProvider = guidProvider ?? new GuidProvider();
            FileSystem = fileSystem ?? new DotNetFileSystem();
            ProgressReporter = progressReporter; // fallback to BackgroundWorker when available
            FileHelper = fileHelper ?? new DefaultSqlBuildFileHelper();
            RetryPolicy = retryPolicy ?? new DefaultBuildRetryPolicy();
            LegacyAdapter = legacyAdapter ?? new DefaultLegacyBuildDataAdapter();
            BuildPreparationService = new Services.DefaultBuildPreparationService(this);
            ScriptBatcher = new Services.DefaultScriptBatcher();
            TokenReplacementService = new Services.DefaultTokenReplacementService();
            ConnectionsService = connectionsService ?? new Services.DefaultConnectionsService();
            SqlLoggingService = new Services.DefaultSqlLoggingService(ConnectionsService, ProgressReporter);
            DatabaseUtility = databaseUtility ?? new Services.DefaultDatabaseUtility(ConnectionsService, SqlLoggingService, ProgressReporter, FileHelper);
            BuildFinalizer = buildFinalizer ?? new Services.DefaultBuildFinalizer(SqlLoggingService, ProgressReporter);


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
            this.multiDbRunData = multiDbRunData;
            this.bgWorker = bgWorker;
            committedScripts.Clear();
            ConnectionsService.Connections.Clear();
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
                    (List<Build> buildRecords, SqlSyncBuildDataModel updatedModel, bool errorOccurred) = BuildFinalizer.PerformRunScriptFinalization(this, ConnectionsService,true, buildResults, (IProgressReporter)bgWorker, ref e);
                    buildResults = buildRecords;
                    ErrorOccured = errorOccurred;
                    return;
                }
            }

            (List<Build> goodBuildRecords, SqlSyncBuildDataModel goodUpdatedModel, bool finalizeErrorOccurred) = BuildFinalizer.PerformRunScriptFinalization(this, ConnectionsService, false, buildResults, (IProgressReporter)bgWorker, ref e);
            buildResults = goodBuildRecords;
            ErrorOccured = finalizeErrorOccurred;
        }


        //public BuildModels.Build ProcessBuild(BuildModels.SqlBuildRunDataModel runData, int allowableTimeoutRetries, BackgroundWorker bgWorker, DoWorkEventArgs e)
        //{
        //    connectDictionary.Clear();
        //    committedScripts.Clear();

        //    return ProcessBuild(runData, bgWorker, e, connData.SQLServerName, false, null, allowableTimeoutRetries);
        //}

        public BuildModels.Build ProcessBuild(BuildModels.SqlBuildRunDataModel runData, BackgroundWorker bgWorker, DoWorkEventArgs e, int allowableTimeoutRetries = 3,  string buildRequestedBy = "", ScriptBatchCollection scriptBatchColl = null)
        {
            ConnectionsService.Connections.Clear();
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

            var orchestrator = new Services.SqlBuildOrchestrator(this, ConnectionsService, SqlLoggingService);
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
            var runner = SqlBuildRunnerFactory(ConnectionsService, this, null);
            return runner.Run(scripts, myBuild, serverName, isMultiDbRun, scriptBatchColl, buildDataModel, ref workEventArgs);
        }

    

     

        //internal BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs)
        //{
        //    return BuildFinalizer.PerformRunScriptFinalization(this, SqlLoggingService, buildFailure, myBuild, buildDataModel, ref workEventArgs);
        //}

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
                    buildDataModel = BuildFinalizer.RecordCommittedScripts(committedScripts, buildDataModel);
                    SqlLoggingService.LogCommittedScriptsToDatabase(committedScripts, this, multiDbRunData);
                }
            }
            else
            {
                if (!isTrialBuild)
                {
                    if (isTransactional)
                    {
                        bgWorker?.ReportProgress(0, new GeneralStatusEventArgs("Attempting to Commit Build"));
                        bool commitSuccess = BuildFinalizer.CommitBuild(ConnectionsService, this.isTransactional);
                        if (commitSuccess)
                            bgWorker?.ReportProgress(0, new GeneralStatusEventArgs("Commit Successful"));
                    }

                    for (int i = 0; i < myBuilds.Count; i++)
                        myBuilds[i] = myBuilds[i] with { FinalStatus = BuildItemStatus.Committed.ToString() };

                    buildDataModel = BuildFinalizer.RecordCommittedScripts(committedScripts, buildDataModel);
                    SqlLoggingService.LogCommittedScriptsToDatabase(committedScripts, this, multiDbRunData);

                    BuildCommittedEvent?.Invoke(this, RunnerReturn.BuildCommitted);
                }
                else
                {
                    if (isTransactional)
                    {
                        BuildFinalizer.CommitBuild(ConnectionsService, this.isTransactional);
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

            SaveBuildDataModel(true);

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

            ConnectionsService.Connections.Clear();
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
     
        internal bool CommitBuild()
        {
            //If we're not in a transaction, they've already committed.
            if (!isTransactional)
                return true;

            Dictionary<string, BuildConnectData>.KeyCollection keys = ConnectionsService.Connections.Keys;
            bool success = true;
            foreach (string key in keys)
            {
                try
                {
                    log.LogInformation($"Committing transaction for {key}");
                    ((BuildConnectData)ConnectionsService.Connections[key]).Transaction.Commit();
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
                    ((BuildConnectData)ConnectionsService.Connections[key]).Connection.Close();
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

            Dictionary<string, BuildConnectData>.KeyCollection keys = ConnectionsService.Connections.Keys;
            foreach (string key in keys)
            {
                try
                {
                    log.LogInformation($"Rolling back transaction for {key}");
                    ((BuildConnectData)ConnectionsService.Connections[key]).Transaction.Rollback();
                }
                catch (Exception e)
                {
                    log.LogError($"Error in RollbackBuild Transaction.Rollback() for database '{key}'. {e.Message}");
                }
                try
                {
                    log.LogDebug($"Closing connection for {key}");
                    ((BuildConnectData)ConnectionsService.Connections[key]).Connection.Close();
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

        internal void SaveBuildDataModel(bool fireSavedEvent)
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
        bool ISqlBuildRunnerProperties.IsTransactional => isTransactional;
        bool ISqlBuildRunnerProperties.IsTrialBuild => isTrialBuild;
        bool ISqlBuildRunnerProperties.RunScriptOnly => runScriptOnly;
        string ISqlBuildRunnerProperties.BuildPackageHash => buildPackageHash;
        string ISqlBuildRunnerProperties.ProjectFilePath => projectFilePath;
        string ISqlBuildRunnerProperties.ProjectFileName => projectFileName;
        string ISqlBuildRunnerProperties.BuildFileName => buildFileName;
        string ISqlBuildRunnerProperties.BuildHistoryXmlFile => buildHistoryXmlFile;
        List<LoggingCommittedScript> ISqlBuildRunnerProperties.CommittedScripts => committedScripts;
        bool ISqlBuildRunnerProperties.ErrorOccured { get => ErrorOccured; set => ErrorOccured = value; }
        string ISqlBuildRunnerProperties.SqlInfoMessage { get => sqlInfoMessage; set => sqlInfoMessage = value; }
        int ISqlBuildRunnerProperties.DefaultScriptTimeout => connData?.ScriptTimeout ?? 30;
        BuildModels.SqlSyncBuildDataModel ISqlBuildRunnerProperties.BuildDataModel => buildDataModel;
        MultiDbData ISqlBuildRunnerProperties.MultiDbRunData => this.multiDbRunData;
        string ISqlBuildRunnerProperties.BuildRequestedBy => buildRequestedBy;
        string ISqlBuildRunnerProperties.BuildDescription => buildDescription;
        string ISqlBuildRunnerProperties.LogToDataBaseName => LogToDatabaseName;
        ConnectionData ISqlBuildRunnerProperties.ConnectionData => connData;

        string ISqlBuildRunnerContext.GetTargetDatabase(string defaultDatabase) => GetTargetDatabase(defaultDatabase);
        Task<string[]> ISqlBuildRunnerContext.ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken) => ScriptBatcher.ReadBatchFromScriptFileAsync(path, stripTransaction, useRegex, cancellationToken);
        string ISqlBuildRunnerContext.PerformScriptTokenReplacement(string script) => PerformScriptTokenReplacement(script);
        Task<string> ISqlBuildRunnerContext.PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken) => TokenReplacementService.ReplaceTokensAsync(script, this, cancellationToken);
        void ISqlBuildRunnerContext.AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) => AddScriptRunToHistory(run, myBuild);
        void ISqlBuildRunnerContext.RollbackBuild() => RollbackBuild();
        void ISqlBuildRunnerContext.SaveBuildDataSet(bool fireSavedEvent) => SaveBuildDataModel(fireSavedEvent);
        BuildModels.Build ISqlBuildRunnerContext.PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs) => PerformRunScriptFinalization(buildFailure, myBuild, buildDataModel, ref workEventArgs);
        void ISqlBuildRunnerContext.PublishScriptLog(bool isError, ScriptLogEventArgs args) => ScriptLogWriteEvent?.Invoke(null, isError, args);
        #endregion

        #region IBuildFinalizerContext implementation
        bool IBuildFinalizerContext.IsTransactional => isTransactional;
        bool IBuildFinalizerContext.IsTrialBuild => isTrialBuild;
        bool IBuildFinalizerContext.RunScriptOnly => runScriptOnly;
        BackgroundWorker IBuildFinalizerContext.BgWorker => bgWorker;
        List<LoggingCommittedScript> IBuildFinalizerContext.CommittedScripts => committedScripts;

        void IBuildFinalizerContext.SaveBuildDataSet(bool finalSave) => SaveBuildDataModel(finalSave);
        void IBuildFinalizerContext.RaiseBuildCommittedEvent(object sender, RunnerReturn rr) => BuildCommittedEvent?.Invoke(sender, rr);
        void IBuildFinalizerContext.RaiseBuildSuccessTrialRolledBackEvent(object sender) => BuildSuccessTrialRolledBackEvent?.Invoke(sender, EventArgs.Empty);
        void IBuildFinalizerContext.RaiseBuildErrorRollBackEvent(object sender) => BuildErrorRollBackEvent?.Invoke(sender, EventArgs.Empty);
        #endregion

 
    }
}
