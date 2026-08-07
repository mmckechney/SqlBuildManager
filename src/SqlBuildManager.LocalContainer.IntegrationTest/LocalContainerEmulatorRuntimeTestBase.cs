using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Storage;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.IntegrationTest;

public abstract class LocalContainerEmulatorRuntimeTestBase
{
    private const string StorageAccount = "devstoreaccount1";
    private const string StorageKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuF2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    protected async Task RunThreadedRuntimeTestAsync(
        string platform,
        string server,
        string database,
        string user,
        string password,
        string jobName,
        Func<string, DbConnection> createConnection,
        string createTableSql,
        string insertSql,
        Func<string[], Task<int>> invokeCommandAsync)
    {
        var previousBlobEndpoint = Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT");
        var testDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"local-{platform}-runtime-{Guid.NewGuid():N}");
        var packagePath = Path.Combine(testDirectory, "package.sbm");
        var overridePath = Path.Combine(testDirectory, "targets.cfg");

        Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", LocalContainerTestEnvironment.BlobEndpoint);
        Directory.CreateDirectory(testDirectory);
        try
        {
            await EnsureDatabaseAsync(createConnection, database, createTableSql);
            CreatePackage(packagePath, database, insertSql);
            await File.WriteAllTextAsync(overridePath, $"{server}:{database},{database}");
            await SeedServiceBusMessageAsync(server, database, jobName);

            var args = new[]
            {
                "--loglevel", "debug", "threaded", "run",
                "--rootloggingpath", Path.Combine(testDirectory, "logs"),
                "--transactional", "false", "--trial", "false", "--timeoutretrycount", "0",
                "--concurrency", "1", "--concurrencytype", "Count",
                "--override", overridePath, "--packagename", packagePath, "--jobname", jobName,
                "--storageaccountname", StorageAccount, "--storageaccountkey", StorageKey,
                "--eventhubconnection", LocalContainerTestEnvironment.EventHubConnectionString,
                "--eventhublogging", "EssentialOnly",
                "--servicebustopicconnection", LocalContainerTestEnvironment.ServiceBusConnectionString,
                "--authtype", "Password", "--username", user, "--password", password,
                "--platform", platform
            };

            if (await invokeCommandAsync(args) != 0)
            {
                throw new InvalidOperationException($"{platform} threaded run failed.");
            }
            if (await CountRowsAsync(createConnection, database) != 1)
            {
                throw new InvalidOperationException($"{platform} threaded run did not update the expected database row.");
            }
            await AssertBlobEffectAsync(jobName);
            await AssertEventHubEffectAsync(jobName);
            await AssertServiceBusEffectAsync(jobName);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", previousBlobEndpoint);
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }

    private static async Task EnsureDatabaseAsync(Func<string, DbConnection> createConnection, string database, string sql)
    {
        await using var connection = createConnection(database);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> CountRowsAsync(Func<string, DbConnection> createConnection, string database)
    {
        await using var connection = createConnection(database);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM transactiontest WHERE message = 'LOCAL EMULATOR THREADED TEST'";
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static void CreatePackage(string path, string database, string insertSql)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        AddEntry(archive, "Insert Local Emulator Test.sql", insertSql);
        AddEntry(archive, "SqlSyncBuildHistory.xml", "<?xml version=\"1.0\" standalone=\"yes\"?><SqlSyncBuildHistory />");
        AddEntry(archive, "SqlSyncBuildProject.xml", $"""
            <?xml version="1.0" standalone="yes"?>
            <SqlSyncBuildData xmlns="http://schemas.mckechney.com/SqlSyncBuildProject.xsd">
              <SqlSyncBuildProject ProjectName="" ScriptTagRequired="true">
                <Scripts>
                  <Script FileName="Insert Local Emulator Test.sql" BuildOrder="1" Description="" RollBackOnError="true" CausesBuildFailure="true" ScriptId="00000000-0000-0000-0000-000000000002" Database="{database}" StripTransactionText="true" AllowMultipleRuns="true" AddedBy="local-test" ScriptTimeOut="20" Tag="Default" />
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

    private static async Task SeedServiceBusMessageAsync(string server, string database, string jobName)
    {
        await using var client = new ServiceBusClient(LocalContainerTestEnvironment.ServiceBusConnectionString);
        await using var sender = client.CreateSender("sqlbuildmanager");
        var target = new
        {
            ServerName = server,
            DbOverrideSequence = new[] { new { ConcurrencyTag = string.Empty, Server = server, DefaultDbTarget = database, OverrideDbTarget = database } },
            ConcurrencyTag = string.Empty
        };
        await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(target))
        {
            Subject = jobName,
            MessageId = Guid.NewGuid().ToString("N")
        });
    }

    private static async Task AssertBlobEffectAsync(string jobName)
    {
        var client = new BlobServiceClient(new Uri(LocalContainerTestEnvironment.BlobEndpoint), new StorageSharedKeyCredential(StorageAccount, StorageKey));
        var container = client.GetBlobContainerClient(jobName);
        if (!await container.ExistsAsync())
        {
            throw new InvalidOperationException("The threaded run did not create its Azurite container.");
        }
        var names = new List<string>();
        await foreach (var blob in container.GetBlobsAsync()) names.Add(blob.Name);
        if (!names.Any(name => name.EndsWith("/commits.log", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("The threaded run did not upload commits.log to Azurite.");
        }
    }

    private static async Task AssertEventHubEffectAsync(string jobName)
    {
        await using var consumer = new EventHubConsumerClient("cg1", LocalContainerTestEnvironment.EventHubConnectionString);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        foreach (var partition in await consumer.GetPartitionIdsAsync(timeout.Token))
        {
            await foreach (var item in consumer.ReadEventsFromPartitionAsync(partition, EventPosition.Earliest, timeout.Token))
            {
                if (Encoding.UTF8.GetString(item.Data.EventBody.ToArray()).Contains(jobName, StringComparison.OrdinalIgnoreCase)) return;
            }
        }
        throw new InvalidOperationException("The threaded run did not publish an Event Hubs message.");
    }

    private static async Task AssertServiceBusEffectAsync(string jobName)
    {
        var adminConnection = LocalContainerTestEnvironment.ServiceBusConnectionString.Replace(":5672/", ":5300/", StringComparison.OrdinalIgnoreCase);
        var admin = new ServiceBusAdministrationClient(adminConnection);
        if (!await admin.SubscriptionExistsAsync("sqlbuildmanager", jobName))
        {
            throw new InvalidOperationException("The threaded run did not create its Service Bus job subscription.");
        }
        await using var client = new ServiceBusClient(LocalContainerTestEnvironment.ServiceBusConnectionString);
        await using var receiver = client.CreateReceiver("sqlbuildmanager", jobName);
        if (await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5)) != null)
        {
            throw new InvalidOperationException("The threaded run left its Service Bus target message unconsumed.");
        }
    }
}
