using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultScriptBatcher : IScriptBatcher
    {
        public List<string> ReadBatchFromScriptText(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter)
            => SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

        public string[] ReadBatchFromScriptFile(string fileName, bool stripTransaction, bool maintainBatchDelimiter)
            => SqlBuildHelper.ReadBatchFromScriptFile(fileName, stripTransaction, maintainBatchDelimiter);

        public ScriptBatchCollection LoadAndBatchSqlScripts(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model, string projectFilePath)
            => SqlBuildHelper.LoadAndBatchSqlScripts(model, projectFilePath);

        public Task<List<string>> ReadBatchFromScriptTextAsync(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default)
            => Task.FromResult(SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter));

        public async Task<string[]> ReadBatchFromScriptFileAsync(string fileName, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default)
        {
            var contents = await File.ReadAllTextAsync(fileName, cancellationToken).ConfigureAwait(false);
            var batches = SqlBuildHelper.ReadBatchFromScriptText(contents, stripTransaction, maintainBatchDelimiter);
            return batches.ToArray();
        }
    }
}
