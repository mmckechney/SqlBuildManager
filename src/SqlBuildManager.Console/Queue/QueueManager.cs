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
using System.Text;
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
        private string batchJobName;

        private ServiceBusClient _client = null;
        private ServiceBusAdministrationClient _adminClient = null;
        private ServiceBusSessionReceiver _sessionReceiver = null;
        private ServiceBusReceiver _messageReceiver = null;
        private CancellationTokenSource tokenSource = null;

       
        public QueueManager(string topicConnectionString, string batchJobName)
        {
            this.topicConnectionString = topicConnectionString;
            this.batchJobName = batchJobName;
        }

        public ServiceBusClient Client
        {
            get
            {
                if(_client == null)
                {
                    _client = new ServiceBusClient(topicConnectionString);
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
                    _adminClient = new ServiceBusAdministrationClient(topicConnectionString);
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
                    _messageReceiver = this.Client.CreateReceiver(this.topicName, this.topicSubscriptionName, new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });
                }
                return _messageReceiver;
            }
        }


        public async Task<int> SendTargetsToQueue(MultiDbData multiDb, ConcurrencyType cType)
        {
            try
            {
                log.LogInformation($"Setting up Topic Subscription with Batch Job filter name '{this.batchJobName}'");
                await RemoveDefaultFilters();
                await CleanUpCustomFilters();
                await CreateBatchJobFilter(cType == ConcurrencyType.Count ? false : true );

                  var sender = this.Client.CreateSender(topicName);

                //Use bucketing to 1 bucket to get flattened list of targest
                var concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, 1, ConcurrencyType.Count);
                var messages = CreateMessages(concurrencyBuckets, batchJobName);
                int count = messages.Count();
                
                //because of partitiioning, can't batch across session Id, so group by SessionId first, then batch
                var bySessionId = messages.GroupBy(s => s.SessionId);
                foreach (var sessionSet in bySessionId)
                    {
                    var msgBatch = sessionSet.Batch(20); //send in batches of 20
                        foreach (var b in msgBatch)
                        {
                            var sbb = await sender.CreateMessageBatchAsync();
                            b.ForEach(m => sbb.TryAddMessage(m));

                            await sender.SendMessagesAsync(sbb);
                        }
                }
                return count;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Failed to send database override targets to Service Bus Queue");
                return -1;
            }

        }
        private List<ServiceBusMessage> CreateMessages(List<IEnumerable<(string, List<DatabaseOverride>)>> buckets, string batchJobName)
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
                        var data = new TargetMessage(){ ServerName = target.Item1, DbOverrideSequence = target.Item2 };
                        var msg = data.AsMessage();
                        msg.Subject = batchJobName;
                        msg.SessionId = target.Item1;
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
            switch(cType)
            {
                case ConcurrencyType.Server:
                    return await GetSessionBasedTargetsFromQueue(1, false);
                
                case ConcurrencyType.MaxPerServer:
                    return await GetSessionBasedTargetsFromQueue(maxMessages, false);

                case ConcurrencyType.Count:
                default:
                    return await GetCountBasedTargetsFromQueue(maxMessages);
            }
            
        }
        private async Task<List<ServiceBusReceivedMessage>> GetCountBasedTargetsFromQueue(int maxMessages)
        {
            var lstMsg = new List<ServiceBusReceivedMessage>();
            var messages = await this.MessageReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));

            foreach (var message in messages)
            {
                if (message.Subject.ToLower().Trim() != batchJobName.ToLower().Trim())
                {
                    await this.MessageReceiver.DeadLetterMessageAsync(message);
                    log.LogWarning($"Send message '{message.MessageId} to deadletter. Subject of '{message.Subject}' did not match batch job name of '{batchJobName}'");
                }
                else
                {
                    lstMsg.Add(message);
                }
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
                        _sessionReceiver = await this.Client.AcceptNextSessionAsync(this.topicName, this.topicSessionSubscriptionName, new ServiceBusSessionReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock }, token);
                        log.LogInformation($"Obtained new subscription for batch job '{batchJobName}' and subscription Id '{_sessionReceiver.SessionId}' ");
                    }
                    catch(TaskCanceledException)
                    {
                        log.LogInformation("No session available by wait time expiration");
                        return lstMsg ;
                    }
                }

                var messages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));

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
                            if (message.Subject.ToLower().Trim() != batchJobName.ToLower().Trim())
                            {
                                log.LogWarning($"Message {message.MessageId} has incorrect Batch Job name '{batchJobName}'");
                                try
                                {
                                    await _sessionReceiver.DeadLetterMessageAsync(message);
                                    log.LogWarning($"Send message '{message.MessageId}' to deadletter. Subject of '{message.Subject}' did not match batch job name of '{batchJobName}'");
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
                        if(lstMsg.Count == 0)
                        {
                            messages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));
                            if(messages.Count == 0)
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
                switch(sbe.Reason)
                {
                    case ServiceBusFailureReason.ServiceTimeout:  //This execption is thrown when no session is available, return empty list to indicate no more messages
                        return lstMsg;
                    
                    case ServiceBusFailureReason.SessionLockLost: //Try to get a new session
                       return await GetSessionBasedTargetsFromQueue(maxMessages, true);

                    case ServiceBusFailureReason.MessageLockLost:
                        log.LogError($"Lock lost for message! There may be a issue with the messages in the topic: '{this.topicSessionSubscriptionName}");
                        if(retry == 5)
                        {
                            throw;
                        }
                        return await GetSessionBasedTargetsFromQueue(maxMessages, true, retry++);

                    default:
                        throw;
                }
                
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
            log.LogInformation($"Completing {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget} message ID '{message.MessageId}'");
            if (this._sessionReceiver != null)
            {
                await this._sessionReceiver.CompleteMessageAsync(message);
            }
            else
            {
                await this.MessageReceiver.CompleteMessageAsync(message);
            }
        }
        public async Task AbandonMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            log.LogInformation($"Abandoning {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget} message ID '{message.MessageId}'");
            if (this._sessionReceiver != null)
            {
                await this._sessionReceiver.AbandonMessageAsync(message);
            }
            else
            {
                await this.MessageReceiver.AbandonMessageAsync(message);
            }
        }
        public async Task DeadletterMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            log.LogInformation($"Deadlettering {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget} message ID '{message.MessageId}'");
            if (this._sessionReceiver != null)
            {
                await this._sessionReceiver.DeadLetterMessageAsync(message);
            }
            else
            { 
                await this.MessageReceiver.DeadLetterMessageAsync(message);
            }
        }


        internal async Task<bool> ClearQueueMessages()
        {
            try
            {
                //Clear regular messages
                var subNames = new List<string>() { this.topicSubscriptionName, $"{this.topicSubscriptionName}/$deadletterqueue", $"{this.topicSessionSubscriptionName}/$deadletterqueue" };
                foreach (var subname in subNames)
                {
                    log.LogInformation($"Clearing all messages from topic '{this.topicName}' subscription '{subname}'");
                    var receiver = this.Client.CreateReceiver(this.topicName, subname, new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
                    while (true)
                    {
                        try
                        {
                            var token = GetCancellationToken();
                            IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.ReceiveMessagesAsync(100, new TimeSpan(0, 0, 5), token);
                            log.LogInformation($"Removing {messages.Count} messages from {subname}");
                            if (!messages.Any())
                            {
                                break;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            log.LogInformation($"No more messages available in {subname} - by wait time expiration");
                            break;
                        }
                    }
                }


                //Clear session messages
                log.LogInformation($"Clearing all messages from topic '{this.topicName}' subscription '{this.topicSessionSubscriptionName}'");
                while (true)
                {
                    
                    
                    try
                    {
                        log.LogInformation($"Attempting to aquire topic session...");
                        var token = GetCancellationToken();
                        _sessionReceiver = await this.Client.AcceptNextSessionAsync(this.topicName, this.topicSessionSubscriptionName, new ServiceBusSessionReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete }, token);
                        log.LogInformation($"Obtained new subscription for subscription Id '{_sessionReceiver.SessionId}' ");
                    }
                    catch (TaskCanceledException)
                    {
                        log.LogInformation("No more messages available - by wait time expiration");
                        break;
                    }
                    catch (ServiceBusException sbe)
                    {
                        if (sbe.Reason == ServiceBusFailureReason.ServiceTimeout)
                        {
                            log.LogInformation("No more session messages available");
                            break;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    while (true)
                    {
                        try
                        {
                            var token = GetCancellationToken();
                            IReadOnlyList<ServiceBusReceivedMessage> messages = await _sessionReceiver.ReceiveMessagesAsync(100, new TimeSpan(0, 0, 5), token);
                            log.LogInformation($"Removing {messages.Count} messages from {this.topicSessionSubscriptionName} session {_sessionReceiver.SessionId}");
                            if (!messages.Any())
                            {
                                break;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            log.LogInformation($"No more messages available in {this.topicSessionSubscriptionName} session {_sessionReceiver.SessionId} - by wait time expiration");
                            break;
                        }
                    }
                }
                log.LogInformation("Completed Dequeueing all messages");
            }
            catch(Exception exe)
            {
                log.LogError($"Problem clearing all messages: {exe.Message}");
            }

            return true;

        }


        private async Task RemoveDefaultFilters()
        {
            log.LogDebug($"Starting to remove default filters.");
            string[] subs = new[] { this.topicSubscriptionName, this.topicSessionSubscriptionName };
            try
            {
                foreach (var sub in subs)
                {
                    try
                    {
                        var defRule = await this.AdminClient.GetRuleAsync(this.topicName, sub, CreateRuleOptions.DefaultRuleName);
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
                                await AdminClient.DeleteRuleAsync(this.topicName, sub, CreateRuleOptions.DefaultRuleName);
                            });
                        }
                    }
                    log.LogDebug($"Default filter for subscription '{this.topicSubscriptionName}' has been removed.");
                }
               

            }
            catch (Exception ex)
            {
                log.LogInformation($"Unable to remove default Topic filter: {ex.Message}");
            }

            return;
        }
        private async Task CreateBatchJobFilter(bool withSession)
        {
            string subName;
            if(withSession)
            {
                subName = this.topicSessionSubscriptionName;
            }
            else
            {
                subName = this.topicSubscriptionName;
            }
            try
            {
                log.LogDebug($"Creating Topic filter for Batch job name: {this.batchJobName}");
                string filter = batchJobName;
                var pollyRetryPolicyForCreate= Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                await pollyRetryPolicyForCreate.ExecuteAsync(async ()  =>
                {
                    await this.AdminClient.CreateRuleAsync(topicName, subName, new CreateRuleOptions()
                    {
                        Filter = new CorrelationRuleFilter()
                        {
                            Subject = batchJobName,
                            
                        },
                        Name = batchJobName
                    });
                });
               
                       log.LogDebug($"Filter named {batchJobName} has been added for subscription `{subName}`.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    log.LogInformation($"The subscription filter '{batchJobName}' already exists");
                }
                else
                {
                    log.LogError(ex, $"Failed to create custom subscription filter for batch job '{batchJobName}'");
                }
            }
            return;
        }
        private async Task CleanUpCustomFilters()
        {
            string[] subs = new[] { this.topicSubscriptionName, this.topicSessionSubscriptionName };
            try
            {
                foreach (var sub in subs)
                {
                    IAsyncEnumerator<RuleProperties> rules = this.AdminClient.GetRulesAsync(this.topicName, sub).GetAsyncEnumerator();
                    while (await rules.MoveNextAsync())
                    {
                        var pollyRetryPolicyForClean = Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                        await pollyRetryPolicyForClean.ExecuteAsync(async () =>
                        {
                            if (rules.Current.Name != this.batchJobName)
                            {
                                await this.AdminClient.DeleteRuleAsync(this.topicName, sub, rules.Current.Name);
                                log.LogDebug($"Rule {rules.Current.Name} has been removed.");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Problem deleting customer filters");
            }
            log.LogInformation("All existing filters have been removed.");
            return;
        }

        public void Dispose()
        {
            var tasks = new List<Task>();
            if(_messageReceiver != null)
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

