using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildFileHelperAsyncTests
    {
        [TestMethod]
        public async Task GetSHA1HashAsync_MatchesSyncBehavior()
        {
            var tmp = Path.GetTempFileName();
            try
            {
                var content = "SELECT 1;\nGO\nSELECT 2;";
                await File.WriteAllTextAsync(tmp, content);

                SqlBuildFileHelper.GetSHA1Hash(tmp, out var fileHashSync, out var textHashSync, stripTransactions: false);
                var (fileHashAsync, textHashAsync) = await SqlBuildFileHelper.GetSHA1HashAsync(tmp, stripTransactions: false);

                Assert.AreEqual(fileHashSync, fileHashAsync);
                Assert.AreEqual(textHashSync, textHashAsync);
            }
            finally
            {
                File.Delete(tmp);
            }
        }

        [TestMethod]
        public async Task SaveSqlBuildProjectFileAsync_CreatesZip()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpDir);
            try
            {
                var scriptPath = Path.Combine(tmpDir, "one.sql");
                await File.WriteAllTextAsync(scriptPath, "SELECT 1;");

                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                model = SqlBuildFileHelper.AddScriptFileToBuild(
                    model,
                    projFileName: Path.Combine(tmpDir, "build.xml"),
                    fileName: Path.GetFileName(scriptPath),
                    buildOrder: 1,
                    description: "",
                    rollBackScript: true,
                    rollBackBuild: true,
                    databaseName: "db",
                    stripTransactions: false,
                    buildZipFileName: Path.Combine(tmpDir, "build.sbm"),
                    saveToZip: false,
                    allowMultipleRuns: true,
                    addedBy: "tester",
                    scriptTimeOut: 30,
                    scriptId: Guid.NewGuid(),
                    tag: "");

                var projFile = Path.Combine(tmpDir, "build.xml");
                var zipFile = Path.Combine(tmpDir, "build.sbm");

                await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, projFile, zipFile);

                Assert.IsTrue(File.Exists(zipFile));
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }
    }
}
