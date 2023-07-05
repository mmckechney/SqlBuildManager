using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {

        
        
        [JsonIgnore]
        public virtual string OutputContainerSasUrl
        {
            set
            {
                BatchArgs.OutputContainerSasUrl = value;
                this.DirectPropertyChangeTracker.Add("Batch.OutputContainerSasUrl");
            }
        }
        [JsonIgnore]
        public virtual bool DeleteBatchPool
        {
            set
            {
                BatchArgs.DeleteBatchPool = value;
                this.DirectPropertyChangeTracker.Add("Batch.DeleteBatchPool");
            }
        }
        [JsonIgnore]
        public virtual bool DeleteBatchJob
        {
            set
            {
                BatchArgs.DeleteBatchJob = value;
                this.DirectPropertyChangeTracker.Add("Batch.DeleteBatchJob");
            }
        }
        [JsonIgnore]
        public virtual int BatchNodeCount
        {
            set
            {
                BatchArgs.BatchNodeCount = value;
                this.DirectPropertyChangeTracker.Add("Batch.BatchNodeCount");
            }
        }
        [JsonIgnore]
        public virtual string BatchJobName
        {
            set
            {
                BatchArgs.BatchJobName = value.ToLower();
                jobName = value.ToLower();
                this.DirectPropertyChangeTracker.Add("Batch.BatchJobName");
            }
        }
        [JsonIgnore]
        public virtual string BatchVmSize
        {
            set
            {
                BatchArgs.BatchVmSize = value;
                this.DirectPropertyChangeTracker.Add("Batch.BatchVmSize");
            }
        }
        [JsonIgnore]
        public virtual string BatchPoolName
        {
            set
            {
                BatchArgs.BatchPoolName = value;
                this.DirectPropertyChangeTracker.Add("Batch.BatchPoolName");
            }
        }
        [JsonIgnore]
        public virtual bool PollBatchPoolStatus
        {
            set
            {
                BatchArgs.PollBatchPoolStatus = value;
                this.DirectPropertyChangeTracker.Add("Batch.PollBatchPoolStatus");
            }
        }
        [JsonIgnore]
        public virtual OsType BatchPoolOs
        {
            set
            {
                BatchArgs.BatchPoolOs = value;
                this.DirectPropertyChangeTracker.Add("Batch.BatchPoolOs");
            }
        }
        [JsonIgnore]
        public virtual string ApplicationPackage
        {
            set
            {
                BatchArgs.ApplicationPackage = value;
                this.DirectPropertyChangeTracker.Add("Batch.ApplicationPackage");
            }
        }
        [JsonIgnore]
        public virtual string BatchResourceGroup
        {
            set
            {
                BatchArgs.ResourceGroup = value;
                this.DirectPropertyChangeTracker.Add("Batch.ResourceGroup");
            }
        }

        [JsonIgnore]
        public virtual int BatchJobMonitorTimeout
        {
            set
            {
                BatchArgs.JobMonitorTimeout = value;
                this.DirectPropertyChangeTracker.Add("Batch.JobMonitorTimeout");
            }
        }

        [Serializable]
        public class Batch : ArgsBase
        {

            public string ResourceGroup { get; set; } = string.Empty;

            [DefaultValue(10)]
            public int BatchNodeCount { get; set; } = 10;
            public string BatchVmSize { get; set; } = null;
            [JsonIgnore]
            public string OutputContainerSasUrl { get; set; }
            [DefaultValue(false)]
            public bool DeleteBatchPool { get; set; } = false;
            [DefaultValue(true)]
            public bool DeleteBatchJob { get; set; } = true;
            [JsonIgnore]
            public string BatchJobName { get; set; } = null;
            [DefaultValue(true)]
            public bool PollBatchPoolStatus { get; set; } = true;
            public string BatchPoolName { get; set; } = null;
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public OsType BatchPoolOs { get; set; }
            public string ApplicationPackage { get; set; } = string.Empty;
            public int JobMonitorTimeout { get; set; } = 30;
            
        }
    }
}
