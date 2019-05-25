using System;
using System.Collections.Generic;
using System.Text;

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
            this.SelectedScriptIds = selectedScriptIds;
            this.BuildData = buildData;
            this.ProjectFileName = projectFileName;
            this.BuildZipFileName = buildZipFileName;
        }
    }
}
