using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Interfaces.Console;
using SqlBuildManager.SqlBuild;
using SqlBuildManager.SqlBuild.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class UnpackPackageTest
    {
        [TestMethod]
        [DataRow("present.sql", true)]
        [DataRow("../outside.sql", false)]
        [DataRow("missing.sql", false)]
        public async Task UnpackSbmFile_ReturnsExtractionStatusAndPreservesCallerFiles(string reference, bool valid)
        {
            var root = Path.Combine(Path.GetTempPath(), "sbm-unpack-" + Guid.NewGuid().ToString("N"));
            var destination = Path.Combine(root, "output");
            Directory.CreateDirectory(destination);
            try
            {
                var sentinel = Path.Combine(destination, "keep.txt");
                await File.WriteAllTextAsync(sentinel, "keep me");
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                model.Script.Add(new Script { FileName = reference });
                var xml = Path.Combine(root, XmlFileNames.MainProjectFile);
                await SqlSyncBuildDataXmlSerializer.SaveAsync(xml, model);
                var package = Path.Combine(root, "test.sbm");
                using (var archive = ZipFile.Open(package, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(xml, XmlFileNames.MainProjectFile);
                    using var writer = new StreamWriter(archive.CreateEntry("present.sql").Open());
                    writer.Write("SELECT 1;");
                }

                var code = await Worker.UnpackSbmFile(new DirectoryInfo(destination), new FileInfo(package));

                Assert.AreEqual((int)(valid ? ExecutionReturn.Successful : ExecutionReturn.BuildFileExtractionError), code);
                Assert.AreEqual("keep me", await File.ReadAllTextAsync(sentinel));
                Assert.AreEqual(valid, File.Exists(Path.Combine(destination, "test.sbx")));
                Assert.AreEqual(valid, File.Exists(Path.Combine(destination, "present.sql")));
                Assert.IsFalse(File.Exists(Path.Combine(destination, XmlFileNames.MainProjectFile)));
                Assert.AreEqual(valid ? 3 : 1, Directory.GetFiles(destination).Length);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}
