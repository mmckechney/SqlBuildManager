using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using SqlBuildManager.Logging.Threaded;
using SqlBuildManager.Interfaces.Console;
namespace SqlBuildManager.Console.Events
{
    public class EventManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string eventHubconnectionString = "";
        private string storageContainerName = "";
        private string storageConnectionString = "";
        private string jobName = "";
        private string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

        public int DatabaseCommits { get; set; } = 0;
        public int DatabaseErrors { get; set; } = 0;

        public EventManager(string eventHubconnectionString, string storageConnectionString, string storageContainerName, string jobName)
        {
            this.eventHubconnectionString = eventHubconnectionString;
            this.jobName = jobName;
            this.storageConnectionString = storageConnectionString;
            this.storageContainerName = storageContainerName;
        }

        private BlobContainerClient _blobClient = null;
        private BlobContainerClient BlobClient
        {
            get
            {
                if(_blobClient == null)
                {
                    _blobClient = new BlobContainerClient(storageConnectionString, storageContainerName);
                }
                return _blobClient;
            }
        }

        private EventProcessorClient _eventClient = null;
        private EventProcessorClient EventClient
        {
            get
            {
                if (_eventClient == null)
                {
                    _eventClient = new EventProcessorClient(this.BlobClient, consumerGroup, eventHubconnectionString);
                    _eventClient.ProcessEventAsync += ProcessEventHandler;
                    _eventClient.ProcessErrorAsync += ProcessErrorHandler;
                }
                return _eventClient;
            }
        }
        public (int, int) GetCommitAndErrorCounts()
        {
            return (this.DatabaseCommits, this.DatabaseErrors);
        }
        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            log.LogError($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            log.LogError(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            var msg = JsonSerializer.Deserialize<EventHubMessageFormat>(Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
            //log.LogInformation($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}{msg.Properties.LogMsg.DatabaseName}");
            if (!string.IsNullOrWhiteSpace(msg.Properties.LogMsg.JobName) && msg.Properties.LogMsg.JobName.ToLower() == this.jobName.ToLower())
            {
                switch(msg.Properties.LogMsg.LogType)
                {
                    case LogType.Commit:
                        log.LogInformation($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.Message.PadRight(13)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}\t{msg.Properties.LogMsg.DatabaseName}");
                        this.DatabaseCommits++;
                        break;
                    case LogType.Error:
                        log.LogError($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.Message.PadRight(13)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}\t{msg.Properties.LogMsg.DatabaseName}");
                        this.DatabaseErrors++;
                        break;
                }
            }
            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        public Task MonitorEventHub()
        {
            // Start the processing
            return this.EventClient.StartProcessingAsync();

        }
    }
}
