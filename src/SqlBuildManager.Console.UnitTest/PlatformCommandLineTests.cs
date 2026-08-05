using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Connection;
using System;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class PlatformCommandLineTests
    {
        [TestMethod]
        public void ParseArguments_PlatformPostgreSQL_SetsDatabasePlatform()
        {
            string[] args = new string[] {
                "build",
                "--server", "localhost",
                "--database", "mydb",
                "--packagename", "test.sbm",
                "--platform", "PostgreSQL"
            };

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.AreEqual(DatabasePlatform.PostgreSQL, cmdLine.AuthenticationArgs.DatabasePlatform);
        }

        [TestMethod]
        public void ParseArguments_PlatformSqlServer_SetsDatabasePlatform()
        {
            string[] args = new string[] {
                "build",
                "--server", "localhost",
                "--database", "mydb",
                "--packagename", "test.sbm",
                "--platform", "SqlServer"
            };

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.AreEqual(DatabasePlatform.SqlServer, cmdLine.AuthenticationArgs.DatabasePlatform);
        }

        [TestMethod]
        public void ParseArguments_NoPlatform_DefaultsToSqlServer()
        {
            string[] args = new string[] {
                "build",
                "--server", "localhost",
                "--database", "mydb",
                "--packagename", "test.sbm"
            };

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.AreEqual(DatabasePlatform.SqlServer, cmdLine.AuthenticationArgs.DatabasePlatform);
        }

        [TestMethod]
        public void ParseArguments_ThreadedRun_PlatformPostgreSQL_SetsDatabasePlatform()
        {
            string[] args = new string[] {
                "threaded", "run",
                "--override", "test.cfg",
                "--packagename", "test.sbm",
                "--platform", "PostgreSQL"
            };

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.AreEqual(DatabasePlatform.PostgreSQL, cmdLine.AuthenticationArgs.DatabasePlatform);
        }

        [TestMethod]
        public void ParseArguments_PlatformPostgreSQL_WithAuth_BothParsed()
        {
            string[] args = new string[] {
                "build",
                "--server", "localhost",
                "--database", "mydb",
                "--packagename", "test.sbm",
                "--platform", "PostgreSQL",
                "--authtype", "Password",
                "--username", "pguser",
                "--password", "pgpass"
            };

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.AreEqual(DatabasePlatform.PostgreSQL, cmdLine.AuthenticationArgs.DatabasePlatform);
            Assert.AreEqual(AuthenticationType.Password, cmdLine.AuthenticationArgs.AuthenticationType);
            Assert.AreEqual("pguser", cmdLine.AuthenticationArgs.UserName);
            Assert.AreEqual("pgpass", cmdLine.AuthenticationArgs.Password);
        }

        [TestMethod]
        public void ParseArguments_PlatformMySQL_SetsDatabasePlatform()
        {
            string[] args = new string[] {
                "build",
                "--server", "localhost",
                "--database", "mydb",
                "--packagename", "test.sbm",
                "--platform", "MySQL"
            };

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.AreEqual(DatabasePlatform.MySQL, cmdLine.AuthenticationArgs.DatabasePlatform);
        }

        [TestMethod]
        public void ToArgs_KeyVault_PreservesNonSqlServerDatabasePlatform()
        {
            var cmdLine = new CommandLineArgs
            {
                KeyVaultName = "test-vault"
            };
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;

            var args = cmdLine.ToArgs();

            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--databaseplatform").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "\"PostgreSQL\"").Length);
        }

        [TestMethod]
        public void ToArgs_WithoutKeyVault_DoesNotDuplicateNonSqlServerDatabasePlatform()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;

            var args = cmdLine.ToArgs();

            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--databaseplatform").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "\"PostgreSQL\"").Length);
        }

        [TestMethod]
        public void ToArgs_KeyVault_PreservesNonSqlServerDatabasePlatform_Batch()
        {
            var cmdLine = new CommandLineArgs
            {
                KeyVaultName = "test-vault"
            };
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;

            var args = cmdLine.ToArgs(StringType.Batch);

            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--databaseplatform").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "\"PostgreSQL\"").Length);
        }

        [TestMethod]
        public void ToArgs_WithoutKeyVault_DoesNotDuplicateNonSqlServerDatabasePlatform_Batch()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;

            var args = cmdLine.ToArgs(StringType.Batch);

            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--databaseplatform").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "\"PostgreSQL\"").Length);
        }

        [TestMethod]
        public void ToArgs_WithoutKeyVault_TrustedCertDoesNotDuplicate()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;
            cmdLine.AuthenticationArgs.TrustServerCertificate = true;

            var args = cmdLine.ToArgs();

            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--databaseplatform").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--trustservercertificate").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "\"PostgreSQL\"").Length);
        }

        [TestMethod]
        public void ToArgs_KeyVault_TrustedCertDoesNotDuplicate_Batch()
        {
            var cmdLine = new CommandLineArgs
            {
                KeyVaultName = "test-vault"
            };
            cmdLine.AuthenticationArgs.DatabasePlatform = DatabasePlatform.PostgreSQL;
            cmdLine.AuthenticationArgs.TrustServerCertificate = true;

            var args = cmdLine.ToArgs(StringType.Batch);

            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--trustservercertificate").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "--databaseplatform").Length);
            Assert.AreEqual(1, Array.FindAll(args, arg => arg == "\"PostgreSQL\"").Length);
        }
    }
}
