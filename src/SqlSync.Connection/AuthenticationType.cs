using System.ComponentModel;

namespace SqlSync.Connection
{
    public enum AuthenticationType
    {
        [Description("Username/Password")]
        Password,
        [Description("Windows Authentication")]
        Windows,
        [Description("Azure AD Integrated Authentication")]
        AzureADIntegrated,
        [Description("Azure AD Password Authentication")]
        AzureADPassword,
        [Description("Managed Identity")]
        ManagedIdentity
    }
}


