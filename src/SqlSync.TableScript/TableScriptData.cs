using System;

namespace SqlSync {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class TableScriptData {

        public TableScriptData()
        {

        }

        public string TableName { get; set; } = string.Empty;
        
        public System.Data.DataTable ValuesTable { get; set; } = null;
        
        public string InsertScript { get; set; } = string.Empty;
        
        public string SelectStatement { get; set; } = string.Empty;
        
        
        
        
    }
}
