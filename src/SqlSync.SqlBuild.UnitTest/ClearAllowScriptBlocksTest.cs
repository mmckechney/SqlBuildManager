using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class ClearAllowScriptBlocksTest
    {
        [TestMethod]
        public void ClearAllowScriptBlocks_ClearsMatchingServerAndId()
        {
            var model = new SqlSyncBuildDataModel(
                SqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                Scripts: Array.Empty<Scripts>(),
                Script: Array.Empty<Script>(),
                Builds: Array.Empty<Builds>(),
                Build: Array.Empty<Build>(),
                ScriptRun: Array.Empty<ScriptRun>(),
                CommittedScript: new List<CommittedScript>
                {
                    new CommittedScript("id-1","ServerA", DateTime.UtcNow, true, "hash", 1),
                    new CommittedScript("id-2","ServerB", DateTime.UtcNow, true, "hash", 1),
                },
                CodeReview: Array.Empty<CodeReview>()
            );

            var updated = SqlBuildHelper.ClearAllowScriptBlocks(model, "ServerA", new [] { "id-1" });

            Assert.AreEqual(false, updated.CommittedScript[0].AllowScriptBlock);
            Assert.AreEqual(true, updated.CommittedScript[1].AllowScriptBlock);
        }
    }
}
