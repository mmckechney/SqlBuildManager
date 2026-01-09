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
        public void Obsolete_RecordCommittedScripts_UpdatesModel()
        {
            var helper = new SqlBuildHelper(new SqlSync.Connection.ConnectionData());
            var scriptId = Guid.NewGuid();
            var committed = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(scriptId, "HASH", 1, "text", "tag", "server", "db")
            };

            var ok = helper.RecordCommittedScripts(committed);

            Assert.IsTrue(ok);
            Assert.IsNotNull(helper.BuildDataModel);
            Assert.AreEqual(1, helper.BuildDataModel.CommittedScript.Count);
            Assert.AreEqual(scriptId.ToString(), helper.BuildDataModel.CommittedScript[0].ScriptId);
            Assert.AreEqual("server", helper.BuildDataModel.CommittedScript[0].ServerName);
        }
    }
}
