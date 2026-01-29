using SqlSync.SqlBuild.Legacy;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultLegacyBuildDataAdapter : ILegacyBuildDataAdapter
    {
        public SqlSyncBuildData ToDataSet(SqlSyncBuildDataModel model)
        {
            return model?.ToDataSet() ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel().ToDataSet();
        }
    }
}
