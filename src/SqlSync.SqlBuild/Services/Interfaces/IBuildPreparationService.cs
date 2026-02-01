using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface IBuildPreparationService
    {
        [System.Obsolete("Use PrepareBuildForRunAsync instead. Will be removed in future version.")]
        BuildPreparationResult PrepareBuildForRun(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl);
        
        Task<BuildPreparationResult> PrepareBuildForRunAsync(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, CancellationToken cancellationToken = default);
    }
}
