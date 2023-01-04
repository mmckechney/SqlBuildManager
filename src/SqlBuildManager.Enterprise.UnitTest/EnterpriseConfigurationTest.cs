using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for EnterpriseConfigurationTest and is intended
    ///to contain all EnterpriseConfigurationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnterpriseConfigurationTest
    {





        /// <summary>
        ///A test for FeatureAccess
        ///</summary>
        [TestMethod()]
        public void FeatureAccessTest_GetAndSet()
        {
            EnterpriseConfiguration target = new EnterpriseConfiguration();
            FeatureAccess acc = new FeatureAccess();
            acc.FeatureId = "TestID";
            acc.Enabled = true;

            FeatureAccess[] expected = new FeatureAccess[] { acc };
            FeatureAccess[] actual;
            target.FeatureAccess = expected;
            actual = target.FeatureAccess;
            Assert.AreEqual(expected, actual);
        }
    }
}
