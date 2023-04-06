using Microsoft.Extensions.Logging;
using System.Text.Json;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ehm = SqlBuildManager.Console.Events.EventManager;
namespace SqlBuildManager.Console.Threaded
{
    public class ThreadedLogging
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ILogger logEventHub;
        private static ILogger logCommitRun;
        private static ILogger logErrorRun;
        private static ILogger logFailures;
        private static ILogger logSuccess;
        private static ILogger logRuntime;
        public static bool TheadedLoggingInitiated = false;
        private string runId;
        CommandLineArgs cmdLine;
        private bool isVerboseEhMessages = false;
        private bool isScriptResultLogging = false;
        public ThreadedLogging(CommandLineArgs cmdLine, string runId)
        {
            this.cmdLine = cmdLine;
            this.runId = runId;
            InitThreadedLogging();
        }
        public void Flush()
        {
            Logging.Threaded.Configure.CloseAndFlushAllLoggers(true);
        }
        public void InitThreadedLogging()
        {

            log.LogInformation("Initilizing Threaded Execution loggers...");
            if (ConnectionStringValidator.IsEventHubConnectionString(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                logEventHub = SqlBuildManager.Logging.Threaded.EventHubLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.ConnectionArgs.EventHubConnectionString);
            }
            else
            {
                (string namespacename, string ehName) = ehm.GetEventHubNamespaceAndName(cmdLine.ConnectionArgs.EventHubConnectionString);
                log.LogInformation($"Using Managed Identity '{cmdLine.IdentityArgs.ClientId}' for Event Logging");
                logEventHub = SqlBuildManager.Logging.Threaded.EventHubLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, namespacename, ehName, cmdLine.IdentityArgs.ClientId);
            }
            logRuntime = SqlBuildManager.Logging.Threaded.RuntimeLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);

            logFailures = SqlBuildManager.Logging.Threaded.FailureDatabaseLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);
            logSuccess = SqlBuildManager.Logging.Threaded.SuccessDatabaseLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);

            logCommitRun = SqlBuildManager.Logging.Threaded.CommitLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);
            logErrorRun = SqlBuildManager.Logging.Threaded.ErrorLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, cmdLine.RootLoggingPath);

            ThreadedLogging.TheadedLoggingInitiated = true;
            this.isVerboseEhMessages = cmdLine.EventHubLogging.Contains(EventHubLogging.VerboseMessages);
            this.isScriptResultLogging = cmdLine.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults) || cmdLine.EventHubLogging.Contains(EventHubLogging.ConsolidatedScriptResults);
        }
        public void WriteToLog(LogMsg msg)
        {
            msg.JobName = cmdLine.JobName;
            msg.ComputeHostName = System.Environment.MachineName;
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug(JsonSerializer.Serialize<LogMsg>(msg));
            }
            if (!ThreadedLogging.TheadedLoggingInitiated)
            {
                InitThreadedLogging();
            }
            log.LogInformation($"{runId}  {msg.ServerName}  {msg.DatabaseName}:{msg.Message}");

            switch (msg.LogType)
            {
                case LogType.FailureDatabases:
                    logFailures.LogInformation(msg.Message);
                    return;

                case LogType.SuccessDatabases:
                    logSuccess.LogInformation(msg.Message);
                    return;

                case LogType.Commit:
                    logCommitRun.LogInformation($"{runId}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");
                    logEventHub?.LogInformation("{@LogMsg}", msg);
                    return;

                case LogType.Error:
                    logErrorRun.LogInformation($"{runId}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");
                    logEventHub?.LogInformation("{@LogMsg}", msg);
                    return;
                    
                case LogType.WorkerCompleted:
                    logRuntime.LogInformation($"{runId}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");
                    logEventHub?.LogInformation("{@LogMsg}", msg);
                    return;
                    
                case LogType.ScriptLog:
                    if (this.isScriptResultLogging)
                    {
                        logEventHub?.LogInformation("{@LogMsg}", msg);
                    }
                    return;

                case LogType.Message:
                default:
                    logRuntime.LogInformation($"{runId}  {msg.ServerName}  {msg.DatabaseName}: {msg.Message}");
                    if(this.isVerboseEhMessages)
                    {
                        logEventHub?.LogDebug("{@LogMsg}", msg);
                    }

                    return;
            }

        }

    }
}
