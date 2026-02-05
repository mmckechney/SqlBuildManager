using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.DbInformation.UnitTest
{
    /// <summary>
    /// Unit tests for CodeTableAudit class
    /// </summary>
    [TestClass]
    public class CodeTableAuditTest
    {
        #region Constructor Tests

        [TestMethod]
        public void CodeTableAuditConstructor_ShouldSetDefaultValues()
        {
            var target = new CodeTableAudit();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.TableName);
            Assert.AreEqual(string.Empty, target.UpdateIdColumn);
            Assert.AreEqual(string.Empty, target.UpdateDateColumn);
            Assert.AreEqual(string.Empty, target.CreateDateColumn);
            Assert.AreEqual(string.Empty, target.CreateIdColumn);
            Assert.AreEqual(-1, target.RowCount);
            Assert.IsFalse(target.HasUpdateTrigger);
            Assert.IsNull(target.LookUpTableRow);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void TableName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            string expected = "TestTable";

            target.TableName = expected;

            Assert.AreEqual(expected, target.TableName);
        }

        [TestMethod]
        public void UpdateIdColumn_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            string expected = "UpdatedBy";

            target.UpdateIdColumn = expected;

            Assert.AreEqual(expected, target.UpdateIdColumn);
        }

        [TestMethod]
        public void UpdateDateColumn_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            string expected = "UpdatedDate";

            target.UpdateDateColumn = expected;

            Assert.AreEqual(expected, target.UpdateDateColumn);
        }

        [TestMethod]
        public void CreateDateColumn_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            string expected = "CreatedDate";

            target.CreateDateColumn = expected;

            Assert.AreEqual(expected, target.CreateDateColumn);
        }

        [TestMethod]
        public void CreateIdColumn_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            string expected = "CreatedBy";

            target.CreateIdColumn = expected;

            Assert.AreEqual(expected, target.CreateIdColumn);
        }

        [TestMethod]
        public void RowCount_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            int expected = 1000;

            target.RowCount = expected;

            Assert.AreEqual(expected, target.RowCount);
        }

        [TestMethod]
        public void HasUpdateTrigger_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();

            target.HasUpdateTrigger = true;

            Assert.IsTrue(target.HasUpdateTrigger);
        }

        [TestMethod]
        public void LookUpTableRow_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new CodeTableAudit();
            object expected = new { Id = 1, Name = "Test" };

            target.LookUpTableRow = expected;

            Assert.AreEqual(expected, target.LookUpTableRow);
        }

        [TestMethod]
        public void AllAuditColumns_ShouldWorkTogether()
        {
            var target = new CodeTableAudit
            {
                TableName = "AuditTable",
                UpdateIdColumn = "ModifiedBy",
                UpdateDateColumn = "ModifiedDate",
                CreateDateColumn = "CreatedDate",
                CreateIdColumn = "CreatedBy",
                RowCount = 500,
                HasUpdateTrigger = true
            };

            Assert.AreEqual("AuditTable", target.TableName);
            Assert.AreEqual("ModifiedBy", target.UpdateIdColumn);
            Assert.AreEqual("ModifiedDate", target.UpdateDateColumn);
            Assert.AreEqual("CreatedDate", target.CreateDateColumn);
            Assert.AreEqual("CreatedBy", target.CreateIdColumn);
            Assert.AreEqual(500, target.RowCount);
            Assert.IsTrue(target.HasUpdateTrigger);
        }

        #endregion
    }
}
