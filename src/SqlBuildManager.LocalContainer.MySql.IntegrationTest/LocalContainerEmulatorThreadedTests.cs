using MySqlConnector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.MySql.IntegrationTest;

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
        const string database = "sbm_mysql_test";
        const string server = "mysql";
        const string user = "root";
        const string password = "MySq1Adm!n";
        await using (var admin = new MySqlConnection($"Server={server};Database=mysql;User ID={user};Password={password}"))
        {
            await admin.OpenAsync();
            await using var command = admin.CreateCommand();
            command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{database}`";
            await command.ExecuteNonQueryAsync();
        }

        var rootCommand = CommandLineBuilder.SetUp();
        await RunThreadedRuntimeTestAsync(
            "MySQL", server, database, user, password, "sbm-mysql-emulator-test",
            db => new MySqlConnection($"Server={server};Database={db};User ID={user};Password={password}"),
            "CREATE TABLE IF NOT EXISTS transactiontest (message varchar(500), guid char(36), datetimestamp datetime)",
            "INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('LOCAL EMULATOR THREADED TEST', '00000000-0000-0000-0000-000000000001', NOW())",
            args => rootCommand.Parse(args).InvokeAsync());
    }
}
