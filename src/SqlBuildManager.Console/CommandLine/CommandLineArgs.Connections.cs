using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
        public Connections ConnectionArgs { get; set; } = new Connections();
        
        [JsonIgnore]
        public string KeyVaultName
        {
            set
            {
                ConnectionArgs.KeyVaultName = value;
                this.DirectPropertyChangeTracker.Add("Connections.KeyVaultName");
            }
        }
        [JsonIgnore]
        public virtual string EventHubConnection
        {
            set
            {
                ConnectionArgs.EventHubConnectionString = value;
                this.DirectPropertyChangeTracker.Add("Connections.EventHubConnectionString");
            }
        }

        [JsonIgnore]
        public virtual string ServiceBusTopicConnection
        {
            set
            {
                ConnectionArgs.ServiceBusTopicConnectionString = value;
                this.DirectPropertyChangeTracker.Add("Connections.ServiceBusTopicConnectionString");
            }
        }

        [JsonIgnore]
        public virtual string StorageAccountName
        {
            set
            {
                ConnectionArgs.StorageAccountName = value;
                this.DirectPropertyChangeTracker.Add("Connections.StorageAccountName");
            }
        }

        [JsonIgnore]
        public virtual string StorageAccountKey
        {
            set
            {
                ConnectionArgs.StorageAccountKey = value;
                this.DirectPropertyChangeTracker.Add("Connections.StorageAccountKey");
            }
        }
        [JsonIgnore]
        public virtual string BatchAccountName
        {
            set
            {
                ConnectionArgs.BatchAccountName = value;
                this.DirectPropertyChangeTracker.Add("Connections.BatchAccountName");
            }
        }

        [JsonIgnore]
        public virtual string BatchAccountKey
        {
            set
            {
                ConnectionArgs.BatchAccountKey = value;
                this.DirectPropertyChangeTracker.Add("Connections.BatchAccountKey");
            }
        }
        [JsonIgnore]
        public virtual string BatchAccountUrl
        {
            set
            {
                ConnectionArgs.BatchAccountUrl = value;
                this.DirectPropertyChangeTracker.Add("Connections.BatchAccountUrl");
            }
        }

        public class Connections : ArgsBase
        {
            public string KeyVaultName { get; set; } = string.Empty;

            public string ServiceBusTopicConnectionString { get; set; } = string.Empty;
            public string EventHubConnectionString { get; set; } = string.Empty;

            public string EventHubResourceGroup{ get; set; } = string.Empty;
            public string StorageAccountName { get; set; } = string.Empty;
            public string StorageAccountKey { get; set; } = string.Empty;
            public string BatchAccountName { get; set; } = string.Empty;
            public string BatchAccountKey { get; set; } = string.Empty;
            public string BatchAccountUrl { get; set; } = string.Empty;

        }
    }
}
