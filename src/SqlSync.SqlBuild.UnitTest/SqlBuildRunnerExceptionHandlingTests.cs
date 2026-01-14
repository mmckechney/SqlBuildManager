using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerExceptionHandlingTests
    {
        [TestMethod]
        public void Run_MarksTimeoutFailure_WhenSqlExceptionTimeout()
        {
            var bg = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            var ctx = new FakeRunnerContext(bg) { IsTransactionalValue = true };
            var runner = new SqlBuildRunner(ctx, new SqlCommandExecutorThatThrows(CreateTimeoutException()));

            var scripts = new List<BuildModels.Script>
            {
                new BuildModels.Script(
                    FileName: "file.sql",
                    BuildOrder: 1,
                    Description: null,
                    RollBackOnError: true,
                    CausesBuildFailure: true,
                    DateAdded: null,
                    ScriptId: "abc",
                    Database: "db",
                    StripTransactionText: false,
                    AllowMultipleRuns: true,
                    AddedBy: null,
                    ScriptTimeOut: 5,
                    DateModified: null,
                    ModifiedBy: null,
                    Scripts_Id: null,
                    Tag: null)
            };
            var myBuild = new BuildModels.Build(
                Name: "name",
                BuildType: "type",
                BuildStart: DateTime.UtcNow,
                BuildEnd: null,
                ServerName: "srv",
                FinalStatus: null,
                BuildId: Guid.NewGuid().ToString(),
                UserId: "user",
                Build_Id: 1,
                Builds_Id: null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel() with { Script = scripts };
            var e = new DoWorkEventArgs(null);

            var result = runner.Run(scripts, myBuild, "srv", isMultiDbRun: false, scriptBatchColl: new ScriptBatchCollection { new ScriptBatch("file.sql", new[] { "SELECT 1;" }, "abc") }, buildDataModel: model, ref e);

            Assert.AreEqual(BuildItemStatus.FailedDueToScriptTimeout.ToString(), result.FinalStatus);
            Assert.IsTrue(ctx.ErrorOccured);
        }

        private static SqlException CreateTimeoutException()
        {
            // Build a SqlError
            var errorCtor = typeof(SqlError)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(c => c.GetParameters().Length == 9);
            var sqlError = (SqlError)errorCtor.Invoke(new object[]
            {
                0, // infoNumber
                (byte)0, // errorState
                (byte)0, // errorClass
                "server", // server
                "timeout expired.", // errorMessage
                "proc", // procedure
                0, // lineNumber
                (uint)0, // win32ErrorCode
                null // exception
            });

            var errorCollection = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), true);
            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(errorCollection, new object[] { sqlError });

            var exceptionFactory = typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(SqlErrorCollection), typeof(string) }, null);
            var sqlException = (SqlException)exceptionFactory.Invoke(null, new object[] { errorCollection, "7.0.0" });
            return sqlException;
        }

        private sealed class SqlCommandExecutorThatThrows : ISqlCommandExecutor
        {
            private readonly Exception _ex;
            public SqlCommandExecutorThatThrows(Exception ex) => _ex = ex;
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => throw _ex;
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
                => Task.Run(() => Execute(sql, timeoutSeconds, cData, isTransactional), cancellationToken);
        }

        private sealed class FakeRunnerContext : ISqlBuildRunnerContext
        {
            public FakeRunnerContext(BackgroundWorker bg) => BgWorkerValue = bg;
            public Microsoft.Extensions.Logging.ILogger Log => NullLogger.Instance;
            public BackgroundWorker BgWorkerValue { get; }
            public BackgroundWorker BgWorker => BgWorkerValue;
            public IProgressReporter ProgressReporter => null;
            public bool IsTransactionalValue { get; set; }
            public bool IsTransactional => IsTransactionalValue;
            public bool IsTrialBuild => false;
            public bool RunScriptOnly => false;
            public string BuildPackageHash => string.Empty;
            public string ProjectFilePath => System.IO.Path.GetTempPath();
            public List<LoggingCommittedScript> CommittedScripts { get; } = new();
            public bool ErrorOccured { get; set; } = false;
            public string SqlInfoMessage { get; set; } = string.Empty;
            public int DefaultScriptTimeout => 5;

            public BuildConnectData GetConnectionDataClass(string serverName, string databaseName) => new BuildConnectData { ServerName = serverName, DatabaseName = databaseName };
            public string GetTargetDatabase(string defaultDatabase) => defaultDatabase;
            public string[] ReadBatchFromScriptFile(string path, bool stripTransaction, bool useRegex) => new[] { "SELECT 1;" };
            public Task<string[]> ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken = default) => Task.FromResult(new[] { "SELECT 1;" });
            public string PerformScriptTokenReplacement(string script) => script;
            public Task<string> PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken = default) => Task.FromResult(script);
            public void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) { }
            public void RollbackBuild() { }
            public void SaveBuildDataSet(bool fireSavedEvent) { }
            public BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs) => myBuild with { FinalStatus = buildFailure ? BuildItemStatus.FailedDueToScriptTimeout.ToString() : BuildItemStatus.Committed.ToString() };
            public void PublishScriptLog(bool isError, ScriptLogEventArgs args) { }
        }
    }
}
