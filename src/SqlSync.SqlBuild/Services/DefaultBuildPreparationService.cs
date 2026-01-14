using System.ComponentModel;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultBuildPreparationService : IBuildPreparationService
    {
        private readonly SqlBuildHelper _helper;
        public DefaultBuildPreparationService(SqlBuildHelper helper) => _helper = helper;

        public SqlBuildHelper.BuildPreparationResult PrepareBuildForRun(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, ref DoWorkEventArgs workEventArgs)
            => _helper.PrepareBuildForRun(model, serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

        //TODO: Extract this from SqlBuildHelper when refactoring for DI
    }
}
