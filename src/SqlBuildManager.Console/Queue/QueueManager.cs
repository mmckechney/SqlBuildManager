using Azure.Messaging.ServiceBus;
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

namespace SqlBuildManager.Console.Queue
{
    public class QueueManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string topicSubscriptionName = "sbmsubscription";
        private readonly string topicName = "sqlbuildmanager";

        private string topicConnectionString;
        private string batchJobName;

        private ServiceBusClient _client = null;
        private ServiceBusAdministrationClient _adminClient = null;
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


        public async Task<int> SendTargetsToQueue(MultiDbData multiDb)
        {
            try
            {
                log.LogInformation($"Setting up Topic Subscription with Batch Job filter name '{this.batchJobName}'");
                await RemoveDefaultFilters();
                await CleanUpCustomFilters();
                await CreateBatchJobFilter();

                  var sender = this.Client.CreateSender(topicName);

                //Use bucketing to 1 bucket to get flattened list of targest
                var concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, 1, ConcurrencyType.Count);
                var messages = CreateMessages(concurrencyBuckets, batchJobName);
                int count = messages.Count();
                //send in batches of 20
                var msgBatch = messages.Batch(20);
                foreach (var b in msgBatch)
                {
                    var sbb = await sender.CreateMessageBatchAsync();
                    b.ForEach(m => sbb.TryAddMessage(m));

                    await sender.SendMessagesAsync(sbb);
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
                        var data = new { ServerName = target.Item1, DbOverrideSequence = target.Item2 };
                        var msg = data.AsMessage();
                        msg.Subject = batchJobName;
                        msg.SessionId = target.Item1;
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

        public ServiceBusReceiver GetQueueReceiver()
        {
            ServiceBusClient client = new ServiceBusClient(this.topicConnectionString);
           var receiver =  client.CreateReceiver(this.topicName, this.topicSubscriptionName);
           
            
            return receiver;
        }


        private async Task RemoveDefaultFilters()
        {
            log.LogDebug($"Starting to remove default filters.");

            try
            {
                try
                {
                    var defRule = await this.AdminClient.GetRuleAsync(this.topicName, this.topicSubscriptionName, CreateRuleOptions.DefaultRuleName);
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
                            await AdminClient.DeleteRuleAsync(this.topicName, this.topicSubscriptionName, CreateRuleOptions.DefaultRuleName);
                        });
                    }
                }
                log.LogDebug($"Default filter for subscription '{this.topicSubscriptionName}' has been removed.");

            }
            catch (Exception ex)
            {
                log.LogInformation($"Unable to remove default Topic filter: {ex.Message}");
            }

            return;
        }

        private async Task CreateBatchJobFilter()
        {
            try
            {
                log.LogDebug($"Creating Topic filter for Batch job name: {this.batchJobName}");
                string filter = batchJobName;
                var pollyRetryPolicyForCreate= Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                await pollyRetryPolicyForCreate.ExecuteAsync(async ()  =>
                {
                    await this.AdminClient.CreateRuleAsync(topicName, topicSubscriptionName, new CreateRuleOptions()
                    {
                        Filter = new CorrelationRuleFilter()
                        {
                            Subject = batchJobName,
                            
                        },
                        Name = batchJobName
                    });
                });
               
                       log.LogDebug($"Filter named {batchJobName} has been added for subscription `{topicSubscriptionName}`.");
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

            try
            {
  
                IAsyncEnumerator<RuleProperties> rules = this.AdminClient.GetRulesAsync(this.topicName, this.topicSubscriptionName).GetAsyncEnumerator();
                while (await rules.MoveNextAsync())
                {
                    var pollyRetryPolicyForClean = Policy.Handle<Exception>(ex => !ex.Message.Contains("already exists")).WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                    await pollyRetryPolicyForClean.ExecuteAsync(async () =>
                    {
                        if (rules.Current.Name != this.batchJobName)
                        {
                            await this.AdminClient.DeleteRuleAsync(this.topicName, this.topicSubscriptionName, rules.Current.Name);
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
    }

}

