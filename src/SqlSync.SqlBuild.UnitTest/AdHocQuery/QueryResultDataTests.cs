using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.Connection;
using System;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest.AdHocQuery
{
    [TestClass]
    public class QueryResultDataTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithServerAndDatabase_SetsProperties()
        {
            // Act
            var result = new QueryResultData("TestServer", "TestDatabase");

            // Assert
            Assert.AreEqual("TestServer", result.Server);
            Assert.AreEqual("TestDatabase", result.Database);
        }

        [TestMethod]
        public void Constructor_InitializesCollections()
        {
            // Act
            var result = new QueryResultData("Server", "Db");

            // Assert
            Assert.IsNotNull(result.Results);
            Assert.IsNotNull(result.QueryAppendData);
            Assert.IsNotNull(result.ColumnDefinition);
            Assert.AreEqual(0, result.Results.Count);
            Assert.AreEqual(0, result.QueryAppendData.Count);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void Server_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var result = new QueryResultData("Initial", "Db");

            // Act
            result.Server = "NewServer";

            // Assert
            Assert.AreEqual("NewServer", result.Server);
        }

        [TestMethod]
        public void Database_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var result = new QueryResultData("Server", "Initial");

            // Act
            result.Database = "NewDatabase";

            // Assert
            Assert.AreEqual("NewDatabase", result.Database);
        }

        [TestMethod]
        public void RowCount_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");

            // Act
            result.RowCount = 42;

            // Assert
            Assert.AreEqual(42, result.RowCount);
        }

        [TestMethod]
        public void Results_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            var newResults = new List<Result>
            {
                new Result(),
                new Result()
            };

            // Act
            result.Results = newResults;

            // Assert
            Assert.AreEqual(2, result.Results.Count);
        }

        [TestMethod]
        public void QueryAppendData_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            var appendData = new List<QueryRowItem>
            {
                new QueryRowItem { ColumnName = "Col1", Value = "Val1" }
            };

            // Act
            result.QueryAppendData = appendData;

            // Assert
            Assert.AreEqual(1, result.QueryAppendData.Count);
        }

        [TestMethod]
        public void ColumnDefinition_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            var columnDef = new ColumnDefinition();
            columnDef.Add("Column1", "nvarchar(50)");

            // Act
            result.ColumnDefinition = columnDef;

            // Assert
            Assert.AreEqual(1, result.ColumnDefinition.Count);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsServerDotDatabase()
        {
            // Arrange
            var result = new QueryResultData("MyServer", "MyDatabase");

            // Act
            var str = result.ToString();

            // Assert
            Assert.AreEqual("MyServer.MyDatabase", str);
        }

        [TestMethod]
        public void ToString_WithSpecialCharacters_ReturnsCorrectly()
        {
            // Arrange
            var result = new QueryResultData("Server\\Instance", "Db-Name");

            // Act
            var str = result.ToString();

            // Assert
            Assert.AreEqual("Server\\Instance.Db-Name", str);
        }

        #endregion

        #region GetRowValuesCsvString Tests

        [TestMethod]
        public void GetRowValuesCsvString_EmptyResults_ReturnsMinimalCsv()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            result.RowCount = 0;

            // Act
            var csv = result.GetRowValuesCsvString();

            // Assert
            Assert.IsNotNull(csv);
            Assert.IsTrue(csv.Contains("Server"));
            Assert.IsTrue(csv.Contains("Db"));
        }

        [TestMethod]
        public void GetRowValuesCsvString_WithResults_IncludesServerAndDatabase()
        {
            // Arrange
            var result = new QueryResultData("TestServer", "TestDb");
            var row = new Result();
            row.Add("Column1", "Value1");
            result.Results.Add(row);
            result.RowCount = 1;

            // Act
            var csv = result.GetRowValuesCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("TestServer"));
            Assert.IsTrue(csv.Contains("TestDb"));
        }

        [TestMethod]
        public void GetRowValuesCsvString_WithQueryAppendData_IncludesAppendValues()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            result.QueryAppendData.Add(new QueryRowItem { ColumnName = "AppendCol", Value = "AppendValue" });
            var row = new Result();
            row.Add("Col1", "Val1");
            result.Results.Add(row);
            result.RowCount = 1;

            // Act
            var csv = result.GetRowValuesCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("AppendValue"));
        }

        #endregion

        #region GetColumnsCsvString Tests

        [TestMethod]
        public void GetColumnsCsvString_ReturnsHeadersWithServerAndDatabase()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");

            // Act
            var csv = result.GetColumnsCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("Server"));
            Assert.IsTrue(csv.Contains("Database"));
            Assert.IsTrue(csv.Contains("RowCount"));
            Assert.IsTrue(csv.Contains("Row #"));
        }

        [TestMethod]
        public void GetColumnsCsvString_WithQueryAppendData_IncludesAppendColumnNames()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            result.QueryAppendData.Add(new QueryRowItem { ColumnName = "CustomColumn", Value = "Value" });

            // Act
            var csv = result.GetColumnsCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("CustomColumn"));
        }

        [TestMethod]
        public void GetColumnsCsvString_WithColumnDefinition_IncludesColumns()
        {
            // Arrange
            var result = new QueryResultData("Server", "Db");
            result.ColumnDefinition.Add("FirstName", "nvarchar(100)");
            result.ColumnDefinition.Add("LastName", "nvarchar(100)");

            // Act
            var csv = result.GetColumnsCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("FirstName"));
            Assert.IsTrue(csv.Contains("LastName"));
        }

        #endregion
    }

    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void Constructor_SetsItemNameToRow()
        {
            // Act
            var result = new Result();

            // Assert
            Assert.AreEqual("Row", result.ItemName);
        }

        [TestMethod]
        public void Add_AndRetrieve_WorksCorrectly()
        {
            // Arrange
            var result = new Result();

            // Act
            result.Add("Column1", "Value1");
            result.Add("Column2", "Value2");

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Value1", result["Column1"]);
            Assert.AreEqual("Value2", result["Column2"]);
        }

        [TestMethod]
        public void Copy_CreatesIndependentCopy()
        {
            // Arrange
            var original = new Result();
            original.Add("Key1", "Value1");
            original.Add("Key2", "Value2");

            // Act
            var copy = original.Copy();
            copy["Key1"] = "ModifiedValue";

            // Assert
            Assert.AreEqual("Value1", original["Key1"]);
            Assert.AreEqual("ModifiedValue", copy["Key1"]);
        }

        [TestMethod]
        public void Copy_PreservesAllValues()
        {
            // Arrange
            var original = new Result();
            original.Add("A", "1");
            original.Add("B", "2");
            original.Add("C", "3");

            // Act
            var copy = original.Copy();

            // Assert
            Assert.AreEqual(3, copy.Count);
            Assert.AreEqual("1", copy["A"]);
            Assert.AreEqual("2", copy["B"]);
            Assert.AreEqual("3", copy["C"]);
        }

        [TestMethod]
        public void GetCsvString_ReturnsQuotedValues()
        {
            // Arrange
            var result = new Result();
            result.Add("Col1", "Value1");
            result.Add("Col2", "Value2");

            // Act
            var csv = result.GetCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("\"Value1\""));
            Assert.IsTrue(csv.Contains("\"Value2\""));
        }

        [TestMethod]
        public void GetCsvString_EmptyResult_ReturnsEmptyString()
        {
            // Arrange
            var result = new Result();

            // Act
            var csv = result.GetCsvString();

            // Assert
            Assert.AreEqual(string.Empty, csv);
        }
    }

    [TestClass]
    public class ColumnDefinitionTests
    {
        [TestMethod]
        public void Constructor_SetsItemNameToDefinition()
        {
            // Act
            var columnDef = new ColumnDefinition();

            // Assert
            Assert.AreEqual("Definition", columnDef.ItemName);
        }

        [TestMethod]
        public void Add_AndRetrieve_WorksCorrectly()
        {
            // Arrange
            var columnDef = new ColumnDefinition();

            // Act
            columnDef.Add("Id", "int");
            columnDef.Add("Name", "nvarchar(100)");

            // Assert
            Assert.AreEqual(2, columnDef.Count);
            Assert.AreEqual("int", columnDef["Id"]);
            Assert.AreEqual("nvarchar(100)", columnDef["Name"]);
        }

        [TestMethod]
        public void GetCsvString_ReturnsQuotedColumnNames()
        {
            // Arrange
            var columnDef = new ColumnDefinition();
            columnDef.Add("FirstColumn", "type1");
            columnDef.Add("SecondColumn", "type2");

            // Act
            var csv = columnDef.GetCsvString();

            // Assert
            Assert.IsTrue(csv.Contains("\"FirstColumn\""));
            Assert.IsTrue(csv.Contains("\"SecondColumn\""));
        }

        [TestMethod]
        public void GetCsvString_EmptyDefinition_ReturnsEmptyString()
        {
            // Arrange
            var columnDef = new ColumnDefinition();

            // Act
            var csv = columnDef.GetCsvString();

            // Assert
            Assert.AreEqual(string.Empty, csv);
        }
    }

    [TestClass]
    public class ResultsDictionaryTests
    {
        [TestMethod]
        public void Constructor_Default_SetsItemNameToItem()
        {
            // Act
            var dict = new ResultsDictionary();

            // Assert
            Assert.AreEqual("item", dict.ItemName);
        }

        [TestMethod]
        public void Constructor_WithItemName_SetsItemName()
        {
            // Act
            var dict = new ResultsDictionary("CustomItem");

            // Assert
            Assert.AreEqual("CustomItem", dict.ItemName);
        }

        [TestMethod]
        public void Add_AndRetrieve_WorksCorrectly()
        {
            // Arrange
            var dict = new ResultsDictionary();

            // Act
            dict.Add("key1", "value1");
            dict.Add("key2", "value2");

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("value1", dict["key1"]);
            Assert.AreEqual("value2", dict["key2"]);
        }

        [TestMethod]
        public void GetSchema_ReturnsNull()
        {
            // Arrange
            var dict = new ResultsDictionary();

            // Act
            var schema = dict.GetSchema();

            // Assert
            Assert.IsNull(schema);
        }

        [TestMethod]
        public void ItemName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var dict = new ResultsDictionary();

            // Act
            dict.ItemName = "NewItemName";

            // Assert
            Assert.AreEqual("NewItemName", dict.ItemName);
        }
    }
}
