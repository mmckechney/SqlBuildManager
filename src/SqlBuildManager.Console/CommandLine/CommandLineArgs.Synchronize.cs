using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
        [JsonIgnore]
        public Synchronize SynchronizeArgs { get; set; } = new Synchronize();
        
        [JsonIgnore]
        public virtual string GoldServer
        {
            set { SynchronizeArgs.GoldServer = value; }
        }
        [JsonIgnore]
        public virtual string GoldDatabase
        {
            set { SynchronizeArgs.GoldDatabase = value; }
        }
        [Serializable]
        public class Synchronize
        {
            public virtual string GoldDatabase { get; set; }
            public virtual string GoldServer { get; set; }
        }
    }
}
