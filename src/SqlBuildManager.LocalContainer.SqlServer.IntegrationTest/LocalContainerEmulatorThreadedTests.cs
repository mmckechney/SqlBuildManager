using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Collections.Generic;
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
}
