using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Messaging.ServiceBus;
using SqlSync.SqlBuild;
using SqlBuildManager.Console.Threaded;
using SqlSync.Connection;
using System.Text.Json;
using MoreLinq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Console.Queue
{
    public class QueueManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<bool> SendTargetsToQueue(string queueConnectionString, string batchJobName, MultiDbData multiDb)
        {
            try
            {
                ServiceBusClient client = new ServiceBusClient(queueConnectionString);
                var sender = client.CreateSender("sqlbuildmanager");

                //Use bucketing to 1 bucket to get flattened list of targest
                var concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, 1, ConcurrencyType.Count);
                var messages = CreateMessages(concurrencyBuckets, batchJobName);

                //send in batches of 20
                var msgBatch = messages.Batch(20);
                foreach (var b in msgBatch)
                {
                    var sbb = await sender.CreateMessageBatchAsync();
                    b.ForEach(m => sbb.TryAddMessage(m));

                    await sender.SendMessagesAsync(sbb);
                }
                return true;
            }
            catch(Exception exe)
            {
                log.LogError(exe, "Failed to send database override targets to Service Bus Queue");
                return false;
            }
            
        }

        public static List<ServiceBusMessage> CreateMessages(List<IEnumerable<(string, List<DatabaseOverride>)>> buckets, string batchJobName)
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
                        var msg = new ServiceBusMessage(JsonSerializer.Serialize(data, options));
                        msg.Subject = batchJobName;
                        msgs.Add(msg);
                    }
                }
                return msgs;
            }
            catch(Exception exe)
            {
                log.LogError(exe, "Unable to contruct Service Bus Messages from override targets");
                return null;
            }
            
        }


    }
}
