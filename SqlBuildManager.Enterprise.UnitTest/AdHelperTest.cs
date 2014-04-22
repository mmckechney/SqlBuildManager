using SqlBuildManager.Enterprise.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for AdHelperTest and is intended
    ///to contain all AdHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AdHelperTest
    {



        /// <summary>
        ///A test for AdHelper Constructor
        ///</summary>
        [TestMethod()]
        public void AdHelperConstructorTest()
        {
            AdHelper target = new AdHelper();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(AdHelper));
        }

        /// <summary>
        ///A test for GetDistinguishedName
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetDistinguishedNameTest_PassWithGoodDn()
        {
            string userName = "mmckechn";
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = AdHelper_Accessor.GetDistinguishedName(userName);
            Assert.IsTrue(actual.Length > 0, "Expected a DN back. If this test failed, check the userName value and make sure you are connected to a domain.");

        }
        /// <summary>
        ///A test for GetDistinguishedName
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetDistinguishedNameTest_PassWithNoDnReturned()
        {
            string userName = "ThisISJUnk";
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = AdHelper_Accessor.GetDistinguishedName(userName);
            Assert.IsTrue(actual.Length == 0, "No DN was expected a back.");

        }

        /// <summary>
        ///A test for GetGroupMemberships
        ///</summary>
        [TestMethod()]
        public void GetGroupMembershipsTest_PassWithGroupList()
        {
            string userName = "mmckechn";
            IList<string> actual;
            actual = AdHelper.GetGroupMemberships(userName);
            Assert.IsTrue(actual.Count > 0, "Expected groups to be returned. If this test failed, check the userName value and make sure you are connected to a domain.");
            
        }
        /// <summary>
        ///A test for GetGroupMemberships
        ///</summary>
        [TestMethod()]
        public void GetGroupMembershipsTest_PassWithNoGroupsInList()
        {
            string userName = "ThisISJUnk";
            IList<string> actual;
            actual = AdHelper.GetGroupMemberships(userName);
            Assert.IsTrue(actual.Count == 0, "No groups were expected to be returned");

        }
    }
}
