namespace SqlSync.SqlBuild.Services
{
    internal interface ITokenReplacementService
    {
        string ReplaceTokens(string script, SqlBuildHelper ctx);
    }
}
