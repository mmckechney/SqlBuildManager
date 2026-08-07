using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.LocalContainer.IntegrationTest;

namespace SqlBuildManager.LocalContainer.MySql.IntegrationTest;

[TestClass]
public class LocalContainerMySqlSmokeTest
{
    [TestMethod]
    public void LocalContainerPlatformIsMySql()
    {
        Environment.SetEnvironmentVariable("SBM_TEST_PLATFORM", "mysql");
        Assert.AreEqual("mysql", LocalContainerTestEnvironment.Platform.ToLowerInvariant());
    }
}
