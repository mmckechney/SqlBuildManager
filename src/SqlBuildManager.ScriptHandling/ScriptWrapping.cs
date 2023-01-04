using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace SqlBuildManager.ScriptHandling
{
    public class ScriptWrapping
    {
        /// <summary>
        /// Modifies the raw text and attempts to convert it to 1 or more
        /// "ALTER TABLE... ALTER COLUMN" statements
        /// 
        /// Expects format like:
        /// CREATE TABLE [LineItemMapping] (
        ///		[ComponentValue] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
        ///		[ClassName] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
        /// </summary>
        /// <param name="rawScript">Incomming "CREATE TABLE" script </param>
        /// <param name="tableName">Name of the table used in the ALTERs</param>
        /// <param name="changedScript">The transformed script</param>
        /// <returns>String list of columns that were found and transformed</returns>
        public static List<string> TransformCreateTableToAlterColumn(string rawScript, string schema, string tableName, out string changedScript)
        {
            List<string> columnList = new List<string>();
            if (string.IsNullOrEmpty(tableName))
            {
                changedScript = rawScript;
                return columnList;
            }

            if (string.IsNullOrEmpty(rawScript))
            {
                changedScript = string.Empty;
                return columnList;
            }


            changedScript = "";
            string columnName = string.Empty;
            string[] lines = rawScript.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //Create table line regex
            //Regex table = new Regex(@"\bCREATE\b \bTABLE\b [A-Za-z0-9\[\]\. _]{1,}", RegexOptions.IgnoreCase);
            //Create table replacement
            Regex createReplace = new Regex(@"\bCREATE\b \bTABLE\b", RegexOptions.IgnoreCase);
            //Column Match
            Regex column = new Regex(@"^[[A-Za-z0-9_]{1,}]");
            //objectName value match 
            Regex objectName = new Regex(@"[A-Za-z0-9_\[\]]{1,}");


            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string alter = "ALTER TABLE [" + schema + "].[" + tableName + "] ALTER COLUMN ";
            string addCol = "ALTER TABLE [" + schema + "].[" + tableName + "] ADD ";
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("CREATE TABLE"))
                    continue;

                if (column.Match(lines[i].Trim(), 0) != null)
                {
                    columnName = (objectName.Match(lines[i]) != null) ? objectName.Match(lines[i]).Value.Replace("[", "").Replace("]", "") : "";
                    if (columnName.Length > 0 && columnName.ToUpper() != "GO" && columnName.ToUpper() != "PRIMARY")
                    {
                        columnList.Add(columnName);
                        string colLine;
                        if (lines[i].IndexOf("CONSTRAINT") > -1)
                            colLine = lines[i].Substring(0, lines[i].IndexOf("CONSTRAINT"));
                        else if (lines[i].Trim().EndsWith(","))
                            colLine = lines[i].Trim().Substring(0, lines[i].Trim().Length - 1);
                        else
                            colLine = lines[i].Trim();

                        sb.Append("IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = '" + tableName + "' AND TABLE_SCHEMA = '" + schema + "' AND COLUMN_NAME = '" + columnName + "')\r\n");

                        sb.Append("\t" + alter + colLine);
                        sb.Append("\r\nELSE\r\n");
                        sb.Append("\t" + addCol + colLine);
                        sb.Append("\r\nGO\r\n\r\n");
                    }

                }
            }
            changedScript = sb.ToString();
            return columnList;
        }
        /// <summary>
        /// Processes both the altering and adding of columns, but will also create the table
        /// if it isn't found and it will also delete any unreferened column.
        /// </summary>
        /// <param name="rawScript"></param>
        /// <returns></returns>
        public static string TransformCreateTableToResyncTable(string rawScript, string schema, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return rawScript;
            }
            string changedScript;
            List<string> columns = TransformCreateTableToAlterColumn(rawScript, schema, tableName, out changedScript);


            System.Text.StringBuilder addbitField = new System.Text.StringBuilder();
            addbitField.Append("IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + tableName + "' AND TABLE_SCHEMA = '" + schema + "')\r\n");
            addbitField.Append("\tCREATE TABLE [" + schema + "].[" + tableName + "]  (temp_will_be_removed bit NULL)\r\nGO\r\n\r\n");

            System.Text.StringBuilder removebitField = new System.Text.StringBuilder();
            removebitField.Append("IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = '" + tableName + "' AND TABLE_SCHEMA = '" + schema + "' AND COLUMN_NAME = 'temp_will_be_removed')\r\n");
            removebitField.Append("\tALTER TABLE [" + schema + "].[" + tableName + "] DROP COLUMN temp_will_be_removed\r\nGO\r\n\r\n");

            System.Text.StringBuilder sqlList = new System.Text.StringBuilder();
            sqlList.Append("--Remove any obsolete columns\r\n");
            sqlList.Append("DECLARE @sql varchar(1000)\r\n");
            sqlList.Append("DECLARE @col varchar(250)\r\n");
            sqlList.Append("DECLARE @FK varchar(250)\r\n");
            sqlList.Append("DECLARE @tmp TABLE(columnName varchar(250))\r\n");
            sqlList.Append("DECLARE @tmpFK TABLE(constraintName varchar(250))\r\n");
            sqlList.Append("INSERT INTO @tmp ");
            sqlList.Append("SELECT COLUMN_NAME FROM information_schema.columns WHERE TABLE_NAME = '" + tableName + "' AND TABLE_SCHEMA = '" + schema + "' AND COLUMN_NAME NOT IN (");
            for (int i = 0; i < columns.Count; i++)
                sqlList.Append("'" + columns[i] + "',");
            sqlList.Length = sqlList.Length - 1;
            sqlList.Append(")\r\n");
            sqlList.Append("DECLARE curRemove CURSOR FOR SELECT columnName FROM @tmp\r\n");
            sqlList.Append("OPEN curRemove\r\n");
            sqlList.Append("FETCH NEXT FROM curRemove INTO @col\r\n");
            sqlList.Append("WHILE @@FETCH_STATUS = 0\r\n");
            sqlList.Append("BEGIN\r\n");
            sqlList.Append("\tINSERT INTO @tmpFK SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE where column_name = @col AND Table_Name = '" + tableName + "' AND TABLE_SCHEMA = '" + schema + "'\r\n");
            sqlList.Append("\tDECLARE curFK CURSOR FOR SELECT constraintName FROM @tmpFK\r\n");
            sqlList.Append("\tOPEN curFK\r\n");
            sqlList.Append("\tFETCH NEXT FROM curFK INTO @FK\r\n");
            sqlList.Append("\tWHILE @@FETCH_STATUS = 0\r\n");
            sqlList.Append("\tBEGIN\r\n");
            sqlList.Append("\t\tSET @sql = 'IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME = '''+@FK+''') ALTER TABLE [" + schema + "].[" + tableName + "] DROP CONSTRAINT ' +@FK\r\n");
            sqlList.Append("\t\tPRINT @sql\r\n");
            sqlList.Append("\t\tEXEC(@sql)\r\n");
            sqlList.Append("\t\tFETCH NEXT FROM curFK INTO @FK\r\n");
            sqlList.Append("\tEND\r\n");
            sqlList.Append("\tCLOSE curFK\r\n");
            sqlList.Append("\tDEALLOCATE curFK \r\n");
            sqlList.Append("\tSET @sql = 'ALTER TABLE [" + schema + "].[" + tableName + "] DROP COLUMN ' +@col\r\n");
            sqlList.Append("\tPRINT @sql\r\n");
            sqlList.Append("\tEXEC(@sql)\r\n");
            sqlList.Append("\tDELETE FROM @tmpFK\r\n");
            sqlList.Append("\tFETCH NEXT FROM curRemove INTO @col\r\n");
            sqlList.Append("END\r\n");
            sqlList.Append("CLOSE curRemove\r\n");
            sqlList.Append("DEALLOCATE curRemove\r\n");
            sqlList.Append("GO\r\n\r\n");

            string val = addbitField + changedScript + "\r\n" + removebitField + sqlList.ToString();
            return val;


        }
        /// <summary>
        /// Extracts the table name and table schema from a table ALTER or CREATE script
        /// </summary>
        /// <param name="rawScript">Initial ALTER or CREATE Table script</param>
        /// <param name="schema">Database schema the table belongs to ("dbo" is defaulted if not specified)</param>
        /// <param name="tableName">Name of the tab</param>
        public static void ExtractTableNameFromScript(string rawScript, out string schema, out string tableName)
        {
            schema = "dbo";
            Regex regFindTable = new Regex(@"\bTABLE\b [A-Za-z0-9\[\]\._]{1,}", RegexOptions.IgnoreCase);
            Regex regTable = new Regex(@"\bTABLE\b", RegexOptions.IgnoreCase);
            if (regFindTable.Match(rawScript).Success)
            {
                string tmp = regFindTable.Match(rawScript).Value;
                tmp = regTable.Replace(tmp, "");
                tmp = tmp.Trim().Replace("[", "").Replace("]", "");
                if (tmp.IndexOf(".") > -1)
                {
                    schema = tmp.Split('.')[0];
                    tableName = tmp.Split('.')[1];
                }
                else
                    tableName = tmp;
            }
            else
            {
                tableName = "";
            }
        }
    }
}
