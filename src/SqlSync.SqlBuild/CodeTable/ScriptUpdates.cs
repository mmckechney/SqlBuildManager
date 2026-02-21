namespace SqlSync.SqlBuild.CodeTable
{
    public class ScriptUpdates
    {
        public string Query { get; set; } = string.Empty;
        public string ShortFileName { get; set; } = string.Empty;
        public string SourceTable { get; set; } = string.Empty;
        public string SourceDatabase { get; set; } = string.Empty;
        public string SourceServer { get; set; } = string.Empty;
        public string KeyCheckColumns { get; set; } = string.Empty;
    }
}
