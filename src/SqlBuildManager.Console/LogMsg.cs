using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using System.IO;
namespace SqlBuildManager.Console
{
    class LogMsg
    {
        public LogMsg()
        {
            if (this.runId == string.Empty && !string.IsNullOrWhiteSpace(ThreadedExecution.RunID))
            {
                this.runId = ThreadedExecution.RunID;
                log4net.ThreadContext.Properties["RunId"] = this.runId;
            }
            if (this.sourceDacPac == string.Empty && !string.IsNullOrWhiteSpace(ThreadedExecution.PlatinumDacPacFileName))
            {
                this.sourceDacPac = ThreadedExecution.PlatinumDacPacFileName;
                log4net.ThreadContext.Properties["SourceDacPac"] = Path.GetFileName(this.sourceDacPac);
            }
            else
            {
                log4net.ThreadContext.Properties["SourceDacPac"] = string.Empty;
            }

            log4net.ThreadContext.Properties["DbName"] = string.Empty;
            log4net.ThreadContext.Properties["ServerName"] = string.Empty;
            log4net.ThreadContext.Properties["Message"] = string.Empty;
            log4net.ThreadContext.Properties["LogType"] = LogType.Message.GetDescription();
        }

        private string sourceDacPac = string.Empty;
        private string databaseName = string.Empty;
        private string serverName = string.Empty;
        private LogType logType = LogType.Message;
        private string runId = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string DatabaseName
        {
            get => databaseName;
            set
            {
                this.databaseName = value;
                log4net.ThreadContext.Properties["DbName"] = value;
            }
        } 
        public string ServerName
        {
            get => serverName; 
            set
            {
                this.serverName = value;
                log4net.ThreadContext.Properties["ServerName"] = value;
            }
        }
        
        public string RunId
        {
            get
            {
                if (this.runId == string.Empty && !string.IsNullOrWhiteSpace(ThreadedExecution.RunID))
                {
                    this.runId = ThreadedExecution.RunID;
                    log4net.ThreadContext.Properties["RunId"] = this.runId;
                }
                return this.runId;
            }
            set
            {
                this.runId = value;
                log4net.ThreadContext.Properties["RunId"] = value;
            }
        }
        public LogType LogType { 
            get
            {
                log4net.ThreadContext.Properties["LogType"] = this.logType.GetDescription();
                return this.logType;
            }
            set
            {
                this.logType = value;
                log4net.ThreadContext.Properties["LogType"] = value.GetDescription(); 
            }
        }
        public string SourceDacPac
        {
            get
            {
                if (this.sourceDacPac == string.Empty && !string.IsNullOrWhiteSpace(ThreadedExecution.PlatinumDacPacFileName))
                {
                    this.sourceDacPac = Path.GetFileName(ThreadedExecution.PlatinumDacPacFileName);
                    log4net.ThreadContext.Properties["SourceDacPac"] = this.sourceDacPac;
                }
                return this.sourceDacPac;
            }
            set
            {
                this.sourceDacPac = Path.GetFileName(value);
                log4net.ThreadContext.Properties["SourceDacPac"] = this.sourceDacPac;
            }
        }

        public override string ToString()
        {
            return $"{this.RunId} -- {this.ServerName}  {this.DatabaseName}: {this.Message}";
        }
    }
}
