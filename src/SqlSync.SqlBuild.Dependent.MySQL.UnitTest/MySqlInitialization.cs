using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using SqlSync.Connection;
using SqlSync.SqlBuild.Dependent.TestBase;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.IO;

namespace SqlSync.SqlBuild.Dependent.MySQL.UnitTest
{
    /// <summary>
    /// MySQL-specific initialization helper. Extends the shared InitializationBase
    /// with MySqlConnector connections and MySQL DDL/DML.
    /// Configure via environment variables:
    ///   SBM_TEST_MYSQL_SERVER (default: localhost)
    ///   SBM_TEST_MYSQL_USER (default: root)
    ///   SBM_TEST_MYSQL_PASSWORD (default: MySq1Adm!n)
    /// </summary>
    public class MySqlInitialization : InitializationBase
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        public string mySqlUser;
        public string mySqlPassword;

        public static string MissingDatabaseErrorMessage = "NOTE: To successfully test, please make sure you have a MySQL instance running. Set SBM_TEST_MYSQL_SERVER, SBM_TEST_MYSQL_USER, SBM_TEST_MYSQL_PASSWORD environment variables.";

        protected override string TestTableName => "transactiontest";

        private static string GetServerName() => Environment.GetEnvironmentVariable("SBM_TEST_MYSQL_SERVER") ?? "localhost";
        private static string GetUser() => Environment.GetEnvironmentVariable("SBM_TEST_MYSQL_USER") ?? "root";
        private static string GetPassword() => Environment.GetEnvironmentVariable("SBM_TEST_MYSQL_PASSWORD") ?? "MySq1Adm!n";

        public MySqlInitialization()
        {
            serverName = GetServerName();
            mySqlUser = GetUser();
            mySqlPassword = GetPassword();

            testDatabaseNames = new System.Collections.Generic.List<string>
            {
                "sbm_mysql_test",
                "sbm_mysql_test1",
                "sbm_mysql_test2",
                "sbm_mysql_test3"
            };

            connData = new ConnectionData()
            {
                SQLServerName = serverName,
                DatabaseName = testDatabaseNames[0],
                UserId = mySqlUser,
                Password = mySqlPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.MySQL,
                ScriptTimeout = 20
            };

            if (!CreateDatabases())
                Assert.Fail(string.Format("Unable to create the target MySQL databases. {0}", MissingDatabaseErrorMessage));
            if (!CreateTestTables())
                Assert.Fail("Unable to create the target MySQL tables");
            if (!CreateSqlBuildLoggingTables())
                Assert.Fail("Unable to create the sqlbuild_logging tables in MySQL");
            if (!CleanTestTables())
                Assert.Fail("Unable to clean pre-existing data from the MySQL target tables");
            if (!CleanSqlBuildLoggingTables())
                Assert.Fail("Unable to clean pre-existing data from the MySQL sqlbuild_logging tables");
        }

        private string GetAdminConnectionString()
        {
            return $"Host={serverName};Database=mysql;Username={mySqlUser};Password={mySqlPassword};Connection Timeout=20";
        }

        private string GetConnectionString(string dbName)
        {
            return $"Host={serverName};Database={dbName};Username={mySqlUser};Password={mySqlPassword};Connection Timeout=20";
        }

        public override bool CreateDatabases()
        {
            try
            {
                using var adminConn = new MySqlConnection(GetAdminConnectionString());
                adminConn.Open();

                foreach (string dbName in testDatabaseNames)
                {
                    using var checkCmd = new MySqlCommand("SELECT 1 FROM information_schema.schemata WHERE schema_name = @name", adminConn);
                    checkCmd.Parameters.AddWithValue("@name", dbName);
                    object? val = checkCmd.ExecuteScalar();

                    if (val == null || val == DBNull.Value)
                    {
                        using var createCmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{dbName}`", adminConn);
                        createCmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create MySQL test databases");
                return false;
            }
        }

        public override bool CreateTestTables()
        {
            try
            {
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS transactiontest (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        message VARCHAR(500) NULL,
                        guid CHAR(36) NULL,
                        datetimestamp DATETIME NULL
                    )";

                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new MySqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new MySqlCommand(createTableSql, conn);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create MySQL test tables");
                return false;
            }
        }

        public override bool CreateSqlBuildLoggingTables()
        {
            try
            {
                var resourceProvider = new MySqlResourceProvider();

                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new MySqlConnection(GetConnectionString(dbName));
                    conn.Open();

                    using (var cmd = new MySqlCommand(resourceProvider.LoggingTableDdl, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new MySqlCommand(resourceProvider.LoggingTableCommitCheckIndex, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create MySQL sqlbuild_logging tables");
                return false;
            }
        }

        public override bool CleanTestTables()
        {
            try
            {
                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new MySqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new MySqlCommand("DELETE FROM transactiontest WHERE datetimestamp < @cutoff", conn);
                    cmd.Parameters.AddWithValue("@cutoff", DateTime.Now.AddHours(-1));
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to clean MySQL test tables");
                return false;
            }
        }

        public override bool CleanSqlBuildLoggingTables()
        {
            try
            {
                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new MySqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new MySqlCommand(
                        "DELETE FROM sqlbuild_logging " +
                        "WHERE buildfilename LIKE 'SqlSyncTest-%' " +
                        "OR buildfilename LIKE 'SqlBuildManagerTest-%' " +
                        "OR buildfilename = 'PreRunEntry' " +
                        "OR commitdate < @cutoff",
                        conn);
                    cmd.Parameters.AddWithValue("@cutoff", DateTime.Now.AddHours(-1));
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to clean MySQL sqlbuild_logging tables");
                return false;
            }
        }

        public override ConnectionData CreateConnectionData(string databaseName)
        {
            return new ConnectionData()
            {
                SQLServerName = serverName,
                DatabaseName = databaseName,
                UserId = mySqlUser,
                Password = mySqlPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.MySQL,
                ScriptTimeout = 20
            };
        }

        public override int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex)
        {
            try
            {
                using var conn = new MySqlConnection(GetConnectionString(testDatabaseNames[databaseIndex]));
                conn.Open();
                using var cmd = new MySqlCommand("SELECT count(*) FROM sqlbuild_logging WHERE buildfilename = @buildfilename", conn);
                cmd.Parameters.AddWithValue("@buildfilename", Path.GetFileName(projectFileName));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to get count for build file");
                return -1;
            }
        }

        public override int GetTestTableRowCount(int databaseIndex)
        {
            try
            {
                using var conn = new MySqlConnection(GetConnectionString(testDatabaseNames[databaseIndex]));
                conn.Open();
                using var cmd = new MySqlCommand("SELECT count(*) FROM transactiontest WHERE guid = @guid", conn);
                cmd.Parameters.AddWithValue("@guid", testGuid.ToString());
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to get test table row count");
                return -1;
            }
        }

        public string PreRunScriptGuid = "47037F10-C217-4e7b-89AE-482F8C09D672";

        public bool InsertPreRunScriptEntry()
        {
            try
            {
                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new MySqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new MySqlCommand(
                        @"INSERT INTO sqlbuild_logging (buildfilename, scriptfilename, scriptid, scriptfilehash, commitdate, tag, scripttext, sequence, description, allowscriptblock, userid)
                          VALUES ('PreRunEntry', 'PreRunScript.sql', @scriptid, 'MadeUpHash', @commitdate, '', 'Pre-run script text', 0, 'Pre-run script', true, 'UnitTest')", conn);
                    cmd.Parameters.AddWithValue("@scriptid", PreRunScriptGuid);
                    cmd.Parameters.AddWithValue("@commitdate", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to insert pre-run script entry");
                return false;
            }
        }

        public void AddPreRunScript(ref SqlSyncBuildDataModel data, bool multipleRun)
        {
            var row = new Script
            {
                AllowMultipleRuns = multipleRun,
                BuildOrder = 1,
                CausesBuildFailure = true,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = true,
                StripTransactionText = true,
                Description = "Test Script to be skipped",
                AddedBy = "UnitTest",
                ScriptId = PreRunScriptGuid
            };

            data.Script.Add(row);
            string script = $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('INSERT TEST', '{testGuid}', '{testTimeStamp:yyyy-MM-dd HH:mm:ss}')";
            File.WriteAllText(row.FileName, script);

            InsertPreRunScriptEntry();
        }

        public SqlBuildHelper CreateSqlBuildHelperAccessor(SqlSyncBuildDataModel buildData)
        {
            SqlBuildHelper target = new SqlBuildHelper(data: connData, connectionsService: new DefaultConnectionsService());

            ((ISqlBuildRunnerProperties)target).BuildDataModel = buildData;

            string logFile = GetTrulyUniqueFile();
            tempFiles.Add(logFile);
            target.scriptLogFileName = logFile;

            SqlSyncBuildDataModel buildHist = CreateSqlSyncSqlBuildDataModelObject();
            ((ISqlBuildRunnerProperties)target).BuildHistoryModel = buildHist;

            projectFileName = GetTrulyUniqueFile();
            tempFiles.Add(projectFileName);
            target.projectFileName = projectFileName;

            buildHistoryXmlFile = GetTrulyUniqueFile();
            tempFiles.Add(buildHistoryXmlFile);
            target.buildHistoryXmlFile = buildHistoryXmlFile;

            return target;
        }

        public int GetSqlBuildLoggingRowCountByScriptId(int databaseIndex, string guidValue)
        {
            try
            {
                using var conn = new MySqlConnection(GetConnectionString(testDatabaseNames[databaseIndex]));
                conn.Open();
                using var cmd = new MySqlCommand("SELECT count(*) FROM sqlbuild_logging WHERE scriptid = @scriptid", conn);
                cmd.Parameters.AddWithValue("@scriptid", guidValue);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                return -1;
            }
        }
    }
}
