using Microsoft.Azure.Amqp.Framing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Dependent.TestBase;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    public class Initialization : InitializationBase
    {
        public int TableLockingLoopCount = 1000000;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string MissingDatabaseErrorMessage = "NOTE: To succesfully test, please make sure you have a SQL Server instance running. Set SBM_TEST_SQL_SERVER, SBM_TEST_SQL_USER, SBM_TEST_SQL_PASSWORD environment variables for non-default configuration.";

        public string connectionString;
        public string databasePath;

        protected override string TestTableName => "TransactionTest";
        protected override string TempFilePrefix => @"\SqlSyncTest-";

        private static string GetServerName() => Environment.GetEnvironmentVariable("SBM_TEST_SQL_SERVER") ?? @"(local)\SQLEXPRESS";
        private static string GetConnectionString(string serverName)
        {
            var user = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
            var password = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
            if (!string.IsNullOrWhiteSpace(user))
                return $"Data Source={serverName};Initial Catalog={{0}};User ID={user};Password={password};CONNECTION TIMEOUT=20;Trust Server Certificate=true;Encrypt=false";
            else
                return $@"Data Source={serverName};Initial Catalog={{0}}; Trusted_Connection=Yes;CONNECTION TIMEOUT=20;Trust Server Certificate=true";
        }
        private static string GetDatabasePath()
        {
            var envPath = Environment.GetEnvironmentVariable("SBM_TEST_DB_PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
                return envPath;
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\SqlBuildManager_UnitTestDatabase" : "/var/opt/mssql/data";
        }

        public string PreRunScriptGuid = "47037F10-C217-4e7b-89AE-482F8C09D672";
        public Initialization() : base()
        {
            serverName = GetServerName();
            connectionString = GetConnectionString(serverName);
            databasePath = GetDatabasePath();

            testDatabaseNames = new List<string>();
            testDatabaseNames.Add("SqlBuildTest");
            testDatabaseNames.Add("SqlBuildTest1");
            testDatabaseNames.Add("SqlBuildTest2");
            testDatabaseNames.Add("SqlBuildTest3");
            testDatabaseNames.Add("SqlBuildTest4");
            testDatabaseNames.Add("SqlBuildTest5");
            testDatabaseNames.Add("SqlBuildTest6");
            testDatabaseNames.Add("SqlBuildTest7");
            testDatabaseNames.Add("SqlBuildTest8");
            testDatabaseNames.Add("SqlBuildTest9");
            testDatabaseNames.Add("SqlBuildTest10");
            testDatabaseNames.Add("SqlBuildTest11");
            testDatabaseNames.Add("SqlBuildTest12");
            testDatabaseNames.Add("SqlBuildTest13");
            testDatabaseNames.Add("SqlBuildTest14");
            testDatabaseNames.Add("SqlBuildTest15");
            testDatabaseNames.Add("SqlBuildTest16");
            testDatabaseNames.Add("SqlBuildTest17");
            testDatabaseNames.Add("SqlBuildTest18");
            testDatabaseNames.Add("SqlBuildTest19");
            testDatabaseNames.Add("SqlBuildTest20");

            testDatabaseNames.Add("SqlBuildTest_SyncTest1");
            testDatabaseNames.Add("SqlBuildTest_SyncTest2");
            testDatabaseNames.Add("SqlBuildTest_SyncTest3");

            connData = new ConnectionData(serverName, testDatabaseNames[0]);
            var sqlUser = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
            var sqlPassword = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
            if (!string.IsNullOrWhiteSpace(sqlUser))
            {
                connData.AuthenticationType = AuthenticationType.Password;
                connData.UserId = sqlUser;
                connData.Password = sqlPassword ?? string.Empty;
            }

            if (!CreateDatabases())
                Assert.Fail(String.Format("Unable to create the target databases. {0}", Initialization.MissingDatabaseErrorMessage));
            if (!CreateTestTables())
                Assert.Fail("Unable to create the target tables");
            if (!CreateSqlBuildLoggingTables())
                Assert.Fail("Unable to create the SqlBuild_Logging tables");

            if (!CleanTestTables())
            {
                Assert.Fail("Unable to clean pre-existing data from the target tables");
            }

            if (!CleanSqlBuildLoggingTables())
            {
                Assert.Fail("Unable to clean pre-existing data from the SqlBuild_Logging tables");
            }
        }
        public override bool CreateDatabases()
        {
            try
            {
                if (!Directory.Exists(databasePath))
                    Directory.CreateDirectory(databasePath);

                string connString = string.Format(connectionString, "master");
                string check = "SELECT 1 FROM dbo.sysdatabases WHERE [name] = @name";
                SqlConnection conn = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(check, conn);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar);

                foreach (string dbName in testDatabaseNames)
                {
                    cmd.Parameters["@name"].Value = dbName;
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    object val = cmd.ExecuteScalar();
                    if (val == DBNull.Value || val == null)
                    {
                        string[] commands = String.Format(Properties.Resources.CreateDatabaseScript, dbName, databasePath).Split(new string[] { "GO" }, StringSplitOptions.None);
                        for (int i = 0; i < commands.Length; i++)
                        {
                            cmd.CommandText = commands[i];
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to create test databases");
                return false;
            }
        }
        public override bool CreateTestTables()
        {

            try
            {

                for (int i = 0; i < testDatabaseNames.Count; i++)
                {
                    string connString = String.Format(connectionString, testDatabaseNames[i]);
                    SqlConnection conn = new SqlConnection(connString);
                    SqlCommand cmd = new SqlCommand(Properties.Resources.CreateTestTablesScript, conn);
                    conn.Open();
                    object val = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public override bool CleanTestTables()
        {
            try
            {

                for (int i = 0; i < testDatabaseNames.Count; i++)
                {
                    string connString = String.Format(connectionString, testDatabaseNames[i]);
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        // Use SET LOCK_TIMEOUT to avoid waiting indefinitely for locks
                        string cleaner = "SET LOCK_TIMEOUT 5000; " + string.Format(Properties.Resources.CleanTestTable, DateTime.Now.AddHours(-1).ToString());
                        using (SqlCommand cmd = new SqlCommand(cleaner, conn))
                        {
                            cmd.CommandTimeout = 120; // 2 minute command timeout
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public override bool CreateSqlBuildLoggingTables()
        {
            for (int i = 0; i < testDatabaseNames.Count; i++)
            {
                string connString = String.Format(connectionString, testDatabaseNames[i]);
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Properties.Resources.LoggingTable, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(Properties.Resources.LoggingTableCommitCheckIndex, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return true;
        }
        public override bool CleanSqlBuildLoggingTables()
        {
            int maxRetries = 5;
            int retryDelayMs = 2000;
            
            for (int i = 0; i < testDatabaseNames.Count; i++)
            {
                string connString = String.Format(connectionString, testDatabaseNames[i]);
                bool success = false;
                
                for (int retry = 0; retry < maxRetries && !success; retry++)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connString))
                        {
                            // Use SET LOCK_TIMEOUT to avoid waiting indefinitely for locks
                            // Also increase command timeout for large tables
                            string cleaner = @"
                                SET LOCK_TIMEOUT 10000; -- 10 second lock timeout
                                IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'SqlBuild_Logging' AND type = 'U') 
                                BEGIN 
                                    DELETE FROM SqlBuild_Logging WHERE BuildFileName LIKE 'SqlSyncTest-%' OR CommitDate < @CutoffDate 
                                END";
                            using (SqlCommand cmd = new SqlCommand(cleaner, conn))
                            {
                                cmd.CommandTimeout = 120; // 2 minute command timeout
                                cmd.Parameters.AddWithValue("@CutoffDate", DateTime.Now.AddHours(-1));
                                conn.Open();
                                cmd.ExecuteNonQuery();
                                success = true;
                            }
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 1222) // Lock timeout
                    {
                        log.LogWarning($"Lock timeout cleaning SqlBuild_Logging for {testDatabaseNames[i]}, retry {retry + 1}/{maxRetries}");
                        if (retry < maxRetries - 1)
                        {
                            System.Threading.Thread.Sleep(retryDelayMs);
                        }
                    }
                    catch (Exception exe)
                    {
                        log.LogError(exe, $"Unable to clean SqlBuild_Logging tables for {testDatabaseNames[i]}");
                        return false;
                    }
                }
                
                // If we couldn't clean after all retries, log warning but continue
                // The test data isolation should still work since we use unique BuildFileName patterns
                if (!success)
                {
                    log.LogWarning($"Could not clean SqlBuild_Logging for {testDatabaseNames[i]} after {maxRetries} retries, continuing anyway");
                }
            }
            return true;
        }
        public bool InsertPreRunScriptEntry()
        {
            try
            {

                for (int i = 0; i < testDatabaseNames.Count; i++)
                {
                    string connString = String.Format(connectionString, testDatabaseNames[i]);
                    SqlConnection conn = new SqlConnection(connString);
                    SqlCommand cmd = new SqlCommand(String.Format(Properties.Resources.InsertPreRunScriptLogEntryScript, PreRunScriptGuid), conn);
                    conn.Open();
                    object val = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void AddPreRunScript(ref SqlSyncBuildDataModel data, bool multipleRun)
        {
            Script row = new();
            row.AllowMultipleRuns = multipleRun;
            row.BuildOrder = 1;
            row.CausesBuildFailure = true;
            row.Database = testDatabaseNames[0];
            row.DateAdded = testTimeStamp;
            string fileName = GetTrulyUniqueFile();

            row.FileName = fileName;
            row.RollBackOnError = true;
            row.StripTransactionText = true;
            row.Description = "Test Script to be skipped";
            row.AddedBy = "UnitTest";
            row.ScriptId = PreRunScriptGuid;
            data.Script.Add(row);
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);

            InsertPreRunScriptEntry();


        }

        public override ConnectionData CreateConnectionData(string databaseName)
        {
            var cd = new ConnectionData(serverName, databaseName);
            cd.AuthenticationType = connData.AuthenticationType;
            cd.UserId = connData.UserId;
            cd.Password = connData.Password;
            return cd;
        }

        public SqlBuildHelper CreateSqlBuildHelperAccessor(SqlSyncBuildDataModel buildData)
        {
            SqlBuildHelper target = new SqlBuildHelper(data: connData, connectionsService: new DefaultConnectionsService());


            //Set fields
            ((ISqlBuildRunnerProperties)target).BuildDataModel = buildData;

            string logFile = GetTrulyUniqueFile();
            tempFiles.Add(logFile);
            target.scriptLogFileName = logFile;

            SqlSyncBuildDataModel buildHist = CreateSqlSyncSqlBuildDataModelObject();
            ((ISqlBuildRunnerProperties)target).BuildHistoryModel = buildHist;

            projectFileName = GetTrulyUniqueFile();
            tempFiles.Add(projectFileName);
            target.projectFileName = projectFileName;

            buildHistoryXmlFile = GetTrulyUniqueFile();
            tempFiles.Add(buildHistoryXmlFile);
            target.buildHistoryXmlFile = buildHistoryXmlFile;

            return target;
        }

        public ScriptBatchCollection GetScriptBatchCollection()
        {
            ScriptBatchCollection coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("File1.sql",
                new string[] { "SELECT top 1 * from SqlBuild_Logging", "SELECT TOP 1 * from SqlBuild_Logging ORDER BY CommitDate DESC" },
                "D0080D2A-7E24-4C47-94D6-8EADFCEF8B57"));

            coll.Add(new ScriptBatch("File2.sql",
                new string[] { "SELECT top 2 * from SqlBuild_Logging", "SELECT TOP 2 * from SqlBuild_Logging ORDER BY CommitDate DESC" },
                "A8318EF0-D6D9-4D65-8207-BB4AC62C4FB8"));

            coll.Add(new ScriptBatch("File3.sql",
                new string[] { "SELECT top 3 * from SqlBuild_Logging", "SELECT TOP 3 * from SqlBuild_Logging ORDER BY CommitDate DESC" },
                "1309E71F-4515-46BA-9446-E054F3523BDF"));

            return coll;

        }

        public void AddScriptForProcessBuild(ref SqlSyncBuildDataModel data, bool multipleRun, int scriptTimeout)
        {
            Script row = new();
            row.AllowMultipleRuns = multipleRun;
            row.BuildOrder = 1;
            row.CausesBuildFailure = true;
            row.Database = testDatabaseNames[0];
            row.DateAdded = testTimeStamp;
            row.ScriptTimeOut = scriptTimeout;
            string fileName = GetTrulyUniqueFile();

            row.FileName = fileName;
            row.RollBackOnError = true;
            row.StripTransactionText = true;
            row.Description = "Test Script to successfully insert into TransactionTest table";
            row.AddedBy = "UnitTest";
            row.ScriptId = "59F8CBCA-3FE5-4142-ACB0-3D1D2C25184D"; //Must match GUID from GetScriptBatchCollectionForProcessBuild()
            data.Script.Add(row);
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public ScriptBatchCollection GetScriptBatchCollectionForProcessBuild()
        {
            ScriptBatchCollection sbc = new ScriptBatchCollection();

            string[] scripts = new string[2];
            scripts[0] = "INSERT INTO dbo.TransactionTest VALUES ('PROCESS BUILD1', newid(), getdate())";
            scripts[1] = "INSERT INTO dbo.TransactionTest VALUES ('PROCESS BUILD2', newid(), getdate())";

            string scriptName = "Transaction Test Insert.sql";
            string scriptId = "59F8CBCA-3FE5-4142-ACB0-3D1D2C25184D";  //Must match GUID from AddScriptForProcessBuild()
            ScriptBatch sb = new ScriptBatch(scriptName, scripts, scriptId);
            sbc.Add(sb);

            string[] scripts2 = new string[2];
            scripts2[0] = "SELECT top 1 * FROM INFORMATION_SCHEMA.tables";
            scripts2[1] = "SELECT top 1 * FROM INFORMATION_SCHEMA.routines";

            scriptName = "Select InfoSchema.sql";
            scriptId = "8A9CC6E9-D0D3-4525-AAF4-5ACCECEF4D25";

            ScriptBatch sb2 = new ScriptBatch(scriptName, scripts2, scriptId);
            sbc.Add(sb2);

            return sbc;
        }

        public SqlBuildRunData GetSqlBuildRunData_TransactionalNotTrial(SqlSyncBuildDataModel buildData)
        {
            var uniqueId = Guid.NewGuid().ToString("N");
            SqlBuildRunData runData = new SqlBuildRunData()
            {
                BuildDataModel = buildData,
                BuildDescription = "UnitTestRun",
                BuildFileName = Path.Combine(Path.GetTempPath(), $"UnitTestBuildFile_{uniqueId}.sbm"),
                BuildType = "Development",
                ProjectFileName = Path.Combine(Path.GetTempPath(), $"ProjectFile_{uniqueId}.xml"),
                Server = serverName,
                StartIndex = 0
            };

            runData.IsTransactional = true;
            runData.IsTrial = false;
            runData.TargetDatabaseOverrides = GetDatabaseOverrides();

            return runData;
        }

        public SqlSync.SqlBuild.Models.SqlBuildRunDataModel GetSqlBuildRunDataModel_TransactionalNotTrial(SqlSyncBuildDataModel buildData)
        {
            var uniqueId = Guid.NewGuid().ToString("N");
            return new SqlSync.SqlBuild.Models.SqlBuildRunDataModel(
                buildDataModel: buildData,
                buildType: "Development",
                server: serverName,
                buildDescription: "UnitTestRun",
                startIndex: 0,
                projectFileName: Path.Combine(Path.Combine(Path.GetTempPath(),$"ProjectFile_{uniqueId}.xml")),
                isTrial: false,
                runItemIndexes: Array.Empty<double>(),
                runScriptOnly: false,
                buildFileName: Path.Combine(Path.GetTempPath(), $"UnitTestBuildFile_{uniqueId}.sbm"),
                logToDatabaseName: string.Empty,
                isTransactional: true,
                platinumDacPacFileName: string.Empty,
                targetDatabaseOverrides: GetDatabaseOverrides(),
                forceCustomDacpac: false,
                buildRevision: string.Empty,
                defaultScriptTimeout: 500,
                allowObjectDelete: false);
        }

        public override int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(String.Format(connectionString, testDatabaseNames[databaseIndex])))
                {
                    SqlCommand cmd = new SqlCommand("SELECT count(*) FROM SqlBuild_Logging WITH (NOLOCK) WHERE BuildFileName = @BuildFileName", conn);
                    cmd.Parameters.AddWithValue("@BuildFileName", Path.GetFileName(projectFileName));
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    return (int)val;
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to get count for build file");
                return -1;
            }

        }
        public int GetSqlBuildLoggingRowCountByScriptId(int databaseIndex, string guidValue)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(String.Format(connectionString, testDatabaseNames[databaseIndex])))
                {
                    SqlCommand cmd = new SqlCommand("SELECT count(*) FROM SqlBuild_Logging WHERE ScriptId = @ScriptId", conn);
                    cmd.Parameters.AddWithValue("@ScriptId", guidValue);
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    return (int)val;
                }
            }
            catch
            {
                return -1;
            }

        }
        public override int GetTestTableRowCount(int databaseIndex)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(String.Format(connectionString, testDatabaseNames[databaseIndex])))
                {
                    SqlCommand cmd = new SqlCommand("SELECT count(*) FROM TransactionTest WHERE [Guid] = @Guid", conn);
                    cmd.Parameters.AddWithValue("@Guid", testGuid);

                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    return (int)val;
                }
            }
            catch
            {
                return -1;
            }
        }
        public string GetTrulyUniqueFile()
        {
            string tmpName = Path.GetTempFileName();
            string newName = Path.Combine(Path.GetDirectoryName(tmpName), $"SqlSyncTest-{Guid.NewGuid().ToString()}.tmp");
            File.Move(tmpName, newName);


            tempFiles.Add(newName);
            return newName;

        }


        public string GetTableLockingScript()
        {
            string lockingScript = string.Format(Properties.Resources.TableLockingScript, TableLockingLoopCount.ToString());
            return lockingScript;
        }


    }
}
