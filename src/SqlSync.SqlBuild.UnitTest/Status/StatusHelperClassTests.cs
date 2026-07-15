using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.Connection;
using SqlSync.DbInformation.ChangeDates;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlSync.SqlBuild.UnitTest.Status
{
    /// <summary>
    /// Unit tests for StatusHelper class.
    /// Note: DetermineScriptRunStatus and SetScriptRunStatusAndDates methods require database connectivity
    /// and are tested in integration tests. These tests focus on the class structure and constants.
    /// </summary>
    [TestClass]
    public class StatusHelperClassTests
    {
        #region Class Structure Tests

        [TestMethod]
        public void StatusHelper_ClassExists_IsPublic()
        {
            // Arrange & Act
            var type = typeof(StatusHelper);

            // Assert
            Assert.IsNotNull(type);
            Assert.IsTrue(type.IsPublic);
            Assert.IsTrue(type.IsClass);
        }

        [TestMethod]
        public void StatusHelper_DetermineScriptRunStatus_MethodExists()
        {
            // Arrange
            var type = typeof(StatusHelper);

            // Act
            var method = type.GetMethod("DetermineScriptRunStatus");

            // Assert
            Assert.IsNotNull(method, "DetermineScriptRunStatus method should exist");
            Assert.IsTrue(method.IsStatic, "DetermineScriptRunStatus should be static");
            Assert.IsTrue(method.IsPublic, "DetermineScriptRunStatus should be public");
        }

        [TestMethod]
        public void StatusHelper_SetScriptRunStatusAndDates_MethodExists()
        {
            // Arrange
            var type = typeof(StatusHelper);

            // Act
            var method = type.GetMethod("SetScriptRunStatusAndDates");

            // Assert
            Assert.IsNotNull(method, "SetScriptRunStatusAndDates method should exist");
            Assert.IsTrue(method.IsStatic, "SetScriptRunStatusAndDates should be static");
            Assert.IsTrue(method.IsPublic, "SetScriptRunStatusAndDates should be public");
        }

        [TestMethod]
        public void StatusHelper_DetermineScriptRunStatus_ReturnsScriptStatusType()
        {
            // Arrange
            var type = typeof(StatusHelper);
            var method = type.GetMethod("DetermineScriptRunStatus");

            // Act & Assert
            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(ScriptStatusType), method.ReturnType);
        }

        [TestMethod]
        public void StatusHelper_SetScriptRunStatusAndDates_ReturnsVoid()
        {
            // Arrange
            var type = typeof(StatusHelper);
            var method = type.GetMethod("SetScriptRunStatusAndDates");

            // Act & Assert
            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(void), method.ReturnType);
        }

        [TestMethod]
        public void StatusHelper_DetermineScriptRunStatus_HasCorrectParameterCount()
        {
            // Arrange
            var type = typeof(StatusHelper);
            var method = type.GetMethod("DetermineScriptRunStatus");

            // Act
            var parameters = method!.GetParameters();
            Assert.AreEqual(8, parameters.Length);
        }

        [TestMethod]
        public void StatusHelper_SetScriptRunStatusAndDates_HasCorrectParameterCount()
        {
            // Arrange
            var type = typeof(StatusHelper);
            var method = type.GetMethod("SetScriptRunStatusAndDates");

            // Act
            var parameters = method!.GetParameters();
            Assert.AreEqual(4, parameters.Length);
        }

        [TestMethod]
        public void SetScriptRunStatusAndDates_BatchesPerDatabaseAndKeepsStatusesIsolated()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            List<DatabaseOverride> originalOverrides = OverrideData.TargetDatabaseOverrides;
            Directory.CreateDirectory(tempDirectory);
            try
            {
                string firstFile = Path.Combine(tempDirectory, "first.sql");
                string secondFile = Path.Combine(tempDirectory, "second.sql");
                File.WriteAllText(firstFile, "SELECT 1");
                File.WriteAllText(secondFile, "SELECT 2");

                SqlBuildFileHelper.GetSHA1Hash(firstFile, out string firstHash, out _, false);
                Guid sharedScriptId = Guid.NewGuid();
                var scripts = new List<Script>
                {
                    new Script { ScriptId = sharedScriptId.ToString(), FileName = "first.sql", Database = "db-one", AllowMultipleRuns = false },
                    new Script { ScriptId = sharedScriptId.ToString(), FileName = "second.sql", Database = "db-two", AllowMultipleRuns = false }
                };
                var model = new SqlSyncBuildDataModel(
                    new List<SqlSyncBuildProject>(),
                    scripts,
                    new List<Build>(),
                    new List<ScriptRun>(),
                    new List<CommittedScript>());
                var connectionData = new ConnectionData("status-server", "default-db");
                OverrideData.TargetDatabaseOverrides = new List<DatabaseOverride>();
                DatabaseObjectChangeDates.Servers["status-server"]["db-one"].LastRefreshTime = DateTime.Now;
                DatabaseObjectChangeDates.Servers["status-server"]["db-two"].LastRefreshTime = DateTime.Now;

                var databaseUtility = new Mock<IDatabaseUtility>(MockBehavior.Strict);
                databaseUtility
                    .Setup(utility => utility.GetBatchBlockingSqlLog(
                        It.Is<IReadOnlyList<Guid>>(ids => ids.Count == 1 && ids[0] == sharedScriptId),
                        connectionData,
                        "db-one"))
                    .Returns(new Dictionary<Guid, SqlLogStatus>
                    {
                        [sharedScriptId] = new SqlLogStatus(true, firstHash, string.Empty, DateTime.UtcNow)
                    });
                databaseUtility
                    .Setup(utility => utility.GetBatchBlockingSqlLog(
                        It.Is<IReadOnlyList<Guid>>(ids => ids.Count == 1 && ids[0] == sharedScriptId),
                        connectionData,
                        "db-two"))
                    .Returns(new Dictionary<Guid, SqlLogStatus>());

                StatusHelper.SetScriptRunStatusAndDates(
                    model, databaseUtility.Object, connectionData, tempDirectory);

                Assert.AreEqual(ScriptStatusType.Locked, scripts[0].ScriptRunStatus);
                Assert.AreEqual(ScriptStatusType.NotRun, scripts[1].ScriptRunStatus);
                databaseUtility.VerifyAll();
                databaseUtility.Verify(utility => utility.HasBlockingSqlLog(
                    It.IsAny<Guid>(), It.IsAny<ConnectionData>(), It.IsAny<string>(),
                    out It.Ref<string>.IsAny, out It.Ref<string>.IsAny, out It.Ref<DateTime>.IsAny), Times.Never);
            }
            finally
            {
                OverrideData.TargetDatabaseOverrides = originalOverrides;
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        #endregion

        #region ScriptStatusType Related Tests

        [TestMethod]
        public void ScriptStatusType_FileMissing_ValueIs6()
        {
            // Assert
            Assert.AreEqual(6, (int)ScriptStatusType.FileMissing);
        }

        [TestMethod]
        public void ScriptStatusType_NotRun_ValueIs0()
        {
            // Assert
            Assert.AreEqual(0, (int)ScriptStatusType.NotRun);
        }

        [TestMethod]
        public void ScriptStatusType_Locked_ValueIs1()
        {
            // Assert
            Assert.AreEqual(1, (int)ScriptStatusType.Locked);
        }

        [TestMethod]
        public void ScriptStatusType_UpToDate_ValueIs2()
        {
            // Assert
            Assert.AreEqual(2, (int)ScriptStatusType.UpToDate);
        }

        [TestMethod]
        public void ScriptStatusType_ChangedSinceCommit_ValueIs3()
        {
            // Assert
            Assert.AreEqual(3, (int)ScriptStatusType.ChangedSinceCommit);
        }

        [TestMethod]
        public void ScriptStatusType_ServerChange_ValueIs4()
        {
            // Assert
            Assert.AreEqual(4, (int)ScriptStatusType.ServerChange);
        }

        [TestMethod]
        public void ScriptStatusType_NotRunButOlderVersion_ValueIs5()
        {
            // Assert
            Assert.AreEqual(5, (int)ScriptStatusType.NotRunButOlderVersion);
        }

        [TestMethod]
        public void ScriptStatusType_Unknown_ValueIs99()
        {
            // Assert
            Assert.AreEqual(99, (int)ScriptStatusType.Unknown);
        }

        #endregion
    }
}
