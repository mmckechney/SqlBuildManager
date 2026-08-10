using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.MySql.IntegrationTest;

[TestClass]
[DoNotParallelize]
public class LocalContainerEmulatorThreadedTests : LocalContainerEmulatorRuntimeTestBase
{
    private const string Server = "mysql";
    private const string User = "root";
    private const string Password = "MySq1Adm!n";

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

        var databases = Enumerable.Range(1, 20).Select(index => $"sbm_mysql_test_{index:00}").ToArray();
        await using (var admin = new MySqlConnection($"Server={Server};Database=mysql;User ID={User};Password={Password}"))
        {
            await admin.OpenAsync();
            foreach (var database in databases)
            {
                await using var command = admin.CreateCommand();
                command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{database}`";
                await command.ExecuteNonQueryAsync();
            }
        }

        var rootCommand = CommandLineBuilder.SetUp();
        var jobName = $"sbm-mysql-20-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunThreadedRuntimeTestAsync(
            "MySQL", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new MySqlConnection($"Server={Server};Database={database};User ID={User};Password={Password}"),
            "CREATE TABLE IF NOT EXISTS transactiontest (message varchar(500), guid char(36), datetimestamp datetime)",
            $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', UUID(), NOW())",
            args => rootCommand.Parse(args).InvokeAsync());
    }
}
