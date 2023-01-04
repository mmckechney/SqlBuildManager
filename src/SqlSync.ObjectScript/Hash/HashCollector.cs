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
namespace SqlSync.ObjectScript.Hash
{
    public class HashCollector
    {
        MultiDbData multiDbData;
        private static SyncObject SyncObj = new SyncObject();
        private List<HashCollectionRunner> runners = new List<HashCollectionRunner>();
        private List<string> dbsSelected = new List<string>();
        private BackgroundWorker bgWorker;
        public HashCollector(MultiDbData multiDbData)
        {
            this.multiDbData = multiDbData;
        }
        public HashCollector()
        {

        }
        public ObjectScriptHashReportData GetObjectHashes()
        {
            BackgroundWorker bw = new BackgroundWorker();
            return GetObjectHashes(ref bw, "", ReportType.XML, true);
        }
        public ObjectScriptHashReportData GetObjectHashes(ref BackgroundWorker bgWorker, string fileName, ReportType reportType, bool runThreaded)
        {
            this.bgWorker = bgWorker;
            int threadTotal = 0;
            string db;

            bool baseLineSet = false;
            foreach (ServerData srv in multiDbData)
            {
                srv.Overrides.Sort(); //sort so the sequence is in proper order.

                foreach (DatabaseOverride ovr in srv.Overrides)
                {
                    db = srv.ServerName + "." + ovr.OverrideDbTarget;
                    if (!dbsSelected.Contains(db))
                    {
                        threadTotal++;
                        lock (HashCollector.SyncObj)
                        {
                            HashCollector.SyncObj.WorkingRunners++;
                        }
                        HashCollectionRunner runner = new HashCollectionRunner(srv.ServerName, ovr.OverrideDbTarget);
                        runner.HashCollectionRunnerUpdate += new HashCollectionRunner.HashCollectionRunnerUpdateEventHandler(runner_HashCollectionRunnerUpdate);
                        if (!baseLineSet) //set the baseline to the first database handled.
                        {
                            runner.IsBaseLine = true;
                            baseLineSet = true;
                        }

                        runners.Add(runner);
                        if (runThreaded)
                            System.Threading.ThreadPool.QueueUserWorkItem(ProcessThreadedHashCollection, runner);
                        else
                            runner.CollectHashes();
                    }
                }
            }

            if (runThreaded)
            {
                int counter = 0;
                while (HashCollector.SyncObj.WorkingRunners > 0)
                {
                    System.Threading.Thread.Sleep(100);
                    counter++;

                    if (bgWorker != null && (counter % 2 == 0))
                        bgWorker.ReportProgress(HashCollector.SyncObj.WorkingRunners, String.Format("Threads remaining: {0}", HashCollector.SyncObj.WorkingRunners.ToString()));
                }
            }

            if (bgWorker != null)
                bgWorker.ReportProgress(0, "Collating Results...");

            List<ObjectScriptHashData> hashes = new List<ObjectScriptHashData>();
            ObjectScriptHashData baseLine = null;
            foreach (HashCollectionRunner runner in runners)
            {
                if (runner.IsBaseLine)
                    baseLine = runner.HashData;
                else
                    hashes.Add(runner.HashData);
            }

            ObjectScriptHashReportData reportData = ProcessHashDifferences(baseLine, hashes);
            GenerateReport(fileName, reportType, reportData);

            hashes.Add(baseLine);

            ObjectScriptHashReportData rawReportData = new ObjectScriptHashReportData();
            rawReportData.ProcessTime = reportData.ProcessTime;
            rawReportData.DatabaseData = hashes;
            return rawReportData;
        }

        void runner_HashCollectionRunnerUpdate(object sender, HashCollectionRunnerUpdateEventArgs e)
        {
            bgWorker.ReportProgress(0, e);
        }
        private void ProcessThreadedHashCollection(object objRunner)
        {
            try
            {
                HashCollectionRunner runner = (HashCollectionRunner)objRunner;
                runner.CollectHashes();
            }
            finally
            {
                lock (HashCollector.SyncObj)
                {
                    HashCollector.SyncObj.WorkingRunners--;
                }
            }
        }
        public ObjectScriptHashReportData ProcessHashDifferences(ObjectScriptHashData baseLine, List<ObjectScriptHashData> hashes)
        {
            ObjectScriptHashReportData reportData = new ObjectScriptHashReportData();
            reportData.BaseLineDatabase = baseLine.Database;
            reportData.BaseLineServer = baseLine.Server;
            reportData.ProcessTime = DateTime.Now;

            foreach (ObjectScriptHashData data in hashes)
            {
                HashComparison(baseLine.Tables, data.Tables);
                HashComparison(baseLine.StoredProcedures, data.StoredProcedures);
                HashComparison(baseLine.Views, data.Views);
                HashComparison(baseLine.Functions, data.Functions);
                HashComparison(baseLine.KeysAndIndexes, data.KeysAndIndexes);
                HashComparison(baseLine.Logins, data.Logins);
                HashComparison(baseLine.Roles, data.Roles);
                HashComparison(baseLine.Schemas, data.Schemas);
                HashComparison(baseLine.Users, data.Users);

                reportData.DatabaseData.Add(data);
            }

            return reportData;
        }
        private void HashComparison(ObjectHashDictionary baseline, ObjectHashDictionary comparisonObj)
        {
            //Compare Table Objects
            foreach (KeyValuePair<string, HashSet> baseObj in baseline)
            {
                if (comparisonObj.ContainsKey(baseObj.Key))
                {
                    if (comparisonObj[baseObj.Key].HashValue == baseObj.Value.HashValue)
                    {
                        if (comparisonObj[baseObj.Key].HashValue == string.Empty && baseObj.Value.HashValue == string.Empty)
                            comparisonObj[baseObj.Key].ComparisonValue = "Missing in both";
                        else
                            comparisonObj[baseObj.Key].ComparisonValue = "Same";
                    }
                    else if (comparisonObj[baseObj.Key].HashValue == string.Empty) //can be empty string via the re-analysis process.
                        comparisonObj[baseObj.Key].ComparisonValue = "Missing";
                    else if (baseObj.Value.HashValue == string.Empty) // can be empty string via the re-analysys
                        comparisonObj[baseObj.Key].ComparisonValue = "Added";
                    else
                        comparisonObj[baseObj.Key].ComparisonValue = "Different";
                }
                else
                {
                    comparisonObj.Add(baseObj.Key, "", "Missing");
                }
            }
        }
        public string GenerateReport(string fileName, ReportType reportType, ObjectScriptHashReportData reportData)
        {

            MemoryStream ms = new MemoryStream();

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                System.Xml.Serialization.XmlSerializer xmlS = new System.Xml.Serialization.XmlSerializer(typeof(ObjectScriptHashReportData));
                xmlS.Serialize(sw, reportData);
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
                        case ReportType.Summary:
                            xsltText = new StringReader(Properties.Resources.DatabaseDiff_Summary);
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
