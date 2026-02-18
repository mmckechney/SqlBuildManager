using System.IO;
using System.Reflection;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// PostgreSQL implementation of ISqlResourceProvider. Returns PostgreSQL-specific
    /// DDL and query strings from embedded resources.
    /// </summary>
    internal class PostgresResourceProvider : ISqlResourceProvider
    {
        private static readonly Assembly _assembly = typeof(PostgresResourceProvider).Assembly;

        private static string ReadEmbeddedResource(string resourceName)
        {
            var fullName = $"SqlSync.SqlBuild.SqlLogging.{resourceName}";
            using var stream = _assembly.GetManifestResourceStream(fullName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource not found: {fullName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static readonly string _loggingTableDdl = ReadEmbeddedResource("LoggingTable.PostgreSQL.sql");
        private static readonly string _loggingTableCommitCheckIndex = ReadEmbeddedResource("LoggingTableCommitCheckIndex.PostgreSQL.sql");
        private static readonly string _logScriptInsert = ReadEmbeddedResource("LogScript.PostgreSQL.sql");

        public string LoggingTableDdl => _loggingTableDdl;

        public string LoggingTableCommitCheckIndex => _loggingTableCommitCheckIndex;

        public string LogScriptInsert => _logScriptInsert;

        public string CheckTableExistsQuery(string tableName)
        {
            // PostgreSQL: use information_schema to check for table existence
            // Use a parameterized query to avoid SQL injection; caller should bind @TableName
            return "SELECT 1 FROM information_schema.tables WHERE table_name = lower(@TableName)";
        }

        public string GetBlockingScriptLogQuery()
        {
            return "SELECT * FROM sqlbuild_logging WHERE scriptid = @ScriptId AND allowscriptblock = true";
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
    }
}
