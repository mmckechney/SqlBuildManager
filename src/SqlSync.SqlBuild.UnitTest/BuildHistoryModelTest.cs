using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class BuildHistoryModelTest
    {
        [TestMethod]
        public void AddScriptRunToHistory_AppendsToModelAndDataSet()
        {
            // Arrange
            var helper = new SqlBuildHelper(new SqlSync.Connection.ConnectionData());
            helper.buildHistoryXmlFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xml");

            var getNewBuildRow = typeof(SqlBuildHelper).GetMethod("GetNewBuildRow", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(getNewBuildRow, "GetNewBuildRow reflection lookup failed");
            var myBuild = (SqlSyncBuildData.BuildRow)getNewBuildRow.Invoke(helper, new object[] { "server" });

            var run = new ScriptRun(
                FileHash: "HASH",
                Results: "OK",
                FileName: "script.sql",
                RunOrder: 1,
                RunStart: DateTime.UtcNow,
                RunEnd: DateTime.UtcNow,
                Success: true,
                Database: "db1",
                ScriptRunId: Guid.NewGuid().ToString(),
                Build_Id: myBuild.Build_Id);

            var addScriptRun = typeof(SqlBuildHelper).GetMethod("AddScriptRunToHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(addScriptRun, "AddScriptRunToHistory reflection lookup failed");

            // Act
            addScriptRun.Invoke(helper, new object[] { run, myBuild });

            // Assert
            Assert.IsNotNull(helper.BuildHistoryModel);
            Assert.AreEqual(1, helper.BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual("script.sql", helper.BuildHistoryModel.ScriptRun[0].FileName);
            Assert.AreEqual(true, helper.BuildHistoryModel.ScriptRun[0].Success);
            Assert.IsNotNull(helper.buildHistoryData);
            Assert.AreEqual(1, helper.buildHistoryData.ScriptRun.Count);
            Assert.AreEqual("script.sql", helper.buildHistoryData.ScriptRun[0].FileName);
            Assert.AreEqual(true, helper.buildHistoryData.ScriptRun[0].Success);
        }
    }
}
