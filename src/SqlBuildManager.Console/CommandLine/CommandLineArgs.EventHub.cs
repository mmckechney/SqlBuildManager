using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;
using Microsoft.SqlServer.Management.Assessment.Expressions;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
        [JsonIgnore]
        public string EventHubResourceGroup
        {
            set
            {
                EventHubArgs.ResourceGroup = value; 
                this.DirectPropertyChangeTracker.Add("EventHub.ResourceGroup");
            }
        }

        [JsonIgnore]
        public string EventHubSubscriptionId
        {
            set
            {
                EventHubArgs.SubscriptionId = value;
                this.DirectPropertyChangeTracker.Add("EventHub.SubscriptionId");
            }
        }

        private EventHubLogging[] _EventHubLogging = new EventHubLogging[] { CommandLine.EventHubLogging.EssentialOnly };

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public EventHubLogging[] EventHubLogging
        {
            get
            {
                return _EventHubLogging;
            }
            set
            {
                if(value == null)
                {
                    _EventHubLogging = null;
                    return;
                }
                if (_EventHubLogging != null && _EventHubLogging.Length > 0)
                {
                    _EventHubLogging = _EventHubLogging.Concat(value).ToArray();
                }
                _EventHubLogging = _EventHubLogging.Distinct().ToArray();
                EventHubArgs.Logging = _EventHubLogging;
            }
        }

        public class EventHub : ArgsBase
        {
            public string ResourceGroup { get; set; } = string.Empty;

            public string SubscriptionId { get; set; } = string.Empty;

            private EventHubLogging[] _logging = new EventHubLogging[] { CommandLine.EventHubLogging.EssentialOnly };
            public EventHubLogging[] Logging
            {
                get
                {
                    return _logging;
                }
                set
                {
                    _logging = _logging.Concat(value).Distinct().ToArray();
                }
            }
        }
    }
}
