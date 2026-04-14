using Spectre.Console;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace SqlBuildManager.Console.CommandLine
{
    internal class GroupedHelpAction : SynchronousCommandLineAction
    {
        private readonly HelpAction _defaultHelp;
        private readonly List<OptionGroup> _groups;

        public GroupedHelpAction(HelpAction defaultHelp, List<OptionGroup> groups)
        {
            _defaultHelp = defaultHelp;
            _groups = groups;
        }

        public override int Invoke(ParseResult parseResult)
        {
            var command = parseResult.CommandResult.Command;

            if (_groups.Count == 0)
            {
                return _defaultHelp.Invoke(parseResult);
            }

            // Description
            if (!string.IsNullOrEmpty(command.Description))
            {
                AnsiConsole.MarkupLine("[bold]Description:[/]");
                AnsiConsole.MarkupLine($"  {Markup.Escape(command.Description)}");
                AnsiConsole.WriteLine();
            }

            // Usage line
            AnsiConsole.MarkupLine("[bold]Usage:[/]");
            var parentNames = GetCommandPath(parseResult);
            AnsiConsole.MarkupLine($"  {Markup.Escape(parentNames)} [[options]]");
            AnsiConsole.WriteLine();

            // Track which options are in a group
            var groupedOptions = _groups.SelectMany(g => g.Options).ToHashSet();

            foreach (var group in _groups)
            {
                AnsiConsole.MarkupLine($"[bold]{Markup.Escape(group.Name)}:[/]");
                foreach (var opt in group.Options)
                {
                    if (opt.Hidden) continue;
                    RenderOption(opt);
                }
                AnsiConsole.WriteLine();
            }

            // Render ungrouped, non-hidden options (--help, --version, etc.)
            var ungrouped = command.Options
                .Where(o => !groupedOptions.Contains(o) && !o.Hidden)
                .ToList();

            if (ungrouped.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold]Other Options:[/]");
                foreach (var opt in ungrouped)
                {
                    RenderOption(opt);
                }
                AnsiConsole.WriteLine();
            }

            // Arguments
            var arguments = command.Arguments.Where(a => !a.Hidden).ToList();
            if (arguments.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold]Arguments:[/]");
                foreach (var arg in arguments)
                {
                    var name = $"<{arg.Name}>";
                    var desc = arg.Description ?? "";
                    var paddedName = name.PadRight(38);
                    AnsiConsole.MarkupLine($"  {Markup.Escape(paddedName)}{Markup.Escape(desc)}");
                }
                AnsiConsole.WriteLine();
            }

            return 0;
        }

        private static void RenderOption(Option opt)
        {
            var allNames = new List<string> { opt.Name };
            foreach (var alias in opt.Aliases)
                allNames.Add(alias);
            var aliases = string.Join(", ", allNames.Distinct().OrderBy(a => a.Length));
            var desc = opt.Description ?? "";
            var required = opt.Required ? " [red](required)[/]" : "";
            var paddedAliases = aliases.PadRight(38);
            AnsiConsole.MarkupLine($"  {Markup.Escape(paddedAliases)}{Markup.Escape(desc)}{required}");
        }

        private static string GetCommandPath(ParseResult parseResult)
        {
            var parts = new List<string>();
            var current = parseResult.CommandResult;
            while (current != null)
            {
                parts.Insert(0, current.Command.Name);
                current = current.Parent as CommandResult;
            }
            return string.Join(" ", parts);
        }
    }
}
