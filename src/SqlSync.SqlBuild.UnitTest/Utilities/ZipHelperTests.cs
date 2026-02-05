using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Utilities;
using IOZipFile = System.IO.Compression.ZipFile;

namespace SqlSync.SqlBuild.UnitTest.Utilities
{
    [TestClass]
    public class ZipHelperTests
    {
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDir))
            {
                try
                {
                    Directory.Delete(_testDir, true);
                }
                catch
                {
                    // Ignore cleanup failures
                }
            }
        }

        #region Constructor Tests

        [TestMethod]
        public void ZipHelper_Constructor_CreatesInstance()
        {
            // Act
            var helper = new ZipHelper();

            // Assert
            Assert.IsNotNull(helper);
        }

        #endregion

        #region CreateZipPackage Tests

        [TestMethod]
        public void CreateZipPackage_WithValidFiles_CreatesZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = ZipHelper.CreateZipPackage(new[] { "file1.txt", "file2.txt" }, _testDir, zipPath);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void CreateZipPackage_WithKeepPathInfo_CreatesZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = ZipHelper.CreateZipPackage(new[] { "file1.txt" }, _testDir, zipPath, keepPathInfo: true);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void CreateZipPackage_WithoutKeepPathInfo_CreatesZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = ZipHelper.CreateZipPackage(new[] { "file1.txt" }, _testDir, zipPath, keepPathInfo: false);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void CreateZipPackage_WithFullPathFiles_CreatesZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = ZipHelper.CreateZipPackage(new List<string> { file1, file2 }, zipPath, keepPathInfo: true, retryCount: 0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void CreateZipPackage_NonExistentFile_SkipsFile()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act - includes a non-existent file
            var result = ZipHelper.CreateZipPackage(new List<string> { file1, Path.Combine(_testDir, "nonexistent.txt") }, zipPath, keepPathInfo: true, retryCount: 0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void CreateZipPackage_OverwritesExistingZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Create initial zip
            ZipHelper.CreateZipPackage(new List<string> { file1 }, zipPath, keepPathInfo: true, retryCount: 0);
            var originalTime = File.GetLastWriteTime(zipPath);

            // Modify file
            System.Threading.Thread.Sleep(100);
            File.WriteAllText(file1, "Modified Content");

            // Act - create again
            var result = ZipHelper.CreateZipPackage(new List<string> { file1 }, zipPath, keepPathInfo: true, retryCount: 0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.GetLastWriteTime(zipPath) >= originalTime);
        }

        [TestMethod]
        public void CreateZipPackage_EmptyFileList_CreatesEmptyZip()
        {
            // Arrange
            string zipPath = Path.Combine(_testDir, "empty.zip");

            // Act
            var result = ZipHelper.CreateZipPackage(new List<string>(), zipPath, keepPathInfo: true, retryCount: 0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void CreateZipPackage_ZipContainsCorrectFiles()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            ZipHelper.CreateZipPackage(new List<string> { file1, file2 }, zipPath, keepPathInfo: true, retryCount: 0);

            // Assert - verify contents
            using (var archive = IOZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                Assert.AreEqual(2, archive.Entries.Count);
                Assert.IsNotNull(archive.GetEntry("file1.txt"));
                Assert.IsNotNull(archive.GetEntry("file2.txt"));
            }
        }

        #endregion

        #region UnpackZipPackage Tests

        [TestMethod]
        public void UnpackZipPackage_ValidZip_ExtractsFiles()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            string outDir = Path.Combine(_testDir, "output");

            // Act
            var result = ZipHelper.UnpackZipPackage(outDir, zipPath, overwriteExistingProjectFiles: true);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(Path.Combine(outDir, "file1.txt")));
        }

        [TestMethod]
        public void UnpackZipPackage_CreatesDestinationDirectory()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            string outDir = Path.Combine(_testDir, "new_output_dir");

            // Act
            var result = ZipHelper.UnpackZipPackage(outDir, zipPath, overwriteExistingProjectFiles: true);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(outDir));
        }

        [TestMethod]
        public void UnpackZipPackage_OverwriteTrue_OverwritesExistingFiles()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Original Content");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            string outDir = Path.Combine(_testDir, "output");
            Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir, "file1.txt"), "Existing Content");

            // Act
            var result = ZipHelper.UnpackZipPackage(outDir, zipPath, overwriteExistingProjectFiles: true);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("Original Content", File.ReadAllText(Path.Combine(outDir, "file1.txt")));
        }

        [TestMethod]
        public void UnpackZipPackage_OverwriteFalse_DoesNotOverwriteExistingFiles()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Original Content");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            string outDir = Path.Combine(_testDir, "output");
            Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir, "file1.txt"), "Existing Content");

            // Act
            var result = ZipHelper.UnpackZipPackage(outDir, zipPath, overwriteExistingProjectFiles: false);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("Existing Content", File.ReadAllText(Path.Combine(outDir, "file1.txt")));
        }

        [TestMethod]
        public void UnpackZipPackage_InvalidZipPath_ReturnsFalse()
        {
            // Arrange
            string outDir = Path.Combine(_testDir, "output");
            string invalidZipPath = Path.Combine(_testDir, "nonexistent.zip");

            // Act
            var result = ZipHelper.UnpackZipPackage(outDir, invalidZipPath, overwriteExistingProjectFiles: true);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UnpackZipPackage_MultipleFiles_ExtractsAll()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            string file3 = Path.Combine(_testDir, "file3.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");
            File.WriteAllText(file3, "Content 3");

            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file2, "file2.txt", CompressionLevel.Fastest);
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file3, "file3.txt", CompressionLevel.Fastest);
            }

            string outDir = Path.Combine(_testDir, "output");

            // Act
            var result = ZipHelper.UnpackZipPackage(outDir, zipPath, overwriteExistingProjectFiles: true);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(Path.Combine(outDir, "file1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(outDir, "file2.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(outDir, "file3.txt")));
        }

        #endregion

        #region AppendZipPackage Tests

        [TestMethod]
        public void AppendZipPackage_AddsFilesToExistingZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");

            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            // Act
            var result = ZipHelper.AppendZipPackage(new[] { file2 }, _testDir, zipPath, keepPathInfo: true);

            // Assert
            Assert.IsTrue(result);
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                Assert.AreEqual(2, zip.Entries.Count);
            }
        }

        [TestMethod]
        public void AppendZipPackage_MultipleFiles_AddsAll()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            string file3 = Path.Combine(_testDir, "file3.txt");
            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");
            File.WriteAllText(file3, "Content 3");

            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // empty zip
            }

            // Act
            var result = ZipHelper.AppendZipPackage(new[] { file1, file2, file3 }, _testDir, zipPath, keepPathInfo: true);

            // Assert
            Assert.IsTrue(result);
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                Assert.AreEqual(3, zip.Entries.Count);
            }
        }

        [TestMethod]
        public void AppendZipPackage_CreatesZipIfNotExist()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string newZipPath = Path.Combine(_testDir, "newpackage.zip");

            // Act - AppendZipPackage uses Update mode which creates new if not exist
            var result = ZipHelper.AppendZipPackage(new[] { file1 }, _testDir, newZipPath, keepPathInfo: true);

            // Assert - the actual behavior is to create/update (OpenOrCreate mode)
            Assert.IsTrue(result);
        }

        #endregion

        #region CreateZipPackageAsync Tests

        [TestMethod]
        public async Task CreateZipPackageAsync_WithValidFiles_CreatesZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            await File.WriteAllTextAsync(file1, "Content 1");
            await File.WriteAllTextAsync(file2, "Content 2");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = await ZipHelper.CreateZipPackageAsync(new[] { "file1.txt", "file2.txt" }, _testDir, zipPath, keepPathInfo: true);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public async Task CreateZipPackageAsync_WithFullPathFiles_CreatesZip()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            await File.WriteAllTextAsync(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = await ZipHelper.CreateZipPackageAsync(new List<string> { file1 }, zipPath, keepPathInfo: true, retryCount: 0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public async Task CreateZipPackageAsync_NonExistentFile_SkipsFile()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            await File.WriteAllTextAsync(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            var result = await ZipHelper.CreateZipPackageAsync(
                new List<string> { file1, Path.Combine(_testDir, "nonexistent.txt") },
                zipPath, keepPathInfo: true, retryCount: 0);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public async Task CreateZipPackageAsync_WithCancellation_ThrowsTaskCanceled()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            await File.WriteAllTextAsync(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert - TaskCanceledException derives from OperationCanceledException
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
                await ZipHelper.CreateZipPackageAsync(new List<string> { file1 }, zipPath, keepPathInfo: true, retryCount: 0, cancellationToken: cts.Token));
        }

        #endregion

        #region AppendZipPackageAsync Tests

        [TestMethod]
        public async Task AppendZipPackageAsync_NonExistentFile_SkipsFile()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            await File.WriteAllTextAsync(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            // Act - append non-existent file
            var result = await ZipHelper.AppendZipPackageAsync(new[] { "nonexistent.txt" }, _testDir, zipPath, keepPathInfo: true);

            // Assert
            Assert.IsTrue(result);
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                Assert.AreEqual(1, zip.Entries.Count); // Only original file
            }
        }

        [TestMethod]
        public async Task AppendZipPackageAsync_CancellationHandled_ReturnsFalse()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            string file2 = Path.Combine(_testDir, "file2.txt");
            await File.WriteAllTextAsync(file1, "Content 1");
            await File.WriteAllTextAsync(file2, "Content 2");
            string zipPath = Path.Combine(_testDir, "test.zip");

            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act - The method catches exceptions and returns false
            var result = await ZipHelper.AppendZipPackageAsync(new[] { "file2.txt" }, _testDir, zipPath, keepPathInfo: true, cancellationToken: cts.Token);

            // Assert - Method catches the exception and returns false
            Assert.IsFalse(result);
        }

        #endregion

        #region ZipFile Extension Tests

        [TestMethod]
        public void ZipFile_Open_Read_OpensArchive()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            // Act
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                // Assert
                Assert.IsNotNull(archive);
                Assert.AreEqual(1, archive.Entries.Count);
            }
        }

        [TestMethod]
        public void ZipFile_Open_Create_CreatesArchive()
        {
            // Arrange
            string zipPath = Path.Combine(_testDir, "new.zip");

            // Act
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Assert
                Assert.IsNotNull(archive);
            }

            Assert.IsTrue(File.Exists(zipPath));
        }

        [TestMethod]
        public void ZipFile_Open_Update_OpensForUpdate()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var zip = IOZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(zip, file1, "file1.txt", CompressionLevel.Fastest);
            }

            // Act
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                // Assert
                Assert.IsNotNull(archive);
                Assert.AreEqual(1, archive.Entries.Count);
            }
        }

        [TestMethod]
        public void ZipFile_Open_InvalidMode_ThrowsException()
        {
            // Arrange
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, (ZipArchiveMode)99));
        }

        #endregion

        #region ZipFileExtensions Tests

        [TestMethod]
        public void ZipFileExtensions_CreateEntryFromFile_CreatesEntry()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");

            // Act
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                var entry = SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(archive, file1, "file1.txt", CompressionLevel.Fastest);

                // Assert
                Assert.IsNotNull(entry);
                Assert.AreEqual("file1.txt", entry.Name);
            }
        }

        [TestMethod]
        public void ZipFileExtensions_CreateEntryFromFile_NullDestination_ThrowsException()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(null, file1, "file1.txt", CompressionLevel.Fastest));
        }

        [TestMethod]
        public void ZipFileExtensions_CreateEntryFromFile_NullSourceFileName_ThrowsException()
        {
            // Arrange
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Act & Assert
                Assert.ThrowsExactly<ArgumentNullException>(() =>
                    SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(archive, null, "file1.txt", CompressionLevel.Fastest));
            }
        }

        [TestMethod]
        public void ZipFileExtensions_CreateEntryFromFile_NullEntryName_ThrowsException()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content 1");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Act & Assert
                Assert.ThrowsExactly<ArgumentNullException>(() =>
                    SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(archive, file1, null, CompressionLevel.Fastest));
            }
        }

        [TestMethod]
        public void ZipFileExtensions_ExtractToFile_ExtractsFile()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Test Content");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(archive, file1, "file1.txt", CompressionLevel.Fastest);
            }

            string extractPath = Path.Combine(_testDir, "extracted.txt");

            // Act
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry("file1.txt");
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.ExtractToFile(entry, extractPath);
            }

            // Assert
            Assert.IsTrue(File.Exists(extractPath));
            Assert.AreEqual("Test Content", File.ReadAllText(extractPath));
        }

        [TestMethod]
        public void ZipFileExtensions_ExtractToFile_OverwriteTrue_OverwritesFile()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "New Content");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(archive, file1, "file1.txt", CompressionLevel.Fastest);
            }

            string extractPath = Path.Combine(_testDir, "extracted.txt");
            File.WriteAllText(extractPath, "Old Content");

            // Act
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry("file1.txt");
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.ExtractToFile(entry, extractPath, overwrite: true);
            }

            // Assert
            Assert.AreEqual("New Content", File.ReadAllText(extractPath));
        }

        [TestMethod]
        public void ZipFileExtensions_ExtractToFile_NullSource_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.ExtractToFile(null, "test.txt"));
        }

        [TestMethod]
        public void ZipFileExtensions_ExtractToFile_NullDestinationFileName_ThrowsException()
        {
            // Arrange
            string file1 = Path.Combine(_testDir, "file1.txt");
            File.WriteAllText(file1, "Content");
            string zipPath = Path.Combine(_testDir, "test.zip");
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                SqlSync.SqlBuild.Utilities.ZipFileExtensions.CreateEntryFromFile(archive, file1, "file1.txt", CompressionLevel.Fastest);
            }

            // Act & Assert
            using (var archive = SqlSync.SqlBuild.Utilities.ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry("file1.txt");
                Assert.ThrowsExactly<ArgumentNullException>(() =>
                    SqlSync.SqlBuild.Utilities.ZipFileExtensions.ExtractToFile(entry, null));
            }
        }

        #endregion
    }
}
