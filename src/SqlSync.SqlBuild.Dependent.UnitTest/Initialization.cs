using Microsoft.Azure.Amqp.Framing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    public class Initialization : IDisposable
    {
        public int TableLockingLoopCount = 1000000;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string MissingDatabaseErrorMessage = "NOTE: To succesfully test, please make sure you have a a (local)\\SQLEXPRESS database instance installed and running";
        public List<string> testDatabaseNames = null;
        public List<string> tempFiles = null;
        public Guid testGuid;
        public DateTime testTimeStamp;
        public ConnectionData connData = null;
        public string projectFileName = null;
        public string buildHistoryXmlFile = null;

        public string connectionString = @"Data Source=(local)\SQLEXPRESS;Initial Catalog={0}; Trusted_Connection=Yes;CONNECTION TIMEOUT=20;Trust Server Certificate=true";
        public string serverName = @"(local)\SQLEXPRESS";

        public string PreRunScriptGuid = "47037F10-C217-4e7b-89AE-482F8C09D672";
        public Initialization()
        {
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

            testGuid = Guid.NewGuid();
            testTimeStamp = DateTime.Now;

            connData = new ConnectionData(serverName, testDatabaseNames[0]);

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



            tempFiles = new List<string>();
        }
        public void Dispose()
        {
            for (int i = 0; i < tempFiles.Count; i++)
            {
                if (File.Exists(tempFiles[i]))
                    File.Delete(tempFiles[i]);
            }

            if (File.Exists(projectFileName))
                File.Delete(projectFileName);

            if (File.Exists(buildHistoryXmlFile))
                File.Delete(buildHistoryXmlFile);
        }

        public bool CreateDatabases()
        {
            try
            {
                string databasePath = @"C:\SqlBuildManager_UnitTestDatabase";
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
        public bool CreateTestTables()
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
        public bool CleanTestTables()
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
        public bool CreateSqlBuildLoggingTables()
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
        public bool CleanSqlBuildLoggingTables()
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
        public SqlSyncBuildDataModel CreateSqlSyncSqlBuildDataModelObject()
        {
            try
            {
                SqlSyncBuildDataModel data = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                return data;
            }
            catch
            {
                return null;
            }
        }
        public void AddInsertScript(ref SqlSyncBuildDataModel data, bool multipleRun)
        {
            Models.Script row = new Script();
            row.AllowMultipleRuns = multipleRun;
            row.BuildOrder = 1;
            row.CausesBuildFailure = true;
            row.Database = testDatabaseNames[0];
            row.DateAdded = testTimeStamp;
            string fileName = GetTrulyUniqueFile();

            row.FileName = fileName;
            row.RollBackOnError = true;
            row.StripTransactionText = true;
            row.Description = "Test Script to successfully insert into TransactionTest table";
            row.AddedBy = "UnitTest";
            row.ScriptId = Guid.NewGuid().ToString();

            data.Script.Add(row);
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public void AddSelectScript(ref SqlSyncBuildDataModel data)
        {
            Script row = new();
            row.AllowMultipleRuns = true;
            row.BuildOrder = 1;
            row.CausesBuildFailure = true;
            row.Database = testDatabaseNames[0];
            row.DateAdded = testTimeStamp;
            string fileName = GetTrulyUniqueFile();

            row.FileName = fileName;
            row.RollBackOnError = true;
            row.StripTransactionText = true;
            row.Description = "Test Script to successfully select from TransactionTest table";
            row.AddedBy = "UnitTest";
            row.ScriptId = Guid.NewGuid().ToString();
            data.Script.Add(row);
            string script = "SELECT * FROM TransactionTest";
            File.WriteAllText(fileName, script);


        }
        public void AddFailureScript(ref SqlSyncBuildDataModel data, bool rollBackOnError, bool causeBuildFailure)
        {
            Script row = new();
            row.AllowMultipleRuns = true;
            row.BuildOrder = 10;
            row.CausesBuildFailure = causeBuildFailure;
            row.Database = testDatabaseNames[0];
            row.DateAdded = testTimeStamp;
            string fileName = GetTrulyUniqueFile();

            row.FileName = fileName;
            row.RollBackOnError = rollBackOnError;
            row.StripTransactionText = true;
            row.Description = "Test Script to cause a failure when inserting into TransactionTest table";
            row.AddedBy = "UnitTest";
            row.ScriptId = Guid.NewGuid().ToString();
            data.Script.Add(row);
            string script = "INSERT INTO TransactionTest (INVALID, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public void AddScriptWithBadDatabase(ref SqlSyncBuildDataModel data)
        {
            Script row = new();
            row.AllowMultipleRuns = true;
            row.BuildOrder = 1;
            row.CausesBuildFailure = true;
            row.Database = "REALLY_BAD_DATABASE";
            row.DateAdded = testTimeStamp;
            string fileName = GetTrulyUniqueFile();

            row.FileName = fileName;
            row.RollBackOnError = true;
            row.StripTransactionText = true;
            row.Description = "Test Script that has an invalid database name";
            row.AddedBy = "UnitTest";
            row.ScriptId = Guid.NewGuid().ToString();
            data.Script.Add(row);

            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public void AddBatchInsertScripts(ref SqlSyncBuildDataModel data, bool multipleRun)
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
            row.Description = "Test Script to successfully insert into TransactionTest table";
            row.AddedBy = "UnitTest";
            row.ScriptId = Guid.NewGuid().ToString();
            data.Script.Add(row);
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            script = script + "\r\nGO\r\n" + script;
            File.WriteAllText(fileName, script);
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

        public SqlBuildHelper CreateSqlBuildHelper(SqlSyncBuildDataModel buildData)
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData);
            return SetSqlBuildHelperValues(helper, buildData);
        }
        public SqlBuildHelper CreateSqlBuildHelper_NonTransactional(SqlSyncBuildDataModel buildData, bool withScriptLog)
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData, withScriptLog, string.Empty, false);
            return SetSqlBuildHelperValues(helper, buildData);
        }
        public SqlBuildHelper CreateSqlBuildHelper_Basic()
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData);
            return SetSqlBuildHelperValues(helper, null);
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
        public SqlBuildHelper SetSqlBuildHelperValues(SqlBuildHelper sbh, SqlSyncBuildDataModel buildData)
        {

            ((ISqlBuildRunnerProperties)sbh).BuildDataModel = buildData;

            string logFile = GetTrulyUniqueFile();
            sbh.scriptLogFileName = logFile;

            SqlSyncBuildDataModel buildHist = CreateSqlSyncSqlBuildDataModelObject();
            ((ISqlBuildRunnerProperties)sbh).BuildHistoryModel = buildHist;

            projectFileName = GetTrulyUniqueFile();
            sbh.projectFileName = projectFileName;

            buildHistoryXmlFile = GetTrulyUniqueFile();
            sbh.buildHistoryXmlFile = buildHistoryXmlFile;
            return sbh;
        }
        public Build GetRunBuildRow(SqlBuildHelper sqlBuildHelper)
        {
            return new Build();
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

        public List<DatabaseOverride> GetDatabaseOverrides()
        {
            List<DatabaseOverride> o = new List<DatabaseOverride>();
            o.Add(new DatabaseOverride(serverName, testDatabaseNames[0], testDatabaseNames[0]));
            return o;

        }
        public SqlBuildRunData GetSqlBuildRunData_TransactionalNotTrial(SqlSyncBuildDataModel buildData)
        {
            SqlBuildRunData runData = new SqlBuildRunData()
            {
                BuildDataModel = buildData,
                BuildDescription = "UnitTestRun",
                BuildFileName = @"C:\temp\UnitTestBuildFile.sbm",
                BuildType = "Development",
                ProjectFileName = @"C:\temp\ProjectFile.xml",
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
            return new SqlSync.SqlBuild.Models.SqlBuildRunDataModel(
                buildDataModel: buildData,
                buildType: "Development",
                server: serverName,
                buildDescription: "UnitTestRun",
                startIndex: 0,
                projectFileName: @"C:\\temp\\ProjectFile.xml",
                isTrial: false,
                runItemIndexes: Array.Empty<double>(),
                runScriptOnly: false,
                buildFileName: @"C:\\temp\\UnitTestBuildFile.sbm",
                logToDatabaseName: string.Empty,
                isTransactional: true,
                platinumDacPacFileName: string.Empty,
                targetDatabaseOverrides: GetDatabaseOverrides(),
                forceCustomDacpac: false,
                buildRevision: string.Empty,
                defaultScriptTimeout: 500,
                allowObjectDelete: false);
        }

        public int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex)
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
        public int GetTestTableRowCount(int databaseIndex)
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
            string newName = Path.GetDirectoryName(tmpName) + @"\SqlSyncTest-" + Guid.NewGuid().ToString() + ".tmp";
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
