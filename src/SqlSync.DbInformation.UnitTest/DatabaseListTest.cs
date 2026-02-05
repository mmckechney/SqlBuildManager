using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Unit tests for DatabaseList class
    /// </summary>
    [TestClass]
    public class DatabaseListTest
    {
        #region Constructor Tests

        [TestMethod]
        public void DatabaseListConstructor_ShouldCreateEmptyList()
        {
            var target = new DatabaseList();

            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.Count);
        }

        #endregion

        #region Add Method Tests

        [TestMethod]
        public void Add_WithDatabaseNameAndIsManuallyEntered_ShouldAddItem()
        {
            var target = new DatabaseList();

            target.Add("TestDb", true);

            Assert.AreEqual(1, target.Count);
            Assert.AreEqual("TestDb", target[0].DatabaseName);
            Assert.IsTrue(target[0].IsManuallyEntered);
        }

        [TestMethod]
        public void Add_WithDatabaseNameNotManuallyEntered_ShouldAddItem()
        {
            var target = new DatabaseList();

            target.Add("TestDb", false);

            Assert.AreEqual(1, target.Count);
            Assert.IsFalse(target[0].IsManuallyEntered);
        }

        [TestMethod]
        public void Add_MultipleDatabases_ShouldAddAll()
        {
            var target = new DatabaseList();

            target.Add("Db1", true);
            target.Add("Db2", false);
            target.Add("Db3", true);

            Assert.AreEqual(3, target.Count);
        }

        #endregion

        #region Contains Method Tests

        [TestMethod]
        public void Contains_ExistingDatabase_ShouldReturnTrue()
        {
            var target = new DatabaseList();
            target.Add("TestDb", true);

            bool result = target.Contains("TestDb");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_NonExistingDatabase_ShouldReturnFalse()
        {
            var target = new DatabaseList();
            target.Add("TestDb", true);

            bool result = target.Contains("NonExistent");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Contains_CaseInsensitive_ShouldReturnTrue()
        {
            var target = new DatabaseList();
            target.Add("TestDb", true);

            bool result = target.Contains("TESTDB");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_EmptyList_ShouldReturnFalse()
        {
            var target = new DatabaseList();

            bool result = target.Contains("TestDb");

            Assert.IsFalse(result);
        }

        #endregion

        #region AddManualList Method Tests

        [TestMethod]
        public void AddManualList_WithValidList_ShouldAddAllAsManuallyEntered()
        {
            var target = new DatabaseList();
            var databases = new List<string> { "Db1", "Db2", "Db3" };

            target.AddManualList(databases);

            Assert.AreEqual(3, target.Count);
            foreach (var item in target)
            {
                Assert.IsTrue(item.IsManuallyEntered);
            }
        }

        [TestMethod]
        public void AddManualList_WithNullList_ShouldNotThrow()
        {
            var target = new DatabaseList();

            target.AddManualList(null);

            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void AddManualList_WithEmptyList_ShouldNotAddItems()
        {
            var target = new DatabaseList();

            target.AddManualList(new List<string>());

            Assert.AreEqual(0, target.Count);
        }

        #endregion

        #region AddExistingList Method Tests

        [TestMethod]
        public void AddExistingList_WithValidList_ShouldAddAllAsNotManuallyEntered()
        {
            var target = new DatabaseList();
            var databases = new List<string> { "Db1", "Db2", "Db3" };

            target.AddExistingList(databases);

            Assert.AreEqual(3, target.Count);
            foreach (var item in target)
            {
                Assert.IsFalse(item.IsManuallyEntered);
            }
        }

        [TestMethod]
        public void AddExistingList_WithNullList_ShouldNotThrow()
        {
            var target = new DatabaseList();

            target.AddExistingList(null);

            Assert.AreEqual(0, target.Count);
        }

        #endregion

        #region AddRangeUnique Method Tests

        [TestMethod]
        public void AddRangeUnique_WithNewItems_ShouldAddAll()
        {
            var target = new DatabaseList();
            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", IsManuallyEntered = true },
                new DatabaseItem { DatabaseName = "Db2", IsManuallyEntered = false }
            };

            target.AddRangeUnique(newItems);

            Assert.AreEqual(2, target.Count);
        }

        [TestMethod]
        public void AddRangeUnique_WithExistingItem_ShouldUpdateProperties()
        {
            var target = new DatabaseList();
            target.Add("Db1", false);
            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", IsManuallyEntered = true, SequenceId = 5 }
            };

            target.AddRangeUnique(newItems);

            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target[0].IsManuallyEntered);
            Assert.AreEqual(5, target[0].SequenceId);
        }

        [TestMethod]
        public void AddRangeUnique_WithNullList_ShouldNotThrow()
        {
            var target = new DatabaseList();

            target.AddRangeUnique(null);

            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void AddRangeUnique_MixedNewAndExisting_ShouldHandleBoth()
        {
            var target = new DatabaseList();
            target.Add("Db1", false);
            var newItems = new List<DatabaseItem>
            {
                new DatabaseItem { DatabaseName = "Db1", IsManuallyEntered = true },
                new DatabaseItem { DatabaseName = "Db2", IsManuallyEntered = true }
            };

            target.AddRangeUnique(newItems);

            Assert.AreEqual(2, target.Count);
            Assert.IsTrue(target.Find("Db1").IsManuallyEntered);
            Assert.IsNotNull(target.Find("Db2"));
        }

        #endregion

        #region Find Method Tests

        [TestMethod]
        public void Find_ExistingDatabase_ShouldReturnItem()
        {
            var target = new DatabaseList();
            target.Add("TestDb", true);

            DatabaseItem result = target.Find("TestDb");

            Assert.IsNotNull(result);
            Assert.AreEqual("TestDb", result.DatabaseName);
        }

        [TestMethod]
        public void Find_NonExistingDatabase_ShouldReturnNull()
        {
            var target = new DatabaseList();
            target.Add("TestDb", true);

            DatabaseItem result = target.Find("NonExistent");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Find_CaseInsensitive_ShouldReturnItem()
        {
            var target = new DatabaseList();
            target.Add("TestDb", true);

            DatabaseItem result = target.Find("testdb");

            Assert.IsNotNull(result);
        }

        #endregion

        #region IsAllManuallyEntered Method Tests

        [TestMethod]
        public void IsAllManuallyEntered_AllManual_ShouldReturnTrue()
        {
            var target = new DatabaseList();
            target.Add("Db1", true);
            target.Add("Db2", true);

            bool result = target.IsAllManuallyEntered();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_MixedEntries_ShouldReturnFalse()
        {
            var target = new DatabaseList();
            target.Add("Db1", true);
            target.Add("Db2", false);

            bool result = target.IsAllManuallyEntered();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_AllNotManual_ShouldReturnFalse()
        {
            var target = new DatabaseList();
            target.Add("Db1", false);
            target.Add("Db2", false);

            bool result = target.IsAllManuallyEntered();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsAllManuallyEntered_EmptyList_ShouldReturnTrue()
        {
            var target = new DatabaseList();

            bool result = target.IsAllManuallyEntered();

            Assert.IsTrue(result);
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for DatabaseListComparer class
    /// </summary>
    [TestClass]
    public class DatabaseListComparerTest
    {
        [TestMethod]
        public void Compare_DifferentNames_ShouldSortAlphabetically()
        {
            var comparer = new DatabaseListComparer();
            var item1 = new DatabaseItem { DatabaseName = "Apple" };
            var item2 = new DatabaseItem { DatabaseName = "Banana" };

            int result = comparer.Compare(item1, item2);

            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void Compare_SameNames_ShouldReturnZero()
        {
            var comparer = new DatabaseListComparer();
            var item1 = new DatabaseItem { DatabaseName = "Same" };
            var item2 = new DatabaseItem { DatabaseName = "Same" };

            int result = comparer.Compare(item1, item2);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Compare_ReverseOrder_ShouldReturnPositive()
        {
            var comparer = new DatabaseListComparer();
            var item1 = new DatabaseItem { DatabaseName = "Zebra" };
            var item2 = new DatabaseItem { DatabaseName = "Apple" };

            int result = comparer.Compare(item1, item2);

            Assert.IsTrue(result > 0);
        }
    }
}
