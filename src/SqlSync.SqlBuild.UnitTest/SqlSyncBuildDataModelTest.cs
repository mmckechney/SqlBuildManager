using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;
using System.IO;
using System.Threading.Tasks;

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
            Assert.AreEqual(0, model.Script.Count);
            Assert.AreEqual(0, model.Build.Count);
        }

        [TestMethod]
        public async Task SaveAndLoadModel_RoundTrips()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var oldProj = model.SqlSyncBuildProject[0];
            var proj = new SqlSyncBuildProject(
                projectName: "TestProj",
                scriptTagRequired: true,
                sqlSyncBuildProjectId: oldProj.SqlSyncBuildProjectId);
            model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new[] { proj },
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: model.CommittedScript);

            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);
            var projFile = Path.Combine(tmpDir, "proj.sbm");
            var zipFile = Path.Combine(tmpDir, "proj.zip");
            try
            {
                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projFile, zipFile, includeHistoryAndLogs: false);
                var (ok, loaded) = await SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(projFile, validateSchema: false);
                Assert.IsTrue(ok);
                Assert.AreEqual("TestProj", loaded.SqlSyncBuildProject[0].ProjectName);
            }
            finally
            {
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            }
        }
    }
}
