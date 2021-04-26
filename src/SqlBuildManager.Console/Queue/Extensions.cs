using System;
using System.Collections.Generic;
using System.Text;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
namespace SqlBuildManager.Console.Queue
{
    public static class Extensions
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

        public static T As<T>(this ServiceBusReceivedMessage message) where T : class
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(message.Body.ToArray()));
        }
        public static ServiceBusMessage AsMessage(this object obj)
        {
            return new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, options)));
        }

        public static bool Any(this IList<ServiceBusReceivedMessage> collection)
        {
            return collection != null && collection.Count > 0;
        }
    }
}
