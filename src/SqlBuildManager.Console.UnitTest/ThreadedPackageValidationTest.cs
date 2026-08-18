using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class ThreadedPackageValidationTest
    {
        [TestMethod]
        public void GetMissingReferencedScripts_ReturnsOnlyMissingFiles()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"sbm-package-validation-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllText(Path.Combine(directory, "present.sql"), "SELECT 1");
                var model = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                    script: new List<Script>
                    {
                        new Script("present.sql", 1, string.Empty, true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "db", false, false, "user", 30, null, null, null),
                        new Script("missing.sql", 2, string.Empty, true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "db", false, false, "user", 30, null, null, null)
                    },
                    build: new List<Build>(),
                    scriptRun: new List<ScriptRun>(),
                    committedScript: new List<CommittedScript>());

                var missing = ThreadedManager.GetMissingReferencedScripts(model, directory);

                CollectionAssert.AreEqual(new[] { "missing.sql" }, new List<string>(missing));
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
