using SqlBuildManager.Enterprise;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for EnterpriseConfigHelperTest and is intended
    ///to contain all EnterpriseConfigHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnterpriseConfigHelperTest
    {

        /// <summary>
        ///A test for EnterpriseConfig
        ///</summary>
        [TestMethod()]
        public void EnterpriseConfigTest_NullConfig()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null; //force a re-read.
            EnterpriseConfiguration actual;
            actual = EnterpriseConfigHelper.EnterpriseConfig;
            Assert.IsNotNull(actual, "The Enterprise config object should have at least a default object value");

        }

        /// <summary>
        ///A test for LoadEnterpriseConfiguration
        ///</summary>
        [TestMethod()]
        public void LoadEnterpriseConfigurationTest_WithConfigPath()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null; //force a re-read.
            string configPath = System.Configuration.ConfigurationManager.AppSettings["Enterprise.ConfigFileLocation"];
            configPath = Path.GetFullPath(configPath);
            EnterpriseConfiguration actual;
            actual = EnterpriseConfigHelper.LoadEnterpriseConfiguration(configPath);
            Assert.AreEqual(1, actual.TableWatch.Length);
            Assert.AreEqual(2, actual.TableWatch[0].Notify.Length);
            Assert.AreEqual(2, actual.TableWatch[0].Table.Length);
        }

        /// <summary>
        ///A test for LoadEnterpriseConfiguration
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void LoadEnterpriseConfigurationTest_NoParameter()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null; //force a re-read.
            EnterpriseConfiguration actual;
            actual = EnterpriseConfigHelper.LoadEnterpriseConfiguration();
            Assert.AreEqual(1, actual.TableWatch.Length);
            Assert.AreEqual(2, actual.TableWatch[0].Notify.Length);
            Assert.AreEqual(2, actual.TableWatch[0].Table.Length);
        }

        /// <summary>
        ///A test for DeserializeConfiguration
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void DeserializeConfigurationTest()
        {
            string configuration = Properties.Resources.EnterpriseConfig;
            EnterpriseConfiguration actual;
            actual = EnterpriseConfigHelper.DeserializeConfiguration(configuration);
            Assert.AreEqual(1, actual.TableWatch.Length);
            Assert.AreEqual(2, actual.TableWatch[0].Notify.Length);
            Assert.AreEqual(2, actual.TableWatch[0].Table.Length);

        }

        /// <summary>
        ///A test for DeserializeConfiguration
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void DeserializeConfigurationTest_BadConfig()
        {
            string configuration = "This is a bad configuration";
            EnterpriseConfiguration actual;
            actual = EnterpriseConfigHelper.DeserializeConfiguration(configuration);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for EnterpriseConfigHelper Constructor
        ///</summary>
        [TestMethod()]
        public void EnterpriseConfigHelperConstructorTest()
        {
            EnterpriseConfigHelper target = new EnterpriseConfigHelper();
            Assert.IsNotNull(target);
        }
    }
}
