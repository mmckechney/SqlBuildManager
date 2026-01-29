using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlSync.Connection;
using System.Collections.Generic;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class ThreadedRunnerTest
    {
        private static CommandLineArgs CreateMinimalCmdArgs()
        {
            var args = new CommandLineArgs();
            args.AuthenticationArgs.UserName = "testuser";
            args.AuthenticationArgs.Password = "testpass";
            args.AuthenticationArgs.AuthenticationType = AuthenticationType.Password;
            args.DacPacArgs.PlatinumDacpac = string.Empty;
            args.IdentityArgs.ClientId = string.Empty;
            args.Trial = false;
            args.Transactional = true;
            args.Description = "Test build";
            args.LogToDatabaseName = string.Empty;
            args.BuildRevision = "1.0";
            args.DefaultScriptTimeout = 30;
            args.AllowObjectDelete = false;
            args.EventHubLogging = System.Array.Empty<EventHubLogging>();
            return args;
        }

        private static List<DatabaseOverride> CreateSingleOverride(string server, string defaultDb, string overrideDb, string tag = "")
        {
            return new List<DatabaseOverride>
            {
                new DatabaseOverride(server, defaultDb, overrideDb) { ConcurrencyTag = tag }
            };
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsServerCorrectly()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.AreEqual("TestServer", runner.Server);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsTargetDatabases()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.AreEqual("OverrideDb", runner.TargetDatabases);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsDefaultDatabaseName()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.AreEqual("DefaultDb", runner.DefaultDatabaseName);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsConcurrencyTag()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb", "TagA");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.AreEqual("TagA", runner.ConcurrencyTag);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsIsTrial_FromCmdArgs()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();
            cmdArgs.Trial = true;

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.IsTrue(runner.IsTrial);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsForceCustomDacpac()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", true);

            // Assert
            Assert.IsTrue(runner.ForceCustomDacpac);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_SetsDacpacName()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();
            cmdArgs.DacPacArgs.PlatinumDacpac = "test.dacpac";

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.AreEqual("test.dacpac", runner.DacpacName);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_WithTagPrefix_ExtractsServerFromOverride()
        {
            // Arrange - when serverName starts with #, it should use server from override
            var overrides = new List<DatabaseOverride>
            {
                new DatabaseOverride("ActualServer", "DefaultDb", "OverrideDb") { ConcurrencyTag = "TagA" }
            };
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("#TagA", overrides, cmdArgs, "testuser", false);

            // Assert - should use server from override, not the tag
            Assert.AreEqual("ActualServer", runner.Server);
        }

        [TestMethod]
        public void ThreadedRunner_Constructor_MultipleOverrides_ConcatenatesTargetDatabases()
        {
            // Arrange
            var overrides = new List<DatabaseOverride>
            {
                new DatabaseOverride("TestServer", "DefaultDb", "Db1"),
                new DatabaseOverride("TestServer", "DefaultDb", "Db2"),
                new DatabaseOverride("TestServer", "DefaultDb", "Db3")
            };
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert - should be semicolon-separated
            Assert.AreEqual("Db1;Db2;Db3", runner.TargetDatabases);
        }

        [TestMethod]
        public void ThreadedRunner_ReturnValue_InitiallyZero()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.AreEqual(0, runner.ReturnValue);
        }

        [TestMethod]
        public void ThreadedRunner_IsTransactional_DefaultsToTrue()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();

            // Act
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Assert
            Assert.IsTrue(runner.IsTransactional);
        }

        [TestMethod]
        public void ThreadedRunner_IsTransactional_CanBeSet()
        {
            // Arrange
            var overrides = CreateSingleOverride("TestServer", "DefaultDb", "OverrideDb");
            var cmdArgs = CreateMinimalCmdArgs();
            var runner = new ThreadedRunner("TestServer", overrides, cmdArgs, "testuser", false);

            // Act
            runner.IsTransactional = false;

            // Assert
            Assert.IsFalse(runner.IsTransactional);
        }
    }
}
