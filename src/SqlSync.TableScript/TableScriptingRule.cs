using System;
namespace SqlSync.TableScript {
    
    [Serializable()]
    [System.ComponentModel.ToolboxItem(true)]
    [System.ComponentModel.DesignerCategory("Component")]
    [System.ComponentModel.DesignTimeVisible(true)]
    public class TableScriptingRule {

        public TableScriptingRule()
        {
        }
        public string TableName { get; set; } = string.Empty;

        public string[] CheckKeyColumns { get; set; } = new string[0];
        
        

        
   
    }
}
