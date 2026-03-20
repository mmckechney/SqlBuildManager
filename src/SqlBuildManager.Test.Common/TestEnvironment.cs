using System;
using SqlSync.Connection;

namespace SqlBuildManager.Test.Common
{
    /// <summary>
    /// Centralized access to test environment configuration.
    /// Reads from environment variables with sensible defaults for local development.
    /// </summary>
    public static class TestEnvironment
    {
        // SQL Server settings
        public static string SqlServer { get; } = Environment.GetEnvironmentVariable("SBM_TEST_SQL_SERVER") ?? @"(local)\SQLEXPRESS";
        public static string SqlUser { get; } = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER") ?? string.Empty;
        public static string SqlPassword { get; } = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD") ?? string.Empty;
        public static bool UseSqlAuth => !string.IsNullOrWhiteSpace(SqlUser);

        // PostgreSQL settings
        public static string PostgresServer { get; } = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "localhost";
        public static string PostgresUser { get; } = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_USER") ?? "postgres";
        public static string PostgresPassword { get; } = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_PASSWORD") ?? "P0stSqlAdm1n";

        /// <summary>
        /// Build a SQL Server connection string with placeholder {0} for the database name.
        /// </summary>
        public static string GetSqlConnectionStringTemplate()
        {
            if (UseSqlAuth)
                return $"Data Source={SqlServer};Initial Catalog={{0}};User ID={SqlUser};Password={SqlPassword};CONNECTION TIMEOUT=20;Trust Server Certificate=true;Encrypt=false";
            else
                return $"Data Source={SqlServer};Initial Catalog={{0}};Trusted_Connection=Yes;CONNECTION TIMEOUT=20;Trust Server Certificate=true;Encrypt=false";
        }

        /// <summary>
        /// Build a ConnectionData for the given database using the current environment auth settings.
        /// </summary>
        public static ConnectionData GetConnectionData(string databaseName)
        {
            var data = new ConnectionData(SqlServer, databaseName);
            if (UseSqlAuth)
            {
                data.AuthenticationType = AuthenticationType.Password;
                data.UserId = SqlUser;
                data.Password = SqlPassword;
            }
            return data;
        }

        /// <summary>
        /// CLI auth arguments for SQL Server tests (--authtype, --username, --password).
        /// </summary>
        public static string[] GetSqlAuthArgs()
        {
            if (UseSqlAuth)
                return new[] { "--authtype", AuthenticationType.Password.ToString(), "--username", SqlUser, "--password", SqlPassword };
            else
                return new[] { "--authtype", AuthenticationType.Windows.ToString() };
        }

        /// <summary>
        /// CLI auth arguments for PostgreSQL tests.
        /// </summary>
        public static string[] GetPostgresAuthArgs()
        {
            return new[] {
                "--authtype", AuthenticationType.Password.ToString(),
                "--username", PostgresUser,
                "--password", PostgresPassword
            };
        }

        /// <summary>
        /// CLI platform arguments for PostgreSQL tests.
        /// </summary>
        public static string[] GetPostgresPlatformArgs()
        {
            return new[] { "--platform", DatabasePlatform.PostgreSQL.ToString() };
        }
    }
}
