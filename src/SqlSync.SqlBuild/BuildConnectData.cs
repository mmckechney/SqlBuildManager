using System;

namespace SqlSync.SqlBuild {

    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class BuildConnectData
    {
        public BuildConnectData() { }

        public Microsoft.Data.SqlClient.SqlConnection Connection { get; set; } = null;

        public Microsoft.Data.SqlClient.SqlTransaction Transaction { get; set; } = null;

        public string DatabaseName { get; set; } = string.Empty;

        public string ServerName { get; set; } = string.Empty;

        public bool HasLoggingTable { get; set; } = false;

    }
}
