namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Provides platform-specific SQL syntax information for batch delimiters,
    /// identifier quoting, hints, and other SQL dialect differences.
    /// </summary>
    public interface IScriptSyntaxProvider
    {
        /// <summary>Regex pattern for batch delimiter (e.g. "^ *GO *" for SQL Server, null for PostgreSQL)</summary>
        string BatchDelimiterPattern { get; }

        /// <summary>Whether the platform requires splitting scripts on batch delimiters</summary>
        bool RequiresBatchSplitting { get; }

        /// <summary>Read-uncommitted hint text (e.g. "WITH (NOLOCK)" for SQL Server, "" for PostgreSQL)</summary>
        string NoLockHint { get; }

        /// <summary>Start character for identifier quoting (e.g. "[" for SQL Server, "\"" for PostgreSQL)</summary>
        string IdentifierQuoteStart { get; }

        /// <summary>End character for identifier quoting (e.g. "]" for SQL Server, "\"" for PostgreSQL)</summary>
        string IdentifierQuoteEnd { get; }

        /// <summary>Default administrative database name (e.g. "master" for SQL Server, "postgres" for PostgreSQL)</summary>
        string DefaultAdminDatabase { get; }

        /// <summary>String concatenation operator (e.g. "+" for SQL Server, "||" for PostgreSQL)</summary>
        string StringConcatOperator { get; }

        /// <summary>Generates a TOP/LIMIT clause (e.g. "TOP(n)" for SQL Server, "LIMIT n" for PostgreSQL)</summary>
        string TopNRowsClause(int n);

        /// <summary>SQL literal for boolean true (e.g. "1" for SQL Server, "true" for PostgreSQL)</summary>
        string BooleanTrueLiteral { get; }
    }
}
