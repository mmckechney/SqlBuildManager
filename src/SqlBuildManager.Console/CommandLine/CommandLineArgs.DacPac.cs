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
        public bool AllowObjectDelete { get; set; } = false;

        
        [JsonIgnore]
        public virtual string PlatinumDacpac
        {
            set
            {
                DacPacArgs.PlatinumDacpac = value;
                this.DirectPropertyChangeTracker.Add("DacPac.PlatinumDacpac");
            }
        }
        [JsonIgnore]
        public virtual string TargetDacpac
        {
            set
            {
                DacPacArgs.TargetDacpac = value;
                this.DirectPropertyChangeTracker.Add("DacPac.TargetDacpac");
            }
        }
        [JsonIgnore]
        public virtual bool ForceCustomDacPac
        {
            set
            {
                DacPacArgs.ForceCustomDacPac = value;
                this.DirectPropertyChangeTracker.Add("DacPac.ForceCustomDacPac");
            }
        }
        [JsonIgnore]
        public virtual string PlatinumDbSource
        {
            set
            {
                DacPacArgs.PlatinumDbSource = value;
                this.DirectPropertyChangeTracker.Add("DacPac.PlatinumDbSource");
            }
        }
        [JsonIgnore]
        public virtual string PlatinumServerSource
        {
            set
            {
                DacPacArgs.PlatinumServerSource = value;
                this.DirectPropertyChangeTracker.Add("DacPac.PlatinumServerSource");
            }
        }
        [Serializable]
        public class DacPac : ArgsBase
        {
            public virtual string PlatinumDacpac { get; set; } = string.Empty;
            public virtual string TargetDacpac { get; set; }
            public string PlatinumDbSource { get; set; }
            public string PlatinumServerSource { get; set; }
            [DefaultValue(false)]
            public bool ForceCustomDacPac { get; set; }
        }
    }
}
