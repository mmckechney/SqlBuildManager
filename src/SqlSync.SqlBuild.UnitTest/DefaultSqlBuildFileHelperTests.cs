using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class DefaultSqlBuildFileHelperTests
    {
        [TestMethod]
        public void GetSHA1Hash_MatchesStatic()
        {
            var helper = new DefaultSqlBuildFileHelper();
            var scripts = new[] { "SELECT 1", "SELECT 2" };

            helper.GetSHA1Hash(scripts, out var viaDefault);
            SqlBuildFileHelper.GetSHA1Hash(scripts, out var viaStatic);

            Assert.AreEqual(viaStatic, viaDefault);
        }

        [TestMethod]
        public void JoinBatchedScripts_MatchesStatic()
        {
            var helper = new DefaultSqlBuildFileHelper();
            var scripts = new[] { "SELECT 1", "SELECT 2" };

            var viaDefault = helper.JoinBatchedScripts(scripts);
            var viaStatic = SqlBuildFileHelper.JoinBatchedScripts(scripts);

            Assert.AreEqual(viaStatic, viaDefault);
        }
    }
}
