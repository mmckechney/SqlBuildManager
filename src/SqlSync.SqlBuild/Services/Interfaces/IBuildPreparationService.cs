using System.ComponentModel;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface IBuildPreparationService
    {
        BuildPreparationResult PrepareBuildForRun(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl);
    }
}
