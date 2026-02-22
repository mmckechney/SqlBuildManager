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
        public string Level { get; set; } = string.Empty;

        [JsonPropertyName("MessageTemplate")]
        public string MessageTemplate { get; set; } = string.Empty;

        [JsonPropertyName("Properties")]
        public Properties Properties { get; set; } = null!;
    }

    public class Properties
    {
        [JsonPropertyName("LogMsg")]
        public LogMsg LogMsg { get; set; } = null!;

        [JsonPropertyName("SourceContext")]
        public string SourceContext { get; set; } = string.Empty;
    }
}
