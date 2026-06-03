using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Assembly-wide test setup. The dependent tests run against a local SQL Express instance
    /// using a self-signed certificate. Production seeds <see cref="ConnectionHelper.TrustServerCertificate"/>
    /// from the operator's <c>--trustservercertificate</c> choice at command startup; the tests
    /// mirror that opt-in here so connection paths that rebuild connections from individual fields
    /// (rather than carrying a <see cref="ConnectionData"/>) can connect.
    /// </summary>
    [TestClass]
    public static class AssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            ConnectionHelper.TrustServerCertificate = true;
        }
    }
}
