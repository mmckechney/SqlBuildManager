using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        /// <summary>
        /// Performs a standard, local SBM execution via command line
        /// </summary>
        private static Command BuildCommand
        {
            get
            {
                var cmd = new Command("build", "Performs a standard, local SBM execution via command line")
                {
                    packagenameOption,
                    serverOption,
                    databaseOption,
                    rootloggingpathOption,
                    trialOption,
                    transactionalOption,
                    overrideOption,
                    descriptionOption,
                    buildrevisionOption,
                    logtodatabasenamedOption,
                    scriptsrcdirOption,
                    timeoutretrycountOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.SetGroupedHelp(
                    new OptionGroup("Database Connection", new List<Option> { serverOption, databaseOption }),
                    new OptionGroup("Build Options", new List<Option> { packagenameOption, overrideOption, trialOption, transactionalOption, descriptionOption, buildrevisionOption, scriptsrcdirOption, timeoutretrycountOption }),
                    new OptionGroup("Authentication", DatabaseAuthArgs),
                    new OptionGroup("Logging", new List<Option> { rootloggingpathOption, logtodatabasenamedOption })
                );
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.RunLocalBuildAsync(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// For updating multiple databases simultaneously from the current machine
        /// </summary>
        private static Command ThreadedRunCommand
        {
            get
            {
                var cmd = new Command("run", "For updating multiple databases simultaneously from the current machine")
                {
                    packagenameOption,
                    rootloggingpathOption,
                    trialOption,
                    transactionalOption,
                    overrideOption,
                    descriptionOption,
                    buildrevisionOption,
                    logtodatabasenamedOption,
                    scriptsrcdirOption,
                    platinumdacpacOption,
                    targetdacpacOption,
                    forcecustomdacpacOption,
                    platinumdbsourceOption,
                    platinumserversourceOption,
                    timeoutretrycountOption,
                    defaultscripttimeoutOption,
                    unitTestOption,
                    jobnameOption,
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    serviceBusconnectionOption,
                    eventhubconnectionOption,
                    eventhubResourceGroupOption,
                    eventhubSubscriptionOption,
                    eventHubLoggingTypeOption,
                    aciMonitorOption,
                    streamEventsOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.SetGroupedHelp(
                    new OptionGroup("Build Options", new List<Option> { packagenameOption, overrideOption, trialOption, transactionalOption, descriptionOption, buildrevisionOption, scriptsrcdirOption, timeoutretrycountOption, defaultscripttimeoutOption }),
                    new OptionGroup("DACPAC", new List<Option> { platinumdacpacOption, targetdacpacOption, forcecustomdacpacOption, platinumdbsourceOption, platinumserversourceOption }),
                    new OptionGroup("Authentication", DatabaseAuthArgs),
                    new OptionGroup("Concurrency", ConcurrencyOptions),
                    new OptionGroup("Logging", new List<Option> { rootloggingpathOption, logtodatabasenamedOption })
                );
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    using var monitorCancellation = new CancellationTokenSource();
                    Task<int>? monitorTask = null;
                    if (parseResult.GetValue(aciMonitorOption))
                    {
                        monitorTask = Worker.MonitorServiceBusRuntimeProgress(
                            cmdLine,
                            parseResult.GetValue(streamEventsOption),
                            DateTime.UtcNow.AddMinutes(-15),
                            unittest,
                            cancellationToken: monitorCancellation.Token);
                    }

                    var result = await Worker.RunThreadedExecutionAsync(cmdLine: cmdLine, unittest: unittest);
                    if (monitorTask != null)
                    {
                        Worker.StopServiceBusRuntimeMonitoring();
                        monitorCancellation.Cancel();
                        try
                        {
                            await monitorTask.WaitAsync(TimeSpan.FromSeconds(10));
                        }
                        catch (OperationCanceledException) when (monitorCancellation.IsCancellationRequested)
                        {
                        }
                        catch (TimeoutException)
                        {
                        }
                    }

                    return result;
                });
                return cmd;
            }
        }

        /// <summary>
        /// Run a SELECT query across multiple databases
        /// </summary>
        private static Command ThreadedQueryCommand
        {
            get
            {
                var cmd = new Command("query", "Run a SELECT query across multiple databases")
                {
                    queryFileRequiredOption,
                    overrideRequiredOption,
                    outputFileRequiredOption,
                    defaultscripttimeoutOption,
                    silentOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                cmd.SetGroupedHelp(
                    new OptionGroup("Query", new List<Option> { queryFileRequiredOption, outputFileRequiredOption, silentOption }),
                    new OptionGroup("Database Targets", new List<Option> { overrideRequiredOption, defaultscripttimeoutOption }),
                    new OptionGroup("Authentication", DatabaseAuthArgs),
                    new OptionGroup("Concurrency", ConcurrencyOptions)
                );
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.QueryDatabasesAsync(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Threaded base commands
        /// </summary>
        private static Command ThreadedCommand
        {
            get
            {
                var tmp = new Command("threaded", "For updating multiple or querying databases simultaneously from the current machine");
                tmp.Add(ThreadedQueryCommand);
                tmp.Add(ThreadedRunCommand);
                return tmp;
            }
        }
    }
}
