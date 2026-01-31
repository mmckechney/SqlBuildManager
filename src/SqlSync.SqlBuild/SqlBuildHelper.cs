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
        #region Static Members

        [Obsolete("Use injected IRunnerFactory instead. Will be removed in future version.")]
        internal static Func<IConnectionsService, ISqlBuildRunnerContext, IBuildFinalizerContext, ISqlCommandExecutor, SqlBuildRunner> SqlBuildRunnerFactory = (connService, ctx, finalizerContext, exec) => new SqlBuildRunner(connService, ctx, finalizerContext, exec);
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Build State

        private readonly BuildModels.BuildExecutionState _state = new BuildModels.BuildExecutionState();
        private ConnectionData connData;

        // Legacy compatibility - these delegate to _state
        internal bool isTransactional { get => _state.IsTransactional; set => _state.IsTransactional = value; }
        internal bool isTrialBuild { get => _state.IsTrialBuild; set => _state.IsTrialBuild = value; }
        internal bool runScriptOnly { get => _state.RunScriptOnly; set => _state.RunScriptOnly = value; }
        internal List<DatabaseOverride> targetDatabaseOverrides { get => _state.TargetDatabaseOverrides; set => _state.TargetDatabaseOverrides = value; }
        internal BuildModels.SqlSyncBuildDataModel buildDataModel { get => _state.BuildDataModel; set => _state.BuildDataModel = value; }
        internal string buildPackageHash { get => _state.BuildPackageHash; set => _state.BuildPackageHash = value; }
        private string buildType { get => _state.BuildType; set => _state.BuildType = value; }
        internal string buildDescription { get => _state.BuildDescription; set => _state.BuildDescription = value; }
        private double startIndex { get => _state.StartIndex; set => _state.StartIndex = value; }
        internal string projectFileName { get => _state.ProjectFileName; set => _state.ProjectFileName = value; }
        private string projectFilePath { get => _state.ProjectFilePath; set => _state.ProjectFilePath = value; }
        internal string buildFileName { get => _state.BuildFileName; set => _state.BuildFileName = value; }
        internal string scriptLogFileName { get => _state.ScriptLogFileName; set => _state.ScriptLogFileName = value; }
        internal string externalScriptLogFileName { get => _state.ExternalScriptLogFileName; set => _state.ExternalScriptLogFileName = value; }
        internal string buildHistoryXmlFile { get => _state.BuildHistoryXmlFile; set => _state.BuildHistoryXmlFile = value; }
        private string logToDatabaseName { get => _state.LogToDatabaseName; set => _state.LogToDatabaseName = value; }
        private string buildRequestedBy { get => _state.BuildRequestedBy; set => _state.BuildRequestedBy = value; }
        internal double[] runItemIndexes { get => _state.RunItemIndexes; set => _state.RunItemIndexes = value; }
        internal bool errorOccured { get => _state.ErrorOccurred; set => _state.ErrorOccurred = value; }
        private MultiDbData multiDbRunData { get => _state.MultiDbRunData; set => _state.MultiDbRunData = value; }
        private string sqlInfoMessage { get => _state.SqlInfoMessage; set => _state.SqlInfoMessage = value; }
        private List<LoggingCommittedScript> committedScripts => _state.CommittedScripts;

        // Expose the state object for services that need direct access
        internal BuildModels.BuildExecutionState State => _state;

        #endregion

        #region Dependency Injection Properties

        internal IClock Clock { get; }
        internal IGuidProvider GuidProvider { get; }
        internal IFileSystem FileSystem { get; }
        internal IProgressReporter ProgressReporter { get; private set; }
        internal ISqlBuildFileHelper FileHelper { get; }
        internal IBuildRetryPolicy RetryPolicy { get; }
        internal Services.IBuildPreparationService BuildPreparationService { get; }
        internal Services.IScriptBatcher ScriptBatcher { get; }
        internal Services.ITokenReplacementService TokenReplacementService { get; }
        internal Services.ISqlLoggingService SqlLoggingService { get; }
        internal Services.IDatabaseUtility DatabaseUtility { get; }
        internal Services.IConnectionsService ConnectionsService { get; }
        internal Services.IBuildFinalizer BuildFinalizer { get; }
        internal Services.IRunnerFactory RunnerFactory { get; }
        internal Services.IScriptLogWriter ScriptLogWriter { get; }
        internal Services.IBuildHistoryTracker BuildHistoryTracker { get; }
        internal Services.IDacPacFallbackHandler DacPacFallbackHandler { get; }

        #endregion

        #region Events

        public event ScriptLogWriteEventHandler ScriptLogWriteEvent;
        public event BuildCommittedEventHandler BuildCommittedEvent;
        public event EventHandler BuildSuccessTrialRolledBackEvent;
        public event EventHandler BuildErrorRollBackEvent;

        #endregion

        #region Constructors

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
            IDatabaseUtility databaseUtility = null,
            IConnectionsService connectionsService = null,
            IBuildFinalizer buildFinalizer = null)
            : this(data, createScriptRunLogFile, externalScriptLogFileName, isTransactional,
                   clock, guidProvider, fileSystem, progressReporter, fileHelper, retryPolicy,
                   databaseUtility, connectionsService, buildFinalizer, null)
        {
        }

        internal SqlBuildHelper(
            ConnectionData data,
            bool createScriptRunLogFile,
            string externalScriptLogFileName,
            bool isTransactional,
            IClock clock,
            IGuidProvider guidProvider,
            IFileSystem fileSystem,
            IProgressReporter progressReporter,
            ISqlBuildFileHelper fileHelper,
            IBuildRetryPolicy retryPolicy,
            IDatabaseUtility databaseUtility,
            IConnectionsService connectionsService,
            IBuildFinalizer buildFinalizer,
            IRunnerFactory runnerFactory)
        {
            connData = data;
            this.isTransactional = isTransactional;
            Clock = clock ?? new SystemClock();
            GuidProvider = guidProvider ?? new GuidProvider();
            FileSystem = fileSystem ?? new DotNetFileSystem();
            ProgressReporter = progressReporter ?? new DefaultProgressReporter();
            FileHelper = fileHelper ?? new DefaultSqlBuildFileHelper();
            RetryPolicy = retryPolicy ?? new DefaultBuildRetryPolicy();
            RunnerFactory = runnerFactory ?? new Services.DefaultRunnerFactory();
            ScriptLogWriter = new Services.DefaultScriptLogWriter();
            BuildHistoryTracker = new Services.DefaultBuildHistoryTracker();
            DacPacFallbackHandler = new Services.DefaultDacPacFallbackHandler();
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

        #endregion


        #region Public Methods

        public BuildResultStatus ProcessMultiDbBuild(MultiDbData multiDbRunData, string projectFileName)
        {
            this.projectFileName = projectFileName;
            return ProcessMultiDbBuild(multiDbRunData);
        }
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
                    isTransactional: multiDbRunData.IsTransactional,
                    platinumDacPacFileName: string.Empty,
                    targetDatabaseOverrides: srvData.Overrides,
                    forceCustomDacpac: false,
                    buildRevision: multiDbRunData.BuildRevision,
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

        public async Task<BuildModels.Build> ProcessBuild(BuildModels.SqlBuildRunDataModel runData, int allowableTimeoutRetries = 3, string buildRequestedBy = "", ScriptBatchCollection scriptBatchColl = null)
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
            errorOccured = false;
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

            //log.LogInformation($"Starting Build Process targeting: {serverName} ");

            var prep = PrepareBuildForRun(buildDataModel, serverName, isMultiDbRun, scriptBatchColl);
            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return prep.Build;
            }

            var orchestrator = new Services.SqlBuildOrchestrator(this, this, this.RetryPolicy, this, ConnectionsService, SqlLoggingService, RunnerFactory);
            var buildResultsModel = orchestrator.Execute(runData, prep, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);

            // Handle DacPac fallback for failed builds
            bool candidateForCustomDacPac = DacPacFallbackHandler.IsCandidateForDacPacFallback(buildResultsModel.FinalStatus ?? BuildItemStatus.Unknown);
            
            if (buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout || 
                buildResultsModel.FinalStatus == BuildItemStatus.FailedWithCustomDacpac)
            {
                log.LogWarning($"Build was not successful. Status is {buildResultsModel.FinalStatus} and Platinum DACPAC name is '{runData.PlatinumDacPacFileName}', and this file exists '{File.Exists(runData.PlatinumDacPacFileName ?? string.Empty)}' ");
            }

            if (candidateForCustomDacPac)
            {
                var dacPacContext = new Services.DacPacFallbackContext
                {
                    RunData = runData,
                    Prep = prep,
                    ServerName = serverName,
                    IsMultiDbRun = isMultiDbRun,
                    ScriptBatchColl = scriptBatchColl,
                    AllowableTimeoutRetries = allowableTimeoutRetries,
                    ConnectionData = connData,
                    ProjectFilePath = projectFilePath,
                    ProcessBuildCallback = ProcessBuild,
                    GetTargetDatabaseCallback = GetTargetDatabase,
                    RaiseBuildCommittedEvent = rr => BuildCommittedEvent?.Invoke(this, rr)
                };

                var fallbackResult = DacPacFallbackHandler.TryDacPacFallback(dacPacContext, buildResultsModel);
                if (fallbackResult.NewStatus.HasValue)
                {
                    buildResultsModel.FinalStatus = fallbackResult.NewStatus.Value;
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
                    finalStatus: BuildItemStatus.Unknown,
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
                    buildPackageHash = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFilePath, buildDataModelParam);
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
            var runner = RunnerFactory.Create(ConnectionsService, this, this, null);
            return runner.Run(scripts, myBuild, serverName, isMultiDbRun, scriptBatchColl, buildDataModel);
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

        #endregion

        #region Internal Helper Methods

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

        internal string PerformScriptTokenReplacement(string script)
        {
            return TokenReplacementService.ReplaceTokens(script, this);
        }

        internal void SqlBuildHelper_ScriptLogWriteEvent(object sender, bool isError, ScriptLogEventArgs e)
        {
            var context = new Services.ScriptLogWriteContext
            {
                ScriptLogFileName = scriptLogFileName,
                ExternalScriptLogFileName = externalScriptLogFileName,
                ServerName = connData?.SQLServerName ?? string.Empty,
                IsTransactional = isTransactional
            };
            ScriptLogWriter.WriteLog(context, isError, e);
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
            BuildHistoryTracker.AddScriptRunToHistory(run, myBuild);
        }

        #endregion

        #region Interface Implementations - ISqlBuildRunnerProperties

        bool ISqlBuildRunnerProperties.IsTransactional => isTransactional;
        bool ISqlBuildRunnerProperties.IsTrialBuild => isTrialBuild;
        bool ISqlBuildRunnerProperties.RunScriptOnly => runScriptOnly;
        string ISqlBuildRunnerProperties.BuildPackageHash => buildPackageHash;
        string ISqlBuildRunnerProperties.ProjectFilePath => projectFilePath;
        string ISqlBuildRunnerProperties.ProjectFileName => projectFileName;
        string ISqlBuildRunnerProperties.BuildFileName => buildFileName;
        string ISqlBuildRunnerProperties.BuildHistoryXmlFile => buildHistoryXmlFile;
        List<LoggingCommittedScript> ISqlBuildRunnerProperties.CommittedScripts => committedScripts;
        bool ISqlBuildRunnerProperties.ErrorOccured { get => errorOccured; set => errorOccured = value; }
        string ISqlBuildRunnerProperties.SqlInfoMessage { get => sqlInfoMessage; set => sqlInfoMessage = value; }
        int ISqlBuildRunnerProperties.DefaultScriptTimeout => connData?.ScriptTimeout ?? 30;
        BuildModels.SqlSyncBuildDataModel ISqlBuildRunnerProperties.BuildDataModel { get => buildDataModel; set => buildDataModel = value; }
        BuildModels.SqlSyncBuildDataModel ISqlBuildRunnerProperties.BuildHistoryModel { get => BuildHistoryTracker.BuildHistoryModel; set { /* Setter is no-op - history is managed by BuildHistoryTracker */ } }
        MultiDbData ISqlBuildRunnerProperties.MultiDbRunData => this.multiDbRunData;
        string ISqlBuildRunnerProperties.BuildRequestedBy => buildRequestedBy;
        string ISqlBuildRunnerProperties.BuildDescription => buildDescription;
        string ISqlBuildRunnerProperties.LogToDatabaseName { get => logToDatabaseName; set => logToDatabaseName = value; }
        ConnectionData ISqlBuildRunnerProperties.ConnectionData => connData;
        List<DatabaseOverride> ISqlBuildRunnerProperties.TargetDatabaseOverrides { get => targetDatabaseOverrides; }

        #endregion

        #region Interface Implementations - ISqlBuildRunnerContext
        ILogger ISqlBuildRunnerContext.Log => log;
        IProgressReporter ISqlBuildRunnerContext.ProgressReporter => ProgressReporter ?? new DefaultProgressReporter();
        string ISqlBuildRunnerContext.GetTargetDatabase(string defaultDatabase) => GetTargetDatabase(defaultDatabase);
        Task<string[]> ISqlBuildRunnerContext.ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken) => ScriptBatcher.ReadBatchFromScriptFileAsync(path, stripTransaction, useRegex, cancellationToken);
        string ISqlBuildRunnerContext.PerformScriptTokenReplacement(string script) => PerformScriptTokenReplacement(script);
        Task<string> ISqlBuildRunnerContext.PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken) => TokenReplacementService.ReplaceTokensAsync(script, this, cancellationToken);
        void ISqlBuildRunnerContext.AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) => AddScriptRunToHistory(run, myBuild);
        void ISqlBuildRunnerContext.PublishScriptLog(bool isError, ScriptLogEventArgs args) => ScriptLogWriteEvent?.Invoke(null, isError, args);

        #endregion

        #region Interface Implementations - IBuildFinalizerContext
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
