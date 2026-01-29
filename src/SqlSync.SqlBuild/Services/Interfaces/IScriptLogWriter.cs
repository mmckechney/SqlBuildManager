namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Handles writing script execution logs to files.
    /// </summary>
    internal interface IScriptLogWriter
    {
        /// <summary>
        /// Writes a script log entry to the log file.
        /// </summary>
        void WriteLog(ScriptLogWriteContext context, bool isError, ScriptLogEventArgs args);
    }

    /// <summary>
    /// Context information needed for script log writing.
    /// </summary>
    internal sealed class ScriptLogWriteContext
    {
        public string ScriptLogFileName { get; set; }
        public string ExternalScriptLogFileName { get; set; }
        public string ServerName { get; set; }
        public bool IsTransactional { get; set; }
    }
}
