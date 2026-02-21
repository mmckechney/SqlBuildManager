using System;
using System.Data.Common;

namespace SqlSync.SqlBuild.Models {

    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class BuildConnectData
    {
        public BuildConnectData() { }

        public DbConnection Connection { get; set; } = null;

        public DbTransaction Transaction { get; set; } = null;

        public string DatabaseName { get; set; } = string.Empty;

        public string ServerName { get; set; } = string.Empty;

        public bool HasLoggingTable { get; set; } = false;

    }
}
