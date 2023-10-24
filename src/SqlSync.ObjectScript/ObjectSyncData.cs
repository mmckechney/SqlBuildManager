using System;

namespace SqlSync.ObjectScript {

    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class ObjectSyncData {
        public ObjectSyncData()
        {

        }

        public string ObjectName { get; set; } = string.Empty;
        
        public string ObjectType { get; set; } = string.Empty;
        
        public string FullPath { get; set; } = string.Empty;
        
        public bool IsInDatabase { get; set; } = false;
        
        public bool IsInFileSystem { get; set; } = false;
        
        public string FileName { get; set; } = string.Empty;
        
        public string SchemaOwner { get; set; } = string.Empty;
        
       
    }
}
