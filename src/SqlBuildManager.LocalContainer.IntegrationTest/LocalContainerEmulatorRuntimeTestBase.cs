using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    protected const string TestMessage = "LOCAL EMULATOR THREADED TEST";
    private const string StorageAccount = "devstoreaccount1";
    private const string StorageKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    private const string EmulatorCountJobName = "sbm-emulator-test";
    private const string EmulatorSessionJobName = "sbm-emulator-session";

    [TestInitialize]
    public void RequireContainerHarness()
    {
        LocalContainerTestEnvironment.RequireRuntimeInfrastructure();
    }

    private static string CreateTestDirectory(string prefix)
    {
        return Path.Combine("/tests/TestResults", $"{prefix}-{Guid.NewGuid():N}");
    }

    protected static string CreateRuntimeTestDirectory(string prefix) => CreateTestDirectory(prefix);

    protected async Task RunThreadedRuntimeTestAsync(
        string platform,
        string server,
        IReadOnlyList<string> databases,
        string user,
        string password,
        string jobName,
        string concurrencyType,
        int concurrency,
        Func<string, DbConnection> createConnection,
        string createTableSql,
        string insertSql)
    {
        LocalContainerTestEnvironment.RequireRuntimeInfrastructure();
        jobName = GetEffectiveJobName(concurrencyType);
        var previousBlobEndpoint = Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT");
        var testDirectory = CreateTestDirectory($"local-{platform}-runtime");
        var packagePath = Path.Combine(testDirectory, "package.sbm");
        var overridePath = Path.Combine(testDirectory, "targets.cfg");

        Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", LocalContainerTestEnvironment.BlobEndpoint);
        Directory.CreateDirectory(testDirectory);
        try
        {
            foreach (var database in databases)
            {
                await EnsureDatabaseAsync(createConnection, database, createTableSql);
            }

            CreatePackage(packagePath, databases[0], insertSql);
            await File.WriteAllLinesAsync(overridePath, databases.Select(database => $"{server}:{database},{database}"));
            await EnsureServiceBusSubscriptionAsync(jobName, concurrencyType);
            await SeedServiceBusMessagesAsync(server, databases[0], databases, jobName, concurrencyType);
            var eventHubStart = DateTimeOffset.UtcNow;

            var argList = new List<string>
            {
                "--loglevel", "debug", "threaded", "run",
                "--rootloggingpath", Path.Combine(testDirectory, "logs"),
                "--transactional", "true", "--trial", "false", "--timeoutretrycount", "0",
                "--concurrency", concurrency.ToString(), "--concurrencytype", concurrencyType,
                "--override", overridePath, "--packagename", packagePath, "--jobname", jobName,
                "--storageaccountname", StorageAccount, "--storageaccountkey", StorageKey,
                "--eventhubconnection", LocalContainerTestEnvironment.EventHubConnectionString,
                "--eventhublogging", "IndividualScriptResults",
                "--servicebustopicconnection", LocalContainerTestEnvironment.ServiceBusConnectionString,
                "--authtype", "Password", "--username", user, "--password", password,
                "--platform", platform, "--monitor", "true", "--stream"
            };

            if (platform.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                // The local SQL Server emulator container uses a self-signed certificate; trust it
                // explicitly rather than requiring full chain validation like a production server would need.
                argList.AddRange(new[] { "--trustservercertificate", "true" });
            }

            var args = argList.ToArray();

            await RunWorkerContainersAsync(platform, args);

            foreach (var database in databases)
            {
                if (await CountRowsAsync(createConnection, database) < 1)
                {
                    throw new InvalidOperationException($"{platform} threaded run did not update {database}.");
                }
            }

            await AssertBlobEffectAsync(jobName);
            await AssertEventHubEffectAsync(jobName, databases.Count, eventHubStart);
            await AssertServiceBusEffectAsync(jobName, concurrencyType);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", previousBlobEndpoint);
        }
    }

    /// <summary>
    /// Local-container equivalent of Aci_Queue_LongRunning_SBMSource_ByConcurrencyType_Success: builds a package
    /// whose single script sleeps for a platform-appropriate duration before completing, then runs it through
    /// `threaded run` (the local analog of `aci run` since no container orchestration is exercised locally).
    /// </summary>
    protected async Task RunLongRunningRuntimeTestAsync(
        string platform,
        string server,
        IReadOnlyList<string> databases,
        string user,
        string password,
        string jobName,
        string concurrencyType,
        int concurrency,
        Func<string, DbConnection> createConnection,
        string createTableSql,
        string longRunningSql,
        int scriptTimeoutSeconds)
    {
        LocalContainerTestEnvironment.RequireRuntimeInfrastructure();
        jobName = GetEffectiveJobName(concurrencyType);
        var previousBlobEndpoint = Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT");
        var testDirectory = CreateTestDirectory($"local-{platform}-longrunning");
        var packagePath = Path.Combine(testDirectory, "package.sbm");
        var overridePath = Path.Combine(testDirectory, "targets.cfg");

        Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", LocalContainerTestEnvironment.BlobEndpoint);
        Directory.CreateDirectory(testDirectory);
        try
        {
            foreach (var database in databases)
            {
                await EnsureDatabaseAsync(createConnection, database, createTableSql);
            }

            CreateLongRunningPackage(packagePath, databases[0], longRunningSql, scriptTimeoutSeconds);
            await File.WriteAllLinesAsync(overridePath, databases.Select(database => $"{server}:{database},{database}"));
            await EnsureServiceBusSubscriptionAsync(jobName, concurrencyType);
            await SeedServiceBusMessagesAsync(server, databases[0], databases, jobName, concurrencyType);
            var eventHubStart = DateTimeOffset.UtcNow;

            var argList = new List<string>
            {
                "--loglevel", "debug", "threaded", "run",
                "--rootloggingpath", Path.Combine(testDirectory, "logs"),
                "--transactional", "true", "--trial", "false", "--timeoutretrycount", "0",
                "--concurrency", concurrency.ToString(), "--concurrencytype", concurrencyType,
                "--override", overridePath, "--packagename", packagePath, "--jobname", jobName,
                "--storageaccountname", StorageAccount, "--storageaccountkey", StorageKey,
                "--eventhubconnection", LocalContainerTestEnvironment.EventHubConnectionString,
                "--eventhublogging", "IndividualScriptResults",
                "--servicebustopicconnection", LocalContainerTestEnvironment.ServiceBusConnectionString,
                "--authtype", "Password", "--username", user, "--password", password,
                "--platform", platform, "--monitor", "true", "--stream"
            };

            if (platform.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                // The local SQL Server emulator container uses a self-signed certificate; trust it
                // explicitly rather than requiring full chain validation like a production server would need.
                argList.AddRange(new[] { "--trustservercertificate", "true" });
            }

            var args = argList.ToArray();

            await RunWorkerContainersAsync(platform, args);

            foreach (var database in databases)
            {
                if (await CountRowsAsync(createConnection, database) < 1)
                {
                    throw new InvalidOperationException($"{platform} long-running threaded run did not update {database}.");
                }
            }

            await AssertBlobEffectAsync(jobName);
            await AssertEventHubEffectAsync(jobName, databases.Count, eventHubStart);
            await AssertServiceBusEffectAsync(jobName, concurrencyType);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", previousBlobEndpoint);
        }
    }

    private static void CreateLongRunningPackage(string path, string database, string longRunningSql, int scriptTimeoutSeconds)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        AddEntry(archive, "Long Running Local Emulator Test.sql", longRunningSql);
        AddEntry(archive, "SqlSyncBuildHistory.xml", "<?xml version=\"1.0\" standalone=\"yes\"?><SqlSyncBuildHistory />");
        AddEntry(archive, "SqlSyncBuildProject.xml", $"""
            <?xml version="1.0" standalone="yes"?>
            <SqlSyncBuildData xmlns="http://schemas.mckechney.com/SqlSyncBuildProject.xsd">
              <SqlSyncBuildProject ProjectName="" ScriptTagRequired="true">
                <Scripts>
                  <Script FileName="Long Running Local Emulator Test.sql" BuildOrder="1" Description="" RollBackOnError="true" CausesBuildFailure="true" ScriptId="00000000-0000-0000-0000-000000000003" Database="{database}" StripTransactionText="true" AllowMultipleRuns="true" AddedBy="local-test" ScriptTimeOut="{scriptTimeoutSeconds}" Tag="Default" />
                </Scripts>
                <Builds />
              </SqlSyncBuildProject>
            </SqlSyncBuildData>
            """);
    }

    private static async Task SetPackageDatabaseAsync(string packagePath, string database)
    {
        using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Update);
        var xmlEntries = archive.Entries
            .Where(entry => entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (xmlEntries.Length == 0)
        {
            throw new InvalidOperationException("The generated DACPAC package does not contain XML project metadata.");
        }

        foreach (var entry in xmlEntries)
        {
            string xml;
            await using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                xml = await reader.ReadToEndAsync();
            }

            entry.Delete();
            var replacement = archive.CreateEntry(entry.FullName);
            await using var replacementStream = replacement.Open();
            await using var writer = new StreamWriter(replacementStream);
            await writer.WriteAsync(xml.Replace("client", database, StringComparison.OrdinalIgnoreCase));
        }
    }

    protected async Task RunThreadedDacpacRuntimeTestAsync(
        string platform,
        string server,
        IReadOnlyList<string> databases,
        string user,
        string password,
        string jobName,
        string concurrencyType,
        int concurrency,
        string dacpacPath,
        string packagePath,
        bool forceCustomDacpac,
        Func<string, Task> verifyDatabaseAsync)
    {
        LocalContainerTestEnvironment.RequireRuntimeInfrastructure();
        jobName = GetEffectiveJobName(concurrencyType);
        var previousBlobEndpoint = Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT");
        var testDirectory = CreateTestDirectory($"local-{platform}-dacpac");
        var overridePath = Path.Combine(testDirectory, "targets.cfg");

        Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", LocalContainerTestEnvironment.BlobEndpoint);
        Directory.CreateDirectory(testDirectory);
        try
        {
            await SetPackageDatabaseAsync(packagePath, databases[0]);
            await File.WriteAllLinesAsync(overridePath, databases.Select(database => $"{server}:{database},{database}"));
            await EnsureServiceBusSubscriptionAsync(jobName, concurrencyType);
            await SeedServiceBusMessagesAsync(server, databases[0], databases, jobName, concurrencyType);
            var eventHubStart = DateTimeOffset.UtcNow;

            var argList = new List<string>
            {
                "--loglevel", "debug", "threaded", "run",
                "--rootloggingpath", Path.Combine(testDirectory, "logs"),
                "--transactional", "true", "--trial", "false", "--timeoutretrycount", "0",
                "--concurrency", concurrency.ToString(), "--concurrencytype", concurrencyType,
                "--override", overridePath, "--packagename", packagePath,
                "--platinumdacpac", dacpacPath, "--jobname", jobName,
                "--storageaccountname", StorageAccount, "--storageaccountkey", StorageKey,
                "--eventhubconnection", LocalContainerTestEnvironment.EventHubConnectionString,
                "--eventhublogging", "IndividualScriptResults",
                "--servicebustopicconnection", LocalContainerTestEnvironment.ServiceBusConnectionString,
                "--authtype", "Password", "--username", user, "--password", password,
                "--platform", platform, "--monitor", "true", "--stream"
            };

            if (forceCustomDacpac)
            {
                argList.AddRange(new[] { "--forcecustomdacpac", "true" });
            }

            if (platform.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                argList.AddRange(new[] { "--trustservercertificate", "true" });
            }

            await RunWorkerContainersAsync(platform, argList.ToArray());

            foreach (var database in databases)
            {
                await verifyDatabaseAsync(database);
            }

            await AssertBlobEffectAsync(jobName);
            await AssertEventHubEffectAsync(jobName, databases.Count, eventHubStart);
            await AssertServiceBusEffectAsync(jobName, concurrencyType);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SBM_BLOB_ENDPOINT", previousBlobEndpoint);
        }
    }

    protected static async Task RunRuntimeCommandAsync(params string[] arguments)
    {
        LocalContainerTestEnvironment.RequireRuntimeInfrastructure();
        var result = await RuntimeContainerClient.RunAsync(default, arguments);
        Console.WriteLine(result.Output);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"The runtime container exited with code {result.ExitCode}.{Environment.NewLine}{result.Output}");
        }
    }

    private static string GetEffectiveJobName(string concurrencyType)
    {
        return concurrencyType.Equals("Count", StringComparison.OrdinalIgnoreCase)
            ? EmulatorCountJobName
            : EmulatorSessionJobName;
    }

    private static async Task RunWorkerContainersAsync(string platform, params string[] arguments)
    {
        var results = await RuntimeContainerClient.RunManyAsync(CancellationToken.None, arguments);
        foreach (var result in results)
        {
            Console.WriteLine(result.Output);
        }

        var failures = results.Where(result => result.ExitCode != 0).ToArray();
        if (failures.Length > 0)
        {
            throw new InvalidOperationException(
                $"{failures.Length} of {results.Length} {platform} runtime containers failed.");
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
        command.CommandText = $"SELECT COUNT(*) FROM transactiontest WHERE message = '{TestMessage}'";
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

    private static async Task SeedServiceBusMessagesAsync(
        string server,
        string defaultDatabase,
        IReadOnlyList<string> databases,
        string jobName,
        string concurrencyType)
    {
        await using var client = new ServiceBusClient(LocalContainerTestEnvironment.ServiceBusConnectionString);
        await using var sender = client.CreateSender("sqlbuildmanager");
        for (var index = 0; index < databases.Count; index++)
        {
            var database = databases[index];
            var target = new
            {
                ServerName = server,
                DbOverrideSequence = new[] { new { ConcurrencyTag = string.Empty, Server = server, DefaultDbTarget = defaultDatabase, OverrideDbTarget = database } },
                ConcurrencyTag = string.Empty
            };
            var message = new ServiceBusMessage(JsonSerializer.Serialize(target))
            {
                Subject = jobName,
                MessageId = $"{jobName}-{database}"
            };
            if (!concurrencyType.Equals("Count", StringComparison.OrdinalIgnoreCase))
            {
                // Model independent server sessions so every runtime worker can consume
                // concurrently without the emulator invalidating another receiver's lock.
                message.SessionId = $"{server}-runtime-{index + 1}";
            }

            await sender.SendMessageAsync(message);
        }
    }

    private static async Task EnsureServiceBusSubscriptionAsync(string jobName, string concurrencyType)
    {
        var adminConnection = LocalContainerTestEnvironment.ServiceBusConnectionString
            .Replace(":5672/", ":5300/", StringComparison.OrdinalIgnoreCase);
        var admin = new ServiceBusAdministrationClient(adminConnection);
        var subscriptionName = concurrencyType.Equals("Count", StringComparison.OrdinalIgnoreCase)
            ? jobName
            : $"{jobName}session";

        if (await admin.SubscriptionExistsAsync("sqlbuildmanager", subscriptionName))
        {
            await EnsureServiceBusRuleAsync(admin, subscriptionName, jobName);
            await Task.Delay(TimeSpan.FromSeconds(2));
            return;
        }

        var options = new CreateSubscriptionOptions("sqlbuildmanager", subscriptionName)
        {
            RequiresSession = !concurrencyType.Equals("Count", StringComparison.OrdinalIgnoreCase),
            LockDuration = TimeSpan.FromMinutes(5)
        };
        await admin.CreateSubscriptionAsync(options);
        await EnsureServiceBusRuleAsync(admin, subscriptionName, jobName);
        await Task.Delay(TimeSpan.FromSeconds(2));
    }

    private static async Task EnsureServiceBusRuleAsync(
        ServiceBusAdministrationClient admin,
        string subscriptionName,
        string jobName)
    {
        if (await admin.RuleExistsAsync("sqlbuildmanager", subscriptionName, jobName))
        {
            return;
        }

        await admin.CreateRuleAsync(
            "sqlbuildmanager",
            subscriptionName,
            new CreateRuleOptions(
                jobName,
                new CorrelationRuleFilter { Subject = jobName }));
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
        await foreach (var blob in container.GetBlobsAsync())
        {
            names.Add(blob.Name);
        }

        if (!names.Any(name => name.EndsWith("/commits.log", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("The threaded run did not upload commits.log to Azurite.");
        }
    }

    private static async Task AssertEventHubEffectAsync(string jobName, int expectedMessages, DateTimeOffset startTime)
    {
        await using var consumer = new EventHubConsumerClient("cg1", LocalContainerTestEnvironment.EventHubConnectionString);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var startPosition = EventPosition.FromEnqueuedTime(startTime.AddSeconds(-5));
        var partitionIds = await consumer.GetPartitionIdsAsync(timeout.Token);

        // Each partition's read is a live, never-completing stream, so partitions must be scanned
        // concurrently rather than sequentially -- otherwise a partition with no further matching
        // events blocks forever and later partitions are never reached before the timeout.
        var matchingMessages = 0;
        var tasks = partitionIds.Select(async partition =>
        {
            try
            {
                await foreach (var item in consumer.ReadEventsFromPartitionAsync(partition, startPosition, timeout.Token))
                {
                    if (Encoding.UTF8.GetString(item.Data.EventBody.ToArray()).Contains(jobName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Interlocked.Increment(ref matchingMessages) >= expectedMessages)
                        {
                            timeout.Cancel();
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        if (matchingMessages < expectedMessages)
        {
            throw new InvalidOperationException($"The threaded run published {matchingMessages} of {expectedMessages} expected Event Hubs messages.");
        }
    }

    private static async Task AssertServiceBusEffectAsync(string jobName, string concurrencyType)
    {
        var adminConnection = LocalContainerTestEnvironment.ServiceBusConnectionString.Replace(":5672/", ":5300/", StringComparison.OrdinalIgnoreCase);
        var admin = new ServiceBusAdministrationClient(adminConnection);
        var subscriptionName = concurrencyType.Equals("Count", StringComparison.OrdinalIgnoreCase)
            ? jobName
            : $"{jobName}session";

        // A successful `threaded run` deletes its own job subscription as part of cleanup,
        // so the subscription's absence here is confirmation that targets were fully consumed.
        if (await admin.SubscriptionExistsAsync("sqlbuildmanager", subscriptionName))
        {
            var clientOptions = new ServiceBusClientOptions
            {
                RetryOptions =
                {
                    MaxRetries = 0,
                    TryTimeout = TimeSpan.FromSeconds(5)
                }
            };
            await using var client = new ServiceBusClient(
                LocalContainerTestEnvironment.ServiceBusConnectionString,
                clientOptions);
            ServiceBusReceiver receiver;
            if (concurrencyType.Equals("Count", StringComparison.OrdinalIgnoreCase))
            {
                receiver = client.CreateReceiver("sqlbuildmanager", subscriptionName);
            }
            else
            {
                try
                {
                    receiver = await client.AcceptNextSessionAsync("sqlbuildmanager", subscriptionName);
                }
                catch (ServiceBusException exception)
                    when (exception.Reason == ServiceBusFailureReason.ServiceTimeout)
                {
                    return;
                }
            }

            await using (receiver)
            {
                if (await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5)) != null)
                {
                    throw new InvalidOperationException("The threaded run left a Service Bus target message unconsumed.");
                }
            }
        }
    }
}
