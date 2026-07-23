namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// MySQL implementation of IScriptSyntaxProvider.
    /// MySQL does not use GO batch delimiters, NOLOCK hints, or bracket quoting.
    /// </summary>
    internal class MySqlSyntaxProvider : IScriptSyntaxProvider
    {
        public string BatchDelimiterPattern => null!;

        public bool RequiresBatchSplitting => false;

        public string NoLockHint => string.Empty;

        public string IdentifierQuoteStart => "`";

        public string IdentifierQuoteEnd => "`";

        public string DefaultAdminDatabase => "mysql";

        public string StringConcatOperator => "||";

        public string TopNRowsClause(int n) => $"LIMIT {n}";

        public string BooleanTrueLiteral => "1";
    }
}
