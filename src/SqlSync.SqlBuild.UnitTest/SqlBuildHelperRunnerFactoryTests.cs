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
        public async Task RunBuildScripts_UsesInjectedRunnerFactory()
        {
            // Arrange
            var expectedBuild = new BuildModels.Build(
                name: "test",
                buildType: "type",
                buildStart: DateTime.Now,
                buildEnd: null,
                serverName: "srv",
                finalStatus: BuildItemStatus.Committed,
                buildId: Guid.NewGuid().ToString(),
                userId: "user");

            var fakeFactory = new FakeRunnerFactory(expectedBuild);
            
            // Use internal constructor to inject the fake factory
            var helper = new SqlBuildHelper(
                data: new ConnectionData(),
                createScriptRunLogFile: false,
                externalScriptLogFileName: "",
                isTransactional: true,
                clock: null,
                guidProvider: null,
                fileSystem: null,
                progressReporter: null,
                fileHelper: null,
                retryPolicy: null,
                databaseUtility: null,
                connectionsService: null,
                buildFinalizer: null,
                runnerFactory: fakeFactory);

            var scripts = new List<BuildModels.Script>();
            var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = await helper.RunBuildScriptsAsync(scripts, expectedBuild, "srv", false, null, buildData);

            // Assert
            Assert.AreEqual(expectedBuild.BuildId, result.BuildId);
            Assert.AreEqual(BuildItemStatus.Committed, result.FinalStatus);
            Assert.IsTrue(fakeFactory.CreateCalled, "Factory.Create should have been called");
        }

        private sealed class FakeRunnerFactory : IRunnerFactory
        {
            private readonly BuildModels.Build _result;
            public bool CreateCalled { get; private set; }

            public FakeRunnerFactory(BuildModels.Build result) => _result = result;

            public SqlBuildRunner Create(IConnectionsService connectionsService, ISqlBuildRunnerContext context, IBuildFinalizerContext finalizerContext, ISqlCommandExecutor executor = null)
            {
                CreateCalled = true;
                return new FakeRunner(_result);
            }
        }

        private sealed class FakeRunner : SqlBuildRunner
        {
            private readonly BuildModels.Build _result;
            public FakeRunner(BuildModels.Build result) : base(MockFactory.CreateMockConnectionsService().Object, MockFactory.CreateMockRunnerContext().Object, new Mock<IBuildFinalizerContext>().Object)
            {
                _result = result;
            }

            public override Task<BuildModels.Build> RunAsync(
                IList<BuildModels.Script> scripts,
                BuildModels.Build myBuild,
                string serverName,
                bool isMultiDbRun,
                ScriptBatchCollection scriptBatchColl,
                BuildModels.SqlSyncBuildDataModel buildDataModel,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_result);
            }
        }

        
    }
}
