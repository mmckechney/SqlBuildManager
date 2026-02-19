namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// SQL Server implementation of IScriptSyntaxProvider. Provides SQL Server-specific
    /// syntax patterns for batch delimiters, NOLOCK hints, bracket quoting, etc.
    /// </summary>
    internal class SqlServerSyntaxProvider : IScriptSyntaxProvider
    {
        public string BatchDelimiterPattern => Properties.Resources.RegexBatchParsingDelimiter;

        public bool RequiresBatchSplitting => true;

        public string NoLockHint => "WITH (NOLOCK)";

        public string IdentifierQuoteStart => "[";

        public string IdentifierQuoteEnd => "]";

        public string DefaultAdminDatabase => "master";

        public string StringConcatOperator => "+";

        public string TopNRowsClause(int n) => $"TOP({n})";
    }
}
