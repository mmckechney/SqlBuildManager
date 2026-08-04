using SqlBuildManager.Console.Aci;
using SqlBuildManager.Console.CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.AzureTest.TestBase
{
    public sealed class AciTestResourceTracker
    {
        private readonly string settingsFileKeyPath;
        private string aciName = string.Empty;
        private string settingsFile = string.Empty;

        public AciTestResourceTracker(string settingsFileKeyPath)
        {
            this.settingsFileKeyPath = settingsFileKeyPath;
        }

        public string Track(string name, string settingsPath)
        {
            aciName = name;
            settingsFile = settingsPath;
            return name;
        }

        public async Task CleanupAsync()
        {
            if (string.IsNullOrWhiteSpace(aciName))
            {
                return;
            }

            var settingsArgs = new CommandLineArgs
            {
                FileInfoSettingsFile = new FileInfo(settingsFile),
                SettingsFileKey = settingsFileKeyPath
            };
            var (decryptSuccess, commandLine) = Cryptography.DecryptSensitiveFields(settingsArgs);
            if (!decryptSuccess)
            {
                throw new InvalidOperationException("Unable to decrypt ACI settings for post-test cleanup.");
            }

            var cleanupSuccess = await AciManager.DeleteAciResources(
                commandLine.AciArgs.SubscriptionId,
                commandLine.AciArgs.ResourceGroup,
                aciName);
            if (!cleanupSuccess)
            {
                throw new InvalidOperationException($"Unable to clean up ACI resources for '{aciName}'.");
            }
        }
    }
}
