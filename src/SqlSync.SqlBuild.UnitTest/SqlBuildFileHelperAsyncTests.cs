using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildFileHelperAsyncTests
    {
        [TestMethod]
        public async Task GetSHA1HashAsync_MatchesSyncBehavior()
        {
            var tmp = Path.GetTempFileName();
            try
            {
                var content = "SELECT 1;\nGO\nSELECT 2;";
                await File.WriteAllTextAsync(tmp, content);

                SqlBuildFileHelper.GetSHA1Hash(tmp, out var fileHashSync, out var textHashSync, stripTransactions: false);
                var (fileHashAsync, textHashAsync) = await SqlBuildFileHelper.GetSHA1HashAsync(tmp, stripTransactions: false);

                Assert.AreEqual(fileHashSync, fileHashAsync);
                Assert.AreEqual(textHashSync, textHashAsync);
            }
            finally
            {
                File.Delete(tmp);
            }
        }
    }
}
