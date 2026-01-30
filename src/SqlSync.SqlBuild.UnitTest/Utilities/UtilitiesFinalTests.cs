using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Utilities
{
    /// <summary>
    /// Final coverage tests for Utility classes
    /// </summary>
    [TestClass]
    public class StringExtensionFinalTests
    {
        #region ClearTrailingSpacesAndTabs Tests

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithTrailingSpaces_RemovesThem()
        {
            // Arrange
            string input = "Hello World   ";

            // Act
            var result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithTrailingTabs_RemovesThem()
        {
            // Arrange
            string input = "Hello World\t\t\t";

            // Act
            var result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithMixedTrailing_RemovesAll()
        {
            // Arrange
            string input = "Hello World \t \t ";

            // Act
            var result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithNoTrailing_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World";

            // Act
            var result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingSpacesAndTabs_WithEmptyString_ReturnsEmpty()
        {
            // Arrange
            string input = "";

            // Act
            var result = input.ClearTrailingSpacesAndTabs();

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region ClearTrailingCarriageReturn Tests

        [TestMethod]
        public void ClearTrailingCarriageReturn_WithCRLF_RemovesIt()
        {
            // Arrange
            string input = "Hello World\r\n";

            // Act
            var result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_WithLF_RemovesIt()
        {
            // Arrange
            string input = "Hello World\n";

            // Act
            var result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_WithNoNewLine_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World";

            // Act
            var result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void ClearTrailingCarriageReturn_WithMiddleNewLine_KeepsThem()
        {
            // Arrange
            string input = "Hello\r\nWorld";

            // Act
            var result = input.ClearTrailingCarriageReturn();

            // Assert
            Assert.AreEqual("Hello\r\nWorld", result);
        }

        #endregion

        #region ConvertNewLinetoCarriageReturnNewLine Tests

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_WithLF_ConvertsToCRLF()
        {
            // Arrange
            string input = "Line1\nLine2\nLine3";

            // Act
            var result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Line1\r\nLine2\r\nLine3", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_WithCRLF_KeepsUnchanged()
        {
            // Arrange
            string input = "Line1\r\nLine2\r\nLine3";

            // Act
            var result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Line1\r\nLine2\r\nLine3", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_WithMixed_ConvertsonlyLF()
        {
            // Arrange
            string input = "Line1\nLine2\r\nLine3\n";

            // Act
            var result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Line1\r\nLine2\r\nLine3\r\n", result);
        }

        [TestMethod]
        public void ConvertNewLinetoCarriageReturnNewLine_WithNoNewLines_ReturnsUnchanged()
        {
            // Arrange
            string input = "Hello World";

            // Act
            var result = input.ConvertNewLinetoCarriageReturnNewLine();

            // Assert
            Assert.AreEqual("Hello World", result);
        }

        #endregion
    }

    /// <summary>
    /// Tests for Cryptography utilities
    /// </summary>
    [TestClass]
    public class CryptographyFinalTests
    {
        [TestMethod]
        public void EncryptText_AndDecryptText_ReturnsOriginal()
        {
            // Arrange
            string original = "This is a secret message";
            string password = "MySecurePassword123";

            // Act
            var encrypted = Cryptography.EncryptText(original, password);
            var (success, decrypted) = Cryptography.DecryptText(encrypted, password, "test");

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(original, decrypted);
        }

        [TestMethod]
        public void EncryptText_ProducesDifferentOutput()
        {
            // Arrange
            string input = "Hello World";
            string password = "Password123";

            // Act
            var encrypted = Cryptography.EncryptText(input, password);

            // Assert
            Assert.AreNotEqual(input, encrypted);
        }

        [TestMethod]
        public void EncryptText_SameInputProducesSameOutput()
        {
            // Arrange
            string input = "Hello World";
            string password = "Password123";

            // Act
            var encrypted1 = Cryptography.EncryptText(input, password);
            var encrypted2 = Cryptography.EncryptText(input, password);

            // Assert
            Assert.AreEqual(encrypted1, encrypted2);
        }

        [TestMethod]
        public void DecryptText_WithWrongPassword_ReturnsOriginalInput()
        {
            // Arrange
            string original = "Hello World";
            string password = "CorrectPassword";
            string wrongPassword = "WrongPassword";

            // Act
            var encrypted = Cryptography.EncryptText(original, password);
            var (success, decrypted) = Cryptography.DecryptText(encrypted, wrongPassword, "test", suppressLog: true);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(encrypted, decrypted); // Returns original input on failure
        }

        [TestMethod]
        public void DecryptText_WithInvalidBase64_ReturnsOriginalInput()
        {
            // Arrange
            string invalidBase64 = "This is not base64!!!";
            string password = "Password123";

            // Act
            var (success, decrypted) = Cryptography.DecryptText(invalidBase64, password, "test", suppressLog: true);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(invalidBase64, decrypted);
        }

        [TestMethod]
        public void Sha256Hash_ProducesConsistentHash()
        {
            // Arrange
            string input = "Hello World";

            // Act
            var hash1 = input.Sha256Hash();
            var hash2 = input.Sha256Hash();

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void Sha256Hash_ProducesDifferentHashForDifferentInput()
        {
            // Arrange
            string input1 = "Hello World";
            string input2 = "Hello World!";

            // Act
            var hash1 = input1.Sha256Hash();
            var hash2 = input2.Sha256Hash();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void Sha256Hash_ReturnsCorrectLength()
        {
            // Arrange
            string input = "Test String";

            // Act
            var hash = input.Sha256Hash();

            // Assert - SHA256 produces 64 hex characters
            Assert.AreEqual(64, hash.Length);
        }

        [TestMethod]
        public void Sha256Hash_OnlyContainsHexCharacters()
        {
            // Arrange
            string input = "Test String";

            // Act
            var hash = input.Sha256Hash();

            // Assert
            Assert.IsTrue(hash.All(c => "0123456789abcdef".Contains(c)));
        }
    }

    /// <summary>
    /// Tests for CustomExtensionMethods
    /// </summary>
    [TestClass]
    public class ExtensionsFinalTests
    {
        [TestMethod]
        public void SplitIntoChunks_WithEvenDistribution_SplitsCorrectly()
        {
            // Arrange
            var list = Enumerable.Range(1, 12).ToList();

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual(4, chunks[0].Count());
            Assert.AreEqual(4, chunks[1].Count());
            Assert.AreEqual(4, chunks[2].Count());
        }

        [TestMethod]
        public void SplitIntoChunks_WithUnevenDistribution_DistributesRemaining()
        {
            // Arrange
            var list = Enumerable.Range(1, 10).ToList();

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(3, chunks.Count);
            var total = chunks.Sum(c => c.Count());
            Assert.AreEqual(10, total);
        }

        [TestMethod]
        public void SplitIntoChunks_WithSingleChunk_ReturnsSingleChunk()
        {
            // Arrange
            var list = Enumerable.Range(1, 10).ToList();

            // Act
            var chunks = list.SplitIntoChunks(1).ToList();

            // Assert
            Assert.AreEqual(1, chunks.Count);
            Assert.AreEqual(10, chunks[0].Count());
        }

        [TestMethod]
        public void SplitIntoChunks_WithMoreChunksThanItems_CreatesFewerChunks()
        {
            // Arrange
            var list = Enumerable.Range(1, 3).ToList();

            // Act
            var chunks = list.SplitIntoChunks(10).ToList();

            // Assert
            var total = chunks.Sum(c => c.Count());
            Assert.AreEqual(3, total);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SplitIntoChunks_WithZeroChunks_ThrowsArgumentException()
        {
            // Arrange
            var list = Enumerable.Range(1, 10).ToList();

            // Act
            list.SplitIntoChunks(0).ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SplitIntoChunks_WithNegativeChunks_ThrowsArgumentException()
        {
            // Arrange
            var list = Enumerable.Range(1, 10).ToList();

            // Act
            list.SplitIntoChunks(-1).ToList();
        }

        [TestMethod]
        public void SplitIntoChunks_PreservesOrder()
        {
            // Arrange
            var list = new List<string> { "A", "B", "C", "D", "E", "F" };

            // Act
            var chunks = list.SplitIntoChunks(2).ToList();

            // Assert
            Assert.AreEqual("A", chunks[0].First());
            Assert.AreEqual("D", chunks[1].First());
        }

        [TestMethod]
        public void SplitIntoChunks_WithEmptyList_ReturnsEmptyChunks()
        {
            // Arrange
            var list = new List<int>();

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(0, chunks.Sum(c => c.Count()));
        }
    }
}
