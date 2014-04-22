using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.SqlBuild
{
    public partial class SqlSyncBuildData
    {
        partial class ScriptRow
        {
            [System.Xml.Serialization.XmlIgnore]
            public SqlSync.SqlBuild.ScriptStatusType ScriptRunStatus = ScriptStatusType.Unknown;

            [System.Xml.Serialization.XmlIgnore]
            public DateTime LastCommitDate;

            [System.Xml.Serialization.XmlIgnore]
            public DateTime ServerChangeDate;

            [System.Xml.Serialization.XmlIgnore]
            public ScriptStatusType PolicyCheckState = ScriptStatusType.PolicyNotRun;


        }
    }
}
