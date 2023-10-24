using System;

namespace SqlSync.DbInformation {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class TableSize {
        public TableSize()
        {
        }

        public string TableName { get; set; } = string.Empty;

        public int RowCount { get; set; }  = 0;
       
    }
}
