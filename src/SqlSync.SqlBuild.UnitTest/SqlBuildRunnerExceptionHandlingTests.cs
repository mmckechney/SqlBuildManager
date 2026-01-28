using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerExceptionHandlingTests
    {
        [TestMethod]
        [Ignore("SqlException created via reflection doesn't properly set Message property, so timeout detection via error message doesn't work. Error number check also doesn't work with the reflection-created exception. This test needs to be updated to either use a real SqlException or a different mocking approach.")]
        public void Run_MarksTimeoutFailure_WhenSqlExceptionTimeout()
        {
            var bg = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            var ctx = new FakeRunnerContext(bg) { IsTransactionalValue = true };
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                new SqlCommandExecutorThatThrows(CreateTimeoutException()),
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());

            var scripts = new List<BuildModels.Script>
            {
                new BuildModels.Script(
                    FileName: "file.sql",
                    BuildOrder: 1,
                    Description: null,
                    RollBackOnError: true,
                    CausesBuildFailure: true,
                    DateAdded: null,
                    ScriptId: "abc",
                    Database: "db",
                    StripTransactionText: false,
                    AllowMultipleRuns: true,
                    AddedBy: null,
                    ScriptTimeOut: 5,
                    DateModified: null,
                    ModifiedBy: null,
                    Tag: null)
            };
            var myBuild = new BuildModels.Build(
                Name: "name",
                BuildType: "type",
                BuildStart: DateTime.UtcNow,
                BuildEnd: null,
                ServerName: "srv",
                FinalStatus: null,
                BuildId: Guid.NewGuid().ToString(),
                UserId: "user");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel() with { Script = scripts };
            var e = new DoWorkEventArgs(null);

            var result = runner.Run(scripts, myBuild, "srv", isMultiDbRun: false, scriptBatchColl: new ScriptBatchCollection { new ScriptBatch("file.sql", new[] { "SELECT 1;" }, "abc") }, buildDataModel: model, ref e);

            Assert.AreEqual(BuildItemStatus.FailedDueToScriptTimeout, result.FinalStatus);
            Assert.IsTrue(ctx.ErrorOccured);
        }

        private static SqlException CreateTimeoutException()
        {
            // Build a SqlError
            var errorCtor = typeof(SqlError)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(c => c.GetParameters().Length == 9);
            var sqlError = (SqlError)errorCtor.Invoke(new object[]
            {
                -2, // infoNumber (-2 is the error number for timeout)
                (byte)0, // errorState
                (byte)0, // errorClass
                "server", // server
                "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", // errorMessage
                "proc", // procedure
                0, // lineNumber
                (uint)0, // win32ErrorCode
                null // exception
            });

            var errorCollection = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), true);
            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(errorCollection, new object[] { sqlError });

            var exceptionFactory = typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(SqlErrorCollection), typeof(string) }, null);
            var sqlException = (SqlException)exceptionFactory.Invoke(null, new object[] { errorCollection, "7.0.0" });
            return sqlException;
        }

        private sealed class SqlCommandExecutorThatThrows : ISqlCommandExecutor
        {
            private readonly Exception _ex;
            public SqlCommandExecutorThatThrows(Exception ex) => _ex = ex;
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => throw _ex;
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
                => Task.Run(() => Execute(sql, timeoutSeconds, cData, isTransactional), cancellationToken);
        }

    }
}
