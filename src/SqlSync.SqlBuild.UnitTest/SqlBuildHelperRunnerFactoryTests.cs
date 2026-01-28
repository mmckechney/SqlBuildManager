using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;
using SqlLogging = SqlSync.SqlBuild.SqlLogging;

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
                FinalStatus: BuildItemStatus.Committed,
                BuildId: Guid.NewGuid().ToString(),
                UserId: "user");

            SqlBuildHelper.SqlBuildRunnerFactory = (connSvc, ctx, finalizerContext, exec) => new FakeRunner(expectedBuild);

            var scripts = new List<BuildModels.Script>();
            var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var dwea = new DoWorkEventArgs(null);

            // Act
            var result = helper.RunBuildScripts(scripts, expectedBuild, "srv", false, null, buildData, ref dwea);

            // Assert
            Assert.AreEqual(expectedBuild.BuildId, result.BuildId);
            Assert.AreEqual(BuildItemStatus.Committed, result.FinalStatus);
        }

        private sealed class FakeRunner : SqlBuildRunner
        {
            private readonly BuildModels.Build _result;
            public FakeRunner(BuildModels.Build result) : base(MockFactory.CreateMockConnectionsService().Object, MockFactory.CreateMockRunnerContext().Object, new Mock<IBuildFinalizerContext>().Object)
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

        
    }
}
