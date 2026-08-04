using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using SqlBuildManager.SqlBuild;
using Microsoft.Extensions.Logging.Abstractions;
using BuildModels = SqlBuildManager.SqlBuild.Models;
using LoggingCommittedScript = SqlBuildManager.SqlBuild.SqlLogging.CommittedScript;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.MultiDb;
using SqlBuildManager.Connection;
using Moq;
using SqlBuildManager.SqlBuild.Services;
namespace SqlBuildManager.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerTests
    {
        private const string ScriptId = "abc";

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsTrue_WhenCommitted()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object,  ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = new BuildModels.SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: new List<BuildModels.CommittedScript>
                {
                    new BuildModels.CommittedScript(ScriptId, serverName: null, committedDate: null, allowScriptBlock: null, scriptHash: null, sqlSyncBuildProjectId: null)
                });

            var result = runner.ShouldSkipDueToCommittedScripts(ScriptId, model);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task LoadBatchScriptsAsync_PrefersPreBatchedScripts()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("file.sql", new[] { "SELECT 1;" }, ScriptId));

            var result = await runner.LoadBatchScriptsAsync(ScriptId, "file.sql", stripTransaction: false, scriptBatchColl: coll, default);

            CollectionAssert.AreEqual(new[] { "SELECT 1;" }, result);
        }

        [TestMethod]
        public async Task LoadBatchScriptsAsync_ReadsViaContext_WhenNoPreBatch()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "SELECT 2;" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            var result = await runner.LoadBatchScriptsAsync(ScriptId, "file.sql", stripTransaction: false, scriptBatchColl: null!, default);

            CollectionAssert.AreEqual(new[] { "SELECT 2;" }, result);
        }
    }
}
