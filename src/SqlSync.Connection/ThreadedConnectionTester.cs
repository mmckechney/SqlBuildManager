using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace SqlSync.Connection
{
    public class ThreadedConnectionTester
    {
        private static SyncObject SyncObj = new SyncObject();
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private class SyncObject
        {
            private int workingRunners = 0;

            public int WorkingRunners
            {
                get { return workingRunners; }
                set { workingRunners = value; }
            }
        }

        public List<ConnectionTestResult> TestDatabaseConnections(Dictionary<string, List<string>> serverDbSets, string userName, string password, AuthenticationType authType)
        {
            List<ConnectionTestResult> results = new List<ConnectionTestResult>();

            foreach (KeyValuePair<string, List<string>> server in serverDbSets)
            {
                foreach (string db in server.Value)
                {

                    lock (ThreadedConnectionTester.SyncObj)
                    {
                        ThreadedConnectionTester.SyncObj.WorkingRunners++;
                    }

                    ConnectionTestResult obj = new ConnectionTestResult() { DatabaseName = db, ServerName = server.Key, DbUserName = userName, DbPassword = password, AuthenticationType = authType };
                    results.Add(obj);
                    string msg = "Queuing up thread for " + obj.ServerName + "." + obj.DatabaseName;
                    log.LogDebug(msg);
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
            result.Successful = ConnectionHelper.TestDatabaseConnection(result.DatabaseName, result.ServerName, result.DbUserName, result.DbPassword, result.AuthenticationType, 2);
            lock (ThreadedConnectionTester.SyncObj)
            {
                ThreadedConnectionTester.SyncObj.WorkingRunners--;
            }
            log.LogDebug("Thread Complete for " + result.ServerName + "." + result.DatabaseName + " :: Successful=" + result.Successful.ToString());
        }
    }
}