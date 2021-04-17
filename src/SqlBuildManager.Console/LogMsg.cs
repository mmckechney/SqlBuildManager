using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using System.IO;
using SqlBuildManager.Console.Threaded;
namespace SqlBuildManager.Console.Threaded
{
    class LogMsg
    {
        private string sourceDacPac = string.Empty;
        private LogType logType = LogType.Message;
        private string runId = string.Empty;

        public LogMsg()
        {
        }
                
        public string Message { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;

        public string RunId
        {
            get
            {
                if (this.runId == string.Empty && !string.IsNullOrWhiteSpace(ThreadedExecution.RunID))
                {
                    this.runId = ThreadedExecution.RunID;
                }
                return this.runId;
            }
            set
            {
                this.runId = value;
            }
        }

        public LogType LogType
        {
            get
            {
                return this.logType;
            }
            set
            {
                this.logType = value;
            }
        }

        public string SourceDacPac
        {
            get
            {
                if (this.sourceDacPac == string.Empty && !string.IsNullOrWhiteSpace(ThreadedExecution.PlatinumDacPacFileName))
                {
                    this.sourceDacPac = Path.GetFileName(ThreadedExecution.PlatinumDacPacFileName);
                }
                return this.sourceDacPac;
            }
            set
            {
                this.sourceDacPac = Path.GetFileName(value);
            }
        }


    }
}
