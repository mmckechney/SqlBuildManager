using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Unit tests for DatabaseItem class
    /// </summary>
    [TestClass]
    public class DatabaseItemTest
    {
        #region Constructor Tests

        [TestMethod]
        public void DatabaseItemConstructor_ShouldSetDefaultValues()
        {
            var target = new DatabaseItem();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.DatabaseName);
            Assert.IsFalse(target.IsManuallyEntered);
            Assert.IsNull(target.SequenceId);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void DatabaseName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new DatabaseItem();
            string expected = "TestDatabase";

            target.DatabaseName = expected;

            Assert.AreEqual(expected, target.DatabaseName);
        }

        [TestMethod]
        public void IsManuallyEntered_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new DatabaseItem();

            target.IsManuallyEntered = true;

            Assert.IsTrue(target.IsManuallyEntered);
        }

        [TestMethod]
        public void SequenceId_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new DatabaseItem();
            int expected = 42;

            target.SequenceId = expected;

            Assert.AreEqual(expected, target.SequenceId);
        }

        [TestMethod]
        public void SequenceId_SetToNull_ShouldReturnNull()
        {
            var target = new DatabaseItem { SequenceId = 10 };

            target.SequenceId = null;

            Assert.IsNull(target.SequenceId);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ShouldReturnDatabaseName()
        {
            var target = new DatabaseItem { DatabaseName = "MyDatabase" };

            string result = target.ToString();

            Assert.AreEqual("MyDatabase", result);
        }

        [TestMethod]
        public void ToString_WithEmptyDatabaseName_ShouldReturnEmptyString()
        {
            var target = new DatabaseItem();

            string result = target.ToString();

            Assert.AreEqual(string.Empty, result);
        }

        #endregion
    }
}
