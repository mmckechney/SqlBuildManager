namespace SqlSync.Compare
{
    public class FileCompareResults
    {

        SqlBuild.SqlSyncBuildData.ScriptRow leftScriptRow = null;
        public SqlBuild.SqlSyncBuildData.ScriptRow LeftScriptRow
        {
            get { return leftScriptRow; }
            set { leftScriptRow = value; }
        }

        SqlBuild.SqlSyncBuildData.ScriptRow rightScriptRow = null;
        public SqlBuild.SqlSyncBuildData.ScriptRow RightScriptRow
        {
            get { return rightScriptRow; }
            set { rightScriptRow = value; }
        }
        string leftScriptText = string.Empty;
        public string LeftScriptText
        {
            get { return leftScriptText; }
            set { leftScriptText = value; }
        }

        string rightSciptText = string.Empty;
        public string RightSciptText
        {
            get { return rightSciptText; }
            set { rightSciptText = value; }
        }

        string leftScriptPath = string.Empty;
        public string LeftScriptPath
        {
            get { return leftScriptPath; }
            set { leftScriptPath = value; }
        }

        string rightScriptPath = string.Empty;
        public string RightScriptPath
        {
            get { return rightScriptPath; }
            set { rightScriptPath = value; }
        }

        string unifiedDiffText = string.Empty;
        public string UnifiedDiffText
        {
            get { return unifiedDiffText; }
            set { unifiedDiffText = value; }
        }

        string leftScriptTag = string.Empty;




    }
}
