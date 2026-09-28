using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild.IntegrationTest.TestBase;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.Services;
using SqlBuildManager.SqlBuild.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SqlBuildManager.SqlBuild.SqlServer.IntegrationTest
{
    [TestClass]
    public class PackageFixtureTests
    {
        [TestMethod]
        public async Task Fixture_UsesRelativeMembersAndOneProjectDirectory()
        {
            using var fixture = new OfflineInitialization();
            var model = fixture.CreateSqlSyncSqlBuildDataModelObject();
            fixture.AddInsertScript(ref model, true);
            fixture.AddSelectScript(ref model);
            fixture.AddFailureScript(ref model, true, true);
            fixture.AddScriptWithBadDatabase(ref model);
            fixture.AddBatchInsertScripts(ref model, true);
            var helper = fixture.CreateSqlBuildHelper(model);

            Assert.AreEqual(fixture.ProjectDirectory, ((ISqlBuildRunnerProperties)helper).ProjectFilePath);
            Assert.AreEqual(fixture.ProjectDirectory, Path.GetDirectoryName(fixture.projectFileName));
            foreach (var script in model.Script)
            {
                Assert.IsNotNull(script.FileName);
                Assert.AreEqual(Path.GetFileName(script.FileName), script.FileName);
                Assert.IsTrue(File.Exists(PackagePath.Resolve(fixture.ProjectDirectory, script.FileName)));
            }

            var batcher = new DefaultScriptBatcher();
            var batches = batcher.LoadAndBatchSqlScripts(model, fixture.ProjectDirectory);
            Assert.AreEqual(5, batches.Count);
            var batchedScriptId = model.Script[4].ScriptId;
            Assert.IsNotNull(batchedScriptId);
            Assert.AreEqual(2, batches.GetScriptBatch(batchedScriptId).ScriptBatchContents.Length);
            Assert.ThrowsExactly<InvalidDataException>(() => batcher.LoadAndBatchSqlScripts(model, string.Empty));
            var hash = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(fixture.ProjectDirectory, model);
            Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        }

        [TestMethod]
        public void Fixture_CleanupDoesNotTouchAnotherFixture()
        {
            using var first = new OfflineInitialization();
            using var second = new OfflineInitialization();
            var ownFile = first.GetTrulyUniqueFile();
            var otherFile = second.GetTrulyUniqueFile();
            Assert.AreNotEqual(first.ProjectDirectory, second.ProjectDirectory);
            first.Dispose();
            Assert.IsFalse(Directory.Exists(first.ProjectDirectory));
            Assert.IsFalse(File.Exists(ownFile));
            Assert.IsTrue(File.Exists(otherFile));
        }

        private sealed class OfflineInitialization : InitializationBase
        {
            protected override string TestTableName => "TransactionTest";
            public OfflineInitialization()
            {
                serverName = "localhost";
                testDatabaseNames = new List<string> { "offline" };
                connData = new ConnectionData(serverName, "offline");
            }

            public override bool CreateDatabases() => throw new System.NotSupportedException();
            public override bool CreateTestTables() => throw new System.NotSupportedException();
            public override bool CreateSqlBuildLoggingTables() => throw new System.NotSupportedException();
            public override bool CleanTestTables() => throw new System.NotSupportedException();
            public override bool CleanSqlBuildLoggingTables() => throw new System.NotSupportedException();
            public override int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex) => throw new System.NotSupportedException();
            public override int GetTestTableRowCount(int databaseIndex) => throw new System.NotSupportedException();
            public override ConnectionData CreateConnectionData(string databaseName) => throw new System.NotSupportedException();
        }
    }
}
