using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Unit tests for ObjectData class
    /// </summary>
    [TestClass]
    public class ObjectDataTest
    {
        #region Constructor Tests

        [TestMethod]
        public void ObjectDataConstructor_ShouldSetDefaultValues()
        {
            var target = new ObjectData();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.ObjectName);
            Assert.AreEqual(string.Empty, target.ObjectType);
            Assert.AreEqual(DateTime.MinValue, target.CreateDate);
            Assert.AreEqual(DateTime.MinValue, target.AlteredDate);
            Assert.AreEqual("dbo", target.SchemaOwner);
            Assert.AreEqual(string.Empty, target.ParentObject);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void ObjectName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectData();
            string expected = "StoredProcedure1";

            target.ObjectName = expected;

            Assert.AreEqual(expected, target.ObjectName);
        }

        [TestMethod]
        public void ObjectType_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectData();
            string expected = "P";

            target.ObjectType = expected;

            Assert.AreEqual(expected, target.ObjectType);
        }

        [TestMethod]
        public void CreateDate_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectData();
            var expected = new DateTime(2024, 1, 15, 10, 30, 0);

            target.CreateDate = expected;

            Assert.AreEqual(expected, target.CreateDate);
        }

        [TestMethod]
        public void AlteredDate_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectData();
            var expected = new DateTime(2024, 6, 20, 14, 45, 0);

            target.AlteredDate = expected;

            Assert.AreEqual(expected, target.AlteredDate);
        }

        [TestMethod]
        public void SchemaOwner_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectData();
            string expected = "custom_schema";

            target.SchemaOwner = expected;

            Assert.AreEqual(expected, target.SchemaOwner);
        }

        [TestMethod]
        public void ParentObject_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectData();
            string expected = "ParentTable";

            target.ParentObject = expected;

            Assert.AreEqual(expected, target.ParentObject);
        }

        [TestMethod]
        public void AllProperties_ShouldWorkTogether()
        {
            var createDate = new DateTime(2023, 1, 1);
            var alterDate = new DateTime(2024, 1, 1);

            var target = new ObjectData
            {
                ObjectName = "usp_GetCustomers",
                ObjectType = "P",
                CreateDate = createDate,
                AlteredDate = alterDate,
                SchemaOwner = "sales",
                ParentObject = ""
            };

            Assert.AreEqual("usp_GetCustomers", target.ObjectName);
            Assert.AreEqual("P", target.ObjectType);
            Assert.AreEqual(createDate, target.CreateDate);
            Assert.AreEqual(alterDate, target.AlteredDate);
            Assert.AreEqual("sales", target.SchemaOwner);
        }

        #endregion
    }
}
