using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest;

[TestClass]
[DoNotParallelize]
public class LocalContainerEmulatorRuntimeTests : LocalContainerEmulatorRuntimeTestBase
{
    [TestMethod]
    public async Task ThreadedRun_WithLocalEmulators_UpdatesDatabaseAndPublishesEffects()
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }
        const string database = "sbm_pg_runtime_test";
        const string server = "postgres";
        const string user = "postgres";
        const string password = "P0stSqlAdm1n";
        await using (var admin = new NpgsqlConnection($"Host={server};Database=postgres;Username={user};Password={password}"))
        {
            await admin.OpenAsync();
            await using var command = admin.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{database}\"";
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "42P04")
            {
            }
        }

        var rootCommand = CommandLineBuilder.SetUp();
        await RunThreadedRuntimeTestAsync(
            "PostgreSQL", server, database, user, password, "sbm-pg-runtime-emulator-test",
            db => new NpgsqlConnection($"Host={server};Database={db};Username={user};Password={password}"),
            """
            CREATE TABLE IF NOT EXISTS transactiontest (
                message varchar(500),
                guid uuid,
                datetimestamp timestamp
            )
            """,
            "INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('LOCAL EMULATOR THREADED TEST', '00000000-0000-0000-0000-000000000001', NOW())",
            args => rootCommand.Parse(args).InvokeAsync());
    }
}
