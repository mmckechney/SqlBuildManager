using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.DbInformation;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml;
namespace SqlSync.SqlBuild.Status
{
    public class StatusReporting
    {
        private SqlSyncBuildData buildData;
        private MultiDb.MultiDbData multiDbData;
        private string projectFilePath;
        private string buildZipFileName;
        int threadTotal = 0;
        private static SyncObject SyncObj = new SyncObject();
        List<StatusReportRunner> runners = new List<StatusReportRunner>();
        public StatusReporting(SqlSyncBuildData buildData, MultiDb.MultiDbData multiDbData, string projectFilePath, string buildZipFileName)
        {
            this.buildData = buildData;
            this.multiDbData = multiDbData;
            this.buildZipFileName = buildZipFileName;
            this.projectFilePath = projectFilePath;
        }
        public string GetScriptStatus()
        {
            BackgroundWorker bw = new BackgroundWorker();
            return this.GetScriptStatus(ref bw, string.Empty,ReportType.XML);
        }
        public string GetScriptStatus(ref BackgroundWorker bgWorker, string fileName, ReportType reportType)
        {

            foreach (ServerData srv in multiDbData)
            {
                foreach (List<DatabaseOverride> ovr in srv.OverrideSequence.Values)
                {
                    threadTotal++;
                    lock (StatusReporting.SyncObj)
                    {
                        StatusReporting.SyncObj.WorkingRunners++;
                    }
                    StatusReportRunner runner = new StatusReportRunner(this.buildData, srv.ServerName, ovr, projectFilePath);
                    runners.Add(runner);
                    System.Threading.ThreadPool.QueueUserWorkItem(ProcessThreadedScriptStatus, runner);
               }
            }

            int counter = 0;
            while (StatusReporting.SyncObj.WorkingRunners > 0)
            {
                System.Threading.Thread.Sleep(100);
                counter++;

                if (bgWorker != null && (counter % 2 == 0))
                    bgWorker.ReportProgress(StatusReporting.SyncObj.WorkingRunners, String.Format("Threads remaining: {0}", StatusReporting.SyncObj.WorkingRunners.ToString()));
            }

            if (bgWorker != null)
                bgWorker.ReportProgress(0, "Collating Results...");

            ServerStatusDataCollection coll = new ServerStatusDataCollection();
            coll.BuildFileNameFull = this.buildZipFileName;
            foreach (StatusReportRunner runner in this.runners)
            {
               coll[runner.ServerName][runner.BaseDatabase] = runner.Status;
            }
            return GenerateReport(coll, fileName, reportType);
        }
        private void ProcessThreadedScriptStatus(object runnerObj)
        {
            try
            {
                StatusReportRunner runner = (StatusReportRunner)runnerObj;
                runner.RetrieveStatus();
            }
            finally
            {
                lock (StatusReporting.SyncObj)
                {
                    StatusReporting.SyncObj.WorkingRunners--;
                }
            }
        }

        private string GenerateReport(ServerStatusDataCollection collection, string fileName,ReportType reportType)
        {
            MemoryStream ms = new MemoryStream();
        
            StringBuilder sb  = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                System.Xml.Serialization.XmlSerializer xmlS = new System.Xml.Serialization.XmlSerializer(typeof(ServerStatusDataCollection));
                xmlS.Serialize(sw, collection);
                sw.Flush();
                sw.Close();
            }

            if (fileName.Length > 0)
            {
                
                XmlTextReader xsltReader;

                XslCompiledTransform trans = new XslCompiledTransform();
                StringReader sr = new StringReader(sb.ToString());
                StringReader xsltText;
                XmlTextReader xmlReader = new XmlTextReader(sr);
                XPathDocument xPathDoc = new XPathDocument(xmlReader);
                using (XmlTextWriter fileWriter = new XmlTextWriter(fileName, Encoding.UTF8))
                {
                    string extension = Path.GetExtension(fileName).ToLower();
                    switch (reportType)
                    {
                        case ReportType.HTML:
                            xsltText = new StringReader(Properties.Resources.ServerReport_html);
                            xsltReader = new XmlTextReader(xsltText);
                            trans.Load(xsltReader);
                            trans.Transform(xPathDoc, null, fileWriter);
                            break;
                        case ReportType.Summary:
                            xsltText = new StringReader(Properties.Resources.ServerReport_summary);
                            xsltReader = new XmlTextReader(xsltText);
                            trans.Load(xsltReader);
                            trans.Transform(xPathDoc, null, fileWriter);
                            break;
                        case ReportType.XML:
                            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                            string utf8String = System.Text.Encoding.UTF8.GetString(utf8Bytes);
                            utf8String = utf8String.Replace("utf-16", "utf-8");
                            fileWriter.WriteRaw(utf8String);
                            break;
                        case ReportType.CSV:
                        default:
                            xsltText = new StringReader(Properties.Resources.ServerReport_csv);
                            xsltReader = new XmlTextReader(xsltText);
                            trans.Load(xsltReader);
                            trans.Transform(xPathDoc, null, fileWriter);
                            break;
                    }
                }
            }

            return sb.ToString();
        }
    }

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
