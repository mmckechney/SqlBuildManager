using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.SqlServer.IntegrationTest;

[TestClass]
[DoNotParallelize]
public class LocalContainerEmulatorThreadedTests : LocalContainerEmulatorRuntimeTestBase
{
    private const string Server = "sqlserver";
    private const string User = "sa";
    private const string Password = "SqlBuildLocal1!";

    [TestMethod]
    [DataRow("Count", 1)]
    [DataRow("Count", 4)]
    [DataRow("MaxPerServer", 4)]
    public async Task ThreadedRun_WithTwentyDatabases_UsesConcurrencyAndPublishesEffects(string concurrencyType, int concurrency)
    {
        var databases = Enumerable.Range(1, 20).Select(index => $"sbm_sql_test_{index:00}").ToArray();
        await using (var admin = new SqlConnection($"Server={Server};Database=master;User Id={User};Password={Password};TrustServerCertificate=True"))
        {
            await admin.OpenAsync();
            foreach (var database in databases)
            {
                await using var command = admin.CreateCommand();
                command.CommandText = $"IF DB_ID('{database}') IS NULL CREATE DATABASE [{database}]";
                await command.ExecuteNonQueryAsync();
            }
        }

        var jobName = $"sbm-sql-20-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunThreadedRuntimeTestAsync(
            "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True"),
            "IF OBJECT_ID('transactiontest', 'U') IS NULL CREATE TABLE transactiontest (message nvarchar(500), guid uniqueidentifier, datetimestamp datetime2)",
            $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', NEWID(), SYSUTCDATETIME())");
    }

    [TestMethod]
    [DataRow("Count", 2)]
    [DataRow("MaxPerServer", 3)]
    public async Task LongRunning_SBMSource_ByConcurrencyType_Success(string concurrencyType, int concurrency)
    {
        var databases = Enumerable.Range(1, 6).Select(index => $"sbm_sql_longrun_{index:00}").ToArray();
        await using (var admin = new SqlConnection($"Server={Server};Database=master;User Id={User};Password={Password};TrustServerCertificate=True"))
        {
            await admin.OpenAsync();
            foreach (var database in databases)
            {
                await using var command = admin.CreateCommand();
                command.CommandText = $"IF DB_ID('{database}') IS NULL CREATE DATABASE [{database}]";
                await command.ExecuteNonQueryAsync();
            }
        }

        var jobName = $"sbm-sql-longrun-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunLongRunningRuntimeTestAsync(
            "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True"),
            "IF OBJECT_ID('transactiontest', 'U') IS NULL CREATE TABLE transactiontest (message nvarchar(500), guid uniqueidentifier, datetimestamp datetime2)",
            $"WAITFOR DELAY '00:00:15'; INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', NEWID(), SYSUTCDATETIME())",
            60);
    }

    [TestMethod]
    [DataRow("Count", 1)]
    [DataRow("MaxPerServer", 3)]
    public async Task ACI_Queue_DacpacSource_KeyVault_Secrets_Success(string concurrencyType, int concurrency)
    {
        var databasePrefix = $"sbm_sql_dacpac_{concurrencyType.ToLowerInvariant()}_{concurrency}";
        var sourceDatabase = $"{databasePrefix}_source";
        var databases = Enumerable.Range(1, 6).Select(index => $"{databasePrefix}_{index:00}").ToArray();
        await EnsureDatabasesAsync(new[] { sourceDatabase }.Concat(databases).ToArray());
        var testDirectory = CreateRuntimeTestDirectory("dacpac-source");
        Directory.CreateDirectory(testDirectory);
        var dacpacPath = Path.Combine(testDirectory, "platinum.dacpac");
        var targetDacpacPath = Path.Combine(testDirectory, "empty-target.dacpac");
        var packagePath = Path.Combine(testDirectory, "platinum-diff.sbm");

        try
        {
            await CreateDacpacTableAsync(sourceDatabase);
            await CreateDacpacAsync(sourceDatabase, dacpacPath);
            await CreateDacpacAsync(databases[0], targetDacpacPath);
            await CreateSbmFromDacpacsAsync(dacpacPath, targetDacpacPath, packagePath);

            var jobName = $"sbm-sql-dacpac-{concurrencyType.ToLowerInvariant()}-{concurrency}";
            await RunThreadedDacpacRuntimeTestAsync(
                "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
                dacpacPath, packagePath, false,
                database => AssertDacpacTableAsync(database, "dacpac_test"));
        }
        finally { }
    }

    [TestMethod]
    [DataRow("Count", 1)]
    [DataRow("MaxPerServer", 3)]
    public async Task ACI_Queue_DacpacSource_ForceApplyCustom_KeyVault_Secrets_Success(string concurrencyType, int concurrency)
    {
        var databasePrefix = $"sbm_sql_custom_dacpac_{concurrencyType.ToLowerInvariant()}_{concurrency}";
        var sourceDatabase = $"{databasePrefix}_source";
        var databases = Enumerable.Range(1, 6).Select(index => $"{databasePrefix}_{index:00}").ToArray();
        await EnsureDatabasesAsync(new[] { sourceDatabase }.Concat(databases).ToArray());
        var testDirectory = CreateRuntimeTestDirectory("dacpac-custom");
        Directory.CreateDirectory(testDirectory);
        var dacpacPath = Path.Combine(testDirectory, "platinum.dacpac");
        var targetDacpacPath = Path.Combine(testDirectory, "empty-target.dacpac");
        var packagePath = Path.Combine(testDirectory, "platinum-diff.sbm");

        try
        {
            await CreateDacpacTableAsync(sourceDatabase);
            await CreateDacpacAsync(sourceDatabase, dacpacPath);
            await CreateDacpacAsync(databases[0], targetDacpacPath);
            await CreateSbmFromDacpacsAsync(dacpacPath, targetDacpacPath, packagePath);
            await CreateSchemaDriftAsync(databases[1]);
            await CreateSchemaDriftAsync(databases[2]);

            var jobName = $"sbm-sql-custom-dacpac-{concurrencyType.ToLowerInvariant()}-{concurrency}";
            await RunThreadedDacpacRuntimeTestAsync(
                "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
                dacpacPath, packagePath, true,
                database => AssertDacpacTableAsync(database, "dacpac_test"));
        }
        finally { }
    }

    private async Task EnsureDatabasesAsync(IReadOnlyList<string> databases)
    {
        await using var admin = new SqlConnection($"Server={Server};Database=master;User Id={User};Password={Password};TrustServerCertificate=True");
        await admin.OpenAsync();
        foreach (var database in databases)
        {
            await using var command = admin.CreateCommand();
            command.CommandText = $"IF DB_ID('{database}') IS NULL CREATE DATABASE [{database}]";
            await command.ExecuteNonQueryAsync();
        }
    }

    private static Task CreateDacpacAsync(string database, string dacpacPath)
    {
        var args = new[]
        {
            "dacpac", "--authtype", "Password", "--username", User, "--password", Password,
            "--server", Server, "--database", database, "--dacpacname", dacpacPath,
            "--platform", "SqlServer", "--trustservercertificate", "true"
        };
        return RunRuntimeCommandAsync(args);
    }

    private static Task CreateSbmFromDacpacsAsync(
        string platinumDacpacPath,
        string targetDacpacPath,
        string packagePath)
    {
        var args = new[]
        {
            "create", "fromdacpacs", "--outputsbm", packagePath,
            "--platinumdacpac", platinumDacpacPath, "--targetdacpac", targetDacpacPath
        };
        return RunRuntimeCommandAsync(args);
    }

    private async Task CreateDacpacTableAsync(string database)
    {
        await using var connection = new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "IF OBJECT_ID('dbo.dacpac_test', 'U') IS NULL CREATE TABLE dbo.dacpac_test (id int NOT NULL PRIMARY KEY, message nvarchar(200) NOT NULL)";
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateSchemaDriftAsync(string database)
    {
        await using var connection = new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "IF OBJECT_ID('dbo.custom_dacpac_drift', 'U') IS NULL CREATE TABLE dbo.custom_dacpac_drift (id int NOT NULL PRIMARY KEY)";
        await command.ExecuteNonQueryAsync();
    }

    private async Task AssertDacpacTableAsync(string database, string tableName)
    {
        await using var connection = new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM sys.tables WHERE name = '{tableName}'";
        if (Convert.ToInt32(await command.ExecuteScalarAsync()) != 1)
        {
            throw new InvalidOperationException($"DACPAC execution did not create {tableName} in {database}.");
        }
    }
}
