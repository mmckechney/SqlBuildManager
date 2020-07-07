using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.Connection;
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

        public string connectionString = @"Data Source=(local)\SQLEXPRESS;Initial Catalog={0}; Trusted_Connection=Yes;CONNECTION TIMEOUT=20;";
        public string serverName = @"(local)\SQLEXPRESS";


        public Initialization()
        {
            testDatabaseNames = new List<string>();
            testDatabaseNames.Add("SqlBuildTest");
            testDatabaseNames.Add("SqlBuildTest1");

            testGuid = Guid.NewGuid();
            testTimeStamp = DateTime.Now;

            connData = new ConnectionData(serverName, testDatabaseNames[0]);

            tempFiles = new List<string>();
        }
    }
}
