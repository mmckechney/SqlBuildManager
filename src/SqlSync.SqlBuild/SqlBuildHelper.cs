using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.SqlBuild.Legacy;
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

        #region Core Build State Fields

        internal bool isTransactional = true;
        internal bool isTrialBuild = false;
        internal bool runScriptOnly = false;
        internal List<DatabaseOverride> targetDatabaseOverrides = null;
        internal BuildModels.SqlSyncBuildDataModel buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
        internal SqlSyncBuildData buildDataCompat;
        internal string buildPackageHash = string.Empty;

        #endregion

        #region Build Configuration Fields

        private ConnectionData connData;
        private string buildType;
        internal string buildDescription;
        private double startIndex;
        internal string projectFileName;
        private string projectFilePath = string.Empty;
        internal string buildFileName = string.Empty;
        internal string scriptLogFileName;
        internal string externalScriptLogFileName = string.Empty;
        internal string buildHistoryXmlFile = string.Empty;
        private string logToDatabaseName = string.Empty;
        private string buildRequestedBy = string.Empty;
        internal double[] runItemIndexes = new double[0];
        internal bool errorOccured;

        #endregion

        #region Runtime State Fields

        private MultiDbData multiDbRunData;
        private string sqlInfoMessage = string.Empty;
        private string lastSqlMessage = string.Empty;
        private System.Guid currentBuildId = System.Guid.Empty;
        private List<LoggingCommittedScript> committedScripts = new List<LoggingCommittedScript>();

        #endregion

        #region Dependency Injection Properties

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
        internal Services.IRunnerFactory RunnerFactory { get; }
        internal Services.IScriptLogWriter ScriptLogWriter { get; }
        internal Services.IBuildHistoryTracker BuildHistoryTracker { get; }

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
            ILegacyBuildDataAdapter legacyAdapter = null,
            IDatabaseUtility databaseUtility = null,
            IConnectionsService connectionsService = null,
            IBuildFinalizer buildFinalizer = null)
            : this(data, createScriptRunLogFile, externalScriptLogFileName, isTransactional,
                   clock, guidProvider, fileSystem, progressReporter, fileHelper, retryPolicy,
                   legacyAdapter, databaseUtility, connectionsService, buildFinalizer, null)
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
            ILegacyBuildDataAdapter legacyAdapter,
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
            LegacyAdapter = legacyAdapter ?? new DefaultLegacyBuildDataAdapter();
            RunnerFactory = runnerFactory ?? new Services.DefaultRunnerFactory();
            ScriptLogWriter = new Services.DefaultScriptLogWriter();
            BuildHistoryTracker = new Services.DefaultBuildHistoryTracker();
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

            log.LogInformation($"Starting Build Process targeting: {serverName} ");

            var prep = PrepareBuildForRun(buildDataModel, serverName, isMultiDbRun, scriptBatchColl);
            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return prep.Build;
            }

            var orchestrator = new Services.SqlBuildOrchestrator(this, this, this.RetryPolicy, this, ConnectionsService, SqlLoggingService, RunnerFactory);
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
                (var stat, var updatedRunData) = DacPacHelper.UpdateBuildRunDataForDacPacSync(runData,  serverName, targetDatabase, connData.AuthenticationType, connData.UserId, connData.Password, projectFilePath, runData.BuildRevision ?? string.Empty, runData.DefaultScriptTimeout, runData.AllowObjectDelete ?? false, connData.ManagedIdentityClientId);

                if (stat == DacpacDeltasStatus.Success)
                {
                    //var tempRunData = MapToRunDataModel(legacyRunData);
                    //var updatedRunData = new BuildModels.SqlBuildRunDataModel(
                    //    buildDataModel: tempRunData.BuildDataModel,
                    //    buildType: tempRunData.BuildType,
                    //    server: tempRunData.Server,
                    //    buildDescription: tempRunData.BuildDescription,
                    //    startIndex: tempRunData.StartIndex,
                    //    projectFileName: tempRunData.ProjectFileName,
                    //    isTrial: tempRunData.IsTrial,
                    //    runItemIndexes: tempRunData.RunItemIndexes,
                    //    runScriptOnly: tempRunData.RunScriptOnly,
                    //    buildFileName: tempRunData.BuildFileName,
                    //    logToDatabaseName: tempRunData.LogToDatabaseName,
                    //    isTransactional: tempRunData.IsTransactional,
                    //    platinumDacPacFileName: string.Empty,
                    //    targetDatabaseOverrides: tempRunData.TargetDatabaseOverrides,
                    //    forceCustomDacpac: tempRunData.ForceCustomDacpac,
                    //    buildRevision: tempRunData.BuildRevision,
                    //    defaultScriptTimeout: tempRunData.DefaultScriptTimeout,
                    //    allowObjectDelete: tempRunData.AllowObjectDelete);
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

        private void SyncBuildDataModel(SqlSyncBuildData ds) => buildDataModel = ds.ToModel();

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
