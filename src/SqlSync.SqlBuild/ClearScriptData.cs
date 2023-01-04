namespace SqlSync.SqlBuild
{
    public class ClearScriptData
    {
        public readonly string[] SelectedScriptIds;
        public readonly SqlSyncBuildData BuildData;
        public readonly string ProjectFileName;
        public readonly string BuildZipFileName;
        public ClearScriptData(string[] selectedScriptIds, SqlSyncBuildData buildData, string projectFileName, string buildZipFileName)
        {
            SelectedScriptIds = selectedScriptIds;
            BuildData = buildData;
            ProjectFileName = projectFileName;
            BuildZipFileName = buildZipFileName;
        }
    }
}
