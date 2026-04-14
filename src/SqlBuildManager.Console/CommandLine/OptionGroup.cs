using System.Collections.Generic;
using System.CommandLine;

namespace SqlBuildManager.Console.CommandLine
{
    /// <summary>
    /// Associates options with a named group for help display.
    /// </summary>
    public class OptionGroup
    {
        public string Name { get; }
        public List<Option> Options { get; }

        public OptionGroup(string name, List<Option> options)
        {
            Name = name;
            Options = options;
        }
    }
}
