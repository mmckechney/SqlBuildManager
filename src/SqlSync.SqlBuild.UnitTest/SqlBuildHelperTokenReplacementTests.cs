using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Services;
using SqlSync.Connection;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildHelperTokenReplacementTests
    {
        [TestMethod]
        public void PerformScriptTokenReplacement_ReplacesAllTokens()
        {
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            helper.buildDescription = "desc";
            helper.buildPackageHash = "hash";
            helper.buildFileName = "file.sbm";

            var script = "#BuildDescription# #BuildPackageHash# #BuildFileName#";
            var result = helper.PerformScriptTokenReplacement(script);

            StringAssert.Contains(result, "desc");
            StringAssert.Contains(result, "hash");
            StringAssert.Contains(result, "file.sbm");
        }

        [TestMethod]
        public async Task ReplaceTokensAsync_DelegatesToHelper()
        {
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            helper.buildDescription = "desc";
            helper.buildPackageHash = "hash";
            helper.buildFileName = "file.sbm";
            var svc = new DefaultTokenReplacementService();

            var script = "#BuildDescription# #BuildPackageHash# #BuildFileName#";
            var result = await svc.ReplaceTokensAsync(script, helper);

            StringAssert.Contains(result, "desc");
            StringAssert.Contains(result, "hash");
            StringAssert.Contains(result, "file.sbm");
        }
    }
}
