﻿using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Events
{
    public class EventManager : IDisposable
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string eventHubconnectionString = "";
        private string storageContainerName = "";
        private string storageConnectionString = "";
        private string jobName = "";
        private string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
        private DateTime utcMonitorStart = DateTime.UtcNow;

        private int databaseCommitMessages = 0;
        private int databaseErrorMessages = 0;
        private int eventsScanned = 0;


        private void IncrementDatabaseCommitMessages()
        {
            Interlocked.Increment(ref databaseCommitMessages);
        }
        private void IncrementDatabaseErrorsMessages()
        {
            Interlocked.Increment(ref databaseErrorMessages);
        }
        private void IncrementEventsScanned()
        {
            Interlocked.Increment(ref eventsScanned);
        }
        public bool StreamEvents { get;set; } = false;

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
                    _blobClient = new BlobContainerClient(storageConnectionString, "eventhubcheckpoint");
                    _blobClient.CreateIfNotExistsAsync().Wait();
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
                    //Tip to future self.. don't checkpoint the handler, use the time stamp.
                    //This is in case there are more than one instance of the app running at the same time
                    _eventClient.PartitionInitializingAsync += InitializeEventHandler;
                }
                return _eventClient;
            }
        }
        public (int, int,int) GetCommitErrorAndScannedCounts()
        {
            return (this.databaseCommitMessages, this.databaseErrorMessages, this.eventsScanned);
        }

        private Task InitializeEventHandler(PartitionInitializingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

               log.LogDebug($"Initialize EventHub partition: { args.PartitionId } from EnqueueTime {utcMonitorStart}");

                // If no checkpoint was found, start processing events enqueued now -5 minutes or in the future.
                // This should be the case -- rely on timestamp, not a checkpoint to ensure nothing is missed across multiple instances
                EventPosition startPositionWhenNoCheckpoint = EventPosition.FromEnqueuedTime(utcMonitorStart);
                args.DefaultStartingPosition = startPositionWhenNoCheckpoint;
            }
            catch
            {
            }

            return Task.CompletedTask;
        }
        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            try
            {
                // Write details about the error to the console window
                log.LogError($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
                log.LogError(eventArgs.Exception.Message);
            }
            catch(Exception exe)
            {
                log.LogError($"Failure to process event error args: {exe.Message}");
            }
            return Task.CompletedTask;
        }

        private Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            try
            {
                this.IncrementEventsScanned();
                var msg = JsonSerializer.Deserialize<EventHubMessageFormat>(Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
                //log.LogInformation($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}{msg.Properties.LogMsg.DatabaseName}");
                if (!string.IsNullOrWhiteSpace(msg.Properties.LogMsg.JobName) && msg.Properties.LogMsg.JobName.ToLower() == this.jobName.ToLower())
                {
                    switch (msg.Properties.LogMsg.LogType)
                    {
                        case LogType.Commit:
                            if (this.StreamEvents) log.LogInformation($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.Message.PadRight(13)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}\t{msg.Properties.LogMsg.DatabaseName}");
                            this.IncrementDatabaseCommitMessages();
                            break;
                        case LogType.Error:
                            if (this.StreamEvents) log.LogError($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.Message.PadRight(13)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}\t{msg.Properties.LogMsg.DatabaseName}");
                            this.IncrementDatabaseErrorsMessages();
                            break;
                    }
                }

            }
            catch (Exception exe)
            {
                log.LogError($"Failure to process event message: {exe.Message}");
            }
            return Task.CompletedTask;
        }

        public Task MonitorEventHub(bool stream, DateTime? monitorUtcStart, CancellationToken cancellationToken)
        {

            if (!monitorUtcStart.HasValue)
            {
                utcMonitorStart = DateTime.UtcNow.AddMinutes(-5);
            }else
            {
                utcMonitorStart = monitorUtcStart.Value;
            }
            this.StreamEvents = stream;
            // Start the processing
            return this.EventClient.StartProcessingAsync(cancellationToken);

        }

        public void Dispose()
        {
            if(this._eventClient != null)
            {
                this._eventClient.StopProcessing();
                this._eventClient.ProcessEventAsync -= ProcessEventHandler;
                this._eventClient.ProcessErrorAsync -= ProcessErrorHandler;
                this._eventClient = null;
            }
            if(this._blobClient != null)
            {
                this._blobClient = null;
            }
        }
    }
}
