using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ViewAlterPolicyTest and is intended
    ///to contain all ViewAlterPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ViewAlterPolicyTest
    {



        /// <summary>
        ///A test for ViewAlterPolicy Constructor
        ///</summary>
        [TestMethod()]
        public void ViewAlterPolicyConstructorTest()
        {
            ViewAlterPolicy target = new ViewAlterPolicy();
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_NoAlterViewStatement()
        {
            ViewAlterPolicy target = new ViewAlterPolicy();
            string script = "SELECT column from dbo.Table WITH (NOLOCK)";
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
        public void CheckPolicyTest_WIthAlterViewStatement()
        {
            ViewAlterPolicy target = new ViewAlterPolicy();
            string script = @"Alter View vw_MyView
            AS
                SELECT column from dbo.table";
            string message = string.Empty;
            string messageExpected = "An \"ALTER VIEW\" was found. Please make sure that no indexes were dropped by SQL Server in the process." +
                    "\r\nIf you have validated that no indexes were dropped, you can add a [No Indexes] tag to suppress this message.";

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
        public void CheckPolicyTest_WithAlterViewStatement_MultiLine()
        {
            ViewAlterPolicy target = new ViewAlterPolicy();
            string script = @"ALTER 
                VIEW
                    vw_MyView
            AS
                SELECT column from dbo.table";
            string message = string.Empty;
            string messageExpected = "An \"ALTER VIEW\" was found. Please make sure that no indexes were dropped by SQL Server in the process." +
                    "\r\nIf you have validated that no indexes were dropped, you can add a [No Indexes] tag to suppress this message.";

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
        public void CheckPolicyTest_AlterViewStatementWithTag()
        {
            ViewAlterPolicy target = new ViewAlterPolicy();
            string script = @"Alter View vw_MyView
            AS
                SELECT column from dbo.table
//[No Indexes]";
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
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            ViewAlterPolicy target = new ViewAlterPolicy(); 
            string actual;
            actual = target.LongDescription;
            string expected = "Creates a reminder to check for indexes dropped by SQL Server in the ALTER process.";
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            ViewAlterPolicy target = new ViewAlterPolicy();
            string actual;
            actual = target.PolicyId;
            Assert.AreEqual(PolicyIdKey.ViewAlterPolicy, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            ViewAlterPolicy target = new ViewAlterPolicy(); 
            string actual;
            actual = target.ShortDescription;
            string expected = "Alter View Reminder";
            Assert.AreEqual(expected, actual);
 
        }
    }
}
