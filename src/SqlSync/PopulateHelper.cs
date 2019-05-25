using System;
using System.Collections.Specialized;
using SQLDMO;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Collections;
using System.Configuration;
namespace SQLSync
{
	/// <summary>
	/// Summary description for PopulateHelper.
	/// </summary>
	public class PopulateHelper
	{
		private ConnectionData data= null;
		private string scriptHeader = string.Empty;
		private string[] tableList = null;
		private SQLDMO.SQLServer2Class server = new SQLDMO.SQLServer2Class();
		private SQLDMO.Database2 database = null;
		private SqlConnection dbConn = null;
		private string[] updateDateFields = new string[0];
		private string[] updateIdFields  = new string[0];
		private bool replaceDateAndId = false;
		private SQLSyncData syncData = null;
		public SQLSyncData SyncData 
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
		public PopulateHelper(ConnectionData data, string[] tableList)
		{
			this.data = data;
			this.tableList = tableList;
			this.scriptHeader = "/* \r\nSource Server:\t"+data.SQLServerName+"\r\n" +
				"Source Db:\t"+data.DatabaseName+"\r\n"+
				"Process Date:\t{0}\r\n"+
				"Table Scripted:\t{1}\r\n"+
				"Scripted By:\t"+System.Environment.UserName+"\r\n"+
				"Query Used:\r\n({2})\r\n*/\r\n\r\n";

			if(ConfigurationSettings.AppSettings["UpdateDateFieldNames"] != null)
			{
				this.updateDateFields = ConfigurationSettings.AppSettings["UpdateDateFieldNames"].Split(',');
			}

			if(ConfigurationSettings.AppSettings["UpdateIdFieldNames"] != null)
			{
				this.updateIdFields = ConfigurationSettings.AppSettings["UpdateIdFieldNames"].Split(',');
			}

		}

		public TableScriptData[] GeneratePopulateScripts()
		{
			try
			{
				ArrayList dataList = new ArrayList();
				string selectSql;
				StringBuilder sb = new StringBuilder();
				this.dbConn = GetConnection(this.data.DatabaseName,this.data.SQLServerName,this.data.UserId,this.data.Password);
				for(int i=0;i<this.tableList.Length;i++)
				{
					DataTable table = GetTableValues(this.tableList[i], out selectSql);
					string[] columnNames = GetColumnNames(table);
					sb.Length = 0;
					sb.Append(String.Format(this.scriptHeader,new object[]{DateTime.Now.ToString(),this.tableList[i],selectSql}));
					for(int j=0;j<table.Rows.Count;j++)
					{
						sb.Append( GenerateRowStatement(this.tableList[i],columnNames,table.Rows[j],j+1));
					}
					TableScriptData data = new TableScriptData();
					data.TableName = this.tableList[i];
					data.ValuesTable = table;
					data.InsertScript = sb.ToString();
					data.SelectStatement = selectSql;
					dataList.Add(data);
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
		
		private DataTable GetTableValues(string tableName, out string selectSql)
		{
			
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
			if(fullSelect.Length > 0)
			{
				selectSql = fullSelect;
			}
			else
			{
				selectSql = "SELECT * FROM ["+tableName+"]" + where;
			}
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
		
		private string[] GetColumnNames(DataTable table)
		{
			ArrayList lst = new ArrayList();
			foreach(DataColumn col in table.Columns)
			{
				lst.Add(col.ColumnName);
			}
			string[] names = new string[lst.Count];
			lst.CopyTo(names);
			return names;
		}


		public bool DbContainsTable(string tableName)
		{
			ConnectToServer();
			try
			{
				foreach(SQLDMO._Table table in database.Tables)
				{
					if(table.Name == tableName)
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public string[] GetColumnNames(string tableName)
		{
			ArrayList list = new ArrayList();
			ConnectToServer();
			foreach(SQLDMO._Table table in database.Tables)
			{
				if(table.Name == tableName)
				{
					foreach(SQLDMO._Column col in table.Columns)
					{
						list.Add(col.Name);
					}
					
				}
			}
			
			string[] cols = new string[list.Count];
			list.CopyTo(cols);
			return cols;
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
	
		public string[] RemainingTables(string[] listedTables)
		{
			ConnectToServer();
			bool alreadyListed;
			ArrayList list = new ArrayList();
			for(int i=1;i<this.database.Tables.Count+1;i++)
			{
				alreadyListed = false;
				for(int j=0;j<listedTables.Length;j++)
				{
					if(listedTables[j].ToUpper() == this.database.Tables.Item(i,"dbo").Name.ToUpper())
					{
						alreadyListed = true;
						break;
					}
				}

				if(!alreadyListed)
				{
					list.Add(this.database.Tables.Item(i,"dbo").Name);
				}
			}

			string[] remaining = new string[list.Count];
			list.CopyTo(remaining);
			return remaining;
		}
	
		public string[] RemainingDatabases(string[] listedDatabases)
		{
			ConnectToServer();
			bool alreadyListed;
			ArrayList list = new ArrayList();
			for(int i=1;i<this.server.Databases.Count+1;i++)
			{
				alreadyListed = false;
				for(int j=0;j<listedDatabases.Length;j++)
				{
					if(listedDatabases[j].ToUpper() == this.server.Databases.Item(i,"dbo").Name.ToUpper())
					{
						alreadyListed = true;
						break;
					}
				}

				if(!alreadyListed)
				{
					list.Add(this.server.Databases.Item(i,"dbo").Name);
				}
			}

			string[] remaining = new string[list.Count];
			list.CopyTo(remaining);
			return remaining;
		}
		
		#region ## Script Generation ##
		private string GenerateRowStatement(string tableName, string[] columnNames, DataRow row, int rowNumber)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( GenerateIfNotExistsScript(tableName, columnNames, row) );
			sb.Append("\r\nBEGIN\r\n");
			sb.Append("   "+ GenerateInsertScript(tableName, columnNames, row) );
			sb.Append("\r\n   PRINT 'Inserted Row "+rowNumber.ToString() +"'\r\n");
			sb.Append("END ELSE BEGIN PRINT 'Skipped Duplicate Row "+rowNumber.ToString() +"' END\r\n\r\n");


			return sb.ToString();

		}
		private string GenerateIfNotExistsScript(string tableName, string[] columnNames, DataRow row)
		{
			StringBuilder sb = new StringBuilder("IF NOT EXISTS (SELECT * FROM ["+tableName +"] WHERE ");
			for(int i=0;i<columnNames.Length;i++)
			{
				bool skipField = false;
				//Exclude the update date field from the select statement
				for(int j=0;j<this.updateDateFields.Length;j++)
				{
					if(columnNames[i].ToUpper() == this.updateDateFields[j].ToUpper())
					{
						skipField = true;
						break;
					}
				}

				//Exclude the update Id field from the select statement
				for(int j=0;j<this.updateIdFields.Length;j++)
				{
					if(columnNames[i].ToUpper() == this.updateIdFields[j].ToUpper())
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
			StringBuilder sb = new StringBuilder("INSERT INTO ["+tableName +"] (");
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
					for(int j=0;j<this.updateDateFields.Length;j++)
					{
						if(columnNames[i].ToUpper() == this.updateDateFields[j].ToUpper())
						{
							val = DateTime.Now.ToString();
							break;
						}
					}

					//Update Id
					for(int j=0;j<this.updateIdFields.Length;j++)
					{
						if(columnNames[i].ToUpper() == this.updateIdFields[j].ToUpper())
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
		
		#endregion
	
		private SqlConnection GetConnection(string dbName, string serverName, string uid, string pw)
		{
			string conn = "User ID="+uid+";Initial Catalog="+dbName+";Data Source="+serverName+";Password="+pw+";";
			SqlConnection dbConn = new SqlConnection(conn);
			return dbConn;
		}

	
		#region ## Server Connect/Disconnect ##
		private bool ConnectToServer()
		{
			try
			{
				int i = this.server.Databases.Count;
				return true;
			}
			catch
			{
				//exceptions thrown due to inability to connect.
			}
			try
			{
				
				this.server.LoginSecure = this.data.UseWindowAuthentication;
				this.server.Connect(
					this.data.SQLServerName,
					this.data.UserId,
					this.data.Password);
			}
			catch(System.Runtime.InteropServices.COMException comE)
			{
				if(comE.Message.IndexOf("object is already connected") == -1)
				{
					throw comE;
				}
			}
			try
			{
				this.database = (SQLDMO.Database2)this.server.Databases.Item(this.data.DatabaseName,"dbo");
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
				this.server.DisConnect();
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
				for(int i=1;i<this.database.Users.Count+1;i++)
				{
					sb.Length = 0;
					SQLDMO._User user =  this.database.Users.Item(i);
					sb.Append( user.Script(SQLDMO_SCRIPT_TYPE.SQLDMOScript_Permissions | 
						SQLDMO_SCRIPT_TYPE.SQLDMOScript_PrimaryObject,null,
						SQLDMO_SCRIPT2_TYPE.SQLDMOScript2_AnsiFile | 
						SQLDMO_SCRIPT2_TYPE.SQLDMOScript2_LoginSID));
					users.Add(user.Name,sb.ToString());
				}
			}
			return users;
		}
	}
}
