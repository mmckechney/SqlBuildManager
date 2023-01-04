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
            ScriptContents = scriptContents;
            ScriptName = scriptName;
        }

    }
}
