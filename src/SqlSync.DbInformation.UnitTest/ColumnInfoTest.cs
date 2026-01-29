using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Unit tests for ColumnInfo class
    /// </summary>
    [TestClass]
    public class ColumnInfoTest
    {
        #region Constructor Tests

        [TestMethod]
        public void ColumnInfoConstructor_ShouldSetDefaultValues()
        {
            var target = new ColumnInfo();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.ColumnName);
            Assert.AreEqual(string.Empty, target.DataType);
            Assert.AreEqual(0, target.CharMaximum);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void ColumnName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ColumnInfo();
            string expected = "TestColumn";

            target.ColumnName = expected;

            Assert.AreEqual(expected, target.ColumnName);
        }

        [TestMethod]
        public void DataType_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ColumnInfo();
            string expected = "nvarchar";

            target.DataType = expected;

            Assert.AreEqual(expected, target.DataType);
        }

        [TestMethod]
        public void CharMaximum_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ColumnInfo();
            int expected = 255;

            target.CharMaximum = expected;

            Assert.AreEqual(expected, target.CharMaximum);
        }

        [TestMethod]
        public void AllProperties_ShouldWorkTogether()
        {
            var target = new ColumnInfo
            {
                ColumnName = "FirstName",
                DataType = "varchar",
                CharMaximum = 100
            };

            Assert.AreEqual("FirstName", target.ColumnName);
            Assert.AreEqual("varchar", target.DataType);
            Assert.AreEqual(100, target.CharMaximum);
        }

        #endregion
    }
}
