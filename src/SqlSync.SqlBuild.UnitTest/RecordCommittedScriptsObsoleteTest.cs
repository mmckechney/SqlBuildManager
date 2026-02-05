using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class RecordCommittedScriptsObsoleteTest
    {
        [TestMethod]
        public void RecordCommittedScripts_UpdatesModel()
        {
            var helper = new SqlBuildHelper(new SqlSync.Connection.ConnectionData());
            var scriptId = Guid.NewGuid();
            var committed = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(scriptId, "HASH", 1, "text", "tag", "server", "db")
            };

            SqlSyncBuildDataModel ok = helper.BuildFinalizer.RecordCommittedScripts(committed, ((ISqlBuildRunnerProperties)helper).BuildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());

            Assert.IsNotNull(ok);
            Assert.AreEqual(1, ok.CommittedScript.Count);
            Assert.AreEqual(scriptId.ToString(), ok.CommittedScript[0].ScriptId);
            Assert.AreEqual("server", ok.CommittedScript[0].ServerName);
        }
    }
}
