using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlBuildManager.Console.ContainerShared;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        // Local option for batch monitor flag
        private static Option<bool> batchMonitorOption = new Option<bool>("--monitor") { Description = "Monitor active progress via Azure Event Hub Events (if configured). To get detailed database statuses, also use the --stream argument" };
        
        /// <summary>
        /// For updating multiple databases simultaneously using Azure batch services
        /// </summary>
        private static Command BatchRunCommand
        {
            get
            {
                var cmd = new Command("run", "For updating multiple databases simultaneously using Azure batch services")
                {
                    overrideRequiredOption,

                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    platinumdacpacOption,
                    packagenameNotReqOption,
                    targetdacpacOption,
                    forcecustomdacpacOption,
                    platinumdbsourceOption,
                    platinumserversourceOption,
                    batchJobMonitorTimeoutMin,
                    batchMonitorOption,
                    unitTestOption,

                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(BatchSettingsOptions);
                cmd.AddRange(BatchComputeOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConnectionAndSecretsOptionsForBatch);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.AddRange(ConcurrencyOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var monitor = parseResult.GetValue(batchMonitorOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.Batch_RunBuild(cmdLine: cmdLine, monitor: monitor, unittest: unittest, stream: stream);
                });
                return cmd;
            }
        }

        /// <summary>
        /// [Internal use only] - this commmand is used to send threaded commands to Azure Batch Nodes
        /// </summary>
        private static Command BatchRunThreadedCommand
        {
            get
            {
                var cmd = new Command("runthreaded", "[Internal use only] - this commmand is used to send threaded commands to Azure Batch Nodes")
                {
                    overrideOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    platinumdacpacOption,
                    packagenameNotReqOption,
                    targetdacpacOption,
                    forcecustomdacpacOption,
                    platinumdbsourceOption,
                    platinumserversourceOption,
                    outputcontainersasurlOption,
                    transactionalOption,
                    timeoutretrycountOption,
                    unitTestOption,
                    //these two options aren't used and are added just for reusability in unit tests
                    new Option<bool>("--monitor"){Hidden = true},

                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(BatchSettingsOptions);
                cmd.AddRange(BatchComputeOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConnectionAndSecretsOptionsForBatch);
                cmd.AddRange(ConcurrencyOptions);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.AddRange(EventHubResourceOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    return await Worker.RunThreadedExecutionAsync(cmdLine: cmdLine, unittest: unittest);
                });
                cmd.Hidden = true;
                return cmd;
            }
        }

        /// <summary>
        /// Pre-stage the Azure Batch VM nodes
        /// </summary>
        private static Command BatchPreStageCommand
        {
            get
            {
                var cmd = new Command("prestage", "Pre-stage the Azure Batch VM nodes")
                {
                    pollbatchpoolstatusOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                BatchComputeOptions.ForEach(o => { if (o.Name != "deletebatchpool") cmd.Add(o); });
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.Add(keyVaultNameOption);
                cmd.Add(batchaccountnameOption);
                cmd.Add(batchaccountkeyOption);
                cmd.Add(batchaccounturlOption);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.Batch_PreStageNodes(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Azure Batch Clean Up - remove VM nodes
        /// </summary>
        private static Command BatchCleanUpCommand
        {
            get
            {
                var cmd = new Command("cleanup", "Azure Batch Clean Up - remove VM nodes")
                {
                    pollbatchpoolstatusOption,
                    keyVaultNameOption,
                    batchaccountnameOption,
                    batchaccountkeyOption,
                    batchaccounturlOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return Worker.Batch_NodeCleanUp(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Delete Jobs from the Azure batch account
        /// </summary>
        private static Command BatchDeleteJobsCommand
        {
            get
            {
                var cmd = new Command("deletejobs", "Delete Jobs from the Azure batch account")
                {
                    keyVaultNameOption,
                    batchaccountnameOption,
                    batchaccountkeyOption,
                    batchaccounturlOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.Hidden = true;
                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return Worker.Batch_DeleteJob(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Save a settings json file for Batch arguments (see Batch documentation)
        /// </summary>
        private static Command BatchSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Save a settings json file for Batch arguments (see Batch documentation)")
                {

                    deletebatchjobOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    //Additional settings
                    timeoutretrycountOption,
                    pollbatchpoolstatusOption,
                    silentOption,
                    cleartextOption
                };
                cmd.AddRange(SettingsFileNewOptions);
                cmd.AddRange(BatchComputeOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConnectionAndSecretsOptionsForBatch);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.AddRange(ConcurrencyOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var clearText = parseResult.GetValue(cleartextOption);
                    return Worker.SaveAndEncryptBatchSettings(cmdLine: cmdLine, clearText: clearText);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Sends database override targets to Service Bus Topic
        /// </summary>
        private static Command BatchEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    batchjobnameRequiredOption,
                    threadedConcurrencyRequiredOption,
                    overrideRequiredOption,
                    keyVaultNameOption,
                    serviceBusconnectionOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.EnqueueOverrideTargets(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
        /// </summary>
        private static Command BatchDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    batchjobnameOption,
                    threadedConcurrencyTypeOption,
                    keyVaultNameOption,
                    serviceBusconnectionOption,

                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.DeQueueOverrideTargets(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Run a SELECT query across multiple databases using Azure Batch
        /// </summary>
        private static Command BatchQueryCommand
        {
            get
            {
                var cmd = new Command("query", "Run a SELECT query across multiple databases using Azure Batch")
                {
                    overrideRequiredOption,
                    queryFileRequiredOption,
                    outputFileRequiredOption,
                    silentOption,
                    deletebatchjobOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    jobnameOption,
                    unitTestOption,
                    batchMonitorOption,
                };
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(BatchComputeOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConnectionAndSecretsOptionsForBatch);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.AddRange(ConcurrencyOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var monitor = parseResult.GetValue(batchMonitorOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.Batch_RunQuery(cmdLine: cmdLine, monitor: monitor, unittest: unittest, stream: stream);
                });
                return cmd;
            }
        }

        /// <summary>
        /// [Internal use only] - this commmand is used to send query commands to Azure Batch Nodes
        /// </summary>
        private static Command BatchQueryThreadedCommand
        {
            get
            {
                var cmd = new Command("querythreaded", "[Internal use only] - this commmand is used to send query commands to Azure Batch Nodes")
                {
                    overrideOption,
                    queryFileRequiredOption,
                    outputFileRequiredOption,
                    deletebatchjobOption,
                    rootloggingpathOption,
                    defaultscripttimeoutOption,
                    //Batch to Threaded node options
                    outputcontainersasurlOption,
                    transactionalOption,
                    timeoutretrycountOption,
                    silentOption,
                    jobnameOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(BatchComputeOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConnectionAndSecretsOptionsForBatch);
                cmd.AddRange(IdentityArgumentsForBatch);
                cmd.AddRange(ConcurrencyOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.QueryDatabasesAsync(cmdLine);
                });
                cmd.Hidden = true;
                return cmd;
            }
        }

        /// <summary>
        /// Commands for setting and executing a batch run or batch query
        /// </summary>
        private static Command BatchCommand
        {
            get
            {
                var tmp = new Command("batch", "Commands for setting and executing a batch run or batch query");
                tmp.Add(BatchSaveSettingsCommand);
                tmp.Add(BatchPreStageCommand);
                tmp.Add(BatchEnqueueTargetsCommand);
                tmp.Add(BatchRunCommand);
                tmp.Add(BatchQueryCommand);
                tmp.Add(BatchCleanUpCommand);
                tmp.Add(BatchDequeueTargetsCommand);
                tmp.Add(BatchRunThreadedCommand);
                tmp.Add(BatchQueryThreadedCommand);
                tmp.Add(BatchDeleteJobsCommand);
                return tmp;
            }
        }
    }
}
