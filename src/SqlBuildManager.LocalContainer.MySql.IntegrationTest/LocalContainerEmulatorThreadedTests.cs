using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
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

        var jobName = $"sbm-mysql-20-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunThreadedRuntimeTestAsync(
            "MySQL", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new MySqlConnection($"Server={Server};Database={database};User ID={User};Password={Password}"),
            "CREATE TABLE IF NOT EXISTS transactiontest (message varchar(500), guid char(36), datetimestamp datetime)",
            $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', UUID(), NOW())");
    }

    [TestMethod]
    [DataRow("Count", 2)]
    [DataRow("MaxPerServer", 3)]
    public async Task LongRunning_SBMSource_ByConcurrencyType_Success(string concurrencyType, int concurrency)
    {
        var databases = Enumerable.Range(1, 6).Select(index => $"sbm_mysql_longrun_{index:00}").ToArray();
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

        var jobName = $"sbm-mysql-longrun-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunLongRunningRuntimeTestAsync(
            "MySQL", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new MySqlConnection($"Server={Server};Database={database};User ID={User};Password={Password}"),
            "CREATE TABLE IF NOT EXISTS transactiontest (message varchar(500), guid char(36), datetimestamp datetime)",
            $"SELECT SLEEP(15); INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', UUID(), NOW())",
            60);
    }
}
