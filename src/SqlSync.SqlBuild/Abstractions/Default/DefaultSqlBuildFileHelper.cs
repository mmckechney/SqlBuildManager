namespace SqlSync.SqlBuild
{
    internal sealed class DefaultSqlBuildFileHelper : ISqlBuildFileHelper
    {
        public void GetSHA1Hash(string[] batchScripts, out string textHash) => SqlBuildFileHelper.GetSHA1Hash(batchScripts, out textHash);
        public string JoinBatchedScripts(string[] batchScripts) => SqlBuildFileHelper.JoinBatchedScripts(batchScripts);
    }
}
