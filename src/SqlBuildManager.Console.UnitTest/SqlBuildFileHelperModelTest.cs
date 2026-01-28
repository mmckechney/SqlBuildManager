using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;
using System.IO;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class SqlBuildFileHelperModelTest
    {
        [TestMethod]
        public void CreateShellModel_NotNull()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
        }

        [TestMethod]
        public void SaveLoadModel_RoundTrips()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);
            var projFile = Path.Combine(tmpDir, "proj.sbm");
            var zipFile = Path.Combine(tmpDir, "proj.zip");
            try
            {
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                var oldProj = model.SqlSyncBuildProject[0];
                var proj = new SqlSyncBuildProject(
                    projectName: "Proj1",
                    scriptTagRequired: oldProj.ScriptTagRequired,
                    sqlSyncBuildProjectId: oldProj.SqlSyncBuildProjectId);
                model = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: new[] { proj },
                    script: model.Script,
                    build: model.Build,
                    scriptRun: model.ScriptRun,
                    committedScript: model.CommittedScript,
                    codeReview: model.CodeReview);
                SqlBuildFileHelper.SaveSqlBuildProjectFile(model, projFile, zipFile, includeHistoryAndLogs: false);
                SqlBuildFileHelper.LoadSqlBuildProjectFile(out SqlSyncBuildDataModel loaded, projFile, validateSchema: false);
                Assert.AreEqual("Proj1", loaded.SqlSyncBuildProject[0].ProjectName);
            }
            finally
            {
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            }
        }
    }
}
