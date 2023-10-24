using System;

namespace SqlSync.TableScript {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class UpdateAutoDetectData {
        public UpdateAutoDetectData()
        {
        }
        public string TableName { get; set; } = string.Empty;
        
        public int RowCount { get; set; } = 0;
                
        public bool HasUpdateTrigger { get; set; } = false;

    }
}
