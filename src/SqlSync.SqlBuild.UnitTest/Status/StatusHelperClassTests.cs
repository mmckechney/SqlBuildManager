using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Status;
using System;

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
            var parameters = method.GetParameters();

            // Assert - should have 8 parameters including 2 out parameters
            Assert.AreEqual(8, parameters.Length);
        }

        [TestMethod]
        public void StatusHelper_SetScriptRunStatusAndDates_HasCorrectParameterCount()
        {
            // Arrange
            var type = typeof(StatusHelper);
            var method = type.GetMethod("SetScriptRunStatusAndDates");

            // Act
            var parameters = method.GetParameters();

            // Assert - buildData (ref), dbUtil, connData, projectFilePath
            Assert.AreEqual(4, parameters.Length);
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
