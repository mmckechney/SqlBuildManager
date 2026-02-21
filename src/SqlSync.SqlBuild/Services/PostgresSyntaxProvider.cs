namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// PostgreSQL implementation of IScriptSyntaxProvider.
    /// PostgreSQL does not use GO batch delimiters, NOLOCK hints, or bracket quoting.
    /// </summary>
    internal class PostgresSyntaxProvider : IScriptSyntaxProvider
    {
        public string BatchDelimiterPattern => null;

        public bool RequiresBatchSplitting => false;

        public string NoLockHint => string.Empty;

        public string IdentifierQuoteStart => "\"";

        public string IdentifierQuoteEnd => "\"";

        public string DefaultAdminDatabase => "postgres";

        public string StringConcatOperator => "||";

        public string TopNRowsClause(int n) => $"LIMIT {n}";

        public string BooleanTrueLiteral => "true";
    }
}
