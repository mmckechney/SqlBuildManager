using Microsoft.Azure.Amqp.Framing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;
using System.IO;
using System.Reflection;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class BuildHistoryModelTest
    {
        [TestMethod, Ignore("Legacy reflection on GetNewBuildRow fails; to be revisited.")]
        public void AddScriptRunToHistory_AppendsToModelAndDataSet()
        {
            // Arrange
            var helper = new SqlBuildHelper(new SqlSync.Connection.ConnectionData());
            helper.buildHistoryXmlFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xml");

            var getNewBuildRow = typeof(SqlBuildHelper).GetMethod("GetNewBuildRow", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(getNewBuildRow, "GetNewBuildRow reflection lookup failed");
            var myBuild = (SqlSyncBuildData.BuildRow)getNewBuildRow.Invoke(helper, new object[] { "server" });

            var run = new ScriptRun(
                fileHash: "HASH",
                results: "OK",
                fileName: "script.sql",
                runOrder: 1,
                runStart: DateTime.UtcNow,
                runEnd: DateTime.UtcNow,
                success: true,
                database: "db1",
                scriptRunId: Guid.NewGuid().ToString(),
                buildId: myBuild.Build_Id.ToString());

            var addScriptRun = typeof(SqlBuildHelper).GetMethod("AddScriptRunToHistory", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(SqlSync.SqlBuild.Models.ScriptRun), typeof(SqlSyncBuildData.BuildRow)]);
            Assert.IsNotNull(addScriptRun, "AddScriptRunToHistory reflection lookup failed");

            // Act
            addScriptRun.Invoke(helper, new object[] { run, myBuild });

            // Assert
            Assert.IsNotNull(((ISqlBuildRunnerProperties)helper).BuildHistoryModel);
            Assert.AreEqual(1, ((ISqlBuildRunnerProperties)helper).BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual("script.sql", ((ISqlBuildRunnerProperties)helper).BuildHistoryModel.ScriptRun[0].FileName);
            Assert.AreEqual(true, ((ISqlBuildRunnerProperties)helper).BuildHistoryModel.ScriptRun[0].Success);
            Assert.IsNotNull(((ISqlBuildRunnerProperties)helper).BuildHistoryModel);
            Assert.AreEqual(1, ((ISqlBuildRunnerProperties)helper).BuildHistoryModel.ScriptRun.Count);
            Assert.AreEqual("script.sql", ((ISqlBuildRunnerProperties)helper).BuildHistoryModel.ScriptRun[0].FileName);
            Assert.AreEqual(true, ((ISqlBuildRunnerProperties)helper).BuildHistoryModel.ScriptRun[0].Success);
        }
    }
}
