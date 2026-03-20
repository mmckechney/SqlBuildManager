using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class SqlBuildRunDataTests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesWithDefaultValues()
        {
            // Act
            var runData = new SqlBuildRunData();

            // Assert
            Assert.IsNull(runData.BuildDataModel);
            Assert.AreEqual(string.Empty, runData.BuildType);
            Assert.AreEqual(string.Empty, runData.Server);
            Assert.AreEqual(string.Empty, runData.BuildDescription);
            Assert.AreEqual(0, runData.StartIndex);
            Assert.AreEqual(string.Empty, runData.ProjectFileName);
            Assert.IsFalse(runData.IsTrial);
            Assert.IsNotNull(runData.RunItemIndexes);
            Assert.AreEqual(0, runData.RunItemIndexes.Length);
            Assert.IsFalse(runData.RunScriptOnly);
            Assert.AreEqual(string.Empty, runData.BuildFileName);
            Assert.AreEqual(string.Empty, runData.LogToDatabaseName);
            Assert.IsTrue(runData.IsTransactional);
            Assert.AreEqual(string.Empty, runData.PlatinumDacPacFileName);
            Assert.IsNotNull(runData.TargetDatabaseOverrides);
            Assert.AreEqual(0, runData.TargetDatabaseOverrides.Count);
            Assert.IsFalse(runData.ForceCustomDacpac);
            Assert.AreEqual(string.Empty, runData.BuildRevision);
            Assert.AreEqual(500, runData.DefaultScriptTimeout);
            Assert.IsFalse(runData.AllowObjectDelete);
        }

        [TestMethod]
        public void BuildDataModel_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            runData.BuildDataModel = model;

            // Assert
            Assert.IsNotNull(runData.BuildDataModel);
            Assert.AreSame(model, runData.BuildDataModel);
        }

        [TestMethod]
        public void BuildType_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.BuildType = "Production";

            // Assert
            Assert.AreEqual("Production", runData.BuildType);
        }

        [TestMethod]
        public void Server_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.Server = "SQLSERVER01";

            // Assert
            Assert.AreEqual("SQLSERVER01", runData.Server);
        }

        [TestMethod]
        public void BuildDescription_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.BuildDescription = "Weekly deployment";

            // Assert
            Assert.AreEqual("Weekly deployment", runData.BuildDescription);
        }

        [TestMethod]
        public void StartIndex_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.StartIndex = 5.5;

            // Assert
            Assert.AreEqual(5.5, runData.StartIndex);
        }

        [TestMethod]
        public void ProjectFileName_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();
            var projectFileName = Path.Combine(Path.GetTempPath(), "Projects", "build.sbx");

            // Act
            runData.ProjectFileName = projectFileName;

            // Assert
            Assert.AreEqual(projectFileName, runData.ProjectFileName);
        }

        [TestMethod]
        public void IsTrial_CanBeSetToTrue()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.IsTrial = true;

            // Assert
            Assert.IsTrue(runData.IsTrial);
        }

        [TestMethod]
        public void RunItemIndexes_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();
            var indexes = new double[] { 1.0, 2.5, 3.0 };

            // Act
            runData.RunItemIndexes = indexes;

            // Assert
            Assert.AreEqual(3, runData.RunItemIndexes.Length);
            Assert.AreEqual(1.0, runData.RunItemIndexes[0]);
            Assert.AreEqual(2.5, runData.RunItemIndexes[1]);
            Assert.AreEqual(3.0, runData.RunItemIndexes[2]);
        }

        [TestMethod]
        public void RunScriptOnly_CanBeSetToTrue()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.RunScriptOnly = true;

            // Assert
            Assert.IsTrue(runData.RunScriptOnly);
        }

        [TestMethod]
        public void BuildFileName_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.BuildFileName = "deploy.sbm";

            // Assert
            Assert.AreEqual("deploy.sbm", runData.BuildFileName);
        }

        [TestMethod]
        public void LogToDatabaseName_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.LogToDatabaseName = "AuditDb";

            // Assert
            Assert.AreEqual("AuditDb", runData.LogToDatabaseName);
        }

        [TestMethod]
        public void IsTransactional_CanBeSetToFalse()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.IsTransactional = false;

            // Assert
            Assert.IsFalse(runData.IsTransactional);
        }

        [TestMethod]
        public void PlatinumDacPacFileName_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.PlatinumDacPacFileName = "reference.dacpac";

            // Assert
            Assert.AreEqual("reference.dacpac", runData.PlatinumDacPacFileName);
        }

        [TestMethod]
        public void TargetDatabaseOverrides_CanAddItems()
        {
            // Arrange
            var runData = new SqlBuildRunData();
            var override1 = new DatabaseOverride("server1", "default", "override1");
            var override2 = new DatabaseOverride("server2", "default2", "override2");

            // Act
            runData.TargetDatabaseOverrides.Add(override1);
            runData.TargetDatabaseOverrides.Add(override2);

            // Assert
            Assert.AreEqual(2, runData.TargetDatabaseOverrides.Count);
        }

        [TestMethod]
        public void ForceCustomDacpac_CanBeSetToTrue()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.ForceCustomDacpac = true;

            // Assert
            Assert.IsTrue(runData.ForceCustomDacpac);
        }

        [TestMethod]
        public void BuildRevision_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.BuildRevision = "1.2.3.4";

            // Assert
            Assert.AreEqual("1.2.3.4", runData.BuildRevision);
        }

        [TestMethod]
        public void DefaultScriptTimeout_CanBeSetAndRetrieved()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.DefaultScriptTimeout = 120;

            // Assert
            Assert.AreEqual(120, runData.DefaultScriptTimeout);
        }

        [TestMethod]
        public void AllowObjectDelete_CanBeSetToTrue()
        {
            // Arrange
            var runData = new SqlBuildRunData();

            // Act
            runData.AllowObjectDelete = true;

            // Assert
            Assert.IsTrue(runData.AllowObjectDelete);
        }

        [TestMethod]
        public void AllProperties_CanBeSetTogether()
        {
            // Arrange & Act
            var projectFileName = Path.Combine(Path.GetTempPath(), "Deploy", "project.sbx");
            var runData = new SqlBuildRunData
            {
                BuildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(),
                BuildType = "FullDeploy",
                Server = "PROD-SQL",
                BuildDescription = "Production release v2.0",
                StartIndex = 1.0,
                ProjectFileName = projectFileName,
                IsTrial = false,
                RunItemIndexes = new double[] { 1.0, 2.0 },
                RunScriptOnly = false,
                BuildFileName = "release.sbm",
                LogToDatabaseName = "DeploymentLog",
                IsTransactional = true,
                PlatinumDacPacFileName = "baseline.dacpac",
                ForceCustomDacpac = true,
                BuildRevision = "2.0.0.0",
                DefaultScriptTimeout = 300,
                AllowObjectDelete = false
            };

            // Assert
            Assert.IsNotNull(runData.BuildDataModel);
            Assert.AreEqual("FullDeploy", runData.BuildType);
            Assert.AreEqual("PROD-SQL", runData.Server);
            Assert.AreEqual("Production release v2.0", runData.BuildDescription);
            Assert.AreEqual(1.0, runData.StartIndex);
            Assert.AreEqual(projectFileName, runData.ProjectFileName);
            Assert.IsFalse(runData.IsTrial);
            Assert.AreEqual(2, runData.RunItemIndexes.Length);
            Assert.IsFalse(runData.RunScriptOnly);
            Assert.AreEqual("release.sbm", runData.BuildFileName);
            Assert.AreEqual("DeploymentLog", runData.LogToDatabaseName);
            Assert.IsTrue(runData.IsTransactional);
            Assert.AreEqual("baseline.dacpac", runData.PlatinumDacPacFileName);
            Assert.IsTrue(runData.ForceCustomDacpac);
            Assert.AreEqual("2.0.0.0", runData.BuildRevision);
            Assert.AreEqual(300, runData.DefaultScriptTimeout);
            Assert.IsFalse(runData.AllowObjectDelete);
        }
    }
}
