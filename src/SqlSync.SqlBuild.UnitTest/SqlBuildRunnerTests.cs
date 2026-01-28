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
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.Connection;
using Moq;
using SqlSync.SqlBuild.Services;
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
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object,  ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = model with
            {
                CommittedScript = new List<BuildModels.CommittedScript>
                {
                    new BuildModels.CommittedScript(ScriptId, ServerName: null, CommittedDate: null, AllowScriptBlock: null, ScriptHash: null, SqlSyncBuildProjectId: null)
                }
            };

            var result = runner.ShouldSkipDueToCommittedScripts(ScriptId, model);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LoadBatchScripts_PrefersPreBatchedScripts()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("file.sql", new[] { "SELECT 1;" }, ScriptId));

            var result = runner.LoadBatchScripts(ScriptId, "file.sql", stripTransaction: false, scriptBatchColl: coll);

            CollectionAssert.AreEqual(new[] { "SELECT 1;" }, result);
        }

        [TestMethod]
        public void LoadBatchScripts_ReadsViaContext_WhenNoPreBatch()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "SELECT 2;" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            var result = runner.LoadBatchScripts(ScriptId, "file.sql", stripTransaction: false, scriptBatchColl: null);

            CollectionAssert.AreEqual(new[] { "SELECT 2;" }, result);
        }
    }
}
