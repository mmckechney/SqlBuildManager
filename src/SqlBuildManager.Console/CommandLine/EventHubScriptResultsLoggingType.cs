using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventHubLogging
    {
        EssentialOnly = 0,
        VerboseMessages = 1,
        IndividualScriptResults = 2,
        ConsolidatedScriptResults = 4
    }
}
