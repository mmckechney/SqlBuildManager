using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using cs = SqlBuildManager.Console.CloudStorage;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.EventHubs.Models;
using Azure.ResourceManager.Resources;
using SqlBuildManager.Console.Arm;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace SqlBuildManager.Console.Events
{
    public class EventManager : IDisposable
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string eventHubconnectionString = "";
        private string storageAccountName = "";
        private string storageAccountKey = "";
        private string jobName = "";
        private string eventHubResourceGroup = "";
        private string eventHubSubscription = "";
        private string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
        private DateTime utcMonitorStart = DateTime.UtcNow.AddMinutes(-5);
        private string eventhubNamespace = "";
        private string eventHub = string.Empty;
        private string eventHubCheckpointContainer = "eventhubcheckpoint";



        private int databaseCommitMessages = 0;
        private int databaseErrorMessages = 0;
        private int eventsScanned = 0;
        private int workersCompleted = 0;
        private List<string> committedDbs = new();
        private List<string> errorDbs = new();
 
        

        public bool HasEventMonitorClientError { get; set; } = false;


        private void IncrementDatabaseCommitMessages()
        {
            Interlocked.Increment(ref databaseCommitMessages);
        }
        private void IncrementDatabaseErrorsMessages()
        {
            Interlocked.Increment(ref databaseErrorMessages);
        }
        private void IncrementWorkerCompletedMessages()
        {
            Interlocked.Increment(ref workersCompleted);
        }
        private void IncrementEventsScanned()
        {
            Interlocked.Increment(ref eventsScanned);
        }
        public bool StreamEvents { get; set; } = false;

        public EventManager(string eventHubconnectionString, string eventHubSubscription, string eventHubResourceGroup, string storageAccountName, string storageAccountKey, string jobName)
        {
            this.eventHubconnectionString = eventHubconnectionString;
            this.jobName = jobName;
            this.storageAccountName = storageAccountName;
            this.storageAccountKey = storageAccountKey;
            this.eventHubResourceGroup = eventHubResourceGroup;
            this.eventHubSubscription = eventHubSubscription;
        }

        private BlobContainerClient _blobClient = null;
        private BlobContainerClient BlobClient
        {
            get
            {
                if (_blobClient == null)
                {
                    _blobClient = cs.StorageManager.GetBlobContainerClient(storageAccountName, storageAccountKey, this.eventHubCheckpointContainer);
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
                    (string namespaceName, string hubName) = GetEventHubNamespaceAndName(eventHubconnectionString);
                    this.eventhubNamespace = namespaceName;
                    this.eventHub = hubName;

                    this.consumerGroup = CreateCustomConsumerGroup(eventHubSubscription, eventHubResourceGroup, namespaceName, hubName, this.jobName);
                    if (this.consumerGroup != EventHubConsumerClient.DefaultConsumerGroupName) this.eventHubCheckpointContainer = this.consumerGroup.ToLower().Trim();

                    if (ConnectionStringValidator.IsEventHubConnectionString(eventHubconnectionString))
                    {
                        _eventClient = new EventProcessorClient(BlobClient, this.consumerGroup, eventHubconnectionString);
                    }
                    else
                    {
                        _eventClient = new EventProcessorClient(BlobClient, this.consumerGroup, $"{namespaceName}.servicebus.windows.net", hubName, Aad.AadHelper.TokenCredential);
                    }

                    _eventClient.ProcessEventAsync += ProcessEventHandler;
                    _eventClient.ProcessErrorAsync += ProcessErrorHandler;
                    //Tip to future self.. don't checkpoint the handler, use the time stamp.
                    //This is in case there are more than one instance of the app running at the same time
                    _eventClient.PartitionInitializingAsync += InitializeEventHandler;
                }
                return _eventClient;
            }
        }
        public static (string, string) GetEventHubNamespaceAndName(string input)
        { 
            if(ConnectionStringValidator.IsEventHubConnectionString(input))
            {
                string pattern = @"^Endpoint=sb:\/\/([^.]+)\.(.+)\/;SharedAccessKeyName=.+;SharedAccessKey=.+;EntityPath=(.+)$";
                Match match = Regex.Match(input, pattern);
                string name = match.Groups[1].Value;
                string domainName = match.Groups[2].Value;
                string entityPath = match.Groups[3].Value;
                log.LogInformation($"Using EventHub Namespace: {name} with Event Hub name: {entityPath}");
                return ($"{name}", entityPath);
            }
           

            string namespaceName = "";
            string hubName = "";

            try
            {
                var split = input.Split("|");
                hubName = split[1];
                if (split[0].ToLower().EndsWith("servicebus.windows.net"))
                {
                    namespaceName = split[0].Split('.')[0];
                }
                else
                {
                    namespaceName = split[0];
                }

                log.LogInformation($"Using EventHub Namespace: {namespaceName} with Event Hub name: {hubName}");
                return (namespaceName, hubName);
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to parse EventHub info: {exe.Message}");
                return ("", "");
            }

        }
        public (int, int, int, int) GetCommitErrorScannedAndWorkerCompleteCounts()
        {
            return (databaseCommitMessages, databaseErrorMessages, eventsScanned, workersCompleted);
        }

        private Task InitializeEventHandler(PartitionInitializingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                log.LogDebug($"Initialize EventHub partition: {args.PartitionId} from EnqueueTime {utcMonitorStart}");

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
                log.LogError($"\tPartition '{eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
                log.LogError(eventArgs.Exception.Message);
            }
            catch (Exception exe)
            {
                log.LogError($"Failure to process event error args: {exe.Message}");
                HasEventMonitorClientError = true;
            }
            return Task.CompletedTask;
        }

        private Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            string dbName;
            try
            {
                
                var msg = JsonSerializer.Deserialize<EventHubMessageFormat>(Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
                if ((!string.IsNullOrWhiteSpace(msg.Properties.LogMsg.JobName) && msg.Properties.LogMsg.JobName.ToLower() == jobName.ToLower()) || (this.jobName.ToLower() == "all"))
                {
                    IncrementEventsScanned(); //only count events that are relevant to this job
                    if (log.IsEnabled(LogLevel.Debug) || this.jobName.ToLower() == "all")
                    {
                        var json = JsonSerializer.Serialize(msg);
                        if (log.IsEnabled(LogLevel.Debug))
                        {
                            log.LogDebug($"{json}");
                        }
                        else
                        {
                            if (msg.Level.ToLower() == "error" ||  msg.Properties.LogMsg.LogType == LogType.ScriptError)
                            {
                                log.LogError($"{json}");
                            }
                            else
                            {
                                log.LogInformation($"{json}");
                            }
                        }
                    }
                    if (this.jobName.ToLower() != "all")
                    {
                        switch (msg.Properties.LogMsg.LogType)
                        {
                            case LogType.Commit:
                                if (StreamEvents) log.LogInformation($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.Message.PadRight(31)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}\t{msg.Properties.LogMsg.DatabaseName}");
                                dbName = $"{msg.Properties.LogMsg.ServerName.ToLower().Trim()}:{msg.Properties.LogMsg.DatabaseName.ToLower().Trim()}";
                                if (!committedDbs.Contains(dbName))
                                {
                                    committedDbs.Add(dbName);
                                    IncrementDatabaseCommitMessages();
                                }
                                break;
                            case LogType.Error:
                                if (StreamEvents) log.LogError($"{msg.Properties.LogMsg.LogType.ToString().PadRight(10)}{msg.Properties.LogMsg.Message.PadRight(31)}{msg.Properties.LogMsg.ServerName.ToString().PadRight(20)}\t{msg.Properties.LogMsg.DatabaseName}");
                                dbName = $"{msg.Properties.LogMsg.ServerName.ToLower().Trim()}:{msg.Properties.LogMsg.DatabaseName.ToLower().Trim()}";
                                if (!errorDbs.Contains(dbName))
                                {
                                    errorDbs.Add(dbName);
                                    IncrementDatabaseErrorsMessages();
                                }
                                break;
                            case LogType.WorkerCompleted:
                                if (StreamEvents) log.LogInformation($"{msg.Properties.LogMsg.LogType.ToString().PadRight(25)}{msg.Properties.LogMsg.Message.PadRight(31)}");
                                IncrementWorkerCompletedMessages();
                                break;
                            case LogType.ScriptLog:
                            case LogType.ScriptError:
                            default:
                                var json = JsonSerializer.Serialize(msg);
                                if (StreamEvents) log.LogDebug($"{json}");
                                break;
                        }
                    }
                }
                else
                {
                    log.LogDebug($"Skipped event as not relevent: {eventArgs.Data.SequenceNumber}");
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
            try
            {
                if (!monitorUtcStart.HasValue)
                {
                    utcMonitorStart = DateTime.UtcNow.AddMinutes(-5);
                }
                else
                {
                    utcMonitorStart = monitorUtcStart.Value;
                }
                StreamEvents = stream;
                // Start the processing
                return EventClient.StartProcessingAsync(cancellationToken);
            }
            catch (Exception exe)
            {
                log.LogError($"Error starting Event Processor monitoring: {exe.ToString()}");
                HasEventMonitorClientError = true;
                return Task.CompletedTask;
            }

        }
        
        public string CreateCustomConsumerGroup(string subscriptionId, string resourceGroup, string namespaceName, string hubName, string jobName)
        {
            if (!string.IsNullOrWhiteSpace(subscriptionId) && !string.IsNullOrWhiteSpace(resourceGroup))
            {
                try
                {
                    var groupName = $"sbm-{jobName}-{Guid.NewGuid().ToString().Substring(0, 4)}";
                    var ehResourceId = EventHubResource.CreateResourceIdentifier(subscriptionId, resourceGroup, namespaceName, hubName);
                    var eventHub = ArmHelper.SbmArmClient.GetEventHubResource(ehResourceId);
                    var consumerGroups = eventHub.GetEventHubsConsumerGroups();
                    var result = consumerGroups.CreateOrUpdate(Azure.WaitUntil.Completed,groupName, new EventHubsConsumerGroupData());

                    log.LogInformation($"Created custom Event Hub Consumer Group: {groupName}");
                    return groupName;
                    
                }
                catch(Exception exe)
                {
                    log.LogError($"Unable to create custom consumer group, using Consumer Group: {EventHubConsumerClient.DefaultConsumerGroupName}. Error: {exe.Message}");
                }
            }
            log.LogInformation($"Event Hub Resource Group name and/or subscription ID were not provided. Using Consumer Group:{EventHubConsumerClient.DefaultConsumerGroupName}");
            return EventHubConsumerClient.DefaultConsumerGroupName;
   
        }

        public void RemoveCustomConsumerGroup()
        {
            if (!string.IsNullOrWhiteSpace(this.eventHubSubscription) && !string.IsNullOrWhiteSpace(this.eventHubResourceGroup) && this.consumerGroup != EventHubConsumerClient.DefaultConsumerGroupName)
            {
                try
                {
                    var ehResourceId = EventHubResource.CreateResourceIdentifier(this.eventHubSubscription, this.eventHubResourceGroup, this.eventhubNamespace, this.eventHub);

                    var consumerGroup = EventHubsConsumerGroupResource.CreateResourceIdentifier(this.eventHubSubscription, this.eventHubResourceGroup, this.eventhubNamespace, this.eventHub, this.consumerGroup);
                   var consumerResource = ArmHelper.SbmArmClient.GetEventHubsConsumerGroupResource(consumerGroup);
                    consumerResource.Delete(Azure.WaitUntil.Completed);
                    log.LogInformation($"Removed custom Event Hub Consumer Group: {this.consumerGroup}");

                }
                catch (Exception exe)
                {
                    log.LogError($"Unable to remove custom consumer group {this.consumerGroup}");
                }
            }else
            {
                log.LogInformation($"No custom consumer group to remove.");
            }
        }

        public void Dispose()
        {
            if (_eventClient != null)
            {
                _eventClient.StopProcessing();
                _eventClient.ProcessEventAsync -= ProcessEventHandler;
                _eventClient.ProcessErrorAsync -= ProcessErrorHandler;
                _eventClient = null;

                RemoveCustomConsumerGroup();
            }
            if (_blobClient != null)
            {
                _blobClient = null;
            }
        }
    }
}
