using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class BuildExecutionStateTests
    {
        [TestMethod]
        public void Reset_ClearsAllStateToDefaults()
        {
            // Arrange
            var state = new BuildExecutionState
            {
                IsTransactional = false,
                IsTrialBuild = true,
                RunScriptOnly = true,
                BuildPackageHash = "abc123",
                BuildType = "TestType",
                BuildDescription = "Test description",
                StartIndex = 5,
                ProjectFileName = "test.sbx",
                ProjectFilePath = "C:\\temp",
                BuildFileName = "build.sbm",
                LogToDatabaseName = "LogDb",
                BuildRequestedBy = "testuser",
                RunItemIndexes = new double[] { 1, 2, 3 },
                ErrorOccurred = true,
                SqlInfoMessage = "Some message",
                CurrentBuildId = Guid.NewGuid()
            };
            state.CommittedScripts.Add(new SqlLogging.CommittedScript(Guid.NewGuid(), "hash", 1, "SELECT 1", "tag", "srv", "db"));

            // Act
            state.Reset();

            // Assert
            Assert.IsTrue(state.IsTransactional);
            Assert.IsFalse(state.IsTrialBuild);
            Assert.IsFalse(state.RunScriptOnly);
            Assert.AreEqual(string.Empty, state.BuildPackageHash);
            Assert.AreEqual(string.Empty, state.BuildType);
            Assert.AreEqual(string.Empty, state.BuildDescription);
            Assert.AreEqual(0, state.StartIndex);
            Assert.AreEqual(string.Empty, state.ProjectFileName);
            Assert.AreEqual(string.Empty, state.ProjectFilePath);
            Assert.AreEqual(string.Empty, state.BuildFileName);
            Assert.AreEqual(string.Empty, state.LogToDatabaseName);
            Assert.AreEqual(string.Empty, state.BuildRequestedBy);
            Assert.AreEqual(0, state.RunItemIndexes.Length);
            Assert.IsFalse(state.ErrorOccurred);
            Assert.AreEqual(string.Empty, state.SqlInfoMessage);
            Assert.AreEqual(Guid.Empty, state.CurrentBuildId);
            Assert.AreEqual(0, state.CommittedScripts.Count);
        }

        [TestMethod]
        public void PopulateFromRunData_SetsPropertiesFromRunData()
        {
            // Arrange
            var state = new BuildExecutionState();
            var runData = new SqlBuildRunDataModel(
                buildDataModel: SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(),
                buildType: "MultiBuild",
                server: "TestServer",
                buildDescription: "Test Build",
                startIndex: 10,
                projectFileName: "C:\\test\\project.sbx",
                isTrial: true,
                runItemIndexes: new double[] { 1, 2, 3 },
                runScriptOnly: true,
                buildFileName: "C:\\test\\build.sbm",
                logToDatabaseName: "LogDatabase",
                isTransactional: false,
                platinumDacPacFileName: null,
                targetDatabaseOverrides: null,
                forceCustomDacpac: false,
                buildRevision: "1.0",
                defaultScriptTimeout: 60,
                allowObjectDelete: true);

            // Act
            state.PopulateFromRunData(runData);

            // Assert
            Assert.AreEqual("MultiBuild", state.BuildType);
            Assert.AreEqual("Test Build", state.BuildDescription);
            Assert.AreEqual(10, state.StartIndex);
            Assert.AreEqual("C:\\test\\project.sbx", state.ProjectFileName);
            Assert.AreEqual("C:\\test", state.ProjectFilePath);
            Assert.IsTrue(state.IsTrialBuild);
            Assert.AreEqual(3, state.RunItemIndexes.Length);
            Assert.IsTrue(state.RunScriptOnly);
            Assert.AreEqual("build.sbm", state.BuildFileName);
            Assert.AreEqual("LogDatabase", state.LogToDatabaseName);
            Assert.IsFalse(state.IsTransactional);
        }

        [TestMethod]
        public void PopulateFromRunData_HandlesNullRunData()
        {
            // Arrange
            var state = new BuildExecutionState
            {
                BuildDescription = "Original"
            };

            // Act
            state.PopulateFromRunData(null);

            // Assert - should not throw and should not change state
            Assert.AreEqual("Original", state.BuildDescription);
        }
    }
}
