using System.ComponentModel;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface ISqlBuildOrchestrator
    {
        Build Execute(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            BackgroundWorker bgWorker,
            DoWorkEventArgs workEventArgs,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries);
    }
}
