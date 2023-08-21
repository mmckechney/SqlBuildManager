using System;

namespace SqlBuildManager.Console.CommandLine
{

    [Serializable]
    public enum ConcurrencyType
    {
        Count,
        Server,
        MaxPerServer, 
        Tag, 
        MaxPerTag
    }
}
