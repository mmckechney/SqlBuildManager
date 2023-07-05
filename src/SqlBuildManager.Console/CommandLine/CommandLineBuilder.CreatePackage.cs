using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
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
                    outputsbmOption.Copy(true),
                    golddatabaseOption.Copy(true),
                    goldserverOption.Copy(true),
                    databaseOption.Copy(true),
                    serverOption.Copy(true),
                    authtypeOption,
                    allowForObjectDeletionOption

                };
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreatePackageFromDiff);
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
                    outputsbmOption.Copy(true),
                    scriptListOption
                };
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreatePackageFromScripts);
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
                    outputsbmOption.Copy(true),
                    platinumdacpacSourceOption,
                    targetdacpacSourceOption,
                    allowForObjectDeletionOption

                };
                cmd.Handler = CommandHandler.Create<string, FileInfo, FileInfo, bool>(Worker.CreatePackageFromDacpacs);
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
                    platinumdacpacOption.Copy(true),
                    outputsbmOption.Copy(true),
                    databaseOption.Copy(true),
                    serverOption.Copy(true),
                    allowForObjectDeletionOption
                };
                cmd.AddRange(DatabaseAuthArgs);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.CreateFromDacpacDiff);
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
