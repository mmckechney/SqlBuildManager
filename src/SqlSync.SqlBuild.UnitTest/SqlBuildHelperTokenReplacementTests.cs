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

        [TestMethod]
        public void ReplaceTokens_NoTokensPresent_ReturnsScriptUnchanged()
        {
            var svc = new DefaultTokenReplacementService();
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            helper.buildDescription = "desc";

            var script = "SELECT 1";
            var result = svc.ReplaceTokens(script, helper);
            Assert.AreEqual("SELECT 1", result);
        }

        [TestMethod]
        public void ReplaceTokens_CaseInsensitive()
        {
            var svc = new DefaultTokenReplacementService();
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            helper.buildDescription = "MyBuild";

            var script = "#builddescription#";
            var result = svc.ReplaceTokens(script, helper);
            Assert.AreEqual("MyBuild", result);
        }

        [TestMethod]
        public void ReplaceTokens_NullValues_ReplacesWithEmptyString()
        {
            var svc = new DefaultTokenReplacementService();
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            helper.buildDescription = null!;
            helper.buildPackageHash = null!;

            var script = "#BuildDescription# #BuildPackageHash#";
            var result = svc.ReplaceTokens(script, helper);
            Assert.AreEqual(" ", result);
        }

        [TestMethod]
        public void ReplaceTokens_NullBuildFileName_UsesDefault()
        {
            var svc = new DefaultTokenReplacementService();
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            helper.buildFileName = null!;

            var script = "#BuildFileName#";
            var result = svc.ReplaceTokens(script, helper);
            Assert.AreEqual("sbx file", result);
        }
    }
}
