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
            SqlSyncBuildDataModel ok = helper.BuildFinalizer.RecordCommittedScripts(committed, model);

            // Assert
            Assert.IsNotNull(ok);
            Assert.AreEqual(1, ok.CommittedScript.Count);
            Assert.AreEqual(scriptId.ToString(), ok.CommittedScript[0].ScriptId);
            Assert.AreEqual("server", ok.CommittedScript[0].ServerName);
            Assert.AreEqual(hash, ok.CommittedScript[0].ScriptHash);
        }
    }
}
