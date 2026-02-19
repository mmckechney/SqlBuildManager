using SqlBuildManager.Test.Common;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
namespace SqlSync.DbInformation.Dependent.UnitTest
{
    class Initialization
    {
        public List<string> testDatabaseNames = null;
        public List<string> tempFiles = null;
        public Guid testGuid;
        public DateTime testTimeStamp;
        public ConnectionData connData = null;
        public string projectFileName = null;
        public string buildHistoryXmlFile = null;

        public string connectionString;
        public string serverName;

        public Initialization()
        {
            serverName = TestEnvironment.SqlServer;
            connectionString = TestEnvironment.GetSqlConnectionStringTemplate();

            testDatabaseNames = new List<string>();
            testDatabaseNames.Add("SqlBuildTest");
            testDatabaseNames.Add("SqlBuildTest1");

            testGuid = Guid.NewGuid();
            testTimeStamp = DateTime.Now;

            connData = TestEnvironment.GetConnectionData(testDatabaseNames[0]);

            tempFiles = new List<string>();
        }
    }
}
