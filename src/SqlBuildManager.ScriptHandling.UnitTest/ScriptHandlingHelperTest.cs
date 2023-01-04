using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlBuildManager.ScriptHandling.UnitTest
{


    /// <summary>
    ///This is a test class for ScriptHandlingHelperTest and is intended
    ///to contain all ScriptHandlingHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptHandlingHelperTest
    {


        /// <summary>
        ///A test for IsInLargeCommentHeader
        ///</summary>
        [TestMethod()]
        public void IsInLargeCommentHeaderTest_NotInCommentHeader()
        {
            string rawScript = @"This is my script
and the results of my search
will not be in a comment header
/****************
*  not here 
*****************/";
            int index = 24;
            bool expected = false;
            bool actual;
            actual = ScriptHandlingHelper.IsInLargeCommentHeader(rawScript, index);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for IsInLargeCommentHeader
        ///</summary>
        [TestMethod()]
        public void IsInLargeCommentHeaderTest_InCommentHeader()
        {
            string rawScript = @"This is my script
and the results of my search
will be in the comment header
/****************
*  I'm here!!
*****************/";
            int index = 85;
            bool expected = true;
            bool actual;
            actual = ScriptHandlingHelper.IsInLargeCommentHeader(rawScript, index);
            Assert.AreEqual(expected, actual);

        }

    }
}
