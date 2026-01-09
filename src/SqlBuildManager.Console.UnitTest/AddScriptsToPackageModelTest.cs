using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console;
using SqlBuildManager.Console.CommandLine;
using sqlB = SqlSync.SqlBuild;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class AddScriptsToPackageModelTest
    {
        [TestMethod]
        public void AddScriptsToPackage_PocoPipeline_AddsScripts()
        {
            var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            var script1 = Path.Combine(tempDir.FullName, "s1.sql");
            File.WriteAllText(script1, "SELECT 1;");
            var outputSbm = Path.Combine(tempDir.FullName, "pkg.sbx");

            File.WriteAllText(outputSbm, string.Empty);

            var args = new CommandLineArgs
            {
                OutputSbm = outputSbm,
                Scripts = new[] { new FileInfo(script1) }
            };

            var rc = Worker.AddScriptsToPackage(args);

            Assert.AreEqual(0, rc);
            Assert.IsTrue(File.Exists(outputSbm));
            // quick check: load as POCO
            var model = sqlB.SqlSyncBuildDataXmlSerializer.Load(outputSbm);
            Assert.AreEqual(1, model.Script.Count);
            Assert.AreEqual("s1.sql", model.Script[0].FileName);
        }
    }
}
