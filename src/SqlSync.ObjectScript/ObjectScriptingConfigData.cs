namespace SqlSync.ObjectScript
{
    public class ObjectScriptingConfigData
    {
        public readonly bool WithDelete;
        public readonly bool CombineTableObjects;
        public readonly bool ZipScripts;
        public readonly bool IncludeFileHeader;
        public readonly bool ReportStatusPerObject;
        public readonly Connection.ConnectionData ConnData;
        public ObjectScriptingConfigData(bool withDelete, bool combineTableObjects, bool zipScripts, bool includeFileHeader, bool reportStatusPerObject, Connection.ConnectionData connData)
        {
            WithDelete = withDelete;
            CombineTableObjects = combineTableObjects;
            ZipScripts = zipScripts;
            IncludeFileHeader = includeFileHeader;
            ReportStatusPerObject = reportStatusPerObject;
            ConnData = connData;
        }
    }
}
