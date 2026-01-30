using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class RebuilderDataTests
    {
        [TestMethod]
        public void DefaultConstructor_SetsDefaults()
        {
            // Arrange & Act
            var data = new RebuilderData();

            // Assert
            Assert.AreEqual(string.Empty, data.ScriptFileName);
            Assert.AreEqual(Guid.Empty, data.ScriptId);
            Assert.AreEqual(-1, data.Sequence);
            Assert.AreEqual(string.Empty, data.ScriptText);
            Assert.AreEqual(string.Empty, data.Database);
            Assert.AreEqual(string.Empty, data.Tag);
        }

        [TestMethod]
        public void ScriptId_IsGuidEmpty_ByDefault()
        {
            // Arrange & Act
            var data = new RebuilderData();

            // Assert
            Assert.AreEqual(Guid.Empty, data.ScriptId);
        }

        [TestMethod]
        public void Sequence_DefaultsToMinusOne()
        {
            // Arrange & Act
            var data = new RebuilderData();

            // Assert
            Assert.AreEqual(-1, data.Sequence);
        }

        [TestMethod]
        public void Properties_CanBeSetAndGet()
        {
            // Arrange
            var data = new RebuilderData();
            var expectedId = Guid.NewGuid();

            // Act
            data.ScriptFileName = "test.sql";
            data.ScriptId = expectedId;
            data.Sequence = 5;
            data.ScriptText = "SELECT 1";
            data.Database = "TestDb";
            data.Tag = "TestTag";

            // Assert
            Assert.AreEqual("test.sql", data.ScriptFileName);
            Assert.AreEqual(expectedId, data.ScriptId);
            Assert.AreEqual(5, data.Sequence);
            Assert.AreEqual("SELECT 1", data.ScriptText);
            Assert.AreEqual("TestDb", data.Database);
            Assert.AreEqual("TestTag", data.Tag);
        }
    }
}
