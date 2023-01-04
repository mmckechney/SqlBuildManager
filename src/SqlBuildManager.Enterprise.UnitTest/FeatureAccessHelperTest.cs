using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Feature;
using System.Collections.Generic;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for FeatureAccessHelperTest and is intended
    ///to contain all FeatureAccessHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FeatureAccessHelperTest
    {



        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_NoFeatureKey()
        {
            string featureKey = string.Empty;
            string loginId = "TestId";
            FeatureAccess[] accessCfg = new FeatureAccess[0];
            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IsFeatureEnabledTest_EmptyAccessCfgArray()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess[] accessCfg = new FeatureAccess[0];
            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_NullAccessConfig()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess[] accessCfg = null;
            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureDisabled()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = false;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledNoAccessSetting()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            bool expected = true;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_BadFeatureKey()
        {
            string featureKey = "MyBadKey";
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = FeatureKey.RemoteExecution;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledDenyUser()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            AccessSetting deny = new AccessSetting();
            deny.LoginId = loginId;
            cfg.Deny = new AccessSetting[] { deny };

            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledDenyGroup()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            AccessSetting deny = new AccessSetting();
            deny.GroupName = "EpicFailGroup";
            cfg.Deny = new AccessSetting[] { deny };

            List<string> adGroups = new List<string>();
            adGroups.Add("IrreleventGroup1");
            adGroups.Add("EpicFailGroup");
            adGroups.Add("IrreleventGroup2");

            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, adGroups, accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledDenyViaGroupAndId()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            AccessSetting deny = new AccessSetting();
            deny.GroupName = "EpicFailGroup";
            AccessSetting deny2 = new AccessSetting();
            deny2.LoginId = loginId;
            cfg.Deny = new AccessSetting[] { deny };

            List<string> adGroups = new List<string>();
            adGroups.Add("IrreleventGroup1");
            adGroups.Add("EpicFailGroup");
            adGroups.Add("IrreleventGroup2");

            bool expected = false;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, adGroups, accessCfg);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledAllowUser()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };
            AccessSetting allow = new AccessSetting();
            allow.LoginId = loginId;
            cfg.Allow = new AccessSetting[] { allow };

            bool expected = true;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, new List<string>(), accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledAllowGroup()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            AccessSetting allow = new AccessSetting();
            allow.GroupName = "EpicSuccessGroup";
            cfg.Allow = new AccessSetting[] { allow };

            List<string> adGroups = new List<string>();
            adGroups.Add("IrreleventGroup1");
            adGroups.Add("EpicSuccessGroup");
            adGroups.Add("IrreleventGroup2");

            bool expected = true;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, adGroups, accessCfg);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsFeatureEnabled
        ///</summary>
        [TestMethod()]
        public void IsFeatureEnabledTest_FeatureEnabledAllowViaGroupAndId()
        {
            string featureKey = FeatureKey.RemoteExecution;
            string loginId = "TestId";
            FeatureAccess cfg = new FeatureAccess();
            cfg.FeatureId = featureKey;
            cfg.Enabled = true;
            FeatureAccess[] accessCfg = new FeatureAccess[] { cfg };

            AccessSetting allow = new AccessSetting();
            allow.GroupName = "EpicSuccessGroup";
            AccessSetting allow2 = new AccessSetting();
            allow2.LoginId = loginId;
            cfg.Allow = new AccessSetting[] { allow, allow2 };

            List<string> adGroups = new List<string>();
            adGroups.Add("IrreleventGroup1");
            adGroups.Add("EpicSuccessGroup");
            adGroups.Add("IrreleventGroup2");

            bool expected = true;
            bool actual;
            actual = FeatureAccessHelper.IsFeatureEnabled(featureKey, loginId, adGroups, accessCfg);
            Assert.AreEqual(expected, actual);
        }
    }
}
