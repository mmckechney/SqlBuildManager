using System;

namespace SqlSync.Constants
{
	public class DbScriptDescription
	{
		public const string Table = "Table";
		public const string View = "View";
		public const string ForeignKey = "Table (Foreign Keys)";
		public const string KeysAndIndexes = "Table (Keys, Constraints, Default and Indexes)";
		public const string StoredProcedure = "Stored Procedure";
		public const string UserDefinedFunction = "User Defined Function";
		public const string Trigger = "Trigger";
		public const string ServerLogin = "Server Login";
		public const string DatabaseUser = "Database User";
        public const string DatabaseRole = "Database Role";
        public const string DatabaseSchema = "Database Schema";
	}
}
