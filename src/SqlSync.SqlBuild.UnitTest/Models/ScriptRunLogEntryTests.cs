using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class ScriptRunLogEntryTests
    {
        [TestMethod]
        public void Constructor_WithAllParameters_CreatesEntry()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var commitDate = DateTime.UtcNow;

            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: scriptId,
                ScriptFileHash: "HASH123",
                CommitDate: commitDate,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            // Assert
            Assert.AreEqual("build.sbm", entry.BuildFileName);
            Assert.AreEqual("script.sql", entry.ScriptFileName);
            Assert.AreEqual(scriptId, entry.ScriptId);
            Assert.AreEqual("HASH123", entry.ScriptFileHash);
            Assert.AreEqual(commitDate, entry.CommitDate);
            Assert.AreEqual(1, entry.Sequence);
            Assert.AreEqual("testuser", entry.UserId);
            Assert.IsTrue(entry.AllowScriptBlock.Value);
            Assert.AreEqual("admin", entry.AllowBlockUpdateId);
            Assert.AreEqual("SELECT 1", entry.ScriptText);
            Assert.AreEqual("v1.0", entry.Tag);
        }

        [TestMethod]
        public void Constructor_WithNullValues_CreatesEntryWithNulls()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: null,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.IsNull(entry.BuildFileName);
            Assert.IsNull(entry.ScriptFileName);
            Assert.IsNull(entry.ScriptId);
            Assert.IsNull(entry.ScriptFileHash);
            Assert.IsNull(entry.CommitDate);
            Assert.IsNull(entry.Sequence);
            Assert.IsNull(entry.UserId);
            Assert.IsNull(entry.AllowScriptBlock);
            Assert.IsNull(entry.AllowBlockUpdateId);
            Assert.IsNull(entry.ScriptText);
            Assert.IsNull(entry.Tag);
        }

        [TestMethod]
        public void Equality_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var commitDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var entry1 = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: scriptId,
                ScriptFileHash: "HASH123",
                CommitDate: commitDate,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            var entry2 = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: scriptId,
                ScriptFileHash: "HASH123",
                CommitDate: commitDate,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            // Assert - records are value-equal
            Assert.AreEqual(entry1, entry2);
        }

        [TestMethod]
        public void Equality_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var entry1 = new ScriptRunLogEntry(
                BuildFileName: "build1.sbm",
                ScriptFileName: "script.sql",
                ScriptId: Guid.NewGuid(),
                ScriptFileHash: "HASH123",
                CommitDate: DateTime.UtcNow,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            var entry2 = new ScriptRunLogEntry(
                BuildFileName: "build2.sbm",
                ScriptFileName: "script.sql",
                ScriptId: Guid.NewGuid(),
                ScriptFileHash: "HASH456",
                CommitDate: DateTime.UtcNow,
                Sequence: 2,
                UserId: "otheruser",
                AllowScriptBlock: false,
                AllowBlockUpdateId: "other",
                ScriptText: "SELECT 2",
                Tag: "v2.0");

            // Assert
            Assert.AreNotEqual(entry1, entry2);
        }

        [TestMethod]
        public void GetHashCode_WithSameValues_ReturnsSameHash()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var commitDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var entry1 = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: scriptId,
                ScriptFileHash: "HASH123",
                CommitDate: commitDate,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            var entry2 = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: scriptId,
                ScriptFileHash: "HASH123",
                CommitDate: commitDate,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            // Assert
            Assert.AreEqual(entry1.GetHashCode(), entry2.GetHashCode());
        }

        [TestMethod]
        public void ToString_ReturnsNonEmptyString()
        {
            // Arrange
            var entry = new ScriptRunLogEntry(
                BuildFileName: "build.sbm",
                ScriptFileName: "script.sql",
                ScriptId: Guid.NewGuid(),
                ScriptFileHash: "HASH123",
                CommitDate: DateTime.UtcNow,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            // Act
            var result = entry.ToString();

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("ScriptRunLogEntry"));
        }

        [TestMethod]
        public void AllowScriptBlock_FalseValue_IsStored()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: null,
                UserId: null,
                AllowScriptBlock: false,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.IsFalse(entry.AllowScriptBlock.Value);
        }

        [TestMethod]
        public void Sequence_ZeroValue_IsStored()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: 0,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.AreEqual(0, entry.Sequence);
        }

        [TestMethod]
        public void Sequence_NegativeValue_IsStored()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: -1,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.AreEqual(-1, entry.Sequence);
        }

        [TestMethod]
        public void ScriptId_EmptyGuid_IsStored()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: Guid.Empty,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: null,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.AreEqual(Guid.Empty, entry.ScriptId);
        }

        [TestMethod]
        public void CommitDate_MinValue_IsStored()
        {
            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: DateTime.MinValue,
                Sequence: null,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: null,
                Tag: null);

            // Assert
            Assert.AreEqual(DateTime.MinValue, entry.CommitDate);
        }

        [TestMethod]
        public void ScriptText_LargeValue_IsStored()
        {
            // Arrange
            var largeScript = new string('X', 10000);

            // Act
            var entry = new ScriptRunLogEntry(
                BuildFileName: null,
                ScriptFileName: null,
                ScriptId: null,
                ScriptFileHash: null,
                CommitDate: null,
                Sequence: null,
                UserId: null,
                AllowScriptBlock: null,
                AllowBlockUpdateId: null,
                ScriptText: largeScript,
                Tag: null);

            // Assert
            Assert.AreEqual(10000, entry.ScriptText.Length);
        }

        [TestMethod]
        public void WithExpression_CreatesNewRecordWithModifiedValue()
        {
            // Arrange
            var originalEntry = new ScriptRunLogEntry(
                BuildFileName: "original.sbm",
                ScriptFileName: "script.sql",
                ScriptId: Guid.NewGuid(),
                ScriptFileHash: "HASH123",
                CommitDate: DateTime.UtcNow,
                Sequence: 1,
                UserId: "testuser",
                AllowScriptBlock: true,
                AllowBlockUpdateId: "admin",
                ScriptText: "SELECT 1",
                Tag: "v1.0");

            // Act - use with expression to modify BuildFileName
            var modifiedEntry = originalEntry with { BuildFileName = "modified.sbm" };

            // Assert
            Assert.AreEqual("original.sbm", originalEntry.BuildFileName);
            Assert.AreEqual("modified.sbm", modifiedEntry.BuildFileName);
            Assert.AreEqual(originalEntry.ScriptFileName, modifiedEntry.ScriptFileName);
        }
    }
}
