using System;
using SqlSync.SqlBuild.Legacy;
#nullable enable

namespace SqlSync.SqlBuild.Models
{
    public class ClearScriptData
    {
        public readonly string[] SelectedScriptIds;
        [Obsolete("Use BuildDataModel for POCO-based execution")] 
        public readonly SqlSyncBuildData? BuildData;
        public readonly SqlSyncBuildDataModel? BuildDataModel;
        public readonly string ProjectFileName;
        public readonly string BuildZipFileName;

        [Obsolete("Use ClearScriptData(string[], SqlSyncBuildDataModel, string, string) for POCO")]
        public ClearScriptData(string[] selectedScriptIds, SqlSyncBuildData buildData, string projectFileName, string buildZipFileName)
        {
            SelectedScriptIds = selectedScriptIds;
            BuildDataModel = buildData.ToModel();
            ProjectFileName = projectFileName;
            BuildZipFileName = buildZipFileName;
        }

        public ClearScriptData(string[] selectedScriptIds, SqlSyncBuildDataModel buildDataModel, string projectFileName, string buildZipFileName)
        {
            SelectedScriptIds = selectedScriptIds;
            BuildDataModel = buildDataModel;
            ProjectFileName = projectFileName;
            BuildZipFileName = buildZipFileName;
        }
    }
}
