using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.SqlBuild;
using SqlBuildManager.SqlBuild.Utilities;
using IOZipFile = System.IO.Compression.ZipFile;

namespace SqlBuildManager.SqlBuild.UnitTest
{
    [TestClass]
    public class ZipHelperAsyncTests
    {
        [TestMethod]
        public async Task UnpackZipPackageAsync_ExtractsEntries()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpDir);
            var zipPath = Path.Combine(tmpDir, "test.zip");
            var filePath = Path.Combine(tmpDir, "file.txt");
            await File.WriteAllTextAsync(filePath, "hello");

            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, filePath, "file.txt", CompressionLevel.Fastest);
            }

            var outDir = Path.Combine(tmpDir, "out");
            Directory.CreateDirectory(outDir);
            var ok = await ZipHelper.UnpackZipPackageAsync(outDir, zipPath, overwriteExistingProjectFiles: true);
            Assert.IsTrue(ok);
            Assert.IsTrue(File.Exists(Path.Combine(outDir, "file.txt")));

            Directory.Delete(tmpDir, true);
        }

        [TestMethod]
        public async Task UnpackZipPackageAsync_ExistingEntryHasWrongLength_ReturnsFalse()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpDir);
            try
            {
                var zipPath = Path.Combine(tmpDir, "test.zip");
                var sourcePath = Path.Combine(tmpDir, "source.txt");
                await File.WriteAllTextAsync(sourcePath, "expected content");
                using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, sourcePath, "file.txt", CompressionLevel.Fastest);
                }

                var outDir = Path.Combine(tmpDir, "out");
                Directory.CreateDirectory(outDir);
                await File.WriteAllTextAsync(Path.Combine(outDir, "file.txt"), "bad");

                var ok = await ZipHelper.UnpackZipPackageAsync(outDir, zipPath, overwriteExistingProjectFiles: false);

                Assert.IsFalse(ok);
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }

        [TestMethod]
        public async Task AppendZipPackageAsync_AppendsFile()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpDir);
            var zipPath = Path.Combine(tmpDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // empty zip
            }

            var filePath = Path.Combine(tmpDir, "file.txt");
            await File.WriteAllTextAsync(filePath, "hello");

            var ok = await ZipHelper.AppendZipPackageAsync(new[] { "file.txt" }, tmpDir, zipPath, keepPathInfo: true);
            Assert.IsTrue(ok);

            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                Assert.IsNotNull(zip.GetEntry("file.txt"));
            }

            Directory.Delete(tmpDir, true);
        }
    }
}
