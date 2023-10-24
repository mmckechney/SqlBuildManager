namespace SqlSync.SqlBuild.CodeTable
{
    public class ScriptUpdates
    {
        public string Query { get; set; }
        public string ShortFileName { get; set; }
        public string SourceTable { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceServer { get; set; }
        public string KeyCheckColumns { get; set; }
    }
}
