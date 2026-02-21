using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest.MultiDb
{
    [TestClass]
    public class MultiDbDataSorterTests
    {
        private SqlSync.SqlBuild.MultiDb.MultiDbDataSorter _sorter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _sorter = new SqlSync.SqlBuild.MultiDb.MultiDbDataSorter();
        }

        [TestMethod]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var sorter = new SqlSync.SqlBuild.MultiDb.MultiDbDataSorter();

            // Assert
            Assert.IsNotNull(sorter);
        }

        [TestMethod]
        public void Compare_BothNumericKeys_XLessThanY_ReturnsNegative()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("1", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("5", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void Compare_BothNumericKeys_XGreaterThanY_ReturnsPositive()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("10", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("5", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void Compare_BothNumericKeys_Equal_ReturnsZero()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("7", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("7", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Compare_BothStringKeys_AlphabeticalOrder()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("Apple", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("Banana", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void Compare_BothStringKeys_ReverseAlphabeticalOrder()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("Zebra", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("Apple", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void Compare_BothStringKeys_Equal_ReturnsZero()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("Same", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("Same", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Compare_MixedKeys_StringAndNumber_StringComesFirst()
        {
            // Arrange - when one is string and one is number, string has length > 0, number doesn't
            var x = new KeyValuePair<string, List<DatabaseOverride>>("Text", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("123", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert - string key comes first (returns -1)
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void Compare_MixedKeys_NumberAndString_NumberComesAfter()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("456", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("Text", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert - number key comes after (returns 1)
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void Compare_DecimalNumericKeys_SortsCorrectly()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("1.5", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("2.5", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void Compare_NegativeNumericKeys_SortsCorrectly()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("-10", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("5", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        public void Compare_LargeNumericKeys_SortsCorrectly()
        {
            // Arrange
            var x = new KeyValuePair<string, List<DatabaseOverride>>("1000000", new List<DatabaseOverride>());
            var y = new KeyValuePair<string, List<DatabaseOverride>>("999999", new List<DatabaseOverride>());

            // Act
            int result = _sorter.Compare(x, y);

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void SortList_UsingComparer_SortsCorrectly()
        {
            // Arrange
            var list = new List<KeyValuePair<string, List<DatabaseOverride>>>
            {
                new KeyValuePair<string, List<DatabaseOverride>>("5", new List<DatabaseOverride>()),
                new KeyValuePair<string, List<DatabaseOverride>>("1", new List<DatabaseOverride>()),
                new KeyValuePair<string, List<DatabaseOverride>>("10", new List<DatabaseOverride>()),
                new KeyValuePair<string, List<DatabaseOverride>>("2", new List<DatabaseOverride>())
            };

            // Act
            list.Sort(_sorter);

            // Assert
            Assert.AreEqual("1", list[0].Key);
            Assert.AreEqual("2", list[1].Key);
            Assert.AreEqual("5", list[2].Key);
            Assert.AreEqual("10", list[3].Key);
        }

        [TestMethod]
        public void SortList_MixedStringAndNumeric_SortsCorrectly()
        {
            // Arrange
            var list = new List<KeyValuePair<string, List<DatabaseOverride>>>
            {
                new KeyValuePair<string, List<DatabaseOverride>>("5", new List<DatabaseOverride>()),
                new KeyValuePair<string, List<DatabaseOverride>>("Alpha", new List<DatabaseOverride>()),
                new KeyValuePair<string, List<DatabaseOverride>>("10", new List<DatabaseOverride>()),
                new KeyValuePair<string, List<DatabaseOverride>>("Beta", new List<DatabaseOverride>())
            };

            // Act
            list.Sort(_sorter);

            // Assert - Strings come first alphabetically, then numbers
            Assert.AreEqual("Alpha", list[0].Key);
            Assert.AreEqual("Beta", list[1].Key);
        }
    }
}
