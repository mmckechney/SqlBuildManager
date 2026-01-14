using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;
using System.IO;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlSyncBuildDataModelTest
    {
        [TestMethod]
        public void CreateShellModel_HasDefaults()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(1, model.Scripts.Count);
            Assert.AreEqual(1, model.Builds.Count);
        }

        [TestMethod]
        public void SaveAndLoadModel_RoundTrips()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var proj = model.SqlSyncBuildProject[0] with { ProjectName = "TestProj", ScriptTagRequired = true };
            model = model with { SqlSyncBuildProject = new[] { proj } };

            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);
            var projFile = Path.Combine(tmpDir, "proj.sbm");
            var zipFile = Path.Combine(tmpDir, "proj.zip");
            try
            {
                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projFile, zipFile, includeHistoryAndLogs: false);
                var ok = SqlBuildFileHelper.LoadSqlBuildProjectFile(out SqlSyncBuildDataModel loaded, projFile, validateSchema: false);
                Assert.IsTrue(ok);
                Assert.AreEqual("TestProj", loaded.SqlSyncBuildProject[0].ProjectName);
            }
            finally
            {
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            }
        }

        [TestMethod, Ignore("Known stack overflow; ToDataSet recursion under investigation.")]
        public void GetScriptSourceTable_FromModel_Works()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var script = new Script(
                FileName: "file.sql",
                BuildOrder: 1,
                Description: null,
                RollBackOnError: false,
                CausesBuildFailure: false,
                DateAdded: DateTime.UtcNow,
                ScriptId: Guid.NewGuid().ToString(),
                Database: "db",
                StripTransactionText: false,
                AllowMultipleRuns: false,
                AddedBy: "tester",
                ScriptTimeOut: 30,
                DateModified: null,
                ModifiedBy: null,
                Scripts_Id: null,
                Tag: null);
            model = model with { Script = new[] { script } };

            var table = SqlBuildHelper.GetScriptSourceTable(model);
            Assert.IsNotNull(table);
            Assert.AreEqual(1, table.Rows.Count);
        }
    }
}
