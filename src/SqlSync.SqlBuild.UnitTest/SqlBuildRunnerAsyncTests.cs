using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerAsyncTests
    {
        [TestMethod]
        public async Task RunAsync_UsesSyncPathAndReturnsBuild()
        {
            var ctx = new LocalFakeRunnerContext();
            var runner = new SqlBuildRunner(ctx, new NoopExecutor());
            var scripts = new List<BuildModels.Script>();
            var myBuild = new BuildModels.Build("name", "type", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u", 1, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var doa = new DoWorkEventArgs(null);

            var result = await runner.RunAsync(scripts, myBuild, "srv", false, null, model, doa, CancellationToken.None);
            Assert.IsNotNull(result);
        }

        private sealed class NoopExecutor : ISqlCommandExecutor
        {
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => new SqlExecutionResult(true, string.Empty);
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default) => Task.FromResult(new SqlExecutionResult(true, string.Empty));
        }

        private sealed class LocalFakeRunnerContext : ISqlBuildRunnerContext
        {
            private static readonly IProgressReporter NoopProgress = new NoopProgressReporter();
            private readonly BackgroundWorker _bg = new BackgroundWorker { WorkerReportsProgress = true };
            public Microsoft.Extensions.Logging.ILogger Log => Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            public BackgroundWorker BgWorker => _bg;
            public IProgressReporter ProgressReporter => NoopProgress;
            public bool IsTransactional => false;
            public bool IsTrialBuild => false;
            public bool RunScriptOnly => false;
            public string BuildPackageHash => string.Empty;
            public string ProjectFilePath => string.Empty;
            public List<LoggingCommittedScript> CommittedScripts { get; } = new();
            public bool ErrorOccured { get; set; }
            public string SqlInfoMessage { get; set; } = string.Empty;
            public int DefaultScriptTimeout => 30;

            public BuildConnectData GetConnectionDataClass(string serverName, string databaseName) => new BuildConnectData { ServerName = serverName, DatabaseName = databaseName };
            public string GetTargetDatabase(string defaultDatabase) => defaultDatabase;
            public string[] ReadBatchFromScriptFile(string path, bool stripTransaction, bool useRegex) => Array.Empty<string>();
            public string PerformScriptTokenReplacement(string script) => script;
            public void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) { }
            public void RollbackBuild() { }
            public void SaveBuildDataSet(bool fireSavedEvent) { }
            public BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs) => myBuild;
            public void PublishScriptLog(bool isError, ScriptLogEventArgs args) { }

            private sealed class NoopProgressReporter : IProgressReporter
            {
                public bool CancellationPending => false;
                public void ReportProgress(int percent, object userState) { }
            }
        }
    }
}
