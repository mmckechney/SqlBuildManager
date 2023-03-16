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
        [JsonIgnore]
        public StoredProcTesting StoredProcTestingArgs { get; set; } = new StoredProcTesting();


        [Serializable]
        public class StoredProcTesting
        {
            public virtual string SpTestFile { get; set; } = string.Empty;
            [DefaultValue(false)]
            public virtual bool SprocTestDesignated { get; set; } = false;
        }
    }
}
