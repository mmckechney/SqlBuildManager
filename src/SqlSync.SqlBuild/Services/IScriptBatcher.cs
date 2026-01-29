using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    public interface IScriptBatcher
    {
        List<string> ReadBatchFromScriptText(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter);
        string[] ReadBatchFromScriptFile(string fileName, bool stripTransaction, bool maintainBatchDelimiter);
        ScriptBatchCollection LoadAndBatchSqlScripts(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model, string projectFilePath);

        Task<List<string>> ReadBatchFromScriptTextAsync(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default);
        Task<string[]> ReadBatchFromScriptFileAsync(string fileName, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default);

        public string RemoveTransactionReferences(string script);
        public string RegexRemoveIfNotInComments(string regexExpression, string script, RegexOptions options);
        public bool IsInComment(string script, int position);
    }
}
