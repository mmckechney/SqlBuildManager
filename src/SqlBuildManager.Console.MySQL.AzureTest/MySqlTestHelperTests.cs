using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Connection;

namespace SqlBuildManager.Console.MySQL.AzureTest
{
    [TestClass]
    public class MySqlTestHelperTests
    {
        [TestMethod]
        [DataRow(AuthenticationType.ManagedIdentity, "", true)]
        [DataRow(AuthenticationType.AzureADDefault, "", true)]
        [DataRow(AuthenticationType.Password, "local-only", false)]
        [DataRow(AuthenticationType.ManagedIdentity, "stale-password", false)]
        public void AzureSettingsRequireIdentityWithoutPassword(AuthenticationType authType, string password, bool valid)
        {
            var settings = new CommandLineArgs();
            settings.AuthenticationArgs.DatabasePlatform = DatabasePlatform.MySQL;
            settings.AuthenticationArgs.AuthenticationType = authType;
            settings.AuthenticationArgs.Password = password;
            settings.IdentityArgs.ClientId = "11111111-1111-1111-1111-111111111111";
            settings.IdentityArgs.IdentityName = "id-test";
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize(settings));
                if (valid)
                {
                    Assert.AreEqual(Path.GetFullPath(path), MySqlTestHelper.RequireManagedIdentitySettings(path));
                }
                else
                {
                    Assert.ThrowsExactly<InvalidDataException>(() => MySqlTestHelper.RequireManagedIdentitySettings(path));
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        [DataRow(DatabasePlatform.SqlServer, "id-test", "client-id", "", false)]
        [DataRow(DatabasePlatform.MySQL, "", "client-id", "", false)]
        [DataRow(DatabasePlatform.MySQL, "id-test", "", "", false)]
        [DataRow(DatabasePlatform.MySQL, "id-test", "", "sa-test", true)]
        public void AzureSettingsValidatePlatformAndIdentity(DatabasePlatform platform, string identityName, string clientId, string serviceAccount, bool valid)
        {
            var settings = new CommandLineArgs();
            settings.AuthenticationArgs.DatabasePlatform = platform;
            settings.AuthenticationArgs.AuthenticationType = AuthenticationType.ManagedIdentity;
            settings.IdentityArgs.IdentityName = identityName;
            settings.IdentityArgs.ClientId = clientId;
            settings.IdentityArgs.ServiceAccountName = serviceAccount;
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize(settings));
                if (valid)
                {
                    Assert.AreEqual(Path.GetFullPath(path), MySqlTestHelper.RequireManagedIdentitySettings(path));
                }
                else
                {
                    Assert.ThrowsExactly<InvalidDataException>(() => MySqlTestHelper.RequireManagedIdentitySettings(path));
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void GeneratedPackagesUseMySqlDatabaseMetadata()
        {
            var simplePackage = MySqlTestHelper.GetMySqlSimpleSelectSbm();
            var doubleClientPackage = MySqlTestHelper.GetMySqlSimpleSelectDoubleClientSbm();

            try
            {
                AssertPackageUsesDatabases(simplePackage, "sbm_mysql_test");
                AssertPackageUsesDatabases(doubleClientPackage, "Client", "Client2");

                using var archive = ZipFile.OpenRead(doubleClientPackage);
                var sqlContents = string.Join(
                    "\n",
                    archive.Entries
                        .Where(entry => entry.FullName.EndsWith(".sql", System.StringComparison.OrdinalIgnoreCase))
                        .Select(entry =>
                        {
                            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                            return reader.ReadToEnd();
                        }));

                StringAssert.Contains(sqlContents, "DATABASE()");
                Assert.IsFalse(sqlContents.Contains("current_database()", System.StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                if (File.Exists(simplePackage))
                {
                    File.Delete(simplePackage);
                }

                if (File.Exists(doubleClientPackage))
                {
                    File.Delete(doubleClientPackage);
                }
            }
        }

        private static void AssertPackageUsesDatabases(string packagePath, params string[] expectedDatabases)
        {
            using var archive = ZipFile.OpenRead(packagePath);
            var projectEntry = archive.Entries.Single(entry =>
                entry.FullName.EndsWith("SqlSyncBuildProject.xml", System.StringComparison.OrdinalIgnoreCase));
            using var reader = new StreamReader(projectEntry.Open(), Encoding.UTF8);
            var projectXml = reader.ReadToEnd();

            foreach (var expectedDatabase in expectedDatabases)
            {
                StringAssert.Contains(projectXml, $"Database=\"{expectedDatabase}\"");
            }

            Assert.IsFalse(projectXml.Contains("sbm_pg_test", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
