using System;

namespace SqlSync.TableScript.Audit
{
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class AuditAutoDetectData : IAuditInfo
    {
        public AuditAutoDetectData()
        {

        }
        public string TableName { get; set; } = string.Empty;

        public int RowCount { get; set; } = 0;

        public bool HasAuditTable { get; set; } = false;

        public bool HasAuditUpdateTrigger { get; set; } = false;

        public bool HasAuditInsertTrigger { get; set; } = false;

        public bool HasAuditDeleteTrigger { get; set; } = false;

    }
}