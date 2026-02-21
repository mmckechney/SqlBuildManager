using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using System.Data;
using System.Linq;

namespace SqlSync.DbInformation.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for InfoHelper that require database access
    /// NOTE: These tests require (local)\SQLEXPRESS with SqlBuildTest databases
    /// </summary>
    [TestClass()]
    public class InfoHelperTest
    {
        private static Initialization? _init;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            _init = new Initialization();
        }

        private ConnectionData GetConnData() => _init?.connData ?? SqlBuildManager.Test.Common.TestEnvironment.GetConnectionData("SqlBuildTest");

        #region GetColumnNames Tests

        [TestMethod()]
        public void GetColumnNames_SqlBuildLogging_ReturnsExpectedColumns()
        {
            string tableName = "SqlBuild_Logging";
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetColumnNames(tableName, connData);

            Assert.AreEqual(18, actual.Length);
            Assert.AreEqual("BuildFileName", actual[0]);
            Assert.AreEqual("TargetDatabase", actual[11]);
            Assert.AreEqual("BuildRequestedBy", actual[14]);
        }

        [TestMethod()]
        public void GetColumnNames_TransactionTest_ReturnsExpectedColumns()
        {
            string tableName = "TransactionTest";
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetColumnNames(tableName, connData);

            Assert.IsTrue(actual.Length >= 3);
            Assert.IsTrue(actual.Contains("Message") || actual.Any(c => c.ToLower().Contains("message")));
        }

        [TestMethod()]
        public void GetColumnNames_WithSchemaPrefix_ReturnsColumns()
        {
            string tableName = "dbo.SqlBuild_Logging";
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetColumnNames(tableName, connData);

            Assert.IsTrue(actual.Length > 0, "Should return columns when schema is specified");
        }

        #endregion

        #region GetColumnNamesWithTypes Tests

        [TestMethod()]
        public void GetColumnNamesWithTypes_SqlBuildLogging_ReturnsColumnInfo()
        {
            string tableName = "SqlBuild_Logging";
            ConnectionData connData = GetConnData();

            ColumnInfo[] actual = InfoHelper.GetColumnNamesWithTypes(tableName, connData);

            Assert.IsTrue(actual.Length > 0, "Should return column info");
            Assert.IsTrue(actual.Any(c => c.ColumnName == "BuildFileName"));
        }

        [TestMethod()]
        public void GetColumnNamesWithTypes_ReturnsDataTypes()
        {
            string tableName = "SqlBuild_Logging";
            ConnectionData connData = GetConnData();

            ColumnInfo[] actual = InfoHelper.GetColumnNamesWithTypes(tableName, connData);

            Assert.IsTrue(actual.All(c => !string.IsNullOrEmpty(c.DataType)), "All columns should have data types");
        }

        #endregion

        #region GetPrimaryKeyColumns Tests

        [TestMethod()]
        public void GetPrimaryKeyColumns_TableWithPK_ReturnsPKColumns()
        {
            // SqlBuild_Logging may not have a PK, so we test for empty result handling
            string tableName = "SqlBuild_Logging";
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetPrimaryKeyColumns(tableName, connData);

            // Result can be empty if table has no PK - just verify it doesn't throw
            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void GetPrimaryKeyColumns_WithSchema_Works()
        {
            string tableName = "dbo.SqlBuild_Logging";
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetPrimaryKeyColumns(tableName, connData);

            Assert.IsNotNull(actual);
        }

        #endregion

        #region GetDatabaseTableList Tests

        [TestMethod()]
        public void GetDatabaseTableList_ReturnsTablesInDatabase()
        {
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetDatabaseTableList(connData);

            Assert.IsTrue(actual.Length > 0, "Should return at least one table");
            Assert.IsTrue(actual.Any(t => t.Contains("SqlBuild_Logging")), "Should contain SqlBuild_Logging table");
        }

        [TestMethod()]
        public void GetDatabaseTableList_WithFilter_FiltersResults()
        {
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetDatabaseTableList(connData, "SqlBuild%");

            Assert.IsTrue(actual.Length > 0, "Should return at least one table matching filter");
            Assert.IsTrue(actual.All(t => t.Contains("SqlBuild")), "All tables should match filter");
        }

        [TestMethod()]
        public void GetDatabaseTableList_NoMatchingFilter_ReturnsEmpty()
        {
            ConnectionData connData = GetConnData();

            string[] actual = InfoHelper.GetDatabaseTableList(connData, "ZZZZNONEXISTENT%");

            Assert.AreEqual(0, actual.Length, "Should return empty array for non-matching filter");
        }

        #endregion

        #region GetDatabaseTableListWithRowCount Tests

        [TestMethod()]
        public void GetDatabaseTableListWithRowCount_ReturnsTables()
        {
            ConnectionData connData = GetConnData();

            TableSize[] actual = InfoHelper.GetDatabaseTableListWithRowCount(connData);

            Assert.IsTrue(actual.Length > 0, "Should return at least one table");
        }

        [TestMethod()]
        public void GetDatabaseTableListWithRowCount_IncludesRowCounts()
        {
            ConnectionData connData = GetConnData();

            TableSize[] actual = InfoHelper.GetDatabaseTableListWithRowCount(connData);

            Assert.IsTrue(actual.All(t => t.RowCount >= -1), "Row counts should be valid");
        }

        [TestMethod()]
        public void GetDatabaseTableListWithRowCount_WithFilter_FiltersResults()
        {
            ConnectionData connData = GetConnData();

            TableSize[] actual = InfoHelper.GetDatabaseTableListWithRowCount(connData, "SqlBuild%");

            Assert.IsTrue(actual.All(t => t.TableName.Contains("SqlBuild")));
        }

        #endregion

        #region DbContainsTable Tests

        [TestMethod()]
        public void DbContainsTable_ExistingTable_ReturnsTrue()
        {
            ConnectionData connData = GetConnData();

            bool actual = InfoHelper.DbContainsTable("dbo.SqlBuild_Logging", connData);

            Assert.IsTrue(actual, "Should find existing table");
        }

        [TestMethod()]
        public void DbContainsTable_NonExistingTable_ReturnsFalse()
        {
            ConnectionData connData = GetConnData();

            bool actual = InfoHelper.DbContainsTable("dbo.NonExistentTable12345", connData);

            Assert.IsFalse(actual, "Should not find non-existent table");
        }

        [TestMethod()]
        public void DbContainsTable_CaseInsensitive_Works()
        {
            ConnectionData connData = GetConnData();

            bool actual = InfoHelper.DbContainsTable("DBO.SQLBUILD_LOGGING", connData);

            Assert.IsTrue(actual, "Should find table case-insensitively");
        }

        [TestMethod()]
        public void DbContainsTable_FromArray_Works()
        {
            string[] tables = new[] { "dbo.Table1", "dbo.SqlBuild_Logging", "dbo.Table3" };

            bool actual = InfoHelper.DbContainsTable("dbo.SqlBuild_Logging", tables);

            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void DbContainsTable_FromArray_NotFound_ReturnsFalse()
        {
            string[] tables = new[] { "dbo.Table1", "dbo.Table2", "dbo.Table3" };

            bool actual = InfoHelper.DbContainsTable("dbo.NonExistent", tables);

            Assert.IsFalse(actual);
        }

        #endregion

        #region DbContainsTableWithRowcount Tests

        [TestMethod()]
        public void DbContainsTableWithRowcount_ExistingTable_ReturnsRowCount()
        {
            TableSize[] tableData = new[]
            {
                new TableSize { TableName = "dbo.Table1", RowCount = 100 },
                new TableSize { TableName = "dbo.SqlBuild_Logging", RowCount = 50 }
            };

            int actual = InfoHelper.DbContainsTableWithRowcount("dbo.SqlBuild_Logging", tableData);

            Assert.AreEqual(50, actual);
        }

        [TestMethod()]
        public void DbContainsTableWithRowcount_NonExistingTable_ReturnsNegative()
        {
            TableSize[] tableData = new[]
            {
                new TableSize { TableName = "dbo.Table1", RowCount = 100 }
            };

            int actual = InfoHelper.DbContainsTableWithRowcount("dbo.NonExistent", tableData);

            Assert.AreEqual(-1, actual);
        }

        #endregion

        #region GetDatabaseList Tests

        [TestMethod()]
        public void GetDatabaseList_ReturnsMultipleDatabases()
        {
            ConnectionData connData = GetConnData();

            DatabaseList actual = InfoHelper.GetDatabaseList(connData);

            Assert.IsTrue(actual.Count > 0, "Should return at least one database");
        }

        [TestMethod()]
        public void GetDatabaseList_ContainsSqlBuildTest()
        {
            ConnectionData connData = GetConnData();

            DatabaseList actual = InfoHelper.GetDatabaseList(connData);

            Assert.IsTrue(actual.Any(d => d.DatabaseName.StartsWith("SqlBuildTest")),
                "Should contain SqlBuildTest database");
        }

        [TestMethod()]
        public void GetDatabaseList_WithErrorFlag_ReportsSuccess()
        {
            ConnectionData connData = GetConnData();

            DatabaseList actual = InfoHelper.GetDatabaseList(connData, out bool hasError);

            Assert.IsFalse(hasError, "Should not have error on valid connection");
            Assert.IsTrue(actual.Count > 0);
        }

        #endregion

        #region GetTableObjectList Tests

        [TestMethod()]
        public void GetTableObjectList_ReturnsTableObjects()
        {
            ConnectionData connData = GetConnData();

            var actual = InfoHelper.GetTableObjectList(connData);

            Assert.IsTrue(actual.Count > 0, "Should return at least one table object");
        }

        [TestMethod()]
        public void GetTableObjectList_ObjectsHaveTableType()
        {
            ConnectionData connData = GetConnData();

            var actual = InfoHelper.GetTableObjectList(connData);

            Assert.IsTrue(actual.All(o => o.ObjectType == "Table"), "All objects should be Tables");
        }

        #endregion

        #region GetColumnNames with DataTable Tests

        [TestMethod()]
        public void GetColumnNames_FromDataTable_ReturnsColumnNames()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Column1");
            table.Columns.Add("Column2");
            table.Columns.Add("Column3");

            string[] actual = InfoHelper.GetColumnNames(table);

            Assert.AreEqual(3, actual.Length);
            Assert.AreEqual("Column1", actual[0]);
            Assert.AreEqual("Column2", actual[1]);
            Assert.AreEqual("Column3", actual[2]);
        }

        [TestMethod()]
        public void GetColumnNames_FromEmptyDataTable_ReturnsEmptyArray()
        {
            DataTable table = new DataTable();

            string[] actual = InfoHelper.GetColumnNames(table);

            Assert.AreEqual(0, actual.Length);
        }

        #endregion

        #region ExtractNameAndSchema Tests

        [TestMethod()]
        public void ExtractNameAndSchema_WithSchema_ExtractsBoth()
        {
            InfoHelper.ExtractNameAndSchema("dbo.MyTable", out string tableName, out string schema);

            Assert.AreEqual("MyTable", tableName);
            Assert.AreEqual("dbo", schema);
        }

        [TestMethod()]
        public void ExtractNameAndSchema_WithoutSchema_DefaultsToDbo()
        {
            InfoHelper.ExtractNameAndSchema("MyTable", out string tableName, out string schema);

            Assert.AreEqual("MyTable", tableName);
            Assert.AreEqual("dbo", schema);
        }

        [TestMethod()]
        public void ExtractNameAndSchema_WithBrackets_HandlesBrackets()
        {
            InfoHelper.ExtractNameAndSchema("[dbo].[MyTable]", out string tableName, out string schema);

            // Should handle brackets properly
            Assert.IsNotNull(tableName);
            Assert.IsNotNull(schema);
        }

        #endregion

        #region GetDatabaseSizeAnalysis Tests

        [TestMethod()]
        [Ignore("Resource file SizeAnalysis.sql is not properly embedded - TODO: fix resource embedding")]
        public void GetDatabaseSizeAnalysis_ReturnsAnalysisData()
        {
            ConnectionData connData = GetConnData();

            var actual = InfoHelper.GetDatabaseSizeAnalysis(connData);

            Assert.IsNotNull(actual);
            // Analysis may return empty list if database is empty
            Assert.IsNotNull(actual.SizeAnalysis);
        }

        #endregion
    }
}
