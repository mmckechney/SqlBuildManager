namespace SqlSync.SqlBuild
{
    internal sealed class DefaultLegacyBuildDataAdapter : ILegacyBuildDataAdapter
    {
        public SqlSyncBuildData ToDataSet(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model)
        {
            return model?.ToDataSet() ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel().ToDataSet();
        }
    }
}
