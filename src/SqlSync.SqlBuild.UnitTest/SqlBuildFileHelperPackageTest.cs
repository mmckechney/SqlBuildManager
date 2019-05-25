using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
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
        public void PackageSbxFileIntoSbmFileTest_EmptySbxName()
        {
            string sbxBuildControlFileName = string.Empty;
            string sbmProjectFileName = @"C:\test.sbm";
            bool expected = false; 
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxBuildControlFileName, sbmProjectFileName);
            Assert.AreEqual(expected, actual);
           
        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_InvalidSbx()
        {
            string sbxBuildControlFileName = Path.GetTempFileName();
            string sbmProjectFileName = Path.GetTempFileName();
            bool expected = false;
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxBuildControlFileName, sbmProjectFileName);

            if(File.Exists(sbxBuildControlFileName))
                File.Delete(sbxBuildControlFileName);
            if (File.Exists(sbmProjectFileName))
                File.Delete(sbmProjectFileName);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_PreExistingSbmFile()
        {
            string sbxBuildControlFileName = Path.GetTempFileName();
            string sbmProjectFileName = @"C:\test.sbm";
            bool expected = false;
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxBuildControlFileName, sbmProjectFileName);

            if(File.Exists(sbxBuildControlFileName))
                File.Delete(sbmProjectFileName);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_Success()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(folder);

            string sbxFile = folder + "sbx_package_tester.sbx";
            string script1 = folder + "CreateTestTablesScript.sql";
            string script2 = folder + "LoggingTable.sql";
            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);


            string sbmProjectFileName = folder + @"test.sbm";
            bool expected = true;
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxFile, sbmProjectFileName);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_SuccessWithPreExistingXmlFile()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(folder);

            string sbxFile = folder + "sbx_package_tester.sbx";
            string script1 = folder + "CreateTestTablesScript.sql";
            string script2 = folder + "LoggingTable.sql";
            string preExistingXml = folder + XmlFileNames.MainProjectFile;
            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);
            File.WriteAllText(preExistingXml,"just want some text");


            string sbmProjectFileName = folder + @"test.sbm";
            bool expected = true;
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxFile, sbmProjectFileName);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_SuccessWithPreExistingSbmFile()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(folder);

            string sbxFile = folder + "sbx_package_tester.sbx";
            string script1 = folder + "CreateTestTablesScript.sql";
            string script2 = folder + "LoggingTable.sql";
            string sbmProjectFileName = folder + @"test.sbm";
            string preExistingXml = folder + XmlFileNames.MainProjectFile;
            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);
            File.WriteAllText(sbmProjectFileName, "just want some text");


            
            bool expected = true;
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxFile, sbmProjectFileName);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_GoodSbxNoScripts()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(folder);

            string sbxFile = folder + "sbx_package_tester.sbx";

            File.WriteAllBytes(sbxFile, Properties.Resources.sbx_package_tester);
 

            string sbmProjectFileName = folder + @"test.sbm";
            bool expected = false ;
            bool actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxFile, sbmProjectFileName);

            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }
        #endregion

        #region PackageSbxFileIntoSbmFileTest DontPassSbmName 
        /// <summary>
        ///A test for PackageSbxFileIntoSbmFile
        ///</summary>
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_DontPassSbmName_Success()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(folder);

            string sbxBuildControlFileName = folder + "sbx_package_tester.sbx";
            string script1 = folder + "CreateTestTablesScript.sql";
            string script2 = folder + "LoggingTable.sql";
            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);

            string expected = Path.GetDirectoryName(sbxBuildControlFileName) + @"\" + Path.GetFileNameWithoutExtension(sbxBuildControlFileName) + ".sbm";
            string actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxBuildControlFileName);
            Directory.Delete(folder, true);
            
            Assert.AreEqual(expected, actual);

        }
        [TestMethod()]
        public void PackageSbxFileIntoSbmFileTest_DontPassSbmName_Fail()
        {
            string folder = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(folder);

            string sbxBuildControlFileName = folder + "sbx_package_tester.sbx";
            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);

            string expected = string.Empty;
            string actual;
            actual = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(sbxBuildControlFileName);
            Directory.Delete(folder, true);

            Assert.AreEqual(expected, actual);

        }
        #endregion

        #region PackageSbxFilesIntoSbmFilesTest
        /// <summary>
        ///A test for PackageSbxFilesIntoSbmFiles
        ///</summary>
        [TestMethod()]
        public void PackageSbxFilesIntoSbmFilesTest_SingleSbx()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(directoryName);

            string sbxBuildControlFileName = directoryName + "sbx_package_tester.sbx";
            string script1 = directoryName + "CreateTestTablesScript.sql";
            string script2 = directoryName + "LoggingTable.sql";
            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);

            string message = string.Empty; 
            string messageExpected = string.Empty; 

            List<string> actual;
            actual = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directoryName, out message);
            Directory.Delete(directoryName, true);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(1, actual.Count);
            Assert.IsTrue(actual[0] == directoryName + "sbx_package_tester.sbm"); 
            
        }
        /// <summary>
        ///A test for PackageSbxFilesIntoSbmFiles
        ///</summary>
        [TestMethod()]
        public void PackageSbxFilesIntoSbmFilesTest_SbxInMainAndSubFolder()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(directoryName);

            string sbxBuildControlFileName = directoryName + "sbx_package_tester.sbx";
            string script1 = directoryName + "CreateTestTablesScript.sql";
            string script2 = directoryName + "LoggingTable.sql";
            File.WriteAllBytes(sbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(script1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(script2, Properties.Resources.LoggingTable);


            string subDirectory = directoryName + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(subDirectory);
            string subSbxBuildControlFileName = subDirectory + "sub_sbx_package_tester.sbx";
            string subScript1 = subDirectory + "CreateTestTablesScript.sql";
            string subScript2 = subDirectory + "LoggingTable.sql";
            File.WriteAllBytes(subSbxBuildControlFileName, Properties.Resources.sbx_package_tester);
            File.WriteAllText(subScript1, Properties.Resources.CreateTestTablesScript);
            File.WriteAllText(subScript2, Properties.Resources.LoggingTable);


            string message = string.Empty;
            string messageExpected = string.Empty;

            List<string> actual;
            actual = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directoryName, out message);
            
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual[0] == directoryName + "sbx_package_tester.sbm");
            Assert.IsTrue(actual[1] == subDirectory + "sub_sbx_package_tester.sbm");

            Assert.IsTrue(File.Exists(actual[0]));
            Assert.IsTrue(File.Exists(actual[1]));

            Directory.Delete(directoryName, true);

        }

        [TestMethod()]
        public void PackageSbxFilesIntoSbmFilesTest_FailNoSbx()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(directoryName);

            string message = string.Empty;
            string messageExpected = String.Format("No SBX files found in source directory '{0}' or any of it's subdirectories", directoryName);

            List<string> actual;
            actual = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directoryName, out message);
            Directory.Delete(directoryName, true);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(0, actual.Count);

        }

        [TestMethod()]
        public void PackageSbxFilesIntoSbmFilesTest_FailBadSbx()
        {
            string directoryName = Path.GetTempPath() + Guid.NewGuid().ToString() + @"\";
            Directory.CreateDirectory(directoryName);

            string sbxBuildControlFileName = directoryName + "sbx_package_tester.sbx";
            File.WriteAllText(sbxBuildControlFileName, "bad contents");
            string message = string.Empty;
            string messageExpected = string.Empty;

            List<string> actual;
            actual = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directoryName, out message);
            Directory.Delete(directoryName, true);
            Assert.IsTrue(message.StartsWith("Error packaging SBX file"));
            Assert.AreEqual(0, actual.Count);


        }

        [TestMethod()]
        public void PackageSbxFilesIntoSbmFilesTest_FailDirectoryIsEmptyString()
        {
            string directoryName = String.Empty;

            string message;
            string messageExpected = "Unable to package SBX files. Source directory parameter is empty";
            List<string> actual;
            actual = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directoryName, out message);

            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(0, actual.Count);
   
        }
        [TestMethod()]
        public void PackageSbxFilesIntoSbmFilesTest_FailDirectoryDoesntExist()
        {
            string directoryName = @"C:\" + Guid.NewGuid().ToString();

            string message;
            string messageExpected = String.Format("Unable to package SBX files. The specified source directory '{0}' does not exist.", directoryName);
            List<string> actual;
            actual = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directoryName, out message);
  
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(0, actual.Count);

        }
        #endregion
    }
}
