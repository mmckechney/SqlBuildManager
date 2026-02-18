using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Initialization helper for PostgreSQL-based dependent tests.
    /// Mirrors the SQL Server Initialization class but uses Npgsql and PostgreSQL syntax.
    /// Configure via environment variables:
    ///   SBM_TEST_POSTGRES_SERVER (default: localhost)
    ///   SBM_TEST_POSTGRES_USER (default: postgres)
    ///   SBM_TEST_POSTGRES_PASSWORD (default: P0stSqlAdm1n)
    /// </summary>
    public class PostgresInitialization : IDisposable
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public List<string> testDatabaseNames = null;
        public List<string> tempFiles = null;
        public Guid testGuid;
        public DateTime testTimeStamp;
        public ConnectionData connData = null;
        public string projectFileName = null;
        public string buildHistoryXmlFile = null;

        public string serverName;
        public string pgUser;
        public string pgPassword;

        public static string MissingDatabaseErrorMessage = "NOTE: To successfully test, please make sure you have a PostgreSQL instance running. Set SBM_TEST_POSTGRES_SERVER, SBM_TEST_POSTGRES_USER, SBM_TEST_POSTGRES_PASSWORD environment variables.";

        private static string GetServerName() => Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "localhost";
        private static string GetUser() => Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_USER") ?? "postgres";
        private static string GetPassword() => Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_PASSWORD") ?? "P0stSqlAdm1n";

        public PostgresInitialization()
        {
            serverName = GetServerName();
            pgUser = GetUser();
            pgPassword = GetPassword();

            testDatabaseNames = new List<string>
            {
                "sbm_pg_test",
                "sbm_pg_test1",
                "sbm_pg_test2",
                "sbm_pg_test3"
            };

            testGuid = Guid.NewGuid();
            testTimeStamp = DateTime.Now;

            connData = new ConnectionData()
            {
                SQLServerName = serverName,
                DatabaseName = testDatabaseNames[0],
                UserId = pgUser,
                Password = pgPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                ScriptTimeout = 20
            };

            tempFiles = new List<string>();

            if (!CreateDatabases())
                Assert.Fail(string.Format("Unable to create the target PostgreSQL databases. {0}", MissingDatabaseErrorMessage));
            if (!CreateTestTables())
                Assert.Fail("Unable to create the target PostgreSQL tables");
            if (!CreateSqlBuildLoggingTables())
                Assert.Fail("Unable to create the sqlbuild_logging tables in PostgreSQL");
            if (!CleanTestTables())
                Assert.Fail("Unable to clean pre-existing data from the PostgreSQL target tables");
            if (!CleanSqlBuildLoggingTables())
                Assert.Fail("Unable to clean pre-existing data from the PostgreSQL sqlbuild_logging tables");
        }

        private string GetAdminConnectionString()
        {
            return $"Host={serverName};Database=postgres;Username={pgUser};Password={pgPassword};Timeout=20";
        }

        private string GetConnectionString(string dbName)
        {
            return $"Host={serverName};Database={dbName};Username={pgUser};Password={pgPassword};Timeout=20";
        }

        public bool CreateDatabases()
        {
            try
            {
                using var adminConn = new NpgsqlConnection(GetAdminConnectionString());
                adminConn.Open();

                foreach (string dbName in testDatabaseNames)
                {
                    using var checkCmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", adminConn);
                    checkCmd.Parameters.AddWithValue("@name", dbName);
                    object val = checkCmd.ExecuteScalar();

                    if (val == null || val == DBNull.Value)
                    {
                        // CREATE DATABASE cannot run inside a transaction in PostgreSQL
                        using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", adminConn);
                        createCmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create PostgreSQL test databases");
                return false;
            }
        }

        public bool CreateTestTables()
        {
            try
            {
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS transactiontest (
                        id SERIAL PRIMARY KEY,
                        message VARCHAR(500) NULL,
                        guid UUID NULL,
                        datetimestamp TIMESTAMP NULL
                    )";

                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new NpgsqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new NpgsqlCommand(createTableSql, conn);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create PostgreSQL test tables");
                return false;
            }
        }

        public bool CreateSqlBuildLoggingTables()
        {
            try
            {
                var resourceProvider = new Services.PostgresResourceProvider();

                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new NpgsqlConnection(GetConnectionString(dbName));
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(resourceProvider.LoggingTableDdl, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand(resourceProvider.LoggingTableCommitCheckIndex, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create PostgreSQL sqlbuild_logging tables");
                return false;
            }
        }

        public bool CleanTestTables()
        {
            try
            {
                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new NpgsqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new NpgsqlCommand("DELETE FROM transactiontest WHERE datetimestamp < @cutoff", conn);
                    cmd.Parameters.AddWithValue("@cutoff", DateTime.Now.AddHours(-1));
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to clean PostgreSQL test tables");
                return false;
            }
        }

        public bool CleanSqlBuildLoggingTables()
        {
            try
            {
                foreach (string dbName in testDatabaseNames)
                {
                    using var conn = new NpgsqlConnection(GetConnectionString(dbName));
                    conn.Open();
                    using var cmd = new NpgsqlCommand("DELETE FROM sqlbuild_logging WHERE buildfilename LIKE 'SqlSyncTest-%' OR commitdate < @cutoff", conn);
                    cmd.Parameters.AddWithValue("@cutoff", DateTime.Now.AddHours(-1));
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to clean PostgreSQL sqlbuild_logging tables");
                return false;
            }
        }

        public SqlSyncBuildDataModel CreateSqlSyncSqlBuildDataModelObject()
        {
            return SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
        }

        public void AddInsertScript(ref SqlSyncBuildDataModel data, bool multipleRun)
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
                Description = "PostgreSQL Test Script to insert into transactiontest table",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            string script = $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('INSERT TEST', '{testGuid}', '{testTimeStamp:yyyy-MM-dd HH:mm:ss}')";
            File.WriteAllText(row.FileName, script);
        }

        public void AddSelectScript(ref SqlSyncBuildDataModel data)
        {
            var row = new Script
            {
                AllowMultipleRuns = true,
                BuildOrder = 1,
                CausesBuildFailure = true,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = true,
                StripTransactionText = true,
                Description = "PostgreSQL Test Script to select from transactiontest table",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            File.WriteAllText(row.FileName, "SELECT * FROM transactiontest");
        }

        public void AddFailureScript(ref SqlSyncBuildDataModel data, bool rollBackOnError, bool causeBuildFailure)
        {
            var row = new Script
            {
                AllowMultipleRuns = true,
                BuildOrder = 10,
                CausesBuildFailure = causeBuildFailure,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = rollBackOnError,
                StripTransactionText = true,
                Description = "PostgreSQL Test Script to cause a failure",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            // Invalid column name will cause failure
            string script = $"INSERT INTO transactiontest (invalid_column, guid, datetimestamp) VALUES ('INSERT TEST', '{testGuid}', '{testTimeStamp:yyyy-MM-dd HH:mm:ss}')";
            File.WriteAllText(row.FileName, script);
        }

        public ConnectionData CreateConnectionData(string databaseName)
        {
            return new ConnectionData()
            {
                SQLServerName = serverName,
                DatabaseName = databaseName,
                UserId = pgUser,
                Password = pgPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                ScriptTimeout = 20
            };
        }

        public SqlBuildHelper CreateSqlBuildHelper(SqlSyncBuildDataModel buildData)
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData);
            return SetSqlBuildHelperValues(helper, buildData);
        }

        public SqlBuildHelper SetSqlBuildHelperValues(SqlBuildHelper sbh, SqlSyncBuildDataModel buildData)
        {
            ((ISqlBuildRunnerProperties)sbh).BuildDataModel = buildData;

            string logFile = GetTrulyUniqueFile();
            sbh.scriptLogFileName = logFile;

            SqlSyncBuildDataModel buildHist = CreateSqlSyncSqlBuildDataModelObject();
            ((ISqlBuildRunnerProperties)sbh).BuildHistoryModel = buildHist;

            projectFileName = GetTrulyUniqueFile();
            sbh.projectFileName = projectFileName;

            buildHistoryXmlFile = GetTrulyUniqueFile();
            sbh.buildHistoryXmlFile = buildHistoryXmlFile;
            return sbh;
        }

        public Build GetRunBuildRow(SqlBuildHelper sqlBuildHelper)
        {
            return new Build();
        }

        public List<DatabaseOverride> GetDatabaseOverrides()
        {
            return new List<DatabaseOverride>
            {
                new DatabaseOverride(serverName, testDatabaseNames[0], testDatabaseNames[0])
            };
        }

        public int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString(testDatabaseNames[databaseIndex]));
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT count(*) FROM sqlbuild_logging WHERE buildfilename = @buildfilename", conn);
                cmd.Parameters.AddWithValue("@buildfilename", Path.GetFileName(projectFileName));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to get count for build file");
                return -1;
            }
        }

        public int GetTestTableRowCount(int databaseIndex)
        {
            try
            {
                using var conn = new NpgsqlConnection(GetConnectionString(testDatabaseNames[databaseIndex]));
                conn.Open();
                using var cmd = new NpgsqlCommand("SELECT count(*) FROM transactiontest WHERE guid = @guid", conn);
                cmd.Parameters.AddWithValue("@guid", testGuid);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to get test table row count");
                return -1;
            }
        }

        public string GetTrulyUniqueFile()
        {
            string tmpName = Path.GetTempFileName();
            string newName = Path.Combine(Path.GetDirectoryName(tmpName), "SqlSyncTest-PG-" + Guid.NewGuid().ToString() + ".tmp");
            File.Move(tmpName, newName);
            tempFiles.Add(newName);
            return newName;
        }

        public void Dispose()
        {
            foreach (var file in tempFiles)
            {
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { }
                }
            }

            if (File.Exists(projectFileName))
                try { File.Delete(projectFileName); } catch { }
            if (File.Exists(buildHistoryXmlFile))
                try { File.Delete(buildHistoryXmlFile); } catch { }
        }
    }
}
