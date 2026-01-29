using System.Text;

namespace SqlSync.SqlBuild.Abstractions.Default
{
    public sealed class DefaultSqlBuildFileHelper : ISqlBuildFileHelper
    {
        public string GetSHA1Hash(string[] batchScripts)
        {
            string scriptText = JoinBatchedScripts(batchScripts);
           return GetSHA1Hash(scriptText);
        }

        public string GetSHA1Hash(string textContents)
        {
            var oSHA1Hasher = System.Security.Cryptography.SHA1.Create();

            byte[] textBytes = new ASCIIEncoding().GetBytes(textContents);
            byte[] arrbytHashValue = oSHA1Hasher.ComputeHash(textBytes);
            string textHash = System.BitConverter.ToString(arrbytHashValue);
            textHash = textHash.Replace("-", "");
            return textHash;
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
