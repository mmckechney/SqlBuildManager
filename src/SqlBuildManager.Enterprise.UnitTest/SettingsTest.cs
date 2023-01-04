using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Properties;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for SettingsTest and is intended
    ///to contain all SettingsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SettingsTest
    {

        /// <summary>
        ///A test for Default
        ///</summary>
        [TestMethod()]
        public void DefaultTest()
        {
            System.Configuration.SettingsPropertyCollection actual;
            actual = Settings.Default.Properties;
            Assert.IsNotNull(actual);
        }
    }
}
