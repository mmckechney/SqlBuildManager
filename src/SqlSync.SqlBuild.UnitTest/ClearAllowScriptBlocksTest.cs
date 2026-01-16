using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest
{
    public class NullProgressReporter : IProgressReporter
    {
        public bool CancellationPending => false;
        public void ReportProgress(int percent, object userState)
        {
            // Do nothing
        }
    }

    [TestClass]
    public class ClearAllowScriptBlocksTest
    {
        [TestMethod]
        public void ClearAllowScriptBlocks_ClearsMatchingServerAndId()
        {
            var model = new SqlSyncBuildDataModel(
                SqlSyncBuildProject: Array.Empty<SqlSyncBuildProject>(),
                Script: Array.Empty<Script>(),
                Build: Array.Empty<Build>(),
                ScriptRun: Array.Empty<ScriptRun>(),
                CommittedScript: new List<CommittedScript>
                {
                    new CommittedScript("id-1","ServerA", DateTime.UtcNow, true, "hash", 1),
                    new CommittedScript("id-2","ServerB", DateTime.UtcNow, true, "hash", 1),
                },
                CodeReview: Array.Empty<CodeReview>()
            );
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null);
            var updated = dbUtil.ClearAllowScriptBlocks(model, "ServerA", new [] { "id-1" });

            Assert.AreEqual(false, updated.CommittedScript[0].AllowScriptBlock);
            Assert.AreEqual(true, updated.CommittedScript[1].AllowScriptBlock);
        }
    }


}
