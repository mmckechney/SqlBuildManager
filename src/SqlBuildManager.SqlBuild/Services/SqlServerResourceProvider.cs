namespace SqlBuildManager.SqlBuild.Services
{
    /// <summary>
    /// SQL Server implementation of ISqlResourceProvider. Returns SQL Server-specific
    /// DDL and query strings from embedded resources.
    /// </summary>
    internal class SqlServerResourceProvider : ISqlResourceProvider
    {
        public string LoggingTableDdl => Properties.Resources.LoggingTable;

        public string LoggingTableCommitCheckIndex => Properties.Resources.LoggingTableCommitCheckIndex;

        public string LogScriptInsert => Properties.Resources.LogScript;

        public string CheckTableExistsQuery(string tableName)
        {
            return $"SELECT 1 FROM sys.objects WITH (NOLOCK) WHERE name = '{tableName}' AND type = 'U'";
        }

        public string GetBlockingScriptLogQuery()
        {
            return "SELECT * FROM SqlBuild_Logging WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1";
        }

        public string GetScriptRunLogQuery()
        {
            return "SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC";
        }

        public string GetObjectRunHistoryQuery()
        {
            return "SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE [ScriptFileName] = @ScriptFileName ORDER BY CommitDate DESC";
        }

        public string GetHasBlockingSqlLogQuery()
        {
            return "SELECT AllowScriptBlock,ScriptFileHash,CommitDate,ScriptText FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC";
        }

        public string GetBatchHasBlockingSqlLogQuery(int paramCount)
        {
            var sb = new System.Text.StringBuilder(
                "SELECT ScriptId, AllowScriptBlock, ScriptFileHash, CommitDate, ScriptText FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId IN (");
            for (int i = 0; i < paramCount; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append("@p").Append(i);
            }
            sb.Append(") ORDER BY ScriptId, CommitDate DESC");
            return sb.ToString();
        }
    }
}
