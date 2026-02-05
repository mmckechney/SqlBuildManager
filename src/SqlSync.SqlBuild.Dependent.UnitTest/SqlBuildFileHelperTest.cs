using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#nullable enable
namespace SqlSync.SqlBuild.Dependent.UnitTest
{


    /// <summary>
    ///This is a test class for SqlBuildFileHelperTest and is intended
    ///to contain all SqlBuildFileHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SqlBuildFileHelperTest
    {


        public TestContext TestContext { get; set; } = null!;

        private static List<Initialization> initColl = new();

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
        ///A test for CreateShellSqlSyncBuildDataModel
        ///</summary>
        [TestMethod()]
        public void CreateShellSqlSyncBuildDataModelTest()
        {

            SqlSyncBuildDataModel actual;
            actual = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.SqlSyncBuildProject.Count);
            Assert.AreEqual(0, actual.Script.Count);
            Assert.AreEqual(0, actual.Build.Count);
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
        public void UpdateObsoleteXmlNamespace_TestEmptyFileName()
        {
            Assert.ThrowsExactly<System.ArgumentException>(() => SqlBuildFileHelper.UpdateObsoleteXmlNamespace(string.Empty));
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
        public async Task CalculateBuildPackageSHA1SignatureFromPathTest_GetHashSuccessfully()
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


            SqlSyncBuildDataModel buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Script row1 = new Script { BuildOrder = 1, FileName = file1 };
            Script row2 = new Script { BuildOrder = 2, FileName = file2 };
            Script row3 = new Script { BuildOrder = 3, FileName = file3 };

            buildData.Script.Add(row1);
            buildData.Script.Add(row2);
            buildData.Script.Add(row3);

            string expected = "4E0F54A4BA40DC62A78822B20C7D83713CE4F766";
            string actual;
            actual = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CalculateBuildPackageSHA1SignatureFromPath
        ///</summary>
        [TestMethod()]
        public async Task CalculateBuildPackageSHA1SignatureFromPathTest_BuildOrderSwitch()
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


            SqlSyncBuildDataModel buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Script row1 = new Script { BuildOrder = 1, FileName = file1 };
            Script row2 = new Script { BuildOrder = 2, FileName = file2 };
            Script row3 = new Script { BuildOrder = 3, FileName = file3 };

            buildData.Script.Add(row1);
            buildData.Script.Add(row2);
            buildData.Script.Add(row3);

            string order123 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 1;
            buildData.Script[1].BuildOrder = 3;
            buildData.Script[2].BuildOrder = 2;

            string order132 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 2;
            buildData.Script[1].BuildOrder = 1;
            buildData.Script[2].BuildOrder = 3;

            string order213 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 2;
            buildData.Script[1].BuildOrder = 3;
            buildData.Script[2].BuildOrder = 1;

            string order231 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 3;
            buildData.Script[1].BuildOrder = 1;
            buildData.Script[2].BuildOrder = 2;

            string order312 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            buildData.Script[0].BuildOrder = 3;
            buildData.Script[1].BuildOrder = 2;
            buildData.Script[2].BuildOrder = 1;

            string order321 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

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
        public async Task CalculateBuildPackageSHA1_CompareMethodologyTest()
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


            SqlSyncBuildDataModel buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Script row1 = new Script { BuildOrder = 1, FileName = file1, StripTransactionText = true };
            Script row2 = new Script { BuildOrder = 2, FileName = file2, StripTransactionText = true };
            Script row3 = new Script { BuildOrder = 3, FileName = file3, StripTransactionText = true };

            buildData.Script.Add(row1);
            buildData.Script.Add(row2);
            buildData.Script.Add(row3);


            string fromPath = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            ScriptBatchCollection batch = scriptBatcher.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath, fromBatch);

        }
        [TestMethod()]
        public async Task CalculateBuildPackageSHA1_CompareMethodologyTest_WithTransactionsToRemove()
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


            SqlSyncBuildDataModel buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Script row1 = new Script { BuildOrder = 1, FileName = file1, StripTransactionText = true };
            Script row2 = new Script { BuildOrder = 2, FileName = file2, StripTransactionText = true };
            Script row3 = new Script { BuildOrder = 3, FileName = file3, StripTransactionText = true };

            buildData.Script.Add(row1);
            buildData.Script.Add(row2);
            buildData.Script.Add(row3);


            string fromPath = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            ScriptBatchCollection batch = scriptBatcher.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath, fromBatch);

        }
        [TestMethod()]
        public async Task CalculateBuildPackageSHA1_CompareMethodologyTest_WithTransactionsButKeep()
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


            SqlSyncBuildDataModel buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Script row1 = new Script { BuildOrder = 1, FileName = file1, StripTransactionText = false };
            Script row2 = new Script { BuildOrder = 2, FileName = file2, StripTransactionText = false };
            Script row3 = new Script { BuildOrder = 3, FileName = file3, StripTransactionText = false };

            buildData.Script.Add(row1);
            buildData.Script.Add(row2);
            buildData.Script.Add(row3);


            string fromPath = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            ScriptBatchCollection batch = scriptBatcher.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            if (Directory.Exists(projectFileExtractionPath))
                Directory.Delete(projectFileExtractionPath, true);

            Assert.AreEqual(fromPath, fromBatch);

        }
        [TestMethod()]
        public async Task CalculateBuildPackageSHA1_CompareMethodologyTest_OrderCheckingWithTransactionsToRemove()
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


            SqlSyncBuildDataModel buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            Script row1 = new Script { BuildOrder = 1, FileName = file1, StripTransactionText = true };
            Script row2 = new Script { BuildOrder = 2, FileName = file2, StripTransactionText = true };
            Script row3 = new Script { BuildOrder = 3, FileName = file3, StripTransactionText = true };

            buildData.Script.Add(row1);
            buildData.Script.Add(row2);
            buildData.Script.Add(row3);


            string fromPath123 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            ScriptBatchCollection batch = scriptBatcher.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
            string fromBatch123 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromBatchCollection(batch);

            buildData.Script[0].BuildOrder = 2;
            buildData.Script[1].BuildOrder = 1;
            buildData.Script[2].BuildOrder = 3;

            string fromPath213 = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(projectFileExtractionPath, buildData);

            batch = scriptBatcher.LoadAndBatchSqlScripts(buildData, projectFileExtractionPath);
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
            IScriptBatcher batcher = new DefaultScriptBatcher();
            ISqlBuildFileHelper fileHelper = new DefaultSqlBuildFileHelper();
            string[] arrScripts = batcher.ReadBatchFromScriptText(script, true, false).ToArray();
            string hashFromArray;
            hashFromArray = fileHelper.GetSHA1Hash(arrScripts);
            string hashFromString = fileHelper.GetSHA1Hash(script.ClearTrailingCarriageReturn());
            Assert.AreEqual(hashFromString, hashFromArray);
        }
        #endregion

        #region CleanProjectFileForRemoteExecutionTest
        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public async Task CleanProjectFileForRemoteExecutionTest_FileNotExist()
        {
            string fileName = string.Empty;
            byte[] expected = new byte[0];
            byte[] actual;
            SqlSyncBuildDataModel cleanedBuildData;
            (actual, cleanedBuildData) = await SqlBuildFileHelper.CleanProjectFileForRemoteExecutionAsync(fileName);
            Assert.IsTrue(actual.Length == 0);
            Assert.AreEqual(expected.Length, actual.Length);
            // Validate the shell model was returned
            Assert.IsTrue(cleanedBuildData.ScriptRun.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Count == 0);


        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public async Task CleanProjectFileForRemoteExecutionTest_NothingToClean()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (Script row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            string zipFileName = init.GetTrulyUniqueFile() + ".sbm";
            string path = Path.GetDirectoryName(zipFileName) ?? Directory.GetCurrentDirectory();

            await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;
            SqlSyncBuildDataModel cleanedBuildData;
            (actual, cleanedBuildData) = await SqlBuildFileHelper.CleanProjectFileForRemoteExecutionAsync(zipFileName);
            Assert.IsTrue(expected.Length == actual.Length);

            Assert.IsTrue(cleanedBuildData.ScriptRun.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Count == 0);
            Assert.AreEqual(buildData.Script.Count, cleanedBuildData.Script.Count);

        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public async Task CleanProjectFileForRemoteExecutionTest_CleanOutUnitTest()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (Script row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);



            string zipFileName = init.GetTrulyUniqueFile();

            string path = Path.GetDirectoryName(zipFileName) ?? Directory.GetCurrentDirectory();
            string projectFileName = Path.Combine(path, XmlFileNames.MainProjectFile);
            await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;

            SqlSyncBuildDataModel cleanedBuildData;
            (actual, cleanedBuildData) = await SqlBuildFileHelper.CleanProjectFileForRemoteExecutionAsync(zipFileName);
            Assert.IsTrue(actual.Length >= 1200);  //can't get exact length due to variations in guids and dates.

            Assert.IsTrue(cleanedBuildData.ScriptRun.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Count == 0);
            Assert.AreEqual(buildData.Script.Count, cleanedBuildData.Script.Count);

        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public async Task CleanProjectFileForRemoteExecutionTest_CleanOutBuildRowsTest()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (Script row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            buildData.Build.Add(new Build() { Name = "Test" });
            buildData.Build.Add(new Build("Script", "Development", DateTime.Now, DateTime.Now, "Server", BuildItemStatus.Committed, Guid.NewGuid().ToString(), "user"));


            string zipFileName = init.GetTrulyUniqueFile();

            string path = Path.GetDirectoryName(zipFileName) ?? Directory.GetCurrentDirectory();
            string projectFileName = Path.Combine(path, XmlFileNames.MainProjectFile);
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projectFileName, buildData);
            await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;

            SqlSyncBuildDataModel cleanedBuildData;
            (actual, cleanedBuildData) = await SqlBuildFileHelper.CleanProjectFileForRemoteExecutionAsync(zipFileName);

            Assert.IsTrue(cleanedBuildData.ScriptRun.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Count == 0);
            Assert.AreEqual(buildData.Script.Count, cleanedBuildData.Script.Count);


        }

        /// <summary>
        ///A test for CleanProjectFileForRemoteExecution
        ///</summary>
        [TestMethod()]
        public async Task CleanProjectFileForRemoteExecutionTest_CleanOutScriptRunRowsTest()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            foreach (Script row in buildData.Script)
                row.FileName = Path.GetFileName(row.FileName);

            buildData.Build.Add(new Build() { Name = "Hello" });
            buildData.Build.Add (new Build("Script", "Development", DateTime.Now, DateTime.Now, "Server",BuildItemStatus.Committed, Guid.NewGuid().ToString(), "user"));
            buildData.ScriptRun.Add(new ScriptRun("HASH", "Committed", "FileName", 2.2, DateTime.Now, DateTime.Now, true, "Database", Guid.NewGuid().ToString(),"EWEW"));

            string zipFileName = init.GetTrulyUniqueFile();

            string path = Path.GetDirectoryName(zipFileName) ?? Directory.GetCurrentDirectory();
            string projectFileName = Path.Combine(path, XmlFileNames.MainProjectFile);
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projectFileName, buildData);
            await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(buildData, path, zipFileName, false);

            byte[] expected = File.ReadAllBytes(zipFileName);
            byte[] actual;

            SqlSyncBuildDataModel cleanedBuildData;
            (actual, cleanedBuildData) = await SqlBuildFileHelper.CleanProjectFileForRemoteExecutionAsync(zipFileName);
            Assert.IsTrue(2000 <= actual.Length);  //can't get exact length due to variations in guids and dates.

            Assert.IsTrue(cleanedBuildData.ScriptRun.Count == 0);
            Assert.IsTrue(cleanedBuildData.Build.Count == 0);
            Assert.AreEqual(buildData.Script.Count, cleanedBuildData.Script.Count);

        }

        #endregion
    }
}
