using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for GrantExecutToPublicPolicyTest and is intended
    ///to contain all GrantExecutToPublicPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GrantExecuteToPublicPolicyTest
    {


        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            GrantExecuteToPublicPolicy target = new GrantExecuteToPublicPolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "GrantExecuteToPublicPolicy";
            Assert.AreEqual(actual, expected);

        }
        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            GrantExecuteToPublicPolicy target = new GrantExecuteToPublicPolicy(); 
            string actual;
            actual = target.ShortDescription;
            Assert.AreEqual("Check for GRANT .. TO [public]", actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            GrantExecuteToPublicPolicy target = new GrantExecuteToPublicPolicy(); 
            string actual;
            actual = target.LongDescription;
            Assert.AreEqual("Checks that scripts do not GRANT any privileges to the [public] group", actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_NoPublicGrant()
        {
            GrantExecuteToPublicPolicy target = new GrantExecuteToPublicPolicy();
            string script = UnitTest.Properties.Resources.NonPublicGrant;
            string message = string.Empty;
            string messageExpected = string.Empty; 
            bool expected = true; 
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);

            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_PublicGrant()
        {
            GrantExecuteToPublicPolicy target = new GrantExecuteToPublicPolicy();
            string script = UnitTest.Properties.Resources.PublicGrant;
            string message = string.Empty;
            string messageExpected = "Script contains a GRANT statement to the [public] group";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_PublicGrantNoBracket()
        {
            GrantExecuteToPublicPolicy target = new GrantExecuteToPublicPolicy();
            string script = UnitTest.Properties.Resources.PublicGrant_nobracket;
            string message = string.Empty;
            string messageExpected = "Script contains a GRANT statement to the [public] group";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        
    }
}
