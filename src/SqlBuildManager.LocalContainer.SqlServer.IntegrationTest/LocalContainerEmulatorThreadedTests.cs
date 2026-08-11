using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.CommandLine;
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
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }

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

        var rootCommand = CommandLineBuilder.SetUp();
        var jobName = $"sbm-sql-20-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunThreadedRuntimeTestAsync(
            "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True"),
            "IF OBJECT_ID('transactiontest', 'U') IS NULL CREATE TABLE transactiontest (message nvarchar(500), guid uniqueidentifier, datetimestamp datetime2)",
            $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', NEWID(), SYSUTCDATETIME())",
            args => rootCommand.Parse(args).InvokeAsync());
    }

    [TestMethod]
    [DataRow("Count", 2)]
    [DataRow("MaxPerServer", 3)]
    public async Task LongRunning_SBMSource_ByConcurrencyType_Success(string concurrencyType, int concurrency)
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }

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

        var rootCommand = CommandLineBuilder.SetUp();
        var jobName = $"sbm-sql-longrun-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunLongRunningRuntimeTestAsync(
            "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True"),
            "IF OBJECT_ID('transactiontest', 'U') IS NULL CREATE TABLE transactiontest (message nvarchar(500), guid uniqueidentifier, datetimestamp datetime2)",
            $"WAITFOR DELAY '00:00:15'; INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', NEWID(), SYSUTCDATETIME())",
            60,
            args => rootCommand.Parse(args).InvokeAsync());
    }

    [TestMethod]
    [TestCategory("RuntimeContainer")]
    public async Task ProductionRuntimeContainer_ThreadedRun_UpdatesDatabasesAndEmulators()
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured ||
            !string.Equals(Environment.GetEnvironmentVariable("SBM_RUNTIME_CONTAINER_MODE"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Inconclusive("Runtime-container tests require the local emulators and SBM_RUNTIME_CONTAINER_MODE=true.");
        }

        var databases = Enumerable.Range(1, 20).Select(index => $"sbm_sql_runtime_{index:00}").ToArray();
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

        var jobName = "sbm-sql-runtime";
        await RunThreadedRuntimeTestAsync(
            "SqlServer", Server, databases, User, Password, jobName, "Count", 4,
            database => new SqlConnection($"Server={Server};Database={database};User Id={User};Password={Password};TrustServerCertificate=True"),
            "IF OBJECT_ID('transactiontest', 'U') IS NULL CREATE TABLE transactiontest (message nvarchar(500), guid uniqueidentifier, datetimestamp datetime2)",
            $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', NEWID(), SYSUTCDATETIME())",
            async args =>
            {
                var result = await RuntimeContainerClient.RunAsync(default, args);
                System.Console.WriteLine(result.Output);
                return result.ExitCode;
            });
    }

    [TestMethod]
    [DataRow("Count", 1)]
    [DataRow("MaxPerServer", 3)]
    public async Task ACI_Queue_DacpacSource_KeyVault_Secrets_Success(string concurrencyType, int concurrency)
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }

        var databasePrefix = $"sbm_sql_dacpac_{concurrencyType.ToLowerInvariant()}_{concurrency}";
        var sourceDatabase = $"{databasePrefix}_source";
        var databases = Enumerable.Range(1, 6).Select(index => $"{databasePrefix}_{index:00}").ToArray();
        await EnsureDatabasesAsync(new[] { sourceDatabase }.Concat(databases).ToArray());
        var rootCommand = CommandLineBuilder.SetUp();
        var testDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"dacpac-source-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        var dacpacPath = Path.Combine(testDirectory, "platinum.dacpac");
        var targetDacpacPath = Path.Combine(testDirectory, "empty-target.dacpac");
        var packagePath = Path.Combine(testDirectory, "platinum-diff.sbm");

        try
        {
            await CreateDacpacTableAsync(sourceDatabase);
            Assert.AreEqual(0, await CreateDacpacAsync(rootCommand, sourceDatabase, dacpacPath));
            Assert.AreEqual(0, await CreateDacpacAsync(rootCommand, databases[0], targetDacpacPath));
            Assert.AreEqual(0, await CreateSbmFromDacpacsAsync(rootCommand, dacpacPath, targetDacpacPath, packagePath));

            var jobName = $"sbm-sql-dacpac-{concurrencyType.ToLowerInvariant()}-{concurrency}";
            await RunThreadedDacpacRuntimeTestAsync(
                "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
                dacpacPath, packagePath, false, args => rootCommand.Parse(args).InvokeAsync(),
                database => AssertDacpacTableAsync(database, "dacpac_test"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    [DataRow("Count", 1)]
    [DataRow("MaxPerServer", 3)]
    public async Task ACI_Queue_DacpacSource_ForceApplyCustom_KeyVault_Secrets_Success(string concurrencyType, int concurrency)
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }

        var databasePrefix = $"sbm_sql_custom_dacpac_{concurrencyType.ToLowerInvariant()}_{concurrency}";
        var sourceDatabase = $"{databasePrefix}_source";
        var databases = Enumerable.Range(1, 6).Select(index => $"{databasePrefix}_{index:00}").ToArray();
        await EnsureDatabasesAsync(new[] { sourceDatabase }.Concat(databases).ToArray());
        var rootCommand = CommandLineBuilder.SetUp();
        var testDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"dacpac-custom-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        var dacpacPath = Path.Combine(testDirectory, "platinum.dacpac");
        var targetDacpacPath = Path.Combine(testDirectory, "empty-target.dacpac");
        var packagePath = Path.Combine(testDirectory, "platinum-diff.sbm");

        try
        {
            await CreateDacpacTableAsync(sourceDatabase);
            Assert.AreEqual(0, await CreateDacpacAsync(rootCommand, sourceDatabase, dacpacPath));
            Assert.AreEqual(0, await CreateDacpacAsync(rootCommand, databases[0], targetDacpacPath));
            Assert.AreEqual(0, await CreateSbmFromDacpacsAsync(rootCommand, dacpacPath, targetDacpacPath, packagePath));
            await CreateSchemaDriftAsync(databases[1]);
            await CreateSchemaDriftAsync(databases[2]);

            var jobName = $"sbm-sql-custom-dacpac-{concurrencyType.ToLowerInvariant()}-{concurrency}";
            await RunThreadedDacpacRuntimeTestAsync(
                "SqlServer", Server, databases, User, Password, jobName, concurrencyType, concurrency,
                dacpacPath, packagePath, true, args => rootCommand.Parse(args).InvokeAsync(),
                database => AssertDacpacTableAsync(database, "dacpac_test"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
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

    private async Task<int> CreateDacpacAsync(RootCommand rootCommand, string database, string dacpacPath)
    {
        var args = new[]
        {
            "dacpac", "--authtype", "Password", "--username", User, "--password", Password,
            "--server", Server, "--database", database, "--dacpacname", dacpacPath,
            "--platform", "SqlServer", "--trustservercertificate", "true"
        };
        return await rootCommand.Parse(args).InvokeAsync();
    }

    private async Task<int> CreateSbmFromDacpacsAsync(
        RootCommand rootCommand,
        string platinumDacpacPath,
        string targetDacpacPath,
        string packagePath)
    {
        var args = new[]
        {
            "create", "fromdacpacs", "--outputsbm", packagePath,
            "--platinumdacpac", platinumDacpacPath, "--targetdacpac", targetDacpacPath
        };
        return await rootCommand.Parse(args).InvokeAsync();
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
