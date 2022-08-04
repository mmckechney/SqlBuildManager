using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Kubernetes
{
    internal enum PodStatus
    {
        Unknown,
        Running,
        Completed,
        Error, 
        Pending,
        KubectlError
    }
}
