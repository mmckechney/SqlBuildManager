using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.LocalContainer.IntegrationTest;

namespace SqlBuildManager.LocalContainer.SqlServer.IntegrationTest;

[TestClass]
public class LocalContainerSqlServerSmokeTest
{
    [TestMethod]
    public void LocalContainerPlatformIsSqlServer()
    {
        Environment.SetEnvironmentVariable("SBM_TEST_PLATFORM", "sqlserver");
        Assert.AreEqual("sqlserver", LocalContainerTestEnvironment.Platform.ToLowerInvariant());
    }
}
