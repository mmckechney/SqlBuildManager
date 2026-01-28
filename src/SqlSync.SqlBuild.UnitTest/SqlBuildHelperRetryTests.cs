using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildHelperRetryTests
    {
        [TestMethod]
        public async Task ProcessBuild_RetriesOnTimeout_ThenSucceeds()
        {
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            var statuses = new Queue<BuildItemStatus>(new[]
            {
                BuildItemStatus.FailedDueToScriptTimeout,
                BuildItemStatus.Committed
            });
            var originalFactory = SqlBuildHelper.SqlBuildRunnerFactory;
            SqlBuildHelper.SqlBuildRunnerFactory = (connSvc, ctx, finalizerCtx, exec) => new TestSqlBuildRunner(ctx, statuses);
            try
            {
                var scriptId = "abc";
                var baseModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                var buildDataModel = new BuildModels.SqlSyncBuildDataModel(
                    sqlSyncBuildProject: baseModel.SqlSyncBuildProject,
                    script: new List<BuildModels.Script>
                    {
                        new BuildModels.Script(
                            fileName: "file.sql",
                            buildOrder: 1,
                            description: null,
                            rollBackOnError: true,
                            causesBuildFailure: true,
                            dateAdded: null,
                            scriptId: scriptId,
                            database: "db",
                            stripTransactionText: false,
                            allowMultipleRuns: true,
                            addedBy: null,
                            scriptTimeOut: 5,
                            dateModified: null,
                            modifiedBy: null,
                            tag: null)
                    },
                    build: baseModel.Build,
                    scriptRun: baseModel.ScriptRun,
                    committedScript: baseModel.CommittedScript,
                    codeReview: baseModel.CodeReview);
                var runData = new BuildModels.SqlBuildRunDataModel(
                    buildDataModel: buildDataModel,
                    buildType: "type",
                    server: "srv",
                    buildDescription: "desc",
                    startIndex: 0,
                    projectFileName: null,
                    isTrial: false,
                    runItemIndexes: System.Array.Empty<double>(),
                    runScriptOnly: false,
                    buildFileName: "file.sbm",
                    logToDatabaseName: string.Empty,
                    isTransactional: true,
                    platinumDacPacFileName: string.Empty,
                    targetDatabaseOverrides: null,
                    forceCustomDacpac: false,
                    buildRevision: null,
                    defaultScriptTimeout: 5,
                    allowObjectDelete: false);

                var scriptBatchColl = new ScriptBatchCollection();
                scriptBatchColl.Add(new ScriptBatch("file.sql", new[] { "SELECT 1;" }, scriptId));

                var bgWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                var e = new DoWorkEventArgs(null);

                var result = await helper.ProcessBuild(runData, allowableTimeoutRetries: 3, buildRequestedBy: string.Empty, scriptBatchColl: scriptBatchColl);

                Assert.AreEqual(BuildItemStatus.CommittedWithTimeoutRetries, result.FinalStatus);
            }
            finally
            {
                SqlBuildHelper.SqlBuildRunnerFactory = originalFactory;
            }
        }

        private sealed class TestSqlBuildRunner : SqlBuildRunner
        {
            private readonly Queue<BuildItemStatus> _statuses;
            public TestSqlBuildRunner(ISqlBuildRunnerContext ctx, Queue<BuildItemStatus> statuses) : base(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object) => _statuses = statuses;

            public override BuildModels.Build Run(
                System.Collections.Generic.IReadOnlyList<BuildModels.Script> scripts,
                BuildModels.Build myBuild,
                string serverName,
                bool isMultiDbRun,
                ScriptBatchCollection scriptBatchColl,
                BuildModels.SqlSyncBuildDataModel buildDataModel)
            {
                var status = _statuses.Count > 0 ? _statuses.Dequeue() : BuildItemStatus.Committed;
                return new BuildModels.Build(
                    name: myBuild.Name,
                    buildType: myBuild.BuildType,
                    buildStart: myBuild.BuildStart,
                    buildEnd: myBuild.BuildEnd,
                    serverName: myBuild.ServerName,
                    finalStatus: status,
                    buildId: myBuild.BuildId,
                    userId: myBuild.UserId);
            }
        }
    }
}
