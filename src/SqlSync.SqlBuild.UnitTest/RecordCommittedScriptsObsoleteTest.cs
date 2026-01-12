using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlSync.SqlBuild.Models;

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

            var ok = helper.RecordCommittedScripts(committed, helper.BuildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(), out var updatedModel);

            Assert.IsTrue(ok);
            Assert.IsNotNull(updatedModel);
            Assert.AreEqual(1, updatedModel.CommittedScript.Count);
            Assert.AreEqual(scriptId.ToString(), updatedModel.CommittedScript[0].ScriptId);
            Assert.AreEqual("server", updatedModel.CommittedScript[0].ServerName);
        }
    }
}
