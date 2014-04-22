using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Collections;
using SqlSync.DbInformation;
namespace SqlSync.TableScript
{
	/// <summary>
	/// Summary description for DataDumpHelper.
	/// </summary>
	public class DataDumpHelper
	{
		SqlSync.Connection.ConnectionData connData = null;
		string[] tableList = null;
		string basePath;
		public DataDumpHelper(SqlSync.Connection.ConnectionData connData, string[] tableList, string basePath)
		{
			this.connData = connData;
			this.tableList = tableList;
			this.basePath = basePath;

			if(this.basePath.EndsWith("/") == false)
				this.basePath = this.basePath +"/";
		}

		public void ExtractAndWriteData()
		{
			for(int i=0;i<tableList.Length;i++)
			{
				if(this.ProcessingTableData != null)
					this.ProcessingTableData(null,new ProcessingTableDataEventArgs(tableList[i],"Getting Column and Key Information",false));

				ColumnInfo[] columnNamesWithTypes = InfoHelper.GetColumnNamesWithTypes(tableList[i],connData);
				int originalCols = columnNamesWithTypes.Length;
				string[] pkName = InfoHelper.GetPrimaryKeyColumns(tableList[i],connData);
				SqlCommand cmd = BuildCommand(tableList[i],pkName,ref columnNamesWithTypes);

				string command = cmd.CommandText;
				if(originalCols != columnNamesWithTypes.Length)
					command +="\r\n--NOTE: Binary column types excluded";

				string header = String.Format(PopulateHelper.scriptHeader,
					DateTime.Now.ToString(),
					tableList[i],
					command,
					this.connData.SQLServerName,
					this.connData.DatabaseName,
					String.Join(",",pkName),
					System.Environment.UserName);
		
				WriteData(tableList[i],cmd,columnNamesWithTypes,header);

			}
			if(this.ProcessingTableData != null)
				this.ProcessingTableData(null,new ProcessingTableDataEventArgs("","",true));

		}


		private SqlCommand BuildCommand(string tableName, string[] pkName, ref ColumnInfo[] columns)
		{
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(tableName, out tableName, out schemaOwner);
			string[] nonSorting = new string[]{"binary","varbinary","text","image","ntext"};
			string[] binaryData = new string[]{"binary","varbinary","image"};
			ArrayList toKeep = new ArrayList();
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT ");
			for(int i=0;i<columns.Length;i++)
			{
				if(Array.IndexOf(binaryData,columns[i].DataType) == -1)
				{
					sb.Append("["+columns[i].ColumnName +"],");
					toKeep.Add(columns[i]);
				}
			}
				
			columns = new ColumnInfo[toKeep.Count];
			toKeep.CopyTo(columns);
		
			sb.Length = sb.Length -1;

			sb.Append("\r\nFROM ["+schemaOwner+"].["+tableName+"] ");

			if(pkName.Length > 0)
			{
				sb.Append("\r\nORDER BY ");
				for(int i=0;i<pkName.Length;i++)
					sb.Append(pkName[i] +" ASC ,");
				
				sb.Length = sb.Length -1;
			} 
			else
			{
				sb.Append("\r\n ORDER BY ");
				for(int i=0;i<columns.Length;i++)
				{
					if(Array.IndexOf(nonSorting,columns[i].DataType) == -1)
						sb.Append("["+columns[i].ColumnName +"] ASC ,");
				}
				
				sb.Length = sb.Length -1;
			}
			
			SqlConnection conn =  SqlSync.Connection.ConnectionHelper.GetConnection(connData.DatabaseName,connData.SQLServerName,connData.UserId,connData.Password,connData.UseWindowAuthentication,connData.ScriptTimeout);
			SqlCommand cmd = new SqlCommand(sb.ToString(),conn);
			return cmd;			
		}

		private bool WriteData(string tableName, SqlCommand cmd, ColumnInfo[] columns,string header)
		{
			try
			{
				StringBuilder sb = new StringBuilder();
				int colCount = columns.Length;

				string fileName = this.basePath+tableName+".data";

				if(this.ProcessingTableData != null)
					this.ProcessingTableData(null,new ProcessingTableDataEventArgs(tableName,"Writing Data to "+Path.GetFileName(fileName),false));

				using(StreamWriter sw = File.CreateText(fileName))
				{
					sw.WriteLine(header);
					sw.WriteLine("[*Start Data*]");
					for(int i=0;i<colCount;i++)
					{
						sw.Write(columns[i].ColumnName);
						if(i<colCount-1) sw.Write("|");
					}
					sw.Write("\r\n");
					if(cmd.Connection.State == ConnectionState.Closed)
						cmd.Connection.Open();

					using(SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						while(dr.Read())
						{
							sb.Length = 0;
							for(int j=0;j<colCount;j++)
							{
								if(dr[j] == DBNull.Value)
									sb.Append("NULL|");
								else if (dr[j].GetType() == typeof(Boolean))
									sb.Append(Convert.ToByte(dr[j])+"|");
								else
									sb.Append(dr[j].ToString().Replace("\r"," ").Replace("\n"," ")+"|");
							}
							sb.Length = sb.Length -1;
							sw.WriteLine(sb.ToString());
						}
					}
					
					sw.Flush();
					sw.Close();
				}

				if(this.FileWritten != null)
				{
					long size = new FileInfo(fileName).Length;
					this.FileWritten(null,new FileWrittenEventArgs(fileName,size));
				}
				return true;
			}
			catch(Exception e)
			{
				string forDebug = e.ToString();
				return false;
			}
		}


		public delegate void ProcessingTableDataEventHandler(object sender, ProcessingTableDataEventArgs e);
		public event ProcessingTableDataEventHandler ProcessingTableData;

		public delegate void FileWrittenEventHandler(object sender, FileWrittenEventArgs e);
		public event FileWrittenEventHandler FileWritten;

		
			
	}
}
