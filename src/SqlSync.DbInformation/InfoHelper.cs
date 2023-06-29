using Microsoft.Azure.Amqp.Framing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.DbInformation.ChangeDates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
namespace SqlSync.DbInformation
{
    /// <summary>
    /// Summary description for InfoHelper.
    /// </summary>
    public class InfoHelper
    {

        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public InfoHelper()
        {
        }
        //private static DatabaseRoutineChangeDates databaseRoutineChangeDates;

        //public static DatabaseRoutineChangeDates DatabaseRoutineChangeDates
        //{
        //    get { return InfoHelper.databaseRoutineChangeDates; }
        //    set { InfoHelper.databaseRoutineChangeDates = value; }
        //}
        public static CodeTableAuditColumnList codeTableAuditCols = new CodeTableAuditColumnList();

        #region .: Column Data :.
        /// <summary>
        /// Gets a list of columns for the supplied table
        /// </summary>
        /// <param name="tableName">Name of the table or view to get columns of</param>
        /// <returns>String array of column names in ordinal order</returns>
        public static string[] GetColumnNames(string tableName, ConnectionData connData)
        {
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schemaOwner);
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand("select column_name from INFORMATION_SCHEMA.COLUMNS where table_name= @TableName  AND table_schema = @Schema ORDER BY ordinal_position", conn);
            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@Schema", schemaOwner);
            ArrayList list = new ArrayList();
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                    list.Add(reader[0].ToString());
                reader.Close();
            }
            string[] columnList = new string[list.Count];
            list.CopyTo(columnList);
            return columnList;
        }
        /// <summary>
        /// Gets a list of columns for the supplied table
        /// </summary>
        /// <param name="tableName">Name of the table or view to get columns of</param>
        /// <returns>String array of column names in ordinal order</returns>
        public static ColumnInfo[] GetColumnNamesWithTypes(string tableName, ConnectionData connData)
        {
            string schemaOwner;
            ExtractNameAndSchema(tableName, out tableName, out schemaOwner);

            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand(@"select column_name,data_type,ISNULL(character_maximum_length,0) character_maximum_length from 
                            INFORMATION_SCHEMA.COLUMNS where table_name= @TableName  and table_schema = @Schema ORDER BY ordinal_position", conn);
            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@Schema", schemaOwner);
            ArrayList list = new ArrayList();
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.IsClosed == false)
                {
                    ColumnInfo inf = new ColumnInfo();
                    if (inf.Fill(reader, false))
                        list.Add(inf);
                }
                reader.Close();
            }
            ColumnInfo[] infList = new ColumnInfo[list.Count];
            list.CopyTo(infList);

            return infList;
        }

        /// <summary>
        /// Retrieves the Primary Key columns for a table
        /// </summary>
        /// <param name="tableName">Name of the table to interrogate</param>
        /// <param name="connData">Connection data to connect to the Db</param>
        /// <returns>String Array of PK columns</returns>
        public static string[] GetPrimaryKeyColumns(string tableName, ConnectionData connData)
        {
            string schemaOwner;
            ExtractNameAndSchema(tableName, out tableName, out schemaOwner);
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            string command = @"select column_Name from information_schema.TABLE_CONSTRAINTS TC 
                                    INNER JOIN information_schema.CONSTRAINT_COLUMN_USAGE cc ON cc.constraint_name = tc.constraint_Name
						            WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' and tc.TABLE_NAME = @TableName and tc.TABLE_SCHEMA = @Schema";
            SqlCommand cmd = new SqlCommand(command, conn);
            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@Schema", schemaOwner);
            ArrayList list = new ArrayList();
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                    list.Add(reader[0].ToString());
                reader.Close();
            }
            string[] pkList = new string[list.Count];
            list.CopyTo(pkList);
            return pkList;
        }

        public static string[] GetColumnNames(DataTable table)
        {
            ArrayList lst = new ArrayList();
            foreach (DataColumn col in table.Columns)
            {
                lst.Add(col.ColumnName);
            }
            string[] names = new string[lst.Count];
            lst.CopyTo(names);
            return names;
        }


        #endregion

        #region .: Table Data :.
        public static string[] GetDatabaseTableList(ConnectionData connData, string filter)
        {
            try
            {
                SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
                SqlCommand cmd;
                if (filter == string.Empty)
                    cmd = new SqlCommand("select Table_schema + '.'+ Table_Name from INFORMATION_SCHEMA.TABLES where table_catalog = @DatabaseName and  Table_Type = 'BASE TABLE' ORDER BY Table_schema, Table_Name", conn);
                else
                    cmd = new SqlCommand("select Table_schema + '.'+ Table_Name from INFORMATION_SCHEMA.TABLES where table_catalog = @DatabaseName and  Table_Type = 'BASE TABLE' AND Table_Name LIKE '" + filter + "' ORDER BY Table_schema, Table_Name", conn);

                cmd.Parameters.AddWithValue("@DatabaseName", connData.DatabaseName);
                ArrayList list = new ArrayList();
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        list.Add(reader[0].ToString());
                    reader.Close();
                }
                string[] tableList = new string[list.Count];
                list.CopyTo(tableList);
                return tableList;
            }
            catch
            {
                return new string[0];
            }
        }
        /// <summary>
        /// Retrieves a list of non-system tables for the database
        /// </summary>
        /// <param name="connData">Connection data object</param>
        /// <returns>String array of table names</returns>
        public static string[] GetDatabaseTableList(ConnectionData connData)
        {
            return GetDatabaseTableList(connData, "");

        }
        /// <summary>
        /// Gets list of tables in a database with the number of rows it contains
        /// </summary>
        /// <param name="connData">Connection data object</param>
        /// <returns>Array of TableSize objects for the database</returns>
        public static TableSize[] GetDatabaseTableListWithRowCount(ConnectionData connData)
        {
            return GetDatabaseTableListWithRowCount(connData, "");
        }

        public static TableSize[] GetDatabaseTableListWithRowCount(ConnectionData connData, string filter)
        {

            string tableName;
            string schemaOwner;
            string[] tables = GetDatabaseTableList(connData, filter);
            TableSize[] sizes = new TableSize[tables.Length];
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                conn.Open();

                for (int i = 0; i < tables.Length; i++)
                {
                    ExtractNameAndSchema(tables[i], out tableName, out schemaOwner);
                    try
                    {
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        cmd.CommandText = "sp_spaceused [" + schemaOwner + "." + tableName + "]";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            sizes[i] = new TableSize();
                            sizes[i].TableName = schemaOwner + "." + reader.GetString(0);
                            sizes[i].RowCount = Int32.Parse(reader[1].ToString());
                        }
                    }
                    catch
                    {
                        sizes[i] = new TableSize();
                        sizes[i].TableName = schemaOwner + "." + tableName;
                        sizes[i].RowCount = -1;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                conn.Close();
            }
            return sizes;
        }
        public static System.Collections.Generic.SortedDictionary<string, CodeTableAudit> GetTablesWithAuditColumns(ConnectionData connData)
        {
            System.Collections.Generic.SortedDictionary<string, CodeTableAudit> tableDefs = new SortedDictionary<string, CodeTableAudit>();

            //if(UpdateDateFields.Length == 0 || UpdateIdFields.Length == 0)
            if (!codeTableAuditCols.IsValid)
                SetUpdateColumnNames();

            string updateId = GetDelimitedListForSql(codeTableAuditCols.UpdateIdColumns);
            string updateDate = GetDelimitedListForSql(codeTableAuditCols.UpdateDateColumns);
            string createId = GetDelimitedListForSql(codeTableAuditCols.CreateIdColumns);
            string createDate = GetDelimitedListForSql(codeTableAuditCols.CreateDateColumns);
            string sql = "select TABLE_NAME, COLUMN_NAME, TABLE_SCHEMA  from INFORMATION_SCHEMA.COLUMNS where column_name IN ({0})  ORDER BY table_name";
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            conn.Open();

            //Tables with Update ID columns
            SqlCommand cmd = new SqlCommand(String.Format(sql, updateId), conn);
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                {
                    CodeTableAudit tbl = new CodeTableAudit();
                    tbl.TableName = reader[2].ToString() + "." + reader[0].ToString();
                    tbl.UpdateIdColumn = reader[1].ToString();
                    tableDefs.Add(tbl.TableName, tbl);
                }
                reader.Close();
            }

            //Tables with Update date columns
            cmd = new SqlCommand(String.Format(sql, updateDate), conn);
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                {
                    string tableName = reader[2].ToString() + "." + reader[0].ToString();
                    if (!tableDefs.ContainsKey(tableName))
                    {
                        CodeTableAudit tbl = new CodeTableAudit();
                        tbl.TableName = tableName;
                        tbl.UpdateDateColumn = reader[1].ToString();
                        tableDefs.Add(tbl.TableName, tbl);
                    }
                    else
                    {
                        CodeTableAudit tbl;
                        if (tableDefs.TryGetValue(tableName, out tbl))
                            tbl.UpdateDateColumn = reader[1].ToString();
                    }
                }
                reader.Close();
            }

            //Tables with Create id columns
            cmd = new SqlCommand(String.Format(sql, createId), conn);
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                {
                    string tableName = reader[2].ToString() + "." + reader[0].ToString();

                    if (!tableDefs.ContainsKey(tableName))
                    {
                        CodeTableAudit tbl = new CodeTableAudit();
                        tbl.TableName = tableName;
                        tbl.CreateIdColumn = reader[1].ToString();
                        tableDefs.Add(tbl.TableName, tbl);
                    }
                    else
                    {
                        CodeTableAudit tbl;
                        if (tableDefs.TryGetValue(tableName, out tbl))
                            tbl.CreateIdColumn = reader[1].ToString();
                    }
                }
                reader.Close();
            }
            //Tables with Create date columns
            cmd = new SqlCommand(String.Format(sql, createDate), conn);
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (reader.Read())
                {
                    string tableName = reader[2].ToString() + "." + reader[0].ToString();
                    if (!tableDefs.ContainsKey(tableName))
                    {
                        CodeTableAudit tbl = new CodeTableAudit();
                        tbl.TableName = tableName;
                        tbl.CreateDateColumn = reader[1].ToString();
                        tableDefs.Add(tbl.TableName, tbl);
                    }
                    else
                    {
                        CodeTableAudit tbl;
                        if (tableDefs.TryGetValue(tableName, out tbl))
                            tbl.CreateDateColumn = reader[1].ToString();
                    }
                }
                reader.Close();
            }

            return tableDefs;
        }

        /// <summary>
        /// Determines whether or not a certain table is in the database
        /// </summary>
        /// <param name="tableName">Name of table to look for</param>
        /// <returns>True if found, false if not</returns>
        public static bool DbContainsTable(string tableName, ConnectionData connData)
        {
            string[] allTables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableList(connData);
            return DbContainsTable(tableName, allTables);
        }
        public static bool DbContainsTable(string tableName, string[] allTables)
        {

            for (int i = 0; i < allTables.Length; i++)
            {
                if (allTables[i].ToLower() == tableName.ToLower())
                    return true;
            }

            return false;
        }

        public static int DbContainsTableWithRowcount(string tableName, TableSize[] tableData)
        {
            for (int i = 0; i < tableData.Length; i++)
            {
                if (tableData[i].TableName.ToLower() == tableName.ToLower())
                    return tableData[i].RowCount;
            }
            return -1;
        }

        public static List<ObjectData> GetTableObjectList(ConnectionData connData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand(@"select distinct table_name as ObjectName, 
												'Table' as ObjectType,
												refdate as AlteredDate, 
												crdate as CreateDate,
                                                table_schema as SchemaOwner
												from INFORMATION_SCHEMA.tables v
												LEFT OUTER JOIN sysobjects s ON v.table_name = s.[name]
												INNER JOIN sys.schemas ss ON s.[uid] = ss.[schema_id]
												WHERE
												v.table_type = 'BASE TABLE'
												and ss.[name] = v.table_schema
												ORDER BY table_name", conn);
            return FillObjectData(cmd);
        }
        #endregion

        #region .: Database Data :.
        public static DatabaseList GetDatabaseList(ConnectionData connData)
        {
            bool hasError;
            return GetDatabaseList(connData, out hasError);
        }
        /// <summary>
        /// Gets a list of databases on the target server
        /// </summary>
        /// <param name="connData"></param>
        /// <returns></returns>
        public static DatabaseList GetDatabaseList(ConnectionData connData, out bool hasError)
        {
            hasError = false;
            string dbName;
            DatabaseList dbList = new DatabaseList();
            //Add any manually entered databases
            StringCollection manualDBs = SqlSync.DbInformation.Properties.Settings.Default.ManuallyEnteredDatabases;
            for (int i = 0; i < manualDBs.Count; i++)
            {
                dbList.Add(manualDBs[i], true);
            }

            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection("master", connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, 5, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand("select distinct [name] from dbo.sysdatabases ORDER BY [name]", conn);

            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        dbName = reader[0].ToString();
                        if (!manualDBs.Contains(dbName))
                            dbList.Add(dbName, false);
                        else
                        {
                            for (int i = 0; i < dbList.Count; i++)
                            {
                                if (dbList[i].DatabaseName == dbName)
                                {
                                    dbList[i].IsManuallyEntered = false;
                                    break;
                                }
                            }
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error getting database list");
                hasError = true;
            }

            dbList.Sort(new DatabaseListComparer());
            return dbList;
        }

        public static SizeAnalysisTable GetDatabaseSizeAnalysis(ConnectionData connData)
        {

            SizeAnalysisTable tbl = new SizeAnalysisTable();
            try
            {
                SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                SqlCommand cmd = new SqlCommand(new ResourceHelper().GetFromResources("SqlSync.DbInformation.SizeAnalysis.sql"));
                cmd.Connection = conn;
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                adapt.Fill(tbl);

            }
            catch (SqlException ex)
            {
                if (ex.ToString().IndexOf("does not exist in database") == -1)
                    throw;
            }
            return tbl;
        }

        public static ServerSizeSummary GetServerDatabaseInfo(ConnectionData connData)
        {
            Regex nums = new Regex(@"\d{1,9}");
            ServerSizeSummary data = new ServerSizeSummary();
            string location;
            Int64 dbSize;
            connData.DatabaseName = "master";
            DatabaseList dbList = GetDatabaseList(connData);
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
            string locationQuery = "select filename from {0}.dbo.sysfiles where filename like '%.mdf'";
            string sizeQuery = "{0}.dbo.sp_spaceused";

            SqlCommand cmdLoc = new SqlCommand();
            cmdLoc.Connection = conn;
            SqlCommand cmdSize = new SqlCommand();
            cmdSize.Connection = conn;

            string dbName = string.Empty;
            for (int i = 0; i < dbList.Count; i++)
            {
                try
                {
                    conn.Open();

                    if (!dbList[i].IsManuallyEntered)
                    {
                        dbName = dbList[i].DatabaseName;
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();


                        cmdLoc.CommandText = String.Format(locationQuery, dbName);
                        cmdSize.CommandText = string.Format(sizeQuery, dbName);
                        location = (string)cmdLoc.ExecuteScalar();
                        using (SqlDataReader reader = cmdSize.ExecuteReader())
                        {
                            reader.Read();
                            dbSize = Int64.Parse(nums.Match(reader[1].ToString()).ToString());
                        }
                        data.AddServerSizeSummaryRow(dbName, location, dbSize, DateTime.MinValue);
                    }

                }
                catch
                {
                    data.AddServerSizeSummaryRow(dbName, string.Empty, 0, DateTime.MinValue);
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                }
            }

            data.AcceptChanges();
            AddDatabaseCreateDate(ref data, connData);
            data.AcceptChanges();
            return data;


        }
        private static void AddDatabaseCreateDate(ref ServerSizeSummary sizeSummary, ConnectionData connData)
        {
            string sql2000Cmd = "SELECT name, crdate FROM sysdatabases";
            string sql2005Cmd = "SELECT name, create_date FROM sys.databases";
            string filter;

            connData.DatabaseName = "master";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
            cmd.CommandType = CommandType.Text;
            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                try
                {
                    cmd.CommandText = sql2005Cmd;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filter = sizeSummary.DatabaseNameColumn.ColumnName + "='" + reader[0].ToString() + "'";
                            if (sizeSummary.Select(filter).Length > 0)
                                ((ServerSizeSummaryRow)sizeSummary.Select(filter)[0]).DateCreated = reader.GetDateTime(1);
                        }
                    }


                }
                catch
                {
                    try
                    {
                        cmd.CommandText = sql2000Cmd;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                filter = sizeSummary.DatabaseNameColumn.ColumnName + "='" + reader[0].ToString() + "'";
                                if (sizeSummary.Select(filter).Length > 0)
                                    ((ServerSizeSummaryRow)sizeSummary.Select(filter)[0]).DateCreated = reader.GetDateTime(1);
                            }
                        }
                    }
                    catch { }
                }
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
            }

        }
        #endregion

        #region .: Stored Procedure Data :.
        public static List<ObjectData> GetStoredProcedureList(ConnectionData connData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand(@"SELECT Routine_Name as ObjectName,
												'Stored Procedure' as ObjectType, 
												Last_Altered as AlteredDate, 
												Created as CreateDate,
                                                routine_schema as SchemaOwner
												FROM INFORMATION_SCHEMA.ROUTINES 
												WHERE Routine_Type = 'PROCEDURE' 
												ORDER BY routine_schema, Routine_Name", conn);
            return FillObjectData(cmd);
        }
        #endregion

        #region .: Function Data :.
        public static List<ObjectData> GetFunctionList(ConnectionData connData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand(@"Select Routine_Name as ObjectName,
												'Function' as ObjectType, 
												Last_Altered as AlteredDate, 
												Created as CreateDate,
                                                routine_schema as SchemaOwner
												from INFORMATION_SCHEMA.ROUTINES 
												WHERE Routine_Type = 'FUNCTION' 
												ORDER BY routine_schema, Routine_Name", conn);
            return FillObjectData(cmd);
        }
        //		private static ObjectData[] FillObjectData(SqlCommand cmd)
        //		{
        //			ArrayList lst = new ArrayList();
        //			if(cmd.Connection.State == ConnectionState.Closed)
        //				cmd.Connection.Open();
        //			using(SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
        //			{
        //				while(reader.Read())
        //				{
        //					ObjectData data = new ObjectData();
        //					data.Fill(reader,false);
        //					lst.Add(data);
        //				}
        //				reader.Close();
        //			}
        //			
        //			ObjectData[] dataArr = new ObjectData[lst.Count];
        //			lst.CopyTo(dataArr);
        //			return dataArr;
        //		}
        #endregion

        #region .: View Data :.
        public static List<ObjectData> GetViewList(ConnectionData connData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
            SqlCommand cmd = new SqlCommand(@"select distinct v.table_name as ObjectName, 
	'View' as ObjectType,
	s.refdate as AlteredDate, 
	s.crdate as CreateDate,
    v.table_schema as SchemaOwner
	from INFORMATION_SCHEMA.views v
	INNER JOIN sys.schemas ss on ss.[name] = v.table_schema
	INNER JOIN sysobjects s ON s.uid = ss.schema_id AND s.[name] = v.table_name
	ORDER BY v.table_schema, v.table_name", conn);
            return FillObjectData(cmd);
        }
        private static List<ObjectData> FillObjectData(SqlCommand cmd)
        {
            List<ObjectData> lst = new List<ObjectData>();
            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (!reader.IsClosed)
                    {
                        ObjectData data = new ObjectData();
                        if (data.Fill(reader, false))
                            lst.Add(data);
                    }
                    reader.Close();
                }
            }
            catch
            {
            }
            return lst;
        }
        #endregion

        #region .: Trigger Data :.
        public static string[] GetTriggers(ConnectionData connData)
        {
            try
            {
                SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                SqlCommand cmd = new SqlCommand(@"
                    SELECT  o.name AS [trigger],  
                            t.name AS [table],  
                            o.type,  
                            s.name AS [schema] 
                    FROM    sys.schemas s RIGHT OUTER JOIN 
                            sys.tables t ON s.schema_id = t.schema_id RIGHT OUTER JOIN 
                            sys.objects o ON t.object_id = o.parent_object_id 
                    WHERE   o.type = 'tr'", conn);
                List<string> list = new List<string>();
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        list.Add(reader[0].ToString());
                    reader.Close();
                }
                return list.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
        public static List<ObjectData> GetTriggerObjectList(ConnectionData connData)
        {

            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
            SqlCommand cmd = new SqlCommand(@"
                    SELECT  t.name + ' - '+ o.name AS ObjectName,  
	                        'Trigger' as ObjectType,
                            t.modify_date as AlteredDate,
	                        t.create_date as CreateDate,
                            s.name  as SchemaOwner
                    FROM    sys.schemas s RIGHT OUTER JOIN 
                            sys.tables t ON s.schema_id = t.schema_id RIGHT OUTER JOIN 
                            sys.objects o ON t.object_id = o.parent_object_id 
                    WHERE   o.type = 'tr' ", conn);

            return FillObjectData(cmd);

        }
        #endregion

        #region Routine Change Data 
        //public static DateTime LastRoutineChangeDateLoad = DateTime.MinValue;
        /// <summary>
        /// Updates the 'DatabaseObjectChangeDates.Servers' static object with the last modified dates for Stored Procedures, Functions, Views, Tables and Triggers
        /// </summary>
        /// <param name="connData">Connection object</param>
        /// <param name="overrides">Target Override object</param>
        public static void UpdateRoutineAndViewChangeDates(ConnectionData connData, List<DatabaseOverride> overrides)
        {
            //string key;
            string startDb = connData.DatabaseName;
            string serverName = connData.SQLServerName;

            if (overrides == null)
            {
                log.LogWarning("overrides parameter was null!");
                return;
            }

            for (int i = 0; i < overrides.Count; i++)
            {
                connData.DatabaseName = overrides[i].OverrideDbTarget;

                if (string.IsNullOrWhiteSpace(connData.DatabaseName))
                {
                    continue;
                }

                DatabaseObject routines = ChangeDates.DatabaseObjectChangeDates.Servers[serverName][connData.DatabaseName];

                //Set the connection timeout to be short so that we are not waiting in the UI for a bad connection
                SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password, connData.AuthenticationType, 2, connData.ManagedIdentityClientId);

                SqlCommand cmd = new SqlCommand(@"select routine_Name,  last_altered, routine_schema from information_schema.routines ORDER BY routine_Name ", conn);
                //Get the information for Stored Procedures and Functions
                try
                {

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    lock (routines)
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                routines[reader[2].ToString().ToLower() + '.' + reader[0].ToString().ToLower()] = reader.GetDateTime(1);
                            }
                            reader.Close();
                        }
                    }

                }
                catch (Exception rExe)
                {
                    log.LogWarning($"Unable to get modify date information for routines: {rExe.Message}");
                    if (rExe.Message.ToLowerInvariant().IndexOf("login failed") > -1)
                    {
                        continue;
                    }
                }

                //Get the information for Views
                try
                {
                    cmd.CommandText = @"select v.name as [View], v.modify_date, s.name as [Schema] 
                            FROM sys.views v 
                            INNER JOIN sys.schemas s ON v.schema_id = s.schema_id";

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    lock (routines)
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                routines[reader[2].ToString().ToLower() + '.' + reader[0].ToString().ToLower()] = reader.GetDateTime(1);
                            }
                            reader.Close();
                        }
                    }

                }
                catch (Exception vExe)
                {
                    log.LogWarning($"Unable to get modify date information for routines: {vExe.Message}");
                    if (vExe.Message.ToLowerInvariant().IndexOf("login failed") > -1)
                    {
                        continue;
                    }
                }

                //Get the information for Tables
                try
                {
                    cmd.CommandText = @"select v.name as [Table], v.modify_date, s.name as [Schema] 
                            FROM sys.tables v 
                            INNER JOIN sys.schemas s ON v.schema_id = s.schema_id";

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    lock (routines)
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                routines[reader[2].ToString().ToLower() + '.' + reader[0].ToString().ToLower()] = reader.GetDateTime(1);
                            }
                            reader.Close();
                        }
                    }

                }
                catch (Exception tExe)
                {
                    log.LogWarning($"Unable to get modify date information for routines: {tExe.Message}");
                    if (tExe.Message.ToLowerInvariant().IndexOf("login failed") > -1)
                    {
                        continue;
                    }
                }

                //Get the information for Triggers
                try
                {
                    cmd.CommandText = @"select v.name as [Trigger], v.modify_date
                            FROM sys.triggers v";


                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    lock (routines)
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                routines[reader[0].ToString().ToLower()] = reader.GetDateTime(1);
                            }
                            reader.Close();
                        }
                    }

                }
                catch (Exception trExe)
                {
                    log.LogWarning($"Unable to get modify date information for routines: {trExe.Message}");
                    if (trExe.Message.ToLowerInvariant().IndexOf("login failed") > -1)
                    {
                        continue;
                    }

                }


                finally
                {
                    DatabaseObjectChangeDates.Servers[serverName][connData.DatabaseName] = routines;

                    if (conn != null && conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            DatabaseObjectChangeDates.Servers[serverName][connData.DatabaseName].LastRefreshTime = DateTime.Now;
            connData.DatabaseName = startDb;

        }
        #endregion


        public static void ExtractNameAndSchema(string fullObjectName, out string name, out string schemaOwner)
        {
            if (fullObjectName.IndexOf('.') > -1)
            {
                char[] delim = new char[] { '.' };
                schemaOwner = fullObjectName.Split(delim)[0];
                name = fullObjectName.Split(delim)[1];
            }
            else
            {
                schemaOwner = "dbo";
                name = fullObjectName;
            }
        }

        public static void SetUpdateColumnNames()
        {

            char[] delims = new char[] { ',', '~', ';', ':', '|', '^', '!', '#' };
            System.Configuration.AppSettingsReader appReader = new AppSettingsReader();
            codeTableAuditCols = new CodeTableAuditColumnList();

            if (appReader.GetValue("UpdateDateColumns", typeof(string)) != null)
                codeTableAuditCols.UpdateDateColumns.AddRange(((String)appReader.GetValue("UpdateDateColumns", typeof(string))).Split(delims));

            if (appReader.GetValue("UpdateIdColumns", typeof(string)) != null)
                codeTableAuditCols.UpdateIdColumns.AddRange(((String)appReader.GetValue("UpdateIdColumns", typeof(string))).Split(delims));

            if (appReader.GetValue("CreateDateColumns", typeof(string)) != null)
                codeTableAuditCols.CreateDateColumns.AddRange(((String)appReader.GetValue("CreateDateColumns", typeof(string))).Split(delims));


            if (appReader.GetValue("CreateIdColumns", typeof(string)) != null)
                codeTableAuditCols.CreateIdColumns.AddRange(((String)appReader.GetValue("CreateIdColumns", typeof(string))).Split(delims));


        }
        private static string GetDelimitedListForSql(System.Collections.Generic.List<string> list)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append("'" + list[i] + "',");
            }
            sb.Length = sb.Length - 1;
            return sb.ToString();
        }


        public static SqlParameterCollection GetStoredProcParameters(string storedProcedureName, ConnectionData connData)
        {
            SqlConnection conn = null;
            try
            {
                conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);

                SqlCommand com = new SqlCommand();
                com.Connection = conn;
                com.CommandText = storedProcedureName;
                com.CommandType = CommandType.StoredProcedure;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                SqlCommandBuilder.DeriveParameters(com);
                com.Parameters.Remove(com.Parameters["@RETURN_VALUE"]);
                return com.Parameters;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }

        }
    }
}
