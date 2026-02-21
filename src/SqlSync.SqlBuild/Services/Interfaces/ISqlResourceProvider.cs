namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Provides platform-specific SQL resource strings for logging table DDL,
    /// insert statements, and diagnostic queries.
    /// </summary>
    public interface ISqlResourceProvider
    {
        /// <summary>DDL script to create the build logging table</summary>
        string LoggingTableDdl { get; }

        /// <summary>DDL script to create the commit-check index on the logging table</summary>
        string LoggingTableCommitCheckIndex { get; }

        /// <summary>Parameterized INSERT statement for the logging table</summary>
        string LogScriptInsert { get; }

        /// <summary>Returns a query to check if a table exists in the database</summary>
        string CheckTableExistsQuery(string tableName);

        /// <summary>Returns a query to get blocking script log entries by script ID</summary>
        string GetBlockingScriptLogQuery();

        /// <summary>Returns a query to get the full script run log by script ID, ordered by commit date descending</summary>
        string GetScriptRunLogQuery();

        /// <summary>Returns a query to get the full script run log by script file name, ordered by commit date descending</summary>
        string GetObjectRunHistoryQuery();

        /// <summary>Returns a query to get blocking information with hash/date for a script ID</summary>
        string GetHasBlockingSqlLogQuery();
    }
}
