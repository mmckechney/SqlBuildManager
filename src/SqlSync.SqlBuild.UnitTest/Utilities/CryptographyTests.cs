using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Utilities;
using System;

namespace SqlSync.SqlBuild.UnitTest.Utilities
{
    [TestClass]
    public class CryptographyTests
    {
        private const string TestPassword = "TestPassword123!";

        [TestMethod]
        public void EncryptText_DecryptText_RoundTrip_Success()
        {
            // Arrange
            string original = "This is a secret message";

            // Act
            string encrypted = Cryptography.EncryptText(original, TestPassword);
            var (success, decrypted) = Cryptography.DecryptText(encrypted, TestPassword, "test value", true);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(original, decrypted);
        }

        [TestMethod]
        public void EncryptText_ProducesDifferentOutputThanInput()
        {
            // Arrange
            string original = "Plain text message";

            // Act
            string encrypted = Cryptography.EncryptText(original, TestPassword);

            // Assert
            Assert.AreNotEqual(original, encrypted);
            Assert.IsTrue(encrypted.Length > 0);
        }

        [TestMethod]
        public void EncryptText_EmptyString_ReturnsEncryptedValue()
        {
            // Arrange
            string original = "";

            // Act
            string encrypted = Cryptography.EncryptText(original, TestPassword);

            // Assert
            Assert.IsNotNull(encrypted);
            Assert.IsTrue(encrypted.Length > 0); // Even empty string produces cipher text
        }

        [TestMethod]
        public void EncryptText_SameInputSamePassword_ProducesSameOutput()
        {
            // Arrange
            string original = "Consistent encryption test";

            // Act
            string encrypted1 = Cryptography.EncryptText(original, TestPassword);
            string encrypted2 = Cryptography.EncryptText(original, TestPassword);

            // Assert
            Assert.AreEqual(encrypted1, encrypted2);
        }

        [TestMethod]
        public void EncryptText_DifferentPasswords_ProducesDifferentOutput()
        {
            // Arrange
            string original = "Secret data";

            // Act
            string encrypted1 = Cryptography.EncryptText(original, "Password1");
            string encrypted2 = Cryptography.EncryptText(original, "Password2");

            // Assert
            Assert.AreNotEqual(encrypted1, encrypted2);
        }

        [TestMethod]
        public void DecryptText_WrongPassword_ReturnsFalseAndOriginalInput()
        {
            // Arrange
            string original = "Secret message";
            string encrypted = Cryptography.EncryptText(original, TestPassword);

            // Act
            var (success, result) = Cryptography.DecryptText(encrypted, "WrongPassword", "test", true);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(encrypted, result); // Returns unmodified input on failure
        }

        [TestMethod]
        public void DecryptText_InvalidBase64_ReturnsFalseAndOriginalInput()
        {
            // Arrange
            string invalidBase64 = "Not-Valid-Base64!!!";

            // Act
            var (success, result) = Cryptography.DecryptText(invalidBase64, TestPassword, "test", true);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(invalidBase64, result);
        }

        [TestMethod]
        public void DecryptText_CorruptedData_ReturnsFalseAndOriginalInput()
        {
            // Arrange
            string corruptedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

            // Act
            var (success, result) = Cryptography.DecryptText(corruptedBase64, TestPassword, "test", true);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(corruptedBase64, result);
        }

        [TestMethod]
        public void Sha256Hash_ProducesConsistentHash()
        {
            // Arrange
            string input = "HashMe";

            // Act
            string hash1 = input.Sha256Hash();
            string hash2 = input.Sha256Hash();

            // Assert
            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(64, hash1.Length); // SHA256 produces 64 hex chars
        }

        [TestMethod]
        public void Sha256Hash_DifferentInputs_ProduceDifferentHashes()
        {
            // Arrange
            string input1 = "Hello";
            string input2 = "World";

            // Act
            string hash1 = input1.Sha256Hash();
            string hash2 = input2.Sha256Hash();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void Sha256Hash_EmptyString_ProducesValidHash()
        {
            // Arrange
            string input = "";

            // Act
            string hash = input.Sha256Hash();

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreEqual(64, hash.Length);
            // Known SHA256 of empty string
            Assert.AreEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", hash);
        }

        [TestMethod]
        public void Sha256Hash_SpecialCharacters_ProducesValidHash()
        {
            // Arrange
            string input = "Special!@#$%^&*()_+{}|:<>?`~";

            // Act
            string hash = input.Sha256Hash();

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreEqual(64, hash.Length);
        }

        [TestMethod]
        public void EncryptText_LongText_RoundTripSuccess()
        {
            // Arrange
            string original = new string('A', 10000); // 10K chars

            // Act
            string encrypted = Cryptography.EncryptText(original, TestPassword);
            var (success, decrypted) = Cryptography.DecryptText(encrypted, TestPassword, "long text", true);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(original, decrypted);
        }

        [TestMethod]
        public void EncryptText_UnicodeText_RoundTripSuccess()
        {
            // Arrange
            string original = "Unicode: 你好世界 🎉 émoji";

            // Act
            string encrypted = Cryptography.EncryptText(original, TestPassword);
            var (success, decrypted) = Cryptography.DecryptText(encrypted, TestPassword, "unicode", true);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(original, decrypted);
        }
    }
}
