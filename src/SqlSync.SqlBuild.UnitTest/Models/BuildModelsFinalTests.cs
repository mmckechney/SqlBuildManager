using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    /// <summary>
    /// Final coverage tests for Build models
    /// </summary>
    [TestClass]
    public class BuildModelsFinalTests
    {
        #region Build Tests

        [TestMethod]
        public void Build_Constructor_SetsAllProperties()
        {
            // Arrange
            var start = new DateTime(2023, 6, 15, 10, 0, 0);
            var end = new DateTime(2023, 6, 15, 10, 30, 0);

            // Act
            var build = new Build(
                name: "TestBuild",
                buildType: "FullBuild",
                buildStart: start,
                buildEnd: end,
                serverName: "TestServer",
                finalStatus: BuildItemStatus.Committed,
                buildId: "BUILD-001",
                userId: "testuser");

            // Assert
            Assert.AreEqual("TestBuild", build.Name);
            Assert.AreEqual("FullBuild", build.BuildType);
            Assert.AreEqual(start, build.BuildStart);
            Assert.AreEqual(end, build.BuildEnd);
            Assert.AreEqual("TestServer", build.ServerName);
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
            Assert.AreEqual("BUILD-001", build.BuildId);
            Assert.AreEqual("testuser", build.UserId);
        }

        [TestMethod]
        public void Build_FinalStatus_CanBeModified()
        {
            // Arrange
            var build = new Build("Test", null, null, null, null, BuildItemStatus.Pending, null, null);

            // Act
            build.FinalStatus = BuildItemStatus.Committed;

            // Assert
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
        }

        [TestMethod]
        public void Build_WithNullValues_StoresNulls()
        {
            // Act
            var build = new Build(null, null, null, null, null, null, null, null);

            // Assert
            Assert.IsNull(build.Name);
            Assert.IsNull(build.BuildType);
            Assert.IsNull(build.BuildStart);
            Assert.IsNull(build.BuildEnd);
            Assert.IsNull(build.ServerName);
            Assert.IsNull(build.FinalStatus);
            Assert.IsNull(build.BuildId);
            Assert.IsNull(build.UserId);
        }

        #endregion

        #region BuildItemStatus Tests

        [TestMethod]
        public void BuildItemStatus_AllValuesAreDefined()
        {
            // Assert all expected values exist
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.Pending));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.Committed));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.RolledBack));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.TrialRolledBack));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.FailedNoTransaction));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.PendingRollBack));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.FailedDueToScriptTimeout));
        }

        #endregion

        #region BuildResultStatus Tests

        [TestMethod]
        public void BuildResultStatus_AllValuesAreDefined()
        {
            // Assert all expected values exist
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.UNKNOWN));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_COMMITTED));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_FAILED_NO_TRANSACTION));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.SCRIPT_GENERATION_COMPLETE));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK));
        }

        #endregion

        #region Script Tests

        [TestMethod]
        public void Script_Constructor_SetsAllProperties()
        {
            // Arrange
            var addedDate = new DateTime(2023, 6, 10);
            var modifiedDate = new DateTime(2023, 6, 15);

            // Act
            var script = new Script(
                fileName: "test.sql",
                buildOrder: 1.5,
                description: "Test script",
                rollBackOnError: true,
                causesBuildFailure: true,
                dateAdded: addedDate,
                scriptId: "SCRIPT-001",
                database: "TestDb",
                stripTransactionText: false,
                allowMultipleRuns: true,
                addedBy: "testuser",
                scriptTimeOut: 60,
                dateModified: modifiedDate,
                modifiedBy: null,
                tag: "v1.0");

            // Assert
            Assert.AreEqual("SCRIPT-001", script.ScriptId);
            Assert.AreEqual("test.sql", script.FileName);
            Assert.AreEqual(1.5, script.BuildOrder);
            Assert.AreEqual("Test script", script.Description);
            Assert.IsTrue(script.RollBackOnError);
            Assert.IsTrue(script.CausesBuildFailure);
            Assert.AreEqual("TestDb", script.Database);
            Assert.IsFalse(script.StripTransactionText);
            Assert.IsTrue(script.AllowMultipleRuns);
            Assert.AreEqual("testuser", script.AddedBy);
            Assert.AreEqual(60, script.ScriptTimeOut);
            Assert.AreEqual("v1.0", script.Tag);
        }

        [TestMethod]
        public void Script_WithNullValues_StoresNulls()
        {
            // Act - use default constructor
            var script = new Script();

            // Assert
            Assert.IsNull(script.ScriptId);
            Assert.IsNull(script.FileName);
        }

        #endregion

        #region ScriptRun Tests

        [TestMethod]
        public void ScriptRun_Constructor_SetsAllProperties()
        {
            // Arrange
            var start = DateTime.Now;
            var end = start.AddSeconds(30);

            // Act
            var run = new ScriptRun(
                fileHash: "ABC123",
                results: "Success",
                fileName: "test.sql",
                runOrder: 1,
                runStart: start,
                runEnd: end,
                success: true,
                database: "TestDb",
                scriptRunId: "RUN-001",
                buildId: "BUILD-001");

            // Assert
            Assert.AreEqual("ABC123", run.FileHash);
            Assert.AreEqual("Success", run.Results);
            Assert.AreEqual("test.sql", run.FileName);
            Assert.AreEqual(1, run.RunOrder);
            Assert.AreEqual(start, run.RunStart);
            Assert.AreEqual(end, run.RunEnd);
            Assert.IsTrue(run.Success);
            Assert.AreEqual("TestDb", run.Database);
            Assert.AreEqual("RUN-001", run.ScriptRunId);
            Assert.AreEqual("BUILD-001", run.BuildId);
        }

        [TestMethod]
        public void ScriptRun_FileHash_CanBeModified()
        {
            // Arrange
            var run = new ScriptRun(null, null, null, null, null, null, null, null, null, null);

            // Act
            run.FileHash = "NEWHASH";

            // Assert
            Assert.AreEqual("NEWHASH", run.FileHash);
        }

        [TestMethod]
        public void ScriptRun_Success_CanBeModified()
        {
            // Arrange
            var run = new ScriptRun(null, null, null, null, null, null, null, null, null, null);

            // Act
            run.Success = false;

            // Assert
            Assert.IsFalse(run.Success);
        }

        [TestMethod]
        public void ScriptRun_Results_CanBeModified()
        {
            // Arrange
            var run = new ScriptRun(null, null, null, null, null, null, null, null, null, null);

            // Act
            run.Results = "Error occurred";

            // Assert
            Assert.AreEqual("Error occurred", run.Results);
        }

        [TestMethod]
        public void ScriptRun_RunEnd_CanBeModified()
        {
            // Arrange
            var run = new ScriptRun(null, null, null, null, null, null, null, null, null, null);
            var end = DateTime.Now;

            // Act
            run.RunEnd = end;

            // Assert
            Assert.AreEqual(end, run.RunEnd);
        }

        #endregion

        #region CommittedScript Tests

        [TestMethod]
        public void CommittedScript_Constructor_SetsAllProperties()
        {
            // Arrange
            var commitDate = new DateTime(2023, 6, 15);

            // Act
            var script = new CommittedScript(
                scriptId: "SCRIPT-001",
                serverName: "TestServer",
                committedDate: commitDate,
                allowScriptBlock: true,
                scriptHash: "HASH123",
                sqlSyncBuildProjectId: 42);

            // Assert
            Assert.AreEqual("SCRIPT-001", script.ScriptId);
            Assert.AreEqual("TestServer", script.ServerName);
            Assert.AreEqual(commitDate, script.CommittedDate);
            Assert.IsTrue(script.AllowScriptBlock);
            Assert.AreEqual("HASH123", script.ScriptHash);
            Assert.AreEqual(42, script.SqlSyncBuildProjectId);
        }

        #endregion

        #region SqlSyncBuildProject Tests

        [TestMethod]
        public void SqlSyncBuildProject_Constructor_SetsAllProperties()
        {
            // Act
            var project = new SqlSyncBuildProject(
                sqlSyncBuildProjectId: 123,
                projectName: "TestProject",
                scriptTagRequired: true);

            // Assert
            Assert.AreEqual(123, project.SqlSyncBuildProjectId);
            Assert.AreEqual("TestProject", project.ProjectName);
            Assert.IsTrue(project.ScriptTagRequired);
        }

        #endregion

        #region SqlSyncBuildDataModel Tests

        [TestMethod]
        public void SqlSyncBuildDataModel_Constructor_SetsAllCollections()
        {
            // Arrange
            var projects = new List<SqlSyncBuildProject> { new SqlSyncBuildProject(1, "Test", false) };
            var scripts = new List<Script> { new Script() { FileName = "file.sql", ScriptId = "id" } };
            var builds = new List<Build> { new Build("Build1", null, null, null, null, null, null, null) };
            var runs = new List<ScriptRun> { new ScriptRun(null, null, null, null, null, null, null, null, null, null) };
            var committed = new List<CommittedScript> { new CommittedScript("id", null, null, null, null, null) };

            // Act
            var model = new SqlSyncBuildDataModel(projects, scripts, builds, runs, committed);

            // Assert
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(1, model.Script.Count);
            Assert.AreEqual(1, model.Build.Count);
            Assert.AreEqual(1, model.ScriptRun.Count);
            Assert.AreEqual(1, model.CommittedScript.Count);
        }

        #endregion
    }

    /// <summary>
    /// Tests for Rebuilder and related data classes
    /// </summary>
    [TestClass]
    public class RebuilderDataFinalTests
    {
        [TestMethod]
        public void RebuilderData_CanSetAllProperties()
        {
            // Act
            var data = new RebuilderData
            {
                ScriptFileName = "test.sql",
                ScriptId = Guid.NewGuid(),
                Sequence = 1,
                ScriptText = "SELECT 1",
                Database = "TestDb",
                Tag = "v1.0"
            };

            // Assert
            Assert.AreEqual("test.sql", data.ScriptFileName);
            Assert.AreEqual(1, data.Sequence);
            Assert.AreEqual("SELECT 1", data.ScriptText);
            Assert.AreEqual("TestDb", data.Database);
            Assert.AreEqual("v1.0", data.Tag);
        }

        [TestMethod]
        public void CommittedBuildData_CanSetAllProperties()
        {
            // Arrange
            var commitDate = new DateTime(2023, 6, 15);

            // Act
            var data = new CommittedBuildData
            {
                BuildFileName = "build.sbm",
                ScriptCount = 10,
                CommitDate = commitDate,
                Database = "TestDb"
            };

            // Assert
            Assert.AreEqual("build.sbm", data.BuildFileName);
            Assert.AreEqual(10, data.ScriptCount);
            Assert.AreEqual(commitDate, data.CommitDate);
            Assert.AreEqual("TestDb", data.Database);
        }
    }
}
