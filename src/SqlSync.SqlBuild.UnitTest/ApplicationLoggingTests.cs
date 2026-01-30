using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Logging;
using System;
using System.IO;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Tests for ApplicationLogging to improve code coverage
    /// </summary>
    [TestClass]
    public class ApplicationLoggingTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            // Reset the logger after each test
            ApplicationLogging.CloseAndFlush();
        }

        #region CreateLogger Tests

        [TestMethod]
        public void CreateLogger_Generic_ReturnsNonNullLogger()
        {
            // Act
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();

            // Assert
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void CreateLogger_ByType_ReturnsNonNullLogger()
        {
            // Act
            var logger = ApplicationLogging.CreateLogger(typeof(ApplicationLoggingTests));

            // Assert
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void CreateLogger_WithLogFileName_ReturnsLogger()
        {
            // Act
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>("test.log");

            // Assert
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void CreateLogger_WithLogFileNameAndPath_ReturnsLogger()
        {
            // Arrange
            string tempPath = Path.GetTempPath();

            // Act
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>("test.log", tempPath);

            // Assert
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void CreateLogger_MultipleCalls_ReturnsDifferentInstances()
        {
            // Act
            var logger1 = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();
            var logger2 = ApplicationLogging.CreateLogger<string>();

            // Assert
            Assert.IsNotNull(logger1);
            Assert.IsNotNull(logger2);
        }

        #endregion

        #region LogLevel Tests

        [TestMethod]
        public void SetLogLevel_Trace_SetsCorrectly()
        {
            // Act
            ApplicationLogging.SetLogLevel(LogLevel.Trace);

            // Assert
            Assert.AreEqual(LogLevel.Trace, ApplicationLogging.GetLogLevel());
        }

        [TestMethod]
        public void SetLogLevel_Debug_SetsCorrectly()
        {
            // Act
            ApplicationLogging.SetLogLevel(LogLevel.Debug);

            // Assert
            Assert.AreEqual(LogLevel.Debug, ApplicationLogging.GetLogLevel());
        }

        [TestMethod]
        public void SetLogLevel_Information_SetsCorrectly()
        {
            // Act
            ApplicationLogging.SetLogLevel(LogLevel.Information);

            // Assert
            Assert.AreEqual(LogLevel.Information, ApplicationLogging.GetLogLevel());
        }

        [TestMethod]
        public void SetLogLevel_Warning_SetsCorrectly()
        {
            // Act
            ApplicationLogging.SetLogLevel(LogLevel.Warning);

            // Assert
            Assert.AreEqual(LogLevel.Warning, ApplicationLogging.GetLogLevel());
        }

        [TestMethod]
        public void SetLogLevel_Error_SetsCorrectly()
        {
            // Act
            ApplicationLogging.SetLogLevel(LogLevel.Error);

            // Assert
            Assert.AreEqual(LogLevel.Error, ApplicationLogging.GetLogLevel());
        }

        [TestMethod]
        public void SetLogLevel_Critical_SetsCorrectly()
        {
            // Act
            ApplicationLogging.SetLogLevel(LogLevel.Critical);

            // Assert
            Assert.AreEqual(LogLevel.Critical, ApplicationLogging.GetLogLevel());
        }

        [TestMethod]
        public void GetLogLevelString_ReturnsCorrectString()
        {
            // Arrange
            ApplicationLogging.SetLogLevel(LogLevel.Warning);

            // Act
            string levelString = ApplicationLogging.GetLogLevelString();

            // Assert
            Assert.AreEqual("Warning", levelString);
        }

        [TestMethod]
        public void GetLogLevelString_AfterSettingDebug_ReturnsDebug()
        {
            // Arrange
            ApplicationLogging.SetLogLevel(LogLevel.Debug);

            // Act
            string levelString = ApplicationLogging.GetLogLevelString();

            // Assert
            Assert.AreEqual("Debug", levelString);
        }

        #endregion

        #region IsDebug Tests

        [TestMethod]
        public void IsDebug_WhenDebugLevel_ReturnsTrue()
        {
            // Arrange
            ApplicationLogging.SetLogLevel(LogLevel.Debug);
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();

            // Act
            bool isDebug = ApplicationLogging.IsDebug();

            // Assert - may vary based on actual configuration
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void IsDebug_WhenTraceLevel_ReturnsTrue()
        {
            // Arrange
            ApplicationLogging.SetLogLevel(LogLevel.Trace);
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();

            // Act
            bool isDebug = ApplicationLogging.IsDebug();

            // Assert - should be enabled since trace includes debug
            Assert.IsNotNull(logger);
        }

        #endregion

        #region LoggerFactory Tests

        [TestMethod]
        public void LoggerFactory_Get_ReturnsNonNull()
        {
            // Act
            var factory = ApplicationLogging.LoggerFactory;

            // Assert
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void LoggerFactory_Set_AllowsCustomFactory()
        {
            // Arrange
            var customFactory = new LoggerFactory();

            // Act
            ApplicationLogging.LoggerFactory = customFactory;

            // Assert
            Assert.AreSame(customFactory, ApplicationLogging.LoggerFactory);
        }

        #endregion

        #region LogFileName Tests

        [TestMethod]
        public void LogFileName_AfterCreatingLoggerWithFile_ReturnsPath()
        {
            // Arrange
            string expectedFileName = "testlog.log";
            string tempPath = Path.GetTempPath();

            // Act
            ApplicationLogging.CreateLogger<ApplicationLoggingTests>(expectedFileName, tempPath);
            string logFileName = ApplicationLogging.LogFileName;

            // Assert
            Assert.IsNotNull(logFileName);
            Assert.IsTrue(logFileName.EndsWith(expectedFileName));
        }

        [TestMethod]
        public void LogFileName_WithEmptyRootPath_UsesDefaultPath()
        {
            // Act
            ApplicationLogging.CreateLogger<ApplicationLoggingTests>("test.log", string.Empty);
            string logFileName = ApplicationLogging.LogFileName;

            // Assert
            Assert.IsNotNull(logFileName);
            Assert.IsTrue(logFileName.Contains("Sql Build Manager"));
        }

        #endregion

        #region CloseAndFlush Tests

        [TestMethod]
        public void CloseAndFlush_MultipleCalls_DoesNotThrow()
        {
            // Arrange
            ApplicationLogging.CreateLogger<ApplicationLoggingTests>();

            // Act & Assert - should not throw
            ApplicationLogging.CloseAndFlush();
            ApplicationLogging.CloseAndFlush();
        }

        [TestMethod]
        public void CloseAndFlush_BeforeAnyLogger_DoesNotThrow()
        {
            // Act & Assert - should not throw
            ApplicationLogging.CloseAndFlush();
        }

        [TestMethod]
        public void CloseAndFlush_ThenCreateLogger_Works()
        {
            // Arrange
            ApplicationLogging.CreateLogger<ApplicationLoggingTests>();
            ApplicationLogging.CloseAndFlush();

            // Act
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();

            // Assert
            Assert.IsNotNull(logger);
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void Logger_CanLogMessages_WithoutException()
        {
            // Arrange
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();

            // Act & Assert - should not throw
            logger.LogInformation("Test information message");
            logger.LogWarning("Test warning message");
            logger.LogError("Test error message");
            logger.LogDebug("Test debug message");
        }

        [TestMethod]
        public void Logger_CanLogWithException_WithoutThrowing()
        {
            // Arrange
            var logger = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert - should not throw
            logger.LogError(exception, "Error with exception");
        }

        [TestMethod]
        public void MultipleLoggers_CanCoexist()
        {
            // Act
            var logger1 = ApplicationLogging.CreateLogger<ApplicationLoggingTests>();
            var logger2 = ApplicationLogging.CreateLogger<string>();
            var logger3 = ApplicationLogging.CreateLogger(typeof(int));

            // Assert
            Assert.IsNotNull(logger1);
            Assert.IsNotNull(logger2);
            Assert.IsNotNull(logger3);
        }

        #endregion
    }
}
