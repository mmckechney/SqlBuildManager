using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.LocalContainer.IntegrationTest;

namespace SqlBuildManager.LocalContainer.PostgreSQL.IntegrationTest;

[TestClass]
public class LocalContainerPostgreSqlSmokeTest
{
    [TestMethod]
    public void LocalContainerPlatformIsPostgreSql()
    {
        Environment.SetEnvironmentVariable("SBM_TEST_PLATFORM", "postgresql");
        Assert.AreEqual("postgresql", LocalContainerTestEnvironment.Platform.ToLowerInvariant());
    }
}
