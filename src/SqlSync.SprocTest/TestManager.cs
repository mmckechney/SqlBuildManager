using Microsoft.Data.SqlClient;
using SqlSync.Connection;
using SqlSync.SprocTest.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
namespace SqlSync.SprocTest
{
    public class TestManager
    {
        string fileName = string.Empty;
        SprocTest.Configuration.Database cfg = null;
        SqlSync.Connection.ConnectionData connData = null;
        private TestManager()
        {
        }
        public TestManager(SprocTest.Configuration.Database cfg, ConnectionData connData) : this()
        {
            this.cfg = cfg;
            this.connData = connData;
        }
        public TestManager(string fileName, string server) : this()
        {
            this.fileName = fileName;
            connData = new ConnectionData();
            connData.SQLServerName = server;
            connData.AuthenticationType = AuthenticationType.Windows;
            connData.ScriptTimeout = 100;
        }


        public void StartTests(BackgroundWorker bgWorker, DoWorkEventArgs workArgs)
        {
            if (cfg == null)
                cfg = ReadConfiguration(fileName);

            connData.DatabaseName = cfg.Name;
            for (int i = 0; i < cfg.StoredProcedure.Length; i++)
            {
                if (!TestStoredProcedure(cfg.StoredProcedure[i], bgWorker, workArgs))
                    break;
            }
        }

        private bool TestStoredProcedure(SqlSync.SprocTest.Configuration.StoredProcedure sp, BackgroundWorker bgWorker, DoWorkEventArgs workArgs)
        {
            SqlConnection conn = Connection.ConnectionHelper.GetConnection(connData);
            if (sp.TestCase == null)
                return true;

            for (int i = 0; i < sp.TestCase.Length; i++)
                if (sp.TestCase[i].SelectedForRun)
                    if (!RunTestCase(ref conn, sp.Name, sp.TestCase[i], bgWorker, workArgs))
                        return false;

            return true;
        }
        public bool RunTestCase(ref SqlConnection conn, string spName, TestCase testCase, BackgroundWorker bgWorker, DoWorkEventArgs workArgs)
        {
            string message = "";
            bool passed = false;

            SqlCommand cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            string paramMessage;
            List<List<SqlParameter>> paramCols = GetParameterListsForTest(testCase, ref conn, out paramMessage);
            if (paramCols == null && paramMessage.Length > 0)
            {
                TestResult args = new TestResult(spName, testCase.Name, paramMessage, false, testCase, "");
                bgWorker.ReportProgress(1, args);
                return false;

            }
            if (paramCols.Count == 0)
            {
                TestResult args = new TestResult(spName, testCase.Name, "Unable to retrieve parameter data.", false, testCase, "");
                bgWorker.ReportProgress(1, args);
                return false;

            }
            for (int i = 0; i < paramCols.Count; i++)
            {
                try
                {
                    if (bgWorker.CancellationPending)
                    {
                        workArgs.Cancel = true;
                        return false;
                    }

                    cmd.Parameters.Clear();
                    for (int j = 0; j < paramCols[i].Count; j++)
                        cmd.Parameters.Add(paramCols[i][j]);


                    passed = false;


                    switch (testCase.ExecuteType)
                    {
                        case SqlSync.SprocTest.Configuration.ExecuteType.NonQuery:
                            passed = ExecuteNonQueryTest(cmd, testCase.ExpectedResult, out message);
                            break;
                        case SqlSync.SprocTest.Configuration.ExecuteType.Scalar:
                            passed = ExecuteScalarTest(cmd, testCase.ExpectedResult, out message);
                            break;
                        case SqlSync.SprocTest.Configuration.ExecuteType.ReturnData:
                        default:
                            passed = ExecuteDataSetTest(cmd, testCase.ExpectedResult, out message);
                            break;
                    }

                    TestResult resultArgs = new TestResult(spName, testCase.Name, message, passed, testCase, GenerateTestSql(spName, paramCols[i], conn.Database));
                    bgWorker.ReportProgress(1, resultArgs);
                }
                catch (Exception exe)
                {
                    message = exe.Message;
                    TestResult resultArgs = new TestResult(spName, testCase.Name, message, false, testCase, GenerateTestSql(spName, paramCols[i], conn.Database));
                    bgWorker.ReportProgress(1, resultArgs);
                }

            }
            return true;

        }

        #region .: Parameter Generation :.
        private List<List<SqlParameter>> GetParameterListsForTest(TestCase tC, ref SqlConnection conn, out string message)
        {
            message = "";
            List<List<SqlParameter>> paramList = new List<List<SqlParameter>>();
            if (tC.Parameter != null)
            {
                for (int i = 0; i < tC.Parameter.Length; i++)
                {
                    if (tC.Parameter[i].UseAsQuery == false || tC.Parameter[i].Value.Length == 0 || tC.Parameter[i].Value.ToUpper() == "NULL")
                    {
                        List<SqlParameter> singleVal = new List<SqlParameter>();
                        SqlParameter param;
                        if (tC.Parameter[i].Value.ToUpper() != "NULL")
                            param = new SqlParameter(tC.Parameter[i].Name, tC.Parameter[i].Value);
                        else
                            param = new SqlParameter(tC.Parameter[i].Name, DBNull.Value);

                        singleVal.Add(param);
                        paramList.Add(singleVal);
                    }
                    else
                    {
                        string valueMessage;
                        List<string> values = GetQueryBasedValues(tC.Parameter[i].Value, ref conn, out valueMessage);
                        if (valueMessage.Length > 0)
                        {
                            message = valueMessage;
                            return null;
                        }
                        List<SqlParameter> dynP = new List<SqlParameter>();
                        for (int j = 0; j < values.Count; j++)
                            dynP.Add(new SqlParameter(tC.Parameter[i].Name, values[j]));

                        paramList.Add(dynP);
                    }

                }
            }

            //If no parameters, return a shell collection
            if (paramList.Count == 0)
            {
                List<List<SqlParameter>> empty = new List<List<SqlParameter>>();
                empty.Add(new List<SqlParameter>());
                return empty;
            }


            //Simplest case.. only one collection...
            if (paramList.Count == 1)
            {
                List<List<SqlParameter>> expanded = new List<List<SqlParameter>>();
                for (int i = 0; i < paramList[0].Count; i++)
                {
                    List<SqlParameter> t = new List<SqlParameter>();
                    t.Add(paramList[0][i]);
                    expanded.Add(t);
                }
                return expanded;
            }

            //It's easier if you get the longest arrays first...
            SqlParamListSorter sorter = new SqlParamListSorter();
            paramList.Sort((IComparer<List<SqlParameter>>)sorter);
            paramList.Reverse();

            if (paramList.Count >= 2)
            {
                List<List<SqlParameter>> blended = new List<List<SqlParameter>>();
                List<SqlParameter> first = paramList[0];
                List<SqlParameter> second = paramList[1];
                //Loop through the first 2 to get started
                for (int i = 0; i < first.Count; i++)
                {
                    for (int j = 0; j < second.Count; j++)
                    {
                        List<SqlParameter> tmp = new List<SqlParameter>();
                        tmp.Add(first[i]);
                        tmp.Add(second[j]);
                        blended.Add(tmp);
                    }
                }
                if (paramList.Count == 2)
                {
                    return blended;
                }

                for (int i = 2; i < paramList.Count; i++)
                {
                    blended = AddCommandParameterValue(paramList[i], blended);
                }

                return blended;

            }
            return null;

        }
        private List<List<SqlParameter>> AddCommandParameterValue(List<SqlParameter> nextSet, List<List<SqlParameter>> blended)
        {
            List<List<SqlParameter>> extended = new List<List<SqlParameter>>();

            for (int i = 0; i < nextSet.Count; i++)
            {
                for (int j = 0; j < blended.Count; j++)
                {
                    List<SqlParameter> tmp = new List<SqlParameter>();
                    tmp.AddRange(blended[j]);
                    tmp.Add(nextSet[i]);
                    extended.Add(tmp);
                }
            }
            return extended;
        }
        private List<string> GetQueryBasedValues(string query, ref SqlConnection conn, out string message)
        {
            message = "";
            try
            {
                List<string> lst = new List<string>();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.Text;
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        lst.Add(reader[0].ToString());
                    }
                    reader.Close();
                }
                return lst;
            }
            catch (SqlException exe)
            {
                message = exe.Message + ", " + exe.LineNumber + ", " + exe.ErrorCode;
                return null;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }
        }
        #endregion

        #region .: Execution Methods :.
        private bool ExecuteDataSetTest(SqlCommand cmd, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        {
            message = "";
            List<string> messages = new List<string>();
            bool passed = true;
            bool outputPassed = true;
            bool rowCountPass = true;
            bool columnCountPass = true;


            try
            {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.Transaction = cmd.Connection.BeginTransaction();
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                cmd.Transaction.Rollback();
                if (ds.Tables == null || ds.Tables.Count == 0)
                {
                    outputPassed = false;
                    messages.Add("No DataTables were populated.");
                }
                else
                {
                    string countMessage;
                    rowCountPass = CheckRowCount(ds.Tables[0].Rows.Count, expected, out countMessage);
                    messages.Add(countMessage);

                    string columnMessage;
                    columnCountPass = CheckColumnCount(ds.Tables[0], expected, out columnMessage);
                    messages.Add(columnMessage);


                    if (expected.OutputResult != null && expected.OutputResult.Length > 0)
                    {
                        string outputMessage;

                        for (int i = 0; i < expected.OutputResult.Length; i++)
                        {
                            if (expected.OutputResult[i].RowNumberSpecified && expected.OutputResult[i].RowNumber > ds.Tables[0].Rows.Count)
                            {
                                passed = false;
                                messages.Add("Output result Row Number exceeds actual row count. Expected " + expected.OutputResult[i].RowNumber.ToString() + ", Returned " + ds.Tables[i].Rows.Count.ToString());
                                continue;
                            }

                            //Row should be set in a non-developer centric 1 based value, vs. developer 0 based value
                            int row = (expected.OutputResult[i].RowNumberSpecified) ? expected.OutputResult[i].RowNumber : 1;
                            if (row == 0) row = 1;
                            try
                            {

                                if (!CheckOutputValue(ds.Tables[0].Rows[row - 1], expected.OutputResult[i], row, out outputMessage))
                                    outputPassed = false;
                                messages.Add(outputMessage);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                messages.Add("Expected output row #" + row + " does not exist");
                                outputPassed = false;
                            }
                        }
                    }
                }

                if (!rowCountPass || !outputPassed || !columnCountPass)
                    passed = false;

                for (int i = 0; i < messages.Count; i++)
                {
                    if (messages[i].Trim().Length > 0)
                        message += messages[i].Trim() + "\r\n";
                }

            }
            catch (SqlException exe)
            {
                return CheckSqlException(expected, exe, out message);
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                passed = false;

            }

            return passed;
        }
        private bool ExecuteNonQueryTest(SqlCommand cmd, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        {

            bool passed = true;

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Transaction = cmd.Connection.BeginTransaction();
                int rowCount = cmd.ExecuteNonQuery();
                cmd.Transaction.Rollback();

                //string countMessage;
                passed = CheckRowCount(rowCount, expected, out message);
                return passed;


            }
            catch (SqlException exe)
            {
                return CheckSqlException(expected, exe, out message);
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return false;
            }
            finally
            {
                if (cmd.Transaction != null)
                    cmd.Transaction.Rollback();

                cmd.Connection.Close();
            }

        }
        private bool ExecuteScalarTest(SqlCommand cmd, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        {
            bool passed = true;
            message = "";
            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Transaction = cmd.Connection.BeginTransaction();
                object val = cmd.ExecuteScalar();
                cmd.Transaction.Rollback();

                if (expected.OutputResult != null && expected.OutputResult.Length > 0)
                {
                    if (val == null || expected.OutputResult[0].Value.ToString().Trim().ToLower() != val.ToString().Trim().ToLower())
                    {
                        message = "Invalid Value Returned for Scalar. Expected: '" + expected.OutputResult[0].Value + "', Retrieved: '" + ((val == null) ? "NULL" : val.ToString()) + "'";
                        passed = false;
                    }
                    else
                        message += "Scalar Value Passed. Expected: '" + expected.OutputResult[0].Value + "', Retrieved: '" + ((val == null) ? "NULL" : val.ToString()) + "'";
                }

            }
            catch (SqlException exe)
            {
                return CheckSqlException(expected, exe, out message);
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                passed = false;

            }

            return passed;
        }
        //private bool ExecuteDataReaderTest(SqlCommand cmd, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        //{
        //    bool passed = true;
        //    message = "";
        //    string columnMessage = "";
        //    List<string> messages = new List<string>();
        //    int rowCount = 0;
        //    bool columnCountPassed = true;
        //    bool outputPassed = false;
        //    try
        //    {
        //        if (cmd.Connection.State == ConnectionState.Closed)
        //            cmd.Connection.Open();

        //        cmd.Transaction = cmd.Connection.BeginTransaction();

        //        int x = 0;
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            if (reader.HasRows)
        //            {
        //                while (reader.Read())
        //                {

        //                    if (x == 0)
        //                    {
        //                        columnCountPassed = CheckColumnCount(reader.FieldCount, expected, out columnMessage);
        //                        if (!columnCountPassed)
        //                            messages.Add(columnMessage);
        //                    }


        //                    if (expected.OutputResult != null && expected.OutputResult.Length > 0)
        //                    {
        //                        string outputMessage;

        //                        for (int i = 0; i < expected.OutputResult.Length; i++)
        //                        {
        //                            if (expected.OutputResult[i].RowNumberSpecified && expected.OutputResult[i].RowNumber > rowCount)
        //                            {
        //                                passed = false;
        //                                messages.Add("Output result Row Number exceeds actual row count. Expected " + expected.OutputResult[i].RowNumber.ToString() + ", Returned " + rowCount.ToString());
        //                                continue;
        //                            }

        //                            int row = (expected.OutputResult[i].RowNumberSpecified) ? expected.OutputResult[i].RowNumber : 0;
        //                            try
        //                            {
        //                                if (rowCount == row)
        //                                    if (!CheckOutputValue(reader, expected.OutputResult[i], rowCount, out outputMessage))
        //                                    {
        //                                        outputPassed = false;
        //                                        messages.Add(outputMessage);
        //                                    }
        //                            }
        //                            catch (IndexOutOfRangeException)
        //                            {
        //                                messages.Add("Expected output row #" + row + " does not exist");
        //                                outputPassed = false;
        //                            }
        //                        }
        //                    }
        //                    rowCount++;
        //                }
        //            }

        //            reader.Close();
        //            cmd.Transaction.Rollback();
        //        }

        //        string countMessage;
        //        bool rowCountPass = CheckRowCount(rowCount, expected, out countMessage);
        //        messages.Add(countMessage);

        //        if (!rowCountPass || !outputPassed || !columnCountPassed)
        //        {
        //            passed = false;
        //            for (int i = 0; i < messages.Count; i++)
        //            {
        //                if (messages[i].Trim().Length > 0)
        //                    message += messages[i].Trim() + "\r\n";
        //            }
        //        }
        //    }
        //    catch (SqlException exe)
        //    {
        //        passed = CheckSqlException(expected, exe, out message);
        //    }
        //    catch (Exception ex)
        //    {
        //        message = ex.ToString();
        //        passed = false;
        //    }
        //    finally
        //    {
        //        if (cmd.Transaction != null)
        //            cmd.Transaction.Rollback();

        //        if (cmd.Connection.State == ConnectionState.Open)
        //            cmd.Connection.Close();
        //    }
        //    return passed;
        //}
        #endregion

        #region .: Result and Output Checks :.
        private bool CheckSqlException(ExpectedResult expected, SqlException exe, out string message)
        {
            message = exe.Message + " Line Number:" + exe.LineNumber.ToString() + " Error Code:" + exe.ErrorCode.ToString();
            switch (expected.ResultType)
            {
                case ResultType.PKViolation:
                    if (exe.Message.IndexOf("Primary Key", 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        message = "Primary Key Violation Expected. " + exe.Message;
                        return true;
                    }
                    else
                        return false;

                case ResultType.FKViolation:
                    if (exe.Message.IndexOf("Foreign Key", 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        message = "Foreign Key Violation Expected. " + exe.Message;
                        return true;
                    }
                    else
                        return false;

                case ResultType.GenericSqlException:
                    message = "SqlException Expected. " + exe.Message;
                    return true;
                case ResultType.Success:
                default:
                    return false;
            }
        }
        private bool CheckRowCount(int rows, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        {
            message = "";
            bool passed = true;
            if (expected.RowCountSpecified)
            {
                if (!expected.RowCountOperatorSpecified)
                    expected.RowCountOperator = SqlSync.SprocTest.Configuration.RowCountOperator.EqualTo;

                switch (expected.RowCountOperator)
                {

                    case SqlSync.SprocTest.Configuration.RowCountOperator.GreaterThan:
                        if (rows <= expected.RowCount)
                        {
                            message += "Invalid Row Count. Expected Greater Than " + expected.RowCount.ToString() + ", Retrieved " + rows;
                            passed = false;
                        }
                        else
                            message += " Row Count Passed. Expected Greater Than " + expected.RowCount.ToString() + ", Retrieved " + rows;
                        break;
                    case SqlSync.SprocTest.Configuration.RowCountOperator.LessThan:
                        if (rows >= expected.RowCount)
                        {
                            message += "Invalid Row Count. Expected Less Than " + expected.RowCount.ToString() + ", Retrieved " + rows;
                            passed = false;
                        }
                        else
                            message += " Row Count Passed. Expected Less Than " + expected.RowCount.ToString() + ", Retrieved " + rows;
                        break;
                    case SqlSync.SprocTest.Configuration.RowCountOperator.EqualTo:
                    default:
                        if (rows != expected.RowCount)
                        {
                            message += "Invalid Row Count. Expected " + expected.RowCount.ToString() + ", Retrieved " + rows;
                            passed = false;
                        }
                        else
                            message += "Row Count Passed. Expected " + expected.RowCount.ToString() + ", Retrieved " + rows;

                        break;
                }
            }
            return passed;
        }
        private bool CheckColumnCount(int columnCount, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        {
            if (expected.ColumnCountSpecified == false)
            {
                message = "";
                return true;
            }
            if (columnCount != expected.ColumnCount)
            {
                message = "Invalid Column Count. Expected " + expected.ColumnCount + ", Retrieved " + columnCount;
                return false;
            }
            else
            {
                message = "Column Count Passed. Expected " + expected.ColumnCount + ", Retrieved " + columnCount;
                return true;
            }
        }
        private bool CheckColumnCount(DataTable tbl, SqlSync.SprocTest.Configuration.ExpectedResult expected, out string message)
        {
            return CheckColumnCount(tbl.Columns.Count, expected, out message);
        }
        private bool CheckOutputValue(DataRow row, SqlSync.SprocTest.Configuration.OutputResult output, int rowNum, out string message)
        {
            message = "";
            bool passed = true;
            try
            {
                int colNumber;
                if (int.TryParse(output.ColumnName, out colNumber))
                {
                    if (row[colNumber].ToString().ToLower().Trim() != output.Value.ToString().ToLower().Trim())
                    {
                        message = "Invalid Value Returned for Column #" + output.ColumnName + ", Row# " + rowNum + ". Expected '" + output.Value.ToString() + "', Retrieved '" + row[output.ColumnName].ToString() + "'";
                        passed = false;
                    }
                }
                else if (row[output.ColumnName].ToString().ToLower().Trim() != output.Value.ToString().ToLower().Trim())
                {
                    message = "Invalid Value Returned for Column " + output.ColumnName + ", Row# " + rowNum + ". Expected '" + output.Value.ToString() + "', Retrieved '" + row[output.ColumnName].ToString() + "'";
                    passed = false;
                }
            }
            catch (SqlException exe)
            {
                message = "Sql Error Eetrieving expect column " + output.ColumnName + ". " + exe.Message + " Line Number:" + exe.LineNumber.ToString() + " Error Code:" + exe.ErrorCode.ToString();
                passed = false;
            }
            catch (Exception ex)
            {
                message = "Error Eetrieving expect column " + output.ColumnName + ". " + ex.ToString();
                passed = false;

            }
            if (passed)
                message = "Value Check Passed. Column: " + output.ColumnName + ", Row# " + rowNum + ". Expected '" + output.Value.ToString() + "', Retrieved '" + row[output.ColumnName].ToString() + "'";

            return passed;
        }
        //private bool CheckOutputValue(SqlDataReader reader , SqlSync.SprocTest.Configuration.OutputResult output, int rowNum, out string message)
        //{
        //    message = "";
        //    bool passed = true;
        //        try
        //        {
        //            if(reader[output.ColumnName].ToString().ToLower() != output.Value.ToString().ToLower())
        //            {
        //                message = "Invalid Value Returned for Column " + output.ColumnName + ", Row# " + rowNum + ". Expected " + output.Value.ToString() + ", Retrieved " + reader[output.ColumnName].ToString();
        //                passed = false;
        //            }
        //        }
        //        catch(SqlException exe)
        //        {
        //            message = "Sql Error Eetrieving expect column " + output.ColumnName + ". " + exe.Message + " Line Number:" + exe.LineNumber.ToString() + " Error Code:" + exe.ErrorCode.ToString();
        //            passed = false;
        //        }
        //        catch(Exception ex)
        //        {
        //           message = "Error Eetrieving expect column "+output.ColumnName+". "+ex.ToString();
        //           passed = false;

        //        }
        //    return passed;
        //}
        #endregion

        public static Database ReadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Unable to locate configuration file.", fileName);
            }

            Database cfg = null;
            using (StreamReader sr = new StreamReader(fileName))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SprocTest.Configuration.Database));
                    object obj = serializer.Deserialize(sr);
                    cfg = (SprocTest.Configuration.Database)obj;
                }
                catch (Exception exe)
                {
                    throw new ApplicationException("Unable to deserialize the test configuration file at: " + fileName + "\r\n" + exe.ToString());
                }

            }
            if (cfg == null)
                throw new ApplicationException("Unable to deserialize the test configuration file at: " + fileName);

            cfg.FileName = fileName;
            return cfg;
        }

        public static void GetConfigurationFromScript(string script, ConnectionData connData, out string storedProcName, out Dictionary<string, string> parameterValues)
        {
            Regex regExec = new Regex("exec", RegexOptions.IgnoreCase);
            script = script.Trim();

            ///Remove any exec directive
            Match execMtch = regExec.Match(script, 0, 5);
            if (execMtch.Success)
                script = script.Substring(execMtch.Length);
            script = script.Trim();

            //Stored proc name should now be first
            int firstSpace = script.IndexOf(' ', 0);
            storedProcName = script.Substring(0, firstSpace).Trim();

            List<string> paramNames = new List<string>();
            SqlParameterCollection spParams = DbInformation.InfoHelper.GetStoredProcParameters(storedProcName, connData);
            if (spParams != null)
                for (int i = 0; i < spParams.Count; i++)
                    paramNames.Add(spParams[i].ParameterName);

            parameterValues = ParseParameterValuesFromScript(storedProcName, paramNames, script);

        }
        public static Dictionary<string, string> ParseParameterValuesFromScript(string sprocName, List<string> parameterNames, string script)
        {
            Dictionary<string, string> paramValues = new Dictionary<string, string>();
            Regex regExec = new Regex("exec", RegexOptions.IgnoreCase);
            Regex regSP = new Regex(sprocName, RegexOptions.IgnoreCase);
            script = script.Trim();

            ///Remove any exec directive
            Match execMtch = regExec.Match(script, 0, 5);
            if (execMtch.Success)
                script = script.Substring(execMtch.Length);

            //Remove the sproc name
            Match spMatch = regSP.Match(script);
            if (spMatch.Success)
                script = regSP.Replace(script, "", 1, 0);

            script = script.Trim();

            //All we should have left is a CSV list of values or a CSV list of name/value pairs.
            if (script.IndexOf('=') > -1)
            {
                /*Assume we have something like this
                 *@EmailQueueID='4E4CB254-3D1E-4A7C-8331-50314A012CD8',@EmailNotificationID='0FA47D97-D5DB-49AC-BC42-F1A429A23830',@EntityID='D0BB2422-CB18-4DAE-9255-ED6244888567',@EntityType='',@NotificationID='NotifyAVRM',@EmailAddress='my@mydomain.com',@Subject='',@Message='',@QueueDate='Jan 16 2007  9:46:23:047AM',@Processed=0,@ProcessedDate=NULL,@RetryCount=0,@UserNoAccept=0
                 */
                string[] keyVal = script.Split(',');
                for (int i = 0; i < keyVal.Length; i++)
                {
                    string[] split = keyVal[i].Split(new char[] { '=' }, 2, StringSplitOptions.None);
                    if (split.Length == 2)
                    {
                        split[1].Trim();
                        if (split[1].StartsWith("'"))
                            split[1] = split[1].Substring(1);
                        if (split[1].EndsWith("'"))
                            split[1] = split[1].Substring(0, split[1].Length - 1);

                        paramValues.Add(split[0].Trim(), split[1]);
                    }
                }
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    if (!paramValues.ContainsKey(parameterNames[i]))
                        paramValues.Add(parameterNames[i], "NULL");
                }
            }
            else
            {
                string[] vals = script.Split(',');
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    if (vals.Length >= i + 1)
                    {
                        if (vals[i].StartsWith("'"))
                            vals[i] = vals[i].Substring(1);
                        if (vals[i].EndsWith("'"))
                            vals[i] = vals[i].Substring(0, vals[i].Length - 1);
                        paramValues.Add(parameterNames[i], vals[i]);
                    }
                    else
                        paramValues.Add(parameterNames[i], "NULL");
                }
            }

            return paramValues;
        }
        public static string GenerateTestSql(string storedProcedureName, string databaseName, Configuration.TestCase testCase)
        {
            System.Text.StringBuilder sb = new StringBuilder("exec " + databaseName + "." + storedProcedureName + " ");
            if (testCase.Parameter != null)
            {
                for (int i = 0; i < testCase.Parameter.Length; i++)
                {
                    sb.Append(testCase.Parameter[i].Name + "=");
                    if (testCase.Parameter[i].Value.ToUpper().Trim() == "NULL")
                        sb.Append("NULL,");
                    else
                        sb.Append("'" + testCase.Parameter[i].Value + "',");
                }
                sb.Length = sb.Length - 1;
            }
            return sb.ToString();

        }
        private static string GenerateTestSql(string storedProcedureName, List<SqlParameter> paramList, string databaseName)
        {
            System.Text.StringBuilder sb = new StringBuilder("exec " + databaseName + "." + storedProcedureName + " ");
            {
                for (int i = 0; i < paramList.Count; i++)
                {
                    sb.Append(paramList[i].ParameterName + "=");
                    if (paramList[i].Value == DBNull.Value)
                        sb.Append("NULL,");
                    else
                        sb.Append("'" + paramList[i].Value + "',");
                }
                sb.Length = sb.Length - 1;
            }
            return sb.ToString();

        }
        public static string GetDefaultValueForSqlDbType(SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.BigInt:
                case SqlDbType.Binary:
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Int:
                case SqlDbType.Money:
                case SqlDbType.Real:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                case SqlDbType.SmallMoney:
                    return "0";

                case SqlDbType.Bit:
                    return "0";

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    return string.Empty;

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    return DateTime.Now.ToString();

                case SqlDbType.UniqueIdentifier:
                    return Guid.NewGuid().ToString();

                case SqlDbType.Timestamp:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                case SqlDbType.Variant:
                default:
                    return string.Empty;
            }





        }

        #region .: Event and Handler :.
        public class TestResult : EventArgs
        {
            public string StoredProcedureName;
            public string TestCaseName;
            public string StatusMessage;
            public bool Passed;
            public TestCase TestCase;
            public string ExecutedSql;
            public TestResult(string storedProcedureName, string testCaseName, string statusMessage, bool passed, TestCase TestCase, string executedSql)
            {
                StoredProcedureName = storedProcedureName;
                TestCaseName = testCaseName;
                StatusMessage = statusMessage;
                Passed = passed;
                this.TestCase = TestCase;
                ExecutedSql = executedSql;
            }
            /// <summary>
            /// Needed for serialization
            /// </summary>
            public TestResult()
            {
            }
        }
        #endregion
    }
}
