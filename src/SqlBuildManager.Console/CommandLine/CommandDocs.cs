using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public class CommandDoc
    {
        public CommandDoc() { }
        public string ParentCommand { get; set; } = string.Empty;
        public string ParentCommandDescription { get; set; } = string.Empty;
        
        public List<SubCommand> SubCommands { get; set; } = new List<SubCommand>();
  
        
    }
    public class SubCommand
    {
        public SubCommand() { }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
