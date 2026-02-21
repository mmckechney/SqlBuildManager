using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        /// <summary>
        /// Create an SBM package from a diff calculated from two databases
        /// </summary>
        private static Command CreateFromDiffCommand
        {
            get
            {
                var cmd = new Command("fromdiff", "Creates an SBM package from a calculated diff between two databases")
                {
                    outputsbmRequiredOption,
                    golddatabaseOption,
                    goldserverOption,
                    databaseRequiredOption,
                    serverRequiredOption,
                    authtypeOption,
                    allowForObjectDeletionOption

                };
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.CreatePackageFromDiff(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Creates an SBM package or SBX project file from a list of scripts
        /// </summary>
        private static Command CreateFromScriptsCommand
        {
            get
            {
                var cmd = new Command("fromscripts", "Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension- .sbm or .sbx)")
                {
                    outputsbmRequiredOption,
                    scriptListOption
                };
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.CreatePackageFromScripts(cmdLine);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Creates an SBM package from differences between two DACPAC files
        /// </summary>
        private static Command CreateFromDacpacsCommand
        {
            get
            {
                //Create from differences between two DACPACS
                var cmd = new Command("fromdacpacs", "Creates an SBM package from differences between two DACPAC files")
                {
                    outputsbmRequiredOption,
                    platinumdacpacSourceOption,
                    targetdacpacSourceOption,
                    allowForObjectDeletionOption

                };
                cmd.SetAction((parseResult) => {
                    var outputsbm = parseResult.GetValue(outputsbmOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacSourceOption);
                    var targetdacpac = parseResult.GetValue(targetdacpacSourceOption);
                    var allowObjectDelete = parseResult.GetValue(allowForObjectDeletionOption);
                    return Worker.CreatePackageFromDacpacs(outputsbm!, platinumdacpac!, targetdacpac!, allowObjectDelete);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Extract a SBM package from a source --platinumdacpac and a target database connection
        /// </summary>
        private static Command CreateFromDacpacDiffCommand
        {
            get
            {
                //Create an SBM from a platium DACPAC file
                var cmd = new Command("fromdacpacdiff", "Extract a SBM package from a source --platinumdacpac and a target database connection")
                {
                    platinumdacpacRequiredOption,
                    outputsbmRequiredOption,
                    databaseRequiredOption,
                    serverRequiredOption,
                    allowForObjectDeletionOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.CreateFromDacpacDiff(cmdLine);
                });
                return cmd;
                
            }
        }

        private static Command CreateCommand
        {
            get
            {
                var cmd = new Command("create", "Creates an SBM package from script files (fromscripts),  calculated database differences (fromdiff) or diffs between two DACPAC files (fromdacpacs)");
                cmd.Add(CreateFromScriptsCommand);
                cmd.Add(CreateFromDiffCommand);
                cmd.Add(CreateFromDacpacsCommand);
                cmd.Add(CreateFromDacpacDiffCommand);
                return cmd;
            }
        }
       

    }
}
