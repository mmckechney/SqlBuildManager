using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.UnitTest
{
    [TestClass]
    public class ConnectionStringRedactorTest
    {
        #region MaskKey

        [TestMethod]
        public void MaskKey_LongValue_KeepsFirstFourCharacters()
        {
            string masked = ConnectionStringRedactor.MaskKey("abcdef123456");

            Assert.AreEqual("abcdxxxxxxxx", masked, "Should keep the first 4 chars and mask the rest with lowercase x");
            Assert.AreEqual(12, masked.Length, "Length should be preserved");
        }

        [TestMethod]
        public void MaskKey_ShortValue_IsFullyMasked()
        {
            Assert.AreEqual("xxxx", ConnectionStringRedactor.MaskKey("abcd"), "Values of length <= 4 are fully masked");
            Assert.AreEqual("xx", ConnectionStringRedactor.MaskKey("ab"));
        }

        [TestMethod]
        public void MaskKey_NullOrEmpty_ReturnsInput()
        {
            Assert.AreEqual(string.Empty, ConnectionStringRedactor.MaskKey(string.Empty));
            Assert.IsNull(ConnectionStringRedactor.MaskKey(null!));
        }

        #endregion

        #region MaskPassword

        [TestMethod]
        public void MaskPassword_LongValue_KeepsFirstAndLastCharacter()
        {
            string masked = ConnectionStringRedactor.MaskPassword("SuperSecret");

            Assert.AreEqual("Sxxxxxxxxxt", masked, "Should keep first and last char and mask the middle");
            Assert.AreEqual(11, masked.Length, "Length should be preserved");
        }

        [TestMethod]
        public void MaskPassword_ShortValue_IsFullyMasked()
        {
            Assert.AreEqual("xx", ConnectionStringRedactor.MaskPassword("ab"), "Values of length <= 2 are fully masked");
            Assert.AreEqual("x", ConnectionStringRedactor.MaskPassword("a"));
        }

        [TestMethod]
        public void MaskPassword_NullOrEmpty_ReturnsInput()
        {
            Assert.AreEqual(string.Empty, ConnectionStringRedactor.MaskPassword(string.Empty));
            Assert.IsNull(ConnectionStringRedactor.MaskPassword(null!));
        }

        #endregion

        #region RedactConnectionString

        [TestMethod]
        public void RedactConnectionString_SqlServer_MasksPasswordWithKeyRule()
        {
            string redacted = ConnectionStringRedactor.RedactConnectionString(
                "Data Source=myserver;Initial Catalog=mydb;User ID=myuser;Password=mypass");

            Assert.IsFalse(redacted.Contains("mypass"), "Full password should not appear");
            Assert.IsTrue(redacted.Contains("mypaxx"), "Password should be masked using the key rule (first 4 chars)");
            Assert.IsTrue(redacted.Contains("myuser"), "Non-secret values should be preserved");
        }

        [TestMethod]
        public void RedactConnectionString_StorageAccount_MasksAccountKey()
        {
            string redacted = ConnectionStringRedactor.RedactConnectionString(
                "DefaultEndpointsProtocol=https;AccountName=mystorage;AccountKey=abcdefghijklmnop;EndpointSuffix=core.windows.net");

            Assert.IsFalse(redacted.Contains("abcdefghijklmnop"), "Full account key should not appear");
            Assert.IsTrue(redacted.Contains("abcdxxxxxxxxxxxx"), "Account key should be masked using the key rule");
            Assert.IsTrue(redacted.Contains("mystorage"), "Account name should be preserved");
        }

        [TestMethod]
        public void RedactConnectionString_ServiceBus_MasksSharedAccessKeyButNotName()
        {
            string redacted = ConnectionStringRedactor.RedactConnectionString(
                "Endpoint=sb://my.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abcdefghijklmnop");

            Assert.IsFalse(redacted.Contains("abcdefghijklmnop"), "Full shared access key should not appear");
            Assert.IsTrue(redacted.Contains("abcdxxxxxxxxxxxx"), "SharedAccessKey should be masked");
            Assert.IsTrue(redacted.Contains("RootManageSharedAccessKey"), "SharedAccessKeyName is not a secret and should be preserved");
        }

        [TestMethod]
        public void RedactConnectionString_NullOrWhitespace_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, ConnectionStringRedactor.RedactConnectionString(null!));
            Assert.AreEqual(string.Empty, ConnectionStringRedactor.RedactConnectionString("   "));
        }

        [TestMethod]
        public void Redact_IsAliasForRedactConnectionString()
        {
            string input = "Data Source=myserver;User ID=myuser;Password=mypass";

            Assert.AreEqual(
                ConnectionStringRedactor.RedactConnectionString(input),
                ConnectionStringRedactor.Redact(input));
        }

        #endregion
    }
}
