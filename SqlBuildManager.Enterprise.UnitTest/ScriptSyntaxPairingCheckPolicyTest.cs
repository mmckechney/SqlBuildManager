using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Collections.Generic;

namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ScriptSyntaxPairingCheckPolicyTest and is intended
    ///to contain all ScriptSyntaxPairingCheckPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptSyntaxPairingCheckPolicyTest
    {

        /// <summary>
        ///A test for ScriptSyntaxPairingCheckPolicy Constructor
        ///</summary>
        [TestMethod()]
        public void ScriptSyntaxPairingCheckPolicyConstructorTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ScriptSyntaxPairingCheckPolicy));
 
        }

        #region CheckPolicyTest - standard

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_MissingParentRegexValue()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "BadName", Value = "" });
            string script = string.Empty; 
            string message = string.Empty;
            string messageExpected = String.Format("The ScriptSyntaxPairingCheckPolicy \"{0}\" does not have a {1} argument/value. Unable to process policy.", target.ShortDescription, ScriptSyntaxPairingCheckPolicy.parentRegexKey); ; 
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
        public void CheckPolicyTest_MissingChildRegexValue()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            string script = string.Empty;
            string message = string.Empty;
            string messageExpected = String.Format("The ScriptSyntaxPairingCheckPolicy \"{0}\" does not have any {1} or {2} argument/values. Unable to process policy.", target.ShortDescription, ScriptSyntaxPairingCheckPolicy.childPairRegexKey, ScriptSyntaxPairingCheckPolicy.childDontPairRegexKey);
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
        public void CheckPolicyTest_DoubleParentRegexValue()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "FIRST" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "SECOND" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" });
            string script = "This is to make sure the second Parentregex is the one that is used. Need to add child to get it to pass!";
            bool expected = true;
            string message = string.Empty;
            string messageExpected = string.Empty;
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
        public void CheckPolicyTest_NoParentMatchPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" });
            string script = "This script does not have a regex match";
            string message = string.Empty;
            string messageExpected = "No parent match found. Policy passes.";
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
        public void CheckPolicyTest_ParentAndChildPairMatchPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" });
            string script = "This script does have a parent and a child regex match";
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
        public void CheckPolicyTest_ParentInCommentAndChildMatchPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" });
            string script = @"This script does have a 
-- parent but it is in a comment
and a child regex match";
            string message = string.Empty;
            string messageExpected = "No uncommented parent match found. Policy passes.";
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
        public void CheckPolicyTest_ParentAndChildInCommentMatchFail()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" , FailureMessage=""});
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" ,FailureMessage="" });
            string script = @"This script does have a 
 parent with a 
-- a child regex match in a comment
";
            string message = string.Empty;
            string messageExpected = String.Format("No child pair found with regular expression {0}", "CHILD");
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
        public void CheckPolicyTest_ParentAndChildInCommentMatchFail_WithFailureText()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD", FailureMessage = "This is my failure text" });
            string script = @"This script does have a 
 parent with a 
-- a child regex match in a comment
";
            string message = string.Empty;
            string messageExpected = "This is my failure text";
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
        public void CheckPolicyTest_ParentAndDoubleChildPairMatchPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "SCRIPT" });
            string script = "This script does have a parent and a child regex match";
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
        public void CheckPolicyTest_ParentAndChildPairMatchAndChildDontPairPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildDontPairRegex", Value = "NEVER" });
            string script = "This script does have a parent and a child regex match";
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
        public void CheckPolicyTest_ParentAndChildPairMatchFail()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT"});
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "CHILD", FailureMessage = "" });
            string script = "This script does have a parent and a chld regex match";
            string message = string.Empty;
            string messageExpected = String.Format("No child pair found with regular expression {0}", "CHILD");
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
        public void CheckPolicyTest_ParentAndChildDontPairPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildDontPairRegex", Value = "NEVER" });
            string script = "This script does have a parent and is missing the a child dont match regex match";
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
        public void CheckPolicyTest_ParentAndDoubleChildDontPairPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildDontPairRegex", Value = "NEVER" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildDontPairRegex", Value = "NEVERAGAIN" });
            string script = "This script does have a parent and is missing the a child dont match regex match";
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
        public void CheckPolicyTest_ParentAndChildDontPairFail()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildDontPairRegex", Value = "CHILD", FailureMessage = "" });
            string script = "This script does have a parent and also has a child dont match regex match. Fail!";
            string message = string.Empty;
            string messageExpected = String.Format("A child match was found for regular expression \"{0}\". This script pairing is not allowed.", "CHILD");
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
        public void CheckPolicyTest_ParentAndChildDontPairFail_WithFailureText()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "PARENT", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildDontPairRegex", Value = "CHILD", FailureMessage = "This is my failure message" });
            string script = "This script does have a parent and also has a child dont match regex match. Fail!";
            string message = string.Empty;
            string messageExpected = "This is my failure message";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region CheckPolicyTest - expanded
        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_ParentAndChildPairMatchAddPrimaryKeyPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bALTER\b\s*\bTABLE\b.*\bADD\b\s*\bCONSTRAINT\b.*\bPRIMARY\b\s*\bKEY\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b" });
            string script = @"ALTER TABLE [dbo].[MyTable] ADD  CONSTRAINT [PK_MyTable] PRIMARY KEY CLUSTERED 
(
      [MyTableID] ASC
)WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
;
";
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
        public void CheckPolicyTest_ParentAndChildPairMatchAddPrimaryKeyFail()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bALTER\b\s*\bTABLE\b.*\bADD\b\s*\bCONSTRAINT\b.*\bPRIMARY\b\s*\bKEY\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b", FailureMessage = "" });
            string script = @"ALTER TABLE [dbo].[MyTable] ADD  CONSTRAINT [PK_MyTable] PRIMARY KEY CLUSTERED 
(
      [MyTableID] ASC
)WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
;
";
            string message = string.Empty;
            string messageExpected = String.Format("No child pair found with regular expression {0}", @"\bONLINE\b\s*=\s*\bON\b");
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
        public void CheckPolicyTest_ParentAndChildPairMatchAddPrimaryKeyFail_CheckFailureMessage()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bALTER\b\s*\bTABLE\b.*\bADD\b\s*\bCONSTRAINT\b.*\bPRIMARY\b\s*\bKEY\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b", FailureMessage= @"PRIMARY KEYS require a STATISTICS_NORECOMPUTE = ON setting" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b" });
            string script = @"ALTER TABLE [dbo].[MyTable] ADD  CONSTRAINT [PK_MyTable] PRIMARY KEY CLUSTERED 
(
      [MyTableID] ASC
)WITH (PAD_INDEX  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
;
";
            string message = string.Empty;
            string messageExpected =  @"PRIMARY KEYS require a STATISTICS_NORECOMPUTE = ON setting";
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
        public void CheckPolicyTest_ParentAndChildPairMatchAddPrimaryKey_NoTargetMatchPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "TargetDatabase", Value = @"MyTarget" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bALTER\b\s*\bTABLE\b.*\bADD\b\s*\bCONSTRAINT\b.*\bPRIMARY\b\s*\bKEY\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b", FailureMessage = @"PRIMARY KEYS require a STATISTICS_NORECOMPUTE = ON setting" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b" });
            string script = @"ALTER TABLE [dbo].[MyTable] ADD  CONSTRAINT [PK_MyTable] PRIMARY KEY CLUSTERED 
(
      [MyTableID] ASC
)WITH (PAD_INDEX  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
;
";
            string message = string.Empty;
            string messageExpected = String.Format("Script target database {0} does not match policy target database {1}. Policy passes.", "NotMyTarget", "MyTarget");
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, "NotMyTarget", commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_ParentAndChildPairMatchAddPrimaryKey_TargetMatchFail()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "TargetDatabase", Value = @"MyTarget" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bALTER\b\s*\bTABLE\b.*\bADD\b\s*\bCONSTRAINT\b.*\bPRIMARY\b\s*\bKEY\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b", FailureMessage = @"PRIMARY KEYS require a STATISTICS_NORECOMPUTE = ON setting" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b" });
            string script = @"ALTER TABLE [dbo].[MyTable] ADD  CONSTRAINT [PK_MyTable] PRIMARY KEY CLUSTERED 
(
      [MyTableID] ASC
)WITH (PAD_INDEX  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
;
";
            string message = string.Empty;
            string messageExpected =  @"PRIMARY KEYS require a STATISTICS_NORECOMPUTE = ON setting";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, "MyTarget", commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_ParentAndChildPairMatchCreateIndexPass()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bCREATE\b.*\bINDEX\b.*\bON" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b" });
            string script = @"CREATE NONCLUSTERED INDEX [mytable_index] ON [dbo].[MyTable] 
(
      [ColumnX] ASC
)
INCLUDE ( [ColumnX],
[ColumnY],
[ColumnZ]) WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE=ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

";
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
        public void CheckPolicyTest_ParentAndChildPairMatchCreateIndexFail()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bCREATE\b.*\bINDEX\b.*\bON", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b", FailureMessage = "" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b", FailureMessage = "" });
            string script = @"CREATE NONCLUSTERED INDEX [mytable_index] ON [dbo].[MyTable] 
(
      [ColumnX] ASC
)
INCLUDE ( [ColumnX],
[ColumnY],
[ColumnZ]) WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

";
            string message = string.Empty;
            string messageExpected = String.Format("No child pair found with regular expression {0}", @"\bFILLFACTOR\b\s*=\s*\b90\b");
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
        public void CheckPolicyTest_ParentAndChildPairMatchCreateIndexFail_CheckFailureMessage()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.ShortDescription = "My Short Desc";
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = @"\bCREATE\b.*\bINDEX\b.*\bON" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bPAD_INDEX\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSTATISTICS_NORECOMPUTE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bSORT_IN_TEMPDB\b\s*=\s*\bOFF\b" , FailureMessage = @"INDEXES require a SORT_IN_TEMPDB = OFF setting"});
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bONLINE\b\s*=\s*\bON\b" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = @"\bFILLFACTOR\b\s*=\s*\b90\b" });
            string script = @"CREATE NONCLUSTERED INDEX [mytable_index] ON [dbo].[MyTable] 
(
      [ColumnX] ASC
)
INCLUDE ( [ColumnX],
[ColumnY],
[ColumnZ]) WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE  = ON, SORT_IN_TEMPDB = ON, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

";
            string message = string.Empty;
            string messageExpected = @"INDEXES require a SORT_IN_TEMPDB = OFF setting";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        #endregion


        /// <summary>
        ///A test for Arguments
        ///</summary>
        [TestMethod()]
        public void ArgumentsTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ParentRegex", Value = "HI" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "ChildPairRegex", Value = "There" });

            Assert.AreEqual(2, target.Arguments.Count);
            Assert.AreEqual("ParentRegex", target.Arguments[0].Name);
            Assert.AreEqual("There", target.Arguments[1].Value);
        }

        /// <summary>
        ///A test for Enforce
        ///</summary>
        [TestMethod()]
        public void EnforceTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy(); 
            bool expected = true; 
            bool actual;
            target.Enforce = expected;
            actual = target.Enforce;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ErrorMessage
        ///</summary>
        [TestMethod()]
        public void ErrorMessageTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy(); 
            string expected = "This is my error message";
            string actual;
            target.ErrorMessage = expected;
            actual = target.ErrorMessage;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            string expected = "This is my long description";
            string actual;
            target.LongDescription = expected;
            actual = target.LongDescription;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy(); 
            string actual;
            actual = target.PolicyId;
            Assert.AreEqual(PolicyIdKey.ScriptSyntaxPairingCheckPolicy, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            ScriptSyntaxPairingCheckPolicy target = new ScriptSyntaxPairingCheckPolicy();
            string expected = "Short Desc";
            string actual;
            target.ShortDescription = expected;
            actual = target.ShortDescription;
            Assert.AreEqual(expected, actual);
   
        }
    }
}
