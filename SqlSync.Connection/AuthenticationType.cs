using System;
using System.ComponentModel;

namespace SqlSync.Connection
{
    public enum AuthenticationType
    {
        [Description("Username/Password")]
        UserNamePassword,
        [Description("Windows Authentication")]
        WindowsAuthentication,
        [Description("Azure AD Integrated Authentication")]
        AzureActiveDirectory,
        [Description("Azure AD Password Authentication")]
        AzureUserNamePassword
    }
}


