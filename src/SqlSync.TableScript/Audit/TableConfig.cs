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
            TableName = tableName;
            ConfigData = configData;
        }
        public override string ToString()
        {
            return TableName;
        }


    }
}
