using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Batch;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int RunBatchCleanUp(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.CleanUpBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal static int RunBatchJobDelete(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            var retVal = Batch.BatchCleaner.DeleteAllCompletedJobs(cmdLine);

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal async static Task<int> RunBatchPreStage(CommandLineArgs cmdLine)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = await batchExe.PreStageBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            return retVal;
        }

        internal static int RunBatchExecution(CommandLineArgs cmdLine, bool monitor = false, bool unittest = false, bool stream = false)
        {
            bool initSuccess = false;
            (initSuccess, cmdLine) = Init(cmdLine);
            if (!initSuccess) return -8675;

            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);


            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);

            log.LogDebug("Entering Batch Execution");
            log.LogInformation("Running Batch Execution...");
            int retVal;
            string readOnlySas;
            Task monitorTask = null;


            //Register the monitoring events if designated
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) && monitor)
            {
                batchExe.BatchProcessStartedEvent += new Batch.BatchMonitorEventHandler(Batch_MonitorStart);
                batchExe.BatchExecutionCompletedEvent += new BatchMonitorEventHandler(Batch_MonitorEnd);
            }
            else
            {
                stream = false;
            }

            Task<(int, string)> batchExeTask = batchExe.StartBatch(stream, unittest);
            batchExeTask.Wait();
            (retVal, readOnlySas) = batchExeTask.Result;

            if (monitorTask != null)
            {
                monitorTask.Wait(500);
            }

            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.LogInformation("Completed Successfully");
            }
            else if (retVal == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                log.LogInformation("Datbases already in sync");
                retVal = (int)ExecutionReturn.Successful;
            }
            else
            {
                log.LogWarning("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            log.LogDebug("Exiting Batch Execution");

            return retVal;
        }

        private static Task<int> batchMonitorTask = null;
        private static void Batch_MonitorStart(object sender, BatchMonitorEventArgs e)
        {
            batchMonitorTask = MonitorServiceBusRuntimeProgress(e.CmdLine, e.Stream, DateTime.UtcNow, e.UnitTest);
        }
        private static void Batch_MonitorEnd(object sender, EventArgs e)
        {
            Worker.activeServiceBusMonitoring = false;
        }

        private static (int, CommandLineArgs) PrepForRemoteQueryExecution(CommandLineArgs cmdLine)
        {

            (bool success, cmdLine) = Init(cmdLine);
            if (!success)
            {
                return (-8675, cmdLine);
            }
            log = Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);
            var outpt = Validation.ValidateQueryArguments(ref cmdLine);
            if (outpt != 0)
            {
                return (outpt, cmdLine);
            }

            //Always run the remote Batch as silent or it will get hung up
            if (cmdLine.Silent == false)
            {
                cmdLine.Silent = true;
            }
            return (0, cmdLine);
        }
        internal static async Task<int> RunBatchQuery(CommandLineArgs cmdLine)
        {
            (int success, cmdLine) = PrepForRemoteQueryExecution(cmdLine);
            if (success != 0)
            {
                return success;
            }

            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine, cmdLine.QueryFile.FullName, Path.Combine(cmdLine.RootLoggingPath, cmdLine.OutputFile.Name));

            log.LogDebug("Entering Batch Query Execution");
            log.LogInformation("Running Batch Query Execution...");
            int retVal;
            string readOnlySas;

            (retVal, readOnlySas) = batchExe.StartBatch().GetAwaiter().GetResult();

            if (!string.IsNullOrWhiteSpace(readOnlySas))
            {
                log.LogInformation("Downloading the consolidated output file...");
                try
                {
                    if (await StorageManager.DownloadBlobToLocal(readOnlySas, cmdLine.OutputFile.FullName))
                    {
                        log.LogInformation($"Output file copied locally to {cmdLine.OutputFile.FullName}");
                    }
                }
                catch (Exception exe)
                {
                    log.LogError($"Unable to download the output file:  {exe.Message}");
                }
            }

            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.LogInformation("Completed Successfully");
            }
            else
            {
                log.LogWarning("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            log.LogDebug("Exiting Batch Execution");

            return retVal;
        }

    }
}
