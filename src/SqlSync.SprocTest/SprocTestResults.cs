using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.SprocTest
{
    public class SprocTestResults
    {
        private string targetServer;

        public string TargetServer
        {
            get { return targetServer; }
            set { targetServer = value; }
        }
        private string executedBy;

        public string ExecutedBy
        {
            get { return executedBy; }
            set { executedBy = value; }
        }
        private string testConfigurationFile;

        public string TestConfigurationFile
        {
            get { return testConfigurationFile; }
            set { testConfigurationFile = value; }
        }
        private DateTime testStartTime;

        public DateTime StartTime
        {
            get { return testStartTime; }
            set { testStartTime = value; }
        }
        private DateTime testEndTime;

        public DateTime EndTime
        {
            get { return testEndTime; }
            set { testEndTime = value; }
        }
        private string targetDatabase;

        private int storedProceduresTested;

        public int StoredProceduresTested
        {
            get { return storedProceduresTested; }
            set { storedProceduresTested = value; }
        }
        private int testCasesRun;

        public int TestCasesRun
        {
            get { return testCasesRun; }
            set { testCasesRun = value; }
        }
        private int passedTests;

        public int PassedTests
        {
            get { return passedTests; }
            set { passedTests = value; }
        }
        private int failedTests;

        public int FailedTests
        {
            get { return failedTests; }
            set { failedTests = value; }
        }

        public string TargetDatabase
        {
            get { return targetDatabase; }
            set { targetDatabase = value; }
        }
        private List<TestManager.TestResult> results = new List<TestManager.TestResult>();

        public List<TestManager.TestResult> Results
        {
            get { return results; }
            set { results = value; }
        }

       

    }
}
