using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class SqlSyncBuildDataXmlSerializerTests
    {
        private const string SampleBuildProjectXml = @"<?xml version=""1.0"" standalone=""yes""?>
<SqlSyncBuildData xmlns=""http://schemas.mckechney.com/SqlSyncBuildProject.xsd"">
  <SqlSyncBuildProject ProjectName=""TestProject"" ScriptTagRequired=""true"">
    <Scripts>
      <Script FileName=""CreateTable.sql"" BuildOrder=""1"" Description=""Creates test table"" RollBackOnError=""true"" CausesBuildFailure=""true"" DateAdded=""2024-01-15T10:30:00Z"" ScriptId=""11111111-1111-1111-1111-111111111111"" Database=""TestDB"" StripTransactionText=""true"" AllowMultipleRuns=""false"" AddedBy=""testuser"" ScriptTimeOut=""60"" DateModified=""2024-01-16T12:00:00Z"" ModifiedBy=""admin"" Tag=""v1.0"" />
      <Script FileName=""InsertData.sql"" BuildOrder=""2"" Description=""Inserts test data"" RollBackOnError=""true"" CausesBuildFailure=""true"" DateAdded=""2024-01-15T11:00:00Z"" ScriptId=""22222222-2222-2222-2222-222222222222"" Database=""TestDB"" StripTransactionText=""false"" AllowMultipleRuns=""true"" AddedBy=""testuser"" ScriptTimeOut=""30"" Tag=""v1.0"" />
    </Scripts>
    <Builds>
      <Build Name=""Build1"" BuildType=""Production"" BuildStart=""2024-01-20T08:00:00Z"" BuildEnd=""2024-01-20T08:05:00Z"" ServerName=""PROD-SQL"" FinalStatus=""Committed"" BuildId=""ABCD1234"" UserId=""deployer"">
        <ScriptRun FileName=""CreateTable.sql"" RunOrder=""1"" RunStart=""2024-01-20T08:01:00Z"" RunEnd=""2024-01-20T08:02:00Z"" Success=""true"" Database=""TestDB"" ScriptRunId=""SR-001"">
          <FileHash>HASH123</FileHash>
          <Results>Success</Results>
        </ScriptRun>
      </Build>
    </Builds>
    <CommittedScript ScriptId=""11111111-1111-1111-1111-111111111111"" ServerName=""PROD-SQL"" CommittedDate=""2024-01-20T08:02:00Z"" AllowScriptBlock=""true"" ScriptHash=""HASH123"" />
  </SqlSyncBuildProject>
</SqlSyncBuildData>";

        #region Load Tests

        [TestMethod]
        public void Load_FromXDocument_ParsesAllProjects()
        {
            // Arrange
            var doc = XDocument.Parse(SampleBuildProjectXml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual("TestProject", model.SqlSyncBuildProject[0].ProjectName);
            Assert.IsTrue(model.SqlSyncBuildProject[0].ScriptTagRequired.Value);
        }

        [TestMethod]
        public void Load_FromXDocument_ParsesAllScripts()
        {
            // Arrange
            var doc = XDocument.Parse(SampleBuildProjectXml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(2, model.Script.Count);
            
            var script1 = model.Script[0];
            Assert.AreEqual("CreateTable.sql", script1.FileName);
            Assert.AreEqual(1.0, script1.BuildOrder);
            Assert.AreEqual("Creates test table", script1.Description);
            Assert.IsTrue(script1.RollBackOnError.Value);
            Assert.IsTrue(script1.CausesBuildFailure.Value);
            Assert.AreEqual("TestDB", script1.Database);
            Assert.IsTrue(script1.StripTransactionText.Value);
            Assert.IsFalse(script1.AllowMultipleRuns.Value);
            Assert.AreEqual("testuser", script1.AddedBy);
            Assert.AreEqual(60, script1.ScriptTimeOut);
            Assert.AreEqual("admin", script1.ModifiedBy);
            Assert.AreEqual("v1.0", script1.Tag);
            
            var script2 = model.Script[1];
            Assert.AreEqual("InsertData.sql", script2.FileName);
            Assert.AreEqual(2.0, script2.BuildOrder);
        }

        [TestMethod]
        public void Load_FromXDocument_ParsesBuilds()
        {
            // Arrange
            var doc = XDocument.Parse(SampleBuildProjectXml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(1, model.Build.Count);
            var build = model.Build[0];
            Assert.AreEqual("Build1", build.Name);
            Assert.AreEqual("Production", build.BuildType);
            Assert.AreEqual("PROD-SQL", build.ServerName);
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
            Assert.AreEqual("ABCD1234", build.BuildId);
            Assert.AreEqual("deployer", build.UserId);
        }

        [TestMethod]
        public void Load_FromXDocument_ParsesScriptRuns()
        {
            // Arrange
            var doc = XDocument.Parse(SampleBuildProjectXml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(1, model.ScriptRun.Count);
            var scriptRun = model.ScriptRun[0];
            Assert.AreEqual("CreateTable.sql", scriptRun.FileName);
            Assert.AreEqual(1.0, scriptRun.RunOrder);
            Assert.IsTrue(scriptRun.Success.Value);
            Assert.AreEqual("TestDB", scriptRun.Database);
            Assert.AreEqual("SR-001", scriptRun.ScriptRunId);
            Assert.AreEqual("HASH123", scriptRun.FileHash);
            Assert.AreEqual("Success", scriptRun.Results);
        }

        [TestMethod]
        public void Load_FromXDocument_ParsesCommittedScripts()
        {
            // Arrange
            var doc = XDocument.Parse(SampleBuildProjectXml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(1, model.CommittedScript.Count);
            var cs = model.CommittedScript[0];
            Assert.AreEqual("11111111-1111-1111-1111-111111111111", cs.ScriptId);
            Assert.AreEqual("PROD-SQL", cs.ServerName);
            Assert.IsTrue(cs.AllowScriptBlock.Value);
            Assert.AreEqual("HASH123", cs.ScriptHash);
        }

        [TestMethod]
        public void Load_FromFile_ParsesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, SampleBuildProjectXml);

                // Act
                var model = SqlSyncBuildDataXmlSerializer.Load(tempFile);

                // Assert
                Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
                Assert.AreEqual(2, model.Script.Count);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Load_FromXDocument_WithNullRoot_ThrowsException()
        {
            // Arrange
            var doc = new XDocument();

            // Act
            SqlSyncBuildDataXmlSerializer.Load(doc);
        }

        [TestMethod]
        public void Load_EmptyProject_CreatesEmptyCollections()
        {
            // Arrange
            var xml = @"<?xml version=""1.0""?>
<SqlSyncBuildData xmlns=""http://schemas.mckechney.com/SqlSyncBuildProject.xsd"">
  <SqlSyncBuildProject ProjectName=""Empty"" ScriptTagRequired=""false"">
    <Scripts />
    <Builds />
  </SqlSyncBuildProject>
</SqlSyncBuildData>";
            var doc = XDocument.Parse(xml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(0, model.Script.Count);
            Assert.AreEqual(0, model.Build.Count);
            Assert.AreEqual(0, model.ScriptRun.Count);
            Assert.AreEqual(0, model.CommittedScript.Count);
        }

        #endregion

        #region Save Tests

        [TestMethod]
        public async Task Save_ToFile_CreatesValidXml()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var model = CreateTestModel();

                // Act
                await SqlSyncBuildDataXmlSerializer.SaveAsync(tempFile, model);

                // Assert
                Assert.IsTrue(File.Exists(tempFile));
                var content = File.ReadAllText(tempFile);
                Assert.IsTrue(content.Contains("SqlSyncBuildData"));
                Assert.IsTrue(content.Contains("TestProject"));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void BuildDocument_CreatesCorrectRootElement()
        {
            // Arrange
            var model = CreateTestModel();

            // Act
            var doc = SqlSyncBuildDataXmlSerializer.BuildDocument(model);

            // Assert
            Assert.IsNotNull(doc.Root);
            Assert.AreEqual("SqlSyncBuildData", doc.Root.Name.LocalName);
            Assert.AreEqual("http://schemas.mckechney.com/SqlSyncBuildProject.xsd", doc.Root.Name.NamespaceName);
        }

        [TestMethod]
        public void BuildDocument_IncludesAllProjects()
        {
            // Arrange
            var model = CreateTestModel();

            // Act
            var doc = SqlSyncBuildDataXmlSerializer.BuildDocument(model);

            // Assert
            var ns = (XNamespace)"http://schemas.mckechney.com/SqlSyncBuildProject.xsd";
            var projects = doc.Root.Elements(ns + "SqlSyncBuildProject").ToList();
            Assert.AreEqual(1, projects.Count);
            Assert.AreEqual("TestProject", (string)projects[0].Attribute("ProjectName"));
        }

        [TestMethod]
        public void BuildDocument_IncludesAllScripts()
        {
            // Arrange
            var model = CreateTestModel();

            // Act
            var doc = SqlSyncBuildDataXmlSerializer.BuildDocument(model);

            // Assert
            var ns = (XNamespace)"http://schemas.mckechney.com/SqlSyncBuildProject.xsd";
            var scripts = doc.Root
                .Element(ns + "SqlSyncBuildProject")
                .Element(ns + "Scripts")
                .Elements(ns + "Script")
                .ToList();
            Assert.AreEqual(1, scripts.Count);
            Assert.AreEqual("test.sql", (string)scripts[0].Attribute("FileName"));
        }

        [TestMethod]
        public void BuildDocument_IncludesBuildsAndScriptRuns()
        {
            // Arrange
            var model = CreateTestModel();

            // Act
            var doc = SqlSyncBuildDataXmlSerializer.BuildDocument(model);

            // Assert
            var ns = (XNamespace)"http://schemas.mckechney.com/SqlSyncBuildProject.xsd";
            var builds = doc.Root
                .Element(ns + "SqlSyncBuildProject")
                .Element(ns + "Builds")
                .Elements(ns + "Build")
                .ToList();
            Assert.AreEqual(1, builds.Count);
            
            var scriptRuns = builds[0].Elements(ns + "ScriptRun").ToList();
            Assert.AreEqual(1, scriptRuns.Count);
        }

        #endregion

        #region Round-Trip Tests

        [TestMethod]
        public async Task RoundTrip_SaveAndLoad_PreservesData()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var originalModel = CreateTestModel();

                // Act
                await SqlSyncBuildDataXmlSerializer.SaveAsync(tempFile, originalModel);
                var loadedModel = SqlSyncBuildDataXmlSerializer.Load(tempFile);

                // Assert
                Assert.AreEqual(originalModel.SqlSyncBuildProject.Count, loadedModel.SqlSyncBuildProject.Count);
                Assert.AreEqual(originalModel.Script.Count, loadedModel.Script.Count);
                Assert.AreEqual(originalModel.Build.Count, loadedModel.Build.Count);
                Assert.AreEqual(originalModel.ScriptRun.Count, loadedModel.ScriptRun.Count);
                Assert.AreEqual(originalModel.CommittedScript.Count, loadedModel.CommittedScript.Count);

                // Verify data integrity
                Assert.AreEqual(originalModel.SqlSyncBuildProject[0].ProjectName, loadedModel.SqlSyncBuildProject[0].ProjectName);
                Assert.AreEqual(originalModel.Script[0].FileName, loadedModel.Script[0].FileName);
                Assert.AreEqual(originalModel.Build[0].Name, loadedModel.Build[0].Name);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [TestMethod]
        public async Task RoundTrip_EmptyModel_PreservesStructure()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var emptyModel = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: new List<SqlSyncBuildProject> { new SqlSyncBuildProject(0, "Empty", false) },
                    script: new List<Script>(),
                    build: new List<Build>(),
                    scriptRun: new List<ScriptRun>(),
                    committedScript: new List<CommittedScript>());

                // Act
                await SqlSyncBuildDataXmlSerializer.SaveAsync(tempFile, emptyModel);
                var loadedModel = SqlSyncBuildDataXmlSerializer.Load(tempFile);

                // Assert
                Assert.AreEqual(1, loadedModel.SqlSyncBuildProject.Count);
                Assert.AreEqual(0, loadedModel.Script.Count);
                Assert.AreEqual(0, loadedModel.Build.Count);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Load_WithNullAttributeValues_ParsesAsNull()
        {
            // Arrange
            var xml = @"<?xml version=""1.0""?>
<SqlSyncBuildData xmlns=""http://schemas.mckechney.com/SqlSyncBuildProject.xsd"">
  <SqlSyncBuildProject>
    <Scripts>
      <Script FileName=""test.sql"" ScriptId=""00000000-0000-0000-0000-000000000001"" />
    </Scripts>
    <Builds />
  </SqlSyncBuildProject>
</SqlSyncBuildData>";
            var doc = XDocument.Parse(xml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(1, model.Script.Count);
            var script = model.Script[0];
            Assert.AreEqual("test.sql", script.FileName);
            Assert.IsNull(script.Description);
            Assert.IsNull(script.BuildOrder);
            Assert.IsNull(script.RollBackOnError);
        }

        [TestMethod]
        public void Load_MultipleBuildsFinalStatus_ParsesAllStatuses()
        {
            // Arrange
            var xml = @"<?xml version=""1.0""?>
<SqlSyncBuildData xmlns=""http://schemas.mckechney.com/SqlSyncBuildProject.xsd"">
  <SqlSyncBuildProject ProjectName=""Test"">
    <Scripts />
    <Builds>
      <Build Name=""Build1"" FinalStatus=""Committed"" />
      <Build Name=""Build2"" FinalStatus=""RolledBack"" />
      <Build Name=""Build3"" FinalStatus=""TrialRolledBack"" />
      <Build Name=""Build4"" FinalStatus=""FailedNoTransaction"" />
    </Builds>
  </SqlSyncBuildProject>
</SqlSyncBuildData>";
            var doc = XDocument.Parse(xml);

            // Act
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            // Assert
            Assert.AreEqual(4, model.Build.Count);
            Assert.AreEqual(BuildItemStatus.Committed, model.Build[0].FinalStatus);
            Assert.AreEqual(BuildItemStatus.RolledBack, model.Build[1].FinalStatus);
            Assert.AreEqual(BuildItemStatus.TrialRolledBack, model.Build[2].FinalStatus);
            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, model.Build[3].FinalStatus);
        }

        [TestMethod]
        public async Task Save_WithSpecialCharactersInDescription_EscapesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var model = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: new List<SqlSyncBuildProject> { new SqlSyncBuildProject(0, "Test", false) },
                    script: new List<Script>
                    {
                        new Script(
                            fileName: "test.sql",
                            buildOrder: 1,
                            description: "Contains <special> & \"characters\"",
                            rollBackOnError: true,
                            causesBuildFailure: true,
                            dateAdded: DateTime.UtcNow,
                            scriptId: Guid.NewGuid().ToString(),
                            database: "TestDB",
                            stripTransactionText: false,
                            allowMultipleRuns: false,
                            addedBy: "user",
                            scriptTimeOut: 30,
                            dateModified: null,
                            modifiedBy: null,
                            tag: null)
                    },
                    build: new List<Build>(),
                    scriptRun: new List<ScriptRun>(),
                    committedScript: new List<CommittedScript>());

                // Act
                await SqlSyncBuildDataXmlSerializer.SaveAsync(tempFile, model);
                var loadedModel = SqlSyncBuildDataXmlSerializer.Load(tempFile);

                // Assert
                Assert.AreEqual("Contains <special> & \"characters\"", loadedModel.Script[0].Description);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #endregion

        #region Helper Methods

        private static SqlSyncBuildDataModel CreateTestModel()
        {
            var scriptId = Guid.NewGuid().ToString();
            var buildId = Guid.NewGuid().ToString();

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
                        buildType: "Production",
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
                        scriptRunId: Guid.NewGuid().ToString(),
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
                });
        }

        #endregion
    }
}
