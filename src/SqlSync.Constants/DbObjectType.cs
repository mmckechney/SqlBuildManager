namespace SqlSync.Constants
{
    public class DbObjectType
    {
        public const string Table = ".TAB";
        public const string View = ".VIW";
        public const string ForeignKey = ".FKY";
        public const string KeysAndIndexes = ".KCI";
        public const string StoredProcedure = ".PRC";
        public const string UserDefinedFunction = ".UDF";
        public const string Trigger = ".TRG";
        public const string ServerLogin = ".LGN";
        public const string DatabaseUser = ".USR";
        public const string PopulateScript = ".POP";
        public const string DatabaseRole = ".ROLE";
        public const string DatabaseSchema = ".SCHEMA";
        public static string[] GetObjectTypes()
        {
            return new string[]{ DbObjectType.Table,DbObjectType.KeysAndIndexes,DbObjectType.StoredProcedure,
                                   DbObjectType.StoredProcedure,DbObjectType.Trigger,DbObjectType.UserDefinedFunction,
                                   DbObjectType.View, DbObjectType.DatabaseRole,DbObjectType.DatabaseSchema};
        }

        public static string[] GetComparableObjectTypes()
        {
            return new string[] { DbObjectType.StoredProcedure, DbObjectType.UserDefinedFunction, DbObjectType.View, DbObjectType.Table, DbObjectType.Trigger, DbObjectType.DatabaseRole };
        }
    }
}
