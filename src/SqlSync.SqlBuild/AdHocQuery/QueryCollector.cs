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
using System.Threading.Tasks;
using System.Linq;
using Azure.Messaging.EventHubs.Processor;
using System.Security.Permissions;

namespace SqlSync.SqlBuild.AdHocQuery
{
    public class QueryCollector
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MultiDbData multiDbData;
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
        public BackgroundWorker BackgroundWorker
        {
            set
            {
                this.bgWorker = value;
            }
        }

        public bool EnsureOutputPath(string outputDirectory)
        {

            try
            {
                DirectoryInfo inf = new DirectoryInfo(outputDirectory);
                inf.Create();
                inf.Attributes = FileAttributes.Hidden;
                return true;
            }
            catch (Exception exe)
            {
                string message = String.Format("Unable to create working temp directory at {0}: {1}", outputDirectory, exe.Message);
                log.LogError(exe, message);
                if (bgWorker != null && bgWorker.WorkerReportsProgress)
                {
                    bgWorker.ReportProgress(-1, message);
                }
                return false;
            }
        }
        /// <summary>
        /// Main method that coordinates the collection of data from each target database
        /// </summary>
        /// <param name="bgWorker">BackgroundWorker object used to communicate progress back to the caller</param>
        /// <param name="fileName">Name of the ultimate report file</param>
        /// <param name="reportType">The report type that the user wants</param>
        /// <param name="query">The SQL query to execute across the databases.</param>
        /// <param name="scriptTimeout">SQL timeout per connection</param>
        public bool GetQueryResults(string fileName, ReportType reportType, string query, int scriptTimeout)
        {
            resultsFilePath = Path.Combine(Path.GetDirectoryName(fileName), Guid.NewGuid().ToString());

            if(!EnsureOutputPath(resultsFilePath))
            {
                return false;
            }

            string db;

            var queryTasks = new List<Task<(int,string)>>();
            //bool baseLineSet = false;
            foreach (ServerData srv in multiDbData)
            {

                foreach (DatabaseOverride ovr in srv.Overrides)
                {
                    if (srv.ServerName.StartsWith("#")) srv.ServerName = ovr.Server;
                    db = srv.ServerName + "." + ovr.OverrideDbTarget;

                    QueryCollectionRunner runner = new QueryCollectionRunner(srv.ServerName, ovr.OverrideDbTarget, query, ovr.QueryRowData, reportType, resultsFilePath, scriptTimeout, connData);
                    queryTasks.Add(runner.CollectQueryData());
                }
            }
            while(true)
            {
                System.Threading.Thread.Sleep(1000);
                var remaining = queryTasks.Count(t => t.Status != TaskStatus.RanToCompletion);
                var percent = remaining == 0 ? 0 : (int)((double)remaining / (double)queryTasks.Count * 100);
                bgWorker?.ReportProgress(percent, String.Format("Databases remaining: {0}", remaining));

                var running = queryTasks.Count(t => t.Status == TaskStatus.Running);
                if (running == 0)
                {
                    break;
                }
                
            }
            Task.WaitAll(queryTasks.ToArray());
 
            if (bgWorker != null && bgWorker.WorkerReportsProgress)
            {
                bgWorker?.ReportProgress(0, "Collating Results...");
            }


            var queryResultFiles = queryTasks.Select(t => t.Result.Item2).ToList();
            var retVal = queryTasks.Select(t => t.Result.Item1).Sum();

            if (bgWorker != null && bgWorker.WorkerReportsProgress)
            {
                bgWorker?.ReportProgress(-1, "Generating combined data report...");
            }

            var reportGenRes =  GenerateReport(fileName, reportType, queryResultFiles);
            if(reportGenRes && retVal == 0)
            {
                return true;
            }
            else
            {
                return false;
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
            if(queryResultsFiles.Count == 0)
            {
                log.LogInformation($"No results files generated, not creating summary report.");
                return true;
            }
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

            GetQueryResults(fileName, reportType, script, scriptTimeout);

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
