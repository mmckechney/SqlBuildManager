using SqlBuildManager.Interfaces.Console;
using System;
using System.Text.Json.Serialization;
using SqlBuildManager.Console.Threaded;
namespace SqlBuildManager.Console.Events
{
    public class EventHubMessageFormat
    {
        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("Level")]
        public string Level { get; set; }

        [JsonPropertyName("MessageTemplate")]
        public string MessageTemplate { get; set; }

        [JsonPropertyName("Properties")]
        public Properties Properties { get; set; }
    }

    public class Properties
    {
        [JsonPropertyName("LogMsg")]
        public LogMsg LogMsg { get; set; }

        [JsonPropertyName("SourceContext")]
        public string SourceContext { get; set; }
    }
}
