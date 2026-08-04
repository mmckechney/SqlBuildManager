using SqlBuildManager.Test.Common;
using System;
using System.Collections.Generic;
using System.IO;
using MySqlConnector;
using SqlBuildManager.Connection;

namespace SqlBuildManager.Console.MySQL.IntegrationTest
{
    /// <summary>
    /// MySQL-specific initialization for Console integration tests.
    /// Manages temp files, connection info, and database setup.
    /// Configure via environment variables:
    ///   SBM_TEST_MYSQL_SERVER (default: localhost)
    ///   SBM_TEST_MYSQL_USER (default: root)
    ///   SBM_TEST_MYSQL_PASSWORD (default: MySq1Adm!n)
    /// </summary>
    class Initialization : IDisposable
    {
        public static string Server => TestEnvironment.MySqlServer;
        public static string User => TestEnvironment.MySqlUser;
        public static string Password => TestEnvironment.MySqlPassword;

        public static string[] GetAuthArgs() => TestEnvironment.GetMySqlAuthArgs();

        public static string[] GetPlatformArgs() => TestEnvironment.GetMySqlPlatformArgs();

        static Initialization()
        {
            EnsureDatabases();
            EnsureTestTables();
        }

        private static readonly string[] TestDatabaseNames = new[]
        {
            "sbm_mysql_test", "sbm_mysql_test1", "sbm_mysql_test2", "sbm_mysql_test3"
        };

        private static string GetAdminConnectionString()
        {
            return $"Host={Server};Database=mysql;Username={User};Password={Password};Connection Timeout=20";
        }

        private static string GetConnectionString(string dbName)
        {
            return $"Host={Server};Database={dbName};Username={User};Password={Password};Connection Timeout=20";
        }

        private static void EnsureDatabases()
        {
            using var conn = new MySqlConnection(GetAdminConnectionString());
            conn.Open();
            foreach (string dbName in TestDatabaseNames)
            {
                using var checkCmd = new MySqlCommand("SELECT 1 FROM information_schema.schemata WHERE schema_name = @name", conn);
                checkCmd.Parameters.AddWithValue("@name", dbName);
                if (checkCmd.ExecuteScalar() == null)
                {
                    using var createCmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{dbName}`", conn);
                    createCmd.ExecuteNonQuery();
                }
            }
        }

        private static void EnsureTestTables()
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS transactiontest (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    message VARCHAR(500) NULL,
                    guid CHAR(36) NULL,
                    datetimestamp DATETIME NULL
                )";

            foreach (string dbName in TestDatabaseNames)
            {
                using var conn = new MySqlConnection(GetConnectionString(dbName));
                conn.Open();
                using var cmd = new MySqlCommand(createTableSql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        private static List<string> tempFiles = null!;
        public static string SqlBuildZipFileName = null!;
        public static string MultiDbFileName = null!;
        public static string DbConfigFileName = null!;

        public Initialization()
        {
            tempFiles = new List<string>();
            SqlBuildZipFileName = TestFileHelper.GetTrulyUniqueFile("sbm");
            tempFiles.Add(SqlBuildZipFileName);
            MultiDbFileName = TestFileHelper.GetTrulyUniqueFile("multidb");
            tempFiles.Add(MultiDbFileName);
            DbConfigFileName = TestFileHelper.GetTrulyUniqueFile("cfg");
            tempFiles.Add(DbConfigFileName);
        }

        public static void CleanUp()
        {
            TestFileHelper.CleanupTempFiles(tempFiles);
        }

        public void CopySbmFileToTestPath()
        {
            File.WriteAllBytes(SqlBuildZipFileName, Properties.Resources.MySQL_SimpleSelect);
        }

        public void CopyDbConfigFileToTestPath()
        {
            File.WriteAllBytes(DbConfigFileName, Properties.Resources.dbconfig);
        }

        public void CopyDbConfigFile4ToTestPath()
        {
            File.WriteAllBytes(DbConfigFileName, Properties.Resources.dbconfig_4);
        }

        public void CopyDbConfigFile8ToTestPath()
        {
            File.WriteAllBytes(DbConfigFileName, Properties.Resources.dbconfig_8);
        }

        public void CopyDoubleDbConfigFileToTestPath()
        {
            File.WriteAllBytes(DbConfigFileName, Properties.Resources.dbconfig_doubledb);
        }

        public void Dispose()
        {
            TestFileHelper.CleanupTempFiles(tempFiles);
        }
    }
}
