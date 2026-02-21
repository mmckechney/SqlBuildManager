using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class BuildHistoryTrackerTests
    {
        [TestMethod]
        public void Constructor_CreatesEmptyModel()
        {
            // Arrange & Act
            var tracker = new DefaultBuildHistoryTracker();

            // Assert
            Assert.IsNotNull(tracker.BuildHistoryModel);
            Assert.AreEqual(0, tracker.BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual(0, tracker.BuildHistoryModel.Build.Count);
        }

        [TestMethod]
        public void AddScriptRunToHistory_AddsRunToModel()
        {
            // Arrange
            var tracker = new DefaultBuildHistoryTracker();
            var run = new ScriptRun(
                fileHash: "HASH123",
                results: "OK",
                fileName: "test.sql",
                runOrder: 1,
                runStart: DateTime.UtcNow,
                runEnd: DateTime.UtcNow,
                success: true,
                database: "TestDb",
                scriptRunId: Guid.NewGuid().ToString(),
                buildId: Guid.NewGuid().ToString());
            var build = new Build(
                name: "TestBuild",
                buildType: "Test",
                buildStart: DateTime.UtcNow,
                buildEnd: DateTime.UtcNow,
                serverName: "TestServer",
                finalStatus: BuildItemStatus.Committed,
                buildId: Guid.NewGuid().ToString(),
                userId: "TestUser");

            // Act
            tracker.AddScriptRunToHistory(run, build);

            // Assert
            Assert.AreEqual(1, tracker.BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual("test.sql", tracker.BuildHistoryModel.ScriptRun[0].FileName);
            Assert.AreEqual(true, tracker.BuildHistoryModel.ScriptRun[0].Success);
            Assert.AreEqual(1, tracker.BuildHistoryModel.Build.Count);
        }

        [TestMethod]
        public void AddScriptRunToHistory_WithNullRun_DoesNothing()
        {
            // Arrange
            var tracker = new DefaultBuildHistoryTracker();
            var build = new Build(
                name: "TestBuild",
                buildType: "Test",
                buildStart: DateTime.UtcNow,
                buildEnd: DateTime.UtcNow,
                serverName: "TestServer",
                finalStatus: BuildItemStatus.Committed,
                buildId: Guid.NewGuid().ToString(),
                userId: "TestUser");

            // Act
            tracker.AddScriptRunToHistory(null!, build);

            // Assert
            Assert.AreEqual(0, tracker.BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual(0, tracker.BuildHistoryModel.Build.Count);
        }

        [TestMethod]
        public void Reset_ClearsTheModel()
        {
            // Arrange
            var tracker = new DefaultBuildHistoryTracker();
            var run = new ScriptRun(
                fileHash: "HASH123",
                results: "OK",
                fileName: "test.sql",
                runOrder: 1,
                runStart: DateTime.UtcNow,
                runEnd: DateTime.UtcNow,
                success: true,
                database: "TestDb",
                scriptRunId: Guid.NewGuid().ToString(),
                buildId: Guid.NewGuid().ToString());
            var build = new Build(
                name: "TestBuild",
                buildType: "Test",
                buildStart: DateTime.UtcNow,
                buildEnd: DateTime.UtcNow,
                serverName: "TestServer",
                finalStatus: BuildItemStatus.Committed,
                buildId: Guid.NewGuid().ToString(),
                userId: "TestUser");
            tracker.AddScriptRunToHistory(run, build);
            Assert.AreEqual(1, tracker.BuildHistoryModel.ScriptRun.Count);

            // Act
            tracker.Reset();

            // Assert
            Assert.AreEqual(0, tracker.BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual(0, tracker.BuildHistoryModel.Build.Count);
        }

        [TestMethod]
        public void BuildHistoryModel_ReturnsCurrentModel()
        {
            // Arrange
            var tracker = new DefaultBuildHistoryTracker();

            // Act
            var model = tracker.BuildHistoryModel;

            // Assert
            Assert.IsNotNull(model);
            Assert.IsInstanceOfType(model, typeof(SqlSyncBuildDataModel));
        }
    }
}
