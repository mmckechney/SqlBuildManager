using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.SqlBuild.Services;
namespace SqlBuildManager.SqlBuild.UnitTest
{
    [TestClass]
    public class DefaultSqlBuildFileHelperTests
    {
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
