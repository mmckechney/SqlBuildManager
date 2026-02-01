using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultBuildPreparationService : IBuildPreparationService
    {
        private readonly SqlBuildHelper _helper;
        public DefaultBuildPreparationService(SqlBuildHelper helper) => _helper = helper;

        [System.Obsolete("Use PrepareBuildForRunAsync instead. Will be removed in future version.")]
        public BuildPreparationResult PrepareBuildForRun(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl)
            => _helper.PrepareBuildForRun(model, serverName, isMultiDbRun, scriptBatchColl);

        public Task<BuildPreparationResult> PrepareBuildForRunAsync(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, CancellationToken cancellationToken = default)
            => _helper.PrepareBuildForRunAsync(model, serverName, isMultiDbRun, scriptBatchColl, cancellationToken);

        //TODO: Extract this from SqlBuildHelper when refactoring for DI
    }
}
