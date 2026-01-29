using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.ObjectScript.UnitTest
{
    /// <summary>
    /// Unit tests for ObjectSyncData class
    /// </summary>
    [TestClass]
    public class ObjectSyncDataTest
    {
        #region Constructor Tests

        [TestMethod]
        public void ObjectSyncDataConstructor_ShouldSetDefaultValues()
        {
            var target = new ObjectSyncData();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.ObjectName);
            Assert.AreEqual(string.Empty, target.ObjectType);
            Assert.AreEqual(string.Empty, target.FullPath);
            Assert.IsFalse(target.IsInDatabase);
            Assert.IsFalse(target.IsInFileSystem);
            Assert.AreEqual(string.Empty, target.FileName);
            Assert.AreEqual(string.Empty, target.SchemaOwner);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void ObjectName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();
            string expected = "StoredProcedure1";

            target.ObjectName = expected;

            Assert.AreEqual(expected, target.ObjectName);
        }

        [TestMethod]
        public void ObjectType_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();
            string expected = "P";

            target.ObjectType = expected;

            Assert.AreEqual(expected, target.ObjectType);
        }

        [TestMethod]
        public void FullPath_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();
            string expected = @"C:\Scripts\StoredProcedure1.sql";

            target.FullPath = expected;

            Assert.AreEqual(expected, target.FullPath);
        }

        [TestMethod]
        public void IsInDatabase_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();

            target.IsInDatabase = true;

            Assert.IsTrue(target.IsInDatabase);
        }

        [TestMethod]
        public void IsInFileSystem_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();

            target.IsInFileSystem = true;

            Assert.IsTrue(target.IsInFileSystem);
        }

        [TestMethod]
        public void FileName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();
            string expected = "StoredProcedure1.sql";

            target.FileName = expected;

            Assert.AreEqual(expected, target.FileName);
        }

        [TestMethod]
        public void SchemaOwner_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ObjectSyncData();
            string expected = "dbo";

            target.SchemaOwner = expected;

            Assert.AreEqual(expected, target.SchemaOwner);
        }

        [TestMethod]
        public void AllProperties_ShouldWorkTogether()
        {
            var target = new ObjectSyncData
            {
                ObjectName = "usp_GetCustomers",
                ObjectType = "P",
                FullPath = @"C:\Scripts\usp_GetCustomers.sql",
                IsInDatabase = true,
                IsInFileSystem = true,
                FileName = "usp_GetCustomers.sql",
                SchemaOwner = "sales"
            };

            Assert.AreEqual("usp_GetCustomers", target.ObjectName);
            Assert.AreEqual("P", target.ObjectType);
            Assert.AreEqual(@"C:\Scripts\usp_GetCustomers.sql", target.FullPath);
            Assert.IsTrue(target.IsInDatabase);
            Assert.IsTrue(target.IsInFileSystem);
            Assert.AreEqual("usp_GetCustomers.sql", target.FileName);
            Assert.AreEqual("sales", target.SchemaOwner);
        }

        #endregion

        #region Sync State Tests

        [TestMethod]
        public void SyncState_InDatabaseNotInFileSystem_RepresentsNewObject()
        {
            var target = new ObjectSyncData
            {
                ObjectName = "NewProcedure",
                IsInDatabase = true,
                IsInFileSystem = false
            };

            Assert.IsTrue(target.IsInDatabase);
            Assert.IsFalse(target.IsInFileSystem);
        }

        [TestMethod]
        public void SyncState_InFileSystemNotInDatabase_RepresentsDeletedObject()
        {
            var target = new ObjectSyncData
            {
                ObjectName = "DeletedProcedure",
                IsInDatabase = false,
                IsInFileSystem = true
            };

            Assert.IsFalse(target.IsInDatabase);
            Assert.IsTrue(target.IsInFileSystem);
        }

        [TestMethod]
        public void SyncState_InBothLocations_RepresentsSyncedObject()
        {
            var target = new ObjectSyncData
            {
                ObjectName = "SyncedProcedure",
                IsInDatabase = true,
                IsInFileSystem = true
            };

            Assert.IsTrue(target.IsInDatabase);
            Assert.IsTrue(target.IsInFileSystem);
        }

        #endregion
    }
}
