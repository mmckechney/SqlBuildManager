namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultTokenReplacementService : ITokenReplacementService
    {
        public string ReplaceTokens(string script, SqlBuildHelper ctx) => ctx.PerformScriptTokenReplacement(script);
    }
}
