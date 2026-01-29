using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Unit tests for TableSize class
    /// </summary>
    [TestClass]
    public class TableSizeTest
    {
        #region Constructor Tests

        [TestMethod]
        public void TableSizeConstructor_ShouldSetDefaultValues()
        {
            var target = new TableSize();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.TableName);
            Assert.AreEqual(0, target.RowCount);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void TableName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new TableSize();
            string expected = "TestTable";

            target.TableName = expected;

            Assert.AreEqual(expected, target.TableName);
        }

        [TestMethod]
        public void RowCount_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new TableSize();
            int expected = 10000;

            target.RowCount = expected;

            Assert.AreEqual(expected, target.RowCount);
        }

        [TestMethod]
        public void AllProperties_ShouldWorkTogether()
        {
            var target = new TableSize
            {
                TableName = "LargeTable",
                RowCount = 1000000
            };

            Assert.AreEqual("LargeTable", target.TableName);
            Assert.AreEqual(1000000, target.RowCount);
        }

        [TestMethod]
        public void RowCount_CanBeZero_ShouldWork()
        {
            var target = new TableSize
            {
                TableName = "EmptyTable",
                RowCount = 0
            };

            Assert.AreEqual(0, target.RowCount);
        }

        #endregion
    }
}
