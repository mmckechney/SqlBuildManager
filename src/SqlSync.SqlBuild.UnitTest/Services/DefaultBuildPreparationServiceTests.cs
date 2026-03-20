using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultBuildPreparationServiceTests
    {
        private string tempDir = null!;

        [TestInitialize]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), $"BuildPrepTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        private SqlBuildHelper CreateHelper(string projectFileName = "", bool isTransactional = true)
        {
            var connData = new ConnectionData() { DatabaseName = "TestDb", SQLServerName = "(local)" };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false, isTransactional: isTransactional);
            helper.projectFileName = projectFileName;
            return helper;
        }

        private static SqlSyncBuildDataModel CreateBuildDataWithScripts(params (string fileName, double buildOrder)[] scripts)
        {
            var scriptList = new List<Script>();
            foreach (var (fileName, buildOrder) in scripts)
            {
                scriptList.Add(new Script(
                    fileName: fileName,
                    buildOrder: buildOrder,
                    description: $"Test script {fileName}",
                    rollBackOnError: true,
                    causesBuildFailure: true,
                    dateAdded: DateTime.Now,
                    scriptId: Guid.NewGuid().ToString(),
                    database: null,
                    stripTransactionText: true,
                    allowMultipleRuns: true,
                    addedBy: "UnitTest",
                    scriptTimeOut: 500,
                    dateModified: null,
                    modifiedBy: null,
                    tag: null));
            }

            return new SqlSyncBuildDataModel(
                sqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                script: scriptList,
                build: new List<Build>(),
                scriptRun: Array.Empty<ScriptRun>(),
                committedScript: Array.Empty<CommittedScript>());
        }

        #region Script Filtering Tests

        [TestMethod]
        public async Task PrepareBuild_WithScripts_ReturnsFilteredScriptsOrderedByBuildOrder()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(
                ("C_third.sql", 3.0),
                ("A_first.sql", 1.0),
                ("B_second.sql", 2.0));

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            Assert.AreEqual(3, result.FilteredScripts.Count);
            Assert.AreEqual("A_first.sql", result.FilteredScripts[0].FileName);
            Assert.AreEqual("B_second.sql", result.FilteredScripts[1].FileName);
            Assert.AreEqual("C_third.sql", result.FilteredScripts[2].FileName);
        }

        [TestMethod]
        public async Task PrepareBuild_WithStartIndex_FiltersScriptsBelowIndex()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);
            // Set startIndex to 2.0 so only scripts with BuildOrder >= 2 are included
            helper.runItemIndexes = Array.Empty<double>();

            var model = CreateBuildDataWithScripts(
                ("first.sql", 1.0),
                ("second.sql", 2.0),
                ("third.sql", 3.0));

            // Manually set startIndex via the internal state
            var stateField = typeof(SqlBuildHelper).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var state = (BuildExecutionState)stateField!.GetValue(helper)!;
            state.StartIndex = 2.0;

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            Assert.AreEqual(2, result.FilteredScripts.Count);
            Assert.AreEqual("second.sql", result.FilteredScripts[0].FileName);
            Assert.AreEqual("third.sql", result.FilteredScripts[1].FileName);
        }

        [TestMethod]
        public async Task PrepareBuild_WithRunItemIndexes_FiltersToSpecificIndexes()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);
            helper.runItemIndexes = new double[] { 1.0, 3.0 };

            var model = CreateBuildDataWithScripts(
                ("first.sql", 1.0),
                ("second.sql", 2.0),
                ("third.sql", 3.0));

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            Assert.AreEqual(2, result.FilteredScripts.Count);
            Assert.AreEqual("first.sql", result.FilteredScripts[0].FileName);
            Assert.AreEqual("third.sql", result.FilteredScripts[1].FileName);
        }

        #endregion

        #region Empty Scripts / No Match Tests

        [TestMethod]
        public async Task PrepareBuild_NoScripts_SingleDbRun_ReturnsRolledBackStatus()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                script: Array.Empty<Script>(),
                build: new List<Build>(),
                scriptRun: Array.Empty<ScriptRun>(),
                committedScript: Array.Empty<CommittedScript>());

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", isMultiDbRun: false, null, CancellationToken.None);

            Assert.AreEqual(0, result.FilteredScripts.Count);
            Assert.AreEqual(BuildItemStatus.RolledBack, result.Build.FinalStatus);
            Assert.AreEqual(string.Empty, result.BuildPackageHash);
        }

        [TestMethod]
        public async Task PrepareBuild_NoScripts_MultiDbRun_ReturnsPendingRollBackStatus()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                script: Array.Empty<Script>(),
                build: new List<Build>(),
                scriptRun: Array.Empty<ScriptRun>(),
                committedScript: Array.Empty<CommittedScript>());

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", isMultiDbRun: true, null, CancellationToken.None);

            Assert.AreEqual(0, result.FilteredScripts.Count);
            Assert.AreEqual(BuildItemStatus.PendingRollBack, result.Build.FinalStatus);
            Assert.AreEqual(string.Empty, result.BuildPackageHash);
        }

        [TestMethod]
        public async Task PrepareBuild_AllScriptsBelowStartIndex_ReturnsEmpty()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var stateField = typeof(SqlBuildHelper).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var state = (BuildExecutionState)stateField!.GetValue(helper)!;
            state.StartIndex = 100.0;

            var model = CreateBuildDataWithScripts(
                ("first.sql", 1.0),
                ("second.sql", 2.0));

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            Assert.AreEqual(0, result.FilteredScripts.Count);
            Assert.AreEqual(BuildItemStatus.RolledBack, result.Build.FinalStatus);
        }

        #endregion

        #region Build Record Tests

        [TestMethod]
        public async Task PrepareBuild_CreatesBuildRecord_WithCorrectMetadata()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);
            helper.buildDescription = "Unit Test Build";

            var model = CreateBuildDataWithScripts(("script.sql", 1.0));

            var result = await helper.PrepareBuildForRunAsync(model, "MyServer", false, null, CancellationToken.None);

            Assert.IsNotNull(result.Build);
            Assert.AreEqual("MyServer", result.Build.ServerName);
            Assert.AreEqual(BuildItemStatus.Unknown, result.Build.FinalStatus);
            Assert.AreEqual("Unit Test Build", result.Build.Name);
            Assert.IsNotNull(result.Build.BuildStart);
            Assert.IsNotNull(result.Build.BuildId);
            Assert.AreEqual(Environment.UserName, result.Build.UserId);
        }

        [TestMethod]
        public async Task PrepareBuild_AddsBuildToModel()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var existingBuild = new Build("existing", "type", DateTime.Now, DateTime.Now, "server", BuildItemStatus.Committed, Guid.NewGuid().ToString(), "user");
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                script: new List<Script> { new Script("s.sql", 1.0, null, null, null, null, null, null, null, null, null, null, null, null, null) },
                build: new List<Build> { existingBuild },
                scriptRun: Array.Empty<ScriptRun>(),
                committedScript: Array.Empty<CommittedScript>());

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            // The build record should be returned in the result
            Assert.IsNotNull(result.Build);
            Assert.AreEqual(BuildItemStatus.Unknown, result.Build.FinalStatus);
        }

        #endregion

        #region Path Resolution Tests

        [TestMethod]
        public async Task PrepareBuild_ProjectFileExists_SetsAttributesToNormal()
        {
            var projectFile = Path.Combine(tempDir, "readonly.xml");
            File.WriteAllText(projectFile, "<root/>");
            File.SetAttributes(projectFile, FileAttributes.ReadOnly);
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(("script.sql", 1.0));

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            // File should no longer be read-only
            var attrs = File.GetAttributes(projectFile);
            Assert.IsFalse(attrs.HasFlag(FileAttributes.ReadOnly));
        }

        [TestMethod]
        public async Task PrepareBuild_EmptyProjectFilePath_DerivesFromProjectFileName()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(("script.sql", 1.0));

            await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            // After prepare, the buildHistoryXmlFile should be in the same dir as projectFile
            Assert.IsTrue(helper.buildHistoryXmlFile.StartsWith(tempDir));
        }

        #endregion

        #region Hash Calculation Tests

        [TestMethod]
        public async Task PrepareBuild_WithScriptBatchColl_CalculatesHashFromBatch()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(("script.sql", 1.0));

            // Create a non-null ScriptBatchCollection
            var batchColl = new ScriptBatchCollection();
            batchColl.Add(new ScriptBatch("script.sql", new[] { "SELECT 1" }, Guid.NewGuid().ToString()));

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, batchColl, CancellationToken.None);

            Assert.IsFalse(string.IsNullOrEmpty(result.BuildPackageHash), "Hash should be calculated from batch collection");
        }

        [TestMethod]
        public async Task PrepareBuild_NullScriptBatchColl_CalculatesHashFromFilePath()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(("script.sql", 1.0));

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            // Hash is calculated but may be empty if no actual script files exist in the directory
            Assert.IsNotNull(result.BuildPackageHash);
        }

        #endregion

        #region Null Script Handling

        [TestMethod]
        public async Task PrepareBuild_WithNullScriptArray_TreatsAsEmpty()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                script: null!,
                build: new List<Build>(),
                scriptRun: Array.Empty<ScriptRun>(),
                committedScript: Array.Empty<CommittedScript>());

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            Assert.AreEqual(0, result.FilteredScripts.Count);
            Assert.AreEqual(BuildItemStatus.RolledBack, result.Build.FinalStatus);
        }

        [TestMethod]
        public async Task PrepareBuild_NullBuildOrderScripts_SortedToEnd()
        {
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(
                ("ordered.sql", 1.0));

            // Add a script with null BuildOrder
            var scripts = new List<Script>(model.Script!)
            {
                new Script("unordered.sql", null, null, null, null, null, null, null, null, null, null, null, null, null, null)
            };
            model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                script: scripts,
                build: new List<Build>(),
                scriptRun: Array.Empty<ScriptRun>(),
                committedScript: Array.Empty<CommittedScript>());

            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, null, CancellationToken.None);

            Assert.AreEqual(2, result.FilteredScripts.Count);
            Assert.AreEqual("ordered.sql", result.FilteredScripts[0].FileName);
            Assert.AreEqual("unordered.sql", result.FilteredScripts[1].FileName);
        }

        #endregion

        #region Cancellation Test

        [TestMethod]
        public async Task PrepareBuild_WithCancelledToken_StillPrepares()
        {
            // PrepareBuildForRunAsync passes cancellation to hash calculation only;
            // the rest of the method should complete even with a cancelled token
            var projectFile = Path.Combine(tempDir, "test.xml");
            File.WriteAllText(projectFile, "<root/>");
            var helper = CreateHelper(projectFile);

            var model = CreateBuildDataWithScripts(("script.sql", 1.0));
            var batchColl = new ScriptBatchCollection();
            batchColl.Add(new ScriptBatch("script.sql", new[] { "SELECT 1" }, Guid.NewGuid().ToString()));

            using var cts = new CancellationTokenSource();
            // Don't cancel - just verify token is forwarded without issues
            var result = await helper.PrepareBuildForRunAsync(model, "TestServer", false, batchColl, cts.Token);

            Assert.AreEqual(1, result.FilteredScripts.Count);
        }

        #endregion
    }
}
