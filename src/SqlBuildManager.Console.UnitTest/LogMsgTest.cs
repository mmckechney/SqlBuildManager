using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using System.Text.Json;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class LogMsgTest
    {
        [TestMethod]
        public void LogMsg_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var msg = new LogMsg();

            // Assert
            Assert.AreEqual(string.Empty, msg.JobName);
            Assert.AreEqual(string.Empty, msg.Message);
            Assert.AreEqual(string.Empty, msg.DatabaseName);
            Assert.AreEqual(string.Empty, msg.ServerName);
            Assert.AreEqual(string.Empty, msg.ComputeHostName);
            Assert.AreEqual(string.Empty, msg.ConcurrencyTag);
            Assert.AreEqual(LogType.Message, msg.LogType);
            Assert.IsNull(msg.TypeTag);
            Assert.IsNull(msg.ScriptLog);
        }

        [TestMethod]
        public void LogMsg_SetProperties_RetainsValues()
        {
            // Arrange
            var msg = new LogMsg
            {
                JobName = "TestJob",
                Message = "Test message",
                DatabaseName = "TestDb",
                ServerName = "TestServer",
                ComputeHostName = "TestHost",
                ConcurrencyTag = "TagA",
                LogType = LogType.Commit,
                TypeTag = "CustomTag",
                RunId = "run123"
            };

            // Assert
            Assert.AreEqual("TestJob", msg.JobName);
            Assert.AreEqual("Test message", msg.Message);
            Assert.AreEqual("TestDb", msg.DatabaseName);
            Assert.AreEqual("TestServer", msg.ServerName);
            Assert.AreEqual("TestHost", msg.ComputeHostName);
            Assert.AreEqual("TagA", msg.ConcurrencyTag);
            Assert.AreEqual(LogType.Commit, msg.LogType);
            Assert.AreEqual("CustomTag", msg.TypeTag);
            Assert.AreEqual("run123", msg.RunId);
        }

        [TestMethod]
        public void LogMsg_RunId_WhenSet_ReturnsSetValue()
        {
            // Arrange
            var msg = new LogMsg { RunId = "explicit-run-id" };

            // Act & Assert
            Assert.AreEqual("explicit-run-id", msg.RunId);
        }

        [TestMethod]
        public void LogMsg_SourceDacPac_ExtractsFileName()
        {
            // Arrange
            var msg = new LogMsg();

            // Act
            msg.SourceDacPac = @"C:\path\to\my.dacpac";

            // Assert
            Assert.AreEqual("my.dacpac", msg.SourceDacPac);
        }

        [TestMethod]
        public void LogMsg_SourceDacPac_HandlesJustFileName()
        {
            // Arrange
            var msg = new LogMsg();

            // Act
            msg.SourceDacPac = "simple.dacpac";

            // Assert
            Assert.AreEqual("simple.dacpac", msg.SourceDacPac);
        }

        [TestMethod]
        public void LogMsg_Serialization_ProducesCorrectJson()
        {
            // Arrange
            var msg = new LogMsg
            {
                JobName = "Job1",
                Message = "Test",
                DatabaseName = "DB1",
                ServerName = "Server1",
                LogType = LogType.Error,
                RunId = "run-abc"
            };

            // Act
            var json = JsonSerializer.Serialize(msg);

            // Assert
            Assert.IsTrue(json.Contains("\"JobName\":\"Job1\""));
            Assert.IsTrue(json.Contains("\"Message\":\"Test\""));
            Assert.IsTrue(json.Contains("\"DatabaseName\":\"DB1\""));
            Assert.IsTrue(json.Contains("\"ServerName\":\"Server1\""));
            Assert.IsTrue(json.Contains("\"LogType\":\"Error\""));
            Assert.IsTrue(json.Contains("\"RunId\":\"run-abc\""));
        }

        [TestMethod]
        public void LogMsg_Deserialization_RestoresObject()
        {
            // Arrange
            var json = @"{""JobName"":""TestJob"",""Message"":""Hello"",""LogType"":""Commit"",""RunId"":""xyz""}";

            // Act
            var msg = JsonSerializer.Deserialize<LogMsg>(json);

            // Assert
            Assert.IsNotNull(msg);
            Assert.AreEqual("TestJob", msg.JobName);
            Assert.AreEqual("Hello", msg.Message);
            Assert.AreEqual(LogType.Commit, msg.LogType);
            Assert.AreEqual("xyz", msg.RunId);
        }

        [TestMethod]
        public void LogMsg_AllLogTypes_SerializeCorrectly()
        {
            // Test each LogType serializes to correct string
            var logTypes = new[]
            {
                LogType.Message,
                LogType.Commit,
                LogType.Error,
                LogType.FailureDatabases,
                LogType.SuccessDatabases,
                LogType.ScriptLog,
                LogType.ScriptError,
                LogType.WorkerCompleted
            };

            foreach (var logType in logTypes)
            {
                var msg = new LogMsg { LogType = logType };
                var json = JsonSerializer.Serialize(msg);
                Assert.IsTrue(json.Contains($"\"LogType\":\"{logType}\""), $"Failed for {logType}");
            }
        }
    }

    [TestClass]
    public class ScriptLogDataTest
    {
        [TestMethod]
        public void ScriptLogData_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var data = new ScriptLogData();

            // Assert
            Assert.AreEqual(-1, data.ScriptIndex);
            Assert.AreEqual(string.Empty, data.ScriptFileName);
            Assert.AreEqual(string.Empty, data.ScriptText);
            Assert.AreEqual(string.Empty, data.Result);
        }

        [TestMethod]
        public void ScriptLogData_SetProperties_RetainsValues()
        {
            // Arrange & Act
            var data = new ScriptLogData
            {
                ScriptIndex = 5,
                ScriptFileName = "test.sql",
                ScriptText = "SELECT 1",
                Result = "Success"
            };

            // Assert
            Assert.AreEqual(5, data.ScriptIndex);
            Assert.AreEqual("test.sql", data.ScriptFileName);
            Assert.AreEqual("SELECT 1", data.ScriptText);
            Assert.AreEqual("Success", data.Result);
        }

        [TestMethod]
        public void ScriptLogData_Serialization_ProducesCorrectJson()
        {
            // Arrange
            var data = new ScriptLogData
            {
                ScriptIndex = 10,
                ScriptFileName = "script.sql",
                ScriptText = "UPDATE t SET x = 1",
                Result = "Rows affected: 5"
            };

            // Act
            var json = JsonSerializer.Serialize(data);

            // Assert
            Assert.IsTrue(json.Contains("\"ScriptIndex\":10"));
            Assert.IsTrue(json.Contains("\"ScriptFileName\":\"script.sql\""));
            Assert.IsTrue(json.Contains("\"ScriptText\":\"UPDATE t SET x = 1\""));
            Assert.IsTrue(json.Contains("\"Result\":\"Rows affected: 5\""));
        }

        [TestMethod]
        public void LogMsg_WithScriptLogData_SerializesNested()
        {
            // Arrange
            var msg = new LogMsg
            {
                Message = "Script execution",
                LogType = LogType.ScriptLog,
                ScriptLog = new ScriptLogData
                {
                    ScriptIndex = 1,
                    ScriptFileName = "init.sql",
                    ScriptText = "CREATE TABLE Test (Id INT)",
                    Result = "OK"
                }
            };

            // Act
            var json = JsonSerializer.Serialize(msg);

            // Assert
            Assert.IsTrue(json.Contains("\"ScriptLog\":{"));
            Assert.IsTrue(json.Contains("\"ScriptIndex\":1"));
            Assert.IsTrue(json.Contains("\"ScriptFileName\":\"init.sql\""));
        }
    }
}
