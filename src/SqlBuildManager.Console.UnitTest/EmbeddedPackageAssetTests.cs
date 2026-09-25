using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class EmbeddedPackageAssetTests
    {
        public static IEnumerable<object[]> Packages() =>
            typeof(EmbeddedPackageAssetTests).Assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith(".sbm", StringComparison.OrdinalIgnoreCase))
                .Select(name => new object[] { name });

        [TestMethod]
        public void IncludesIntegrationAndAzurePackageAssets()
        {
            var names = Packages().Select(row => (string)row[0]).ToList();
            Assert.IsTrue(names.Any(name => name.Contains("SqlServer.IntegrationTest")));
            Assert.IsTrue(names.Any(name => name.Contains("PostgreSQL.IntegrationTest")));
            Assert.IsTrue(names.Any(name => name.Contains("MySQL.IntegrationTest")));
            Assert.IsTrue(names.Any(name => name.Contains("AzureTest.TestBase")));
            Assert.IsTrue(names.Any(name => name.Contains("SqlServer.AzureTest")));
        }

        [TestMethod]
        [DynamicData(nameof(Packages))]
        public void EmbeddedPackage_ContainsOnlySafeAndCompleteMembers(string resourceName)
        {
            using var stream = typeof(EmbeddedPackageAssetTests).Assembly.GetManifestResourceStream(resourceName);
            Assert.IsNotNull(stream);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in archive.Entries)
            {
                var name = PackagePath.NormalizeRelativePath(entry.FullName);
                Assert.IsTrue(names.Add(name), $"Duplicate archive entry: {entry.FullName}");
            }

            var project = archive.GetEntry("SqlSyncBuildProject.xml");
            Assert.IsNotNull(project, $"Missing project XML in {resourceName}");
            using var xml = project.Open();
            var model = SqlSyncBuildDataXmlSerializer.Load(XDocument.Load(xml));
            foreach (var script in model.Script)
            {
                Assert.IsNotNull(script.FileName);
                PackagePath.NormalizeRelativePath(script.FileName);
                Assert.IsNotNull(archive.GetEntry(script.FileName),
                    $"Missing script '{script.FileName}' in {resourceName}");
            }
        }
    }
}
