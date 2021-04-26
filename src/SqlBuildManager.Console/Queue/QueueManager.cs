﻿using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SqlBuildManager.Console.Threaded;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using System.Linq;
using System.Text;
using System.Threading;

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
        private async Task<List<ServiceBusReceivedMessage>> GetSessionBasedTargetsFromQueue(int maxMessages, bool resetSession)
        {
            var lstMsg = new List<ServiceBusReceivedMessage>();
            try
            {
                if (_sessionReceiver == null || resetSession)
                {
                    log.LogInformation("Attempting to get new queue session for next Server...");
                    var token = sessionTokenSource.Token;
                    StartCancellationTimer();
                    try
                    {
                        _sessionReceiver = await this.Client.AcceptNextSessionAsync(this.topicName, this.topicSessionSubscriptionName, new ServiceBusSessionReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock }, token);
                    }catch(TaskCanceledException)
                    {
                        return lstMsg;
                    }
                }

                var messages = await _sessionReceiver.ReceiveMessagesAsync(maxMessages, new TimeSpan(0, 0, 10));
                if (messages.Count == 0 && resetSession == false)
                {
                    return await GetSessionBasedTargetsFromQueue(maxMessages, true);
                }
                foreach (var message in messages)
                {
                    if (message.Subject.ToLower().Trim() != batchJobName.ToLower().Trim())
                    {
                        log.LogWarning($"Message {message.MessageId} has incorrect Batch Job name");
                        await this.MessageReceiver.DeadLetterMessageAsync(message);
                        log.LogWarning($"Send message '{message.MessageId} to deadletter. Subject of '{message.Subject}' did not match batch job name of '{batchJobName}'");
                    }
                    else
                    {
                        lstMsg.Add(message);
                    }
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
                        break;
                    default:
                        throw;
                }
                
            }
            return lstMsg;
        }

        CancellationTokenSource sessionTokenSource = new CancellationTokenSource();
        private void StartCancellationTimer()
        {
            sessionTokenSource.CancelAfter(5000);
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


        internal async Task<bool> RetrieveTargetsFromQueue()
        {
            var receiver = this.Client.CreateReceiver(this.topicName, this.topicSubscriptionName, new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (true)
            {
                IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.ReceiveMessagesAsync(maxMessages: 100);
                if (messages.Any())
                {
                    foreach (ServiceBusReceivedMessage message in messages)
                    {
                        if (message.Subject.ToLower().Trim() != batchJobName.ToLower().Trim())
                        {
                            await receiver.DeadLetterMessageAsync(message);
                            log.LogWarning($"Send message '{message.MessageId} to deadletter. Subject of '{message.Subject}' did not match batch job name of '{batchJobName}'");
                        }
                        else
                        {
                            string msg = $"{Environment.NewLine}JobName: {message.Subject}{Environment.NewLine}{Encoding.UTF8.GetString(message.Body.ToArray())}";
                            log.LogInformation(msg);
                            await receiver.CompleteMessageAsync(message);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            log.LogInformation("Compelted message receive");
            return true;

        }
        public ServiceBusReceiver GetQueueReceiver()
        {
            ServiceBusClient client = new ServiceBusClient(this.topicConnectionString);
           var receiver =  client.CreateReceiver(this.topicName, this.topicSubscriptionName);
           
            
            return receiver;
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
