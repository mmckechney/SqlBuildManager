using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.SqlServer.IntegrationTest;

[TestClass]
[DoNotParallelize]
public class LocalContainerEmulatorThreadedTests : LocalContainerEmulatorRuntimeTestBase
{
    [TestMethod]
    public async Task ThreadedRun_WithLocalEmulators_UpdatesDatabaseAndPublishesEffects()
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }
        const string database = "sbm_sql_test";
        const string server = "sqlserver";
        const string user = "sa";
        const string password = "SqlBuildLocal1!";
        await using (var admin = new SqlConnection($"Server={server};Database=master;User Id={user};Password={password};TrustServerCertificate=True"))
        {
            await admin.OpenAsync();
            await using var command = admin.CreateCommand();
            command.CommandText = $"IF DB_ID('{database}') IS NULL CREATE DATABASE [{database}]";
            await command.ExecuteNonQueryAsync();
        }

        var rootCommand = CommandLineBuilder.SetUp();
        await RunThreadedRuntimeTestAsync(
            "SqlServer", server, database, user, password, "sbm-sql-emulator-test",
            db => new SqlConnection($"Server={server};Database={db};User Id={user};Password={password};TrustServerCertificate=True"),
            "IF OBJECT_ID('transactiontest', 'U') IS NULL CREATE TABLE transactiontest (message nvarchar(500), guid uniqueidentifier, datetimestamp datetime2)",
            "INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('LOCAL EMULATOR THREADED TEST', '00000000-0000-0000-0000-000000000001', SYSUTCDATETIME())",
            args => rootCommand.Parse(args).InvokeAsync());
    }
}
