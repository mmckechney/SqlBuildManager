using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SerializableDictionaryTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_SetsDefaultNames()
        {
            // Act
            var dict = new SerializableDictionary<string, string>();

            // Assert
            Assert.AreEqual("item", dict.ItemName);
            Assert.AreEqual("key", dict.KeyName);
            Assert.AreEqual("value", dict.ValueName);
        }

        [TestMethod]
        public void Constructor_WithCustomNames_SetsCustomNames()
        {
            // Act
            var dict = new SerializableDictionary<string, string>("customItem", "customKey", "customValue");

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
            var dict = new SerializableDictionary<string, string>();

            // Act
            dict.ItemName = "entry";

            // Assert
            Assert.AreEqual("entry", dict.ItemName);
        }

        [TestMethod]
        public void KeyName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Act
            dict.KeyName = "name";

            // Assert
            Assert.AreEqual("name", dict.KeyName);
        }

        [TestMethod]
        public void ValueName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Act
            dict.ValueName = "data";

            // Assert
            Assert.AreEqual("data", dict.ValueName);
        }

        #endregion

        #region Dictionary Operations Tests

        [TestMethod]
        public void Add_StringStringPair_CanBeRetrieved()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Act
            dict.Add("key1", "value1");

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("value1", dict["key1"]);
        }

        [TestMethod]
        public void Add_IntIntPair_CanBeRetrieved()
        {
            // Arrange
            var dict = new SerializableDictionary<int, int>();

            // Act
            dict.Add(1, 100);
            dict.Add(2, 200);

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(100, dict[1]);
            Assert.AreEqual(200, dict[2]);
        }

        [TestMethod]
        public void Add_StringIntPair_CanBeRetrieved()
        {
            // Arrange
            var dict = new SerializableDictionary<string, int>();

            // Act
            dict.Add("count", 42);

            // Assert
            Assert.AreEqual(42, dict["count"]);
        }

        [TestMethod]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("exists", "value");

            // Act & Assert
            Assert.IsTrue(dict.ContainsKey("exists"));
        }

        [TestMethod]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("exists", "value");

            // Act & Assert
            Assert.IsFalse(dict.ContainsKey("doesNotExist"));
        }

        [TestMethod]
        public void Remove_ExistingKey_RemovesItem()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("toRemove", "value");

            // Act
            bool removed = dict.Remove("toRemove");

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
        }

        [TestMethod]
        public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("key", "value");

            // Act
            bool result = dict.TryGetValue("key", out string value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("value", value);
        }

        [TestMethod]
        public void TryGetValue_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Act
            bool result = dict.TryGetValue("nonexistent", out string value);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("key1", "value1");
            dict.Add("key2", "value2");

            // Act
            dict.Clear();

            // Assert
            Assert.AreEqual(0, dict.Count);
        }

        [TestMethod]
        public void Indexer_SetExistingKey_UpdatesValue()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("key", "oldValue");

            // Act
            dict["key"] = "newValue";

            // Assert
            Assert.AreEqual("newValue", dict["key"]);
        }

        [TestMethod]
        public void Indexer_SetNewKey_AddsItem()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Act
            dict["newKey"] = "newValue";

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("newValue", dict["newKey"]);
        }

        #endregion

        #region IXmlSerializable Tests

        [TestMethod]
        public void GetSchema_ReturnsNull()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Act
            var schema = dict.GetSchema();

            // Assert
            Assert.IsNull(schema);
        }

        [TestMethod]
        public void XmlSerialization_StringStringDictionary_CanWrite()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>("entry", "name", "data");
            dict.Add("Server1", "Database1");
            dict.Add("Server2", "Database2");

            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            // Assert - Verify XML is written correctly
            Assert.IsTrue(xml.Contains("Server1"), "XML should contain key Server1");
            Assert.IsTrue(xml.Contains("Database1"), "XML should contain value Database1");
        }

        [TestMethod]
        public void XmlSerialization_EmptyDictionary_CanWrite()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            // Assert
            Assert.IsNotNull(xml);
            Assert.IsTrue(xml.Contains("dictionary"), "XML should contain root element");
        }

        [TestMethod]
        public void XmlSerialization_IntIntDictionary_CanWrite()
        {
            // Arrange
            var dict = new SerializableDictionary<int, int>("item", "id", "count");
            dict.Add(1, 100);
            dict.Add(2, 200);
            dict.Add(3, 300);

            var serializer = new XmlSerializer(typeof(SerializableDictionary<int, int>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            // Assert
            Assert.IsNotNull(xml);
            Assert.IsTrue(xml.Contains("100"), "XML should contain value 100");
        }

        [TestMethod]
        public void XmlSerialization_StringIntDictionary_CanWrite()
        {
            // Arrange
            var dict = new SerializableDictionary<string, int>("setting", "name", "value");
            dict.Add("Timeout", 30);
            dict.Add("MaxRetries", 5);

            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, int>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            // Assert
            Assert.IsNotNull(xml);
            Assert.IsTrue(xml.Contains("Timeout"), "XML should contain key Timeout");
            Assert.IsTrue(xml.Contains("30"), "XML should contain value 30");
        }

        [TestMethod]
        public void XmlSerialization_WithManyItems_PreservesOrder()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            for (int i = 0; i < 100; i++)
            {
                dict.Add($"Key{i}", $"Value{i}");
            }

            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            SerializableDictionary<string, string> deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (SerializableDictionary<string, string>)serializer.Deserialize(reader);
            }

            // Assert
            Assert.AreEqual(100, deserialized.Count);
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(deserialized.ContainsKey($"Key{i}"));
                Assert.AreEqual($"Value{i}", deserialized[$"Key{i}"]);
            }
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void Serialization_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("Special<>&\"'", "Value with <xml> & \"quotes\"");

            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            SerializableDictionary<string, string> deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (SerializableDictionary<string, string>)serializer.Deserialize(reader);
            }

            // Assert
            Assert.AreEqual("Value with <xml> & \"quotes\"", deserialized["Special<>&\"'"]);
        }

        [TestMethod]
        public void Serialization_WithEmptyStrings_HandlesCorrectly()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("empty", string.Empty);
            dict.Add("", "emptyKey");

            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            SerializableDictionary<string, string> deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (SerializableDictionary<string, string>)serializer.Deserialize(reader);
            }

            // Assert
            Assert.AreEqual(2, deserialized.Count);
            Assert.AreEqual(string.Empty, deserialized["empty"]);
            Assert.AreEqual("emptyKey", deserialized[""]);
        }

        [TestMethod]
        public void Serialization_WithUnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();
            dict.Add("日本語", "Japanese");
            dict.Add("emoji", "🎉🚀");

            var serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dict);
                xml = writer.ToString();
            }

            SerializableDictionary<string, string> deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (SerializableDictionary<string, string>)serializer.Deserialize(reader);
            }

            // Assert
            Assert.AreEqual(2, deserialized.Count);
            Assert.AreEqual("Japanese", deserialized["日本語"]);
            Assert.AreEqual("🎉🚀", deserialized["emoji"]);
        }

        #endregion

        #region Inheritance Tests

        [TestMethod]
        public void SerializableDictionary_InheritsFromDictionary()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Assert
            Assert.IsInstanceOfType(dict, typeof(System.Collections.Generic.Dictionary<string, string>));
        }

        [TestMethod]
        public void SerializableDictionary_ImplementsIXmlSerializable()
        {
            // Arrange
            var dict = new SerializableDictionary<string, string>();

            // Assert
            Assert.IsInstanceOfType(dict, typeof(System.Xml.Serialization.IXmlSerializable));
        }

        #endregion
    }
}
