#nullable enable

namespace SqlSync.SqlBuild.Models
{
    public class ClearScriptData
    {
        public readonly string[] SelectedScriptIds;
        public readonly SqlSyncBuildDataModel? BuildDataModel;
        public readonly string ProjectFileName;
        public readonly string BuildZipFileName;

        public ClearScriptData(string[] selectedScriptIds, SqlSyncBuildDataModel buildDataModel, string projectFileName, string buildZipFileName)
        {
            SelectedScriptIds = selectedScriptIds;
            BuildDataModel = buildDataModel;
            ProjectFileName = projectFileName;
            BuildZipFileName = buildZipFileName;
        }
    }
}
