using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
namespace SqlSync.SqlBuild.AdHocQuery
{
    public class QueryCollector
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MultiDbData multiDbData;
        private static SyncObject SyncObj = new SyncObject();
        private List<QueryCollectionRunner> runners = new List<QueryCollectionRunner>();
        private BackgroundWorker bgWorker;
        private ConnectionData connData = null;
        /// <summary>
        /// The directory where the ultimate results file will be created. Will be used as the root for the temp files
        /// </summary>
        private string resultsFilePath = string.Empty;
        /// <summary>
        /// Constructor that takes int the MultiDbData object that contains the list of target databases and their servers.
        /// </summary>
        /// <param name="multiDbData">MultiDbData containing the target databases</param>
        /// <param name="resultsFilePath">The directory where the ultimate results file will be created. Will be used as the root for the temp files</param>
        public QueryCollector(MultiDbData multiDbData, ConnectionData connData)
        {
            this.multiDbData = multiDbData;
            this.connData = connData;

        }
        private QueryCollector()
        {

        }
        /// <summary>
        /// Main method that coordinates the collection of data from each target database
        /// </summary>
        /// <param name="bgWorker">BackgroundWorker object used to communicate progress back to the caller</param>
        /// <param name="fileName">Name of the ultimate report file</param>
        /// <param name="reportType">The report type that the user wants</param>
        /// <param name="query">The SQL query to execute across the databases.</param>
        /// <param name="scriptTimeout">SQL timeout per connection</param>
        public bool GetQueryResults(ref BackgroundWorker bgWorker, string fileName, ReportType reportType, string query, int scriptTimeout)
        {
            resultsFilePath = Path.Combine(Path.GetDirectoryName(fileName), Guid.NewGuid().ToString());

            try
            {
                DirectoryInfo inf = new DirectoryInfo(resultsFilePath);
                inf.Create();
                inf.Attributes = FileAttributes.Hidden;
            }
            catch (Exception exe)
            {
                string message = String.Format("Unable to create working temp directory at {0}: {1}", resultsFilePath, exe.Message);
                log.LogError(exe, message);
                if (bgWorker != null && bgWorker.WorkerReportsProgress)
                {
                    bgWorker.ReportProgress(-1, message);
                }
                return false;
            }

            this.bgWorker = bgWorker;
            int threadTotal = 0;
            string db;

            //bool baseLineSet = false;
            foreach (ServerData srv in multiDbData)
            {

                foreach (DatabaseOverride ovr in srv.Overrides)
                {
                    db = srv.ServerName + "." + ovr.OverrideDbTarget;

                    threadTotal++;
                    lock (QueryCollector.SyncObj)
                    {
                        QueryCollector.SyncObj.WorkingRunners++;
                    }
                    QueryCollectionRunner runner = new QueryCollectionRunner(srv.ServerName, ovr.OverrideDbTarget, query, ovr.QueryRowData, reportType, resultsFilePath, scriptTimeout, connData);
                    runner.QueryCollectionRunnerUpdate += new QueryCollectionRunner.QueryCollectionRunnerUpdateEventHandler(runner_HashCollectionRunnerUpdate);
                    runners.Add(runner);
                    System.Threading.ThreadPool.QueueUserWorkItem(ProcessThreadedHashCollection, runner);
                }
            }


            int counter = 0;
            while (QueryCollector.SyncObj.WorkingRunners > 0)
            {
                System.Threading.Thread.Sleep(1000);
                counter++;

                if (bgWorker != null && bgWorker.WorkerReportsProgress && (counter % 2 == 0))
                {
                    bgWorker.ReportProgress(QueryCollector.SyncObj.WorkingRunners, String.Format("Databases remaining: {0}", QueryCollector.SyncObj.WorkingRunners.ToString()));
                }
            }

            if (bgWorker != null && bgWorker.WorkerReportsProgress)
            {
                bgWorker.ReportProgress(0, "Collating Results...");
            }

            List<string> queryResultFiles = new List<string>();
            foreach (QueryCollectionRunner runner in runners)
            {
                queryResultFiles.Add(runner.ResultsTempFile);
            }


            if (bgWorker != null && bgWorker.WorkerReportsProgress)
            {
                bgWorker.ReportProgress(-1, "Generating combined data report...");
            }

            return GenerateReport(fileName, reportType, queryResultFiles);

        }

        /// <summary>
        /// Event handler that collects responses from the threaded runners and uses the Backgroundworker to transmit back to caller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void runner_HashCollectionRunnerUpdate(object sender, QueryCollectionRunnerUpdateEventArgs e)
        {

            if (bgWorker != null && bgWorker.WorkerReportsProgress)
            {
                bgWorker.ReportProgress(0, e);
            }
        }
        /// <summary>
        /// Method called by ThreadPool to kick off data collection
        /// </summary>
        /// <param name="objRunner"></param>
        private void ProcessThreadedHashCollection(object objRunner)
        {
            try
            {
                QueryCollectionRunner runner = (QueryCollectionRunner)objRunner;
                runner.CollectQueryData();
            }
            catch (Exception exe)
            {
                log.LogError($"Error with QueryCollectionRunner{Environment.NewLine}{exe.ToString()}");
            }
            finally
            {
                lock (QueryCollector.SyncObj)
                {
                    QueryCollector.SyncObj.WorkingRunners--;
                }
            }
        }
        /// <summary>
        /// Collates the data from each runner's temp output file and creates the final report
        /// </summary>
        /// <param name="fileName">Name of the final report file</param>
        /// <param name="reportType">The output report type requested</param>
        /// <param name="queryResultFiles">The list of report files created by the runner objects</param>
        public bool GenerateReport(string fileName, ReportType reportType, List<string> queryResultFiles)
        {
            string tmpCombined = string.Empty;
            try
            {
                if (reportType == ReportType.CSV)
                {
                    return CreateCombinedCsvFile(fileName, queryResultFiles);
                }
                tmpCombined = CombineQueryResultsFiles(queryResultFiles);


                if (fileName.Length > 0)
                {



                    string extension = Path.GetExtension(fileName).ToLower();
                    switch (reportType)
                    {
                        case ReportType.HTML:
                            XmlTextReader xsltReader;

                            XslCompiledTransform trans = new XslCompiledTransform();
                            StringReader xsltText;
                            XPathDocument xPathDoc = new XPathDocument(tmpCombined);
                            using (XmlTextWriter fileWriter = new XmlTextWriter(fileName, Encoding.UTF8))
                            {
                                xsltText = new StringReader(Properties.Resources.QueryResult_html);
                                xsltReader = new XmlTextReader(xsltText);
                                trans.Load(xsltReader);
                                trans.Transform(xPathDoc, null, fileWriter);
                            }
                            break;
                        case ReportType.XML:

                            if (File.Exists(fileName))
                                File.Delete(fileName);

                            File.Move(tmpCombined, fileName);
                            break;
                    }
                }
                return true;

            }
            catch (IOException ioEXe)
            {
                log.LogError(ioEXe, "IO Error in GenerateReport");
                return false;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error in GenerateReport");
                return false;
            }
            finally
            {
                if (File.Exists(tmpCombined))
                    File.Delete(tmpCombined);

                if (Directory.Exists(resultsFilePath))
                    Directory.Delete(resultsFilePath, true);

                foreach (QueryCollectionRunner runner in runners)
                    runner.Dispose();

                runners = null;
            }
        }
        /// <summary>
        /// Combines the raw result files from the runners in to a single raw file
        /// </summary>
        /// <param name="queryResultFiles">List of raw report files from the runner objects</param>
        /// <returns>The name of the combined raw data temporary file created for further report processing</returns>
        private string CombineQueryResultsFiles(List<string> queryResultFiles)
        {
            string tmpCombined = resultsFilePath + String.Format("Combined-{0}.txt", Guid.NewGuid().ToString());
            try
            {
                string tmpLine = null;
                using (StreamWriter sw = new StreamWriter(tmpCombined))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine("<ArrayOfQueryResultData>");

                    foreach (string resultFile in queryResultFiles)
                    {
                        using (StreamReader sr = new StreamReader(resultFile))
                        {
                            while (sr.Peek() > 0)
                            {
                                tmpLine = sr.ReadLine();
                                if (tmpLine.Trim().StartsWith("<?xml"))
                                    continue;

                                sw.WriteLine(tmpLine);
                            }
                        }
                    }
                    sw.WriteLine("</ArrayOfQueryResultData>");
                }
            }
            catch (IOException ioExe)
            {
                log.LogError(ioExe, "IO Error in CombineQueryResultsFiles");
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error in CombineQueryResultsFiles");
            }
            return tmpCombined;
        }
        /// <summary>
        /// Specialized method for creating a CVS final report file
        /// </summary>
        /// <param name="fileName">The filename of the final report output file</param>
        /// <param name="queryResultFiles">List of raw report files from the runner objects</param>
        private bool CreateCombinedCsvFile(string fileName, List<string> queryResultsFiles)
        {
            log.LogInformation($"Creating combined CSV from {queryResultsFiles.Count} results files");
            try
            {
                if (queryResultsFiles.Count == 1)
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    File.Move(queryResultsFiles[0], fileName);
                    return true;
                }

                File.WriteAllLines(fileName, File.ReadAllLines(queryResultsFiles[0]));
                File.Delete(queryResultsFiles[0]);
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    for (int i = 1; i < queryResultsFiles.Count; i++)
                    {
                        using (StreamReader sr = new StreamReader(queryResultsFiles[i]))
                        {
                            //dump the first line which is the header...
                            sr.ReadLine();
                            while (sr.Peek() > 0)
                                sw.WriteLine(sr.ReadLine());
                        }
                        File.Delete(queryResultsFiles[i]);
                    }
                    sw.Flush();
                    sw.Close();
                }
                return true;
            }
            catch (IOException ioExe)
            {
                log.LogError($"IO Error in CreateCombinedCsvFile{Environment.NewLine}{ioExe.ToString()}");
                return false;
            }
            catch (Exception exe)
            {
                log.LogError($"Error in CreateCombinedCsvFile{Environment.NewLine}{exe.ToString()}");
                return false;
            }

        }

        public void GetBuildValidationResults(ref BackgroundWorker bgWorker, string fileName, string checkValue, ReportType reportType, BuildValidationType validationType, int scriptTimeout)
        {
            string scriptFormat = @"SELECT {0}, {1}, CommitDate, count(1) as [Script Count] 
FROM dbo.SqlBuild_Logging WITH (NOLOCK) WHERE {0} = '{2}'
GROUP BY {0}, {1}, CommitDate ORDER BY CommitDate DESC";
            string script;
            switch (validationType)
            {
                case BuildValidationType.BuildFileName:
                    script = string.Format(scriptFormat, "BuildFileName", "BuildProjectHash", checkValue);
                    break;
                case BuildValidationType.IndividualScriptHash:
                    script = string.Format(scriptFormat, "ScriptFileHash", "ScriptFileName", checkValue);
                    break;
                case BuildValidationType.IndividualScriptName:
                    script = string.Format(scriptFormat, "ScriptFileName", "ScriptFileHash", checkValue);
                    break;
                case BuildValidationType.BuildFileHash:
                default:
                    script = string.Format(scriptFormat, "BuildProjectHash", "BuildFileName", checkValue);
                    break;
            }

            GetQueryResults(ref bgWorker, fileName, reportType, script, scriptTimeout);

        }

    }
    /// <summary>
    /// Class used to keep count of all of the threaded runner objects
    /// </summary>
    class SyncObject
    {
        private int workingRunners = 0;

        public int WorkingRunners
        {
            get { return workingRunners; }
            set { workingRunners = value; }
        }
    }

}
