using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.ServiceClient.Sbm.BuildService 
{
    public partial class BuildSettings
    {
        public DistributionType DistributionType
        {
            get;
            set;
        }

        public List<ServerConfigData> RemoteExecutionServers
        {
            get;
            set;
        }

        
    }
}
