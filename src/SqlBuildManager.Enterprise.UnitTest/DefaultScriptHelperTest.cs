using SqlBuildManager.Enterprise.DefaultScripts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlBuildManager.Enterprise;
using System.Collections.Generic;
using SqlSync.SqlBuild.DefaultScripts;
using System.IO;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DefaultScriptHelperTest and is intended
    ///to contain all DefaultScriptHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DefaultScriptHelperTest
    {
        public string fileOne = string.Empty;
        public string fileTwo = string.Empty;
        public string fileThree = string.Empty;

        #region Additional test attributes

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            this.fileOne = Path.GetTempFileName();
            File.WriteAllText(this.fileOne, "This is this it - the contents of file 1");

            this.fileTwo = Path.GetTempFileName();
            File.WriteAllText(this.fileTwo, "This is this it - the contents of file 2");

            this.fileThree = Path.GetTempFileName();
            File.WriteAllText(this.fileThree, "This is this it - the contents of file 3");
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            if(File.Exists(this.fileOne))
                File.Delete(this.fileOne);


            if (File.Exists(this.fileTwo))
                File.Delete(this.fileTwo);

            if (File.Exists(this.fileThree))
                File.Delete(this.fileThree);
        }
      
        #endregion


        /// <summary>
        ///A test for DefaultScriptHelper Constructor
        ///</summary>
        [TestMethod()]
        public void DefaultScriptHelperConstructorTest()
        {
            DefaultScriptHelper target = new DefaultScriptHelper();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(DefaultScriptHelper));
        }


        #region CopyEnterpriseToLocalTest
        /// <summary>
        ///A test for CopyEnterpriseToLocal
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void CopyEnterpriseToLocalTest_EnterpriseFileDoesntExist()
        {
            string localFilePath = this.fileOne;
            string enterpriseFilePath = @"C:\thisfileshouldnotexist";
            bool expected = false; 
            bool actual;
            actual = DefaultScriptHelper.CopyEnterpriseToLocal(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CopyEnterpriseToLocal
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void CopyEnterpriseToLocalTest_LocalFileExists()
        {
            string localFilePath = this.fileOne;
            string enterpriseFilePath = this.fileTwo;
            bool expected = true;
            bool actual;
            actual = DefaultScriptHelper.CopyEnterpriseToLocal(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CopyEnterpriseToLocal
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void CopyEnterpriseToLocalTest_EnterpriseFileNotSet()
        {
            string localFilePath = this.fileOne;
            string enterpriseFilePath = "";
            bool expected = false;
            bool actual;
            actual = DefaultScriptHelper.CopyEnterpriseToLocal(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CopyEnterpriseToLocal
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void CopyEnterpriseToLocalTest_LocalFileNotSet()
        {
            string localFilePath = "";
            string enterpriseFilePath = this.fileTwo;
            bool expected = false;
            bool actual;
            actual = DefaultScriptHelper.CopyEnterpriseToLocal(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);
        }
        #endregion


        #region GetApplicableDefaultScriptRegTest
        /// <summary>
        ///A test for GetApplicableDefaultScriptReg
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetApplicableDefaultScriptRegTest_FoundOneMatch_FirstInList()
        {
            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "MyGroup";
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);

            List<string> groupMemberships = new List<string>(new string[] {  "Group1", "Group2", "MyGroup" });
 
            DefaultScriptRegistryFile actual;
            actual = DefaultScriptHelper.GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(DefaultScriptRegistryFile));
            Assert.AreEqual(Path.GetFileName(this.fileOne), actual.FileName);
        }

        /// <summary>
        ///A test for GetApplicableDefaultScriptReg
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetApplicableDefaultScriptRegTest_FoundOneMatch_SecondInList()
        {
            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "MyGroup";
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile2);
            defaultScriptRegs.Add(srFile1);
            

            List<string> groupMemberships = new List<string>(new string[] { "MyGroup", "Group1", "Group2" });

            DefaultScriptRegistryFile actual;
            actual = DefaultScriptHelper.GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(DefaultScriptRegistryFile));
            Assert.AreEqual(Path.GetFileName(this.fileOne), actual.FileName);
        }

        /// <summary>
        ///A test for GetApplicableDefaultScriptReg
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetApplicableDefaultScriptRegTest_NoMatch()
        {
            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "ReallyNotMyGroup";
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);
            
            List<string> groupMemberships = new List<string>(new string[] { "MyGroup", "Group1", "Group2" });

            DefaultScriptRegistryFile actual;
            actual = DefaultScriptHelper.GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetApplicableDefaultScriptReg
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetApplicableDefaultScriptRegTest_NullRegistry()
        {

            List<DefaultScriptRegistryFile> defaultScriptRegs = null;

            List<string> groupMemberships = new List<string>(new string[] { "MyGroup", "Group1", "Group2" });

            DefaultScriptRegistryFile actual;
            actual = DefaultScriptHelper.GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetApplicableDefaultScriptReg
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetApplicableDefaultScriptRegTest_NullGroups()
        {
            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "ReallyNotMyGroup";
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);

            List<string> groupMemberships = null;

            DefaultScriptRegistryFile actual;
            actual = DefaultScriptHelper.GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetApplicableDefaultScriptReg
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetApplicableDefaultScriptRegTest_EmptyGroups()
        {
            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "ReallyNotMyGroup";
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);

            List<string> groupMemberships = new List<string>();

            DefaultScriptRegistryFile actual;
            actual = DefaultScriptHelper.GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            Assert.IsNull(actual);

        }

        #endregion

        #region GetEnterpriseRegistrySettingTest
        /// <summary>
        ///A test for GetEnterpriseRegistrySetting
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetEnterpriseRegistrySettingTest_GoodFileExists()
        {
            string filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, Properties.Resources.DefaultScriptRegistry);
 
            DefaultScriptRegistry actual;
            actual = DefaultScriptHelper.GetEnterpriseRegistrySetting(filePath);
            File.Delete(filePath);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(SqlSync.SqlBuild.DefaultScripts.DefaultScriptRegistry));
            Assert.IsNotNull(actual.Items);
            Assert.IsTrue(actual.Items.Length == 1);
        }

        /// <summary>
        ///A test for GetEnterpriseRegistrySetting
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetEnterpriseRegistrySettingTest_FileDoesNotExist()
        {
            string filePath = @"C:\" + Guid.NewGuid().ToString();

            DefaultScriptRegistry actual;
            actual = DefaultScriptHelper.GetEnterpriseRegistrySetting(filePath);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for GetEnterpriseRegistrySetting
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetEnterpriseRegistrySettingTest_InvalidFile()
        {
            string filePath = this.fileOne;

            DefaultScriptRegistry actual;
            actual = DefaultScriptHelper.GetEnterpriseRegistrySetting(filePath);
            Assert.IsNull(actual);
        }
        #endregion

        /// <summary>
        ///A test for SetEnterpriseDefaultScripts
        ///</summary>
        [TestMethod()]
        public void SetEnterpriseDefaultScriptsTest_Successful()
        {
            SqlSync.SqlBuild.SqlBuildFileHelper.DefaultScriptXmlFile = this.fileThree;

             DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "MyGroup";
            File.WriteAllText(this.fileOne, String.Format(Properties.Resources.DefaultScriptRegistryWithToken, this.fileOne));
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);

            List<string> groupMemberships = new List<string>(new string[] {  "Group1", "Group2", "MyGroup" });
            bool expected = true; 
            bool actual;
            actual = DefaultScriptHelper.SetEnterpriseDefaultScripts(defaultScriptRegs, groupMemberships);
            Assert.AreEqual(expected, actual);
 
        }

        /// <summary>
        ///A test for SetEnterpriseDefaultScripts
        ///</summary>
        [TestMethod()]
        public void SetEnterpriseDefaultScriptsTest_NoMatchingScriptRegistryFile()
        {
            SqlSync.SqlBuild.SqlBuildFileHelper.DefaultScriptXmlFile = this.fileThree;

            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "ReallyNotMyGroup";
            File.WriteAllText(this.fileOne, String.Format(Properties.Resources.DefaultScriptRegistryWithToken, this.fileOne));
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);

            List<string> groupMemberships = new List<string>(new string[] { "Group1", "Group2", "MyGroup" });
            bool expected = false;
            bool actual;
            actual = DefaultScriptHelper.SetEnterpriseDefaultScripts(defaultScriptRegs, groupMemberships);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for SetEnterpriseDefaultScripts
        ///</summary>
        [TestMethod()]
        public void SetEnterpriseDefaultScriptsTest_NoItemsScriptRegistryFile()
        {
            SqlSync.SqlBuild.SqlBuildFileHelper.DefaultScriptXmlFile = this.fileThree;

            DefaultScriptRegistryFile srFile1 = new DefaultScriptRegistryFile();
            srFile1.ApplyToGroup = "MyGroup";
            File.WriteAllText(this.fileOne, "<?xml version=\"1.0\" encoding=\"utf-8\" ?><DefaultScriptRegistry xmlns=\"http://schemas.mckechney.com/DefaultScriptRegistry.xsd\"></DefaultScriptRegistry>");
            
            
            srFile1.FileName = Path.GetFileName(this.fileOne);
            srFile1.Path = Path.GetDirectoryName(this.fileOne);

            DefaultScriptRegistryFile srFile2 = new DefaultScriptRegistryFile();
            srFile2.ApplyToGroup = "NotMyGroup";
            srFile2.FileName = Path.GetFileName(this.fileTwo);
            srFile2.Path = Path.GetDirectoryName(this.fileTwo);

            List<DefaultScriptRegistryFile> defaultScriptRegs = new List<DefaultScriptRegistryFile>();
            defaultScriptRegs.Add(srFile1);
            defaultScriptRegs.Add(srFile2);

            List<string> groupMemberships = new List<string>(new string[] { "Group1", "Group2", "MyGroup" });
            bool expected = false;
            bool actual;
            actual = DefaultScriptHelper.SetEnterpriseDefaultScripts(defaultScriptRegs, groupMemberships);
            Assert.AreEqual(expected, actual);

        }

        #region ValidateLocalToEnterpriseTest
        /// <summary>
        ///A test for ValidateLocalToEnterprise
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void ValidateLocalToEnterpriseTest_LocalFileDoesntExist()
        {
            string localFilePath = @"C:\thisfileshouldnotexist";
            string enterpriseFilePath = this.fileTwo;
            bool expected = false;
            bool actual;
            actual = DefaultScriptHelper.ValidateLocalToEnterprise(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);
   
        }

        /// <summary>
        ///A test for ValidateLocalToEnterprise
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void ValidateLocalToEnterpriseTest_EnterpriseFileDoesntExist()
        {
            string localFilePath = this.fileOne;
            string enterpriseFilePath = @"C:\thisfileshouldnotexist";
            bool expected = true;
            bool actual;
            actual = DefaultScriptHelper.ValidateLocalToEnterprise(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateLocalToEnterprise
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void ValidateLocalToEnterpriseTest_FilesMatch()
        {
            string localFilePath = this.fileOne;
            string enterpriseFilePath = this.fileOne;
            bool expected = true;
            bool actual;
            actual = DefaultScriptHelper.ValidateLocalToEnterprise(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateLocalToEnterprise
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void ValidateLocalToEnterpriseTest_FilesDontMatch()
        {
            string localFilePath = this.fileOne;
            string enterpriseFilePath = this.fileTwo;
            bool expected = false;
            bool actual;
            actual = DefaultScriptHelper.ValidateLocalToEnterprise(localFilePath, enterpriseFilePath);
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}
