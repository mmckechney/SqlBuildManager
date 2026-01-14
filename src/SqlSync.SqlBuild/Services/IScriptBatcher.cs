using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.Services
{
    internal interface IScriptBatcher
    {
        List<string> ReadBatchFromScriptText(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter);
        string[] ReadBatchFromScriptFile(string fileName, bool stripTransaction, bool maintainBatchDelimiter);
        ScriptBatchCollection LoadAndBatchSqlScripts(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model, string projectFilePath);

        Task<List<string>> ReadBatchFromScriptTextAsync(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default);
        Task<string[]> ReadBatchFromScriptFileAsync(string fileName, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default);
    }
}
