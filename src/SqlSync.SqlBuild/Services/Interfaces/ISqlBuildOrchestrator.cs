using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface ISqlBuildOrchestrator
    {
        Build Execute(
            SqlBuildRunDataModel runData,
            BuildPreparationResult prep,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries);

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
