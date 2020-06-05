using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
namespace SqlSync.ObjectScript
{
	/// <summary>
	/// Code adapted from http://www.codeproject.com/useritems/validatingsql.asp
	/// by IPC2000
	/// </summary>
	public class ObjectValidator
	{
		Connection.ConnectionData connData = null;
        BackgroundWorker bgWorker;
		public ObjectValidator()
		{

		}
		public void Validate(Connection.ConnectionData connData, BackgroundWorker bgWorker, DoWorkEventArgs eW)
		{
            this.bgWorker = bgWorker;
			this.connData = connData;
           //string getObjSql = "select name, OBJECTPROPERTY(id, 'ExecIsQuotedIdentOn') as quoted_ident_on, OBJECTPROPERTY(id, 'ExecIsAnsiNullsOn') as ansi_nulls_on, user_name(o.uid) owner, type from sys.objects o where type in ('P', 'V', 'FN') and category = 0 order by type";
            string getObjSql = @"select o.name, OBJECTPROPERTY(id, 'ExecIsQuotedIdentOn') as quoted_ident_on, 
OBJECTPROPERTY(id, 'ExecIsAnsiNullsOn') as ansi_nulls_on, user_name(o.uid) owner, s.name as [schema], type 
from sys.objects o 
INNER JOIN sys.schemas s ON s.schema_id = o.uid where type in ('P', 'V', 'FN') and category = 0 order by type";
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
            SqlCommand cmd = new SqlCommand(getObjSql, conn);
            SqlDataAdapter adapt = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            try
            {
                adapt.Fill(table);
            }
            catch (Exception e)
            {
                this.bgWorker.ReportProgress(0, new ValidationResultEventArgs("", "", e.Message, ValidationResultValue.Invalid));
                return;
            }

            Regex crossDbCheck = new Regex(@"\.\w*\.",RegexOptions.IgnoreCase);
            //for each object get the command text and execute the command
            foreach (DataRow procRow in table.Rows)
            {
                if (bgWorker.CancellationPending)
                {
                    eW.Cancel = true;
                    return;
                }
                conn.ConnectionString = SqlSync.Connection.ConnectionHelper.GetConnectionString(connData);
                string objectName = procRow["Name"].ToString();
                string quoted_ident = Convert.ToBoolean(procRow["quoted_ident_on"]) ? "ON" : "OFF";
                string ansi_nulls_on = Convert.ToBoolean(procRow["ansi_nulls_on"]) ? "ON" : "OFF";
                string owner = procRow["owner"].ToString();
                string type = procRow["type"].ToString();
                string schema = procRow["schema"].ToString();

                //call sp_helptext to the create command
                string getObjTextSql = string.Format("exec sp_helptext '{1}.{0}'", objectName, schema);
                cmd = new SqlCommand(getObjTextSql, conn);

                SqlDataReader textRdr = null;
                try
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    textRdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (SqlException ex)
                {
                    string errText = string.Format("COULD NOT BE READ : {0}", ex.Message);
                    this.bgWorker.ReportProgress(0, new ValidationResultEventArgs(schema+"."+ objectName, type, errText, ValidationResultValue.Invalid));
                    continue;
                }

                //loop through the command text and build up the command string
                StringBuilder sb = new StringBuilder(8000);
                bool detectedCrossDatabase = false;
                using (textRdr)
                {
                    while (textRdr.Read())
                    {
                        string thisText = textRdr["text"].ToString();
                        if (detectedCrossDatabase == false && crossDbCheck.Match(thisText).Success && !thisText.Trim().StartsWith("--")) //Check for 3 part declaration for cross DB check.
                            detectedCrossDatabase = true;

                        sb.Append(thisText);
                    }
                }
                string procText = sb.ToString();

                #region << Check that the columns are valid >>
                //execute the command (this will check for valid colums)
                try
                {
                    using (conn)
                    {
                        conn.Open();
                        SqlCommand sqlCmd = new SqlCommand("SET NOEXEC ON", conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand("SET QUOTED_IDENTIFIER " + quoted_ident, conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand("SET ANSI_NULLS " + ansi_nulls_on, conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand(procText, conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand("SET QUOTED_IDENTIFIER OFF ", conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand("SET ANSI_NULLS ON", conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand("SET NOEXEC OFF", conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                        sqlCmd = new SqlCommand("SET PARSEONLY OFF", conn);
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.ExecuteNonQuery();

                    }

                }
                catch (SqlException ex)
                {
                    this.bgWorker.ReportProgress(0, new ValidationResultEventArgs(schema+"."+ objectName, type, ex.Message.Replace("\r", "; ").Replace("\n", ""), ValidationResultValue.Invalid));
                    continue;
                }
                #endregion

                #region << Check that the table references are valid >>
                string dependsSQL = String.Format("sp_depends [{1}.{0}]", objectName,schema);
                conn.ConnectionString = SqlSync.Connection.ConnectionHelper.GetConnectionString(connData);
                cmd = new SqlCommand(dependsSQL, conn);
                DataSet ds = new DataSet();
                adapt.SelectCommand = cmd;

                try
                {
                    adapt.Fill(ds);
                    if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    {
                        string errText;
                        ValidationResultValue val;
                        if (detectedCrossDatabase)
                        {
                            errText = string.Format("Unable to Validate: Possible Cross Database Join");
                            val = ValidationResultValue.CrossDatabaseJoin;
                        }
                        else
                        {
                            errText = string.Format("Invalid Table Reference? No Table Dependencies found.");
                            val = ValidationResultValue.Caution;
                        }

                        this.bgWorker.ReportProgress(0, new ValidationResultEventArgs(schema+"."+ objectName, type, errText, val));
                        continue;
                    }
                }
                catch (SqlException ex)
                {
                    //string errText = string.Format("{0} sp_depends FAILED : {1}", objectName, ex.ToString()).Replace("\r", " ").Replace("\n", " ");
                    this.bgWorker.ReportProgress(0, new ValidationResultEventArgs(schema+"."+ objectName, type, ex.Message, ValidationResultValue.Invalid));
                    continue;

                }

                #endregion


                //Success!
                this.bgWorker.ReportProgress(0, new ValidationResultEventArgs(schema+"."+ objectName, type, "Valid", ValidationResultValue.Valid));

            }
          
		}

        //public event EventHandler ValidationComplete;
        //public event ValidationResultEventHandler ValidationResult;
        //public delegate void ValidationResultEventHandler(object sender, ValidationResultEventArgs e);
		

	}

	public enum ValidationResultValue
	{
		Valid,
		Invalid,
		Caution,
		CrossDatabaseJoin
	}

	public class ValidationResultEventArgs : EventArgs
	{
		public readonly string Type;
		public readonly string Name;
		public readonly string Message;
		public readonly ValidationResultValue ResultValue;

		public ValidationResultEventArgs(string name, string type, string message, ValidationResultValue resultValue)
		{
			this.Name = name;
			this.Type = type;
			this.Message = message;
			this.ResultValue = resultValue;
		}
	}

}
