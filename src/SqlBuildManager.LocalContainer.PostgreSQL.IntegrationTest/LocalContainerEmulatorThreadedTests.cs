using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlBuildManager.Connection;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest;

[TestClass]
[DoNotParallelize]
public class LocalContainerEmulatorThreadedTests
{
    private const string StorageAccount = "devstoreaccount1";
    private const string StorageKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    private const string JobName = "sbm-emulator-test";
    private const string DatabaseName = "sbm_pg_test";

    [TestMethod]
    public async Task ThreadedRun_WithLocalEmulators_UpdatesDatabaseAndPublishesEffects()
    {
        RequireEmulators();

        var server = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "localhost";
        var user = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_PASSWORD") ?? "P0stSqlAdm1n";
        var databaseConnection = $"Host={server};Database={DatabaseName};Username={user};Password={password};Timeout=20";
        var adminConnection = $"Host={server};Database=postgres;Username={user};Password={password};Timeout=20";
        var testDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            $"local-emulator-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        var packagePath = Path.Combine(testDirectory, "package.sbm");
        var overridePath = Path.Combine(testDirectory, "targets.cfg");
        var loggingPath = Path.Combine(testDirectory, "logs");
        var previousBlobEndpoint = Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT");
        Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", LocalContainerTestEnvironment.BlobEndpoint);

        try
        {
            await EnsureDatabaseAsync(adminConnection, databaseConnection);
            await using (var connection = new NpgsqlConnection(databaseConnection))
            {
                await connection.OpenAsync();
                await using var clear = new NpgsqlCommand(
                    "DELETE FROM transactiontest WHERE message = 'LOCAL EMULATOR THREADED TEST'", connection);
                await clear.ExecuteNonQueryAsync();
            }

            CreatePackage(packagePath);
            await File.WriteAllTextAsync(overridePath, $"{server}:{DatabaseName},{DatabaseName}");
            await SeedServiceBusMessageAsync();

            var args = new List<string>
            {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "false",
                "--trial", "false",
                "--override", overridePath,
                "--packagename", packagePath,
                "--timeoutretrycount", "0",
                "--concurrency", "1",
                "--concurrencytype", "Count",
                "--jobname", JobName,
                "--storageaccountname", StorageAccount,
                "--storageaccountkey", StorageKey,
                "--eventhubconnection", LocalContainerTestEnvironment.EventHubConnectionString,
                "--eventhublogging", "EssentialOnly",
                "--servicebustopicconnection", LocalContainerTestEnvironment.ServiceBusConnectionString,
                "--authtype", AuthenticationType.Password.ToString(),
                "--username", user,
                "--password", password,
                "--platform", DatabasePlatform.PostgreSQL.ToString()
            };

            var rootCommand = CommandLineBuilder.SetUp();
            var result = await rootCommand.Parse(args.ToArray()).InvokeAsync();
            Assert.AreEqual(0, result, "threaded run did not complete successfully.");

            await using (var connection = new NpgsqlConnection(databaseConnection))
            {
                await connection.OpenAsync();
                await using var query = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM transactiontest WHERE message = 'LOCAL EMULATOR THREADED TEST'", connection);
                Assert.AreEqual(1L, (long)(await query.ExecuteScalarAsync())!);
            }

            await AssertBlobEffectAsync();
            await AssertEventHubEffectAsync();
            await AssertServiceBusEffectAsync();
        }
        finally
        {
            Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", previousBlobEndpoint);
            DeleteIfExists(packagePath);
            DeleteIfExists(overridePath);
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }

    private static void RequireEmulators()
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive(
                "Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }
    }

    private static async Task EnsureDatabaseAsync(string adminConnection, string databaseConnection)
    {
        await using (var admin = new NpgsqlConnection(adminConnection))
        {
            await admin.OpenAsync();
            await using var exists = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @name", admin);
            exists.Parameters.AddWithValue("name", DatabaseName);
            if (await exists.ExecuteScalarAsync() == null)
            {
                await using var create = new NpgsqlCommand(
                    $"CREATE DATABASE \"{DatabaseName}\"", admin);
                await create.ExecuteNonQueryAsync();
            }
        }

        await using var connection = new NpgsqlConnection(databaseConnection);
        await connection.OpenAsync();
        await using var table = new NpgsqlCommand(
            """
            CREATE TABLE IF NOT EXISTS transactiontest (
                id SERIAL PRIMARY KEY,
                message VARCHAR(500) NULL,
                guid UUID NULL,
                datetimestamp TIMESTAMP NULL
            )
            """, connection);
        await table.ExecuteNonQueryAsync();
    }

    private static void CreatePackage(string packagePath)
    {
        using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
        AddEntry(archive, "Insert Local Emulator Test.sql",
            "INSERT INTO transactiontest (message, guid, datetimestamp) VALUES ('LOCAL EMULATOR THREADED TEST', '00000000-0000-0000-0000-000000000001', now())");
        AddEntry(archive, "SqlSyncBuildHistory.xml",
            "<?xml version=\"1.0\" standalone=\"yes\"?><SqlSyncBuildHistory />");
        AddEntry(archive, "SqlSyncBuildProject.xml",
            """
            <?xml version="1.0" standalone="yes"?>
            <SqlSyncBuildData xmlns="http://schemas.mckechney.com/SqlSyncBuildProject.xsd">
              <SqlSyncBuildProject ProjectName="" ScriptTagRequired="true">
                <Scripts>
                  <Script FileName="Insert Local Emulator Test.sql" BuildOrder="1" Description="" RollBackOnError="true" CausesBuildFailure="true" ScriptId="00000000-0000-0000-0000-000000000002" Database="sbm_pg_test" StripTransactionText="true" AllowMultipleRuns="true" AddedBy="local-test" ScriptTimeOut="20" Tag="Default" />
                </Scripts>
                <Builds />
              </SqlSyncBuildProject>
            </SqlSyncBuildData>
            """);
    }

    private static void AddEntry(ZipArchive archive, string name, string content)
    {
        using var writer = new StreamWriter(archive.CreateEntry(name).Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static async Task SeedServiceBusMessageAsync()
    {
        await using var client = new ServiceBusClient(LocalContainerTestEnvironment.ServiceBusConnectionString);
        await using var sender = client.CreateSender("sqlbuildmanager");
        var target = new
        {
            ServerName = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "postgres",
            DbOverrideSequence = new[]
            {
                new
                {
                    ConcurrencyTag = string.Empty,
                    Server = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "postgres",
                    DefaultDbTarget = DatabaseName,
                    OverrideDbTarget = DatabaseName
                }
            },
            ConcurrencyTag = string.Empty
        };
        var message = new ServiceBusMessage(JsonSerializer.Serialize(target))
        {
            Subject = JobName,
            MessageId = Guid.NewGuid().ToString("N")
        };
        await sender.SendMessageAsync(message);
    }

    private static async Task AssertBlobEffectAsync()
    {
        var client = new BlobServiceClient(
            new Uri(LocalContainerTestEnvironment.BlobEndpoint),
            new StorageSharedKeyCredential(StorageAccount, StorageKey));
        var container = client.GetBlobContainerClient(JobName);
        Assert.IsTrue(await container.ExistsAsync(), "The threaded run did not create its Azurite container.");
        var names = new List<string>();
        await foreach (var blob in container.GetBlobsAsync())
        {
            names.Add(blob.Name);
        }
        Assert.IsTrue(
            names.Any(name => name.EndsWith("/commits.log", StringComparison.OrdinalIgnoreCase)),
            "The threaded run did not upload commits.log to Azurite.");
    }

    private static async Task AssertEventHubEffectAsync()
    {
        await using var consumer = new EventHubConsumerClient(
            "cg1",
            LocalContainerTestEnvironment.EventHubConnectionString);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var partitions = await consumer.GetPartitionIdsAsync(timeout.Token);
        foreach (var partition in partitions)
        {
            await foreach (var item in consumer.ReadEventsFromPartitionAsync(
                partition, EventPosition.Earliest, timeout.Token))
            {
                var body = Encoding.UTF8.GetString(item.Data.EventBody.ToArray());
                if (body.Contains(JobName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
        }
        Assert.Fail("The threaded run did not publish an Event Hubs message.");
    }

    private static async Task AssertServiceBusEffectAsync()
    {
        var admin = new ServiceBusAdministrationClient(GetServiceBusAdministrationConnectionString());
        Assert.IsTrue(
            await admin.SubscriptionExistsAsync("sqlbuildmanager", JobName),
            "The threaded run did not create its Service Bus job subscription.");

        await using var client = new ServiceBusClient(LocalContainerTestEnvironment.ServiceBusConnectionString);
        await using var receiver = client.CreateReceiver("sqlbuildmanager", JobName);
        var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
        Assert.IsNull(message, "The threaded run left its Service Bus target message unconsumed.");
    }

    private static string GetServiceBusAdministrationConnectionString()
    {
        return LocalContainerTestEnvironment.ServiceBusConnectionString
            .Replace(":5672/", ":5300/", StringComparison.OrdinalIgnoreCase);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
