using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        private static RootCommand _rootCommand;

        /// <summary>
        /// Gets the configured RootCommand for parsing. Caches the result.
        /// </summary>
        public static RootCommand GetRootCommand()
        {
            if (_rootCommand == null)
            {
                _rootCommand = SetUp();
            }
            return _rootCommand;
        }

        /// <summary>
        /// Parses arguments directly using the root command.
        /// In System.CommandLine 2.0, we use RootCommand.Parse() directly.
        /// </summary>
        public static ParseResult Parse(string[] args)
        {
            var rootCommand = GetRootCommand();
            return rootCommand.Parse(args);
        }

        private static string OptionString(Option option)
        {
            var str = string.Join(", ", option.Aliases) + $" <{option.Name}>";

            if (option.Required)
            {
                return str + " (REQUIRED)";
            }
            else
            {
                return str;
            }
        }

        public static (CommandLineArgs, string) ParseArgumentsWithMessage(string[] args)
        {
            var parseResult = Parse(args);

            // If help was requested, skip error checking and return null with empty message
            // so the caller can invoke the parse result to display help
            if (args.Any(a => a == "-?" || a == "-h" || a == "--help"))
            {
                return (null, string.Empty);
            }

            if (parseResult.Errors.Count > 0)
            {
                return (null, string.Join<string>(System.Environment.NewLine, parseResult.Errors.Select(e => e.Message).ToArray()));
            }

            // Use our explicit binder instead of the deprecated ModelBinder
            var instance = CommandLineArgsBinder.Bind(parseResult);

            return (instance, string.Empty);
        }

        public static CommandLineArgs ParseArguments(string[] args)
        {
            (CommandLineArgs cmd, string msg) = ParseArgumentsWithMessage(args);
            if (cmd == null)
            {
                throw new System.Exception($"Unable to parse arguments: {msg}");
            }
            else
            {
                return cmd;
            }
        }

        public static List<List<string>> ListCommands()
        {
            var cmdList = new List<List<string>>();
            var rootCommand = GetRootCommand();

            var commands = rootCommand.Subcommands;
            cmdList.AddRange(commands.Select(c => new List<string> { c.Name }));

            foreach(var cmd in commands)
            {
                foreach (var sub in cmd.Subcommands)
                {
                    cmdList.Add(new List<string> { cmd.Name, sub.Name });
                    foreach (var sub2 in sub.Subcommands)
                    {
                        cmdList.Add(new List<string> { cmd.Name, sub.Name, sub2.Name });
                        foreach (var sub3 in sub2.Subcommands)
                        {
                            cmdList.Add(new List<string> { cmd.Name, sub.Name, sub2.Name, sub3.Name });
                        }
                    }
                }
            }


            return cmdList;
        }

        public static List<CommandDoc> ListCommands_ForDocs()
        {
            var cmdDocs = new List<CommandDoc>();
            var filledCmdDocs = new List<CommandDoc>();
            var rootCommand = GetRootCommand();

            var commands = rootCommand.Subcommands;
            cmdDocs.AddRange(commands.Select(c => new CommandDoc { ParentCommand = c.Name, ParentCommandDescription = c.Description }));

            foreach (var cmd in commands)
            {
                if (cmd.Hidden)
                {
                    continue;
                }
                var targetParent = cmdDocs.Where(c => c.ParentCommand == cmd.Name).FirstOrDefault();
                foreach (var sub in cmd.Subcommands)
                {

                    if (!sub.Hidden) { targetParent.SubCommands.Add(new SubCommand() { Name = sub.Name, Description = sub.Description }); } else { continue; }
                    foreach (var sub2 in sub.Subcommands)
                    {
                        if (!sub2.Hidden)
                            targetParent.SubCommands.Add(new SubCommand() { Name = $"{sub.Name} {sub2.Name}", Description = sub2.Description });
                        else
                            continue;
                        foreach (var sub3 in sub2.Subcommands)
                        {
                            if (!sub3.Hidden)
                                targetParent.SubCommands.Add(new SubCommand() { Name = $"{sub.Name} {sub2.Name} {sub2.Name}", Description = sub3.Description });
                        }
                    }
                }
                filledCmdDocs.Add(targetParent);
            }

            return filledCmdDocs;
        }

        public static Command FirstBuildRunCommand { get; private set; }
        public static Command FirstUtilityCommand { get; private set; }
        public static Command FirstPackageManagementCommand { get; private set; }
        public static Command FirstPackageInformationCommand { get; private set; }
        public static Command FirstAdditionalCommand { get; private set; }
    }
}
