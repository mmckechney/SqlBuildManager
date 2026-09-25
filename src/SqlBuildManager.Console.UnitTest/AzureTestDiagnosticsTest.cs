using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.AzureTest.TestBase;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Connection;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class AzureTestDiagnosticsTest
    {
        [TestMethod]
        public void DatabaseSetup_UsesWorkerSettings_ForDirectConnectionsAndDacpac()
        {
            var path = Path.Combine(Path.GetTempPath(), $"sbm-worker-settings-{Guid.NewGuid():N}.json");
            const string worker = "11111111-1111-1111-1111-111111111111";
            var settings = new CommandLineArgs();
            settings.AuthenticationArgs.AuthenticationType = AuthenticationType.ManagedIdentity;
            settings.IdentityArgs.ClientId = worker;
            settings.ConnectionArgs.RelayProxyEndpoint = "https://example.servicebus.windows.net/test";
            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize(settings));
                var cmd = new CommandLineArgs();
                cmd.IdentityArgs.ClientId = "22222222-2222-2222-2222-222222222222";
                DatabaseHelper.ConfigureRelayEndpoint(cmd, path, string.Empty);
                using var connection = DatabaseHelper.CreateSqlConnection(cmd, "test.database.windows.net", "db");
                var builder = new SqlConnectionStringBuilder(connection.ConnectionString);
                Assert.AreEqual(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity, builder.Authentication);
                Assert.AreEqual(worker, builder.UserID);
                Assert.IsFalse(builder.TrustServerCertificate);
                Assert.AreEqual(SqlConnectionEncryptOption.Mandatory, builder.Encrypt);
                Assert.AreEqual(settings.ConnectionArgs.RelayProxyEndpoint, cmd.ConnectionArgs.RelayProxyEndpoint);
                var args = DatabaseHelper.CreateDacpacArguments(cmd, "test.database.windows.net", "db", "output.dacpac");
                Assert.AreEqual(worker, args[Array.IndexOf(args, "--clientid") + 1]);
                Assert.AreEqual(AuthenticationType.ManagedIdentity.ToString(), args[Array.IndexOf(args, "--authtype") + 1]);
                Assert.AreEqual(0, CommandLineBuilder.SetUp().Parse(args).Errors.Count);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void DatabaseSetup_PreservesNativeAuthenticationAndLocalCertificateOption()
        {
            var cmd = new CommandLineArgs();
            cmd.AuthenticationArgs.AuthenticationType = AuthenticationType.Password;
            cmd.AuthenticationArgs.UserName = "local-user";
            cmd.AuthenticationArgs.Password = "local-test-only";
            cmd.AuthenticationArgs.TrustServerCertificate = true;
            using var connection = DatabaseHelper.CreateSqlConnection(cmd, "localhost", "db");
            var builder = new SqlConnectionStringBuilder(connection.ConnectionString);
            Assert.AreEqual(SqlAuthenticationMethod.SqlPassword, builder.Authentication);
            Assert.AreEqual("local-user", builder.UserID);
            Assert.IsTrue(builder.TrustServerCertificate);
            var args = DatabaseHelper.CreateDacpacArguments(cmd, "localhost", "db", "output.dacpac");
            Assert.AreEqual("local-user", args[Array.IndexOf(args, "--username") + 1]);
            Assert.AreEqual("true", args[Array.IndexOf(args, "--trustservercertificate") + 1]);
            Assert.AreEqual(0, CommandLineBuilder.SetUp().Parse(args).Errors.Count);
        }

        [TestMethod]
        [DataRow("Working/server/db/Error.log", true)]
        [DataRow("Working/server/dbError.log", true)]
        [DataRow("working/server/dbERROR.LOG", true)]
        [DataRow("Task1/Working/server/db/Error.log", true)]
        [DataRow("host/SqlBuildManager.Console.log", true)]
        [DataRow("Working/SELECT.sql", false)]
        [DataRow("Working/server/db/history.xml", false)]
        public void DiagnosticFilter_IncludesCurrentAndHistoricalDetailedErrors(string blobName, bool expected)
        {
            Assert.AreEqual(expected, BlobLogValidator.IsTaskExecutionLog(blobName));
        }

        [TestMethod]
        [DataRow(0, false)]
        [DataRow(1, false)]
        [DataRow(2, false)]
        [DataRow(3, false)]
        [DataRow(4, false)]
        [DataRow(5, false)]
        [DataRow(8, false)]
        [DataRow(0, true)]
        [DataRow(1, true)]
        [DataRow(2, true)]
        [DataRow(3, true)]
        [DataRow(4, true)]
        [DataRow(5, true)]
        [DataRow(8, true)]
        public void BuildSuccess_RequiresExactlyFourTaskErrors(int errorCount, bool customDacpac)
        {
            var validator = CreateSuccessfulBlobLogs();
            validator.TaskExecutionLogs["Task0/SqlBuildManager.Console.log"] = CreateTaskLog(errorCount, customDacpac);

            if (errorCount == 4)
            {
                validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4);
            }
            else
            {
                var error = Assert.ThrowsExactly<AssertFailedException>(() =>
                    validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4));
                StringAssert.Contains(error.Message, "exactly 4 ERR entries in total");
                StringAssert.Contains(error.Message, $"Found {errorCount}.");
            }
        }

        [TestMethod]
        [DataRow(4, 0)]
        [DataRow(0, 4)]
        [DataRow(2, 2)]
        [DataRow(4, 4)]
        public void BuildSuccess_CountsErrorsAcrossAllWorkers(int firstCount, int secondCount)
        {
            var validator = CreateSuccessfulBlobLogs();
            validator.TaskExecutionLogs["Task0/SqlBuildManager.Console.log"] = CreateTaskLog(firstCount);
            validator.TaskExecutionLogs["Task1/SqlBuildManager.Console.log"] = CreateTaskLog(secondCount);

            if (firstCount + secondCount == 4)
            {
                validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4);
            }
            else
            {
                var error = Assert.ThrowsExactly<AssertFailedException>(() =>
                    validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4));
                StringAssert.Contains(error.Message, "Found 8.");
                StringAssert.Contains(error.Message, "Task0/SqlBuildManager.Console.log");
            }
        }

        [TestMethod]
        public void BuildSuccess_RequiresErrorsEvenWhenTaskLogsAreMissing()
        {
            var validator = CreateSuccessfulBlobLogs();
            Assert.ThrowsExactly<AssertFailedException>(() =>
                validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4));
        }

        [TestMethod]
        public void BuildSuccess_PreservesExistingErrorExclusionsUnlessCountIsExplicit()
        {
            var validator = CreateSuccessfulBlobLogs();
            validator.AssertBuildSuccess(1);
            validator.TaskExecutionLogs["worker/SqlBuildManager.Console.log"] = CreateTaskLog(1);
            Assert.ThrowsExactly<AssertFailedException>(() => validator.AssertBuildSuccess(1));

            validator.TaskExecutionLogs["worker/SqlBuildManager.Console.log"] = CreateTaskLog(4, customDacpac: true);
            validator.AssertBuildSuccess(1);
            Assert.ThrowsExactly<AssertFailedException>(() =>
                validator.AssertBuildSuccess(1, expectedTaskErrorCount: 0));
        }

        [TestMethod]
        public void BuildSuccess_ExactCountPreservesTransientShutdownExclusions()
        {
            var validator = CreateSuccessfulBlobLogs();
            validator.TaskExecutionLogs["worker/SqlBuildManager.Console.log"] = CreateTaskLog(4) +
                "\n[2026-09-24 11:00:00.000 ERR TH: 1] MessagingEntityNotFound\n" +
                "[2026-09-24 11:00:00.000 ERR TH: 1] Problem getting messages from Service Bus\n" +
                "Exception continuation mentions ERR but is not a separate log entry.";
            validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4);
        }

        [TestMethod]
        [DataRow("ErrorsLog", "errors.log should be empty")]
        [DataRow("FailureDatabases", "failuredatabases.cfg should be empty")]
        [DataRow("DetailedError", "Per-target error")]
        [DataRow("CommitsLog", "commits.log should contain")]
        [DataRow("DatabaseCount", "Working/ directories should contain")]
        public void BuildSuccess_ExpectedTaskErrorsDoNotExcuseOtherFailures(string failure, string expectedMessage)
        {
            var validator = CreateSuccessfulBlobLogs();
            validator.TaskExecutionLogs["worker/SqlBuildManager.Console.log"] = CreateTaskLog(4);
            switch (failure)
            {
                case "DetailedError":
                    validator.TaskExecutionLogs["Working/server/db/Error.log"] = "Unexpected failure";
                    break;
                case "DatabaseCount":
                    validator.BlobNames.Clear();
                    break;
                default:
                    SetBlobLog(validator, failure, failure == "CommitsLog" ? string.Empty : "Unexpected failure");
                    break;
            }

            var error = Assert.ThrowsExactly<AssertFailedException>(() =>
                validator.AssertBuildSuccess(1, expectedTaskErrorCount: 4));
            StringAssert.Contains(error.Message, expectedMessage);
        }

        private static BlobLogValidator CreateSuccessfulBlobLogs()
        {
            var validator = new BlobLogValidator("offlinestorage", string.Empty, "offline-job");
            SetBlobLog(validator, nameof(BlobLogValidator.CommitsLog), "server/db: Committed");
            SetBlobLog(validator, nameof(BlobLogValidator.SuccessDatabases), "server:db");
            validator.BlobNames.Add("Working/server/db/SqlSyncBuildHistory.xml");
            return validator;
        }

        private static void SetBlobLog(BlobLogValidator validator, string name, string content)
        {
            var property = typeof(BlobLogValidator).GetProperty(name);
            Assert.IsNotNull(property);
            property.SetValue(validator, content);
        }

        private static string CreateTaskLog(int errorCount, bool customDacpac = false)
        {
            var prefix = customDacpac
                ? "[2026-09-24 11:00:00.000 WRN TH: 1] SqlSync.SqlBuild.Services.DefaultDacPacFallbackHandler - Custom dacpac required\n"
                : string.Empty;
            return prefix + string.Join("\n", Enumerable.Range(0, errorCount)
                .Select(index => $"[2026-09-24 11:00:00.000 ERR TH: 1] Expected recovery entry {index}"));
        }

        [TestMethod]
        public async Task SuccessfulCommand_DoesNotRequireDiagnosticAccess()
        {
            await BlobLogValidator.AssertCommandSucceededAsync(0, "missing-settings.json", "missing-key", "job");
        }

        [TestMethod]
        public async Task FailedDiagnosticCollection_PreservesCommandFailure()
        {
            var error = await Assert.ThrowsAsync<AssertFailedException>(() =>
                BlobLogValidator.AssertCommandSucceededAsync(27,
                    Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"), "missing-key", "job"));
            StringAssert.Contains(error.Message, "exit code 27");
            StringAssert.Contains(error.Message, "Unable to collect worker diagnostics");
        }
    }
}
