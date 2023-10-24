using System;

namespace SqlSync.Compare {

   
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class FileDiff {
        public FileDiff()
        {
        }

        public string FileName { get; set; } = String.Empty;

        public string UnifiedDiff { get; set; } = String.Empty;
    }
}
