using System.IO;
using System.Reflection;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// MySQL implementation of ISqlResourceProvider. Returns MySQL-specific
    /// DDL and query strings from embedded resources.
    /// </summary>
    internal class MySqlResourceProvider : ISqlResourceProvider
    {
        private static readonly Assembly _assembly = typeof(MySqlResourceProvider).Assembly;

        private static string ReadEmbeddedResource(string resourceName)
        {
            var fullName = $"SqlSync.SqlBuild.SqlLogging.{resourceName}";
            using var stream = _assembly.GetManifestResourceStream(fullName);
            if (stream == null)
            {
                throw new FileNotFoundException($"Embedded resource not found: {fullName}");
            }
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static readonly string _loggingTableDdl = ReadEmbeddedResource("LoggingTable.MySQL.sql");
        private static readonly string _loggingTableCommitCheckIndex = ReadEmbeddedResource("LoggingTableCommitCheckIndex.MySQL.sql");
        private static readonly string _logScriptInsert = ReadEmbeddedResource("LogScript.MySQL.sql");

        public string LoggingTableDdl => _loggingTableDdl;

        public string LoggingTableCommitCheckIndex => _loggingTableCommitCheckIndex;

        public string LogScriptInsert => _logScriptInsert;

        public string CheckTableExistsQuery(string tableName)
        {
            return $"SELECT 1 FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '{tableName.ToLowerInvariant()}'";
        }

        public string GetBlockingScriptLogQuery()
        {
            return "SELECT * FROM sqlbuild_logging WHERE scriptid = @ScriptId AND allowscriptblock = 1";
        }

        public string GetScriptRunLogQuery()
        {
            return "SELECT * FROM sqlbuild_logging WHERE scriptid = @ScriptId ORDER BY commitdate DESC";
        }

        public string GetObjectRunHistoryQuery()
        {
            return "SELECT * FROM sqlbuild_logging WHERE scriptfilename = @ScriptFileName ORDER BY commitdate DESC";
        }

        public string GetHasBlockingSqlLogQuery()
        {
            return "SELECT allowscriptblock, scriptfilehash, commitdate, scripttext FROM sqlbuild_logging WHERE scriptid = @ScriptId ORDER BY commitdate DESC";
        }

        public string GetBatchHasBlockingSqlLogQuery(int paramCount)
        {
            var sb = new System.Text.StringBuilder(
                "SELECT scriptid, allowscriptblock, scriptfilehash, commitdate, scripttext FROM sqlbuild_logging WHERE scriptid IN (");
            for (int i = 0; i < paramCount; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }
                sb.Append("@p").Append(i);
            }
            sb.Append(") ORDER BY scriptid, commitdate DESC");
            return sb.ToString();
        }
    }
}
