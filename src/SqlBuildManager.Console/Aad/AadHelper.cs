using Azure.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Aad
{
    internal class AadHelper
    {
        private static TokenCredential _tokenCred = null;
        internal static TokenCredential TokenCredential
        {
            get
            {
                if (_tokenCred == null)

                {
                    _tokenCred = new ChainedTokenCredential(
                        new ManagedIdentityCredential(),
                        new AzureCliCredential(),
                        new AzurePowerShellCredential(),
                        new VisualStudioCredential(),
                        new VisualStudioCodeCredential(),
                        new InteractiveBrowserCredential());
                }
                return _tokenCred;
            }
        }
    }
}
