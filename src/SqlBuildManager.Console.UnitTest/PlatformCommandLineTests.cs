using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;

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
    }
}
