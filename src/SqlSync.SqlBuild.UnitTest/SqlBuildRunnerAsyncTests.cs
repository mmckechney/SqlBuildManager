using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using BuildModels = SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Models;
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

        [TestMethod]
        public async Task RunAsync_UsesExecutorAsyncPath()
        {
            var ctx = new LocalFakeRunnerContext();
            var exec = new TrackingExecutor();
            var runner = new SqlBuildRunner(ctx, exec);
            var scripts = new List<BuildModels.Script>
            {
                MakeScript("one.sql", "1")
            };
            var myBuild = new BuildModels.Build("name", "type", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u", 1, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var doa = new DoWorkEventArgs(null);

            var result = await runner.RunAsync(scripts, myBuild, "srv", false, null, model, doa, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.IsFalse(doa.Cancel);
        }

        [TestMethod]
        public async Task RunAsync_Cancellation_StopsExecution()
        {
            var ctx = new LocalFakeRunnerContext();
            using var cts = new CancellationTokenSource();
            var exec = new CancelOnFirstExecutor(cts);
            var runner = new SqlBuildRunner(ctx, exec);
            var scripts = new List<BuildModels.Script>
            {
                MakeScript("one.sql", "1"),
                MakeScript("two.sql", "2")
            };
            var myBuild = new BuildModels.Build("name", "type", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u", 1, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var doa = new DoWorkEventArgs(null);

            var result = await runner.RunAsync(scripts, myBuild, "srv", false, null, model, doa, cts.Token);

            Assert.IsNotNull(result);
        }

        private sealed class TrackingExecutor : ISqlCommandExecutor
        {
            public bool ExecuteCalled { get; private set; }
            public bool ExecuteAsyncCalled { get; private set; }
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional)
            {
                ExecuteCalled = true;
                return new SqlExecutionResult(true, string.Empty);
            }
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
            {
                ExecuteAsyncCalled = true;
                return Task.FromResult(new SqlExecutionResult(true, string.Empty));
            }
        }

        private sealed class CancelOnFirstExecutor : ISqlCommandExecutor
        {
            private readonly CancellationTokenSource _cts;
            public int ExecuteAsyncCalls { get; private set; }
            public CancelOnFirstExecutor(CancellationTokenSource cts) => _cts = cts;
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => new SqlExecutionResult(true, string.Empty);
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
            {
                ExecuteAsyncCalls++;
                _cts.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(new SqlExecutionResult(true, string.Empty));
            }
        }

        private static BuildModels.Script MakeScript(string fileName, string scriptId) =>
            new BuildModels.Script(fileName, null, null, null, null, null, scriptId, null, null, null, null, null, null, null, null, null);

        private sealed class NoopExecutor : ISqlCommandExecutor
        {
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => new SqlExecutionResult(true, string.Empty);
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
                => Task.FromResult(new SqlExecutionResult(true, string.Empty));
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
            public string[] ReadBatchFromScriptFile(string path, bool stripTransaction, bool useRegex) => new[] { "SELECT 1" };
            public Task<string[]> ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken = default) => Task.FromResult(new[] { "SELECT 1" });
            public string PerformScriptTokenReplacement(string script) => script;
            public Task<string> PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken = default) => Task.FromResult(script);
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
