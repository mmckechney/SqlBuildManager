using System.ComponentModel;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal interface IBuildPreparationService
    {
        SqlBuildHelper.BuildPreparationResult PrepareBuildForRun(SqlSyncBuildDataModel model, string serverName, bool isMultiDbRun, ScriptBatchCollection scriptBatchColl, ref DoWorkEventArgs workEventArgs);
    }
}
