using SqlSync.SqlBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System;
namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for SqlBuildFileHelperTest and is intended
    ///to contain all SqlBuildFileHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SqlBuildFileHelperTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private static List<Initialization> initColl;

        [ClassInitialize()]
        public static void InitializeTests(TestContext testContext)
        {
            initColl = new List<Initialization>();
        }
        private Initialization GetInitializationObject()
        {
            Initialization init = new Initialization();
            initColl.Add(init);
            return init;
        }
        [ClassCleanup()]
        public static void Cleanup()
        {
            for (int i = 0; i < initColl.Count; i++)
            {
                initColl[i].Dispose();
            }
        }


        /// <summary>
        ///A test for CreateShellSqlSyncBuildDataObject
        ///</summary>
        [TestMethod()]
        public void CreateShellSqlSyncBuildDataObjectTest()
        {

            SqlSyncBuildData actual;
            actual = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.SqlSyncBuildProject.Rows.Count);
            Assert.AreEqual(1, actual.Scripts.Rows.Count);
            Assert.AreEqual(1, actual.Builds.Rows.Count);
            Assert.AreEqual(false, actual.SqlSyncBuildProject[0].ScriptTagRequired);

        }

        /// <summary>
        ///A test for UpdateObsoleteXmlNamespace
        ///</summary>
        [TestMethod()]
        public void UpdateObsoleteXmlNamespaceTest()
        {
            Initialization init = GetInitializationObject();
            string fileName = init.GetTrulyUniqueFile();
            File.WriteAllText(fileName, Properties.Resources.XmlWithInvalidNamespace);

            bool actual = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(fileName);
            Assert.AreEqual(true, actual);

            string contents = File.ReadAllText(fileName);
            Assert.IsTrue(contents.Contains("xmlns=\"http://schemas.mckechney.com/"));
        }
        /// <summary>
        ///A test for UpdateObsoleteXmlNamespace
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException), "Empty path name is not legal.")]
        public void UpdateObsoleteXmlNamespace_TestEmptyFileName()
        {

            bool actual = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(string.Empty);

        }
        /// <summary>
        ///A test for UpdateObsoleteXmlNamespace
        ///</summary>
        [TestMethod()]
        public void UpdateObsoleteXmlNamespaceTest_XmlNoNamespace()
        {
            Initialization init = GetInitializationObject();
            string fileName = init.GetTrulyUniqueFile();
            File.WriteAllText(fileName, Properties.Resources.XmlWithNoNamespace);

            bool actual = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(fileName);
            Assert.AreEqual(false, actual);

            string contents = File.ReadAllText(fileName);
            Assert.IsFalse(contents.Contains("xmlns=\"http://schemas.mckechney.com/"));
        }


        [TestMethod()]
        public void GetIntertedIndexValuesTest_WholeValuesWithRoom()
        {
            double floor = .9;
            double ceiling = 6.1;
            int insertCount = 3;
            List<double> actual;
            actual = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);
            Assert.AreEqual(2, actual[0]);
            Assert.AreEqual(3, actual[1]);
            Assert.AreEqual(4, actual[2]);

        }

        [TestMethod()]
        public void GetIntertedIndexValuesTest_WholeValuesPerfectFit()
        {
            double floor = 1;
            double ceiling = 5;
            int insertCount = 3;
            List<double> actual;
            actual = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);
            Assert.AreEqual(2, actual[0]);
            Assert.AreEqual(3, actual[1]);
            Assert.AreEqual(4, actual[2]);

        }

        [TestMethod()]
        public void GetIntertedIndexValuesTest_TenthValues()
        {
            double floor = .9;
            double ceiling = 3.1;
            int insertCount = 3;
            List<double> actual;
            actual = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);
            Assert.AreEqual(1.1, actual[0]);
            Assert.AreEqual(1.2, actual[1]);
            Assert.AreEqual(1.3, actual[2]);

        }
        [TestMethod()]
        public void GetIntertedIndexValuesTest_DecimalValues()
        {
            double floor = .9;
            double ceiling = 1.1;
            int insertCount = 3;
            List<double> actual;
            actual = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);
            Assert.AreEqual(0.94, actual[0]);
            Assert.AreEqual(0.98, actual[1]);
            Assert.AreEqual(1.02, actual[2]);

        }


        #region CalculateBuildPackageSHA1SignatureFromPath
        /// <summary>
        ///A test for CalculateBuildPackageSHA1SignatureFromPath
        ///</summary>
        [TestMethod()]
        public void CalculateBuildPackageSHA1SignatureFromPathTest_GetHashSuccessfully()
        {
            //Set up directory and files...
            string projectFileExtractionPath = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
            if (!Directory.Exists(projectFileExtractionPath))
                Directory.CreateDirectory(projectFileExtractionPath);

            string file1 = "File1.sql";
            File.WriteAllText(projectFileExtractionPath + file1, Properties.Resources.CreateDatabaseScript);

            string file2 = "File2.sql";
            File.WriteAllText(projectFileExtractionPath + file2, Properties.Resources.CreateTestTablesScript);

            string file3 = "File3.sql";
            File.WriteAllText(projectFileExtractionPath + file3, Properties.Resources.LoggingTable);


            SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            SqlSyncBuildData.ScriptRow row1 = buildData.Script.NewScriptRow();
            row1.BuildOrder = 1;
            row1.FileName = file1;

            SqlSyncBuildData.ScriptRow row2 = buildData.Script.NewScriptRow();
            row2.BuildOrder = 2;
            row2.FileName = file2;

            SqlSyncBuildData.ScriptRow row3 = buildData.Script.NewScriptRow();
            row3.BuildOrder = 3;
            row3.FileName = file3;

            buildData.Script.Rows.Add(row1);
            buildData.Script.Rows.Add(row2);
            buildData.Script.Rows.Add(row3);

            string expected = "4E0F54A4BA40DC62A78822B20C7D83713CE4F766";
            string actual;
            actual = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CalculateBuildPackageSHA1SignatureFromPath
        ///</summary>
        [TestMethod()]
        public void CalculateBuildPackageSHA1SignatureFromPathTest_BuildOrderSwitch()
        {
            //Set up directory and files...
            string projectFileExtractionPath = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
            if (!Directory.Exists(projectFileExtractionPath))
                Directory.CreateDirectory(projectFileExtractionPath);

            string file1 = "File1.sql";
            File.WriteAllText(projectFileExtractionPath + file1, Properties.Resources.CreateDatabaseScript);

            string file2 = "File2.sql";
            File.WriteAllText(projectFileExtractionPath + file2, Properties.Resources.CreateTestTablesScript);

            string file3 = "File3.sql";
            File.WriteAllText(projectFileExtractionPath + file3, Properties.Resources.LoggingTable);


            SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            SqlSyncBuildData.ScriptRow row1 = buildData.Script.NewScriptRow();
            row1.BuildOrder = 1;
            row1.FileName = file1;

            SqlSyncBuildData.ScriptRow row2 = buildData.Script.NewScriptRow();
            row2.BuildOrder = 2;
            row2.FileName = file2;

            SqlSyncBuildData.ScriptRow row3 = buildData.Script.NewScriptRow();
            row3.BuildOrder = 3;
            row3.FileName = file3;

            buildData.Script.Rows.Add(row1);
            buildData.Script.Rows.Add(row2);
            buildData.Script.Rows.Add(row3);

            string order123 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 1;
            buildData.Script[1].BuildOrder = 3;
            buildData.Script[2].BuildOrder = 2;
            buildData.AcceptChanges();

            string order132 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 2;
            buildData.Script[1].BuildOrder = 1;
            buildData.Script[2].BuildOrder = 3;
            buildData.AcceptChanges();

            string order213 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 2;
            buildData.Script[1].BuildOrder = 3;
            buildData.Script[2].BuildOrder = 1;
            buildData.AcceptChanges();

            string order231 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 3;
            buildData.Script[1].BuildOrder = 1;
            buildData.Script[2].BuildOrder = 2;
            buildData.AcceptChanges();

            string order312 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 3;
            buildData.Script[1].BuildOrder = 2;
            buildData.Script[2].BuildOrder = 1;
            buildData.AcceptChanges();

            string order321 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreNotEqual(order123, order132);
            Assert.AreNotEqual(order123, order213);
            Assert.AreNotEqual(order123, order231);
            Assert.AreNotEqual(order123, order312);
            Assert.AreNotEqual(order312, order321);

            Assert.AreNotEqual(order132, order213);
            Assert.AreNotEqual(order132, order231);
            Assert.AreNotEqual(order132, order312);
            Assert.AreNotEqual(order132, order321);

            Assert.AreNotEqual(order213, order231);
            Assert.AreNotEqual(order213, order312);
            Assert.AreNotEqual(order213, order321);

            Assert.AreNotEqual(order231, order312);
            Assert.AreNotEqual(order231, order321);

            Assert.AreNotEqual(order231, order321);

        }
        #endregion

        #region CalculateBuildPackageSHA1SignatureFromBatchCollection
        /// <summary>
        ///A test for CalculateBuildPackageSHA1SignatureFromBatchCollection
        ///</summary>
        [TestMethod()]
        public void CalculateBuildPackageSHA1SignatureFromBatchCollectionTest()
        {
            ScriptBatch batch1 = new ScriptBatch(
                "File1.sql",
                new string[] { "Line one goes here", "Line 2 goes there" },
                Guid.NewGuid().ToString());

            ScriptBatch batch2 = new ScriptBatch(
                "File2.sql",
                new string[] { "My Batch Line one goes here", "Second batch Line 2 goes there" },
                Guid.NewGuid().ToString());


            ScriptBatchCollection scriptBatchColl = new ScriptBatchCollection();
            scriptBatchColl.Add(batch1);
            scriptBatchColl.Add(batch2);
            string expected = "E00B044F80A5F40EDAFC53BE8B559BD4DB5229A0";
            string actual;
            actual = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(scriptBatchColl);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CalculateBuildPackageSHA1SignatureFromBatchCollection
        ///</summary>
        [TestMethod()]
        public void CalculateBuildPackageSHA1SignatureFromBatchCollectionTest_BatchOrder()
        {
            ScriptBatch batch1 = new ScriptBatch(
                "File1.sql",
                new string[] { "Line one goes here", "Line 2 goes there" },
                Guid.NewGuid().ToString());

            ScriptBatch batch2 = new ScriptBatch(
                "File2.sql",
                new string[] { "My Batch Line one goes here", "Second batch Line 2 goes there" },
                Guid.NewGuid().ToString());


            ScriptBatchCollection scriptBatchColl = new ScriptBatchCollection();
            scriptBatchColl.Add(batch1);
            scriptBatchColl.Add(batch2);

            string order12 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(scriptBatchColl);

            scriptBatchColl = new ScriptBatchCollection();
            scriptBatchColl.Add(batch2);
            scriptBatchColl.Add(batch1);

            string order21 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(scriptBatchColl);

            Assert.AreNotEqual(order12, order21);

        }
        #endregion

        #region CalculateBuildPackageSHA1_CompareMethodology
        [TestMethod()]
        public void CalculateBuildPackageSHA1_CompareMethodologyTest()
        {
            //Set up directory and files...
            string projectFileExtractionPath = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
            if (!Directory.Exists(projectFileExtractionPath))
                Directory.CreateDirectory(projectFileExtractionPath);

            string file1 = "File1.sql";
            File.WriteAllText(projectFileExtractionPath + file1, Properties.Resources.CreateDatabaseScript);

            string file2 = "File2.sql";
            File.WriteAllText(projectFileExtractionPath + file2, Properties.Resources.CreateTestTablesScript);

            string file3 = "File3.sql";
            File.WriteAllText(projectFileExtractionPath + file3, Properties.Resources.LoggingTable);


            SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            SqlSyncBuildData.ScriptRow row1 = buildData.Script.NewScriptRow();
            row1.BuildOrder = 1;
            row1.FileName = file1;
            row1.StripTransactionText = true;

            SqlSyncBuildData.ScriptRow row2 = buildData.Script.NewScriptRow();
            row2.BuildOrder = 2;
            row2.FileName = file2;
            row2.StripTransactionText = true;

            SqlSyncBuildData.ScriptRow row3 = buildData.Script.NewScriptRow();
            row3.BuildOrder = 3;
            row3.FileName = file3;
            row3.StripTransactionText = true;

            buildData.Script.Rows.Add(row1);
            buildData.Script.Rows.Add(row2);
            buildData.Script.Rows.Add(row3);


            string fromPath = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);


            ScriptBatchCollection batch = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath, fromBatch);

        }
        [TestMethod()]
        public void CalculateBuildPackageSHA1_CompareMethodologyTest_WithTransactionsToRemove()
        {
            //Set up directory and files...
            string projectFileExtractionPath = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
            if (!Directory.Exists(projectFileExtractionPath))
                Directory.CreateDirectory(projectFileExtractionPath);

            string file1 = "File1.sql";
            File.WriteAllText(projectFileExtractionPath + file1, @"This is My script
with my 
COMMIT TRANS
test");

            string file2 = "File2.sql";
            File.WriteAllText(projectFileExtractionPath + file2, Properties.Resources.CreateTestTablesScript);

            string file3 = "File3.sql";
            File.WriteAllText(projectFileExtractionPath + file3, @"This is another test that has
--ROLLBACK TRANSACTION
where the 
BEGIN TRAN
needs to be removed");


            SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            SqlSyncBuildData.ScriptRow row1 = buildData.Script.NewScriptRow();
            row1.BuildOrder = 1;
            row1.FileName = file1;
            row1.StripTransactionText = true;

            SqlSyncBuildData.ScriptRow row2 = buildData.Script.NewScriptRow();
            row2.BuildOrder = 2;
            row2.FileName = file2;
            row2.StripTransactionText = true;

            SqlSyncBuildData.ScriptRow row3 = buildData.Script.NewScriptRow();
            row3.BuildOrder = 3;
            row3.FileName = file3;
            row3.StripTransactionText = true;

            buildData.Script.Rows.Add(row1);
            buildData.Script.Rows.Add(row2);
            buildData.Script.Rows.Add(row3);


            string fromPath = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);


            ScriptBatchCollection batch = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath, fromBatch);

        }
        [TestMethod()]
        public void CalculateBuildPackageSHA1_CompareMethodologyTest_WithTransactionsButKeep()
        {
            //Set up directory and files...
            string projectFileExtractionPath = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
            if (!Directory.Exists(projectFileExtractionPath))
                Directory.CreateDirectory(projectFileExtractionPath);

            string file1 = "File1.sql";
            File.WriteAllText(projectFileExtractionPath + file1, @"This is My script
with my 
COMMIT TRANS
test");

            string file2 = "File2.sql";
            File.WriteAllText(projectFileExtractionPath + file2, Properties.Resources.CreateTestTablesScript);

            string file3 = "File3.sql";
            File.WriteAllText(projectFileExtractionPath + file3, @"This is another test that has
--ROLLBACK TRANSACTION
where the 
BEGIN TRAN
needs to be removed");


            SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            SqlSyncBuildData.ScriptRow row1 = buildData.Script.NewScriptRow();
            row1.BuildOrder = 1;
            row1.FileName = file1;
            row1.StripTransactionText = false;

            SqlSyncBuildData.ScriptRow row2 = buildData.Script.NewScriptRow();
            row2.BuildOrder = 2;
            row2.FileName = file2;
            row2.StripTransactionText = false;

            SqlSyncBuildData.ScriptRow row3 = buildData.Script.NewScriptRow();
            row3.BuildOrder = 3;
            row3.FileName = file3;
            row3.StripTransactionText = false;

            buildData.Script.Rows.Add(row1);
            buildData.Script.Rows.Add(row2);
            buildData.Script.Rows.Add(row3);


            string fromPath = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);


            ScriptBatchCollection batch = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath, fromBatch);

        }
        [TestMethod()]
        public void CalculateBuildPackageSHA1_CompareMethodologyTest_OrderCheckingWithTransactionsToRemove()
        {
            //Set up directory and files...
            string projectFileExtractionPath = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";
            if (!Directory.Exists(projectFileExtractionPath))
                Directory.CreateDirectory(projectFileExtractionPath);

            string file1 = "File1.sql";
            File.WriteAllText(projectFileExtractionPath + file1, @"This is My script
with my 
COMMIT TRANS
test");

            string file2 = "File2.sql";
            File.WriteAllText(projectFileExtractionPath + file2, Properties.Resources.CreateTestTablesScript);

            string file3 = "File3.sql";
            File.WriteAllText(projectFileExtractionPath + file3, @"This is another test that has
--ROLLBACK TRANSACTION
where the 
BEGIN TRAN
needs to be removed");


            SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
            SqlSyncBuildData.ScriptRow row1 = buildData.Script.NewScriptRow();
            row1.BuildOrder = 1;
            row1.FileName = file1;
            row1.StripTransactionText = true;

            SqlSyncBuildData.ScriptRow row2 = buildData.Script.NewScriptRow();
            row2.BuildOrder = 2;
            row2.FileName = file2;
            row2.StripTransactionText = true;

            SqlSyncBuildData.ScriptRow row3 = buildData.Script.NewScriptRow();
            row3.BuildOrder = 3;
            row3.FileName = file3;
            row3.StripTransactionText = true;

            buildData.Script.Rows.Add(row1);
            buildData.Script.Rows.Add(row2);
            buildData.Script.Rows.Add(row3);


            string fromPath123 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            ScriptBatchCollection batch = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch123 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            buildData.Script[0].BuildOrder = 2;
            buildData.Script[1].BuildOrder = 1;
            buildData.Script[2].BuildOrder = 3;
            buildData.AcceptChanges();

            string fromPath213 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(projectFileExtractionPath, buildData);

            batch = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch213 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);


            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath123, fromBatch123);
            Assert.AreEqual(fromPath213, fromBatch213);
            Assert.AreNotEqual(fromPath123, fromBatch213);
            Assert.AreNotEqual(fromPath213, fromBatch123);

        }
        #endregion


        #region  Individual Script Hash Calc
        [TestMethod()]
        public void CalculateScriptSHA1_CompareResults()
        {
            string script = @"/* 
Source Server:	localhost\sqlexpress
Source Db:	SqlBuildTest_SyncTest1
Process Date:	7/21/2014 1:36:55 PM
Object Scripted:dbo.SyncTestTable
Object Type:	Table
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
Script PK with Table:False
*/
SET ANSI_NULLS ON
GO


SET QUOTED_IDENTIFIER ON
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SyncTestTable]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SyncTestTable](
	[ColumnTable1] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ColumnTable2] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
END
";

            string[] arrScripts = SqlBuildHelper.ReadBatchFromScriptText(script,true, false).ToArray();
            string hashFromArray;
            SqlBuildFileHelper.GetSHA1Hash(arrScripts, out hashFromArray);
            string hashFromString = SqlBuildFileHelper.GetSHA1Hash(script.ClearTrailingCarriageReturn());
            Assert.AreEqual(hashFromString,hashFromArray);
        }
        #endregion

        #region CleanProjectFileForRemoteExecutionTest
        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public void CleanProjectFileForRemoteExecutionTest_FileNotExist()
        {
            string fileName = string.Empty;
            byte[] expected = new byte[0];
            byte[] actual;
            SqlSyncBuildData cleanedBuildData;
            actual = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(fileName, out cleanedBuildData);
            Assert.IsTrue(actual.Length == 0);
            Assert.AreEqual(expected.Length, actual.Length);
            Assert.AreEqual(81, cleanedBuildData.GetXml().Length); //The size of an "empty" build data object

            Assert.IsTrue(cleanedBuildData.ScriptRun.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.CodeReview.Rows.Count == 0);


        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public void CleanProjectFileForRemoteExecutionTest_NothingToClean()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            string zipFileName = init.GetTrulyUniqueFile() + ".sbm";
            string path = Path.GetDirectoryName(zipFileName);

            SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;
            SqlSyncBuildData cleanedBuildData;
            actual = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(zipFileName, out cleanedBuildData);
            Assert.IsTrue(cleanedBuildData.GetXml().ToString().Length > 100);
            Assert.AreEqual(buildData.GetXml().ToString().Length, cleanedBuildData.GetXml().ToString().Length);
            Assert.IsTrue(1500 <= actual.Length, string.Format("Actual length of cleaned XML {0}.\r\n{1}", actual.Length.ToString(), cleanedBuildData.GetXml())); //can't get exact length due to variations in guids and dates.
            Assert.IsTrue(expected.Length == actual.Length);

            Assert.IsTrue(cleanedBuildData.ScriptRun.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.CodeReview.Rows.Count == 0);
            Assert.AreEqual(buildData.Script.Rows.Count, cleanedBuildData.Script.Rows.Count);

        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public void CleanProjectFileForRemoteExecutionTest_CleanOutUnitTest()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            //Add in code review rows
            buildData.CodeReview.AddCodeReviewRow(
                Guid.NewGuid(),
                buildData.Script[0],
                DateTime.Now,
                "Reviewer",
                1,
                "Comment",
                "12345",
                "AABBCCDD",
                "EEFFGGHHII");

            buildData.AcceptChanges();

            string zipFileName = init.GetTrulyUniqueFile();

            string path = Path.GetDirectoryName(zipFileName);
            string projectFileName = path + @"\" + XmlFileNames.MainProjectFile;
            buildData.WriteXml(projectFileName);
            SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;

            SqlSyncBuildData cleanedBuildData;
            actual = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(zipFileName, out cleanedBuildData);
            Assert.IsTrue(actual.Length >= 1200);  //can't get exact length due to variations in guids and dates.
            Assert.IsTrue(cleanedBuildData.GetXml().ToString().Length > 100);
            Assert.IsTrue(buildData.GetXml().ToString().Length > cleanedBuildData.GetXml().ToString().Length);

            Assert.IsTrue(cleanedBuildData.ScriptRun.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.CodeReview.Rows.Count == 0);
            Assert.AreEqual(buildData.Script.Rows.Count, cleanedBuildData.Script.Rows.Count);

        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public void CleanProjectFileForRemoteExecutionTest_CleanOutBuildRowsTest()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            buildData.Builds.AddBuildsRow((SqlSyncBuildData.SqlSyncBuildProjectRow)buildData.SqlSyncBuildProject.Rows[0]);
            buildData.Build.AddBuildRow("Script", "Development", DateTime.Now, DateTime.Now, "Server", "Committed", Guid.NewGuid().ToString(), "user", buildData.Builds[0]);

            buildData.AcceptChanges();

            string zipFileName = init.GetTrulyUniqueFile();

            string path = Path.GetDirectoryName(zipFileName);
            string projectFileName = path + @"\" + XmlFileNames.MainProjectFile;
            buildData.WriteXml(projectFileName);
            SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;

            SqlSyncBuildData cleanedBuildData;
            actual = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(zipFileName, out cleanedBuildData);
            Assert.IsTrue(cleanedBuildData.GetXml().ToString().Length > 100);
            Assert.IsTrue(2000 <= actual.Length);  //can't get exact length due to variations in guids and dates.
            Assert.IsTrue(buildData.GetXml().ToString().Length > cleanedBuildData.GetXml().ToString().Length);

            Assert.IsTrue(cleanedBuildData.ScriptRun.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.CodeReview.Rows.Count == 0);
            Assert.AreEqual(buildData.Script.Rows.Count, cleanedBuildData.Script.Rows.Count);


        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public void CleanProjectFileForRemoteExecutionTest_CleanOutScriptRunRowsTest()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            buildData.Builds.AddBuildsRow((SqlSyncBuildData.SqlSyncBuildProjectRow)buildData.SqlSyncBuildProject.Rows[0]);
            buildData.Build.AddBuildRow("Script", "Development", DateTime.Now, DateTime.Now, "Server", "Committed", Guid.NewGuid().ToString(), "user", buildData.Builds[0]);
            buildData.ScriptRun.AddScriptRunRow("HASH", "Committed", "FileName", 2.2, DateTime.Now, DateTime.Now, true, "Database", Guid.NewGuid().ToString(), (SqlSyncBuildData.BuildRow)buildData.Build[0]);


            buildData.AcceptChanges();

            string zipFileName = init.GetTrulyUniqueFile();

            string path = Path.GetDirectoryName(zipFileName);
            string projectFileName = path + @"\" + XmlFileNames.MainProjectFile;
            buildData.WriteXml(projectFileName);
            SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;

            SqlSyncBuildData cleanedBuildData;
            actual = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(zipFileName, out cleanedBuildData);
            Assert.IsTrue(2000 <= actual.Length);  //can't get exact length due to variations in guids and dates.
            Assert.IsTrue(cleanedBuildData.GetXml().ToString().Length > 100);
            Assert.IsTrue(buildData.GetXml().ToString().Length > cleanedBuildData.GetXml().ToString().Length);

            Assert.IsTrue(cleanedBuildData.ScriptRun.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Rows.Count == 0);
            Assert.IsTrue(cleanedBuildData.CodeReview.Rows.Count == 0);
            Assert.AreEqual(buildData.Script.Rows.Count, cleanedBuildData.Script.Rows.Count);

        }

        #endregion
    }
}
