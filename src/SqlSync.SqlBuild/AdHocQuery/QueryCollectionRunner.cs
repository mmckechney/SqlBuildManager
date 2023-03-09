using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using SqlSync.Connection;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
namespace SqlSync.SqlBuild.AdHocQuery
{
    public class QueryCollectionRunner : IDisposable
    {
        private ConnectionData masterConnData = null;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string serverName;
        private string databaseName;
        private Policy pollyRetryPolicy = null;

        private string query;

        public string Query
        {
            get { return query; }
            set { query = value; }
        }
        private IList<QueryRowItem> appendData = new List<QueryRowItem>();

        public IList<QueryRowItem> AppendData
        {
            get { return appendData; }
            set { appendData = value; }
        }

        public string ResultsTempFile
        {
            get;
            set;
        }
        private QueryResultData results;

        private ReportType reportType;
        private string tempWorkingDirectory;
        private int scriptTimeout;
        private void ConfigurePollyRetryPolicies()
        {
            pollyRetryPolicy = Policy.Handle<Exception>().WaitAndRetry(
                                                        5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));

        }
        public QueryCollectionRunner(string serverName, string databaseName, string query, IList<QueryRowItem> appendData, ReportType reportType, string tempWorkingDirectory, int scriptTimeout, ConnectionData masterConnData)
        {
            this.databaseName = databaseName;
            this.serverName = serverName;
            this.query = query;
            this.appendData = appendData;
            this.reportType = reportType;
            this.tempWorkingDirectory = tempWorkingDirectory;
            this.scriptTimeout = scriptTimeout;
            this.masterConnData = masterConnData;
        }

        public Task<(int, string)> CollectQueryData()
        {
            var coll = Task<(int, string)>.Run(() =>
            {
                int queryResult = 0;
                ConfigurePollyRetryPolicies();
                string errorMessage = string.Empty;
                if (QueryCollectionRunnerUpdate != null)
                    QueryCollectionRunnerUpdate(this, new QueryCollectionRunnerUpdateEventArgs(serverName, databaseName, "Starting"));

                ConnectionData connData = new ConnectionData(serverName, databaseName);
                if (masterConnData.AuthenticationType == AuthenticationType.AzureADPassword || masterConnData.AuthenticationType == AuthenticationType.Password)
                {
                    connData.UserId = masterConnData.UserId;
                    connData.Password = masterConnData.Password;
                    connData.AuthenticationType = masterConnData.AuthenticationType;
                }
                else
                {
                    connData.AuthenticationType = masterConnData.AuthenticationType;
                }
                connData.ScriptTimeout = scriptTimeout;
                SqlConnection conn = ConnectionHelper.GetConnection(connData);
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.Text;


                results = new QueryResultData(serverName, databaseName);
                results.QueryAppendData = (List<QueryRowItem>)AppendData;
                int rowCount = 0;
                try
                {
                    pollyRetryPolicy.Execute(() =>
                        {
                            if (conn.State == ConnectionState.Closed)
                                conn.Open();

                            rowCount = 0;

                            string columnName = string.Empty;
                            using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                            {

                                int fieldCount = 0;
                                //Add column definitions...
                                DataTable tbl = reader.GetSchemaTable();
                                int columnCount = tbl.Rows.Count;
                                for (int i = 0; i < columnCount; i++)
                                {
                                    columnName = tbl.Rows[i]["ColumnName"].ToString();
                                    if (columnName.Length == 0)
                                        columnName = "Column " + (i + 1).ToString();
                                    else if (results.ColumnDefinition.ContainsKey(columnName))
                                        columnName = columnName + (i + 1).ToString();

                                    results.ColumnDefinition.Add(columnName, i.ToString());
                                }


                                while (reader.Read())
                                {
                                    fieldCount = reader.FieldCount;
                                    Result tmp = new Result();
                                    for (int i = 0; i < fieldCount; i++)
                                    {
                                        if (reader[i] == DBNull.Value)
                                            tmp.Add(i.ToString(), "NULL");
                                        else
                                            tmp.Add(i.ToString(), reader[i].ToString());
                                    }
                                    results.Results.Add(tmp);
                                    rowCount++;

                                    if (rowCount % 10000 == 0)
                                        DumpResults();
                                }
                                reader.Close();
                                reader.Dispose();
                            }
                        });

                    results.RowCount = rowCount;
                }
                catch (OutOfMemoryException omExe)
                {
                    log.LogError(omExe, $"Ran out of memory running Query: {query} on {connData.SQLServerName}.{connData.DatabaseName}");
                    queryResult = -1;
                }
                catch (Exception exe)
                {
                    errorMessage = exe.Message;
                    log.LogError(exe, $"Error Executing Query: {query}");
                    Result r = new Result();
                    r.Add("", "** Execution Error: " + errorMessage);
                    results.Results.Add(r);
                    queryResult = -2;
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                }

                DumpResults();
                string combined = MergeDumpFiles();
                SerializeToTempFile(combined);
                results = null;
                return (queryResult, this.ResultsTempFile);
                
            });

            return coll;
            
        }

        #region Buffering for very large results

        private List<string> dumpFiles = new List<string>();
        /// <summary>
        /// Takes the sub-set of results and puts them into a temporary file.
        /// This is called every 10,000 rows to keep memory usage under control.
        /// </summary>
        private void DumpResults()
        {
            string tmpDump = Path.Combine(tempWorkingDirectory, String.Format("Dump-{0}.txt", Guid.NewGuid().ToString()));
            if (reportType == ReportType.CSV)
            {
                File.WriteAllText(tmpDump, results.GetRowValuesCsvString());
            }
            else
            {
                using (XmlTextWriter sw = new XmlTextWriter(tmpDump, Encoding.UTF8))
                {
                    sw.Formatting = Formatting.Indented;
                    System.Xml.Serialization.XmlSerializer xmlS = new System.Xml.Serialization.XmlSerializer(typeof(List<Result>));
                    xmlS.Serialize(sw, results.Results);
                    sw.Flush();
                    sw.Close();
                }
            }
            dumpFiles.Add(tmpDump);
            results.Results = new List<Result>();
        }
        /// <summary>
        /// Once an extract is complete, each dump file that was created gets merged into a single file representing this set of data.
        /// </summary>
        /// <returns></returns>
        private string MergeDumpFiles()
        {
            string tmpCombined = Path.Combine(tempWorkingDirectory, String.Format("Merge-{0}.txt", Guid.NewGuid().ToString()));
            if (reportType == ReportType.CSV)
            {
                File.WriteAllText(tmpCombined, results.GetColumnsCsvString() + "\r\n");
                foreach (string partial in dumpFiles)
                {
                    File.AppendAllText(tmpCombined, File.ReadAllText(partial));
                    File.Delete(partial);
                }
            }
            else
            {
                string tempLine = null;
                using (StreamWriter sw = new StreamWriter(tmpCombined, true))
                {
                    foreach (string partial in dumpFiles)
                    {
                        using (StreamReader sr = new StreamReader(partial))
                        {
                            while (sr.Peek() > 0)
                            {
                                tempLine = sr.ReadLine();
                                if (tempLine.StartsWith("<?xml", StringComparison.InvariantCultureIgnoreCase) ||
                                    tempLine.StartsWith("<ArrayOfResult", StringComparison.InvariantCultureIgnoreCase) ||
                                    tempLine.StartsWith("</ArrayOfResult>"))
                                    continue;

                                sw.WriteLine(tempLine);
                            }
                        }
                        File.Delete(partial);
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            return tmpCombined;

        }

        #endregion

        public event QueryCollectionRunnerUpdateEventHandler QueryCollectionRunnerUpdate;
        public delegate void QueryCollectionRunnerUpdateEventHandler(object sender, QueryCollectionRunnerUpdateEventArgs e);

        /// <summary>
        /// Takes the merged dump file and serializes it in the proper ReportType format
        /// </summary>
        /// <param name="combinedFile">The name of the merged dump file.</param>
        private void SerializeToTempFile(string combinedFile)
        {
            if (reportType == ReportType.CSV)
            {
                ResultsTempFile = combinedFile;
                return;
            }
            //Write the results shell to a file... but remember, we've dumped all the data, that needs to be re-integrated...
            string tmpShell = Path.Combine(tempWorkingDirectory, String.Format("Shell-{0}.txt", Guid.NewGuid().ToString()));
            using (XmlTextWriter sw = new XmlTextWriter(tmpShell, Encoding.UTF8))
            {
                sw.Formatting = Formatting.Indented;
                System.Xml.Serialization.XmlSerializer xmlS = new System.Xml.Serialization.XmlSerializer(typeof(QueryResultData));
                xmlS.Serialize(sw, results);
                sw.Flush();
                sw.Close();
            }

            ResultsTempFile = Path.Combine(tempWorkingDirectory, String.Format("Combined-{0}.txt", Guid.NewGuid().ToString()));
            string tmpLine = null;
            using (StreamWriter sw = new StreamWriter(ResultsTempFile))
            {
                using (StreamReader srShell = new StreamReader(tmpShell))
                {
                    while (srShell.Peek() > 0)
                    {
                        tmpLine = srShell.ReadLine();
                        if (tmpLine.Trim().StartsWith("<Results", StringComparison.InvariantCultureIgnoreCase))
                            break;
                        else
                            sw.WriteLine(tmpLine);

                    }
                    sw.WriteLine("<Results>");
                    using (StreamReader srCombined = new StreamReader(combinedFile))
                    {
                        while (srCombined.Peek() > 0)
                            sw.WriteLine(srCombined.ReadLine());
                    }
                    sw.WriteLine("</Results>");
                    while (srShell.Peek() > 0)
                    {
                        sw.WriteLine(srShell.ReadLine());
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            File.Delete(combinedFile);
            File.Delete(tmpShell);


        }

        #region IDisposable Members

        /// <summary>
        /// Used to make sure the Runner cleans up after itself.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (File.Exists(ResultsTempFile))
                {
                    File.Delete(ResultsTempFile);
                }
            }
            catch
            {
            }
        }

        #endregion
    }
    public class QueryCollectionRunnerUpdateEventArgs
    {
        public readonly string Server;
        public readonly string Database;
        public readonly string Message;
        public QueryCollectionRunnerUpdateEventArgs(string server, string database, string message)
        {
            Server = server;
            Database = database;
            Message = message;
        }
    }
}
