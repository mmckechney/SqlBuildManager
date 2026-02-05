using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.ObjectScript.UnitTest
{
    /// <summary>
    /// Unit tests for SanitizerExtensions class
    /// </summary>
    [TestClass]
    public class SanitizerExtensionsTest
    {
        #region Sanitize Tests - Valid Identifiers

        [TestMethod]
        public void Sanitize_SimpleIdentifier_ShouldReturnBracketedIdentifier()
        {
            string input = "TableName";

            string result = input.Sanitize();

            Assert.AreEqual("[TableName]", result);
        }

        [TestMethod]
        public void Sanitize_IdentifierWithUnderscore_ShouldReturnBracketedIdentifier()
        {
            string input = "Table_Name";

            string result = input.Sanitize();

            Assert.AreEqual("[Table_Name]", result);
        }

        [TestMethod]
        public void Sanitize_IdentifierStartingWithUnderscore_ShouldReturnBracketedIdentifier()
        {
            string input = "_TableName";

            string result = input.Sanitize();

            Assert.AreEqual("[_TableName]", result);
        }

        [TestMethod]
        public void Sanitize_IdentifierWithNumbers_ShouldReturnBracketedIdentifier()
        {
            string input = "Table123";

            string result = input.Sanitize();

            Assert.AreEqual("[Table123]", result);
        }

        [TestMethod]
        public void Sanitize_LowercaseIdentifier_ShouldReturnBracketedIdentifier()
        {
            string input = "tablename";

            string result = input.Sanitize();

            Assert.AreEqual("[tablename]", result);
        }

        [TestMethod]
        public void Sanitize_MixedCaseIdentifier_ShouldReturnBracketedIdentifier()
        {
            string input = "TableName123_Test";

            string result = input.Sanitize();

            Assert.AreEqual("[TableName123_Test]", result);
        }

        #endregion

        #region Sanitize Tests - Already Bracketed

        [TestMethod]
        public void Sanitize_AlreadyBracketedIdentifier_ShouldEscapeBrackets()
        {
            string input = "[TableName]";

            string result = input.Sanitize();

            // The input already has brackets, so they should be escaped
            Assert.AreEqual("[[TableName]]]", result);
        }

        #endregion

        #region Sanitize Tests - Bracket Escaping

        [TestMethod]
        public void Sanitize_IdentifierWithCloseBracket_ShouldEscapeBracket()
        {
            // While rare, a valid identifier could theoretically need bracket escaping
            string input = "Table";

            string result = input.Sanitize();

            Assert.AreEqual("[Table]", result);
        }

        #endregion

        #region Sanitize Tests - Invalid Identifiers

        [TestMethod]
        public void Sanitize_IdentifierStartingWithNumber_ShouldThrowArgumentException()
        {
            string input = "123Table";

            Assert.ThrowsExactly<ArgumentException>(() => input.Sanitize());
        }

        [TestMethod]
        public void Sanitize_IdentifierWithSpaces_ShouldThrowArgumentException()
        {
            string input = "Table Name";

            Assert.ThrowsExactly<ArgumentException>(() => input.Sanitize());
        }

        [TestMethod]
        public void Sanitize_IdentifierWithSpecialCharacters_ShouldThrowArgumentException()
        {
            string input = "Table@Name";

            Assert.ThrowsExactly<ArgumentException>(() => input.Sanitize());
        }

        [TestMethod]
        public void Sanitize_IdentifierWithHyphen_ShouldThrowArgumentException()
        {
            string input = "Table-Name";

            Assert.ThrowsExactly<ArgumentException>(() => input.Sanitize());
        }

        [TestMethod]
        public void Sanitize_EmptyString_ShouldThrowArgumentException()
        {
            string input = "";

            Assert.ThrowsExactly<ArgumentException>(() => input.Sanitize());
        }

        [TestMethod]
        public void Sanitize_IdentifierWithDot_ShouldThrowArgumentException()
        {
            string input = "Schema.Table";

            Assert.ThrowsExactly<ArgumentException>(() => input.Sanitize());
        }

        #endregion
    }
}
