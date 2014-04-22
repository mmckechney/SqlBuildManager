using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSync.Connection
{
    public class ThreadedConnectionTester
    {
        private static SyncObject SyncObj = new SyncObject();
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private class SyncObject
        {
            private int workingRunners = 0;

            public int WorkingRunners
            {
                get { return workingRunners; }
                set { workingRunners = value; }
            }
        }
      
        public List<ConnectionTestResult> TestDatabaseConnections(Dictionary<string, List<string>> serverDbSets)
        {
            List<ConnectionTestResult> results = new List<ConnectionTestResult>();

            foreach (KeyValuePair<string,List<string>> server in serverDbSets)
            {
                foreach (string db in server.Value)
                {

                    lock (ThreadedConnectionTester.SyncObj)
                    {
                        ThreadedConnectionTester.SyncObj.WorkingRunners++;
                    }

                    ConnectionTestResult obj = new ConnectionTestResult() { DatabaseName = db, ServerName = server.Key };
                    results.Add(obj);
                    string msg = "Queuing up thread for " + obj.ServerName + "." + obj.DatabaseName;
                    log.Debug(msg);
                    System.Threading.ThreadPool.QueueUserWorkItem(TestSingleConnection, obj);
                }
            }

            while (ThreadedConnectionTester.SyncObj.WorkingRunners > 0)
            {
                System.Threading.Thread.Sleep(100);
            }

            return results;
        }
        private void TestSingleConnection(object data)
        {
            ConnectionTestResult result = (ConnectionTestResult)data;
            result.Successful = ConnectionHelper.TestDatabaseConnection(result.DatabaseName, result.ServerName, 2);
            lock (ThreadedConnectionTester.SyncObj)
            {
                ThreadedConnectionTester.SyncObj.WorkingRunners--;
            }
            log.Debug("Thread Complete for "+result.ServerName +"."+result.DatabaseName + " :: Successful="+result.Successful.ToString());
        }
    }
}