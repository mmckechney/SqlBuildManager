using SqlBuildManager.Interfaces.Console;
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
        [JsonPropertyName("_typeTag")]
        public string TypeTag { get; set; } = null;
        
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
            get
            {
                if (runId == string.Empty && !string.IsNullOrWhiteSpace(ThreadedManager.RunID))
                {
                    runId = ThreadedManager.RunID;
                }
                return runId;
            }
            set
            {
                runId = value;
            }
        }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("LogType")]
        public LogType LogType
        {
            get
            {
                return logType;
            }
            set
            {
                logType = value;
            }
        }
   
        [JsonPropertyName("SourceDacPac")]
        public string SourceDacPac
        {
            get
            {
                if (sourceDacPac == string.Empty && !string.IsNullOrWhiteSpace(ThreadedManager.PlatinumDacPacFileName))
                {
                    sourceDacPac = Path.GetFileName(ThreadedManager.PlatinumDacPacFileName);
                }
                return sourceDacPac;
            }
            set
            {
                sourceDacPac = Path.GetFileName(value);
            }
        }

        [JsonPropertyName("ComputeHostName")]
        public string ComputeHostName { get; set; } = string.Empty;

        [JsonPropertyName("ConcurrencyTag")]
        public string ConcurrencyTag { get; set; } = string.Empty;

        [JsonPropertyName("ScriptLog")]
        public ScriptLogData ScriptLog { get; set; } = null;
            
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
