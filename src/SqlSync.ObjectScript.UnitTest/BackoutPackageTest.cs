using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Constants;
using System.IO;

namespace SqlSync.ObjectScript.UnitTest
{
    /// <summary>
    /// Unit tests for BackoutPackage class (non-database-dependent tests)
    /// </summary>
    [TestClass]
    public class BackoutPackageTest
    {
        #region GetDefaultPackageName Tests

        [TestMethod]
        public void GetDefaultPackageName_SimpleFileName_ShouldPrependBackout()
        {
            string sourcePath = @"C:\Packages\MyPackage.sbm";

            string result = BackoutPackage.GetDefaultPackageName(sourcePath);

            Assert.AreEqual(@"C:\Packages\Backout_MyPackage.sbm", result);
        }

        [TestMethod]
        public void GetDefaultPackageName_FileNameWithSpaces_ShouldPrependBackout()
        {
            string sourcePath = @"C:\Packages\My Package.sbm";

            string result = BackoutPackage.GetDefaultPackageName(sourcePath);

            Assert.AreEqual(@"C:\Packages\Backout_My Package.sbm", result);
        }

        [TestMethod]
        public void GetDefaultPackageName_DeepPath_ShouldPrependBackoutInSameDirectory()
        {
            string sourcePath = @"C:\Projects\Release\2024\Q1\Package.sbm";

            string result = BackoutPackage.GetDefaultPackageName(sourcePath);

            Assert.AreEqual(@"C:\Projects\Release\2024\Q1\Backout_Package.sbm", result);
        }

        [TestMethod]
        public void GetDefaultPackageName_RootPath_ShouldWork()
        {
            string sourcePath = @"C:\Package.sbm";

            string result = BackoutPackage.GetDefaultPackageName(sourcePath);

            Assert.AreEqual(@"C:\Backout_Package.sbm", result);
        }

        #endregion

        #region CreateRoutineDropScript Tests

        [TestMethod]
        public void CreateRoutineDropScript_StoredProcedure_ShouldReturnValidDropScript()
        {
            string schema = "dbo";
            string objectName = "usp_GetCustomers";
            string objectType = DbScriptDescription.StoredProcedure;

            string result = BackoutPackage.CreateRoutineDropScript(schema, objectName, objectType);

            Assert.IsTrue(result.Contains("DROP PROCEDURE"));
            Assert.IsTrue(result.Contains("[dbo].[usp_GetCustomers]"));
            Assert.IsTrue(result.Contains("IF  EXISTS"));
        }

        [TestMethod]
        public void CreateRoutineDropScript_UserDefinedFunction_ShouldReturnValidDropScript()
        {
            string schema = "dbo";
            string objectName = "fn_GetTotal";
            string objectType = DbScriptDescription.UserDefinedFunction;

            string result = BackoutPackage.CreateRoutineDropScript(schema, objectName, objectType);

            Assert.IsTrue(result.Contains("DROP FUNCTION"));
            Assert.IsTrue(result.Contains("[dbo].[fn_GetTotal]"));
            Assert.IsTrue(result.Contains("IF  EXISTS"));
        }

        [TestMethod]
        public void CreateRoutineDropScript_View_ShouldReturnValidDropScript()
        {
            string schema = "dbo";
            string objectName = "vw_CustomerSummary";
            string objectType = DbScriptDescription.View;

            string result = BackoutPackage.CreateRoutineDropScript(schema, objectName, objectType);

            Assert.IsTrue(result.Contains("DROP VIEW"));
            Assert.IsTrue(result.Contains("[dbo].[vw_CustomerSummary]"));
            Assert.IsTrue(result.Contains("IF  EXISTS"));
        }

        [TestMethod]
        public void CreateRoutineDropScript_Trigger_ShouldReturnValidDropScript()
        {
            string schema = "dbo";
            // Trigger names include table and trigger name separated by hyphen
            string objectName = "Customers - tr_CustomersUpdate";
            string objectType = DbScriptDescription.Trigger;

            string result = BackoutPackage.CreateRoutineDropScript(schema, objectName, objectType);

            Assert.IsTrue(result.Contains("DROP TRIGGER"));
            Assert.IsTrue(result.Contains("tr_CustomersUpdate"));
            Assert.IsTrue(result.Contains("IF  EXISTS"));
        }

        [TestMethod]
        public void CreateRoutineDropScript_UnknownObjectType_ShouldReturnEmptyString()
        {
            string schema = "dbo";
            string objectName = "SomeObject";
            string objectType = "UnknownType";

            string result = BackoutPackage.CreateRoutineDropScript(schema, objectName, objectType);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CreateRoutineDropScript_StoredProcedure_ShouldContainGOStatement()
        {
            string result = BackoutPackage.CreateRoutineDropScript("dbo", "TestProc", DbScriptDescription.StoredProcedure);

            Assert.IsTrue(result.Contains("GO"));
        }

        [TestMethod]
        public void CreateRoutineDropScript_CustomSchema_ShouldUseCorrectSchema()
        {
            string schema = "sales";
            string objectName = "usp_GetOrders";
            string objectType = DbScriptDescription.StoredProcedure;

            string result = BackoutPackage.CreateRoutineDropScript(schema, objectName, objectType);

            Assert.IsTrue(result.Contains("[sales].[usp_GetOrders]"));
        }

        [TestMethod]
        public void CreateRoutineDropScript_Function_ChecksCorrectObjectTypes()
        {
            string result = BackoutPackage.CreateRoutineDropScript("dbo", "fn_Test", DbScriptDescription.UserDefinedFunction);

            // Function check should include FN, IF, TF, FS, FT
            Assert.IsTrue(result.Contains("N'FN'"));
            Assert.IsTrue(result.Contains("N'IF'"));
            Assert.IsTrue(result.Contains("N'TF'"));
        }

        #endregion

        #region SetBackoutSourceDatabaseAndServer Tests

        [TestMethod]
        public void SetBackoutSourceDatabaseAndServer_WithValidList_ShouldUpdateAllItems()
        {
            var list = new System.Collections.Generic.List<SqlSync.SqlBuild.Objects.ObjectUpdates>
            {
                new SqlSync.SqlBuild.Objects.ObjectUpdates { SourceDatabase = "", SourceServer = "" },
                new SqlSync.SqlBuild.Objects.ObjectUpdates { SourceDatabase = "", SourceServer = "" }
            };
            string serverName = "TestServer";
            string databaseName = "TestDatabase";

            BackoutPackage.SetBackoutSourceDatabaseAndServer(ref list, serverName, databaseName);

            foreach (var item in list)
            {
                Assert.AreEqual("TestDatabase", item.SourceDatabase);
                Assert.AreEqual("TestServer", item.SourceServer);
            }
        }

        [TestMethod]
        public void SetBackoutSourceDatabaseAndServer_WithEmptyList_ShouldNotThrow()
        {
            var list = new System.Collections.Generic.List<SqlSync.SqlBuild.Objects.ObjectUpdates>();

            BackoutPackage.SetBackoutSourceDatabaseAndServer(ref list, "Server", "Database");

            Assert.AreEqual(0, list.Count);
        }

        #endregion
    }
}
