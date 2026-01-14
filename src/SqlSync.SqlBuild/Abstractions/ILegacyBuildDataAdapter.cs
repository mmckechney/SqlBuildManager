namespace SqlSync.SqlBuild
{
    public interface ILegacyBuildDataAdapter
    {
        SqlSyncBuildData ToDataSet(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model);
    }
}
