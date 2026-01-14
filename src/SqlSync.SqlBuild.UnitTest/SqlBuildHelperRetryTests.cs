using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using BuildModels = SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildHelperRetryTests
    {
        [TestMethod]
        public void ProcessBuild_RetriesOnTimeout_ThenSucceeds()
        {
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            var statuses = new Queue<string>(new[]
            {
                BuildItemStatus.FailedDueToScriptTimeout.ToString(),
                BuildItemStatus.Committed.ToString()
            });
            var originalFactory = SqlBuildHelper.SqlBuildRunnerFactory;
            SqlBuildHelper.SqlBuildRunnerFactory = (ctx, exec) => new TestSqlBuildRunner(ctx, statuses);
            try
            {
                var scriptId = "abc";
                var runData = new BuildModels.SqlBuildRunDataModel(
                    BuildDataModel: SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel() with
                    {
                        Script = new List<BuildModels.Script>
                        {
                            new BuildModels.Script(
                                FileName: "file.sql",
                                BuildOrder: 1,
                                Description: null,
                                RollBackOnError: true,
                                CausesBuildFailure: true,
                                DateAdded: null,
                                ScriptId: scriptId,
                                Database: "db",
                                StripTransactionText: false,
                                AllowMultipleRuns: true,
                                AddedBy: null,
                                ScriptTimeOut: 5,
                                DateModified: null,
                                ModifiedBy: null,
                                Scripts_Id: null,
                                Tag: null)
                        }
                    },
                    BuildType: "type",
                    Server: "srv",
                    BuildDescription: "desc",
                    StartIndex: 0,
                    ProjectFileName: null,
                    IsTrial: false,
                    RunItemIndexes: System.Array.Empty<double>(),
                    RunScriptOnly: false,
                    BuildFileName: "file.sbm",
                    LogToDatabaseName: string.Empty,
                    IsTransactional: true,
                    PlatinumDacPacFileName: string.Empty,
                    TargetDatabaseOverrides: null,
                    ForceCustomDacpac: false,
                    BuildRevision: null,
                    DefaultScriptTimeout: 5,
                    AllowObjectDelete: false);

                var scriptBatchColl = new ScriptBatchCollection();
                scriptBatchColl.Add(new ScriptBatch("file.sql", new[] { "SELECT 1;" }, scriptId));

                var bgWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                var e = new DoWorkEventArgs(null);

                var result = helper.ProcessBuild(runData, bgWorker, e, allowableTimeoutRetries: 3, buildRequestedBy: string.Empty, scriptBatchColl: scriptBatchColl);

                Assert.AreEqual(BuildItemStatus.CommittedWithTimeoutRetries.ToString(), result.FinalStatus);
            }
            finally
            {
                SqlBuildHelper.SqlBuildRunnerFactory = originalFactory;
            }
        }

        private sealed class TestSqlBuildRunner : SqlBuildRunner
        {
            private readonly Queue<string> _statuses;
            public TestSqlBuildRunner(ISqlBuildRunnerContext ctx, Queue<string> statuses) : base(ctx) => _statuses = statuses;

            public override BuildModels.Build Run(
                System.Collections.Generic.IReadOnlyList<BuildModels.Script> scripts,
                BuildModels.Build myBuild,
                string serverName,
                bool isMultiDbRun,
                ScriptBatchCollection scriptBatchColl,
                BuildModels.SqlSyncBuildDataModel buildDataModel,
                ref DoWorkEventArgs workEventArgs)
            {
                var status = _statuses.Count > 0 ? _statuses.Dequeue() : BuildItemStatus.Committed.ToString();
                return myBuild with { FinalStatus = status };
            }
        }
    }
}
