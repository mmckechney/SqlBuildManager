using SqlSync.Connection;
using System.Collections.Generic;

namespace SqlBuildManager.Console.Queue
{
    public class TargetMessage
    {
        public string ServerName { get; set; }
        public List<DatabaseOverride> DbOverrideSequence { get; set; }
    }
}
