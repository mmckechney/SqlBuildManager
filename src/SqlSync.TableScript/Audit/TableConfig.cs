using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.TableScript.Audit
{
    public class TableConfig
    {
        public string TableName = string.Empty;
        public SQLSyncAuditingDatabaseTableToAudit ConfigData = null;
        public TableConfig()
        {
        }
        public TableConfig(string tableName, SQLSyncAuditingDatabaseTableToAudit configData)
        {
            this.TableName = tableName;
            this.ConfigData = configData;
        }
        public override string ToString()
        {
            return this.TableName;
        }


    }
}
