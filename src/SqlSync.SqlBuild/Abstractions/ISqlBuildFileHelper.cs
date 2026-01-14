namespace SqlSync.SqlBuild
{
    public interface ISqlBuildFileHelper
    {
        string GetSHA1Hash(string[] batchScripts);
        string GetSHA1Hash(string batchScript);
        string JoinBatchedScripts(string[] batchScripts);
    }
}
