using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class SqlSyncBuildDataModelsTests
    {
        #region SqlSyncBuildProject Tests

        [TestMethod]
        public void SqlSyncBuildProject_Constructor_SetsAllProperties()
        {
            // Act
            var project = new SqlSyncBuildProject(1, "TestProject", true);

            // Assert
            Assert.AreEqual(1, project.SqlSyncBuildProjectId);
            Assert.AreEqual("TestProject", project.ProjectName);
            Assert.IsTrue(project.ScriptTagRequired.Value);
        }

        [TestMethod]
        public void SqlSyncBuildProject_Constructor_WithNulls_SetsNullValues()
        {
            // Act
            var project = new SqlSyncBuildProject(0, null, null);

            // Assert
            Assert.AreEqual(0, project.SqlSyncBuildProjectId);
            Assert.IsNull(project.ProjectName);
            Assert.IsNull(project.ScriptTagRequired);
        }

        [TestMethod]
        public void SqlSyncBuildProject_Properties_CanBeModified()
        {
            // Arrange
            var project = new SqlSyncBuildProject(1, "Original", false);

            // Act
            project.SqlSyncBuildProjectId = 2;
            project.ProjectName = "Modified";
            project.ScriptTagRequired = true;

            // Assert
            Assert.AreEqual(2, project.SqlSyncBuildProjectId);
            Assert.AreEqual("Modified", project.ProjectName);
            Assert.IsTrue(project.ScriptTagRequired.Value);
        }

        #endregion

        #region Script Tests

        [TestMethod]
        public void Script_DefaultConstructor_CreatesEmptyScript()
        {
            // Act
            var script = new Script();

            // Assert
            Assert.IsNull(script.FileName);
            Assert.IsNull(script.BuildOrder);
            Assert.IsNull(script.ScriptId);
        }

        [TestMethod]
        public void Script_ParameterizedConstructor_SetsAllProperties()
        {
            // Arrange
            var scriptId = Guid.NewGuid().ToString();
            var dateAdded = DateTime.UtcNow;
            var dateModified = DateTime.UtcNow.AddDays(1);

            // Act
            var script = new Script(
                fileName: "test.sql",
                buildOrder: 1.5,
                description: "Test script",
                rollBackOnError: true,
                causesBuildFailure: true,
                dateAdded: dateAdded,
                scriptId: scriptId,
                database: "TestDB",
                stripTransactionText: true,
                allowMultipleRuns: false,
                addedBy: "testuser",
                scriptTimeOut: 60,
                dateModified: dateModified,
                modifiedBy: "admin",
                tag: "v1.0");

            // Assert
            Assert.AreEqual("test.sql", script.FileName);
            Assert.AreEqual(1.5, script.BuildOrder);
            Assert.AreEqual("Test script", script.Description);
            Assert.IsTrue(script.RollBackOnError.Value);
            Assert.IsTrue(script.CausesBuildFailure.Value);
            Assert.AreEqual(dateAdded, script.DateAdded);
            Assert.AreEqual(scriptId, script.ScriptId);
            Assert.AreEqual("TestDB", script.Database);
            Assert.IsTrue(script.StripTransactionText.Value);
            Assert.IsFalse(script.AllowMultipleRuns.Value);
            Assert.AreEqual("testuser", script.AddedBy);
            Assert.AreEqual(60, script.ScriptTimeOut);
            Assert.AreEqual(dateModified, script.DateModified);
            Assert.AreEqual("admin", script.ModifiedBy);
            Assert.AreEqual("v1.0", script.Tag);
        }

        [TestMethod]
        public void Script_Properties_CanBeModified()
        {
            // Arrange
            var script = new Script();

            // Act
            script.FileName = "modified.sql";
            script.BuildOrder = 2.0;
            script.Description = "Modified description";
            script.RollBackOnError = true;
            script.ScriptTimeOut = 120;

            // Assert
            Assert.AreEqual("modified.sql", script.FileName);
            Assert.AreEqual(2.0, script.BuildOrder);
            Assert.AreEqual("Modified description", script.Description);
            Assert.IsTrue(script.RollBackOnError.Value);
            Assert.AreEqual(120, script.ScriptTimeOut);
        }

        #endregion

        #region Build Tests

        [TestMethod]
        public void Build_DefaultConstructor_CreatesEmptyBuild()
        {
            // Act
            var build = new Build();

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

        [TestMethod]
        public void Build_ParameterizedConstructor_SetsAllProperties()
        {
            // Arrange
            var buildStart = DateTime.UtcNow;
            var buildEnd = DateTime.UtcNow.AddMinutes(5);
            var buildId = Guid.NewGuid().ToString();

            // Act
            var build = new Build(
                name: "Production Build",
                buildType: "FullDeploy",
                buildStart: buildStart,
                buildEnd: buildEnd,
                serverName: "PROD-SQL",
                finalStatus: BuildItemStatus.Committed,
                buildId: buildId,
                userId: "admin");

            // Assert
            Assert.AreEqual("Production Build", build.Name);
            Assert.AreEqual("FullDeploy", build.BuildType);
            Assert.AreEqual(buildStart, build.BuildStart);
            Assert.AreEqual(buildEnd, build.BuildEnd);
            Assert.AreEqual("PROD-SQL", build.ServerName);
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
            Assert.AreEqual(buildId, build.BuildId);
            Assert.AreEqual("admin", build.UserId);
        }

        [TestMethod]
        public void Build_FinalStatus_AllStatusValuesSupported()
        {
            // Arrange & Act & Assert
            var build1 = new Build { FinalStatus = BuildItemStatus.Committed };
            Assert.AreEqual(BuildItemStatus.Committed, build1.FinalStatus);

            var build2 = new Build { FinalStatus = BuildItemStatus.RolledBack };
            Assert.AreEqual(BuildItemStatus.RolledBack, build2.FinalStatus);

            var build3 = new Build { FinalStatus = BuildItemStatus.FailedNoTransaction };
            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, build3.FinalStatus);

            var build4 = new Build { FinalStatus = BuildItemStatus.TrialRolledBack };
            Assert.AreEqual(BuildItemStatus.TrialRolledBack, build4.FinalStatus);
        }

        #endregion

        #region ScriptRun Tests

        [TestMethod]
        public void ScriptRun_DefaultConstructor_CreatesEmptyScriptRun()
        {
            // Act
            var scriptRun = new ScriptRun();

            // Assert
            Assert.IsNull(scriptRun.FileHash);
            Assert.IsNull(scriptRun.Results);
            Assert.IsNull(scriptRun.FileName);
            Assert.IsNull(scriptRun.RunOrder);
            Assert.IsNull(scriptRun.RunStart);
            Assert.IsNull(scriptRun.RunEnd);
            Assert.IsNull(scriptRun.Success);
            Assert.IsNull(scriptRun.Database);
            Assert.IsNull(scriptRun.ScriptRunId);
            Assert.IsNull(scriptRun.BuildId);
        }

        [TestMethod]
        public void ScriptRun_ParameterizedConstructor_SetsAllProperties()
        {
            // Arrange
            var runStart = DateTime.UtcNow;
            var runEnd = DateTime.UtcNow.AddSeconds(10);
            var scriptRunId = Guid.NewGuid().ToString();
            var buildId = Guid.NewGuid().ToString();

            // Act
            var scriptRun = new ScriptRun(
                fileHash: "HASH123",
                results: "Success",
                fileName: "script.sql",
                runOrder: 1.0,
                runStart: runStart,
                runEnd: runEnd,
                success: true,
                database: "TestDB",
                scriptRunId: scriptRunId,
                buildId: buildId);

            // Assert
            Assert.AreEqual("HASH123", scriptRun.FileHash);
            Assert.AreEqual("Success", scriptRun.Results);
            Assert.AreEqual("script.sql", scriptRun.FileName);
            Assert.AreEqual(1.0, scriptRun.RunOrder);
            Assert.AreEqual(runStart, scriptRun.RunStart);
            Assert.AreEqual(runEnd, scriptRun.RunEnd);
            Assert.IsTrue(scriptRun.Success.Value);
            Assert.AreEqual("TestDB", scriptRun.Database);
            Assert.AreEqual(scriptRunId, scriptRun.ScriptRunId);
            Assert.AreEqual(buildId, scriptRun.BuildId);
        }

        [TestMethod]
        public void ScriptRun_Success_FalseValue_IsStored()
        {
            // Act
            var scriptRun = new ScriptRun { Success = false };

            // Assert
            Assert.IsFalse(scriptRun.Success.Value);
        }

        #endregion

        #region CommittedScript Tests

        [TestMethod]
        public void CommittedScript_DefaultConstructor_CreatesEmptyCommittedScript()
        {
            // Act
            var committedScript = new CommittedScript();

            // Assert
            Assert.IsNull(committedScript.ScriptId);
            Assert.IsNull(committedScript.ServerName);
            Assert.IsNull(committedScript.CommittedDate);
            Assert.IsNull(committedScript.AllowScriptBlock);
            Assert.IsNull(committedScript.ScriptHash);
            Assert.IsNull(committedScript.SqlSyncBuildProjectId);
        }

        [TestMethod]
        public void CommittedScript_ParameterizedConstructor_SetsAllProperties()
        {
            // Arrange
            var scriptId = Guid.NewGuid().ToString();
            var committedDate = DateTime.UtcNow;

            // Act
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "PROD-SQL",
                committedDate: committedDate,
                allowScriptBlock: true,
                scriptHash: "HASH123",
                sqlSyncBuildProjectId: 1);

            // Assert
            Assert.AreEqual(scriptId, committedScript.ScriptId);
            Assert.AreEqual("PROD-SQL", committedScript.ServerName);
            Assert.AreEqual(committedDate, committedScript.CommittedDate);
            Assert.IsTrue(committedScript.AllowScriptBlock.Value);
            Assert.AreEqual("HASH123", committedScript.ScriptHash);
            Assert.AreEqual(1, committedScript.SqlSyncBuildProjectId);
        }

        [TestMethod]
        public void CommittedScript_AllowScriptBlock_FalseValue_IsStored()
        {
            // Act
            var committedScript = new CommittedScript { AllowScriptBlock = false };

            // Assert
            Assert.IsFalse(committedScript.AllowScriptBlock.Value);
        }

        #endregion

        #region SqlSyncBuildDataModel Tests

        [TestMethod]
        public void SqlSyncBuildDataModel_Constructor_SetsAllCollections()
        {
            // Arrange
            var projects = new List<SqlSyncBuildProject> { new SqlSyncBuildProject(1, "Test", true) };
            var scripts = new List<Script> { new Script { FileName = "test.sql" } };
            var builds = new List<Build> { new Build { Name = "Build1" } };
            var scriptRuns = new List<ScriptRun> { new ScriptRun { FileName = "test.sql" } };
            var committedScripts = new List<CommittedScript> { new CommittedScript { ScriptId = "id1" } };

            // Act
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: projects,
                script: scripts,
                build: builds,
                scriptRun: scriptRuns,
                committedScript: committedScripts);

            // Assert
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(1, model.Script.Count);
            Assert.AreEqual(1, model.Build.Count);
            Assert.AreEqual(1, model.ScriptRun.Count);
            Assert.AreEqual(1, model.CommittedScript.Count);
        }

        [TestMethod]
        public void SqlSyncBuildDataModel_Constructor_WithEmptyCollections_CreatesEmptyModel()
        {
            // Act
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Assert
            Assert.AreEqual(0, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(0, model.Script.Count);
            Assert.AreEqual(0, model.Build.Count);
            Assert.AreEqual(0, model.ScriptRun.Count);
            Assert.AreEqual(0, model.CommittedScript.Count);
        }

        [TestMethod]
        public void SqlSyncBuildDataModel_Collections_CanBeModified()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Act
            model.Script.Add(new Script { FileName = "new.sql" });
            model.Build.Add(new Build { Name = "NewBuild" });

            // Assert
            Assert.AreEqual(1, model.Script.Count);
            Assert.AreEqual("new.sql", model.Script[0].FileName);
            Assert.AreEqual(1, model.Build.Count);
            Assert.AreEqual("NewBuild", model.Build[0].Name);
        }

        #endregion

        #region SqlBuildRunDataModel Tests

        [TestMethod]
        public void SqlBuildRunDataModel_DefaultConstructor_CreatesEmptyModel()
        {
            // Act
            var model = new SqlBuildRunDataModel();

            // Assert
            Assert.IsNull(model.BuildDataModel);
            Assert.IsNull(model.BuildType);
            Assert.IsNull(model.Server);
            Assert.IsNull(model.BuildDescription);
            Assert.IsNull(model.StartIndex);
            Assert.IsNull(model.ProjectFileName);
            Assert.IsNull(model.IsTrial);
            Assert.IsNotNull(model.RunItemIndexes);
            Assert.IsNull(model.RunScriptOnly);
            Assert.IsNull(model.BuildFileName);
            Assert.IsNull(model.LogToDatabaseName);
            Assert.IsNull(model.IsTransactional);
            Assert.IsNull(model.PlatinumDacPacFileName);
            Assert.IsNotNull(model.TargetDatabaseOverrides);
            Assert.IsNull(model.ForceCustomDacpac);
            Assert.IsNull(model.BuildRevision);
            Assert.AreEqual(500, model.DefaultScriptTimeout);
            Assert.IsNull(model.AllowObjectDelete);
        }

        [TestMethod]
        public void SqlBuildRunDataModel_ParameterizedConstructor_SetsAllProperties()
        {
            // Arrange
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var runItemIndexes = new List<double> { 1.0, 2.0 };
            var overrides = new List<SqlSync.Connection.DatabaseOverride>();

            // Act
            var model = new SqlBuildRunDataModel(
                buildDataModel: buildDataModel,
                buildType: "Production",
                server: "PROD-SQL",
                buildDescription: "Production deployment",
                startIndex: 1.0,
                projectFileName: "project.sbx",
                isTrial: false,
                runItemIndexes: runItemIndexes,
                runScriptOnly: false,
                buildFileName: "build.sbm",
                logToDatabaseName: "LogDb",
                isTransactional: true,
                platinumDacPacFileName: "baseline.dacpac",
                targetDatabaseOverrides: overrides,
                forceCustomDacpac: false,
                buildRevision: "1.0.0",
                defaultScriptTimeout: 120,
                allowObjectDelete: true);

            // Assert
            Assert.IsNotNull(model.BuildDataModel);
            Assert.AreEqual("Production", model.BuildType);
            Assert.AreEqual("PROD-SQL", model.Server);
            Assert.AreEqual("Production deployment", model.BuildDescription);
            Assert.AreEqual(1.0, model.StartIndex);
            Assert.AreEqual("project.sbx", model.ProjectFileName);
            Assert.IsFalse(model.IsTrial.Value);
            Assert.AreEqual(2, model.RunItemIndexes.Count);
            Assert.IsFalse(model.RunScriptOnly.Value);
            Assert.AreEqual("build.sbm", model.BuildFileName);
            Assert.AreEqual("LogDb", model.LogToDatabaseName);
            Assert.IsTrue(model.IsTransactional.Value);
            Assert.AreEqual("baseline.dacpac", model.PlatinumDacPacFileName);
            Assert.IsNotNull(model.TargetDatabaseOverrides);
            Assert.IsFalse(model.ForceCustomDacpac.Value);
            Assert.AreEqual("1.0.0", model.BuildRevision);
            Assert.AreEqual(120, model.DefaultScriptTimeout);
            Assert.IsTrue(model.AllowObjectDelete.Value);
        }

        #endregion
    }
}
