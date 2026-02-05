using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Status;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.UnitTest.Status
{
    [TestClass]
    public class StatusDictionaryTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_SetsDefaultNames()
        {
            // Act
            var dict = new StatusDictionary<string>();

            // Assert
            Assert.AreEqual("item", dict.ItemName);
            Assert.AreEqual("key", dict.KeyName);
            Assert.AreEqual("value", dict.ValueName);
        }

        [TestMethod]
        public void Constructor_WithCustomNames_SetsCustomNames()
        {
            // Act
            var dict = new StatusDictionary<string>("customItem", "customKey", "customValue");

            // Assert
            Assert.AreEqual("customItem", dict.ItemName);
            Assert.AreEqual("customKey", dict.KeyName);
            Assert.AreEqual("customValue", dict.ValueName);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void ItemName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.ItemName = "myItem";

            // Assert
            Assert.AreEqual("myItem", dict.ItemName);
        }

        [TestMethod]
        public void KeyName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.KeyName = "myKey";

            // Assert
            Assert.AreEqual("myKey", dict.KeyName);
        }

        [TestMethod]
        public void ValueName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.ValueName = "myValue";

            // Assert
            Assert.AreEqual("myValue", dict.ValueName);
        }

        #endregion

        #region Dictionary Operations Tests

        [TestMethod]
        public void Add_SingleItem_CanBeRetrieved()
        {
            // Arrange
            var dict = new StatusDictionary<int>();

            // Act
            dict.Add("test", 42);

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(42, dict["test"]);
        }

        [TestMethod]
        public void Add_MultipleItems_AllCanBeRetrieved()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.Add("key1", "value1");
            dict.Add("key2", "value2");
            dict.Add("key3", "value3");

            // Assert
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual("value1", dict["key1"]);
            Assert.AreEqual("value2", dict["key2"]);
            Assert.AreEqual("value3", dict["key3"]);
        }

        [TestMethod]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new StatusDictionary<string>();
            dict.Add("existing", "value");

            // Act & Assert
            Assert.IsTrue(dict.ContainsKey("existing"));
        }

        [TestMethod]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new StatusDictionary<string>();
            dict.Add("existing", "value");

            // Act & Assert
            Assert.IsFalse(dict.ContainsKey("nonexisting"));
        }

        [TestMethod]
        public void Remove_ExistingKey_RemovesItem()
        {
            // Arrange
            var dict = new StatusDictionary<string>();
            dict.Add("toRemove", "value");

            // Act
            bool removed = dict.Remove("toRemove");

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
        }

        #endregion

        #region IXmlSerializable Tests

        [TestMethod]
        public void GetSchema_ReturnsNull()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            var schema = dict.GetSchema();

            // Assert
            Assert.IsNull(schema);
        }

        [TestMethod]
        public void Dictionary_AddAndRetrieve_WorksCorrectly()
        {
            // Arrange
            var dict = new StatusDictionary<string>("Entry", "Name", "Data");
            
            // Act
            dict.Add("Server1", "Database1");
            dict.Add("Server2", "Database2");

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("Database1", dict["Server1"]);
            Assert.AreEqual("Database2", dict["Server2"]);
        }

        [TestMethod]
        public void Dictionary_WithIntValues_WorksCorrectly()
        {
            // Arrange
            var dict = new StatusDictionary<int>("item", "key", "value");
            
            // Act
            dict.Add("count1", 100);
            dict.Add("count2", 200);

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(100, dict["count1"]);
            Assert.AreEqual(200, dict["count2"]);
        }

        #endregion
    }

    [TestClass]
    public class StatusHelperTypeTests
    {
        [TestMethod]
        public void ScriptStatusType_CoreEnumValues_AreDefined()
        {
            // Assert that core expected enum values exist with correct values
            Assert.AreEqual(ScriptStatusType.NotRun, (ScriptStatusType)0);
            Assert.AreEqual(ScriptStatusType.Locked, (ScriptStatusType)1);
            Assert.AreEqual(ScriptStatusType.UpToDate, (ScriptStatusType)2);
            Assert.AreEqual(ScriptStatusType.ChangedSinceCommit, (ScriptStatusType)3);
            Assert.AreEqual(ScriptStatusType.ServerChange, (ScriptStatusType)4);
            Assert.AreEqual(ScriptStatusType.NotRunButOlderVersion, (ScriptStatusType)5);
            Assert.AreEqual(ScriptStatusType.FileMissing, (ScriptStatusType)6);
            Assert.AreEqual(ScriptStatusType.Unknown, (ScriptStatusType)99);
        }

        [TestMethod]
        public void ScriptStatusType_CanBeConvertedToString()
        {
            // Act & Assert
            Assert.AreEqual("NotRun", ScriptStatusType.NotRun.ToString());
            Assert.AreEqual("Locked", ScriptStatusType.Locked.ToString());
            Assert.AreEqual("UpToDate", ScriptStatusType.UpToDate.ToString());
            Assert.AreEqual("ChangedSinceCommit", ScriptStatusType.ChangedSinceCommit.ToString());
            Assert.AreEqual("ServerChange", ScriptStatusType.ServerChange.ToString());
            Assert.AreEqual("NotRunButOlderVersion", ScriptStatusType.NotRunButOlderVersion.ToString());
            Assert.AreEqual("FileMissing", ScriptStatusType.FileMissing.ToString());
            Assert.AreEqual("Unknown", ScriptStatusType.Unknown.ToString());
        }

        [TestMethod]
        public void ScriptStatusType_CanBeParsedFromString()
        {
            // Act & Assert
            Assert.AreEqual(ScriptStatusType.NotRun, Enum.Parse<ScriptStatusType>("NotRun"));
            Assert.AreEqual(ScriptStatusType.UpToDate, Enum.Parse<ScriptStatusType>("UpToDate"));
            Assert.AreEqual(ScriptStatusType.FileMissing, Enum.Parse<ScriptStatusType>("FileMissing"));
        }
    }

    [TestClass]
    public class ReportTypeTests
    {
        [TestMethod]
        public void ReportType_AllEnumValues_AreDefined()
        {
            // Assert all report types exist
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.XML));
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.CSV));
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.HTML));
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.Summary));
        }

        [TestMethod]
        public void ReportType_CanBeConvertedToString()
        {
            // Act & Assert
            Assert.AreEqual("XML", ReportType.XML.ToString());
            Assert.AreEqual("CSV", ReportType.CSV.ToString());
            Assert.AreEqual("HTML", ReportType.HTML.ToString());
            Assert.AreEqual("Summary", ReportType.Summary.ToString());
        }

        [TestMethod]
        public void ReportType_CanBeParsedFromString()
        {
            // Act & Assert
            Assert.AreEqual(ReportType.XML, Enum.Parse<ReportType>("XML"));
            Assert.AreEqual(ReportType.CSV, Enum.Parse<ReportType>("CSV"));
            Assert.AreEqual(ReportType.HTML, Enum.Parse<ReportType>("HTML"));
            Assert.AreEqual(ReportType.Summary, Enum.Parse<ReportType>("Summary"));
        }

        [TestMethod]
        public void ReportType_HTMLValue_IsZero()
        {
            // HTML is first in the enum, so value = 0
            Assert.AreEqual(0, (int)ReportType.HTML);
        }

        [TestMethod]
        public void ReportType_CSVValue_IsOne()
        {
            // CSV is second in the enum, so value = 1
            Assert.AreEqual(1, (int)ReportType.CSV);
        }

        [TestMethod]
        public void ReportType_SummaryValue_IsTwo()
        {
            // Summary is third in the enum, so value = 2
            Assert.AreEqual(2, (int)ReportType.Summary);
        }

        [TestMethod]
        public void ReportType_XMLValue_IsThree()
        {
            // XML is fourth in the enum, so value = 3
            Assert.AreEqual(3, (int)ReportType.XML);
        }
    }
}
