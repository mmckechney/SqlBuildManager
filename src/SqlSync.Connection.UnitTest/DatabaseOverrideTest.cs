using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;

namespace SqlSync.Connection.UnitTest
{
    /// <summary>
    /// Unit tests for DatabaseOverride class
    /// </summary>
    [TestClass]
    public class DatabaseOverrideTest
    {
        #region Constructor Tests

        [TestMethod]
        public void DatabaseOverrideConstructor_Default_ShouldSetEmptyValues()
        {
            var target = new DatabaseOverride();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.ConcurrencyTag);
            Assert.AreEqual(string.Empty, target.DefaultDbTarget);
            Assert.IsNull(target.Server);
        }

        [TestMethod]
        public void DatabaseOverrideConstructor_WithParameters_ShouldSetProperties()
        {
            string server = "server1";
            string defaultDb = "defaultDb";
            string overrideDb = "overrideDb";

            var target = new DatabaseOverride(server, defaultDb, overrideDb);

            Assert.AreEqual(server, target.Server);
            Assert.AreEqual(defaultDb, target.DefaultDbTarget);
            Assert.AreEqual(overrideDb, target.OverrideDbTarget);
            Assert.AreEqual(string.Empty, target.ConcurrencyTag);
        }

        [TestMethod]
        public void DatabaseOverrideConstructor_WithTag_ShouldSetConcurrencyTag()
        {
            string server = "server1";
            string defaultDb = "defaultDb";
            string overrideDb = "overrideDb";
            string tag = "tag123";

            var target = new DatabaseOverride(server, defaultDb, overrideDb, tag);

            Assert.AreEqual(tag, target.ConcurrencyTag);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void OverrideDbTarget_WhenEmpty_ShouldReturnDefaultDbTarget()
        {
            var target = new DatabaseOverride
            {
                DefaultDbTarget = "defaultDb"
            };

            Assert.AreEqual("defaultDb", target.OverrideDbTarget);
        }

        [TestMethod]
        public void OverrideDbTarget_WhenWhitespace_ShouldReturnDefaultDbTarget()
        {
            var target = new DatabaseOverride
            {
                DefaultDbTarget = "defaultDb",
                OverrideDbTarget = "   "
            };

            Assert.AreEqual("defaultDb", target.OverrideDbTarget);
        }

        [TestMethod]
        public void OverrideDbTarget_WhenSet_ShouldReturnOverrideValue()
        {
            var target = new DatabaseOverride
            {
                DefaultDbTarget = "defaultDb",
                OverrideDbTarget = "overrideDb"
            };

            Assert.AreEqual("overrideDb", target.OverrideDbTarget);
        }

        [TestMethod]
        public void QueryRowData_DefaultValue_ShouldBeEmptyList()
        {
            var target = new DatabaseOverride();

            Assert.IsNotNull(target.QueryRowData);
            Assert.AreEqual(0, target.QueryRowData.Count);
        }

        [TestMethod]
        public void QueryRowData_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new DatabaseOverride();
            var expected = new List<QueryRowItem>
            {
                new QueryRowItem("Col1", "Value1"),
                new QueryRowItem("Col2", "Value2")
            };

            target.QueryRowData = expected;

            Assert.AreEqual(2, target.QueryRowData.Count);
            Assert.AreEqual("Col1", target.QueryRowData[0].ColumnName);
            Assert.AreEqual("Value1", target.QueryRowData[0].Value);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_WithoutConcurrencyTag_ShouldReturnCorrectFormat()
        {
            var target = new DatabaseOverride("server", "defaultDb", "overrideDb");

            string result = target.ToString();

            Assert.AreEqual("defaultDb;overrideDb", result);
        }

        [TestMethod]
        public void ToString_WithConcurrencyTag_ShouldIncludeTag()
        {
            var target = new DatabaseOverride("server", "defaultDb", "overrideDb", "tag123");

            string result = target.ToString();

            Assert.AreEqual("defaultDb;overrideDb#tag123", result);
        }

        [TestMethod]
        public void ToString_WithEmptyOverride_ShouldReturnDefaultOnly()
        {
            var target = new DatabaseOverride("server", "defaultDb", "");

            string result = target.ToString();

            Assert.AreEqual("defaultDb;", result);
        }

        #endregion

        #region AppendedQueryRowData Tests

        [TestMethod]
        public void AppendedQueryRowData_ShouldAddItemsFromStartIndex()
        {
            var target = new DatabaseOverride();
            var dataTable = new DataTable();
            dataTable.Columns.Add("Server", typeof(string));
            dataTable.Columns.Add("Database", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));
            dataTable.Columns.Add("Count", typeof(string));

            object[] dataArray = { "Server1", "DB1", "Active", "100" };

            target.AppendedQueryRowData(dataArray, 2, dataTable.Columns);

            Assert.AreEqual(2, target.QueryRowData.Count);
            Assert.AreEqual("Status", target.QueryRowData[0].ColumnName);
            Assert.AreEqual("Active", target.QueryRowData[0].Value);
            Assert.AreEqual("Count", target.QueryRowData[1].ColumnName);
            Assert.AreEqual("100", target.QueryRowData[1].Value);
        }

        [TestMethod]
        public void AppendedQueryRowData_WithStartIndexAtEnd_ShouldAddNoItems()
        {
            var target = new DatabaseOverride();
            var dataTable = new DataTable();
            dataTable.Columns.Add("Col1", typeof(string));
            dataTable.Columns.Add("Col2", typeof(string));

            object[] dataArray = { "Value1", "Value2" };

            target.AppendedQueryRowData(dataArray, 2, dataTable.Columns);

            Assert.AreEqual(0, target.QueryRowData.Count);
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for QueryRowItem struct
    /// </summary>
    [TestClass]
    public class QueryRowItemTest
    {
        [TestMethod]
        public void QueryRowItemConstructor_ShouldTrimEndValues()
        {
            // The constructor only calls TrimEnd(), not Trim()
            var target = new QueryRowItem("ColumnName  ", "Value  ");

            Assert.AreEqual("ColumnName", target.ColumnName);
            Assert.AreEqual("Value", target.Value);
        }

        [TestMethod]
        public void QueryRowItem_Properties_SetAndGet()
        {
            var target = new QueryRowItem();

            target.ColumnName = "TestColumn";
            target.Value = "TestValue";

            Assert.AreEqual("TestColumn", target.ColumnName);
            Assert.AreEqual("TestValue", target.Value);
        }
    }

    /// <summary>
    /// Unit tests for DatabaseOverrideSorter class
    /// </summary>
    [TestClass]
    public class DatabaseOverrideSorterTest
    {
        [TestMethod]
        public void Compare_ShouldSortAlphabetically()
        {
            var sorter = new DatabaseOverrideSorter();
            var override1 = new DatabaseOverride("server", "default", "Apple");
            var override2 = new DatabaseOverride("server", "default", "Banana");

            int result = sorter.Compare(override1, override2);

            Assert.IsTrue(result < 0, "Apple should come before Banana");
        }

        [TestMethod]
        public void Compare_EqualValues_ShouldReturnZero()
        {
            var sorter = new DatabaseOverrideSorter();
            var override1 = new DatabaseOverride("server", "default", "Same");
            var override2 = new DatabaseOverride("server", "default", "Same");

            int result = sorter.Compare(override1, override2);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Compare_ReverseOrder_ShouldReturnPositive()
        {
            var sorter = new DatabaseOverrideSorter();
            var override1 = new DatabaseOverride("server", "default", "Zebra");
            var override2 = new DatabaseOverride("server", "default", "Apple");

            int result = sorter.Compare(override1, override2);

            Assert.IsTrue(result > 0, "Zebra should come after Apple");
        }
    }
}
