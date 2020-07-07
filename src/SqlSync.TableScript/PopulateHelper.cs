using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.TableScript;
namespace SqlSync.TableScript
{
	/// <summary>
	/// Summary description for PopulateHelper.
	/// </summary>
	public class PopulateHelper
	{
		private ConnectionData data= null;
		private static string updateTrigFormat = "[{1}].[{0}_UpdateCols_uTrig]";
        private static string shortTrigFormat = "{0}_UpdateCols_uTrig";
		//private string scriptHeader = string.Empty;
		private TableScriptingRule[] tableScriptRules = null;
        private Server smoServer = null;
		private Database smoDatabase = null;
		private SqlConnection dbConn = null;
		private bool replaceDateAndId = false;
		private bool includeUpdates = false;
		private bool addBatchGoSeparators = true;
		private DateTime selectByUpdateDate = DateTime.MinValue;

		public DateTime SelectByUpdateDate
		{
			get {  return selectByUpdateDate; }
			set { selectByUpdateDate = value; }    
		}
	
		private SqlSync.TableScript.SQLSyncData syncData = null;
		public const string scriptHeader = 
			"/* \r\nSource Server:\t{3}\r\n" +
		"Source Db:\t{4}\r\n"+
		"Process Date:\t{0}\r\n"+
		"Table Scripted:\t{1}\r\n"+
		"Scripted By:\t{6}\r\n"+
		"Key Check Columns:{5}\r\n"+
		"Query Used:\r\n{2}\r\n*/\r\n\r\n";

		public SqlSync.TableScript.SQLSyncData SyncData 
		{
			set
			{
				this.syncData = value;
			}
			get
			{
				return this.syncData;
			}
		}
		public PopulateHelper(ConnectionData data, TableScriptingRule[] tableScriptRules)
		{
			this.data = data;
			this.tableScriptRules = tableScriptRules;
			SqlSync.DbInformation.InfoHelper.SetUpdateColumnNames();
			

		}
		
		public PopulateHelper(ConnectionData data)
		{
			this.data = data;
			SqlSync.DbInformation.InfoHelper.SetUpdateColumnNames();
		}

        /// <summary>
        /// Creates Insert and Update scripts using the file contents of a "Data Extract" (data dump) file.
        /// </summary>
        /// <param name="fileContents">sting array contents of the source file</param>
        /// <returns>SQL update scripts generated from the source file</returns>
        public string GeneratePopulateScriptsFromDataExtractFile(string[] fileContents)
        {
            StringBuilder sb = new StringBuilder();

            //Parse out the key columns... header should be in the format of...
            /* 
            Source Server:	localhost
            Source Db:	AdventureWorks
            Process Date:	10/3/2008 1:48:54 PM
            Table Scripted:	Person.Address
            Scripted By:	mmckechn
            Key Check Columns:AddressID
            Query Used:
            SELECT [AddressID],[AddressLine1],[AddressLine2],[City],[StateProvinceID],[PostalCode],[rowguid],[ModifiedDate]
            FROM [Person].[Address] 
            ORDER BY AddressID ASC 
            */
            string[] keyColumns = null;
            string[] allColumns = null;
            string tableName = null;
            int len = fileContents.Length;
            int lastIndex = 0;

            //Get the table name
            for (int i = 0; i < len; i++)
            {
                lastIndex = i;
                if (fileContents[i].Trim().StartsWith("Table Scripted:", StringComparison.CurrentCultureIgnoreCase))
                {
                    int tmpStart = "Table Scripted:".Length;
                    tableName = fileContents[i].Trim().Substring(tmpStart, fileContents[i].Trim().Length - tmpStart).Trim();
                    break;
                }
            }

            //Get the key columns
            for (int i = lastIndex; i < len; i++)
            {
                lastIndex = i;
                if (fileContents[i].Trim().StartsWith("Key Check Columns:", StringComparison.CurrentCultureIgnoreCase))
                {
                    int tmpStart = "Key Check Columns:".Length;
                    string tmp = fileContents[i].Trim().Substring(tmpStart, fileContents[i].Trim().Length - tmpStart);
                    keyColumns = tmp.Split(',');
                    break;
                }
                
            }

            //Parse out the column names...
            for (int i = lastIndex; i < len; i++)
            {
                lastIndex = i;
                if (fileContents[i].Trim().StartsWith("[*Start Data*]", StringComparison.CurrentCultureIgnoreCase) && len > i + 1)
                {
                    allColumns = fileContents[i + 1].Split('|');
                    lastIndex = i + 1;
                    break;
                }
            }
            //Just a little insurance here
            if(len <= lastIndex)
                return string.Empty;

            //Construct a dummy DataTable so we can re-use the scripting functions that are created for the code table scripting
            DataTable tbl = new DataTable();
            for (int i = 0; i < allColumns.Length; i++)
            {
                tbl.Columns.Add(allColumns[i],typeof(string));
            }


            int counter = 0;
            for (int i = lastIndex+1; i < len; i++)
            {
                if (fileContents[i].Length > 0)
                {
                    string[] data = fileContents[i].Split('|');
                    if (data.Length == allColumns.Length)
                    {
                        tbl.Rows.Add(data);
                        sb.Append(GenerateRowStatement(tableName, allColumns, tbl.Rows[0], counter++, keyColumns));
                        sb.Append("GO\r\n\r\n");
                        tbl.Rows.Clear();
                    }
                    else
                    {
                        //TODO: add error message.
                    }
                }
            }
            return sb.ToString();
        }

		public void  GenerateUpdatedPopulateScript(SqlBuild.CodeTable.ScriptUpdates updateRule,out string updatedScript)
		{
			//Get the connection
			this.dbConn = SqlSync.Connection.ConnectionHelper.GetConnection(this.data.DatabaseName,this.data.SQLServerName,this.data.UserId,this.data.Password,this.data.AuthenticationType,this.data.ScriptTimeout);
			
			//get the data
			DataTable table = new DataTable();
			SqlDataAdapter adapt = new SqlDataAdapter(updateRule.Query,this.dbConn);
			try
			{
				adapt.Fill(table);
				string[] columnNames = SqlSync.DbInformation.InfoHelper.GetColumnNames(table);
				StringBuilder sb = new StringBuilder();
				sb.Append(String.Format(scriptHeader,new object[]{DateTime.Now.ToString(),updateRule.SourceTable,updateRule.Query,updateRule.SourceServer,updateRule.SourceDatabase,updateRule.KeyCheckColumns,System.Environment.UserName}));
				for(int j=0;j<table.Rows.Count;j++)
				{
					sb.Append( GenerateRowStatement(updateRule.SourceTable,columnNames,table.Rows[j],j+1, updateRule.KeyCheckColumns.Split(',')));
					if(this.addBatchGoSeparators)
					{
						sb.Append("GO\r\n\r\n");
					}
				}
				updatedScript = sb.ToString();
			}
			catch(Exception e)
			{
				string error = e.ToString();
				updatedScript = string.Empty;
			}
				

		}
		public TableScriptData[] GeneratePopulateScripts()
		{
			try
			{
				ArrayList dataList = new ArrayList();
				string selectSql;
				StringBuilder sb = new StringBuilder();
				this.dbConn = SqlSync.Connection.ConnectionHelper.GetConnection(this.data.DatabaseName,this.data.SQLServerName,this.data.UserId,this.data.Password, this.data.AuthenticationType,this.data.ScriptTimeout);
				for(int i=0;i<this.tableScriptRules.Length;i++)
				{
					DataTable table = GetTableValues(this.tableScriptRules[i].TableName, this.selectByUpdateDate, out selectSql);
					string[] columnNames = SqlSync.DbInformation.InfoHelper.GetColumnNames(table);
					sb.Length = 0;
					sb.Append(String.Format(scriptHeader,new object[]{DateTime.Now.ToString(),this.tableScriptRules[i].TableName,selectSql,data.SQLServerName,data.DatabaseName,String.Join(",",this.tableScriptRules[i].CheckKeyColumns),System.Environment.UserName}));
					for(int j=0;j<table.Rows.Count;j++)
					{
						sb.Append( GenerateRowStatement(this.tableScriptRules[i].TableName,columnNames,table.Rows[j],j+1, this.tableScriptRules[i].CheckKeyColumns));
						if(this.addBatchGoSeparators)
						{
							sb.Append("GO\r\n\r\n");
						}
					}
					TableScriptData tsData = new TableScriptData();
					tsData.TableName = this.tableScriptRules[i].TableName;
					tsData.ValuesTable = table;
					tsData.InsertScript = sb.ToString();
					tsData.SelectStatement = selectSql;
					dataList.Add(tsData);
				}

				TableScriptData[] arrData = new TableScriptData[dataList.Count];
				dataList.CopyTo(arrData);
				return arrData;
			}
			finally
			{
				this.DisconnectServer();
			}
		}
		
		private DataTable GetTableValues(string tableName, DateTime selectUpdateDate, out string selectSql)
		{
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schemaOwner);
			string where = string.Empty;
			string fullSelect = string.Empty;
			if(this.syncData != null)
			{
				DataRow[] dbRows = this.syncData.Database.Select(this.syncData.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName+"'");
				if(dbRows.Length > 0)
				{
					SQLSyncData.LookUpTableRow[] tableRows = ((SQLSyncData.DatabaseRow) dbRows[0]).GetLookUpTableRows();
					for(int i=0;i<tableRows.Length;i++)
					{
						if(tableRows[i].Name.ToLower() == tableName.ToLower() &&  tableRows[i].WhereClause.Length > 0)
						{
							if(tableRows[i].UseAsFullSelect)
							{
								fullSelect = tableRows[i].WhereClause;
							}
							else
							{
								where =  tableRows[i].WhereClause;
							}
						}
					}
				}
			}

			//add the date selection
			if(selectByUpdateDate != DateTime.MinValue)
			{
				string updateDateCol = string.Empty;
                string[] columns = SqlSync.DbInformation.InfoHelper.GetColumnNames(schemaOwner +"."+tableName, this.data);
				for(int i=0;i<SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns.Count;i++)
				{
					for(int j=0;j<columns.Length;j++)
					{
                        if (columns[j].ToLower() == SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns[i].ToLower())
						{
                            updateDateCol = SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns[i];
							break;
						}
					}
					if(updateDateCol != string.Empty)
						break;
				}
				if(updateDateCol.Length > 0)
				{
					if(where.Length > 0)
						where += " AND ";
					else
						where = " WHERE ";

					where += "["+updateDateCol+"]>='"+selectByUpdateDate.ToShortDateString()+"' ";
				}

			}
			if(fullSelect.Length > 0)
				selectSql = fullSelect;
			else
				selectSql = "SELECT * FROM ["+schemaOwner+"].["+tableName+"]" + where;
			
			SqlCommand cmd = new SqlCommand(selectSql, dbConn);
			DataTable table = new DataTable();
			SqlDataAdapter adapt = new SqlDataAdapter(cmd);
			try
			{
				adapt.Fill(table);
			}
			catch
			{
			}

			return table;
		}
		


		public bool ReplaceDateAndId
		{
			get
			{
				return this.replaceDateAndId;
			}
			set
			{
				this.replaceDateAndId = value;
			}
		}
		public bool IncludeUpdates
		{
			get
			{
				return this.includeUpdates;
			}
			set
			{
				this.includeUpdates = value;
			}
		}
		public bool AddBatchGoSeparators
		{
			get
			{
				return this.addBatchGoSeparators;
			}
			set
			{
				this.addBatchGoSeparators = value;
			}
		}

        public List<string> RemainingTables(List<string> listedTables)
		{
			bool alreadyListed;
            List<string> list = new List<string>();
			string[] allTables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableList(this.data);
			for(int i=0;i<allTables.Length;i++)
			{
				alreadyListed = false;
				for(int j=0;j<listedTables.Count;j++)
				{
					if(listedTables[j].ToUpper() == allTables[i].ToUpper())
					{
						alreadyListed = true;
						break;
					}
				}

				if(!alreadyListed)
				{
					list.Add(allTables[i]);
				}
			}
            return list;
		}
	
		public string[] RemainingDatabases(string[] listedDatabases)
		{
			ConnectToServer();
			bool alreadyListed;
			ArrayList list = new ArrayList();
			for(int i=0;i<this.smoServer.Databases.Count;i++)
			{
				alreadyListed = false;
				for(int j=0;j<listedDatabases.Length;j++)
				{
                    if (listedDatabases[j].ToUpper() == this.smoServer.Databases[i].Name.ToUpper())
					{
						alreadyListed = true;
						break;
					}
				}

				if(!alreadyListed)
				{
                    list.Add(this.smoServer.Databases[i].Name);
				}
			}

			string[] remaining = new string[list.Count];
			list.CopyTo(remaining);
			return remaining;
		}
		
        
	
		#region .: Script Generation :.
		private string GenerateRowStatement(string tableName, string[] columnNames, DataRow row, int rowNumber,  string[] checkKeyColumns)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( GenerateIfNotExistsScript(tableName, columnNames, row,checkKeyColumns) );
			sb.Append("\r\nBEGIN\r\n");
			sb.Append("   "+ GenerateInsertScript(tableName, columnNames, row) );
			sb.Append("\r\n   PRINT 'Inserted Row "+rowNumber.ToString() +"'\r\n");
			sb.Append("END\r\n");
			if(checkKeyColumns.Length > 0 && checkKeyColumns[0].Length > 0)
			{
				sb.Append("ELSE\r\nBEGIN\r\n");
				sb.Append("   "+ GenerateUpdateScript(tableName, columnNames, row, checkKeyColumns) );
				sb.Append("\r\n   PRINT 'Updated Row "+rowNumber.ToString() +"'\r\n");
				sb.Append("END\r\n\r\n");
			}
			else
			{
				sb.Append("ELSE BEGIN PRINT 'Skipped Duplicate Row "+rowNumber.ToString() +"' END\r\n\r\n");
			}


			return sb.ToString();

		}
		private string GenerateIfNotExistsScript(string tableName, string[] columnNames, DataRow row, string[] checkKeyColumns)
		{
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schemaOwner);

			//Instead of checking values on all columns, just use
			//The specified check columns
			if(checkKeyColumns.Length > 0 && checkKeyColumns[0].Length > 0)
			{
				columnNames = checkKeyColumns;
			}
			StringBuilder sb = new StringBuilder("IF NOT EXISTS (SELECT 1 FROM ["+schemaOwner+"].["+tableName +"] WITH (NOLOCK) WHERE ");
			for(int i=0;i<columnNames.Length;i++)
			{
				bool skipField = false;
                List<string> updateDateCols = SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns;
				//Exclude the update date field from the select statement
				for(int j=0;j<updateDateCols.Count;j++)
				{
					if(columnNames[i].ToUpper() == updateDateCols[j].ToUpper())
					{
						skipField = true;
						break;
					}
				}

				//Exclude the update Id field from the select statement
                List<string> updateIdCols = SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateIdColumns;

                for (int j = 0; j < updateIdCols.Count; j++)
				{
                    if (columnNames[i].ToUpper() == updateIdCols[j].ToUpper())
					{
						skipField = true;
						break;
					}
				}

				//Exclude Date data types since the string formatting
				//won't necessarily match
				if(row[columnNames[i]].GetType() == typeof(DateTime))
				{
					skipField = true;
				}

				
				//Skip the field if selected.
				if(skipField) 
				{
					continue;
				}

				sb.Append("["+columnNames[i]+"]");
				if(row[columnNames[i]] == DBNull.Value)
				{
					sb.Append(" IS NULL ");
				}
				else
				{
					if(row[columnNames[i]].GetType() == typeof(Boolean))
					{
						sb.Append("="+Convert.ToByte(row[columnNames[i]]));
					}
					else
					{
						sb.Append("='"+row[columnNames[i]].ToString().TrimEnd().Replace("'","''")+"'");
					}
				}										 
			
				if(i<columnNames.Length-1)
				{
					sb.Append(" AND ");
				}
			}
			if(sb.ToString().EndsWith(" AND "))
			{
				sb.Length = sb.Length - 5;
			}
			sb.Append(")");
			return sb.ToString();
		}
		private string GenerateInsertScript(string tableName, string[] columnNames, DataRow row)
		{
			string val;
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schemaOwner);
			StringBuilder sb = new StringBuilder("INSERT INTO ["+schemaOwner+"].["+tableName +"] (");
			for(int i=0;i<columnNames.Length;i++)
			{
				sb.Append("["+columnNames[i]+"]");
				if(i<columnNames.Length-1)
				{
					sb.Append(",");
				}
			}
			sb.Append(") VALUES (");
			for(int i=0;i<columnNames.Length;i++)
			{
				val = string.Empty;
				//Check to see if we need to insert our new values
				if(this.ReplaceDateAndId == true)
				{
					//Update date
					for(int j=0;j<SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns.Count;j++)
					{
                        if (columnNames[i].ToUpper() == SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns[j].ToUpper())
						{
							val = DateTime.Now.ToString();
							break;
						}
					}

					//Update Id
                    for (int j = 0; j < SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateIdColumns.Count; j++)
					{
                        if (columnNames[i].ToUpper() == SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateIdColumns[j].ToUpper())
						{
							val = System.Environment.UserName;
							break;
						}
					}
				}
				

				if(row[columnNames[i]] == DBNull.Value && val == string.Empty)
				{
					sb.Append( " NULL ");
				}
				else
				{
					if(val == string.Empty)
					{
						if(row[columnNames[i]].GetType() == typeof(Boolean))
						{
							sb.Append(Convert.ToByte(row[columnNames[i]]));
						}
						else
						{
							sb.Append("'"+row[columnNames[i]].ToString().TrimEnd().Replace("'","''")+"'");
						}
					}
					else
					{
						sb.Append("'"+val+"'");
					}
				}
				if(i<columnNames.Length-1)
				{
					sb.Append(",");
				}
			}
			sb.Append(")");
			return sb.ToString();
		}
		
		private string GenerateUpdateScript(string tableName, string[] columnNames, DataRow row, string[] updateKeyColumns)
		{
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schemaOwner);
			string val;
			StringBuilder sb = new StringBuilder("UPDATE ["+schemaOwner+"].["+tableName +"] SET ");
			
			for(int i=0;i<columnNames.Length;i++)
			{
				//Skip columns defined as key columns
				bool skipColumn = false;
				for(int j=0;j<updateKeyColumns.Length;j++)
				{
					if(columnNames[i] == updateKeyColumns[j])
					{
						skipColumn = true;
						break;
					}
				}
				if(skipColumn) continue;

				//Column Name
				sb.Append("["+columnNames[i]+"]=");
				val = string.Empty;
				//Check to see if we need to insert our new values
				if(this.ReplaceDateAndId == true)
				{
					//Update date
                    for (int j = 0; j < SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns.Count; j++)
					{
                        if (columnNames[i].ToUpper() == SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns[j].ToUpper())
						{
							val = DateTime.Now.ToString();
							break;
						}
					}

					//Update Id
                    for (int j = 0; j < SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateIdColumns.Count; j++)
					{
                        if (columnNames[i].ToUpper() == SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateIdColumns[j].ToUpper())
						{
							val = System.Environment.UserName;
							break;
						}
					}
				}
				

				if(row[columnNames[i]] == DBNull.Value && val == string.Empty)
				{
					sb.Append( " NULL ");
				}
				else
				{
					if(val == string.Empty)
					{
						if(row[columnNames[i]].GetType() == typeof(Boolean))
						{
							sb.Append(Convert.ToByte(row[columnNames[i]]));
						}
						else
						{
							sb.Append("'"+row[columnNames[i]].ToString().TrimEnd().Replace("'","''")+"'");
						}
					}
					else
					{
						sb.Append("'"+val+"'");
					}
				}
				if(i<columnNames.Length-1)
				{
					sb.Append(",");
				}
			}
			if(sb.ToString().EndsWith(","))
			{
				sb.Length = sb.Length-1;
			}
			//Where clause
			sb.Append (" WHERE ");
			for(int i=0;i<updateKeyColumns.Length;i++)
			{
				sb.Append("["+updateKeyColumns[i]+"]=");
				for(int j=0;j<columnNames.Length;j++)
				{
					if(columnNames[j] == updateKeyColumns[i])
					{
						sb.Append("'"+row[columnNames[j]].ToString().TrimEnd().Replace("'","''")+"'");
						break;
					}
				}
				if(i<updateKeyColumns.Length-1)
					sb.Append(" AND ");
			}

			return sb.ToString();
		}
		

		#endregion
	
		#region .: UpdateDate and UpdateId Handling :.
        public static string ScriptForMissingColumns(CodeTableAudit codeTable)
		{
			//if(SqlSync.DbInformation.InfoHelper.UpdateDateFields.Length == 0 || SqlSync.DbInformation.InfoHelper.UpdateIdFields.Length == 0)
			if(!SqlSync.DbInformation.InfoHelper.codeTableAuditCols.IsValid)
                SqlSync.DbInformation.InfoHelper.SetUpdateColumnNames();

            string tableName = codeTable.TableName;
            string schema;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schema);
			//Create the columns as null
			StringBuilder sb = new StringBuilder();
            if (codeTable.CreateIdColumn.Length == 0)
                sb.Append(ScriptTemplates.ADD_UPDATE_ID_COLUMN.Replace("<<tableName>>", tableName)
                    .Replace("<<schema>>",schema)
                    .Replace("<<columnName>>", SqlSync.DbInformation.InfoHelper.codeTableAuditCols.CreateIdColumns[0])
                    .Replace("<<userName>>", System.Environment.UserName));

            if (codeTable.CreateDateColumn.Length == 0)
                sb.Append(ScriptTemplates.ADD_UPDATE_DATE_COLUMN.Replace("<<tableName>>", tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", SqlSync.DbInformation.InfoHelper.codeTableAuditCols.CreateDateColumns[0])
                    .Replace("<<date>>", DateTime.Now.ToString()));

            if (codeTable.UpdateIdColumn.Length == 0)
                sb.Append(ScriptTemplates.ADD_UPDATE_ID_COLUMN.Replace("<<tableName>>", tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateIdColumns[0])
                    .Replace("<<userName>>", System.Environment.UserName));

			if(codeTable.UpdateDateColumn.Length == 0)
				sb.Append(ScriptTemplates.ADD_UPDATE_DATE_COLUMN.Replace("<<tableName>>",tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", SqlSync.DbInformation.InfoHelper.codeTableAuditCols.UpdateDateColumns[0])
                    .Replace("<<date>>",DateTime.Now.ToString()));

			return sb.ToString();
		}
	
		public static string ScriptForUpdateTrigger(CodeTableAudit auditTable, ConnectionData connData)
		{
            string updateDateCol = auditTable.UpdateDateColumn;
            string updateIdCol = auditTable.UpdateIdColumn;
            string tableName = auditTable.TableName;
            string schema;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schema);
			string triggerName = String.Format(updateTrigFormat,tableName,schema);
            string shortTrigger = String.Format(shortTrigFormat, tableName);
			StringBuilder remove = new StringBuilder();
            remove.Append("IF EXISTS (SELECT name FROM sys.objects WHERE name = '" + shortTrigger + "' AND type = 'TR')\r\n");
			remove.Append("\tDROP TRIGGER "+triggerName+"\r\n");
			remove.Append("GO\r\n\r\n");

           

            if (updateDateCol == string.Empty || updateIdCol == string.Empty)
				return string.Empty;

            ArrayList pkCols = GetPrimaryKeyColumnsWithType(auditTable.TableName, connData);
			if(pkCols.Count == 0)
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			sb.Append("CREATE TRIGGER "+triggerName+" ON ["+schema+"].["+tableName+"] FOR UPDATE, INSERT\r\nAS\r\nBEGIN\r\n");
			for(int i=0;i<pkCols.Count;i++)
			{
				string[] col = (string[])pkCols[i];
				sb.Append("\tDECLARE @"+col[0].Replace(" ","") +" "+col[1]+"\r\n");
			}
			sb.Append("\r\n\tSELECT ");
			for(int i=0;i<pkCols.Count;i++)
			{
				string[] col = (string[])pkCols[i];
				sb.Append("@"+col[0].Replace(" ","") +"=["+col[0]+"],");
			}
			sb.Length = sb.Length-1;
			sb.Append(" FROM inserted\r\n\r\n");

            sb.Append("\tIF NOT EXISTS(SELECT 1 FROM [" + schema + "].[" + tableName + "] WHERE ");
			for(int i=0;i<pkCols.Count;i++)
			{
				string[] col = (string[])pkCols[i];
				sb.Append("["+col[0]+"]=@"+col[0].Replace(" ","")+ " AND ");
			}
			sb.Length = sb.Length -4;
			sb.Append(")\r\n");
            sb.Append("\t\tRAISERROR('Unable to complete UpdateId/UpdateDate auditing trigger for [" + schema + "].[" + tableName + "] table. PK Value Not Found. Change Rolled Back.',16,1)\r\n\r\n");

           sb.Append("\r\n\tSELECT @@nestlevel");
	        sb.Append("\r\n\tIF @@nestlevel <= 10");
	        sb.Append("\r\n\tBEGIN");

            sb.Append("\r\n\t\tUPDATE [" + schema + "].[" + tableName + "] SET [" + updateDateCol + "] = getdate(), [" + updateIdCol + "] = SYSTEM_USER\r\n");
			sb.Append("\t\tWHERE ");
			for(int i=0;i<pkCols.Count;i++)
			{
				string[] col = (string[])pkCols[i];
				sb.Append("["+col[0]+"]=@"+col[0].Replace(" ","")+ " AND ");
			}
			sb.Length = sb.Length -4;

			sb.Append("\r\n\r\n\t\tIF(@@Error <> 0)\r\n");
            sb.Append("\t\t\tRAISERROR('Unable to complete UpdateId/UpdateDate auditing trigger for [" + schema + "].[" + tableName + "] table. Change Rolled Back.',16,1)\r\n");

            sb.Append("\r\n\tEND");
			sb.Append("\r\nEND");
			sb.Append("\r\nGO\r\n");


			return remove.ToString() + sb.ToString();

		}
        public static string ScriptColumnDefaultsReset(CodeTableAudit codeTable)
		{
            string tableName = codeTable.TableName;
            string schema;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schema);
			StringBuilder sb = new StringBuilder();

            if (codeTable.UpdateIdColumn.Length > 0)
				sb.Append(ScriptTemplates.DropExistingDefaultConstraint.Replace("<<tableName>>",tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", codeTable.UpdateIdColumn)
                    .Replace("<<defaultValue>>","SYSTEM_USER"));

            if (codeTable.UpdateDateColumn.Length > 0)
				sb.Append(ScriptTemplates.DropExistingDefaultConstraint.Replace("<<tableName>>",tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", codeTable.UpdateDateColumn)
                    .Replace("<<defaultValue>>","getdate()"));

            if (codeTable.CreateIdColumn.Length > 0)
                sb.Append(ScriptTemplates.DropExistingDefaultConstraint.Replace("<<tableName>>", tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", codeTable.CreateIdColumn)
                    .Replace("<<defaultValue>>", "SYSTEM_USER"));

            if (codeTable.CreateDateColumn.Length > 0)
                sb.Append(ScriptTemplates.DropExistingDefaultConstraint.Replace("<<tableName>>", tableName)
                    .Replace("<<schema>>", schema)
                    .Replace("<<columnName>>", codeTable.CreateDateColumn)
                    .Replace("<<defaultValue>>", "getdate()"));

			return sb.ToString();
		}
		internal static ArrayList GetPrimaryKeyColumnsWithType(string tableName, ConnectionData connData)
		{
            string schema;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schema);
			SqlConnection conn =  SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName,connData.SQLServerName,connData.UserId,connData.Password,connData.AuthenticationType, connData.ScriptTimeout);
			string command = @"
                select cc.column_Name,c.data_type +
	                CASE WHEN CHARACTER_MAXIMUM_LENGTH IS NULL THEN '' 
	                ELSE '('+CONVERT(varchar(50),CHARACTER_MAXIMUM_LENGTH)+')' END from 
	                information_schema.TABLE_CONSTRAINTS TC
	                INNER JOIN information_schema.CONSTRAINT_COLUMN_USAGE cc ON cc.constraint_name = tc.constraint_Name 
	                INNER JOIN information_schema.COLUMNS c ON c.TABLE_NAME = tc.Table_Name and c.column_name = cc.column_name
	                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' and 
	                tc.TABLE_NAME = @TableName and 
                    tc.TABLE_SCHEMA = @Schema ";
			SqlCommand cmd = new SqlCommand(command,conn);
			cmd.Parameters.AddWithValue("@TableName",tableName);
            cmd.Parameters.AddWithValue("@Schema", schema);
			ArrayList list = new ArrayList();
			conn.Open();
			using(SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
			{
				while(reader.Read())
					list.Add(new string[]{reader[0].ToString(),reader[1].ToString()});
				reader.Close();
			}
			return list;
		}
        //public static void FindUpdateColumnNames(string tableName, ConnectionData connData, out string updateByColumn, out bool updateByExists, out string updateDateColumn, out bool updateDateExists)
        //{
        //    string[] columns = SqlSync.DbInformation.InfoHelper.GetColumnNames(tableName,connData);
		

        //    updateByColumn = SqlSync.DbInformation.InfoHelper.UpdateIdFields[0];
        //    updateByExists = false;
        //    for(int i=0;i<columns.Length;i++)
        //    {
        //        for(int j=0;j<SqlSync.DbInformation.InfoHelper.UpdateIdFields.Length;j++)
        //        {
        //            if(columns[i].ToLower() == SqlSync.DbInformation.InfoHelper.UpdateIdFields[j].ToLower())
        //            {
        //                updateByColumn = SqlSync.DbInformation.InfoHelper.UpdateIdFields[j];
        //                updateByExists = true;
        //                break;
        //            }
        //        }
        //        if(updateByExists)
        //            break;
        //    }

        //    updateDateColumn = SqlSync.DbInformation.InfoHelper.UpdateDateFields[0];
        //    updateDateExists = false;
        //    for(int i=0;i<columns.Length;i++)
        //    {
        //        for(int j=0;j<SqlSync.DbInformation.InfoHelper.UpdateDateFields.Length;j++)
        //        {
        //            if(columns[i].ToLower() == SqlSync.DbInformation.InfoHelper.UpdateDateFields[j].ToLower())
        //            {
        //                updateDateColumn = SqlSync.DbInformation.InfoHelper.UpdateDateFields[j];
        //                updateDateExists = true;
        //                break;
        //            }
        //        }
        //        if(updateDateExists)
        //            break;
        //    }
        //}
        //public static void FindUpdateColumnNames(string tableName, ConnectionData connData, out string updateByColumn,  out string updateDateColumn)
        //{
        //    bool notUsed1;
        //    bool notUsed2;
        //    FindUpdateColumnNames(tableName,connData,out updateByColumn,out notUsed1,out updateDateColumn,out notUsed2);
        //}

		public static UpdateAutoDetectData[] AutoDetectUpdateTriggers(ConnectionData connData,TableSize[] allTable)
		{
			string[] allTriggers = SqlSync.DbInformation.InfoHelper.GetTriggers(connData);
			ArrayList lst = new ArrayList();
			for(int i=0;i<allTriggers.Length;i++)
			{
				for(int j=0;j<allTable.Length;j++)
				{
                    //TODO: account for the . in the table name
                    string table, schema;
                    InfoHelper.ExtractNameAndSchema(allTable[j].TableName, out table, out schema);
                    if (allTriggers[i] == String.Format(shortTrigFormat, table))
					{
						UpdateAutoDetectData dat = new UpdateAutoDetectData();
						dat.TableName = allTable[j].TableName;
						dat.RowCount = allTable[j].RowCount;
						dat.HasUpdateTrigger = true;
						lst.Add(dat);
						break;
					}
				}
			}

			UpdateAutoDetectData[] data = new UpdateAutoDetectData[lst.Count];
			lst.CopyTo(data);
			return data;
		}
		public static UpdateAutoDetectData[] AutoDetectUpdateTriggers(ConnectionData connData)
		{
			TableSize[] allTable = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(connData);
			return AutoDetectUpdateTriggers(connData,allTable);

		}
		
		#endregion


		#region .: Server Connect/Disconnect :.
        private bool ConnectToServer()
        {
            try
            {
                if (this.smoServer == null)
                {
                    if (this.data.AuthenticationType == Connection.AuthenticationType.Windows || this.data.AuthenticationType == Connection.AuthenticationType.AzureADIntegrated)
                        this.smoServer = new Microsoft.SqlServer.Management.Smo.Server(this.data.SQLServerName);
                    else
                        this.smoServer = new Server(new ServerConnection(this.data.SQLServerName, this.data.UserId, this.data.Password));
                }

                if (this.smoServer == null)
                    throw new ApplicationException();

                return true;
            }
            catch
            {
                return false;
            }

        }


        public bool DisconnectServer()
        {
            try
            {
                this.smoServer = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

		#endregion

		public NameValueCollection TestUser()
		{
			NameValueCollection users = new NameValueCollection();
			StringBuilder sb = new StringBuilder();
			if(ConnectToServer())
			{
                this.smoDatabase = this.smoServer.Databases[this.data.DatabaseName];
                if (this.smoDatabase == null)
                    return users;

                for (int i = 1; i < this.smoDatabase.Users.Count; i++)
				{
					sb.Length = 0;
                    User user = this.smoDatabase.Users[i];
                    ScriptingOptions options = new ScriptingOptions();
                    options.Permissions = true;
                    options.PrimaryObject = true;
                    options.LoginSid = true;
                    options.AnsiFile = true;
                    StringCollection coll =  user.Script(options);
                    foreach (string s in coll)
                    {
                        sb.AppendLine(s);
                        sb.AppendLine("GO");
                        sb.AppendLine("\r\n");
                    }

					users.Add(user.Name,sb.ToString());
				}
			}
			return users;
		}

		
	}
}
