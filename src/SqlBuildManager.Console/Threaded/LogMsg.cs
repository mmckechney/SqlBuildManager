using SqlBuildManager.Interfaces.Console;
using System.IO;
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
        public string JobName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;

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


    }
}
