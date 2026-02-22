using SqlBuildManager.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlSync.SqlBuild.Dependent.TestBase
{
    /// <summary>
    /// Base class for platform-specific test initialization helpers.
    /// Contains shared logic for setting up SqlBuildHelper, creating test scripts,
    /// managing temp files, and querying test results.
    /// Platform-specific subclasses implement database creation, table DDL, and queries.
    /// </summary>
    public abstract class InitializationBase : IDisposable
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        public List<string> testDatabaseNames = null!;
        public List<string> tempFiles = null!;
        public Guid testGuid;
        public DateTime testTimeStamp;
        public ConnectionData connData = null!;
        public string projectFileName = null!;
        public string buildHistoryXmlFile = null!;
        public string serverName = string.Empty;

        /// <summary>
        /// The table name used for test INSERT/SELECT scripts. 
        /// SQL Server uses "TransactionTest", PostgreSQL uses "transactiontest".
        /// </summary>
        protected abstract string TestTableName { get; }

        protected InitializationBase()
        {
            tempFiles = new List<string>();
            testGuid = Guid.NewGuid();
            testTimeStamp = DateTime.Now;
        }

        /// <summary>
        /// Create all test databases if they don't exist.
        /// </summary>
        public abstract bool CreateDatabases();

        /// <summary>
        /// Create the test table (TransactionTest / transactiontest) in all test databases.
        /// </summary>
        public abstract bool CreateTestTables();

        /// <summary>
        /// Create the build logging table in all test databases.
        /// </summary>
        public abstract bool CreateSqlBuildLoggingTables();

        /// <summary>
        /// Clean old data from test tables.
        /// </summary>
        public abstract bool CleanTestTables();

        /// <summary>
        /// Clean old data from logging tables.
        /// </summary>
        public abstract bool CleanSqlBuildLoggingTables();

        /// <summary>
        /// Get the row count from the logging table for the current build file.
        /// </summary>
        public abstract int GetSqlBuildLoggingRowCountByBuildFileName(int databaseIndex);

        /// <summary>
        /// Get the row count from the test table for the current test guid.
        /// </summary>
        public abstract int GetTestTableRowCount(int databaseIndex);

        /// <summary>
        /// Create a ConnectionData for a specific database, using the platform-specific settings.
        /// </summary>
        public abstract ConnectionData CreateConnectionData(string databaseName);

        public SqlSyncBuildDataModel CreateSqlSyncSqlBuildDataModelObject()
        {
            try
            {
                return SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            }
            catch
            {
                return null!;
            }
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
            return SetSqlBuildHelperValues(helper, null!);
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

        public List<DatabaseOverride> GetDatabaseOverrides()
        {
            return new List<DatabaseOverride>
            {
                new DatabaseOverride(serverName, testDatabaseNames[0], testDatabaseNames[0])
            };
        }

        public void AddInsertScript(ref SqlSyncBuildDataModel data, bool multipleRun)
        {
            var row = new Script
            {
                AllowMultipleRuns = multipleRun,
                BuildOrder = 1,
                CausesBuildFailure = true,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = true,
                StripTransactionText = true,
                Description = "Test Script to successfully insert into test table",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            string script = GetInsertScript();
            File.WriteAllText(row.FileName, script);
        }

        public void AddSelectScript(ref SqlSyncBuildDataModel data)
        {
            var row = new Script
            {
                AllowMultipleRuns = true,
                BuildOrder = 1,
                CausesBuildFailure = true,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = true,
                StripTransactionText = true,
                Description = "Test Script to successfully select from test table",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            File.WriteAllText(row.FileName, $"SELECT * FROM {TestTableName}");
        }

        public void AddFailureScript(ref SqlSyncBuildDataModel data, bool rollBackOnError, bool causeBuildFailure)
        {
            var row = new Script
            {
                AllowMultipleRuns = true,
                BuildOrder = 10,
                CausesBuildFailure = causeBuildFailure,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = rollBackOnError,
                StripTransactionText = true,
                Description = "Test Script to cause a failure",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            string script = GetFailureScript();
            File.WriteAllText(row.FileName, script);
        }

        /// <summary>
        /// Get a platform-appropriate INSERT script for the test table.
        /// </summary>
        protected virtual string GetInsertScript()
        {
            return $"INSERT INTO {TestTableName} (Message, Guid, DateTimeStamp) VALUES ('INSERT TEST','{testGuid}','{testTimeStamp:yyyy-MM-dd HH:mm:ss}')";
        }

        /// <summary>
        /// Get a platform-appropriate script that will cause a failure (invalid column name).
        /// </summary>
        protected virtual string GetFailureScript()
        {
            return $"INSERT INTO {TestTableName} (INVALID, Guid, DateTimeStamp) VALUES ('INSERT TEST','{testGuid}','{testTimeStamp:yyyy-MM-dd HH:mm:ss}')";
        }

        public void AddScriptWithBadDatabase(ref SqlSyncBuildDataModel data)
        {
            var row = new Script
            {
                AllowMultipleRuns = true,
                BuildOrder = 1,
                CausesBuildFailure = true,
                Database = "REALLY_BAD_DATABASE",
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = true,
                StripTransactionText = true,
                Description = "Test Script that has an invalid database name",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            File.WriteAllText(row.FileName, GetInsertScript());
        }

        public void AddBatchInsertScripts(ref SqlSyncBuildDataModel data, bool multipleRun)
        {
            var row = new Script
            {
                AllowMultipleRuns = multipleRun,
                BuildOrder = 1,
                CausesBuildFailure = true,
                Database = testDatabaseNames[0],
                DateAdded = testTimeStamp,
                FileName = GetTrulyUniqueFile(),
                RollBackOnError = true,
                StripTransactionText = true,
                Description = "Test Script to successfully insert into test table (batched)",
                AddedBy = "UnitTest",
                ScriptId = Guid.NewGuid().ToString()
            };

            data.Script.Add(row);
            string script = GetInsertScript();
            script = script + "\r\nGO\r\n" + script;
            File.WriteAllText(row.FileName, script);
        }

        public string GetTrulyUniqueFile(string extension = "tmp")
        {
            string newName = TestFileHelper.GetTrulyUniqueFile(extension);
            tempFiles.Add(newName);
            return newName;
        }

        public virtual void Dispose()
        {
            if (tempFiles != null)
            {
                foreach (var file in tempFiles)
                {
                    if (File.Exists(file))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }

            if (File.Exists(projectFileName))
                try { File.Delete(projectFileName); } catch { }
            if (File.Exists(buildHistoryXmlFile))
                try { File.Delete(buildHistoryXmlFile); } catch { }
        }
    }
}
