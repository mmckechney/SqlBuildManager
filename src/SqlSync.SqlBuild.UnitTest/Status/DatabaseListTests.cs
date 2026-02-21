using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.DbInformation;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest.Status
{
    /// <summary>
    /// Unit tests for DatabaseList class from SqlSync.DbInformation namespace
    /// </summary>
    [TestClass]
    public class DatabaseListTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_CreatesEmptyList()
        {
            // Act
            var list = new DatabaseList();

            // Assert
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        #endregion

        #region Add Overload Tests

        [TestMethod]
        public void Add_WithDatabaseNameAndIsManuallyEntered_AddsItem()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.Add("TestDatabase", true);

            // Assert
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("TestDatabase", list[0].DatabaseName);
            Assert.IsTrue(list[0].IsManuallyEntered);
        }

        [TestMethod]
        public void Add_WithDatabaseNameAndIsManuallyEnteredFalse_AddsItem()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.Add("TestDatabase", false);

            // Assert
            Assert.AreEqual(1, list.Count);
            Assert.IsFalse(list[0].IsManuallyEntered);
        }

        [TestMethod]
        public void Add_MultipleDatabases_AddsAll()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.Add("Database1", true);
            list.Add("Database2", false);
            list.Add("Database3", true);

            // Assert
            Assert.AreEqual(3, list.Count);
        }

        #endregion

        #region Contains Tests

        [TestMethod]
        public void Contains_ExistingDatabase_ReturnsTrue()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("TestDatabase", true);

            // Act
            var result = list.Contains("TestDatabase");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_NonExistingDatabase_ReturnsFalse()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("TestDatabase", true);

            // Act
            var result = list.Contains("OtherDatabase");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Contains_CaseInsensitiveMatch_ReturnsTrue()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("TestDatabase", true);

            // Act
            var resultLower = list.Contains("testdatabase");
            var resultUpper = list.Contains("TESTDATABASE");
            var resultMixed = list.Contains("TeStDaTaBaSe");

            // Assert
            Assert.IsTrue(resultLower);
            Assert.IsTrue(resultUpper);
            Assert.IsTrue(resultMixed);
        }

        [TestMethod]
        public void Contains_EmptyList_ReturnsFalse()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            var result = list.Contains("AnyDatabase");

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region AddManualList Tests

        [TestMethod]
        public void AddManualList_WithValidList_AddsAllAsManuallyEntered()
        {
            // Arrange
            var list = new DatabaseList();
            var databases = new List<string> { "Db1", "Db2", "Db3" };

            // Act
            list.AddManualList(databases);

            // Assert
            Assert.AreEqual(3, list.Count);
            foreach (var item in list)
            {
                Assert.IsTrue(item.IsManuallyEntered);
            }
        }

        [TestMethod]
        public void AddManualList_WithNullList_DoesNotThrow()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.AddManualList(null!);

            // Assert
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void AddManualList_WithEmptyList_DoesNotAddItems()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.AddManualList(new List<string>());

            // Assert
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void AddManualList_PreservesOrder()
        {
            // Arrange
            var list = new DatabaseList();
            var databases = new List<string> { "Alpha", "Beta", "Gamma" };

            // Act
            list.AddManualList(databases);

            // Assert
            Assert.AreEqual("Alpha", list[0].DatabaseName);
            Assert.AreEqual("Beta", list[1].DatabaseName);
            Assert.AreEqual("Gamma", list[2].DatabaseName);
        }

        #endregion

        #region AddExistingList Tests

        [TestMethod]
        public void AddExistingList_WithValidList_AddsAllAsNotManuallyEntered()
        {
            // Arrange
            var list = new DatabaseList();
            var databases = new List<string> { "Db1", "Db2", "Db3" };

            // Act
            list.AddExistingList(databases);

            // Assert
            Assert.AreEqual(3, list.Count);
            foreach (var item in list)
            {
                Assert.IsFalse(item.IsManuallyEntered);
            }
        }

        [TestMethod]
        public void AddExistingList_WithNullList_DoesNotThrow()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.AddExistingList(null!);

            // Assert
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void AddExistingList_WithEmptyList_DoesNotAddItems()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.AddExistingList(new List<string>());

            // Assert
            Assert.AreEqual(0, list.Count);
        }

        #endregion

        #region AddRangeUnique Tests

        [TestMethod]
        public void AddRangeUnique_WithNewItems_AddsAll()
        {
            // Arrange
            var list = new DatabaseList();
            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", IsManuallyEntered = true },
                new DatabaseItem { DatabaseName = "Db2", IsManuallyEntered = false }
            };

            // Act
            list.AddRangeUnique(newItems);

            // Assert
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void AddRangeUnique_WithExistingItem_UpdatesProperties()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", false);

            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", IsManuallyEntered = true, SequenceId = 10 }
            };

            // Act
            list.AddRangeUnique(newItems);

            // Assert
            Assert.AreEqual(1, list.Count);
            Assert.IsTrue(list[0].IsManuallyEntered);
            Assert.AreEqual(10, list[0].SequenceId);
        }

        [TestMethod]
        public void AddRangeUnique_WithNullList_DoesNotThrow()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            list.AddRangeUnique(null!);

            // Assert
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void AddRangeUnique_MixedNewAndExisting_HandlesBoth()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", false);

            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", IsManuallyEntered = true },
                new DatabaseItem { DatabaseName = "Db2", IsManuallyEntered = true }
            };

            // Act
            list.AddRangeUnique(newItems);

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Find("Db1").IsManuallyEntered);
            Assert.IsNotNull(list.Find("Db2"));
        }

        [TestMethod]
        public void AddRangeUnique_UpdatesSequenceId()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", false);

            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", SequenceId = 42 }
            };

            // Act
            list.AddRangeUnique(newItems);

            // Assert
            Assert.AreEqual(42, list[0].SequenceId);
        }

        #endregion

        #region Find Tests

        [TestMethod]
        public void Find_ExistingDatabase_ReturnsItem()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("TestDatabase", true);

            // Act
            var result = list.Find("TestDatabase");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestDatabase", result.DatabaseName);
        }

        [TestMethod]
        public void Find_NonExistingDatabase_ReturnsNull()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("TestDatabase", true);

            // Act
            var result = list.Find("OtherDatabase");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Find_CaseInsensitive_ReturnsItem()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("TestDatabase", true);

            // Act
            var result = list.Find("testdatabase");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestDatabase", result.DatabaseName);
        }

        [TestMethod]
        public void Find_EmptyList_ReturnsNull()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            var result = list.Find("AnyDatabase");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region IsAllManuallyEntered Tests

        [TestMethod]
        public void IsAllManuallyEntered_AllManual_ReturnsTrue()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", true);
            list.Add("Db2", true);
            list.Add("Db3", true);

            // Act
            var result = list.IsAllManuallyEntered();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_SomeNotManual_ReturnsFalse()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", true);
            list.Add("Db2", false);
            list.Add("Db3", true);

            // Act
            var result = list.IsAllManuallyEntered();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_AllNotManual_ReturnsFalse()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", false);
            list.Add("Db2", false);

            // Act
            var result = list.IsAllManuallyEntered();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_EmptyList_ReturnsTrue()
        {
            // Arrange
            var list = new DatabaseList();

            // Act
            var result = list.IsAllManuallyEntered();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_SingleManualItem_ReturnsTrue()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", true);

            // Act
            var result = list.IsAllManuallyEntered();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_SingleNotManualItem_ReturnsFalse()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Db1", false);

            // Act
            var result = list.IsAllManuallyEntered();

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region DatabaseListComparer Tests

        [TestMethod]
        public void DatabaseListComparer_Compare_DifferentNames_ReturnsNegative()
        {
            // Arrange
            var comparer = new DatabaseListComparer();
            var item1 = new DatabaseItem { DatabaseName = "Alpha" };
            var item2 = new DatabaseItem { DatabaseName = "Beta" };

            // Act
            var result = comparer.Compare(item1, item2);

            // Assert
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void DatabaseListComparer_Compare_SameNames_ReturnsZero()
        {
            // Arrange
            var comparer = new DatabaseListComparer();
            var item1 = new DatabaseItem { DatabaseName = "Same" };
            var item2 = new DatabaseItem { DatabaseName = "Same" };

            // Act
            var result = comparer.Compare(item1, item2);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void DatabaseListComparer_Compare_ReverseOrder_ReturnsPositive()
        {
            // Arrange
            var comparer = new DatabaseListComparer();
            var item1 = new DatabaseItem { DatabaseName = "Zebra" };
            var item2 = new DatabaseItem { DatabaseName = "Alpha" };

            // Act
            var result = comparer.Compare(item1, item2);

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void DatabaseList_Sort_WithComparer_SortsAlphabetically()
        {
            // Arrange
            var list = new DatabaseList();
            list.Add("Charlie", true);
            list.Add("Alpha", true);
            list.Add("Beta", true);

            // Act
            list.Sort(new DatabaseListComparer());

            // Assert
            Assert.AreEqual("Alpha", list[0].DatabaseName);
            Assert.AreEqual("Beta", list[1].DatabaseName);
            Assert.AreEqual("Charlie", list[2].DatabaseName);
        }

        #endregion
    }
}
