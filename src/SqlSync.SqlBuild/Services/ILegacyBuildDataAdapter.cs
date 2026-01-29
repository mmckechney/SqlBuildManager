namespace SqlSync.SqlBuild.Services
{
    public interface ILegacyBuildDataAdapter
    {
        Legacy.SqlSyncBuildData ToDataSet(Models.SqlSyncBuildDataModel model);
    }
}
