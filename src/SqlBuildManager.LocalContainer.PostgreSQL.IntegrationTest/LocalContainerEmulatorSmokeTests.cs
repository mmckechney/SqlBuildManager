using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.ServiceBus;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Events;
using SqlBuildManager.LocalContainer.IntegrationTest;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest;

[TestClass]
public class LocalContainerEmulatorSmokeTests
{
    private static void RequireEmulators()
    {
        if (!LocalContainerTestEnvironment.EmulatorsConfigured)
        {
            Assert.Inconclusive("Emulator tests require SBM_TEST_BLOB_ENDPOINT, SBM_TEST_EVENTHUB_CONNECTION_STRING, and SBM_TEST_SERVICEBUS_CONNECTION_STRING.");
        }
    }

    [TestMethod]
    public async Task BlobCanCreateUploadAndRead()
    {
        RequireEmulators();
        var client = new BlobServiceClient(
            new Uri(LocalContainerTestEnvironment.BlobEndpoint),
            new StorageSharedKeyCredential(
                "devstoreaccount1",
                "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
        var container = client.GetBlobContainerClient("sbm-emulator-test");
        await container.CreateIfNotExistsAsync();
        var blob = container.GetBlobClient(Guid.NewGuid().ToString("N"));
        await blob.UploadAsync(BinaryData.FromString("local-container"), overwrite: true);
        Assert.AreEqual("local-container", (await blob.DownloadContentAsync()).Value.Content.ToString());
        await blob.DeleteIfExistsAsync();
    }

    [TestMethod]
    public async Task EventHubsCanProduceAndConsume()
    {
        RequireEmulators();
        var connection = LocalContainerTestEnvironment.EventHubConnectionString;
        var message = Guid.NewGuid().ToString("N");
        await using var producer = new EventHubProducerClient(connection);
        using var batch = await producer.CreateBatchAsync();
        batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));
        await producer.SendAsync(batch);

        await using var consumer = new EventHubConsumerClient("cg1", connection);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var partition = (await consumer.GetPartitionIdsAsync(timeout.Token)).First();
        await foreach (var item in consumer.ReadEventsFromPartitionAsync(partition, EventPosition.Earliest, timeout.Token))
        {
            if (Encoding.UTF8.GetString(item.Data.EventBody.ToArray()) == message)
            {
                return;
            }
        }
        Assert.Fail("The Event Hubs message was not received.");
    }

    [TestMethod]
    public async Task ServiceBusTopicCanAdministerSendAndReceive()
    {
        RequireEmulators();
        const string topic = "sqlbuildmanager";
        const string subscription = "sbm-emulator-test";
        var message = Guid.NewGuid().ToString("N");
        await using var client = new ServiceBusClient(LocalContainerTestEnvironment.ServiceBusConnectionString);
        await using var sender = client.CreateSender(topic);
        await sender.SendMessageAsync(new ServiceBusMessage(message));
        await using var receiver = client.CreateReceiver(topic, subscription);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var received = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(20), timeout.Token);
        Assert.IsNotNull(received);
        Assert.AreEqual(message, received!.Body.ToString());
        await receiver.CompleteMessageAsync(received);
    }

    [TestMethod]
    public void EventHubEmulatorConnectionStringIsParsed()
    {
        var result = EventManager.GetEventHubNamespaceAndName(
            "Endpoint=sb://eventhubs-emulator:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY;EntityPath=sbm-events");
        Assert.AreEqual("eventhubs-emulator:5672", result.Item1);
        Assert.AreEqual("sbm-events", result.Item2);
    }
}
