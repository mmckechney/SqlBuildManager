using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.LocalContainer.IntegrationTest;

namespace SqlBuildManager.LocalContainer.MySql.IntegrationTest;

[TestClass]
public class LocalContainerRuntimeContainerSmokeTest
{
    [TestMethod]
    [TestCategory("RuntimeContainer")]
    public async Task ProductionRuntimeImage_SbmVersion_Succeeds()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SBM_RUNTIME_IMAGE")))
        {
            Assert.Inconclusive("Runtime-container environment is not configured.");
        }

        var result = await RuntimeContainerClient.RunVersionAsync();
        Assert.AreEqual(0, result.ExitCode, result.Output);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Output), "The runtime image produced no output.");
    }
}
