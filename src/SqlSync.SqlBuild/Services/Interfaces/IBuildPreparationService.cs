using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface IBuildPreparationService
    {
        Task<BuildPreparationResult> PrepareBuildForRunAsync(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, CancellationToken cancellationToken = default);
    }
}
