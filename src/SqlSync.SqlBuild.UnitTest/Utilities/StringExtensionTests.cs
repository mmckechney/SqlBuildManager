using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Utilities;

namespace SqlSync.SqlBuild.UnitTest.Utilities
{
    [TestClass]
    public class StringExtensionTests
    {
        #region ClearTrailingSpacesAndTabs Tests

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithTrailingSpaces_RemovesSpaces()
        {
            // Arrange
            string input = "Hello World   ";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithTrailingTabs_RemovesTabs()
        {
            // Arrange
            string input = "Hello World\t\t\t";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithMixedSpacesAndTabs_RemovesBoth()
        {
            // Arrange
            string input = "Hello World \t \t ";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_NoTrailingWhitespace_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string input = "";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_OnlySpaces_ReturnsEmpty()
        {
            // Arrange
            string input = "     ";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_OnlyTabs_ReturnsEmpty()
        {
            // Arrange
            string input = "\t\t\t";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_LeadingSpacesPreserved()
        {
            // Arrange
            string input = "   Hello World   ";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("   Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_MiddleSpacesPreserved()
        {
            // Arrange
            string input = "Hello   World   ";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello   World", result);
        }

        #endregion

        #region ClearTrailingCarriageReturn Tests

        [TestMethod]
        public void ClearTrailingCarriageReturn_WithCRLF_RemovesCRLF()
        {
            // Arrange
            string input = "Hello World\r\n";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_WithLFOnly_RemovesLF()
        {
            // Arrange
            string input = "Hello World\n";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_NoTrailingNewline_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string input = "";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_MultipleCRLF_RemovesOnlyLast()
        {
            // Arrange
            string input = "Hello\r\nWorld\r\n";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello\r\nWorld", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_JustCRLF_ReturnsEmpty()
        {
            // Arrange
            string input = "\r\n";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_JustLF_ReturnsEmpty()
        {
            // Arrange
            string input = "\n";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_CRWithoutLF_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World\r";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World\r", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_InternalNewlines_PreservesInternal()
        {
            // Arrange
            string input = "Line1\r\nLine2\r\n";

            // Act
            string result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Line1\r\nLine2", result);
        }

        #endregion

        #region ConvertNewLinetoCarriageReturnNewLine Tests

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_LFOnly_ConvertsToCRLF()
        {
            // Arrange
            string input = "Line1\nLine2\nLine3";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Line1\r\nLine2\r\nLine3", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_AlreadyCRLF_NoChange()
        {
            // Arrange
            string input = "Line1\r\nLine2\r\nLine3";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Line1\r\nLine2\r\nLine3", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_MixedLineEndings_NormalizesToCRLF()
        {
            // Arrange
            string input = "Line1\nLine2\r\nLine3\nLine4";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Line1\r\nLine2\r\nLine3\r\nLine4", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_NoNewLines_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_EmptyString_ReturnsEmpty()
        {
            // Arrange
            string input = "";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_SingleLF_ConvertsToCRLF()
        {
            // Arrange
            string input = "\n";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("\r\n", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_MultipleLF_ConvertsAllToCRLF()
        {
            // Arrange
            string input = "\n\n\n";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("\r\n\r\n\r\n", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_LFAtEnd_ConvertsToCRLF()
        {
            // Arrange
            string input = "Hello World\n";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Hello World\r\n", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_LFAtStart_ConvertsToCRLF()
        {
            // Arrange
            string input = "\nHello World";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("\r\nHello World", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_SqlScript_HandlesCorrectly()
        {
            // Arrange - simulating a SQL script with Unix line endings
            string input = "SELECT *\nFROM Users\nWHERE Id = 1";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("SELECT *\r\nFROM Users\r\nWHERE Id = 1", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_CRAlone_NotConverted()
        {
            // Arrange - CR alone should not be affected
            string input = "Line1\rLine2";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert - The regex only matches LF not preceded by CR
            Assert.AreEqual("Line1\rLine2", result);
        }

        #endregion

        #region Edge Cases and Combined Tests

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_ThenClearCarriageReturn_WorksInSequence()
        {
            // Arrange
            string input = "Hello World   \r\n";

            // Act - First remove \r\n, then remove trailing spaces
            string result = input.ClearTrailingCarriageReturn().ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void AllMethods_ChainedTogether_WorksCorrectly()
        {
            // Arrange
            string input = "Line1\nLine2   \t\r\n";

            // Act
            string result = input
                .ConvertNewLinetoCarriageReturnNewLine()
                .ClearTrailingCarriageReturn()
                .ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Line1\r\nLine2", result);
        }

        [TestMethod]
        public void StringExtensions_WithUnicodeCharacters_WorksCorrectly()
        {
            // Arrange
            string input = "Hello 世界   ";

            // Act
            string result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello 世界", result);
        }

        [TestMethod]
        public void StringExtensions_WithSqlBatchSeparator_WorksCorrectly()
        {
            // Arrange
            string input = "SELECT 1\nGO\nSELECT 2";

            // Act
            string result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("SELECT 1\r\nGO\r\nSELECT 2", result);
        }

        #endregion
    }
}
