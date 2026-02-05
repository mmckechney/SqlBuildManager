using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
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
