using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for CommentHeaderPolicyTest and is intended
    ///to contain all CommentHeaderPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommentHeaderPolicyTest
    {

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "CommentHeaderPolicy";
            Assert.AreEqual(actual, expected);

        }
        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string actual;
            actual = target.LongDescription;
            string expected = "Checks that Stored Procedure and Function have a comments header and recent comments";
            Assert.AreEqual(actual, expected);

        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string actual;
            actual = target.ShortDescription;
            string expected = "Check for Comments";
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderNoDates()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: Today
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		--------	--------		--------------------------------
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "No create date or change dates found.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderOldDates()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: 2/5/2007
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		1/1/2008	Me		    Testing a bad date
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "No recent comment found (last entry @ 01/01/2008). Please add a dated comment in mm/dd/yyyy format.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderTodayDate()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: 1/1/2008
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		" + System.DateTime.Now.ToString("MM/dd/yyyy") + @"   Me		    Testing for today
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderYesterdayDate()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: 1/1/2008
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		" + System.DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy") + @"   Me		    Testing yesterday
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderBeyondSetThreshold()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            target.DayThreshold = 10;
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: 1/1/2008
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		" + System.DateTime.Now.AddDays(-11).ToString("MM/dd/yyyy") + @"   Me		    Testing yesterday
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "No recent comment found (last entry @ " + System.DateTime.Now.AddDays(-11).ToString("MM/dd/yyyy") + "). Please add a dated comment in mm/dd/yyyy format.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderWithinSetThreshold()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            target.DayThreshold = 10;
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: 1/1/2008
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		" + System.DateTime.Now.AddDays(-9).ToString("MM/dd/yyyy") + @"   Me		    Testing yesterday
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_WithHeaderAtSetThreshold()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            target.DayThreshold = 10;
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
	            )
                AS
	            /******************************************************************************
	            **		Name: Routine Name
	            **		Desc: My Routine
	            **              
	            **		Auth: Me
	            **		Date: 1/1/2008
	            *******************************************************************************
	            **		Change History
	            *******************************************************************************
	            **		Date:		Author:		Description:
	            **		" + System.DateTime.Now.AddDays(-10).ToString("MM/dd/yyyy") + @"   Me		    Testing yesterday
	            **		
	            *******************************************************************************/

                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_MissingHeader()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]
	            (
		            @AccrualTemplateDetailID int
            		
	            )
                AS
                DELETE FROM dbo.AccrualRules
                WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";
            string message;
            string messageExpected = "No standard comment header found";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_NoRoutines()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"SELECT * FROM dbo.NoRoutinesHere";
            string message;
            string messageExpected = "No routines found";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_FunctionNoHeader()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            string script = @"        CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]()
RETURNS [datetime] 
AS 
BEGIN
    RETURN DATEADD(millisecond, -2, CONVERT(datetime, '2004-07-01', 101));
END;";
            string message;
            string messageExpected = "No standard comment header found";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void DayThreshold_Test()
        {
            CommentHeaderPolicy target = new CommentHeaderPolicy();
            target.DayThreshold = 40;
            Assert.AreEqual(40, target.DayThreshold);
        }

    }
}
