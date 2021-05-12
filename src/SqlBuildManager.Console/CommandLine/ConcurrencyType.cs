using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBuildManager.Console.CommandLine
{

    [Serializable]
    public enum ConcurrencyType
    {
        Count,
        Server,
        MaxPerServer
    }
}
