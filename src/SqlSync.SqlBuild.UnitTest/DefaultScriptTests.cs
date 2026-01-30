using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.DefaultScripts;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class DefaultScriptTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_SetsDefaultValues()
        {
            // Act
            var script = new DefaultScript();

            // Assert
            Assert.AreEqual(1000, script.BuildOrder);
            Assert.AreEqual(string.Empty, script.ScriptName);
            Assert.AreEqual(string.Empty, script.Description);
            Assert.IsTrue(script.RollBackScript);
            Assert.IsTrue(script.RollBackBuild);
            Assert.IsFalse(script.StripTransactions);
            Assert.AreEqual(string.Empty, script.DatabaseName);
            Assert.IsTrue(script.AllowMultipleRuns);
            Assert.AreEqual(500, script.ScriptTimeout);
            Assert.AreEqual("Default", script.ScriptTag);
            Assert.AreEqual(string.Empty, script.ApplyToGroups);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void BuildOrder_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.BuildOrder = 500;

            // Assert
            Assert.AreEqual(500, script.BuildOrder);
        }

        [TestMethod]
        public void BuildOrder_SetToZero_AcceptsValue()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.BuildOrder = 0;

            // Assert
            Assert.AreEqual(0, script.BuildOrder);
        }

        [TestMethod]
        public void BuildOrder_SetToNegative_AcceptsValue()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.BuildOrder = -1;

            // Assert
            Assert.AreEqual(-1, script.BuildOrder);
        }

        [TestMethod]
        public void ScriptName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();
            string expected = "CreateTable.sql";

            // Act
            script.ScriptName = expected;

            // Assert
            Assert.AreEqual(expected, script.ScriptName);
        }

        [TestMethod]
        public void Description_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();
            string expected = "Creates the users table";

            // Act
            script.Description = expected;

            // Assert
            Assert.AreEqual(expected, script.Description);
        }

        [TestMethod]
        public void RollBackScript_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.RollBackScript = false;

            // Assert
            Assert.IsFalse(script.RollBackScript);
        }

        [TestMethod]
        public void RollBackBuild_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.RollBackBuild = false;

            // Assert
            Assert.IsFalse(script.RollBackBuild);
        }

        [TestMethod]
        public void StripTransactions_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.StripTransactions = true;

            // Assert
            Assert.IsTrue(script.StripTransactions);
        }

        [TestMethod]
        public void DatabaseName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();
            string expected = "ProductionDB";

            // Act
            script.DatabaseName = expected;

            // Assert
            Assert.AreEqual(expected, script.DatabaseName);
        }

        [TestMethod]
        public void AllowMultipleRuns_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.AllowMultipleRuns = false;

            // Assert
            Assert.IsFalse(script.AllowMultipleRuns);
        }

        [TestMethod]
        public void ScriptTimeout_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();

            // Act
            script.ScriptTimeout = 300;

            // Assert
            Assert.AreEqual(300, script.ScriptTimeout);
        }

        [TestMethod]
        public void ScriptTag_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();
            string expected = "Migration";

            // Act
            script.ScriptTag = expected;

            // Assert
            Assert.AreEqual(expected, script.ScriptTag);
        }

        [TestMethod]
        public void ApplyToGroups_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var script = new DefaultScript();
            string expected = "Group1,Group2";

            // Act
            script.ApplyToGroups = expected;

            // Assert
            Assert.AreEqual(expected, script.ApplyToGroups);
        }

        #endregion

        #region Serialization Tests

        [TestMethod]
        public void XmlSerialization_DefaultScript_SerializesAndDeserializesCorrectly()
        {
            // Arrange
            var script = new DefaultScript
            {
                BuildOrder = 100,
                ScriptName = "TestScript.sql",
                Description = "Test Description",
                RollBackScript = false,
                RollBackBuild = true,
                StripTransactions = true,
                DatabaseName = "TestDB",
                AllowMultipleRuns = false,
                ScriptTimeout = 600,
                ScriptTag = "CustomTag",
                ApplyToGroups = "Dev,QA"
            };

            var serializer = new XmlSerializer(typeof(DefaultScript));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, script);
                xml = writer.ToString();
            }

            DefaultScript deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (DefaultScript)serializer.Deserialize(reader);
            }

            // Assert
            Assert.AreEqual(script.BuildOrder, deserialized.BuildOrder);
            Assert.AreEqual(script.ScriptName, deserialized.ScriptName);
            Assert.AreEqual(script.Description, deserialized.Description);
            Assert.AreEqual(script.RollBackScript, deserialized.RollBackScript);
            Assert.AreEqual(script.RollBackBuild, deserialized.RollBackBuild);
            Assert.AreEqual(script.StripTransactions, deserialized.StripTransactions);
            Assert.AreEqual(script.DatabaseName, deserialized.DatabaseName);
            Assert.AreEqual(script.AllowMultipleRuns, deserialized.AllowMultipleRuns);
            Assert.AreEqual(script.ScriptTimeout, deserialized.ScriptTimeout);
            Assert.AreEqual(script.ScriptTag, deserialized.ScriptTag);
            Assert.AreEqual(script.ApplyToGroups, deserialized.ApplyToGroups);
        }

        [TestMethod]
        public void XmlSerialization_WithDefaultValues_OmitsDefaultAttributes()
        {
            // Arrange
            var script = new DefaultScript();
            var serializer = new XmlSerializer(typeof(DefaultScript));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, script);
                xml = writer.ToString();
            }

            // Assert - Default values should be omitted from XML per DefaultValue attributes
            Assert.IsFalse(xml.Contains("BuildOrder=\"1000\""), "Default BuildOrder should be omitted");
            Assert.IsFalse(xml.Contains("RollBackScript=\"true\""), "Default RollBackScript should be omitted");
        }

        [TestMethod]
        public void XmlSerialization_WithNonDefaultValues_IncludesAttributes()
        {
            // Arrange
            var script = new DefaultScript
            {
                BuildOrder = 50, // Non-default
                ScriptName = "custom.sql" // Non-default
            };
            var serializer = new XmlSerializer(typeof(DefaultScript));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, script);
                xml = writer.ToString();
            }

            // Assert
            Assert.IsTrue(xml.Contains("BuildOrder=\"50\""), "Non-default BuildOrder should be included");
            Assert.IsTrue(xml.Contains("ScriptName=\"custom.sql\""), "Non-default ScriptName should be included");
        }

        #endregion
    }

    [TestClass]
    public class DefaultScriptRegistryTests
    {
        [TestMethod]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var registry = new DefaultScriptRegistry();

            // Assert
            Assert.IsNotNull(registry);
            Assert.IsNull(registry.Items);
        }

        [TestMethod]
        public void Items_SetAndGet_ReturnsCorrectArray()
        {
            // Arrange
            var registry = new DefaultScriptRegistry();
            var scripts = new[]
            {
                new DefaultScript { ScriptName = "Script1.sql" },
                new DefaultScript { ScriptName = "Script2.sql" }
            };

            // Act
            registry.Items = scripts;

            // Assert
            Assert.AreEqual(2, registry.Items.Length);
            Assert.AreEqual("Script1.sql", registry.Items[0].ScriptName);
            Assert.AreEqual("Script2.sql", registry.Items[1].ScriptName);
        }

        [TestMethod]
        public void Items_SetToEmpty_ReturnsEmptyArray()
        {
            // Arrange
            var registry = new DefaultScriptRegistry();

            // Act
            registry.Items = Array.Empty<DefaultScript>();

            // Assert
            Assert.IsNotNull(registry.Items);
            Assert.AreEqual(0, registry.Items.Length);
        }

        [TestMethod]
        public void XmlSerialization_Registry_SerializesAndDeserializesCorrectly()
        {
            // Arrange
            var registry = new DefaultScriptRegistry
            {
                Items = new[]
                {
                    new DefaultScript
                    {
                        BuildOrder = 1,
                        ScriptName = "First.sql",
                        DatabaseName = "DB1"
                    },
                    new DefaultScript
                    {
                        BuildOrder = 2,
                        ScriptName = "Second.sql",
                        DatabaseName = "DB2"
                    }
                }
            };
            var serializer = new XmlSerializer(typeof(DefaultScriptRegistry));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, registry);
                xml = writer.ToString();
            }

            DefaultScriptRegistry deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (DefaultScriptRegistry)serializer.Deserialize(reader);
            }

            // Assert
            Assert.AreEqual(2, deserialized.Items.Length);
            Assert.AreEqual("First.sql", deserialized.Items[0].ScriptName);
            Assert.AreEqual("Second.sql", deserialized.Items[1].ScriptName);
            Assert.AreEqual(1, deserialized.Items[0].BuildOrder);
            Assert.AreEqual(2, deserialized.Items[1].BuildOrder);
        }

        [TestMethod]
        public void XmlSerialization_EmptyRegistry_SerializesCorrectly()
        {
            // Arrange
            var registry = new DefaultScriptRegistry();
            var serializer = new XmlSerializer(typeof(DefaultScriptRegistry));

            // Act
            string xml;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, registry);
                xml = writer.ToString();
            }

            DefaultScriptRegistry deserialized;
            using (var reader = new StringReader(xml))
            {
                deserialized = (DefaultScriptRegistry)serializer.Deserialize(reader);
            }

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.IsNull(deserialized.Items);
        }
    }
}
