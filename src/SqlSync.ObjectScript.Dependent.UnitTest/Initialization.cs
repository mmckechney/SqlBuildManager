using SqlSync.Connection;
using System;
using System.Collections.Generic;
namespace SqlSync.ObjectScript.Dependent.UnitTest
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
            serverName = Environment.GetEnvironmentVariable("SBM_TEST_SQL_SERVER") ?? @"(local)\SQLEXPRESS";
            var user = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
            var password = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
            if (!string.IsNullOrWhiteSpace(user))
                connectionString = $"Data Source={serverName};Initial Catalog={{0}};User ID={user};Password={password};CONNECTION TIMEOUT=20;Trust Server Certificate=true;Encrypt=false";
            else
                connectionString = $@"Data Source={serverName};Initial Catalog={{0}}; Trusted_Connection=Yes;CONNECTION TIMEOUT=20;";

            testDatabaseNames = new List<string>();
            testDatabaseNames.Add("SqlBuildTest");
            testDatabaseNames.Add("SqlBuildTest1");

            testGuid = Guid.NewGuid();
            testTimeStamp = DateTime.Now;

            connData = new ConnectionData(serverName, testDatabaseNames[0]);
            var sqlUser = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
            var sqlPassword = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
            if (!string.IsNullOrWhiteSpace(sqlUser))
            {
                connData.AuthenticationType = AuthenticationType.Password;
                connData.UserId = sqlUser;
                connData.Password = sqlPassword ?? string.Empty;
            }

            tempFiles = new List<string>();
        }
    }
}
