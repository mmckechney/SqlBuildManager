using SqlBuildManager.Interfaces.Console;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.Threaded
{
    public class LogMsg
    {
        private string sourceDacPac = string.Empty;
        private LogType logType = LogType.Message;
        private string runId = string.Empty;

        public LogMsg()
        {
        }
        
        /// <summary>
        /// Creates a LogMsg with the specified runId and sourceDacPac.
        /// </summary>
        public LogMsg(string runId, string sourceDacPac = null!)
        {
            this.runId = runId ?? string.Empty;
            if (!string.IsNullOrEmpty(sourceDacPac))
            {
                int lastSep = Math.Max(sourceDacPac.LastIndexOf('/'), sourceDacPac.LastIndexOf('\\'));
                this.sourceDacPac = lastSep >= 0 ? sourceDacPac.Substring(lastSep + 1) : sourceDacPac;
            }
        }
        
        [JsonPropertyName("_typeTag")]
        public string TypeTag { get; set; } = null!;
        
        [JsonPropertyName("JobName")]
        public string JobName { get; set; } = string.Empty;
       
        [JsonPropertyName("Message")]
        public string Message { get; set; } = string.Empty;
       
        [JsonPropertyName("DatabaseName")]
        public string DatabaseName { get; set; } = string.Empty;
       
        [JsonPropertyName("ServerName")]
        public string ServerName { get; set; } = string.Empty;
 
        [JsonPropertyName("RunId")]
        public string RunId
        {
            get => runId;
            set => runId = value ?? string.Empty;
        }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("LogType")]
        public LogType LogType
        {
            get => logType;
            set => logType = value;
        }
   
        [JsonPropertyName("SourceDacPac")]
        public string SourceDacPac
        {
            get => sourceDacPac;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    sourceDacPac = string.Empty;
                }
                else
                {
                    int lastSep = Math.Max(value.LastIndexOf('/'), value.LastIndexOf('\\'));
                    sourceDacPac = lastSep >= 0 ? value.Substring(lastSep + 1) : value;
                }
            }
        }

        [JsonPropertyName("ComputeHostName")]
        public string ComputeHostName { get; set; } = string.Empty;

        [JsonPropertyName("ConcurrencyTag")]
        public string ConcurrencyTag { get; set; } = string.Empty;

        [JsonPropertyName("ScriptLog")]
        public ScriptLogData ScriptLog { get; set; } = null!;
            
    }

    public class ScriptLogData
    {
        [JsonPropertyName("ScriptIndex")]
        public int ScriptIndex { get; set; } = -1;

        [JsonPropertyName("ScriptFileName")]
        public string ScriptFileName { get; set; } = string.Empty;

        [JsonPropertyName("ScriptText")]
        public string ScriptText { get; set; } = string.Empty;

        [JsonPropertyName("Result")]
        public string Result { get; set; } = string.Empty;

    }
}
