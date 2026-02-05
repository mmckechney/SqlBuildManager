using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildFileHelperPackageTest
    {
        #region PackageSbxFileIntoSbmFileTest
        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_EmptySbxName()
        {
            string sbxBuildControlFileName = string.Empty;
            string sbmProjectFileName = @"C:\test.sbm";
            bool expected = false;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, sbmProjectFileName, System.Threading.CancellationToken.None);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_InvalidSbx()
        {
            string sbxBuildControlFileName = Path.GetTempFileName();
            string sbmProjectFileName = Path.GetTempFileName();
            bool expected = false;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, sbmProjectFileName, System.Threading.CancellationToken.None);

            if (File.Exists(sbxBuildControlFileName))
                File.Delete(sbxBuildControlFileName);
            if (File.Exists(sbmProjectFileName))
                File.Delete(sbmProjectFileName);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_PreExistingSbmFile()
        {
            string sbxBuildControlFileName = Path.GetTempFileName();
            string sbmProjectFileName = @"C:\test.sbm";
            bool expected = false;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, sbmProjectFileName, System.Threading.CancellationToken.None);

            if (File.Exists(sbxBuildControlFileName))
                File.Delete(sbmProjectFileName);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_Success()
        {
            string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(folder);

            string sbxFile = Path.Combine(folder, "sbx_package_tester.sbx");
            string script1 = Path.Combine(folder, "CreateTestTablesScript.sql");
            string script2 = Path.Combine(folder, "LoggingTable.sql");
            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);


            string sbmProjectFileName = Path.Combine(folder, "test.sbm");
            bool expected = true;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxFile, sbmProjectFileName, System.Threading.CancellationToken.None);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_SuccessWithPreExistingXmlFile()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(folder);

            string sbxFile = Path.Combine(folder, "sbx_package_tester.sbx");
            string script1 = Path.Combine(folder, "CreateTestTablesScript.sql");
            string script2 = Path.Combine(folder, "LoggingTable.sql");
            string preExistingXml = Path.Combine(folder, XmlFileNames.MainProjectFile);
            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);
            File.WriteAllText(preExistingXml, "just want some text");


            string sbmProjectFileName = Path.Combine(folder, @"test.sbm");
            bool expected = true;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxFile, sbmProjectFileName, System.Threading.CancellationToken.None);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_SuccessWithPreExistingSbmFile()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(folder);

            string sbxFile = Path.Combine(folder, "sbx_package_tester.sbx");
            string script1 = Path.Combine(folder, "CreateTestTablesScript.sql");
            string script2 = Path.Combine(folder, "LoggingTable.sql");
            string sbmProjectFileName = Path.Combine(folder, @"test.sbm");
            string preExistingXml = Path.Combine(folder, XmlFileNames.MainProjectFile);
            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);
            File.WriteAllText(sbmProjectFileName, "just want some text");



            bool expected = true;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxFile, sbmProjectFileName, System.Threading.CancellationToken.None);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_GoodSbxNoScripts()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(folder);

            string sbxFile = Path.Combine(folder, "sbx_package_tester.sbx");

            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);


            string sbmProjectFileName = Path.Combine(folder, @"test.sbm");
            bool expected = false;
            bool actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxFile, sbmProjectFileName, System.Threading.CancellationToken.None);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }
        #endregion

        #region PackageSbxFileIntoSbmFileTest DontPassSbmName 
        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_DontPassSbmName_Success()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(folder);

            string sbxBuildControlFileName = Path.Combine(folder, "sbx_package_tester.sbx");
            string script1 = Path.Combine(folder, "CreateTestTablesScript.sql");
            string script2 = Path.Combine(folder, "LoggingTable.sql");

            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);

            string expected = Path.Combine(Path.GetDirectoryName(sbxBuildControlFileName), Path.GetFileNameWithoutExtension(sbxBuildControlFileName) + ".sbm");
            string actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, System.Threading.CancellationToken.None);
            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }
        [TestMethod()]
        public async Task PackageSbxFileIntoSbmFileTest_DontPassSbmName_Fail()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(folder);

            string sbxBuildControlFileName = Path.Combine(folder, "sbx_package_tester.sbx");
            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);

            string expected = string.Empty;
            string actual;
            actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, System.Threading.CancellationToken.None);
            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }
        #endregion

        #region PackageSbxFilesIntoSbmFilesTest
        /// <summary>
        ///A test for PackageSbxFilesIntoSbmFiles
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFilesIntoSbmFilesTest_SingleSbx()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(directoryName);

            string sbxBuildControlFileName = Path.Combine(directoryName, "sbx_package_tester.sbx");
            string script1 = Path.Combine(directoryName, "CreateTestTablesScript.sql");
            string script2 = Path.Combine(directoryName, "LoggingTable.sql");
            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);

            List<string> actual;
            actual = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName, System.Threading.CancellationToken.None);
            Directory.Delete(directoryName, true);
            Assert.AreEqual(1, actual.Count);
            Assert.IsTrue(actual[0] == Path.Combine(directoryName, "sbx_package_tester.sbm"));

        }
        /// <summary>
        ///A test for PackageSbxFilesIntoSbmFiles
        ///</summary>
        [TestMethod()]
        public async Task PackageSbxFilesIntoSbmFilesTest_SbxInMainAndSubFolder()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(directoryName);

            string sbxBuildControlFileName = Path.Combine(directoryName, "sbx_package_tester.sbx");
            string script1 = Path.Combine(directoryName, "CreateTestTablesScript.sql");
            string script2 = Path.Combine(directoryName, "LoggingTable.sql");

            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);


            string subDirectory = Path.Combine(directoryName, Guid.NewGuid().ToString());
            Directory.CreateDirectory(subDirectory);
            string subSbxBuildControlFileName = Path.Combine(subDirectory, "sub_sbx_package_tester.sbx");
            string subScript1 = Path.Combine(subDirectory, "CreateTestTablesScript.sql");
            string subScript2 = Path.Combine(subDirectory, "LoggingTable.sql");
            File.WriteAllBytes(subSbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(subScript1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(subScript2, Properties.Resources.LoggingTable);

            List<string> actual;
            actual = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName, System.Threading.CancellationToken.None);

            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual[0] == Path.Combine(directoryName, "sbx_package_tester.sbm"));
            Assert.IsTrue(actual[1] == Path.Combine(subDirectory, "sub_sbx_package_tester.sbm"));

            Assert.IsTrue(File.Exists(actual[0]));
            Assert.IsTrue(File.Exists(actual[1]));

            Directory.Delete(directoryName, true);

        }

        [TestMethod()]
        public async Task PackageSbxFilesIntoSbmFilesTest_FailNoSbx()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(directoryName);

            List<string> actual;
            actual = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName, System.Threading.CancellationToken.None);
            Directory.Delete(directoryName, true);
            Assert.AreEqual(0, actual.Count);

        }

        [TestMethod()]
        public async Task PackageSbxFilesIntoSbmFilesTest_FailBadSbx()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(directoryName);

            string sbxBuildControlFileName = Path.Combine(directoryName, "sbx_package_tester.sbx");
            File.WriteAllText(sbxBuildControlFileName, "bad contents");

            List<string> actual;
            actual = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName, System.Threading.CancellationToken.None);
            Directory.Delete(directoryName, true);
            Assert.AreEqual(0, actual.Count);

        }

        [TestMethod()]
        public async Task PackageSbxFilesIntoSbmFilesTest_FailDirectoryIsEmptyString()
        {
            string directoryName = String.Empty;

            List<string> actual;
            actual = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName, System.Threading.CancellationToken.None);

            Assert.AreEqual(0, actual.Count);

        }
        [TestMethod()]
        public async Task PackageSbxFilesIntoSbmFilesTest_FailDirectoryDoesntExist()
        {
            string directoryName = @"C:\" + Guid.NewGuid().ToString();
            List<string> actual;
            actual = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName, System.Threading.CancellationToken.None);

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public async Task PackageSbxFileIntoSbmFileAsyncTest_InvalidSbx()
        {
            string sbxBuildControlFileName = Path.GetTempFileName();
            string sbmProjectFileName = Path.GetTempFileName();
            var actual = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(sbxBuildControlFileName, sbmProjectFileName);
            Assert.IsFalse(actual);
            File.Delete(sbxBuildControlFileName);
            File.Delete(sbmProjectFileName);
        }

        [TestMethod]
        public async Task PackageSbxFilesIntoSbmFilesAsyncTest_EmptyDirectory()
        {
            string directoryName = String.Empty;
            var result = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(directoryName);
            Assert.AreEqual(0, result.Count);
        }
        #endregion
    }
}
