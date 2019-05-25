using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSync.SqlBuild.Objects
{
    public class UpdatedObject 
    {
        public string ScriptName
        {
            get;
            set;
        }
        public string ScriptContents
        {
            get;
            set;
        }
        public UpdatedObject(string scriptName, string scriptContents)
        {
            this.ScriptContents = scriptContents;
            this.ScriptName = scriptName;
        }
        
    }
}
