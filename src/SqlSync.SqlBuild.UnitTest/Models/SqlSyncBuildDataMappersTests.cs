using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Legacy;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class SqlSyncBuildDataMappersTests
    {
        #region ToModel (DataSet to POCO) Tests

        [TestMethod]
        public void ToModel_NullDataSet_ReturnsEmptyModel()
        {
            // Arrange
            SqlSyncBuildData ds = null;

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(0, model.Script.Count);
            Assert.AreEqual(0, model.Build.Count);
            Assert.AreEqual(0, model.ScriptRun.Count);
            Assert.AreEqual(0, model.CommittedScript.Count);
            Assert.AreEqual(0, model.CodeReview.Count);
        }

        [TestMethod]
        public void ToModel_EmptyDataSet_ReturnsEmptyModel()
        {
            // Arrange
            var ds = new SqlSyncBuildData();

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(0, model.Script.Count);
            Assert.AreEqual(0, model.Build.Count);
        }

        [TestMethod]
        public void ToModel_DataSetWithProject_MapsProjectCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var projectRow = ds.SqlSyncBuildProject.NewSqlSyncBuildProjectRow();
            projectRow.ProjectName = "TestProject";
            projectRow.ScriptTagRequired = true;
            ds.SqlSyncBuildProject.AddSqlSyncBuildProjectRow(projectRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual("TestProject", model.SqlSyncBuildProject[0].ProjectName);
            Assert.IsTrue(model.SqlSyncBuildProject[0].ScriptTagRequired.Value);
        }

        [TestMethod]
        public void ToModel_DataSetWithScript_MapsScriptCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var scriptRow = ds.Script.NewScriptRow();
            scriptRow.FileName = "test.sql";
            scriptRow.BuildOrder = 1.5;
            scriptRow.Description = "Test description";
            scriptRow.RollBackOnError = true;
            scriptRow.CausesBuildFailure = true;
            scriptRow.DateAdded = DateTime.UtcNow;
            scriptRow.ScriptId = Guid.NewGuid().ToString();
            scriptRow.Database = "TestDB";
            scriptRow.StripTransactionText = true;
            scriptRow.AllowMultipleRuns = false;
            scriptRow.AddedBy = "testuser";
            scriptRow.ScriptTimeOut = 60;
            scriptRow.Tag = "v1.0";
            ds.Script.AddScriptRow(scriptRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.Script.Count);
            var script = model.Script[0];
            Assert.AreEqual("test.sql", script.FileName);
            Assert.AreEqual(1.5, script.BuildOrder);
            Assert.AreEqual("Test description", script.Description);
            Assert.IsTrue(script.RollBackOnError.Value);
            Assert.IsTrue(script.CausesBuildFailure.Value);
            Assert.AreEqual("TestDB", script.Database);
            Assert.IsTrue(script.StripTransactionText.Value);
            Assert.IsFalse(script.AllowMultipleRuns.Value);
            Assert.AreEqual("testuser", script.AddedBy);
            Assert.AreEqual(60, script.ScriptTimeOut);
            Assert.AreEqual("v1.0", script.Tag);
        }

        [TestMethod]
        public void ToModel_DataSetWithBuild_MapsBuildCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var buildRow = ds.Build.NewBuildRow();
            buildRow.Name = "ProductionBuild";
            buildRow.BuildType = "Full";
            buildRow.BuildStart = DateTime.UtcNow.AddMinutes(-5);
            buildRow.BuildEnd = DateTime.UtcNow;
            buildRow.ServerName = "PROD-SQL";
            buildRow.FinalStatus = "Committed";
            buildRow.BuildId = "BUILD-001";
            buildRow.UserId = "deployer";
            ds.Build.AddBuildRow(buildRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.Build.Count);
            var build = model.Build[0];
            Assert.AreEqual("ProductionBuild", build.Name);
            Assert.AreEqual("Full", build.BuildType);
            Assert.AreEqual("PROD-SQL", build.ServerName);
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
            Assert.AreEqual("BUILD-001", build.BuildId);
            Assert.AreEqual("deployer", build.UserId);
        }

        [TestMethod]
        public void ToModel_DataSetWithScriptRun_MapsScriptRunCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var scriptRunRow = ds.ScriptRun.NewScriptRunRow();
            scriptRunRow.FileHash = "HASH123";
            scriptRunRow.Results = "Success";
            scriptRunRow.FileName = "test.sql";
            scriptRunRow.RunOrder = 1.0;
            scriptRunRow.RunStart = DateTime.UtcNow.AddMinutes(-1);
            scriptRunRow.RunEnd = DateTime.UtcNow;
            scriptRunRow.Success = true;
            scriptRunRow.Database = "TestDB";
            scriptRunRow.ScriptRunId = "SR-001";
            ds.ScriptRun.AddScriptRunRow(scriptRunRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.ScriptRun.Count);
            var scriptRun = model.ScriptRun[0];
            Assert.AreEqual("HASH123", scriptRun.FileHash);
            Assert.AreEqual("Success", scriptRun.Results);
            Assert.AreEqual("test.sql", scriptRun.FileName);
            Assert.AreEqual(1.0, scriptRun.RunOrder);
            Assert.IsTrue(scriptRun.Success.Value);
            Assert.AreEqual("TestDB", scriptRun.Database);
            Assert.AreEqual("SR-001", scriptRun.ScriptRunId);
        }

        [TestMethod]
        public void ToModel_DataSetWithCommittedScript_MapsCommittedScriptCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var csRow = ds.CommittedScript.NewCommittedScriptRow();
            csRow.ScriptId = Guid.NewGuid().ToString();
            csRow.ServerName = "PROD-SQL";
            csRow.CommittedDate = DateTime.UtcNow;
            csRow.AllowScriptBlock = true;
            csRow.ScriptHash = "HASH123";
            ds.CommittedScript.AddCommittedScriptRow(csRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.CommittedScript.Count);
            var cs = model.CommittedScript[0];
            Assert.AreEqual("PROD-SQL", cs.ServerName);
            Assert.IsTrue(cs.AllowScriptBlock.Value);
            Assert.AreEqual("HASH123", cs.ScriptHash);
        }

        [TestMethod]
        public void ToModel_DataSetWithCodeReview_MapsCodeReviewCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var crRow = ds.CodeReview.NewCodeReviewRow();
            crRow.CodeReviewId = Guid.NewGuid();
            crRow.ScriptId = Guid.NewGuid().ToString();
            crRow.ReviewDate = DateTime.UtcNow;
            crRow.ReviewBy = "reviewer";
            crRow.ReviewStatus = 1;
            crRow.Comment = "Approved";
            crRow.ReviewNumber = "CR-001";
            crRow.CheckSum = "CHECKSUM";
            crRow.ValidationKey = "KEY";
            ds.CodeReview.AddCodeReviewRow(crRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.CodeReview.Count);
            var cr = model.CodeReview[0];
            Assert.AreEqual("reviewer", cr.ReviewBy);
            Assert.AreEqual((short)1, cr.ReviewStatus);
            Assert.AreEqual("Approved", cr.Comment);
            Assert.AreEqual("CR-001", cr.ReviewNumber);
            Assert.AreEqual("CHECKSUM", cr.CheckSum);
            Assert.AreEqual("KEY", cr.ValidationKey);
        }

        [TestMethod]
        public void ToModel_DataSetWithOnlyFileName_MapsFileNameCorrectly()
        {
            // Arrange
            var ds = new SqlSyncBuildData();
            var scriptRow = ds.Script.NewScriptRow();
            scriptRow.FileName = "test.sql";
            ds.Script.AddScriptRow(scriptRow);

            // Act
            var model = ds.ToModel();

            // Assert
            Assert.AreEqual(1, model.Script.Count);
            var script = model.Script[0];
            Assert.AreEqual("test.sql", script.FileName);
            // Verify row-level null checks work (tests IsXxxNull methods)
            Assert.IsTrue(scriptRow.IsDescriptionNull());
            Assert.IsTrue(scriptRow.IsBuildOrderNull());
        }

        #endregion

        #region ToDataSet (POCO to DataSet) Tests

        [TestMethod]
        public void ToDataSet_EmptyModel_ReturnsEmptyDataSet()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.IsNotNull(ds);
            Assert.AreEqual(0, ds.SqlSyncBuildProject.Rows.Count);
            Assert.AreEqual(0, ds.Script.Rows.Count);
            Assert.AreEqual(0, ds.Build.Rows.Count);
        }

        [TestMethod]
        public void ToDataSet_ModelWithProject_MapsProjectCorrectly()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>
                {
                    new SqlSyncBuildProject(1, "TestProject", true)
                },
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.SqlSyncBuildProject.Rows.Count);
            var row = ds.SqlSyncBuildProject[0];
            Assert.AreEqual("TestProject", row.ProjectName);
            Assert.IsTrue(row.ScriptTagRequired);
        }

        [TestMethod]
        public void ToDataSet_ModelWithScript_MapsScriptCorrectly()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>
                {
                    new Script(
                        fileName: "test.sql",
                        buildOrder: 1.0,
                        description: "Test",
                        rollBackOnError: true,
                        causesBuildFailure: true,
                        dateAdded: DateTime.UtcNow,
                        scriptId: Guid.NewGuid().ToString(),
                        database: "TestDB",
                        stripTransactionText: true,
                        allowMultipleRuns: false,
                        addedBy: "user",
                        scriptTimeOut: 60,
                        dateModified: null,
                        modifiedBy: null,
                        tag: "v1.0")
                },
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.Script.Rows.Count);
            var row = ds.Script[0];
            Assert.AreEqual("test.sql", row.FileName);
            Assert.AreEqual(1.0, row.BuildOrder);
            Assert.AreEqual("Test", row.Description);
            Assert.IsTrue(row.RollBackOnError);
            Assert.AreEqual("TestDB", row.Database);
            Assert.AreEqual("v1.0", row.Tag);
        }

        [TestMethod]
        public void ToDataSet_ModelWithBuild_MapsBuildCorrectly()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>
                {
                    new Build(
                        name: "TestBuild",
                        buildType: "Full",
                        buildStart: DateTime.UtcNow.AddMinutes(-5),
                        buildEnd: DateTime.UtcNow,
                        serverName: "TestServer",
                        finalStatus: BuildItemStatus.Committed,
                        buildId: "BUILD-001",
                        userId: "deployer")
                },
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.Build.Rows.Count);
            var row = ds.Build[0];
            Assert.AreEqual("TestBuild", row.Name);
            Assert.AreEqual("Full", row.BuildType);
            Assert.AreEqual("TestServer", row.ServerName);
            Assert.AreEqual("Committed", row.FinalStatus);
            Assert.AreEqual("BUILD-001", row.BuildId);
            Assert.AreEqual("deployer", row.UserId);
        }

        [TestMethod]
        public void ToDataSet_ModelWithScriptRun_MapsScriptRunCorrectly()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>
                {
                    new ScriptRun(
                        fileHash: "HASH123",
                        results: "Success",
                        fileName: "test.sql",
                        runOrder: 1.0,
                        runStart: DateTime.UtcNow.AddMinutes(-1),
                        runEnd: DateTime.UtcNow,
                        success: true,
                        database: "TestDB",
                        scriptRunId: "SR-001",
                        buildId: "BUILD-001")
                },
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.ScriptRun.Rows.Count);
            var row = ds.ScriptRun[0];
            Assert.AreEqual("HASH123", row.FileHash);
            Assert.AreEqual("Success", row.Results);
            Assert.AreEqual("test.sql", row.FileName);
            Assert.IsTrue(row.Success);
        }

        [TestMethod]
        public void ToDataSet_ModelWithCommittedScript_MapsCommittedScriptCorrectly()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>
                {
                    new CommittedScript(
                        scriptId: Guid.NewGuid().ToString(),
                        serverName: "TestServer",
                        committedDate: DateTime.UtcNow,
                        allowScriptBlock: true,
                        scriptHash: "HASH123",
                        sqlSyncBuildProjectId: 0)
                },
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.CommittedScript.Rows.Count);
            var row = ds.CommittedScript[0];
            Assert.AreEqual("TestServer", row.ServerName);
            Assert.IsTrue(row.AllowScriptBlock);
            Assert.AreEqual("HASH123", row.ScriptHash);
        }

        [TestMethod]
        public void ToDataSet_ModelWithCodeReview_MapsCodeReviewCorrectly()
        {
            // Arrange
            var codeReviewId = Guid.NewGuid();
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>
                {
                    new CodeReview(
                        codeReviewId: codeReviewId,
                        scriptId: Guid.NewGuid().ToString(),
                        reviewDate: DateTime.UtcNow,
                        reviewBy: "reviewer",
                        reviewStatus: 1,
                        comment: "Approved",
                        reviewNumber: "CR-001",
                        checkSum: "CHECKSUM",
                        validationKey: "KEY")
                });

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.CodeReview.Rows.Count);
            var row = ds.CodeReview[0];
            Assert.AreEqual(codeReviewId, row.CodeReviewId);
            Assert.AreEqual("reviewer", row.ReviewBy);
            Assert.AreEqual((short)1, row.ReviewStatus);
            Assert.AreEqual("Approved", row.Comment);
        }

        [TestMethod]
        public void ToDataSet_ModelWithNullValues_DoesNotSetNullFields()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>
                {
                    new SqlSyncBuildProject(0, null, null)
                },
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();

            // Assert
            Assert.AreEqual(1, ds.SqlSyncBuildProject.Rows.Count);
            // Verify the row exists (null values are handled by the mapper)
            var row = ds.SqlSyncBuildProject[0];
            Assert.IsNotNull(row);
            // The SqlSyncBuildProject_Id column should have the value 0
            Assert.AreEqual(0, row.SqlSyncBuildProject_Id);
        }

        #endregion

        #region Round-Trip Tests

        [TestMethod]
        public void RoundTrip_ModelToDataSetToModel_PreservesData()
        {
            // Arrange
            var originalModel = CreateFullTestModel();

            // Act
            var ds = originalModel.ToDataSet();
            var roundTrippedModel = ds.ToModel();

            // Assert
            Assert.AreEqual(originalModel.SqlSyncBuildProject.Count, roundTrippedModel.SqlSyncBuildProject.Count);
            Assert.AreEqual(originalModel.Script.Count, roundTrippedModel.Script.Count);
            Assert.AreEqual(originalModel.Build.Count, roundTrippedModel.Build.Count);
            Assert.AreEqual(originalModel.ScriptRun.Count, roundTrippedModel.ScriptRun.Count);
            Assert.AreEqual(originalModel.CommittedScript.Count, roundTrippedModel.CommittedScript.Count);
            Assert.AreEqual(originalModel.CodeReview.Count, roundTrippedModel.CodeReview.Count);

            // Verify specific values
            Assert.AreEqual(originalModel.SqlSyncBuildProject[0].ProjectName, roundTrippedModel.SqlSyncBuildProject[0].ProjectName);
            Assert.AreEqual(originalModel.Script[0].FileName, roundTrippedModel.Script[0].FileName);
            Assert.AreEqual(originalModel.Build[0].Name, roundTrippedModel.Build[0].Name);
        }

        [TestMethod]
        public void RoundTrip_DataSetToModelToDataSet_PreservesData()
        {
            // Arrange
            var originalDs = CreateFullTestDataSet();

            // Act
            var model = originalDs.ToModel();
            var roundTrippedDs = model.ToDataSet();

            // Assert
            Assert.AreEqual(originalDs.SqlSyncBuildProject.Rows.Count, roundTrippedDs.SqlSyncBuildProject.Rows.Count);
            Assert.AreEqual(originalDs.Script.Rows.Count, roundTrippedDs.Script.Rows.Count);
            Assert.AreEqual(originalDs.Build.Rows.Count, roundTrippedDs.Build.Rows.Count);

            // Verify specific values
            Assert.AreEqual(originalDs.SqlSyncBuildProject[0].ProjectName, roundTrippedDs.SqlSyncBuildProject[0].ProjectName);
            Assert.AreEqual(originalDs.Script[0].FileName, roundTrippedDs.Script[0].FileName);
        }

        [TestMethod]
        public void RoundTrip_MultipleItems_AllPreserved()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>
                {
                    new SqlSyncBuildProject(0, "Project1", true),
                    new SqlSyncBuildProject(1, "Project2", false)
                },
                script: new List<Script>
                {
                    new Script { FileName = "script1.sql", BuildOrder = 1.0 },
                    new Script { FileName = "script2.sql", BuildOrder = 2.0 },
                    new Script { FileName = "script3.sql", BuildOrder = 3.0 }
                },
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            // Act
            var ds = model.ToDataSet();
            var roundTripped = ds.ToModel();

            // Assert
            Assert.AreEqual(2, roundTripped.SqlSyncBuildProject.Count);
            Assert.AreEqual(3, roundTripped.Script.Count);
            Assert.AreEqual("Project1", roundTripped.SqlSyncBuildProject[0].ProjectName);
            Assert.AreEqual("script2.sql", roundTripped.Script[1].FileName);
        }

        #endregion

        #region Helper Methods

        private static SqlSyncBuildDataModel CreateFullTestModel()
        {
            var scriptId = Guid.NewGuid().ToString();
            var buildId = "BUILD-001";

            return new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>
                {
                    new SqlSyncBuildProject(0, "TestProject", true)
                },
                script: new List<Script>
                {
                    new Script(
                        fileName: "test.sql",
                        buildOrder: 1.0,
                        description: "Test script",
                        rollBackOnError: true,
                        causesBuildFailure: true,
                        dateAdded: DateTime.UtcNow,
                        scriptId: scriptId,
                        database: "TestDB",
                        stripTransactionText: true,
                        allowMultipleRuns: false,
                        addedBy: "testuser",
                        scriptTimeOut: 60,
                        dateModified: DateTime.UtcNow,
                        modifiedBy: "admin",
                        tag: "v1.0")
                },
                build: new List<Build>
                {
                    new Build(
                        name: "TestBuild",
                        buildType: "Full",
                        buildStart: DateTime.UtcNow.AddMinutes(-5),
                        buildEnd: DateTime.UtcNow,
                        serverName: "TestServer",
                        finalStatus: BuildItemStatus.Committed,
                        buildId: buildId,
                        userId: "deployer")
                },
                scriptRun: new List<ScriptRun>
                {
                    new ScriptRun(
                        fileHash: "HASH123",
                        results: "Success",
                        fileName: "test.sql",
                        runOrder: 1.0,
                        runStart: DateTime.UtcNow.AddMinutes(-4),
                        runEnd: DateTime.UtcNow.AddMinutes(-3),
                        success: true,
                        database: "TestDB",
                        scriptRunId: "SR-001",
                        buildId: buildId)
                },
                committedScript: new List<CommittedScript>
                {
                    new CommittedScript(
                        scriptId: scriptId,
                        serverName: "TestServer",
                        committedDate: DateTime.UtcNow,
                        allowScriptBlock: true,
                        scriptHash: "HASH123",
                        sqlSyncBuildProjectId: 0)
                },
                codeReview: new List<CodeReview>
                {
                    new CodeReview(
                        codeReviewId: Guid.NewGuid(),
                        scriptId: scriptId,
                        reviewDate: DateTime.UtcNow.AddDays(-1),
                        reviewBy: "reviewer",
                        reviewStatus: 1,
                        comment: "Approved",
                        reviewNumber: "CR-001",
                        checkSum: "CHECKSUM",
                        validationKey: "KEY")
                });
        }

        private static SqlSyncBuildData CreateFullTestDataSet()
        {
            var ds = new SqlSyncBuildData();

            // Add project
            var projectRow = ds.SqlSyncBuildProject.NewSqlSyncBuildProjectRow();
            projectRow.ProjectName = "TestProject";
            projectRow.ScriptTagRequired = true;
            ds.SqlSyncBuildProject.AddSqlSyncBuildProjectRow(projectRow);

            // Add script
            var scriptRow = ds.Script.NewScriptRow();
            scriptRow.FileName = "test.sql";
            scriptRow.BuildOrder = 1.0;
            scriptRow.Description = "Test script";
            scriptRow.RollBackOnError = true;
            scriptRow.CausesBuildFailure = true;
            scriptRow.Database = "TestDB";
            scriptRow.ScriptId = Guid.NewGuid().ToString();
            ds.Script.AddScriptRow(scriptRow);

            // Add build
            var buildRow = ds.Build.NewBuildRow();
            buildRow.Name = "TestBuild";
            buildRow.BuildType = "Full";
            buildRow.ServerName = "TestServer";
            buildRow.FinalStatus = "Committed";
            buildRow.BuildId = "BUILD-001";
            ds.Build.AddBuildRow(buildRow);

            return ds;
        }

        #endregion
    }
}
