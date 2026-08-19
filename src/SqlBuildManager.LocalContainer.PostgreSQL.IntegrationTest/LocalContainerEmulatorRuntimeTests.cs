using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest;

[TestClass]
[DoNotParallelize]
public class LocalContainerEmulatorRuntimeTests : LocalContainerEmulatorRuntimeTestBase
{
    private const string Server = "postgres";
    private const string User = "postgres";
    private const string Password = "P0stSqlAdm1n";

    [TestMethod]
    [DataRow("Count", 1)]
    [DataRow("Count", 4)]
    [DataRow("MaxPerServer", 4)]
    public async Task ThreadedRun_WithTwentyDatabases_UsesConcurrencyAndPublishesEffects(string concurrencyType, int concurrency)
    {
        var databases = Enumerable.Range(1, 20).Select(index => $"sbm_pg_test_{index:00}").ToArray();
        await using (var admin = new NpgsqlConnection($"Host={Server};Database=postgres;Username={User};Password={Password}"))
        {
            await admin.OpenAsync();
            foreach (var database in databases)
            {
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
        }

        var jobName = $"sbm-pg-20-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunThreadedRuntimeTestAsync(
            "PostgreSQL", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new NpgsqlConnection($"Host={Server};Database={database};Username={User};Password={Password}"),
            """
            CREATE TABLE IF NOT EXISTS transactiontest (
                message varchar(500),
                guid uuid,
                datetimestamp timestamp
            )
            """,
            $"INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', '00000000-0000-0000-0000-000000000001', NOW())");
    }

    [TestMethod]
    [DataRow("Count", 2)]
    [DataRow("MaxPerServer", 3)]
    public async Task LongRunning_SBMSource_ByConcurrencyType_Success(string concurrencyType, int concurrency)
    {
        var databases = Enumerable.Range(1, 6).Select(index => $"sbm_pg_longrun_{index:00}").ToArray();
        await using (var admin = new NpgsqlConnection($"Host={Server};Database=postgres;Username={User};Password={Password}"))
        {
            await admin.OpenAsync();
            foreach (var database in databases)
            {
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
        }

        var jobName = $"sbm-pg-longrun-{concurrencyType.ToLowerInvariant()}-{concurrency}";
        await RunLongRunningRuntimeTestAsync(
            "PostgreSQL", Server, databases, User, Password, jobName, concurrencyType, concurrency,
            database => new NpgsqlConnection($"Host={Server};Database={database};Username={User};Password={Password}"),
            """
            CREATE TABLE IF NOT EXISTS transactiontest (
                message varchar(500),
                guid uuid,
                datetimestamp timestamp
            )
            """,
            $"SELECT pg_sleep(15); INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('{TestMessage}', '00000000-0000-0000-0000-000000000001', NOW())",
            60);
    }
}
