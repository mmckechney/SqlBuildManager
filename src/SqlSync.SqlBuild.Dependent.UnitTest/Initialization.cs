using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
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
                    SqlConnection conn = new SqlConnection(connString);
                    string cleaner = string.Format(Properties.Resources.CleanTestTable, DateTime.Now.AddHours(-1).ToString());
                    SqlCommand cmd = new SqlCommand(cleaner, conn);
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
        public bool CreateSqlBuildLoggingTables()
        {
            for (int i = 0; i < testDatabaseNames.Count; i++)
            {
                string connString = String.Format(connectionString, testDatabaseNames[i]);
                SqlConnection conn = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(Properties.Resources.LoggingTable, conn);
                conn.Open();
                object val = cmd.ExecuteNonQuery();

                cmd = new SqlCommand(Properties.Resources.LoggingTableCommitCheckIndex, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            return true;
        }
        public bool CleanSqlBuildLoggingTables()
        {
            try
            {

                for (int i = 0; i < testDatabaseNames.Count; i++)
                {
                    string connString = String.Format(connectionString, testDatabaseNames[i]);
                    SqlConnection conn = new SqlConnection(connString);
                    string cleaner = string.Format(Properties.Resources.CleanLoggingTable, DateTime.Now.AddHours(-1).ToString());
                    SqlCommand cmd = new SqlCommand(cleaner, conn);
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
        public SqlSyncBuildData CreateSqlSyncSqlBuildDataObject()
        {
            try
            {
                SqlSyncBuildData data = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                return data;
            }
            catch
            {
                return null;
            }
        }
        public void AddInsertScript(ref SqlSyncBuildData data, bool multipleRun)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public void AddSelectScript(ref SqlSyncBuildData data)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
            string script = "SELECT * FROM TransactionTest";
            File.WriteAllText(fileName, script);


        }
        public void AddFailureScript(ref SqlSyncBuildData data, bool rollBackOnError, bool causeBuildFailure)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
            string script = "INSERT INTO TransactionTest (INVALID, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public void AddScriptWithBadDatabase(ref SqlSyncBuildData data)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);


        }
        public void AddBatchInsertScripts(ref SqlSyncBuildData data, bool multipleRun)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            script = script + "\r\nGO\r\n" + script;
            File.WriteAllText(fileName, script);
        }
        public void AddPreRunScript(ref SqlSyncBuildData data, bool multipleRun)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
            string script = "INSERT INTO TransactionTest (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','" + testGuid + "','" + testTimeStamp.ToString() + "')";
            File.WriteAllText(fileName, script);

            InsertPreRunScriptEntry();


        }

        public SqlBuildHelper CreateSqlBuildHelper(SqlSyncBuildData buildData)
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData);
            return SetSqlBuildHelperValues(helper, buildData);
        }
        public SqlBuildHelper CreateSqlBuildHelper_NonTransactional(SqlSyncBuildData buildData, bool withScriptLog)
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData, withScriptLog, string.Empty, false);
            return SetSqlBuildHelperValues(helper, buildData);
        }
        public SqlBuildHelper CreateSqlBuildHelper_Basic()
        {
            SqlBuildHelper helper = new SqlBuildHelper(connData);
            return SetSqlBuildHelperValues(helper, null);
        }

        public SqlBuildHelper CreateSqlBuildHelperAccessor(SqlSyncBuildData buildData)
        {
            SqlBuildHelper target = new SqlBuildHelper(connData);


            //Set fields
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
            target.bgWorker = bg;

            target.buildData = buildData;

            string logFile = GetTrulyUniqueFile();
            tempFiles.Add(logFile);
            target.scriptLogFileName = logFile;

            SqlSyncBuildData buildHist = CreateSqlSyncSqlBuildDataObject();
            target.buildHistoryData = buildHist;

            projectFileName = GetTrulyUniqueFile();
            tempFiles.Add(projectFileName);
            target.projectFileName = projectFileName;

            buildHistoryXmlFile = GetTrulyUniqueFile();
            tempFiles.Add(buildHistoryXmlFile);
            target.buildHistoryXmlFile = buildHistoryXmlFile;

            return target;
        }
        public SqlBuildHelper SetSqlBuildHelperValues(SqlBuildHelper sbh, SqlSyncBuildData buildData)
        {
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
            sbh.bgWorker = bg;

            sbh.buildData = buildData;

            string logFile = GetTrulyUniqueFile();
            sbh.scriptLogFileName = logFile;

            SqlSyncBuildData buildHist = CreateSqlSyncSqlBuildDataObject();
            sbh.buildHistoryData = buildHist;

            projectFileName = GetTrulyUniqueFile();
            sbh.projectFileName = projectFileName;

            buildHistoryXmlFile = GetTrulyUniqueFile();
            sbh.buildHistoryXmlFile = buildHistoryXmlFile;
            return sbh;
        }
        public SqlSyncBuildData.BuildRow GetRunBuildRow(SqlBuildHelper sqlBuildHelper)
        {
            SqlSyncBuildData histData = sqlBuildHelper.buildHistoryData;
            return histData.Build.NewBuildRow();
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

        public void AddScriptForProcessBuild(ref SqlSyncBuildData data, bool multipleRun, int scriptTimeout)
        {
            SqlSyncBuildData.ScriptRow row = data.Script.NewScriptRow();
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
            row.SetParentRow(data.Scripts[0]);
            data.Script.AddScriptRow(row);
            data.Script.AcceptChanges();
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
        public BackgroundWorker GetBackgroundWorker()
        {
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
            return bg;
        }
        public List<DatabaseOverride> GetDatabaseOverrides()
        {
            List<DatabaseOverride> o = new List<DatabaseOverride>();
            o.Add(new DatabaseOverride(serverName, testDatabaseNames[0], testDatabaseNames[0]));
            return o;

        }
        public SqlBuildRunData GetSqlBuildRunData_TransactionalNotTrial(SqlSyncBuildData buildData)
        {
            SqlBuildRunData runData = new SqlBuildRunData()
            {
                BuildData = buildData,
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

        public int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(String.Format(connectionString, testDatabaseNames[databaseIndex])))
                {
                    SqlCommand cmd = new SqlCommand("SELECT count(*) FROM SqlBuild_Logging WHERE BuildFileName = @BuildFileName", conn);
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
