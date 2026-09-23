using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.AzureTest.TestBase;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Connection;
using System;
using System.IO;
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
