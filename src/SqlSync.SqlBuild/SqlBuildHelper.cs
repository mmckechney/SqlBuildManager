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
        internal static Func<IConnectionsService, ISqlBuildRunnerContext, IBuildFinalizerContext , ISqlCommandExecutor, SqlBuildRunner> SqlBuildRunnerFactory = (connService, ctx, finalizerContext, exec) => new SqlBuildRunner(connService,  ctx, finalizerContext, exec);
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal bool isTransactional = true;
        internal List<DatabaseOverride> targetDatabaseOverrides = null;
        public List<DatabaseOverride> TargetDatabaseOverrides
        {
            get { return targetDatabaseOverrides; }
            set { targetDatabaseOverrides = value; }
        }
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
            IList<BuildModels.Script> FilteredScripts,
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
        public System.Guid CurrentBuildId => currentBuildId;

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
        private BuildModels.SqlSyncBuildDataModel buildHistoryModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

        internal BuildModels.SqlSyncBuildDataModel BuildHistoryModel
        {
            get => buildHistoryModel;
            set => buildHistoryModel = value;
        }
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
                buildDataModel: runData.BuildDataModel,
                buildType: runData.BuildType,
                server: runData.Server,
                buildDescription: runData.BuildDescription,
                startIndex: runData.StartIndex,
                projectFileName: runData.ProjectFileName,
                isTrial: runData.IsTrial,
                runItemIndexes: runData.RunItemIndexes,
                runScriptOnly: runData.RunScriptOnly,
                buildFileName: runData.BuildFileName,
                logToDatabaseName: runData.LogToDatabaseName,
                isTransactional: runData.IsTransactional,
                platinumDacPacFileName: runData.PlatinumDacPacFileName,
                targetDatabaseOverrides: runData.TargetDatabaseOverrides,
                forceCustomDacpac: runData.ForceCustomDacpac,
                buildRevision: runData.BuildRevision,
                defaultScriptTimeout: runData.DefaultScriptTimeout,
                allowObjectDelete: runData.AllowObjectDelete);
        }


        private static BuildModels.Script MapLegacyScriptRowToModel(SqlSyncBuildData.ScriptRow row) => row.ToModel();

        private static BuildModels.Build MapLegacyBuildRowToModel(SqlSyncBuildData.BuildRow row) => row.ToModel();
        private void SyncBuildDataModel(SqlSyncBuildData ds) => buildDataModel = ds.ToModel();

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
            ProgressReporter = progressReporter ?? new DefaultProgressReporter();
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

        public BuildResultStatus ProcessMultiDbBuild(MultiDbData multiDbRunData, string projectFileName)
        {
            this.projectFileName = projectFileName;
            return ProcessMultiDbBuild(multiDbRunData); // Call to process the multi-db build
        }
        /// <summary>
        /// Used when the user wants to run the build across multiple databases and/or servers
        /// </summary>
        /// <param name="multiDbRunData"></param>
        /// <param name="bgWorker"></param>
        /// <param name="e"></param>
        public BuildResultStatus ProcessMultiDbBuild(MultiDbData multiDbRunData)
        {
            this.multiDbRunData = multiDbRunData;
            committedScripts.Clear();
            ConnectionsService.Connections.Clear();
            var buildResults = new List<BuildModels.Build>();
            List<BuildResultStatus> finalizedBuildStatus = new List<BuildResultStatus>();
            BuildResultStatus tmpStatus;

            foreach (var srvData in multiDbRunData)
            {
                var runData = new BuildModels.SqlBuildRunDataModel(
                    buildDataModel: multiDbRunData.BuildData,
                    buildType: "Multi Db Build",
                    server: srvData.ServerName,
                    buildDescription: string.IsNullOrEmpty(multiDbRunData.BuildDescription)
                        ? $"Multi-Database Run ID: {multiDbRunData.MultiRunId}.  Server:{srvData.ServerName}"
                        : multiDbRunData.BuildDescription,
                    startIndex: 0,
                    projectFileName: multiDbRunData.ProjectFileName,
                    isTrial: multiDbRunData.RunAsTrial,
                    runItemIndexes: Array.Empty<double>(),
                    runScriptOnly: false,
                    buildFileName: multiDbRunData.BuildFileName,
                    logToDatabaseName: string.Empty,
                    isTransactional: true,
                    platinumDacPacFileName: string.Empty,
                    targetDatabaseOverrides: srvData.Overrides,
                    forceCustomDacpac: false,
                    buildRevision: string.Empty,
                    defaultScriptTimeout: 500,
                    allowObjectDelete: false);

                targetDatabaseOverrides = srvData.Overrides;
                var buildResult = ProcessBuild(runData,srvData.ServerName, true, null, multiDbRunData.AllowableTimeoutRetries);
                

                if (buildResult.FinalStatus == BuildItemStatus.PendingRollBack || buildResult.FinalStatus == BuildItemStatus.FailedNoTransaction)
                {
                    (buildResult, buildDataModel, tmpStatus) = BuildFinalizer.PerformRunScriptFinalization(this, ConnectionsService,this, true, buildResult);
                    finalizedBuildStatus.Add(tmpStatus);
                    buildResults.Add(buildResult);
                }
                else
                {
                    (buildResult, buildDataModel, tmpStatus) = BuildFinalizer.PerformRunScriptFinalization(this, ConnectionsService, this, false, buildResult);
                    finalizedBuildStatus.Add(tmpStatus);
                    buildResults.Add(buildResult);
                }
                this.committedScripts.Clear();
            }
            BuildResultStatus calculatedStatus =  BuildFinalizer.CalculateFinalStatus(finalizedBuildStatus);

            return calculatedStatus;
        }

        public async Task<BuildModels.Build> ProcessBuild(BuildModels.SqlBuildRunDataModel runData, int allowableTimeoutRetries = 3,  string buildRequestedBy = "", ScriptBatchCollection scriptBatchColl = null)
        {
            ConnectionsService.Connections.Clear();
            committedScripts.Clear();
            this.buildRequestedBy = buildRequestedBy;
            BuildModels.Build returnval = null;
            await Task.Run(() =>
            {
                returnval =  ProcessBuild(runData: runData, serverName: connData.SQLServerName, isMultiDbRun: false, scriptBatchColl: scriptBatchColl, allowableTimeoutRetries: allowableTimeoutRetries);
            });

            return returnval;
        }

        internal BuildModels.Build ProcessBuild(BuildModels.SqlBuildRunDataModel runData, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, int allowableTimeoutRetries)
        {
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

            log.LogInformation($"Starting Build Process targeting: {serverName} ");

            var prep = PrepareBuildForRun(buildDataModel, serverName, isMultiDbRun, scriptBatchColl);
            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return prep.Build;
            }

            var orchestrator = new Services.SqlBuildOrchestrator(this, ConnectionsService, SqlLoggingService);
            var buildResultsModel = orchestrator.Execute(runData, prep, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);

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
                    var tempRunData = MapToRunDataModel(legacyRunData);
                    var updatedRunData = new BuildModels.SqlBuildRunDataModel(
                        buildDataModel: tempRunData.BuildDataModel,
                        buildType: tempRunData.BuildType,
                        server: tempRunData.Server,
                        buildDescription: tempRunData.BuildDescription,
                        startIndex: tempRunData.StartIndex,
                        projectFileName: tempRunData.ProjectFileName,
                        isTrial: tempRunData.IsTrial,
                        runItemIndexes: tempRunData.RunItemIndexes,
                        runScriptOnly: tempRunData.RunScriptOnly,
                        buildFileName: tempRunData.BuildFileName,
                        logToDatabaseName: tempRunData.LogToDatabaseName,
                        isTransactional: tempRunData.IsTransactional,
                        platinumDacPacFileName: string.Empty,
                        targetDatabaseOverrides: tempRunData.TargetDatabaseOverrides,
                        forceCustomDacpac: tempRunData.ForceCustomDacpac,
                        buildRevision: tempRunData.BuildRevision,
                        defaultScriptTimeout: tempRunData.DefaultScriptTimeout,
                        allowObjectDelete: tempRunData.AllowObjectDelete);
                    log.LogInformation($"Executing custom dacpac on {targetDatabase}");
                    var dacBuild = ProcessBuild(updatedRunData,serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
                    var dacFinalStatus = dacBuild.FinalStatus;
                    if (dacFinalStatus == BuildItemStatus.Committed || dacFinalStatus == BuildItemStatus.CommittedWithTimeoutRetries)
                    {
                        buildResultsModel.FinalStatus = BuildItemStatus.CommittedWithCustomDacpac;
                        if (BuildCommittedEvent != null)
                            BuildCommittedEvent(this, RunnerReturn.CommittedWithCustomDacpac);
                    }
                    else
                    {
                        buildResultsModel.FinalStatus = BuildItemStatus.FailedWithCustomDacpac;
                    }
                }
                else if (stat == DacpacDeltasStatus.InSync || stat == DacpacDeltasStatus.OnlyPostDeployment)
                {
                    buildResultsModel.FinalStatus = BuildItemStatus.AlreadyInSync;
                    if (BuildCommittedEvent != null)
                        BuildCommittedEvent(this, RunnerReturn.DacpacDatabasesInSync);
                }

            }

            //If a timeout gets here.. need to decide how to label the rollback
            if (buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout)
            {
                buildResultsModel.FinalStatus = allowableTimeoutRetries > 0 ? BuildItemStatus.RolledBackAfterRetries : BuildItemStatus.RolledBack;
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

        internal BuildPreparationResult PrepareBuildForRun(BuildModels.SqlSyncBuildDataModel buildDataModelParam, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl)
        {
            try
            {
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

               
                scriptLogFileName = Path.Combine(projectFilePath, $"LogFile-{DateTime.Now:yyyy-MM-dd at HH_mm_ss}.log");
                log.LogInformation($"Creating Script Log File: {scriptLogFileName}");

                
                var nextBuildId = new Guid().ToString();
                log.LogInformation($"Generating Build Record ID: {nextBuildId}");

                var myBuild = new BuildModels.Build(
                    name: buildDescription,
                    buildType: buildType,
                    buildStart: DateTime.Now,
                    buildEnd: null,
                    serverName: serverName,
                    finalStatus: null,
                    buildId: Guid.NewGuid().ToString(),
                    userId: Environment.UserName);

                // add the build to model
                var builds = buildDataModelParam.Build?.ToList() ?? new List<BuildModels.Build>();
                builds.Add(myBuild);
                buildDataModelParam = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: buildDataModelParam.SqlSyncBuildProject,
                    script: buildDataModelParam.Script,
                    build: builds,
                    scriptRun: buildDataModelParam.ScriptRun,
                    committedScript: buildDataModelParam.CommittedScript,
                    codeReview: buildDataModelParam.CodeReview);

                log.LogInformation("Reading Scripting configuration");
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
                        myBuild.FinalStatus = BuildItemStatus.PendingRollBack;
                    else
                        myBuild.FinalStatus = BuildItemStatus.RolledBack;

                    return new BuildPreparationResult(new List<BuildModels.Script>(), myBuild, string.Empty);
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

        internal BuildModels.Build RunBuildScripts(IList<BuildModels.Script> scripts, BuildModels.Build myBuild, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, BuildModels.SqlSyncBuildDataModel buildDataModel)
        {
            var runner = SqlBuildRunnerFactory(ConnectionsService, this, this, null);
            return runner.Run(scripts, myBuild, serverName, isMultiDbRun, scriptBatchColl, buildDataModel);
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
                        log.LogInformation($"Creating External Log file directory '{tmpPath}'");
                        Directory.CreateDirectory(tmpPath);
                    }

                    File.Copy(scriptLogFileName, externalScriptLogFileName, true);
                    log.LogInformation($"Copied log file to '{externalScriptLogFileName}'");
                }
                catch (Exception exe)
                {
                    log.LogError($"Error copying results file to '{externalScriptLogFileName}': {exe}");
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

        //internal void SaveBuildDataModel(bool fireSavedEvent)
        //{
        //    log.LogInformation("Saving Build File Updates");

        //    if (projectFileName == null || projectFileName.Length == 0)
        //    {
        //        string message = "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.";
        //        log.LogWarning(message);
        //        throw new ArgumentException(message);
        //    }

        //    SqlBuildFileHelper.SaveSqlBuildProjectFile(buildDataModel, projectFileName, buildFileName, includeHistoryAndLogs: true);


        //    if (buildHistoryXmlFile == null || buildHistoryXmlFile.Length == 0)
        //    {
        //        string message = "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.";
        //        log.LogWarning(message);
        //        throw new ArgumentException(message);
        //    }

        //    if (buildHistoryModel != null)
        //        SqlSyncBuildDataXmlSerializer.Save(buildHistoryXmlFile, buildHistoryModel); //TODO: Fix this so it writes an actual file!!
        //        //buildHistoryData.WriteXml(buildHistoryXmlFile);

        //    if (fireSavedEvent)
        //        bgWorker.ReportProgress(0, new ScriptRunProjectFileSavedEventArgs(true));
        //}

        private void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild)
        {
            if (run == null) return;
            buildHistoryModel = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: buildHistoryModel.SqlSyncBuildProject,
                script: buildHistoryModel.Script,
                build: buildHistoryModel.Build,
                scriptRun: buildHistoryModel.ScriptRun.Concat(new[] { run }).ToList(),
                committedScript: buildHistoryModel.CommittedScript,
                codeReview: buildHistoryModel.CodeReview);
            if(!buildHistoryModel.Build.Contains(myBuild))
            {
                buildHistoryModel = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: buildHistoryModel.SqlSyncBuildProject,
                    script: buildHistoryModel.Script,
                    build: buildHistoryModel.Build.Concat(new[] { myBuild }).ToList(),
                    scriptRun: buildHistoryModel.ScriptRun,
                    committedScript: buildHistoryModel.CommittedScript,
                    codeReview: buildHistoryModel.CodeReview);
            }
            // optionally keep dataset for compatibility
            //if (buildHistoryData != null)
            //{
            //    var row = buildHistoryData.ScriptRun.NewScriptRunRow();
            //    row.Database = run.Database;
            //    row.RunOrder = run.RunOrder ?? 0;
            //    row.RunStart = run.RunStart ?? DateTime.MinValue;
            //    row.FileName = run.FileName;
            //    row.ScriptRunId = run.ScriptRunId;
            //    if (run.FileHash != null) row.FileHash = run.FileHash;
            //    if (run.RunEnd.HasValue) row.RunEnd = run.RunEnd.Value;
            //    if (run.Success.HasValue) row.Success = run.Success.Value;
            //    if (run.Results != null) row.Results = run.Results;
            //    buildHistoryData.ScriptRun.AddScriptRunRow(row);
            //    buildHistoryData.AcceptChanges();
            //}
        }



        #region ISqlBuildRunnerProperties

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
        #endregion

        #region ISqlBuildRunnerContext
        ILogger ISqlBuildRunnerContext.Log => log;
        IProgressReporter ISqlBuildRunnerContext.ProgressReporter => ProgressReporter ?? new DefaultProgressReporter();
        string ISqlBuildRunnerContext.GetTargetDatabase(string defaultDatabase) => GetTargetDatabase(defaultDatabase);
        Task<string[]> ISqlBuildRunnerContext.ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken) => ScriptBatcher.ReadBatchFromScriptFileAsync(path, stripTransaction, useRegex, cancellationToken);
        string ISqlBuildRunnerContext.PerformScriptTokenReplacement(string script) => PerformScriptTokenReplacement(script);
        Task<string> ISqlBuildRunnerContext.PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken) => TokenReplacementService.ReplaceTokensAsync(script, this, cancellationToken);
        void ISqlBuildRunnerContext.AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) => AddScriptRunToHistory(run, myBuild);
        void ISqlBuildRunnerContext.PublishScriptLog(bool isError, ScriptLogEventArgs args) => ScriptLogWriteEvent?.Invoke(null, isError, args);
        #endregion

        #region IBuildFinalizerContext implementation
        bool IBuildFinalizerContext.IsTransactional => isTransactional;
        bool IBuildFinalizerContext.IsTrialBuild => isTrialBuild;
        bool IBuildFinalizerContext.RunScriptOnly => runScriptOnly;
        List<LoggingCommittedScript> IBuildFinalizerContext.CommittedScripts => committedScripts;

        void IBuildFinalizerContext.RaiseBuildCommittedEvent(object sender, RunnerReturn rr) => BuildCommittedEvent?.Invoke(sender, rr);
        void IBuildFinalizerContext.RaiseBuildSuccessTrialRolledBackEvent(object sender) => BuildSuccessTrialRolledBackEvent?.Invoke(sender, EventArgs.Empty);
        void IBuildFinalizerContext.RaiseBuildErrorRollBackEvent(object sender) => BuildErrorRollBackEvent?.Invoke(sender, EventArgs.Empty);
        #endregion

 
    }
}
