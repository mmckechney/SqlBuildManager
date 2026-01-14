namespace SqlSync.SqlBuild
{
    internal sealed class DefaultSqlBuildFileHelper : ISqlBuildFileHelper
    {
        public void GetSHA1Hash(string[] batchScripts, out string textHash)
        {
            string scriptText = JoinBatchedScripts(batchScripts);
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = System.Text.Encoding.ASCII.GetBytes(scriptText);
            var hash = sha1.ComputeHash(bytes);
            textHash = System.BitConverter.ToString(hash).Replace("-", "");
        }

        public string JoinBatchedScripts(string[] batchedScripts)
        {
            if (batchedScripts == null || batchedScripts.Length == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < batchedScripts.Length - 1; i++)
            {
                if (batchedScripts[i].EndsWith("\r\n"))
                    sb.Append(batchedScripts[i] + BatchParsing.Delimiter + "\r\n");
                else
                    sb.Append(batchedScripts[i] + "\r\n" + BatchParsing.Delimiter + "\r\n");
            }
            sb.Append(batchedScripts[batchedScripts.Length - 1]);
            return sb.ToString();
        }
    }
}
