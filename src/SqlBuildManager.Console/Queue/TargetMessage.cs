using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBuildManager.Console.Queue
{
    public class TargetMessage
    {
        public string ServerName { get; set; }
        public List<DatabaseOverride> DbOverrideSequence { get; set; }
    }
}
