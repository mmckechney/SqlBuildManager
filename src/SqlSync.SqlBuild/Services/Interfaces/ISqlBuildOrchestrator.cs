using System.Threading;
using System.Threading.Tasks;
using SqlBuildManager.SqlBuild.Models;

namespace SqlBuildManager.SqlBuild.Services
{
    internal interface ISqlBuildOrchestrator
    {
        Task<Build> ExecuteAsync(
            SqlBuildRunDataModel runData,
            BuildPreparationResult prep,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries,
            CancellationToken cancellationToken = default);
    }
}
