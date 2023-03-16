using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Linq;
using System.Text;
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
                    packagenameOption.Copy(true),
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
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.RunLocalBuildAsync);
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
                    unitTestOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.RunThreadedExecution);
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
                    queryFileOption.Copy(true),
                    overrideOption.Copy(true),
                    outputFileOption.Copy(true),
                    defaultscripttimeoutOption,
                    silentOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.QueryDatabases);
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
