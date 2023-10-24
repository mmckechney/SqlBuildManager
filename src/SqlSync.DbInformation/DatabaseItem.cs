namespace SqlSync.DbInformation {
    using System;
    
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class DatabaseItem {
        
        public DatabaseItem()
        {
        }

        public int? SequenceId { get; set; } = null;

        public string DatabaseName { get; set; } = string.Empty;

        public bool IsManuallyEntered { get; set; } = false;

        public override string ToString()
        {
            return DatabaseName;
        }



    }
}
