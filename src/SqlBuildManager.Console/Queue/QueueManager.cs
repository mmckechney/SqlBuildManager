using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Polly;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Shared;
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
        private string jobName;
        private ConcurrencyType concurrencyType;

        private ServiceBusClient _client = null;
        private ServiceBusAdministrationClient _adminClient = null;
        private ServiceBusSessionReceiver _sessionReceiver = null;
        private ServiceBusReceiver _messageReceiver = null;
        private CancellationTokenSource tokenSource = null;

        public QueueManager(string topicConnectionString, string jobName, ConcurrencyType concurrencyType)
        {
            this.topicConnectionString = topicConnectionString;
            this.jobName = jobName;

            this.topicSubscriptionName = jobName;
            this.topicSessionSubscriptionName = $"{jobName}session";
            this.concurrencyType = concurrencyType;
            CreateSubscriptions().Wait();
        }

        private string EnsureQualifiedNamespace(string input)
        {
            //Do we just have the Service Bus name?
            if(input.ToLower().IndexOf("servicebus.windows.net") == -1)
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
                    if (ConnectionValidator.IsServiceBusConnectionString(topicConnectionString))
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
                    if (ConnectionValidator.IsServiceBusConnectionString(topicConnectionString))
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
                    _messageReceiver = this.Client.CreateReceiver(this.topicName, this.topicSubscriptionName, new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });
                }
                return _messageReceiver;
            }
        }


        public async Task<int> SendTargetsToQueue(MultiDbData multiDb, ConcurrencyType cType)
        {
            try
            {
                log.LogInformation($"Setting up Topic Subscription with Job filter name '{this.jobName}'");
                await RemoveDefaultFilters();
                await CleanUpCustomFilters();
                await CreateBatchJobFilter(cType == ConcurrencyType.Count ? false : true );

                var sender = this.Client.CreateSender(topicName);

                //Use bucketing to 1 bucket to get flattened list of targest
                var concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, 1, ConcurrencyType.Count);
                var messages = CreateMessages(concurrencyBuckets, jobName);
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
                if(sentCount != count)
                {
                    log.LogError($"Only {sentCount} out of {count} database targets were sent to the Service Bus. Before running your workload, please run a 'dequeue' command and try again");
                    return -1;
                }

                //Confirm message count in Queue 
                int retry = 0;
                var activeMessages = await MonitorServiceBustopic(cType);
                while(activeMessages != count && retry < 4 )
                {
                    Thread.Sleep(1000);
                    activeMessages = await MonitorServiceBustopic(cType);
                }

                if(activeMessages != count)
                {

                    log.LogError($"After attempting to queue messages, there are only {activeMessages} out of {count} messages in the Service Bus Subscription. Before running your workload, please run a 'dequeue' command and try again");
                    return -1;
                }
                else
                {
                    log.LogInformation($"Validated {activeMessages} of {count} active messages in Service Bus Subscription {this.topicName}:{this.topicSessionSubscriptionName}");
                }

                return count;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Failed to send database override targets to Service Bus Queue");
                return -1;
            }

        }
        private List<ServiceBusMessage> CreateMessages(List<IEnumerable<(string, List<DatabaseOverride>)>> buckets, string jobName)
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
                        msg.Subject = jobName;
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

            if(messages.Count == 0)
            {
                return lstMsg;
            }
            foreach (var message in messages)
            {
                if (message.Subject.ToLower().Trim() != jobName.ToLower().Trim())
                {
                    await this.MessageReceiver.DeadLetterMessageAsync(message);
                    log.LogWarning($"Send message '{message.MessageId} to deadletter. Subject of '{message.Subject}' did not match batch job name of '{jobName}'");
                }
                else
                {
                    lstMsg.Add(message);
                }
            }
            if(lstMsg.Count == 0)
            {
                return await GetCountBasedTargetsFromQueue(maxMessages);
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
                        log.LogInformation($"Obtained new subscription for batch job '{jobName}' and subscription Id '{_sessionReceiver.SessionId}' ");
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
                    case ServiceBusFailureReason.MessagingEntityNotFound: //This execption is thrown when the subscription has been deleted, return empty list to indicate no more messages
                        log.LogInformation($"Service Bus response: MessagingEntityNotFound: {sbe.Message} ");
                        return lstMsg;
                    case ServiceBusFailureReason.ServiceTimeout:  //This execption is thrown when no session is available, return empty list to indicate no more messages
                        log.LogInformation($"Service Bus response: ServiceTimeout: {sbe.Message} ");
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
            catch(Exception exe)
            {
                log.LogError($"Error getting messages: {exe.ToString()}");
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
            try
            {
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
            catch(ServiceBusException sbE)
            {
                log.LogError($"Unable to compelete message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}. This may result in duplicate processing: {sbE.Message}");
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to compelete message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}. This may result in duplicate processing: {exe.Message}");
            }
        }
        public async Task AbandonMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            try
            {
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
            catch (Exception exe)
            {
                log.LogError($"Failed to Abandon message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}: {exe.Message}");
            }
        }
        public async Task DeadletterMessage(ServiceBusReceivedMessage message)
        {
            var t = message.As<TargetMessage>();
            try
            {
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
            catch(Exception exe)
            {
                log.LogError($"Failed to Deadletter message for {t.ServerName}.{t.DbOverrideSequence[0].OverrideDbTarget}: {exe.Message}");
            }
        }

        internal async Task<bool> DeleteSubscription()
        {
            string topicSub;
            switch (this.concurrencyType)
            {
                case ConcurrencyType.Count:
                    topicSub = this.topicSubscriptionName;
                    break;
                default:
                    topicSub = this.topicSessionSubscriptionName;
                    break;
            }

            try
            {
                var results = await this.AdminClient.DeleteSubscriptionAsync(this.topicName, topicSub);
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
            catch(Exception ex)
            {
                log.LogError($"Problem deleting subscriptipn '{topicSub}': {ex.Message}");
                return false;
            }
        }

        private async Task RemoveDefaultFilters()
        {
            log.LogDebug($"Starting to remove default filters.");
            string topicSub;
            switch(this.concurrencyType)
            {
                case ConcurrencyType.Count:
                    topicSub = this.topicSubscriptionName;
                    break;
                default:
                    topicSub = this.topicSessionSubscriptionName;
                    break;
            }
            try
            {
                try
                {
                    var defRule = await this.AdminClient.GetRuleAsync(this.topicName, topicSub, CreateRuleOptions.DefaultRuleName);
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
                            await AdminClient.DeleteRuleAsync(this.topicName, topicSub, CreateRuleOptions.DefaultRuleName);
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

        private async Task CreateSubscriptions()
        {
            try
            {
                switch (this.concurrencyType)
                {
                    case ConcurrencyType.Count:
                        if (!await this.AdminClient.SubscriptionExistsAsync(this.topicName, this.topicSubscriptionName))
                        {
                            log.LogInformation($"Creating topic subscripton for `{this.jobName}'");
                            var stdOptions = new CreateSubscriptionOptions(this.topicName, this.topicSubscriptionName);
                            var result = await this.AdminClient.CreateSubscriptionAsync(stdOptions);
                        }
                        break;

                    case ConcurrencyType.MaxPerServer:
                    case ConcurrencyType.Server:
                        if (!await this.AdminClient.SubscriptionExistsAsync(this.topicName, this.topicSessionSubscriptionName))
                        {
                            log.LogInformation($"Creating session enabled topic subscripton for `{this.jobName}'");
                            var sessionOptions = new CreateSubscriptionOptions(this.topicName, this.topicSessionSubscriptionName);
                            sessionOptions.RequiresSession = true;
                            var result = await this.AdminClient.CreateSubscriptionAsync(sessionOptions);
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
                log.LogDebug($"Creating Topic filter for Batch job name: {this.jobName}");
                string filter = jobName;
                var pollyRetryPolicyForCreate= Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                await pollyRetryPolicyForCreate.ExecuteAsync(async ()  =>
                {
                    await this.AdminClient.CreateRuleAsync(topicName, subName, new CreateRuleOptions()
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
            switch (this.concurrencyType)
            {
                case ConcurrencyType.Count:
                    topicSub = this.topicSubscriptionName;
                    break;
                default:
                    topicSub = this.topicSessionSubscriptionName;
                    break;
            }
            try
            {

                IAsyncEnumerator<RuleProperties> rules = this.AdminClient.GetRulesAsync(this.topicName, topicSub).GetAsyncEnumerator();
                while (await rules.MoveNextAsync())
                {
                    var pollyRetryPolicyForClean = Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                    await pollyRetryPolicyForClean.ExecuteAsync(async () =>
                    {
                        if (rules.Current.Name != this.jobName)
                        {
                            await this.AdminClient.DeleteRuleAsync(this.topicName, topicSub, rules.Current.Name);
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
            if(concurrencyType == ConcurrencyType.Count)
            {
                props = await this.AdminClient.GetSubscriptionRuntimePropertiesAsync(this.topicName, this.topicSubscriptionName,new CancellationToken());
            }else
            {
                props = await this.AdminClient.GetSubscriptionRuntimePropertiesAsync(this.topicName, this.topicSessionSubscriptionName, new CancellationToken());
            }
            return props.ActiveMessageCount;
      
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

