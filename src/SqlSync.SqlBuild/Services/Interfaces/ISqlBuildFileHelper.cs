namespace SqlSync.SqlBuild.Services
{
    public interface ISqlBuildFileHelper
    {
        string GetSHA1Hash(string[] batchScripts);
        string GetSHA1Hash(string batchScript);
        string JoinBatchedScripts(string[] batchScripts);
    }
}
