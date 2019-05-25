using System;
using System.IO;
namespace SqlSync.Highlighting
{
	/// <summary>
	/// Summary description for KeyWords.
	/// </summary>
	public class Keywords
	{
		private static string[] sqlReserved = null;
		public static string[] SqlReserved
		{
			get
			{
				if(Keywords.sqlReserved == null)
					SetSqlKeyWords();

				return Keywords.sqlReserved;
			}
		}
		private static void SetSqlKeyWords()
		{
			sqlReserved = new string[]{   "SELECT",
										  "CONSTRAINT",
										  "WHERE",
										  "INDEX",
										  "UPDATE",
										  "AND",
										  "INSERT",
										  "INNER",
										  "IF",
										  "WHEN",
										  "ELSE",
										  "OR",
										  "END",
										  "CHAR",
										  "VARCHAR",
										  "NOCOUNT",
                                          "(NOLOCK)",
										  "CURSOR",
										  "ORDER",
										  "QUOTED_IDENTIFIER",
										  "ANSI_NULLS",
										  "ADD",
										  "EXCEPT",
										  "PERCENT",
										  "ALL",
                                          "NO",
                                          "LOCK",
										  "EXEC",
										  "PLAN",
										  "ALTER",
										  "THEN",
										  "BY",
										  "EXISTS",
										  "PRIMARY",
										  "ANY",
										  "EXECUTE",
										  "PRECISION",
										  "EXIT",
										  "PRINT",
										  "AS",
										  "FETCH",
										  "PROC",
										  "ASC",
										  "FILE",
										  "PROCEDURE",
										  "AUTHORIZATION",
										  "FILLFACTOR",
										  "PUBLIC",
										  "BACKUP",
										  "FOR",
										  "RAISERROR",
										  "BEGIN",
										  "FOREIGN",
										  "READ",
										  "BETWEEN",
										  "FREETEXT",
										  "READTEXT",
										  "BREAK",
										  "FREETEXTTABLE",
										  "RECONFIGURE",
										  "BROWSE",
										  "FROM",
										  "REFERENCES",
										  "BULK",
										  "FULL",
										  "REPLICATION",
										  "FUNCTION",
										  "RESTORE",
										  "CASCADE",
										  "GOTO",
										  "RESTRICT",
										  "CASE",
										  "GRANT",
										  "RETURN",
										  "CHECK",
										  "GROUP",
										  "REVOKE",
										  "CHECKPOINT",
										  "HAVING",
										  "RIGHT",
										  "CLOSE",
										  "HOLDLOCK",
										  "ROLLBACK",
										  "CLUSTERED",
										  "IDENTITY",
										  "ROWCOUNT",
										  "COALESCE",
										  "IDENTITY_INSERT",
										  "ROWGUIDCOL",
										  "COLLATE",
										  "IDENTITYCOL",
										  "RULE",
										  "COLUMN",
										  "SAVE",
										  "COMMIT",
										  "IN",
										  "SCHEMA",
										  "COMPUTE",
										  "SESSION_USER",
										  "CONTAINS",
										  "SET",
										  "CONTAINSTABLE",
										  "INTERSECT",
										  "SETUSER",
										  "CONTINUE",
										  "INTO",
										  "SHUTDOWN",
										  "CONVERT",
										  "IS",
										  "SOME",
										  "CREATE",
                                          "'CREATE",
										  "JOIN",
										  "STATISTICS",
										  "CROSS",
										  "KEY",
										  "SYSTEM_USER",
										  "CURRENT",
										  "KILL",
										  "TABLE",
										  "CURRENT_DATE",
										  "LEFT",
										  "TEXTSIZE",
										  "CURRENT_TIME",
										  "LIKE",
										  "CURRENT_TIMESTAMP",
										  "LINENO",
										  "TO",
										  "CURRENT_USER",
										  "LOAD",
										  "TOP",
										  "NATIONAL",
										  "DATABASE",
										  "NOCHECK",
										  "TRANSACTION",
										  "DBCC",
										  "NONCLUSTERED",
										  "TRIGGER",
										  "DEALLOCATE",
										  "NOT",
										  "TRUNCATE",
										  "DECLARE",
										  "NULL",
										  "TSEQUAL",
										  "DEFAULT",
										  "NULLIF",
										  "UNION",
										  "DELETE",
										  "OF",
										  "UNIQUE",
										  "DENY",
										  "OFF",
										  "DESC",
										  "OFFSETS",
										  "UPDATETEXT",
										  "DISK",
										  "ON",
										  "USE",
										  "DISTINCT",
										  "OPEN",
										  "USER",
										  "DISTRIBUTED",
										  "OPENDATASOURCE",
										  "VALUES",
										  "DOUBLE",
										  "OPENQUERY",
										  "VARYING",
										  "DROP",
										  "OPENROWSET",
										  "VIEW",
										  "DUMMY",
										  "OPENXML",
										  "WAITFOR",
										  "DUMP",
										  "OPTION",
										  "WHILE",
										  "ERRLVL",
										  "OUTER",
										  "WITH",
										  "ESCAPE",
										  "OVER",
										  "WRITETEXT",
                                          "RECOMPILE"
										  };
		}



		private static string[] sqlFunctions = null;
		public static string[] SqlFunctions
		{
			get
			{
				if(Keywords.sqlFunctions == null)
					SetSqlFunctions();
				return sqlFunctions;
			}
		}
		private static void SetSqlFunctions()
		{
			sqlFunctions = new string[]{"CASE",
										   "CAST",
										   "CURRENT_USER",
										   "ISNULL",
										   "NEWID",
										   "LEN",
										   "@@DATEFIRST",
										   "@@OPTIONS",
										   "@@DBTS",
										   "@@REMSERVER",
										   "@@LANGID",
										   "@@SERVERNAME",
										   "@@LANGUAGE",
										   "@@SERVICENAME",
										   "@@LOCK_TIMEOUT",
										   "@@SPID",
										   "@@MAX_CONNECTIONS",
										   "@@TEXTSIZE",
										   "@@MAX_PRECISION",
										   "@@VERSION",
										   "@@NESTLEVEL",
										   "@@CONNECTIONS",
										   "@@PACK_RECEIVED",
										   "@@CPU_BUSY",
										   "@@PACK_SENT",
										   "fn_virtualfilestats",
										   "@@TIMETICKS",
										   "@@IDLE",
										   "@@TOTAL_ERRORS",
										   "@@IO_BUSY",
										   "@@TOTAL_READ",
										   "@@PACKET_ERRORS",
										   "@@TOTAL_WRITE",
										   "APP_NAME",
										   "COALESCE",
										   "COLLATIONPROPERTY",
										   "CURRENT_TIMESTAMP",
										   "DATALENGTH",
										   "@@ERROR",
										   "fn_helpcollations",
										   "fn_servershareddrives",
										   "fn_virtualfilestats",
										   "FORMATMESSAGE",
										   "GETANSINULL",
										   "HOST_ID",
										   "HOST_NAME",
										   "IDENT_CURRENT",
										   "IDENT_INCR",
										   "IDENT_SEED",
										   "@@IDENTITY",
										   "IDENTITY",
										   "ISDATE",
										   "ISNUMERIC",
										   "NULLIF",
										   "PARSENAME",
										   "PERMISSIONS",
										   "@@ROWCOUNT",
										   "ROWCOUNT_BIG",
										   "SCOPE_IDENTITY",
										   "SERVERPROPERTY",
										   "SESSIONPROPERTY",
										   "SESSION_USER",
										   "STATS_DATE",
										   "SYSTEM_USER",
										   "@@TRANCOUNT",
										   "USER_NAME",
										   "AVG",
										   "MAX",
										   "BINARY_CHECKSUM",
										   "MIN",
										   "CHECKSUM",
										   "SUM",
										   "CHECKSUM_AGG",
										   "STDEV",
										   "COUNT",
										   "STDEVP",
										   "COUNT_BIG",
										   "VAR",
										   "GROUPING",
										   "VARP",
										   "ASCII",
										   "NCHAR",
										   "SOUNDEX",
										   "PATINDEX",
										   "SPACE",
										   "CHARINDEX",
										   "REPLACE",
										   "STR",
										   "DIFFERENCE",
										   "QUOTENAME",
										   "STUFF",
										   "LEFT",
										   "REPLICATE",
										   "SUBSTRING",
										   "REVERSE",
										   "UNICODE",
										   "LOWER",
										   "RIGHT",
										   "UPPER",
										   "LTRIM",
										   "RTRIM",
										   "DATEADD",
										   "DATEDIFF",
										   "DATENAME",
										   "DATEPART",
										   "DAY",
										   "GETDATE",
										   "GETUTCDATE",
										   "MONTH",
										   "YEAR",
										   "ABS",
										   "DEGREES",
										   "RAND",
										   "ACOS",
										   "EXP",
										   "ROUND",
										   "ASIN",
										   "FLOOR",
										   "SIGN",
										   "ATAN",
										   "LOG",
										   "SIN",
										   "ATN2",
										   "LOG10",
										   "SQUARE",
										   "CEILING",
										   "PI",
										   "SQRT",
										   "COS",
										   "POWER",
										   "TAN",
										   "COT",
										   "RADIANS",
                                            "OBJECT_ID",};
		}


        
	}

	
}