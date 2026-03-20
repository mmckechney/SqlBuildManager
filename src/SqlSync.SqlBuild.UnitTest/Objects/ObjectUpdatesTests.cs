using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Objects;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.UnitTest.Objects
{
    [TestClass]
    public class ObjectUpdatesTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_CreatesInstanceWithNullProperties()
        {
            // Act
            var obj = new ObjectUpdates();

            // Assert
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.ShortFileName);
            Assert.IsNull(obj.SourceObject);
            Assert.IsNull(obj.SourceDatabase);
            Assert.IsNull(obj.SourceServer);
            Assert.IsNull(obj.ObjectType);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void ShortFileName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();
            string expected = "MyScript.sql";

            // Act
            obj.ShortFileName = expected;

            // Assert
            Assert.AreEqual(expected, obj.ShortFileName);
        }

        [TestMethod]
        public void SourceObject_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();
            string expected = "dbo.MyStoredProcedure";

            // Act
            obj.SourceObject = expected;

            // Assert
            Assert.AreEqual(expected, obj.SourceObject);
        }

        [TestMethod]
        public void SourceDatabase_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();
            string expected = "ProductionDB";

            // Act
            obj.SourceDatabase = expected;

            // Assert
            Assert.AreEqual(expected, obj.SourceDatabase);
        }

        [TestMethod]
        public void SourceServer_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();
            string expected = "SqlServer01";

            // Act
            obj.SourceServer = expected;

            // Assert
            Assert.AreEqual(expected, obj.SourceServer);
        }

        [TestMethod]
        public void ObjectType_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();
            string expected = "StoredProcedure";

            // Act
            obj.ObjectType = expected;

            // Assert
            Assert.AreEqual(expected, obj.ObjectType);
        }

        [TestMethod]
        public void IncludePermissions_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Act
            obj.IncludePermissions = true;

            // Assert
            Assert.IsTrue(obj.IncludePermissions);
        }

        [TestMethod]
        public void IncludePermissionsSpecified_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Act
            obj.IncludePermissionsSpecified = true;

            // Assert
            Assert.IsTrue(obj.IncludePermissionsSpecified);
        }

        [TestMethod]
        public void ScriptAsAlter_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Act
            obj.ScriptAsAlter = true;

            // Assert
            Assert.IsTrue(obj.ScriptAsAlter);
        }

        [TestMethod]
        public void ScriptAsAlterSpecified_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Act
            obj.ScriptAsAlterSpecified = true;

            // Assert
            Assert.IsTrue(obj.ScriptAsAlterSpecified);
        }

        [TestMethod]
        public void ScriptPkWithTable_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Act
            obj.ScriptPkWithTable = true;

            // Assert
            Assert.IsTrue(obj.ScriptPkWithTable);
        }

        [TestMethod]
        public void ScriptPkWithTableSpecified_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Act
            obj.ScriptPkWithTableSpecified = true;

            // Assert
            Assert.IsTrue(obj.ScriptPkWithTableSpecified);
        }

        #endregion

        #region Boolean Property Default Values Tests

        [TestMethod]
        public void IncludePermissions_Default_IsFalse()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Assert
            Assert.IsFalse(obj.IncludePermissions);
            Assert.IsFalse(obj.IncludePermissionsSpecified);
        }

        [TestMethod]
        public void ScriptAsAlter_Default_IsFalse()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Assert
            Assert.IsFalse(obj.ScriptAsAlter);
            Assert.IsFalse(obj.ScriptAsAlterSpecified);
        }

        [TestMethod]
        public void ScriptPkWithTable_Default_IsFalse()
        {
            // Arrange
            var obj = new ObjectUpdates();

            // Assert
            Assert.IsFalse(obj.ScriptPkWithTable);
            Assert.IsFalse(obj.ScriptPkWithTableSpecified);
        }

        #endregion

        #region Serialization Tests

        [TestMethod]
        public void XmlSerialization_WithAllProperties_RoundTripsCorrectly()
        {
            // Arrange
            var obj = new ObjectUpdates
            {
                ShortFileName = "UpdateProc.sql",
                SourceObject = "dbo.GetUsers",
                SourceDatabase = "UserDB",
                SourceServer = "ProdServer",
                ObjectType = "StoredProcedure",
                IncludePermissions = true,
                IncludePermissionsSpecified = true,
                ScriptAsAlter = true,
                ScriptAsAlterSpecified = true,
                ScriptPkWithTable = false,
                ScriptPkWithTableSpecified = true
            };

            var serializer = new XmlSerializer(typeof(ObjectUpdates));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                xml = writer.ToString();
            }

            ObjectUpdates deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (ObjectUpdates)serializer.Deserialize(reader)!;
            }

            // Assert
            Assert.AreEqual(obj!.ShortFileName, deserialized.ShortFileName);
            Assert.AreEqual(obj.SourceObject, deserialized.SourceObject);
            Assert.AreEqual(obj.SourceDatabase, deserialized.SourceDatabase);
            Assert.AreEqual(obj.SourceServer, deserialized.SourceServer);
            Assert.AreEqual(obj.ObjectType, deserialized.ObjectType);
            Assert.AreEqual(obj.IncludePermissions, deserialized.IncludePermissions);
            Assert.AreEqual(obj.ScriptAsAlter, deserialized.ScriptAsAlter);
            Assert.AreEqual(obj.ScriptPkWithTable, deserialized.ScriptPkWithTable);
        }

        [TestMethod]
        public void XmlSerialization_EmptyObject_RoundTripsCorrectly()
        {
            // Arrange
            var obj = new ObjectUpdates();
            var serializer = new XmlSerializer(typeof(ObjectUpdates));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                xml = writer.ToString();
            }

            ObjectUpdates deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (ObjectUpdates)serializer.Deserialize(reader)!;
            }

            // Assert
            Assert.IsNotNull(deserialized);
        }

        [TestMethod]
        public void XmlSerialization_ContainsCorrectNamespace()
        {
            // Arrange
            var obj = new ObjectUpdates
            {
                ShortFileName = "Test.sql"
            };
            var serializer = new XmlSerializer(typeof(ObjectUpdates));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                xml = writer.ToString();
            }

            // Assert
            Assert.IsTrue(xml.Contains("http://www.mckechney.com/ObjectUpdates.xsd"), "XML should contain correct namespace");
        }

        [TestMethod]
        public void XmlDeserialization_FromXmlString_WorksCorrectly()
        {
            // Arrange
            string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ObjectUpdates xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
    ShortFileName=""TestProc.sql"" 
    SourceObject=""dbo.TestProc"" 
    SourceDatabase=""TestDB"" 
    SourceServer=""TestServer"" 
    ObjectType=""StoredProcedure""
    IncludePermissions=""true""
    ScriptAsAlter=""false""
    xmlns=""http://www.mckechney.com/ObjectUpdates.xsd"" />";

            var serializer = new XmlSerializer(typeof(ObjectUpdates));

            // Act
            ObjectUpdates deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (ObjectUpdates)serializer.Deserialize(reader)!;
            }

            // Assert
            Assert.AreEqual("TestProc.sql", deserialized.ShortFileName);
            Assert.AreEqual("dbo.TestProc", deserialized.SourceObject);
            Assert.AreEqual("TestDB", deserialized.SourceDatabase);
            Assert.AreEqual("TestServer", deserialized.SourceServer);
            Assert.AreEqual("StoredProcedure", deserialized.ObjectType);
            Assert.IsTrue(deserialized.IncludePermissions);
            Assert.IsFalse(deserialized.ScriptAsAlter);
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void Properties_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var obj = new ObjectUpdates
            {
                ShortFileName = "Script With Spaces & Special!@#.sql",
                SourceObject = "dbo.[Object-Name]",
                SourceDatabase = "DB_Name-Test",
                SourceServer = "Server\\Instance"
            };

            // Assert
            Assert.AreEqual("Script With Spaces & Special!@#.sql", obj.ShortFileName);
            Assert.AreEqual("dbo.[Object-Name]", obj.SourceObject);
            Assert.AreEqual("DB_Name-Test", obj.SourceDatabase);
            Assert.AreEqual("Server\\Instance", obj.SourceServer);
        }

        [TestMethod]
        public void Properties_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange
            var obj = new ObjectUpdates
            {
                ShortFileName = string.Empty,
                SourceObject = string.Empty,
                SourceDatabase = string.Empty,
                SourceServer = string.Empty,
                ObjectType = string.Empty
            };

            // Assert
            Assert.AreEqual(string.Empty, obj.ShortFileName);
            Assert.AreEqual(string.Empty, obj.SourceObject);
            Assert.AreEqual(string.Empty, obj.SourceDatabase);
            Assert.AreEqual(string.Empty, obj.SourceServer);
            Assert.AreEqual(string.Empty, obj.ObjectType);
        }

        #endregion
    }
}
