using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
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
    }
}
