using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using SqlSync.Connection;

namespace SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest
{
    /// <summary>
    /// PostgreSQL-specific initialization for Console integration tests.
    /// Manages temp files, connection info, and database setup.
    /// Configure via environment variables:
    ///   SBM_TEST_POSTGRES_SERVER (default: localhost)
    ///   SBM_TEST_POSTGRES_USER (default: postgres)
    ///   SBM_TEST_POSTGRES_PASSWORD (default: P0stSqlAdm1n)
    /// </summary>
    class Initialization : IDisposable
    {
        public static string Server;
        public static string User;
        public static string Password;

        public static string[] GetAuthArgs()
        {
            return new[] {
                "--authtype", AuthenticationType.Password.ToString(),
                "--username", User,
                "--password", Password
            };
        }

        public static string[] GetPlatformArgs()
        {
            return new[] { "--platform", DatabasePlatform.PostgreSQL.ToString() };
        }

        static Initialization()
        {
            Server = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "localhost";
            User = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_USER") ?? "postgres";
            Password = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_PASSWORD") ?? "P0stSqlAdm1n";

            EnsureDatabases();
            EnsureTestTables();
        }

        private static readonly string[] TestDatabaseNames = new[]
        {
            "sbm_pg_test", "sbm_pg_test1", "sbm_pg_test2", "sbm_pg_test3"
        };

        private static string GetAdminConnectionString()
        {
            return $"Host={Server};Database=postgres;Username={User};Password={Password};Timeout=20";
        }

        private static string GetConnectionString(string dbName)
        {
            return $"Host={Server};Database={dbName};Username={User};Password={Password};Timeout=20";
        }

        private static void EnsureDatabases()
        {
            using var conn = new NpgsqlConnection(GetAdminConnectionString());
            conn.Open();
            foreach (string dbName in TestDatabaseNames)
            {
                using var checkCmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", conn);
                checkCmd.Parameters.AddWithValue("@name", dbName);
                if (checkCmd.ExecuteScalar() == null)
                {
                    using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", conn);
                    createCmd.ExecuteNonQuery();
                }
            }
        }

        private static void EnsureTestTables()
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS transactiontest (
                    id SERIAL PRIMARY KEY,
                    message VARCHAR(500) NULL,
                    guid UUID NULL,
                    datetimestamp TIMESTAMP NULL
                )";

            foreach (string dbName in TestDatabaseNames)
            {
                using var conn = new NpgsqlConnection(GetConnectionString(dbName));
                conn.Open();
                using var cmd = new NpgsqlCommand(createTableSql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        private static List<string> tempFiles;
        public static string SqlBuildZipFileName;
        public static string MultiDbFileName;
        public static string DbConfigFileName;

        public Initialization()
        {
            tempFiles = new List<string>();
            SqlBuildZipFileName = GetTrulyUniqueFile("sbm");
            MultiDbFileName = GetTrulyUniqueFile("multidb");
            DbConfigFileName = GetTrulyUniqueFile("cfg");
        }

        public static void CleanUp()
        {
            foreach (string f in tempFiles)
            {
                try { File.Delete(f); } catch { }
            }
        }

        public void CopySbmFileToTestPath()
        {
            File.WriteAllBytes(SqlBuildZipFileName, Properties.Resources.PG_SimpleSelect);
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

        public string GetTrulyUniqueFile(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Replace(".", "");
            string tmpName = Path.GetTempFileName();
            string newName = Path.Combine(Path.GetDirectoryName(tmpName), "SqlBuildManager-PG-Console-" + Guid.NewGuid().ToString() + "." + extension);
            File.Move(tmpName, newName);
            tempFiles.Add(newName);
            return newName;
        }

        public void Dispose()
        {
            foreach (string file in tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }
            }
        }
    }
}
