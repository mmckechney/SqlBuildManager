namespace SqlSync.SqlBuild
{
    public interface ISqlBuildFileHelper
    {
        void GetSHA1Hash(string[] batchScripts, out string textHash);
        string JoinBatchedScripts(string[] batchScripts);
    }
}
