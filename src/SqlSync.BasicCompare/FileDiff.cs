using System;
using System.ComponentModel;

namespace SqlSync.BasicCompare
{
    

    [Serializable, DesignerCategory("Component"), DesignTimeVisible(true), ToolboxItem(true)]
    public class FileDiff
    {
        public FileDiff()
        {
        }
        public string FileName { get; set; } = string.Empty;
        public string UnifiedDiff { get; set; } = string.Empty;

   

    }
}
