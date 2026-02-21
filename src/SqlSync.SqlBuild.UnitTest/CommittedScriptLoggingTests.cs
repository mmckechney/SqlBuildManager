using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.SqlLogging;
using System;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Tests for CommittedScript (SqlLogging version)
    /// </summary>
    [TestClass]
    public class CommittedScriptLoggingTests
    {
        #region Constructor Tests

        [TestMethod]
        public void CommittedScript_Constructor_SetsAllProperties()
        {
            var scriptId = Guid.NewGuid();
            var fileHash = "ABC123";
            var sequence = 5;
            var scriptText = "SELECT 1;";
            var tag = "v1.0";
            var serverName = "TestServer";
            var databaseTarget = "TestDb";

            var script = new CommittedScript(scriptId, fileHash, sequence, scriptText, tag, serverName, databaseTarget);

            Assert.AreEqual(scriptId, script.ScriptId);
            Assert.AreEqual(fileHash, script.FileHash);
            Assert.AreEqual(sequence, script.Sequence);
            Assert.AreEqual(scriptText, script.ScriptText);
            Assert.AreEqual(tag, script.Tag);
            Assert.AreEqual(serverName, script.ServerName);
            Assert.AreEqual(databaseTarget, script.DatabaseTarget);
        }

        [TestMethod]
        public void CommittedScript_RunStartRunEnd_CanBeSet()
        {
            var script = new CommittedScript(Guid.NewGuid(), "hash", 1, "script", "tag", "server", "db");
            var start = new DateTime(2024, 1, 15, 10, 0, 0);
            var end = new DateTime(2024, 1, 15, 10, 0, 30);

            script.RunStart = start;
            script.RunEnd = end;

            Assert.AreEqual(start, script.RunStart);
            Assert.AreEqual(end, script.RunEnd);
        }

        [TestMethod]
        public void CommittedScript_Fields_AreReadonly()
        {
            var scriptId = Guid.NewGuid();
            var script = new CommittedScript(scriptId, "hash", 1, "script", "tag", "server", "db");

            // Readonly fields cannot be changed after construction
            Assert.AreEqual(scriptId, script.ScriptId);
            Assert.AreEqual("hash", script.FileHash);
        }

        [TestMethod]
        public void CommittedScript_WithNullValues_Accepted()
        {
            var script = new CommittedScript(Guid.Empty, null!, 0, null!, null!, null!, null!);

            Assert.AreEqual(Guid.Empty, script.ScriptId);
            Assert.IsNull(script.FileHash);
            Assert.AreEqual(0, script.Sequence);
            Assert.IsNull(script.ScriptText);
            Assert.IsNull(script.Tag);
            Assert.IsNull(script.ServerName);
            Assert.IsNull(script.DatabaseTarget);
        }

        [TestMethod]
        public void CommittedScript_WithEmptyStrings_Accepted()
        {
            var script = new CommittedScript(Guid.NewGuid(), string.Empty, 1, string.Empty, string.Empty, string.Empty, string.Empty);

            Assert.AreEqual(string.Empty, script.FileHash);
            Assert.AreEqual(string.Empty, script.ScriptText);
            Assert.AreEqual(string.Empty, script.Tag);
            Assert.AreEqual(string.Empty, script.ServerName);
            Assert.AreEqual(string.Empty, script.DatabaseTarget);
        }

        [TestMethod]
        public void CommittedScript_Sequence_CanBeZeroOrNegative()
        {
            var scriptZero = new CommittedScript(Guid.NewGuid(), "hash", 0, "script", "tag", "server", "db");
            var scriptNegative = new CommittedScript(Guid.NewGuid(), "hash", -1, "script", "tag", "server", "db");

            Assert.AreEqual(0, scriptZero.Sequence);
            Assert.AreEqual(-1, scriptNegative.Sequence);
        }

        [TestMethod]
        public void CommittedScript_RunStartRunEnd_DefaultToMinValue()
        {
            var script = new CommittedScript(Guid.NewGuid(), "hash", 1, "script", "tag", "server", "db");

            // DateTime defaults to DateTime.MinValue
            Assert.AreEqual(DateTime.MinValue, script.RunStart);
            Assert.AreEqual(DateTime.MinValue, script.RunEnd);
        }

        #endregion
    }
}
