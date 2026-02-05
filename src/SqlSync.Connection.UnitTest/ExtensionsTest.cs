using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.Connection.UnitTest
{
    /// <summary>
    /// Unit tests for Extensions class
    /// </summary>
    [TestClass]
    public class ExtensionsTest
    {
        #region GetDescription Tests

        [TestMethod]
        public void GetDescription_WithDescriptionAttribute_ShouldReturnDescription()
        {
            var authType = AuthenticationType.Password;

            string result = authType.GetDescription();

            Assert.AreEqual("Username/Password", result);
        }

        [TestMethod]
        public void GetDescription_WindowsAuth_ShouldReturnCorrectDescription()
        {
            var authType = AuthenticationType.Windows;

            string result = authType.GetDescription();

            Assert.AreEqual("Windows Authentication", result);
        }

        [TestMethod]
        public void GetDescription_AzureADIntegrated_ShouldReturnCorrectDescription()
        {
            var authType = AuthenticationType.AzureADIntegrated;

            string result = authType.GetDescription();

            Assert.AreEqual("Azure AD Integrated Authentication", result);
        }

        [TestMethod]
        public void GetDescription_AzureADPassword_ShouldReturnCorrectDescription()
        {
            var authType = AuthenticationType.AzureADPassword;

            string result = authType.GetDescription();

            Assert.AreEqual("Azure AD Password Authentication", result);
        }

        [TestMethod]
        public void GetDescription_ManagedIdentity_ShouldReturnCorrectDescription()
        {
            var authType = AuthenticationType.ManagedIdentity;

            string result = authType.GetDescription();

            Assert.AreEqual("Managed Identity", result);
        }

        [TestMethod]
        public void GetDescription_AzureADInteractive_ShouldReturnCorrectDescription()
        {
            var authType = AuthenticationType.AzureADInteractive;

            string result = authType.GetDescription();

            Assert.AreEqual("Azure AD Interactive", result);
        }

        #endregion

        #region GetValueFromDescription Tests

        [TestMethod]
        public void GetValueFromDescription_ValidDescription_ShouldReturnEnumValue()
        {
            string description = "Username/Password";

            var result = Extensions.GetValueFromDescription<AuthenticationType>(description);

            Assert.AreEqual(AuthenticationType.Password, result);
        }

        [TestMethod]
        public void GetValueFromDescription_WindowsAuthentication_ShouldReturnCorrectValue()
        {
            string description = "Windows Authentication";

            var result = Extensions.GetValueFromDescription<AuthenticationType>(description);

            Assert.AreEqual(AuthenticationType.Windows, result);
        }

        [TestMethod]
        public void GetValueFromDescription_ManagedIdentity_ShouldReturnCorrectValue()
        {
            string description = "Managed Identity";

            var result = Extensions.GetValueFromDescription<AuthenticationType>(description);

            Assert.AreEqual(AuthenticationType.ManagedIdentity, result);
        }

        [TestMethod]
        public void GetValueFromDescription_AllAuthenticationTypes_ShouldWork()
        {
            // Test all authentication types with their description values
            Assert.AreEqual(AuthenticationType.AzureADIntegrated, 
                Extensions.GetValueFromDescription<AuthenticationType>("Azure AD Integrated Authentication"));
            Assert.AreEqual(AuthenticationType.AzureADPassword, 
                Extensions.GetValueFromDescription<AuthenticationType>("Azure AD Password Authentication"));
            Assert.AreEqual(AuthenticationType.AzureADInteractive, 
                Extensions.GetValueFromDescription<AuthenticationType>("Azure AD Interactive"));
        }

        [TestMethod]
        public void GetValueFromDescription_InvalidDescription_ShouldThrowArgumentException()
        {
            string description = "NonExistentAuthType";

            Assert.ThrowsExactly<ArgumentException>(() =>
                Extensions.GetValueFromDescription<AuthenticationType>(description));
        }

        [TestMethod]
        public void GetValueFromDescription_NonEnumType_ShouldThrowInvalidOperationException()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                Extensions.GetValueFromDescription<int>("test"));
        }

        #endregion
    }
}
