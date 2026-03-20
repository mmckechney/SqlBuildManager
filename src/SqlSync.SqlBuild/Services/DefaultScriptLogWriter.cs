using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Default implementation of IScriptLogWriter that writes script logs to files.
    /// </summary>
    internal sealed class DefaultScriptLogWriter : IScriptLogWriter
    {
        private static readonly ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(DefaultScriptLogWriter));

        public void WriteLog(ScriptLogWriteContext context, bool isError, ScriptLogEventArgs e)
        {
            if (context.ScriptLogFileName == null)
                throw new NullReferenceException("Attempting to write to Script Log File, but ScriptLogFileName is null");

            // Write header if file doesn't exist or starting a new transaction
            if (!File.Exists(context.ScriptLogFileName) || e.InsertStartTransaction)
            {
                using (StreamWriter sw = File.AppendText(context.ScriptLogFileName))
                {
                    sw.WriteLine("-- Start Time: " + DateTime.Now.ToString() + " --");
                    if (context.IsTransactional)
                    {
                        sw.WriteLine("-- Start Transaction --");
                        sw.WriteLine("BEGIN TRANSACTION");
                    }
                    else
                    {
                        sw.WriteLine("-- Executed without a transaction --");
                    }
                }
            }

            // Write the script entry
            using (StreamWriter sw = File.AppendText(context.ScriptLogFileName))
            {
                sw.WriteLine("/************************************");
                sw.WriteLine("Script #" + e.ScriptIndex.ToString() + "; Source File: " + e.SourceFile);
                sw.WriteLine("Server: " + context.ServerName + "; Run On Database:" + e.Database + "  */");
                if (e.Database.Length > 0)
                    sw.WriteLine("use " + e.Database + "\r\nGO\r\n");
                sw.WriteLine(e.SqlScript + "\r\nGO\r\n");
                sw.WriteLine("/*Script #" + e.ScriptIndex.ToString() + " Result: " + e.Results.Trim() + "  */");
                sw.Flush();
            }

            // Handle end of build and external log copy
            if (e.ScriptIndex == -10000 && !string.IsNullOrEmpty(context.ExternalScriptLogFileName))
            {
                using (StreamWriter sw = File.AppendText(context.ScriptLogFileName))
                {
                    sw.WriteLine("-- END Time: " + DateTime.Now.ToString() + " --");
                    sw.Flush();
                }

                try
                {
                    string tmpPath = Path.GetDirectoryName(context.ExternalScriptLogFileName)!;
                    if (!Directory.Exists(tmpPath) && !File.Exists(tmpPath))
                    {
                        log.LogInformation($"Creating External Log file directory '{tmpPath}'");
                        Directory.CreateDirectory(tmpPath);
                    }

                    File.Copy(context.ScriptLogFileName, context.ExternalScriptLogFileName, true);
                    log.LogInformation($"Copied log file to '{context.ExternalScriptLogFileName}'");
                }
                catch (Exception exe)
                {
                    log.LogError($"Error copying results file to '{context.ExternalScriptLogFileName}': {exe}");
                }
            }
        }
    }
}
