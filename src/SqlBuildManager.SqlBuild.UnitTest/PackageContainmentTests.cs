using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.Services;
using SqlBuildManager.SqlBuild.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using IOZipFile = System.IO.Compression.ZipFile;

namespace SqlBuildManager.SqlBuild.UnitTest
{
    [TestClass]
    public class PackageContainmentTests
    {
        private string root = null!;
        private string work = null!;
        private string sentinel = null!;

        [TestInitialize]
        public void Initialize()
        {
            root = Path.Combine(Path.GetTempPath(), "sbm-containment-" + Guid.NewGuid().ToString("N"));
            work = Path.Combine(root, "work");
            Directory.CreateDirectory(work);
            sentinel = Path.Combine(root, "outside.sql");
            File.WriteAllText(sentinel, "outside sentinel");
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(root, true);

        private static SqlSyncBuildDataModel Model(params string[] names)
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            foreach (var name in names)
                model.Script.Add(new Script { FileName = name, ScriptId = Guid.NewGuid().ToString(), BuildOrder = model.Script.Count + 1 });
            return model;
        }

        private async Task<string> Package(string reference, params (string Name, string Text)[] files)
        {
            var xml = Path.Combine(root, "source.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(xml, Model(reference));
            var path = Path.Combine(root, Guid.NewGuid() + ".sbm");
            using var archive = IOZipFile.Open(path, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(xml, XmlFileNames.MainProjectFile);
            foreach (var file in files)
            {
                using var writer = new StreamWriter(archive.CreateEntry(file.Name).Open());
                writer.Write(file.Text);
            }
            return path;
        }

        [TestMethod]
        [DataRow("../outside.sql")]
        [DataRow(@"..\outside.sql")]
        [DataRow("/outside.sql")]
        [DataRow(@"C:\outside.sql")]
        [DataRow(@"C:outside.sql")]
        [DataRow(@"\\server\share\outside.sql")]
        [DataRow("script.sql:stream")]
        [DataRow("dir/../../outside.sql")]
        [DataRow("script.sql.")]
        [DataRow("script.sql ")]
        [DataRow("NUL.sql")]
        [DataRow("COM1")]
        [DataRow("CON .sql")]
        [DataRow("CONOUT$")]
        [DataRow("")]
        public async Task InvalidMetadata_IsRejectedBeforeExtraction(string name)
        {
            var package = await Package(name, ("valid.sql", "SELECT 1"));
            var result = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, true);
            Assert.IsFalse(result.success);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.result));
            Assert.AreEqual(0, Directory.GetFiles(work).Length);
            Assert.AreEqual("outside sentinel", File.ReadAllText(sentinel));
        }

        [TestMethod]
        public async Task MissingArchiveMember_CannotUseLeftoverFile()
        {
            File.WriteAllText(Path.Combine(work, "leftover.sql"), "SELECT 1");
            var package = await Package("leftover.sql", ("different.sql", "SELECT 2"));
            var result = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, true);
            Assert.IsFalse(result.success);
            Assert.AreEqual("SELECT 1", File.ReadAllText(Path.Combine(work, "leftover.sql")));
            Assert.AreEqual(1, Directory.GetFiles(work).Length);
            Assert.ThrowsExactly<InvalidDataException>(() => SqlSyncBuildDataXmlSerializer.Load(package));
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() => SqlSyncBuildDataXmlSerializer.LoadAsync(package));
        }

        [TestMethod]
        public async Task LeftoverManifest_CannotBecomePackageManifest()
        {
            await SqlSyncBuildDataXmlSerializer.SaveAsync(Path.Combine(work, XmlFileNames.MainProjectFile), Model("file.sql"));
            var package = Path.Combine(root, "no-manifest.sbm");
            using (var archive = IOZipFile.Open(package, ZipArchiveMode.Create))
                archive.CreateEntry("file.sql");
            var result = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, true);
            Assert.IsFalse(result.success);
            Assert.IsTrue(File.Exists(Path.Combine(work, XmlFileNames.MainProjectFile)));
            Assert.IsFalse(File.Exists(Path.Combine(work, "file.sql")));
        }

        [TestMethod]
        [DataRow("one/a.sql", "two/a.sql")]
        [DataRow("a.sql", "A.sql")]
        [DataRow(@"one\a.sql", "two/a.sql")]
        [DataRow("a.sql", "a.sql")]
        public async Task DuplicateFlattenedNames_AreRejectedBeforeWrites(string first, string second)
        {
            var package = await Package("a.sql", (first, "first"), (second, "second"));
            Assert.IsFalse(ZipHelper.UnpackZipPackage(work, package, true));
            Assert.IsFalse(await ZipHelper.UnpackZipPackageAsync(work, package, true));
            Assert.IsFalse((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, true)).success);
            Assert.AreEqual(0, Directory.GetFiles(work).Length);
        }

        [TestMethod]
        [DataRow("../outside.sql")]
        [DataRow(@"..\outside.sql")]
        [DataRow("file.sql:stream")]
        [DataRow("/root.sql")]
        public async Task InvalidArchiveNames_AreRejectedEvenWhenFlattening(string name)
        {
            var package = await Package("valid.sql", ("valid.sql", "SELECT 1"), (name, "bad"));
            Assert.IsFalse(ZipHelper.UnpackZipPackage(work, package, true));
            Assert.IsFalse(await ZipHelper.UnpackZipPackageAsync(work, package, true));
            Assert.AreEqual(0, Directory.GetFiles(work).Length);
        }

        [TestMethod]
        public async Task ArchiveSymlink_IsRejected()
        {
            var package = await Package("link.sql", ("link.sql", "../outside.sql"));
            using (var archive = IOZipFile.Open(package, ZipArchiveMode.Update))
                archive.GetEntry("link.sql")!.ExternalAttributes = 0xA1FF << 16;
            Assert.IsFalse(ZipHelper.UnpackZipPackage(work, package, true));
            Assert.IsFalse(await ZipHelper.UnpackZipPackageAsync(work, package, true));
            Assert.AreEqual(0, Directory.GetFiles(work).Length);
        }

        [TestMethod]
        public async Task ExistingSameLengthSubstitute_IsNotAccepted()
        {
            var package = await Package("a.sql", ("a.sql", "SELECT 1"));
            File.WriteAllText(Path.Combine(work, "a.sql"), "SELECT 2");
            Assert.IsFalse((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, false)).success);
            Assert.AreEqual("SELECT 2", File.ReadAllText(Path.Combine(work, "a.sql")));
            Assert.IsFalse(File.Exists(Path.Combine(work, XmlFileNames.MainProjectFile)));
            Assert.IsTrue((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, true)).success);
            Assert.AreEqual("SELECT 1", File.ReadAllText(Path.Combine(work, "a.sql")));
        }

        [TestMethod]
        public async Task ValidNestedArchive_FlattensAndSupportsRepeatExtraction()
        {
            var package = await Package("a.sql", ("folder/a.sql", "SELECT 1"));
            Assert.IsTrue((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, false)).success);
            Assert.IsTrue((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, work, false, false)).success);
            Assert.AreEqual("SELECT 1", File.ReadAllText(Path.Combine(work, "a.sql")));
        }

        [TestMethod]
        public async Task MetadataCannotReadHashExportDeleteOrImportOutsideFile()
        {
            var model = Model("../outside.sql");
            var project = Path.Combine(work, XmlFileNames.MainProjectFile);
            var package = Path.Combine(root, "output.sbm");
            var export = Path.Combine(root, "export");
            Directory.CreateDirectory(export);
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(work, model));
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                SqlBuildFileHelper.PackageProjectFileIntoZipAsync(model, work, package, false));
            Assert.IsFalse(File.Exists(package));
            Assert.IsFalse(SqlBuildFileHelper.CopyIndividualScriptsToFolder(model, export, work, false, false));
            Assert.IsFalse(await SqlBuildFileHelper.CopyIndividualScriptsToFolderAsync(model, export, work, false, true));
            Assert.IsFalse(SqlBuildFileHelper.CopyScriptsToSingleFile(model, Path.Combine(export, "all.sql"), work, package, false));
            Assert.IsFalse(await SqlBuildFileHelper.CopyScriptsToSingleFileAsync(model, Path.Combine(export, "all.sql"), work, package, false));
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                SqlBuildFileHelper.RemoveScriptFilesFromBuildAsync(model, project, package, model.Script, true));
            var imported = await SqlBuildFileHelper.ImportSqlScriptFileAsync(Model(), model, work, 0,
                export, Path.Combine(export, XmlFileNames.MainProjectFile), package, true);
            Assert.AreEqual(-1d, imported.buildNumber);
            Assert.AreEqual(0, Directory.GetFiles(export).Length);
            Assert.AreEqual("outside sentinel", File.ReadAllText(sentinel));
        }

        [TestMethod]
        public async Task CachedBatch_DoesNotBypassPathValidation()
        {
            var context = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object,
                context, new Mock<IBuildFinalizerContext>().Object);
            var batches = new ScriptBatchCollection { new ScriptBatch("../outside.sql", new[] { "SELECT 1" }, "id") };
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                runner.LoadBatchScriptsAsync("id", "../outside.sql", false, batches, default));
            Assert.ThrowsExactly<InvalidDataException>(() =>
                new DefaultScriptBatcher().LoadAndBatchSqlScripts(Model("../outside.sql"), work));
        }

        [TestMethod]
        public async Task ExplicitSourcePathsRemainSupported_ButPackageRelativePathsCannotEscape()
        {
            var output = Path.Combine(work, "trusted.sbm");
            Assert.IsTrue(await ZipHelper.CreateZipPackageAsync(new() { sentinel }, output, true));
            Assert.ThrowsExactly<InvalidDataException>(() =>
                ZipHelper.CreateZipPackage(new[] { "../outside.sql" }, work, Path.Combine(work, "bad.sbm")));
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                ZipHelper.CreateZipPackageAsync(new[] { "../outside.sql" }, work, Path.Combine(work, "bad.sbm"), true));
        }

        [TestMethod]
        public async Task DuplicateAppend_FailsWithoutChangingExistingEntry()
        {
            var package = await Package("a.sql", ("a.sql", "SELECT 1"));
            var source = Path.Combine(work, "a.sql");
            File.WriteAllText(source, "SELECT 2");
            Assert.IsFalse(ZipHelper.AppendZipPackage(new[] { source }, work, package, true));
            Assert.IsFalse(await ZipHelper.AppendZipPackageAsync(new[] { "a.sql" }, work, package, true));
            using var archive = IOZipFile.OpenRead(package);
            using var reader = new StreamReader(archive.GetEntry("a.sql")!.Open());
            Assert.AreEqual("SELECT 1", await reader.ReadToEndAsync());
            Assert.AreEqual(2, archive.Entries.Count);
        }

        [TestMethod]
        public async Task RepeatedScriptReference_PackagesOneFile()
        {
            File.WriteAllText(Path.Combine(work, "a.sql"), "SELECT 1");
            var package = Path.Combine(root, "repeated.sbm");
            Assert.IsTrue(await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(Model("a.sql", "a.sql"), work, package, false));
            var output = Path.Combine(root, "repeated");
            Assert.IsTrue((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, output, false, true)).success);
            Assert.AreEqual(2, SqlSyncBuildDataXmlSerializer.Load(package).Script.Count);
            using var archive = IOZipFile.OpenRead(package);
            Assert.AreEqual(2, archive.Entries.Count);
        }

        [TestMethod]
        public async Task OwnedExtractionFailure_RemovesOnlyAllocatedDirectory()
        {
            var package = await Package("../outside.sql", ("a.sql", "SELECT 1"));
            var result = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package);
            Assert.IsFalse(result.success);
            Assert.IsFalse(Directory.Exists(result.workingDirectory));
            Assert.IsTrue(Directory.Exists(work));
            Assert.AreEqual("outside sentinel", File.ReadAllText(sentinel));
        }

        [TestMethod]
        public async Task LinkedDestinationAndScript_AreRejectedWithoutDeletingTarget()
        {
            var target = Path.Combine(root, "target");
            Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(target, "a.sql"), "sentinel");
            var link = Path.Combine(work, "link");
            if (OperatingSystem.IsWindows())
            {
                var start = new System.Diagnostics.ProcessStartInfo("cmd.exe")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                start.ArgumentList.Add("/c");
                start.ArgumentList.Add("mklink");
                start.ArgumentList.Add("/J");
                start.ArgumentList.Add(link);
                start.ArgumentList.Add(target);
                using var process = System.Diagnostics.Process.Start(start)!;
                await process.WaitForExitAsync();
                Assert.AreEqual(0, process.ExitCode, await process.StandardError.ReadToEndAsync());
            }
            else
            {
                Directory.CreateSymbolicLink(link, target);
            }
            try
            {
                Assert.ThrowsExactly<InvalidDataException>(() => PackagePath.Resolve(work, "link/a.sql"));
                var package = await Package("a.sql", ("a.sql", "SELECT 1"));
                Assert.IsFalse((await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, link, false, true)).success);
                Assert.AreEqual("sentinel", File.ReadAllText(Path.Combine(target, "a.sql")));
                Assert.IsTrue(Directory.Exists(link));
                await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                    SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(work, Model("link/a.sql")));
            }
            finally
            {
                Directory.Delete(link);
            }
        }
    }
}
