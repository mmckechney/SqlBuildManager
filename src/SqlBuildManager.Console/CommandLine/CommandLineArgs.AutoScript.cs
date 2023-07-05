using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
      
        [Serializable]
        public class AutoScripting
        {
            [DefaultValue(false)]
            public virtual bool AutoScriptDesignated { get; set; } = false;
            public virtual string AutoScriptFileName { get; set; } = string.Empty;
        }
    }
}
