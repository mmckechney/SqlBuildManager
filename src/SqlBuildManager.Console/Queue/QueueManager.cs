﻿using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Polly;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace SqlBuildManager.Console.Queue
{
    public class QueueManager : IDisposable
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string topicSubscriptionName = "sbmsubscription";
        private readonly string topicSessionSubscriptionName = "sbmsubscriptionsession";
        private readonly string topicName = "sqlbuildmanager";

        private string topicConnectionString;
        private string jobName;
        private ConcurrencyType concurrencyType;

        private ServiceBusClient _client = null;
        private ServiceBusAdministrationClient _adminClient = null;
        private ServiceBusSessionReceiver _sessionReceiver = null;
        private ServiceBusReceiver _messageReceiver = null;
        private CancellationTokenSource tokenSource = null;

        public QueueManager(string topicConnectionString, string jobName, ConcurrencyType concurrencyType, bool unitest = false)
        {
            this.topicConnectionString = topicConnectionString;
            this.jobName = jobName;

            topicSubscriptionName = jobName;
            topicSessionSubscriptionName = $"{jobName}session";
            this.concurrencyType = concurrencyType;
            if (!unitest)
            {
                CreateSubscriptions().Wait();
            }
        }

        private string EnsureQualifiedNamespace(string input)
        {
            //Do we just have the Service Bus name?
            if (input.ToLower().IndexOf("servicebus.windows.net") == -1)
            {
                input += ".servicebus.windows.net";
            }
            log.LogInformation($"Using Service Bus: {input}");
            return input;
        }
        public ServiceBusClient Client
        {
            get
            {
                if (_client == null)
                {
                    if (ConnectionStringValidator.IsServiceBusConnectionString(topicConnectionString))
                    {
                        _client = new ServiceBusClient(topicConnectionString);
                    }
                    else
                    {
                        //If not a full connection string, should be a qualified namespace for Azure Identity Auth
                        var tmp = EnsureQualifiedNamespace(topicConnectionString);
                        _client = new ServiceBusClient(tmp, Aad.AadHelper.TokenCredential);

                    }
                }
                return _client;
            }
        }
        public ServiceBusAdministrationClient AdminClient
        {
            get
            {
                if (_adminClient == null)
                {
                    if (ConnectionStringValidator.IsServiceBusConnectionString(topicConnectionString))
                    {
                        _adminClient = new ServiceBusAdministrationClient(topicConnectionString);
                    }
                    else
                    {
                        //If not a full connection string, should be a qualified namespace for Azure Identity Auth
                        var tmp = EnsureQualifiedNamespace(topicConnectionString);
                        _adminClient = new ServiceBusAdministrationClient(tmp, Aad.AadHelper.TokenCredential);
                    }
                }
                return _adminClient;
            }
        }
        public ServiceBusReceiver MessageReceiver
        {
            get
            {
                if (_messageReceiver == null)
                {
                    _messageReceiver = Client.CreateReceiver(topicName, topicSubscriptionName, new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });
                }
                return _messageReceiver;
            }
        }


        public async Task<int> SendTargetsToQueue(MultiDbData multiDb, ConcurrencyType cType)
        {
            try
            {
                if ( (cType == ConcurrencyType.Tag || cType == ConcurrencyType.MaxPerTag) && multiDb.AsQueryable().Where(m => m.Overrides.Where(o => o.ConcurrencyTag.Length == 0).Any()).Any())
                {
                    log.LogError($"There are database targets that do not have a concurrency tag. This is required when the Concurrency Type is '{cType.ToString()}'. Please add a concurrency tag to all database targets before sending to the queue.");
                    return 0;
                }
                log.LogInformation($"Setting up Topic Subscription with Job filter name '{jobName}'");
                await RemoveDefaultFilters();
                await CleanUpCustomFilters();
                await CreateBatchJobFilter(cType == ConcurrencyType.Count ? false : true);

                var sender = Client.CreateSender(topicName);

                //Use bucketing to 1 bucket to get flattened list of targest
                var concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, 1, ConcurrencyType.Count);
                var messages = CreateMessages(concurrencyBuckets, jobName, cType);
                if(messages == null)
                {
                    return 0;
                }
                int count = messages.Count();
                int sentCount = 0;

                //because of partitiioning, can't batch across session Id, so group by SessionId first, then batch
                var bySessionId = messages.GroupBy(s => s.SessionId);
                foreach (var sessionSet in bySessionId)
                {
                    var msgBatch = sessionSet.Batch(20); //send in batches of 20
                    foreach (var b in msgBatch)
                    {
                        var sbb = await sender.CreateMessageBatchAsync();
                        foreach (var msg in b)
                        {
                            if (!sbb.TryAddMessage(msg))
                            {
                                log.LogError($"Failed to add message to Service Bus batch.{Environment.NewLine}{msg.Body}");
                            }
                            else
                            {
                                sentCount++;
                            }
                        }
                        await sender.SendMessagesAsync(sbb);
                    }
                }
                if (sentCount != count)
                {
                    log.LogError($"Only {sentCount} out of {count} database targets were sent to the Service Bus. Before running your workload, please run a 'dequeue' command and try again");
                    return -1;
                }

                //Confirm message count in Queue 
                int retry = 0;
                var activeMessages = await MonitorServiceBustopic(cType);
                while (activeMessages < count && retry < 4)
                {
                    Thread.Sleep(1000);
                    activeMessages = await MonitorServiceBustopic(cType);
                }

                if (activeMessages != count)
                {

                    log.LogError($"After attempting to queue messages, there are only {activeMessages} out of {count} messages in the Service Bus Subscription. Before running your workload, please run a 'dequeue' command and try again");
                    return -1;
                }
                else
                {
                    log.LogInformation($"Validated {activeMessages} of {count} active messages in Service Bus Subscription {topicName}:{topicSessionSubscriptionName}");
                }

                return count;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Failed to send database override targets to Service Bus Queue");
                return -1;
            }

        }
        public List<ServiceBusMessage> CreateMessages(List<IEnumerable<(string, List<DatabaseOverride>)>> buckets, string jobName, ConcurrencyType cType)
        {
            try
            {
                List<ServiceBusMessage> msgs = new List<ServiceBusMessage>();
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;

                foreach (var bucket in buckets)
                {
                    foreach (var target in bucket)
                    {
                        TargetMessage data;
                        if (target.Item1.StartsWith("#"))
                        {
                            data = new TargetMessage() { ServerName = target.Item2[0].Server, DbOverrideSequence = target.Item2, ConcurrencyTag = target.Item2[0].ConcurrencyTag };
                        }else
                        {
                            data = new TargetMessage() { ServerName = target.Item1, DbOverrideSequence = target.Item2, ConcurrencyTag = target.Item2[0].ConcurrencyTag };
                        }

                        switch (cType)
                        {
                            case ConcurrencyType.Tag:
                            case ConcurrencyType.MaxPerTag:
                                if(data.ConcurrencyTag.Length == 0)
                                {
                                    var message = $"A Concurrency Tag is required in the override settings when Concurrency Type '{cType.ToString()}' is set. Unable to queue messages.";
                                    log.LogError(message);
                                    return null; 
                                }
                                break;
                            default:
                                break;
                        }


                        var msg = data.AsMessage();
                        msg.Subject = jobName;
                        if (target.Item2[0].ConcurrencyTag.Length > 0)
                        {
                            msg.SessionId = target.Item2[0].ConcurrencyTag;

                        }
                        else
                        {
                            msg.SessionId = target.Item1;
                        }
                        msg.MessageId = Guid.NewGuid().ToString();
                        msgs.Add(msg);
                    }
                }
                return msgs;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to contruct Service Bus Messages from override targets");
                return null;
            }

        }

        public async Task<List<ServiceBusReceivedMessage>> GetDatabaseTargetFromQueue(int maxMessages, ConcurrencyType cType)
        {
            switch (cType)
            {
                case ConcurrencyType.Server:
                case ConcurrencyType.Tag:
                    return await GetSessionBasedTargetsFromQueue(1, false);

                case ConcurrencyType.MaxPerServer:
                case ConcurrencyType.MaxPerTag:
                    return await GetSessionBasedTargetsFromQueue(maxMessages, false);

                case ConcurrencyType.Count:
                default:
                    return await GetCountBasedTargetsFromQueue(maxMessages);
            }

        }
        private async Task<List<ServiceBusReceivedMessage>> GetCountBasedTargetsFromQueue(int maxMessages)
        {
            var lstMsg = new List<ServiceBusReceivedMessage>();
            try
            { 
                
                var messages = await MessageReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));
                log.LogDebug($"Received {messages.Count} messages from Service Bus Queue for batch job '{jobName}'");
                if (messages.Count == 0)
                {
                    return lstMsg;
                }
                foreach (var message in messages)
                {
                    if (message.Subject.ToLower().Trim() != jobName.ToLower().Trim())
                    {
                        await MessageReceiver.DeadLetterMessageAsync(message);
                        log.LogWarning($"Send message '{message.MessageId} to deadletter. Subject of '{message.Subject}' did not match batch job name of '{jobName}'");
                    }
                    else
                    {
                        lstMsg.Add(message);
                    }
                }
                if (lstMsg.Count == 0)
                {
                    return await GetCountBasedTargetsFromQueue(maxMessages);
                }
            }
            catch(Exception exe)
            {
                log.LogError($"Problem getting messages from Service Bus Queue: {exe.Message}");
            }
            return lstMsg;
        }
        private async Task<List<ServiceBusReceivedMessage>> GetSessionBasedTargetsFromQueue(int maxMessages, bool resetSession, int retry = 0)
        {
            var lstMsg = new List<ServiceBusReceivedMessage>();
            bool foundMessages = false;
            try
            {

                //Init the receiver and try to acquire a session
                if (_sessionReceiver == null || resetSession)
                {
                    log.LogInformation("Attempting to get new queue session for next Server...");
                    var token = GetCancellationToken();
                    try
                    {
                        _sessionReceiver = await Client.AcceptNextSessionAsync(topicName, topicSessionSubscriptionName, new ServiceBusSessionReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock }, token);
                        log.LogInformation($"Obtained new subscription for batch job '{jobName}' and subscription Id '{_sessionReceiver.SessionId}' ");
                    }
                    catch (TaskCanceledException)
                    {
                        log.LogInformation("No session available by wait time expiration");
                        return lstMsg;
                    }
                }

                var messages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));
                log.LogDebug($"Received {messages.Count} messages from Service Bus Queue for batch job '{jobName}' and subscription Id '{_sessionReceiver.SessionId}'");
                //If no messages in the current session, try to acquire a new session
                if (messages.Count == 0 && resetSession == false)
                {
                    return await GetSessionBasedTargetsFromQueue(maxMessages, true);
                }
                else
                {
                    foundMessages = true;
                }

                //Got messages, not try to see if they are a match for the current job, if not, deadletter. Keep looking until some for the current job are found
                if (foundMessages)
                {
                    while (foundMessages || lstMsg.Count() == 0)
                    {
                        foreach (var message in messages)
                        {
                            if (message.Subject.ToLower().Trim() != jobName.ToLower().Trim())
                            {
                                log.LogWarning($"Message {message.MessageId} has incorrect Batch Job name '{jobName}'");
                                try
                                {
                                    await _sessionReceiver.DeadLetterMessageAsync(message);
                                    log.LogWarning($"Send message '{message.MessageId}' to deadletter. Subject of '{message.Subject}' did not match batch job name of '{jobName}'");
                                }
                                catch (Exception exe)
                                {
                                    log.LogWarning($"Failed to deadletter message '{message.MessageId}': {exe.Message}");
                                }
                            }
                            else
                            {
                                lstMsg.Add(message);
                            }
                        }

                        //if they all got deadlettered, try to get some more, until there are none left!
                        if (lstMsg.Count == 0)
                        {
                            messages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));
                            if (messages.Count == 0)
                            {
                                foundMessages = false;
                            }
                        }
                        else
                        {
                            return lstMsg;
                        }
                    }
                }
                else
                {
                    return lstMsg;
                }
            }
            catch (ServiceBusException sbe)
            {
                switch (sbe.Reason)
                {
                    case ServiceBusFailureReason.MessagingEntityNotFound: //This execption is thrown when the subscription has been deleted, return empty list to indicate no more messages
                        log.LogInformation($"Service Bus response: MessagingEntityNotFound: {sbe.Message} ");
                        return lstMsg;
                    case ServiceBusFailureReason.ServiceTimeout:  //This execption is thrown when no session is available, return empty list to indicate no more messages
                        log.LogInformation($"Service Bus response: ServiceTimeout: {sbe.Message} ");
                        return lstMsg;
                    case ServiceBusFailureReason.SessionLockLost: //Try to get a new session
                        return await GetSessionBasedTargetsFromQueue(maxMessages, true);

                    case ServiceBusFailureReason.MessageLockLost:
                        log.LogError($"Lock lost for message! There may be a issue with the messages in the topic: '{topicSessionSubscriptionName}");
                        if (retry == 5)
                        {
                            throw;
                        }
                        return await GetSessionBasedTargetsFromQueue(maxMessages, true, retry++);

                    default:
                        throw;
                }

            }
            catch (Exception exe)
            {
                log.LogError($"Error getting messages: {exe.Message}");
                return lstMsg;
            }
            return lstMsg;
        }


        private CancellationToken GetCancellationToken(int waitMs = 5000)
        {
            tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(waitMs);
            var token = tokenSource.Token;
            return token;

        }

        public async Task CompleteMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            string concurrency = t.ConcurrencyTag.Length == 0 ? "" : $" [ConcurrencyTag: {t.ConcurrencyTag}]";
            try
            {

                log.LogInformation($"Completing {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency} message ID '{message.MessageId}'");

                if (_sessionReceiver != null)
                {
                    await _sessionReceiver.CompleteMessageAsync(message);
                }
                else
                {
                    await MessageReceiver.CompleteMessageAsync(message);
                }
            }
            catch (ServiceBusException sbE)
            {
                log.LogError($"Unable to complete message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency}. This may result in duplicate processing: {sbE.Message}");
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to complete message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency}. This may result in duplicate processing: {exe.Message}");
            }
            return;
        }
        public async Task AbandonMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            string concurrency = t.ConcurrencyTag.Length == 0 ? "" : $" [ConcurrencyTag: {t.ConcurrencyTag}]";
            try
            {
                log.LogInformation($"Abandoning {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency} message ID '{message.MessageId}'");
                if (_sessionReceiver != null)
                {
                    await _sessionReceiver.AbandonMessageAsync(message);
                }
                else
                {
                    await MessageReceiver.AbandonMessageAsync(message);
                }
            }
            catch (Exception exe)
            {
                log.LogError($"Failed to Abandon message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency}: {exe.Message}");
            }
            return;
        }
        public async Task DeadletterMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            string concurrency = t.ConcurrencyTag.Length == 0 ? "" : $" [ConcurrencyTag: {t.ConcurrencyTag}]";
            try
            {
                log.LogInformation($"Deadlettering {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency} message ID '{message.MessageId}'");
                if (_sessionReceiver != null)
                {
                    await _sessionReceiver.DeadLetterMessageAsync(message);
                }
                else
                {
                    await MessageReceiver.DeadLetterMessageAsync(message);
                }
            }
            catch (Exception exe)
            {
                log.LogError($"Failed to Deadletter message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency}: {exe.Message}");
            }
            return;
        }
        public async Task RenewMessageLock(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            string concurrency = t.ConcurrencyTag.Length == 0 ? "" : $" [ConcurrencyTag: {t.ConcurrencyTag}]";
            try
            {
                log.LogDebug($"Renewing message lock on {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency} message ID '{message.MessageId}'");
                if (_sessionReceiver != null)
                {
                    await _sessionReceiver.RenewSessionLockAsync();
                }
                else
                {
                    await MessageReceiver.RenewMessageLockAsync(message);
                }
            }
            catch (Exception exe)
            {
                log.LogError($"Failed to renew message lock for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}{concurrency}: {exe.Message}");
            }
            return;
        }

        internal async Task<bool> DeleteSubscription()
        {
            string topicSub;
            switch (concurrencyType)
            {
                case ConcurrencyType.Count:
                    topicSub = topicSubscriptionName;
                    break;
                default:
                    topicSub = topicSessionSubscriptionName;
                    break;
            }

            try
            {
                var results = await AdminClient.DeleteSubscriptionAsync(topicName, topicSub);
                if (results.Status < 300)
                {
                    log.LogInformation($"Deleted Service Bus Topic subscription: {topicSub}");
                    return true;
                }
                else
                {
                    log.LogError($"Problem deleting subscriptipn '{topicSub}': {results.Content}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Problem deleting subscriptipn '{topicSub}': {ex.Message}");
                return false;
            }
        }

        private async Task RemoveDefaultFilters()
        {
            log.LogDebug($"Starting to remove default filters.");
            string topicSub;
            switch (concurrencyType)
            {
                case ConcurrencyType.Count:
                    topicSub = topicSubscriptionName;
                    break;
                default:
                    topicSub = topicSessionSubscriptionName;
                    break;
            }
            try
            {
                try
                {
                    var defRule = await AdminClient.GetRuleAsync(topicName, topicSub, CreateRuleOptions.DefaultRuleName);
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("not found"))
                    {
                        log.LogDebug($"No default filter found.");
                        return;
                    }
                    else
                    {
                        var pollyRetryPolicyForDefaultRemove = Policy.Handle<Exception>(ex => !ex.Message.Contains("could not be found")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                        await pollyRetryPolicyForDefaultRemove.ExecuteAsync(async () =>
                        {
                            await AdminClient.DeleteRuleAsync(topicName, topicSub, CreateRuleOptions.DefaultRuleName);
                        });
                    }
                }
                log.LogDebug($"Default filter for subscription '{topicSub}' has been removed.");



            }
            catch (Exception ex)
            {
                log.LogInformation($"Unable to remove default Topic filter: {ex.Message}");
            }

            return;
        }

        public async Task<bool> SubscriptionIsPreExisting()
        {
            try
            {
                switch (concurrencyType)
                {
                    case ConcurrencyType.MaxPerServer:
                    case ConcurrencyType.Server:
                    case ConcurrencyType.MaxPerTag:
                    case ConcurrencyType.Tag:
                        return await AdminClient.SubscriptionExistsAsync(topicName, topicSessionSubscriptionName);

                    case ConcurrencyType.Count:
                    default:
                        return await AdminClient.SubscriptionExistsAsync(topicName, topicSubscriptionName);
                }
            }
            catch (Exception ex)
            {
                if (!ex.ToString().Contains("Status: 409"))
                {
                    throw;
                }
                return false;
            }
        }
        private async Task CreateSubscriptions()
        {
            try
            {
                switch (concurrencyType)
                {
                    case ConcurrencyType.Count:
                        if (!await AdminClient.SubscriptionExistsAsync(topicName, topicSubscriptionName))
                        {
                            log.LogInformation($"Creating topic subscripton for `{jobName}'");
                            var stdOptions = new CreateSubscriptionOptions(topicName, topicSubscriptionName);
                            var result = await AdminClient.CreateSubscriptionAsync(stdOptions);
                        }
                        break;

                    case ConcurrencyType.MaxPerServer:
                    case ConcurrencyType.Server:
                    case ConcurrencyType.Tag:
                    case ConcurrencyType.MaxPerTag:
                        if (!await AdminClient.SubscriptionExistsAsync(topicName, topicSessionSubscriptionName))
                        {
                            log.LogInformation($"Creating session enabled topic subscripton for `{jobName}'");
                            var sessionOptions = new CreateSubscriptionOptions(topicName, topicSessionSubscriptionName);
                            sessionOptions.RequiresSession = true;
                            var result = await AdminClient.CreateSubscriptionAsync(sessionOptions);
                        }
                        break;
                    default:
                        throw new ArgumentException("Unknown concurrency type");
                }
            }
            catch (Exception ex)
            {
                if (!ex.ToString().Contains("Status: 409"))
                {
                    throw;
                }
            }
        }
        private async Task CreateBatchJobFilter(bool withSession)
        {
            string subName;
            if (withSession)
            {
                subName = topicSessionSubscriptionName;
            }
            else
            {
                subName = topicSubscriptionName;
            }
            try
            {
                log.LogDebug($"Creating Topic filter for Batch job name: {jobName}");
                string filter = jobName;
                var pollyRetryPolicyForCreate = Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                await pollyRetryPolicyForCreate.ExecuteAsync(async () =>
                {
                    await AdminClient.CreateRuleAsync(topicName, subName, new CreateRuleOptions()
                    {
                        Filter = new CorrelationRuleFilter()
                        {
                            Subject = jobName,

                        },
                        Name = jobName
                    });
                });

                log.LogDebug($"Filter named {jobName} has been added for subscription `{subName}`.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    log.LogInformation($"The subscription filter '{jobName}' already exists");
                    _adminClient = null;
                }
                else
                {
                    log.LogError(ex, $"Failed to create custom subscription filter for batch job '{jobName}'");
                }
            }
            return;
        }
        private async Task CleanUpCustomFilters()
        {
            string topicSub;
            switch (concurrencyType)
            {
                case ConcurrencyType.Count:
                    topicSub = topicSubscriptionName;
                    break;
                default:
                    topicSub = topicSessionSubscriptionName;
                    break;
            }
            try
            {

                IAsyncEnumerator<RuleProperties> rules = AdminClient.GetRulesAsync(topicName, topicSub).GetAsyncEnumerator();
                while (await rules.MoveNextAsync())
                {
                    var pollyRetryPolicyForClean = Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                    await pollyRetryPolicyForClean.ExecuteAsync(async () =>
                    {
                        if (rules.Current.Name != jobName)
                        {
                            await AdminClient.DeleteRuleAsync(topicName, topicSub, rules.Current.Name);
                            log.LogDebug($"Rule {rules.Current.Name} has been removed.");
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Problem deleting customer filters");
            }
            log.LogInformation("All existing filters have been removed.");
            return;
        }

        public async Task<long> MonitorServiceBustopic(ConcurrencyType concurrencyType)
        {
            SubscriptionRuntimeProperties props;
            switch (concurrencyType)
            {
                case ConcurrencyType.Count:
                
                    props = await AdminClient.GetSubscriptionRuntimePropertiesAsync(topicName, topicSubscriptionName, new CancellationToken());
                    break;
                case ConcurrencyType.MaxPerTag:
                case ConcurrencyType.MaxPerServer:
                case ConcurrencyType.Server:
                case ConcurrencyType.Tag:
                    props = await AdminClient.GetSubscriptionRuntimePropertiesAsync(topicName, topicSessionSubscriptionName, new CancellationToken());
                    break;
                default:
                    throw new ArgumentException($"Unknow concurrency type of {concurrencyType}");
            }
            return props.ActiveMessageCount;
        }

        public void Dispose()
        {
            var tasks = new List<Task>();
            if (_messageReceiver != null)
            {
                tasks.Add(_messageReceiver.DisposeAsync().AsTask());
            }
            if (_client != null)
            {
                tasks.Add(_client.DisposeAsync().AsTask());
            }

            Task.WaitAll(tasks.ToArray());

        }

    }

}

