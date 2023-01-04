using SqlBuildManager.Interfaces.Console;
using System;
using System.Text.Json.Serialization;
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

    public class LogMsg
    {
        [JsonPropertyName("_typeTag")]
        public string TypeTag { get; set; }

        [JsonPropertyName("JobName")]
        public string JobName { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }

        [JsonPropertyName("DatabaseName")]
        public string DatabaseName { get; set; }

        [JsonPropertyName("ServerName")]
        public string ServerName { get; set; }

        [JsonPropertyName("RunId")]
        public string RunId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("LogType")]
        public LogType LogType { get; set; }

        [JsonPropertyName("SourceDacPac")]
        public object SourceDacPac { get; set; }
    }

    public class Properties
    {
        [JsonPropertyName("LogMsg")]
        public LogMsg LogMsg { get; set; }

        [JsonPropertyName("SourceContext")]
        public string SourceContext { get; set; }
    }
}
