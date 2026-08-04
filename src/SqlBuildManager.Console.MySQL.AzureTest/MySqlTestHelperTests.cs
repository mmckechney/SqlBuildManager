using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Console.MySQL.AzureTest
{
    [TestClass]
    public class MySqlTestHelperTests
    {
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
