using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlBuildManager.ScriptHandling.UnitTest
{
    /// <summary>
    /// Additional unit tests for ScriptHandlingHelper class to improve coverage
    /// </summary>
    [TestClass]
    public class ScriptHandlingHelperAdditionalTest
    {
        #region GetScriptCommentBlocks Tests

        [TestMethod]
        public void GetScriptCommentBlocks_WithSingleLineComment_ShouldReturnMatch()
        {
            string rawScript = "SELECT * FROM Table1\n-- This is a comment\nWHERE Id = 1\n";

            var result = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Value.Contains("-- This is a comment"));
        }

        [TestMethod]
        public void GetScriptCommentBlocks_WithMultiLineComment_ShouldReturnMatch()
        {
            string rawScript = "SELECT * FROM Table1\n/* This is a \nmulti-line comment */\nWHERE Id = 1";

            var result = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Value.Contains("multi-line comment"));
        }

        [TestMethod]
        public void GetScriptCommentBlocks_WithBothCommentTypes_ShouldReturnAllMatches()
        {
            string rawScript = "SELECT * FROM Table1\n-- Single line comment\n/* Multi-line\ncomment */\nWHERE Id = 1\n";

            var result = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetScriptCommentBlocks_WithNoComments_ShouldReturnEmptyList()
        {
            string rawScript = "SELECT * FROM Table1 WHERE Id = 1";

            var result = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetScriptCommentBlocks_WithEmptyString_ShouldReturnEmptyList()
        {
            string rawScript = "";

            var result = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetScriptCommentBlocks_WithMultipleSingleLineComments_ShouldReturnAll()
        {
            string rawScript = "-- Comment 1\nSELECT *\n-- Comment 2\nFROM Table1\n-- Comment 3\n";

            var result = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

            Assert.AreEqual(3, result.Count);
        }

        #endregion

        #region IsInComment (Match List) Tests

        [TestMethod]
        public void IsInComment_MatchList_IndexInComment_ShouldReturnTrue()
        {
            string rawScript = "SELECT * FROM Table1\n-- This is a comment\nWHERE Id = 1\n";
            var commentBlocks = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);
            int indexInComment = rawScript.IndexOf("This is");

            bool result = ScriptHandlingHelper.IsInComment(indexInComment, commentBlocks);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInComment_MatchList_IndexOutsideComment_ShouldReturnFalse()
        {
            string rawScript = "SELECT * FROM Table1\n-- This is a comment\nWHERE Id = 1\n";
            var commentBlocks = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);
            int indexOutsideComment = rawScript.IndexOf("SELECT");

            bool result = ScriptHandlingHelper.IsInComment(indexOutsideComment, commentBlocks);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_MatchList_EmptyList_ShouldReturnFalse()
        {
            var emptyList = new List<Match>();

            bool result = ScriptHandlingHelper.IsInComment(10, emptyList);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_MatchList_IndexAtCommentStart_ShouldReturnFalse()
        {
            string rawScript = "SELECT * FROM Table1\n-- This is a comment\nWHERE Id = 1\n";
            var commentBlocks = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);
            // Index at exact start of comment (not inside it)
            int indexAtStart = rawScript.IndexOf("--");

            bool result = ScriptHandlingHelper.IsInComment(indexAtStart, commentBlocks);

            // Based on the code logic: index > m.Index (strictly greater than)
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_MatchList_IndexInMultiLineComment_ShouldReturnTrue()
        {
            string rawScript = "SELECT * /* multi-line\ncomment */ FROM Table1";
            var commentBlocks = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);
            int indexInComment = rawScript.IndexOf("multi-line");

            bool result = ScriptHandlingHelper.IsInComment(indexInComment, commentBlocks);

            Assert.IsTrue(result);
        }

        #endregion

        #region GetScriptCommentIndexes Tests

        [TestMethod]
        public void GetScriptCommentIndexes_WithSingleLineComment_ShouldReturnIndexes()
        {
            string rawScript = "SELECT * FROM Table1\n-- Comment\nWHERE Id = 1\n";
            int commentStart = rawScript.IndexOf("--");
            int commentEnd = rawScript.IndexOf("\nWHERE");

            var result = ScriptHandlingHelper.GetScriptCommentIndexes(rawScript);

            Assert.IsTrue(result.Contains(commentStart));
            Assert.IsTrue(result.Contains(commentStart + 1)); // Should include all indexes in comment
        }

        [TestMethod]
        public void GetScriptCommentIndexes_WithNoComments_ShouldReturnEmptyList()
        {
            string rawScript = "SELECT * FROM Table1 WHERE Id = 1";

            var result = ScriptHandlingHelper.GetScriptCommentIndexes(rawScript);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetScriptCommentIndexes_WithEmptyString_ShouldReturnEmptyList()
        {
            string rawScript = "";

            var result = ScriptHandlingHelper.GetScriptCommentIndexes(rawScript);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetScriptCommentIndexes_WithMultiLineComment_ShouldReturnIndexes()
        {
            string rawScript = "SELECT /* comment */ FROM Table1";
            int commentStart = rawScript.IndexOf("/*");

            var result = ScriptHandlingHelper.GetScriptCommentIndexes(rawScript);

            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result.Contains(commentStart));
        }

        #endregion

        #region IsInComment (Int List) Tests

        [TestMethod]
        public void IsInComment_IntList_IndexInComment_ShouldReturnTrue()
        {
            string rawScript = "SELECT * FROM Table1\n-- This is a comment\nWHERE Id = 1\n";
            var commentIndexes = ScriptHandlingHelper.GetScriptCommentIndexes(rawScript);
            int indexInComment = rawScript.IndexOf("This is");

            bool result = ScriptHandlingHelper.IsInComment(indexInComment, commentIndexes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInComment_IntList_IndexOutsideComment_ShouldReturnFalse()
        {
            string rawScript = "SELECT * FROM Table1\n-- This is a comment\nWHERE Id = 1\n";
            var commentIndexes = ScriptHandlingHelper.GetScriptCommentIndexes(rawScript);
            int indexOutsideComment = rawScript.IndexOf("SELECT");

            bool result = ScriptHandlingHelper.IsInComment(indexOutsideComment, commentIndexes);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_IntList_EmptyList_ShouldReturnFalse()
        {
            var emptyList = new List<int>();

            bool result = ScriptHandlingHelper.IsInComment(10, emptyList);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_IntList_IndexNotInList_ShouldReturnFalse()
        {
            var commentIndexes = new List<int> { 5, 6, 7, 8, 9 };

            bool result = ScriptHandlingHelper.IsInComment(20, commentIndexes);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_IntList_IndexInList_ShouldReturnTrue()
        {
            var commentIndexes = new List<int> { 5, 6, 7, 8, 9 };

            bool result = ScriptHandlingHelper.IsInComment(7, commentIndexes);

            Assert.IsTrue(result);
        }

        #endregion

        #region IsInLargeCommentHeader Additional Tests

        [TestMethod]
        public void IsInLargeCommentHeader_IndexAtBoundary_ShouldReturnFalse()
        {
            string rawScript = "/** header comment **/\nSELECT * FROM Table1";
            int indexAtStart = 0; // At the very start boundary

            bool result = ScriptHandlingHelper.IsInLargeCommentHeader(rawScript, indexAtStart);

            // Index must be strictly inside (greater than start and less than end)
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInLargeCommentHeader_IndexAfterHeader_ShouldReturnFalse()
        {
            string rawScript = "/** header **/\nSELECT * FROM Table1";
            int indexAfterHeader = rawScript.IndexOf("SELECT");

            bool result = ScriptHandlingHelper.IsInLargeCommentHeader(rawScript, indexAfterHeader);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInLargeCommentHeader_EmptyScript_ShouldReturnFalse()
        {
            string rawScript = "";

            bool result = ScriptHandlingHelper.IsInLargeCommentHeader(rawScript, 0);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInLargeCommentHeader_ScriptWithOnlySQL_ShouldReturnFalse()
        {
            string rawScript = "SELECT * FROM Table1 WHERE Id = 1";

            bool result = ScriptHandlingHelper.IsInLargeCommentHeader(rawScript, 5);

            Assert.IsFalse(result);
        }

        #endregion
    }
}
