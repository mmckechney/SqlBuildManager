using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.SqlLogging;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class RecordCommittedScriptsModelTest
    {
        [TestMethod]
        public void RecordCommittedScripts_PocoModel_AppendsCommittedScript()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var conn = new SqlSync.Connection.ConnectionData();
            var helper = new SqlBuildHelper(conn);
            var scriptId = Guid.NewGuid();
            var hash = "HASH";
            var committed = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(scriptId, hash, 1, "text", "tag", "server", "db")
            };

            // Act
            var ok = helper.RecordCommittedScripts(committed, model, out var updated);

            // Assert
            Assert.IsTrue(ok);
            Assert.AreEqual(1, updated.CommittedScript.Count);
            Assert.AreEqual(scriptId.ToString(), updated.CommittedScript[0].ScriptId);
            Assert.AreEqual("server", updated.CommittedScript[0].ServerName);
            Assert.AreEqual(hash, updated.CommittedScript[0].ScriptHash);
        }
    }
}
