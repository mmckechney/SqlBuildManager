using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.SqlBuild
{
    public class ScriptBatchCollection : List<ScriptBatch>
    {
        public ScriptBatch GetScriptBatch(string scriptId)
        {
            foreach (ScriptBatch b in this)
            {
                if (b.ScriptId == scriptId)
                    return b;
            }
            return null;
        }
    }

    public class ScriptBatch
    {
        private string scriptfileName = string.Empty;

        public string ScriptfileName
        {
            get { return scriptfileName; }
            set { scriptfileName = value; }
        }
        private string[] scriptBatchContents = new string[0];

        public string[] ScriptBatchContents
        {
            get { return scriptBatchContents; }
            set { scriptBatchContents = value; }
        }
        private string scriptId = string.Empty;

        public string ScriptId
        {
            get { return scriptId; }
            set { scriptId = value; }
        }
        public ScriptBatch(string scriptfileName, string[] scriptBatchContents, string scriptId)
        {
            this.scriptfileName = scriptfileName;
            this.scriptBatchContents = scriptBatchContents;
            this.scriptId = scriptId;
        }
    }
}
