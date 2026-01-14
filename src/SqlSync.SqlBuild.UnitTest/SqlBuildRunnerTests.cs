using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild;
using Microsoft.Extensions.Logging.Abstractions;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerTests
    {
        private const string ScriptId = "abc";

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsTrue_WhenCommitted()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(ctx);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = model with
            {
                CommittedScript = new List<BuildModels.CommittedScript>
                {
                    new BuildModels.CommittedScript(ScriptId, ServerName: null, CommittedDate: null, AllowScriptBlock: null, ScriptHash: null, SqlSyncBuildProject_Id: null)
                }
            };

            var result = runner.ShouldSkipDueToCommittedScripts(ScriptId, model);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LoadBatchScripts_PrefersPreBatchedScripts()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(ctx);
            var coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("file.sql", new[] { "SELECT 1;" }, ScriptId));

            var result = runner.LoadBatchScripts(ScriptId, "file.sql", stripTransaction: false, scriptBatchColl: coll);

            CollectionAssert.AreEqual(new[] { "SELECT 1;" }, result);
        }

        [TestMethod]
        public void LoadBatchScripts_ReadsViaContext_WhenNoPreBatch()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "SELECT 2;" } };
            var runner = new SqlBuildRunner(ctx);

            var result = runner.LoadBatchScripts(ScriptId, "file.sql", stripTransaction: false, scriptBatchColl: null);

            CollectionAssert.AreEqual(new[] { "SELECT 2;" }, result);
        }

        private sealed class FakeRunnerContext : ISqlBuildRunnerContext
        {
            public string[] ReadBatchReturn { get; set; } = Array.Empty<string>();

            public Microsoft.Extensions.Logging.ILogger Log => NullLogger.Instance;
            public BackgroundWorker BgWorker => throw new NotImplementedException();
            public IProgressReporter ProgressReporter => null;
            public bool IsTransactional => false;
            public bool IsTrialBuild => false;
            public bool RunScriptOnly => false;
            public string BuildPackageHash => string.Empty;
            public string ProjectFilePath => string.Empty;
            public List<LoggingCommittedScript> CommittedScripts => new();
            public bool ErrorOccured { get; set; }
            public string SqlInfoMessage { get; set; } = string.Empty;
            public int DefaultScriptTimeout => 30;
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default) => Task.FromResult(new SqlExecutionResult(true, string.Empty));

            public BuildConnectData GetConnectionDataClass(string serverName, string databaseName) => throw new NotImplementedException();
            public string GetTargetDatabase(string defaultDatabase) => defaultDatabase;
            public string[] ReadBatchFromScriptFile(string path, bool stripTransaction, bool useRegex) => ReadBatchReturn;
            public string PerformScriptTokenReplacement(string script) => script;
            public void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) => throw new NotImplementedException();
            public void RollbackBuild() => throw new NotImplementedException();
            public void SaveBuildDataSet(bool fireSavedEvent) => throw new NotImplementedException();
            public BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs) => throw new NotImplementedException();
            public void PublishScriptLog(bool isError, ScriptLogEventArgs args) => throw new NotImplementedException();
        }
    }
}
