using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.DbInformation;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest.CodeTable
{
    /// <summary>
    /// Unit tests for CodeTableAuditColumnList class
    /// </summary>
    [TestClass]
    public class CodeTableAuditColumnListTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var list = new CodeTableAuditColumnList();

            // Assert
            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void Constructor_Default_InitializesEmptyLists()
        {
            // Act
            var list = new CodeTableAuditColumnList();

            // Assert
            Assert.IsNotNull(list.UpdateDateColumns);
            Assert.IsNotNull(list.UpdateIdColumns);
            Assert.IsNotNull(list.CreateDateColumns);
            Assert.IsNotNull(list.CreateIdColumns);
            Assert.AreEqual(0, list.UpdateDateColumns.Count);
            Assert.AreEqual(0, list.UpdateIdColumns.Count);
            Assert.AreEqual(0, list.CreateDateColumns.Count);
            Assert.AreEqual(0, list.CreateIdColumns.Count);
        }

        #endregion

        #region Property Tests - UpdateDateColumns

        [TestMethod]
        public void UpdateDateColumns_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            var expected = new List<string> { "ModifiedDate", "UpdatedOn" };

            // Act
            list.UpdateDateColumns = expected;

            // Assert
            Assert.AreSame(expected, list.UpdateDateColumns);
        }

        [TestMethod]
        public void UpdateDateColumns_Set_MarksAsValidated()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();

            // Act
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };

            // Assert - Setting should mark this property as validated
            var validationResult = list.Validate();
            // Should only contain the 3 other properties that weren't set
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(System.Array.Exists(validationResult, s => s == "UpdateDateColumns"));
        }

        #endregion

        #region Property Tests - UpdateIdColumns

        [TestMethod]
        public void UpdateIdColumns_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            var expected = new List<string> { "ModifiedBy", "UpdatedBy" };

            // Act
            list.UpdateIdColumns = expected;

            // Assert
            Assert.AreSame(expected, list.UpdateIdColumns);
        }

        [TestMethod]
        public void UpdateIdColumns_Set_MarksAsValidated()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();

            // Act
            list.UpdateIdColumns = new List<string> { "ModifiedBy" };

            // Assert
            var validationResult = list.Validate();
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(System.Array.Exists(validationResult, s => s == "UpdateIdColumns"));
        }

        #endregion

        #region Property Tests - CreateDateColumns

        [TestMethod]
        public void CreateDateColumns_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            var expected = new List<string> { "CreatedDate", "CreateOn" };

            // Act
            list.CreateDateColumns = expected;

            // Assert
            Assert.AreSame(expected, list.CreateDateColumns);
        }

        [TestMethod]
        public void CreateDateColumns_Set_MarksAsValidated()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();

            // Act
            list.CreateDateColumns = new List<string> { "CreatedDate" };

            // Assert
            var validationResult = list.Validate();
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(System.Array.Exists(validationResult, s => s == "CreateDateColumns"));
        }

        #endregion

        #region Property Tests - CreateIdColumns

        [TestMethod]
        public void CreateIdColumns_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            var expected = new List<string> { "CreatedBy", "CreateBy" };

            // Act
            list.CreateIdColumns = expected;

            // Assert
            Assert.AreSame(expected, list.CreateIdColumns);
        }

        [TestMethod]
        public void CreateIdColumns_Set_MarksAsValidated()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();

            // Act
            list.CreateIdColumns = new List<string> { "CreatedBy" };

            // Assert
            var validationResult = list.Validate();
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(System.Array.Exists(validationResult, s => s == "CreateIdColumns"));
        }

        #endregion

        #region IsValid Property Tests

        [TestMethod]
        public void IsValid_NoPropertiesSet_ReturnsFalse()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();

            // Act
            var result = list.IsValid;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_AllPropertiesSet_ReturnsTrue()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };
            list.UpdateIdColumns = new List<string> { "ModifiedBy" };
            list.CreateDateColumns = new List<string> { "CreatedDate" };
            list.CreateIdColumns = new List<string> { "CreatedBy" };

            // Act
            var result = list.IsValid;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_SomePropertiesSet_ReturnsFalse()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };
            list.UpdateIdColumns = new List<string> { "ModifiedBy" };

            // Act
            var result = list.IsValid;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_OnlyUpdateDateColumnsSet_ReturnsFalse()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };

            // Act
            var result = list.IsValid;

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Validate Method Tests

        [TestMethod]
        public void Validate_NoPropertiesSet_ReturnsAllPropertyNames()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();

            // Act
            var result = list.Validate();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Length);
            Assert.IsTrue(System.Array.Exists(result, s => s == "UpdateDateColumns"));
            Assert.IsTrue(System.Array.Exists(result, s => s == "UpdateIdColumns"));
            Assert.IsTrue(System.Array.Exists(result, s => s == "CreateDateColumns"));
            Assert.IsTrue(System.Array.Exists(result, s => s == "CreateIdColumns"));
        }

        [TestMethod]
        public void Validate_AllPropertiesSet_ReturnsNull()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };
            list.UpdateIdColumns = new List<string> { "ModifiedBy" };
            list.CreateDateColumns = new List<string> { "CreatedDate" };
            list.CreateIdColumns = new List<string> { "CreatedBy" };

            // Act
            var result = list.Validate();

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_OnePropertySet_ReturnsThreePropertyNames()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };

            // Act
            var result = list.Validate();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.IsFalse(System.Array.Exists(result, s => s == "UpdateDateColumns"));
        }

        [TestMethod]
        public void Validate_TwoPropertiesSet_ReturnsTwoPropertyNames()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };
            list.CreateIdColumns = new List<string> { "CreatedBy" };

            // Act
            var result = list.Validate();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.IsTrue(System.Array.Exists(result, s => s == "UpdateIdColumns"));
            Assert.IsTrue(System.Array.Exists(result, s => s == "CreateDateColumns"));
        }

        [TestMethod]
        public void Validate_ThreePropertiesSet_ReturnsOnePropertyName()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string> { "ModifiedDate" };
            list.UpdateIdColumns = new List<string> { "ModifiedBy" };
            list.CreateDateColumns = new List<string> { "CreatedDate" };

            // Act
            var result = list.Validate();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("CreateIdColumns", result[0]);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Properties_WithEmptyLists_CountsAsSet()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = new List<string>();
            list.UpdateIdColumns = new List<string>();
            list.CreateDateColumns = new List<string>();
            list.CreateIdColumns = new List<string>();

            // Act
            var isValid = list.IsValid;

            // Assert - Setting to empty list still counts as "set"
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Properties_WithNullValues_CountsAsSet()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            list.UpdateDateColumns = null!;
            list.UpdateIdColumns = null!;
            list.CreateDateColumns = null!;
            list.CreateIdColumns = null!;

            // Act
            var isValid = list.IsValid;

            // Assert - Setting to null still marks the property as validated
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Properties_WithMultipleItems_PreservesAll()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            var columns = new List<string> { "Column1", "Column2", "Column3" };

            // Act
            list.UpdateDateColumns = columns;

            // Assert
            Assert.AreEqual(3, list.UpdateDateColumns.Count);
            Assert.AreEqual("Column1", list.UpdateDateColumns[0]);
            Assert.AreEqual("Column2", list.UpdateDateColumns[1]);
            Assert.AreEqual("Column3", list.UpdateDateColumns[2]);
        }

        [TestMethod]
        public void Properties_ReassignedMultipleTimes_UsesLastValue()
        {
            // Arrange
            var list = new CodeTableAuditColumnList();
            var first = new List<string> { "First" };
            var second = new List<string> { "Second" };
            var third = new List<string> { "Third" };

            // Act
            list.UpdateDateColumns = first;
            list.UpdateDateColumns = second;
            list.UpdateDateColumns = third;

            // Assert
            Assert.AreSame(third, list.UpdateDateColumns);
            Assert.AreEqual("Third", list.UpdateDateColumns[0]);
        }

        #endregion

        #region Serialization Attributes Tests

        [TestMethod]
        public void CodeTableAuditColumnList_HasSerializableAttribute()
        {
            // Arrange
            var type = typeof(CodeTableAuditColumnList);

            // Act
            var hasAttribute = System.Attribute.IsDefined(type, typeof(System.SerializableAttribute));

            // Assert
            Assert.IsTrue(hasAttribute, "CodeTableAuditColumnList should have Serializable attribute");
        }

        [TestMethod]
        public void CodeTableAuditColumnList_HasToolboxItemAttribute()
        {
            // Arrange
            var type = typeof(CodeTableAuditColumnList);

            // Act
            var attribute = System.Attribute.GetCustomAttribute(type, typeof(System.ComponentModel.ToolboxItemAttribute));

            // Assert
            Assert.IsNotNull(attribute, "CodeTableAuditColumnList should have ToolboxItem attribute");
        }

        #endregion
    }
}
