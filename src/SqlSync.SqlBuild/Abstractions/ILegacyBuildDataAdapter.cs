namespace SqlSync.SqlBuild.Abstractions
{
    public interface ILegacyBuildDataAdapter
    {
        SqlSync.SqlBuild.Legacy.SqlSyncBuildData ToDataSet(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model);
    }
}
