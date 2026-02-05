using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Tests for build data model classes and enums
    /// </summary>
    [TestClass]
    public class BuildDataModelTests
    {
        #region BuildResultStatus Tests

        [TestMethod]
        public void BuildResultStatus_AllValuesAreDefined()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.UNKNOWN));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_COMMITTED));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_FAILED_NO_TRANSACTION));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.SCRIPT_GENERATION_COMPLETE));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildResultStatus), BuildResultStatus.BUILD_CANCELLED_NO_TRANSACTION));
        }

        [TestMethod]
        public void BuildResultStatus_ToStringReturnsExpectedValues()
        {
            Assert.AreEqual("UNKNOWN", BuildResultStatus.UNKNOWN.ToString());
            Assert.AreEqual("BUILD_COMMITTED", BuildResultStatus.BUILD_COMMITTED.ToString());
            Assert.AreEqual("BUILD_FAILED_AND_ROLLED_BACK", BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK.ToString());
        }

        #endregion

        #region BuildItemStatus Tests

        [TestMethod]
        public void BuildItemStatus_AllValuesAreDefined()
        {
            // Assert - check common values
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.Committed));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.RolledBack));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.FailedNoTransaction));
            Assert.IsTrue(Enum.IsDefined(typeof(BuildItemStatus), BuildItemStatus.TrialRolledBack));
        }

        #endregion

        #region Script Model Tests

        [TestMethod]
        public void Script_Constructor_SetsAllProperties()
        {
            // Arrange
            var dateAdded = DateTime.Now;
            var dateModified = dateAdded.AddDays(1);

            // Act
            var script = new Script(
                fileName: "test.sql",
                buildOrder: 5.0,
                description: "Test description",
                rollBackOnError: true,
                causesBuildFailure: true,
                dateAdded: dateAdded,
                scriptId: "script-123",
                database: "TestDb",
                stripTransactionText: true,
                allowMultipleRuns: false,
                addedBy: "TestUser",
                scriptTimeOut: 60,
                dateModified: dateModified,
                modifiedBy: "ModUser",
                tag: "v1.0");

            // Assert
            Assert.AreEqual("test.sql", script.FileName);
            Assert.AreEqual(5.0, script.BuildOrder);
            Assert.AreEqual("Test description", script.Description);
            Assert.IsTrue(script.RollBackOnError);
            Assert.IsTrue(script.CausesBuildFailure);
            Assert.AreEqual(dateAdded, script.DateAdded);
            Assert.AreEqual("script-123", script.ScriptId);
            Assert.AreEqual("TestDb", script.Database);
            Assert.IsTrue(script.StripTransactionText);
            Assert.IsFalse(script.AllowMultipleRuns);
            Assert.AreEqual("TestUser", script.AddedBy);
            Assert.AreEqual(60, script.ScriptTimeOut);
            Assert.AreEqual(dateModified, script.DateModified);
            Assert.AreEqual("ModUser", script.ModifiedBy);
            Assert.AreEqual("v1.0", script.Tag);
        }

        #endregion

        #region CommittedScript Model Tests

        [TestMethod]
        public void CommittedScript_Constructor_SetsAllProperties()
        {
            // Arrange
            var committedDate = DateTime.Now;

            // Act
            var script = new CommittedScript(
                scriptId: "script-456",
                serverName: "TestServer",
                committedDate: committedDate,
                allowScriptBlock: true,
                scriptHash: "HASH123",
                sqlSyncBuildProjectId: 42);

            // Assert
            Assert.AreEqual("script-456", script.ScriptId);
            Assert.AreEqual("TestServer", script.ServerName);
            Assert.AreEqual(committedDate, script.CommittedDate);
            Assert.IsTrue(script.AllowScriptBlock);
            Assert.AreEqual("HASH123", script.ScriptHash);
            Assert.AreEqual(42, script.SqlSyncBuildProjectId);
        }

        #endregion

        #region Build Model Tests

        [TestMethod]
        public void Build_Constructor_SetsAllProperties()
        {
            // Arrange
            var buildStart = DateTime.Now;
            var buildEnd = buildStart.AddMinutes(10);

            // Act
            var build = new Build(
                name: "Release Build",
                buildType: "Full",
                buildStart: buildStart,
                buildEnd: buildEnd,
                serverName: "BuildServer",
                finalStatus: BuildItemStatus.Committed,
                buildId: "BUILD-789",
                userId: "BuildUser");

            // Assert
            Assert.AreEqual("Release Build", build.Name);
            Assert.AreEqual("Full", build.BuildType);
            Assert.AreEqual(buildStart, build.BuildStart);
            Assert.AreEqual(buildEnd, build.BuildEnd);
            Assert.AreEqual("BuildServer", build.ServerName);
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
            Assert.AreEqual("BUILD-789", build.BuildId);
            Assert.AreEqual("BuildUser", build.UserId);
        }

        #endregion

        #region ScriptRun Model Tests

        [TestMethod]
        public void ScriptRun_Constructor_SetsAllProperties()
        {
            // Arrange
            var runStart = DateTime.Now;
            var runEnd = runStart.AddSeconds(30);

            // Act - use correct constructor signature
            var scriptRun = new ScriptRun(
                fileHash: "HASH123",
                results: "Script executed successfully",
                fileName: "run.sql",
                runOrder: 3.0,
                runStart: runStart,
                runEnd: runEnd,
                success: true,
                database: "RunDb",
                scriptRunId: "run-001",
                buildId: "build-001");

            // Assert
            Assert.AreEqual("run.sql", scriptRun.FileName);
            Assert.AreEqual(3.0, scriptRun.RunOrder);
            Assert.AreEqual(runStart, scriptRun.RunStart);
            Assert.AreEqual(runEnd, scriptRun.RunEnd);
            Assert.AreEqual(true, scriptRun.Success);
            Assert.AreEqual("Script executed successfully", scriptRun.Results);
            Assert.AreEqual("RunDb", scriptRun.Database);
            Assert.AreEqual("build-001", scriptRun.BuildId);
            Assert.AreEqual("HASH123", scriptRun.FileHash);
            Assert.AreEqual("run-001", scriptRun.ScriptRunId);
        }

        [TestMethod]
        public void ScriptRun_DefaultConstructor_CreatesInstance()
        {
            // Act
            var scriptRun = new ScriptRun();

            // Assert
            Assert.IsNotNull(scriptRun);
            Assert.IsNull(scriptRun.FileName);
        }

        #endregion

        #region SqlSyncBuildDataModel Tests

        [TestMethod]
        public void SqlSyncBuildDataModel_Constructor_SetsAllCollections()
        {
            // Arrange
            var projects = new List<SqlSyncBuildProject> { new SqlSyncBuildProject(1, "Project1", false) };
            var scripts = new List<Script>();
            var builds = new List<Build>();
            var scriptRuns = new List<ScriptRun>();
            var committedScripts = new List<CommittedScript>();

            // Act
            var model = new SqlSyncBuildDataModel(projects, scripts, builds, scriptRuns, committedScripts);

            // Assert
            Assert.IsNotNull(model.SqlSyncBuildProject);
            Assert.IsNotNull(model.Script);
            Assert.IsNotNull(model.Build);
            Assert.IsNotNull(model.ScriptRun);
            Assert.IsNotNull(model.CommittedScript);
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
        }

        #endregion

        #region SqlSyncBuildProject Tests

        [TestMethod]
        public void SqlSyncBuildProject_Constructor_SetsProperties()
        {
            // Act
            var project = new SqlSyncBuildProject(
                sqlSyncBuildProjectId: 123,
                projectName: "My Project",
                scriptTagRequired: true);

            // Assert
            Assert.AreEqual(123, project.SqlSyncBuildProjectId);
            Assert.AreEqual("My Project", project.ProjectName);
            Assert.IsTrue(project.ScriptTagRequired);
        }

        #endregion

        #region ScriptRunLogEntry Tests

        [TestMethod]
        public void ScriptRunLogEntry_Constructor_SetsAllProperties()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var commitDate = DateTime.Now;

            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: scriptId,
                ScriptFileHash: "HASH",
                CommitDate: commitDate,
                Sequence: 5,
                UserId: "user1",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "update1",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            // Assert
            Assert.AreEqual("build.sbm", entry.BuildFileName);
            Assert.AreEqual("script.sql", entry.ScriptFileName);
            Assert.AreEqual(scriptId, entry.ScriptId);
            Assert.AreEqual("HASH", entry.ScriptFileHash);
            Assert.AreEqual(commitDate, entry.CommitDate);
            Assert.AreEqual(5, entry.Sequence);
            Assert.AreEqual("user1", entry.UserId);
            Assert.AreEqual(true, entry.AllowScriptBlock);
            Assert.AreEqual("update1", entry.AllowBlockUpdateId);
            Assert.AreEqual("SELECT 1", entry.ScriptText);
            Assert.AreEqual("v1.0", entry.Tag);
        }

        [TestMethod]
        public void ScriptRunLogEntry_WithNullValues_HandlesGracefully()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: null,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.IsNull(entry.BuildFileName);
            Assert.IsNull(entry.ScriptFileName);
            Assert.IsNull(entry.ScriptId);
        }

        #endregion
    }

    [TestClass]
    public class ScriptTokensTests
    {
        [TestMethod]
        public void ScriptTokens_BuildDescription_IsNotNull()
        {
            // Assert
            Assert.IsNotNull(ScriptTokens.BuildDescription);
            Assert.IsFalse(string.IsNullOrEmpty(ScriptTokens.BuildDescription));
        }
    }

    [TestClass]
    public class XmlFileNamesTests
    {
        [TestMethod]
        public void XmlFileNames_MainProjectFile_HasCorrectValue()
        {
            // Assert
            Assert.IsNotNull(XmlFileNames.MainProjectFile);
            Assert.IsTrue(XmlFileNames.MainProjectFile.EndsWith(".xml"));
        }

        [TestMethod]
        public void XmlFileNames_HistoryFile_HasCorrectValue()
        {
            // Assert
            Assert.IsNotNull(XmlFileNames.HistoryFile);
        }

        [TestMethod]
        public void XmlFileNames_ExportFile_HasCorrectValue()
        {
            // Assert
            Assert.IsNotNull(XmlFileNames.ExportFile);
        }
    }
}
