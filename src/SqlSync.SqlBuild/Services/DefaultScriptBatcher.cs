using System.Collections.Generic;

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
    }
}
