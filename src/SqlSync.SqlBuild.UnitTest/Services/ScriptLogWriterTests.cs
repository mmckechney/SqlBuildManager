using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Services;
using System;
using System.IO;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class ScriptLogWriterTests
    {
        [TestMethod]
        public void WriteLog_CreatesLogFileWithHeader_WhenFileDoesNotExist()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var logFile = Path.Combine(tempDir, "test.log");
            
            var writer = new DefaultScriptLogWriter();
            var context = new ScriptLogWriteContext
            {
                ScriptLogFileName = logFile,
                ExternalScriptLogFileName = string.Empty,
                ServerName = "TestServer",
                IsTransactional = true
            };
            var args = new ScriptLogEventArgs(1, "SELECT 1;", "TestDb", "test.sql", "Success", false);

            try
            {
                // Act
                writer.WriteLog(context, false, args);

                // Assert
                Assert.IsTrue(File.Exists(logFile));
                var content = File.ReadAllText(logFile);
                Assert.IsTrue(content.Contains("BEGIN TRANSACTION"));
                Assert.IsTrue(content.Contains("TestServer"));
                Assert.IsTrue(content.Contains("test.sql"));
                Assert.IsTrue(content.Contains("SELECT 1;"));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void WriteLog_WritesNonTransactionalHeader_WhenNotTransactional()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var logFile = Path.Combine(tempDir, "test.log");
            
            var writer = new DefaultScriptLogWriter();
            var context = new ScriptLogWriteContext
            {
                ScriptLogFileName = logFile,
                ExternalScriptLogFileName = string.Empty,
                ServerName = "TestServer",
                IsTransactional = false
            };
            var args = new ScriptLogEventArgs(1, "SELECT 1;", "TestDb", "test.sql", "Success", false);

            try
            {
                // Act
                writer.WriteLog(context, false, args);

                // Assert
                var content = File.ReadAllText(logFile);
                Assert.IsTrue(content.Contains("Executed without a transaction"));
                Assert.IsFalse(content.Contains("BEGIN TRANSACTION"));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void WriteLog_ThrowsNullReferenceException_WhenScriptLogFileNameIsNull()
        {
            // Arrange
            var writer = new DefaultScriptLogWriter();
            var context = new ScriptLogWriteContext
            {
                ScriptLogFileName = null,
                ExternalScriptLogFileName = string.Empty,
                ServerName = "TestServer",
                IsTransactional = true
            };
            var args = new ScriptLogEventArgs(1, "SELECT 1;", "TestDb", "test.sql", "Success", false);

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() => writer.WriteLog(context, false, args));
        }
    }
}
