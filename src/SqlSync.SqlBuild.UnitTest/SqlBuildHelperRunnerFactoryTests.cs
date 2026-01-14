using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.Connection;
using BuildModels = SqlSync.SqlBuild.Models;
using SqlLogging = SqlSync.SqlBuild.SqlLogging;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildHelperRunnerFactoryTests
    {
        [TestMethod]
        public void RunBuildScripts_UsesRunnerFactory()
        {
            // Arrange
            var helper = new SqlBuildHelper(new ConnectionData())
            {
                // Ensure bgWorker not null for reporting
                bgWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true }
            };

            var expectedBuild = new BuildModels.Build(
                Name: "test",
                BuildType: "type",
                BuildStart: DateTime.Now,
                BuildEnd: null,
                ServerName: "srv",
                FinalStatus: BuildItemStatus.Committed.ToString(),
                BuildId: Guid.NewGuid().ToString(),
                UserId: "user",
                Build_Id: 1,
                Builds_Id: null);

            SqlBuildHelper.SqlBuildRunnerFactory = (ctx, exec) => new FakeRunner(expectedBuild);

            var scripts = new List<BuildModels.Script>();
            var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var dwea = new DoWorkEventArgs(null);

            // Act
            var result = helper.RunBuildScripts(scripts, expectedBuild, "srv", false, null, buildData, ref dwea);

            // Assert
            Assert.AreEqual(expectedBuild.BuildId, result.BuildId);
            Assert.AreEqual(BuildItemStatus.Committed.ToString(), result.FinalStatus);
        }

        private sealed class FakeRunner : SqlBuildRunner
        {
            private readonly BuildModels.Build _result;
            public FakeRunner(BuildModels.Build result) : base(new FakeContext(), null)
            {
                _result = result;
            }

            public override BuildModels.Build Run(
                IReadOnlyList<BuildModels.Script> scripts,
                BuildModels.Build myBuild,
                string serverName,
                bool isMultiDbRun,
                ScriptBatchCollection scriptBatchColl,
                BuildModels.SqlSyncBuildDataModel buildDataModel,
                ref DoWorkEventArgs workEventArgs)
            {
                return _result;
            }
        }

        private sealed class FakeContext : ISqlBuildRunnerContext
        {
            public Microsoft.Extensions.Logging.ILogger Log => NullLogger.Instance;
            public System.ComponentModel.BackgroundWorker BgWorker { get; } = new BackgroundWorker();
                public IProgressReporter ProgressReporter => null;
            public bool IsTransactional => true;
            public bool IsTrialBuild => false;
            public bool RunScriptOnly => false;
            public string BuildPackageHash => string.Empty;
            public string ProjectFilePath => string.Empty;
            public List<SqlLogging.CommittedScript> CommittedScripts => new();
            public bool ErrorOccured { get; set; }
            public string SqlInfoMessage { get; set; } = string.Empty;
            public int DefaultScriptTimeout => 30;
            public BuildConnectData GetConnectionDataClass(string serverName, string databaseName) => throw new NotImplementedException();
            public string GetTargetDatabase(string defaultDatabase) => defaultDatabase;
            public string[] ReadBatchFromScriptFile(string path, bool stripTransaction, bool useRegex) => Array.Empty<string>();
            public Task<string[]> ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<string>());
            public string PerformScriptTokenReplacement(string script) => script;
            public Task<string> PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken = default) => Task.FromResult(script);
            public void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild) { }
            public void RollbackBuild() { }
            public void SaveBuildDataSet(bool fireSavedEvent) { }
            public BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs) => myBuild;
            public void PublishScriptLog(bool isError, ScriptLogEventArgs args) { }
        }
    }
}
