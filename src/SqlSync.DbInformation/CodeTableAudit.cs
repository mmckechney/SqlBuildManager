using System;

namespace SqlSync.DbInformation {
   
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class CodeTableAudit {
        public CodeTableAudit()
        {

        }

        public string TableName { get; set; } = string.Empty;
        
        public string UpdateIdColumn { get; set; } = string.Empty;
        
        public string UpdateDateColumn { get; set; } = string.Empty;
        
        public string CreateDateColumn { get; set; } = string.Empty;
        
        public string CreateIdColumn { get; set; } = string.Empty;
        
        public int RowCount { get; set; } = -1;
        
        public bool HasUpdateTrigger { get; set; } = false;
        
        public object LookUpTableRow { get; set; } = null;
        
        
       
        

  
    }
}
